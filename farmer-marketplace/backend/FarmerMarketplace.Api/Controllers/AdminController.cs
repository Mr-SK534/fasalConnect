// backend/FarmerMarketplace.Api/Controllers/AdminController.cs

using System.Security.Claims;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "PlatformAdmin,FpoAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET /api/admin/users
        // Read-only list of all platform users — powers the AdminDashboard "UsersTable"
        [HttpGet("users")]
        public async Task<ActionResult<List<UserResponseDto>>> GetUsers()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirstValue("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // FpoAdmin only sees users linked under their own FPO,
            // PlatformAdmin sees everyone
            var result = await _adminService.GetUsersAsync(userId, role);
            return Ok(result);
        }

        // GET /api/admin/summary
        // Stat cards for the AdminDashboard (total farmers, buyers, orders, etc.)
        [HttpGet("summary")]
        public async Task<ActionResult<AdminSummaryDto>> GetSummary()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirstValue("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _adminService.GetSummaryAsync(userId, role);
            return Ok(result);
        }

        // GET /api/admin/orders
        // TODO: enable once Order.cs / OrderService exist (per build order: Orders comes
        // before this gets wired). Placeholder route kept here so frontend can scaffold
        // against a stable contract now.
        [HttpGet("orders")]
        public IActionResult GetOrders()
        {
            return StatusCode(501, new { message = "Not implemented yet — pending Order model/service." });
        }
    }
}