# Document Custom Listing View & Navigation Integration

**Date:** 2026-03-18

## Summary

Created a standalone Document listing view at `/documents` following the established `*-custom-listing` + `*-custom-table` pattern, and wired it into the sidebar and header mobile menu navigation. This builds on the earlier session work that added Documents tabs and count badges to Contact and Resource detail views.

## Changes Made

### New Files (6)
- `components/document-custom/document-custom-listing/document-custom-listing.component.ts` — Listing component with premium glassmorphism header, search bar with debounced filter, total/filtered document counts via `DocumentService.GetDocumentsRowCount()`
- `components/document-custom/document-custom-listing/document-custom-listing.component.html` — Header template with count badges, search input, and child `<app-document-custom-table>`
- `components/document-custom/document-custom-listing/document-custom-listing.component.scss` — Gradient header, glass utilities, search bar styling
- `components/document-custom/document-custom-table/document-custom-table.component.ts` — Table component with virtual scroll, dynamic `TableColumn[]` columns, sorting, client-side filtering, delete/undelete actions
- `components/document-custom/document-custom-table/document-custom-table.component.html` — Desktop table with sortable headers + mobile card layout
- `components/document-custom/document-custom-table/document-custom-table.component.scss` — Virtual scroll table styles matching other custom tables

### Modified Files (4)
- `app.module.ts` — Added imports and declarations for `DocumentCustomListingComponent` and `DocumentCustomTableComponent`
- `app-routing.module.ts` — Added `{ path: 'documents', component: DocumentCustomListingComponent, canActivate: [AuthGuard] }` route
- `components/sidebar/sidebar.component.html` — Added Documents nav link in core section (after Contacts, before Finances) with `fa-file-lines` icon
- `components/header/header.component.html` — Added Documents link in mobile menu overlay, same position

## Key Decisions

- **Core nav placement**: Documents placed in the always-visible core tier (Overview → Schedule → Contacts → Documents → Finances) rather than the collapsible Setup group, since documents are operational data, not configuration
- **No admin column gating**: Removed `isSchedulerAdministrator` check since the property doesn't exist on `AuthService`; the route is already behind `AuthGuard`
- **Default table columns**: Name, Document Type, Description, Status, File Name, MIME Type, Uploaded Date, Uploaded By, Contact (link), Resource (link), Event (link), Notes
- **Owner entity links**: Contact, Resource, and Event columns render as `routerLink` navigating to the respective detail views

## Testing / Verification

- Production build (`npx ng build --configuration production`) — zero new errors from document-custom components
- Pre-existing build errors in `ShiftPatternCustomDetailComponent`, `SystemHealthComponent`, and volunteer components remain unchanged
