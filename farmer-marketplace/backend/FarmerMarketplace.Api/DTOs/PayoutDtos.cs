// backend/FarmerMarketplace.Api/DTOs/PayoutDtos.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    public class FarmerPayoutHistoryDto
    {
        public Guid PayoutId { get; set; }
        public Guid FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public Guid? FpoAdminId { get; set; }
        public string? FpoAdminName { get; set; }
        public List<Guid> OrderIds { get; set; } = new();
        public decimal TotalAmountRs { get; set; }
        public DateTime PayoutDate { get; set; }
        public PayoutPaymentMethod PaymentMethod { get; set; }
        public string? UpiIdOrBankAccount { get; set; }
        public PayoutStatus Status { get; set; }
        public DateTime? ConfirmationTimestamp { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProcessPayoutsResultDto
    {
        public int PayoutCount { get; set; }
        public decimal TotalPayoutAmountRs { get; set; }
        public int FailedTransfers { get; set; }
        public List<FarmerPayoutHistoryDto> Payouts { get; set; } = new();
    }
}
