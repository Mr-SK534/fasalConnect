// backend/FarmerMarketplace.Api/DTOs/UpdateBasicProfileDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    public class UpdateBasicProfileDto
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? Address { get; set; }
        public string? District { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? Region { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIfsc { get; set; }
        public string? AccountHolderName { get; set; }
        public string? UpiId { get; set; }
        public string? BusinessName { get; set; }
        public string? GstNumber { get; set; }
        public string? DeliveryAddress { get; set; }
    }
}