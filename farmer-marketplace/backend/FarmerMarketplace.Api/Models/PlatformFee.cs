// backend/FarmerMarketplace.Api/Models/PlatformFee.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public class PlatformFee
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EscrowId { get; set; }

        [ForeignKey(nameof(EscrowId))]
        public EscrowTransaction? Escrow { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        public decimal CommissionPct { get; set; } = 0.08m; // 8%

        [Column(TypeName = "decimal(12,2)")]
        public decimal CommissionAmountRs { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal LogisticsPartnerChargePerKg { get; set; } = 2.0m; // ₹2.0/kg

        [Column(TypeName = "decimal(10,2)")]
        public decimal LogisticsPlatformMarginPerKg { get; set; } = 0.5m; // ₹0.5/kg

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalLogisticsChargeRs { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        public decimal PaymentGatewayFeePct { get; set; } = 0.02m; // 2%

        [Column(TypeName = "decimal(12,2)")]
        public decimal PaymentGatewayFeeRs { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
