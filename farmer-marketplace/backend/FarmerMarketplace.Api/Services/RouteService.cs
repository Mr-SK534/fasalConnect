using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using FarmerMarketplace.Api.Records;
using RouteStopRecord = FarmerMarketplace.Api.Records.RouteStop;
using FarmerMarketplace.Api.Utils;
using Google.OrTools.ConstraintSolver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FarmerMarketplace.Api.Services
{
    public class RouteService : IRouteService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RouteService> _logger;

        private const int PickupServiceTimeMinutes = 15;
        private const int DeliveryServiceTimeMinutes = 10;
        private const double AvgSpeedKmPerHour = 40.0;
        private const double AvgSpeedMetersPerMinute = (AvgSpeedKmPerHour * 1000.0) / 60.0;

        private const int MaxShiftDurationMinutes = 720;
        private const long MaxShiftDurationSeconds = MaxShiftDurationMinutes * 60;

        public RouteService(AppDbContext context, ILogger<RouteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =========================================================================
        // 1. SINGLE ROUTE OPTIMIZATION (Admin / Delivery Hub Direct)
        // =========================================================================

        public async Task<RouteResponseDto> OptimizeAsync(Guid adminId, RouteOptimizeDto dto)
        {
            var orders = await _context.Orders
                .Where(o => dto.OrderIds.Contains(o.Id))
                .ToListAsync();

            if (orders.Count != dto.OrderIds.Count)
                throw new KeyNotFoundException("One or more orders were not found.");

            var missingCoords = orders.Where(o => !o.Latitude.HasValue || !o.Longitude.HasValue).ToList();
            if (missingCoords.Any())
                throw new InvalidOperationException($"{missingCoords.Count} order(s) missing delivery coordinates.");

            var points = new List<(double Lat, double Lng)>
            {
                (dto.DeliveryHubLocation.Lat, dto.DeliveryHubLocation.Lng)
            };
            points.AddRange(orders.Select(o => (o.Latitude!.Value, o.Longitude!.Value)));

            var n = points.Count;
            var distanceMatrix = new long[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j) { distanceMatrix[i, j] = 0; continue; }
                    var km = HaversineHelper.DistanceKm(points[i].Lat, points[i].Lng, points[j].Lat, points[j].Lng);
                    distanceMatrix[i, j] = (long)(km * 1000);
                }
            }

            var orderedStopIndices = SolveSingleRoute(distanceMatrix, n);

            var route = new FarmerMarketplace.Api.Models.Route
            {
                DeliveryHubLat = dto.DeliveryHubLocation.Lat,
                DeliveryHubLng = dto.DeliveryHubLocation.Lng,
                CreatedBy = adminId
            };

            double cumulativeDistanceKm = 0;
            var currentTime = DateTime.UtcNow;
            int sequence = 1;

            for (int idx = 1; idx < orderedStopIndices.Count; idx++)
            {
                var prevPointIndex = orderedStopIndices[idx - 1];
                var currPointIndex = orderedStopIndices[idx];

                if (currPointIndex == 0) continue;

                var legDistanceKm = HaversineHelper.DistanceKm(
                    points[prevPointIndex].Lat, points[prevPointIndex].Lng,
                    points[currPointIndex].Lat, points[currPointIndex].Lng);

                cumulativeDistanceKm += legDistanceKm;
                currentTime = currentTime.Add(HaversineHelper.EstimateTravelTime(legDistanceKm));

                var order = orders[currPointIndex - 1];

                route.Stops.Add(new FarmerMarketplace.Api.Models.RouteStop
                {
                    OrderId = order.Id,
                    StopSequence = sequence++,
                    Latitude = points[currPointIndex].Lat,
                    Longitude = points[currPointIndex].Lng,
                    DistanceFromPreviousKm = (decimal)legDistanceKm,
                    EstimatedArrival = currentTime
                });
            }

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            return MapToResponseDto(route);
        }

        public async Task<RouteResponseDto> GetByIdAsync(Guid routeId)
        {
            var route = await _context.Routes
                .Include(r => r.Stops)
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null) throw new KeyNotFoundException("Route not found.");

            return MapToResponseDto(route);
        }

        private static List<int> SolveSingleRoute(long[,] distanceMatrix, int nodeCount)
        {
            var manager = new RoutingIndexManager(nodeCount, 1, 0);
            var routing = new RoutingModel(manager);

            int transitCallbackIndex = routing.RegisterTransitCallback((fromIndex, toIndex) =>
            {
                var fromNode = manager.IndexToNode(fromIndex);
                var toNode = manager.IndexToNode(toIndex);
                return distanceMatrix[fromNode, toNode];
            });

            routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

            var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
            searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;

            var solution = routing.SolveWithParameters(searchParameters);

            var result = new List<int>();
            if (solution == null)
            {
                for (int i = 0; i < nodeCount; i++) result.Add(i);
                return result;
            }

            var index = routing.Start(0);
            while (!routing.IsEnd(index))
            {
                result.Add(manager.IndexToNode(index));
                index = solution.Value(routing.NextVar(index));
            }
            result.Add(manager.IndexToNode(index));

            return result;
        }

        private static RouteResponseDto MapToResponseDto(FarmerMarketplace.Api.Models.Route route)
        {
            return new RouteResponseDto
            {
                Id = route.Id,
                RouteId = route.Id,
                DeliveryHubLat = route.DeliveryHubLat,
                DeliveryHubLng = route.DeliveryHubLng,
                Stops = route.Stops
                    .OrderBy(s => s.StopSequence)
                    .Select(s => new RouteStopResponseDto
                    {
                        OrderId = s.OrderId,
                        StopSequence = s.StopSequence,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        DistanceFromPreviousKm = s.DistanceFromPreviousKm,
                        EstimatedArrival = s.EstimatedArrival
                    }).ToList(),
                CreatedAt = route.CreatedAt,
                BatchDate = route.CreatedAt
            };
        }

        // =========================================================================
        // 2. FLEET VRP DISPATCH (Multi-Vehicle, Capacity, Shift & Perishability)
        // =========================================================================

        public record PerishabilityInfo(string Tier, double MaxDeliveryHours);

        public static PerishabilityInfo GetPerishabilityInfo(ProductCategory? category, string? cropName)
        {
            string catStr = category?.ToString() ?? "";
            string nameStr = cropName ?? "";

            if (ContainsAny(catStr, nameStr, "Leafy", "Mushroom", "Herb", "Flower", "Spinach", "Methi", "Coriander", "Lettuce", "Mint"))
                return new PerishabilityInfo("Critical", 24);

            if (category == ProductCategory.Fruits || category == ProductCategory.Dairy ||
                ContainsAny(catStr, nameStr, "Tomato", "Berry", "Fruit", "Dairy", "Milk", "Butter", "Cheese", "Strawberry", "Mango", "Banana"))
                return new PerishabilityInfo("High", 48);

            if (category == ProductCategory.Vegetables ||
                ContainsAny(catStr, nameStr, "Root", "Onion", "Potato", "Egg", "Carrot", "Radish", "Beetroot", "Vegetable"))
                return new PerishabilityInfo("Medium", 120);

            return new PerishabilityInfo("Low", 720);
        }

        private static bool ContainsAny(string category, string cropName, params string[] keywords)
        {
            foreach (var kw in keywords)
            {
                if (category.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    cropName.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static PerishabilityInfo GetOrderPerishability(Order order)
        {
            PerishabilityInfo? mostUrgent = null;
            foreach (var item in order.Items)
            {
                var info = GetPerishabilityInfo(item.Product?.Category, item.Product?.CropName);
                if (mostUrgent == null || info.MaxDeliveryHours < mostUrgent.MaxDeliveryHours)
                    mostUrgent = info;
            }
            return mostUrgent ?? new PerishabilityInfo("Low", 720);
        }

        private class OrderChunkNode
        {
            public Order Order { get; set; } = null!;
            public double ChunkWeightKg { get; set; }
            public int ChunkIndex { get; set; }
            public int TotalChunks { get; set; }
            public PerishabilityInfo Perishability { get; set; } = new("Low", 720);
        }

        public async Task<DeliveryRoute> OptimizeAsync(List<Guid> orderIds, double depotLat, double depotLng, string batchWindow = "manual")
        {
            if (orderIds == null || !orderIds.Any())
                throw new ArgumentException("At least one order ID must be provided.", nameof(orderIds));

            var orders = await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Items).ThenInclude(i => i.Farmer)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Where(o => orderIds.Contains(o.Id))
                .ToListAsync();

            var validOrders = new List<Order>();

            foreach (var order in orders)
            {
                var farmer = order.Items.Select(i => i.Farmer).FirstOrDefault(f => f != null);
                double? pickupLat = farmer?.Latitude;
                double? pickupLng = farmer?.Longitude;
                double? deliveryLat = order.Latitude;
                double? deliveryLng = order.Longitude;

                if (!pickupLat.HasValue || !pickupLng.HasValue || !deliveryLat.HasValue || !deliveryLng.HasValue)
                    continue;

                order.PickupLat = pickupLat;
                order.PickupLng = pickupLng;
                order.DeliveryLat = deliveryLat;
                order.DeliveryLng = deliveryLng;
                validOrders.Add(order);
            }

            if (!validOrders.Any())
                throw new InvalidOperationException("No valid orders with complete coordinates found.");

            double totalQuantityKg = 0;
            var chunkNodes = new List<OrderChunkNode>();

            foreach (var order in validOrders)
            {
                double orderTotalKg = order.Items.Sum(i => ConvertToKg((double)i.Quantity, i.Product?.Unit));
                if (orderTotalKg <= 0) orderTotalKg = 1.0;
                totalQuantityKg += orderTotalKg;

                var perishability = GetOrderPerishability(order);
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
                        TotalChunks = numChunks,
                        Perishability = perishability
                    });
                }
            }

            int requiredVehicles = (int)Math.Ceiling(totalQuantityKg / 2000.0);
            int fleetSize = Math.Max(requiredVehicles, Math.Min(25, requiredVehicles + 1));
            int pairCount = chunkNodes.Count;
            int nodeCount = 1 + 2 * pairCount;

            var locations = new List<(double Lat, double Lng)> { (depotLat, depotLng) };

            foreach (var chunk in chunkNodes)
            {
                locations.Add((chunk.Order.PickupLat!.Value, chunk.Order.PickupLng!.Value));
                locations.Add((chunk.Order.DeliveryLat!.Value, chunk.Order.DeliveryLng!.Value));
            }

            long[,] distanceMatrix = new long[nodeCount, nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = 0; j < nodeCount; j++)
                {
                    distanceMatrix[i, j] = (i == j) ? 0 : GeoUtils.DistanceInMeters(locations[i].Lat, locations[i].Lng, locations[j].Lat, locations[j].Lng);
                }
            }

            var manager = new RoutingIndexManager(nodeCount, fleetSize, 0);
            var routing = new RoutingModel(manager);

            int distanceCallbackIndex = routing.RegisterTransitCallback((fromIndex, toIndex) =>
                distanceMatrix[manager.IndexToNode(fromIndex), manager.IndexToNode(toIndex)]);
            routing.SetArcCostEvaluatorOfAllVehicles(distanceCallbackIndex);
            routing.AddDimension(distanceCallbackIndex, 0, 10_000_000, true, "Distance");

            int timeCallbackIndex = routing.RegisterTransitCallback((fromIndex, toIndex) =>
            {
                int fromNode = manager.IndexToNode(fromIndex);
                int toNode = manager.IndexToNode(toIndex);
                long driveSeconds = (distanceMatrix[fromNode, toNode] * 9) / 100;
                long serviceSeconds = (fromNode == 0) ? 0 : (fromNode % 2 != 0 ? PickupServiceTimeMinutes * 60 : DeliveryServiceTimeMinutes * 60);
                return driveSeconds + serviceSeconds;
            });

            routing.AddDimension(timeCallbackIndex, 0, 86400 * 7, true, "Time");
            var timeDimension = routing.GetMutableDimension("Time");

            for (int v = 0; v < fleetSize; v++)
                timeDimension.CumulVar(routing.End(v)).SetMax(MaxShiftDurationSeconds);

            long[] demands = new long[nodeCount];
            for (int i = 0; i < pairCount; i++)
            {
                long q = (long)Math.Ceiling(chunkNodes[i].ChunkWeightKg);
                demands[1 + i * 2] = q;
                demands[2 + i * 2] = -q;
            }

            int demandCallbackIndex = routing.RegisterUnaryTransitCallback(fromIndex => demands[manager.IndexToNode(fromIndex)]);
            routing.AddDimension(demandCallbackIndex, 0, 2000, true, "Capacity");

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
                solver.Add(timeDimension.CumulVar(pickupIdx) + (PickupServiceTimeMinutes * 60) <= timeDimension.CumulVar(deliveryIdx));

                routing.AddDisjunction(new[] { pickupIdx }, disjunctionPenalty);
                routing.AddDisjunction(new[] { deliveryIdx }, disjunctionPenalty);
            }

            routing.SetFixedCostOfAllVehicles(100_000);

            var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
            searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
            searchParameters.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = 8 };

            var solution = routing.SolveWithParameters(searchParameters);

            DateTime routeStartTime = DateTime.UtcNow;
            var allStops = new List<RouteStopRecord>();
            double totalDistanceMeters = 0;

            for (int vehicle = 0; vehicle < fleetSize; vehicle++)
            {
                long index = routing.Start(vehicle);
                int stopSeq = 1;
                double currentLoad = 0;
                (double Lat, double Lng) prevLoc = (depotLat, depotLng);
                DateTime currentDeparture = routeStartTime;

                if (solution != null) index = solution.Value(routing.NextVar(index));

                while (!routing.IsEnd(index) && index != -1)
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

                        double lat = locations[node].Lat;
                        double lng = locations[node].Lng;
                        double segDistanceMeters = GeoUtils.DistanceInMeters(prevLoc.Lat, prevLoc.Lng, lat, lng);
                        totalDistanceMeters += segDistanceMeters;
                        prevLoc = (lat, lng);

                        double driveTimeMinutes = segDistanceMeters / AvgSpeedMetersPerMinute;
                        DateTime arrivalTime = currentDeparture.AddMinutes(driveTimeMinutes);
                        int serviceMinutes = isPickup ? PickupServiceTimeMinutes : DeliveryServiceTimeMinutes;
                        currentDeparture = arrivalTime.AddMinutes(serviceMinutes);

                        var farmerName = order.Items.Select(i => i.Farmer?.Name).FirstOrDefault() ?? "Farmer";
                        var buyerName = order.Buyer?.Name ?? "Buyer";
                        string chunkLabel = chunkNode.TotalChunks > 1 ? $" (Part {chunkNode.ChunkIndex}/{chunkNode.TotalChunks})" : "";

                        allStops.Add(new RouteStopRecord(
                            Sequence: stopSeq,
                            VehicleNumber: vehicle + 1,
                            Type: isPickup ? "pickup" : "delivery",
                            Label: isPickup ? $"{farmerName} (Pickup {q:0.#} kg){chunkLabel}" : $"{buyerName} (Delivery {q:0.#} kg){chunkLabel}",
                            Lat: lat,
                            Lng: lng,
                            OrderId: order.Id,
                            QuantityAtStop: Math.Round(currentLoad, 1),
                            EstimatedArrival: arrivalTime,
                            PerishabilityTier: chunkNode.Perishability.Tier,
                            DeliveryDeadline: routeStartTime.AddHours(chunkNode.Perishability.MaxDeliveryHours),
                            IsUrgent: arrivalTime > routeStartTime.AddHours(chunkNode.Perishability.MaxDeliveryHours)
                        ));

                        order.VehicleNumber = vehicle + 1;
                        order.StopSequence = stopSeq;
                        if (!isPickup)
                        {
                            order.EstimatedArrival = arrivalTime;
                            currentLoad -= q;
                        }
                        stopSeq++;
                    }
                    if (solution != null) index = solution.Value(routing.NextVar(index));
                    else break;
                }

                if (stopSeq > 1)
                    totalDistanceMeters += GeoUtils.DistanceInMeters(prevLoc.Lat, prevLoc.Lng, depotLat, depotLng);
            }

            var vehicleMapping = allStops
                .Select(s => s.VehicleNumber)
                .Distinct()
                .Select((v, idx) => (OldV: v, NewV: idx + 1))
                .ToDictionary(x => x.OldV, x => x.NewV);

            var remappedStops = allStops.Select(s => s with { VehicleNumber = vehicleMapping[s.VehicleNumber] }).ToList();

            foreach (var order in validOrders)
            {
                var stop = remappedStops.FirstOrDefault(s => s.OrderId == order.Id);
                if (stop != null) order.VehicleNumber = stop.VehicleNumber;
            }

            var deliveryRoute = new DeliveryRoute
            {
                Id = Guid.NewGuid(),
                BatchDate = DateTime.UtcNow,
                BatchWindow = batchWindow,
                VehicleCount = remappedStops.Select(s => s.VehicleNumber).Distinct().Count(),
                TotalDistanceKm = Math.Round(totalDistanceMeters / 1000.0, 2),
                StopsJson = JsonSerializer.Serialize(remappedStops),
                Status = DeliveryRouteStatus.Generated,
                CreatedAt = DateTime.UtcNow
            };

            _context.DeliveryRoutes.Add(deliveryRoute);
            foreach (var order in validOrders) order.RouteId = deliveryRoute.Id;

            await _context.SaveChangesAsync();
            return deliveryRoute;
        }

        public async Task<DeliveryRoute> GetRouteByIdAsync(Guid routeId)
        {
            var route = await _context.DeliveryRoutes.FirstOrDefaultAsync(r => r.Id == routeId);
            if (route == null) throw new KeyNotFoundException($"Route '{routeId}' not found.");
            return route;
        }

        public async Task<List<DeliveryRoute>> GetAllRoutesAsync()
        {
            return await _context.DeliveryRoutes.OrderByDescending(r => r.CreatedAt).ToListAsync();
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

            return unroutedOrders.Select(o => new PendingOrderDto
            {
                OrderId = o.Id,
                BuyerName = o.Buyer?.Name ?? "Buyer",
                FarmerName = o.Items.Select(i => i.Farmer?.Name).FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? "Farmer",
                Quantity = o.Items.Sum(i => ConvertToKg((double)i.Quantity, i.Product?.Unit)),
                TotalQuantity = o.Items.Sum(i => ConvertToKg((double)i.Quantity, i.Product?.Unit)),
                CropName = o.Items.Select(i => i.Product?.CropName).FirstOrDefault(c => !string.IsNullOrEmpty(c)) ?? "Crops",
                DeliveryAddress = o.DeliveryAddress ?? "N/A",
                CreatedAt = o.CreatedAt
            }).ToList();
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
    }
}