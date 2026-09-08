// backend/FarmerMarketplace.Api/Controllers/ForecastController.cs

using FarmerMarketplace.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForecastController : ControllerBase
    {
        private readonly IForecastService _forecastService;

        public ForecastController(IForecastService forecastService)
        {
            _forecastService = forecastService;
        }

        /// <summary>
        /// Gets ML.NET time-series demand forecast for a specific crop.
        /// Accessible by all users (Farmers, FPOs, Buyers, Admins).
        /// </summary>
        [HttpGet("crop/{cropName}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCropForecast(
            string cropName,
            [FromQuery] string? region = null,
            [FromQuery] int horizonDays = 30)
        {
            if (string.IsNullOrWhiteSpace(cropName))
            {
                return BadRequest(new { statusCode = 400, message = "Crop name is required." });
            }

            var result = await _forecastService.ForecastCropDemandAsync(cropName, region, horizonDays);
            return Ok(result);
        }

        /// <summary>
        /// Gets demand forecast for a specific farmer's listed/primary crops.
        /// Accessible by SuperAdmin, Admin, Manager, FPO Admin, or the farmer themselves.
        /// </summary>
        [HttpGet("farmer/{farmerId}")]
        [Authorize]
        public async Task<IActionResult> GetFarmerForecast(
            Guid farmerId,
            [FromQuery] int horizonDays = 30)
        {
            var result = await _forecastService.ForecastFarmerDemandAsync(farmerId, horizonDays);
            return Ok(result);
        }

        /// <summary>
        /// Returns list of farmers for administrator selection dropdown.
        /// Accessible by PlatformAdmin, SuperAdmin, Admin, Manager, FpoAdmin.
        /// </summary>
        [HttpGet("farmers")]
        [Authorize]
        public async Task<IActionResult> GetFarmersList([FromQuery] string? search = null)
        {
            var farmers = await _forecastService.GetFarmersForForecastAsync(search);
            return Ok(farmers);
        }

        /// <summary>
        /// Gets aggregated demand summaries across top marketplace crops.
        /// </summary>
        [HttpGet("summary")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopCropsSummary([FromQuery] int horizonDays = 14)
        {
            var summary = await _forecastService.GetTopDemandedCropsForecastAsync(horizonDays);
            return Ok(summary);
        }

        /// <summary>
        /// Returns all available dynamically discovered crops from active products and sales history.
        /// </summary>
        [HttpGet("available-crops")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableCrops()
        {
            var crops = await _forecastService.GetAvailableCropsAsync();
            return Ok(crops);
        }
    }
}
