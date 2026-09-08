// backend/FarmerMarketplace.Api/Services/PaymentEscrowService.cs

using System.Text.Json;
using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class PaymentEscrowService : IPaymentEscrowService
    {
        private readonly AppDbContext _context;
        private readonly IPlatformConfigService _configService;
        private readonly ILogger<PaymentEscrowService> _logger;

        public PaymentEscrowService(AppDbContext context, IPlatformConfigService configService, ILogger<PaymentEscrowService> logger)
        {
            _context = context;
            _configService = configService;
            _logger = logger;
        }

        public async Task<EscrowStatusResponseDto> InitiateEscrowAsync(Guid currentUserId, InitiateEscrowDto dto)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Include(o => o.Buyer)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            var buyerId = dto.BuyerId ?? order.BuyerId;

            // Populate metadata defaults on Order if missing
            if (string.IsNullOrWhiteSpace(order.CropName) && order.Items.Any())
            {
                order.CropName = order.Items.First().Product?.CropName ?? "Produce";
            }
            if (string.IsNullOrWhiteSpace(order.Season))
            {
                order.Season = "S1 - Rabi Glut (Jan-Apr)";
            }

            var totalOrderedKg = order.QuantityOrderedKg ?? (double)order.Items.Sum(i => i.Quantity);
            if (totalOrderedKg <= 0) totalOrderedKg = 36.0;
            order.QuantityOrderedKg = totalOrderedKg;

            var askingPrice = order.FarmerAskingPricePerKg ?? (order.Items.Any() ? order.Items.First().PriceAtOrderTime : 20.0m);
            if (askingPrice <= 0) askingPrice = 20.0m;
            order.FarmerAskingPricePerKg = askingPrice;

            if (!order.FarmerId.HasValue && order.Items.Any())
            {
                order.FarmerId = order.Items.First().FarmerId;
            }

            if (order.FarmerId.HasValue && !order.FpoAdminId.HasValue)
            {
                var farmerUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == order.FarmerId.Value);
                if (farmerUser?.FpoId.HasValue == true)
                {
                    order.FpoAdminId = farmerUser.FpoId.Value;
                }
            }

            var orderedAmountRs = dto.OverrideAmount ?? (decimal)totalOrderedKg * askingPrice;
            order.TotalAmount = orderedAmountRs;

            // Check if existing escrow record exists
            var escrow = await _context.EscrowTransactions
                .FirstOrDefaultAsync(e => e.OrderId == order.Id);

            if (escrow == null)
            {
                escrow = new EscrowTransaction
                {
                    OrderId = order.Id,
                    BuyerId = buyerId,
                    PlatformAccountId = "PLATFORM_ESCROW_WALLET_01",
                    OrderedAmountRs = orderedAmountRs,
                    ActualAmountRs = orderedAmountRs,
                    Status = EscrowStatus.Held,
                    HeldDate = DateTime.UtcNow
                };

                _context.EscrowTransactions.Add(escrow);
            }
            else
            {
                escrow.OrderedAmountRs = orderedAmountRs;
                escrow.ActualAmountRs = orderedAmountRs;
                escrow.Status = EscrowStatus.Held;
                escrow.HeldDate = DateTime.UtcNow;
            }

            // Log double-entry ledger
            var ledger = new TransactionLedger
            {
                OrderId = order.Id,
                TransactionType = LedgerTransactionType.BuyerPaymentHeld,
                FromAccount = $"buyer:{buyerId}",
                ToAccount = escrow.PlatformAccountId,
                AmountRs = orderedAmountRs,
                Status = LedgerStatus.Completed,
                Notes = $"Buyer payment held in escrow for Order {order.Id} ({totalOrderedKg} kg @ ₹{askingPrice}/kg)",
                CreatedBy = currentUserId.ToString()
            };
            _context.TransactionLedgers.Add(ledger);

            await _context.SaveChangesAsync();

            return MapToEscrowResponse(escrow);
        }

        public async Task<EscrowStatusResponseDto> GetEscrowStatusAsync(Guid orderId)
        {
            var escrow = await _context.EscrowTransactions
                .Include(e => e.Order)
                .FirstOrDefaultAsync(e => e.OrderId == orderId);

            if (escrow == null)
                throw new KeyNotFoundException($"Escrow transaction for Order {orderId} not found.");

            return MapToEscrowResponse(escrow);
        }

        public async Task<ConfirmDeliveryResultDto> ConfirmDeliveryAsync(ConfirmDeliveryDto dto)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            var escrow = await _context.EscrowTransactions
                .FirstOrDefaultAsync(e => e.OrderId == order.Id);

            if (escrow == null)
            {
                // Auto-create escrow if missing
                var initiateDto = new InitiateEscrowDto { OrderId = order.Id };
                await InitiateEscrowAsync(order.BuyerId, initiateDto);
                escrow = await _context.EscrowTransactions.FirstAsync(e => e.OrderId == order.Id);
            }

            var deliveryTimestamp = dto.Timestamp ?? DateTime.UtcNow;
            order.QuantityDeliveredKg = dto.DeliveredKg;
            order.DeliveryConfirmedDate = deliveryTimestamp;
            order.Status = OrderStatus.Delivered;

            var orderedKg = order.QuantityOrderedKg ?? (double)order.Items.Sum(i => i.Quantity);
            if (orderedKg <= 0) orderedKg = dto.DeliveredKg;
            order.QuantityOrderedKg = orderedKg;

            var askingPrice = order.FarmerAskingPricePerKg ?? 20.0m;

            // Financial Model Calculations (Dynamic from PlatformConfig)
            var commissionPct = await _configService.GetDecimalAsync("commission_pct", 0.08m);
            if (!string.IsNullOrWhiteSpace(order.CropName))
            {
                var cropKey = $"{order.CropName.Trim().ToLower()}_commission_pct";
                commissionPct = await _configService.GetDecimalAsync(cropKey, commissionPct);
            }
            var logisticsPartnerPerKg = await _configService.GetDecimalAsync("logistics_partner_payout_per_kg", 2.0m);
            var logisticsPlatformMarginPerKg = await _configService.GetDecimalAsync("logistics_platform_margin_per_kg", 0.5m);
            var gatewayPct = await _configService.GetDecimalAsync("payment_gateway_pct", 0.02m);

            var commissionPerKg = askingPrice * commissionPct;
            var logisticsPerKg = logisticsPartnerPerKg + logisticsPlatformMarginPerKg;
            var consumerPricePerKg = askingPrice + commissionPerKg + logisticsPerKg;

            var buyerSubtotal = consumerPricePerKg * (decimal)dto.DeliveredKg;
            var gatewayFee = buyerSubtotal * gatewayPct;
            var buyerFinalTotal = buyerSubtotal + gatewayFee;

            var farmerGross = askingPrice * (decimal)dto.DeliveredKg;
            var platformCommissionTotal = commissionPerKg * (decimal)dto.DeliveredKg;
            var platformLogisticsMargin = logisticsPlatformMarginPerKg * (decimal)dto.DeliveredKg;
            var platformGrossRevenue = platformCommissionTotal + platformLogisticsMargin;
            var logisticsPartnerCost = logisticsPartnerPerKg * (decimal)dto.DeliveredKg;

            escrow.ActualAmountRs = farmerGross;

            // Delivery Weight Log
            var weightLog = new DeliveryWeightLog
            {
                OrderId = order.Id,
                RouteId = order.RouteId,
                PickupDatetime = order.CreatedAt,
                WeightAtPickupKg = order.QuantityPickedUpKg ?? orderedKg,
                DeliveryDatetime = deliveryTimestamp,
                WeightAtDeliveryKg = dto.DeliveredKg,
                WeightVarianceKg = orderedKg - dto.DeliveredKg,
                VarianceReason = dto.DeliveredKg < orderedKg ? $"Short delivery: {(orderedKg - dto.DeliveredKg):F2} kg loss" : "Normal delivery"
            };
            _context.DeliveryWeightLogs.Add(weightLog);

            // Platform Fee record
            var platformFee = await _context.PlatformFees.FirstOrDefaultAsync(pf => pf.OrderId == order.Id);
            if (platformFee == null)
            {
                platformFee = new PlatformFee
                {
                    EscrowId = escrow.Id,
                    OrderId = order.Id,
                    CommissionPct = commissionPct,
                    CommissionAmountRs = platformCommissionTotal,
                    LogisticsPartnerChargePerKg = logisticsPartnerPerKg,
                    LogisticsPlatformMarginPerKg = logisticsPlatformMarginPerKg,
                    TotalLogisticsChargeRs = (logisticsPartnerPerKg + logisticsPlatformMarginPerKg) * (decimal)dto.DeliveredKg,
                    PaymentGatewayFeePct = gatewayPct,
                    PaymentGatewayFeeRs = gatewayFee,
                    Timestamp = deliveryTimestamp
                };
                _context.PlatformFees.Add(platformFee);
            }
            else
            {
                platformFee.CommissionAmountRs = platformCommissionTotal;
                platformFee.TotalLogisticsChargeRs = (logisticsPartnerPerKg + logisticsPlatformMarginPerKg) * (decimal)dto.DeliveredKg;
                platformFee.PaymentGatewayFeeRs = gatewayFee;
                platformFee.Timestamp = deliveryTimestamp;
            }

            // Ledger entry for Gateway fee
            _context.TransactionLedgers.Add(new TransactionLedger
            {
                OrderId = order.Id,
                TransactionType = LedgerTransactionType.GatewayFee,
                FromAccount = $"buyer:{order.BuyerId}",
                ToAccount = "payment_gateway",
                AmountRs = gatewayFee,
                Status = LedgerStatus.Completed,
                Notes = $"Payment Gateway fee for Order {order.Id}"
            });

            // Check variance logic
            var weightVariance = orderedKg - dto.DeliveredKg;
            if (weightVariance > 0.001)
            {
                escrow.Status = EscrowStatus.PendingConfirmation;
                escrow.DisputeReason = $"Weight variance: {weightVariance:F2} kg loss ({((weightVariance / orderedKg) * 100):F1}%)";
            }
            else
            {
                escrow.Status = EscrowStatus.Released;
                escrow.ReleaseDate = deliveryTimestamp;

                var superAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.SuperAdmin);
                var superAdminAccountStr = superAdmin != null 
                    ? $"superadmin_bank:{superAdmin.BankAccountNumber ?? superAdmin.UpiId ?? superAdmin.Email}" 
                    : "superadmin_bank:DEFAULT";

                // Log platform extra buyer markup transfer to SuperAdmin bank account
                _context.TransactionLedgers.Add(new TransactionLedger
                {
                    OrderId = order.Id,
                    TransactionType = LedgerTransactionType.PlatformCommission,
                    FromAccount = escrow.PlatformAccountId,
                    ToAccount = superAdminAccountStr,
                    AmountRs = platformGrossRevenue,
                    Status = LedgerStatus.Completed,
                    Notes = $"Extra buyer markup transferred to SuperAdmin bank account ({superAdmin?.Email ?? "superadmin"}). Farmer receives 100% of listed price (₹{askingPrice}/kg).",
                    CreatedBy = superAdmin?.Email ?? "system"
                });

                // Auto-disburse 100% farmer asking price to farmer's registered bank account
                var targetFarmerId = order.FarmerId ?? order.Items.FirstOrDefault()?.FarmerId;
                if (targetFarmerId.HasValue)
                {
                    var farmer = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetFarmerId.Value);
                    var farmerBankStr = farmer != null 
                        ? $"farmer_bank:{farmer.BankAccountNumber ?? farmer.UpiId ?? farmer.Phone}" 
                        : "farmer_bank:DEFAULT";

                    _context.TransactionLedgers.Add(new TransactionLedger
                    {
                        OrderId = order.Id,
                        TransactionType = LedgerTransactionType.PayoutToFarmer,
                        FromAccount = escrow.PlatformAccountId,
                        ToAccount = farmerBankStr,
                        AmountRs = farmerGross,
                        Status = LedgerStatus.Completed,
                        Notes = $"100% asking price payout transferred directly to farmer bank account ({farmer?.BankAccountNumber ?? farmer?.UpiId ?? "Bank Account"}).",
                        CreatedBy = "system_escrow_release"
                    });

                    var orderIdStr = order.Id.ToString();
                    var existingPayout = await _context.FarmerPayouts.FirstOrDefaultAsync(p => p.OrderIdsJson.Contains(orderIdStr));
                    if (existingPayout == null && farmer != null)
                    {
                        var payout = new FarmerPayout
                        {
                            FarmerId = farmer.Id,
                            FpoAdminId = farmer.FpoId,
                            OrderIdsJson = JsonSerializer.Serialize(new List<Guid> { order.Id }),
                            TotalAmountRs = farmerGross,
                            PayoutDate = deliveryTimestamp,
                            PaymentMethod = !string.IsNullOrWhiteSpace(farmer.UpiId) ? PayoutPaymentMethod.Upi : PayoutPaymentMethod.BankTransfer,
                            UpiIdOrBankAccount = farmer.BankAccountNumber ?? farmer.UpiId ?? "Bank Transfer",
                            Status = PayoutStatus.Completed,
                            ConfirmationTimestamp = deliveryTimestamp,
                            CreatedAt = deliveryTimestamp
                        };
                        _context.FarmerPayouts.Add(payout);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return new ConfirmDeliveryResultDto
            {
                OrderId = order.Id,
                EscrowId = escrow.Id,
                EscrowStatus = escrow.Status,
                QuantityOrderedKg = orderedKg,
                QuantityDeliveredKg = dto.DeliveredKg,
                WeightVarianceKg = weightVariance,
                OrderedAmountRs = escrow.OrderedAmountRs,
                ActualAmountRs = farmerGross,
                BuyerFinalPriceRs = buyerFinalTotal,
                FarmerReceivesRs = farmerGross,
                PlatformRevenueRs = platformGrossRevenue,
                PlatformLogisticsCostRs = logisticsPartnerCost,
                GatewayFeeRs = gatewayFee,
                DisputeReason = escrow.DisputeReason
            };
        }

        public async Task<EscrowStatusResponseDto> InitiateDisputeAsync(InitiateDisputeDto dto)
        {
            var escrow = await _context.EscrowTransactions
                .Include(e => e.Order)
                .FirstOrDefaultAsync(e => e.OrderId == dto.OrderId);

            if (escrow == null)
                throw new KeyNotFoundException("Escrow record not found for order.");

            escrow.Status = EscrowStatus.Disputed;
            escrow.DisputeReason = dto.Reason + (string.IsNullOrWhiteSpace(dto.BuyerNotes) ? "" : $" | Notes: {dto.BuyerNotes}");

            _context.TransactionLedgers.Add(new TransactionLedger
            {
                OrderId = dto.OrderId,
                TransactionType = LedgerTransactionType.DisputeResolution,
                FromAccount = $"buyer:{escrow.BuyerId}",
                ToAccount = "platform_dispute",
                AmountRs = escrow.ActualAmountRs ?? escrow.OrderedAmountRs,
                Status = LedgerStatus.Pending,
                Notes = $"Dispute initiated: {escrow.DisputeReason}"
            });

            await _context.SaveChangesAsync();
            return MapToEscrowResponse(escrow);
        }

        public async Task<EscrowStatusResponseDto> ResolveDisputeAsync(ResolveDisputeDto dto)
        {
            var escrow = await _context.EscrowTransactions
                .Include(e => e.Order)
                .FirstOrDefaultAsync(e => e.Id == dto.DisputeId || e.OrderId == dto.DisputeId);

            if (escrow == null)
                throw new KeyNotFoundException("Escrow dispute record not found.");

            var amount = escrow.ActualAmountRs ?? escrow.OrderedAmountRs;
            escrow.DisputeResolvedDate = DateTime.UtcNow;

            if (dto.AdminDecision.ToLower() == "full_refund")
            {
                escrow.Status = EscrowStatus.Refunded;
                escrow.RefundDate = DateTime.UtcNow;

                _context.TransactionLedgers.Add(new TransactionLedger
                {
                    OrderId = escrow.OrderId,
                    TransactionType = LedgerTransactionType.RefundToBuyer,
                    FromAccount = escrow.PlatformAccountId,
                    ToAccount = $"buyer:{escrow.BuyerId}",
                    AmountRs = amount,
                    Status = LedgerStatus.Completed,
                    Notes = $"Full refund issued to buyer. Reason/Notes: {dto.Notes}"
                });
            }
            else if (dto.AdminDecision.ToLower() == "partial_refund")
            {
                var refundAmount = amount * dto.RefundSplitPct;
                var farmerAmount = amount - refundAmount;

                escrow.Status = EscrowStatus.Released;
                escrow.ReleaseDate = DateTime.UtcNow;
                escrow.ActualAmountRs = farmerAmount;

                _context.TransactionLedgers.Add(new TransactionLedger
                {
                    OrderId = escrow.OrderId,
                    TransactionType = LedgerTransactionType.RefundToBuyer,
                    FromAccount = escrow.PlatformAccountId,
                    ToAccount = $"buyer:{escrow.BuyerId}",
                    AmountRs = refundAmount,
                    Status = LedgerStatus.Completed,
                    Notes = $"Partial refund ({dto.RefundSplitPct * 100}%) issued to buyer. Notes: {dto.Notes}"
                });
            }
            else
            {
                // Default: release to farmer
                escrow.Status = EscrowStatus.Released;
                escrow.ReleaseDate = DateTime.UtcNow;

                _context.TransactionLedgers.Add(new TransactionLedger
                {
                    OrderId = escrow.OrderId,
                    TransactionType = LedgerTransactionType.DisputeResolution,
                    FromAccount = "platform_dispute",
                    ToAccount = escrow.PlatformAccountId,
                    AmountRs = amount,
                    Status = LedgerStatus.Completed,
                    Notes = $"Dispute resolved in favor of farmer. Notes: {dto.Notes}"
                });
            }

            await _context.SaveChangesAsync();
            return MapToEscrowResponse(escrow);
        }

        public async Task<ProcessPayoutsResultDto> ProcessPayoutsAsync(ProcessPayoutsDto dto)
        {
            var targetDate = dto.TargetDate ?? DateTime.UtcNow;

            // Query released escrows not yet paid out
            var releasedEscrows = await _context.EscrowTransactions
                .Include(e => e.Order)
                .ThenInclude(o => o!.Farmer)
                .Where(e => e.Status == EscrowStatus.Released)
                .ToListAsync();

            // Filter out escrows already in completed payouts
            var existingPayoutOrderIds = (await _context.FarmerPayouts
                .Select(p => p.OrderIdsJson)
                .ToListAsync())
                .SelectMany(json => JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>())
                .ToHashSet();

            var eligibleEscrows = releasedEscrows
                .Where(e => !existingPayoutOrderIds.Contains(e.OrderId))
                .ToList();

            if (!eligibleEscrows.Any())
            {
                return new ProcessPayoutsResultDto
                {
                    PayoutCount = 0,
                    TotalPayoutAmountRs = 0,
                    FailedTransfers = 0,
                    Payouts = new List<FarmerPayoutHistoryDto>()
                };
            }

            // Group by FarmerId (or FpoAdminId if FPO exists)
            var grouped = eligibleEscrows
                .GroupBy(e => e.Order?.FpoAdminId.HasValue == true ? e.Order.FpoAdminId.Value : (e.Order?.FarmerId ?? Guid.Empty))
                .ToList();

            var payoutResults = new List<FarmerPayoutHistoryDto>();
            decimal globalTotalPayout = 0;

            foreach (var group in grouped)
            {
                var recipientId = group.Key;
                if (recipientId == Guid.Empty) continue;

                var ordersInGroup = group.Select(e => e.Order!).Where(o => o != null).ToList();
                var orderIds = ordersInGroup.Select(o => o.Id).ToList();

                // FPO aggregation & payout breakdown:
                // Gross farmer earnings
                decimal totalGrossAmount = group.Sum(e => e.ActualAmountRs ?? e.OrderedAmountRs);
                double totalDeliveredKg = ordersInGroup.Sum(o => o.QuantityDeliveredKg ?? o.QuantityOrderedKg ?? 0);

                // Farmer/FPO receives 100% of listed asking price (extra buyer markup goes to SuperAdmin bank)
                decimal netPayout = totalGrossAmount;

                var isFpo = ordersInGroup.Any(o => o.FpoAdminId == recipientId);
                var fpoAdminId = isFpo ? recipientId : ordersInGroup.FirstOrDefault()?.FpoAdminId;
                var farmerId = isFpo ? (ordersInGroup.FirstOrDefault()?.FarmerId ?? recipientId) : recipientId;

                var recipientUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == recipientId);

                var payout = new FarmerPayout
                {
                    FarmerId = farmerId,
                    FpoAdminId = fpoAdminId,
                    OrderIdsJson = JsonSerializer.Serialize(orderIds),
                    TotalAmountRs = netPayout,
                    PayoutDate = targetDate,
                    PaymentMethod = PayoutPaymentMethod.BankTransfer,
                    UpiIdOrBankAccount = recipientUser?.UpiId ?? recipientUser?.BankAccountNumber ?? "BANK_ACCT_998877",
                    Status = PayoutStatus.Completed,
                    ConfirmationTimestamp = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.FarmerPayouts.Add(payout);

                // Double-entry ledger
                _context.TransactionLedgers.Add(new TransactionLedger
                {
                    TransactionType = LedgerTransactionType.PayoutToFarmer,
                    FromAccount = "platform_escrow",
                    ToAccount = isFpo ? $"fpo_admin:{fpoAdminId}" : $"farmer:{farmerId}",
                    AmountRs = netPayout,
                    Status = LedgerStatus.Completed,
                    Notes = $"Batched payout for {orderIds.Count} order(s). Gross: ₹{totalGrossAmount}, Net: ₹{netPayout}",
                    CreatedBy = "system_batch_job"
                });

                globalTotalPayout += netPayout;

                payoutResults.Add(new FarmerPayoutHistoryDto
                {
                    PayoutId = payout.Id,
                    FarmerId = farmerId,
                    FarmerName = recipientUser?.Name ?? "Farmer",
                    FpoAdminId = fpoAdminId,
                    OrderIds = orderIds,
                    TotalAmountRs = netPayout,
                    PayoutDate = targetDate,
                    PaymentMethod = payout.PaymentMethod,
                    UpiIdOrBankAccount = payout.UpiIdOrBankAccount,
                    Status = payout.Status,
                    ConfirmationTimestamp = payout.ConfirmationTimestamp,
                    CreatedAt = payout.CreatedAt
                });
            }

            await _context.SaveChangesAsync();

            return new ProcessPayoutsResultDto
            {
                PayoutCount = payoutResults.Count,
                TotalPayoutAmountRs = globalTotalPayout,
                FailedTransfers = 0,
                Payouts = payoutResults
            };
        }

        public async Task<List<FarmerPayoutHistoryDto>> GetFarmerPayoutHistoryAsync(Guid farmerId)
        {
            var payouts = await _context.FarmerPayouts
                .Include(p => p.Farmer)
                .Include(p => p.FpoAdmin)
                .Where(p => p.FarmerId == farmerId || p.FpoAdminId == farmerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payouts.Select(p => new FarmerPayoutHistoryDto
            {
                PayoutId = p.Id,
                FarmerId = p.FarmerId,
                FarmerName = p.Farmer?.Name ?? "Farmer",
                FpoAdminId = p.FpoAdminId,
                FpoAdminName = p.FpoAdmin?.Name,
                OrderIds = JsonSerializer.Deserialize<List<Guid>>(p.OrderIdsJson) ?? new List<Guid>(),
                TotalAmountRs = p.TotalAmountRs,
                PayoutDate = p.PayoutDate,
                PaymentMethod = p.PaymentMethod,
                UpiIdOrBankAccount = p.UpiIdOrBankAccount,
                Status = p.Status,
                ConfirmationTimestamp = p.ConfirmationTimestamp,
                CreatedAt = p.CreatedAt
            }).ToList();
        }

        public async Task<FarmerEarningsDto> GetFarmerEarningsAsync(Guid farmerId)
        {
            var farmer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == farmerId);
            if (farmer == null)
                throw new KeyNotFoundException("Farmer account not found.");

            var orderItems = await _context.OrderItems.AsNoTracking()
                .Include(i => i.Order).ThenInclude(o => o.Buyer)
                .Include(i => i.Product)
                .Where(i => i.FarmerId == farmerId)
                .ToListAsync();

            var escrows = await _context.EscrowTransactions.AsNoTracking().ToListAsync();
            var escrowMap = escrows.ToDictionary(e => e.OrderId);

            decimal totalEarnings = 0;
            decimal heldInEscrow = 0;
            decimal disbursedPayouts = 0;
            int deliveredOrdersCount = 0;

            var ledger = new List<FarmerEarningsOrderItemDto>();

            var groupedOrders = orderItems.GroupBy(i => i.OrderId);

            foreach (var group in groupedOrders)
            {
                var firstItem = group.First();
                var order = firstItem.Order;
                if (order == null) continue;

                var deliveredKg = order.QuantityDeliveredKg ?? order.QuantityOrderedKg ?? (double)group.Sum(i => i.Quantity);
                var farmerAskingPrice = order.FarmerAskingPricePerKg ?? (firstItem.Product?.Price ?? 20.0m);
                var farmerEarnings = farmerAskingPrice * (decimal)deliveredKg;

                escrowMap.TryGetValue(order.Id, out var escrow);
                var isReleased = escrow?.Status == EscrowStatus.Released;
                var isDelivered = order.Status == OrderStatus.Delivered || isReleased;
                var escrowStatusStr = isReleased ? "Disbursed" : (order.Status == OrderStatus.Delivered ? "Disbursed" : (escrow?.Status.ToString() ?? "Held in Escrow"));

                totalEarnings += farmerEarnings;

                if (isDelivered)
                {
                    deliveredOrdersCount++;
                    disbursedPayouts += farmerEarnings;
                }
                else
                {
                    heldInEscrow += farmerEarnings;
                }

                ledger.Add(new FarmerEarningsOrderItemDto
                {
                    OrderItemId = firstItem.Id,
                    OrderId = order.Id,
                    OrderNumber = order.Id.ToString()[..8].ToUpper(),
                    CropName = order.CropName ?? firstItem.Product?.CropName ?? "Produce",
                    FarmerName = farmer.Name,
                    BuyerName = order.Buyer?.Name ?? "Buyer",
                    QuantityKg = deliveredKg,
                    ListedPricePerKg = farmerAskingPrice,
                    TotalFarmerEarningsRs = farmerEarnings,
                    Status = order.Status.ToString(),
                    EscrowStatus = escrowStatusStr,
                    CreatedAt = order.CreatedAt,
                    DeliveryConfirmedDate = order.DeliveryConfirmedDate
                });
            }

            return new FarmerEarningsDto
            {
                FarmerId = farmer.Id,
                FarmerName = farmer.Name,
                Email = farmer.Email ?? string.Empty,
                Phone = farmer.Phone,
                TotalEarningsRs = totalEarnings,
                HeldInEscrowRs = heldInEscrow,
                DisbursedBankPayoutsRs = disbursedPayouts,
                DeliveredOrdersCount = deliveredOrdersCount,
                TotalOrdersCount = groupedOrders.Count(),
                BankAccountNumber = farmer.BankAccountNumber,
                BankIfsc = farmer.BankIfsc,
                AccountHolderName = farmer.AccountHolderName ?? farmer.Name,
                UpiId = farmer.UpiId,
                OrderLedger = ledger.OrderByDescending(l => l.CreatedAt).ToList()
            };
        }

        private static EscrowStatusResponseDto MapToEscrowResponse(EscrowTransaction escrow)
        {
            return new EscrowStatusResponseDto
            {
                EscrowId = escrow.Id,
                OrderId = escrow.OrderId,
                BuyerId = escrow.BuyerId,
                AmountHeld = escrow.OrderedAmountRs,
                AmountActual = escrow.ActualAmountRs,
                Status = escrow.Status,
                HeldDate = escrow.HeldDate,
                ReleaseDate = escrow.ReleaseDate,
                RefundDate = escrow.RefundDate,
                DisputeReason = escrow.DisputeReason,
                DisputeResolvedDate = escrow.DisputeResolvedDate
            };
        }
    }
}
