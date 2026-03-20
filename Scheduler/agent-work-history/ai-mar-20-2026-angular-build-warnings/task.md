# Markdown Editor & Entity Scratchpad

## Phase 1 — Core Editor
- [x] Server: GET/PUT `Documents/{id}/Content` endpoints
- [x] Client: `getDocumentContent()` / `saveDocumentContent()` in service
- [x] `fm-text-editor` component (auto-save, preview, versions, undo/redo)
- [x] File manager integration (Edit context menu + overlay)
- [x] Module registration

## Phase 2 — Entity Scratchpad
- [x] Server: GET/POST/Archive `Scratchpad/{entityType}/{entityId}` endpoints
- [x] Client: `getScratchpad()` / `createScratchpad()` / `archiveScratchpad()` in service
- [x] `fm-scratchpad` component (create/archive lifecycle wrapper)
- [x] Notes tab/section in 10 entity detail views
- [x] Module registration

## Verification
- [x] Build verification (exit code 1 — pre-existing errors only, no new errors)
