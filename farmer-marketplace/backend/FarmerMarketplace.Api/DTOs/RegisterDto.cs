// backend/FarmerMarketplace.Api/DTOs/RegisterDto.cs

using System.ComponentModel.DataAnnotations;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // v2: phone is now mandatory
        [Required]
        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        // v2: email is now optional
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public string? Location { get; set; }
        public string? PreferredLanguage { get; set; }
        public Guid? FpoId { get; set; }
    }
}