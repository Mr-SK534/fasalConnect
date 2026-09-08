// backend/FarmerMarketplace.Api/DTOs/EscrowDtos.cs

using System.ComponentModel.DataAnnotations;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    public class InitiateEscrowDto
    {
        [Required]
        public Guid OrderId { get; set; }
        
        public Guid? BuyerId { get; set; }
        
        public decimal? OverrideAmount { get; set; }
    }

    public class EscrowStatusResponseDto
    {
        public Guid EscrowId { get; set; }
        public Guid OrderId { get; set; }
        public Guid BuyerId { get; set; }
        public decimal AmountHeld { get; set; }
        public decimal? AmountActual { get; set; }
        public EscrowStatus Status { get; set; }
        public DateTime HeldDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime? RefundDate { get; set; }
        public string? DisputeReason { get; set; }
        public DateTime? DisputeResolvedDate { get; set; }
    }

    public class ConfirmDeliveryDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [Range(0.001, 1000000)]
        public double DeliveredKg { get; set; }

        public string? BuyerSignatureOrOtp { get; set; }

        public DateTime? Timestamp { get; set; }
    }

    public class ConfirmDeliveryResultDto
    {
        public Guid OrderId { get; set; }
        public Guid EscrowId { get; set; }
        public EscrowStatus EscrowStatus { get; set; }
        public double QuantityOrderedKg { get; set; }
        public double QuantityDeliveredKg { get; set; }
        public double WeightVarianceKg { get; set; }
        public decimal OrderedAmountRs { get; set; }
        public decimal ActualAmountRs { get; set; }
        public decimal BuyerFinalPriceRs { get; set; }
        public decimal FarmerReceivesRs { get; set; }
        public decimal PlatformRevenueRs { get; set; }
        public decimal PlatformLogisticsCostRs { get; set; }
        public decimal GatewayFeeRs { get; set; }
        public string? DisputeReason { get; set; }
    }

    public class InitiateDisputeDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public List<string>? PhotoUrls { get; set; }

        public string? BuyerNotes { get; set; }
    }

    public class ResolveDisputeDto
    {
        [Required]
        public Guid DisputeId { get; set; }

        // "release_to_farmer", "full_refund", "partial_refund"
        [Required]
        public string AdminDecision { get; set; } = "release_to_farmer";

        public decimal RefundSplitPct { get; set; } = 0.50m; // Default 50% for partial refund

        public string? Notes { get; set; }
    }

    public class ProcessPayoutsDto
    {
        public DateTime? TargetDate { get; set; }
    }
}
