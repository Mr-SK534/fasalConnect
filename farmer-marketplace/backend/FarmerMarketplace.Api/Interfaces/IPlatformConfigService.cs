// backend/FarmerMarketplace.Api/Interfaces/IPlatformConfigService.cs

using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IPlatformConfigService
    {
        Task ReloadAsync();
        Task<decimal> GetDecimalAsync(string key, decimal defaultValue);
        Task<double> GetDoubleAsync(string key, double defaultValue);
        Task<int> GetIntAsync(string key, int defaultValue);
        Task<bool> GetBoolAsync(string key, bool defaultValue);
        Task<string> GetStringAsync(string key, string defaultValue);
        Task<decimal> CalculateBuyerPriceAsync(decimal farmerAskingPrice, string? cropName = null, string? season = null);

        Task<PlatformConfigResponseDto> GetAllConfigAsync(string userRole);
        Task<ConfigUpdateResultDto> UpdateConfigAsync(ConfigUpdateRequestDto dto, string userEmail, string userRole);
        Task<ConfigUpdateResultDto> AddCropConfigAsync(AddCropConfigRequestDto dto, string userEmail, string userRole);
        Task EnsureCropConfigExistsAsync(string cropName);
        Task<List<TransactionLedger>> GetAuditHistoryAsync(int limit);
        Task<SimulatePriceChangeResultDto> SimulatePriceChangeAsync(SimulatePriceChangeRequestDto dto);
    }
}
