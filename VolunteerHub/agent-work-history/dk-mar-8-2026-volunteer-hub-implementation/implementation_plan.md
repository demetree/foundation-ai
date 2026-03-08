# Volunteer Hub — Implementation Plan (Revised)

Addresses all remaining gaps from the [gap analysis](file:///C:/Users/demet/.gemini/antigravity/brain/0e9bce8c-0cba-4e5e-9f78-b3efea9c8415/volunteer_hub_gap_analysis.md). Updated after discovering the existing standalone [VolunteerHub.Client](file:///g:/source/repos/Scheduler/VolunteerHub/VolunteerHub.Client) project.

## Existing VolunteerHub.Client — Status

| Component | Status | Notes |
|-----------|--------|-------|
| `HubLoginComponent` | ✅ Complete | 2-step OTP flow with branded UI |
| `HubShellComponent` | ✅ Complete | Header + mobile bottom nav (Home, Schedule, Hours, Profile) |
| `HubDashboardComponent` | ✅ Complete | Welcome, stats cards, upcoming assignments list |
| `HubAuthService` | ✅ Complete | OTP flow, localStorage session, BehaviorSubject login state |
| `HubApiService` | ⚠️ Partial | Only has `getMyProfile()`, `getMyAssignments()`, `validateSession()` |
| `HubAuthGuard` | ✅ Complete | Session check → redirect to /login |
| `HubScheduleComponent` | ❌ Stub | "Coming Soon" placeholder |
| `HubHoursComponent` | ❌ Stub | "Coming Soon" placeholder |
| `HubProfileComponent` | ❌ Stub | "Coming Soon" placeholder |

---

## Phase 1 — Server-Side: Hardening & New Endpoints

### [MODIFY] [VolunteerHubController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/VolunteerHubController.cs)

**Fixes:**
- Replace `new SecurityContext()` with DI-injected `SecurityContext`
- Add authorization check to `POST admin/provision-access`
- Remove OTP code from `Information`-level log (line ~107)
- Replace `ResolveSessionUserAsync` Tuple return with named record `SessionResolution`

**New endpoints:**
- `POST auth/logout` — clears `authenticationToken` on the `SecurityUser`
- `PUT me/profile` — partial update: `availabilityPreferences`, `interestsAndSkillsNotes`, `emergencyContactNotes`
- `POST me/assignments/{assignmentId}/report-hours` — sets `reportedVolunteerHours` + `volunteerNotes`
- `POST me/assignments/{assignmentId}/respond` — sets assignment status (accept/decline)

---

## Phase 2 — FullCalendar Dependency (Scheduler.Client)

### [MODIFY] [package.json](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/package.json)

```bash
cd Scheduler/Scheduler.Client
npm install @fullcalendar/core @fullcalendar/angular @fullcalendar/daygrid @fullcalendar/timegrid @fullcalendar/interaction
```

---

## Phase 3 — VolunteerHub.Client: Implement Stub Components

### [MODIFY] [hub-api.service.ts](file:///g:/source/repos/Scheduler/VolunteerHub/VolunteerHub.Client/src/app/services/hub-api.service.ts)

Add methods for new server endpoints:
- `reportHours(assignmentId, hours, notes)` → `POST me/assignments/{id}/report-hours`
- `respondToAssignment(assignmentId, accepted)` → `POST me/assignments/{id}/respond`
- `updateProfile(data)` → `PUT me/profile`
- `logout()` → `POST auth/logout` (extend `HubAuthService.logout()` to call server-side)

### [MODIFY] [hub-schedule.component.ts](file:///g:/source/repos/Scheduler/VolunteerHub/VolunteerHub.Client/src/app/components/hub-schedule/hub-schedule.component.ts)

Replace "Coming Soon" stub with:
- Load all assignments via `HubApiService.getMyAssignments()`
- Display as a month/week calendar view (candidate: simple CSS grid calendar or list grouped by week)
- Show assignment cards with event name, time, status badge
- Accept/decline actions on pending assignments

### [MODIFY] [hub-hours.component.ts](file:///g:/source/repos/Scheduler/VolunteerHub/VolunteerHub.Client/src/app/components/hub-hours/hub-hours.component.ts)

Replace "Coming Soon" stub with:
- Load past assignments via `HubApiService.getMyAssignments()`
- Summary cards: total reported hours, total approved hours, pending count
- Assignments table with date, event name, reported/approved hours
- Inline "Report Hours" form for assignments without reported hours
- Date range filter (all / year / month)

### [MODIFY] [hub-profile.component.ts](file:///g:/source/repos/Scheduler/VolunteerHub/VolunteerHub.Client/src/app/components/hub-profile/hub-profile.component.ts)

Replace "Coming Soon" stub with:
- Load profile via `HubApiService.getMyProfile()`
- Read-only display: name, status, onboarded date, total hours, BG check status
- Editable fields: availability preferences, interests/skills, emergency contact
- Save button → `HubApiService.updateProfile()`

---

## Phase 4 — Verification

### Build Verification
- `dotnet build` on `Scheduler.Server`
- `ng build` on `Scheduler.Client` (FullCalendar check)
- `ng build` on `VolunteerHub.Client`

### Manual E2E Testing
1. Admin: Navigate to `/volunteers`, verify listing/detail/tabs/calendar all work
2. Hub login: Navigate to `VolunteerHub.Client` `/login`, complete OTP flow
3. Hub dashboard: Verify profile + upcoming assignments load
4. Hub schedule: Verify assignment calendar, accept/decline
5. Hub hours: Report hours on an assignment, verify appears in admin hours tab as pending
6. Hub profile: Edit availability, save, verify persists
7. Hub logout: Verify session cleared, redirect to login

---

## User Review Required

> [!IMPORTANT]
> **Accept/decline**: Should volunteers be able to decline previously accepted assignments, or only respond to new/pending ones?

> [!IMPORTANT]
> **Hub schedule view**: Should this be a full calendar grid (month view) or a simpler chronological list grouped by week? A calendar grid would add more complexity.
