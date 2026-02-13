# Foundation — Key Concepts

Core concepts and terminology for the Foundation platform modules.

---

## Security Module

### Users

Every person who logs in is a **SecurityUser**.  Users have:
- **Username** and **email** (both unique)
- **Tenant** membership — users belong to exactly one tenant
- **Security roles** — mapped via `SecurityUserSecurityRole`
- **Departments** and **Teams** — organizational grouping
- **Session tracking** — active sessions can be viewed and revoked

### Tenants

Multi-tenancy is enforced at the data level.  Each tenant:
- Has its own **tenant profile** with settings
- Sees only its own data across all Foundation apps
- Can have **data visibility groups** for finer-grained access within the tenant

### Roles & Privileges

Foundation uses a layered permission model:

| Level | Description |
|-------|-------------|
| **Security Role** | Named collection of privilege settings (e.g., "Administrator", "ReadOnly") |
| **Module Privilege** | Per-module access: No Access, Anonymous Read Only, Read Only, Read/Write, Administrative, Custom |
| **Table Access** | Numeric read (0–255) and write (0–255) levels per table |

### OIDC / Authentication

Foundation implements its own **OAuth2 / OpenID Connect** server via **OpenIddict**:
- Password flow for first-party clients (Scheduler, Alerting, Foundation)
- External login support for Google, Facebook, Microsoft, Twitter
- JWT Bearer tokens with configurable PEM signing certificates
- Each Foundation app registers itself as an OIDC client on startup

### Sessions

- Active sessions are tracked in the Security database
- Sessions can be viewed and revoked via the Foundation UI
- `SessionValidationMiddleware` checks session validity on every request
- Session expiry triggers client-side re-login modals

---

## Auditor Module

The Auditor records **audit events** for security-relevant actions:
- User logins, logouts, failed login attempts
- Data reads and writes (configurable)
- Administrative actions (user creation, role changes)

Audit events include:
- **Entity states** — before/after snapshots of changed data
- **Error messages** — for failed operations
- **Purge support** — old audit data can be purged via `AuditEventPurgeController`

Auditor mode is configurable (`DispatchToBackgroundImmediately` for async logging).

---

## Telemetry Module

Foundation hosts a **TelemetryCollectorService** that periodically collects metrics from all configured applications:

| Setting | Default | Purpose |
|---------|---------|---------|
| `CollectionIntervalMinutes` | 1 | How often to collect |
| `RetentionDays` | 90 | How long to keep history |
| `CollectAuditErrors` | true | Include audit error counts |
| `CollectLogErrors` | true | Include log error counts |

The telemetry dashboard visualizes these metrics over time.

---

## System Health

Foundation provides a **System Health** dashboard that monitors:
- Database connectivity and health for all registered contexts
- Authenticated user counts
- Application-specific metrics (via `IApplicationMetricsProvider`)
- Cross-application health checks for all `MonitoredApplications`

Each application registers health endpoints that Foundation polls.

---

## Cross-App Monitoring

Foundation can monitor all Foundation-based applications:

```json
"MonitoredApplications": [
    { "Name": "Foundation", "Url": "https://localhost:9101", "IsSelf": true },
    { "Name": "Scheduler", "Url": "https://localhost:10101" },
    { "Name": "Alerting", "Url": "https://localhost:11101" }
]
```

The **Systems Dashboard** and **Log Viewer** aggregate data across all configured applications.
