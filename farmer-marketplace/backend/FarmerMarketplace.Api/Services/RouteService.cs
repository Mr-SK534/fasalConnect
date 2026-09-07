// backend/FarmerMarketplace.Api/Services/RouteService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;
using Google.OrTools.ConstraintSolver;
using System.Text.Json;
using System.Globalization;

namespace FarmerMarketplace.Api.Services
{
    public class RouteService : IRouteService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RouteService> _logger;

        // Default hub coordinates per Indian state (state capital / major city).
        // Falls back to geographic centre of India for unknown states.
        private static readonly Dictionary<string, (double Lat, double Lng)> StateHubs = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Andhra Pradesh"]     = (17.3850, 78.4867),
            ["Arunachal Pradesh"]  = (27.0844, 93.6053),
            ["Assam"]              = (26.1433, 91.7898),
            ["Bihar"]              = (25.5941, 85.1376),
            ["Chhattisgarh"]       = (21.2514, 81.6296),
            ["Goa"]                = (15.2993, 74.1240),
            ["Gujarat"]            = (23.0225, 72.5714),
            ["Haryana"]            = (29.0588, 76.0856),
            ["Himachal Pradesh"]   = (31.1048, 77.1734),
            ["Jharkhand"]          = (23.3441, 85.3096),
            ["Karnataka"]          = (12.9716, 77.5946),
            ["Kerala"]             = (8.5241,  76.9366),
            ["Madhya Pradesh"]     = (23.2599, 77.4126),
            ["Maharashtra"]        = (19.0760, 72.8777),
            ["Manipur"]            = (24.6637, 93.9063),
            ["Meghalaya"]          = (25.5788, 91.8933),
            ["Mizoram"]            = (23.1645, 92.9376),
            ["Nagaland"]           = (25.6751, 94.1086),
            ["Odisha"]             = (20.2961, 85.8245),
            ["Punjab"]             = (30.7333, 76.7794),
            ["Rajasthan"]          = (26.9124, 75.7873),
            ["Sikkim"]             = (27.3314, 88.6138),
            ["Tamil Nadu"]         = (13.0827, 80.2707),
            ["Telangana"]          = (17.3850, 78.4867),
            ["Tripura"]            = (23.9408, 91.9882),
            ["Uttar Pradesh"]      = (26.8467, 80.9462),
            ["Uttarakhand"]        = (30.3165, 78.0322),
            ["West Bengal"]        = (22.5726, 88.3639),
            ["Delhi"]              = (28.6139, 77.2090),
            ["Jammu and Kashmir"]  = (34.0837, 74.7973),
            ["Ladakh"]             = (34.1526, 77.5770),
        };

        private static (double Lat, double Lng) GetHubForState(string? state) =>
            !string.IsNullOrWhiteSpace(state) && StateHubs.TryGetValue(state.Trim(), out var hub)
                ? hub
                : (20.5937, 78.9629); // geographic centre of India

        public RouteService(AppDbContext context, IHttpClientFactory httpClientFactory, ILogger<RouteService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public API: admin-triggered single optimization
        // ─────────────────────────────────────────────────────────────────────

        public async Task<List<RouteResponseDto>> OptimizeAsync(Guid adminId, RouteOptimizeDto dto)
        {
            if (dto.OrderIds == null || dto.OrderIds.Count < 2)
                throw new ArgumentException("At least 2 orders are needed to optimize a route.");

            var orders = await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Farmer)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Where(o => dto.OrderIds.Contains(o.Id))
                .ToListAsync();

            if (orders.Count != dto.OrderIds.Distinct().Count())
                throw new KeyNotFoundException("One or more selected orders were not found.");

            // All selected orders must be Confirmed Delivery type
            var invalid = orders.FirstOrDefault(o =>
                o.Status != OrderStatus.Confirmed || o.DeliveryType != DeliveryType.Delivery);
            if (invalid != null)
                throw new InvalidOperationException("All selected orders must be confirmed delivery orders.");

            // Geocode delivery addresses for any order missing coordinates
            foreach (var order in orders.Where(o => !o.Latitude.HasValue || !o.Longitude.HasValue))
            {
                var coords = await GeocodeAddressAsync(order.DeliveryAddress);
                if (!coords.HasValue)
                    throw new InvalidOperationException(
                        $"Order '{order.Id}' needs a delivery address that can be mapped before it can be routed.");
                order.Latitude  = coords.Value.Lat;
                order.Longitude = coords.Value.Lng;
            }
            await _context.SaveChangesAsync();

            var (routes, _) = await BuildAndPersistRouteAsync(
                adminId, orders,
                dto.DeliveryHubLocation.Lat, dto.DeliveryHubLocation.Lng,
                skipReasons: null,
                cancellationToken: CancellationToken.None);

            return routes.Select(Map).ToList();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public API: GET /routes/{id}
        // ─────────────────────────────────────────────────────────────────────

        public async Task<RouteResponseDto> GetByIdAsync(Guid routeId)
        {
            var route = await _context.Routes
                .AsNoTracking()
                .Include(r => r.Stops)
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
                throw new KeyNotFoundException("Route not found.");

            return Map(route);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public API: GET /routes/batch-status
        // ─────────────────────────────────────────────────────────────────────

        public async Task<BatchStatusDto> GetBatchStatusAsync()
        {
            var lastLog = await _context.BatchRunLogs
                .AsNoTracking()
                .OrderByDescending(l => l.RanAt)
                .FirstOrDefaultAsync();

            // Count confirmed delivery orders that have no RouteStop assigned
            var routedOrderIds = await _context.RouteStops
                .Select(s => s.OrderId)
                .Distinct()
                .ToListAsync();

            var unrouted = await _context.Orders
                .CountAsync(o =>
                    o.Status == OrderStatus.Confirmed &&
                    o.DeliveryType == DeliveryType.Delivery &&
                    !routedOrderIds.Contains(o.Id));

            var nowIst = ToIst(DateTime.UtcNow);
            var nextBatchIst = GetNextBatchTime(nowIst);

            return new BatchStatusDto
            {
                NextBatchTime          = FromIst(nextBatchIst),
                LastBatchTime          = lastLog?.RanAt,
                LastBatchOrderCount    = lastLog?.OrdersRouted ?? 0,
                LastBatchRouteCount    = lastLog?.RoutesCreated ?? 0,
                UnroutedConfirmedOrderCount = unrouted,
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public API: POST /routes/batch/run  (manual trigger)
        // ─────────────────────────────────────────────────────────────────────

        public async Task<BatchRunResultDto> RunBatchNowAsync()
        {
            return await ExecuteBatchAsync(isManual: true, CancellationToken.None);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Internal: core batch execution (called by both background service
        // and the manual trigger endpoint)
        // ─────────────────────────────────────────────────────────────────────

        public async Task<BatchRunResultDto> ExecuteBatchAsync(bool isManual, CancellationToken ct)
        {
            var result = new BatchRunResultDto();

            try
            {
                _logger.LogInformation("Route batch starting (manual={IsManual})", isManual);

                // Find all confirmed delivery orders not yet assigned to a route stop
                var routedOrderIds = await _context.RouteStops
                    .Select(s => s.OrderId)
                    .Distinct()
                    .ToListAsync(ct);

                var allOrders = await _context.Orders
                    .Include(o => o.Items).ThenInclude(i => i.Farmer)
                    .Include(o => o.Items).ThenInclude(i => i.Product)
                    .Where(o =>
                        o.Status == OrderStatus.Confirmed &&
                        o.DeliveryType == DeliveryType.Delivery &&
                        !routedOrderIds.Contains(o.Id))
                    .ToListAsync(ct);

                if (!allOrders.Any())
                {
                    _logger.LogInformation("Batch: no unrouted confirmed delivery orders found.");
                    await PersistBatchLog(isManual, 0, 0, 0, ct);
                    return result;
                }

                // Filter out orders with missing delivery coordinates; geocode where possible
                var validOrders = new List<Order>();
                foreach (var order in allOrders)
                {
                    if (ct.IsCancellationRequested) break;

                    if (!order.Latitude.HasValue || !order.Longitude.HasValue)
                    {
                        if (!string.IsNullOrWhiteSpace(order.DeliveryAddress))
                        {
                            await Task.Delay(500, ct); // Nominatim rate-limit
                            var coords = await GeocodeAddressAsync(order.DeliveryAddress);
                            if (coords.HasValue)
                            {
                                order.Latitude  = coords.Value.Lat;
                                order.Longitude = coords.Value.Lng;
                                validOrders.Add(order);
                                continue;
                            }
                        }
                        result.SkippedOrders++;
                        result.SkippedReasons.Add(
                            $"Order {order.Id.ToString()[..8]}: delivery address could not be geocoded.");
                        continue;
                    }

                    // Check that at least one farmer has coordinates
                    var farmer = order.Items.Select(i => i.Farmer).FirstOrDefault(f => f != null);
                    if (farmer?.Latitude == null || farmer.Longitude == null)
                    {
                        result.SkippedOrders++;
                        result.SkippedReasons.Add(
                            $"Order {order.Id.ToString()[..8]}: farmer has no coordinates.");
                        continue;
                    }

                    validOrders.Add(order);
                }

                if (validOrders.Any())
                    await _context.SaveChangesAsync(ct);

                // Group by physical proximity (50km radius clustering)
                var clusters = ClusterOrders(validOrders, 50.0);

                int clusterIndex = 1;
                foreach (var cluster in clusters)
                {
                    if (ct.IsCancellationRequested) break;
                    if (cluster.Count < 2)
                    {
                        // Can't form a route with 1 order — skip
                        result.SkippedOrders += cluster.Count;
                        foreach (var o in cluster)
                            result.SkippedReasons.Add(
                                $"Order {o.Id.ToString()[..8]}: not enough nearby orders to form a route.");
                        continue;
                    }

                    try
                    {
                        // Find most common state in the cluster to select the hub
                        var state = cluster.SelectMany(o => o.Items.Select(i => i.Farmer?.State)).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? "";
                        var hub = GetHubForState(state);
                        
                        var (routes, skips) = await BuildAndPersistRouteAsync(
                            Guid.Empty, cluster,
                            hub.Lat, hub.Lng,
                            skipReasons: result.SkippedReasons,
                            cancellationToken: ct);

                        result.RoutesCreated += routes.Count;
                        result.OrdersRouted += routes.SelectMany(r => r.Stops).Select(s => s.OrderId).Distinct().Count();
                        result.SkippedOrders += skips;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Batch: could not optimize cluster {Index} ({Count} orders).",
                            clusterIndex, cluster.Count);
                        result.SkippedOrders += cluster.Count;
                        foreach (var o in cluster)
                            result.SkippedReasons.Add(
                                $"Order {o.Id.ToString()[..8]}: route optimization failed — {ex.Message}");
                    }
                    clusterIndex++;
                }

                await PersistBatchLog(isManual, result.OrdersRouted, result.RoutesCreated, result.SkippedOrders, ct);
                _logger.LogInformation(
                    "Batch complete. Routes={Routes}, Orders={Orders}, Skipped={Skipped}",
                    result.RoutesCreated, result.OrdersRouted, result.SkippedOrders);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Batch execution failed unexpectedly.");
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Core VRP: build node list → distance matrix → OR-Tools → persist
        // ─────────────────────────────────────────────────────────────────────

        private async Task<(List<FarmerMarketplace.Api.Models.Route> Routes, int SkippedCount)>
            BuildAndPersistRouteAsync(
                Guid createdBy,
                List<Order> orders,
                double hubLat, double hubLng,
                List<string>? skipReasons,
                CancellationToken cancellationToken)
        {
            var nodes = new List<RouteNodeDto>();
            var eligibleOrders = new List<Order>();

            foreach (var order in orders)
            {
                var farmers = order.Items
                    .Where(i => i.Farmer != null)
                    .Select(i => i.Farmer!)
                    .GroupBy(f => f.Id)
                    .Select(g => g.First())
                    .ToList();

                foreach (var farmer in farmers)
                {
                    if (!farmer.Latitude.HasValue || !farmer.Longitude.HasValue)
                    {
                        var farmerAddress = BuildProfileAddress(farmer);
                        var coords = await GeocodeAddressAsync(farmerAddress);
                        if (coords.HasValue)
                        {
                            farmer.Latitude = coords.Value.Lat;
                            farmer.Longitude = coords.Value.Lng;
                        }
                    }
                }

                farmers = farmers.Where(f => f.Latitude.HasValue && f.Longitude.HasValue).ToList();

                if (!farmers.Any())
                {
                    skipReasons?.Add($"Order {order.Id.ToString()[..8]}: no farmers have coordinates.");
                    continue;
                }
                if (!order.Latitude.HasValue || !order.Longitude.HasValue)
                {
                    skipReasons?.Add($"Order {order.Id.ToString()[..8]}: delivery address could not be geocoded.");
                    continue;
                }

                long maxCapacityForChunking = 2000;
                foreach (var farmer in farmers)
                {
                    var pickupAddress = BuildProfileAddress(farmer);
                    
                    // Calculate weight specifically for this farmer's items in the order
                    long totalWeightKg = 0;
                    foreach (var item in order.Items.Where(i => i.FarmerId == farmer.Id))
                    {
                        decimal multiplier = item.Product?.Unit switch
                        {
                            ProductUnit.Quintal => 100m,
                            ProductUnit.Ton => 1000m,
                            _ => 1m
                        };
                        totalWeightKg += (long)(item.Quantity * multiplier);
                    }
                    if (totalWeightKg <= 0) totalWeightKg = 1; // avoid zero demand

                    int chunks = (int)Math.Ceiling((double)totalWeightKg / maxCapacityForChunking);
                    long remainingWeight = totalWeightKg;

                    for (int c = 0; c < chunks; c++)
                    {
                        long chunkWeight = Math.Min(remainingWeight, maxCapacityForChunking);
                        remainingWeight -= chunkWeight;

                        nodes.Add(new RouteNodeDto
                        {
                            OrderId    = order.Id,
                            IsPickup   = true,
                            Latitude   = farmer.Latitude!.Value,
                            Longitude  = farmer.Longitude!.Value,
                            Address    = pickupAddress,
                            FarmerName = farmer.Name,
                            Demand     = chunkWeight
                        });
                        nodes.Add(new RouteNodeDto
                        {
                            OrderId    = order.Id,
                            IsPickup   = false,
                            Latitude   = order.Latitude!.Value,
                            Longitude  = order.Longitude!.Value,
                            Address    = order.DeliveryAddress,
                            FarmerName = farmer.Name,
                            Demand     = chunkWeight
                        });
                    }
                }
                eligibleOrders.Add(order);
            }

            int skippedCount = orders.Count - eligibleOrders.Count;

            if (eligibleOrders.Count < 2)
                throw new InvalidOperationException(
                    "Could not find a valid route for the given orders. " +
                    "Ensure all orders have valid coordinates.");

            int nodeCount = 1 + nodes.Count;
            var allLocs = new List<(double Lat, double Lng)> { (hubLat, hubLng) };
            allLocs.AddRange(nodes.Select(n => (n.Latitude, n.Longitude)));

            var (distancesKm, durationsSeconds) = await GetRoadMatrixAsync(allLocs);

            long maxCapacity = 2000; // 2000 kg capacity
            long[] demands = new long[nodeCount];
            long totalDemand = 0;

            // Depot demand is 0.
            for (int i = 0; i < nodes.Count; i++)
            {
                var routeNode = nodes[i];
                demands[i + 1] = routeNode.IsPickup ? routeNode.Demand : -routeNode.Demand;
                if (routeNode.IsPickup) totalDemand += routeNode.Demand;
            }

            int requiredVehicles = (int)Math.Ceiling((double)totalDemand / maxCapacity);
            int fleetSize = Math.Max(5, requiredVehicles + 2); // Dynamic fleet size with a small buffer
            if (fleetSize > 25) fleetSize = 25; // Cap at 25 trucks to bound solver complexity

            var manager = new RoutingIndexManager(nodeCount, fleetSize, 0);
            var routing = new RoutingModel(manager);

            // 1. Time / Service Time Dimension (instead of just distance)
            int timeTransitIdx = routing.RegisterTransitCallback((from, to) =>
            {
                int fromNode = manager.IndexToNode(from);
                int toNode = manager.IndexToNode(to);
                long drivingTime = durationsSeconds[fromNode, toNode];
                // 15 mins (900s) service time at every stop, except the depot
                long serviceTime = fromNode == 0 ? 0 : 900;
                return drivingTime + serviceTime;
            });
            routing.SetArcCostEvaluatorOfAllVehicles(timeTransitIdx);
            // Relax max time to avoid failures on long routes across India
            routing.AddDimension(timeTransitIdx, 3600, 7 * 24 * 3600, true, "Time"); 
            var timeDim = routing.GetMutableDimension("Time");

            // 2. Capacity Dimension
            int demandCallbackIndex = routing.RegisterUnaryTransitCallback((fromIndex) =>
            {
                int fromNode = manager.IndexToNode(fromIndex);
                return demands[fromNode];
            });
            long[] vehicleCapacities = Enumerable.Repeat(maxCapacity, fleetSize).ToArray();
            routing.AddDimensionWithVehicleCapacity(demandCallbackIndex, 0, vehicleCapacities, true, "Capacity");

            var solver = routing.solver();
            int totalPairs = nodes.Count / 2;
            long penalty = 10000000; // High penalty for dropping a node

            for (int i = 0; i < totalPairs; i++)
            {
                int pickupNode   = 1 + i * 2;
                int deliveryNode = 2 + i * 2;
                var pickupIdx   = manager.NodeToIndex(pickupNode);
                var deliveryIdx = manager.NodeToIndex(deliveryNode);

                routing.AddPickupAndDelivery(pickupIdx, deliveryIdx);
                solver.Add(routing.VehicleVar(pickupIdx) == routing.VehicleVar(deliveryIdx));
                // Pickup must happen before delivery
                solver.Add(timeDim.CumulVar(pickupIdx) <= timeDim.CumulVar(deliveryIdx));
                
                // Allow the solver to drop unroutable nodes (e.g. if > 2000kg)
                routing.AddDisjunction(new[] { pickupIdx }, penalty);
                routing.AddDisjunction(new[] { deliveryIdx }, penalty);
            }

            var searchParams = operations_research_constraint_solver.DefaultRoutingSearchParameters();
            searchParams.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
            searchParams.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
            searchParams.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = 10 };

            var solution = routing.SolveWithParameters(searchParams);
            if (solution == null)
                throw new InvalidOperationException(
                    "Could not find a valid route for the given orders within capacity constraints.");

            var createdRoutes = new List<FarmerMarketplace.Api.Models.Route>();
            var pickupDay = DateTime.UtcNow.Date;

            for (int vehicle = 0; vehicle < fleetSize; vehicle++)
            {
                var idx = routing.Start(vehicle);
                if (routing.IsEnd(solution.Value(routing.NextVar(idx))))
                    continue; // Vehicle not used

                var sequence = new List<int>();
                idx = solution.Value(routing.NextVar(idx)); // skip start depot
                while (!routing.IsEnd(idx))
                {
                    int node = manager.IndexToNode(idx);
                    if (node != 0) sequence.Add(node);
                    idx = solution.Value(routing.NextVar(idx));
                }

                if (!sequence.Any()) continue;

                var route = new FarmerMarketplace.Api.Models.Route
                {
                    CreatedBy      = createdBy,
                    DeliveryHubLat = hubLat,
                    DeliveryHubLng = hubLng,
                };

                int seq = 1;
                double cumulativeKm = 0;
                long cumulativeSeconds = 0;
                int prev = 0; // Depot

                foreach (int node in sequence)
                {
                    var routeNode  = nodes[node - 1];
                    var orderObj   = eligibleOrders.First(o => o.Id == routeNode.OrderId);
                    double distKm  = distancesKm[prev, node];
                    long durSec    = durationsSeconds[prev, node] + (prev == 0 ? 0 : 900);
                    
                    cumulativeKm  += distKm;
                    cumulativeSeconds += durSec;

                    if (!routeNode.IsPickup && route.Stops.Any())
                    {
                        var lastStop = route.Stops.Last();
                        if (lastStop.OrderId == routeNode.OrderId && lastStop.StopType == "Delivery")
                        {
                            prev = node;
                            continue; // Skip duplicate delivery nodes for same order
                        }
                    }

                    var eta = DateTime.UtcNow.AddSeconds(cumulativeSeconds);
                    var deliveryDate = pickupDay.AddDays(IsPerishable(orderObj) ? 1 : 2);

                    route.Stops.Add(new RouteStop
                    {
                        OrderId                = routeNode.OrderId,
                        StopSequence           = seq++,
                        Latitude               = routeNode.Latitude,
                        Longitude              = routeNode.Longitude,
                        DistanceFromPreviousKm = Convert.ToDecimal(distKm),
                        EstimatedArrival       = eta,
                        StopType               = routeNode.IsPickup ? "Pickup" : "Delivery",
                        Address                = routeNode.Address,
                        FarmerName             = routeNode.FarmerName,
                        PickupDate             = pickupDay,
                        DeliveryDate           = deliveryDate,
                    });

                    prev = node;
                }

                _context.Routes.Add(route);
                createdRoutes.Add(route);
            }

            // Calculate how many eligible orders were dropped by the solver
            var routedOrderIds = createdRoutes.SelectMany(r => r.Stops).Select(s => s.OrderId).Distinct().ToList();
            skippedCount += eligibleOrders.Count - routedOrderIds.Count;

            await _context.SaveChangesAsync(cancellationToken);
            return (createdRoutes, skippedCount);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Mapping
        // ─────────────────────────────────────────────────────────────────────

        private static RouteResponseDto Map(FarmerMarketplace.Api.Models.Route route) => new()
        {
            RouteId          = route.Id,
            DeliveryHubLat   = route.DeliveryHubLat,
            DeliveryHubLng   = route.DeliveryHubLng,
            CreatedAt        = route.CreatedAt,
            Stops = route.Stops
                .OrderBy(s => s.StopSequence)
                .Select(s => new RouteStopResponseDto
                {
                    OrderId                = s.OrderId,
                    StopSequence           = s.StopSequence,
                    Latitude               = s.Latitude,
                    Longitude              = s.Longitude,
                    DistanceFromPreviousKm = s.DistanceFromPreviousKm,
                    EstimatedArrival       = s.EstimatedArrival,
                    StopType               = s.StopType,
                    Address                = s.Address,
                    FarmerName             = s.FarmerName,
                    PickupDate             = s.PickupDate,
                    DeliveryDate           = s.DeliveryDate,
                })
                .ToList(),
        };

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Haversine great-circle distance in km.</summary>
        public static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0;
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0)
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static bool IsPerishable(Order order) =>
            order.Items.Any(i =>
                i.Product?.Category is
                    ProductCategory.Vegetables or
                    ProductCategory.Fruits or
                    ProductCategory.Dairy);

        private static string? BuildProfileAddress(User? farmer)
        {
            if (farmer == null) return null;
            var addressParts = new[] {
                farmer.Address, farmer.District, farmer.State, farmer.Pincode,
            }.Where(p => !string.IsNullOrWhiteSpace(p));
            return string.Join(", ", addressParts.Distinct());
        }

        private static DateTime ToIst(DateTime utc)
        {
            var ist = TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, ist);
        }

        private static DateTime FromIst(DateTime ist)
        {
            var istZone = TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata");
            return TimeZoneInfo.ConvertTimeToUtc(ist, istZone);
        }

        internal static DateTime GetNextBatchTime(DateTime nowIst)
        {
            var today8am  = nowIst.Date.AddHours(8);
            var today2pm  = nowIst.Date.AddHours(14);
            if (nowIst < today8am)  return today8am;
            if (nowIst < today2pm)  return today2pm;
            return nowIst.Date.AddDays(1).AddHours(8);
        }

        private async Task PersistBatchLog(bool isManual, int ordersRouted, int routesCreated, int skipped, CancellationToken ct)
        {
            _context.BatchRunLogs.Add(new BatchRunLog
            {
                RanAt         = DateTime.UtcNow,
                IsManual      = isManual,
                OrdersRouted  = ordersRouted,
                RoutesCreated = routesCreated,
                SkippedOrders = skipped,
            });
            await _context.SaveChangesAsync(ct);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Clustering
        // ─────────────────────────────────────────────────────────────────────
        
        private static List<List<Order>> ClusterOrders(List<Order> orders, double maxDistanceKm = 50.0)
        {
            var clusters = new List<List<Order>>();
            var unassigned = new HashSet<Order>(orders);

            while (unassigned.Count > 0)
            {
                var current = unassigned.First();
                var cluster = new List<Order> { current };
                unassigned.Remove(current);

                var queue = new Queue<Order>();
                queue.Enqueue(current);

                while (queue.Count > 0)
                {
                    var pivot = queue.Dequeue();
                    var pivotLocs = GetOrderLocations(pivot);

                    var neighbors = unassigned.Where(o => 
                        GetOrderLocations(o).Any(loc1 => 
                            pivotLocs.Any(loc2 => HaversineKm(loc1.Lat, loc1.Lng, loc2.Lat, loc2.Lng) <= maxDistanceKm)))
                        .ToList();

                    foreach (var neighbor in neighbors)
                    {
                        cluster.Add(neighbor);
                        unassigned.Remove(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }

                clusters.Add(cluster);
            }

            return clusters;
        }

        private static List<(double Lat, double Lng)> GetOrderLocations(Order order)
        {
            var locs = new List<(double Lat, double Lng)>();
            if (order.Latitude.HasValue && order.Longitude.HasValue)
                locs.Add((order.Latitude.Value, order.Longitude.Value));
            
            foreach (var item in order.Items)
            {
                if (item.Farmer?.Latitude.HasValue == true && item.Farmer?.Longitude.HasValue == true)
                    locs.Add((item.Farmer.Latitude.Value, item.Farmer.Longitude.Value));
            }
            return locs;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Road distance matrix (OSRM) with Haversine fallback
        // ─────────────────────────────────────────────────────────────────────

        private async Task<(double[,] DistancesKm, long[,] DurationsSeconds)>
            GetRoadMatrixAsync(List<(double Lat, double Lng)> locations)
        {
            int n = locations.Count;
            var fallbackDist = new double[n, n];
            var fallbackDur  = new long[n, n];
            for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                fallbackDist[i, j] = HaversineKm(locations[i].Lat, locations[i].Lng,
                                                  locations[j].Lat, locations[j].Lng);
                fallbackDur[i, j]  = (long)Math.Max(300, fallbackDist[i, j] / 30.0 * 3600.0);
            }

            try
            {
                var coords = string.Join(";", locations.Select(l =>
                    $"{l.Lng.ToString(CultureInfo.InvariantCulture)},{l.Lat.ToString(CultureInfo.InvariantCulture)}"));

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("FasalConnect/1.0 route-planner");

                using var response = await client.GetAsync(
                    $"https://router.project-osrm.org/table/v1/driving/{coords}?annotations=distance,duration");
                response.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var distances = doc.RootElement.GetProperty("distances");
                var durations = doc.RootElement.GetProperty("durations");

                var roadDist = new double[n, n];
                var roadDur  = new long[n, n];
                for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    roadDist[i, j] = distances[i][j].GetDouble() / 1000.0;
                    roadDur[i, j]  = (long)Math.Max(300, durations[i][j].GetDouble());
                }
                return (roadDist, roadDur);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Road matrix lookup failed; falling back to Haversine estimates.");
                return (fallbackDist, fallbackDur);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Nominatim geocoder (shared between on-demand and batch)
        // ─────────────────────────────────────────────────────────────────────

        private async Task<(double Lat, double Lng)?> GeocodeAddressAsync(string? address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            var normalized = address.Trim();
            var postalCode = System.Text.RegularExpressions.Regex.Match(normalized, @"\b\d{6}\b").Value;
            var locality   = System.Text.RegularExpressions.Regex.Replace(
                normalized,
                @"^(flat|floor|house|plot|door|unit)\s+[^,]+,?\s*",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

            var candidates = new[]
            {
                normalized,
                $"{normalized}, India",
                locality,
                $"{locality}, India",
                string.IsNullOrWhiteSpace(postalCode) ? null : $"{postalCode}, India",
            }
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!)
            .Distinct(StringComparer.OrdinalIgnoreCase);

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "FasalConnect/1.0 (route-planner; contact: admin@fasalconnect.local)");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-IN,en;q=0.8");

            foreach (var candidate in candidates)
            {
                try
                {
                    var url = $"https://nominatim.openstreetmap.org/search" +
                              $"?format=jsonv2&addressdetails=1&limit=1&countrycodes=in" +
                              $"&q={Uri.EscapeDataString(candidate)}";
                    using var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode) continue;

                    using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    var result = doc.RootElement.EnumerateArray().FirstOrDefault();
                    if (result.ValueKind == JsonValueKind.Undefined) continue;

                    var latStr = result.TryGetProperty("lat", out var lp) ? lp.GetString() : null;
                    var lngStr = result.TryGetProperty("lon", out var lnp) ? lnp.GetString() : null;
                    if (double.TryParse(latStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)
                     && double.TryParse(lngStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
                        return (lat, lng);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Geocoding failed for candidate {Address}", candidate);
                }
            }

            _logger.LogWarning("No map result found for delivery address {Address}", address);
            return null;
        }
    }
}
