// backend/FarmerMarketplace.Api/Helpers/UnitConverter.cs

using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.Helpers
{
    public static class UnitConverter
    {
        /// <summary>
        /// Gets the conversion factor to convert quantity from the specified unit to KG.
        /// 1 Ton = 1000 KG
        /// 1 Quintal = 100 KG
        /// 1 KG = 1 KG
        /// Default/Fallback = 1.0 (for missing, invalid or non-weight units)
        /// </summary>
        public static decimal GetKgFactor(ProductUnit unit)
        {
            return unit switch
            {
                ProductUnit.Ton => 1000m,
                ProductUnit.Quintal => 100m,
                ProductUnit.Kg => 1m,
                _ => 1m
            };
        }

        /// <summary>
        /// Gets the conversion factor from string unit representation with fallback to 1.0.
        /// </summary>
        public static decimal GetKgFactor(string? unitStr)
        {
            if (string.IsNullOrWhiteSpace(unitStr)) return 1m;

            if (Enum.TryParse<ProductUnit>(unitStr, true, out var parsedUnit))
            {
                return GetKgFactor(parsedUnit);
            }

            var normalized = unitStr.Trim().ToLowerInvariant();
            if (normalized is "ton" or "tons" or "t") return 1000m;
            if (normalized is "quintal" or "quintals" or "q") return 100m;
            if (normalized is "kg" or "kgs" or "kilogram" or "kilograms") return 1m;

            // Fallback for missing/invalid or unhandled unit types
            return 1m;
        }

        /// <summary>
        /// Converts a quantity from the given unit to KG.
        /// Example: 5 TON -> 5000 KG
        /// </summary>
        public static decimal ToKgQuantity(decimal quantity, ProductUnit unit)
        {
            return Math.Round(quantity * GetKgFactor(unit), 2);
        }

        /// <summary>
        /// Converts a quantity from string unit to KG.
        /// </summary>
        public static decimal ToKgQuantity(decimal quantity, string? unitStr)
        {
            return Math.Round(quantity * GetKgFactor(unitStr), 2);
        }

        /// <summary>
        /// Asking price is specified per KG by farmers across all product listing units.
        /// Example: ₹26 per KG for a product listed with 230 Quintals available quantity.
        /// </summary>
        public static decimal ToPricePerKg(decimal price, ProductUnit unit)
        {
            return Math.Round(price, 4);
        }

        /// <summary>
        /// Returns asking price per KG from string representation.
        /// </summary>
        public static decimal ToPricePerKg(decimal price, string? unitStr)
        {
            return Math.Round(price, 4);
        }

        /// <summary>
        /// Converts a quantity in KG back to the product's original unit for database inventory updates.
        /// Example: 500 KG -> 5.0 QUINTAL (500 / 100)
        /// </summary>
        public static decimal ToOriginalUnitQuantity(decimal kgQuantity, ProductUnit unit)
        {
            var factor = GetKgFactor(unit);
            return factor > 0 ? Math.Round(kgQuantity / factor, 4) : kgQuantity;
        }

        /// <summary>
        /// Returns asking price per KG.
        /// </summary>
        public static decimal ToOriginalUnitPrice(decimal pricePerKg, ProductUnit unit)
        {
            return Math.Round(pricePerKg, 2);
        }
    }
}
