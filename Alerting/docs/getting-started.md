# Alerting — Getting Started

Setup guide for the Alerting incident management system.

---

## Prerequisites

Same as the broader Foundation platform — see [Scheduler Getting Started](file:///d:/source/repos/scheduler/Scheduler/docs/getting-started.md) for .NET 10, Node.js, Angular CLI, SQL Server, and Git long-path requirements.

---

## Database Setup

Alerting uses **three databases**:

| Database | Purpose |
|----------|---------|
| `Alerting` | Incidents, escalation policies, integrations, notifications, on-call schedules |
| `Security` | Shared — user accounts, roles, tenants, OIDC (same as Foundation & Scheduler) |
| `Auditor` | Shared — access audit events |

---

## Server Setup

### 1. Create `appsettings.development.json`

```powershell
cd Alerting\Alerting.Server
copy appsettings.development.json.example appsettings.development.json
```

Key settings to configure:

| Section | Setting | Purpose |
|---------|---------|---------|
| `ConnectionStrings` | `Alerting`, `Security`, `Auditor` | Database connection strings |
| `Kestrel.Endpoints` | HTTP `11100`, HTTPS `11101` | Server ports |
| `OIDC` | `Certificate`, `Key` | PEM certificate paths in `Certificates/` |
| `Settings.SendGridAPIKey` | API key | Email notification delivery |
| `Twilio` | `AccountSid`, `AuthToken`, `FromNumber` | SMS and voice call notifications |
| `Firebase` | `CredentialPath` | Push notification delivery |
| `NotificationEngine` | Various | Escalation intervals, retry settings |

### 2. Notification Engine Configuration

```json
"NotificationEngine": {
    "EscalationWorkerIntervalSeconds": 10,
    "RetryWorkerIntervalSeconds": 60,
    "MaxRetryAttempts": 3,
    "RetryBackoffMinutes": [1, 5, 15],
    "LogLevel": "Debug",
    "DisableLogThrottling": true
}
```

### 3. Build and Run

```powershell
cd Alerting\Alerting.Server
dotnet run
```

The server starts on `https://localhost:11101`. Swagger UI is available at `/swagger` in Development mode.

---

## Client Setup

```powershell
cd Alerting\Alerting.Client
npm install
npx ng serve
```

---

## Default Ports

| Application | HTTP | HTTPS |
|------------|------|-------|
| Foundation | 9100 | 9101 |
| Scheduler | 10100 | 10101 |
| **Alerting** | **11100** | **11101** |
