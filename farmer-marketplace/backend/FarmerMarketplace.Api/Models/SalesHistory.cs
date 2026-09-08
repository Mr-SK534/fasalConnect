// backend/FarmerMarketplace.Api/Models/SalesHistory.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public class SalesHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string CropName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Region { get; set; } = string.Empty;

        public Guid? FarmerId { get; set; }

        [ForeignKey(nameof(FarmerId))]
        public User? Farmer { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public float QuantitySoldKg { get; set; }

        public float AveragePricePerKg { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
