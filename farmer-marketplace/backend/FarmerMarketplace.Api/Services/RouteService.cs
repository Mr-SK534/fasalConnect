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

        public RouteService(AppDbContext context, IHttpClientFactory httpClientFactory, ILogger<RouteService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<RouteResponseDto> OptimizeAsync(Guid adminId, RouteOptimizeDto dto)
        {
            if (dto.OrderIds == null || dto.OrderIds.Count < 2)
                throw new ArgumentException("At least 2 orders are needed to optimize a route.");

            var orders = await _context.Orders
                .Include(order => order.Items)
                    .ThenInclude(item => item.Farmer)
                .Include(order => order.Items)
                    .ThenInclude(item => item.Product)
                .Where(order => dto.OrderIds.Contains(order.Id))
                .ToListAsync();

            if (orders.Count != dto.OrderIds.Distinct().Count())
                throw new KeyNotFoundException("One or more selected orders were not found.");

            var invalidOrder = orders.FirstOrDefault(order =>
                order.Status != OrderStatus.Confirmed ||
                order.DeliveryType != DeliveryType.Delivery);

            if (invalidOrder != null)
                throw new InvalidOperationException("All selected orders must be confirmed delivery orders.");

            foreach (var order in orders.Where(order => !order.Latitude.HasValue || !order.Longitude.HasValue))
            {
                var coordinates = await GeocodeAddressAsync(order.DeliveryAddress);
                if (!coordinates.HasValue)
                    throw new InvalidOperationException($"Order '{order.Id}' needs a delivery address that can be mapped before it can be routed.");

                order.Latitude = coordinates.Value.Lat;
                order.Longitude = coordinates.Value.Lng;
            }

            await _context.SaveChangesAsync();

            var nodes = new List<RouteNodeDto>();
            foreach (var order in orders)
            {
                var farmer = order.Items.Select(item => item.Farmer).FirstOrDefault(item => item != null);
                var pickupAddress = BuildProfileAddress(farmer);
                var pickupLat = farmer?.Latitude;
                var pickupLng = farmer?.Longitude;
                if (!pickupLat.HasValue || !pickupLng.HasValue)
                {
                    var pickupCoordinates = await GeocodeAddressAsync(pickupAddress);
                    if (!pickupCoordinates.HasValue)
                        throw new InvalidOperationException($"Farmer pickup location for order '{order.Id}' could not be mapped from the profile address.");
                    pickupLat = pickupCoordinates.Value.Lat;
                    pickupLng = pickupCoordinates.Value.Lng;
                }

                nodes.Add(new RouteNodeDto { OrderId = order.Id, IsPickup = true, Latitude = pickupLat.Value, Longitude = pickupLng.Value, Address = pickupAddress, FarmerName = farmer?.Name });
                nodes.Add(new RouteNodeDto { OrderId = order.Id, IsPickup = false, Latitude = order.Latitude ?? 0d, Longitude = order.Longitude ?? 0d, Address = order.DeliveryAddress, FarmerName = farmer?.Name });
            }

            var locations = new List<(double Lat, double Lng)> { (dto.DeliveryHubLocation.Lat, dto.DeliveryHubLocation.Lng) };
            locations.AddRange(nodes.Select(node => (node.Latitude, node.Longitude)));
            var roadMatrix = await GetRoadMatrixAsync(locations);
            var orderSequence = OptimizeOrderSequence(roadMatrix.DurationsSeconds, nodes);

            var route = new FarmerMarketplace.Api.Models.Route
            {
                CreatedBy = adminId,
                DeliveryHubLat = dto.DeliveryHubLocation.Lat,
                DeliveryHubLng = dto.DeliveryHubLocation.Lng
            };

            var sequence = 1;
            var arrival = DateTime.UtcNow;
            var previousNode = 0;
            var pickupDay = DateTime.UtcNow.Date;

            foreach (var node in orderSequence)
            {
                var routeNode = nodes[node - 1];
                var next = orders.First(order => order.Id == routeNode.OrderId);
                var distance = roadMatrix.DistancesKm[previousNode, node];
                var durationSeconds = roadMatrix.DurationsSeconds[previousNode, node];
                arrival = arrival.AddSeconds(Math.Max(300, durationSeconds));
                var deliveryDate = pickupDay.AddDays(IsPerishable(next) ? 1 : 2);
                route.Stops.Add(new RouteStop
                {
                    OrderId = next.Id,
                    StopSequence = sequence++,
                    Latitude = routeNode.Latitude,
                    Longitude = routeNode.Longitude,
                    DistanceFromPreviousKm = Convert.ToDecimal(distance),
                    EstimatedArrival = arrival,
                    StopType = routeNode.IsPickup ? "Pickup" : "Delivery",
                    Address = routeNode.Address,
                    FarmerName = routeNode.FarmerName,
                    PickupDate = pickupDay,
                    DeliveryDate = deliveryDate
                });

                previousNode = node;
            }

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return Map(route);
        }

        public async Task<RouteResponseDto> GetByIdAsync(Guid routeId)
        {
            var route = await _context.Routes
                .AsNoTracking()
                .Include(item => item.Stops)
                .FirstOrDefaultAsync(item => item.Id == routeId);

            if (route == null)
                throw new KeyNotFoundException("Route not found.");

            return Map(route);
        }

        private static RouteResponseDto Map(FarmerMarketplace.Api.Models.Route route) => new()
        {
            RouteId = route.Id,
            DeliveryHubLat = route.DeliveryHubLat,
            DeliveryHubLng = route.DeliveryHubLng,
            CreatedAt = route.CreatedAt,
            Stops = route.Stops
                .OrderBy(stop => stop.StopSequence)
                .Select(stop => new RouteStopResponseDto
                {
                    OrderId = stop.OrderId,
                    StopSequence = stop.StopSequence,
                    Latitude = stop.Latitude,
                    Longitude = stop.Longitude,
                    DistanceFromPreviousKm = stop.DistanceFromPreviousKm,
                    EstimatedArrival = stop.EstimatedArrival,
                    StopType = stop.StopType,
                    Address = stop.Address,
                    FarmerName = stop.FarmerName,
                    PickupDate = stop.PickupDate,
                    DeliveryDate = stop.DeliveryDate
                })
                .ToList()
        };

        private static double DistanceKm(double lat1, double lng1, double lat2, double lng2)
        {
            const double earthRadiusKm = 6371;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLng = DegreesToRadians(lng2 - lng1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2))
                * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;

        private static int[] OptimizeOrderSequence(long[,] durations, List<RouteNodeDto> nodes)
        {
            var manager = new RoutingIndexManager(durations.GetLength(0), 1, 0);
            var routing = new RoutingModel(manager);
            var transitCallback = routing.RegisterTransitCallback((fromIndex, toIndex) =>
            {
                var fromNode = manager.IndexToNode(fromIndex);
                var toNode = manager.IndexToNode(toIndex);
                return durations[fromNode, toNode];
            });
            routing.SetArcCostEvaluatorOfAllVehicles(transitCallback);
            for (var pair = 0; pair < nodes.Count; pair += 2)
            {
                var pickupIndex = manager.NodeToIndex(pair + 1);
                var deliveryIndex = manager.NodeToIndex(pair + 2);
                routing.AddPickupAndDelivery(pickupIndex, deliveryIndex);
                routing.solver().Add(routing.VehicleVar(pickupIndex) == routing.VehicleVar(deliveryIndex));
                routing.solver().Add(routing.NextVar(pickupIndex) != pickupIndex);
            }
            var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
            searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
            searchParameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
            searchParameters.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = 3 };
            var solution = routing.SolveWithParameters(searchParameters);
            if (solution == null) throw new InvalidOperationException("Could not find a feasible delivery route.");

            var sequence = new List<int>();
            var index = routing.Start(0);
            while (!routing.IsEnd(index))
            {
                var node = manager.IndexToNode(index);
                if (node != 0) sequence.Add(node);
                index = solution.Value(routing.NextVar(index));
            }
            return sequence.ToArray();
        }

        private static bool IsPerishable(Order order)
        {
            return order.Items.Any(item => item.Product?.Category is ProductCategory.Vegetables or ProductCategory.Fruits or ProductCategory.Dairy);
        }

        private static string? BuildProfileAddress(User? farmer)
        {
            if (farmer == null) return null;
            return string.Join(", ", new[]
            {
                farmer.Village,
                farmer.District,
                farmer.State,
                farmer.Pincode,
                farmer.Location,
                farmer.Region
            }.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct());
        }

        private async Task<(double[,] DistancesKm, long[,] DurationsSeconds)> GetRoadMatrixAsync(List<(double Lat, double Lng)> locations)
        {
            var fallbackDistances = new double[locations.Count, locations.Count];
            var fallbackDurations = new long[locations.Count, locations.Count];
            for (var from = 0; from < locations.Count; from++)
            for (var to = 0; to < locations.Count; to++)
            {
                fallbackDistances[from, to] = DistanceKm(locations[from].Lat, locations[from].Lng, locations[to].Lat, locations[to].Lng);
                fallbackDurations[from, to] = (long)Math.Max(300, fallbackDistances[from, to] / 30d * 3600d);
            }

            try
            {
                var coordinates = string.Join(";", locations.Select(location => $"{location.Lng.ToString(CultureInfo.InvariantCulture)},{location.Lat.ToString(CultureInfo.InvariantCulture)}"));
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("FasalConnect/1.0 route-planner");
                using var response = await client.GetAsync($"https://router.project-osrm.org/table/v1/driving/{coordinates}?annotations=distance,duration");
                response.EnsureSuccessStatusCode();
                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var distances = document.RootElement.GetProperty("distances");
                var durations = document.RootElement.GetProperty("durations");
                var roadDistances = new double[locations.Count, locations.Count];
                var roadDurations = new long[locations.Count, locations.Count];
                for (var from = 0; from < locations.Count; from++)
                for (var to = 0; to < locations.Count; to++)
                {
                    roadDistances[from, to] = distances[from][to].GetDouble() / 1000d;
                    roadDurations[from, to] = (long)Math.Max(300, durations[from][to].GetDouble());
                }
                return (roadDistances, roadDurations);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Road matrix lookup failed; falling back to straight-line estimates.");
                return (fallbackDistances, fallbackDurations);
            }
        }

        private async Task<(double Lat, double Lng)?> GeocodeAddressAsync(string? address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FasalConnect/1.0 (route-planner; contact: admin@fasalconnect.local)");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-IN,en;q=0.8");

            var normalizedAddress = address.Trim();
            var postalCode = System.Text.RegularExpressions.Regex.Match(normalizedAddress, @"\b\d{6}\b").Value;
            var localityAddress = System.Text.RegularExpressions.Regex.Replace(
                normalizedAddress,
                @"^(flat|floor|house|plot|door|unit)\s+[^,]+,?\s*",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

            var candidates = new[]
            {
                normalizedAddress,
                $"{normalizedAddress}, India",
                localityAddress,
                $"{localityAddress}, India",
                string.IsNullOrWhiteSpace(postalCode) ? null : $"{postalCode}, India"
            }
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Select(candidate => candidate!)
            .Distinct(StringComparer.OrdinalIgnoreCase)!;

            foreach (var candidate in candidates)
            {
                try
                {
                    var url = $"https://nominatim.openstreetmap.org/search?format=jsonv2&addressdetails=1&limit=1&countrycodes=in&q={Uri.EscapeDataString(candidate)}";
                    using var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Geocoding request returned HTTP {StatusCode} for address candidate {Address}", response.StatusCode, candidate);
                        continue;
                    }

                    using var document = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    var result = document.RootElement.EnumerateArray().FirstOrDefault();
                    if (result.ValueKind == System.Text.Json.JsonValueKind.Undefined) continue;

                    var latText = result.TryGetProperty("lat", out var latProperty) ? latProperty.GetString() : null;
                    var lngText = result.TryGetProperty("lon", out var lngProperty) ? lngProperty.GetString() : null;
                    if (double.TryParse(latText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat)
                        && double.TryParse(lngText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lng))
                        return (lat, lng);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Geocoding failed for address candidate {Address}", candidate);
                }
            }

            _logger.LogWarning("No map result found for delivery address {Address}", address);
            return null;
        }
    }
}
