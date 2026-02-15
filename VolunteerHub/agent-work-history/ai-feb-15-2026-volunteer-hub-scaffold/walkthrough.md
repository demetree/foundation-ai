# Volunteer Hub — Phase A Walkthrough

## What Was Built

### Server Side

**Database Schema** — Added `linkedUserGuid` field to `VolunteerProfile` in:
- [SchedulerDatabaseGenerator.cs](file:///d:/source/repos/scheduler/SchedulerDatabaseGenerator/SchedulerDatabaseGenerator.cs) (generator)
- [VolunteerProfile.cs](file:///d:/source/repos/scheduler/SchedulerDatabase/Database/VolunteerProfile.cs) (entity)
- [VolunteerProfileExtension.cs](file:///d:/source/repos/scheduler/SchedulerDatabase/EntityExtensions/VolunteerProfileExtension.cs) (all 8 DTO mappings)

**API Controller** — [VolunteerHubController.cs](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Server/Controllers/VolunteerHubController.cs)
| Endpoint | Purpose |
|---|---|
| `POST /api/volunteerhub/auth/request-code` | Send OTP via email/SMS |
| `POST /api/volunteerhub/auth/verify-code` | Verify OTP → session token |
| `GET /api/volunteerhub/auth/session` | Validate session |
| `GET /api/volunteerhub/me` | Get volunteer profile |
| `GET /api/volunteerhub/me/assignments` | Get assignments (date filter) |

---

### Client Side — `VolunteerHub.Client`

New standalone Angular 17 SPA at [VolunteerHub.Client](file:///d:/source/repos/scheduler/VolunteerHub/VolunteerHub.Client).

**Architecture:**
- **Port**: 12300 (dev server)
- **Auth**: Passwordless OTP (email/phone → 6-digit code → session token in `localStorage`)
- **Design**: Dark glassmorphism theme with indigo/violet palette, Inter font, mobile-first

**Key Files:**

| File | Purpose |
|---|---|
| [hub-auth.service.ts](file:///d:/source/repos/scheduler/VolunteerHub/VolunteerHub.Client/src/app/services/hub-auth.service.ts) | OTP flow + session management |
| [hub-api.service.ts](file:///d:/source/repos/scheduler/VolunteerHub/VolunteerHub.Client/src/app/services/hub-api.service.ts) | API client with `X-Volunteer-Session` header |
| [hub-auth.guard.ts](file:///d:/source/repos/scheduler/VolunteerHub/VolunteerHub.Client/src/app/guards/hub-auth.guard.ts) | Route guard |
| [hub-login](file:///d:/source/repos/scheduler/VolunteerHub/VolunteerHub.Client/src/app/components/hub-login/hub-login.component.ts) | 2-step OTP login UI |
| [hub-shell](file:///d:/source/repos/scheduler/VolunteerHub/VolunteerHub.Client/src/app/components/hub-shell/hub-shell.component.ts) | Header + bottom nav layout |
| [hub-dashboard](file:///d:/source/repos/scheduler/VolunteerHub/VolunteerHub.Client/src/app/components/hub-dashboard/hub-dashboard.component.ts) | Stats + upcoming assignments |
| [styles.scss](file:///d:/source/repos/scheduler/VolunteerHub/VolunteerHub.Client/src/styles.scss) | Design system (glassmorphism, badges, etc.) |

---

## Verification

- ✅ Server `dotnet build` — exit code 0
- ✅ Angular `ng build --configuration development` — dist generated with 24 output files
- ⏳ End-to-end OTP login flow test (requires DB migration first)

## Remaining Work

- Re-run database generator to fully propagate `linkedUserGuid`
- Add "Linked User" field to admin volunteer form
- End-to-end testing once DB is migrated
