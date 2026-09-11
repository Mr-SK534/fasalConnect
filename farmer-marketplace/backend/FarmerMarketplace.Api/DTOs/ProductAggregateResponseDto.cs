namespace FarmerMarketplace.Api.DTOs
{
    public class ProductAggregateResponseDto
    {
        public string CropName { get; set; } = string.Empty;
        public decimal TotalAvailableQuantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int FarmerCount { get; set; }
        public List<ProductAggregateFarmerDto> Farmers { get; set; } = new();
    }

    public class ProductAggregateFarmerDto
    {
        public Guid FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string? FarmerLocation { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal Price { get; set; }
    }
}
