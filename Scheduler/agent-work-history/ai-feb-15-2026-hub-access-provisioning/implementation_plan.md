# Linked User — Hub Access Provisioning

Add a "Hub Access" section to the volunteer add/edit form that lets admins provision a SecurityUser for a volunteer. This creates the passwordless OTP login account for the Volunteer Hub.

## Proposed Changes

### Server — VolunteerHubController

#### [MODIFY] [VolunteerHubController.cs](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Server/Controllers/VolunteerHubController.cs)

Add a new endpoint `POST /api/volunteerhub/admin/provision-access`:

- **Input**: `{ volunteerProfileId, email, phone, firstName, lastName }`
- **Behaviour**:
  1. Look up existing SecurityUser by email (accountName). If found, reuse it
  2. If not found, create a new SecurityUser via `SecurityLogic.CreateLocalUserRecord()` pattern:
     - `accountName` = email
     - `emailAddress` = email
     - `cellPhoneNumber` = phone
     - `twoFactorSendByEmail` = `true`
     - `twoFactorSendBySMS` = phone is provided
     - No password (OTP-only)
  3. Update `VolunteerProfile.linkedUserGuid` to the SecurityUser's `objectGuid`
  4. Return the linked user GUID + account name

- This endpoint requires the caller to be an authenticated Scheduler admin (use existing JWT auth via `SecureWebAPIController` pattern or a simple admin check)

---

### Client — Volunteer Add/Edit Form

#### [MODIFY] [volunteer-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-add-edit/volunteer-custom-add-edit.component.ts)

- Add `isHubAccessPanelOpen` toggle
- Add fields: `hubEmail`, `hubPhone`, `isProvisioning`, `linkedUserInfo`
- Add `provisionHubAccess()` method → calls the new endpoint
- Add `removeHubAccess()` method → clears `linkedUserGuid` on save
- On `openModal(data)`: populate `linkedUserInfo` from `data.linkedUserGuid`
- Include `linkedUserGuid` in `submitForm()` data object

#### [MODIFY] [volunteer-custom-add-edit.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-add-edit/volunteer-custom-add-edit.component.html)

Add a new collapsible "Hub Access" panel (after Appearance, before Advanced) with:

- **When not linked**: Email + phone inputs + "Provision Access" button
- **When linked**: Show linked status (user GUID) + "Remove Link" button
- Badge shows "Active" when linked

#### [MODIFY] [volunteer-custom-add-edit.component.scss](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-add-edit/volunteer-custom-add-edit.component.scss)

Minor styling for the hub access section (success badge, provision button).

## Verification Plan

### Automated Tests
- `dotnet build` for server project
- `ng build` for Scheduler.Client

### Manual Verification
- Open volunteer add/edit modal → confirm "Hub Access" panel appears
- Provision access with email → verify SecurityUser created and `linkedUserGuid` populated
