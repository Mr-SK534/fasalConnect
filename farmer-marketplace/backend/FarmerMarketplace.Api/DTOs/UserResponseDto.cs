// backend/FarmerMarketplace.Api/DTOs/UserResponseDto.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public UserRole Role { get; set; }
        public string? Location { get; set; }
        public string PreferredLanguage { get; set; } = "en";
        public Guid? FpoId { get; set; }

        // v2: required on every user object per contract
        public bool IsProfileComplete { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? Village { get; set; }
        public string? District { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? PrimaryCrops { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIfsc { get; set; }
        public string? AccountHolderName { get; set; }
        public string? UpiId { get; set; }
        public string? BusinessName { get; set; }
        public string? GstNumber { get; set; }
        public string? DeliveryAddress { get; set; }
    }
}