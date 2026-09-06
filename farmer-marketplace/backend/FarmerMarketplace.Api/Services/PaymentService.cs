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

        public PaymentService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<CreatePaymentOrderResponseDto> CreateOrderAsync(Guid buyerId, CreatePaymentOrderDto dto)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            if (order.BuyerId != buyerId)
                throw new UnauthorizedAccessException("You can only pay for your own orders.");

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException("This order is not awaiting payment.");

            // Always use the server-recorded total — never trust dto.Amount directly.
            var amount = order.TotalAmount;
            var amountInPaise = (int)(amount * 100);

            var keyId = _config["Razorpay:KeyId"];
            var keySecret = _config["Razorpay:KeySecret"];
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

        public async Task HandleWebhookAsync(string rawBody, string? signatureHeader)
        {
            var webhookSecret = _config["Razorpay:WebhookSecret"];

            if (string.IsNullOrEmpty(signatureHeader) || !VerifySignature(rawBody, signatureHeader, webhookSecret!))
                throw new UnauthorizedAccessException("Invalid webhook signature.");

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
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == PaymentStatus.Paid);

            if (payment == null)
                throw new InvalidOperationException("No completed payment found for this order.");

            var alreadySplit = await _context.PaymentSplits.AnyAsync(s => s.PaymentId == payment.Id);
            if (alreadySplit)
                throw new InvalidOperationException("This payment has already been split.");

            var itemsByFarmer = await _context.OrderItems
                .Where(i => i.OrderId == orderId)
                .GroupBy(i => i.FarmerId)
                .Select(g => new { FarmerId = g.Key, Amount = g.Sum(i => i.SubTotal) })
                .ToListAsync();

            var results = new List<PaymentSplitResultDto>();

            foreach (var group in itemsByFarmer)
            {
                var farmer = await _context.Users.FirstOrDefaultAsync(u => u.Id == group.FarmerId);

                // TODO: actual Razorpay Route transfer requires the farmer to have a
                // linked Razorpay account (razorpay_account_id) from onboarding/KYC —
                // not yet captured on User. For now, record the split as Pending so
                // the money owed is tracked; wire up the real transfer call once
                // farmer bank details are verified through Razorpay's onboarding flow.
                var split = new PaymentSplit
                {
                    PaymentId = payment.Id,
                    FarmerId = group.FarmerId,
                    Amount = group.Amount,
                    TransferStatus = TransferStatus.Pending
                };

                _context.PaymentSplits.Add(split);

                results.Add(new PaymentSplitResultDto
                {
                    FarmerId = group.FarmerId,
                    FarmerName = farmer?.Name ?? string.Empty,
                    Amount = group.Amount,
                    TransferStatus = TransferStatus.Pending,
                    RazorpayTransferId = null
                });
            }

            await _context.SaveChangesAsync();

            return results;
        }

        private static bool VerifySignature(string payload, string signature, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            return computedSignature == signature.ToLowerInvariant();
        }
    }
}