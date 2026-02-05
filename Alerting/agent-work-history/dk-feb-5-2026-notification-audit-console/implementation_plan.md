# Notification Audit Console

Admin-facing component to browse, search, and inspect all notification deliveries with full content visibility for forensic auditing.

## Proposed Changes

### Backend - Service Layer

#### [NEW] [NotificationAuditService.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Services/NotificationAuditService.cs)

Service to aggregate notification delivery data with metrics:
- `GetDeliveryListAsync()` - Paginated, filterable delivery attempts
- `GetDeliveryDetailsAsync()` - Full delivery with content
- `GetMetricsAsync()` - KPI aggregation (delivery rate, channel breakdown, etc.)

#### [NEW] [NotificationAuditDto.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Models/NotificationAuditDto.cs)

DTOs for audit data:
- `NotificationAuditMetricsDto` - KPI summary data
- `DeliveryAttemptDetailDto` - Full delivery detail including content

---

### Backend - API Layer

#### [NEW] [NotificationAuditController.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Controllers/NotificationAuditController.cs)

Endpoints:
- `GET /api/notification-audit/deliveries` - List with filters
- `GET /api/notification-audit/deliveries/{id}` - Single delivery detail
- `GET /api/notification-audit/metrics` - KPI summary

---

### Frontend - Angular Component

#### [NEW] [notification-audit.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/notification-audit/notification-audit.component.ts)

Premium split-pane layout:
- **Left panel**: Filterable list (channel, status, date, search)
- **Right panel**: Detail view with content preview
- **Top**: Metrics cards (sent/failed today, success rate, avg latency)

Email HTML will render in a sandboxed iframe. Other channels show formatted text/JSON.

---

### Frontend - Routing

#### [MODIFY] [app-routing.module.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/app-routing.module.ts)

Add route: `/notification-audit` → `NotificationAuditComponent`

---

## Metrics/KPIs (Top Cards)

| Metric | Description |
|--------|-------------|
| **Sent Today** | Total successful deliveries in last 24h |
| **Failed Today** | Total failed deliveries in last 24h |
| **Success Rate** | Rolling 7-day success percentage |
| **Avg Latency** | Average time from escalation to delivery |
| **By Channel** | Pie/donut showing Email/SMS/Push/Voice/Teams distribution |

---

## Verification Plan

### Build Verification
```bash
dotnet build Alerting/Alerting.Server
npm run build --prefix Alerting/Alerting.Client
```

### Visual Verification
- Navigate to `/notification-audit`
- Confirm list loads with recent deliveries
- Click a row to view content
- Verify email HTML renders correctly
