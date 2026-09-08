namespace FarmerMarketplace.Api.DTOs
{
    public class FpoEarningsDto
    {
        public decimal TotalEarnings { get; set; }
        public int PaidOrders { get; set; }
        public int LinkedFarmers { get; set; }
    }
}
