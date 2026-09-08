// backend/FarmerMarketplace.Api/Services/RouteService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
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