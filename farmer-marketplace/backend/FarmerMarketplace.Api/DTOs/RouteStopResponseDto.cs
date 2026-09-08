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
        public string StopType { get; set; } = "Delivery";
        public string? Address { get; set; }
        public string? FarmerName { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime DeliveryDate { get; set; }
    }
}