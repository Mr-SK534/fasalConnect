// backend/FarmerMarketplace.Api/DTOs/RouteOptimizeDto.cs

using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.DTOs
{
    public class LatLngDto
    {
        [Required]
        public double Lat { get; set; }

        [Required]
        public double Lng { get; set; }
    }

    // POST /routes/optimize
    public class RouteOptimizeDto
    {
        [Required]
        [MinLength(2, ErrorMessage = "At least 2 orders are needed to optimize a route.")]
        public List<Guid> OrderIds { get; set; } = new();

        [Required]
        public LatLngDto DeliveryHubLocation { get; set; } = new();
    }
}