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
    [Authorize(Roles = "PlatformAdmin,FpoAdmin,SuperAdmin,Admin,Manager")]
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
        public async Task<ActionResult<AdminUserListResponseDto>> GetUsers([FromQuery] string? role = null, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var claimRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirstValue("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // FpoAdmin only sees users linked under their own FPO,
            // PlatformAdmin/SuperAdmin/Admin sees everyone
            var result = await _adminService.GetUsersAsync(userId, claimRole, role, search, page, pageSize);
            return Ok(result);
        }

        [HttpGet("users/{id}")]
        [Authorize(Roles = "PlatformAdmin,SuperAdmin,Admin,Manager")]
        public async Task<ActionResult<UserResponseDto>> GetUser(Guid id) => Ok(await _adminService.GetUserByIdAsync(id));

        [HttpPost("users")]
        [Authorize(Roles = "PlatformAdmin,SuperAdmin,Admin")]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateAdminUserDto dto)
        {
            var result = await _adminService.CreateUserAsync(dto);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("users/{id}/suspend")]
        [Authorize(Roles = "PlatformAdmin,SuperAdmin,Admin")]
        public async Task<ActionResult<UserResponseDto>> SuspendUser(Guid id, [FromBody] SuspendUserDto dto) => Ok(await _adminService.SuspendUserAsync(id, dto));

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

        [HttpGet("orders")]
        [Authorize(Roles = "PlatformAdmin,SuperAdmin,Admin,Manager")]
        public async Task<ActionResult<AdminOrderListResponseDto>> GetOrders([FromQuery] string? status = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => Ok(await _adminService.GetOrdersAsync(status, dateFrom, dateTo, page, pageSize));

        [HttpPut("orders/{id}/override-status")]
        [Authorize(Roles = "PlatformAdmin,SuperAdmin,Admin")]
        public async Task<ActionResult<OrderResponseDto>> OverrideOrderStatus(Guid id, [FromBody] OverrideOrderStatusDto dto) => Ok(await _adminService.OverrideOrderStatusAsync(id, dto));
    }
}