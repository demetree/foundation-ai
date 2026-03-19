# File Manager Housekeeping — Theming, Drag-Drop Fix, Visual Consistency

**Date:** 2026-03-19

## Summary

Three housekeeping fixes for the File Manager component: aligned it with the `--sch-*` CSS variable theming system, fixed the broken drag-and-drop upload, and restructured the HTML/SCSS to match the standard component patterns used throughout the Scheduler application.

## Changes Made

### Modified Files
- `Scheduler.Client/src/app/components/file-manager/file-manager.component.scss` — Complete rewrite replacing ~30 hardcoded color values (`#1e2530`, `#212a36`, `#f8f9fa`, `#4a90d9`, etc.) with `--sch-*` CSS custom properties. Adopted `page-container` + `premium-header` layout pattern. Changed drag overlay from `*ngIf`-driven to always-in-DOM with CSS class toggle.
- `Scheduler.Client/src/app/components/file-manager/file-manager.component.html` — Restructured to standard `page-container` + gradient `premium-header` card (purple/indigo). Moved search into header with glass-input pattern. Replaced `*ngIf="isDragOver"` on drop overlay with `[class.active]="isDragOver"`. Added `(dragenter)` event binding. Removed `btn-close-white` in favor of theme-aware `var(--sch-close-btn-filter)`.
- `Scheduler.Client/src/app/components/file-manager/file-manager.component.ts` — Added `dragEnterCount` counter for reliable nested dragenter/dragleave handling. Added `onDragEnter()` method. Removed `isDragOver = true` from `onDragOver()` (moved to `onDragEnter()`). Counter resets to 0 on drop.

## Key Decisions

- **Drag-drop counter pattern** — The flicker was caused by `*ngIf` DOM insertion triggering recursive dragenter/dragleave events. Fix uses a `dragEnterCount` integer that increments on enter, decrements on leave, and only sets `isDragOver = false` when count reaches 0. The overlay also has `pointer-events: none` to avoid intercepting events.
- **Purple/indigo gradient** — Chose `#667eea → #764ba2` for the File Manager's `premium-header` to visually distinguish it from Resources (green), Documents (blue), and other existing components.
- **Kept sidebar structure** — The folder tree sidebar and split-panel layout are unique to this component and were retained, just restyled with theme tokens.

## Testing / Verification

- **Angular build** — 0 new errors or warnings from file-manager files. All build warnings are pre-existing (`VolunteerGroupOverviewTabComponent`, `SystemHealthComponent`, case-sensitivity).
- **Manual testing** — Pending: theme switching across all 5 themes, drag-and-drop file upload, and visual comparison with other components.
