// backend/FarmerMarketplace.Api/Controllers/RoutesController.cs

using System.Text.Json;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Records;
using FarmerMarketplace.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/routes")]
    [Authorize(Roles = "PlatformAdmin,SuperAdmin,Admin,Manager")]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService _routeService;
        private readonly RouteBatchingService _routeBatchingService;

        public RoutesController(IRouteService routeService, RouteBatchingService routeBatchingService)
        {
            _routeService = routeService;
            _routeBatchingService = routeBatchingService;
        }

        // POST /api/routes/optimize
        [HttpPost("optimize")]
        public async Task<ActionResult<RouteResponseDto>> Optimize([FromBody] OptimizeRouteDto dto)
        {
            if (dto.OrderIds == null || !dto.OrderIds.Any())
            {
                return BadRequest("OrderIds list cannot be empty.");
            }

            var route = await _routeService.OptimizeAsync(dto.OrderIds, dto.DepotLat, dto.DepotLng);

            var stopsList = JsonSerializer.Deserialize<List<RouteStop>>(route.StopsJson) ?? new List<RouteStop>();
            var stopsByVehicle = stopsList
                .GroupBy(s => s.VehicleNumber)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Sequence).ToList());

            bool hasUrgentStops = stopsList.Any(s => s.IsUrgent);
            int urgentCount = stopsList.Count(s => s.IsUrgent);
            string? warningMessage = hasUrgentStops
                ? $"Warning: {urgentCount} stops may receive spoiled produce at current ETA. Consider adding more vehicles or reducing batch size."
                : null;

            var response = new RouteResponseDto
            {
                Id = route.Id,
                BatchDate = route.BatchDate,
                VehicleCount = route.VehicleCount,
                TotalDistanceKm = route.TotalDistanceKm,
                Status = route.Status,
                StopsByVehicle = stopsByVehicle,
                HasUrgentStops = hasUrgentStops,
                WarningMessage = warningMessage
            };

            return StatusCode(201, response);
        }

        // POST /api/routes/batch-window
        [HttpPost("batch-window")]
        public async Task<IActionResult> RunBatchWindow([FromQuery] string windowName = "manual")
        {
            await _routeBatchingService.ProcessBatchWindowAsync(windowName);
            return Ok(new { message = $"Batch window '{windowName}' executed successfully." });
        }

        // GET /api/routes
        [HttpGet]
        public async Task<ActionResult<List<RouteSummaryDto>>> GetAllRoutes()
        {
            var routes = await _routeService.GetAllRoutesAsync();
            var result = routes.Select(r => new RouteSummaryDto
            {
                Id = r.Id,
                BatchDate = r.BatchDate,
                BatchWindow = r.BatchWindow,
                VehicleCount = r.VehicleCount,
                TotalDistanceKm = r.TotalDistanceKm,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            }).ToList();

            return Ok(result);
        }

        // GET /api/routes/pending
        [HttpGet("pending")]
        public async Task<ActionResult<List<PendingOrderDto>>> GetPendingOrders()
        {
            var pendingOrders = await _routeService.GetPendingOrdersAsync();
            return Ok(pendingOrders);
        }

        // GET /api/routes/{routeId}
        [HttpGet("{routeId}")]
        public async Task<ActionResult<RouteResponseDto>> GetRouteById(Guid routeId)
        {
            var route = await _routeService.GetRouteByIdAsync(routeId);

            var stopsList = JsonSerializer.Deserialize<List<RouteStop>>(route.StopsJson) ?? new List<RouteStop>();
            var stopsByVehicle = stopsList
                .GroupBy(s => s.VehicleNumber)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Sequence).ToList());

            bool hasUrgentStops = stopsList.Any(s => s.IsUrgent);
            int urgentCount = stopsList.Count(s => s.IsUrgent);
            string? warningMessage = hasUrgentStops
                ? $"Warning: {urgentCount} stops may receive spoiled produce at current ETA. Consider adding more vehicles or reducing batch size."
                : null;

            var response = new RouteResponseDto
            {
                Id = route.Id,
                BatchDate = route.BatchDate,
                VehicleCount = route.VehicleCount,
                TotalDistanceKm = route.TotalDistanceKm,
                Status = route.Status,
                StopsByVehicle = stopsByVehicle,
                HasUrgentStops = hasUrgentStops,
                WarningMessage = warningMessage
            };

            return Ok(response);
        }
    }
}