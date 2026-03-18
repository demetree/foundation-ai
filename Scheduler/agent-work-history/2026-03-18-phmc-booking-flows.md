# PHMC Booking Flows — EventType Entity & Purpose-Built Booking Components

**Date:** 2026-03-18

## Summary

Designed and implemented purpose-built booking flows for the PHMC (Petty Harbour) recreation committee. Created the `EventType` entity with 7 requirement flags, seeded PHMC-specific data, generated the full API/service stack, and built two streamlined Simple Mode booking components plus a calendar booking type chooser. Follow-up session added theming system integration, facility resource picker, EventType admin link, and build fix.

## Changes Made

### Schema & Seed Data
- `SchedulerDatabaseGenerator.cs` — Added `EventType` table with fields: `requiresRentalAgreement`, `requiresExternalContact`, `requiresPayment`, `requiresDeposit`, `requiresBarService`, `allowsTicketSales`, `isInternalEvent`, `defaultPrice`, `defaultChargeTypeId`. Added `eventTypeId` FK to `ScheduledEvent`.
- `SchedulerTools/Program.cs` — Seeded in `ConfigurePettyHarbour()`: Facility ResourceType, Petty Harbour Recreation Centre Resource, Hall Rental ChargeType, 7 EventType records (Birthday, Wedding, Bridal Shower, Private Event, Community Event, Fundraiser, Special Event).

### Code Generation (run by user)
- EventType entity, controller, DTOs, Angular data service with caching — all generated via code gen pipeline.

### New Components
- `private-rental-booking/` (3 files) — Wizard modal for private rentals. Sections: event type, date/time, facility picker, contact, special requests, bar service toggle, payment with deposit, rental agreement. Auto-populates from EventType defaults, creates EventCharge records.
- `committee-event-booking/` (3 files) — Simplified modal for committee events. Sections: event details, date/time, facility picker, bar service, ticket sales (conditional), notes.

### Calendar Integration
- `scheduler-calendar.component.ts` — Modified `handleDateSelect` to show a booking type chooser in Simple Mode (Private Rental / Committee Event / Custom Event). Advanced Mode opens the full modal directly.
- `scheduler-calendar.component.html` — Added floating chooser overlay with animated entrance.
- `scheduler-calendar.component.scss` — Chooser styles using `--sch-*` theme tokens.

### Module Registration
- `app.module.ts` — Added imports and declarations for both booking components.

### Theming System Integration (Session 2)
- Rewrote all 3 SCSS files (`private-rental-booking`, `committee-event-booking`, `scheduler-calendar` chooser) to use `--sch-*` CSS custom properties instead of hardcoded colors. Now works across all 5 themes (Default, Warm, Midnight, Slate, Ocean) including dark modes.

### Facility Resource Picker (Session 2)
- Both booking components now load resources of type "Facility" on init.
- **0 facilities**: Warning alert shown, booking proceeds without facility assignment.
- **1 facility**: Auto-selected with green checkmark + "Auto-selected" badge.
- **2+ facilities**: Dropdown picker for user selection.
- Selected facility saved as `resourceId` on the event.

### Administration Page (Session 2)
- `administration.component.html` — Added "Event Types" button to the Scheduling & Events card, routing to `/eventtypes`.

### Build Fix (Session 2)
- `event-add-edit-modal.component.ts` — Added missing `eventTypeId` to the submit data object literal (required after code-gen added it to `ScheduledEventSubmitData`).

## Key Decisions

- **Two distinct flows** rather than one adaptive modal — keeps the UX focused for coordinators who deal with either private rentals or committee events.
- **Bar service is an override toggle**, not strictly tied to EventType flag — any event can have bar service enabled/disabled regardless of the default.
- **EventType drives UI visibility** — flags like `requiresPayment`, `requiresRentalAgreement`, `requiresExternalContact` conditionally show/hide form sections.
- **Financial pipeline reused** — Payments and deposits flow through existing `EventCharge` entity rather than a new system, keeping the financial audit trail intact.
- **Attributes JSON** used for rental agreement details, bar service notes, and special requests — avoids schema changes for flexible metadata.
- **Theme tokens only** — No hardcoded colors; all styling uses `--sch-*` variables for consistent theming.
- **Facility auto-select** — Single-facility tenants (like PHMC) get zero-click facility assignment.

## Testing / Verification

- Angular build passes (exit code 0) with all changes.
- Verified all `EventTypeData` fields align with generated service (7 flags + pricing + chargeTypeId).
- Verified `ScheduledEventSubmitData` has `eventTypeId`, `bookingContactName/Email/Phone`, `partySize`, `attributes`, `resourceId`.
- Module registration confirmed (imports + declarations in `app.module.ts`).
- Remaining: live end-to-end testing with seeded data.
