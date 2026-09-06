// backend/FarmerMarketplace.Api/DTOs/OrderDto.cs

using System.ComponentModel.DataAnnotations;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    // POST /orders
    public class OrderDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Order must contain at least one item.")]
        public List<OrderItemDto> Items { get; set; } = new();

        public bool IsBulkOrder { get; set; } = false;

        [Required]
        public DeliveryType DeliveryType { get; set; } = DeliveryType.Delivery;

        // Required only when DeliveryType == Delivery — validated in the service,
        // since [Required] here would incorrectly block Pickup orders
        public string? DeliveryAddress { get; set; }
    }
}