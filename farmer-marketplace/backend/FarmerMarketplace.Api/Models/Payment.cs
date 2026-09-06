// backend/FarmerMarketplace.Api/Models/Payment.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public enum PaymentStatus
    {
        Created,
        Paid,
        Failed,
        Refunded
    }

    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        [Required]
        [MaxLength(100)]
        public string RazorpayOrderId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? RazorpayPaymentId { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "INR";

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Created;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PaymentSplit> Splits { get; set; } = new List<PaymentSplit>();
    }
}