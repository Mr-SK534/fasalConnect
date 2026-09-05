// backend/FarmerMarketplace.Api/Interfaces/IProductService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductResponseDto>> GetAllAsync(ProductQueryDto query);

        Task<ProductResponseDto> GetByIdAsync(Guid id);

        Task<List<ProductResponseDto>> GetByFarmerIdAsync(Guid farmerId);

        Task<ProductResponseDto> CreateAsync(Guid farmerId, ProductDto dto);

        // requestingUserId + role passed so service can enforce "owner only" edit rule
        Task<ProductResponseDto> UpdateAsync(Guid id, Guid requestingUserId, string? role, ProductDto dto);

        Task DeleteAsync(Guid id, Guid requestingUserId, string? role);
    }
}