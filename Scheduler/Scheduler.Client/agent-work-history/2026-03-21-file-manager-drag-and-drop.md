# File Manager Drag & Drop Document Reorganization

**Date:** 2026-03-21

## Summary

Implemented internal drag-and-drop within the file manager component, allowing users to reorganize documents by dragging them onto folders. Supports multi-select bulk moves. The existing external file upload drop functionality was preserved.

## Changes Made

### file-manager.component.ts
- Added drag state properties (`draggedDocIds`, `dropTargetFolderId`, `dragGhostEl`)
- Added drag source handlers: `onDocDragStart`, `onDocDragEnd`
- Added drop target handlers: `onFolderDragOver`, `onFolderDragLeave`, `onFolderDrop`
- Updated existing `onDragOver`, `onDragEnter` to skip internal drags (custom MIME type `application/x-fm-doc`)
- Updated existing `onDrop` to ignore internal drag events
- Added guard in `selectDocument` to suppress selection during active drag
- `isBeingDragged()` helper for template class binding

### file-manager.component.html
- Added `draggable="true"`, `(dragstart)`, `(dragend)` on document cards (grid) and rows (list)
- Added `draggable="false"` on child `<img>`, `<input>`, `<button>` to prevent native browser drag hijacking
- Added `(dragover)`, `(dragleave)`, `(drop)` on child folder cards and breadcrumb segments
- Added `[class.drop-hover]` and `[class.fm-dragging]` bindings

### fm-sidebar.component.ts
- Added `@Input() dropTargetFolderId` for visual highlighting
- Added `@Output()` events: `folderDrop`, `folderDragOver`, `folderDragLeave`

### fm-sidebar.component.html
- Added drag event handlers and `[class.drop-hover]` on Root and recursive folder nodes

### file-manager.component.scss
- `.fm-dragging` — opacity: 0.4 on dragged cards/rows
- `.drop-hover` — indigo outline/glow on drop targets (folder cards, breadcrumbs)
- `.fm-drag-ghost` — off-screen styled label for drag image
- `.fm-file-card` — `user-select: none`, `cursor: grab`, removed `transform: translateY(-2px)` hover

### fm-sidebar.component.scss
- `.fm-folder-node.drop-hover` — indigo highlight for sidebar folder nodes

## Key Decisions

- **Custom MIME type** (`application/x-fm-doc`): Used to distinguish internal document drags from external OS file drops, preserving existing upload functionality
- **Multi-select aware**: If multiple docs are checked, dragging one moves the entire selection
- **Ghost element lifecycle**: Must persist in DOM until `dragend` — removing it in `setTimeout(0)` caused instant drag cancellation
- **No `pointer-events: none`** on dragging source: Applying it during `dragstart` caused the browser to immediately cancel the drag (the main bug)
- **No hover transform**: `transform: translateY(-2px)` interfered with drag initiation; replaced with shadow/border-only hover effect
- **Sequential API calls**: Documents are moved one-by-one via `FileManagerService.moveDocument()` for proper error handling

## Testing / Verification

- Built successfully with `ng build --configuration=development` (0 TS errors)
- Browser-tested at `https://localhost:5901/filemanager`:
  - Created a "Test DnD" folder
  - Dragged PHMC_Logo.png onto folder card → successfully moved
  - Verified file appeared inside Test DnD and disappeared from Root
- Debugged drag initiation issues using console logging to identify root causes:
  - `pointer-events: none` on `.fm-dragging` class caused instant drag cancel
  - Ghost element removal in `setTimeout(0)` caused instant drag cancel
  - Native browser image drag on `<img>` thumbnails overrode card drag
