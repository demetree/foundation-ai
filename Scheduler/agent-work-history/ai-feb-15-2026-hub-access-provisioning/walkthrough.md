# Hub Access Provisioning — Walkthrough

## Changes Made

### Server — [VolunteerHubController.cs](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Server/Controllers/VolunteerHubController.cs)

New endpoint `POST /api/volunteerhub/admin/provision-access`:
- Creates a SecurityUser with **random GUID password** (blocks admin login)
- Sets `description` to `"Volunteer Hub access provisioned for {firstName} {lastName}"`
- Configures 2FA delivery: email always, SMS if phone provided
- If a SecurityUser with the same email already exists, reuses it
- Links the volunteer profile via `linkedUserGuid`

### Client — Volunteer Add/Edit Form

| File | Change |
|------|--------|
| [volunteer-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-add-edit/volunteer-custom-add-edit.component.ts) | `HttpClient` import, hub access state, `provisionHubAccess()`, `removeHubAccess()`, `linkedUserGuid` in submit data |
| [volunteer-custom-add-edit.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-add-edit/volunteer-custom-add-edit.component.html) | Collapsible "Hub Access" panel with provision form / linked status display |

### UI States

- **Not linked**: Email + phone inputs + "Provision Access" button
- **Linked**: Green "Active" badge, account name, "Remove" button
- Panel only shows in edit mode (volunteer must be saved first)

## Verification

- ✅ `dotnet build` — Server compiles (exit code 0)
- ✅ `ng build --configuration development` — Client compiles (exit code 0)
