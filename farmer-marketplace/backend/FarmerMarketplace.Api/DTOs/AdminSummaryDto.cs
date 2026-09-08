// backend/FarmerMarketplace.Api/DTOs/AdminSummaryDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    public class SuperAdminBankPayoutItemDto
    {
        public Guid OrderId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public double DeliveredKg { get; set; }
        public decimal FarmerListedPricePerKg { get; set; }
        public decimal FarmerTotalPayoutRs { get; set; }
        public decimal ExtraBuyerMarkupPerKg { get; set; }
        public decimal SuperAdminCollectedRevenueRs { get; set; }
        public string SuperAdminBankAccount { get; set; } = string.Empty;
        public string Status { get; set; } = "Completed";
        public DateTime Timestamp { get; set; }
    }

    public class AdminSummaryDto
    {
        public int TotalFarmers { get; set; }
        public int TotalBuyers { get; set; }
        public int TotalFpoAdmins { get; set; }

        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }

        // --- SuperAdmin Bank & Collected Revenue Wallet ---
        public string? SuperAdminName { get; set; }
        public string? SuperAdminEmail { get; set; }
        public string? SuperAdminPhone { get; set; }
        public string? SuperAdminBankAccountNumber { get; set; }
        public string? SuperAdminBankIfsc { get; set; }
        public string? SuperAdminAccountHolderName { get; set; }
        public string? SuperAdminUpiId { get; set; }

        public decimal TotalBuyerPaymentsRs { get; set; }
        public decimal TotalFarmerPayoutsRs { get; set; }
        public decimal TotalSuperAdminRevenueRs { get; set; }

        public List<SuperAdminBankPayoutItemDto> SuperAdminPayoutBreakdown { get; set; } = new();
    }
}