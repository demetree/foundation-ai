# MOC Visibility UI Patterns

Three visibility states exist — `Public`, `Unlisted`, `Private` — but the UI only surfaces Public MOCs. This plan adds discoverable paths for all states.

## Gaps Identified

| Gap | Current | Fix |
|-----|---------|-----|
| No way to find your own non-Public MOCs | Explore only queries `visibility == "Public"` | Add "My MOCs" section |
| Unlisted badge identical to Private | Both get lock icon | Distinct icon + color per state |
| Private MOC detail page leaks to anonymous | Only blocks at `GET /moc/{id}` | Already handled (returns 404) ✅ |
| Explore MOC cards have no visibility indicator | No badge on cards | Add visibility badge to "My MOCs" cards |

## Proposed Changes

### Server — New "My MOCs" Endpoint

#### [MODIFY] [MocHubController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/MocHubController.cs)

Add `GET /api/mochub/my-mocs` — authenticated endpoint:
- Query: `tenantGuid == userTenantGuid && isPublished && active && !deleted` (no visibility filter)
- Returns all the user's MOCs regardless of visibility state
- Includes `visibility` in the response so the UI can badge them
- Sorted by `publishedDate` descending

---

### Client — Explore Page "My MOCs" Section

#### [MODIFY] [mochub-explore.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/mochub-explore/mochub-explore.component.ts)
- Add `myMocs: any[]`, `myMocsLoading: boolean` properties
- Call `loadMyMocs()` on init if logged in → `GET /api/mochub/my-mocs`

#### [MODIFY] [mochub-explore.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/mochub-explore/mochub-explore.component.html)
- Add a "Your MOCs" section above the sort bar (only when logged in and has MOCs)
- Horizontal scrollable row of compact MOC cards with **visibility badges**:
  - 🌐 **Public** — green badge
  - 👁‍🗨 **Unlisted** — amber badge
  - 🔒 **Private** — red badge

#### [MODIFY] [mochub-explore.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/mochub-explore/mochub-explore.component.scss)
- Styles for the "Your MOCs" row, compact cards, and visibility badge colors

---

### Client — Repo Detail Visibility Badge

#### [MODIFY] [mochub-repo.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/mochub-repo/mochub-repo.component.html)
- Change the visibility badge to use three distinct states instead of binary Public/non-Public:
  - `Public` → globe icon, green tint
  - `Unlisted` → eye-slash icon, amber tint  
  - `Private` → lock icon, red tint

#### [MODIFY] [mochub-repo.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/mochub-repo/mochub-repo.component.scss)
- Add `.unlisted` badge class with amber styling

---

## Verification Plan

### Manual Verification
1. Log in, navigate to MOCHub explore — verify "Your MOCs" section appears with all owned MOCs
2. Change a MOC to Unlisted via Settings — confirm it shows in "Your MOCs" with amber badge but not in the public grid
3. Change a MOC to Private — confirm amber→red badge, still in "Your MOCs"
4. Log out — confirm "Your MOCs" section disappears
5. Navigate to an Unlisted MOC's detail page by direct link — verify it loads with eye-slash badge
