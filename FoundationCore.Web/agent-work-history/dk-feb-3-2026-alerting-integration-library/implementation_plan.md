# FoundationCore.Web Alerting Integration Library

## Goal
Create a shared integration library allowing any Foundation-based application to easily integrate with Alerting for incident management.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    FoundationCore.Web                        │
│  ┌─────────────────────────────────────────────────────────┐│
│  │         AlertingIntegrationService (client)             ││
│  │  - RegisterAsync() → gets API key via OIDC              ││
│  │  - RaiseIncidentAsync()                                 ││
│  │  - GetIncidentStatusAsync()                             ││
│  └─────────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────────┐│
│  │        AlertingWebhookHandler (server)                  ││
│  │  - Receives incident state changes                      ││
│  │  - IAlertingWebhookCallback interface for apps          ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
```

---

## Phase 1: Alerting API Enhancements

### [NEW] `IntegrationRegistrationController`
- **Endpoint:** `POST /api/integrations/register`
- **Auth:** OIDC token (same as user login)
- **Input:** `{ serviceName, serviceUrl, callbackUrl }`
- **Output:** `{ integrationId, apiKey, serviceName }`
- Creates Integration record with auto-generated API key

---

## Phase 2: FoundationCore.Web Library

### [NEW] `AlertingIntegrationService`
```csharp
public interface IAlertingIntegrationService
{
    Task<string> RegisterAsync(string serviceName, string callbackUrl);
    Task<IncidentResponse> RaiseIncidentAsync(RaiseIncidentRequest request);
    Task<IncidentStatusResponse> GetIncidentStatusAsync(string incidentKey);
    Task<List<IncidentSummary>> GetMyIncidentsAsync(IncidentFilter filter);
}
```

### [NEW] `AlertingWebhookController` base class
```csharp
public abstract class AlertingWebhookControllerBase : ControllerBase
{
    protected abstract Task OnIncidentCreatedAsync(IncidentWebhookPayload payload);
    protected abstract Task OnIncidentAcknowledgedAsync(IncidentWebhookPayload payload);
    protected abstract Task OnIncidentResolvedAsync(IncidentWebhookPayload payload);
}
```

### [NEW] Configuration
```json
"Alerting": {
    "BaseUrl": "https://alerting.mycompany.com",
    "ApiKey": "auto-populated-on-registration",
    "ServiceName": "Foundation",
    "CallbackUrl": "https://foundation.mycompany.com/api/alerting-webhook"
}
```

---

## Phase 3: Webhook Delivery (Alerting.Server)

Enhance existing webhook system to deliver to registered callback URLs when incidents change state.

---

## API Key Storage Decision

**Supported approaches (configurable):**

1. **File-based** (default) - Writes to separate key file or appsettings
   - Simple, works out of the box
   - Good for single-tenant deployments

2. **Database per-tenant** (future) - Store in tenant settings table
   - Requires new `TenantSettings` infrastructure in FoundationCore
   - Better for multi-tenant SaaS deployments
   - Could store other tenant-level secrets/config

> [!NOTE]
> Initial implementation will use file-based storage. Database option can be added when `TenantSettings` infrastructure is built.

---

## Verification
- Unit tests for client library
- Integration test: Foundation → Alerting → Webhook callback
