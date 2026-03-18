# Event Document Management

**Date:** 2026-03-18

## Summary

Built a reusable event document management system enabling rec committee coordinators to upload, view, and manage documents (especially signed rental agreements) against scheduled events. Includes a full-featured document panel component integrated into the event modal, plus an optional at-booking attachment flow in the rental booking modal.

## Changes Made

### Seed Data
- **SchedulerTools/Program.cs** — Added 5 DocumentType seed records: Rental Agreement, Insurance Certificate, Permit, Receipt, Other (with colors and descriptions)

### New Component: EventDocumentPanelComponent
- **event-document-panel.component.ts** — Reusable panel with drag-and-drop upload, inline PDF/image preview (via DomSanitizer), download, status workflow (Uploaded → Reviewed → Filed), delete, auto-detect document type from filename
- **event-document-panel.component.html** — Document cards, drop zone, upload form with type selector, preview overlay with iframe/img rendering
- **event-document-panel.component.scss** — Themed styles using `--sch-*` tokens for consistency across all themes

### Event Modal Integration
- **event-add-edit-modal.component.html** — Added "Documents" tab (edit mode only) with `<app-event-document-panel>` embedding

### Rental Booking Optional Attachment
- **private-rental-booking.component.ts** — Added DocumentService, DocumentTypeService, AuthService injection; file reading/base64 encoding; post-save document creation with "Rental Agreement" type
- **private-rental-booking.component.html** — Added file input in Rental Agreement section with preview and remove button

### Module Registration
- **app.module.ts** — Import + declaration for EventDocumentPanelComponent

## Key Decisions

- **Reusable panel component** — Same component works in event modal and can be embedded in any future detail view
- **Base64 storage** — Uses existing `fileDataData` blob field on Document entity, no external file storage needed
- **At-booking upload is optional** — Most rental agreements are filed after the fact; the booking flow is not blocked
- **Status workflow** — Simple string-based status (Uploaded/Reviewed/Filed) with dropdown to change
- **DomSanitizer** — Required for PDF preview in iframes since Angular blocks `data:` URLs by default
- **Non-null assertion** — Documents tab only shown in edit mode where `event` is guaranteed non-null

## Testing / Verification

- `npx ng build` — **Passed** (exit code 0, no errors)
- Initial build failure was a type-safety issue (`ScheduledEventData | null` vs `ScheduledEventData`) — fixed with non-null assertion since the tab is gated behind `isEditMode`
