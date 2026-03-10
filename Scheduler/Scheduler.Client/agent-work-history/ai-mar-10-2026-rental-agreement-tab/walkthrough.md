# Rental Agreement Tab — Walkthrough

## Summary

Added a **"Rental Agreement" tab** to the existing event add/edit modal, completing the end-to-end data entry workflow for rental agreement tracking.

## Data Flow

```
Event Modal (Rental Tab)
  ↓ writes to
attributesParsed.rentalAgreement
  ↓ serialized on save (line 898)
ScheduledEvent.attributes (JSON string in DB)
  ↓ parsed on load
Rental Agreement Tracker dashboard reads same data
```

## Changes

### Modified Files

| File | Change |
|------|--------|
| [event-add-edit-modal.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-add-edit-modal/event-add-edit-modal.component.html) | Added tab button + pane with 6 fields |
| [event-add-edit-modal.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-add-edit-modal/event-add-edit-modal.component.ts) | Added `getRentalStatus()`, `getRentalField()`, `setRentalField()` helpers |

### Rental Agreement Tab Fields

- **Agreement Status** — dropdown: None / Draft / Signed / Expired
- **Contact Name** — text (renter/org name)
- **Signed Date** — date picker
- **Expiry Date** — date picker
- **Deposit Amount** — currency input
- **Agreement Notes** — textarea

### Key Behaviors

- Setting status to "No Agreement" clears all rental data from attributes
- Tab shows colored status badge (yellow=Draft, green=Signed, red=Expired)
- Status bar at bottom of tab shows contextual message
- Available on both create and edit mode (not gated to edit-only)

## Verification

- **Build:** `npx ng build --configuration production` — **zero TypeScript errors**
