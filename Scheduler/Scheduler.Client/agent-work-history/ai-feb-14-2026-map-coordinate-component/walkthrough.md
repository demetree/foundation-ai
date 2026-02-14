# Map Component Walkthrough

## Overview
Implemented a reusable Leaflet-based `LocationMapComponent` and integrated it across all entities that have latitude/longitude fields: **Office**, **Client**, and **TenantProfile**.

## Changes Made

### New Files
- [location-map.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/location-map/location-map.component.ts) — Core map component using Leaflet + OpenStreetMap
- [location-map.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/location-map/location-map.component.html) — Template with map container and overlay prompts
- [location-map.component.scss](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/location-map/location-map.component.scss) — Styles for map wrapper and overlays

### Modified Files

| File | Change |
|------|--------|
| [angular.json](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/angular.json) | Added `leaflet.css` to global styles |
| [app.module.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts) | Imported, declared, and exported `LocationMapComponent` |
| [office-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/office-custom/office-custom-add-edit/office-custom-add-edit.component.ts) | Added `mapLatitude`/`mapLongitude` tracking + coordinate handler |
| [office-custom-add-edit.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/office-custom/office-custom-add-edit/office-custom-add-edit.component.html) | Added editable map section after Address |
| [office-overview-tab.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/office-custom/office-overview-tab/office-overview-tab.component.html) | Added read-only map card |
| [client-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/client-custom/client-custom-add-edit/client-custom-add-edit.component.ts) | Added `mapLatitude`/`mapLongitude` tracking + coordinate handler |
| [client-custom-add-edit.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/client-custom/client-custom-add-edit/client-custom-add-edit.component.html) | Replaced plain lat/lng inputs with editable map |
| [client-overview-tab.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/client-custom/client-overview-tab/client-overview-tab.component.html) | Added read-only map card |
| [add-tenant-profile.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/add-tenant-profile/add-tenant-profile.component.ts) | Added coordinate handler |
| [add-tenant-profile.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/add-tenant-profile/add-tenant-profile.component.html) | Added editable map section after Address |
| [administration.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/administration/administration.component.html) | Added read-only map in Company Profile tab |

### Skipped
- **SchedulingTargetAddress** — auto-generated components that would be overwritten by code generation

## Component Features
- **Editable mode**: Click map to place/move marker, drag marker to reposition. Emits `coordinatesChanged` event
- **Read-only mode**: Displays marker at saved coordinates with no interaction
- **No coordinates**: Shows world view with "Click to set location" prompt (editable) or hides entirely (read-only overview cards)
- **OpenStreetMap tiles**: No API key required

## Verification
- ✅ `ng build --configuration development` completed with exit code 0
- Only unrelated optional-chain warnings in `SystemHealthComponent`
