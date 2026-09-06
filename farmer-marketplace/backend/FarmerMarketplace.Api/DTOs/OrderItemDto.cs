// backend/FarmerMarketplace.Api/DTOs/OrderItemDto.cs

using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.DTOs
{
    // Used inside OrderDto.Items[] — one line item per product being ordered
    public class OrderItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }
    }
}