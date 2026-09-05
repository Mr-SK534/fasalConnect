// backend/FarmerMarketplace.Api/Controllers/ProductsController.cs

using System.Security.Claims;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET /api/products?category=&region=&search=&minPrice=&maxPrice=
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductResponseDto>>> GetAll([FromQuery] ProductQueryDto query)
        {
            var result = await _productService.GetAllAsync(query);
            return Ok(result);
        }

        // GET /api/products/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductResponseDto>> GetById(Guid id)
        {
            var result = await _productService.GetByIdAsync(id);
            return Ok(result);
        }

        // GET /api/products/farmer/{farmerId}
        [HttpGet("farmer/{farmerId}")]
        [Authorize]
        public async Task<ActionResult<List<ProductResponseDto>>> GetByFarmerId(Guid farmerId)
        {
            var result = await _productService.GetByFarmerIdAsync(farmerId);
            return Ok(result);
        }

        // POST /api/products
        [HttpPost]
        [Authorize(Roles = "Farmer,FpoAdmin")]
        public async Task<ActionResult<ProductResponseDto>> Create([FromBody] ProductDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _productService.CreateAsync(userId.Value, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT /api/products/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Farmer,FpoAdmin")]
        public async Task<ActionResult<ProductResponseDto>> Update(Guid id, [FromBody] ProductDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var result = await _productService.UpdateAsync(id, userId.Value, role, dto);
            return Ok(result);
        }

        // DELETE /api/products/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Farmer,FpoAdmin,PlatformAdmin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var role = User.FindFirstValue(ClaimTypes.Role);
            await _productService.DeleteAsync(id, userId.Value, role);
            return NoContent();
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