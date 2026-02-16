# Notification Audit Local Buffer (IndexedDB Integration)

## Setup
- [x] Add `Foundation.IndexedDB` project reference to `Alerting.Server.csproj`

## Core Service
- [x] Create `NotificationAuditBuffer` service with Dexter API
- [x] Create `LocalDeliveryRecord` model for the local store
- [x] Create `NotificationAuditBufferWorker` background service for batch flushing

## Integration
- [x] Inject buffer into `NotificationDispatcher` — write locally alongside SQL Server writes
- [x] Register services in `Program.cs`
- [x] Add configuration section for buffer settings

## Verification
- [x] Build the solution successfully (0 errors, 8 pre-existing warnings)
