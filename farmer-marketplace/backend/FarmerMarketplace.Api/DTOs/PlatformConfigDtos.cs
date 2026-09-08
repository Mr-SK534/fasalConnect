// backend/FarmerMarketplace.Api/DTOs/PlatformConfigDtos.cs

using System.ComponentModel.DataAnnotations;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    public class PlatformConfigItemDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public ConfigValueType ValueType { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? Description { get; set; }
        public string RequiresRole { get; set; } = "admin";
        public bool CanEdit { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    public class PlatformConfigResponseDto
    {
        public string UserRole { get; set; } = string.Empty;
        public Dictionary<string, List<PlatformConfigItemDto>> Config { get; set; } = new();
    }

    public class ConfigUpdateRequestDto
    {
        [Required]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string NewValue { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    public class ConfigUpdateResultDto
    {
        public string Status { get; set; } = "updated";
        public string Key { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
        public string ChangedByRole { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class SimulatePriceChangeRequestDto
    {
        public decimal FarmerPrice { get; set; }
        public decimal NewCommissionPct { get; set; }
    }

    public class SimulatePriceChangeResultDto
    {
        public decimal FarmerPrice { get; set; }
        public decimal NewCommissionPct { get; set; }
        public decimal BuyerPrice { get; set; }
        public decimal PlatformRevenuePerKg { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
