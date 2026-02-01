# Network Utilization Monitoring Enhancement

Add network interface utilization metrics to the Foundation telemetry system, completing the "Health Quad" (CPU, Memory, Disk, **Network**) for comprehensive cloud system monitoring.

## Technical Approach

Network utilization differs from other metrics because it's **cumulative** (total bytes sent/received) rather than instantaneous. The implementation will:

1. **Capture raw counters** (`bytesSent`, `bytesReceived`) per interface
2. **Calculate throughput rates** (bytes/sec) using delta between samples
3. **Compute % utilization** relative to reported link speed (with fallback logic for cloud VMs)

> [!NOTE]
> On cloud hypervisors, link speed detection can be unreliable. The implementation will cap utilization at 100% and mark interfaces with unknown link speeds for manual review.

---

## Proposed Changes

### Database Layer

#### [MODIFY] [TelemetryDatabaseGenerator.cs](file:///g:/source/repos/Scheduler/DatabaseGenerators/TelemetryDatabaseGenerator/TelemetryDatabaseGenerator.cs)

Add `TelemetryNetworkHealth` table definition after the existing `TelemetryDiskHealth` block (around line 194):

```csharp
//
// TelemetryNetworkHealth - Network interface metrics per snapshot
//
// Child table capturing network utilization for each active interface.
// Enables tracking of bandwidth consumption trends and saturation warnings.
//
Database.Table telemetryNetworkHealthTable = database.AddTable("TelemetryNetworkHealth");
telemetryNetworkHealthTable.comment = "Network interface utilization metrics per snapshot.";
telemetryNetworkHealthTable.isWritable = true;
telemetryNetworkHealthTable.adminAccessNeededToWrite = true;
telemetryNetworkHealthTable.AddIdField();
telemetryNetworkHealthTable.AddForeignKeyField("telemetrySnapshotId", telemetrySnapshotTable, false);
telemetryNetworkHealthTable.AddString100Field("interfaceName", false);
telemetryNetworkHealthTable.AddString250Field("interfaceDescription", true);
telemetryNetworkHealthTable.AddDoubleField("linkSpeedMbps", true);
telemetryNetworkHealthTable.AddLongField("bytesSentTotal", true);
telemetryNetworkHealthTable.AddLongField("bytesReceivedTotal", true);
telemetryNetworkHealthTable.AddDoubleField("bytesSentPerSecond", true);
telemetryNetworkHealthTable.AddDoubleField("bytesReceivedPerSecond", true);
telemetryNetworkHealthTable.AddDoubleField("utilizationPercent", true);
telemetryNetworkHealthTable.AddString50Field("status", true);
telemetryNetworkHealthTable.AddBoolField("isActive", false, true);
```

> [!IMPORTANT]
> After modifying the database generator, you'll need to run the `SchedulerTools` to regenerate database scripts and then use `EF Core Power Tools` to regenerate the EF model—these are **manual steps** per the README.

---

### Backend Layer

#### [MODIFY] [SystemHealthController.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Controllers/Utility/SystemHealthController.cs)

**1. Add network metrics endpoint** (after `GetDisk` at ~line 267):

```csharp
// GET: api/SystemHealth/network
// 
// Returns network interface utilization metrics
[HttpGet("network")]
[ResponseCache(Duration = 1)]
public ActionResult GetNetwork()
{
    return Ok(GetNetworkMetrics());
}
```

**2. Add network metrics collection method** (after `GetDiskMetrics` at ~line 566):

```csharp
private object GetNetworkMetrics()
{
    List<object> interfaces = new List<object>();

    try
    {
        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            //
            // Skip loopback, tunnel, and down interfaces
            //
            if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel ||
                nic.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            IPv4InterfaceStatistics stats = nic.GetIPv4Statistics();
            long linkSpeedBps = nic.Speed; // bits per second
            double linkSpeedMbps = linkSpeedBps / 1_000_000.0;

            interfaces.Add(new
            {
                Name = nic.Name,
                Description = nic.Description,
                LinkSpeedMbps = Math.Round(linkSpeedMbps, 2),
                BytesSentTotal = stats.BytesSent,
                BytesReceivedTotal = stats.BytesReceived,
                // Throughput rates calculated by collector using delta between samples
                BytesSentPerSecond = 0.0,
                BytesReceivedPerSecond = 0.0,
                UtilizationPercent = 0.0,  // Calculated by collector
                Status = "Healthy",
                IsActive = true
            });
        }
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Error getting network interface information");
    }

    return new
    {
        Interfaces = interfaces
    };
}
```

**3. Include network in the consolidated `GetStatus` response** (modify the return object at ~line 120):

Add to the existing status response:
```csharp
Network = GetNetworkMetrics(),
```

---

#### [MODIFY] [TelemetryCollectorService.cs](file:///g:/source/repos/Scheduler/Foundation.Telemetry/TelemetryCollectorService.cs)

**1. Add network parsing to `PopulateChildRecordsFromHealthData`** (after disk handling at ~line 370):

```csharp
// Network health records - navigate to network.interfaces
if (healthJson.TryGetProperty("network", out var networkRoot) 
    && networkRoot.TryGetProperty("interfaces", out var interfaces) 
    && interfaces.ValueKind == JsonValueKind.Array)
{
    foreach (var nic in interfaces.EnumerateArray())
    {
        var networkHealth = new TelemetryNetworkHealth
        {
            interfaceName = nic.TryGetProperty("name", out var n) ? n.GetString() : "Unknown",
            interfaceDescription = nic.TryGetProperty("description", out var d) ? d.GetString() : null,
            linkSpeedMbps = nic.TryGetProperty("linkSpeedMbps", out var ls) ? ls.GetDouble() : 0,
            bytesSentTotal = nic.TryGetProperty("bytesSentTotal", out var bs) ? bs.GetInt64() : 0,
            bytesReceivedTotal = nic.TryGetProperty("bytesReceivedTotal", out var br) ? br.GetInt64() : 0,
            // Throughput calculation will be done separately using previous snapshot
            bytesSentPerSecond = 0,
            bytesReceivedPerSecond = 0,
            utilizationPercent = 0,
            status = nic.TryGetProperty("status", out var s) ? s.GetString() : null,
            isActive = nic.TryGetProperty("isActive", out var ia) && ia.GetBoolean()
        };
        snapshot.TelemetryNetworkHealths.Add(networkHealth);
    }
    
    // Calculate throughput by comparing with previous snapshot
    await CalculateNetworkThroughputAsync(context, snapshot);
}
```

**2. Add throughput calculation helper method** (new private method):

```csharp
/// <summary>
/// Calculate network throughput rates by comparing current counters with previous snapshot.
/// Updates utilizationPercent based on link speed if available.
/// </summary>
private async Task CalculateNetworkThroughputAsync(TelemetryContext context, TelemetrySnapshot currentSnapshot)
{
    //
    // Get the previous snapshot for this application to calculate deltas
    //
    var previousSnapshot = await context.TelemetrySnapshots
        .Where(s => s.telemetryApplicationId == currentSnapshot.telemetryApplicationId
                 && s.id != currentSnapshot.id)
        .OrderByDescending(s => s.collectedAt)
        .Include(s => s.TelemetryNetworkHealths)
        .FirstOrDefaultAsync();

    if (previousSnapshot == null || previousSnapshot.TelemetryNetworkHealths == null)
    {
        return;
    }

    double elapsedSeconds = (currentSnapshot.collectedAt - previousSnapshot.collectedAt).TotalSeconds;
    if (elapsedSeconds <= 0)
    {
        return;
    }

    foreach (var currentNic in currentSnapshot.TelemetryNetworkHealths)
    {
        var previousNic = previousSnapshot.TelemetryNetworkHealths
            .FirstOrDefault(p => p.interfaceName == currentNic.interfaceName);

        if (previousNic != null)
        {
            //
            // Calculate bytes per second (handle counter wraparound gracefully)
            //
            long sentDelta = currentNic.bytesSentTotal >= previousNic.bytesSentTotal
                ? currentNic.bytesSentTotal.GetValueOrDefault() - previousNic.bytesSentTotal.GetValueOrDefault()
                : currentNic.bytesSentTotal.GetValueOrDefault();

            long receivedDelta = currentNic.bytesReceivedTotal >= previousNic.bytesReceivedTotal
                ? currentNic.bytesReceivedTotal.GetValueOrDefault() - previousNic.bytesReceivedTotal.GetValueOrDefault()
                : currentNic.bytesReceivedTotal.GetValueOrDefault();

            currentNic.bytesSentPerSecond = Math.Round(sentDelta / elapsedSeconds, 2);
            currentNic.bytesReceivedPerSecond = Math.Round(receivedDelta / elapsedSeconds, 2);

            //
            // Calculate utilization percentage (combined send+receive vs link capacity)
            //
            if (currentNic.linkSpeedMbps > 0)
            {
                double linkSpeedBytesPerSecond = currentNic.linkSpeedMbps.GetValueOrDefault() * 125000; // Mbps to bytes/sec
                double totalBytesPerSecond = currentNic.bytesSentPerSecond.GetValueOrDefault() 
                                           + currentNic.bytesReceivedPerSecond.GetValueOrDefault();
                double utilization = (totalBytesPerSecond / linkSpeedBytesPerSecond) * 100;
                currentNic.utilizationPercent = Math.Min(100, Math.Round(utilization, 2));

                //
                // Set status based on utilization thresholds
                //
                if (currentNic.utilizationPercent >= 90)
                {
                    currentNic.status = "Critical";
                }
                else if (currentNic.utilizationPercent >= 70)
                {
                    currentNic.status = "Warning";
                }
                else
                {
                    currentNic.status = "Healthy";
                }
            }
        }
    }
}
```

---

### Frontend Layer

#### [MODIFY] [telemetry.service.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/services/telemetry.service.ts)

**1. Add network trend DTOs and API method**:

```typescript
// Network trend response
export interface NetworkTrendPoint {
    timestamp: Date;
    utilizationPercent?: number;
    bytesSentPerSecond?: number;
    bytesReceivedPerSecond?: number;
}

export interface NetworkTrendsResponse {
    applicationName?: string;
    data: NetworkTrendPoint[];
}

// Add to TelemetryService class:
getNetworkTrends(appName?: string, hours: number = 24): Observable<NetworkTrendsResponse> {
    let url = `${this.baseUrl}/trends/network?hours=${hours}`;
    if (appName) {
        url += `&appName=${encodeURIComponent(appName)}`;
    }
    return this.http.get<NetworkTrendsResponse>(url);
}
```

---

#### [MODIFY] [systems-dashboard.component.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/systems-dashboard/systems-dashboard.component.ts)

**1. Add network sparkline property** (after `diskSparkline` at ~line 152):

```typescript
networkSparkline: ChartData<'line'> | null = null;
```

**2. Load network trends in `loadSystemSparklines`** (after disk trends loading at ~line 402):

```typescript
// Load network trends
this.telemetryService.getNetworkTrends(undefined, this.sparklineHours)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
        next: (response) => {
            const sorted = [...response.data].sort((a, b) =>
                new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
            this.networkSparkline = {
                labels: sorted.map(() => ''),
                datasets: [{
                    data: sorted.map(t => t.utilizationPercent ?? 0),
                    borderColor: '#e83e8c',  // Pink to differentiate from other metrics
                    backgroundColor: 'rgba(232, 62, 140, 0.1)',
                    fill: true
                }]
            };
        },
        error: (err: Error) => {
            console.error('Failed to load network trends:', err);
        }
    });
```

**3. Include network in health score calculation** (add to `calculateHealthScore` at ~line 502):

```typescript
// Network penalty (0-20 points) - penalize if high utilization
// This would need to fetch from latest snapshot's networkHealth data
const netUtil = snap.networkUtilizationPercent ?? 0;
if (netUtil >= 90) score -= 20;
else if (netUtil >= 70) score -= 10;
```

---

#### [MODIFY] [systems-dashboard.component.html](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/systems-dashboard/systems-dashboard.component.html)

**Add network sparkline card** in the system metrics grid alongside CPU/Memory/Disk cards:

```html
<!-- Network Utilization Card -->
<div class="metric-card network-card">
    <div class="metric-header">
        <i class="fas fa-network-wired text-pink"></i>
        <span>Network</span>
    </div>
    <div class="sparkline-container">
        <canvas *ngIf="networkSparkline"
                baseChart
                [data]="networkSparkline"
                [options]="sparklineOptions"
                [type]="lineChartType">
        </canvas>
        <div *ngIf="!networkSparkline" class="sparkline-placeholder">
            <span class="text-muted">Loading...</span>
        </div>
    </div>
    <div class="metric-footer">
        <span class="metric-label">Utilization %</span>
    </div>
</div>
```

---

#### [MODIFY] [TelemetryController.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Controllers/Utility/TelemetryController.cs)

**Add network trends endpoint** (following the existing patterns for memory/cpu/disk):

```csharp
// GET: api/Telemetry/trends/network
// 
// Returns network utilization trend data for sparkline visualization.
[HttpGet("trends/network")]
public async Task<ActionResult> GetNetworkTrends(string appName = null, int hours = 24)
{
    DateTime cutoffTime = DateTime.UtcNow.AddHours(-hours);

    using (var context = new TelemetryContext())
    {
        IQueryable<TelemetryNetworkHealth> query = context.TelemetryNetworkHealths
            .Include(n => n.telemetrySnapshot)
                .ThenInclude(s => s.telemetryApplication)
            .Where(n => n.telemetrySnapshot.collectedAt >= cutoffTime);

        if (!string.IsNullOrEmpty(appName))
        {
            query = query.Where(n => n.telemetrySnapshot.telemetryApplication.name == appName);
        }

        var data = await query
            .OrderByDescending(n => n.telemetrySnapshot.collectedAt)
            .Take(200)  // Limit data points
            .Select(n => new
            {
                timestamp = n.telemetrySnapshot.collectedAt,
                utilizationPercent = n.utilizationPercent,
                bytesSentPerSecond = n.bytesSentPerSecond,
                bytesReceivedPerSecond = n.bytesReceivedPerSecond
            })
            .ToListAsync();

        return Ok(new
        {
            applicationName = appName,
            data = data
        });
    }
}
```

---

## Files Summary

| Component | File | Change Type |
|-----------|------|-------------|
| **Database** | `TelemetryDatabaseGenerator.cs` | Add table definition |
| **Database** | `TelemetryNetworkHealth.cs` | NEW - EF entity (auto-generated) |
| **Database** | `TelemetryNetworkHealthExtension.cs` | NEW - DTO extensions (manual) |
| **Database** | `TelemetryContext.cs` | Add DbSet (auto-generated) |
| **Database** | `TelemetrySnapshot.cs` | Add nav property (auto-generated) |
| **Backend** | `SystemHealthController.cs` | Add network endpoint + metrics collection |
| **Backend** | `TelemetryCollectorService.cs` | Add network parsing + throughput calculation |
| **Backend** | `TelemetryController.cs` | Add network trends endpoint |
| **Frontend** | `telemetry.service.ts` | Add network DTOs + API method |
| **Frontend** | `systems-dashboard.component.ts` | Add network sparkline loading |
| **Frontend** | `systems-dashboard.component.html` | Add network card UI |

---

## Verification Plan

### Manual Testing

Since this feature involves telemetry collection over time and requires manual database regeneration steps, the verification will be primarily manual:

#### Step 1: Build Verification
1. After all code changes, build the solution: `dotnet build Scheduler.sln`
2. Verify no compilation errors

#### Step 2: Database Schema
1. Run `SchedulerTools` to generate new database scripts
2. Apply scripts to the Telemetry database
3. Run `EF Core Power Tools` to regenerate EF model
4. Verify `TelemetryNetworkHealth` table exists in database

#### Step 3: API Endpoint Testing
1. Start Foundation.Server locally
2. Navigate to `https://localhost:9101/api/SystemHealth/network` in browser
3. Verify JSON response contains network interface data with properties:
   - `name`, `description`, `linkSpeedMbps`, `bytesSentTotal`, `bytesReceivedTotal`
4. Navigate to `https://localhost:9101/api/SystemHealth/status`
5. Verify the consolidated response now includes a `network` section

#### Step 4: Telemetry Collection
1. Wait for 2-3 telemetry collection cycles (~5 minutes apart by default)
2. Query the database: `SELECT * FROM Telemetry.TelemetryNetworkHealth`
3. Verify records are being inserted with network interface data
4. After 2+ cycles, verify `bytesSentPerSecond`, `bytesReceivedPerSecond`, and `utilizationPercent` are being calculated (non-zero values after the 2nd cycle)

#### Step 5: Dashboard UI
1. Navigate to the Systems Dashboard in Foundation.Client
2. Verify a new "Network" sparkline card appears alongside CPU/Memory/Disk
3. Verify the sparkline populates with trend data after collection cycles

> [!NOTE]
> The first collection cycle will show 0% utilization because throughput requires comparing two snapshots. Real values will appear after the second cycle.

### Questions for User

1. **Link Speed Fallback**: For cloud VMs where link speed is reported as 0 or an unrealistic value, should I:
   - Use a configurable default (e.g., 1 Gbps)?
   - Skip utilization calculation and just show throughput?
   - Mark the interface with a "Unknown Speed" status?

2. **Multiple Interfaces**: Should the dashboard show aggregated network metrics (sum across all interfaces) or the primary interface only?

3. **Alert Thresholds**: The plan uses 70%/90% for warning/critical. Do these thresholds work for your use case?
