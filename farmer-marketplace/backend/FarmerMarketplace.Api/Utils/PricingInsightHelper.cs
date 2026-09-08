// backend/FarmerMarketplace.Api/Utils/PricingInsightHelper.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.Utils
{
    public class PricingInsight
    {
        public double TypicalFarmerSharePercent { get; set; }
        public decimal EstimatedTraditionalRetailPrice { get; set; }
        public double FarmerEarningsAdvantagePercent { get; set; }
    }

    // Static benchmark data derived from RBI's TOP price dynamics paper and NABARD/
    // agri value-chain research. These are illustrative averages, not live market
    // prices — the goal is to visualize the "middleman gap" problem your research
    // identified, not to provide a precise real-time pricing recommendation.
    public static class PricingInsightHelper
    {
        // Directly backed by the research doc's crop-specific tables
        private static readonly Dictionary<string, double> CropSpecificFarmerSharePercent =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "Tomato", 33 },
                { "Onion", 36 },
                { "Potato", 37 },
                { "Banana", 31 },
                { "Grapes", 35 },
                { "Mango", 43 }
            };

        // Category-level fallback for crops not individually covered by the research.
        // Vegetables/Fruits averages are close to the research's own examples;
        // Grains/Pulses/Spices/Dairy/Other are reasonable estimates, not sourced —
        // update these if better data becomes available.
        private static readonly Dictionary<ProductCategory, double> CategoryFallbackPercent = new()
        {
            { ProductCategory.Vegetables, 35 },
            { ProductCategory.Fruits, 36 },
            { ProductCategory.Grains, 40 },
            { ProductCategory.Pulses, 40 },
            { ProductCategory.Spices, 35 },
            { ProductCategory.Dairy, 45 },
            { ProductCategory.Other, 35 }
        };

        public static PricingInsight Calculate(string cropName, ProductCategory category, decimal listedPrice)
        {
            var percent = CropSpecificFarmerSharePercent.TryGetValue(cropName.Trim(), out var specific)
                ? specific
                : CategoryFallbackPercent.GetValueOrDefault(category, 35);

            // If `listedPrice` is what the farmer fully receives here, this estimates
            // what the equivalent retail price would be if the farmer only received
            // their typical mandi-channel share instead.
            var estimatedTraditionalRetailPrice = listedPrice / (decimal)(percent / 100.0);
            var advantagePercent = Math.Round(((100.0 / percent) - 1) * 100, 1);

            return new PricingInsight
            {
                TypicalFarmerSharePercent = percent,
                EstimatedTraditionalRetailPrice = Math.Round(estimatedTraditionalRetailPrice, 2),
                FarmerEarningsAdvantagePercent = advantagePercent
            };
        }
    }
}