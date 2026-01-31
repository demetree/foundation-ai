# Suppress Login Modal on Login Page

## Problem
When launching the system with an expired token, the user sees the login page form. Then a background auth check triggers a 401, causing the auth system to display a re-login modal **on top of** the already-visible login form. This is redundant and annoying.

## Root Cause Analysis

The flow is:
1. User lands on `/login` route with expired token
2. Some API request returns 401
3. `SecureEndpointBase.handleError()` tries to refresh the token
4. Refresh fails → calls `authService.reLogin()`
5. `reLogin()` triggers `reLoginDelegate` → `openLoginModal()` in `app.component.ts`
6. Modal appears **on top of** the existing login form

## Proposed Fix

### [MODIFY] [app.component.ts](file:///d:/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app.component.ts)

In the `openLoginModal()` method (line 185), add a route check to suppress the modal when already on the login page:

```diff
 openLoginModal() {

+  //
+  // Suppress the modal if user is already on the login page - no value in showing a modal over the login form
+  //
+  if (this.router.url === '/login') {
+    return;
+  }
+
   if (this.isloginModalShown == true) {
     return;
   }
```

This leverages the existing `router` which is already injected into the component.

## Verification Plan

### Manual Verification
1. Clear browser storage/cookies to simulate no active session
2. Navigate directly to `http://localhost:<port>/login`
3. Observe that **no modal popup** appears over the login form
4. Navigate to a protected route with an expired token → modal **should** appear as expected

> [!NOTE]
> Since this is UI behavior with expired tokens, automated testing would require mocking auth state. Manual verification is most practical here.
