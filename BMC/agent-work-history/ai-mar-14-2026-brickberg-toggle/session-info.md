# Session Information

- **Conversation ID:** 2996df04-0762-40de-a750-03e5e95ea830
- **Date:** 2026-03-14
- **Time:** 09:07 NST (UTC-2:30)
- **Duration:** ~30 minutes

## Summary

Implemented a user-facing toggle for the Brickberg Terminal feature in BMC. The setting is stored server-side via the Foundation `UserSettings` API (`bmc-brickberg-enabled` key), enabling cross-device sync with no schema changes. Brickberg is disabled by default — users opt-in via Profile Settings.

## Files Modified

- **[NEW]** `BMC.Client/src/app/services/brickberg-preference.service.ts` — Reactive service wrapping UserSettings API
- `BMC.Client/src/app/components/profile-settings/profile-settings.component.ts` — Injected service, added toggle state
- `BMC.Client/src/app/components/profile-settings/profile-settings.component.html` — New "Features" card with toggle
- `BMC.Client/src/app/components/sidebar/sidebar.component.ts` — Nav item filtering via `requiresBrickberg`
- `BMC.Client/src/app/components/sidebar/sidebar.component.html` — Uses `getVisibleItems()` for filtered rendering
- `BMC.Client/src/app/components/welcome/welcome.component.ts` — Brickberg preference subscription
- `BMC.Client/src/app/components/welcome/welcome.component.html` — Pathway card + feature strip gated
- `BMC.Client/src/app/components/set-detail/set-detail.component.ts` — Terminal section + API call gated
- `BMC.Client/src/app/components/set-detail/set-detail.component.html` — Terminal `*ngIf` updated
- `BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.ts` — Soft landing + data load gating
- `BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.html` — Disabled page + conditional main content
- `BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.scss` — Disabled page styles

## Related Sessions

- This session also fixed the BMC pre-Angular loading SVG animation (removed static yellow studs) earlier in the conversation.
