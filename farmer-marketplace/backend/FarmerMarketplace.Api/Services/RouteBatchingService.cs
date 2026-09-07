// backend/FarmerMarketplace.Api/Services/RouteBatchingService.cs

using FarmerMarketplace.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    /// <summary>
    /// Hosted background service that auto-triggers route optimization at
    /// 8:00 AM and 2:00 PM IST every day.
    /// Exceptions are caught and logged — a crash in the batch MUST NOT
    /// take down the rest of the API.
    /// </summary>
    public class RouteBatchingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RouteBatchingService> _logger;

        public RouteBatchingService(
            IServiceScopeFactory scopeFactory,
            ILogger<RouteBatchingService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RouteBatchingService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nowIst = RouteService.GetNextBatchTime(ToIst(DateTime.UtcNow));
                    // GetNextBatchTime already returns the NEXT run time — wait until then
                    var waitUntilIst = nowIst;
                    var waitUntilUtc = FromIst(waitUntilIst);
                    var delay = waitUntilUtc - DateTime.UtcNow;

                    if (delay > TimeSpan.Zero)
                    {
                        _logger.LogInformation(
                            "RouteBatchingService: next run at {Time} IST (in {Delay:hh\\:mm\\:ss}).",
                            waitUntilIst, delay);
                        await Task.Delay(delay, stoppingToken);
                    }

                    if (stoppingToken.IsCancellationRequested) break;

                    _logger.LogInformation("RouteBatchingService: starting scheduled batch.");
                    await RunBatchAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Shutting down — expected
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RouteBatchingService: unexpected error in scheduler loop. Retrying in 60 s.");
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }
            }

            _logger.LogInformation("RouteBatchingService stopped.");
        }

        private async Task RunBatchAsync(CancellationToken ct)
        {
            try
            {
                // RouteService is Scoped, so we must resolve it within a scope
                using var scope = _scopeFactory.CreateScope();
                var routeService = scope.ServiceProvider.GetRequiredService<RouteService>();
                var result = await routeService.ExecuteBatchAsync(isManual: false, ct);

                _logger.LogInformation(
                    "RouteBatchingService batch complete. " +
                    "Routes={Routes}, Orders={Orders}, Skipped={Skipped}.",
                    result.RoutesCreated, result.OrdersRouted, result.SkippedOrders);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Log and continue — never crash the background service
                _logger.LogError(ex, "RouteBatchingService: batch execution threw an exception.");
            }
        }

        // ── IST helpers ──────────────────────────────────────────────────────

        private static DateTime ToIst(DateTime utc)
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, zone);
        }

        private static DateTime FromIst(DateTime ist)
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata");
            return TimeZoneInfo.ConvertTimeToUtc(ist, zone);
        }
    }
}
