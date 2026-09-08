// backend/FarmerMarketplace.Api/DTOs/FarmerEarningsDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    public class FarmerEarningsOrderItemDto
    {
        public Guid OrderItemId { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CropName { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public double QuantityKg { get; set; }
        public decimal ListedPricePerKg { get; set; }
        public decimal TotalFarmerEarningsRs { get; set; }
        public string Status { get; set; } = string.Empty;
        public string EscrowStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveryConfirmedDate { get; set; }
    }

    public class FarmerEarningsDto
    {
        public Guid FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public decimal TotalEarningsRs { get; set; }
        public decimal TotalGrossEarningsRs => TotalEarningsRs;

        public decimal HeldInEscrowRs { get; set; }

        public decimal DisbursedBankPayoutsRs { get; set; }
        public decimal DisbursedPayoutsRs => DisbursedBankPayoutsRs;

        public int DeliveredOrdersCount { get; set; }
        public int TotalFulfilledOrdersCount => DeliveredOrdersCount;
        public int TotalOrdersCount { get; set; }

        public string? BankAccountNumber { get; set; }
        public string? BankIfsc { get; set; }
        public string? AccountHolderName { get; set; }
        public string? UpiId { get; set; }

        public List<FarmerEarningsOrderItemDto> OrderLedger { get; set; } = new();
    }
}
