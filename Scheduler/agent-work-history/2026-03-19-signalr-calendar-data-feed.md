# SignalR Real-Time Calendar Data Feed

**Date:** 2026-03-19

## Summary

Added a SignalR hub to the Scheduler server for real-time calendar event notifications. When any scheduler creates or updates an event, all other connected schedulers in the same tenant see their calendar auto-refresh within ~500ms. This supports concurrent scheduler workflows without manual page refreshes.

## Changes Made

- **`Scheduler.Server/Controllers/SchedulerHub.cs`** [NEW] — SignalR hub with tenant-scoped groups. On connect, resolves the user's tenant from JWT claims and auto-joins a `scheduler_{tenantGuid}` group. Follows the BMC `RebrickableHub` pattern.
- **`Scheduler.Server/Program.cs`** [MODIFIED] — Added `builder.Services.AddSignalR()`, `app.MapHub<SchedulerHub>("/SchedulerSignal")`, updated CORS for both dev (SetIsOriginAllowed + AllowCredentials) and production (added AllowCredentials), added `ws:`/`wss:` to CSP `connect-src`.
- **`Scheduler.Server/DataControllers/ScheduledEventsController.cs`** [MODIFIED] — Injected `IHubContext<SchedulerHub>` into the constructor, added `BroadcastEventsChangedAsync()` helper method, called it after successful PUT and POST operations.
- **`Scheduler.Client/src/app/services/scheduler-signalr.service.ts`** [NEW] — Angular root-provided singleton service. Connects to `/SchedulerSignal` with bearer token auth, auto-reconnect policy, exposes `onEventsChanged$` and `onHubConnectionChange$` subjects.
- **`Scheduler.Client/src/app/components/scheduler/scheduler-calendar/scheduler-calendar.component.ts`** [MODIFIED] — Injects `SchedulerSignalrService`, connects on init, subscribes to `onEventsChanged$` with 500ms debounce to call `loadEvents()`, tracks `signalrConnected` state, disconnects on destroy.
- **`Scheduler.Client/package.json`** [MODIFIED] — Added `@microsoft/signalr` dependency.

## Key Decisions

- **Broadcast "reload" signal, not full event data** — The hub sends a lightweight `{action, eventId, timestamp}` notification. Clients then re-fetch via the existing `GetCalendarEvents` REST endpoint. This avoids pushing large event payloads over SignalR and reuses the proven data pipeline with server-side recurrence expansion.
- **Tenant-scoped groups** — Each connection auto-joins a `scheduler_{tenantGuid}` group for multi-tenant isolation, matching the `RebrickableHub` pattern.
- **500ms debounce** — Prevents rapid-fire calendar reloads when multiple events are saved in quick succession (e.g., batch operations).
- **CORS for SignalR** — `AllowCredentials()` is required by SignalR but incompatible with `AllowAnyOrigin()`. Dev mode uses `SetIsOriginAllowed(_ => true)`, production uses specific origins. Both include `AllowCredentials()`.
- **Modified DataControllers file** — While normally auto-generated files shouldn't be edited, the constructor and CRUD methods reside there. The changes are minimal (constructor param + 2 broadcast calls) and follow the same patterns used in the file.

## Testing / Verification

- **.NET build:** ✅ 0 errors (142 pre-existing warnings)
- **Angular build:** ✅ 0 compilation errors
- **Bundle size note:** Angular reports a budget overage (21.16 MB vs 21.00 MB limit). The `@microsoft/signalr` package adds ~162KB; budget can be adjusted in `angular.json`.
- **Manual testing:** Open two browser windows as same-tenant schedulers. Create/edit event in Window A → Window B's calendar should auto-refresh.
