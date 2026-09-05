// backend/FarmerMarketplace.Api/Controllers/UsersController.cs (add these two endpoints)

using System.Security.Claims;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET /api/users/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PublicProfileDto>> GetById(Guid id)
        {
            var result = await _userService.GetPublicProfileAsync(id);
            return Ok(result);
        }

        // PUT /api/users/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<UserResponseDto>> Update(Guid id, [FromBody] UpdateBasicProfileDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirstValue("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var requestingUserId))
                return Unauthorized();

            var result = await _userService.UpdateBasicProfileAsync(id, requestingUserId, dto);
            return Ok(result);
        }

        // PUT /api/users/{id}/profile — already exists from before, kept unchanged
        [HttpPut("{id}/profile")]
        [Authorize]
        public async Task<ActionResult<UserResponseDto>> UpdateProfile(Guid id, [FromBody] ProfileSetupDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirstValue("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var requestingUserId))
                return Unauthorized();

            var result = await _userService.UpdateProfileAsync(id, requestingUserId, dto);
            return Ok(result);
        }
    }
}