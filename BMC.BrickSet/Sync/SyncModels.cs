using System;


namespace BMC.BrickSet.Sync
{
    /// <summary>
    /// Current BrickSet sync status for display in the UI.
    /// </summary>
    public class BrickSetSyncStatus
    {
        public bool IsConnected { get; set; }
        public string BrickSetUsername { get; set; }
        public string SyncDirection { get; set; }

        public DateTime? LastSyncDate { get; set; }
        public DateTime? LastEnrichmentDate { get; set; }
        public string LastSyncError { get; set; }

        public int TotalTransactions { get; set; }
        public int RecentErrorCount { get; set; }

        /// <summary>
        /// Remaining API calls today (from getKeyUsageStats).
        /// Null if not yet checked.
        /// </summary>
        public int? ApiCallsRemainingToday { get; set; }
    }


    /// <summary>
    /// Result of a set enrichment operation.
    /// </summary>
    public class BrickSetEnrichmentResult
    {
        public int SetsEnriched { get; set; }
        public int ReviewsCached { get; set; }
        public int InstructionsFound { get; set; }
        public int ErrorCount { get; set; }
        public string LastError { get; set; }
        public int ApiCallsUsed { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }

        public TimeSpan Duration => CompletedAt - StartedAt;
    }
}
