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

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        // Optional at registration, can be filled later via PUT /users/{id}
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? PreferredLanguage { get; set; }

        // Only relevant if Role == Farmer and they're joining an existing FPO
        public Guid? FpoId { get; set; }
    }
}