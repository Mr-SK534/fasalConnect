// backend/FarmerMarketplace.Api/DTOs/ReportingDtos.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.DTOs
{
    public class TransactionLedgerFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public LedgerTransactionType? TransactionType { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? FarmerId { get; set; }
    }

    public class PlatformPlnlDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal CommissionRevenueRs { get; set; }
        public decimal LogisticsMarginRs { get; set; }
        public decimal SubscriptionRevenueRs { get; set; }
        public decimal TotalRevenueRs { get; set; }
        public decimal GatewayFeesRs { get; set; }
        public decimal LogisticsPayoutsRs { get; set; }
        public decimal FixedCostsRs { get; set; }
        public decimal TotalCostsRs { get; set; }
        public decimal NetProfitRs { get; set; }
        public decimal NetMarginPct { get; set; }
        public decimal PlatformTakeOfConsumerSpendPct { get; set; }
    }

    public class FarmerEarningsBreakdownDto
    {
        public Guid FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string CropName { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public double QuantityOrderedKg { get; set; }
        public double QuantityDeliveredKg { get; set; }
        public decimal AskingPricePerKg { get; set; }
        public decimal GrossEarningsRs { get; set; }
        public decimal CommissionDeductedRs { get; set; }
        public decimal LogisticsDeductedRs { get; set; }
        public decimal NetPayoutRs { get; set; }
        public int OrderCount { get; set; }
    }
}
