// backend/FarmerMarketplace.Api/Models/EscrowTransaction.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public enum EscrowStatus
    {
        Held,
        PendingConfirmation,
        Released,
        Refunded,
        Disputed
    }

    public class EscrowTransaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        [Required]
        public Guid BuyerId { get; set; }

        [ForeignKey(nameof(BuyerId))]
        public User? Buyer { get; set; }

        [MaxLength(100)]
        public string PlatformAccountId { get; set; } = "PLATFORM_ESCROW_WALLET_01";

        [Column(TypeName = "decimal(12,2)")]
        public decimal OrderedAmountRs { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? ActualAmountRs { get; set; }

        [Required]
        public EscrowStatus Status { get; set; } = EscrowStatus.Held;

        public DateTime HeldDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReleaseDate { get; set; }
        public DateTime? RefundDate { get; set; }

        [MaxLength(1000)]
        public string? DisputeReason { get; set; }

        public DateTime? DisputeResolvedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
