# Foundation — Getting Started

Setup guide for the Foundation administrative console.

---

## Prerequisites

Same as Scheduler — see [Scheduler Getting Started](file:///d:/source/repos/scheduler/Scheduler/docs/getting-started.md) for .NET, Node.js, Angular CLI, SQL Server, and Git long-path requirements.

---

## Database Setup

Foundation uses **three databases** (no application-specific database of its own):

| Database | Purpose |
|----------|---------|
| `Security` | User accounts, roles, tenants, OIDC clients, sessions |
| `Auditor` | Access audit events |
| `Telemetry` | Historical telemetry metrics |

These are the **same Security and Auditor databases** used by Scheduler and Alerting. Telemetry is specific to Foundation.

---

## Server Setup

### 1. Create `appsettings.development.json`

```powershell
cd Foundation\Foundation.Server
copy appsettings.development.json.example appsettings.development.json
```

Key settings:
- **`ConnectionStrings`** — Security, Auditor, Telemetry (default: `LOCALHOST` with Integrated Security)
- **`Kestrel.Endpoints`** — HTTP `9100`, HTTPS `9101`
- **`OIDC`** — PEM certificate paths in `Certificates/`
- **`MonitoredApplications`** — list of apps to monitor (Foundation, Scheduler, Alerting with URLs)
- **`LogViewer.LogFolders`** — paths to log directories for cross-app log viewing
- **`Telemetry`** — collection interval, retention days, list of apps to collect from

### 2. Build and Run

```powershell
cd Foundation\Foundation.Server
dotnet run
```

The server starts on `https://localhost:9101`. Swagger UI available at `/swagger` in Development mode.

---

## Client Setup

```powershell
cd Foundation\Foundation.Client
npm install
npx ng serve
```

The Angular dev server serves at the configured port with live reload.

---

## Default Ports

| Application | HTTP | HTTPS |
|------------|------|-------|
| Foundation | 9100 | 9101 |
| Scheduler | 10100 | 10101 |
| Alerting | 11100 | 11101 |
