# FoundationCore — Architecture

FoundationCore is a .NET 10 class library that provides the core platform infrastructure: Security module, Auditor module, and shared services/utilities used by all Foundation-based applications.

---

## Module Structure

```
FoundationCore/
├── Security/           ← Security module (93 files)
│   ├── Database/       ← SecurityContext, entity classes
│   ├── Configuration/  ← OIDC setup, AppSettings
│   ├── OIDC/           ← OpenIddict application manager
│   └── Logic/          ← SecurityLogic, privilege checks
│
├── Auditor/            ← Auditor module (36 files)
│   ├── Database/       ← AuditorContext, entity classes
│   └── Logic/          ← Audit event creation and management
│
├── Services/           ← Shared service contracts (12 files)
├── Utility/            ← Core utilities (24 files)
├── Models/             ← Shared models
└── appsettings.json    ← Default configuration
```

---

## Security Module

The Security module provides:

- **User management** — `SecurityUser` entity with roles, tenants, departments, teams
- **Role-based access** — `SecurityRole` with per-module privilege levels and per-table access levels
- **Multi-tenancy** — tenant isolation at the data layer
- **Data visibility** — additional access grouping within tenants
- **OIDC** — `OidcApplicationManager` registers client applications (Scheduler, Foundation, Alerting, Swagger) with OpenIddict
- **Session tracking** — login/logout events, session revocation
- **External login** — Google, Facebook, Microsoft, Twitter integration hooks

### Key Classes

| Class | Purpose |
|-------|---------|
| `SecurityContext` | EF Core DbContext for the Security database |
| `OidcApplicationManager` | Registers and manages OIDC client apps |
| `SecurityLogic` | Business logic for privilege checks and user event types |
| `AppSettings` | Strongly-typed configuration model |

---

## Auditor Module

The Auditor module provides automatic audit logging:

- Records create/read/update/delete operations
- Captures entity state snapshots (before/after)
- Supports async dispatch (`DispatchToBackgroundImmediately` mode)
- Configurable per-table auditing levels

### Key Classes

| Class | Purpose |
|-------|---------|
| `AuditorContext` | EF Core DbContext for the Auditor database |
| Audit event entities | `AuditEvent`, `AuditEventEntityState`, `AuditEventErrorMessage` |

---

## Services (`Services/`)

Shared service interfaces and implementations:

| File | Purpose |
|------|---------|
| `IApplicationMetricsProvider` | Interface for app-specific metrics (implemented per app) |
| `IAuthenticatedUsersProvider` | Interface for authenticated user counts |
| `IDatabaseHealthProvider` | Interface for database health checks |
| `DbContextHealthProvider` | Generic implementation that health-checks any `DbContext` |
| `SecurityContextAuthenticatedUsersProvider` | Queries `SecurityContext` for active user counts |
| `ICredentialCacheService` / `CredentialCacheService` | Caches OIDC credentials for service-to-service calls |
| `ISessionTrackingService` / `SessionTrackingService` | Tracks active user sessions |
| `SendGridEmailService` | Email delivery via SendGrid API |
| `ILogErrorProvider` / `FileLogErrorProvider` | Abstract + file-based log error collection |

---

## Utility (`Utility/`)

Foundation's workhorse utility classes — 24 files covering infrastructure concerns:

| Class | Purpose |
|-------|---------|
| `Logger` | Platform-wide logging implementation (file-based, structured) |
| `Configuration` | Static configuration access (`GetStringConfigurationSetting`, etc.) |
| `StartupBasics` | Shared startup helpers (`ConfigureAuditor`, `BuildFoundationServices`) |
| `DatabaseSchemaValidator` | Validates EF model against live SQL Server schema on startup |
| `DatabaseBuilder` | Programmatic database creation/migration |
| `Entity` | Base class for all EF entities (common fields like `Id`, `Active`, `Deleted`) |
| `Cache` | Thread-safe in-memory caching |
| `UtcDateTimeInterceptor` | EF Core interceptor enforcing UTC DateTime storage |
| `BackgroundJob` / `RecurringJob` | Foundation background job infrastructure |
| `DBContextExcelExporter` | Generic Excel export from any DbContext |
| `DBContextExtensions` | EF Core extension methods |
| `JSON` | `UtcDateTimeConverter` and other JSON serialization helpers |
| `HTTP` | HTTP client utilities |
| `OidcTokenHelper` | Helper for obtaining service-to-service tokens |
| `LogErrorNotificationConsumer/Options` | Error notification batching and delivery |
| `LogFileService` | Log file access and management |
| `ChangeHistoryToolSet` | Auto-generated change history helpers |
| `DataLoadHelper` | Data seeding and loading utilities |
| `Extensions` | General .NET extension methods |
| `StringIterator` | String parsing helper |
| `ImageUtility` | Image processing |
| `Utility` | Miscellaneous utilities |

---

## How Other Projects Use FoundationCore

Every Foundation-based application references FoundationCore (usually transitively via FoundationCore.Web):

- `BuildFoundationServices(builder, logger)` — called in every app's `Program.cs` to set up Security, OIDC, and database contexts
- `DatabaseSchemaValidator` — validates schemas on startup
- `Entity` base class — all EF entities inherit from this
- `Logger` — shared logging across all apps
