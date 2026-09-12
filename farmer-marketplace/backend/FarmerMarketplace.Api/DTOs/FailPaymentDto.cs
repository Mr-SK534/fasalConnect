// backend/FarmerMarketplace.Api/DTOs/FailPaymentDto.cs

using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.DTOs
{
    public class FailPaymentDto
    {
        [Required]
        public Guid OrderId { get; set; }

        public string? Reason { get; set; }
    }
}
