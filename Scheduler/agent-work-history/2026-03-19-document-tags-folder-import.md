# Document Tags UI & Folder Drag-and-Drop Import

**Date:** 2026-03-19

## Summary

Three areas of work on the File Manager component: (1) integrated the document tagging system into the UI with 6 features, (2) added folder-based drag-and-drop batch import with recursive directory walking, and (3) fixed a discoverability issue with the tag section.

## Changes Made

### Server-Side — Tag CRUD Endpoints
- `IFileStorageService.cs` — Added `UpdateTagAsync` and `DeleteTagAsync` to the interface
- `SqlFileStorageService.cs` — Implemented both methods with soft-delete and change history auditing
- `FileManagerController.cs` — Added `PUT api/FileManager/Tags` and `DELETE api/FileManager/Tags/{tagId}` endpoints

### Client-Side — Tag UI (6 Features)
- `file-manager.service.ts` — Added `updateTag()` and `deleteTag()` service methods
- `file-manager.component.ts` — Added ~300 lines: tag state, `forkJoin`-based parallel tag loading, `filteredDocuments` getter, tag CRUD, bulk operations, filter toggles
- `file-manager.component.html` — Integrated all 6 tag features:
  1. Detail panel tags (colored chips with add/remove)
  2. Tag filter bar (scrolling pills, instant client-side filtering)
  3. Tags on file cards (colored dots) and list view (mini-chip column)
  4. Tag management modal (create/rename/delete with 12-color picker)
  5. Bulk tagging (multi-select checkboxes + batch tag/untag)
  6. Sidebar tag section (tag list with counts, gear icon for management)
- `file-manager.component.scss` — Added ~430 lines of tag styles

### Client-Side — Folder Drag-and-Drop Batch Import
- `file-manager.component.ts` — Updated `onDrop()` to detect directory entries via `webkitGetAsEntry()`, added `importFolderEntries()` orchestrator, `walkDirectoryTree()` recursive walker, `readAllEntries()` (handles Chrome 100-item cap), `fileEntryToFile()` converter
- `file-manager.component.html` — Import progress overlay (phase, detail, progress bar, counter)
- `file-manager.component.scss` — `.fm-import-*` progress card styles

### Bug Fix — Tag Section Discoverability
- Removed `allTags.length > 0` condition from sidebar tag section so it always appears
- Added "Create your first tag" clickable prompt when no tags exist

## Key Decisions

- **Client-side tag filtering** — Tag filters use a `filteredDocuments` getter that intersects active filters with the `documentTagsMap` cache, avoiding server round-trips
- **Conflict detection in folder import** — When importing, if a folder with the same name already exists under the same parent, it reuses the existing folder's ID instead of creating a duplicate
- **Sequential folder creation** — Folders are created parent-before-child (sorted by path depth) to guarantee parent IDs are available; files are uploaded per-folder after all folders exist
- **Tag color system** — 12 curated color options in the color picker, defaulting to indigo (#6366f1)

## Testing / Verification

- Angular build: 0 errors/warnings from file-manager files (both tag and folder import changes)
- .NET server build: 0 errors (fixed one `DocumentTag.notes` reference that didn't exist on the entity)
- Manual testing pending: tag creation/filtering/bulk operations, folder drag-and-drop with nested subfolders
