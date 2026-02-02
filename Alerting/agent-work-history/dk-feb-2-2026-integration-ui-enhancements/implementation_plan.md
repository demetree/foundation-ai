# Integration Management Enhancement - New Fields and Callback Event Types

Add support for new Integration fields and the IntegrationCallbackIncidentEventType child table to enable per-integration configuration of webhook retries and event type filtering.

## New Database Fields

| Field | Type | Purpose |
|-------|------|---------|
| `maxRetryAttempts` | `int?` | Max retry attempts for webhook callbacks |
| `retryBackoffSeconds` | `int?` | Seconds between retry attempts |
| `lastCallbackSuccessAt` | `DateTime?` | *Read-only* - Last success timestamp |
| `consecutiveCallbackFailures` | `int?` | *Read-only* - Failure count |

**Child Table:** `IntegrationCallbackIncidentEventType` links integrations to the incident event types that should trigger callbacks.

## Proposed Changes

### Backend - IntegrationManagementController

#### [MODIFY] [IntegrationManagementController.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Controllers/IntegrationManagementController.cs)

1. Update DTOs to include new fields:
   - `CreateIntegrationRequest` - add `maxRetryAttempts`, `retryBackoffSeconds`, `callbackEventTypeIds`
   - `UpdateIntegrationRequest` - add same fields
   - `IntegrationDto` - add new fields plus read-only `lastCallbackSuccessAt`, `consecutiveCallbackFailures`, and `callbackEventTypes` list

2. Update `CreateIntegration` to:
   - Set new fields on Integration entity
   - Create `IntegrationCallbackIncidentEventType` records for selected event types

3. Update `UpdateIntegration` to:
   - Update new fields
   - Sync child table (delete removed, add new event type mappings)

4. Update `MapToDto` to include new fields and event types

---

### Frontend - Angular Service

#### [MODIFY] [integration-management.service.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/services/integration-management.service.ts)

1. Update DTOs to match backend:
   - Add `maxRetryAttempts?`, `retryBackoffSeconds?`, `callbackEventTypeIds?` to request types
   - Add new fields and `callbackEventTypes` array to response types

---

### Frontend - Component Logic

#### [MODIFY] [integration-management.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/integration-management/integration-management.component.ts)

1. Add `IncidentEventTypeService` import and inject
2. Add form fields: `formMaxRetryAttempts`, `formRetryBackoffSeconds`, `formCallbackEventTypeIds`
3. Add `incidentEventTypes: IncidentEventTypeData[]` for dropdown population
4. Load event types in `ngOnInit()`
5. Update `resetForm()`, `openEditModal()`, `saveIntegration()` to handle new fields

---

### Frontend - Template

#### [MODIFY] [integration-management.component.html](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/integration-management/integration-management.component.html)

1. Add "Webhook Configuration" collapsible section after the Webhook URL field:
   - Max Retry Attempts (number input, optional)
   - Retry Backoff Seconds (number input, optional)



2. Add "Callback Event Types" section:
   - Checkbox list of available IncidentEventTypes
   - "Select All" / "Clear All" helpers
   - Help text explaining that only selected event types will trigger callbacks

---

## Verification Plan

### Build Verification
- Run `npm run build` in Alerting.Client to verify no TypeScript errors
- Run `dotnet build` in Alerting.Server to verify no C# errors

### Manual Testing
- Create new integration with retry settings and event type selections
- Edit existing integration to modify settings
- Verify event types are persisted and displayed correctly
