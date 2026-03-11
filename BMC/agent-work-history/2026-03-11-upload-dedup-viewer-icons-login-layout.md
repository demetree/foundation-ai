# Upload De-duplication, Viewer Icon Fix & Login Layout

**Date:** 2026-03-11

## Summary

Added duplicate name handling for model uploads, fixed blank icons in the MOC 3D viewer, and resolved login page layout issues (scrollbar, blank header bar, offset background image).

## Changes Made

### Server — `BMC.Server`

- **`Services/ModelImportService.cs`**
  - Added `GetUniqueProjectNameAsync()` — checks `UC_Project_tenantGuid_name` constraint before insert, appends ` (2)`, ` (3)` etc. if name exists
  - Added `GetUniqueSubmodelNameAsync()` — same pattern for `UC_Submodel_tenantGuid_name` constraint
  - Both are called in `CreateProjectFromModelsAsync` before entity creation
  - Both include a safety cap at suffix 1000 (falls back to random hex string)

### Client — `BMC.Client`

- **`src/app/components/moc-viewer/moc-viewer.component.html`**
  - Replaced all Bootstrap Icons (`bi bi-*`) with Font Awesome (`fas fa-*`) — Bootstrap Icons CSS was never imported, causing all header/sidebar/step-control buttons to render blank

- **`src/app/components/moc-viewer/moc-viewer.component.scss`**
  - Changed `:host` height from `100vh` to `calc(100vh - var(--header-height, 56px))` to prevent scrollbar
  - Added negative margins and width override to counteract `.app-main` padding for edge-to-edge viewer

- **`src/app/app.component.html`**
  - Added `[class.on-login-page]="isOnLoginPage"` to `.app-body` and `.app-main` elements

- **`src/app/app.component.scss`**
  - Added `.app-body.on-login-page` rule: `margin-top: 0` (removes blank header bar)
  - Added `.app-main.on-login-page` rule: `padding: 0`, `min-height: 100vh`, `overflow: hidden`, and `&::after { top: 0 }` (fills background image edge-to-edge)

- **`src/app/components/login/login.component.scss`**
  - Changed `.login-page` from `min-height: 100vh` to `height: 100vh` with `overflow: hidden` for proper centering without scrollbar

## Key Decisions

- **Font Awesome over Bootstrap Icons**: BMC loads `@fortawesome/fontawesome-free` globally via `angular.json` — Bootstrap Icons was installed as a dependency but its CSS was never imported, so all `bi` class icons were invisible
- **Negative margin pattern**: Used `margin: -28px -32px` with `width: calc(100% + 64px)` to break the viewer out of `.app-main` padding, rather than modifying the shared app layout
- **Login page layout via class toggle**: Added `on-login-page` class rather than route-based CSS selectors, since `isOnLoginPage` was already computed in the app component

## Testing / Verification

- Confirmed `@fortawesome/fontawesome-free/css/all.min.css` is loaded in `angular.json` (lines 43, 122)
- Confirmed no other BMC component uses `bi bi-*` classes — all use `fas fa-*`
- Verified `UC_Project_tenantGuid_name` and `UC_Submodel_tenantGuid_name` unique constraints exist in `BMCContext.cs`
- User confirmed: login card centered, no scrollbar, background fills page, viewer icons visible
