// backend/FarmerMarketplace.Api/Controllers/RoutesController.cs

using System.Security.Claims;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
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
        public async Task<ActionResult<RouteResponseDto>> Optimize([FromBody] RouteOptimizeDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirstValue("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var adminId))
                return Unauthorized();

            var result = await _routeService.OptimizeAsync(adminId, dto);
            return StatusCode(201, result);
        }

        // GET /api/routes/{routeId}
        [HttpGet("{routeId}")]
        public async Task<ActionResult<RouteResponseDto>> GetById(Guid routeId)
        {
            var result = await _routeService.GetByIdAsync(routeId);
            return Ok(result);
        }
    }
}