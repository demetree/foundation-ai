/*

   GENERATED SERVICE FOR THE TELEMETRYAPPLICATION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the TelemetryApplication table.

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
export class TelemetryApplicationQueryParameters {
    name: string | null | undefined = null;
    url: string | null | undefined = null;
    isSelf: boolean | null | undefined = null;
    firstSeen: string | null | undefined = null;        // ISO 8601 (full datetime)
    lastSeen: string | null | undefined = null;        // ISO 8601 (full datetime)
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class TelemetryApplicationSubmitData {
    id!: bigint | number;
    name!: string;
    url: string | null = null;
    isSelf!: boolean;
    firstSeen!: string;      // ISO 8601 (full datetime)
    lastSeen: string | null = null;     // ISO 8601 (full datetime)
}


export class TelemetryApplicationBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TelemetryApplicationChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `telemetryApplication.TelemetryApplicationChildren$` — use with `| async` in templates
//        • Promise:    `telemetryApplication.TelemetryApplicationChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="telemetryApplication.TelemetryApplicationChildren$ | async"`), or
//        • Access the promise getter (`telemetryApplication.TelemetryApplicationChildren` or `await telemetryApplication.TelemetryApplicationChildren`)
//    - Simply reading `telemetryApplication.TelemetryApplicationChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await telemetryApplication.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TelemetryApplicationData {
    id!: bigint | number;
    name!: string;
    url!: string | null;
    isSelf!: boolean;
    firstSeen!: string;      // ISO 8601 (full datetime)
    lastSeen!: string | null;   // ISO 8601 (full datetime)

    //
    // Private lazy-loading caches for related collections
    //
    private _telemetrySnapshots: TelemetrySnapshotData[] | null = null;
    private _telemetrySnapshotsPromise: Promise<TelemetrySnapshotData[]> | null  = null;
    private _telemetrySnapshotsSubject = new BehaviorSubject<TelemetrySnapshotData[] | null>(null);

                
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
    public TelemetrySnapshots$ = this._telemetrySnapshotsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._telemetrySnapshots === null && this._telemetrySnapshotsPromise === null) {
            this.loadTelemetrySnapshots(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public TelemetrySnapshotsCount$ = TelemetrySnapshotService.Instance.GetTelemetrySnapshotsRowCount({telemetryApplicationId: this.id,
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

  
    public TelemetryErrorEventsCount$ = TelemetryErrorEventService.Instance.GetTelemetryErrorEventsRowCount({telemetryApplicationId: this.id,
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

  
    public TelemetryLogErrorsCount$ = TelemetryLogErrorService.Instance.GetTelemetryLogErrorsRowCount({telemetryApplicationId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any TelemetryApplicationData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.telemetryApplication.Reload();
  //
  //  Non Async:
  //
  //     telemetryApplication[0].Reload().then(x => {
  //        this.telemetryApplication = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TelemetryApplicationService.Instance.GetTelemetryApplication(this.id, includeRelations)
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
     * Gets the TelemetrySnapshots for this TelemetryApplication.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.telemetryApplication.TelemetrySnapshots.then(telemetryApplications => { ... })
     *   or
     *   await this.telemetryApplication.telemetryApplications
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
            TelemetryApplicationService.Instance.GetTelemetrySnapshotsForTelemetryApplication(this.id)
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
     *
     * Gets the TelemetryErrorEvents for this TelemetryApplication.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.telemetryApplication.TelemetryErrorEvents.then(telemetryApplications => { ... })
     *   or
     *   await this.telemetryApplication.telemetryApplications
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
            TelemetryApplicationService.Instance.GetTelemetryErrorEventsForTelemetryApplication(this.id)
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
     * Gets the TelemetryLogErrors for this TelemetryApplication.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.telemetryApplication.TelemetryLogErrors.then(telemetryApplications => { ... })
     *   or
     *   await this.telemetryApplication.telemetryApplications
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
            TelemetryApplicationService.Instance.GetTelemetryLogErrorsForTelemetryApplication(this.id)
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
     * Updates the state of this TelemetryApplicationData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TelemetryApplicationData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TelemetryApplicationSubmitData {
        return TelemetryApplicationService.Instance.ConvertToTelemetryApplicationSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TelemetryApplicationService extends SecureEndpointBase {

    private static _instance: TelemetryApplicationService;
    private listCache: Map<string, Observable<Array<TelemetryApplicationData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TelemetryApplicationBasicListData>>>;
    private recordCache: Map<string, Observable<TelemetryApplicationData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private telemetrySnapshotService: TelemetrySnapshotService,
        private telemetryErrorEventService: TelemetryErrorEventService,
        private telemetryLogErrorService: TelemetryLogErrorService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TelemetryApplicationData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TelemetryApplicationBasicListData>>>();
        this.recordCache = new Map<string, Observable<TelemetryApplicationData>>();

        TelemetryApplicationService._instance = this;
    }

    public static get Instance(): TelemetryApplicationService {
      return TelemetryApplicationService._instance;
    }


    public ClearListCaches(config: TelemetryApplicationQueryParameters | null = null) {

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


    public ConvertToTelemetryApplicationSubmitData(data: TelemetryApplicationData): TelemetryApplicationSubmitData {

        let output = new TelemetryApplicationSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.url = data.url;
        output.isSelf = data.isSelf;
        output.firstSeen = data.firstSeen;
        output.lastSeen = data.lastSeen;

        return output;
    }

    public GetTelemetryApplication(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetryApplicationData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const telemetryApplication$ = this.requestTelemetryApplication(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryApplication", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, telemetryApplication$);

            return telemetryApplication$;
        }

        return this.recordCache.get(configHash) as Observable<TelemetryApplicationData>;
    }

    private requestTelemetryApplication(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetryApplicationData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TelemetryApplicationData>(this.baseUrl + 'api/TelemetryApplication/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTelemetryApplication(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryApplication(id, includeRelations));
            }));
    }

    public GetTelemetryApplicationList(config: TelemetryApplicationQueryParameters | any = null) : Observable<Array<TelemetryApplicationData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const telemetryApplicationList$ = this.requestTelemetryApplicationList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryApplication list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, telemetryApplicationList$);

            return telemetryApplicationList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TelemetryApplicationData>>;
    }


    private requestTelemetryApplicationList(config: TelemetryApplicationQueryParameters | any) : Observable <Array<TelemetryApplicationData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetryApplicationData>>(this.baseUrl + 'api/TelemetryApplications', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTelemetryApplicationList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryApplicationList(config));
            }));
    }

    public GetTelemetryApplicationsRowCount(config: TelemetryApplicationQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const telemetryApplicationsRowCount$ = this.requestTelemetryApplicationsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryApplications row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, telemetryApplicationsRowCount$);

            return telemetryApplicationsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTelemetryApplicationsRowCount(config: TelemetryApplicationQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/TelemetryApplications/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryApplicationsRowCount(config));
            }));
    }

    public GetTelemetryApplicationsBasicListData(config: TelemetryApplicationQueryParameters | any = null) : Observable<Array<TelemetryApplicationBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const telemetryApplicationsBasicListData$ = this.requestTelemetryApplicationsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryApplications basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, telemetryApplicationsBasicListData$);

            return telemetryApplicationsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TelemetryApplicationBasicListData>>;
    }


    private requestTelemetryApplicationsBasicListData(config: TelemetryApplicationQueryParameters | any) : Observable<Array<TelemetryApplicationBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetryApplicationBasicListData>>(this.baseUrl + 'api/TelemetryApplications/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryApplicationsBasicListData(config));
            }));

    }


    public PutTelemetryApplication(id: bigint | number, telemetryApplication: TelemetryApplicationSubmitData) : Observable<TelemetryApplicationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TelemetryApplicationData>(this.baseUrl + 'api/TelemetryApplication/' + id.toString(), telemetryApplication, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetryApplication(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTelemetryApplication(id, telemetryApplication));
            }));
    }


    public PostTelemetryApplication(telemetryApplication: TelemetryApplicationSubmitData) : Observable<TelemetryApplicationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TelemetryApplicationData>(this.baseUrl + 'api/TelemetryApplication', telemetryApplication, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetryApplication(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTelemetryApplication(telemetryApplication));
            }));
    }

  
    public DeleteTelemetryApplication(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/TelemetryApplication/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTelemetryApplication(id));
            }));
    }


    private getConfigHash(config: TelemetryApplicationQueryParameters | any): string {

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

    public userIsTelemetryTelemetryApplicationReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsTelemetryTelemetryApplicationReader = this.authService.isTelemetryReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Telemetry.TelemetryApplications
        //
        if (userIsTelemetryTelemetryApplicationReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsTelemetryTelemetryApplicationReader = user.readPermission >= 0;
            } else {
                userIsTelemetryTelemetryApplicationReader = false;
            }
        }

        return userIsTelemetryTelemetryApplicationReader;
    }


    public userIsTelemetryTelemetryApplicationWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsTelemetryTelemetryApplicationWriter = this.authService.isTelemetryReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Telemetry.TelemetryApplications
        //
        if (userIsTelemetryTelemetryApplicationWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsTelemetryTelemetryApplicationWriter = user.writePermission >= 0;
          } else {
            userIsTelemetryTelemetryApplicationWriter = false;
          }      
        }

        return userIsTelemetryTelemetryApplicationWriter;
    }

    public GetTelemetrySnapshotsForTelemetryApplication(telemetryApplicationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TelemetrySnapshotData[]> {
        return this.telemetrySnapshotService.GetTelemetrySnapshotList({
            telemetryApplicationId: telemetryApplicationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetTelemetryErrorEventsForTelemetryApplication(telemetryApplicationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TelemetryErrorEventData[]> {
        return this.telemetryErrorEventService.GetTelemetryErrorEventList({
            telemetryApplicationId: telemetryApplicationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetTelemetryLogErrorsForTelemetryApplication(telemetryApplicationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TelemetryLogErrorData[]> {
        return this.telemetryLogErrorService.GetTelemetryLogErrorList({
            telemetryApplicationId: telemetryApplicationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full TelemetryApplicationData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TelemetryApplicationData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TelemetryApplicationTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTelemetryApplication(raw: any): TelemetryApplicationData {
    if (!raw) return raw;

    //
    // Create a TelemetryApplicationData object instance with correct prototype
    //
    const revived = Object.create(TelemetryApplicationData.prototype) as TelemetryApplicationData;

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
    // 2. But private methods (loadTelemetryApplicationXYZ, etc.) are not accessible via the typed variable
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

    (revived as any).TelemetrySnapshotsCount$ = TelemetrySnapshotService.Instance.GetTelemetrySnapshotsRowCount({telemetryApplicationId: (revived as any).id,
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

    (revived as any).TelemetryErrorEventsCount$ = TelemetryErrorEventService.Instance.GetTelemetryErrorEventsRowCount({telemetryApplicationId: (revived as any).id,
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

    (revived as any).TelemetryLogErrorsCount$ = TelemetryLogErrorService.Instance.GetTelemetryLogErrorsRowCount({telemetryApplicationId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveTelemetryApplicationList(rawList: any[]): TelemetryApplicationData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTelemetryApplication(raw));
  }

}
