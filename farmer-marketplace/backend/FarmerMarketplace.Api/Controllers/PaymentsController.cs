// backend/FarmerMarketplace.Api/Controllers/PaymentsController.cs

using System.Security.Claims;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // POST /api/payments/create-order
        [HttpPost("create-order")]
        [Authorize(Roles = "Buyer")]
        public async Task<ActionResult<CreatePaymentOrderResponseDto>> CreateOrder([FromBody] CreatePaymentOrderDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _paymentService.CreateOrderAsync(userId.Value, dto);
                return StatusCode(201, result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Payment order creation failed: {Message}", ex.Message);
                return NotFound(new { statusCode = 404, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Payment authorization failed: {Message}", ex.Message);
                return StatusCode(403, new { statusCode = 403, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Payment order conflict: {Message}", ex.Message);
                return Conflict(new { statusCode = 409, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment order creation failed unexpectedly: {Message}", ex.Message);
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        // POST /api/payments/webhook
        // Public — Razorpay calls this server-to-server. Signature verification
        // inside the service is what actually authenticates the request.
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();

            // Must return 200 immediately per contract — do NOT let this throw
            // synchronously in a way that delays Razorpay's retry logic.
            try
            {
                await _paymentService.HandleWebhookAsync(rawBody, signature);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Razorpay webhook rejected: {Message}", ex.Message);
                // Invalid signature — still return 200 so Razorpay doesn't retry
                // a request that will never validate, but log server-side.
                return Ok();
            }

            return Ok();
        }

        // POST /api/payments/split
        [HttpPost("split")]
        [Authorize(Roles = "PlatformAdmin")]
        public async Task<ActionResult<List<PaymentSplitResultDto>>> Split([FromBody] PaymentSplitDto dto)
        {
            var result = await _paymentService.SplitAsync(dto.OrderId);
            return Ok(result);
        }

        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirstValue("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return null;

            return userId;
        }
    }
}