// backend/FarmerMarketplace.Api/Controllers/AuthController.cs

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return StatusCode(201, result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserResponseDto>> GetMe()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _authService.GetMeAsync(userId.Value);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

            if (jti == null || expClaim == null || !long.TryParse(expClaim, out var expUnix))
                return Unauthorized();

            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            await _authService.LogoutAsync(jti, expiresAt);

            return Ok(new { message = "Logged out successfully." });
        }

        [Authorize]
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _authService.RefreshAsync(userId.Value);
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