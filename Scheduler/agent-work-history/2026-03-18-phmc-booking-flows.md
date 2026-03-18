# PHMC Booking Flows — EventType Entity & Purpose-Built Booking Components

**Date:** 2026-03-18

## Summary

Designed and implemented purpose-built booking flows for the PHMC (Petty Harbour) recreation committee. Created the `EventType` entity with 7 requirement flags, seeded PHMC-specific data, generated the full API/service stack, and built two streamlined Simple Mode booking components plus a calendar booking type chooser.

## Changes Made

### Schema & Seed Data
- `SchedulerDatabaseGenerator.cs` — Added `EventType` table with fields: `requiresRentalAgreement`, `requiresExternalContact`, `requiresPayment`, `requiresDeposit`, `requiresBarService`, `allowsTicketSales`, `isInternalEvent`, `defaultPrice`, `defaultChargeTypeId`. Added `eventTypeId` FK to `ScheduledEvent`.
- `SchedulerTools/Program.cs` — Seeded in `ConfigurePettyHarbour()`: Facility ResourceType, Petty Harbour Recreation Centre Resource, Hall Rental ChargeType, 7 EventType records (Birthday, Wedding, Bridal Shower, Private Event, Community Event, Fundraiser, Special Event).

### Code Generation (run by user)
- EventType entity, controller, DTOs, Angular data service with caching — all generated via code gen pipeline.

### New Components
- `private-rental-booking/` (3 files) — Purple-themed wizard modal for private rentals. 7 sections: event type, date/time, contact, special requests, bar service toggle, payment with deposit, rental agreement. Auto-populates from EventType defaults, creates EventCharge records.
- `committee-event-booking/` (3 files) — Green-themed simplified modal for committee events. 5 sections: event details, date/time, bar service, ticket sales (conditional), notes.

### Calendar Integration
- `scheduler-calendar.component.ts` — Modified `handleDateSelect` to show a booking type chooser in Simple Mode (Private Rental / Committee Event / Custom Event). Advanced Mode opens the full modal directly.
- `scheduler-calendar.component.html` — Added glassmorphic floating chooser overlay with animated entrance.
- `scheduler-calendar.component.scss` — Added chooser styles with color-coded icons per type.

### Module Registration
- `app.module.ts` — Added imports and declarations for both booking components.

## Key Decisions

- **Two distinct flows** rather than one adaptive modal — keeps the UX focused for coordinators who deal with either private rentals or committee events.
- **Bar service is an override toggle**, not strictly tied to EventType flag — any event can have bar service enabled/disabled regardless of the default.
- **EventType drives UI visibility** — flags like `requiresPayment`, `requiresRentalAgreement`, `requiresExternalContact` conditionally show/hide form sections.
- **Financial pipeline reused** — Payments and deposits flow through existing `EventCharge` entity rather than a new system, keeping the financial audit trail intact.
- **Attributes JSON** used for rental agreement details, bar service notes, and special requests — avoids schema changes for flexible metadata.

## Testing / Verification

- Verified all `EventTypeData` fields align with generated service (7 flags + pricing + chargeTypeId).
- Verified `ScheduledEventSubmitData` has `eventTypeId`, `bookingContactName/Email/Phone`, `partySize`, `attributes`.
- Module registration confirmed (imports + declarations in `app.module.ts`).
- Remaining: live build test and end-to-end testing with seeded data.
