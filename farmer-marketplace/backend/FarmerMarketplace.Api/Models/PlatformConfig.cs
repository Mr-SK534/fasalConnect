// backend/FarmerMarketplace.Api/Models/PlatformConfig.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public enum ConfigValueType
    {
        Decimal,
        Integer,
        Percent,
        Boolean,
        Days,
        String
    }

    public class PlatformConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Key { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;

        [Required]
        public ConfigValueType ValueType { get; set; } = ConfigValueType.Decimal;

        [Column(TypeName = "decimal(12,4)")]
        public decimal? MinValue { get; set; }

        [Column(TypeName = "decimal(12,4)")]
        public decimal? MaxValue { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequiresRole { get; set; } = "admin"; // "superadmin" or "admin"

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
