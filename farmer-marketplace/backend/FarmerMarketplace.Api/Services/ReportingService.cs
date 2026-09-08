// backend/FarmerMarketplace.Api/Services/ReportingService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class ReportingService : IReportingService
    {
        private readonly AppDbContext _context;
        private readonly IPlatformConfigService _configService;
        private readonly ILogger<ReportingService> _logger;

        public ReportingService(AppDbContext context, IPlatformConfigService configService, ILogger<ReportingService> logger)
        {
            _context = context;
            _configService = configService;
            _logger = logger;
        }

        public async Task<List<TransactionLedger>> GetTransactionLedgerAsync(TransactionLedgerFilterDto filter)
        {
            var query = _context.TransactionLedgers
                .Include(tl => tl.Order)
                .AsQueryable();

            if (filter.StartDate.HasValue)
            {
                query = query.Where(tl => tl.Timestamp >= filter.StartDate.Value);
            }
            if (filter.EndDate.HasValue)
            {
                query = query.Where(tl => tl.Timestamp <= filter.EndDate.Value);
            }
            if (filter.TransactionType.HasValue)
            {
                query = query.Where(tl => tl.TransactionType == filter.TransactionType.Value);
            }
            if (filter.OrderId.HasValue)
            {
                query = query.Where(tl => tl.OrderId == filter.OrderId.Value);
            }
            if (filter.FarmerId.HasValue)
            {
                var farmerStr = filter.FarmerId.Value.ToString();
                query = query.Where(tl => tl.FromAccount.Contains(farmerStr) || tl.ToAccount.Contains(farmerStr));
            }

            return await query.OrderByDescending(tl => tl.Timestamp).ToListAsync();
        }

        public async Task<PlatformPlnlDto> GetPlatformPlnlAsync(DateTime? startDate, DateTime? endDate, decimal? annualFixedCosts)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var defaultFixed = await _configService.GetDecimalAsync("annual_fixed_costs_rs", 90000.0m);
            var fixedAnnual = annualFixedCosts ?? defaultFixed;

            var days = (end - start).TotalDays;
            if (days <= 0) days = 1;

            var platformFees = await _context.PlatformFees
                .Include(pf => pf.Order)
                .Where(pf => pf.Timestamp >= start && pf.Timestamp <= end)
                .ToListAsync();

            decimal totalCommission = platformFees.Sum(pf => pf.CommissionAmountRs);
            decimal totalLogisticsMargin = platformFees.Sum(pf => pf.LogisticsPlatformMarginPerKg * (decimal)(pf.Order?.QuantityDeliveredKg ?? pf.Order?.QuantityOrderedKg ?? 36.0));
            decimal totalGatewayFees = platformFees.Sum(pf => pf.PaymentGatewayFeeRs);
            decimal totalLogisticsPartnerPayouts = platformFees.Sum(pf => pf.LogisticsPartnerChargePerKg * (decimal)(pf.Order?.QuantityDeliveredKg ?? pf.Order?.QuantityOrderedKg ?? 36.0));

            // Calculate active farmers for subscription revenue (e.g. ₹500 / farmer / year)
            int activeFarmersCount = await _context.Users.CountAsync(u => u.Role == UserRole.Farmer);
            decimal subscriptionPerFarmerYear = await _configService.GetDecimalAsync("subscription_fee_per_farmer_per_year", 500.0m);
            decimal proratedSubscription = (activeFarmersCount * subscriptionPerFarmerYear / 365m) * (decimal)days;

            decimal totalRevenue = totalCommission + totalLogisticsMargin + proratedSubscription;
            decimal proratedFixedCost = (fixedAnnual / 365m) * (decimal)days;
            decimal totalCosts = totalGatewayFees + proratedFixedCost + totalLogisticsPartnerPayouts;

            decimal netProfit = totalRevenue - totalCosts;
            decimal netMarginPct = totalRevenue > 0 ? (netProfit / totalRevenue) * 100m : 0m;

            var defaultCommissionPct = await _configService.GetDecimalAsync("commission_pct", 0.08m);
            var defaultLogisticsPartner = await _configService.GetDecimalAsync("logistics_partner_payout_per_kg", 2.0m);
            var defaultLogisticsMargin = await _configService.GetDecimalAsync("logistics_platform_margin_per_kg", 0.5m);

            // Total consumer spend calculate
            decimal totalConsumerSpend = platformFees.Sum(pf => {
                var delKg = (decimal)(pf.Order?.QuantityDeliveredKg ?? pf.Order?.QuantityOrderedKg ?? 36.0);
                var askingPrice = pf.Order?.FarmerAskingPricePerKg ?? 20.0m;
                var consumerPricePerKg = askingPrice + (askingPrice * defaultCommissionPct) + (defaultLogisticsPartner + defaultLogisticsMargin);
                return (consumerPricePerKg * delKg) + pf.PaymentGatewayFeeRs;
            });

            decimal platformTakePct = totalConsumerSpend > 0 ? (totalRevenue / totalConsumerSpend) * 100m : 0m;

            return new PlatformPlnlDto
            {
                StartDate = start,
                EndDate = end,
                CommissionRevenueRs = Math.Round(totalCommission, 2),
                LogisticsMarginRs = Math.Round(totalLogisticsMargin, 2),
                SubscriptionRevenueRs = Math.Round(proratedSubscription, 2),
                TotalRevenueRs = Math.Round(totalRevenue, 2),
                GatewayFeesRs = Math.Round(totalGatewayFees, 2),
                LogisticsPayoutsRs = Math.Round(totalLogisticsPartnerPayouts, 2),
                FixedCostsRs = Math.Round(proratedFixedCost, 2),
                TotalCostsRs = Math.Round(totalCosts, 2),
                NetProfitRs = Math.Round(netProfit, 2),
                NetMarginPct = Math.Round(netMarginPct, 2),
                PlatformTakeOfConsumerSpendPct = Math.Round(platformTakePct, 2)
            };
        }

        public async Task<List<FarmerEarningsBreakdownDto>> GetFarmerEarningsBreakdownAsync(Guid farmerId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.FarmerId == farmerId || o.Items.Any(i => i.FarmerId == farmerId));

            if (startDate.HasValue) query = query.Where(o => o.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(o => o.CreatedAt <= endDate.Value);

            var orders = await query.ToListAsync();
            var farmer = await _context.Users.FirstOrDefaultAsync(u => u.Id == farmerId);

            var grouped = orders.GroupBy(o => new {
                Crop = string.IsNullOrWhiteSpace(o.CropName) ? (o.Items.FirstOrDefault()?.Product?.CropName ?? "Produce") : o.CropName,
                Season = string.IsNullOrWhiteSpace(o.Season) ? "S1 - Rabi Glut (Jan-Apr)" : o.Season
            });

            var result = new List<FarmerEarningsBreakdownDto>();

            foreach (var g in grouped)
            {
                double orderedKg = g.Sum(o => o.QuantityOrderedKg ?? (double)o.Items.Sum(i => i.Quantity));
                double deliveredKg = g.Sum(o => o.QuantityDeliveredKg ?? o.QuantityOrderedKg ?? (double)o.Items.Sum(i => i.Quantity));
                decimal askingPrice = g.FirstOrDefault()?.FarmerAskingPricePerKg ?? (g.FirstOrDefault()?.Items.FirstOrDefault()?.PriceAtOrderTime ?? 20.0m);

                decimal gross = (decimal)deliveredKg * askingPrice;
                decimal commission = gross * 0.08m;
                decimal logistics = (decimal)deliveredKg * 2.5m;
                decimal net = gross - commission - logistics;
                if (net < 0) net = gross;

                result.Add(new FarmerEarningsBreakdownDto
                {
                    FarmerId = farmerId,
                    FarmerName = farmer?.Name ?? "Farmer",
                    CropName = g.Key.Crop,
                    Season = g.Key.Season,
                    QuantityOrderedKg = orderedKg,
                    QuantityDeliveredKg = deliveredKg,
                    AskingPricePerKg = askingPrice,
                    GrossEarningsRs = Math.Round(gross, 2),
                    CommissionDeductedRs = Math.Round(commission, 2),
                    LogisticsDeductedRs = Math.Round(logistics, 2),
                    NetPayoutRs = Math.Round(net, 2),
                    OrderCount = g.Count()
                });
            }

            return result;
        }
    }
}
