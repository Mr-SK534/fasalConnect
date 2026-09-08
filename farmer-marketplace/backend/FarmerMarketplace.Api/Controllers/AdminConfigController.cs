// backend/FarmerMarketplace.Api/Controllers/AdminConfigController.cs

using System.Security.Claims;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/admin/config")]
    [Authorize]
    public class AdminConfigController : ControllerBase
    {
        private readonly IPlatformConfigService _configService;
        private readonly ILogger<AdminConfigController> _logger;

        public AdminConfigController(IPlatformConfigService configService, ILogger<AdminConfigController> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        // GET /api/admin/config
        [HttpGet]
        public async Task<ActionResult<PlatformConfigResponseDto>> GetAllConfig()
        {
            var userRole = GetUserRole();
            var result = await _configService.GetAllConfigAsync(userRole);
            return Ok(result);
        }

        // POST /api/admin/config/update
        [HttpPost("update")]
        public async Task<ActionResult<ConfigUpdateResultDto>> UpdateConfig([FromBody] ConfigUpdateRequestDto dto)
        {
            var userEmail = GetUserEmail();
            var userRole = GetUserRole();

            try
            {
                var result = await _configService.UpdateConfigAsync(dto, userEmail, userRole);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { statusCode = 404, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { statusCode = 403, message = ex.Message });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Config update failed unexpectedly: {Message}", ex.Message);
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        // POST /api/admin/config/add-crop
        [HttpPost("add-crop")]
        public async Task<ActionResult<ConfigUpdateResultDto>> AddCropConfig([FromBody] AddCropConfigRequestDto dto)
        {
            var userEmail = GetUserEmail();
            var userRole = GetUserRole();

            try
            {
                var result = await _configService.AddCropConfigAsync(dto, userEmail, userRole);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { statusCode = 403, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add crop config: {Message}", ex.Message);
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        // GET /api/admin/config/audit-history
        [HttpGet("audit-history")]
        public async Task<ActionResult<List<TransactionLedger>>> GetAuditHistory([FromQuery] int limit = 50)
        {
            var history = await _configService.GetAuditHistoryAsync(limit);
            return Ok(history);
        }

        // POST /api/admin/config/simulate
        [HttpPost("simulate")]
        public async Task<ActionResult<SimulatePriceChangeResultDto>> SimulatePriceChange([FromBody] SimulatePriceChangeRequestDto dto)
        {
            var result = await _configService.SimulatePriceChangeAsync(dto);
            return Ok(result);
        }

        private string GetUserRole()
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? "Manager";
            return roleClaim;
        }

        private string GetUserEmail()
        {
            var emailClaim = User.FindFirstValue(ClaimTypes.Email)
                          ?? User.FindFirstValue("email")
                          ?? User.FindFirstValue(ClaimTypes.Name)
                          ?? "admin@farmermarketplace.com";
            return emailClaim;
        }
    }
}
