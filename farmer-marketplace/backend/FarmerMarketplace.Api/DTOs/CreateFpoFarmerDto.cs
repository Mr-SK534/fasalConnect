using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.DTOs
{
    public class CreateFpoFarmerDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress, MaxLength(150)]
        public string? Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
