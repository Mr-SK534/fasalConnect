// backend/FarmerMarketplace.Api/Services/RouteBatchingService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using FarmerMarketplace.Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class RouteBatchingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RouteBatchingService> _logger;
        private DateTime? _lastBatchRunTime;

        public RouteBatchingService(IServiceProvider serviceProvider, ILogger<RouteBatchingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RouteBatchingService started. Scheduled batch windows: 8:00 AM & 2:00 PM (14:00) daily.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nowUtc = DateTime.UtcNow;
                    var nowLocal = DateTime.Now;

                    // Trigger at 8:00 AM (morning) or 2:00 PM (14:00 afternoon)
                    bool isMorningWindow = (nowUtc.Hour == 8 && nowUtc.Minute == 0) || (nowLocal.Hour == 8 && nowLocal.Minute == 0);
                    bool isAfternoonWindow = (nowUtc.Hour == 14 && nowUtc.Minute == 0) || (nowLocal.Hour == 14 && nowLocal.Minute == 0);

                    if (isMorningWindow || isAfternoonWindow)
                    {
                        // Ensure we only trigger once per minute slot
                        if (!_lastBatchRunTime.HasValue || (nowUtc - _lastBatchRunTime.Value).TotalMinutes > 2)
                        {
                            _lastBatchRunTime = nowUtc;
                            string windowName = isMorningWindow ? "morning" : "afternoon";
                            _logger.LogInformation("Automated trigger activated for batch window: '{WindowName}' at {Time}", windowName, nowLocal);
                            await ProcessBatchWindowAsync(windowName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RouteBatchingService background cycle.");
                }

                // Check every 30 seconds
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        public async Task ProcessBatchWindowAsync(string batchWindow = "manual")
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var routeService = scope.ServiceProvider.GetRequiredService<IRouteService>();

            // 1. Fetch unrouted confirmed orders with items, farmer, and buyer details
            var unroutedOrders = await context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Items).ThenInclude(i => i.Farmer)
                .Where(o => o.Status == OrderStatus.Confirmed && o.RouteId == null)
                .ToListAsync();

            if (!unroutedOrders.Any())
            {
                _logger.LogInformation("RouteBatching: No unrouted confirmed orders found for batch window '{BatchWindow}'.", batchWindow);
                return;
            }

            // 2. Validate coordinates & compute midpoints
            var validOrders = new List<(Order Order, double MidLat, double MidLng)>();

            foreach (var order in unroutedOrders)
            {
                var farmer = order.Items.Select(i => i.Farmer).FirstOrDefault(f => f != null);
                double? pickupLat = farmer?.Latitude;
                double? pickupLng = farmer?.Longitude;
                double? deliveryLat = order.Latitude;
                double? deliveryLng = order.Longitude;

                if (!pickupLat.HasValue || !pickupLng.HasValue || !deliveryLat.HasValue || !deliveryLng.HasValue)
                {
                    _logger.LogWarning("Skipped order {OrderId}: missing coordinates", order.Id);
                    continue;
                }

                double midLat = (pickupLat.Value + deliveryLat.Value) / 2.0;
                double midLng = (pickupLng.Value + deliveryLng.Value) / 2.0;

                validOrders.Add((order, midLat, midLng));
            }

            if (!validOrders.Any())
            {
                _logger.LogWarning("RouteBatching: No orders with valid coordinates found.");
                return;
            }

            int totalOrderCount = validOrders.Count;
            int K = Math.Max(1, totalOrderCount / 8);

            // 3. Pure C# K-Means Clustering Algorithm (Max 20 iterations)
            var centroids = new List<(double Lat, double Lng)>();
            for (int k = 0; k < K; k++)
            {
                int idx = (k * totalOrderCount) / K;
                centroids.Add((validOrders[idx].MidLat, validOrders[idx].MidLng));
            }

            var assignments = new int[totalOrderCount];
            bool changed = true;
            int iteration = 0;
            const int maxIterations = 20;

            while (changed && iteration < maxIterations)
            {
                changed = false;
                iteration++;

                // Assignment Step
                for (int i = 0; i < totalOrderCount; i++)
                {
                    double bestDist = double.MaxValue;
                    int bestCluster = 0;

                    for (int k = 0; k < K; k++)
                    {
                        double dist = GeoUtils.DistanceInMeters(
                            validOrders[i].MidLat, validOrders[i].MidLng,
                            centroids[k].Lat, centroids[k].Lng);

                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestCluster = k;
                        }
                    }

                    if (assignments[i] != bestCluster)
                    {
                        assignments[i] = bestCluster;
                        changed = true;
                    }
                }

                // Update Centroids Step
                for (int k = 0; k < K; k++)
                {
                    var clusterPoints = validOrders.Where((_, idx) => assignments[idx] == k).ToList();
                    if (clusterPoints.Any())
                    {
                        double avgLat = clusterPoints.Average(p => p.MidLat);
                        double avgLng = clusterPoints.Average(p => p.MidLng);
                        centroids[k] = (avgLat, avgLng);
                    }
                }
            }

            // 4. Group into clusters & enforce minimum cluster size of 2
            var rawClusters = new Dictionary<int, List<(Order Order, double MidLat, double MidLng)>>();
            for (int k = 0; k < K; k++) rawClusters[k] = new List<(Order, double, double)>();

            for (int i = 0; i < totalOrderCount; i++)
            {
                rawClusters[assignments[i]].Add(validOrders[i]);
            }

            var validClusters = rawClusters.Values.Where(c => c.Count >= 2).ToList();
            var smallClusterOrders = rawClusters.Values.Where(c => c.Count == 1).SelectMany(c => c).ToList();

            // Merge singletons into nearest non-empty cluster
            if (smallClusterOrders.Any())
            {
                if (!validClusters.Any())
                {
                    validClusters.Add(smallClusterOrders);
                }
                else
                {
                    foreach (var singleton in smallClusterOrders)
                    {
                        double minDistance = double.MaxValue;
                        List<(Order Order, double MidLat, double MidLng)>? targetCluster = null;

                        foreach (var cluster in validClusters)
                        {
                            double centerLat = cluster.Average(p => p.MidLat);
                            double centerLng = cluster.Average(p => p.MidLng);

                            double dist = GeoUtils.DistanceInMeters(singleton.MidLat, singleton.MidLng, centerLat, centerLng);
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                targetCluster = cluster;
                            }
                        }

                        targetCluster?.Add(singleton);
                    }
                }
            }

            var clusterSizes = validClusters.Select(c => c.Count).ToList();
            _logger.LogInformation("Batch created {ClusterCount} clusters from {OrderCount} orders — cluster sizes: [{ClusterSizes}]",
                validClusters.Count, totalOrderCount, string.Join(", ", clusterSizes));

            // 5. Run OptimizeAsync for each cluster and dispatch routes (Out for delivery -> InProgress)
            int createdRoutesCount = 0;

            foreach (var cluster in validClusters)
            {
                double hubLat = cluster.Average(p => p.MidLat);
                double hubLng = cluster.Average(p => p.MidLng);
                var orderIds = cluster.Select(p => p.Order.Id).ToList();

                try
                {
                    var deliveryRoute = await routeService.OptimizeAsync(orderIds, hubLat, hubLng, batchWindow);

                    // Mark route in progress (out for delivery) and orders InTransit
                    deliveryRoute.Status = DeliveryRouteStatus.InProgress;

                    var clusterOrdersInDb = await context.Orders.Where(o => orderIds.Contains(o.Id)).ToListAsync();
                    foreach (var ord in clusterOrdersInDb)
                    {
                        ord.Status = OrderStatus.InTransit;
                        ord.UpdatedAt = DateTime.UtcNow;
                    }

                    await context.SaveChangesAsync();
                    createdRoutesCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to optimize route cluster with {Count} orders.", orderIds.Count);
                }
            }

            _logger.LogInformation("Automated batch window '{BatchWindow}': created {RouteCount} route(s) and dispatched orders to InTransit.",
                batchWindow, createdRoutesCount);
        }
    }
}
