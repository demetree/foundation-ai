# File Manager Feature — Full-Stack Implementation

**Date:** 2026-03-19

## Summary

Implemented a complete File Manager feature for the Scheduler application. This includes an `IFileStorageService` abstraction layer with a SQL-backed implementation, a `FileManagerController` exposing REST API endpoints, and an Explorer-like Angular UI with folder tree, dual-view (grid/list) file browser, drag-and-drop uploads, search, context menus, and a file detail panel.

## Changes Made

### Server — New Files
- `Scheduler.Server/Services/IFileStorageService.cs` — Abstraction interface for file storage operations (folder CRUD, document CRUD, upload, download, move, search, tag management). All operations are tenant-scoped.
- `Scheduler.Server/Services/SqlFileStorageService.cs` — SQL Server / EF Core implementation using the `Document` and `DocumentFolder` tables. Uses `Foundation.ChangeHistoryToolset` for auditing. Excludes binary data from listing queries for performance.
- `Scheduler.Server/Controllers/FileManagerController.cs` — Custom controller exposing all `IFileStorageService` operations via REST endpoints. Handles multipart file uploads, blob downloads, and entity linking via query parameters.

### Server — Modified Files
- `Scheduler.Server/Program.cs` — Registered `IFileStorageService` / `SqlFileStorageService` as scoped services; added `FileManagerController` to custom controller list.

### Client — New Files
- `Scheduler.Client/src/app/services/file-manager.service.ts` — Angular service wrapping all API endpoints. Includes typed DTOs (`FolderDTO`, `DocumentDTO`, `DocumentTagDTO`, `UploadOptions`), folder tree builder, file size formatter, and mime-type icon/color resolver.
- `Scheduler.Client/src/app/components/file-manager/file-manager.component.ts` — Component logic: folder navigation with breadcrumbs, grid/list document views, drag-and-drop upload, search, right-click context menus, and inline modal dialogs for folder/document CRUD.
- `Scheduler.Client/src/app/components/file-manager/file-manager.component.html` — Explorer-like template with dark toolbar, collapsible left folder tree, right content area (subfolder cards + file grid/list), animated detail side-panel, context menus, and lightweight inline modals.
- `Scheduler.Client/src/app/components/file-manager/file-manager.component.scss` — Premium styling: dark sidebar/toolbar with light content area, card shadows, hover micro-animations, glassmorphic context menus, drag-drop overlay pulse animation, responsive breakpoints.

### Client — Modified Files
- `app.module.ts` — Import + declaration of `FileManagerComponent`.
- `app-routing.module.ts` — Import + route: `{ path: 'filemanager', component: FileManagerComponent, canActivate: [AuthGuard], title: 'File Manager' }`.
- `sidebar.component.html` — Added "File Manager" nav item with `fa-folder-tree` icon between Documents and Finances.

## Key Decisions

- **Abstraction layer** — `IFileStorageService` is specced so a cloud-storage backend (Azure Blob, S3, etc.) can replace the SQL implementation without changing the controller or UI.
- **Tenant isolation** — All queries are scoped by `tenantProfileId` extracted from the JWT.
- **Performance** — Document listing and search queries exclude the `fileDataData` binary column.
- **Auditing** — All write operations create audit events via `Foundation.ChangeHistoryToolset` using `AuditEngine.AuditType` enums (`CreateEntity`, `UpdateEntity`, `Miscellaneous`).
- **Single component** — The File Manager UI is a single Angular component (not split into sub-components) to keep the initial implementation simple. Can be decomposed later if complexity grows.

## Testing / Verification

- **Server build** — Compiled with 0 new errors (pre-existing warnings only).
- **Client build** — Compiled with 0 new errors (pre-existing warnings in `SystemHealthComponent`, `VolunteerOverviewTabComponent`, and `VolunteerGroupOverviewTabComponent` are unrelated).
- **Manual testing** — Pending (folder CRUD, file upload/download, search, drag-and-drop, context menus).
