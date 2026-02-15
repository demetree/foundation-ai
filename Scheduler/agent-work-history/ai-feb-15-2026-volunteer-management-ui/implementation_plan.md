# Phase 1: Volunteer Management UI — Implementation Plan

Build custom UI components for managing volunteers, volunteer profiles, volunteer statuses, and volunteer groups. All patterns follow the established `crew-custom` and `resource-custom` component conventions.

## User Review Required

> [!IMPORTANT]
> **No server-side custom controllers or services are included in this phase.** All CRUD operations use the existing auto-generated `VolunteerProfileService`, `VolunteerGroupService`, `VolunteerGroupMemberService`, and `VolunteerStatusService`. Custom server-side logic (e.g., composite "create volunteer" endpoint that creates Resource + VolunteerProfile together) can be added in a follow-up if needed.

> [!IMPORTANT]
> **The "Volunteer Hours Dashboard" from the original roadmap is deferred to a follow-up.** It requires a custom server-side aggregation endpoint and is better built after the core CRUD screens are stable.

---

## Proposed Changes

### Navigation & Routing

#### [MODIFY] [sidebar.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/sidebar/sidebar.component.html)
Add two new nav items after the "Crews" entry (line 94):
- **Volunteers** → `/volunteers` with icon `fa-solid fa-hand-holding-heart`
- **Volunteer Groups** → `/volunteergroups` with icon `fa-solid fa-people-group`

#### [MODIFY] [header.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/header/header.component.html)
Add matching nav items for Volunteers and Volunteer Groups in the mobile header menu.

#### [MODIFY] [app-routing.module.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/app-routing.module.ts)
Add routes:
```typescript
{ path: 'volunteers', component: VolunteerCustomListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Volunteers' },
{ path: 'volunteers/:volunteerProfileId', component: VolunteerCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Volunteer Detail' },
{ path: 'volunteergroups', component: VolunteerGroupCustomListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Volunteer Groups' },
{ path: 'volunteergroups/:volunteerGroupId', component: VolunteerGroupCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Volunteer Group Detail' },
```

#### [MODIFY] [app.module.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts)
Import and declare all new components (12 total).

---

### Volunteer Profile Components

Component group: `components/volunteer-custom/`

#### [NEW] [volunteer-custom-listing.component.*](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-listing/)
**Pattern:** `crew-custom-listing` — Premium header card with search bar, count badges, and add-edit modal.
- Uses `VolunteerProfileService` for row counts and data
- Search filter with debounce
- Status filter dropdown (uses `VolunteerStatusService` to load statuses)

#### [NEW] [volunteer-custom-table.component.*](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-table/)
**Pattern:** `crew-custom-table` — Virtual-scroll table for desktop, card layout for mobile.
- Columns: Avatar/Name, Status (with color badge), Total Hours, Onboarded Date, Background Check status, Groups count
- Row click navigates to detail view
- Responsive card view for small screens

#### [NEW] [volunteer-custom-add-edit.component.*](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-add-edit/)
**Pattern:** `crew-custom-add-edit` — Modal dialog for creating/editing volunteer profiles.
- **Create mode:** Requires selecting an existing Resource (typeahead) OR creating a new one. Creates both Resource and VolunteerProfile records.
- **Edit mode:** Loads existing VolunteerProfile with all fields.
- Fields: Resource (FK picker), Volunteer Status (dropdown), onboared date, background check completed/date/expiry, confidentiality agreement signed/date, availability preferences (textarea), interests/skills notes (textarea), emergency contact notes (textarea), optional Constituent link, icon, color.
- Emits `volunteerProfileChanged` event on save.

#### [NEW] [volunteer-custom-detail.component.*](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-detail/)
**Pattern:** `resource-custom-detail` — Tabbed detail view with route param loading.
- **Overview tab:** Key info card (status badge, hours served, onboarded date, compliance indicators)
- **Groups tab:** List of VolunteerGroups this volunteer belongs to (via `VolunteerGroupMemberService`) with badge count
- **Assignments tab:** Recent volunteer assignments from `EventResourceAssignment` where `isVolunteer=true`, showing hours reported/approved
- **History tab:** Change history via `VolunteerProfileChangeHistoryService`
- Edit button opens the add-edit modal in edit mode.

#### [NEW] [volunteer-overview-tab.component.*](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-overview-tab/)
Overview stats card showing: status with color, total hours served, onboarded date, background check status (with expiry indicator), confidentiality agreement status, availability preferences, interests/skills, emergency contacts, linked constituent (if any).

#### [NEW] [volunteer-groups-tab.component.*](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-groups-tab/)
List of groups this volunteer is a member of, with name, role, join date, and link to group detail.

---

### Volunteer Group Components

Component group: `components/volunteer-group-custom/`

#### [NEW] [volunteer-group-custom-listing.component.*](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-custom-listing/)
**Pattern:** `crew-custom-listing` — Premium header card with search bar and count badges.
- Uses `VolunteerGroupService` for data

#### [NEW] [volunteer-group-custom-table.component.*](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-custom-table/)
**Pattern:** `crew-custom-table` — Virtual-scroll table + responsive cards.
- Columns: Icon/Name, Purpose, Members count, Office, Max Members, Min Status

#### [NEW] [volunteer-group-custom-add-edit.component.*](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-custom-add-edit/)
**Pattern:** `crew-custom-add-edit` — Modal for creating/editing groups.
- Fields: name, description, purpose, office (FK picker), min volunteer status (dropdown), max members, icon, color, avatar upload.

#### [NEW] [volunteer-group-custom-detail.component.*](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-custom-detail/)
**Pattern:** `crew-custom-detail` — Tabbed detail view.
- **Overview tab:** Group info card
- **Members tab:** List of members with add/remove (via `VolunteerGroupMemberService`) and badge count
- **History tab:** Change history

#### [NEW] [volunteer-group-overview-tab.component.*](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-overview-tab/)
Group stats card: name, description, purpose, office link, min status badge, member count vs max, color/icon.

#### [NEW] [volunteer-group-members-tab.component.*](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-members-tab/)
**Pattern:** `crew-members-tab` — List of group members with:
- Add member button (modal with Resource typeahead, role picker, join date)
- Remove member button
- Display: volunteer name, role badge, join date, notes
- Emits `membersChanged` event

---

## File Summary

| Type | Count | Matches Pattern |
|------|-------|-----------------|
| New component directories | 12 | crew-custom, resource-custom |
| New files (ts + html + scss) | 36 | Standard Angular component triple |
| Modified files | 4 | sidebar, header, routing, module |
| Server-side changes | 0 | Uses existing auto-generated services |

---

## Verification Plan

### Build Verification
```bash
cd d:\source\repos\scheduler\Scheduler\Scheduler.Client
ng build --configuration=development
```
Must compile with zero errors.

### Browser Testing
Use the browser tool to verify each screen:

1. **Sidebar navigation** — Confirm "Volunteers" and "Volunteer Groups" links appear and navigate correctly
2. **Volunteer Listing** — Verify the listing page loads, search works, count badges display
3. **Volunteer Add/Edit** — Open the modal, verify all fields render, test form validation
4. **Volunteer Detail** — Navigate to a volunteer detail page, verify tabs render (Overview, Groups, Assignments, History)
5. **Volunteer Group Listing** — Verify the listing page loads with search and counts
6. **Volunteer Group Add/Edit** — Open the modal, verify all fields render
7. **Volunteer Group Detail** — Navigate to detail page, verify tabs render (Overview, Members, History)

### Manual Verification (User)
After initial build + browser checks, the user can:
1. Launch the app with `ng serve`
2. Navigate to `/volunteers` — should see empty listing with "No Volunteers" badge
3. Click "Add" — modal should open with all volunteer profile fields
4. Navigate to `/volunteergroups` — should see empty listing
5. Click "Add" — modal should open with all group fields
