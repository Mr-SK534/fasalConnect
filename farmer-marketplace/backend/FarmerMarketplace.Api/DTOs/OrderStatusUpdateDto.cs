// backend/FarmerMarketplace.Api/DTOs/OrderStatusUpdateDto.cs

using System.ComponentModel.DataAnnotations;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    // PUT /orders/{id}/status
    public class OrderStatusUpdateDto
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}