# WebDAV Server & File Manager Entity Links

**Date:** 2026-03-19

## Summary

Built a standalone WebDAV server project (`Scheduler.WebDAV`) that exposes the Scheduler's file storage as a mountable network drive, using HTTP Basic Auth against the existing Security database. Also added entity link UI features to the file manager, including sidebar filtering, grid/list chip display, and detail panel management.

## Changes Made

### WebDAV Server (New Project)

- **`Scheduler.WebDAV/Scheduler.WebDAV.csproj`** — New minimal ASP.NET Core web project referencing FoundationCore, SchedulerDatabase, SchedulerServices
- **`Scheduler.WebDAV/Program.cs`** — Kestrel startup using `BuildFoundationServices`, configures BasicAuth → WebDAV middleware pipeline
- **`Scheduler.WebDAV/appsettings.json`** — Connection strings, Kestrel port 5100, credential cache TTL configuration
- **`Scheduler.WebDAV/Middleware/BasicAuthMiddleware.cs`** — HTTP Basic Auth: extracts credentials, validates via `SecurityLogic.AuthenticateLocalCredentials`, resolves tenant via `SecurityFramework.UserTenantGuidAsync`
- **`Scheduler.WebDAV/Middleware/BasicAuthCredentialCache.cs`** — Thread-safe in-memory credential cache (SHA256-hashed password keys, configurable TTL) to avoid DB round-trips on chatty WebDAV clients
- **`Scheduler.WebDAV/Services/WebDavContext.cs`** — Per-request context carrying authenticated user, tenant GUID, and security user ID
- **`Scheduler.WebDAV/Middleware/WebDavMiddleware.cs`** — HTTP method router dispatching to handler classes
- **`Scheduler.WebDAV/Xml/DavXmlBuilder.cs`** — System.Xml.Linq helpers for RFC 4918 multistatus XML responses
- **`Scheduler.WebDAV/Handlers/PathResolver.cs`** — URL path → folder chain + document name resolution (walks tenant folder tree)
- **`Scheduler.WebDAV/Handlers/OptionsHandler.cs`** — DAV:1 compliance and supported methods
- **`Scheduler.WebDAV/Handlers/PropFindHandler.cs`** — 207 Multi-Status XML listing folder/file properties (Depth 0 and 1)
- **`Scheduler.WebDAV/Handlers/GetHandler.cs`** — File download + browser landing page (glassmorphism HTML with connection instructions)
- **`Scheduler.WebDAV/Handlers/HeadHandler.cs`** — File metadata without body
- **`Scheduler.WebDAV/Handlers/PutHandler.cs`** — Upload/overwrite files with MIME type guessing
- **`Scheduler.WebDAV/Handlers/DeleteHandler.cs`** — Soft-delete files or cascade-delete folders
- **`Scheduler.WebDAV/Handlers/MkColHandler.cs`** — Create folders with conflict detection
- **`Scheduler.WebDAV/Handlers/MoveHandler.cs`** — Move/rename files and folders via Destination header
- **`Scheduler.WebDAV/Handlers/CopyHandler.cs`** — Copy files (folder copy returns 403 in Phase 1)

### Service Extraction

- **`SchedulerServices/IFileStorageService.cs`** — Moved from `Scheduler.Server/Services/` to shared library
- **`SchedulerServices/SqlFileStorageService.cs`** — Moved from `Scheduler.Server/Services/` to shared library
- Deleted originals from `Scheduler.Server/Services/` — no code changes needed since namespace was preserved

### File Manager Entity Links (User-authored)

- **`file-manager.component.ts`** — Added `fk` field to `getEntityLinks()` return type, entity link filtering/toggle methods
- **`file-manager.component.html`** — Sidebar entity link filter section, grid card entity chips, list view entity chips column, detail panel entity links with remove action
- **`file-manager.component.scss`** — Entity chip styles for grid/list views, detail panel entity link list/item/remove styles

## Key Decisions

- **No 3rd party dependencies** for WebDAV — built entirely on ASP.NET Core + System.Xml.Linq
- **Basic Auth over HTTPS** — WebDAV clients (Windows Explorer, macOS Finder) don't support OAuth2/OIDC
- **Credential caching** (5-min TTL) to handle chatty WebDAV client behavior without hammering the Security DB
- **Tenant auto-resolution** from `SecurityUser.securityTenantId` — no tenant GUID in the URL
- **Service extraction to SchedulerServices** rather than duplicating code — both Scheduler.Server and Scheduler.WebDAV reference the shared library
- **Browser landing page** — GET requests with `Accept: text/html` return an informative HTML page with connection instructions
- **2FA users excluded** in Phase 1 — Basic Auth can't support 2FA prompts; app-specific passwords planned for a future phase

## Testing / Verification

- `Scheduler.WebDAV` project — **Build succeeded, 0 errors**
- `Scheduler.Server` project — **Build succeeded, 0 errors** (service extraction was clean)
- Fixed 7 DateTime type errors (`Document.uploadedDate` is non-nullable `DateTime`, not `DateTime?`)
- Project added to solution via `dotnet sln add`
