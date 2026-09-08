namespace FarmerMarketplace.Api.DTOs
{
    public class WhatsAppIncomingDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}