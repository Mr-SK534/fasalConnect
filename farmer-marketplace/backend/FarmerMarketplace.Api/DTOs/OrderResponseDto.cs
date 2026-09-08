// backend/FarmerMarketplace.Api/DTOs/OrderResponseDto.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string? BuyerPhone { get; set; }
        public bool IsBulkOrder { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public string? DeliveryAddress { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // --- Extended Fields ---
        public string? CropName { get; set; }
        public string? Season { get; set; }
        public double? QuantityOrderedKg { get; set; }
        public double? QuantityPickedUpKg { get; set; }
        public double? QuantityDeliveredKg { get; set; }
        public decimal? FarmerAskingPricePerKg { get; set; }
        public Guid? FarmerId { get; set; }
        public string? FarmerName { get; set; }
        public Guid? FpoAdminId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? DeliveryLat { get; set; }
        public double? DeliveryLng { get; set; }
        public double? PickupLat { get; set; }
        public double? PickupLng { get; set; }
        public DateTime? DeliveryDateTarget { get; set; }
        public DateTime? DeliveryConfirmedDate { get; set; }
        public Guid? RouteId { get; set; }
        public int? StopSequence { get; set; }
        public int? VehicleNumber { get; set; }
        public DateTime? EstimatedArrival { get; set; }
    }
}