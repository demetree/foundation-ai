# Zip Download Fix & SharePoint-Class File Manager Features

**Date:** 2026-03-19

## Summary

Fixed empty folder zip download bug, refactored zip endpoints to stream directly to the response body, and implemented 6 SharePoint-class features for the File Manager: document preview, recycle bin, version history, favorites, storage quota, and SignalR real-time sync.

## Changes Made

### Bug Fix: Empty Folder Zip Download
- **`SqlFileStorageService.cs`** — Root cause: `GetDocumentsInFolderAsync` excludes `fileDataData` for performance. Fixed by fetching each blob via `GetDocumentByIdAsync` during zip creation.

### Streaming Zip Refactor
- **`FileManagerController.cs`** — Refactored `DownloadFolderAsZip` and `DownloadDocumentsAsZip` to stream `ZipArchive` directly to `Response.Body` instead of buffering. Only one blob in memory at a time.

### Feature 1: Document Preview
- `file-manager.component.ts` — `DomSanitizer`, preview type checkers, text content loading
- `file-manager.component.html` — Conditional `<img>`, `<iframe>`, `<pre>` in detail panel
- `file-manager.component.scss` — Preview styles

### Feature 2: Recycle Bin
- `IFileStorageService.cs` — `GetDeletedDocumentsAsync`, `RestoreDocumentAsync`, `PermanentlyDeleteDocumentAsync`
- `SqlFileStorageService.cs` — SQL implementations with change history
- `FileManagerController.cs` — `GET/POST/DELETE api/FileManager/Trash`
- `file-manager.service.ts` — `getTrash()`, `restoreFromTrash()`, `permanentlyDelete()`
- Component — Sidebar trash entry with badge, full trash view with restore/delete/empty

### Feature 3: Version History
- `IFileStorageService.cs` / `SqlFileStorageService.cs` — `GetDocumentVersionsAsync` using `objectGuid`
- `FileManagerController.cs` — `GET Documents/{id}/Versions`, `POST Documents/{id}/NewVersion`
- `file-manager.service.ts` — `getVersions()`, `uploadNewVersion()`
- Component — Collapsible version list in detail panel, upload-new-version button

### Feature 4: Favorites / Pinning
- Component — localStorage-based `favoriteDocIds` Set with star buttons on grid cards and detail panel
- Sidebar — Favorites section with quick-access list

### Feature 5: Storage Quota
- `IFileStorageService.cs` / `SqlFileStorageService.cs` — `GetStorageUsageAsync` (total bytes + doc count)
- `FileManagerController.cs` — `GET api/FileManager/Storage`
- Component — Storage usage display in sidebar footer

### Feature 6: SignalR Real-Time Sync
- **`FileManagerHub.cs`** (NEW) — `IFileManagerHubClient` with 4 signals, tenant-scoped groups
- `Program.cs` — `MapHub<FileManagerHub>("/FileManagerSignal")`
- `FileManagerController.cs` — Injected `IHubContext`, broadcast helpers, broadcast on upload
- **`filemanager-signalr.service.ts`** (NEW) — Client SignalR service with 4 RxJS Subjects
- Component — Connect on init, subscribe for auto-refresh, disconnect on destroy

## Key Decisions

- **Streaming zip** — stream directly to `Response.Body` to avoid MemoryStream buffer duplication
- **Preview** — reuses existing download endpoint URL for `<img src>` / `<iframe src>`
- **Version history** — leverages existing `objectGuid` / `versionNumber` fields in Document entity
- **Favorites** — localStorage for quick delivery; can upgrade to server-side `DocumentFavorite` table later
- **SignalR** — follows the existing `SchedulerHub` pattern with tenant-scoped groups

## Testing / Verification

- .NET server build: 0 errors ✓
- Angular build: no file-manager errors; pre-existing NG8107/NG8102 warnings in other components cause exit code 1
