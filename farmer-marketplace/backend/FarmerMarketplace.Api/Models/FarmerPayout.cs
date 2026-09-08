// backend/FarmerMarketplace.Api/Models/FarmerPayout.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public enum PayoutPaymentMethod
    {
        Upi,
        BankTransfer,
        Check
    }

    public enum PayoutStatus
    {
        Scheduled,
        Processing,
        Completed,
        Failed
    }

    public class FarmerPayout
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid FarmerId { get; set; }

        [ForeignKey(nameof(FarmerId))]
        public User? Farmer { get; set; }

        public Guid? FpoAdminId { get; set; }

        [ForeignKey(nameof(FpoAdminId))]
        public User? FpoAdmin { get; set; }

        // JSON string of Order IDs consolidated in this payout batch
        [Required]
        public string OrderIdsJson { get; set; } = "[]";

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmountRs { get; set; }

        public DateTime PayoutDate { get; set; } = DateTime.UtcNow;

        [Required]
        public PayoutPaymentMethod PaymentMethod { get; set; } = PayoutPaymentMethod.BankTransfer;

        [MaxLength(100)]
        public string? UpiIdOrBankAccount { get; set; }

        [Required]
        public PayoutStatus Status { get; set; } = PayoutStatus.Scheduled;

        public DateTime? ConfirmationTimestamp { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
