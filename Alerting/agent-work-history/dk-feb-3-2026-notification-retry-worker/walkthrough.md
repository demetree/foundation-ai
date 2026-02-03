# Walkthrough: Notification Enhancements

## Summary
- Added retry worker for failed notifications
- Added Teams/WebPush to database seed data
- Fixed provider channel IDs
- Updated preferences UI icons

---

## 1. Background Retry Worker ✅

**Created:** [NotificationRetryWorker.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/Notifications/NotificationRetryWorker.cs)

- Interval: 60 seconds
- Exponential backoff: 1 min → 5 min → 15 min
- Max 3 retries before "Abandoned" status

---

## 2. Database Seed Data ✅

**Modified:** [AlertingDatabaseGenerator.cs](file:///g:/source/repos/Scheduler/DatabaseGenerators/AlertingDatabaseGenerator/AlertingDatabaseGenerator.cs#L104-L109)

| ID | Channel | Priority | Status |
|----|---------|----------|--------|
| 1 | Email | 30 | Existing |
| 2 | SMS | 10 | Existing |
| 3 | VoiceCall | 5 | Existing |
| 4 | MobilePush | 20 | Existing |
| 5 | **WebPush** | 25 | **NEW** |
| 6 | **Teams** | 40 | **NEW** |

---

## 3. Provider ID Fixes ✅

| Provider | Before | After |
|----------|--------|-------|
| `TeamsNotificationProvider` | 5 | **6** |
| `PushNotificationProvider` | 4 (MobilePush) | **5** (WebPush) |

---

## 4. Preferences UI ✅

Added icons in [notification-preferences-editor.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/notification-preferences-editor/notification-preferences-editor.component.ts#L77-L85):
- `WebPush`: `bi-bell-fill`
- `Teams`: `bi-chat-square-dots-fill`

---

## Next Step

> [!IMPORTANT]
> You'll need to **regenerate the Alerting database** (or run migrations) to add the new channel types. Until then, the UI will only show the original 4 channels.
