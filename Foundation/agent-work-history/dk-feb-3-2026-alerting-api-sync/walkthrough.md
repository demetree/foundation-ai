# Walkthrough: Foundation Incidents Report Screen

## Summary
Created a new Incidents Report screen in the Foundation Admin client to display incidents from the Alerting system. Includes a "Test Integration" feature to verify Alerting connectivity by raising a low-severity test incident.

---

## New Files

### Backend
| File | Purpose |
|------|---------|
| [IncidentsController.cs](file:///g:/source/repos/Scheduler/Foundation/Foundation.Server/Controllers/IncidentsController.cs) | API endpoints for incidents |

### Frontend
| File | Purpose |
|------|---------|
| [incidents.service.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/services/incidents.service.ts) | HTTP service with DTOs |
| [incidents-report.component.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/incidents-report/incidents-report.component.ts) | Component logic |
| [incidents-report.component.html](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/incidents-report/incidents-report.component.html) | Template |
| [incidents-report.component.scss](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/incidents-report/incidents-report.component.scss) | Styles |

---

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/incidents` | List incidents with filtering |
| POST | `/api/incidents/test` | Raise test incident to verify connectivity |

---

## Features

- **Hero Header**: Alerting-themed gradient (red/orange/yellow) with glassmorphism
- **Statistics Cards**: Triggered, Acknowledged, Resolved, Total counts
- **Filters**: Time range, status, severity dropdowns
- **Auto-Refresh**: Toggle with countdown (30s interval)
- **Test Integration**: Raises "Info" severity test incident with dedup key
- **Graceful Degradation**: Shows message when Alerting not configured

---

## Test Integration Logic

The "Test Integration" button:
1. Raises an `Info` severity incident with title `[TEST] Foundation Integration Test`
2. Uses deduplication key `foundation-test-{date}` to prevent spam
3. Shows success/failure alert with 5-second auto-dismiss
4. Refreshes incident list on success

---

## Modified Files

| File | Changes |
|------|---------|
| [app-routing.module.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/app-routing.module.ts) | Added `/incidents` route |
| [app.module.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/app.module.ts) | Registered component and service |
| [sidebar.component.html](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/sidebar/sidebar.component.html) | Added "Incidents" menu item |
| [IAlertingIntegrationService.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/Alerting/IAlertingIntegrationService.cs) | Added `IsRegistered` property |
| [AlertingIntegrationService.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Services/Alerting/AlertingIntegrationService.cs) | Fixed `IsConfigured` logic, added `IsRegistered` |
