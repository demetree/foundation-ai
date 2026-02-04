# Resolve Incident Feature

Add a "Resolve" button to the Foundation Incidents Report table allowing users to resolve incidents directly from Foundation Admin.

## Proposed Changes

### Foundation.Client

#### [NEW] `services/resolve-incident-dialog/resolve-incident-dialog.component.ts`
- Modal dialog with:
  - Incident title/key display
  - Optional textarea for resolution note
  - Confirm/Cancel buttons
- Returns `{ confirmed: boolean, resolution?: string }`

#### [NEW] `services/resolve-incident-dialog/resolve-incident-dialog.component.html`
- Premium styled modal matching existing dialogs

---

#### [MODIFY] `services/incidents.service.ts`
- Add `resolveIncident(incidentKey: string, resolution?: string): Observable<ResolveResponse>`

---

#### [MODIFY] `components/incidents-report/incidents-report.component.ts`
- Add `resolveIncident(incident)` method
- Open modal, call service on confirm, refresh list

#### [MODIFY] `components/incidents-report/incidents-report.component.html`
- Add "Resolve" button in table row (visible when status != 'Resolved')

#### [MODIFY] `components/incidents-report/incidents-report.component.scss`
- Button styling for resolve action

---

### Foundation.Server

#### [MODIFY] `Controllers/IncidentsController.cs`
- Add `POST /api/incidents/{incidentKey}/resolve` endpoint
- Calls `AlertingIntegrationService.ResolveIncidentAsync()`

---

### Alerting.Server (Already Done)
- `AlertingService.ResolveByKeyAsync()` already logs timeline event with `{ resolution, source: "API" }`
- Enhancement: Include integration name in the timeline event details

## Verification Plan

### Manual Testing
1. Navigate to Incidents Report
2. Click Resolve on a Triggered/Acknowledged incident
3. Confirm modal appears with optional note field
4. Enter optional note, click Confirm
5. Verify incident shows as Resolved
6. Check Alerting timeline shows resolution note and source
