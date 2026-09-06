// backend/FarmerMarketplace.Api/Interfaces/IPaymentService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IPaymentService
    {
        Task<CreatePaymentOrderResponseDto> CreateOrderAsync(Guid buyerId, CreatePaymentOrderDto dto);

        // rawBody + signature header, so the controller can pass through the exact
        // bytes Razorpay signed — needed for HMAC verification to match
        Task HandleWebhookAsync(string rawBody, string? signatureHeader);

        Task<List<PaymentSplitResultDto>> SplitAsync(Guid orderId);
    }
}