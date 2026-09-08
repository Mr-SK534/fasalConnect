// backend/FarmerMarketplace.Api/Interfaces/IReportingService.cs

using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Models;

namespace FarmerMarketplace.Api.Interfaces
{
    public interface IReportingService
    {
        Task<List<TransactionLedger>> GetTransactionLedgerAsync(TransactionLedgerFilterDto filter);
        Task<PlatformPlnlDto> GetPlatformPlnlAsync(DateTime? startDate, DateTime? endDate, decimal? annualFixedCosts);
        Task<List<FarmerEarningsBreakdownDto>> GetFarmerEarningsBreakdownAsync(Guid farmerId, DateTime? startDate, DateTime? endDate);
    }
}
