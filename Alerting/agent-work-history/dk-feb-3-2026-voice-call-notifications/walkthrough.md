# Walkthrough: Voice Call Notifications

## Summary
Implemented `VoiceCallNotificationProvider` using Twilio Voice API for automated incident notification calls.

---

## Created: [VoiceCallNotificationProvider.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/Notifications/VoiceCallNotificationProvider.cs)

**Features:**
- Uses same Twilio credentials as SMS (`Twilio:AccountSid`, `Twilio:AuthToken`)
- Optional `Twilio:VoiceFromNumber` (falls back to `Twilio:FromNumber`)
- TwiML with Polly.Joanna neural voice for natural speech
- Clear incident announcement: severity, service, title
- Spells out incident key character by character for clarity

**TwiML Output:**
```xml
<Response>
    <Say voice="Polly.Joanna">
        Alert! Severity: Critical. Service: Payment API. 
        Incident: Database connection timeout...
    </Say>
</Response>
```

---

## Notification Channel Summary

| ID | Channel | Provider | Status |
|----|---------|----------|--------|
| 1 | Email | `EmailNotificationProvider` | ✅ |
| 2 | SMS | `SmsNotificationProvider` | ✅ |
| 3 | **VoiceCall** | **`VoiceCallNotificationProvider`** | ✅ **NEW** |
| 4 | MobilePush | - | ❌ |
| 5 | WebPush | `PushNotificationProvider` | ✅ |
| 6 | Teams | `TeamsNotificationProvider` | ✅ |

---

## Verification
Build succeeded with 0 errors.
