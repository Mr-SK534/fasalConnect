// backend/FarmerMarketplace.Api/Interfaces/IRouteService.cs

using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IRouteService
    {
        Task<DeliveryRoute> OptimizeAsync(List<Guid> orderIds, double depotLat, double depotLng, string batchWindow = "manual");
        Task<DeliveryRoute> GetRouteByIdAsync(Guid routeId);
        Task<List<DeliveryRoute>> GetAllRoutesAsync();
        Task<List<PendingOrderDto>> GetPendingOrdersAsync();
    }
}