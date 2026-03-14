# Manual Editor — LPub3D Feature Parity

## Phase 1: Quick UI Wins (expose existing data)
- [ ] Part count badge on step cards (deferred — needs BuildStepPart count API)
- [x] Callout badge on callout steps
- [x] Page background colour from backgroundColorHex
- [x] Fade step toggle in step properties
- [x] PLI toggle in step properties
- [x] Callout info section in properties panel
- [ ] Cover page indicator (deferred — hasCoverPage not on DTO)
- [ ] Page orientation (deferred — landscape not on DTO)

## Phase 2: PLI Image Generation
- [x] Add RenderPartThumbnail to RenderService
- [x] Add PLI grid composition helper (RenderPliGrid)
- [x] Integrate Step 8d into ModelImportService
- [x] Show PLI image in manual editor UI
- [x] Add PLI SCSS styles
- [ ] BOM page generation
- [ ] PDF export
- [ ] Step drag-and-drop reorder
