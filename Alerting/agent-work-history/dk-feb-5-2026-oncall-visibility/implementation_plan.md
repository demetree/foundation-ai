# On-Call Visibility Enhancements

Improve the visibility of who is currently on-call across the Alerting module by showing actual user names instead of generic "User" text.

## User Review Required

> [!IMPORTANT]
> Both changes are frontend-only. The backend already returns the `userObjectGuid`—we just need to resolve the actual display names client-side using `AlertingUserService`.

## Proposed Changes

### Component: alerting-overview (Command Center)

#### [MODIFY] [alerting-overview.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/alerting-overview/alerting-overview.component.ts)

- Inject `AlertingUserService` 
- Load users on init and store in a Map<guid, displayName>
- Add helper method `resolveUserName(guid: string): string`

#### [MODIFY] [alerting-overview.component.html](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/alerting-overview/alerting-overview.component.html)

- Update the on-call panel to show resolved user names instead of `user.displayName` (which comes from backend as "User")
- Use `resolveUserName(user.userObjectGuid)` instead

---

### Component: schedule-management (Schedule Listing)

#### [MODIFY] [schedule-management.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/schedule-management/schedule-management.component.ts)

- Already has `AlertingUserService` injected
- Load users on init (if not already)
- Add `getCurrentOnCallForSchedule(schedule)` method that replicates the logic from `schedule-editor.getCurrentOnCall()`:
  - Get layers via Promise
  - Calculate which member is currently on-call based on rotation math
  - Return user display name

#### [MODIFY] [schedule-management.component.html](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/schedule-management/schedule-management.component.html)

- Add new "On-Call Now" column between "Layers" and "Status"
- Show the currently on-call user name with a person icon

#### [MODIFY] [schedule-management.component.scss](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/schedule-management/schedule-management.component.scss)

- Add styling for the new column

---

## Verification Plan

### Build Verification
- Run `npm run build` to ensure no compilation errors

### Visual Verification
- Navigate to Alerting Command Center and confirm actual user names appear in "Who's On Call" panel
- Navigate to On-Call Schedules listing and confirm "On-Call Now" column shows actual user names
