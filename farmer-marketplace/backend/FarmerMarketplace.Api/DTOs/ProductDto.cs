// backend/FarmerMarketplace.Api/DTOs/ProductDto.cs

using System.ComponentModel.DataAnnotations;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    // Used for POST /products and PUT /products/{id}
    public class ProductDto
    {
        [Required]
        [MaxLength(100)]
        public string CropName { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }

        [Required]
        public ProductUnit Unit { get; set; }

        [Required]
        public ProductCategory Category { get; set; }

        [Required]
        public DateTime HarvestDate { get; set; }

        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Region { get; set; }
    }
}