// backend/FarmerMarketplace.Api/Models/User.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmerMarketplace.Api.Models
{
    public enum UserRole
    {
        Farmer,
        Buyer,
        FpoAdmin,
        PlatformAdmin
    }

    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

    
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(10)]
        public string PreferredLanguage { get; set; } = "en";

        public Guid? FpoId { get; set; }

        [ForeignKey(nameof(FpoId))]
        public User? Fpo { get; set; }

        // ---- Profile setup fields ----

        [MaxLength(100)]
        public string? Village { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(10)]
        public string? Pincode { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [MaxLength(200)]
        public string? Region { get; set; }

        // Comma-separated, e.g. "Wheat,Onion,Tomato"
        [MaxLength(500)]
        public string? PrimaryCrops { get; set; }

        [MaxLength(30)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(20)]
        public string? BankIfsc { get; set; }

        [MaxLength(100)]
        public string? AccountHolderName { get; set; }

        [MaxLength(100)]
        public string? UpiId { get; set; }

        [MaxLength(150)]
        public string? BusinessName { get; set; }

        [MaxLength(20)]
        public string? GstNumber { get; set; }

        [MaxLength(300)]
        public string? DeliveryAddress { get; set; }

        public bool IsProfileComplete { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}