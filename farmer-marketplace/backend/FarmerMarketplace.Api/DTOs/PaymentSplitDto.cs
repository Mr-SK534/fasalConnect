// backend/FarmerMarketplace.Api/DTOs/PaymentSplitDto.cs

using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.DTOs
{
    // POST /payments/split
    public class PaymentSplitDto
    {
        [Required]
        public Guid OrderId { get; set; }
    }
}