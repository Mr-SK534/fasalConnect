// backend/FarmerMarketplace.Api/Services/ProductService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Helpers;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using FarmerMarketplace.Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IPlatformConfigService _configService;

        public ProductService(AppDbContext context, IPlatformConfigService configService)
        {
            _context = context;
            _configService = configService;
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

            var result = new List<ProductResponseDto>();
            foreach (var p in products)
            {
                result.Add(await MapToResponseDtoAsync(p, isBuyerContext: true));
            }
            return result;
        }

        public async Task<ProductAggregateResponseDto> GetAggregateAsync(string cropName)
        {
            var products = await _context.Products
                .AsNoTracking()
                .Include(product => product.Farmer)
                .Where(product => product.IsActive && product.Quantity > 0 && product.CropName.ToLower() == cropName.ToLower())
                .OrderByDescending(product => product.Quantity)
                .ToListAsync();

            var first = products.FirstOrDefault();
            if (first == null)
            {
                return new ProductAggregateResponseDto { CropName = cropName, Unit = "Kg" };
            }

            var buyerPricesPerKg = new List<decimal>();
            var farmerDtos = new List<ProductAggregateFarmerDto>();

            foreach (var product in products)
            {
                var farmerPricePerKg = UnitConverter.ToPricePerKg(product.Price, product.Unit);
                var buyerPricePerKg = await _configService.CalculateBuyerPriceAsync(farmerPricePerKg, product.CropName);
                var availableQtyKg = UnitConverter.ToKgQuantity(product.Quantity, product.Unit);

                buyerPricesPerKg.Add(buyerPricePerKg);
                farmerDtos.Add(new ProductAggregateFarmerDto
                {
                    FarmerId = product.FarmerId,
                    FarmerName = product.Farmer?.Name ?? string.Empty,
                    FarmerLocation = product.Farmer?.Address ?? product.Region ?? product.Farmer?.Location,
                    AvailableQuantity = availableQtyKg,
                    Price = buyerPricePerKg
                });
            }

            var totalQtyKg = products.Sum(product => UnitConverter.ToKgQuantity(product.Quantity, product.Unit));

            return new ProductAggregateResponseDto
            {
                CropName = cropName,
                TotalAvailableQuantity = totalQtyKg,
                Unit = "Kg",
                AveragePrice = buyerPricesPerKg.Any() ? Math.Round(buyerPricesPerKg.Average(), 2) : 0m,
                MinPrice = buyerPricesPerKg.Any() ? buyerPricesPerKg.Min() : 0m,
                MaxPrice = buyerPricesPerKg.Any() ? buyerPricesPerKg.Max() : 0m,
                FarmerCount = products.Select(product => product.FarmerId).Distinct().Count(),
                Farmers = farmerDtos
            };
        }

        public async Task<ProductResponseDto> GetByIdAsync(Guid id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Farmer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            return await MapToResponseDtoAsync(product, isBuyerContext: true);
        }

        public async Task<List<ProductResponseDto>> GetByFarmerIdAsync(Guid farmerId, Guid? requestingUserId = null, string? role = null, bool includeInactive = false)
        {
            if (includeInactive)
            {
                var canViewInactive = role == nameof(UserRole.PlatformAdmin)
                    || role == nameof(UserRole.SuperAdmin)
                    || (requestingUserId == farmerId && (role == nameof(UserRole.Farmer) || role == nameof(UserRole.FpoAdmin)))
                    || (role == nameof(UserRole.FpoAdmin) && await _context.Users.AnyAsync(user => user.Id == farmerId && user.FpoId == requestingUserId && user.Role == UserRole.Farmer));

                if (!canViewInactive)
                    throw new UnauthorizedAccessException("You do not have permission to view inactive products.");
            }

            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Farmer)
                .Where(p => p.FarmerId == farmerId && (includeInactive || p.IsActive))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var result = new List<ProductResponseDto>();
            foreach (var p in products)
            {
                result.Add(await MapToResponseDtoAsync(p, isBuyerContext: false));
            }
            return result;
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
                ImageData = !string.IsNullOrWhiteSpace(dto.ImageBase64) ? Convert.FromBase64String(dto.ImageBase64) : null,
                ImageContentType = dto.ImageContentType,
                Region = dto.Region,
                FarmerId = farmerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Auto-register crop in PlatformConfig with default 0.08 commission if not already present
            await _configService.EnsureCropConfigExistsAsync(dto.CropName);

            // Reload with Farmer included so response has FarmerName/FarmerLocation populated
            var created = await _context.Products
                .AsNoTracking()
                .Include(p => p.Farmer)
                .FirstAsync(p => p.Id == product.Id);

            return await MapToResponseDtoAsync(created, isBuyerContext: false);
        }

        public async Task<ProductResponseDto> UpdateAsync(Guid id, Guid requestingUserId, string? role, ProductDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Farmer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            var isOwner = product.FarmerId == requestingUserId;
            var isAdmin = !string.IsNullOrEmpty(role) && (
                role.Equals(nameof(UserRole.PlatformAdmin), StringComparison.OrdinalIgnoreCase) ||
                role.Equals(nameof(UserRole.SuperAdmin), StringComparison.OrdinalIgnoreCase) ||
                role.Equals(nameof(UserRole.Admin), StringComparison.OrdinalIgnoreCase)
            );

            var isFpoAdminOfThisFarmer = role == nameof(UserRole.FpoAdmin)
                   && product.Farmer != null
                   && product.Farmer.FpoId == requestingUserId;

            if (!isOwner && !isAdmin && !isFpoAdminOfThisFarmer)
                throw new UnauthorizedAccessException("You do not have permission to edit this product.");

            product.CropName = dto.CropName;
            product.Price = dto.Price;
            product.Quantity = dto.Quantity;
            product.Unit = dto.Unit;
            product.Category = dto.Category;
            product.HarvestDate = dto.HarvestDate;
            product.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.ImageBase64))
            {
                product.ImageData = Convert.FromBase64String(dto.ImageBase64);
                product.ImageContentType = dto.ImageContentType;
            }

            product.Region = dto.Region;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await MapToResponseDtoAsync(product, isBuyerContext: false);
        }

        public async Task DeleteAsync(Guid id, Guid requestingUserId, string? role)
        {
            var product = await _context.Products
                .Include(p => p.Farmer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            var isOwner = product.FarmerId == requestingUserId;
            var isPlatformAdmin = !string.IsNullOrEmpty(role) && (
                role.Equals(nameof(UserRole.PlatformAdmin), StringComparison.OrdinalIgnoreCase) ||
                role.Equals(nameof(UserRole.SuperAdmin), StringComparison.OrdinalIgnoreCase) ||
                role.Equals(nameof(UserRole.Admin), StringComparison.OrdinalIgnoreCase)
            );

            var isFpoAdminOfThisFarmer = role == nameof(UserRole.FpoAdmin)
                 && product.Farmer != null
                 && product.Farmer.FpoId == requestingUserId;

            if (!isOwner && !isPlatformAdmin && !isFpoAdminOfThisFarmer)
                throw new UnauthorizedAccessException("You do not have permission to delete this product.");

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private async Task<ProductResponseDto> MapToResponseDtoAsync(Product product, bool isBuyerContext = true)
        {
            var insight = PricingInsightHelper.Calculate(product.CropName, product.Category, product.Price);
            var farmerPricePerKg = UnitConverter.ToPricePerKg(product.Price, product.Unit);
            var buyerPricePerKg = await _configService.CalculateBuyerPriceAsync(farmerPricePerKg, product.CropName);

            var farmerPriceOriginal = product.Price;
            var buyerPriceOriginal = await _configService.CalculateBuyerPriceAsync(farmerPriceOriginal, product.CropName);

            var quantityInKg = UnitConverter.ToKgQuantity(product.Quantity, product.Unit);

            return new ProductResponseDto
            {
                Id = product.Id,
                CropName = product.CropName,
                Price = isBuyerContext ? buyerPricePerKg : farmerPriceOriginal,
                FarmerPrice = isBuyerContext ? farmerPricePerKg : farmerPriceOriginal,
                BuyerPrice = isBuyerContext ? buyerPricePerKg : buyerPriceOriginal,
                Quantity = isBuyerContext ? quantityInKg : product.Quantity,
                Unit = isBuyerContext ? ProductUnit.Kg : product.Unit,
                OriginalUnit = product.Unit,
                OriginalPrice = product.Price,
                OriginalQuantity = product.Quantity,
                PricePerKg = buyerPricePerKg,
                QuantityInKg = quantityInKg,
                Category = product.Category,
                HarvestDate = product.HarvestDate,
                Description = product.Description,
                ImageUrl = $"/api/products/{product.Id}/image",
                Region = product.Region,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                FarmerId = product.FarmerId,
                FarmerName = product.Farmer?.Name ?? string.Empty,
                FarmerLocation = product.Farmer?.Address ?? product.Farmer?.Location ?? product.Region,
                TypicalFarmerSharePercent = insight.TypicalFarmerSharePercent,
                EstimatedTraditionalRetailPrice = insight.EstimatedTraditionalRetailPrice,
                FarmerEarningsAdvantagePercent = insight.FarmerEarningsAdvantagePercent
            };
        }

        public async Task<(byte[] Data, string ContentType)> GetImageAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product?.ImageData == null)
                throw new KeyNotFoundException("Image not found.");

            return (product.ImageData, product.ImageContentType ?? "image/jpeg");
        }
    }
}