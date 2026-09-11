// backend/FarmerMarketplace.Api/DTOs/BatchStatusDto.cs

namespace FarmerMarketplace.Api.DTOs
{
    /// <summary>Response for GET /routes/batch-status</summary>
    public class BatchStatusDto
    {
        /// <summary>Next scheduled batch run time (IST, returned as UTC).</summary>
        public DateTime NextBatchTime { get; set; }

        /// <summary>When the most recent batch was run. Null if never run.</summary>
        public DateTime? LastBatchTime { get; set; }

        /// <summary>Orders routed in the last batch run.</summary>
        public int LastBatchOrderCount { get; set; }

        /// <summary>Routes created in the last batch run.</summary>
        public int LastBatchRouteCount { get; set; }

        /// <summary>Confirmed Delivery orders currently waiting (no RouteStop yet).</summary>
        public int UnroutedConfirmedOrderCount { get; set; }
    }

    /// <summary>Response for POST /routes/batch/run</summary>
    public class BatchRunResultDto
    {
        public int RoutesCreated { get; set; }
        public int OrdersRouted { get; set; }
        public int SkippedOrders { get; set; }
        public List<string> SkippedReasons { get; set; } = new();
    }
}
