// backend/FarmerMarketplace.Api/DTOs/PaymentSplitResultDto.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    public class PaymentSplitResultDto
    {
        public Guid FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public string? RazorpayTransferId { get; set; }
    }
}