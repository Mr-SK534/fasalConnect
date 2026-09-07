// backend/FarmerMarketplace.Api/Services/RouteService.cs

using System.Text.Json;
using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using FarmerMarketplace.Api.Records;
using RouteStop = FarmerMarketplace.Api.Records.RouteStop;
using FarmerMarketplace.Api.Utils;
using Google.OrTools.ConstraintSolver;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class RouteService : IRouteService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RouteService> _logger;

        public RouteService(AppDbContext context, ILogger<RouteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private static double ConvertToKg(double quantity, ProductUnit? unit)
        {
            if (quantity <= 0) return 0;
            return unit switch
            {
                ProductUnit.Ton => quantity < 50.0 ? quantity * 1000.0 : quantity,
                ProductUnit.Quintal => quantity < 100.0 ? quantity * 100.0 : quantity,
                _ => quantity
            };
        }

        private class OrderChunkNode
        {
            public Order Order { get; set; } = null!;
            public double ChunkWeightKg { get; set; }
            public int ChunkIndex { get; set; }
            public int TotalChunks { get; set; }
        }

        public async Task<DeliveryRoute> OptimizeAsync(List<Guid> orderIds, double depotLat, double depotLng)
        {
            if (orderIds == null || !orderIds.Any())
                throw new ArgumentException("At least one order ID must be provided for optimization.", nameof(orderIds));

            // 1. Load orders with items, farmer, and buyer details
            var orders = await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Items).ThenInclude(i => i.Farmer)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Where(o => orderIds.Contains(o.Id))
                .ToListAsync();

            var validOrders = new List<Order>();
            var skippedOrderIds = new List<Guid>();

            // 2. Validate coordinates for each order
            foreach (var order in orders)
            {
                var farmer = order.Items.Select(i => i.Farmer).FirstOrDefault(f => f != null);
                double? pickupLat = farmer?.Latitude;
                double? pickupLng = farmer?.Longitude;

                double? deliveryLat = order.Latitude;
                double? deliveryLng = order.Longitude;

                if (!pickupLat.HasValue || !pickupLng.HasValue || !deliveryLat.HasValue || !deliveryLng.HasValue)
                {
                    _logger.LogWarning("Skipping order {OrderId}: missing pickup or delivery coordinates.", order.Id);
                    skippedOrderIds.Add(order.Id);
                    continue;
                }

                order.PickupLat = pickupLat;
                order.PickupLng = pickupLng;
                order.DeliveryLat = deliveryLat;
                order.DeliveryLng = deliveryLng;

                validOrders.Add(order);
            }

            if (!validOrders.Any())
            {
                throw new InvalidOperationException("No valid orders with complete pickup and delivery coordinates were found.");
            }

            // 3. Compute weight in Kilograms for each order and split orders > 2000 kg into chunks
            double totalQuantityKg = 0;
            var chunkNodes = new List<OrderChunkNode>();

            foreach (var order in validOrders)
            {
                double orderTotalKg = order.Items.Sum(i => ConvertToKg((double)i.Quantity, i.Product?.Unit));
                if (orderTotalKg <= 0) orderTotalKg = 1.0;
                totalQuantityKg += orderTotalKg;

                // Chunk orders larger than 2000 kg into virtual demands <= 2000 kg
                int numChunks = (int)Math.Ceiling(orderTotalKg / 2000.0);
                double remainingKg = orderTotalKg;

                for (int c = 0; c < numChunks; c++)
                {
                    double chunkWeight = Math.Min(remainingKg, 2000.0);
                    remainingKg -= chunkWeight;

                    chunkNodes.Add(new OrderChunkNode
                    {
                        Order = order,
                        ChunkWeightKg = chunkWeight,
                        ChunkIndex = c + 1,
                        TotalChunks = numChunks
                    });
                }
            }

            // 4. Fleet sizing: ceiling of total kg / 2000, with a safety buffer for route geometry
            int requiredVehicles = (int)Math.Ceiling(totalQuantityKg / 2000.0);
            int fleetSize = Math.Max(requiredVehicles, Math.Min(25, requiredVehicles + 2));

            int pairCount = chunkNodes.Count;
            int nodeCount = 1 + 2 * pairCount; // 0 = depot, (2i+1)=pickup, (2i+2)=delivery

            // 5. Build location list
            var locations = new List<(double Lat, double Lng)>
            {
                (depotLat, depotLng) // Index 0: Depot
            };

            foreach (var chunk in chunkNodes)
            {
                locations.Add((chunk.Order.PickupLat!.Value, chunk.Order.PickupLng!.Value));    // 2i + 1
                locations.Add((chunk.Order.DeliveryLat!.Value, chunk.Order.DeliveryLng!.Value)); // 2i + 2
            }

            // 6. Build distance matrix (N x N in meters)
            long[,] distanceMatrix = new long[nodeCount, nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = 0; j < nodeCount; j++)
                {
                    if (i == j)
                    {
                        distanceMatrix[i, j] = 0;
                    }
                    else
                    {
                        distanceMatrix[i, j] = GeoUtils.DistanceInMeters(
                            locations[i].Lat, locations[i].Lng,
                            locations[j].Lat, locations[j].Lng);
                    }
                }
            }

            // 7. Initialize OR-Tools Routing model
            var manager = new RoutingIndexManager(nodeCount, fleetSize, 0);
            var routing = new RoutingModel(manager);

            int transitCallbackIndex = routing.RegisterTransitCallback((fromIndex, toIndex) =>
            {
                int fromNode = manager.IndexToNode(fromIndex);
                int toNode = manager.IndexToNode(toIndex);
                return distanceMatrix[fromNode, toNode];
            });

            routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);
            routing.AddDimension(transitCallbackIndex, 0, 10_000_000, true, "Distance");

            // 8. Add Capacity Dimension (max 2000 kg per vehicle)
            long[] demands = new long[nodeCount];
            demands[0] = 0; // depot
            for (int i = 0; i < pairCount; i++)
            {
                long q = (long)Math.Ceiling(chunkNodes[i].ChunkWeightKg);
                demands[1 + i * 2] = q;   // Pickup gains load
                demands[2 + i * 2] = -q;  // Delivery releases load
            }

            int demandCallbackIndex = routing.RegisterUnaryTransitCallback((fromIndex) =>
            {
                int fromNode = manager.IndexToNode(fromIndex);
                return demands[fromNode];
            });

            routing.AddDimension(
                demandCallbackIndex,
                0,     // slack
                2000,  // vehicle capacity cap = 2000 kg
                true,  // fix_start_cumul_to_zero
                "Capacity");

            // 9. Add Pickup-and-Delivery constraints & Disjunctions
            var solver = routing.solver();
            var capacityDim = routing.GetMutableDimension("Capacity");
            long disjunctionPenalty = 10_000_000;

            for (int i = 0; i < pairCount; i++)
            {
                int pickupNode = 1 + i * 2;
                int deliveryNode = 2 + i * 2;
                long pickupIdx = manager.NodeToIndex(pickupNode);
                long deliveryIdx = manager.NodeToIndex(deliveryNode);

                routing.AddPickupAndDelivery(pickupIdx, deliveryIdx);
                solver.Add(routing.VehicleVar(pickupIdx) == routing.VehicleVar(deliveryIdx));
                solver.Add(capacityDim.CumulVar(pickupIdx) <= capacityDim.CumulVar(deliveryIdx));

                // Disjunctions prevent solver crashes if constraints are tight
                routing.AddDisjunction(new[] { pickupIdx }, disjunctionPenalty);
                routing.AddDisjunction(new[] { deliveryIdx }, disjunctionPenalty);
            }

            // High fixed cost to encourage order consolidation onto minimum trucks
            routing.SetFixedCostOfAllVehicles(100_000);

            // 10. Search parameters
            var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
            searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
            searchParameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
            searchParameters.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = 15 };

            // 11. Solve
            var solution = routing.SolveWithParameters(searchParameters);
            if (solution == null)
            {
                throw new InvalidOperationException("OR-Tools could not find a valid routing solution under 2000 kg capacity constraints.");
            }

            // 12. Extract Solution
            var allStops = new List<RouteStop>();
            double totalDistanceMeters = 0;

            for (int vehicle = 0; vehicle < fleetSize; vehicle++)
            {
                long index = routing.Start(vehicle);
                int stopSeq = 1;
                double currentLoad = 0;

                (double Lat, double Lng) prevLoc = (depotLat, depotLng);

                // Advance past depot start
                index = solution.Value(routing.NextVar(index));

                while (!routing.IsEnd(index))
                {
                    int node = manager.IndexToNode(index);
                    if (node != 0)
                    {
                        int chunkIndex = (node - 1) / 2;
                        bool isPickup = (node % 2) != 0;
                        var chunkNode = chunkNodes[chunkIndex];
                        var order = chunkNode.Order;
                        double q = chunkNode.ChunkWeightKg;

                        if (isPickup) currentLoad += q;

                        var farmerName = order.Items.Select(i => i.Farmer?.Name).FirstOrDefault() ?? "Farmer";
                        var buyerName = order.Buyer?.Name ?? "Buyer";
                        
                        string chunkLabel = chunkNode.TotalChunks > 1 ? $" (Part {chunkNode.ChunkIndex}/{chunkNode.TotalChunks})" : "";
                        string label = isPickup
                            ? $"{farmerName} (Pickup {q:0.#} kg){chunkLabel}"
                            : $"{buyerName} (Delivery {q:0.#} kg){chunkLabel}";

                        double lat = locations[node].Lat;
                        double lng = locations[node].Lng;

                        totalDistanceMeters += GeoUtils.DistanceInMeters(prevLoc.Lat, prevLoc.Lng, lat, lng);
                        prevLoc = (lat, lng);

                        var stop = new RouteStop(
                            Sequence: stopSeq,
                            VehicleNumber: vehicle + 1,
                            Type: isPickup ? "pickup" : "delivery",
                            Label: label,
                            Lat: lat,
                            Lng: lng,
                            OrderId: order.Id,
                            QuantityAtStop: Math.Round(currentLoad, 1)
                        );

                        allStops.Add(stop);

                        // Update Order attributes
                        order.VehicleNumber = vehicle + 1;
                        order.StopSequence = stopSeq;

                        if (!isPickup)
                        {
                            currentLoad -= q;
                        }

                        stopSeq++;
                    }

                    index = solution.Value(routing.NextVar(index));
                }

                // Distance back to depot if vehicle visited stops
                if (stopSeq > 1)
                {
                    totalDistanceMeters += GeoUtils.DistanceInMeters(prevLoc.Lat, prevLoc.Lng, depotLat, depotLng);
                }
            }

            double totalDistanceKm = Math.Round(totalDistanceMeters / 1000.0, 2);

            // Active vehicle count (how many vehicles were assigned at least 1 stop)
            int activeVehicleCount = allStops.Select(s => s.VehicleNumber).Distinct().Count();

            // 13. Persist DeliveryRoute entity
            var deliveryRoute = new DeliveryRoute
            {
                Id = Guid.NewGuid(),
                BatchDate = DateTime.UtcNow,
                BatchWindow = "manual",
                VehicleCount = activeVehicleCount,
                TotalDistanceKm = totalDistanceKm,
                StopsJson = JsonSerializer.Serialize(allStops),
                Status = DeliveryRouteStatus.Generated,
                CreatedAt = DateTime.UtcNow
            };

            _context.DeliveryRoutes.Add(deliveryRoute);

            // Update orders with RouteId
            foreach (var order in validOrders)
            {
                order.RouteId = deliveryRoute.Id;
            }

            await _context.SaveChangesAsync();
            return deliveryRoute;
        }

        public async Task<DeliveryRoute> GetRouteByIdAsync(Guid routeId)
        {
            var route = await _context.DeliveryRoutes.FirstOrDefaultAsync(r => r.Id == routeId);
            if (route == null)
            {
                throw new KeyNotFoundException($"DeliveryRoute with ID '{routeId}' was not found.");
            }
            return route;
        }

        public async Task<List<PendingOrderDto>> GetPendingOrdersAsync()
        {
            var unroutedOrders = await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Items).ThenInclude(i => i.Farmer)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Where(o => o.Status == OrderStatus.Confirmed && o.RouteId == null)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return unroutedOrders.Select(o =>
            {
                double totalKg = o.Items.Sum(i => ConvertToKg((double)i.Quantity, i.Product?.Unit));

                var farmerName = o.Items.Select(i => i.Farmer?.Name).FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? "Farmer";
                var cropName = o.Items.Select(i => i.Product?.CropName).FirstOrDefault(c => !string.IsNullOrEmpty(c)) ?? "Crops";

                return new PendingOrderDto
                {
                    OrderId = o.Id,
                    BuyerName = o.Buyer?.Name ?? "Buyer",
                    FarmerName = farmerName,
                    Quantity = totalKg,
                    TotalQuantity = totalKg,
                    CropName = cropName,
                    DeliveryAddress = o.DeliveryAddress ?? "N/A",
                    CreatedAt = o.CreatedAt
                };
            }).ToList();
        }
    }
}
