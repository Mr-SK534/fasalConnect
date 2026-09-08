// backend/FarmerMarketplace.Api/DTOs/OptimizeRouteDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    public class OptimizeRouteDto
    {
        public List<Guid> OrderIds { get; set; } = new();
        public double DepotLat { get; set; }
        public double DepotLng { get; set; }
    }
}
