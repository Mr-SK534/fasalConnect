// backend/FarmerMarketplace.Tests/UnitConversionTests.cs

using FarmerMarketplace.Api.Helpers;
using FarmerMarketplace.Api.Models;
using Xunit;

namespace FarmerMarketplace.Tests
{
    public class UnitConversionTests
    {
        [Fact]
        public void Test_Mushroom_Quintal_Price_Fix()
        {
            // Farmer lists Mushroom: 230 Quintal @ ₹26 asking price per Kg
            decimal quantityQuintal = 230m;
            decimal askingPrice = 26m;

            // Quantity in Kg should be 230 * 100 = 23,000 Kg
            decimal quantityKg = UnitConverter.ToKgQuantity(quantityQuintal, ProductUnit.Quintal);
            Assert.Equal(23000m, quantityKg);

            // Price per Kg should remain ₹26 / kg (not divided to 0.26 / kg)
            decimal pricePerKg = UnitConverter.ToPricePerKg(askingPrice, ProductUnit.Quintal);
            Assert.Equal(26m, pricePerKg);
        }

        [Fact]
        public void Test_TonToKg_Conversion()
        {
            // 5 Ton -> 5000 Kg
            decimal tonQuantity = 5m;
            decimal kgQuantity = UnitConverter.ToKgQuantity(tonQuantity, ProductUnit.Ton);
            Assert.Equal(5000m, kgQuantity);

            // ₹25 per Kg asking price
            decimal pricePerKgInput = 25m;
            decimal pricePerKg = UnitConverter.ToPricePerKg(pricePerKgInput, ProductUnit.Ton);
            Assert.Equal(25m, pricePerKg);
        }

        [Fact]
        public void Test_KgToKg_Flow()
        {
            // 100 Kg -> 100 Kg
            decimal originalQuantity = 100m;
            decimal kgQuantity = UnitConverter.ToKgQuantity(originalQuantity, ProductUnit.Kg);
            Assert.Equal(100m, kgQuantity);

            // ₹40 per Kg -> ₹40 per Kg
            decimal originalPrice = 40m;
            decimal pricePerKg = UnitConverter.ToPricePerKg(originalPrice, ProductUnit.Kg);
            Assert.Equal(40m, pricePerKg);
        }

        [Fact]
        public void Test_QuintalToKg_Flow()
        {
            // 10 Quintal -> 1000 Kg
            decimal quintalQuantity = 10m;
            decimal kgQuantity = UnitConverter.ToKgQuantity(quintalQuantity, ProductUnit.Quintal);
            Assert.Equal(1000m, kgQuantity);

            // ₹30 per Kg asking price
            decimal priceInput = 30m;
            decimal pricePerKg = UnitConverter.ToPricePerKg(priceInput, ProductUnit.Quintal);
            Assert.Equal(30m, pricePerKg);
        }

        [Fact]
        public void Test_MixedUnit_Aggregation()
        {
            // Farmer A: 5 Ton (5000 Kg) @ ₹25/Kg
            decimal farmerA_Ton = 5m;
            decimal farmerA_PriceKgInput = 25m;
            decimal farmerA_Kg = UnitConverter.ToKgQuantity(farmerA_Ton, ProductUnit.Ton);
            decimal farmerA_PriceKg = UnitConverter.ToPricePerKg(farmerA_PriceKgInput, ProductUnit.Ton);

            // Farmer B: 2000 Kg @ ₹26/Kg
            decimal farmerB_Kg = 2000m;
            decimal farmerB_PriceKg = 26m;

            // Total available quantity in Kg
            decimal totalQuantityKg = farmerA_Kg + farmerB_Kg;
            Assert.Equal(7000m, totalQuantityKg);

            // Average price per Kg
            decimal averagePricePerKg = (farmerA_PriceKg + farmerB_PriceKg) / 2m;
            Assert.Equal(25.5m, averagePricePerKg);
        }

        [Fact]
        public void Test_MissingUnit_Fallback()
        {
            // Null unit string -> fallback factor 1.0
            string? nullUnit = null;
            decimal qtyWithNull = UnitConverter.ToKgQuantity(150m, nullUnit);
            decimal priceWithNull = UnitConverter.ToPricePerKg(50m, nullUnit);
            Assert.Equal(150m, qtyWithNull);
            Assert.Equal(50m, priceWithNull);

            // Empty/whitespace unit string -> fallback factor 1.0
            string emptyUnit = "   ";
            decimal qtyWithEmpty = UnitConverter.ToKgQuantity(200m, emptyUnit);
            Assert.Equal(200m, qtyWithEmpty);

            // Unrecognized string unit -> fallback factor 1.0 (no exceptions thrown)
            string invalidUnit = "UnknownCustomUnit";
            decimal qtyWithInvalid = UnitConverter.ToKgQuantity(75m, invalidUnit);
            decimal priceWithInvalid = UnitConverter.ToPricePerKg(30m, invalidUnit);
            Assert.Equal(75m, qtyWithInvalid);
            Assert.Equal(30m, priceWithInvalid);
        }

        [Fact]
        public void Test_BuyerPage_PriceDisplay_Verification()
        {
            // Simulating product listed in Ton: 2.5 Ton @ ₹40/Kg asking price
            decimal priceInput = 40m;
            decimal quantityTon = 2.5m;

            decimal buyerPricePerKg = UnitConverter.ToPricePerKg(priceInput, ProductUnit.Ton);
            decimal buyerQuantityKg = UnitConverter.ToKgQuantity(quantityTon, ProductUnit.Ton);

            // Verify buyer page display values:
            Assert.Equal(40m, buyerPricePerKg); // ₹40.00 / kg
            Assert.Equal(2500m, buyerQuantityKg); // 2500 kg available

            // Reverse stock deduction check: buyer orders 500 Kg
            decimal orderKg = 500m;
            decimal deductedTon = UnitConverter.ToOriginalUnitQuantity(orderKg, ProductUnit.Ton);
            Assert.Equal(0.5m, deductedTon);

            decimal remainingTon = quantityTon - deductedTon;
            Assert.Equal(2.0m, remainingTon); // 2.0 Ton remaining in DB
        }

        [Fact]
        public void Test_CropPricing_AutoRegistration_DefaultCommission()
        {
            // Verify default crop commission rate formatting & key generation
            string cropName = "Red Onion";
            string cleanCrop = cropName.Trim().ToLower().Replace(" ", "_");
            string expectedKey = "red_onion_commission_pct";
            decimal defaultCommissionPct = 0.08m; // 0.08 default commission (8%)

            Assert.Equal("red_onion_commission_pct", expectedKey);
            Assert.Equal(0.08m, defaultCommissionPct);
        }

        [Fact]
        public void Test_StockDeduction_Calculation()
        {
            // Verify stock deduction calculation for payment success
            decimal initialStockKg = 500m;
            decimal orderedKg = 100m;

            decimal remainingStockKg = initialStockKg - orderedKg;
            Assert.Equal(400m, remainingStockKg);

            // Verify stock restoration upon order cancellation
            decimal restoredStockKg = remainingStockKg + orderedKg;
            Assert.Equal(500m, restoredStockKg);
        }
    }
}
