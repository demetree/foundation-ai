# Finish Volunteer & Volunteer Group Components

## Volunteer Custom Detail — History Tab
- [x] Wire up `change-history-viewer` in the History tab (currently placeholder text)
- [x] Add `auditHistory` / `isLoadingHistory` fields to component TS
- [x] Add lazy `loadHistory()` method using `GetVolunteerProfileAuditHistory`
- [x] Update `onTabChange()` to trigger `loadHistory()` on 'history' tab switch

## Volunteer Group Custom Detail — History Tab
- [x] Wire up `change-history-viewer` in the History tab (currently placeholder text)
- [x] Add `auditHistory` / `isLoadingHistory` fields to component TS
- [x] Add lazy `loadHistory()` method using `GetVolunteerGroupAuditHistory`
- [x] Update `onTabChange()` to trigger `loadHistory()` on 'history' tab switch

## Volunteer Group Members Tab — Add/Remove Member
- [x] Create `volunteer-group-add-member-modal` component (modeled on `crew-add-to-crew-modal`)
- [x] Add "Add Member" button to members tab header
- [x] Add "Remove" actions to each member row
- [x] Wire up refresh after mutations
- [x] Register `VolunteerGroupAddMemberModalComponent` in `app.module.ts`
- [x] Fix permission check to use `VolunteerGroupService.userIsSchedulerVolunteerGroupWriter()`
- [x] Remove invalid `ResourceService.Instance.ReviveResource` calls

## Volunteer Group Members Tab — Link to Volunteer Profile
- [x] Members link to resource detail page via `navigateToVolunteer()`
- [x] Show volunteer status badge (active/left) on member rows

## Verification
- [x] Build compiles successfully (`ng build`) — exit code 0, warnings only
- [ ] Manual visual verification via browser
