using System;


namespace BMC.Rebrickable.Sync
{
    /// <summary>
    /// Result of a pull/import operation from Rebrickable.
    /// </summary>
    public class SyncImportResult
    {
        public int SetsCreated { get; set; }
        public int SetsUpdated { get; set; }
        public int SetsRemoved { get; set; }

        public int SetListsCreated { get; set; }
        public int SetListsUpdated { get; set; }
        public int SetListsRemoved { get; set; }
        public int SetListItemsCreated { get; set; }
        public int SetListItemsUpdated { get; set; }
        public int SetListItemsRemoved { get; set; }

        public int PartListsCreated { get; set; }
        public int PartListsUpdated { get; set; }
        public int PartListsRemoved { get; set; }
        public int PartListItemsCreated { get; set; }
        public int PartListItemsUpdated { get; set; }
        public int PartListItemsRemoved { get; set; }

        public int LostPartsCreated { get; set; }
        public int LostPartsUpdated { get; set; }
        public int LostPartsRemoved { get; set; }

        public int TotalCreated => SetsCreated + SetListsCreated + SetListItemsCreated +
                                   PartListsCreated + PartListItemsCreated + LostPartsCreated;

        public int TotalUpdated => SetsUpdated + SetListsUpdated + SetListItemsUpdated +
                                   PartListsUpdated + PartListItemsUpdated + LostPartsUpdated;

        public int TotalRemoved => SetsRemoved + SetListsRemoved + SetListItemsRemoved +
                                   PartListsRemoved + PartListItemsRemoved + LostPartsRemoved;

        public int ErrorCount { get; set; }
        public string LastError { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }

        public TimeSpan Duration => CompletedAt - StartedAt;
    }


    /// <summary>
    /// Current sync status for display in the UI.
    /// </summary>
    public class SyncStatus
    {
        public bool IsConnected { get; set; }
        public string AuthMode { get; set; }
        public string IntegrationMode { get; set; }
        public string RebrickableUsername { get; set; }

        public DateTime? LastPullDate { get; set; }
        public DateTime? LastPushDate { get; set; }
        public string LastSyncError { get; set; }

        public int? PullIntervalMinutes { get; set; }
        public int TotalTransactions { get; set; }
        public int RecentErrorCount { get; set; }
    }
}
