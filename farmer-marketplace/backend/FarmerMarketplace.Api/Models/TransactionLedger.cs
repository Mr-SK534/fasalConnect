// backend/FarmerMarketplace.Api/Models/TransactionLedger.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public enum LedgerTransactionType
    {
        BuyerPaymentHeld,
        PayoutToFarmer,
        PlatformCommission,
        RefundToBuyer,
        DisputeResolution,
        GatewayFee,
        ConfigChange
    }

    public enum LedgerStatus
    {
        Pending,
        Completed,
        Failed
    }

    public class TransactionLedger
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public LedgerTransactionType TransactionType { get; set; }

        public Guid? OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        [Required]
        [MaxLength(100)]
        public string FromAccount { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ToAccount { get; set; } = string.Empty;

        [Column(TypeName = "decimal(12,2)")]
        public decimal AmountRs { get; set; }

        [Required]
        public LedgerStatus Status { get; set; } = LedgerStatus.Completed;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}
