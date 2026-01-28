/*

   GENERATED SERVICE FOR THE TELEMETRYSNAPSHOT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the TelemetrySnapshot table.

   It should suffice for many workflows and data access needs, but if anything more is needed, then extend this in a 
   custom version or add an additional targeted helper service.

*/
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, BehaviorSubject, catchError, throwError, lastValueFrom, map } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';
import { TelemetryApplicationData } from './telemetry-application.service';
import { TelemetryCollectionRunData } from './telemetry-collection-run.service';
import { TelemetryDatabaseHealthService, TelemetryDatabaseHealthData } from './telemetry-database-health.service';
import { TelemetryDiskHealthService, TelemetryDiskHealthData } from './telemetry-disk-health.service';
import { TelemetrySessionSnapshotService, TelemetrySessionSnapshotData } from './telemetry-session-snapshot.service';
import { TelemetryApplicationMetricService, TelemetryApplicationMetricData } from './telemetry-application-metric.service';
import { TelemetryErrorEventService, TelemetryErrorEventData } from './telemetry-error-event.service';
import { TelemetryLogErrorService, TelemetryLogErrorData } from './telemetry-log-error.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class TelemetrySnapshotQueryParameters {
    telemetryApplicationId: bigint | number | null | undefined = null;
    telemetryCollectionRunId: bigint | number | null | undefined = null;
    collectedAt: string | null | undefined = null;        // ISO 8601
    isOnline: boolean | null | undefined = null;
    uptimeSeconds: bigint | number | null | undefined = null;
    memoryWorkingSetMB: number | null | undefined = null;
    memoryGcHeapMB: number | null | undefined = null;
    cpuPercent: number | null | undefined = null;
    threadPoolWorkerThreads: bigint | number | null | undefined = null;
    threadPoolCompletionPortThreads: bigint | number | null | undefined = null;
    threadPoolPendingWorkItems: bigint | number | null | undefined = null;
    machineName: string | null | undefined = null;
    dotNetVersion: string | null | undefined = null;
    statusJson: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class TelemetrySnapshotSubmitData {
    id!: bigint | number;
    telemetryApplicationId!: bigint | number;
    telemetryCollectionRunId!: bigint | number;
    collectedAt!: string;      // ISO 8601
    isOnline!: boolean;
    uptimeSeconds: bigint | number | null = null;
    memoryWorkingSetMB: number | null = null;
    memoryGcHeapMB: number | null = null;
    cpuPercent: number | null = null;
    threadPoolWorkerThreads: bigint | number | null = null;
    threadPoolCompletionPortThreads: bigint | number | null = null;
    threadPoolPendingWorkItems: bigint | number | null = null;
    machineName: string | null = null;
    dotNetVersion: string | null = null;
    statusJson: string | null = null;
}


export class TelemetrySnapshotBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TelemetrySnapshotChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `telemetrySnapshot.TelemetrySnapshotChildren$` — use with `| async` in templates
//        • Promise:    `telemetrySnapshot.TelemetrySnapshotChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="telemetrySnapshot.TelemetrySnapshotChildren$ | async"`), or
//        • Access the promise getter (`telemetrySnapshot.TelemetrySnapshotChildren` or `await telemetrySnapshot.TelemetrySnapshotChildren`)
//    - Simply reading `telemetrySnapshot.TelemetrySnapshotChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await telemetrySnapshot.Reload()` to refresh the entire object and clear all lazy caches.
//    - Useful after mutations or when navigating into a navigation property.
//
// 5. **Cache clearing**:
//    - Use `ClearXCache()` methods after mutations to force fresh data on next access.
//
// 6. **Nav Properties**: if loaded with 'includeRelations = true' will be data objects of their appropriate types in data only.  They
//     will need to be 'Revived' and 'Reloaded' to access their nav properties, or lazy load their children.
//
// 7. **Dates are typed as strings**: because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z");
//
export class TelemetrySnapshotData {
    id!: bigint | number;
    telemetryApplicationId!: bigint | number;
    telemetryCollectionRunId!: bigint | number;
    collectedAt!: string;      // ISO 8601
    isOnline!: boolean;
    uptimeSeconds!: bigint | number;
    memoryWorkingSetMB!: number | null;
    memoryGcHeapMB!: number | null;
    cpuPercent!: number | null;
    threadPoolWorkerThreads!: bigint | number;
    threadPoolCompletionPortThreads!: bigint | number;
    threadPoolPendingWorkItems!: bigint | number;
    machineName!: string | null;
    dotNetVersion!: string | null;
    statusJson!: string | null;
    telemetryApplication: TelemetryApplicationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    telemetryCollectionRun: TelemetryCollectionRunData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _telemetryDatabaseHealths: TelemetryDatabaseHealthData[] | null = null;
    private _telemetryDatabaseHealthsPromise: Promise<TelemetryDatabaseHealthData[]> | null  = null;
    private _telemetryDatabaseHealthsSubject = new BehaviorSubject<TelemetryDatabaseHealthData[] | null>(null);

                
    private _telemetryDiskHealths: TelemetryDiskHealthData[] | null = null;
    private _telemetryDiskHealthsPromise: Promise<TelemetryDiskHealthData[]> | null  = null;
    private _telemetryDiskHealthsSubject = new BehaviorSubject<TelemetryDiskHealthData[] | null>(null);

                
    private _telemetrySessionSnapshots: TelemetrySessionSnapshotData[] | null = null;
    private _telemetrySessionSnapshotsPromise: Promise<TelemetrySessionSnapshotData[]> | null  = null;
    private _telemetrySessionSnapshotsSubject = new BehaviorSubject<TelemetrySessionSnapshotData[] | null>(null);

                
    private _telemetryApplicationMetrics: TelemetryApplicationMetricData[] | null = null;
    private _telemetryApplicationMetricsPromise: Promise<TelemetryApplicationMetricData[]> | null  = null;
    private _telemetryApplicationMetricsSubject = new BehaviorSubject<TelemetryApplicationMetricData[] | null>(null);

                
    private _telemetryErrorEvents: TelemetryErrorEventData[] | null = null;
    private _telemetryErrorEventsPromise: Promise<TelemetryErrorEventData[]> | null  = null;
    private _telemetryErrorEventsSubject = new BehaviorSubject<TelemetryErrorEventData[] | null>(null);

                
    private _telemetryLogErrors: TelemetryLogErrorData[] | null = null;
    private _telemetryLogErrorsPromise: Promise<TelemetryLogErrorData[]> | null  = null;
    private _telemetryLogErrorsSubject = new BehaviorSubject<TelemetryLogErrorData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public TelemetryDatabaseHealths$ = this._telemetryDatabaseHealthsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._telemetryDatabaseHealths === null && this._telemetryDatabaseHealthsPromise === null) {
            this.loadTelemetryDatabaseHealths(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public TelemetryDatabaseHealthsCount$ = TelemetryDatabaseHealthService.Instance.GetTelemetryDatabaseHealthsRowCount({telemetrySnapshotId: this.id,
      active: true,
      deleted: false
    });



    public TelemetryDiskHealths$ = this._telemetryDiskHealthsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._telemetryDiskHealths === null && this._telemetryDiskHealthsPromise === null) {
            this.loadTelemetryDiskHealths(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public TelemetryDiskHealthsCount$ = TelemetryDiskHealthService.Instance.GetTelemetryDiskHealthsRowCount({telemetrySnapshotId: this.id,
      active: true,
      deleted: false
    });



    public TelemetrySessionSnapshots$ = this._telemetrySessionSnapshotsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._telemetrySessionSnapshots === null && this._telemetrySessionSnapshotsPromise === null) {
            this.loadTelemetrySessionSnapshots(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public TelemetrySessionSnapshotsCount$ = TelemetrySessionSnapshotService.Instance.GetTelemetrySessionSnapshotsRowCount({telemetrySnapshotId: this.id,
      active: true,
      deleted: false
    });



    public TelemetryApplicationMetrics$ = this._telemetryApplicationMetricsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._telemetryApplicationMetrics === null && this._telemetryApplicationMetricsPromise === null) {
            this.loadTelemetryApplicationMetrics(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public TelemetryApplicationMetricsCount$ = TelemetryApplicationMetricService.Instance.GetTelemetryApplicationMetricsRowCount({telemetrySnapshotId: this.id,
      active: true,
      deleted: false
    });



    public TelemetryErrorEvents$ = this._telemetryErrorEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._telemetryErrorEvents === null && this._telemetryErrorEventsPromise === null) {
            this.loadTelemetryErrorEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public TelemetryErrorEventsCount$ = TelemetryErrorEventService.Instance.GetTelemetryErrorEventsRowCount({telemetrySnapshotId: this.id,
      active: true,
      deleted: false
    });



    public TelemetryLogErrors$ = this._telemetryLogErrorsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._telemetryLogErrors === null && this._telemetryLogErrorsPromise === null) {
            this.loadTelemetryLogErrors(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public TelemetryLogErrorsCount$ = TelemetryLogErrorService.Instance.GetTelemetryLogErrorsRowCount({telemetrySnapshotId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any TelemetrySnapshotData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.telemetrySnapshot.Reload();
  //
  //  Non Async:
  //
  //     telemetrySnapshot[0].Reload().then(x => {
  //        this.telemetrySnapshot = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TelemetrySnapshotService.Instance.GetTelemetrySnapshot(this.id, includeRelations)
    );

    // Merge fresh data into this instance (preserves reference)
    this.UpdateFrom(fresh as this);

    // Clear all lazy caches to force re-load on next access
    this.clearAllLazyCaches();

    return this;
  }


  private clearAllLazyCaches(): void {
     //
     // Reset every collection cache and notify subscribers
     //
     this._telemetryDatabaseHealths = null;
     this._telemetryDatabaseHealthsPromise = null;
     this._telemetryDatabaseHealthsSubject.next(null);

     this._telemetryDiskHealths = null;
     this._telemetryDiskHealthsPromise = null;
     this._telemetryDiskHealthsSubject.next(null);

     this._telemetrySessionSnapshots = null;
     this._telemetrySessionSnapshotsPromise = null;
     this._telemetrySessionSnapshotsSubject.next(null);

     this._telemetryApplicationMetrics = null;
     this._telemetryApplicationMetricsPromise = null;
     this._telemetryApplicationMetricsSubject.next(null);

     this._telemetryErrorEvents = null;
     this._telemetryErrorEventsPromise = null;
     this._telemetryErrorEventsSubject.next(null);

     this._telemetryLogErrors = null;
     this._telemetryLogErrorsPromise = null;
     this._telemetryLogErrorsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the TelemetryDatabaseHealths for this TelemetrySnapshot.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.telemetrySnapshot.TelemetryDatabaseHealths.then(telemetrySnapshots => { ... })
     *   or
     *   await this.telemetrySnapshot.telemetrySnapshots
     *
    */
    public get TelemetryDatabaseHealths(): Promise<TelemetryDatabaseHealthData[]> {
        if (this._telemetryDatabaseHealths !== null) {
            return Promise.resolve(this._telemetryDatabaseHealths);
        }

        if (this._telemetryDatabaseHealthsPromise !== null) {
            return this._telemetryDatabaseHealthsPromise;
        }

        // Start the load
        this.loadTelemetryDatabaseHealths();

        return this._telemetryDatabaseHealthsPromise!;
    }



    private loadTelemetryDatabaseHealths(): void {

        this._telemetryDatabaseHealthsPromise = lastValueFrom(
            TelemetrySnapshotService.Instance.GetTelemetryDatabaseHealthsForTelemetrySnapshot(this.id)
        )
        .then(TelemetryDatabaseHealths => {
            this._telemetryDatabaseHealths = TelemetryDatabaseHealths ?? [];
            this._telemetryDatabaseHealthsSubject.next(this._telemetryDatabaseHealths);
            return this._telemetryDatabaseHealths;
         })
        .catch(err => {
            this._telemetryDatabaseHealths = [];
            this._telemetryDatabaseHealthsSubject.next(this._telemetryDatabaseHealths);
            throw err;
        })
        .finally(() => {
            this._telemetryDatabaseHealthsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached TelemetryDatabaseHealth. Call after mutations to force refresh.
     */
    public ClearTelemetryDatabaseHealthsCache(): void {
        this._telemetryDatabaseHealths = null;
        this._telemetryDatabaseHealthsPromise = null;
        this._telemetryDatabaseHealthsSubject.next(this._telemetryDatabaseHealths);      // Emit to observable
    }

    public get HasTelemetryDatabaseHealths(): Promise<boolean> {
        return this.TelemetryDatabaseHealths.then(telemetryDatabaseHealths => telemetryDatabaseHealths.length > 0);
    }


    /**
     *
     * Gets the TelemetryDiskHealths for this TelemetrySnapshot.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.telemetrySnapshot.TelemetryDiskHealths.then(telemetrySnapshots => { ... })
     *   or
     *   await this.telemetrySnapshot.telemetrySnapshots
     *
    */
    public get TelemetryDiskHealths(): Promise<TelemetryDiskHealthData[]> {
        if (this._telemetryDiskHealths !== null) {
            return Promise.resolve(this._telemetryDiskHealths);
        }

        if (this._telemetryDiskHealthsPromise !== null) {
            return this._telemetryDiskHealthsPromise;
        }

        // Start the load
        this.loadTelemetryDiskHealths();

        return this._telemetryDiskHealthsPromise!;
    }



    private loadTelemetryDiskHealths(): void {

        this._telemetryDiskHealthsPromise = lastValueFrom(
            TelemetrySnapshotService.Instance.GetTelemetryDiskHealthsForTelemetrySnapshot(this.id)
        )
        .then(TelemetryDiskHealths => {
            this._telemetryDiskHealths = TelemetryDiskHealths ?? [];
            this._telemetryDiskHealthsSubject.next(this._telemetryDiskHealths);
            return this._telemetryDiskHealths;
         })
        .catch(err => {
            this._telemetryDiskHealths = [];
            this._telemetryDiskHealthsSubject.next(this._telemetryDiskHealths);
            throw err;
        })
        .finally(() => {
            this._telemetryDiskHealthsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached TelemetryDiskHealth. Call after mutations to force refresh.
     */
    public ClearTelemetryDiskHealthsCache(): void {
        this._telemetryDiskHealths = null;
        this._telemetryDiskHealthsPromise = null;
        this._telemetryDiskHealthsSubject.next(this._telemetryDiskHealths);      // Emit to observable
    }

    public get HasTelemetryDiskHealths(): Promise<boolean> {
        return this.TelemetryDiskHealths.then(telemetryDiskHealths => telemetryDiskHealths.length > 0);
    }


    /**
     *
     * Gets the TelemetrySessionSnapshots for this TelemetrySnapshot.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.telemetrySnapshot.TelemetrySessionSnapshots.then(telemetrySnapshots => { ... })
     *   or
     *   await this.telemetrySnapshot.telemetrySnapshots
     *
    */
    public get TelemetrySessionSnapshots(): Promise<TelemetrySessionSnapshotData[]> {
        if (this._telemetrySessionSnapshots !== null) {
            return Promise.resolve(this._telemetrySessionSnapshots);
        }

        if (this._telemetrySessionSnapshotsPromise !== null) {
            return this._telemetrySessionSnapshotsPromise;
        }

        // Start the load
        this.loadTelemetrySessionSnapshots();

        return this._telemetrySessionSnapshotsPromise!;
    }



    private loadTelemetrySessionSnapshots(): void {

        this._telemetrySessionSnapshotsPromise = lastValueFrom(
            TelemetrySnapshotService.Instance.GetTelemetrySessionSnapshotsForTelemetrySnapshot(this.id)
        )
        .then(TelemetrySessionSnapshots => {
            this._telemetrySessionSnapshots = TelemetrySessionSnapshots ?? [];
            this._telemetrySessionSnapshotsSubject.next(this._telemetrySessionSnapshots);
            return this._telemetrySessionSnapshots;
         })
        .catch(err => {
            this._telemetrySessionSnapshots = [];
            this._telemetrySessionSnapshotsSubject.next(this._telemetrySessionSnapshots);
            throw err;
        })
        .finally(() => {
            this._telemetrySessionSnapshotsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached TelemetrySessionSnapshot. Call after mutations to force refresh.
     */
    public ClearTelemetrySessionSnapshotsCache(): void {
        this._telemetrySessionSnapshots = null;
        this._telemetrySessionSnapshotsPromise = null;
        this._telemetrySessionSnapshotsSubject.next(this._telemetrySessionSnapshots);      // Emit to observable
    }

    public get HasTelemetrySessionSnapshots(): Promise<boolean> {
        return this.TelemetrySessionSnapshots.then(telemetrySessionSnapshots => telemetrySessionSnapshots.length > 0);
    }


    /**
     *
     * Gets the TelemetryApplicationMetrics for this TelemetrySnapshot.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.telemetrySnapshot.TelemetryApplicationMetrics.then(telemetrySnapshots => { ... })
     *   or
     *   await this.telemetrySnapshot.telemetrySnapshots
     *
    */
    public get TelemetryApplicationMetrics(): Promise<TelemetryApplicationMetricData[]> {
        if (this._telemetryApplicationMetrics !== null) {
            return Promise.resolve(this._telemetryApplicationMetrics);
        }

        if (this._telemetryApplicationMetricsPromise !== null) {
            return this._telemetryApplicationMetricsPromise;
        }

        // Start the load
        this.loadTelemetryApplicationMetrics();

        return this._telemetryApplicationMetricsPromise!;
    }



    private loadTelemetryApplicationMetrics(): void {

        this._telemetryApplicationMetricsPromise = lastValueFrom(
            TelemetrySnapshotService.Instance.GetTelemetryApplicationMetricsForTelemetrySnapshot(this.id)
        )
        .then(TelemetryApplicationMetrics => {
            this._telemetryApplicationMetrics = TelemetryApplicationMetrics ?? [];
            this._telemetryApplicationMetricsSubject.next(this._telemetryApplicationMetrics);
            return this._telemetryApplicationMetrics;
         })
        .catch(err => {
            this._telemetryApplicationMetrics = [];
            this._telemetryApplicationMetricsSubject.next(this._telemetryApplicationMetrics);
            throw err;
        })
        .finally(() => {
            this._telemetryApplicationMetricsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached TelemetryApplicationMetric. Call after mutations to force refresh.
     */
    public ClearTelemetryApplicationMetricsCache(): void {
        this._telemetryApplicationMetrics = null;
        this._telemetryApplicationMetricsPromise = null;
        this._telemetryApplicationMetricsSubject.next(this._telemetryApplicationMetrics);      // Emit to observable
    }

    public get HasTelemetryApplicationMetrics(): Promise<boolean> {
        return this.TelemetryApplicationMetrics.then(telemetryApplicationMetrics => telemetryApplicationMetrics.length > 0);
    }


    /**
     *
     * Gets the TelemetryErrorEvents for this TelemetrySnapshot.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.telemetrySnapshot.TelemetryErrorEvents.then(telemetrySnapshots => { ... })
     *   or
     *   await this.telemetrySnapshot.telemetrySnapshots
     *
    */
    public get TelemetryErrorEvents(): Promise<TelemetryErrorEventData[]> {
        if (this._telemetryErrorEvents !== null) {
            return Promise.resolve(this._telemetryErrorEvents);
        }

        if (this._telemetryErrorEventsPromise !== null) {
            return this._telemetryErrorEventsPromise;
        }

        // Start the load
        this.loadTelemetryErrorEvents();

        return this._telemetryErrorEventsPromise!;
    }



    private loadTelemetryErrorEvents(): void {

        this._telemetryErrorEventsPromise = lastValueFrom(
            TelemetrySnapshotService.Instance.GetTelemetryErrorEventsForTelemetrySnapshot(this.id)
        )
        .then(TelemetryErrorEvents => {
            this._telemetryErrorEvents = TelemetryErrorEvents ?? [];
            this._telemetryErrorEventsSubject.next(this._telemetryErrorEvents);
            return this._telemetryErrorEvents;
         })
        .catch(err => {
            this._telemetryErrorEvents = [];
            this._telemetryErrorEventsSubject.next(this._telemetryErrorEvents);
            throw err;
        })
        .finally(() => {
            this._telemetryErrorEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached TelemetryErrorEvent. Call after mutations to force refresh.
     */
    public ClearTelemetryErrorEventsCache(): void {
        this._telemetryErrorEvents = null;
        this._telemetryErrorEventsPromise = null;
        this._telemetryErrorEventsSubject.next(this._telemetryErrorEvents);      // Emit to observable
    }

    public get HasTelemetryErrorEvents(): Promise<boolean> {
        return this.TelemetryErrorEvents.then(telemetryErrorEvents => telemetryErrorEvents.length > 0);
    }


    /**
     *
     * Gets the TelemetryLogErrors for this TelemetrySnapshot.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.telemetrySnapshot.TelemetryLogErrors.then(telemetrySnapshots => { ... })
     *   or
     *   await this.telemetrySnapshot.telemetrySnapshots
     *
    */
    public get TelemetryLogErrors(): Promise<TelemetryLogErrorData[]> {
        if (this._telemetryLogErrors !== null) {
            return Promise.resolve(this._telemetryLogErrors);
        }

        if (this._telemetryLogErrorsPromise !== null) {
            return this._telemetryLogErrorsPromise;
        }

        // Start the load
        this.loadTelemetryLogErrors();

        return this._telemetryLogErrorsPromise!;
    }



    private loadTelemetryLogErrors(): void {

        this._telemetryLogErrorsPromise = lastValueFrom(
            TelemetrySnapshotService.Instance.GetTelemetryLogErrorsForTelemetrySnapshot(this.id)
        )
        .then(TelemetryLogErrors => {
            this._telemetryLogErrors = TelemetryLogErrors ?? [];
            this._telemetryLogErrorsSubject.next(this._telemetryLogErrors);
            return this._telemetryLogErrors;
         })
        .catch(err => {
            this._telemetryLogErrors = [];
            this._telemetryLogErrorsSubject.next(this._telemetryLogErrors);
            throw err;
        })
        .finally(() => {
            this._telemetryLogErrorsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached TelemetryLogError. Call after mutations to force refresh.
     */
    public ClearTelemetryLogErrorsCache(): void {
        this._telemetryLogErrors = null;
        this._telemetryLogErrorsPromise = null;
        this._telemetryLogErrorsSubject.next(this._telemetryLogErrors);      // Emit to observable
    }

    public get HasTelemetryLogErrors(): Promise<boolean> {
        return this.TelemetryLogErrors.then(telemetryLogErrors => telemetryLogErrors.length > 0);
    }




    /**
     * Updates the state of this TelemetrySnapshotData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TelemetrySnapshotData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TelemetrySnapshotSubmitData {
        return TelemetrySnapshotService.Instance.ConvertToTelemetrySnapshotSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TelemetrySnapshotService extends SecureEndpointBase {

    private static _instance: TelemetrySnapshotService;
    private listCache: Map<string, Observable<Array<TelemetrySnapshotData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TelemetrySnapshotBasicListData>>>;
    private recordCache: Map<string, Observable<TelemetrySnapshotData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private telemetryDatabaseHealthService: TelemetryDatabaseHealthService,
        private telemetryDiskHealthService: TelemetryDiskHealthService,
        private telemetrySessionSnapshotService: TelemetrySessionSnapshotService,
        private telemetryApplicationMetricService: TelemetryApplicationMetricService,
        private telemetryErrorEventService: TelemetryErrorEventService,
        private telemetryLogErrorService: TelemetryLogErrorService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TelemetrySnapshotData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TelemetrySnapshotBasicListData>>>();
        this.recordCache = new Map<string, Observable<TelemetrySnapshotData>>();

        TelemetrySnapshotService._instance = this;
    }

    public static get Instance(): TelemetrySnapshotService {
      return TelemetrySnapshotService._instance;
    }


    public ClearListCaches(config: TelemetrySnapshotQueryParameters | null = null) {

        const configHash = this.getConfigHash(config);

        if (this.listCache.has(configHash)) {
          this.listCache.delete(configHash);
        }

        if (this.rowCountCache.has(configHash)) {
            this.rowCountCache.delete(configHash);
        }

        if (this.basicListDataCache.has(configHash)) {
            this.basicListDataCache.delete(configHash);
        }
    }


    public ClearRecordCache(id: bigint | number, includeRelations: boolean = true) {

        const configHash = this.utilityService.hashCode(`_${id}_${includeRelations}`);

        if (this.recordCache.has(configHash)) {
            this.recordCache.delete(configHash);
        }
    }


    public ClearAllCaches() {
        this.listCache.clear();
        this.rowCountCache.clear();
        this.basicListDataCache.clear();
        this.recordCache.clear();
    }


    public ConvertToTelemetrySnapshotSubmitData(data: TelemetrySnapshotData): TelemetrySnapshotSubmitData {

        let output = new TelemetrySnapshotSubmitData();

        output.id = data.id;
        output.telemetryApplicationId = data.telemetryApplicationId;
        output.telemetryCollectionRunId = data.telemetryCollectionRunId;
        output.collectedAt = data.collectedAt;
        output.isOnline = data.isOnline;
        output.uptimeSeconds = data.uptimeSeconds;
        output.memoryWorkingSetMB = data.memoryWorkingSetMB;
        output.memoryGcHeapMB = data.memoryGcHeapMB;
        output.cpuPercent = data.cpuPercent;
        output.threadPoolWorkerThreads = data.threadPoolWorkerThreads;
        output.threadPoolCompletionPortThreads = data.threadPoolCompletionPortThreads;
        output.threadPoolPendingWorkItems = data.threadPoolPendingWorkItems;
        output.machineName = data.machineName;
        output.dotNetVersion = data.dotNetVersion;
        output.statusJson = data.statusJson;

        return output;
    }

    public GetTelemetrySnapshot(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetrySnapshotData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const telemetrySnapshot$ = this.requestTelemetrySnapshot(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetrySnapshot", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, telemetrySnapshot$);

            return telemetrySnapshot$;
        }

        return this.recordCache.get(configHash) as Observable<TelemetrySnapshotData>;
    }

    private requestTelemetrySnapshot(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetrySnapshotData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TelemetrySnapshotData>(this.baseUrl + 'api/TelemetrySnapshot/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTelemetrySnapshot(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetrySnapshot(id, includeRelations));
            }));
    }

    public GetTelemetrySnapshotList(config: TelemetrySnapshotQueryParameters | any = null) : Observable<Array<TelemetrySnapshotData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const telemetrySnapshotList$ = this.requestTelemetrySnapshotList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetrySnapshot list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, telemetrySnapshotList$);

            return telemetrySnapshotList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TelemetrySnapshotData>>;
    }


    private requestTelemetrySnapshotList(config: TelemetrySnapshotQueryParameters | any) : Observable <Array<TelemetrySnapshotData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetrySnapshotData>>(this.baseUrl + 'api/TelemetrySnapshots', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTelemetrySnapshotList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetrySnapshotList(config));
            }));
    }

    public GetTelemetrySnapshotsRowCount(config: TelemetrySnapshotQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const telemetrySnapshotsRowCount$ = this.requestTelemetrySnapshotsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetrySnapshots row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, telemetrySnapshotsRowCount$);

            return telemetrySnapshotsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTelemetrySnapshotsRowCount(config: TelemetrySnapshotQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/TelemetrySnapshots/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetrySnapshotsRowCount(config));
            }));
    }

    public GetTelemetrySnapshotsBasicListData(config: TelemetrySnapshotQueryParameters | any = null) : Observable<Array<TelemetrySnapshotBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const telemetrySnapshotsBasicListData$ = this.requestTelemetrySnapshotsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetrySnapshots basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, telemetrySnapshotsBasicListData$);

            return telemetrySnapshotsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TelemetrySnapshotBasicListData>>;
    }


    private requestTelemetrySnapshotsBasicListData(config: TelemetrySnapshotQueryParameters | any) : Observable<Array<TelemetrySnapshotBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetrySnapshotBasicListData>>(this.baseUrl + 'api/TelemetrySnapshots/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetrySnapshotsBasicListData(config));
            }));

    }


    public PutTelemetrySnapshot(id: bigint | number, telemetrySnapshot: TelemetrySnapshotSubmitData) : Observable<TelemetrySnapshotData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TelemetrySnapshotData>(this.baseUrl + 'api/TelemetrySnapshot/' + id.toString(), telemetrySnapshot, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetrySnapshot(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTelemetrySnapshot(id, telemetrySnapshot));
            }));
    }


    public PostTelemetrySnapshot(telemetrySnapshot: TelemetrySnapshotSubmitData) : Observable<TelemetrySnapshotData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TelemetrySnapshotData>(this.baseUrl + 'api/TelemetrySnapshot', telemetrySnapshot, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetrySnapshot(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTelemetrySnapshot(telemetrySnapshot));
            }));
    }

  
    public DeleteTelemetrySnapshot(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/TelemetrySnapshot/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTelemetrySnapshot(id));
            }));
    }


    private getConfigHash(config: TelemetrySnapshotQueryParameters | any): string {

        if (!config) {
            return '_';
        }

        // Normalize the config object, excluding null and undefined properties
        const normalizedConfig = Object.keys(config)
            .sort() // Ensure consistent property order
            .reduce((obj: any, key: string) => {
                if (config[key] != null) { // Exclude null and undefined
                    obj[key] = config[key];
                }
                return obj;
            }, {});

        if (Object.keys(normalizedConfig).length > 0) {
            return this.utilityService.hashCode(JSON.stringify(normalizedConfig));
        }

        return '_';
    }

    public userIsTelemetryTelemetrySnapshotReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsTelemetryTelemetrySnapshotReader = this.authService.isTelemetryReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Telemetry.TelemetrySnapshots
        //
        if (userIsTelemetryTelemetrySnapshotReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsTelemetryTelemetrySnapshotReader = user.readPermission >= 0;
            } else {
                userIsTelemetryTelemetrySnapshotReader = false;
            }
        }

        return userIsTelemetryTelemetrySnapshotReader;
    }


    public userIsTelemetryTelemetrySnapshotWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsTelemetryTelemetrySnapshotWriter = this.authService.isTelemetryReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Telemetry.TelemetrySnapshots
        //
        if (userIsTelemetryTelemetrySnapshotWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsTelemetryTelemetrySnapshotWriter = user.writePermission >= 0;
          } else {
            userIsTelemetryTelemetrySnapshotWriter = false;
          }      
        }

        return userIsTelemetryTelemetrySnapshotWriter;
    }

    public GetTelemetryDatabaseHealthsForTelemetrySnapshot(telemetrySnapshotId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TelemetryDatabaseHealthData[]> {
        return this.telemetryDatabaseHealthService.GetTelemetryDatabaseHealthList({
            telemetrySnapshotId: telemetrySnapshotId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetTelemetryDiskHealthsForTelemetrySnapshot(telemetrySnapshotId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TelemetryDiskHealthData[]> {
        return this.telemetryDiskHealthService.GetTelemetryDiskHealthList({
            telemetrySnapshotId: telemetrySnapshotId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetTelemetrySessionSnapshotsForTelemetrySnapshot(telemetrySnapshotId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TelemetrySessionSnapshotData[]> {
        return this.telemetrySessionSnapshotService.GetTelemetrySessionSnapshotList({
            telemetrySnapshotId: telemetrySnapshotId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetTelemetryApplicationMetricsForTelemetrySnapshot(telemetrySnapshotId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TelemetryApplicationMetricData[]> {
        return this.telemetryApplicationMetricService.GetTelemetryApplicationMetricList({
            telemetrySnapshotId: telemetrySnapshotId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetTelemetryErrorEventsForTelemetrySnapshot(telemetrySnapshotId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TelemetryErrorEventData[]> {
        return this.telemetryErrorEventService.GetTelemetryErrorEventList({
            telemetrySnapshotId: telemetrySnapshotId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetTelemetryLogErrorsForTelemetrySnapshot(telemetrySnapshotId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TelemetryLogErrorData[]> {
        return this.telemetryLogErrorService.GetTelemetryLogErrorList({
            telemetrySnapshotId: telemetrySnapshotId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full TelemetrySnapshotData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TelemetrySnapshotData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TelemetrySnapshotTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTelemetrySnapshot(raw: any): TelemetrySnapshotData {
    if (!raw) return raw;

    //
    // Create a TelemetrySnapshotData object instance with correct prototype
    //
    const revived = Object.create(TelemetrySnapshotData.prototype) as TelemetrySnapshotData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._telemetryDatabaseHealths = null;
    (revived as any)._telemetryDatabaseHealthsPromise = null;
    (revived as any)._telemetryDatabaseHealthsSubject = new BehaviorSubject<TelemetryDatabaseHealthData[] | null>(null);

    (revived as any)._telemetryDiskHealths = null;
    (revived as any)._telemetryDiskHealthsPromise = null;
    (revived as any)._telemetryDiskHealthsSubject = new BehaviorSubject<TelemetryDiskHealthData[] | null>(null);

    (revived as any)._telemetrySessionSnapshots = null;
    (revived as any)._telemetrySessionSnapshotsPromise = null;
    (revived as any)._telemetrySessionSnapshotsSubject = new BehaviorSubject<TelemetrySessionSnapshotData[] | null>(null);

    (revived as any)._telemetryApplicationMetrics = null;
    (revived as any)._telemetryApplicationMetricsPromise = null;
    (revived as any)._telemetryApplicationMetricsSubject = new BehaviorSubject<TelemetryApplicationMetricData[] | null>(null);

    (revived as any)._telemetryErrorEvents = null;
    (revived as any)._telemetryErrorEventsPromise = null;
    (revived as any)._telemetryErrorEventsSubject = new BehaviorSubject<TelemetryErrorEventData[] | null>(null);

    (revived as any)._telemetryLogErrors = null;
    (revived as any)._telemetryLogErrorsPromise = null;
    (revived as any)._telemetryLogErrorsSubject = new BehaviorSubject<TelemetryLogErrorData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadTelemetrySnapshotXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).TelemetryDatabaseHealths$ = (revived as any)._telemetryDatabaseHealthsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._telemetryDatabaseHealths === null && (revived as any)._telemetryDatabaseHealthsPromise === null) {
                (revived as any).loadTelemetryDatabaseHealths();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).TelemetryDatabaseHealthsCount$ = TelemetryDatabaseHealthService.Instance.GetTelemetryDatabaseHealthsRowCount({telemetrySnapshotId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).TelemetryDiskHealths$ = (revived as any)._telemetryDiskHealthsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._telemetryDiskHealths === null && (revived as any)._telemetryDiskHealthsPromise === null) {
                (revived as any).loadTelemetryDiskHealths();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).TelemetryDiskHealthsCount$ = TelemetryDiskHealthService.Instance.GetTelemetryDiskHealthsRowCount({telemetrySnapshotId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).TelemetrySessionSnapshots$ = (revived as any)._telemetrySessionSnapshotsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._telemetrySessionSnapshots === null && (revived as any)._telemetrySessionSnapshotsPromise === null) {
                (revived as any).loadTelemetrySessionSnapshots();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).TelemetrySessionSnapshotsCount$ = TelemetrySessionSnapshotService.Instance.GetTelemetrySessionSnapshotsRowCount({telemetrySnapshotId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).TelemetryApplicationMetrics$ = (revived as any)._telemetryApplicationMetricsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._telemetryApplicationMetrics === null && (revived as any)._telemetryApplicationMetricsPromise === null) {
                (revived as any).loadTelemetryApplicationMetrics();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).TelemetryApplicationMetricsCount$ = TelemetryApplicationMetricService.Instance.GetTelemetryApplicationMetricsRowCount({telemetrySnapshotId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).TelemetryErrorEvents$ = (revived as any)._telemetryErrorEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._telemetryErrorEvents === null && (revived as any)._telemetryErrorEventsPromise === null) {
                (revived as any).loadTelemetryErrorEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).TelemetryErrorEventsCount$ = TelemetryErrorEventService.Instance.GetTelemetryErrorEventsRowCount({telemetrySnapshotId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).TelemetryLogErrors$ = (revived as any)._telemetryLogErrorsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._telemetryLogErrors === null && (revived as any)._telemetryLogErrorsPromise === null) {
                (revived as any).loadTelemetryLogErrors();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).TelemetryLogErrorsCount$ = TelemetryLogErrorService.Instance.GetTelemetryLogErrorsRowCount({telemetrySnapshotId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveTelemetrySnapshotList(rawList: any[]): TelemetrySnapshotData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTelemetrySnapshot(raw));
  }

}
