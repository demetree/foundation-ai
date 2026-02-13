# Foundation.Telemetry — Overview

Foundation.Telemetry is a **.NET 10** class library that provides a **telemetry collection and storage** system for monitoring Foundation applications over time.

---

## Contents

| File / Folder | Purpose |
|---------------|---------|
| `TelemetryCollectorService.cs` | Background service that periodically polls configured applications for health metrics and stores results in the Telemetry database |
| `TelemetryConfiguration.cs` | Configuration model for telemetry settings (interval, retention, app list) |
| `TelemetryServiceExtensions.cs` | DI extension methods: `AddTelemetryServices()`, `UseTelemetryCollector()` |
| `Database/` | EF Core entity classes + `TelemetryContext` (12 entities) |
| `EntityExtensions/` | Auto-generated partial class extensions (10 files) |
| `Controllers/` | 1 custom controller for telemetry data access |
| `DataControllers/` | 10 auto-generated CRUD controllers |

> [!CAUTION]
> `Database/`, `DataControllers/`, and `EntityExtensions/` folders are auto-generated.

---

## How It Works

1. `TelemetryCollectorService` runs as a background timer (default: every 1 minute)
2. It polls each configured application's health endpoint
3. Collected metrics are stored in the `TelemetryContext` database
4. The Foundation.Client's **Telemetry Dashboard** visualizes this data
5. Old data is automatically purged after the configured retention period (default: 90 days)

---

## Configuration

```json
"Telemetry": {
    "Enabled": true,
    "CollectionIntervalMinutes": 1,
    "RetentionDays": 90,
    "Applications": [
        { "Name": "Foundation", "Url": "https://localhost:9101", "IsSelf": true },
        { "Name": "Scheduler", "Url": "https://localhost:10101" }
    ]
}
```

---

## Registration

In `Foundation.Server/Program.cs`:

```csharp
builder.Services.AddTelemetryServices(builder.Configuration);  // Register services
app.Services.UseTelemetryCollector();                           // Start collector
```

Currently only used by Foundation.Server. Other apps expose health endpoints that Telemetry polls.
