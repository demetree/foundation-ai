# FoundationCore — Key Classes Reference

Quick reference for the most commonly used classes in FoundationCore. Organized by folder.

---

## Security/

| Class | Namespace | Usage |
|-------|-----------|-------|
| `SecurityContext` | `Foundation.Security.Database` | EF Core DbContext for the Security database. Used by all apps. |
| `OidcApplicationManager` | `Foundation.Security.OIDC` | Registers OIDC clients on app startup. Constants: `SCHEDULER_SERVER_NAME`, `FOUNDATION_SERVER_NAME`, etc. |
| `SecurityLogic` | `Foundation.Security` | `EnsureSecurityUserEventTypesAsync()` — seeds event types. Privilege check helpers. |
| `AppSettings` | `Foundation.Security.Configuration` | Strongly-typed model for the `Settings` section of `appsettings.json`. |

---

## Auditor/

| Class | Namespace | Usage |
|-------|-----------|-------|
| `AuditorContext` | `Foundation.Auditor.Database` | EF Core DbContext for the Auditor database. |

---

## Services/

| Interface / Class | Usage |
|-------------------|-------|
| `IApplicationMetricsProvider` | Implement in each app to expose custom metrics to the System Health dashboard. |
| `IDatabaseHealthProvider` | Register per-database to enable health monitoring. |
| `DbContextHealthProvider<T>` | Generic implementation — pass any `DbContext` type and a label. |
| `IAuthenticatedUsersProvider` | Returns authenticated user count for health dashboard. |
| `SecurityContextAuthenticatedUsersProvider` | Default implementation using `SecurityContext`. |
| `ICredentialCacheService` | Caches OIDC tokens for service-to-service HTTP calls. |
| `ISessionTrackingService` | Tracks user sessions for the Security module. |
| `SendGridEmailService` | Send emails via SendGrid. Requires `SendGridAPIKey` in config. |
| `ILogErrorProvider` / `FileLogErrorProvider` | Collects log errors for health monitoring. |
| `IMonitoredApplicationService` / `MonitoredApplicationService` | Cross-app health check polling. |

---

## Utility/

### Startup & Configuration

| Class | Key Method / Property | Usage |
|-------|----------------------|-------|
| `StartupBasics` | `ConfigureAuditor()`, `BuildFoundationServices()` | Called in every app's `Program.cs`. |
| `Configuration` | `GetStringConfigurationSetting()`, `GetBoolConfigurationSetting()` | Static config access anywhere in the codebase. |

### Logging

| Class | Key Method | Usage |
|-------|-----------|-------|
| `Logger` | `LogInformation()`, `LogException()`, `LogSystem()`, `LogCritical()` | Foundation-wide logging. Auto-creates daily log files. |
| `LogFileService` | `GetLogFiles()`, `GetLogContent()` | Used by the Log Viewer UI. |
| `LogErrorNotificationConsumer` | Background consumer | Batches error notifications and sends via email/Alerting. |

### Database

| Class | Key Method | Usage |
|-------|-----------|-------|
| `DatabaseSchemaValidator<T>` | `ValidateSchemaAsync()` | Compares EF model against live SQL schema. Throws on mismatch. |
| `DatabaseBuilder` | `EnsureDatabase()` | Programmatic database creation. |
| `Entity` | Base class | All EF entities inherit from this — provides `Id`, `Active`, `Deleted`, `CreatedDate`, etc. |
| `UtcDateTimeInterceptor` | EF interceptor | Forces UTC on all DateTime reads/writes. Registered on every `DbContext`. |
| `DBContextExcelExporter` | `ExportToExcel()` | Exports any DbContext table to Excel. Used by `DataController`. |
| `DBContextExtensions` | Extension methods | Helpers for DbContext operations. |
| `ChangeHistoryToolSet` | Auto-gen helper | Manages change history records for entities. |

### Data & Caching

| Class | Key Method | Usage |
|-------|-----------|-------|
| `Cache` | `Get()`, `Set()`, `Remove()` | Thread-safe in-memory cache with expiration. |
| `DataLoadHelper` | Seeding methods | Loads reference data on startup. |

### Background Jobs

| Class | Usage |
|-------|-------|
| `BackgroundJob` | Base class for one-shot background tasks. |
| `RecurringJob` | Base class for periodic background tasks. |

### Serialization & HTTP

| Class | Usage |
|-------|-------|
| `JSON` | Contains `UtcDateTimeConverter` for System.Text.Json. |
| `HTTP` | HTTP client helper methods. |
| `OidcTokenHelper` | Obtains tokens for service-to-service API calls. |

### Other

| Class | Usage |
|-------|-------|
| `Extensions` | General .NET extension methods. |
| `StringIterator` | Character-by-character string parsing. |
| `ImageUtility` | Image resizing/thumbnail generation. |
| `Utility` | Miscellaneous helper methods. |
