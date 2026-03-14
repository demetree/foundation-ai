# Brickberg Terminal Toggle — Walkthrough

## What Changed

Users can now opt-in to the Brickberg Terminal via **Profile Settings → Features → Brickberg Terminal**. Disabled by default — this keeps the interface clean for casual collectors while making the full financial terminal available to power users.

The setting syncs across devices using the Foundation `UserSettings` API (`bmc-brickberg-enabled` key).

---

## Files Modified

### New Service
#### [brickberg-preference.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/brickberg-preference.service.ts)
Reads/writes `bmc-brickberg-enabled` via `GET/PUT /api/UserSettings/{key}`. Exposes `isEnabled$` (observable) and `isEnabled` (synchronous getter).

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/brickberg-preference.service.ts)

---

### Profile Settings — Toggle UI
#### [profile-settings.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/profile-settings/profile-settings.component.ts)
#### [profile-settings.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/profile-settings/profile-settings.component.html)

New **Features** card between Privacy and Social Links with a toggle matching the existing `isPublic` toggle pattern.

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/profile-settings/profile-settings.component.html)

---

### Sidebar — Nav Item Filtering
#### [sidebar.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/sidebar/sidebar.component.ts)
#### [sidebar.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/sidebar/sidebar.component.html)

Brickberg nav item tagged with `requiresBrickberg: true`, filtered via `getVisibleItems()`.

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/sidebar/sidebar.component.ts)

---

### Welcome — Pathway Card & Feature Strip
#### [welcome.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/welcome/welcome.component.ts)
#### [welcome.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/welcome/welcome.component.html)

Brickberg pathway card and feature link hidden with `*ngIf="brickbergEnabled"`.

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/welcome/welcome.component.html)

---

### Set Detail — Terminal Section
#### [set-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.ts)
#### [set-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.html)

Terminal section gated with `brickbergEnabled`, API call skipped when disabled.

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.ts)

---

### Brickberg Dashboard — Soft Landing
#### [brickberg-dashboard.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.ts)
#### [brickberg-dashboard.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.html)
#### [brickberg-dashboard.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.scss)

If disabled, shows a soft-landing card explaining Brickberg with a link to enable it in Profile Settings. Data loading is gated behind the preference.

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.html)

---

## Build Verification

`ng build` completed successfully — application bundle generated. The only diagnostic is a **pre-existing** CSS selector warning (`.form-floating>~label`) unrelated to these changes.
