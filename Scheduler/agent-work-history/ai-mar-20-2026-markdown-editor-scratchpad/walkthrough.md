# Walkthrough: Markdown Editor & Entity Scratchpad

## Overview

Implemented a full-stack markdown text editor with versioning, plus an entity scratchpad system that embeds the editor as a "Notes" tab across 10 entity detail views.

---

## Phase 1 — Core Markdown Editor

### Server Endpoints

#### [FileManagerController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FileManagerController.cs)

Two new content endpoints:

| Method | Route | Purpose |
|--------|-------|---------|
| `GET` | `Documents/{id}/Content` | Returns text content as UTF-8 string + version number |
| `PUT` | `Documents/{id}/Content` | Saves text content as a new document version |

- **MIME validation** via `EditableMimeTypes` (text/plain, text/markdown, text/html, text/css, application/json, etc.)
- **Versioning**: Each save creates a new version preserving `objectGuid` and incrementing `versionNumber`

### Client Service

#### [file-manager.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/services/file-manager.service.ts)

Added `getDocumentContent()` and `saveDocumentContent()` methods.

### Editor Component

#### [fm-text-editor](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-text-editor)

| Feature | Detail |
|---------|--------|
| **Auto-save** | 60s countdown timer, visual indicator |
| **Manual save** | Ctrl+S / Save button |
| **Markdown preview** | Side-by-side split pane |
| **Version history** | Right panel showing all versions |
| **Undo/Redo** | Ctrl+Z / Ctrl+Shift+Z with 100-item stack |
| **Tab insertion** | 4-space indent on Tab key |
| **Status bar** | Line/column position, character count |
| **Dirty state** | `beforeunload` warning for unsaved changes |

### File Manager Integration

- **Edit context menu option** — visible only for text-based files
- **Full-screen editor overlay** — overlays the file manager with a back button and document name header

---

## Phase 2 — Entity Scratchpad

### Server Endpoints

#### [FileManagerController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FileManagerController.cs)

Three scratchpad endpoints:

| Method | Route | Purpose |
|--------|-------|---------|
| `GET` | `Scratchpad/{entityType}/{entityId}` | Finds active scratchpad for an entity |
| `POST` | `Scratchpad/{entityType}/{entityId}` | Creates a new scratchpad document in `_Notes` folder |
| `POST` | `Scratchpad/{entityType}/{entityId}/Archive` | Renames current note with timestamp and creates fresh one |

- **Entity mapping** via `ScratchpadEntityMap` — 11 entity types with FK property names
- **`_Notes` folder** auto-created as a system-managed folder
- **Reflection** used to set the correct FK field on the Document model

### Client Service

Added `getScratchpad()`, `createScratchpad()`, `archiveScratchpad()` methods.

### Scratchpad Component

#### [fm-scratchpad](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-scratchpad)

Wrapper component with three states:
1. **Loading** — spinner while checking for existing scratchpad
2. **Empty** — "Create Notes" button when no scratchpad exists
3. **Active** — embeds `fm-text-editor` with an "Archive & Start New" button

### Entity Integration

Notes tab/section added to **10 entity detail views**:

| Entity | Component | Integration Style |
|--------|-----------|-------------------|
| Client | `client-custom-detail` | ngbNav tab |
| Contact | `contact-custom-detail` | ngbNav tab |
| Resource | `resource-custom-detail` | ngbNav tab |
| Office | `office-custom-detail` | ngbNav tab |
| Crew | `crew-custom-detail` | ngbNav tab |
| Scheduling Target | `scheduling-target-custom-detail` | ngbNav tab |
| Volunteer | `volunteer-custom-detail` | ngbNav tab |
| Invoice | `invoice-custom-detail` | Manual tab system |
| Receipt | `receipt-custom-detail` | Flat section |
| Payment | `payment-custom-detail` | Flat section |

---

## Build Verification

Build exits with code 1 due to **pre-existing template errors** in:
- `SystemHealthComponent`
- `VolunteerOverviewTabComponent`
- `VolunteerGroupOverviewTabComponent`

> [!NOTE]
> These errors were documented in prior sessions and are unrelated to the markdown editor or scratchpad changes. No new compilation errors were introduced.

## Files Changed

### New Files
- `fm-text-editor/fm-text-editor.component.ts`
- `fm-text-editor/fm-text-editor.component.html`
- `fm-text-editor/fm-text-editor.component.scss`
- `fm-scratchpad/fm-scratchpad.component.ts`
- `fm-scratchpad/fm-scratchpad.component.html`
- `fm-scratchpad/fm-scratchpad.component.scss`

### Modified Files
- `FileManagerController.cs` — Content + Scratchpad endpoints
- `file-manager.service.ts` — 5 new service methods
- `file-manager.component.ts` — Editor state + isTextFile helper
- `file-manager.component.html` — Edit context menu + editor overlay
- `file-manager.component.scss` — Editor overlay styles
- `app.module.ts` — Component registration
- 10 entity detail HTML templates — Notes tab/section
