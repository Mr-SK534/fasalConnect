// backend/FarmerMarketplace.Api/DTOs/UpdateBasicProfileDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    // PUT /users/{id} — per v2 contract: name, phone, preferredLanguage only
    public class UpdateBasicProfileDto
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? PreferredLanguage { get; set; }
    }
}