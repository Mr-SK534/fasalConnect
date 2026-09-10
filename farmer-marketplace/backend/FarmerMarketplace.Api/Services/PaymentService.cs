// backend/FarmerMarketplace.Api/Services/PaymentService.cs
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Models;
using FarmerMarketplace.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;

namespace FarmerMarketplace.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(AppDbContext context, IConfiguration config, ILogger<PaymentService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        public async Task<CreatePaymentOrderResponseDto> CreateOrderAsync(Guid buyerId, CreatePaymentOrderDto dto)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                throw new KeyNotFoundException("Order not found");

            if (order.BuyerId != buyerId)
                throw new UnauthorizedAccessException("This order does not belong to you");

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException("Order is not in Pending status");

            // Always use the server-recorded total — never trust dto.Amount directly.
            var amount = order.TotalAmount;
            var amountInPaise = (int)(amount * 100);

            var keyId = _config["Razorpay:KeyId"] ?? _config["RAZORPAY_KEY_ID"] ?? Environment.GetEnvironmentVariable("RAZORPAY_KEY_ID") ?? Environment.GetEnvironmentVariable("Razorpay__KeyId");
            var keySecret = _config["Razorpay:KeySecret"] ?? _config["RAZORPAY_KEY_SECRET"] ?? Environment.GetEnvironmentVariable("RAZORPAY_KEY_SECRET") ?? Environment.GetEnvironmentVariable("Razorpay__KeySecret");

            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
            {
                _logger.LogError("Razorpay configuration is missing. KeyId configured: {HasKeyId}, KeySecret configured: {HasKeySecret}", !string.IsNullOrWhiteSpace(keyId), !string.IsNullOrWhiteSpace(keySecret));
                throw new InvalidOperationException("Razorpay credentials (KeyId and KeySecret) are missing. Please set Razorpay__KeyId and Razorpay__KeySecret in Render Environment settings.");
            }

            try
            {
                var client = new RazorpayClient(keyId, keySecret);
                var options = new Dictionary<string, object>
                {
                    { "amount", amountInPaise },
                    { "currency", "INR" },
                    { "receipt", order.Id.ToString() }
                };

                Razorpay.Api.Order rzpOrder = client.Order.Create(options);
                var razorpayOrderId = rzpOrder["id"].ToString()!;

                var payment = new FarmerMarketplace.Api.Models.Payment
                {
                    OrderId = order.Id,
                    RazorpayOrderId = razorpayOrderId,
                    Amount = amount,
                    Currency = "INR",
                    Status = PaymentStatus.Created
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return new CreatePaymentOrderResponseDto
                {
                    RazorpayOrderId = razorpayOrderId,
                    Amount = amount,
                    Currency = "INR"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Razorpay order creation failed for internal order {OrderId}: {Message}", order.Id, ex.Message);
                throw new InvalidOperationException($"Razorpay order creation failed: {ex.Message}", ex);
            }
        }

        public async Task HandleWebhookAsync(string rawBody, string? signatureHeader)
        {
            var webhookSecret = _config["Razorpay:WebhookSecret"];

            if (string.IsNullOrEmpty(webhookSecret))
            {
                _logger.LogWarning("Razorpay WebhookSecret is not set in configuration. Skipping signature verification in development.");
            }
            else if (string.IsNullOrEmpty(signatureHeader) || !VerifySignature(rawBody, signatureHeader, webhookSecret))
            {
                throw new UnauthorizedAccessException("Invalid webhook signature.");
            }

            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;

            var eventType = root.GetProperty("event").GetString();

            if (eventType != "payment.captured" && eventType != "payment.failed")
                return; // ignore events we don't act on

            var paymentEntity = root
                .GetProperty("payload")
                .GetProperty("payment")
                .GetProperty("entity");

            var razorpayOrderId = paymentEntity.GetProperty("order_id").GetString();
            var razorpayPaymentId = paymentEntity.GetProperty("id").GetString();

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.RazorpayOrderId == razorpayOrderId);

            if (payment == null) return; // unknown order — nothing to update

            if (eventType == "payment.captured")
            {
                payment.Status = PaymentStatus.Paid;
                payment.RazorpayPaymentId = razorpayPaymentId;

                if (payment.Order != null)
                    payment.Order.Status = OrderStatus.Confirmed;

                // TODO: trigger WhatsApp "payment confirmed" notification once
                // WhatsAppService exists (per contract's NotificationService triggers)
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
            }

            payment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<List<PaymentSplitResultDto>> SplitAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException($"Order with ID '{orderId}' not found.");

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment == null)
            {
                payment = new Models.Payment
                {
                    OrderId = orderId,
                    Amount = order.TotalAmount,
                    Currency = "INR",
                    Status = PaymentStatus.Paid,
                    RazorpayOrderId = $"order_escrow_{order.Id.ToString().Substring(0, 8)}",
                    RazorpayPaymentId = $"pay_escrow_{Guid.NewGuid().ToString().Substring(0, 8)}"
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
            }
            else if (payment.Status != PaymentStatus.Paid)
            {
                payment.Status = PaymentStatus.Paid;
                await _context.SaveChangesAsync();
            }

            var orderItems = await _context.OrderItems
                .Include(i => i.Product)
                .Where(i => i.OrderId == orderId)
                .ToListAsync();

            var defaultFarmerAskingPrice = order.FarmerAskingPricePerKg ?? 20.0m;

            var farmerGroups = orderItems
                .GroupBy(i => i.FarmerId)
                .Select(g => new
                {
                    FarmerId = g.Key,
                    FarmerAskingAmount = g.Sum(i => (decimal)i.Quantity * (i.Product != null && i.Product.Price > 0 ? i.Product.Price : defaultFarmerAskingPrice))
                })
                .ToList();

            var existingSplits = await _context.PaymentSplits
                .Include(s => s.Farmer)
                .Where(s => s.PaymentId == payment.Id)
                .ToListAsync();

            if (existingSplits.Any())
            {
                // Update existing splits to strictly reflect the farmer's asked price * quantity
                foreach (var split in existingSplits)
                {
                    var group = farmerGroups.FirstOrDefault(g => g.FarmerId == split.FarmerId);
                    if (group != null)
                    {
                        split.Amount = group.FarmerAskingAmount;
                    }
                }
                await _context.SaveChangesAsync();

                return existingSplits.Select(s => new PaymentSplitResultDto
                {
                    FarmerId = s.FarmerId,
                    FarmerName = s.Farmer?.Name ?? "Farmer",
                    Amount = s.Amount,
                    TransferStatus = s.TransferStatus,
                    RazorpayTransferId = s.RazorpayTransferId ?? $"trf_{Guid.NewGuid().ToString().Substring(0, 8)}"
                }).ToList();
            }

            var results = new List<PaymentSplitResultDto>();

            foreach (var group in farmerGroups)
            {
                var farmer = await _context.Users.FirstOrDefaultAsync(u => u.Id == group.FarmerId);

                var transferId = $"trf_{Guid.NewGuid().ToString().Substring(0, 8)}";
                var split = new PaymentSplit
                {
                    PaymentId = payment.Id,
                    FarmerId = group.FarmerId,
                    Amount = group.FarmerAskingAmount,
                    TransferStatus = TransferStatus.Completed,
                    RazorpayTransferId = transferId
                };

                _context.PaymentSplits.Add(split);

                results.Add(new PaymentSplitResultDto
                {
                    FarmerId = group.FarmerId,
                    FarmerName = farmer?.Name ?? "Farmer",
                    Amount = group.FarmerAskingAmount,
                    TransferStatus = TransferStatus.Completed,
                    RazorpayTransferId = transferId
                });
            }

            await _context.SaveChangesAsync();
            return results;
        }

        private static bool VerifySignature(string payload, string? signature, string? secret)
        {
            if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret)) return false;
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            return computedSignature == signature.ToLowerInvariant();
        }
    }
}