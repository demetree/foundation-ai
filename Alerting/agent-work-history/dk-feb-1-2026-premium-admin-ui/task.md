# Alerting Module Development

## Phase 1: Database & EF Core Setup ✅
- [x] Review/finalize schema with multi-tenancy and user preferences
- [x] Scaffold database and EF entities
- [x] Generate Alerting.Server with DataControllers
- [x] Generate Alerting.Client with data services

---

## Phase 2: Core Alerting Services ✅
- [x] Implement `AlertingService` - incident lifecycle (trigger, ack, resolve)
- [x] Implement `EscalationService` - rule processing and target resolution
- [ ] Implement `OnCallScheduleService` (basic logic in EscalationService)
- [ ] Implement `NotificationService` - channel delivery (stubbed)
- [ ] Implement `WebhookService` - outbound callbacks (stubbed)
- [x] Implement `AlertingMetricsProvider` - active incidents for System Health

---

## Phase 3: REST API Controllers ✅
- [x] Create `AlertsController` - inbound `/api/alerts/v1/enqueue` endpoint
- [x] Create `IncidentController` - ack/resolve/notes/stats endpoints
- [x] Wire up EscalationWorker as background service

---

## Phase 5.1: Angular Test Harness UI ✅
  - [x] Create `AlertTestHarnessService` with DTOs
  - [x] Create `TestHarnessComponent` (TypeScript)
  - [x] Create test harness HTML template
  - [x] Create test harness SCSS styling
  - [x] Register in `app.module.ts` and `app-routing.module.ts`
  - [x] Verify Angular build passes

---

## Phase 5.2: Integration Management UI ✅
- [x] Create `IntegrationManagementComponent` - combined list/modal CRUD
- [x] Add API key reveal/copy functionality
- [x] Register in module and routing
- [x] Add to sidebar navigation
- [x] Verify build passes

## Phase 4: Angular Dashboard
- [ ] Create `OverviewComponent` - active incidents summary
- [ ] Create `IncidentListComponent` - filterable incident listing
- [ ] Create `IncidentDetailComponent` - timeline, notes, actions
- [ ] Add to sidebar navigation
- [ ] Admin screens for Services, Escalation Policies, Schedules
