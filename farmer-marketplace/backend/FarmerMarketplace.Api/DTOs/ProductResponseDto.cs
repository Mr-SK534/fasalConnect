// backend/FarmerMarketplace.Api/DTOs/ProductResponseDto.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    // Used for GET /products, GET /products/{id}, GET /products/farmer/{farmerId}
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string CropName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public ProductUnit Unit { get; set; }
        public ProductCategory Category { get; set; }
        public DateTime HarvestDate { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Region { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Flattened farmer info for display on listings — avoids a second API call
        // from the frontend just to show "sold by" info on a product card
        public Guid FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string? FarmerLocation { get; set; }
    }
}