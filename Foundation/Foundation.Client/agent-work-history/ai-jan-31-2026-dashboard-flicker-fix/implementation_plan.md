# Reduce Systems Dashboard Flicker

Eliminate screen flickering during 1-second auto-refresh by updating DOM values in-place.

## Root Causes

1. **Loading spinner during refresh**: `loading = true` triggers `*ngIf="loading"` which hides content and shows a spinner
2. **Missing `trackBy`**: All `*ngFor` loops lack `trackBy` functions, causing Angular to destroy/recreate all DOM elements
3. **Chart data replacement**: Sparkline chart data objects are replaced causing chart re-render

## Proposed Changes

### Component TypeScript

#### [MODIFY] [systems-dashboard.component.ts](file:///d:/repos/Scheduler/Foundation/Foundation.Client/src/app/components/systems-dashboard/systems-dashboard.component.ts)

1. **Add `trackBy` functions** for stable element identity:
   ```typescript
   trackByAppName(index: number, app: { name: string }): string {
       return app.name;
   }
   trackByMetricName(index: number, metric: { metricName: string }): string {
       return metric.metricName;
   }
   trackBySnapshotId(index: number, snap: { applicationName: string }): string {
       return snap.applicationName;
   }
   trackByDbName(index: number, db: { name: string }): string {
       return db.name;
   }
   ```

2. **Separate initial load from refresh**: 
   - Add `initialLoadComplete` flag
   - Only set `loading = true` when `!initialLoadComplete`
   - Refreshes will update data without showing spinner

---

### Component Template

#### [MODIFY] [systems-dashboard.component.html](file:///d:/repos/Scheduler/Foundation/Foundation.Client/src/app/components/systems-dashboard/systems-dashboard.component.html)

1. **Apply `trackBy` to all `*ngFor` loops**:
   ```html
   <!-- Example changes -->
   *ngFor="let app of summary.applications; trackBy: trackByAppName"
   *ngFor="let metric of fleetMetrics.metrics; trackBy: trackByMetricName"
   *ngFor="let snap of summary.latestSnapshots; trackBy: trackBySnapshotId"
   *ngFor="let db of healthStatus.database.databases; trackBy: trackByDbName"
   ```

2. **Show content even while refreshing**: Change loading guard from:
   ```html
   *ngIf="activeMainTab === 'overview' && !loading && summary"
   ```
   to:
   ```html
   *ngIf="activeMainTab === 'overview' && summary"
   ```
   (only hide content when summary is null, not during refresh)

---

## Verification Plan

### Build
```powershell
dotnet build Scheduler.sln --no-restore -v q -nologo
```

### Manual Testing
1. Enable 1-second auto-refresh
2. Verify values update smoothly without spinner/flicker
3. Test all three tabs: Overview, Real-Time, Historical
