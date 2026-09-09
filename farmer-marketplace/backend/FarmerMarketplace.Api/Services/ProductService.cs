// backend/FarmerMarketplace.Api/Services/ProductService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;
using FarmerMarketplace.Api.Utils;

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
                .Select(p => new Product
                {
                    Id = p.Id,
                    CropName = p.CropName,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Unit = p.Unit,
                    Category = p.Category,
                    HarvestDate = p.HarvestDate,
                    Description = p.Description,
                    FarmerId = p.FarmerId,
                    ImageContentType = p.ImageContentType,
                    Farmer = p.Farmer,
                    Region = p.Region,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            var cropAggregates = products
                .GroupBy(p => p.CropName.Trim().ToLower())
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        TotalQtyKg = g.Sum(p => FarmerMarketplace.Api.Helpers.UnitConverter.ToKgQuantity(p.Quantity, p.Unit)),
                        FarmerCount = g.Select(p => p.FarmerId).Distinct().Count()
                    }
                );

            var result = new List<ProductResponseDto>();
            foreach (var p in products)
            {
                cropAggregates.TryGetValue(p.CropName.Trim().ToLower(), out var agg);
                var totalQtyKg = agg?.TotalQtyKg ?? FarmerMarketplace.Api.Helpers.UnitConverter.ToKgQuantity(p.Quantity, p.Unit);
                var farmerCount = agg?.FarmerCount ?? 1;

                result.Add(await MapToResponseDtoAsync(p, isBuyerContext: true, totalQtyKg: totalQtyKg, farmerCount: farmerCount));
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
                var farmerPricePerKg = FarmerMarketplace.Api.Helpers.UnitConverter.ToPricePerKg(product.Price, product.Unit);
                var buyerPricePerKg = await _configService.CalculateBuyerPriceAsync(farmerPricePerKg, product.CropName);
                var availableQtyKg = FarmerMarketplace.Api.Helpers.UnitConverter.ToKgQuantity(product.Quantity, product.Unit);

                buyerPricesPerKg.Add(buyerPricePerKg);
                farmerDtos.Add(new ProductAggregateFarmerDto
                {
                    FarmerId = product.FarmerId,
                    FarmerName = product.Farmer?.Name ?? string.Empty,
                    FarmerLocation = product.Farmer?.Address ?? product.Region,
                    AvailableQuantity = availableQtyKg,
                    Price = buyerPricePerKg
                });
            }

            var totalQtyKg = products.Sum(product => FarmerMarketplace.Api.Helpers.UnitConverter.ToKgQuantity(product.Quantity, product.Unit));

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
                .Select(p => new Product
                {
                    Id = p.Id,
                    CropName = p.CropName,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Unit = p.Unit,
                    Category = p.Category,
                    HarvestDate = p.HarvestDate,
                    Description = p.Description,
                    FarmerId = p.FarmerId,
                    ImageContentType = p.ImageContentType,
                    Farmer = p.Farmer,
                    Region = p.Region,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
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
                ImageData = Convert.FromBase64String(dto.ImageBase64),
                ImageContentType = dto.ImageContentType,
                Region = dto.Region,
                FarmerId = farmerId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Auto-register crop in PlatformConfig with default 0.08 commission if not already present
            await _configService.EnsureCropConfigExistsAsync(dto.CropName);

            // reload with Farmer included so response has FarmerName/FarmerLocation populated
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

            // FpoAdmin can edit only if this product's farmer is actually linked to them
            var isFpoAdminOfThisFarmer = role == nameof(UserRole.FpoAdmin)
                   && product.Farmer != null
                   && product.Farmer.FpoId == requestingUserId;

            if (!isOwner && !isFpoAdminOfThisFarmer)
                throw new UnauthorizedAccessException("You do not have permission to edit this product.");

            product.CropName = dto.CropName;
            product.Price = dto.Price;
            product.Quantity = dto.Quantity;
            product.Unit = dto.Unit;
            product.Category = dto.Category;
            product.HarvestDate = dto.HarvestDate;
            product.Description = dto.Description;
            product.ImageData = Convert.FromBase64String(dto.ImageBase64);
            product.ImageContentType = dto.ImageContentType;
            product.Region = dto.Region;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await MapToResponseDtoAsync(product, isBuyerContext: false);
        }

        public async Task DeleteAsync(Guid id, Guid requestingUserId, string? role)
        {
            var product = await _context.Products.Include(p => p.Farmer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            var isOwner = product.FarmerId == requestingUserId;
            var isPlatformAdmin = role == nameof(UserRole.PlatformAdmin);

            var isFpoAdminOfThisFarmer = role == nameof(UserRole.FpoAdmin)
                 && product.Farmer != null
                 && product.Farmer.FpoId == requestingUserId;

            // FpoAdmin can delete only if this product's farmer is actually linked to them
            if (!isOwner && !isPlatformAdmin && !isFpoAdminOfThisFarmer)
                throw new UnauthorizedAccessException("You do not have permission to delete this product.");

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private async Task<ProductResponseDto> MapToResponseDtoAsync(Product product, bool isBuyerContext = true, decimal totalQtyKg = 0m, int farmerCount = 0)
        {
            var insight = PricingInsightHelper.Calculate(product.CropName, product.Category, product.Price);
            var farmerPricePerKg = FarmerMarketplace.Api.Helpers.UnitConverter.ToPricePerKg(product.Price, product.Unit);
            var buyerPricePerKg = await _configService.CalculateBuyerPriceAsync(farmerPricePerKg, product.CropName);

            var farmerPriceOriginal = product.Price;
            var buyerPriceOriginal = await _configService.CalculateBuyerPriceAsync(farmerPriceOriginal, product.CropName);

            var quantityInKg = FarmerMarketplace.Api.Helpers.UnitConverter.ToKgQuantity(product.Quantity, product.Unit);

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
                FarmerLocation = product.Farmer?.Location,
                TypicalFarmerSharePercent = insight.TypicalFarmerSharePercent,
                EstimatedTraditionalRetailPrice = insight.EstimatedTraditionalRetailPrice,
                FarmerEarningsAdvantagePercent = insight.FarmerEarningsAdvantagePercent,
                TotalAvailableQuantityKg = totalQtyKg > 0m ? totalQtyKg : quantityInKg,
                FarmerCountForCrop = farmerCount > 0 ? farmerCount : 1
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