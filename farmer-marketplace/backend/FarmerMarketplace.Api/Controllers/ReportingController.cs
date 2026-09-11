// backend/FarmerMarketplace.Api/Controllers/ReportingController.cs

using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/reporting")]
    [Authorize]
    public class ReportingController : ControllerBase
    {
        private readonly IReportingService _reportingService;
        private readonly ILogger<ReportingController> _logger;

        public ReportingController(IReportingService reportingService, ILogger<ReportingController> logger)
        {
            _reportingService = reportingService;
            _logger = logger;
        }

        // GET /api/reporting/transaction-ledger
        [HttpGet("transaction-ledger")]
        [Authorize(Roles = "PlatformAdmin,FpoAdmin,SuperAdmin,Admin,Manager")]
        public async Task<ActionResult<List<TransactionLedger>>> GetTransactionLedger(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] LedgerTransactionType? transactionType,
            [FromQuery] Guid? orderId,
            [FromQuery] Guid? farmerId)
        {
            var filter = new TransactionLedgerFilterDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TransactionType = transactionType,
                OrderId = orderId,
                FarmerId = farmerId
            };

            var result = await _reportingService.GetTransactionLedgerAsync(filter);
            return Ok(result);
        }

        // GET /api/reporting/platform-plnl
        [HttpGet("platform-plnl")]
        [Authorize(Roles = "PlatformAdmin,FpoAdmin,SuperAdmin,Admin,Manager")]
        public async Task<ActionResult<PlatformPlnlDto>> GetPlatformPlnl(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] decimal? annualFixedCosts)
        {
            var result = await _reportingService.GetPlatformPlnlAsync(startDate, endDate, annualFixedCosts);
            return Ok(result);
        }

        // GET /api/reporting/farmer-earnings-breakdown/{farmerId}
        [HttpGet("farmer-earnings-breakdown/{farmerId}")]
        public async Task<ActionResult<List<FarmerEarningsBreakdownDto>>> GetFarmerEarningsBreakdown(
            Guid farmerId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var result = await _reportingService.GetFarmerEarningsBreakdownAsync(farmerId, startDate, endDate);
            return Ok(result);
        }
    }
}
