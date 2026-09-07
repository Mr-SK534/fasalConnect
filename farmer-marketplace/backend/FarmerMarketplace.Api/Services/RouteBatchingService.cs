// backend/FarmerMarketplace.Api/Services/RouteBatchingService.cs

namespace FarmerMarketplace.Api.Services
{
    public class RouteBatchingService : BackgroundService
    {
        private readonly ILogger<RouteBatchingService> _logger;

        public RouteBatchingService(ILogger<RouteBatchingService> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RouteBatchingService started.");
            return Task.CompletedTask;
        }
    }
}
