# Reusable Map Coordinate Component (Leaflet)

Add a shared Angular component that renders an interactive Leaflet map for viewing and editing latitude/longitude coordinates. Integrate it into the four entities that carry coordinate fields.

## Audit Results

Four tables in the database schema carry `latitude` / `longitude` (`double`, nullable):

| Table | Generator Line | Custom Components | Has Overview Tab? |
|---|---|---|---|
| **Office** | L1591–1592 | `office-custom/*` | ✅ Yes |
| **Client** | L1709–1710 | `client-custom/*` | ✅ Yes |
| **TenantProfile** | L1770–1771 | `add-tenant-profile` + `administration` | ✅ Admin Profile Tab |
| **SchedulingTargetAddress** | L2170–2171 | `scheduling-target-custom/*` | ❌ No |

Currently, all four use plain `<input type="number">` fields for lat/lng in their add/edit modals. Overview tabs display raw text values or nothing at all.

## User Review Required

> [!IMPORTANT]
> **New npm dependency**: This plan introduces `leaflet` (BSD-2-Clause, ~40 KB gzipped) and `@types/leaflet` (dev-only). Leaflet is a mature, zero-dependency mapping library that uses OpenStreetMap tiles by default — no API key required. Please confirm this is acceptable, or suggest an alternative (e.g., OpenLayers, MapLibre GL).

> [!NOTE]
> **TenantProfile** is managed via the `add-tenant-profile` modal (used from the Administration page). Its edit modal currently has full address fields but no lat/lng inputs. The Administration Company Profile tab shows address read-only — we'll add a read-only map there and lat/lng editing to the modal.

## Proposed Changes

### Shared Map Component (New)

#### [NEW] [location-map.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/location-map/location-map.component.ts)
#### [NEW] [location-map.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/location-map/location-map.component.html)
#### [NEW] [location-map.component.scss](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/location-map/location-map.component.scss)

A reusable component with these features:

- **Inputs**: `latitude`, `longitude` (nullable numbers), `editable` (boolean, default false), `height` (string, default `'300px'`)
- **Outputs**: `coordinatesChanged` emitting `{ latitude: number, longitude: number }`
- **Behavior**:
  - When `latitude` and `longitude` are both non-null, centers the map on that point with a draggable marker (if `editable == true`)
  - When coordinates are null/missing, shows a default world view with a prompt message
  - In edit mode, clicking the map or dragging the marker emits updated coordinates
  - Uses OpenStreetMap tile layer (free, no API key)
  - Marker uses Leaflet's default blue pin icon

---

### Package Installation

#### [MODIFY] [package.json](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/package.json)

Add `leaflet` to `dependencies` and `@types/leaflet` to `devDependencies`.

#### [MODIFY] [angular.json](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/angular.json)

Add Leaflet CSS to the project styles array: `"node_modules/leaflet/dist/leaflet.css"`.

---

### Office Integration

#### [MODIFY] [office-custom-add-edit.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/office-custom/office-custom-add-edit/office-custom-add-edit.component.html)

Replace the two separate lat/lng `<input type="number">` fields with the `<app-location-map>` component in edit mode, plus the original inputs kept as hidden/secondary fields for manual override if desired.

#### [MODIFY] [office-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/office-custom/office-custom-add-edit/office-custom-add-edit.component.ts)

Add handler for `coordinatesChanged` event to patch the form's lat/lng values.

#### [MODIFY] [office-overview-tab.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/office-custom/office-overview-tab/office-overview-tab.component.html)

Add a read-only `<app-location-map>` card below the Core Information card, visible only when coordinates are present.

---

### Client Integration

#### [MODIFY] [client-custom-add-edit.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/client-custom/client-custom-add-edit/client-custom-add-edit.component.html)

Same pattern as Office — replace plain inputs with `<app-location-map>` in edit mode.

#### [MODIFY] [client-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/client-custom/client-custom-add-edit/client-custom-add-edit.component.ts)

Add handler for `coordinatesChanged` event.

#### [MODIFY] [client-overview-tab.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/client-custom/client-overview-tab/client-overview-tab.component.html)

Add read-only map card visible when coordinates are present.

---

### Scheduling Target Address Integration

#### [MODIFY] [scheduling-target-address add/edit components](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/scheduler-data-components/scheduling-target-address)

Add `<app-location-map>` to the generated add-edit forms alongside the existing lat/lng fields. Since these are code-generated components, we should check whether custom overrides exist already.

---

### Tenant Profile Integration (Administration)

#### [MODIFY] [add-tenant-profile.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/add-tenant-profile/add-tenant-profile.component.html)

Add `<app-location-map>` in edit mode after the Address section in the modal (~line 191, after the Country field).

#### [MODIFY] [add-tenant-profile.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/add-tenant-profile/add-tenant-profile.component.ts)

Add handler for `coordinatesChanged` to update the submit data's lat/lng.

#### [MODIFY] [administration.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/administration/administration.component.html)

Add a read-only `<app-location-map>` below the Address section in the Company Profile tab (~line 132), visible when coordinates are present.

---

### Module Registration

#### [MODIFY] [app.module.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts)

Declare `LocationMapComponent` in the module's `declarations` array.

## Verification Plan

### Automated Tests
- `ng build` — confirms the project compiles with zero errors after all changes

### Manual Verification
1. **Office Add/Edit**: Navigate to Offices → click Edit on any office → confirm the map appears with the current coordinates (or shows a default view if no coordinates set). Click the map or drag the marker → confirm lat/lng input fields update automatically. Save → confirm coordinates persist.
2. **Office Overview Tab**: Navigate to an Office detail page → Overview tab → confirm read-only map card appears when coordinates exist, and is hidden when they're null.
3. **Client Add/Edit**: Same as Office flow but for Clients.
4. **Client Overview Tab**: Same as Office but for Clients.
5. **Scheduling Target Address**: Navigate to a Scheduling Target → Addresses tab → Edit an address → confirm map appears.
