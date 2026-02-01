# Network Utilization Monitoring Enhancement

## Objective
Add network interface utilization metrics to the Foundation telemetry system, providing % utilization for each network interface alongside existing CPU, Memory, and Disk metrics.

## Tasks

- [x] **Planning Phase**
  - [x] Research existing telemetry patterns (DiskHealth, DatabaseHealth)
  - [x] Review SystemHealthController structure
  - [x] Review TelemetryCollectorService parsing logic
  - [x] Review dashboard components for metric display patterns
  - [x] Create implementation plan
  - [x] Get user approval

- [x] **Database Layer**
  - [x] Add TelemetryNetworkHealth table to DatabaseGenerator
  - [ ] Generate and apply database scripts *(Manual)*
  - [ ] Run EF Core scaffolding *(Manual)*

- [x] **Backend Collection**
  - [x] Add network metrics to SystemHealthController response (`/network` endpoint)
  - [x] Update TelemetryCollectorService to parse and persist network data
  - [x] Implement `CalculateNetworkThroughput` for delta-based utilization
  - [x] Create `TelemetryNetworkHealthExtension.cs` with DTOs

- [x] **API Endpoints**
  - [x] Add `/api/Telemetry/trends/network` endpoint

- [x] **Frontend - Overview Tab**
  - [x] Add `NetworkTrendPoint` and `NetworkTrendsResponse` DTOs to telemetry.service.ts
  - [x] Add `getNetworkTrends()` method to TelemetryService
  - [x] Add `networkSparkline` property and loading logic
  - [x] Add Network sparkline card to HTML template
  - [x] Add `.bg-orange` CSS styling

- [x] **Frontend - Real-Time Tab**
  - [x] Add `NetworkMetrics` and `NetworkInterfaceInfo` interfaces to system-health.service.ts
  - [x] Add `network` property to `SystemHealthStatus` interface
  - [x] Add `getNetwork()` method to SystemHealthService
  - [x] Add Network Interfaces card with per-interface utilization display
  - [x] Add `.bg-orange-soft` and `.text-orange` CSS classes
  - [x] Add `trackByNicName` method

- [x] **Frontend - Historical Tab**
  - [x] Add `networkChartData` property
  - [x] Add Network Utilization Trend chart (full-width)

- [ ] **Verification** *(Manual)*
  - [ ] Test metric collection locally
  - [ ] Verify historical data persistence
  - [ ] Validate dashboard visualization
