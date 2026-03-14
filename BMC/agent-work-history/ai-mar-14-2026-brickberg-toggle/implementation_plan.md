# Brickberg Terminal Toggle Feature

Allow users to opt in/out of the Brickberg Terminal financial features. Users who don't care about LEGO market data get a cleaner, simpler UI.

> [!TIP]
> Uses Foundation's **UserSettings API** (`GET/PUT /api/UserSettings/{key}`) backed by the `SecurityUser.settings` JSON column — cross-device sync with zero schema changes.

## Proposed Changes

### New Service

#### [NEW] [brickberg-preference.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/brickberg-preference.service.ts)

Angular service that wraps the Foundation UserSettings API:
- **Setting key** = `bmc-brickberg-enabled` (stored as `"true"` / `"false"` string)
- **`isEnabled$`** — `BehaviorSubject<boolean>` for reactive bindings
- **`isEnabled`** — synchronous getter for `*ngIf`
- **`loadPreference()`** — calls `GET /api/UserSettings/bmc-brickberg-enabled` on init
- **`setEnabled(value)`** — calls `PUT /api/UserSettings/bmc-brickberg-enabled` with `{ Value: "true"/"false" }` and emits
- **Default = `false`** when setting doesn't exist yet

---

### Profile Settings (Toggle UI)

#### [MODIFY] [profile-settings.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/profile-settings/profile-settings.component.ts)

- Inject `BrickbergPreferenceService`
- Add `brickbergEnabled` boolean, load from service on init
- On toggle change → call `setEnabled()` immediately (no need to wait for Save)

#### [MODIFY] [profile-settings.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/profile-settings/profile-settings.component.html)

- New **"Features"** card between Privacy and Social Links with toggle switch:
  - **Label**: `Brickberg Terminal`
  - **Description**: `Enable the LEGO financial terminal — portfolio tracking, market data, and investment analytics`
  - Reuses existing `.toggle-switch` / `.toggle-slider` CSS

---

### Sidebar (Nav Link)

#### [MODIFY] [sidebar.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/sidebar/sidebar.component.ts)

- Inject `BrickbergPreferenceService`, expose `brickbergEnabled` getter

#### [MODIFY] [sidebar.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/sidebar/sidebar.component.html)

- Filter Brickberg nav item from TOOLS group when disabled

---

### Welcome Page

#### [MODIFY] [welcome.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/welcome/welcome.component.ts)

- Inject `BrickbergPreferenceService`, expose `brickbergEnabled` getter

#### [MODIFY] [welcome.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/welcome/welcome.component.html)

- Wrap `.brickberg-featured` card with `*ngIf="brickbergEnabled"`
- Filter Brickberg entry from `featureLinks` strip

---

### Set Detail (Terminal Section)

#### [MODIFY] [set-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.ts)

- Inject `BrickbergPreferenceService`
- Gate `loadBrickbergData()` — skip API call if disabled
- Expose `brickbergEnabled` getter

#### [MODIFY] [set-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.html)

- Wrap `<section class="brickberg-terminal">` with `*ngIf="brickbergEnabled"`

---

### Brickberg Dashboard (Soft Landing)

#### [MODIFY] [brickberg-dashboard.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.ts)

- Inject service, expose getter

#### [MODIFY] [brickberg-dashboard.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.html)

- When disabled: show friendly prompt with link to Profile Settings to enable

---

### App Module

#### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app.module.ts)

- Add `BrickbergPreferenceService` to providers

## Verification Plan

### Automated Tests
- `ng build` to verify clean compilation

### Manual Verification
- Toggle OFF → sidebar link hidden, welcome card hidden, set-detail terminal hidden, `/brickberg` shows enable prompt
- Toggle ON → everything appears as before
- Log in on another device → setting persists
