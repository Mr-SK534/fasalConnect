// backend/FarmerMarketplace.Api/DTOs/ConfirmPaymentDto.cs

using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.DTOs
{
    public class ConfirmPaymentDto
    {
        [Required]
        public Guid OrderId { get; set; }

        public string? RazorpayPaymentId { get; set; }

        public string? RazorpayOrderId { get; set; }

        public string? RazorpaySignature { get; set; }
    }
}
