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

        [Required]
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

        // For FpoAdmin: link farmers under this FPO
        // For Farmer: which FPO (if any) they belong to
        public Guid? FpoId { get; set; }

        [ForeignKey(nameof(FpoId))]
        public User? Fpo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}