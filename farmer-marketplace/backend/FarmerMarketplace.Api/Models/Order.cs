// backend/FarmerMarketplace.Api/Models/Order.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        PickupScheduled,
        PickedUp,
        InTransit,
        Delivered,
        Cancelled
    }

    public enum DeliveryType
    {
        Delivery,
        Pickup
    }

    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BuyerId { get; set; }

        [ForeignKey(nameof(BuyerId))]
        public User? Buyer { get; set; }

        public bool IsBulkOrder { get; set; } = false;

        [Required]
        public DeliveryType DeliveryType { get; set; } = DeliveryType.Delivery;

        // Required if DeliveryType == Delivery; optional/ignored if Pickup
        [MaxLength(300)]
        public string? DeliveryAddress { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public Guid? RouteId { get; set; }
        [ForeignKey(nameof(RouteId))]
        public DeliveryRoute? Route { get; set; }

        public int? StopSequence { get; set; }
        public int? VehicleNumber { get; set; }
        public DateTime? EstimatedArrival { get; set; }

        public double? DeliveryLat { get; set; }
        public double? DeliveryLng { get; set; }
        public double? PickupLat { get; set; }
        public double? PickupLng { get; set; }

        // --- New Escrow & Weight Tracking Fields ---
        public double? QuantityPickedUpKg { get; set; }
        public double? QuantityDeliveredKg { get; set; }
        public DateTime? DeliveryConfirmedDate { get; set; }

        [MaxLength(100)]
        public string? CropName { get; set; }

        [MaxLength(100)]
        public string? Season { get; set; }

        public double? QuantityOrderedKg { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? FarmerAskingPricePerKg { get; set; }

        public Guid? FarmerId { get; set; }
        [ForeignKey(nameof(FarmerId))]
        public User? Farmer { get; set; }

        public Guid? FpoAdminId { get; set; }
        [ForeignKey(nameof(FpoAdminId))]
        public User? FpoAdmin { get; set; }

        public DateTime? DeliveryDateTarget { get; set; }
    }
}