# Notification Audit Local Buffer — IndexedDB Integration

Add a local write-ahead buffer using `Foundation.IndexedDB` for notification delivery audit records. Every delivery attempt is immediately written to a fast local SQLite store via the Dexter API, then a background worker flushes completed records to SQL Server in batches. This decouples the audit trail from the main notification hot path and provides resilience if the central DB is temporarily slow.

## User Review Required

> [!IMPORTANT]
> This feature **augments** the existing audit flow — it does NOT replace the current direct SQL Server writes. The dispatcher still writes to `AlertingContext` as before. The local buffer is an additional, parallel write that provides:
> - A local forensic copy that survives SQL Server outages
> - A foundation for future offline-first or edge-deployed alerting scenarios

> [!NOTE]
> If you'd prefer the buffer to **replace** the direct SQL writes (write locally first, flush to SQL Server later), that's a bigger architectural change. Let me know and I'll redesign.

## Proposed Changes

### Alerting.Server Project

#### [MODIFY] [Alerting.Server.csproj](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Alerting.Server.csproj)

Add project reference to `Foundation.IndexedDB`.

---

#### [NEW] [LocalDeliveryRecord.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Models/LocalDeliveryRecord.cs)

POCO for the local IndexedDB store. Flat structure optimized for JSON serialization (no EF navigation properties):

| Field | Type | IndexedDB Role |
|-------|------|----------------|
| `Id` | `int` | `++Id` (auto-increment PK) |
| `CorrelationId` | `string` | `&CorrelationId` (unique index — maps to `objectGuid`) |
| `AttemptedAt` | `DateTime` | `attemptedAt` (indexed for time-range queries) |
| `ChannelTypeId` | `int` | `channelTypeId` (indexed) |
| `Status` | `string` | — |
| `IncidentNotificationId` | `int` | — |
| `RecipientAddress` | `string` | — |
| `Subject` | `string` | — |
| `BodyContent` | `string` | — |
| `ErrorMessage` | `string` | — |
| `Response` | `string` | — |
| `TenantGuid` | `string` | — |
| `FlushedToServer` | `bool` | `flushedToServer` (indexed — for flush worker queries) |

---

#### [NEW] [NotificationAuditBuffer.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/NotificationAuditBuffer.cs)

Core service implementing the local buffer using `DexterDatabase`:

```csharp
public class NotificationAuditDb : DexterDatabase
{
    public DexterTable<LocalDeliveryRecord, int> DeliveryAttempts { get; }

    public NotificationAuditDb(IDBDatabase db) : base(db)
    {
        Version(1).DefineStores(new Dictionary<string, string>
        {
            ["deliveryAttempts"] = "++Id, &CorrelationId, attemptedAt, channelTypeId, flushedToServer"
        });
        DeliveryAttempts = Table<LocalDeliveryRecord, int>("deliveryAttempts");
    }
}
```

`INotificationAuditBuffer` interface with:
- `RecordAttemptAsync(NotificationDeliveryAttempt attempt)` — write to local store
- `GetUnflushedAsync(int batchSize)` — query unflushed records
- `MarkFlushedAsync(IEnumerable<int> ids)` — mark batch as flushed
- `GetRecentAsync(int count)` — admin query for recent local records

---

#### [NEW] [NotificationAuditBufferWorker.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/NotificationAuditBufferWorker.cs)

`BackgroundService` that periodically flushes completed local records:
1. Query unflushed records where `Status != "Pending"` and `FlushedToServer == false`
2. Batch-insert/update corresponding `NotificationDeliveryAttempt` rows in SQL Server
3. Mark local records as flushed
4. Configurable interval (default: 30 seconds) and batch size (default: 100)

---

#### [MODIFY] [NotificationDispatcher.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/Notifications/NotificationDispatcher.cs)

Inject `INotificationAuditBuffer` and add a single line after each `SaveChangesAsync` in `DispatchToChannelAsync`:

```diff
 await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
+// Buffer to local audit store
+await _auditBuffer.RecordAttemptAsync(attempt).ConfigureAwait(false);
```

This is a fire-and-forget-safe call — if the local write fails, we log and continue (the SQL Server write already succeeded).

---

#### [MODIFY] [Program.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Program.cs)

Register new services in the DI container:

```csharp
// Notification Audit Local Buffer (IndexedDB)
builder.Services.AddSingleton<INotificationAuditBuffer, NotificationAuditBuffer>();
builder.Services.AddHostedService<NotificationAuditBufferWorker>();
```

---

#### [MODIFY] [appsettings.json](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/appsettings.json)

Add configuration section:

```json
"NotificationAuditBuffer": {
    "FlushIntervalSeconds": 30,
    "BatchSize": 100,
    "DatabaseName": "NotificationAudit"
}
```

## Verification Plan

### Automated Tests
- `dotnet build Alerting\Alerting.Server\Alerting.Server.csproj` — must compile with 0 errors

### Manual Verification
- After deploying, a `NotificationAudit.sqlite` file should appear alongside the Alerting Server executable
- Triggering a test notification should result in a record appearing in both:
  - The local SQLite database (viewable with any SQLite browser)
  - The SQL Server `NotificationDeliveryAttempts` table (as before)
