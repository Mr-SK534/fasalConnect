// backend/FarmerMarketplace.Api/Interfaces/IForecastService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IForecastService
    {
        Task<CropForecastResultDto> ForecastCropDemandAsync(string cropName, string? region = null, int horizonDays = 30);
        Task<FarmerForecastResultDto> ForecastFarmerDemandAsync(Guid farmerId, int horizonDays = 30);
        Task<List<FarmerListItemDto>> GetFarmersForForecastAsync(string? search = null);
        Task<List<CropDemandSummaryDto>> GetTopDemandedCropsForecastAsync(int horizonDays = 14);
        Task<List<string>> GetAvailableCropsAsync();
    }
}
