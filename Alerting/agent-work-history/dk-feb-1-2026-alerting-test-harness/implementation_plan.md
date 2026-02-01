# Alerting Module Implementation Plan

## Current State

**Phase 1 Complete ✅**
- `AlertingDatabase` - 32 EF entities scaffolded
- `Alerting.Server` - 30 DataControllers for CRUD
- `Alerting.Client` - Generated data services and components

**Remaining Work:**
- Core alerting services (trigger, escalate, notify)
- Inbound alert REST API
- Angular dashboard (Overview, Incident List, Admin)

---

## Proposed Changes

### Phase 2: Core Services

#### [NEW] [Services/IAlertingService.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/IAlertingService.cs)

Incident lifecycle management:

```csharp
public interface IAlertingService
{
    Task<Incident> TriggerAsync(string integrationKey, AlertPayload payload);
    Task<Incident> AcknowledgeAsync(int incidentId, Guid actorObjectGuid);
    Task<Incident> ResolveAsync(int incidentId, Guid actorObjectGuid);
    Task<List<Incident>> GetActiveIncidentsAsync(Guid tenantGuid, int? serviceId = null);
}
```

---

#### [NEW] [Services/EscalationWorker.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/EscalationWorker.cs)

Recurring job (30-second interval) that:
1. Queries incidents where `nextEscalationAt <= NOW` AND status = Triggered
2. Resolves target users via `OnCallScheduleService`
3. Creates `IncidentNotification` + `NotificationDeliveryAttempt` records
4. Advances escalation rule or loops if `repeatCount > 0`
5. Triggers webhook callbacks

---

#### [MODIFY] [Services/AlertingMetricsProvider.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/AlertingMetricsProvider.cs)

Uncomment and implement real metrics:
- **Active Incidents by Severity** (Critical/High/Medium/Low counts)
- **MTTR** (Mean Time To Resolve) - last 24 hours
- **Triggered → Acknowledged** - average response time

---

### Phase 3: REST API

#### [NEW] [Controllers/AlertsController.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Controllers/AlertsController.cs)

| Endpoint | Auth | Description |
|----------|------|-------------|
| `POST /api/alerts/v1/enqueue` | API Key | Trigger incident from external system |
| `POST /api/alerts/v1/resolve/{key}` | API Key | Programmatically resolve by incidentKey |

---

#### [NEW] [Controllers/IncidentController.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Controllers/IncidentController.cs)

| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET /api/incidents` | OIDC | List with filters (status, service, severity) |
| `GET /api/incidents/{id}` | OIDC | Details with timeline |
| `POST /api/incidents/{id}/acknowledge` | OIDC | Acknowledge |
| `POST /api/incidents/{id}/resolve` | OIDC | Resolve |
| `POST /api/incidents/{id}/notes` | OIDC | Add note |

---

### Phase 4: Angular Dashboard

#### [NEW] [components/overview/](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/overview/)

**Command Center pattern:**
- Hero header with active incident count badge
- 4 stat cards (Critical/High/Medium/Low)
- Recent incidents mini-table (last 10)
- Quick actions (Ack All, Filter by Service)

---

#### [NEW] [components/incidents/](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/incidents/)

- `incident-list.component` - Filterable table with status badges
- `incident-detail.component` - Timeline view, notes, Ack/Resolve buttons

---

#### [NEW] [components/admin/](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/admin/)

- Services CRUD
- Escalation Policies CRUD with rule builder
- On-Call Schedules with layer/member management
- Integrations with API key generation

---

## Implementation Order Options

> [!IMPORTANT]
> **Choose your priority:**

| Option | Focus | Deliverable |
|--------|-------|-------------|
| **A: Backend First** | Core services → API → UI | Working webhook-capable alerting engine |
| **B: Dashboard First** | Overview → Incident screens → Admin | Visual incident management (manual CRUD) |
| **C: Vertical Slice** | One service → full stack | End-to-end flow for single incident |

**My recommendation: Option A** (Backend First) - the escalation engine is the core value. Dashboard can use generated data components initially.

---

## Verification Plan

1. **Backend**: POST test alert → verify Incident created → observe escalation worker → check webhook delivery
2. **Dashboard**: Navigate to Overview → see incident counts → view incident detail → Ack/Resolve
