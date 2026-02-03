# Walkthrough: Alerting Integration Library

## Summary
Created a shared integration library in `FoundationCore.Web` enabling any Foundation-based application to easily integrate with Alerting.

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
| [AlertingIntegrationService.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/Alerting/AlertingIntegrationService.cs) | HTTP client implementation |
| [AlertingIntegrationExtensions.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/Alerting/AlertingIntegrationExtensions.cs) | DI extension |
| [AlertingWebhookControllerBase.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/Alerting/AlertingWebhookControllerBase.cs) | Webhook handler base class |

---

## Usage Example

### 1. Configuration (`appsettings.json`)
```json
"Alerting": {
    "BaseUrl": "https://alerting.mycompany.com",
    "ServiceName": "Foundation",
    "CallbackUrl": "https://foundation.mycompany.com/api/alerting-webhook"
}
```

### 2. Registration (`Program.cs`)
```csharp
builder.Services.AddAlertingIntegration(builder.Configuration);
```

### 3. Self-Registration (one-time)
```csharp
var alerting = app.Services.GetRequiredService<IAlertingIntegrationService>();
var result = await alerting.RegisterAsync(accessToken);
// Store result.ApiKey in config
```

### 4. Raise Incidents
```csharp
await alerting.RaiseIncidentAsync(new RaiseIncidentRequest
{
    Severity = "Critical",
    Title = "Database connection failed",
    Description = "Unable to connect to primary DB"
});
```

### 5. Receive Webhooks
```csharp
[Route("api/alerting-webhook")]
public class MyWebhookController : AlertingWebhookControllerBase
{
    protected override Task OnIncidentResolvedAsync(IncidentWebhookPayload p)
    {
        // Clear local alert state
    }
}
```

---

## Verification
- ✅ Alerting.Server: 0 errors
- ✅ FoundationCore.Web: 0 errors
