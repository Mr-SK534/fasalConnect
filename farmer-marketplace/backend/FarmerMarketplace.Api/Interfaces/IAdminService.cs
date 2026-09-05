// backend/FarmerMarketplace.Api/Interfaces/IAdminService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IAdminService
    {
        // role is passed as a raw claim string ("PlatformAdmin" / "FpoAdmin") since that's
        // what ClaimTypes.Role gives the controller — service parses/validates internally.
        Task<List<UserResponseDto>> GetUsersAsync(Guid requestingUserId, string? role);

        Task<AdminSummaryDto> GetSummaryAsync(Guid requestingUserId, string? role);
    }
}