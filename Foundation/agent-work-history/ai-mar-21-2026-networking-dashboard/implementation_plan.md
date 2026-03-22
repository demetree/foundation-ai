# Unified Networking Operations Dashboard

A single-page dashboard in Foundation.Client that provides real-time operational visibility into all 8 networking services + the existing TURN server. Follows the established TURN dashboard pattern (hero header, status cards, auto-refresh).

## Design Overview

The dashboard at `/networking` presents a **grid of service cards** — each card shows real-time status (healthy/degraded/offline), a key metric, and the service name. Clicking a card expands an **inline detail panel** below the grid with service-specific data (tables, stats, actions). This avoids the overhead of 8 separate pages while still allowing deep inspection.

### Architecture

```
Foundation.Server                    Foundation.Client
┌─────────────────────────┐         ┌──────────────────────────┐
│ NetworkingController.cs │◄────────│ networking.service.ts    │
│   (DI: all 8 libraries) │         │   (HTTP + auth headers)  │
│   GET /api/networking/* │         │                          │
└─────────────────────────┘         │ networking-dashboard/    │
                                    │   .component.ts          │
                                    │   .component.html        │
                                    │   .component.scss        │
                                    └──────────────────────────┘
```

> [!IMPORTANT]
> The controller injects the networking libraries **directly** (same pattern as `TurnServerController` which injects `TurnServer` via DI), NOT proxying to the standalone hosts. The standalone hosts remain available for independent testing.

## Proposed Changes

### Server — Foundation.Server

#### [NEW] [NetworkingController.cs](file:///g:/source/repos/Scheduler/Foundation/Foundation.Server/Controllers/NetworkingController.cs)

API controller at `api/networking` with endpoints for each service:

| Endpoint | Returns |
|----------|---------|
| `GET /api/networking/overview` | Aggregated status of all 8 services |
| `GET /api/networking/watchtower/latency` | LatencyMonitorService summaries |
| `GET /api/networking/locksmith/certificates` | Certificate monitor statuses |
| `GET /api/networking/skynet/status` | Firewall rules + backend pool stats |
| `GET /api/networking/switchboard/status` | Load balancer strategy + backend counts |
| `GET /api/networking/hivemind/status` | Cache stats + session count |
| `GET /api/networking/deepspace/status` | Storage provider stats |
| `GET /api/networking/beacon/status` | DNS zones + service discovery stats |
| `GET /api/networking/conduit/status` | Active connections + channel count |

Each endpoint wraps the library calls in try/catch and returns structured DTOs. The `overview` endpoint calls each service and aggregates into a single response with per-service health status ("healthy" / "degraded" / "offline").

#### [MODIFY] [Program.cs or Startup DI](file:///g:/source/repos/Scheduler/Foundation/Foundation.Server)

Register the 8 networking service instances in DI so the controller can inject them.

---

### Client — Foundation.Client

#### [NEW] [networking.service.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/services/networking.service.ts)

Angular service following `TurnServerService` pattern:
- `getOverview(): Observable<NetworkingOverview>` — all services at once
- Per-service detail methods for expanded card views

#### [NEW] [networking-dashboard.component.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/networking-dashboard/networking-dashboard.component.ts)

Dashboard component with:
- Hero header (gradient: `#667eea → #764ba2` — purple networking theme)
- 3×3 grid of service cards (8 services + TURN)
- Click-to-expand detail panel per service
- Auto-refresh with countdown (10s default)
- Back button navigation

#### [NEW] [networking-dashboard.component.html](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/networking-dashboard/networking-dashboard.component.html)

#### [NEW] [networking-dashboard.component.scss](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/networking-dashboard/networking-dashboard.component.scss)

Premium styling matching the TURN dashboard pattern (glass morphism, gradient hero).

#### [MODIFY] [app-routing.module.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/app-routing.module.ts)

Add route: `{ path: 'networking', component: NetworkingDashboardComponent, canActivate: [AuthGuard], title: 'Networking' }`

#### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/app.module.ts)

Add import and declaration for `NetworkingDashboardComponent`.

## Verification Plan

### Automated Tests
- `dotnet build` Foundation.Server — verify controller compiles
- `ng build` Foundation.Client — verify component compiles
- Navigate to `/networking` route in browser and verify dashboard renders

### Manual Verification
- Confirm all 9 service cards render with status indicators
- Verify auto-refresh updates the data
- Test click-to-expand on each service card
