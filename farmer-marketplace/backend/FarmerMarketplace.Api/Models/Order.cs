// backend/FarmerMarketplace.Api/Models/Order.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
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
    }
}