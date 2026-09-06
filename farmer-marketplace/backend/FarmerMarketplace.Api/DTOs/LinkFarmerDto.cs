// backend/FarmerMarketplace.Api/DTOs/LinkFarmerDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    // POST /fpo/{fpoId}/farmers
    public class LinkFarmerDto
    {
        public Guid? FarmerId { get; set; }
        public string? FarmerEmail { get; set; }
    }
}