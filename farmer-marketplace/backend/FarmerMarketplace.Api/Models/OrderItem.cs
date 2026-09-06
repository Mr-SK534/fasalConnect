// backend/FarmerMarketplace.Api/Models/OrderItem.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        // Denormalized so farmer-side order queries don't need to join through Product
        // every time, and so it's preserved even if the product is later deleted
        [Required]
        public Guid FarmerId { get; set; }

        [ForeignKey(nameof(FarmerId))]
        public User? Farmer { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        // Snapshot of price at order time — protects against the farmer changing
        // the product's price later and retroactively altering past order totals
        [Column(TypeName = "decimal(10,2)")]
        public decimal PriceAtOrderTime { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal SubTotal { get; set; }
    }
}