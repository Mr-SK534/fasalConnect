// backend/FarmerMarketplace.Api/DTOs/ProductQueryDto.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    // Bound from query string in GET /products?category=...&region=...&search=...
    public class ProductQueryDto
    {
        public ProductCategory? Category { get; set; }
        public string? Region { get; set; }
        public string? Search { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}