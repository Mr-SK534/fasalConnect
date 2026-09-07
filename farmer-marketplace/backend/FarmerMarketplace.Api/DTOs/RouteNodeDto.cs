namespace FarmerMarketplace.Api.DTOs
{
    public class RouteNodeDto
    {
        public Guid OrderId { get; set; }
        public bool IsPickup { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? FarmerName { get; set; }
        public long Demand { get; set; }
    }
}
