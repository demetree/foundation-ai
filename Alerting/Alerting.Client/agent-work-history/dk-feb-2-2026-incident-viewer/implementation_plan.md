# Incident Management Dashboard

Build a premium, command-center style Incident Management UI for the Alerting module.

## Core Features

### 1. Incident Dashboard (List View)
Full-page incident listing with real-time feel:

| Feature | Description |
|---------|-------------|
| **Status Tabs** | Triggered / Acknowledged / Resolved / All |
| **Severity Filter** | Critical, High, Medium, Low badges with color coding |
| **Service Filter** | Filter by service name |
| **Search** | Quick search by title or incident key |
| **Sortable Columns** | Created, severity, status, service |
| **Pagination** | Server-side paging for performance |

**Card Layout per Incident:**
- Severity badge (color-coded: Critical=red, High=orange, Medium=yellow, Low=blue)
- Status badge (Triggered=pulsing red, Acknowledged=amber, Resolved=green)
- Incident title & key
- Service name
- Time since triggered (relative)
- Current assignee avatar (if any)
- Quick action buttons (Ack / Resolve)

---

### 2. Incident Detail View (Editor)
Full-page detail view for a single incident:

**Hero Header:**
- Severity + Status badges
- Incident title
- Service name with link
- Quick actions: Acknowledge, Resolve, Assign

**Main Content (3-column or tabbed):**

| Section | Content |
|---------|---------|
| **Overview** | Key details, source payload, created/ack/resolved timestamps |
| **Timeline** | Chronological event list with icons per event type |
| **Notes** | Add/view responder notes |
| **Notifications** | Who was notified, via which channel, delivery status |

**Timeline Events:**
- Triggered (🔴)
- Escalated (⬆️)
- Acknowledged (✅)
- Resolved (☑️)
- NoteAdded (📝)
- NotificationSent (📧)

---

### 3. Responder Actions

| Action | Behavior |
|--------|----------|
| **Acknowledge** | Sets status to Acknowledged, records timestamp |
| **Resolve** | Sets status to Resolved, records timestamp |
| **Add Note** | Adds responder note with current user as author |
| **Assign** | Sets currentAssigneeObjectGuid |

---

## Proposed Files

### New Components

| File | Purpose |
|------|---------|
| `incident-dashboard/incident-dashboard.component.ts` | Main dashboard listing |
| `incident-dashboard/incident-dashboard.component.html` | Dashboard template |
| `incident-dashboard/incident-dashboard.component.scss` | Premium styling |
| `incident-viewer/incident-viewer.component.ts` | Detail view for single incident |
| `incident-viewer/incident-viewer.component.html` | Detail template with timeline/notes |
| `incident-viewer/incident-viewer.component.scss` | Premium detail styling |

### Module Updates

| File | Change |
|------|--------|
| `app-routing.module.ts` | Add `/incidents` and `/incidents/:id` routes |
| `app.module.ts` | Register new components |
| `sidebar.component.html` | Add "Incidents" navigation link (🔔 or ⚠️) |

---

## UI/UX Design

**Color Scheme:**
- **Gradient Header:** Deep red/orange for urgency (`#dc2626` → `#ea580c`)
- **Dark Background:** `#0f172a` → `#1e293b`
- **Glassmorphism Cards:** `rgba(255,255,255,0.05)` with blur

**Severity Colors:**
- Critical: `#ef4444` (red)
- High: `#f97316` (orange)
- Medium: `#eab308` (yellow)
- Low: `#3b82f6` (blue)

**Status Badges:**
- Triggered: Pulsing red badge
- Acknowledged: Amber badge
- Resolved: Green badge with checkmark

---

## Verification Plan

### Build Verification
```powershell
npm run build
```

### Manual Testing
1. Navigate to `/incidents`
2. Verify status tabs filter correctly
3. Click an incident to view detail
4. Test Acknowledge and Resolve actions
5. Add a note and verify it appears in timeline
6. Check responsive behavior

---

## Implementation Order

1. **Phase 1:** Incident Dashboard (list view with filters)
2. **Phase 2:** Incident Viewer (detail view with timeline/notes)
3. **Phase 3:** Responder Actions (acknowledge, resolve, add note)
4. **Phase 4:** Polish (animations, responsive, accessibility)
