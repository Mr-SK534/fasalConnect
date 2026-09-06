// backend/FarmerMarketplace.Api/DTOs/RouteResponseDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    public class RouteStopResponseDto
    {
        public Guid OrderId { get; set; }
        public int StopSequence { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public decimal DistanceFromPreviousKm { get; set; }
        public DateTime EstimatedArrival { get; set; }
    }

    public class RouteResponseDto
    {
        public Guid RouteId { get; set; }
        public double DeliveryHubLat { get; set; }
        public double DeliveryHubLng { get; set; }
        public List<RouteStopResponseDto> Stops { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}