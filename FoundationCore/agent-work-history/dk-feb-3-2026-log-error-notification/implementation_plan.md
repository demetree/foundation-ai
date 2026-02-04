# Log Error Notification System

Implement a reusable log consumer that monitors for Exception/Error level log entries and sends batched notifications via email and/or the Alerting system.

## Background

The current `Logger.cs` writes errors to disk, but these often go unnoticed until problems escalate. The logger already has an `ILogConsumer` interface (lines 39-42) that allows plugging in custom consumers:

```csharp
public interface ILogConsumer
{
    void Log(DateTime timestamp, Logger.LogLevels level, string message, string threadName);
}
```

## User Review Required

> [!IMPORTANT]
> **Notification Channel Decision:** Choose one or more approaches:
> 1. **Email Only** — Uses existing `SendGridEmailService` (requires SendGrid config)
> 2. **Alerting Only** — Uses `AlertingIntegrationService` to raise incidents (takes advantage of escalation, on-call, and multi-channel notification)  
> 3. **Both** — Email for immediate visibility, Alerting for incident tracking and escalation
>
> **Recommendation:** Option 3 gives the best coverage—email for immediate developer awareness, Alerting for operational tracking.

> [!NOTE]
> **Placement Decision:** The new class will be placed in `FoundationCore/Utility/` (not `FoundationCore.Web`) so it can be used by non-web applications. It will accept optional interfaces for email/alerting so it works in all scenarios.

## Proposed Design

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Logger                                         │
│  ┌───────────────────┐                                                      │
│  │ ILogConsumer List │──────────────┐                                       │
│  └───────────────────┘              │                                       │
└─────────────────────────────────────┼───────────────────────────────────────┘
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                     LogErrorNotificationConsumer                            │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │ State:                                                                  │ │
│  │   - _pendingErrors: ConcurrentQueue<LogEntry>                          │ │
│  │   - _lastNotificationTime: DateTime                                    │ │
│  │   - _firstErrorInBatch: DateTime?                                      │ │
│  │   - _batchTimer: Timer (fires every minute to check window)            │ │
│  ├────────────────────────────────────────────────────────────────────────┤ │
│  │ Flow:                                                                   │ │
│  │   1. Log() called by Logger (fire-and-forget)                          │ │
│  │   2. If level <= Error, queue the entry                                │ │
│  │   3. If first error in window, send immediately                        │ │
│  │   4. Suppress further sends for BatchWindowMinutes (default 10)        │ │
│  │   5. After window expires, flush accumulated errors                    │ │
│  │   6. Repeat                                                            │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                              │                         │                    │
│                              ▼                         ▼                    │
│              ┌───────────────────────┐   ┌───────────────────────────────┐  │
│              │ IEmailNotifier        │   │ IAlertingNotifier             │  │
│              │ (optional)            │   │ (optional)                    │  │
│              └───────────────────────┘   └───────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Proposed Changes

---

### FoundationCore — Core Consumer

#### [NEW] [LogErrorNotificationConsumer.cs](file:///g:/source/repos/Scheduler/FoundationCore/Utility/LogErrorNotificationConsumer.cs)

Main consumer class implementing `Logger.ILogConsumer`:

```csharp
public class LogErrorNotificationConsumer : Logger.ILogConsumer, IDisposable
{
    // Configuration
    private readonly LogErrorNotificationOptions _options;
    
    // Optional notification channels (injected or set via factory)
    private readonly Func<string, string, Task<bool>> _sendEmailAsync;
    private readonly Func<string, string, Task> _raiseAlertAsync;
    
    // State
    private readonly ConcurrentQueue<ErrorEntry> _pendingErrors;
    private DateTime _lastNotificationTime = DateTime.MinValue;
    private DateTime? _firstErrorInBatch = null;
    private Timer _batchTimer;
    
    // ILogConsumer implementation
    public void Log(DateTime timestamp, Logger.LogLevels level, string message, string threadName)
    {
        // Only capture Exception and Error levels
        if (level > Logger.LogLevels.Error) return;
        
        // Queue the error
        _pendingErrors.Enqueue(new ErrorEntry(timestamp, level, message, threadName));
        
        // If first error in window, send immediately
        if (_firstErrorInBatch == null)
        {
            _firstErrorInBatch = DateTime.UtcNow;
            Task.Run(() => FlushPendingErrorsAsync());
        }
    }
    
    private async Task FlushPendingErrorsAsync() { /* batch and send */ }
}
```

**Key behaviors:**
- **Immediate first notification:** On first error, sends immediately
- **Batching window:** Suppresses additional notifications for configurable period (default 10 minutes)
- **Accumulation:** Queues all errors during suppression window
- **Flush on window expiry:** Sends batched summary when window expires
- **Thread-safe:** Uses `ConcurrentQueue` for lock-free operation

---

#### [NEW] [LogErrorNotificationOptions.cs](file:///g:/source/repos/Scheduler/FoundationCore/Utility/LogErrorNotificationOptions.cs)

Configuration class:

```csharp
public class LogErrorNotificationOptions
{
    // System identification
    public string SystemName { get; set; } = "Unknown System";
    public string Environment { get; set; } = "Production";
    
    // Email configuration
    public bool EnableEmail { get; set; } = true;
    public List<string> NotificationEmails { get; set; } = new();
    public string EmailFromAddress { get; set; }
    public string EmailFromName { get; set; } = "System Monitor";
    
    // Alerting configuration  
    public bool EnableAlerting { get; set; } = false;
    public string AlertingSeverity { get; set; } = "High";
    
    // Batching configuration
    public int BatchWindowMinutes { get; set; } = 10;
    public int MaxErrorsPerBatch { get; set; } = 100;
    
    // Log level filter
    public Logger.LogLevels MinimumLevel { get; set; } = Logger.LogLevels.Error;
}
```

---

### FoundationCore.Web — DI Integration

#### [NEW] [LogErrorNotificationExtensions.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/LogErrorNotificationExtensions.cs)

Extension method for easy DI registration:

```csharp
public static class LogErrorNotificationExtensions
{
    public static IServiceCollection AddLogErrorNotification(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LogErrorNotificationOptions>(
            configuration.GetSection("LogErrorNotification"));
        
        services.AddSingleton<LogErrorNotificationConsumer>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<LogErrorNotificationOptions>>().Value;
            var logger = Logger.GetCommonLogger();
            
            // Wire up email sender if configured
            Func<string, string, Task<bool>> emailSender = null;
            if (options.EnableEmail && options.NotificationEmails.Any())
            {
                emailSender = (subject, body) => 
                    SendGridEmailService.SendEmailToMultipleRecipientsAsync(
                        options.EmailFromAddress,
                        options.EmailFromName,
                        options.NotificationEmails,
                        subject,
                        body,
                        includeSignature: false);
            }
            
            // Wire up alerting if configured
            Func<string, string, Task> alertSender = null;
            if (options.EnableAlerting)
            {
                var alertingService = sp.GetService<IAlertingIntegrationService>();
                if (alertingService?.IsRegistered == true)
                {
                    alertSender = async (title, description) =>
                    {
                        await alertingService.RaiseIncidentAsync(new RaiseIncidentRequest
                        {
                            Severity = options.AlertingSeverity,
                            Title = title,
                            Description = description,
                            DeduplicationKey = $"{options.SystemName}-log-errors",
                            Source = options.SystemName
                        });
                    };
                }
            }
            
            var consumer = new LogErrorNotificationConsumer(options, emailSender, alertSender);
            
            // Register with common logger
            logger.AddConsumer(consumer);
            
            return consumer;
        });
        
        return services;
    }
}
```

---

### Scheduler.Server — Integration Example

#### [MODIFY] [appsettings.json](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/appsettings.json)

Add configuration section:

```json
{
  "LogErrorNotification": {
    "SystemName": "Scheduler",
    "Environment": "Production",
    "EnableEmail": true,
    "NotificationEmails": ["admin@example.com"],
    "EmailFromAddress": "scheduler@example.com",
    "EmailFromName": "Scheduler System Monitor",
    "EnableAlerting": true,
    "AlertingSeverity": "High",
    "BatchWindowMinutes": 10,
    "MaxErrorsPerBatch": 100,
    "MinimumLevel": "Error"
  }
}
```

#### [MODIFY] [Program.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Program.cs)

Wire up the consumer during startup (after alerting integration is registered):

```csharp
// After AddAlertingIntegration()
builder.Services.AddLogErrorNotification(builder.Configuration);
```

---

## Email Format

The notification email will be formatted like this:

**Subject:** `🚨 [Scheduler] Log Errors Detected - 5 errors since 10:30 AM`

**Body:**
```
⚠️ Log Error Summary for Scheduler

Environment: Production
Time Window: Feb 3, 2026 10:30:00 - 10:40:00 (local)
Total Errors: 5

───────────────────────────────────────────────────────────────
ERROR DETAILS
───────────────────────────────────────────────────────────────

[10:30:15.123456] EXCEPTION - WorkerThread
Job 'DataSync' failed with exception: System.InvalidOperationException: Connection timeout
   at DataSyncService.ExecuteAsync(...)
   at ...

[10:32:45.654321] ERROR - MainThread  
Database connection pool exhausted. Current: 100, Max: 100

[10:35:12.987654] ERROR - BackgroundJob
Failed to process batch ID 12345: Timeout after 30 seconds

... (2 more errors)

───────────────────────────────────────────────────────────────
This is an automated notification. Next notification suppressed until 10:40 AM.
```

---

## Verification Plan

### Build Verification
```powershell
cd g:\source\repos\Scheduler
dotnet build Scheduler.sln
```

### Manual Testing

Since this is a notification system that triggers on real errors, manual testing will involve:

1. **Add the new files and configuration**
2. **Temporarily inject a test error** in Scheduler.Server:
   - Add a test endpoint or startup code that logs an error
   - Example: `Logger.GetCommonLogger().LogException("Test error for notification system");`
3. **Verify email/alert arrives** within expected time window
4. **Log multiple errors** and verify batching behavior

> [!NOTE]
> I recommend you manually test this after implementation by:
> 1. Running Scheduler.Server locally
> 2. Triggering a test error via a simple test endpoint
> 3. Confirming the email arrives at your configured address
>
> Do you have a preferred email address to use for testing, or should I add a test endpoint you can hit manually?

---

## Summary

| File | Purpose |
|------|---------|
| [LogErrorNotificationConsumer.cs](file:///g:/source/repos/Scheduler/FoundationCore/Utility/LogErrorNotificationConsumer.cs) | Core consumer with batching logic |
| [LogErrorNotificationOptions.cs](file:///g:/source/repos/Scheduler/FoundationCore/Utility/LogErrorNotificationOptions.cs) | Configuration class |
| [LogErrorNotificationExtensions.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/LogErrorNotificationExtensions.cs) | DI extension for easy wiring |
| [appsettings.json](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/appsettings.json) | Example configuration |
| [Program.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Program.cs) | Startup wiring |
