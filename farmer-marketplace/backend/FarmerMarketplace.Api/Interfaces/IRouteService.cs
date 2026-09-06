// backend/FarmerMarketplace.Api/Interfaces/IRouteService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IRouteService
    {
        Task<RouteResponseDto> OptimizeAsync(Guid adminId, RouteOptimizeDto dto);
        Task<RouteResponseDto> GetByIdAsync(Guid routeId);
    }
}