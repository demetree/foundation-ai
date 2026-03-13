# MOCHub Phase 5 — Remaining Features

**Date:** 2026-03-13

## Summary

Implemented all five remaining MOCHub features: version diff viewer with 3D preview, fork network visualization, MOC settings page, markdown README rendering, and loading/error polish.

## Changes Made

### Server (BMC.Server)
- **MocVersioningService.cs** — Added `GenerateDiffMpdAsync` (color-coded diff MPD: green added, translucent red removed) and `GetVersionMpdAsync` (raw MPD snapshots)
- **MocHubController.cs** — Added `GET /api/mochub/moc/{id}/versions/{v}/mpd`, `GET /api/mochub/moc/{id}/versions/{v}/diff-mpd`, and `DELETE /api/mochub/moc/{id}` (owner-only unpublish via soft-delete using `GetSecurityUserAsync` + `UserTenantGuidAsync`)

### Client (BMC.Client)
- **mochub-repo.component.ts/html/scss** — Inline expandable diff panel in versions tab with Three.js 3D toggle, canvas, orbit controls, and color legend; redesigned forks tab with tree-layout clickable fork cards and lineage header; markdown README rendering via `marked` + `DomSanitizer`; settings gear link for owners
- **mochub-settings.component.ts/html/scss** — New settings page with general metadata form, visibility radio cards, license/forking toggles, and danger zone with name-confirmation unpublish
- **mochub-explore.component.ts/html/scss** — Added `errorMessage` state with error UI and retry button
- **app.module.ts** — Registered `MochubSettingsComponent`
- **app-routing.module.ts** — Added AuthGuard-protected route `/mochub/moc/:id/settings`
- **package.json** — Added `marked` dependency

## Key Decisions

- Diff MPD generation is handled server-side for efficiency; client renders pre-built colored MPD via Three.js LDrawLoader
- DELETE endpoint uses `SecureWebAPIController` base class pattern (`GetSecurityUserAsync` + `UserTenantGuidAsync`) rather than a separate auth service, matching codebase conventions
- Markdown rendering uses `bypassSecurityTrustHtml` with memoization to avoid re-parsing on every change detection cycle
- Settings route uses `AuthGuard` (not `PublicAccessGuard`) since only owners should access it

## Testing / Verification

- Angular client production build succeeds (CSS warning from Bootstrap `.form-floating>~label` is pre-existing)
- Server build: zero errors from `MocHubController.cs`; 44 pre-existing errors in other solution files confirmed unrelated via build output filtering
