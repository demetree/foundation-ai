# UserService API Integration Walkthrough

## Summary
Exposed the existing `UserService` via REST API endpoints and updated the Escalation Policy Editor to use a user dropdown instead of manual GUID input.

## Changes Made

### Backend

#### [NEW] [UsersController.cs](file:///g:/source/repos/Scheduler/Alerting/Alerting.Server/Controllers/UsersController.cs)

Created a new controller that exposes the `IUserService` functionality via REST API:

| Endpoint | Description |
|----------|-------------|
| `GET /api/users` | List all users with Alerting module access in the current tenant |
| `GET /api/users/{userGuid}` | Get a specific user by GUID |
| `GET /api/teams` | List all teams in the current tenant |
| `GET /api/teams/{teamGuid}` | Get a specific team by GUID |
| `GET /api/teams/{teamGuid}/users` | Get all users in a specific team |

**Key Features:**
- Extends `SecureWebAPIController` with proper authorization
- Uses `GetSecurityUserAsync` and `UserTenantGuidAsync` for tenant scoping
- DTOs with computed `DisplayName` field for frontend convenience
- Proper error handling and logging

---

### Frontend

#### [NEW] [alerting-user.service.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/services/alerting-user.service.ts)

Angular service providing access to user and team API endpoints:

```typescript
@Injectable({ providedIn: 'root' })
export class AlertingUserService extends SecureEndpointBase {
    // Extends SecureEndpointBase for auth handling
    // Uses authService.GetAuthenticationHeaders() for requests
}

export interface AlertingUser {
    objectGuid: string;
    displayName: string;
    // ... other fields
}

export interface AlertingTeam {
    objectGuid: string;
    name: string;
    description: string;
}
```

**Authorization Pattern:**
- Extends `SecureEndpointBase` (consistent with other data services)
- Injects `BASE_URL` for API endpoint configuration
- Uses `this.authService.GetAuthenticationHeaders()` on all requests
- Includes `catchError` with `handleError` for proper 401/403 handling

---

#### [MODIFY] [escalation-policy-editor.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/escalation-policy-editor/escalation-policy-editor.component.ts)

render_diffs(file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/escalation-policy-editor/escalation-policy-editor.component.ts)

**Changes:**
1. Imported `AlertingUserService` and `AlertingUser`
2. Updated `users` property type to `AlertingUser[]`
3. Injected `AlertingUserService` in constructor
4. Updated `loadTargetOptions()` to fetch users from API
5. Added `onUserSelection()` handler for user dropdown changes

---

#### [MODIFY] [escalation-policy-editor.component.html](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/escalation-policy-editor/escalation-policy-editor.component.html)

render_diffs(file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/escalation-policy-editor/escalation-policy-editor.component.html)

**Changes:**
- Replaced text input for "User GUID" with a dropdown select populated from `users` array
- Displays `user.displayName` in dropdown options

---

## Verification

| Step | Result |
|------|--------|
| Backend build | ✅ Succeeded |
| Frontend build | ✅ Succeeded |

## Usage

The Escalation Policy Editor now shows a dropdown of users with their display names when configuring escalation rules. Users are displayed by name instead of requiring manual GUID entry.

## API Response Example

```json
// GET /api/users
[
  {
    "objectGuid": "12345678-1234-1234-1234-123456789012",
    "accountName": "jsmith",
    "firstName": "John",
    "lastName": "Smith",
    "displayName": "John Smith",
    "emailAddress": "jsmith@example.com",
    "cellPhoneNumber": "+1-555-0100",
    "teamGuid": null
  }
]
```
