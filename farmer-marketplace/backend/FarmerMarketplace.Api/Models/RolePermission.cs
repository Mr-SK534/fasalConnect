// backend/FarmerMarketplace.Api/Models/RolePermission.cs

using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.Models
{
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty; // "superadmin", "admin", "manager"

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty; // "view_config", "edit_config", "edit_pricing", "edit_critical"

        [Required]
        [MaxLength(100)]
        public string Resource { get; set; } = "all";

        public bool CanPerform { get; set; } = false;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
