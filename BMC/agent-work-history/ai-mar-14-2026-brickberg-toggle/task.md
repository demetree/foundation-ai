# Brickberg Terminal Toggle

## Research
- [x] Run onboarding workflow
- [x] Identify where Brickberg is surfaced (sidebar, welcome, set-detail, dashboard)
- [x] Identify user preference storage (Foundation UserSettings API)

## Planning
- [x] Create implementation plan
- [x] Get user approval (approved with UserSettings API change)

## Implementation
- [x] Create `BrickbergPreferenceService` (`brickberg-preference.service.ts`)
- [x] Modify **sidebar** — filter Brickberg nav item (`requiresBrickberg` + `getVisibleItems()`)
- [x] Modify **welcome** — gate pathway card + filter feature strip
- [x] Modify **set-detail** — gate terminal section + API call
- [x] Modify **profile-settings** — add Features card with toggle switch
- [x] Modify **brickberg-dashboard** — soft landing when disabled + gate data loading
- [x] Add disabled-page SCSS styles to brickberg-dashboard

## Verification
- [/] Run `ng build` — verify compilation
- [ ] Update walkthrough
