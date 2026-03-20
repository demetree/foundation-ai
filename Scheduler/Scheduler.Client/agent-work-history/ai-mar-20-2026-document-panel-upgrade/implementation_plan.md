# Upgrade EventDocumentPanelComponent

The `EventDocumentPanelComponent` is a reusable document listing used in **11 entity detail views** (contact, client, office, crew, resource, scheduling target, volunteer, invoice, receipt, payment, scheduled event). It was built before the full Documents component existed and uses the auto-generated `DocumentService` with inline base64 data. This plan upgrades it to leverage existing `FileManagerService` infrastructure and align its UX with the main Documents component.

## Proposed Changes

### Phase 1 — Visual Enhancements

---

#### [MODIFY] [event-document-panel.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-document-panel/event-document-panel.component.ts)

**1a. Add thumbnails**
- Inject `FileManagerService`
- After loading documents, iterate through image/PDF documents and call `fetchThumbnailBlob()` to get blob URLs
- Store in a `Map<number, string>` of `thumbnailUrls` (same pattern used in `FileManagerComponent`)

**1b. Add version badge**
- The `DocumentData` object already includes `versionNumber` from the CRUD endpoint — just display it

**1c. Add "Open in Documents" navigation**
- Inject `Router`
- Add `openInDocuments(doc)` method that navigates to `/filemanager` with the document's folder path, e.g. `this.router.navigate(['/filemanager'], { queryParams: { docId: doc.id } })`

---

#### [MODIFY] [event-document-panel.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-document-panel/event-document-panel.component.html)

**1a. Thumbnails** — Replace the color icon square with a thumbnail `<img>` when one is available:
```html
<!-- Before: always shows icon -->
<div class="doc-type-indicator">
  <i class="fa-solid" [ngClass]="getFileIcon(doc.mimeType)"></i>
</div>

<!-- After: shows thumbnail if available, falls back to icon -->
<div class="doc-type-indicator" [class.has-thumbnail]="thumbnailUrls.has(doc.id)">
  <img *ngIf="thumbnailUrls.has(doc.id)" [src]="thumbnailUrls.get(doc.id)"
       class="doc-thumbnail" loading="lazy" />
  <i *ngIf="!thumbnailUrls.has(doc.id)" class="fa-solid"
     [ngClass]="getFileIcon(doc.mimeType)"></i>
</div>
```

**1b. Version badge** — Add version indicator next to the document name:
```html
<div class="doc-name">
  {{ doc.name }}
  <span class="badge doc-version-badge" *ngIf="doc.versionNumber > 1">
    v{{ doc.versionNumber }}
  </span>
</div>
```

**1c. "Open in Documents" button** — Add to the action buttons:
```html
<button class="btn btn-sm btn-action" (click)="openInDocuments(doc)"
        title="Open in Documents">
  <i class="fa-solid fa-up-right-from-square"></i>
</button>
```

---

#### [MODIFY] [event-document-panel.component.scss](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-document-panel/event-document-panel.component.scss)

- Add `.has-thumbnail` modifier to `.doc-type-indicator` with `overflow: hidden`
- Add `.doc-thumbnail` styles: `width: 100%; height: 100%; object-fit: cover; border-radius: 8px`
- Add `.doc-version-badge` styles: small, subtle pill badge

---

### Phase 2 — Text File Editing

---

#### [MODIFY] [event-document-panel.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-document-panel/event-document-panel.component.ts)

- Add `editingDocumentId` / `editingDocumentName` state properties
- Add `isTextFile(doc)` check (same logic as in `FileManagerComponent`)
- Add `openTextEditor(doc)` / `closeTextEditor()` / `onEditorSaved()` methods

#### [MODIFY] [event-document-panel.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-document-panel/event-document-panel.component.html)

- Add Edit button in `doc-actions` for text files (using `*ngIf="isTextFile(doc)"`)
- Add text editor overlay at the bottom of the template (reusing `<fm-text-editor>` child component):
```html
<div class="editor-overlay" *ngIf="editingDocumentId">
  <div class="editor-header">
    <button (click)="closeTextEditor()">← Back</button>
    <span>{{ editingDocumentName }}</span>
  </div>
  <fm-text-editor [documentId]="editingDocumentId"
                  (saved)="onEditorSaved($event)">
  </fm-text-editor>
</div>
```

#### [MODIFY] [event-document-panel.component.scss](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-document-panel/event-document-panel.component.scss)

- Add `.editor-overlay` styles (positioned over the panel content, similar to the Documents component overlay)

---

### Phase 3 — Migrate to FileManager Endpoints

---

#### [MODIFY] [event-document-panel.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-document-panel/event-document-panel.component.ts)

**3a. Migrate download/preview**
- Replace `downloadDocument()` (currently decodes inline base64 → blob) with a call to `FileManagerService.downloadDocument(doc.id)` which streams from the server
- Replace `openPreview()` to fetch blob via `FileManagerService.downloadDocument()` instead of reading `doc.fileDataData`
- This means previews work even when `fileDataData` is `null` (disk-based storage mode)

**3b. Migrate upload**
- Replace `uploadDocument()` which uses `DocumentService.PostDocument()` with inline base64 data
- Instead, use `FileManagerService.uploadDocuments()` or the chunked upload path for large files
- After upload, set the entity FK link by calling `FileManagerService.updateDocument()` with the `ownerField`
- *Alternative*: keep using `DocumentService.PostDocument()` for the metadata but switch to the File Manager's chunked upload for the binary data — depends on file size expectations for entity documents

> [!IMPORTANT]
> Phase 3 changes the underlying API calls from the auto-generated `DocumentService` (which embeds base64 inline) to `FileManagerService` (which uses streaming endpoints). This is a plumbing change — the UX stays the same but large files will work reliably. Since *all 11 entity detail views* share this component, this is a single-point fix.

---

## Verification Plan

### Build Verification
- `npx ng build --configuration=development` — must pass with exit code 0

### Manual Testing
1. **Thumbnails** — Open a contact/client with image documents, verify thumbnail previews appear in cards
2. **Version badge** — Upload a new version of a document via the full Documents component, return to the entity detail, verify `v2` badge appears
3. **Open in Documents** — Click the external link icon, verify navigation to `/filemanager` with the document visible
4. **Text editing** — Attach a `.md` or `.txt` file, click Edit, verify the editor opens and saves correctly
5. **Download** — Download a document, verify the file streams correctly (not base64 decoded)
6. **Upload** — Upload a file >1MB via the entity panel, verify it succeeds (would have failed with base64 for very large files)
