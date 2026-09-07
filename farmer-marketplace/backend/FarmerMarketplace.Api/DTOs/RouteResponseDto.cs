// backend/FarmerMarketplace.Api/DTOs/RouteResponseDto.cs

using FarmerMarketplace.Api.Models;
using RouteStop = FarmerMarketplace.Api.Records.RouteStop;

namespace FarmerMarketplace.Api.DTOs
{
    public class RouteResponseDto
    {
        public Guid Id { get; set; }
        public DateTime BatchDate { get; set; }
        public int VehicleCount { get; set; }
        public double TotalDistanceKm { get; set; }
        public DeliveryRouteStatus Status { get; set; }
        public Dictionary<int, List<RouteStop>> StopsByVehicle { get; set; } = new();
        public List<Guid> SkippedOrderIds { get; set; } = new();
        public bool HasUrgentStops { get; set; }
        public string? WarningMessage { get; set; }
    }
}
