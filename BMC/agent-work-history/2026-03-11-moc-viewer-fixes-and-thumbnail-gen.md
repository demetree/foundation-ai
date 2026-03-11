# MOC Viewer Fixes, Thumbnail Support & Auto-Generation

**Date:** 2026-03-11

## Summary

Fixed multiple issues with the My Projects page and MOC 3D Viewer. Projects now load correctly, display thumbnails, navigate to the viewer, and respect the active BMC theme. Added server-side thumbnail caching and automatic thumbnail generation during import for files that don't include one.

## Changes Made

### Client — `BMC.Client`

- **`src/app/services/project.service.ts`**
  - Fixed `projectUrl` from `/api/Project` (POST/create route) to `/api/Projects` (plural list endpoint)
  - Fixed `deleteProject()` to use literal `/api/Project/{id}` instead of the list URL
  - (Previous session) Fixed `uploadModel()` to remove `Content-Type` header for `FormData` uploads

- **`src/app/components/my-projects/my-projects.component.ts`**
  - Added `HttpClient` and `AuthService` injection
  - Added `thumbnailUrls` map and `loadThumbnails()` method — fetches thumbnails as authenticated blobs since `<img>` tags can't send Authorization headers
  - Added `getThumbnailUrl()` helper for template use

- **`src/app/components/my-projects/my-projects.component.html`**
  - Updated thumbnail rendering to use `getThumbnailUrl()` blob URLs
  - Fixed navigation from `/projects/{id}` (auto-generated CRUD) to `/my-projects/{id}/viewer` (3D viewer)
  - Applied to both grid view and list view

- **`src/app/components/moc-viewer/moc-viewer.component.scss`**
  - Replaced all hardcoded dark theme colors with BMC CSS custom properties (`--bmc-bg-deep`, `--bmc-bg-card`, `--bmc-glass-bg`, `--bmc-text-primary`, `--bmc-border`, `--bmc-primary`, etc.)

- **`src/app/components/moc-viewer/moc-viewer.component.ts`**
  - Updated grid helper to read `--bmc-border` CSS variable at runtime instead of hardcoded `0x333355`

### Server — `BMC.Server`

- **`Controllers/MocExportController.cs`**
  - Added `static ExpiringCache<int, byte[]>` for thumbnail caching (5-minute sliding expiry)
  - `GetProjectThumbnail` now checks cache first, only querying DB on cache miss
  - Added `using Foundation.Concurrent` import

- **`Services/ModelImportService.cs`**
  - Added `IConfiguration` injection for `LDraw:DataPath` access
  - Added `using BMC.LDraw.Render` and `using Microsoft.Extensions.Configuration`
  - Added `GenerateThumbnailFromLDraw()` method — creates a `RenderService` and renders a 512×512 PNG thumbnail from parsed LDraw lines with 2× SSAA
  - Wired into `CreateProjectFromModelsAsync` as fallback when no `.io` thumbnail exists
  - Thumbnail generation is non-fatal — wrapped in try/catch so import succeeds even if rendering fails
  - Passed `lDrawLines` through to `CreateProjectFromModelsAsync` for rendering access

## Key Decisions

- **Blob URL approach for thumbnails**: Since `<img src>` tags can't include Authorization headers, thumbnails are fetched via `HttpClient` with auth headers and converted to `URL.createObjectURL()` blob URLs
- **Static `ExpiringCache` for thumbnails**: Shared across all controller instances, 5-minute sliding expiry, no compression (PNG data is already compressed)
- **Non-fatal thumbnail generation**: Render failures during import log a warning but don't block the import — the project is created without a thumbnail
- **Theme-aware viewer**: Used CSS custom properties throughout so the viewer respects light/dark/custom themes; 3D canvas uses `alpha: true` with transparent clear color so CSS background shows through

## Testing / Verification

- Confirmed `RenderToPng(string[] lines, string fileName, ...)` overload exists in `RenderService.cs` (line 770) and matches the call signature
- Confirmed `ModelImportService` is registered as `AddScoped<ModelImportService>()` in `Program.cs:177` — `IConfiguration` auto-resolves from DI
- Confirmed auto-generated `ProjectsController` routes: `GET /api/Projects` (list), `GET /api/Project/{id}` (single), `DELETE /api/Project/{id}` (delete)
- User verified: project card now appears in My Projects after upload, thumbnail displays correctly, clicking card navigates to 3D viewer
