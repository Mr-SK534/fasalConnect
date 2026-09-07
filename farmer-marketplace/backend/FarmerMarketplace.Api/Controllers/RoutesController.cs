// backend/FarmerMarketplace.Api/Controllers/RoutesController.cs

using System.Text.Json;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Records;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/routes")]
    [Authorize(Roles = "PlatformAdmin")]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService _routeService;

        public RoutesController(IRouteService routeService)
        {
            _routeService = routeService;
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

            var response = new RouteResponseDto
            {
                Id = route.Id,
                BatchDate = route.BatchDate,
                VehicleCount = route.VehicleCount,
                TotalDistanceKm = route.TotalDistanceKm,
                Status = route.Status,
                StopsByVehicle = stopsByVehicle
            };

            return StatusCode(201, response);
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

            var response = new RouteResponseDto
            {
                Id = route.Id,
                BatchDate = route.BatchDate,
                VehicleCount = route.VehicleCount,
                TotalDistanceKm = route.TotalDistanceKm,
                Status = route.Status,
                StopsByVehicle = stopsByVehicle
            };

            return Ok(response);
        }
    }
}