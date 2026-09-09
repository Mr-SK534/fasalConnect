using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IWhatsAppService
    {
        Task ReceiveMessageAsync(WhatsAppIncomingDto request);

        Task SendMessageAsync(WhatsAppSendDto request);

         Task<bool> SendMessageAsync(string phoneNumber, string message);
        Task NotifyOrderStatusChangeAsync(Order order, OrderStatus newStatus);
        Task NotifyPaymentCapturedAsync( Order order, decimal amount, string paymentId);
    }
}