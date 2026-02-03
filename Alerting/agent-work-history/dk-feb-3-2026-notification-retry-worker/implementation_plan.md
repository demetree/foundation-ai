# Notification Enhancements

## Goals
1. **Background retry worker** - Automatically retry failed notification deliveries
2. **Update notification-preferences-editor** - Add Teams/Push channel icons, ensure feature complete
3. **Verify on-call schedule integration** - Confirm routing to on-call responders works correctly

---

## Proposed Changes

### 1. Background Retry Worker

#### [NEW] [NotificationRetryWorker.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/NotificationRetryWorker.cs)

Background service (follows `EscalationWorker` pattern):

- **Interval:** 60 seconds
- **Logic:**
  - Query `NotificationDeliveryAttempts` where `status = 'Failed'` AND `attemptNumber < MaxRetries`
  - For each, increment `attemptNumber`, re-attempt via the appropriate provider
  - Update status to `Sent` or `Failed` with error message
  - Mark as `Abandoned` after max retries (e.g., 3)

- **Retry backoff:** Exponential (1 min, 5 min, 15 min) based on attemptNumber

---

#### [MODIFY] [Program.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Program.cs)

Register the worker:
```csharp
builder.Services.AddHostedService<NotificationRetryWorker>();
```

---

### 2. Update Notification Preferences Editor

#### [MODIFY] [notification-preferences-editor.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/notification-preferences-editor/notification-preferences-editor.component.ts#L78-L83)

Add missing channel icons for Teams and Push:

```typescript
private channelIcons: { [key: string]: string } = {
    'Email': 'bi-envelope-fill',
    'SMS': 'bi-chat-text-fill',
    'VoiceCall': 'bi-telephone-fill',
    'MobilePush': 'bi-phone-fill',
    'WebPush': 'bi-bell-fill',       // NEW
    'Teams': 'bi-microsoft-teams'     // NEW (or bi-chat-square-dots)
};
```

---

### 3. On-Call Schedule Integration (Review)

**Current state:** Already integrated in `EscalationService`:
- `ProcessPendingEscalationsAsync` queries `OnCallSchedules`
- `EscalationWorker` runs every 30s to process escalations
- When an incident triggers, it finds on-call users via schedules

**No changes needed** - integration is already complete.

---

## Verification Plan

### Automated
```powershell
# Build verification
dotnet build --no-restore
npm run build
```

### Manual
1. **Test retry:** Create a notification, force failure (invalid config), verify retry worker picks it up
2. **UI test:** Navigate to notification preferences, verify Teams/Push icons display correctly
