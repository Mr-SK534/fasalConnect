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
    public class RouteService: IRouteService
    {
        private readonly AppDbContext _context;

        public RouteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RouteResponseDto> OptimizeAsync(Guid adminId, RouteOptimizeDto dto)
        {
            var orders = await _context.Orders
                .Where(o => dto.OrderIds.Contains(o.Id))
                .ToListAsync();

            if (orders.Count != dto.OrderIds.Count)
                throw new KeyNotFoundException("One or more orders were not found.");

            var missingCoords = orders.Where(o => !o.Latitude.HasValue || !o.Longitude.HasValue).ToList();
            if (missingCoords.Any())
                throw new InvalidOperationException(
                    $"{missingCoords.Count} order(s) are missing delivery coordinates and cannot be routed.");

            // Index 0 = depot (hub). Indices 1..N = orders, in the same order as `orders` list.
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
                    distanceMatrix[i, j] = (long)(km * 1000); // meters, OR-Tools works best with integers
                }
            }

            var orderedStopIndices = SolveRoute(distanceMatrix, n);

            // Build the Route + RouteStops from the solved order (skip index 0 = depot)
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

                if (currPointIndex == 0) continue; // skip if depot appears again (return leg)

                var legDistanceKm = HaversineHelper.DistanceKm(
                    points[prevPointIndex].Lat, points[prevPointIndex].Lng,
                    points[currPointIndex].Lat, points[currPointIndex].Lng);

                cumulativeDistanceKm += legDistanceKm;
                currentTime = currentTime.Add(HaversineHelper.EstimateTravelTime(legDistanceKm));

                var order = orders[currPointIndex - 1]; // -1 to offset the depot at index 0

                route.Stops.Add(new RouteStop
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

            if (route == null)
                throw new KeyNotFoundException("Route not found.");

            return MapToResponseDto(route);
        }

        // Solves the shortest path visiting all nodes starting from the depot (index 0)
        // using OR-Tools' RoutingModel — single vehicle, no return-to-depot requirement.
        private static List<int> SolveRoute(long[,] distanceMatrix, int nodeCount)
        {
            var manager = new RoutingIndexManager(nodeCount, 1, 0); // 1 vehicle, depot = node 0
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
                // Fallback: no solution found (shouldn't happen for small N), return nodes in original order
                for (int i = 0; i < nodeCount; i++) result.Add(i);
                return result;
            }

            var index = routing.Start(0);
            while (!routing.IsEnd(index))
            {
                result.Add(manager.IndexToNode(index));
                index = solution.Value(routing.NextVar(index));
            }
            result.Add(manager.IndexToNode(index)); // final node (back to depot, if closed tour)

            return result;
        }

        private static RouteResponseDto MapToResponseDto(FarmerMarketplace.Api.Models.Route route)
        {
            return new RouteResponseDto
            {
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
                CreatedAt = route.CreatedAt
            };
        }
    }
}
    public class RouteService : IRouteService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RouteService> _logger;

        // Service Times & Speed Constants
        private const int PickupServiceTimeMinutes = 15;
        private const int DeliveryServiceTimeMinutes = 10;
        private const double AvgSpeedKmPerHour = 40.0;
        private const double AvgSpeedMetersPerMinute = (AvgSpeedKmPerHour * 1000.0) / 60.0; // 666.6667 m/min

        // Shift & Trip Boundary Constants (Requirement 6)
        private const int MaxShiftDurationMinutes = 720; // 12 Hours
        private const long MaxShiftDurationSeconds = MaxShiftDurationMinutes * 60; // 43,200 Seconds

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

        public record PerishabilityInfo(string Tier, double MaxDeliveryHours);

        public static PerishabilityInfo GetPerishabilityInfo(ProductCategory? category, string? cropName)
        {
            string catStr = category?.ToString() ?? "";
            string nameStr = cropName ?? "";

            // Critical (24h): Leafy Vegetables, Mushrooms, Fresh Herbs, Flowers
            if (ContainsAny(catStr, nameStr, "Leafy", "Mushroom", "Herb", "Flower", "Spinach", "Methi", "Coriander", "Lettuce", "Mint"))
            {
                return new PerishabilityInfo("Critical", 24);
            }

            // High (48h): Tomatoes, Berries, Fresh Fruits, Dairy
            if (category == ProductCategory.Fruits || category == ProductCategory.Dairy ||
                ContainsAny(catStr, nameStr, "Tomato", "Berry", "Fruit", "Dairy", "Milk", "Butter", "Cheese", "Strawberry", "Mango", "Banana"))
            {
                return new PerishabilityInfo("High", 48);
            }

            // Medium (120h = 5 days): Root Vegetables, Onions, Potatoes, Eggs
            if (category == ProductCategory.Vegetables ||
                ContainsAny(catStr, nameStr, "Root", "Onion", "Potato", "Egg", "Carrot", "Radish", "Beetroot", "Vegetable"))
            {
                return new PerishabilityInfo("Medium", 120);
            }

            // Low (720h = 30 days): Grains, Pulses, Dry Spices, Rice, Wheat
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
                {
                    mostUrgent = info;
                }
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
                throw new ArgumentException("At least one order ID must be provided for optimization.", nameof(orderIds));

            // 1. Load orders with items, farmer, buyer, and product details
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

            // 4. Fleet sizing: ceiling of total kg / 2000, with a safety buffer
            int requiredVehicles = (int)Math.Ceiling(totalQuantityKg / 2000.0);
            int fleetSize = Math.Max(requiredVehicles, Math.Min(25, requiredVehicles + 1));

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

            int distanceCallbackIndex = routing.RegisterTransitCallback((fromIndex, toIndex) =>
            {
                int fromNode = manager.IndexToNode(fromIndex);
                int toNode = manager.IndexToNode(toIndex);
                return distanceMatrix[fromNode, toNode];
            });

            routing.SetArcCostEvaluatorOfAllVehicles(distanceCallbackIndex);
            routing.AddDimension(distanceCallbackIndex, 0, 10_000_000, true, "Distance");

            // 8. Add Time Dimension (travel time + service time in seconds)
            // Speed = 40 km/h = 100/9 m/s. Travel time = distance * 9 / 100 seconds.
            int timeCallbackIndex = routing.RegisterTransitCallback((fromIndex, toIndex) =>
            {
                int fromNode = manager.IndexToNode(fromIndex);
                int toNode = manager.IndexToNode(toIndex);

                long driveDistanceMeters = distanceMatrix[fromNode, toNode];
                long driveTimeSeconds = (driveDistanceMeters * 9) / 100;

                long serviceTimeSeconds = 0;
                if (fromNode != 0)
                {
                    bool isPickup = (fromNode % 2) != 0;
                    serviceTimeSeconds = isPickup ? (PickupServiceTimeMinutes * 60) : (DeliveryServiceTimeMinutes * 60);
                }

                return driveTimeSeconds + serviceTimeSeconds;
            });

            routing.AddDimension(timeCallbackIndex, 0, 86400 * 7, true, "Time");
            var timeDimension = routing.GetMutableDimension("Time");

            // Requirement 6: Enforce 4-Hour Shift / Trip Limit per vehicle
            for (int v = 0; v < fleetSize; v++)
            {
                timeDimension.CumulVar(routing.End(v)).SetMax(MaxShiftDurationSeconds);
            }

            // 9. Add Capacity Dimension (max 2000 kg per vehicle)
            long[] demands = new long[nodeCount];
            demands[0] = 0;
            for (int i = 0; i < pairCount; i++)
            {
                long q = (long)Math.Ceiling(chunkNodes[i].ChunkWeightKg);
                demands[1 + i * 2] = q;   // Pickup
                demands[2 + i * 2] = -q;  // Delivery
            }

            int demandCallbackIndex = routing.RegisterUnaryTransitCallback((fromIndex) =>
            {
                int fromNode = manager.IndexToNode(fromIndex);
                return demands[fromNode];
            });

            routing.AddDimension(
                demandCallbackIndex,
                0,
                2000,
                true,
                "Capacity");

            var solver = routing.solver();
            var capacityDim = routing.GetMutableDimension("Capacity");
            long disjunctionPenalty = 10_000_000;

            // 10. Hard Pickup-and-Delivery Precedence Constraints & Vehicle Disjunction Rules
            for (int i = 0; i < pairCount; i++)
            {
                int pickupNode = 1 + i * 2;
                int deliveryNode = 2 + i * 2;
                long pickupIdx = manager.NodeToIndex(pickupNode);
                long deliveryIdx = manager.NodeToIndex(deliveryNode);

                routing.AddPickupAndDelivery(pickupIdx, deliveryIdx);
                solver.Add(routing.VehicleVar(pickupIdx) == routing.VehicleVar(deliveryIdx));
                solver.Add(capacityDim.CumulVar(pickupIdx) <= capacityDim.CumulVar(deliveryIdx));

                // Hard Precedence Rule (Requirement 1): Delivery must occur after Pickup + Pickup Service Time
                solver.Add(timeDimension.CumulVar(pickupIdx) + (PickupServiceTimeMinutes * 60) <= timeDimension.CumulVar(deliveryIdx));

                // Soft Priority Constraint for Critical perishability tier
                if (chunkNodes[i].Perishability.Tier.Equals("Critical", StringComparison.OrdinalIgnoreCase))
                {
                    timeDimension.SetCumulVarSoftUpperBound(deliveryIdx, 10_800, 100);
                }

                routing.AddDisjunction(new[] { pickupIdx }, disjunctionPenalty);
                routing.AddDisjunction(new[] { deliveryIdx }, disjunctionPenalty);
            }

            routing.SetFixedCostOfAllVehicles(100_000);

            // 11. Search parameters with 10-second time limit
            var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
            searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
            searchParameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
            searchParameters.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = 10 };

            // 12. Solve
            var solution = routing.SolveWithParameters(searchParameters);
            if (solution == null)
            {
                _logger.LogWarning("Time limit reached or solver constraints tight — will apply Greedy Nearest-Neighbor fallback if needed.");
            }

            // 13. Extract Solution & Calculate ETAs including Travel + Service Times with Stop Merging
            DateTime routeStartTime = DateTime.UtcNow;
            var allStops = new List<RouteStop>();
            double totalDistanceMeters = 0;

            for (int vehicle = 0; vehicle < fleetSize; vehicle++)
            {
                long index = routing.Start(vehicle);
                int stopSeq = 1;
                double currentLoad = 0;

                (double Lat, double Lng) prevLoc = (depotLat, depotLng);
                DateTime currentDeparture = routeStartTime;
                var vehicleRawStops = new List<RouteStop>();

                if (solution != null)
                {
                    index = solution.Value(routing.NextVar(index));
                }

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

                        // Travel time + ETA calculation
                        double driveTimeMinutes = segDistanceMeters / AvgSpeedMetersPerMinute;
                        DateTime arrivalTime = currentDeparture.AddMinutes(driveTimeMinutes);

                        int serviceMinutes = isPickup ? PickupServiceTimeMinutes : DeliveryServiceTimeMinutes;
                        currentDeparture = arrivalTime.AddMinutes(serviceMinutes);

                        var perishability = chunkNode.Perishability;
                        DateTime deadline = routeStartTime.AddHours(perishability.MaxDeliveryHours);
                        bool isUrgent = arrivalTime > deadline;

                        var farmerName = order.Items.Select(i => i.Farmer?.Name).FirstOrDefault() ?? "Farmer";
                        var buyerName = order.Buyer?.Name ?? "Buyer";

                        string chunkLabel = chunkNode.TotalChunks > 1 ? $" (Part {chunkNode.ChunkIndex}/{chunkNode.TotalChunks})" : "";
                        string label = isPickup
                            ? $"{farmerName} (Pickup {q:0.#} kg){chunkLabel}"
                            : $"{buyerName} (Delivery {q:0.#} kg){chunkLabel}";

                        var stop = new RouteStop(
                            Sequence: stopSeq,
                            VehicleNumber: vehicle + 1,
                            Type: isPickup ? "pickup" : "delivery",
                            Label: label,
                            Lat: lat,
                            Lng: lng,
                            OrderId: order.Id,
                            QuantityAtStop: Math.Round(currentLoad, 1),
                            EstimatedArrival: arrivalTime,
                            PerishabilityTier: perishability.Tier,
                            DeliveryDeadline: deadline,
                            IsUrgent: isUrgent
                        );

                        // Merge consecutive stops at exact same physical location & type if any exist
                        if (vehicleRawStops.Count > 0)
                        {
                            var prevStop = vehicleRawStops.Last();
                            if (prevStop.Type == stop.Type && GeoUtils.DistanceInMeters(prevStop.Lat, prevStop.Lng, stop.Lat, stop.Lng) < 10)
                            {
                                double mergedQty = prevStop.QuantityAtStop + (isPickup ? q : -q);
                                string mergedLabel = $"{prevStop.Label} & {stop.Label}";
                                var mergedStop = prevStop with
                                {
                                    QuantityAtStop = Math.Round(mergedQty, 1),
                                    Label = mergedLabel
                                };
                                vehicleRawStops[vehicleRawStops.Count - 1] = mergedStop;

                                if (!isPickup) currentLoad -= q;
                                if (solution != null) index = solution.Value(routing.NextVar(index));
                                else break;
                                continue;
                            }
                        }

                        vehicleRawStops.Add(stop);

                        // Update Order attributes
                        order.VehicleNumber = vehicle + 1;
                        order.StopSequence = stopSeq;
                        if (!isPickup)
                        {
                            order.EstimatedArrival = arrivalTime;
                            currentLoad -= q;
                        }

                        stopSeq++;
                    }

                    if (solution != null)
                    {
                        index = solution.Value(routing.NextVar(index));
                    }
                    else
                    {
                        break;
                    }
                }

                allStops.AddRange(vehicleRawStops);

                if (stopSeq > 1)
                {
                    totalDistanceMeters += GeoUtils.DistanceInMeters(prevLoc.Lat, prevLoc.Lng, depotLat, depotLng);
                }
            }

            // GUARANTEE: If OR-Tools solution was null or produced 0 stops, execute Greedy Nearest-Neighbor VRP Fallback
            if (solution == null || !allStops.Any())
            {
                _logger.LogWarning("Executing Greedy Nearest-Neighbor VRP fallback to ensure all orders are routed.");
                allStops.Clear();
                totalDistanceMeters = 0;

                int vNum = 1;
                double currentVehLoad = 0;
                (double Lat, double Lng) curLoc = (depotLat, depotLng);
                DateTime curDeparture = routeStartTime;
                int seq = 1;

                foreach (var chunkNode in chunkNodes)
                {
                    double q = chunkNode.ChunkWeightKg;
                    if (currentVehLoad + q > 2000)
                    {
                        totalDistanceMeters += GeoUtils.DistanceInMeters(curLoc.Lat, curLoc.Lng, depotLat, depotLng);
                        vNum++;
                        currentVehLoad = 0;
                        curLoc = (depotLat, depotLng);
                        curDeparture = routeStartTime;
                        seq = 1;
                    }

                    var order = chunkNode.Order;
                    var perishability = chunkNode.Perishability;
                    var farmerName = order.Items.Select(i => i.Farmer?.Name).FirstOrDefault() ?? "Farmer";
                    var buyerName = order.Buyer?.Name ?? "Buyer";
                    string chunkLabel = chunkNode.TotalChunks > 1 ? $" (Part {chunkNode.ChunkIndex}/{chunkNode.TotalChunks})" : "";

                    // Pickup
                    double pickupLat = order.PickupLat!.Value;
                    double pickupLng = order.PickupLng!.Value;
                    double pickupDist = GeoUtils.DistanceInMeters(curLoc.Lat, curLoc.Lng, pickupLat, pickupLng);
                    totalDistanceMeters += pickupDist;
                    curLoc = (pickupLat, pickupLng);
                    DateTime pickupArrival = curDeparture.AddMinutes(pickupDist / AvgSpeedMetersPerMinute);
                    curDeparture = pickupArrival.AddMinutes(PickupServiceTimeMinutes);
                    currentVehLoad += q;

                    var pickupStop = new RouteStop(
                        Sequence: seq++,
                        VehicleNumber: vNum,
                        Type: "pickup",
                        Label: $"{farmerName} (Pickup {q:0.#} kg){chunkLabel}",
                        Lat: pickupLat,
                        Lng: pickupLng,
                        OrderId: order.Id,
                        QuantityAtStop: Math.Round(currentVehLoad, 1),
                        EstimatedArrival: pickupArrival,
                        PerishabilityTier: perishability.Tier,
                        DeliveryDeadline: routeStartTime.AddHours(perishability.MaxDeliveryHours),
                        IsUrgent: false
                    );
                    allStops.Add(pickupStop);

                    // Delivery
                    double deliveryLat = order.DeliveryLat!.Value;
                    double deliveryLng = order.DeliveryLng!.Value;
                    double deliveryDist = GeoUtils.DistanceInMeters(curLoc.Lat, curLoc.Lng, deliveryLat, deliveryLng);
                    totalDistanceMeters += deliveryDist;
                    curLoc = (deliveryLat, deliveryLng);
                    DateTime deliveryArrival = curDeparture.AddMinutes(deliveryDist / AvgSpeedMetersPerMinute);
                    curDeparture = deliveryArrival.AddMinutes(DeliveryServiceTimeMinutes);

                    DateTime deadline = routeStartTime.AddHours(perishability.MaxDeliveryHours);
                    bool isUrgent = deliveryArrival > deadline;

                    var deliveryStop = new RouteStop(
                        Sequence: seq++,
                        VehicleNumber: vNum,
                        Type: "delivery",
                        Label: $"{buyerName} (Delivery {q:0.#} kg){chunkLabel}",
                        Lat: deliveryLat,
                        Lng: deliveryLng,
                        OrderId: order.Id,
                        QuantityAtStop: Math.Round(currentVehLoad, 1),
                        EstimatedArrival: deliveryArrival,
                        PerishabilityTier: perishability.Tier,
                        DeliveryDeadline: deadline,
                        IsUrgent: isUrgent
                    );
                    allStops.Add(deliveryStop);

                    currentVehLoad -= q;
                    order.VehicleNumber = vNum;
                    order.StopSequence = seq - 1;
                    order.EstimatedArrival = deliveryArrival;
                }

                totalDistanceMeters += GeoUtils.DistanceInMeters(curLoc.Lat, curLoc.Lng, depotLat, depotLng);
            }

            // Renumber active vehicle numbers sequentially starting from 1 (Vehicle #1, Vehicle #2, etc.)
            var vehicleMapping = allStops
                .Select(s => s.VehicleNumber)
                .Distinct()
                .Select((v, idx) => (OldV: v, NewV: idx + 1))
                .ToDictionary(x => x.OldV, x => x.NewV);

            var remappedStops = allStops.Select(s => s with { VehicleNumber = vehicleMapping[s.VehicleNumber] }).ToList();

            // Update VehicleNumber on validOrders
            foreach (var order in validOrders)
            {
                var stopForOrder = remappedStops.FirstOrDefault(s => s.OrderId == order.Id);
                if (stopForOrder != null)
                {
                    order.VehicleNumber = stopForOrder.VehicleNumber;
                }
            }

            double totalDistanceKm = Math.Round(totalDistanceMeters / 1000.0, 2);
            int activeVehicleCount = remappedStops.Select(s => s.VehicleNumber).Distinct().Count();

            // 14. Persist DeliveryRoute entity
            var deliveryRoute = new DeliveryRoute
            {
                Id = Guid.NewGuid(),
                BatchDate = DateTime.UtcNow,
                BatchWindow = batchWindow,
                VehicleCount = activeVehicleCount,
                TotalDistanceKm = totalDistanceKm,
                StopsJson = JsonSerializer.Serialize(remappedStops),
                Status = DeliveryRouteStatus.Generated,
                CreatedAt = DateTime.UtcNow
            };

            _context.DeliveryRoutes.Add(deliveryRoute);

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

        public async Task<List<DeliveryRoute>> GetAllRoutesAsync()
        {
            return await _context.DeliveryRoutes
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
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
