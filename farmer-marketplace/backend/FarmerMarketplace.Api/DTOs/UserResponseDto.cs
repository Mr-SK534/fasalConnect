// backend/FarmerMarketplace.Api/DTOs/UserResponseDto.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string PreferredLanguage { get; set; } = "en";
        public Guid? FpoId { get; set; }
    }
}