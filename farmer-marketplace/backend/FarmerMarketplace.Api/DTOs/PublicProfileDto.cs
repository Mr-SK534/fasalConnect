// backend/FarmerMarketplace.Api/DTOs/PublicProfileDto.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    // GET /users/{id} — per v2 contract: never returns email, password, or bank details
    public class PublicProfileDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? Village { get; set; }
        public string? District { get; set; }
        public string? State { get; set; }
        public string? Phone { get; set; }
        public string PreferredLanguage { get; set; } = "en";
        public string? PrimaryCrops { get; set; }
    }
}