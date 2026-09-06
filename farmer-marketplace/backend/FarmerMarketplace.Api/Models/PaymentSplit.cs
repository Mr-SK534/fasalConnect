// backend/FarmerMarketplace.Api/Models/PaymentSplit.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public enum TransferStatus
    {
        Pending,
        Completed,
        Failed
    }

    // One row per farmer who supplied part of the order — records how much
    // of the payment is owed to them, and (eventually) the Razorpay transfer id.
    public class PaymentSplit
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PaymentId { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public Payment? Payment { get; set; }

        [Required]
        public Guid FarmerId { get; set; }

        [ForeignKey(nameof(FarmerId))]
        public User? Farmer { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        [Required]
        public TransferStatus TransferStatus { get; set; } = TransferStatus.Pending;

        [MaxLength(100)]
        public string? RazorpayTransferId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}