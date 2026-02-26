/*

   GENERATED SERVICE FOR THE TELEMETRYCOLLECTIONRUN TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the TelemetryCollectionRun table.

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
import { TelemetrySnapshotService, TelemetrySnapshotData } from './telemetry-snapshot.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class TelemetryCollectionRunQueryParameters {
    startTime: string | null | undefined = null;        // ISO 8601 (full datetime)
    endTime: string | null | undefined = null;        // ISO 8601 (full datetime)
    applicationsPolled: bigint | number | null | undefined = null;
    applicationsSucceeded: bigint | number | null | undefined = null;
    errorMessage: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class TelemetryCollectionRunSubmitData {
    id!: bigint | number;
    startTime!: string;      // ISO 8601 (full datetime)
    endTime: string | null = null;     // ISO 8601 (full datetime)
    applicationsPolled: bigint | number | null = null;
    applicationsSucceeded: bigint | number | null = null;
    errorMessage: string | null = null;
}


export class TelemetryCollectionRunBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TelemetryCollectionRunChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `telemetryCollectionRun.TelemetryCollectionRunChildren$` — use with `| async` in templates
//        • Promise:    `telemetryCollectionRun.TelemetryCollectionRunChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="telemetryCollectionRun.TelemetryCollectionRunChildren$ | async"`), or
//        • Access the promise getter (`telemetryCollectionRun.TelemetryCollectionRunChildren` or `await telemetryCollectionRun.TelemetryCollectionRunChildren`)
//    - Simply reading `telemetryCollectionRun.TelemetryCollectionRunChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await telemetryCollectionRun.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TelemetryCollectionRunData {
    id!: bigint | number;
    startTime!: string;      // ISO 8601 (full datetime)
    endTime!: string | null;   // ISO 8601 (full datetime)
    applicationsPolled!: bigint | number;
    applicationsSucceeded!: bigint | number;
    errorMessage!: string | null;

    //
    // Private lazy-loading caches for related collections
    //
    private _telemetrySnapshots: TelemetrySnapshotData[] | null = null;
    private _telemetrySnapshotsPromise: Promise<TelemetrySnapshotData[]> | null  = null;
    private _telemetrySnapshotsSubject = new BehaviorSubject<TelemetrySnapshotData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public TelemetrySnapshots$ = this._telemetrySnapshotsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._telemetrySnapshots === null && this._telemetrySnapshotsPromise === null) {
            this.loadTelemetrySnapshots(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _telemetrySnapshotsCount$: Observable<bigint | number> | null = null;
    public get TelemetrySnapshotsCount$(): Observable<bigint | number> {
        if (this._telemetrySnapshotsCount$ === null) {
            this._telemetrySnapshotsCount$ = TelemetrySnapshotService.Instance.GetTelemetrySnapshotsRowCount({telemetryCollectionRunId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._telemetrySnapshotsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any TelemetryCollectionRunData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.telemetryCollectionRun.Reload();
  //
  //  Non Async:
  //
  //     telemetryCollectionRun[0].Reload().then(x => {
  //        this.telemetryCollectionRun = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TelemetryCollectionRunService.Instance.GetTelemetryCollectionRun(this.id, includeRelations)
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
     this._telemetrySnapshots = null;
     this._telemetrySnapshotsPromise = null;
     this._telemetrySnapshotsSubject.next(null);
     this._telemetrySnapshotsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the TelemetrySnapshots for this TelemetryCollectionRun.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.telemetryCollectionRun.TelemetrySnapshots.then(telemetryCollectionRuns => { ... })
     *   or
     *   await this.telemetryCollectionRun.telemetryCollectionRuns
     *
    */
    public get TelemetrySnapshots(): Promise<TelemetrySnapshotData[]> {
        if (this._telemetrySnapshots !== null) {
            return Promise.resolve(this._telemetrySnapshots);
        }

        if (this._telemetrySnapshotsPromise !== null) {
            return this._telemetrySnapshotsPromise;
        }

        // Start the load
        this.loadTelemetrySnapshots();

        return this._telemetrySnapshotsPromise!;
    }



    private loadTelemetrySnapshots(): void {

        this._telemetrySnapshotsPromise = lastValueFrom(
            TelemetryCollectionRunService.Instance.GetTelemetrySnapshotsForTelemetryCollectionRun(this.id)
        )
        .then(TelemetrySnapshots => {
            this._telemetrySnapshots = TelemetrySnapshots ?? [];
            this._telemetrySnapshotsSubject.next(this._telemetrySnapshots);
            return this._telemetrySnapshots;
         })
        .catch(err => {
            this._telemetrySnapshots = [];
            this._telemetrySnapshotsSubject.next(this._telemetrySnapshots);
            throw err;
        })
        .finally(() => {
            this._telemetrySnapshotsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached TelemetrySnapshot. Call after mutations to force refresh.
     */
    public ClearTelemetrySnapshotsCache(): void {
        this._telemetrySnapshots = null;
        this._telemetrySnapshotsPromise = null;
        this._telemetrySnapshotsSubject.next(this._telemetrySnapshots);      // Emit to observable
    }

    public get HasTelemetrySnapshots(): Promise<boolean> {
        return this.TelemetrySnapshots.then(telemetrySnapshots => telemetrySnapshots.length > 0);
    }




    /**
     * Updates the state of this TelemetryCollectionRunData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TelemetryCollectionRunData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TelemetryCollectionRunSubmitData {
        return TelemetryCollectionRunService.Instance.ConvertToTelemetryCollectionRunSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TelemetryCollectionRunService extends SecureEndpointBase {

    private static _instance: TelemetryCollectionRunService;
    private listCache: Map<string, Observable<Array<TelemetryCollectionRunData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TelemetryCollectionRunBasicListData>>>;
    private recordCache: Map<string, Observable<TelemetryCollectionRunData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private telemetrySnapshotService: TelemetrySnapshotService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TelemetryCollectionRunData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TelemetryCollectionRunBasicListData>>>();
        this.recordCache = new Map<string, Observable<TelemetryCollectionRunData>>();

        TelemetryCollectionRunService._instance = this;
    }

    public static get Instance(): TelemetryCollectionRunService {
      return TelemetryCollectionRunService._instance;
    }


    public ClearListCaches(config: TelemetryCollectionRunQueryParameters | null = null) {

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


    public ConvertToTelemetryCollectionRunSubmitData(data: TelemetryCollectionRunData): TelemetryCollectionRunSubmitData {

        let output = new TelemetryCollectionRunSubmitData();

        output.id = data.id;
        output.startTime = data.startTime;
        output.endTime = data.endTime;
        output.applicationsPolled = data.applicationsPolled;
        output.applicationsSucceeded = data.applicationsSucceeded;
        output.errorMessage = data.errorMessage;

        return output;
    }

    public GetTelemetryCollectionRun(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetryCollectionRunData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const telemetryCollectionRun$ = this.requestTelemetryCollectionRun(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryCollectionRun", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, telemetryCollectionRun$);

            return telemetryCollectionRun$;
        }

        return this.recordCache.get(configHash) as Observable<TelemetryCollectionRunData>;
    }

    private requestTelemetryCollectionRun(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetryCollectionRunData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TelemetryCollectionRunData>(this.baseUrl + 'api/TelemetryCollectionRun/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTelemetryCollectionRun(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryCollectionRun(id, includeRelations));
            }));
    }

    public GetTelemetryCollectionRunList(config: TelemetryCollectionRunQueryParameters | any = null) : Observable<Array<TelemetryCollectionRunData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const telemetryCollectionRunList$ = this.requestTelemetryCollectionRunList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryCollectionRun list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, telemetryCollectionRunList$);

            return telemetryCollectionRunList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TelemetryCollectionRunData>>;
    }


    private requestTelemetryCollectionRunList(config: TelemetryCollectionRunQueryParameters | any) : Observable <Array<TelemetryCollectionRunData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetryCollectionRunData>>(this.baseUrl + 'api/TelemetryCollectionRuns', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTelemetryCollectionRunList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryCollectionRunList(config));
            }));
    }

    public GetTelemetryCollectionRunsRowCount(config: TelemetryCollectionRunQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const telemetryCollectionRunsRowCount$ = this.requestTelemetryCollectionRunsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryCollectionRuns row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, telemetryCollectionRunsRowCount$);

            return telemetryCollectionRunsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTelemetryCollectionRunsRowCount(config: TelemetryCollectionRunQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/TelemetryCollectionRuns/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryCollectionRunsRowCount(config));
            }));
    }

    public GetTelemetryCollectionRunsBasicListData(config: TelemetryCollectionRunQueryParameters | any = null) : Observable<Array<TelemetryCollectionRunBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const telemetryCollectionRunsBasicListData$ = this.requestTelemetryCollectionRunsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryCollectionRuns basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, telemetryCollectionRunsBasicListData$);

            return telemetryCollectionRunsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TelemetryCollectionRunBasicListData>>;
    }


    private requestTelemetryCollectionRunsBasicListData(config: TelemetryCollectionRunQueryParameters | any) : Observable<Array<TelemetryCollectionRunBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetryCollectionRunBasicListData>>(this.baseUrl + 'api/TelemetryCollectionRuns/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryCollectionRunsBasicListData(config));
            }));

    }


    public PutTelemetryCollectionRun(id: bigint | number, telemetryCollectionRun: TelemetryCollectionRunSubmitData) : Observable<TelemetryCollectionRunData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TelemetryCollectionRunData>(this.baseUrl + 'api/TelemetryCollectionRun/' + id.toString(), telemetryCollectionRun, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetryCollectionRun(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTelemetryCollectionRun(id, telemetryCollectionRun));
            }));
    }


    public PostTelemetryCollectionRun(telemetryCollectionRun: TelemetryCollectionRunSubmitData) : Observable<TelemetryCollectionRunData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TelemetryCollectionRunData>(this.baseUrl + 'api/TelemetryCollectionRun', telemetryCollectionRun, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetryCollectionRun(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTelemetryCollectionRun(telemetryCollectionRun));
            }));
    }

  
    public DeleteTelemetryCollectionRun(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/TelemetryCollectionRun/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTelemetryCollectionRun(id));
            }));
    }


    private getConfigHash(config: TelemetryCollectionRunQueryParameters | any): string {

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

    public userIsTelemetryTelemetryCollectionRunReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsTelemetryTelemetryCollectionRunReader = this.authService.isTelemetryReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Telemetry.TelemetryCollectionRuns
        //
        if (userIsTelemetryTelemetryCollectionRunReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsTelemetryTelemetryCollectionRunReader = user.readPermission >= 0;
            } else {
                userIsTelemetryTelemetryCollectionRunReader = false;
            }
        }

        return userIsTelemetryTelemetryCollectionRunReader;
    }


    public userIsTelemetryTelemetryCollectionRunWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsTelemetryTelemetryCollectionRunWriter = this.authService.isTelemetryReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Telemetry.TelemetryCollectionRuns
        //
        if (userIsTelemetryTelemetryCollectionRunWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsTelemetryTelemetryCollectionRunWriter = user.writePermission >= 0;
          } else {
            userIsTelemetryTelemetryCollectionRunWriter = false;
          }      
        }

        return userIsTelemetryTelemetryCollectionRunWriter;
    }

    public GetTelemetrySnapshotsForTelemetryCollectionRun(telemetryCollectionRunId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TelemetrySnapshotData[]> {
        return this.telemetrySnapshotService.GetTelemetrySnapshotList({
            telemetryCollectionRunId: telemetryCollectionRunId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full TelemetryCollectionRunData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TelemetryCollectionRunData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TelemetryCollectionRunTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTelemetryCollectionRun(raw: any): TelemetryCollectionRunData {
    if (!raw) return raw;

    //
    // Create a TelemetryCollectionRunData object instance with correct prototype
    //
    const revived = Object.create(TelemetryCollectionRunData.prototype) as TelemetryCollectionRunData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._telemetrySnapshots = null;
    (revived as any)._telemetrySnapshotsPromise = null;
    (revived as any)._telemetrySnapshotsSubject = new BehaviorSubject<TelemetrySnapshotData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadTelemetryCollectionRunXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).TelemetrySnapshots$ = (revived as any)._telemetrySnapshotsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._telemetrySnapshots === null && (revived as any)._telemetrySnapshotsPromise === null) {
                (revived as any).loadTelemetrySnapshots();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._telemetrySnapshotsCount$ = null;



    return revived;
  }

  private ReviveTelemetryCollectionRunList(rawList: any[]): TelemetryCollectionRunData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTelemetryCollectionRun(raw));
  }

}
