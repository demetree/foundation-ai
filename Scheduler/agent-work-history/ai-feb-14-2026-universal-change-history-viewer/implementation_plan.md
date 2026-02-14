# Universal Change History Viewer — Implementation Plan

## Problem

The Foundation generates `*-change-history-table` components for every entity, but they render **raw data**: numeric `userId` values and JSON blobs for `data`. They're unusable for end users. Meanwhile, only 4 of 9 custom detail components have any history integration at all, and those use duplicated custom code.

## Solution

Create a single **`ChangeHistoryViewerComponent`** that:
1. Uses the `AuditHistory` endpoint (returns `VersionInformation<T>[]` with resolved user names)
2. Renders a polished timeline with expandable version-to-version diffs
3. Can be dropped into any entity's History tab with minimal wiring

---

## Component Design

### API

```typescript
// Inputs
@Input() auditHistory: any[] | null = null;   // VersionInformation<T>[]
@Input() isLoading: boolean = false;
@Input() entityName: string = '';              // For display ("Contact", "Resource", etc.)
@Input() excludeFields: string[] = [];         // Fields to hide from diff (e.g., 'objectGuid', 'attributes')
@Input() fieldLabels: { [key: string]: string } = {}; // Override display names (e.g., { 'contactTypeId': 'Contact Type' })
```

### Rendering

Each history entry renders as a **timeline item**:

```
┌─────────────────────────────────────────────────┐
│ ● Version 3                        2 days ago   │
│   by John Smith                                 │
│   ▸ 3 fields changed                           │
│   ┌─────────────────────────────────────────┐   │
│   │ Name:     "Old Value" → "New Value"     │   │
│   │ Status:   true → false                  │   │
│   │ Notes:    (changed)                     │   │
│   └─────────────────────────────────────────┘   │
├─────────────────────────────────────────────────┤
│ ● Version 2                        5 days ago   │
│   by Admin                                      │
│   ▸ Initial creation                            │
└─────────────────────────────────────────────────┘
```

### Diff Logic

The component compares adjacent versions in the `auditHistory` array (which includes `data: T` snapshots). For each version:
- Compare `data` of version N with version N-1
- Identify changed fields (skip `id`, `objectGuid`, `versionNumber`, and anything in `excludeFields`)
- Display old → new values for simple fields
- For long strings (>100 chars), show "(changed)" with expandable view
- For null → value transitions, show "— → New Value"
- Version 1 shows "Initial creation" (no diff available)

> [!NOTE]
> The `data: T` field is populated when `includeData: true` is passed to `Get*AuditHistory()`. All existing usages (Shift, ShiftPattern) already pass `true` or get data by default.

---

## File Structure

### [NEW] [change-history-viewer.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/change-history-viewer/change-history-viewer.component.ts)
- Standalone-style component in `shared/`
- Takes pre-fetched `auditHistory` array as input (parent is responsible for calling the API)
- Computes diffs between adjacent versions on data change
- Handles loading, empty, and populated states

### [NEW] [change-history-viewer.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/change-history-viewer/change-history-viewer.component.html)
- Timeline layout with Bootstrap-compatible classes
- Expandable diff cards per version
- Responsive design (works in both tabs and modals)

### [NEW] [change-history-viewer.component.scss](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/change-history-viewer/change-history-viewer.component.scss)
- Timeline styling (vertical line, dots, cards)
- Diff highlighting (green for additions, red for removals)

---

## Integration Pattern

Each parent component will:

```typescript
// In component TS
public auditHistory: any[] | null = null;
public isLoadingHistory = false;

loadHistory(): void {
    if (this.auditHistory != null) return;
    this.isLoadingHistory = true;
    this.entityService.Get*AuditHistory(this.entityId, true).subscribe({
        next: (data) => { this.auditHistory = data || []; this.isLoadingHistory = false; },
        error: () => { this.auditHistory = []; this.isLoadingHistory = false; }
    });
}

onTabChange(event: any): void {
    if (event?.nextId === 'history') this.loadHistory();
}
```

```html
<!-- In template, inside a History tab -->
<app-change-history-viewer
    [auditHistory]="auditHistory"
    [isLoading]="isLoadingHistory"
    [entityName]="'Contact'"
    [excludeFields]="['objectGuid', 'avatarData']">
</app-change-history-viewer>
```

---

## Integration Targets

### Adding History Tabs (5 components)
| Component | Service Method | Notes |
|-----------|---------------|-------|
| Resource | `ResourceService.GetResourceAuditHistory()` | Already shows version badge |
| Office | `OfficeService.GetOfficeAuditHistory()` | Already shows version badge |
| Client | `ClientService.GetClientAuditHistory()` | Already shows version badge |
| Crew | `CrewService.GetCrewAuditHistory()` | Already shows version badge |
| Calendar | `CalendarService.GetCalendarAuditHistory()` | Already shows version badge |

### Replacing Custom Implementations (4 components)
| Component | Current Approach | Change |
|-----------|-----------------|--------|
| Shift | Custom `loadChangeHistory()` + inline list | Replace template with `<app-change-history-viewer>` |
| ShiftPattern | Custom `loadHistory()` + inline timeline | Replace template with `<app-change-history-viewer>` |
| SchedulingTarget | Auto-generated `<app-scheduling-target-change-history-table>` | Replace with `<app-change-history-viewer>` |
| Contact | TODO placeholder alert | Wire "View Full History" to show component (in a dedicated tab or modal) |

---

## Verification Plan

### Build
- `ng build --configuration=development` must pass with no new errors

### Visual Inspection
- Use browser to verify rendering on at least one component with actual data
