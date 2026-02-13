# FoundationCore.Web — Key Classes Reference

Quick reference for all controllers, services, and hubs in FoundationCore.Web.

---

## Base Controllers

| Class | Key Members | Usage |
|-------|------------|-------|
| `FoundationControllerBase` | Auth header extraction, tenant resolution, `GetCurrentUser()` | Inherit from this for any custom controller |
| `DashboardControllerBase` | Dashboard-specific helpers | Inherit for dashboard-type controllers |

---

## Security Controllers

| Class | Key Endpoints |
|-------|--------------|
| `AuthorizationController` | `POST /connect/token` — OIDC password flow |
| `SecurityUsersController` | `GET/POST/PUT/DELETE /api/SecurityUsers` — full user CRUD |
| `SecurityUsersController.AdminActions` | `POST /api/SecurityUsers/{id}/Lock`, `/Unlock`, `/ForcePasswordReset` |
| `SecurityProfileController` | `GET /api/SecurityProfile` — current user's profile |
| `SessionsController` | `GET /api/Sessions`, `DELETE /api/Sessions/{id}` — view/revoke |
| `TenantSettingsController` | `GET/PUT /api/TenantSettings` |
| `UserSettingsController` | `GET/PUT /api/UserSettings` |
| `NewUserController` | `POST /api/NewUser` — self-registration |
| `ResetPasswordController` | `POST /api/ResetPassword` |

---

## Auditor Controllers

| Class | Key Endpoints |
|-------|--------------|
| `AuditEventsController` | `GET /api/AuditEvents` — filtered audit log |
| `AuditEventEntityStatesController` | `GET /api/AuditEventEntityStates` — before/after data |
| `AuditEventPurgeController` | `POST /api/AuditEventPurge` — purge old data |

---

## Utility Controllers

| Class | Key Endpoints |
|-------|--------------|
| `SystemHealthController` | `GET /api/SystemHealth` — DB status, users, metrics |
| `MonitoredApplicationsController` | `GET /api/MonitoredApplications` — cross-app health |
| `LogViewerController` | `GET /api/LogViewer/Files`, `GET /api/LogViewer/Content` |
| `TileProxyController` | `GET /api/TileProxy` — map tile proxy |

---

## SignalR Hubs

| Class | Path | Purpose |
|-------|------|---------|
| `Hub` | `/hub` | Base real-time communication |
| `AlertHub` | `/alerthub` | Real-time alert/incident notifications |

---

## Services

| Class | Registration | Purpose |
|-------|-------------|---------|
| `AlertingIntegrationService` | `builder.Services.AddAlertingIntegration(config)` | Registers with Alerting system, sends incidents |
| `AlertingWebhookControllerBase` | Inherit in app | Base for receiving alerting webhooks |
| `MonitoredApplicationService` | Singleton | Polls monitored apps for health |
| `TileManagementService` | — | Map tile caching |

---

## Middleware

| Class | Registration | Purpose |
|-------|-------------|---------|
| `SessionValidationMiddleware` | `app.UseMiddleware<SessionValidationMiddleware>()` | Rejects revoked sessions on every request |

---

## Utility / Startup Helpers

| Method | Purpose |
|--------|---------|
| `StartupBasics.AddFoundationEssentialWebAPIControllers()` | Core auth/profile controllers |
| `StartupBasics.AddSecurityWebAPIControllers()` | Full Security module |
| `StartupBasics.AddAuditorWebAPIControllers()` | Full Auditor module |
| `StartupBasics.AddSystemHealthControllers()` | Health dashboard |
| `StartupBasics.AddMonitoredApplicationsController()` | Cross-app monitoring |
| `StartupBasics.AddFoundationAdvancedWebAPIControllers()` | Tenant settings, system settings, log viewer |
| `StartupBasics.RegisterWithAlertingAsync()` | Self-registers app with Alerting system |
| `TelemetryStartupBasics.AddTelemetryWebAPIControllers()` | Telemetry module controllers |
