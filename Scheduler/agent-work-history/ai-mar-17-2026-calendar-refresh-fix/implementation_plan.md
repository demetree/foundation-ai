# Simple Mode for Scheduler

Add a **hierarchical Simple / Advanced mode** that hides power-user features, giving small-town coordinators a focused UI while preserving the full system for advanced users.

## Design Principles

- **Hierarchical mode** — a **global** simple/advanced toggle sets the system-wide default. Individual components (event editor, sidebar sections, etc.) can be **overridden** independently so a user can stay in simple mode overall but unlock advanced features for specific areas as they grow.
- **Per-user setting** — stored via `UserSettingsService`. Global key: `scheduler.uiMode` (`simple` | `advanced`). Override keys: `scheduler.uiMode.eventEditor`, `scheduler.uiMode.sidebar`, etc. Missing override = inherit global.
- **Default: Simple** — new users start in simple mode.
- **No data loss** — switching modes is purely visual. All entity fields are still saved/loaded; hidden fields retain their values.

---

## Proposed Changes

### Mode Infrastructure

#### [NEW] [scheduler-mode.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/services/scheduler-mode.service.ts)

Centralized service that manages the hierarchical mode:

```typescript
// Core API surface:
globalMode$: BehaviorSubject<'simple' | 'advanced'>

isSimpleMode(component?: string): Observable<boolean>  // checks override, falls back to global
isAdvancedMode(component?: string): Observable<boolean>

setGlobalMode(mode): void         // persists via UserSettingsService
setComponentOverride(component, mode | null): void  // null = clear override, inherit global
```

Component keys: `'eventEditor'`, `'sidebar'`, `'overview'`, `'recurrence'`

---

### Sidebar

#### [MODIFY] [sidebar.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/sidebar/sidebar.component.html)
#### [MODIFY] [sidebar.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/sidebar/sidebar.component.ts)

| Simple Mode Shows | Simple Mode Hides |
|-------------------|-------------------|
| Overview | Volunteers group (2 items) |
| Schedule | Setup group: Clients, Targets, Resources, Crews, Templates, Shifts, Shift Patterns, Rate Sheets, Offices |
| Contacts | Administration |
| Finances | |

Add a **mode toggle widget** at the bottom of the sidebar — a subtle pill switch labelled "Simple" / "Advanced" with an icon. Advanced mode reveals all items.

---

### Event Add/Edit Modal

#### [MODIFY] [event-add-edit-modal.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-add-edit-modal/event-add-edit-modal.component.html)
#### [MODIFY] [event-add-edit-modal.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-add-edit-modal/event-add-edit-modal.component.ts)

**Simple mode shows 2 tabs (vs 7):**

| Tab | What's Visible |
|-----|---------------|
| **Details** | Name, Start/End (with duration pills), All-Day, Location, Booking Contact, Description |
| **Recurrence** | Simplified version (see below) |

**Hidden in Simple mode:** Status, Priority, Color, Scheduling Target, Client, Office, Booking Source, Calendars, Dynamic Attributes, Notes, Template Picker. Tabs hidden: Assignments, Advanced, Dependencies, Financials, Rental Agreement.

---

### Recurrence Builder

#### [MODIFY] [recurrence-builder.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/recurrence-builder/recurrence-builder.component.html)
#### [MODIFY] [recurrence-builder.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/recurrence-builder/recurrence-builder.component.ts)

Accepts a new `@Input() simpleMode: boolean` to simplify itself.

| Feature | Simple | Advanced |
|---------|--------|----------|
| Frequencies | Daily, Weekly only | Daily, Weekly, Monthly, Yearly |
| Weekly day picker | ✅ | ✅ |
| Monthly/Yearly options | Hidden | ✅ |
| End: Never | ✅ | ✅ |
| End: On date | ✅ | ✅ |
| End: After N occurrences | Hidden | ✅ |
| Interval ("every N weeks") | Hidden (defaults to 1) | ✅ |
| Preview summary | ✅ | ✅ |
| Exceptions (edit mode) | Hidden | ✅ |

---

### Overview Dashboard

#### [MODIFY] [overview.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/overview/overview.component.html)

| Simple Mode | Advanced Mode |
|-------------|--------------|
| Header: "Events Today" + "Running Now" only | Full stat bar |
| Overview tab only | All 4 tabs (Overview, Manager, Dispatcher, Scheduler) |
| Overview tab: "Today at a Glance" + "Week Forecast" + "Quick Access" | Full content including Activity/Resources cards |

---

## File Summary

| File | Action | What Changes |
|------|--------|-------------|
| `scheduler-mode.service.ts` | **NEW** | Hierarchical mode service |
| `sidebar.component.ts` + `.html` | MODIFY | Inject mode service, hide items, add toggle widget |
| `event-add-edit-modal.component.ts` + `.html` | MODIFY | Hide tabs and fields |
| `recurrence-builder.component.ts` + `.html` | MODIFY | Accept `simpleMode` input, hide advanced options |
| `overview.component.ts` + `.html` | MODIFY | Hide tabs and cards |

Total: **1 new file, 8 modified files** (4 components × 2 files each)

---

## Verification Plan

### Manual Verification

1. `cd Scheduler\Scheduler.Client && npx ng build` — confirm no build errors
2. Log in → confirm Simple mode defaults (3 sidebar items, simplified event editor, 1 overview tab)
3. Toggle to Advanced → confirm full UI appears
4. Toggle back to Simple → confirm clean hide
5. Refresh page → confirm mode persists
6. Create an event in Advanced with all fields → switch to Simple → edit same event → save → switch to Advanced → confirm all fields intact
7. Test recurrence in Simple: create a weekly recurring event → confirm only Daily/Weekly available
8. Test component override: stay in Simple global, unlock Advanced for the event editor → confirm sidebar stays simple but event editor shows all tabs
