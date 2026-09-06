// backend/FarmerMarketplace.Api/Controllers/OrdersController.cs

using System.Security.Claims;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // POST /api/orders
        [HttpPost]
        [Authorize(Roles = "Buyer")]
        public async Task<ActionResult<OrderResponseDto>> Create([FromBody] OrderDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _orderService.CreateAsync(userId.Value, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // GET /api/orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetById(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var result = await _orderService.GetByIdAsync(id, userId.Value, role);
            return Ok(result);
        }

        // GET /api/orders/buyer/{buyerId}
        [HttpGet("buyer/{buyerId}")]
        public async Task<ActionResult<List<OrderResponseDto>>> GetByBuyerId(Guid buyerId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var result = await _orderService.GetByBuyerIdAsync(buyerId, userId.Value, role);
            return Ok(result);
        }

        // GET /api/orders/farmer/{farmerId}
        [HttpGet("farmer/{farmerId}")]
        public async Task<ActionResult<List<OrderResponseDto>>> GetByFarmerId(Guid farmerId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var result = await _orderService.GetByFarmerIdAsync(farmerId, userId.Value, role);
            return Ok(result);
        }

        // PUT /api/orders/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Farmer,FpoAdmin,PlatformAdmin")]
        public async Task<ActionResult<OrderResponseDto>> UpdateStatus(Guid id, [FromBody] OrderStatusUpdateDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var result = await _orderService.UpdateStatusAsync(id, userId.Value, role, dto);
            return Ok(result);
        }

        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirstValue("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return null;

            return userId;
        }
    }
}