# Session Information

- **Conversation ID:** 72a1dac3-0880-405e-913e-cf0052eaa90b
- **Date:** 2026-01-30
- **Time:** 12:48 NST (UTC-03:30)
- **Duration:** ~10 minutes

## Summary

Investigated and fixed an annoying UX issue where the login re-authentication modal would appear on top of the login page form when launching the app with an expired token. Added a route check in `openLoginModal()` to suppress the modal when already on `/login`.

## Files Modified

- `Scheduler/Scheduler.Client/src/app/app.component.ts` - Added route check in `openLoginModal()` to suppress modal on login page

## Related Sessions

None - this was a standalone investigation and fix.
