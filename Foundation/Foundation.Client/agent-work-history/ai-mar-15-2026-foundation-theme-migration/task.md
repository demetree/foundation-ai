# Foundation.Client Theme Migration

## Phase 1 — Theme Foundation
- [x] Create `_themes.scss` with `--fnd-*` CSS custom property tokens
- [x] Create `theme.service.ts`
- [x] Import `_themes.scss` in `styles.scss` and replace hardcoded colors

## Phase 2 — Core Components
- [x] Migrate header.component.scss
- [x] Migrate sidebar.component.scss
- [x] Migrate app.component.scss
- [x] Migrate overview.component.scss
- [x] Wire ThemeService into app.component.ts
- [x] Add theme picker to header ✅
- [x] Theme-aware logo (white/black based on isDarkTheme) ✅

## Phase 3 — Custom Components (batch)
- [x] Batch-replace hardcoded page backgrounds in 35 SCSS files

## Phase 4 — Data Components
- [x] Confirmed code generator already has `applicationThemePrefix: "fnd"` ✅
- [x] Confirmed data component SCSS already contains `--fnd-*` tokens ✅

## Phase 5 — Verification
- [x] ng build — zero errors ✅ (both rounds)
