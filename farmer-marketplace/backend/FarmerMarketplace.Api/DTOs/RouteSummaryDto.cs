// backend/FarmerMarketplace.Api/DTOs/RouteSummaryDto.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    public class RouteSummaryDto
    {
        public Guid Id { get; set; }
        public DateTime BatchDate { get; set; }
        public string BatchWindow { get; set; } = "manual";
        public int VehicleCount { get; set; }
        public double TotalDistanceKm { get; set; }
        public DeliveryRouteStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
