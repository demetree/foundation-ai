# Markdown Editor & Entity Scratchpad

An in-app markdown text editor that leverages the existing versioning system for full document history. Phase 1 builds the core editor component. Phase 2 embeds it as a "Notes" scratchpad on all 11 entity detail views.

## User Review Required

> [!IMPORTANT]
> **Auto-save versioning granularity** — The current plan debounces auto-save at **60 seconds** of idle time after edits. Each auto-save creates a new version in the document history. A visual countdown timer shows time until next save. Is 60s the right interval, or would you prefer longer (e.g. 2–5 minutes) to avoid excessive version clutter?

> [!IMPORTANT]
> **Scratchpad folder** — The plan creates a system-managed `_Notes` folder to store all scratchpad documents. This keeps them organized but visible in the file manager. Alternatively, they could be stored at root level. Preference?

> [!IMPORTANT]
> **Archive behavior** — When archiving an active note, the plan renames it with a timestamp suffix (e.g. `Client - Acme Corp Notes (2026-03-20).md`) and creates a fresh document. The archived file stays in the same folder with full version history. Does that approach work?

---

## Proposed Changes

### Phase 1 — Core Markdown Editor Component

---

#### Server-Side

##### [NEW] `PUT api/FileManager/Documents/{id}/Content` endpoint
Add a new endpoint to [FileManagerController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FileManagerController.cs) that accepts raw text content and saves it as a new version. This avoids the overhead of multipart form uploads for text content.

- Accepts `{ content: string }` JSON body
- Converts to byte array, calls existing `UploadNewVersionAsync` internally
- Returns updated `DocumentDTO`

##### [NEW] `GET api/FileManager/Documents/{id}/Content` endpoint
Add endpoint to retrieve document content as plain text string (not blob download).

- Returns `{ content: string, versionNumber: number }`
- Only serves `text/*` and `application/json` MIME types (rejects binary files)

---

#### Client-Side Service

##### [MODIFY] [file-manager.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/services/file-manager.service.ts)

Add two new methods:
- `getDocumentContent(documentId: number): Observable<{ content: string; versionNumber: number }>` — fetches text content
- `saveDocumentContent(documentId: number, content: string): Observable<DocumentDTO>` — saves new version via PUT

---

#### Client-Side Component

##### [NEW] `fm-text-editor` component
New reusable component at `components/file-manager/fm-text-editor/`.

**Inputs:**
- `documentId: number` — the document to edit
- `autoSaveInterval: number` — debounce time in seconds (default 60)
- `readOnly: boolean` — view-only mode

**Outputs:**
- `saved: EventEmitter<DocumentDTO>` — emitted after each save
- `contentChanged: EventEmitter<boolean>` — dirty/clean state

**Files:**
- `fm-text-editor.component.ts` — Core logic: load content, track dirty state, auto-save with debounce timer, manual save, version history integration
- `fm-text-editor.component.html` — Editor UI with toolbar, textarea/contenteditable area, status bar with save countdown, version history panel
- `fm-text-editor.component.scss` — Editor styling: monospace font, line numbers (optional), dark/light theme support

**UI Layout:**
```
┌─ Toolbar ──────────────────────────────────────┐
│  💾 Save Now  │  ↶ Undo  │  ↷ Redo  │  📋 Versions  │  👁 Preview  │
├────────────────────────────────────────────────┤
│                                                │
│   Markdown editing area                        │
│   (textarea with monospace font)               │
│                                                │
│                                                │
├─ Status Bar ───────────────────────────────────┤
│  ✓ Saved v3  │  Ln 42, Col 8  │  ⏱ 45s  │   │
└────────────────────────────────────────────────┘
```

**Auto-save logic:**
1. On each keystroke, mark dirty and reset a debounce timer (default 60s)
2. Status bar shows countdown: `⏱ 45s` → `⏱ 44s` → ...
3. When timer reaches 0, call `saveDocumentContent()` → new version created
4. On response, update saved version number, clear dirty flag
5. "Save Now" button triggers immediate save and resets timer
6. Warn on navigation away if dirty (via `CanDeactivate` guard or `beforeunload`)

**Markdown Preview:**
- Toggle button switches between edit and side-by-side preview
- Uses a lightweight markdown-to-HTML renderer (we can use `marked` or build a simple one)
- Preview renders in a styled `<div>` alongside the editor

---

#### File Manager Integration

##### [MODIFY] [file-manager.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/file-manager.component.ts)
- Add "Edit" option in document context menu and detail panel for `text/*` and `text/markdown` documents
- On edit, open the `fm-text-editor` component (either inline replacing the detail panel, or as a modal/full-screen overlay)

##### [MODIFY] [file-manager.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/file-manager.component.html)
- Add the editor overlay/panel conditional rendering

---

### Phase 2 — Entity Scratchpad

---

#### Server-Side

##### [NEW] `GET api/FileManager/Scratchpad/{entityType}/{entityId}` endpoint
Returns the active scratchpad document for an entity, or `404` if none exists yet.

##### [NEW] `POST api/FileManager/Scratchpad/{entityType}/{entityId}` endpoint
Creates a new scratchpad document:
- Creates a `_Notes` system folder if it doesn't exist
- Creates a `text/markdown` document named `{EntityType} - {EntityName} Notes.md`
- Links it to the entity via the appropriate FK field
- Returns the new `DocumentDTO`

##### [NEW] `POST api/FileManager/Scratchpad/{entityType}/{entityId}/Archive` endpoint
Archives the current active scratchpad:
- Renames with timestamp suffix
- Creates a fresh empty scratchpad document linked to the entity
- Returns the new `DocumentDTO`

---

#### Client-Side Component

##### [NEW] `fm-scratchpad` component
Wrapper component at `components/file-manager/fm-scratchpad/`.

**Inputs:**
- `entityType: string` — e.g. `'Client'`, `'Contact'`, `'Resource'`
- `entityId: number` — the entity's ID
- `entityName: string` — display name for the header

**Behavior:**
1. On init, call `GET Scratchpad/{entityType}/{entityId}`
2. If 404 → show "Create Notes" button
3. If found → load `fm-text-editor` with the document ID
4. "Archive & Start Fresh" button calls the archive endpoint

**UI:**
```
┌─ 📝 Notes ─────────────────────────────────────┐
│  [Archive & Start New]            v3 saved ✓   │
├────────────────────────────────────────────────┤
│                                                │
│  (fm-text-editor embedded here)                │
│                                                │
└────────────────────────────────────────────────┘
```

---

#### Entity Detail Integration

##### [MODIFY] 11 entity detail component HTML files
Add a "Notes" tab (or section) to each entity detail component that uses `app-event-document-panel`:

| Component | Owner Field |
|-----------|-------------|
| [client-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/client-custom/client-custom-detail/client-custom-detail.component.html) | `clientId` |
| [contact-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/contact-custom/contact-custom-detail/contact-custom-detail.component.html) | `contactId` |
| [resource-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/resource-custom/resource-custom-detail/resource-custom-detail.component.html) | `resourceId` |
| [office-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/office-custom/office-custom-detail/office-custom-detail.component.html) | `officeId` |
| [crew-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/crew-custom/crew-custom-detail/crew-custom-detail.component.html) | `crewId` |
| [scheduling-target-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduling-target-custom/scheduling-target-custom-detail/scheduling-target-custom-detail.component.html) | `schedulingTargetId` |
| [volunteer-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-detail/volunteer-custom-detail.component.html) | `volunteerProfileId` |
| [invoice-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-detail/invoice-custom-detail.component.html) | `invoiceId` |
| [receipt-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/receipt-custom/receipt-custom-detail/receipt-custom-detail.component.html) | `receiptId` |
| [payment-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/payment-custom/payment-custom-detail/payment-custom-detail.component.html) | `paymentTransactionId` |
| [event-add-edit-modal](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-add-edit-modal/event-add-edit-modal.component.html) | `scheduledEventId` |

Each gets:
```html
<li ngbNavItem="notes">
  <button ngbNavLink><i class="fa-solid fa-pencil me-1"></i> Notes</button>
  <ng-template ngbNavContent>
    <fm-scratchpad [entityType]="'Client'" [entityId]="client!.id"
                   [entityName]="client!.name"></fm-scratchpad>
  </ng-template>
</li>
```

---

#### Module Registration

##### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts)
- Import and declare `FmTextEditorComponent` and `FmScratchpadComponent`

---

## Verification Plan

### Build
- `npx ng build --configuration=development` — zero new errors

### Manual Testing (Phase 1)
- Create a `.md` file via upload → open in editor → verify content loads
- Edit content → verify auto-save countdown → verify new version created
- Click "Save Now" → immediate save
- Toggle preview mode → verify markdown renders
- View version history → verify versions listed
- Navigate away with unsaved changes → verify dirty warning

### Manual Testing (Phase 2)
- Open Client detail → "Notes" tab → "Create Notes" → verify document created in `_Notes` folder
- Type content → auto-save → verify version history
- Archive → verify renamed with timestamp → new blank note created
- Verify scratchpad is visible in file manager flat mode with entity link
