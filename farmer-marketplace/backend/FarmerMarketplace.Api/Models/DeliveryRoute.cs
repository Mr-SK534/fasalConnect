// backend/FarmerMarketplace.Api/Models/DeliveryRoute.cs

using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.Models
{
    public enum DeliveryRouteStatus
    {
        Generated,
        InProgress,
        Completed
    }

    public class DeliveryRoute
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime BatchDate { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string BatchWindow { get; set; } = "manual";

        public int VehicleCount { get; set; }

        public double TotalDistanceKm { get; set; }

        public string StopsJson { get; set; } = "[]";

        public DeliveryRouteStatus Status { get; set; } = DeliveryRouteStatus.Generated;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
