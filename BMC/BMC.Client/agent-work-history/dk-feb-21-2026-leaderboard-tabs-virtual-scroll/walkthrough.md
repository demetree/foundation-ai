# BMC Responsive UI Audit — Walkthrough

## What Was Done

Audited all 9 sidebar-linked components plus the layout shell (Sidebar, Header, App layout) for responsive design gaps. Made targeted SCSS additions to 7 files.

## Changes Made

### Layout Shell (3 files)

| File | Breakpoints Added | What Changed |
|------|------------------|--------------|
| [sidebar.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/sidebar/sidebar.component.scss) | 1024px, 768px | Tighter padding at tablet; hidden at mobile (safety net) |
| [header.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.scss) | 900px, 600px | Hide brand subtitle; shrink brand text; icon-only logout on phone |
| [app.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app.component.scss) | 1024px | Reduced main content padding from 28px 32px → 20px |

### Component-Level (4 files)

| File | Breakpoints Added | What Changed |
|------|------------------|--------------|
| [dashboard.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/dashboard/dashboard.component.scss) | 480px | Single-column stats grid, reduced welcome banner padding |
| [colour-library.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/colour-library/colour-library.component.scss) | 1024px, enhanced 768px | Detail panel `left` adapts to sidebar state; filter bar stacks vertically; search wrapper shrinks |
| [catalog-part-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.scss) | 900px, enhanced 600px | Sets table progressively hides columns (Colour at 900px, Set# at 600px); search input shrinks |
| [lego-universe.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.scss) | Enhanced 1024px | Quick-nav: 4-col → 2-col → 1-col smooth transition |

### Components Already Well-Covered (no changes needed)

- Set Explorer (900px + 600px)
- Parts Catalog (900px + 600px)
- My Collection (900px + 600px)
- Profile (768px + 480px)

### Overflow Truncation Fix (follow-up)

Both LEGO Universe and Catalog Part Detail were truncating on the right side. Root cause: `.app-main` had `flex: 1` but no `min-width: 0` — in CSS flex, children default to `min-width: auto` which prevents them from shrinking below content width.

| File | Fix |
|------|-----|
| [app.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app.component.scss) | Added `min-width: 0` + `overflow-x: hidden` to `.app-main` |
| [lego-universe.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.scss) | Added `overflow-x: hidden` on `:host`, `overflow: hidden` on container |
| [catalog-part-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.scss) | Added `overflow-x: hidden` on `:host`, `overflow: hidden` on container |

## Verification

- **Production build**: ✅ Passed (23.2s, no SCSS errors)
- Pre-existing Bootstrap warning about `.form-floating` selector — unrelated to our changes
