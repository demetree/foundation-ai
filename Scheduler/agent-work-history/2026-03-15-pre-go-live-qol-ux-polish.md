# PHMC Scheduler — Pre-Go-Live QoL & UX Polish

**Date:** 2026-03-15

## Summary

Conducted a comprehensive pre-go-live audit of the PHMC Scheduler from the perspective of Denise (recreation committee coordinator), then implemented all identified improvements across two phases: (1) booking contact fields + print confirmation, and (2) 5 UX polish recommendations.

## Changes Made

### Phase 1 — Booking Contact Fields & Print Confirmation (8 files)

**Database / Server:**
- `SchedulerDatabaseGenerator/SchedulerDatabaseGenerator.cs` — 3 new field definitions on ScheduledEvent table (bookingContactName, bookingContactEmail, bookingContactPhone)
- `SchedulerDatabase/Database/ScheduledEvent.cs` — 3 new entity properties
- `SchedulerTools/Program.cs` — Data loader populates dedicated fields instead of stuffing contact info into notes

**Angular Client:**
- `scheduled-event.service.ts` — 3 fields added to QueryParameters, SubmitData, and Data classes
- `event-add-edit-modal.component.ts` — Form controls, populate/submit wiring, `printBookingSummary()` method
- `event-add-edit-modal.component.html` — Booking Contact section in Details tab, Print button in footer
- `scheduled-event-add-edit.component.ts` — Auto-generated form updated with 3 fields
- `scheduled-event-detail.component.ts` — Auto-generated form updated with 3 fields

### Phase 2 — UX Polish (4 files)

**Calendar (`scheduler-calendar.component.ts`):**
- `scrollTime: '08:00:00'` — Auto-scroll to business hours
- Smart status-based color palette in `getEventColor()` (Confirmed=green, Tentative=amber, Cancelled=red, In Progress=blue, Planned=violet, Completed=gray, Draft=neutral)
- Mobile day view default (`timeGridDay` when viewport < 768px)

**Sidebar (`sidebar.component.ts`, `.html`, `.scss`):**
- Collapsible Operations section grouping Crews, Volunteers, Volunteer Groups, Shifts, Shift Patterns
- Chevron toggle animation, nested indentation styles

## Key Decisions

- Booking contact fields are nullable strings (not FK to Contact) for lightweight entry
- Status palette uses `includes()` matching for flexible naming conventions
- Operations section remains accessible in collapsed sidebar (icon-only) but grouped when expanded
- Rec Committee dashboard tab already had tailored widgets — no changes needed

## Testing / Verification

- **dotnet build** — 0 errors
- **ng build --configuration=production** — 0 new errors from our changes
- **Browser testing** — Desktop, tablet (768x1024), phone (375x667): booking contact fields, data persistence, and print button all verified
