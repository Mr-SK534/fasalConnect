// backend/FarmerMarketplace.Api/DTOs/RegisterDto.cs

using System;
using System.ComponentModel.DataAnnotations;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // v2: phone is mandatory
        [Required]
        [MaxLength(15)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        // v2: email is optional
        [MaxLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(10)]
        public string? PreferredLanguage { get; set; } = "en";

        public Guid? FpoId { get; set; }
    }
}