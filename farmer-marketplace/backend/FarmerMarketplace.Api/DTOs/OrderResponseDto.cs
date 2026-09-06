// backend/FarmerMarketplace.Api/DTOs/OrderResponseDto.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    // GET /orders/{id}, GET /orders/buyer/{buyerId}, GET /orders/farmer/{farmerId}
    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public bool IsBulkOrder { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public string? DeliveryAddress { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}