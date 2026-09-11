using System;
using System.Threading.Tasks;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IWhatsAppService
    {
        Task NotifyOrderStatusChangeAsync(Order order, OrderStatus newStatus);
        Task NotifyPaymentCapturedAsync(Order order, decimal amount, string paymentId);
        Task NotifyDeliveryDispatchedAsync(Order order, string? vehicleNumber, DateTime? estimatedArrival);
        Task ReceiveMessageAsync(WhatsAppIncomingDto request);
        Task<bool> SendMessageAsync(string phoneNumber, string message);
        Task SendMessageAsync(WhatsAppSendDto request);
    }
}