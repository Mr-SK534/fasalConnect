// backend/FarmerMarketplace.Api/Controllers/FpoController.cs

using System.Security.Claims;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/fpo")]
    [Authorize(Roles = "FpoAdmin,PlatformAdmin")]
    public class FpoController : ControllerBase
    {
        private readonly IFpoService _fpoService;

        public FpoController(IFpoService fpoService)
        {
            _fpoService = fpoService;
        }

        // GET /api/fpo/{fpoId}/farmers
        [HttpGet("{fpoId}/farmers")]
        public async Task<ActionResult<List<UserResponseDto>>> GetLinkedFarmers(Guid fpoId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var result = await _fpoService.GetLinkedFarmersAsync(fpoId, userId.Value, role);
            return Ok(result);
        }

        // POST /api/fpo/{fpoId}/farmers
        [HttpPost("{fpoId}/farmers")]
        [Authorize(Roles = "FpoAdmin")]
        public async Task<ActionResult<UserResponseDto>> LinkFarmer(Guid fpoId, [FromBody] LinkFarmerDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _fpoService.LinkFarmerAsync(fpoId, userId.Value, dto);
            return Ok(result);
        }

        // DELETE /api/fpo/{fpoId}/farmers/{farmerId}
        [HttpDelete("{fpoId}/farmers/{farmerId}")]
        [Authorize(Roles = "FpoAdmin")]
        public async Task<IActionResult> UnlinkFarmer(Guid fpoId, Guid farmerId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _fpoService.UnlinkFarmerAsync(fpoId, farmerId, userId.Value);
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