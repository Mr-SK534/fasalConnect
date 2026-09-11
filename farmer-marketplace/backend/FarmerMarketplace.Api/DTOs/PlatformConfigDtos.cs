// backend/FarmerMarketplace.Api/DTOs/PlatformConfigDtos.cs

using System.ComponentModel.DataAnnotations;
using FarmerMarketplace.Api.Models;

using System.Text.Json.Serialization;

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
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        private string _newValue = string.Empty;

        [Required]
        [JsonPropertyName("newValue")]
        public string NewValue
        {
            get => _newValue;
            set => _newValue = value;
        }

        [JsonPropertyName("new_value")]
        public string NewValueSnakeCase
        {
            get => _newValue;
            set => _newValue = value;
        }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class AddCropConfigRequestDto
    {
        [Required]
        [JsonPropertyName("cropName")]
        public string CropName { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("commissionPct")]
        public decimal CommissionPct { get; set; } = 0.08m;

        [JsonPropertyName("description")]
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
        [JsonPropertyName("farmerPrice")]
        public decimal FarmerPrice { get; set; }

        [JsonPropertyName("farmer_price")]
        public decimal FarmerPriceSnakeCase { get => FarmerPrice; set => FarmerPrice = value; }

        [JsonPropertyName("newCommissionPct")]
        public decimal NewCommissionPct { get; set; }

        [JsonPropertyName("new_commission_pct")]
        public decimal NewCommissionPctSnakeCase { get => NewCommissionPct; set => NewCommissionPct = value; }
    }

    public class SimulatePriceChangeResultDto
    {
        [JsonPropertyName("farmerPrice")]
        public decimal FarmerPrice { get; set; }

        [JsonPropertyName("newCommissionPct")]
        public decimal NewCommissionPct { get; set; }

        [JsonPropertyName("buyerPrice")]
        public decimal BuyerPrice { get; set; }

        [JsonPropertyName("platformRevenuePerKg")]
        public decimal PlatformRevenuePerKg { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}
