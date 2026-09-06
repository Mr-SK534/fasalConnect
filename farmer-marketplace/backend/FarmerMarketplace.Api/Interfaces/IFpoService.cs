// backend/FarmerMarketplace.Api/Interfaces/IFpoService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IFpoService
    {
        Task<List<UserResponseDto>> GetLinkedFarmersAsync(Guid fpoId, Guid requestingUserId, string? role);

        Task<UserResponseDto> LinkFarmerAsync(Guid fpoId, Guid requestingUserId, LinkFarmerDto dto);

        Task<UserResponseDto> CreateFarmerAsync(Guid fpoId, Guid requestingUserId, CreateFpoFarmerDto dto);

        Task UnlinkFarmerAsync(Guid fpoId, Guid farmerId, Guid requestingUserId);

        Task<FpoEarningsDto> GetEarningsAsync(Guid fpoId, Guid requestingUserId, string? role);
    }
}