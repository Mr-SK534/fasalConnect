// backend/FarmerMarketplace.Api/DTOs/ProfileSetupDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    public class ProfileSetupDto
    {
        public string? Village { get; set; }
        public string? District { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Region { get; set; }
        public string? PrimaryCrops { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIfsc { get; set; }
        public string? AccountHolderName { get; set; }
        public string? UpiId { get; set; }
        public string? BusinessName { get; set; }
        public string? GstNumber { get; set; }
        public string? DeliveryAddress { get; set; }
    }
}