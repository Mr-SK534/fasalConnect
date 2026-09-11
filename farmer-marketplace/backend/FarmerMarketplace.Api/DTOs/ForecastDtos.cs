// backend/FarmerMarketplace.Api/DTOs/ForecastDtos.cs

namespace FarmerMarketplace.Api.DTOs
{
    public class HistoricalDataPointDto
    {
        public DateTime Date { get; set; }
        public float QuantitySoldKg { get; set; }
        public float AvgPricePerKg { get; set; }
    }

    public class ForecastDataPointDto
    {
        public DateTime Date { get; set; }
        public float ForecastedQuantityKg { get; set; }
        public float LowerBoundKg { get; set; }
        public float UpperBoundKg { get; set; }
    }

    public class CropForecastResultDto
    {
        public string CropName { get; set; } = string.Empty;
        public string Category { get; set; } = "Vegetables";
        public string Region { get; set; } = "All Regions";
        public int ForecastHorizonDays { get; set; } = 30;
        public double ConfidenceLevel { get; set; } = 0.95;
        public string Trend { get; set; } = "Stable";
        public double TotalProjectedDemandKg { get; set; }
        public string HarvestAdvisory { get; set; } = string.Empty;
        public string PriceAdvisory { get; set; } = string.Empty;
        public bool IsCategoryTransferModel { get; set; } = false;
        public string CategoryModelNote { get; set; } = string.Empty;
        public List<HistoricalDataPointDto> HistoricalPoints { get; set; } = new();
        public List<ForecastDataPointDto> ForecastPoints { get; set; } = new();
    }

    public class FarmerForecastResultDto
    {
        public Guid FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<string> PrimaryCrops { get; set; } = new();
        public List<CropForecastResultDto> CropForecasts { get; set; } = new();
        public double Combined30DayDemandKg { get; set; }
        public string OverallRecommendation { get; set; } = string.Empty;
    }

    public class FarmerListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string PrimaryCrops { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class CropDemandSummaryDto
    {
        public string CropName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public float CurrentWeeklyDemandKg { get; set; }
        public float ProjectedWeeklyDemandKg { get; set; }
        public double TrendPercentage { get; set; }
        public string TrendLabel { get; set; } = "Stable";
        public string RecommendedAction { get; set; } = string.Empty;
    }
}
