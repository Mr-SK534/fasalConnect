// backend/FarmerMarketplace.Api/Interfaces/IAuthService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<UserResponseDto> GetMeAsync(Guid userId);
    }
}