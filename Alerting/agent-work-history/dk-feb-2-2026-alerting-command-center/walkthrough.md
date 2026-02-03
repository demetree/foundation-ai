# Alerting Command Center - Implementation Walkthrough

## Overview

Implemented a premium **Alerting Command Center** dashboard that serves as the landing page after login. The screen provides a holistic view of the alerting system with real-time operational status, incident metrics, on-call information, and quick navigation.

## Backend Implementation

### DTOs Created
New file: [DashboardSummaryDto.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Models/DashboardSummaryDto.cs)

- `DashboardSummaryDto` - Main response object aggregating all dashboard data
- `OperationalStatus` - Enum (Healthy, Degraded, Critical)
- `IncidentMetricsDto` - Active, triggered, acknowledged, resolved counts with severity breakdown
- `OnCallScheduleSummaryDto` & `OnCallUserDto` - Current on-call users per schedule
- `RecentActivityDto` - Latest incident timeline events
- `ConfigurationCountsDto` - Services, integrations, schedules, policies counts
- `PerformanceMetricsDto` - MTTA/MTTR with trend analysis

### Dashboard Service
New files:
- [IDashboardService.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/IDashboardService.cs)
- [DashboardService.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/DashboardService.cs)

Key features:
- Parallel data aggregation for performance
- MTTA/MTTR calculation over last 7 days
- Trend analysis comparing current vs previous 7-day periods
- Integration with `EscalationService` for on-call resolution

### Dashboard Controller
New file: [DashboardController.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Controllers/DashboardController.cs)

- `GET /api/Dashboard/summary` - Returns aggregated dashboard data
- Secured with `[Authorize]` attribute

### DI Registration
Updated [Program.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Program.cs):
- Added `IDashboardService` → `DashboardService` scoped registration
- Registered `DashboardController` in controllers list

---

## Frontend Implementation

### AlertingOverviewComponent
New files in `Alerting.Client/src/app/components/alerting-overview/`:
- [alerting-overview.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/alerting-overview/alerting-overview.component.ts)
- [alerting-overview.component.html](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/alerting-overview/alerting-overview.component.html)
- [alerting-overview.component.scss](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/alerting-overview/alerting-overview.component.scss)

### UI Features

| Section | Description |
|---------|-------------|
| **Hero Header** | Status-based gradient (Healthy=green, Degraded=amber, Critical=red) with floating orb effect |
| **Incident Metrics** | 4-card grid showing active, critical, high, and resolved counts with pulsing animation for active |
| **Performance** | MTTA/MTTR cards with trend indicators (improving ↓, worsening ↑, stable —) |
| **Who's On Call** | Current on-call users per schedule with avatar and layer info |
| **Recent Activity** | Timeline of latest incident events with severity badges |
| **Quick Navigation** | 6-card grid for Incidents, Schedules, Services, Integrations, Policies, My Shift |
| **Configuration** | Counts for services, integrations, schedules, policies |

### Component Features
- **30-second auto-refresh** using RxJS timer
- **Manual refresh** button in header
- **Context-aware back navigation** using `NavigationService`
- **Severity-based styling** for activity items
- **Trend visualization** for MTTA/MTTR metrics

### Styling
Premium glassmorphic design with:
- Dark gradient background (`#1e293b` → `#0f172a`)
- Glass effect cards (`rgba(255,255,255,0.08)` with subtle borders)
- Status-based header gradients
- Pulsing animation for active incidents
- Smooth hover transitions

### Module & Routing Updates
- Registered in [app.module.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/app.module.ts)
- Updated [app-routing.module.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/app-routing.module.ts) to make Command Center the default landing page

---

## Build Verification

✅ **Angular build successful** (exit code 0)

```
npx ng build --configuration development
```

Backend build was blocked by file locks from running Visual Studio/Server processes (not code errors).

---

## Files Changed

### New Files
| File | Purpose |
|------|---------|
| `DashboardSummaryDto.cs` | DTOs for dashboard API response |
| `IDashboardService.cs` | Service interface |
| `DashboardService.cs` | Data aggregation implementation |
| `DashboardController.cs` | REST API endpoint |
| `alerting-overview.component.ts` | Angular component logic |
| `alerting-overview.component.html` | Template with UI sections |
| `alerting-overview.component.scss` | Premium glassmorphic styling |

### Modified Files
| File | Change |
|------|--------|
| `Program.cs` | Added DI registration |
| `app.module.ts` | Registered component |
| `app-routing.module.ts` | Updated default route |
