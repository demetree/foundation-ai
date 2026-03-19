# Document Entity Linking in File Manager

**Date:** 2026-03-19

## Summary

Made the file manager entity-aware by surfacing all 17 document-entity links (Contact, Client, Office, Invoice, etc.) throughout the UI. Documents linked to Scheduler entities now display entity names as colored chips, are filterable by entity type in the sidebar, and are clickable to navigate to entity detail views (for the 11 entity types that have detail pages).

## Changes Made

### Server

- **SchedulerServices/SqlFileStorageService.cs** — Added `.Include()` for all 17 entity nav properties in `GetDocumentsInFolderAsync()` and `SearchDocumentsAsync()`, plus corresponding assignments in the `Select()` projections.
- **Scheduler.Server/Controllers/FileManagerController.cs** — Changed `GetDocumentsInFolder` and `SearchDocuments` endpoints from `Document.ToDTOList()` to `Document.ToOutputDTOList()` so entity nav properties flow to the client.

### Client

- **services/file-manager.service.ts** — Extended the `DocumentDTO` interface with optional nav property types for all 17 linked entity types.
- **components/file-manager/file-manager.component.ts** — Added `entityLinkConfig` (17-entry config map), `getEntityLinks()` helper, `activeEntityLinkTypes` computed getter, entity link filter state (`activeEntityLinkFilter`), `removeEntityLink()` method, and updated `filteredDocuments` to combine tag + entity link filters.
- **components/file-manager/file-manager.component.html** — Added sidebar "Entity Links" filter section, entity link chips in grid and list views, a "Linked To" column in the list view table, and an "Entity Links" section in the detail panel with remove buttons.
- **components/file-manager/file-manager.component.scss** — Added styles for `.fm-entity-chip`, `.fm-entity-chip-sm`, `.fm-entity-link-list`, `.fm-entity-link-item`, `.fm-entity-link-remove`, etc.

## Key Decisions

- **All 17 entity types include names** (not just the 11 with detail pages). The 6 without detail pages (FinancialTransaction, FinancialOffice, TenantProfile, Campaign, Household, Constituent, Tribute) show names but are not clickable — a TODO was added to make them routable once detail pages are built.
- **Eager loading via `.Include()`** was chosen over a separate endpoint for simplicity. EF LEFT JOINs on nullable FKs add minimal overhead since most will be null.
- **Reused the `entityLinkConfig` pattern** from `document-custom-table.getLinkedTo()` but expanded it to cover all 17 types with icons, colors, routes, and name field resolution (including nested paths like `resource.name` for volunteerProfile).

## Testing / Verification

- **Angular build**: No file-manager errors. Build exits with code 1 from pre-existing errors in unrelated components (SystemHealth, VolunteerOverviewTab, ShiftPatternCustomDetail).
- **SCSS fix**: Caught and fixed 3 invalid `a&` compound selectors (not valid in Angular SCSS) → replaced with standalone `a.fm-entity-*` selectors.
