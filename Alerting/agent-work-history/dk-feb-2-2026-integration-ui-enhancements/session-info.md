# Session Information

- **Conversation ID:** 73023cc9-1c67-48b5-a022-fcfcdfa664d5
- **Date:** 2026-02-02
- **Time:** 14:10 NST (UTC-3:30)
- **Duration:** ~2 hours

## Summary

Updated the Integration Management UI to support new database fields (`maxRetryAttempts`, `retryBackoffSeconds`) and the `IntegrationCallbackIncidentEventType` child table. This enables granular selection of which incident event types trigger webhook callbacks for each integration.

## Files Modified

### Backend
- `Alerting.Server/Controllers/IntegrationManagementController.cs` - Updated DTOs, CRUD operations, and queries to handle retry settings and callback event type associations

### Frontend Service
- `Alerting.Client/src/app/services/integration-management.service.ts` - Updated DTO interfaces to match backend changes

### Frontend Component
- `Alerting.Client/src/app/components/integration-management/integration-management.component.ts` - Added form fields, event type loading, toggle helpers, and fixed edit modal to fetch full data from management API
- `Alerting.Client/src/app/components/integration-management/integration-management.component.html` - Added webhook retry settings section, callback event types checkbox list, replaced obsolete API Key column with Webhook column

## Key Changes

1. **New Form Fields**: Max retry attempts and retry backoff seconds (shown when webhook URL is configured)
2. **Callback Event Types**: Checkbox list to select which incident events trigger callbacks (with Select All/Clear All)
3. **Removed API Key Column**: Replaced with Webhook column showing configured URL (API keys are now hashed server-side and cannot be retrieved)
4. **Fixed Edit Modal**: Now fetches full data from management API instead of using incomplete listing data

## Related Sessions

This work is a continuation of the Integration table refactoring that added the new fields and child table to the database schema.
