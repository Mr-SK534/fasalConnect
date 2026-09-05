// backend/FarmerMarketplace.Api/DTOs/AdminSummaryDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    public class AdminSummaryDto
    {
        public int TotalFarmers { get; set; }
        public int TotalBuyers { get; set; }
        public int TotalFpoAdmins { get; set; }

        // Orders/Products counts default to 0 until those models/services exist —
        // AdminService will populate these once OrderService/ProductService are wired in.
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
    }
}