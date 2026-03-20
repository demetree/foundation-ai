# Document Exchange Features — Walkthrough

## What Was Built

### Feature 1: Shareable Links
External users can download files via GUID-based public URLs — no login required.

### Feature 2: Email a File
Users can email documents as attachments or via auto-generated share links.

---

## Files Changed

### Database Layer

| File | Change |
|------|--------|
| [SchedulerDatabaseGenerator.cs](file:///g:/source/repos/Scheduler/SchedulerDatabaseGenerator/SchedulerDatabaseGenerator.cs) | Added `DocumentShareLink` table (token, passwordHash, expiresAt, maxDownloads, downloadCount) |
| [DocumentShareLink.cs](file:///g:/source/repos/Scheduler/SchedulerDatabase/Database/DocumentShareLink.cs) | **[NEW]** EF Core entity class |
| [DocumentShareLinkExtension.cs](file:///g:/source/repos/Scheduler/SchedulerDatabase/EntityExtensions/DocumentShareLinkExtension.cs) | **[NEW]** DTOs, ToDTO/FromDTO, Clone. Password hash is never exposed — `hasPassword` boolean returned instead |
| [SchedulerContext.cs](file:///g:/source/repos/Scheduler/SchedulerDatabase/Database/SchedulerContext.cs) | Added DbSet + OnModelCreating fluent config |

### Server Layer

| File | Change |
|------|--------|
| [FileManagerController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FileManagerController.cs) | Added 7 endpoints (see below) |
| [Scheduler.Server.csproj](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Scheduler.Server.csproj) | Added `BCrypt.Net-Next` NuGet package |

### Client Layer

| File | Change |
|------|--------|
| [file-manager.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/services/file-manager.service.ts) | Added `ShareLinkDTO`, `EmailDocumentRequest` interfaces + 6 service methods |

### UI Layer

| File | Change |
|------|--------|
| [fm-detail-panel.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-detail-panel/fm-detail-panel.component.ts) | Added share link management + email form state, methods, `OnChanges` lifecycle |
| [fm-detail-panel.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-detail-panel/fm-detail-panel.component.html) | Collapsible share link panel (create form, links list, copy/revoke) + email form section |
| [fm-detail-panel.component.scss](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-detail-panel/fm-detail-panel.component.scss) | Premium styling for share items (expired/exhausted states), email form fields |

---

## New API Endpoints

### Authenticated (Share Link Management)
| Method | Route | Purpose |
|--------|-------|---------|
| POST | `api/FileManager/Documents/{id}/ShareLink` | Create a share link (optional: password, expiry, download limit) |
| GET | `api/FileManager/Documents/{id}/ShareLinks` | List all share links for a document |
| DELETE | `api/FileManager/ShareLinks/{linkId}` | Revoke (soft-delete) a share link |

### Unauthenticated (Public Access)
| Method | Route | Purpose |
|--------|-------|---------|
| GET | `api/Share/{token}` | Get link metadata (file name, size, password required?) |
| POST | `api/Share/{token}/Verify` | Verify password for protected links |
| GET | `api/Share/{token}/Download` | Download the shared file |

### Email
| Method | Route | Purpose |
|--------|-------|---------|
| POST | `api/FileManager/Documents/{id}/Email` | Email document (≤10MB → attachment, >10MB → auto share link) |

---

## Key Design Decisions

- **Password hashing**: BCrypt for share link passwords — hash stored in DB, never sent to client
- **Smart email sizing**: Files ≤10MB sent as attachments, larger files get auto-generated 7-day share links
- **Token vs ObjectGuid**: Share links use a separate `token` GUID for public URLs (not the internal `objectGuid`)
- **Soft-delete revocation**: Revoking a link sets `deleted=true, active=false` — links remain in DB for audit trail

## Verification

- **Server build**: ✅ 0 errors (`dotnet build` on `Scheduler.Server`)
- **Client build**: ✅ 0 TS errors (`ng build --configuration=development`)

## Remaining Work

- [ ] Feature 3: Upload Portal _(deferred to follow-up session)_
