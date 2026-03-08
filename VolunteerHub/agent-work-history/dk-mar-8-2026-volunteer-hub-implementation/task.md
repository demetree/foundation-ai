# Volunteer Hub — Task Checklist

## Phase 1 — Server-Side Hardening & New Endpoints
- [x] Replace `new SecurityContext()` with DI-injected instance (kept project convention)
- [x] Add `[Authorize]` to `POST admin/provision-access`
- [x] Remove OTP code from log message
- [x] Replace Tuple return with named `SessionResolution` record
- [x] Add `POST auth/logout` endpoint
- [x] Add `PUT me/profile` endpoint
- [x] Add `POST me/assignments/{id}/report-hours` endpoint
- [x] Add `POST me/assignments/{id}/respond` endpoint

## Phase 2 — FullCalendar Dependency (Scheduler.Client)
- [x] Install `@fullcalendar/*` packages
- [x] Verify Scheduler.Client compiles

## Phase 3 — VolunteerHub.Client Stub Implementation
- [x] Extend `hub-api.service.ts` with new endpoint methods
- [x] Extend `hub-auth.service.ts` logout to call server
- [x] Implement `hub-schedule` component
- [x] Implement `hub-hours` component
- [x] Implement `hub-profile` component

## Phase 4 — Verification
- [x] `dotnet build` on Scheduler.Server — 0 errors
- [x] `ng build` on Scheduler.Client — 0 errors
- [x] `ng build` on VolunteerHub.Client — 0 errors
- [ ] Manual test: admin volunteer management
- [ ] Manual test: hub login → dashboard → schedule → hours → profile → logout
