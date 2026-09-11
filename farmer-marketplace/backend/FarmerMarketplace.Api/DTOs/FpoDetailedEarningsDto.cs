// backend/FarmerMarketplace.Api/DTOs/FpoDetailedEarningsDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    public class FpoFarmerBreakdownDto
    {
        public Guid FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public double TotalQuantityKg { get; set; }
        public decimal TotalGrossEarningsRs { get; set; }
        public string BankAccount { get; set; } = string.Empty;
        public string UpiId { get; set; } = string.Empty;
    }

    public class FpoDetailedEarningsDto
    {
        public Guid FpoId { get; set; }
        public string FpoName { get; set; } = string.Empty;

        public decimal TotalNetworkEarningsRs { get; set; }
        public decimal HeldInEscrowRs { get; set; }
        public decimal DisbursedPayoutsRs { get; set; }
        public int LinkedFarmersCount { get; set; }
        public int ActiveFarmersCount { get; set; }
        public int TotalFulfilledOrdersCount { get; set; }

        public List<FpoFarmerBreakdownDto> FarmerBreakdown { get; set; } = new();
        public List<FarmerEarningsOrderItemDto> NetworkOrderLedger { get; set; } = new();
    }
}
