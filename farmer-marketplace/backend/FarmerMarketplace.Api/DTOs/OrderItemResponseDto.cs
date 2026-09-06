// backend/FarmerMarketplace.Api/DTOs/OrderItemResponseDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    public class OrderItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string CropName { get; set; } = string.Empty;
        public Guid FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal PriceAtOrderTime { get; set; }
        public decimal SubTotal { get; set; }
    }
}