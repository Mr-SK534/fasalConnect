// backend/FarmerMarketplace.Api/Models/Product.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
   

    

    public class Product
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string CropName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        [Required]
        public ProductUnit Unit { get; set; }

        [Required]
        public ProductCategory Category { get; set; }

        [Required]
        public DateTime HarvestDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        // Ownership — the farmer (or FpoAdmin listing on behalf of a linked farmer) who created this
        [Required]
        public Guid FarmerId { get; set; }

        [ForeignKey(nameof(FarmerId))]
        public User? Farmer { get; set; }

        // Denormalized for fast filtering on GET /products?region=
        [MaxLength(200)]
        public string? Region { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}