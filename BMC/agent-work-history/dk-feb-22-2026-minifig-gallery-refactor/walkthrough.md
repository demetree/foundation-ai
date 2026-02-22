# Minifig Gallery — Refactor Walkthrough

## Goal
Replace the server-paginated minifig gallery with a precomputed server endpoint, IndexedDB caching, and CDK virtual scroll — the same pattern used for the set-explorer.

## Changes Made

### Server (3 files)

| File | Action |
|------|--------|
| [MinifigGalleryService.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Services/MinifigGalleryService.cs) | **NEW** — `BackgroundService` that loads all active minifigs at startup, derives year via `LegoSetMinifig` → `LegoSet` join (`MAX(year)`), projects to lean DTO, sorted year desc + name asc |
| [MinifigGalleryController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/MinifigGalleryController.cs) | **NEW** — `GET /api/minifig-gallery` returns precomputed list (<1ms) |
| [Program.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Program.cs) | **Modified** — registered `MinifigGalleryService` as singleton + hosted service |

### Client (4 files)

| File | Action |
|------|--------|
| [minifig-gallery-api.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/minifig-gallery-api.service.ts) | **NEW** — fetches from `/api/minifig-gallery`, cached in IndexedDB (24h TTL) |
| [minifig-gallery.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/minifig-gallery/minifig-gallery.component.ts) | **Rewritten** — loads all minifigs once, client-side filter/sort, CDK virtual scroll, responsive columns |
| [minifig-gallery.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/minifig-gallery/minifig-gallery.component.html) | **Rewritten** — `cdk-virtual-scroll-viewport` + `*cdkVirtualFor`, sort controls (year/name/parts/fig#), year badges, no pagination |
| [minifig-gallery.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/minifig-gallery/minifig-gallery.component.scss) | **Rewritten** — viewport/card-row/sort-bar styles, removed pagination |

### Key Design Decisions

- **Year is derived, not stored.** `LegoMinifig` has no year column. The server joins through `LegoSetMinifigs` → `LegoSets` and takes `MAX(year)`. Minifigs not linked to any set get `Year = 0`.
- **Default sort: year desc, name asc** — shows the newest minifigs first.
- **No theme filter** — unlike set-explorer, minifigs don't have a direct theme association.

### IndexedDB Cache Inventory

| Store Name | Source | TTL |
|------------|--------|-----|
| `minifig-gallery` | `/api/minifig-gallery` | 24h |

## Verification

- ✅ Server build — 0 CS errors
- ✅ Client build — 0 TS errors, bundle generated (615 kB gzipped)
