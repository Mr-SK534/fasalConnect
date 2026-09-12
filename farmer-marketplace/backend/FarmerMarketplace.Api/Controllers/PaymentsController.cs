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
        private readonly IPaymentEscrowService _escrowService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IPaymentEscrowService escrowService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _escrowService = escrowService;
            _logger = logger;
        }

        // POST /api/payments/create-order
        [HttpPost("create-order")]
        [Authorize]
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

        // POST /api/payments/confirm
        [HttpPost("confirm")]
        [Authorize]
        public async Task<ActionResult<OrderResponseDto>> ConfirmPayment([FromBody] ConfirmPaymentDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _paymentService.ConfirmPaymentAsync(userId.Value, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { statusCode = 404, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { statusCode = 403, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment confirmation failed: {Message}", ex.Message);
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        // POST /api/payments/fail
        [HttpPost("fail")]
        [Authorize]
        public async Task<ActionResult<OrderResponseDto>> FailPayment([FromBody] FailPaymentDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _paymentService.FailPaymentAsync(userId.Value, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { statusCode = 404, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { statusCode = 403, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment failure sync failed: {Message}", ex.Message);
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        // POST /api/payments/webhook
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();

            try
            {
                await _paymentService.HandleWebhookAsync(rawBody, signature);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Razorpay webhook rejected: {Message}", ex.Message);
                return Ok();
            }

            return Ok();
        }

        // POST /api/payments/split
        [HttpPost("split")]
        [Authorize(Roles = "PlatformAdmin,SuperAdmin,Admin")]
        public async Task<ActionResult<List<PaymentSplitResultDto>>> Split([FromBody] PaymentSplitDto dto)
        {
            var result = await _paymentService.SplitAsync(dto.OrderId);
            return Ok(result);
        }

        // --- NEW ESCROW & FINANCIAL MODEL ENDPOINTS ---

        // POST /api/payments/initiate-escrow
        [HttpPost("initiate-escrow")]
        [Authorize]
        public async Task<ActionResult<EscrowStatusResponseDto>> InitiateEscrow([FromBody] InitiateEscrowDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _escrowService.InitiateEscrowAsync(userId.Value, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { statusCode = 404, message = ex.Message });
            }
        }

        // GET /api/payments/escrow-status/{orderId}
        [HttpGet("escrow-status/{orderId}")]
        [Authorize]
        public async Task<ActionResult<EscrowStatusResponseDto>> GetEscrowStatus(Guid orderId)
        {
            try
            {
                var result = await _escrowService.GetEscrowStatusAsync(orderId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { statusCode = 404, message = ex.Message });
            }
        }

        // POST /api/payments/confirm-delivery
        [HttpPost("confirm-delivery")]
        [Authorize]
        public async Task<ActionResult<ConfirmDeliveryResultDto>> ConfirmDelivery([FromBody] ConfirmDeliveryDto dto)
        {
            try
            {
                var result = await _escrowService.ConfirmDeliveryAsync(dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { statusCode = 404, message = ex.Message });
            }
        }

        // POST /api/payments/initiate-dispute
        [HttpPost("initiate-dispute")]
        [Authorize]
        public async Task<ActionResult<EscrowStatusResponseDto>> InitiateDispute([FromBody] InitiateDisputeDto dto)
        {
            try
            {
                var result = await _escrowService.InitiateDisputeAsync(dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { statusCode = 404, message = ex.Message });
            }
        }

        // POST /api/payments/resolve-dispute
        [HttpPost("resolve-dispute")]
        [Authorize(Roles = "PlatformAdmin,FpoAdmin,SuperAdmin,Admin")]
        public async Task<ActionResult<EscrowStatusResponseDto>> ResolveDispute([FromBody] ResolveDisputeDto dto)
        {
            try
            {
                var result = await _escrowService.ResolveDisputeAsync(dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { statusCode = 404, message = ex.Message });
            }
        }

        // GET /api/payments/farmer-payout-history/{farmerId}
        [HttpGet("farmer-payout-history/{farmerId}")]
        [Authorize]
        public async Task<ActionResult<List<FarmerPayoutHistoryDto>>> GetFarmerPayoutHistory(Guid farmerId)
        {
            var result = await _escrowService.GetFarmerPayoutHistoryAsync(farmerId);
            return Ok(result);
        }

        // GET /api/payments/farmer-earnings/{farmerId}
        [HttpGet("farmer-earnings/{farmerId}")]
        [Authorize]
        public async Task<ActionResult<FarmerEarningsDto>> GetFarmerEarnings(Guid farmerId)
        {
            var result = await _escrowService.GetFarmerEarningsAsync(farmerId);
            return Ok(result);
        }

        // POST /api/payments/process-payouts
        [HttpPost("process-payouts")]
        [Authorize(Roles = "PlatformAdmin,FpoAdmin,SuperAdmin,Admin")]
        public async Task<ActionResult<ProcessPayoutsResultDto>> ProcessPayouts([FromBody] ProcessPayoutsDto dto)
        {
            var result = await _escrowService.ProcessPayoutsAsync(dto);
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