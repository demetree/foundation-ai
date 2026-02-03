# Implementation Plan: Notification Delivery System

## Goal
Transform the Alerting module's notification system from a **stub that records pending deliveries** into a **production-ready delivery pipeline** that actually sends SMS, Email, Voice, and Push notifications to on-call responders.

---

## Current State Analysis

### ✅ Already Exists (Good Foundation)
| Component | Location | Status |
|-----------|----------|--------|
| **NotificationChannelType** table | Schema | Email, SMS, VoiceCall, MobilePush defined with priority |
| **UserNotificationPreference** table | Schema | Quiet hours, DND, timezone support |
| **UserNotificationChannelPreference** table | Schema | Per-user channel enable/disable |
| **NotificationDeliveryAttempt** table | Schema | Tracks attempts, status, errors |
| **IncidentNotification** table | Schema | Links incident → user → escalation rule |
| **SendGridEmailService** | FoundationCore | Working SendGrid integration |

### ❌ Missing (To Be Built)
| Gap | Issue |
|-----|-------|
| **No dispatcher** | `CreateNotificationAsync` creates DB records but never sends |
| **No provider abstraction** | No `INotificationProvider` interface |
| **No background worker** | No service to process pending deliveries |
| **No user contact info** | Need phone number, push token storage |
| **No Twilio integration** | SMS/Voice not implemented |
| **No retry logic** | Failed deliveries aren't retried |

---

## Proposed Changes

### Phase 1: Core Infrastructure

---

#### [NEW] [INotificationProvider.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/Notifications/INotificationProvider.cs)

Abstract interface for notification channels:

```csharp
public interface INotificationProvider
{
    NotificationChannelType ChannelType { get; }
    Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken ct);
}

public record NotificationRequest(
    Guid UserObjectGuid,
    string UserEmail,
    string? UserPhoneNumber,
    string? PushToken,
    IncidentData Incident,
    SeverityData Severity
);

public record NotificationResult(
    bool Success,
    string? ExternalMessageId,
    string? ErrorMessage
);
```

---

#### [NEW] [EmailNotificationProvider.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/Notifications/EmailNotificationProvider.cs)

Wraps existing `SendGridEmailService`:

```csharp
public class EmailNotificationProvider : INotificationProvider
{
    public NotificationChannelType ChannelType => NotificationChannelType.Email;
    
    public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken ct)
    {
        var subject = $"[{request.Severity.Name}] {request.Incident.Title}";
        var body = BuildEmailBody(request);
        
        var success = await SendGridEmailService.SendEmailAsync(
            senderEmail: null,  // Use config default
            senderName: "Alerting System",
            toEmail: request.UserEmail,
            subject: subject,
            body: body,
            bodyIsHtml: true
        );
        
        return new NotificationResult(success, null, success ? null : "SendGrid delivery failed");
    }
}
```

---

#### [NEW] [SmsNotificationProvider.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/Notifications/SmsNotificationProvider.cs)

New Twilio integration:

```csharp
public class SmsNotificationProvider : INotificationProvider
{
    private readonly TwilioRestClient _client;
    private readonly IConfiguration _config;
    
    public NotificationChannelType ChannelType => NotificationChannelType.SMS;
    
    public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.UserPhoneNumber))
            return new NotificationResult(false, null, "No phone number configured");
            
        var message = await MessageResource.CreateAsync(
            to: new PhoneNumber(request.UserPhoneNumber),
            from: new PhoneNumber(_config["Twilio:FromNumber"]),
            body: $"[{request.Severity.Name}] {request.Incident.Title} - {request.Incident.IncidentKey}"
        );
        
        return new NotificationResult(
            message.Status != MessageResource.StatusEnum.Failed,
            message.Sid,
            message.ErrorMessage
        );
    }
}
```

---

#### [NEW] [VoiceNotificationProvider.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/Notifications/VoiceNotificationProvider.cs)

Twilio voice call with TwiML:

```csharp
public class VoiceNotificationProvider : INotificationProvider
{
    public NotificationChannelType ChannelType => NotificationChannelType.VoiceCall;
    
    public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken ct)
    {
        var call = await CallResource.CreateAsync(
            to: new PhoneNumber(request.UserPhoneNumber),
            from: new PhoneNumber(_config["Twilio:FromNumber"]),
            twiml: new Twiml($@"
                <Response>
                    <Say voice='alice'>
                        Alert from Alerting System.
                        {request.Severity.Name} severity incident.
                        {request.Incident.Title}.
                        Press 1 to acknowledge.
                    </Say>
                    <Gather numDigits='1' action='/api/twilio/acknowledge/{request.Incident.Id}'/>
                </Response>
            ")
        );
        
        return new NotificationResult(true, call.Sid, null);
    }
}
```

---

#### [NEW] [PushNotificationProvider.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/Notifications/PushNotificationProvider.cs)

Firebase Cloud Messaging:

```csharp
public class PushNotificationProvider : INotificationProvider
{
    public NotificationChannelType ChannelType => NotificationChannelType.MobilePush;
    
    public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.PushToken))
            return new NotificationResult(false, null, "No push token registered");
            
        var message = new Message
        {
            Token = request.PushToken,
            Notification = new Notification
            {
                Title = $"[{request.Severity.Name}] {request.Incident.Title}",
                Body = request.Incident.Description?.Substring(0, 100)
            },
            Data = new Dictionary<string, string>
            {
                ["incidentId"] = request.Incident.Id.ToString(),
                ["incidentKey"] = request.Incident.IncidentKey
            }
        };
        
        var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        return new NotificationResult(true, response, null);
    }
}
```

---

#### [NEW] [NotificationDispatcher.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/Notifications/NotificationDispatcher.cs)

Orchestrator that selects channels based on user preferences:

```csharp
public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IEnumerable<INotificationProvider> _providers;
    private readonly AlertingContext _context;
    
    public async Task DispatchAsync(IncidentNotification notification, CancellationToken ct)
    {
        // 1. Load user preferences
        var prefs = await GetUserPreferences(notification.UserObjectGuid);
        
        // 2. Check quiet hours & DND
        if (IsInQuietHours(prefs) || prefs.IsDoNotDisturb)
            return; // Or fallback to critical-only channel
            
        // 3. Get enabled channels sorted by priority
        var channels = await GetEnabledChannelsInPriorityOrder(prefs);
        
        // 4. Dispatch to each channel
        foreach (var channel in channels)
        {
            var provider = _providers.FirstOrDefault(p => p.ChannelType == channel);
            if (provider == null) continue;
            
            var result = await provider.SendAsync(BuildRequest(notification), ct);
            await RecordDeliveryAttempt(notification, channel, result);
            
            if (result.Success) break; // Stop on first success (or continue for all)
        }
    }
}
```

---

#### [NEW] [NotificationDeliveryWorker.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Workers/NotificationDeliveryWorker.cs)

Background service to process pending deliveries with retry:

```csharp
public class NotificationDeliveryWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Find pending delivery attempts
            var pending = await _context.NotificationDeliveryAttempts
                .Where(a => a.status == "Pending" || 
                           (a.status == "Failed" && a.attemptNumber < MaxRetries))
                .OrderBy(a => a.attemptedAt)
                .Take(50)
                .ToListAsync();
                
            foreach (var attempt in pending)
            {
                await ProcessAttemptAsync(attempt, stoppingToken);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

---

### Phase 2: Schema Enhancement

#### [MODIFY] [AlertingDatabaseGenerator.cs](file:///g:/source/repos/Scheduler/DatabaseGenerators/AlertingDatabaseGenerator/AlertingDatabaseGenerator.cs)

Add user contact info storage:

```csharp
// New table: UserContactInfo
Database.Table userContactTable = database.AddTable("UserContactInfo");
userContactTable.comment = "User contact information for notification delivery.";
userContactTable.AddIdField();
userContactTable.AddMultiTenantSupport();
userContactTable.AddGuidField("securityUserObjectGuid", false);
userContactTable.AddString250Field("primaryEmail", true);
userContactTable.AddString50Field("primaryPhoneNumber", true);  // E.164 format
userContactTable.AddString50Field("secondaryPhoneNumber", true);
userContactTable.AddString500Field("fcmPushToken", true);       // Firebase token
userContactTable.AddString500Field("apnsPushToken", true);      // Apple token
userContactTable.AddBoolField("phoneVerified", false, false);
userContactTable.AddBoolField("emailVerified", false, false);
userContactTable.AddVersionControl();
userContactTable.AddControlFields(true);
userContactTable.AddUniqueConstraint("tenantGuid", "securityUserObjectGuid");
```

Add retry tracking to delivery attempts:

```csharp
// Enhance NotificationDeliveryAttempt
deliveryAttemptTable.AddDateTimeField("nextRetryAt", true);
deliveryAttemptTable.AddIntField("maxAttempts", false, 3);
```

---

### Phase 3: Configuration

#### [MODIFY] [appsettings.json](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/appsettings.json)

```json
{
  "NotificationProviders": {
    "Email": {
      "Enabled": true,
      "Provider": "SendGrid"
    },
    "SMS": {
      "Enabled": true,
      "Provider": "Twilio",
      "TwilioAccountSid": "{{secret}}",
      "TwilioAuthToken": "{{secret}}",
      "TwilioFromNumber": "+1234567890"
    },
    "VoiceCall": {
      "Enabled": true,
      "Provider": "Twilio"
    },
    "MobilePush": {
      "Enabled": false,
      "Provider": "Firebase"
    }
  }
}
```

---

## Verification Plan

### Automated Tests
1. Unit tests for each `INotificationProvider` with mocked external services
2. Integration tests for `NotificationDispatcher` preference resolution
3. Test quiet hours and DND logic

### Manual Verification
1. Trigger test incident → verify Email received
2. Trigger test incident → verify SMS received
3. Test retry logic by simulating provider failure
4. Verify delivery attempts recorded in database

---

## Dependencies
- **Twilio NuGet**: `Twilio` package for SMS/Voice
- **FirebaseAdmin NuGet**: `FirebaseAdmin` for push notifications
- **SendGrid**: Already referenced in FoundationCore

---

## Implementation Order

| Phase | Component | Effort |
|-------|-----------|--------|
| 1a | `INotificationProvider` interface | 30 min |
| 1b | `EmailNotificationProvider` (wrap existing) | 1 hr |
| 1c | `NotificationDispatcher` | 2 hr |
| 1d | Modify `EscalationService` to call dispatcher | 1 hr |
| 2a | `UserContactInfo` schema | 1 hr |
| 2b | Contact info UI in Notification Preferences | 2 hr |
| 3a | `SmsNotificationProvider` + Twilio | 2 hr |
| 3b | `NotificationDeliveryWorker` + retry | 2 hr |
| 4 | `VoiceNotificationProvider` | 2 hr |
| 5 | `PushNotificationProvider` + Firebase | 3 hr |

**Total Estimated Effort: ~16 hours**
