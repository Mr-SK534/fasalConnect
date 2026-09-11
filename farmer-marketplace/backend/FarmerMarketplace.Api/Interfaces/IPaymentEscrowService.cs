// backend/FarmerMarketplace.Api/Interfaces/IPaymentEscrowService.cs

using FarmerMarketplace.Api.DTOs;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IPaymentEscrowService
    {
        Task<EscrowStatusResponseDto> InitiateEscrowAsync(Guid currentUserId, InitiateEscrowDto dto);
        Task<EscrowStatusResponseDto> GetEscrowStatusAsync(Guid orderId);
        Task<ConfirmDeliveryResultDto> ConfirmDeliveryAsync(ConfirmDeliveryDto dto);
        Task<EscrowStatusResponseDto> InitiateDisputeAsync(InitiateDisputeDto dto);
        Task<EscrowStatusResponseDto> ResolveDisputeAsync(ResolveDisputeDto dto);
        Task<ProcessPayoutsResultDto> ProcessPayoutsAsync(ProcessPayoutsDto dto);
        Task<List<FarmerPayoutHistoryDto>> GetFarmerPayoutHistoryAsync(Guid farmerId);
        Task<FarmerEarningsDto> GetFarmerEarningsAsync(Guid farmerId);
    }
}
