// backend/FarmerMarketplace.Api/Services/ProductService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductResponseDto>> GetAllAsync(ProductQueryDto query)
        {
            IQueryable<Product> productsQuery = _context.Products
                .AsNoTracking()
                .Include(p => p.Farmer)
                .Where(p => p.IsActive);

            if (query.Category.HasValue)
                productsQuery = productsQuery.Where(p => p.Category == query.Category.Value);

            if (!string.IsNullOrWhiteSpace(query.Region))
                productsQuery = productsQuery.Where(p => p.Region != null &&
                    p.Region.ToLower().Contains(query.Region.ToLower()));

            if (!string.IsNullOrWhiteSpace(query.Search))
                productsQuery = productsQuery.Where(p =>
                    p.CropName.ToLower().Contains(query.Search.ToLower()));

            if (query.MinPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);

            if (query.MaxPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);

            var products = await productsQuery
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(MapToResponseDto).ToList();
        }

        public async Task<ProductResponseDto> GetByIdAsync(Guid id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Farmer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            return MapToResponseDto(product);
        }

        public async Task<List<ProductResponseDto>> GetByFarmerIdAsync(Guid farmerId)
        {
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Farmer)
                .Where(p => p.FarmerId == farmerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(MapToResponseDto).ToList();
        }

        public async Task<ProductResponseDto> CreateAsync(Guid farmerId, ProductDto dto)
        {
            var farmerExists = await _context.Users.AnyAsync(u => u.Id == farmerId);
            if (!farmerExists)
                throw new KeyNotFoundException("Farmer account not found.");

            var product = new Product
            {
                CropName = dto.CropName,
                Price = dto.Price,
                Quantity = dto.Quantity,
                Unit = dto.Unit,
                Category = dto.Category,
                HarvestDate = dto.HarvestDate,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Region = dto.Region,
                FarmerId = farmerId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // reload with Farmer included so response has FarmerName/FarmerLocation populated
            var created = await _context.Products
                .AsNoTracking()
                .Include(p => p.Farmer)
                .FirstAsync(p => p.Id == product.Id);

            return MapToResponseDto(created);
        }

        public async Task<ProductResponseDto> UpdateAsync(Guid id, Guid requestingUserId, string? role, ProductDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Farmer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            // Contract rule: PUT is "Owner only"
            if (product.FarmerId != requestingUserId)
                throw new UnauthorizedAccessException("Only the owner can edit this product.");

            product.CropName = dto.CropName;
            product.Price = dto.Price;
            product.Quantity = dto.Quantity;
            product.Unit = dto.Unit;
            product.Category = dto.Category;
            product.HarvestDate = dto.HarvestDate;
            product.Description = dto.Description;
            product.ImageUrl = dto.ImageUrl;
            product.Region = dto.Region;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponseDto(product);
        }

        public async Task DeleteAsync(Guid id, Guid requestingUserId, string? role)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            var isOwner = product.FarmerId == requestingUserId;
            var isPlatformAdmin = role == nameof(UserRole.PlatformAdmin);

            // Contract rule: "Owner/Admin" — owner or PlatformAdmin can delete
            if (!isOwner && !isPlatformAdmin)
                throw new UnauthorizedAccessException("Only the owner or an admin can delete this product.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        private static ProductResponseDto MapToResponseDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                CropName = product.CropName,
                Price = product.Price,
                Quantity = product.Quantity,
                Unit = product.Unit,
                Category = product.Category,
                HarvestDate = product.HarvestDate,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                Region = product.Region,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                FarmerId = product.FarmerId,
                FarmerName = product.Farmer?.Name ?? string.Empty,
                FarmerLocation = product.Farmer?.Location
            };
        }
    }
}