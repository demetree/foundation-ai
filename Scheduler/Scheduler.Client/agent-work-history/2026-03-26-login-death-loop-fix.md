# Login Death Loop Fix

**Date:** 2026-03-26

## Summary

Fixed a login death loop that occurred when restarting the VS debugger while a browser tab from a previous session was still open. The stale tokens in localStorage triggered a cascade of failed refresh attempts that locked the user out of the login form.

## Changes Made

- **`Scheduler.Client/src/app/services/auth.service.ts`**
  - `startTokenRefreshTimer()`: Added early-exit when the stored token expiry is already in the past. Instead of scheduling a `setTimeout` with a negative delay (which fires immediately and triggers a doomed refresh), the method now calls `logout()` and returns — cleanly clearing stale credentials.
  - `refreshLogin()`: Removed the `reLogin()` call from the `catchError` handler. Callers (`startTokenRefreshTimer`, `SecureEndpointBase.handleError`) already handle re-login after exhausting retries, so the duplicate call was creating a compounding loop.

## Key Decisions

- **`logout()` only, no `reLogin()` in the expired-token path**: Calling `reLogin()` during `AuthService` constructor initialization (before `AppComponent.ngOnInit` sets up the `reLoginDelegate`) would attempt to open a modal on an un-rendered DOM. Instead, `logout()` clears localStorage, emits `loginStatus = false`, and the `AuthGuard` naturally redirects to `/login` on the next navigation.
- **Kept the `SecureEndpointBase` retry mechanism unchanged**: The 3-retry logic in `handleError()` is still valid for transient 401s during normal operation. The fix prevents the stale-token scenario from reaching that code path at all.

## Testing / Verification

- Manual verification: Start debugger → log in → stop debugger → restart debugger with browser open. The app should silently clear stale tokens and show a clean login page without "Session Expired" toast floods or redirect loops.
