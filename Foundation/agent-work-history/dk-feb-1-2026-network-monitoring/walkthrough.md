# Network Utilization Monitoring - Implementation Walkthrough

## Summary
Implemented network interface utilization monitoring for the Foundation telemetry system, completing the "Health Quad" alongside CPU, Memory, and Disk metrics.

---

## Changes Made

### 1. Database Schema
Added `TelemetryNetworkHealth` table to [TelemetryDatabaseGenerator.cs](file:///g:/source/repos/Scheduler/FoundationCore/Telemetry/TelemetryDatabaseGenerator.cs):

```csharp
// Key columns: interfaceName, description, linkSpeedMbps, 
// bytesSentTotal, bytesReceivedTotal, bytesSentPerSecond, 
// bytesReceivedPerSecond, utilizationPercent, isActive, status
```

---

### 2. Backend Collection

#### SystemHealthController (network endpoint)
[SystemHealthController.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Controllers/Utility/SystemHealthController.cs#L301-L350)
- Added `/api/SystemHealth/network` endpoint returning per-interface data

#### TelemetryCollectorService (parsing + throughput)
[TelemetryCollectorService.cs](file:///g:/source/repos/Scheduler/Foundation.Telemetry/TelemetryCollectorService.cs#L370-L420)
- Parses `network.interfaces` from health JSON
- `CalculateNetworkThroughput()` computes delta-based throughput by comparing consecutive snapshots
- Calculates utilization % as `(bytesSent + bytesReceived) / linkSpeed`

#### Entity Extension
[TelemetryNetworkHealthExtension.cs](file:///g:/source/repos/Scheduler/FoundationCore/Telemetry/EntityExtensions/TelemetryNetworkHealthExtension.cs)
- DTOs for API responses
- Extension methods following `TelemetryDiskHealthExtension` pattern

---

### 3. API Endpoint
[TelemetryController.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Controllers/Utility/TelemetryController.cs#L347-L400)
- Added `GET /api/Telemetry/trends/network` endpoint for sparkline data

---

### 4. Frontend Integration

#### Telemetry Service
[telemetry.service.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/services/telemetry.service.ts#L77-L92)
- Added `NetworkTrendPoint` and `NetworkTrendsResponse` DTOs
- Added `getNetworkTrends()` method

#### Dashboard Component
[systems-dashboard.component.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/systems-dashboard/systems-dashboard.component.ts#L153)
- Added `networkSparkline` property
- Added loading logic in `loadSystemSparklines()`

#### HTML Template
[systems-dashboard.component.html](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/systems-dashboard/systems-dashboard.component.html#L270-L287)
- Network utilization card with sparkline (orange color scheme)

#### CSS
[systems-dashboard.component.scss](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/systems-dashboard/systems-dashboard.component.scss#L112-L115)
- Added `.bg-orange` gradient style

---

## Build Verification

| Component | Status |
|-----------|--------|
| Backend (`Foundation.Web.csproj`) | ✅ Build successful |
| Frontend (`npm run build`) | ✅ Build successful |

---

### 5. Lightweight System Health Components

Extended the Network card pattern to the simpler system-health components in both Foundation and Scheduler projects.

#### Foundation System Health
- [system-health.component.html](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/system-health/system-health.component.html#L307-L341) - Network Interface card
- [system-health.component.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/system-health/system-health.component.ts#L459-L472) - `formatNetworkBytes()` helper

#### Scheduler System Health
- [system-health.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/system-health/system-health.component.html#L311-L345) - Network Interface card
- [system-health.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/system-health/system-health.component.ts#L384-L396) - `formatNetworkBytes()` helper
- [system-health.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/services/system-health.service.ts#L126-L140) - Added `NetworkMetrics`/`NetworkInterfaceInfo` types

---

## Next Steps (Manual)

1. **Generate Database**: Run SchemaGenerator to create migration scripts
2. **EF Scaffold**: Run scaffolding to generate `TelemetryNetworkHealth.cs` entity
3. **Test**: Start the application and verify network metrics appear in dashboard
