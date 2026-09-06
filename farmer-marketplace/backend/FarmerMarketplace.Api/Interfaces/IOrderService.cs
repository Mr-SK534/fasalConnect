// backend/FarmerMarketplace.Api/Interfaces/IOrderService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateAsync(Guid buyerId, OrderDto dto);

        Task<OrderResponseDto> GetByIdAsync(Guid id, Guid requestingUserId, string? role);

        Task<List<OrderResponseDto>> GetByBuyerIdAsync(Guid buyerId, Guid requestingUserId, string? role);

        // Returns orders this farmer has items in — each result's Items list is
        // filtered to only that farmer's own items (per contract)
        Task<List<OrderResponseDto>> GetByFarmerIdAsync(Guid farmerId, Guid requestingUserId, string? role);

        Task<OrderResponseDto> UpdateStatusAsync(Guid id, Guid requestingUserId, string? role, OrderStatusUpdateDto dto);
    }
}