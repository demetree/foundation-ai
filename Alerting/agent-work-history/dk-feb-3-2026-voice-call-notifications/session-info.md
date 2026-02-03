# Session Information

- **Conversation ID:** 73023cc9-1c67-48b5-a022-fcfcdfa664d5
- **Date:** 2026-02-03
- **Time:** 12:05 PM NST (-03:30)
- **Duration:** ~15 minutes

## Summary

Implemented `VoiceCallNotificationProvider` using Twilio Voice API with TwiML text-to-speech for automated incident notification calls.

## Files Modified

### New Files
- `Alerting.Server/Services/Notifications/VoiceCallNotificationProvider.cs` - Twilio voice call provider with Polly.Joanna TTS

### Modified Files
- `Alerting.Server/Program.cs` - Registered VoiceCallNotificationProvider

## Key Features
- Uses same Twilio credentials as SMS (`Twilio:AccountSid`, `Twilio:AuthToken`)
- Optional `Twilio:VoiceFromNumber` config (falls back to SMS number)
- TwiML with Polly.Joanna neural voice for natural speech
- Spells out incident key for clarity (e.g., "I N C dash 0 0 1")
- Announces: severity, service, title with repeat

## Related Sessions

- `dk-feb-3-2026-notification-retry-worker` - Retry worker and channel seed data
- `dk-feb-3-2026-push-notifications` - WebPush/FCM implementation
