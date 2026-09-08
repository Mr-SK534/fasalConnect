using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IWhatsAppService
    {
        Task ReceiveMessageAsync(WhatsAppIncomingDto request);

        Task SendMessageAsync(WhatsAppSendDto request);
    }
}