# Download Zip Feature

**Date:** 2026-03-19

## Summary

Added zip download capabilities to the File Manager at both the folder and bulk-document levels.

## Changes Made

### Server-Side
- `FileManagerController.cs` — Added two endpoints:
  - `GET api/FileManager/Folders/{folderId}/Download` — Recursively collects all documents in a folder tree, builds a ZipArchive preserving the directory structure, returns as `application/zip`
  - `POST api/FileManager/Documents/DownloadZip` — Accepts a list of document IDs, zips them flat with duplicate filename deduplication (`report.pdf`, `report (2).pdf`)
  - Added private `CollectDocumentsRecursively` helper for recursive folder walking
  - Added `System.IO.Compression` using

### Client-Side
- `file-manager.service.ts` — Added `downloadFolderAsZip(folderId)` and `downloadDocumentsAsZip(docIds)` methods
- `file-manager.component.ts` — Added `downloadFolderZip(folder)` and `downloadSelectedAsZip()` with blob-to-link download pattern
- `file-manager.component.html` — Added "Download Zip" to folder context menu and "Download Zip" button in bulk action bar

## Key Decisions

- **No new service interface methods** — Controller uses existing `GetDocumentsInFolderAsync`, `GetDocumentByIdAsync`, and `GetFoldersAsync` from `IFileStorageService`
- **In-memory ZipArchive** — Uses `MemoryStream` + `ZipArchive` with `CompressionLevel.Optimal`; suitable for current scale but may need streaming for very large folders
- **File name deduplication** — Bulk downloads auto-suffix duplicates with `(2)`, `(3)`, etc.

## Testing / Verification

- Angular build: 0 errors/warnings from file-manager files
- .NET server build: 0 errors
- Manual testing pending: folder zip with subfolders, bulk document zip with duplicate names
