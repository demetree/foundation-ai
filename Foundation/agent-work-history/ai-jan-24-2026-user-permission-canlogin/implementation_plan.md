# User Permission Fields and canLogin Authorization

## Problem Statement

Two gaps identified in the user security management:

1. **Permission levels not visible** in user detail view - `readPermissionLevel` and `writePermissionLevel` fields exist in the add/edit form but aren't displayed on the user overview tab.

2. **canLogin not enforced** - The `AuthorizationController.Exchange()` method only checks `active` and `deleted` flags but does NOT check `canLogin`. Users with `canLogin == false` can still obtain auth tokens.

---

## Proposed Changes

### Foundation.Client (Angular)

#### [MODIFY] [user-overview-tab.component.html](file:///G:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/user-custom/user-overview-tab/user-overview-tab.component.html)

Add permission level display in the "Account Settings" card (after "Failed Login Count"):

```html
<!-- Permission Levels -->
<div class="info-item">
    <span class="info-label">Read Permission</span>
    <span class="info-value">{{ user.readPermissionLevel }}</span>
</div>
<div class="info-item">
    <span class="info-label">Write Permission</span>
    <span class="info-value">{{ user.writePermissionLevel }}</span>
</div>
```

---

### FoundationCore.Web (C#)

#### [MODIFY] [AuthorizationController.cs](file:///G:/source/repos/Scheduler/FoundationCore.Web/Controllers/Security/AuthorizationController.cs)

Add `canLogin` checks at three locations where user state is validated:

**1. Password Grant flow (after line 106):**

```diff
 if (securityUser.active == false || securityUser.deleted == true)
 {
     return GetForbidResult("The user cannot login.");
 }
+
+if (securityUser.canLogin == false)
+{
+    return GetForbidResult("The user account is not permitted to login.");
+}
```

**2. Refresh Token flow (after line 152):**

```diff
 if (securityUser.active == false || securityUser.deleted == true)
 {
     return GetForbidResult("The specified user account is disabled.");
 }
+
+if (securityUser.canLogin == false)
+{
+    return GetForbidResult("The user account is not permitted to login.");
+}
```

**3. Extension Grant flow (after line 223):**

```diff
 if (securityUser.active == false || securityUser.deleted == true)
 {
     return GetForbidResult("The specified user account is disabled.");
 }
+
+if (securityUser.canLogin == false)
+{
+    return GetForbidResult("The user account is not permitted to login.");
+}
```

---

## Verification Plan

### Automated Build Verification

**1. Build Foundation.Client:**
```powershell
cd G:\source\repos\Scheduler\Foundation\Foundation.Client
npm run build
```
Expected: Build succeeds with no errors.

**2. Build FoundationCore.Web:**
```powershell
cd G:\source\repos\Scheduler\FoundationCore.Web
dotnet build
```
Expected: Build succeeds with no errors.

### Manual Verification

1. **Permission Levels Display:**
   - Run Foundation.Server
   - Navigate to Admin → Users
   - Click on any user to view the detail page
   - Confirm "Read Permission" and "Write Permission" values appear in the Account Settings card

2. **canLogin Enforcement (requires test user):**
   - Set a test user's `canLogin` to `false` in the database or via the UI
   - Attempt to log in as that user
   - Confirm login is rejected with appropriate error message
