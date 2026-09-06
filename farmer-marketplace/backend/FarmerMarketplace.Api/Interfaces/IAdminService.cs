// backend/FarmerMarketplace.Api/Interfaces/IAdminService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IAdminService
    {
        // role is passed as a raw claim string ("PlatformAdmin" / "FpoAdmin") since that's
        // what ClaimTypes.Role gives the controller — service parses/validates internally.
        Task<AdminUserListResponseDto> GetUsersAsync(Guid requestingUserId, string? role, string? userRole, string? search, int page, int pageSize);

        Task<UserResponseDto> GetUserByIdAsync(Guid id);

        Task<UserResponseDto> CreateUserAsync(CreateAdminUserDto dto);

        Task<UserResponseDto> SuspendUserAsync(Guid id, SuspendUserDto dto);

        Task<AdminOrderListResponseDto> GetOrdersAsync(string? status, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize);

        Task<OrderResponseDto> OverrideOrderStatusAsync(Guid id, OverrideOrderStatusDto dto);

        Task<AdminSummaryDto> GetSummaryAsync(Guid requestingUserId, string? role);
    }
}