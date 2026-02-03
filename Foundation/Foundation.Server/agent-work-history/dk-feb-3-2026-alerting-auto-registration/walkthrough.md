# Walkthrough: Alerting Integration Library

## Summary
Created a shared integration library in `FoundationCore.Web` enabling any Foundation-based application to easily integrate with Alerting. API keys are stored in the Security database via `SystemSettings` for multi-node sync.

---

## New Files

### Alerting.Server
| File | Purpose |
|------|---------|
| [IntegrationRegistrationController.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Controllers/IntegrationRegistrationController.cs) | OIDC-based self-registration endpoint |

### FoundationCore.Web/Services/Alerting/
| File | Purpose |
|------|---------|
| [AlertingIntegrationOptions.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/Alerting/AlertingIntegrationOptions.cs) | Configuration options |
| [AlertingIntegrationDtos.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/Alerting/AlertingIntegrationDtos.cs) | Shared DTOs |
| [IAlertingIntegrationService.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/Alerting/IAlertingIntegrationService.cs) | Service interface |
| [AlertingIntegrationService.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/Alerting/AlertingIntegrationService.cs) | HTTP client with SystemSettings |
| [AlertingIntegrationExtensions.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/Alerting/AlertingIntegrationExtensions.cs) | DI extension |
| [AlertingWebhookControllerBase.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/Alerting/AlertingWebhookControllerBase.cs) | Webhook handler base class |

---

## API Key Storage (SystemSettings)

After registration, credentials are stored in the Security database:

| Setting Name | Description |
|--------------|-------------|
| `Alerting:Integration:{ServiceName}:ApiKey` | The API key for authenticated calls |
| `Alerting:Integration:{ServiceName}:IntegrationId` | The integration record ID |
| `Alerting:Integration:{ServiceName}:ServiceId` | The service record ID |

**Benefits:**
- All server nodes share the same credentials
- No risk of appsettings.json drift
- Can be rotated without app restarts

---

## Verification
- ✅ FoundationCore.Web: 0 errors
- ✅ Alerting.Server: 0 errors
