using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.DTOs
{
    public class SuspendUserDto
    {
        public bool Suspended { get; set; }
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}
