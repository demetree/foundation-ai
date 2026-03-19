# Document Tab Expansion — Contact & Resource

**Date:** 2026-03-18

## Summary

Generalized the existing `EventDocumentPanelComponent` from an event-only document manager into a reusable panel that works with any entity having a FK on the Document table. Added Documents tabs to the Contact and Resource detail views.

## Changes Made

- **`Scheduler.Client/src/app/components/scheduler/event-document-panel/event-document-panel.component.ts`**
  - Replaced `@Input() event: ScheduledEventData` with `@Input() ownerField: string` and `@Input() ownerId: number | bigint`
  - Updated `loadData()` to build a dynamic query using `ownerField` as the FK filter
  - Updated `uploadDocument()` to set the FK dynamically via `(submit as any)[this.ownerField] = this.ownerId`
  - Removed `ScheduledEventData` import and all `this.event.ClearDocumentsCache()` calls

- **`Scheduler.Client/src/app/components/scheduler/event-add-edit-modal/event-add-edit-modal.component.html`**
  - Updated event modal Documents tab from `[event]="event!"` to `[ownerField]="'scheduledEventId'" [ownerId]="event!.id"`

- **`Scheduler.Client/src/app/components/contact-custom/contact-custom-detail/contact-custom-detail.component.html`**
  - Added `ngbNavItem="documents"` tab after Schedule tab, before History tab
  - Uses `[ownerField]="'contactId'" [ownerId]="contact!.id"`
  - Added document count badge matching existing tab convention

- **`Scheduler.Client/src/app/components/contact-custom/contact-custom-detail/contact-custom-detail.component.ts`**
  - Imported `DocumentService`, injected it, added `DocumentCount$` observable
  - Populated via `GetDocumentsRowCount({ contactId })` after contact loads

- **`Scheduler.Client/src/app/components/resource-custom/resource-custom-detail/resource-custom-detail.component.html`**
  - Added `ngbNavItem="documents"` tab after Rate Overrides tab, before History tab
  - Uses `[ownerField]="'resourceId'" [ownerId]="resource!.id"`
  - Added document count badge matching existing tab convention

- **`Scheduler.Client/src/app/components/resource-custom/resource-custom-detail/resource-custom-detail.component.ts`**
  - Imported `DocumentService`, injected it, added `DocumentCount$` observable
  - Populated via `GetDocumentsRowCount({ resourceId })` in `refreshRowCountObservables()`

## Key Decisions

- **Generic owner pattern over separate components**: Instead of creating `ContactDocumentPanel` and `ResourceDocumentPanel`, the existing component was generalized with a dynamic FK approach. Any entity with a Document FK can now add a Documents tab with a single line of HTML.
- **Document table already had FKs**: The `Document` entity already had nullable FK columns for `contactId`, `resourceId`, `invoiceId`, `receiptId`, `financialTransactionId`, and `scheduledEventId`. No schema changes were needed.
- **No module changes required**: `EventDocumentPanelComponent` was already declared in the app module; no new declarations were needed.

## Testing / Verification

- `npx ng build --configuration production` — zero errors from the 4 modified files
- Pre-existing build warnings/errors in unrelated components (VolunteerOverviewTab, SystemHealth, ShiftPatternCustomDetail) remain unchanged and are not caused by this work
