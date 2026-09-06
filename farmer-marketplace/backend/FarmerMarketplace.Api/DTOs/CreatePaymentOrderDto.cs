// backend/FarmerMarketplace.Api/DTOs/CreatePaymentOrderDto.cs

using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.DTOs
{
    // POST /payments/create-order
    public class CreatePaymentOrderDto
    {
        [Required]
        public Guid OrderId { get; set; }

        // Client-sent amount is accepted per contract shape, but the service
        // always recalculates from Order.TotalAmount server-side and ignores
        // this value if it doesn't match — never trust payment amount from the client.
        public decimal? Amount { get; set; }
    }
}