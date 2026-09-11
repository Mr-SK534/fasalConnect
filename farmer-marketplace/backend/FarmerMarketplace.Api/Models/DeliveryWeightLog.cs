// backend/FarmerMarketplace.Api/Models/DeliveryWeightLog.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public class DeliveryWeightLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        public Guid? RouteId { get; set; }

        [ForeignKey(nameof(RouteId))]
        public DeliveryRoute? Route { get; set; }

        public DateTime PickupDatetime { get; set; } = DateTime.UtcNow;

        public double WeightAtPickupKg { get; set; }

        public DateTime? DeliveryDatetime { get; set; }

        public double? WeightAtDeliveryKg { get; set; }

        public double? WeightVarianceKg { get; set; }

        [MaxLength(500)]
        public string? VarianceReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
