using System.ComponentModel.DataAnnotations;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    public class OverrideOrderStatusDto
    {
        [Required]
        public OrderStatus Status { get; set; }
        [Required]
        public string Note { get; set; } = string.Empty;
    }
}
