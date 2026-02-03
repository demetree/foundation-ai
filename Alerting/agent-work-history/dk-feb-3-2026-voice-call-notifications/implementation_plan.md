# Voice Call Notification Provider

## Goal
Implement automated voice calls for incident notifications using Twilio Voice API.

---

## Proposed Changes

### [NEW] [VoiceCallNotificationProvider.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/Notifications/VoiceCallNotificationProvider.cs)

Follows `SmsNotificationProvider` pattern:
- Uses same Twilio credentials (`Twilio:AccountSid`, `Twilio:AuthToken`)
- Adds `Twilio:VoiceFromNumber` config (can be same as SMS or different)
- Uses `CallResource.CreateAsync()` with TwiML URL for text-to-speech
- ChannelTypeId = 3 (VoiceCall)

**Features:**
- Text-to-speech using Twilio's `<Say>` verb
- Clear incident announcement with severity, title, service
- Configurable voice (Polly voices)

---

### [MODIFY] [Program.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Program.cs)

Register the provider:
```csharp
builder.Services.AddScoped<INotificationProvider, VoiceCallNotificationProvider>();
```

---

### [MODIFY] [appsettings.json](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/appsettings.json)

Add optional voice-specific config:
```json
"Twilio": {
  "VoiceFromNumber": "+1234567890",
  "VoiceUrl": null  // Optional: custom TwiML endpoint
}
```

---

## Verification

```powershell
dotnet build --no-restore
```
