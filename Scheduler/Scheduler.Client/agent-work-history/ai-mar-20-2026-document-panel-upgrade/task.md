# Upgrade EventDocumentPanelComponent

## Phase 1 — Visual Enhancements
- [x] Add thumbnails to document cards
- [x] Add version badge to document names
- [x] Add "Open in Documents" navigation button
- [x] Add Phase 1 SCSS (thumbnail styles, version badge, etc.)

## Phase 2 — Text File Editing
- [x] Add text editor state, isTextFile check, open/close/save handlers
- [x] Add Edit button and fm-text-editor overlay to HTML
- [x] Add editor overlay SCSS

## Phase 3 — Migrate to FileManager Endpoints
- [x] Migrate download/preview from base64 to FileManagerService streaming
- [ ] Migrate upload from DocumentService base64 to FileManagerService

## Verification
- [ ] Angular build passes
