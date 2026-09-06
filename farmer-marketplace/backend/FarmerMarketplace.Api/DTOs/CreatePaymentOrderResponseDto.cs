// backend/FarmerMarketplace.Api/DTOs/CreatePaymentOrderResponseDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    public class CreatePaymentOrderResponseDto
    {
        public string RazorpayOrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
    }
}