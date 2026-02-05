# Session Information

- **Conversation ID:** 97719f41-dce7-406f-8f7a-d6757eb4de06
- **Date:** 2026-02-05
- **Time:** 11:46 NST (UTC-3:30)
- **Duration:** ~45 minutes

## Summary

Enhanced on-call visibility across the Alerting module by resolving actual user display names in the Command Center's "Who's On Call" panel and adding a new "On-Call Now" column to the schedule-management table.

## Files Modified

### alerting-overview (Command Center)
- `alerting-overview.component.ts` - Added `AlertingUserService` injection and `resolveUserName()` helper
- `alerting-overview.component.html` - Updated to use resolved names instead of generic "User"

### schedule-management (Schedule Listing)
- `schedule-management.component.ts` - Added user loading and on-call calculation logic (`loadOnCallNow()`, `getOnCallNow()`)
- `schedule-management.component.html` - Added "On-Call Now" column between Layers and Status
- `schedule-management.component.scss` - Added `.oncall-badge` styling

## Related Sessions

- This session also included UI consistency updates (icon floating animations) for service-management, integration-management, schedule-management, and schedule-editor components
