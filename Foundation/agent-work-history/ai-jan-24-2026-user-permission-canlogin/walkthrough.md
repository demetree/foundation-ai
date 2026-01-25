# User Permission Fields and canLogin Authorization - Walkthrough

## Summary

Added readPermissionLevel/writePermissionLevel display to user overview and enforced canLogin check in authorization.

---

## Changes Made

### 1. Permission Level Display (Angular)

**File:** [user-overview-tab.component.html](file:///G:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/user-custom/user-overview-tab/user-overview-tab.component.html)

Added Read Permission and Write Permission fields to the Account Settings card:

render_diffs(file:///G:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/user-custom/user-overview-tab/user-overview-tab.component.html)

---

### 2. canLogin Authorization Enforcement (C#)

**File:** [AuthorizationController.cs](file:///G:/source/repos/Scheduler/FoundationCore.Web/Controllers/Security/AuthorizationController.cs)

Added `canLogin == false` check at 3 authorization checkpoints (password grant, refresh token, extension grant):

render_diffs(file:///G:/source/repos/Scheduler/FoundationCore.Web/Controllers/Security/AuthorizationController.cs)

---

## Verification

| Test | Result |
|------|--------|
| FoundationCore.Web build | ✅ 0 errors |
| Foundation.Client build | ✅ 0 errors |

---

## Manual Verification Steps

1. Run Foundation.Server and navigate to Admin → Users → select any user
2. Confirm "Read Permission" and "Write Permission" appear in Account Settings
3. To test canLogin: set a user's canLogin to false, attempt login, verify rejection
