// backend/FarmerMarketplace.Api/DTOs/PendingOrderDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    public class PendingOrderDto
    {
        public Guid OrderId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public double TotalQuantity { get; set; }
        public string CropName { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
