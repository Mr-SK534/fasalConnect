// backend/FarmerMarketplace.Api/Interfaces/IUserService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IUserService
    {
        Task<PublicProfileDto> GetPublicProfileAsync(Guid id);

        Task<UserResponseDto> UpdateBasicProfileAsync(Guid id, Guid requestingUserId, UpdateBasicProfileDto dto);
        Task<UserResponseDto> UpdateProfileAsync(Guid id, Guid requestingUserId, ProfileSetupDto dto);
    }
}