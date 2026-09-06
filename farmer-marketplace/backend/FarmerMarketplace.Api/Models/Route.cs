// backend/FarmerMarketplace.Api/Models/Route.cs

using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.Models
{
    public class Route
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public double DeliveryHubLat { get; set; }

        [Required]
        public double DeliveryHubLng { get; set; }

        [Required]
        public Guid CreatedBy { get; set; }

        public ICollection<RouteStop> Stops { get; set; } = new List<RouteStop>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}