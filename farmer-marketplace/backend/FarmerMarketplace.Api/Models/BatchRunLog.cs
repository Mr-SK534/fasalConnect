// backend/FarmerMarketplace.Api/Models/BatchRunLog.cs

using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.Models
{
    /// <summary>
    /// Persists metadata about each batch routing run so the admin dashboard
    /// can show "last batch" and "next batch" information.
    /// </summary>
    public class BatchRunLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>When the batch run was triggered (UTC).</summary>
        public DateTime RanAt { get; set; } = DateTime.UtcNow;

        /// <summary>Whether this was triggered automatically (scheduled) or manually.</summary>
        public bool IsManual { get; set; } = false;

        /// <summary>How many orders were successfully routed in this run.</summary>
        public int OrdersRouted { get; set; }

        /// <summary>How many Route objects were created.</summary>
        public int RoutesCreated { get; set; }

        /// <summary>How many orders were skipped (missing coords, etc.).</summary>
        public int SkippedOrders { get; set; }
    }
}
