// backend/FarmerMarketplace.Api/Interfaces/IRouteService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IRouteService
    {
        Task<List<RouteResponseDto>> OptimizeAsync(Guid adminId, RouteOptimizeDto dto);
        Task<RouteResponseDto> GetByIdAsync(Guid routeId);
        Task<BatchStatusDto> GetBatchStatusAsync();
        Task<BatchRunResultDto> RunBatchNowAsync();
    }
}