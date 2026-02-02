# On-Call Schedule Management UI - Implementation Walkthrough

## Overview

Implemented a world-class on-call schedule management UI for the Alerting module, inspired by PagerDuty, Opsgenie, and Incident.io. The implementation includes a premium listing component and a comprehensive full-page editor with visual timeline preview.

## Components Created

### 1. ScheduleManagementComponent (Listing)

**Path:** `components/schedule-management/`

A premium "ultra-tier" listing component following the established UI patterns from `ServiceManagementComponent`:

- **Teal/cyan gradient theme** - Distinct visual identity for the scheduling domain
- **Search and pagination** - Standard filtering capabilities
- **Quick-add modal** - Create new schedules inline
- **Table with status indicators** - Shows name, timezone, layers, and active status
- **Edit/Delete actions** - Navigate to full editor or soft-delete

**Files:**
- [schedule-management.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/schedule-management/schedule-management.component.ts)
- [schedule-management.component.html](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/schedule-management/schedule-management.component.html)
- [schedule-management.component.scss](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/schedule-management/schedule-management.component.scss)

---

### 2. ScheduleEditorComponent (Full-Page Editor)

**Path:** `components/schedule-editor/`

A comprehensive editor following Pattern 94 (Full-Page Editor) with:

#### Features:
- **Schedule metadata editing** - Name, description, timezone, active status
- **Visual 14-day timeline preview** - Shows on-call spans across layers
- **Layer management** - Add, remove, reorder (drag-drop), expand/collapse
- **Layer configuration** - Rotation start, rotation period (daily/weekly/etc), handoff time
- **Member management** - Add, remove, reorder (drag-drop), user selection dropdown
- **Who's on-call now** - Real-time display in header badge
- **Unsaved changes detection** - With `canDeactivate` guard integration
- **Sticky action bar** - Save/Cancel buttons always visible

#### Timeline Visualization:
- Day headers with today highlight
- Layer rows with color-coded spans
- User avatars and initials
- Hover tooltips with user/layer details
- Current time marker

**Files:**
- [schedule-editor.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/schedule-editor/schedule-editor.component.ts)
- [schedule-editor.component.html](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/schedule-editor/schedule-editor.component.html)
- [schedule-editor.component.scss](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/schedule-editor/schedule-editor.component.scss)

---

## Routing

Routes added to [app-routing.module.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/app-routing.module.ts):

```typescript
{ path: 'schedule-management', component: ScheduleManagementComponent, canActivate: [AuthGuard], title: 'Schedule Management' },
{ path: 'schedule-management/new', component: ScheduleEditorComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'New Schedule' },
{ path: 'schedule-management/:id/edit', component: ScheduleEditorComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Schedule' },
```

---

## Sidebar Navigation

Added "Schedules" link to [sidebar.component.html](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/sidebar/sidebar.component.html) with `fa-calendar-alt` icon, positioned after Escalation Policies.

---

## Data Services Used

- **OnCallScheduleService** - CRUD for schedules
- **ScheduleLayerService** - CRUD for layers
- **ScheduleLayerMemberService** - CRUD for layer members
- **AlertingUserService** - Fetch users for assignment dropdown

---

## Design Decisions

1. **Inline Timeline** - Built timeline calculation directly in the editor component rather than a separate service for simplicity and reactivity
2. **EditableLayer/EditableMember interfaces** - Track UI state (isNew, isModified, isExpanded) alongside data
3. **Client-side span calculation** - Timeline spans computed from layer configuration without server round-trips
4. **User color assignment** - Dynamic color palette cycling for timeline visualization

---

## Verification

### Build Status: ✅ SUCCESS

```
npx ng build
Exit code: 0
```

Only pre-existing warnings (optional chain operators in SystemHealthComponent, TestHarnessComponent).

---

## Next Steps

1. **Manual testing** - Create/edit flows with real API
2. **Gap detection** - Add visual warnings when no one is on-call
3. **Override management** - Allow temporary schedule overrides
4. **Integration with Escalation Policies** - Link schedules to escalation rules
