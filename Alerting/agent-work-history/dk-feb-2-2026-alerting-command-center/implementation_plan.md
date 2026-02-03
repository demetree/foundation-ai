# Alerting Command Center - Implementation Plan

Premium capstone overview screen for the Alerting module, serving as the primary landing page after login.

## User Review Required

> [!IMPORTANT]
> **Landing Page Change**: This will replace the current default route (IncidentListing) with the new Command Center as the login landing page. The incident listing will remain accessible from the Command Center.

## Proposed Changes

### Backend: New Dashboard API Endpoint

#### [NEW] CommandCenter/DashboardController.cs
Create a dedicated controller to aggregate dashboard data in a single call:

```csharp
[Route("api/[controller]")]
public class DashboardController : SecureWebApiController<Dashboard>
{
    [HttpGet]
    public async Task<DashboardSummaryDto> GetDashboardSummary()
}
```

**DashboardSummaryDto** will include:
- **Incident Metrics**: Active count by status (Triggered/Acknowledged/Resolved), counts by severity, MTTA, MTTR
- **On-Call Summary**: List of schedules with current on-call user(s) per schedule
- **Recent Activity**: Last 10 incident timeline events (triggers, acks, resolves, escalations)
- **Configuration Counts**: Services, Integrations, Schedules, Policies

---

### Frontend Components

#### [NEW] alerting-overview/alerting-overview.component.ts
Main Command Center component (~500-600 lines), following Foundation Overview pattern.

**Key Features:**
| Section | Description |
|---------|-------------|
| **Hero Header** | Glassmorphic gradient header with operational status indicator (Healthy/Degraded/Critical based on active incident count) |
| **Incident Metrics Grid** | 4 stat cards: Active Incidents, Critical, Acknowledged, Resolved Today |
| **MTTA/MTTR Cards** | Performance metrics with trend arrows (improving/worsening) |
| **Who's On Call** | Visual grid showing current on-call responders per schedule |
| **Incident Feed** | Live-updating list of recent/active incidents with severity badges |
| **Quick Navigation Grid** | 6 cards linking to: Incidents, Schedules, Services, Integrations, Policies, My Shift |

#### [NEW] alerting-overview/alerting-overview.component.html
Premium glassmorphic UI following established patterns.

#### [NEW] alerting-overview/alerting-overview.component.scss
Styling with:
- Dark gradient header (`$dark-gradient`)
- Animated floating orb and pattern effects
- Glassmorphic cards with translucent backgrounds
- Pulsing animation for critical incidents
- Stat cards with gradient icons

---

### Backend Service: Dashboard Data Aggregation

#### [NEW] Services/DashboardService.cs
Aggregates data from multiple sources:

```csharp
public class DashboardService : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        // Parallel queries for performance
        var incidentCounts = GetIncidentCountsAsync();
        var onCallSummary = GetOnCallSummaryAsync();
        var recentActivity = GetRecentActivityAsync();
        var configCounts = GetConfigurationCountsAsync();
        var performanceMetrics = GetPerformanceMetricsAsync();
        
        await Task.WhenAll(...);
        return new DashboardSummaryDto { ... };
    }
}
```

**On-Call Resolution** leverages existing `EscalationService.GetCurrentOnCallUsersAsync()`.

---

### DTOs

#### [NEW] Models/DashboardSummaryDto.cs

```csharp
public class DashboardSummaryDto
{
    public OperationalStatus Status { get; set; } // Healthy, Degraded, Critical
    public IncidentMetricsDto IncidentMetrics { get; set; }
    public List<OnCallScheduleSummaryDto> OnCallSummary { get; set; }
    public List<RecentActivityDto> RecentActivity { get; set; }
    public ConfigurationCountsDto ConfigCounts { get; set; }
    public PerformanceMetricsDto Performance { get; set; }
}

public class OnCallScheduleSummaryDto
{
    public int ScheduleId { get; set; }
    public string ScheduleName { get; set; }
    public string Timezone { get; set; }
    public List<OnCallUserDto> OnCallUsers { get; set; }
}

public class PerformanceMetricsDto
{
    public decimal? MttaMinutes { get; set; }  // Mean Time to Acknowledge
    public decimal? MttrMinutes { get; set; }  // Mean Time to Resolve
    public string MttaTrend { get; set; }      // improving, worsening, stable
    public string MttrTrend { get; set; }
}
```

---

### Routing Update

#### [MODIFY] app-routing.module.ts
- Import `AlertingOverviewComponent`
- Add route: `{ path: 'overview', component: AlertingOverviewComponent, ... }`
- Update default route: `{ path: '', component: AlertingOverviewComponent, ... }`

---

### Module Registration

#### [MODIFY] app.module.ts
- Declare `AlertingOverviewComponent`
- Import `NgChartsModule` if not already present (for optional trend sparklines)

---

## Verification Plan

### Automated Tests
```bash
# Backend build
cd Alerting/Alerting.Server && dotnet build

# Frontend build  
cd Alerting/Alerting.Client && npm run build
```

### Manual Verification
1. Navigate to root URL after login → Command Center displays
2. Verify incident metrics match database counts
3. Confirm on-call users display correctly for active schedules
4. Test navigation cards link to correct components
5. Verify responsive layout on mobile viewport
6. Check operational status indicator reflects incident state:
   - 0 active = Healthy (green)
   - 1-4 active = Degraded (yellow)
   - 5+ active = Critical (red pulsing)

---

## Implementation Order

1. **Backend DTOs** - Define data contracts
2. **Backend DashboardService** - Implement data aggregation
3. **Backend DashboardController** - Expose API endpoint
4. **Frontend Component** - Build UI with service calls
5. **Routing** - Wire up as landing page
6. **Polish** - Animations, responsive design, testing
