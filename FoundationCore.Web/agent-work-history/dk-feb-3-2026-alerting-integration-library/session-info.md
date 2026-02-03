# Session Information

- **Conversation ID:** 73023cc9-1c67-48b5-a022-fcfcdfa664d5
- **Date:** 2026-02-03
- **Time:** 12:37 PM - 1:49 PM NST (-03:30)
- **Duration:** ~1 hour 15 minutes

## Summary

Created a shared Alerting integration library in FoundationCore.Web enabling any Foundation-based application to easily integrate with the Alerting system for incident management. Updated to use database-backed SystemSettings for API key storage instead of file-based storage.

## Files Created

### Alerting.Server
- `Controllers/IntegrationRegistrationController.cs` - OIDC-based self-registration endpoint

### FoundationCore.Web/Services/Alerting/
- `AlertingIntegrationOptions.cs` - Configuration options class
- `AlertingIntegrationDtos.cs` - Shared DTOs for API communication
- `IAlertingIntegrationService.cs` - Service interface
- `AlertingIntegrationService.cs` - HTTP client implementation with SystemSettings integration
- `AlertingIntegrationExtensions.cs` - DI extension method
- `AlertingWebhookControllerBase.cs` - Webhook handler base class

## Key Features

1. **Self-Registration:** Foundation apps can register with Alerting using OIDC auth to receive an API key
2. **Database-Backed Storage:** API keys stored in SystemSettings as `Alerting:Integration:{ServiceName}:ApiKey`
3. **Multi-Node Sync:** All server nodes read credentials from the shared Security database
4. **In-Memory Caching:** API key cached after first retrieval to avoid database hits
5. **Webhook Handler:** Base class for receiving incident state change callbacks
6. **Zero-friction DI:** `services.AddAlertingIntegration(configuration)`

## Configuration

```json
"Alerting": {
    "BaseUrl": "https://alerting.mycompany.com",
    "ServiceName": "Foundation",
    "CallbackUrl": "https://foundation.mycompany.com/api/alerting-webhook"
}
```

## SystemSettings Keys

After registration, these keys are stored:
- `Alerting:Integration:{ServiceName}:ApiKey`
- `Alerting:Integration:{ServiceName}:IntegrationId`
- `Alerting:Integration:{ServiceName}:ServiceId`

## Related Sessions

- `dk-feb-3-2026-voice-call-notifications` - Voice call notification provider
- `dk-feb-2-2026-api-key-hashing` - API key generation/hashing in integrations
