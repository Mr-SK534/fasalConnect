// backend/FarmerMarketplace.Api/Interfaces/IFpoService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IFpoService
    {
        Task<List<UserResponseDto>> GetLinkedFarmersAsync(Guid fpoId, Guid requestingUserId, string? role);

        Task<UserResponseDto> LinkFarmerAsync(Guid fpoId, Guid requestingUserId, LinkFarmerDto dto);

        Task UnlinkFarmerAsync(Guid fpoId, Guid farmerId, Guid requestingUserId);
    }
}