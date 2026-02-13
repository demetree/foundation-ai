# ShiftPattern Integration — Option C

## Key Discovery

> [!TIP]
> The `shiftPatternId` FK **already exists** on the Resource table (database generator line 2315) and is fully wired in the client `ResourceService` (`shiftPatternId`, `shiftPattern` nav property). No schema or migration changes needed.

The `ShiftPatternDay` fields (`shiftPatternId`, `dayOfWeek`, `startTime`, `hours`, `label`) mirror `ResourceShift` fields exactly — confirming the "template → stamp" design.

## Proposed Changes

### Phase 1: Shift Pattern Management Pages

Build custom components for managing shift patterns and their days, following the same aesthetic and patterns as the existing shift-custom and resource-custom components.

---

#### [NEW] Shift Pattern Custom Components

| Component | Purpose |
|---|---|
| `ShiftPatternCustomListingComponent` | Lists all patterns with name, color swatch, day count, resource count badges |
| `ShiftPatternCustomTableComponent` | Virtual-scroll table + mobile cards for patterns |
| `ShiftPatternCustomDetailComponent` | Detail page with tabs: Overview, Days (inline timetable), Assigned Resources |
| `ShiftPatternCustomAddEditComponent` | Modal wrapper for creating/editing patterns (name, description, color, timezone) |
| `ShiftPatternDayAddEditModalComponent` | Modal for adding/editing individual days within a pattern |

**Routes:**
- `/shiftpatterns` → Listing
- `/shiftpattern/:shiftPatternId` → Detail

**Files (15 new):** 5 components × 3 files each (`.ts`, `.html`, `.scss`)

---

#### [MODIFY] [app-routing.module.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/app-routing.module.ts)

Add imports and routes for shift pattern custom components.

#### [MODIFY] [app.module.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts)

Add declarations for all 5 new shift pattern components.

---

### Phase 2: "Apply Pattern" Action

#### [MODIFY] [resource-shift-tab.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/resource-custom/resource-shift-tab/resource-shift-tab.component.ts)

Add an "Apply Shift Pattern" button that:
1. Opens a dropdown/modal to select from available `ShiftPattern` records
2. Reads all `ShiftPatternDay` records for the chosen pattern
3. Shows a confirmation dialog (replace vs merge existing shifts)
4. Bulk-creates `ResourceShift` records, copying `dayOfWeek`, `startTime`, `hours`, `label` from each `ShiftPatternDay`
5. Updates the resource's `shiftPatternId` FK to track which pattern was applied

#### [MODIFY] [resource-shift-tab.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/resource-custom/resource-shift-tab/resource-shift-tab.component.html)

Add the "Apply Pattern" button UI next to the existing "Add Shift" button.

---

### Phase 3: ShiftPattern Display on Resource Detail

#### [MODIFY] [resource-overview-tab.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/resource-custom/resource-overview-tab/resource-overview-tab.component.ts)

Show the assigned shift pattern (with link) on the resource overview, if set.

---

## User Review Required

> [!IMPORTANT]
> **Replace vs Merge behavior**: When applying a pattern, should the default be:
> - **Replace**: Delete all existing `ResourceShift` records first, then create from pattern?
> - **Merge**: Only add missing days, skip days that already have a shift?
> - **Prompt user**: Show a dialog letting them choose?
>
> I'm leaning toward **prompt user** with Replace as the default, since applying a pattern implies the intent to standardize.

> [!IMPORTANT]
> **Phase ordering**: I recommend building Phase 1 (pattern management) first, then Phase 2 (apply action), then Phase 3 (overview display). This lets you create/view patterns before the "apply" button needs them. Want me to proceed in this order?

## Verification Plan

### Automated Tests
- `ng build --configuration development` after each phase

### Manual Verification
- Navigate to `/shiftpatterns` listing, create a pattern, add days
- Apply a pattern to a resource from the Shifts tab
- Verify resource's shift records match the pattern days
