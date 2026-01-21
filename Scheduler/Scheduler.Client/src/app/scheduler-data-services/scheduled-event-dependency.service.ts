/*

   GENERATED SERVICE FOR THE SCHEDULEDEVENTDEPENDENCY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ScheduledEventDependency table.

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
import { ScheduledEventData } from './scheduled-event.service';
import { DependencyTypeData } from './dependency-type.service';
import { ScheduledEventDependencyChangeHistoryService, ScheduledEventDependencyChangeHistoryData } from './scheduled-event-dependency-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ScheduledEventDependencyQueryParameters {
    predecessorEventId: bigint | number | null | undefined = null;
    successorEventId: bigint | number | null | undefined = null;
    dependencyTypeId: bigint | number | null | undefined = null;
    lagMinutes: bigint | number | null | undefined = null;
    versionNumber: bigint | number | null | undefined = null;
    objectGuid: string | null | undefined = null;
    active: boolean | null | undefined = null;
    deleted: boolean | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class ScheduledEventDependencySubmitData {
    id!: bigint | number;
    predecessorEventId!: bigint | number;
    successorEventId!: bigint | number;
    dependencyTypeId!: bigint | number;
    lagMinutes!: bigint | number;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}



//
// Version history information returned from version history API endpoints.
// Matches server-side VersionInformation<T> structure.
//
export interface VersionInformationUser {
    id: bigint | number;
    userName: string;
    firstName: string | null;
    middleName: string | null;
    lastName: string | null;
}

export interface VersionInformation<T> {
    timeStamp: string;           // ISO 8601
    user: VersionInformationUser;
    versionNumber: number;
    data: T | null;
}

export class ScheduledEventDependencyBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ScheduledEventDependencyChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `scheduledEventDependency.ScheduledEventDependencyChildren$` — use with `| async` in templates
//        • Promise:    `scheduledEventDependency.ScheduledEventDependencyChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="scheduledEventDependency.ScheduledEventDependencyChildren$ | async"`), or
//        • Access the promise getter (`scheduledEventDependency.ScheduledEventDependencyChildren` or `await scheduledEventDependency.ScheduledEventDependencyChildren`)
//    - Simply reading `scheduledEventDependency.ScheduledEventDependencyChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await scheduledEventDependency.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ScheduledEventDependencyData {
    id!: bigint | number;
    predecessorEventId!: bigint | number;
    successorEventId!: bigint | number;
    dependencyTypeId!: bigint | number;
    lagMinutes!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    dependencyType: DependencyTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    predecessorEvent: ScheduledEventData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)
    successorEvent: ScheduledEventData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _scheduledEventDependencyChangeHistories: ScheduledEventDependencyChangeHistoryData[] | null = null;
    private _scheduledEventDependencyChangeHistoriesPromise: Promise<ScheduledEventDependencyChangeHistoryData[]> | null  = null;
    private _scheduledEventDependencyChangeHistoriesSubject = new BehaviorSubject<ScheduledEventDependencyChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ScheduledEventDependencyData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ScheduledEventDependencyData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduledEventDependencyData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ScheduledEventDependencyChangeHistories$ = this._scheduledEventDependencyChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventDependencyChangeHistories === null && this._scheduledEventDependencyChangeHistoriesPromise === null) {
            this.loadScheduledEventDependencyChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventDependencyChangeHistoriesCount$ = ScheduledEventDependencyChangeHistoryService.Instance.GetScheduledEventDependencyChangeHistoriesRowCount({scheduledEventDependencyId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ScheduledEventDependencyData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.scheduledEventDependency.Reload();
  //
  //  Non Async:
  //
  //     scheduledEventDependency[0].Reload().then(x => {
  //        this.scheduledEventDependency = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ScheduledEventDependencyService.Instance.GetScheduledEventDependency(this.id, includeRelations)
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
     this._scheduledEventDependencyChangeHistories = null;
     this._scheduledEventDependencyChangeHistoriesPromise = null;
     this._scheduledEventDependencyChangeHistoriesSubject.next(null);

     this._currentVersionInfo = null;
     this._currentVersionInfoPromise = null;
     this._currentVersionInfoSubject.next(null);
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ScheduledEventDependencyChangeHistories for this ScheduledEventDependency.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEventDependency.ScheduledEventDependencyChangeHistories.then(scheduledEventDependencies => { ... })
     *   or
     *   await this.scheduledEventDependency.scheduledEventDependencies
     *
    */
    public get ScheduledEventDependencyChangeHistories(): Promise<ScheduledEventDependencyChangeHistoryData[]> {
        if (this._scheduledEventDependencyChangeHistories !== null) {
            return Promise.resolve(this._scheduledEventDependencyChangeHistories);
        }

        if (this._scheduledEventDependencyChangeHistoriesPromise !== null) {
            return this._scheduledEventDependencyChangeHistoriesPromise;
        }

        // Start the load
        this.loadScheduledEventDependencyChangeHistories();

        return this._scheduledEventDependencyChangeHistoriesPromise!;
    }



    private loadScheduledEventDependencyChangeHistories(): void {

        this._scheduledEventDependencyChangeHistoriesPromise = lastValueFrom(
            ScheduledEventDependencyService.Instance.GetScheduledEventDependencyChangeHistoriesForScheduledEventDependency(this.id)
        )
        .then(ScheduledEventDependencyChangeHistories => {
            this._scheduledEventDependencyChangeHistories = ScheduledEventDependencyChangeHistories ?? [];
            this._scheduledEventDependencyChangeHistoriesSubject.next(this._scheduledEventDependencyChangeHistories);
            return this._scheduledEventDependencyChangeHistories;
         })
        .catch(err => {
            this._scheduledEventDependencyChangeHistories = [];
            this._scheduledEventDependencyChangeHistoriesSubject.next(this._scheduledEventDependencyChangeHistories);
            throw err;
        })
        .finally(() => {
            this._scheduledEventDependencyChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventDependencyChangeHistory. Call after mutations to force refresh.
     */
    public ClearScheduledEventDependencyChangeHistoriesCache(): void {
        this._scheduledEventDependencyChangeHistories = null;
        this._scheduledEventDependencyChangeHistoriesPromise = null;
        this._scheduledEventDependencyChangeHistoriesSubject.next(this._scheduledEventDependencyChangeHistories);      // Emit to observable
    }

    public get HasScheduledEventDependencyChangeHistories(): Promise<boolean> {
        return this.ScheduledEventDependencyChangeHistories.then(scheduledEventDependencyChangeHistories => scheduledEventDependencyChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (scheduledEventDependency.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await scheduledEventDependency.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ScheduledEventDependencyData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ScheduledEventDependencyData>> {
        const info = await lastValueFrom(
            ScheduledEventDependencyService.Instance.GetScheduledEventDependencyChangeMetadata(this.id, this.versionNumber as number)
        );
        this._currentVersionInfo = info;
        this._currentVersionInfoSubject.next(info);
        return info;
    }


    public ClearCurrentVersionInfoCache(): void {
        this._currentVersionInfo = null;
        this._currentVersionInfoPromise = null;
        this._currentVersionInfoSubject.next(null);
    }



    /**
     * Updates the state of this ScheduledEventDependencyData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ScheduledEventDependencyData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ScheduledEventDependencySubmitData {
        return ScheduledEventDependencyService.Instance.ConvertToScheduledEventDependencySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ScheduledEventDependencyService extends SecureEndpointBase {

    private static _instance: ScheduledEventDependencyService;
    private listCache: Map<string, Observable<Array<ScheduledEventDependencyData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ScheduledEventDependencyBasicListData>>>;
    private recordCache: Map<string, Observable<ScheduledEventDependencyData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private scheduledEventDependencyChangeHistoryService: ScheduledEventDependencyChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ScheduledEventDependencyData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ScheduledEventDependencyBasicListData>>>();
        this.recordCache = new Map<string, Observable<ScheduledEventDependencyData>>();

        ScheduledEventDependencyService._instance = this;
    }

    public static get Instance(): ScheduledEventDependencyService {
      return ScheduledEventDependencyService._instance;
    }


    public ClearListCaches(config: ScheduledEventDependencyQueryParameters | null = null) {

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


    public ConvertToScheduledEventDependencySubmitData(data: ScheduledEventDependencyData): ScheduledEventDependencySubmitData {

        let output = new ScheduledEventDependencySubmitData();

        output.id = data.id;
        output.predecessorEventId = data.predecessorEventId;
        output.successorEventId = data.successorEventId;
        output.dependencyTypeId = data.dependencyTypeId;
        output.lagMinutes = data.lagMinutes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetScheduledEventDependency(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventDependencyData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const scheduledEventDependency$ = this.requestScheduledEventDependency(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventDependency", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, scheduledEventDependency$);

            return scheduledEventDependency$;
        }

        return this.recordCache.get(configHash) as Observable<ScheduledEventDependencyData>;
    }

    private requestScheduledEventDependency(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventDependencyData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventDependencyData>(this.baseUrl + 'api/ScheduledEventDependency/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveScheduledEventDependency(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventDependency(id, includeRelations));
            }));
    }

    public GetScheduledEventDependencyList(config: ScheduledEventDependencyQueryParameters | any = null) : Observable<Array<ScheduledEventDependencyData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const scheduledEventDependencyList$ = this.requestScheduledEventDependencyList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventDependency list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, scheduledEventDependencyList$);

            return scheduledEventDependencyList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ScheduledEventDependencyData>>;
    }


    private requestScheduledEventDependencyList(config: ScheduledEventDependencyQueryParameters | any) : Observable <Array<ScheduledEventDependencyData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventDependencyData>>(this.baseUrl + 'api/ScheduledEventDependencies', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveScheduledEventDependencyList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventDependencyList(config));
            }));
    }

    public GetScheduledEventDependenciesRowCount(config: ScheduledEventDependencyQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const scheduledEventDependenciesRowCount$ = this.requestScheduledEventDependenciesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventDependencies row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, scheduledEventDependenciesRowCount$);

            return scheduledEventDependenciesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestScheduledEventDependenciesRowCount(config: ScheduledEventDependencyQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ScheduledEventDependencies/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventDependenciesRowCount(config));
            }));
    }

    public GetScheduledEventDependenciesBasicListData(config: ScheduledEventDependencyQueryParameters | any = null) : Observable<Array<ScheduledEventDependencyBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const scheduledEventDependenciesBasicListData$ = this.requestScheduledEventDependenciesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventDependencies basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, scheduledEventDependenciesBasicListData$);

            return scheduledEventDependenciesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ScheduledEventDependencyBasicListData>>;
    }


    private requestScheduledEventDependenciesBasicListData(config: ScheduledEventDependencyQueryParameters | any) : Observable<Array<ScheduledEventDependencyBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventDependencyBasicListData>>(this.baseUrl + 'api/ScheduledEventDependencies/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventDependenciesBasicListData(config));
            }));

    }


    public PutScheduledEventDependency(id: bigint | number, scheduledEventDependency: ScheduledEventDependencySubmitData) : Observable<ScheduledEventDependencyData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventDependencyData>(this.baseUrl + 'api/ScheduledEventDependency/' + id.toString(), scheduledEventDependency, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventDependency(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutScheduledEventDependency(id, scheduledEventDependency));
            }));
    }


    public PostScheduledEventDependency(scheduledEventDependency: ScheduledEventDependencySubmitData) : Observable<ScheduledEventDependencyData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ScheduledEventDependencyData>(this.baseUrl + 'api/ScheduledEventDependency', scheduledEventDependency, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventDependency(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostScheduledEventDependency(scheduledEventDependency));
            }));
    }

  
    public DeleteScheduledEventDependency(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ScheduledEventDependency/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteScheduledEventDependency(id));
            }));
    }

    public RollbackScheduledEventDependency(id: bigint | number, versionNumber: bigint | number) : Observable<ScheduledEventDependencyData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventDependencyData>(this.baseUrl + 'api/ScheduledEventDependency/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventDependency(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackScheduledEventDependency(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ScheduledEventDependency.
     */
    public GetScheduledEventDependencyChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ScheduledEventDependencyData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduledEventDependencyData>>(this.baseUrl + 'api/ScheduledEventDependency/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventDependencyChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ScheduledEventDependency.
     */
    public GetScheduledEventDependencyAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ScheduledEventDependencyData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduledEventDependencyData>[]>(this.baseUrl + 'api/ScheduledEventDependency/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventDependencyAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ScheduledEventDependency.
     */
    public GetScheduledEventDependencyVersion(id: bigint | number, version: number): Observable<ScheduledEventDependencyData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventDependencyData>(this.baseUrl + 'api/ScheduledEventDependency/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduledEventDependency(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventDependencyVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ScheduledEventDependency at a specific point in time.
     */
    public GetScheduledEventDependencyStateAtTime(id: bigint | number, time: string): Observable<ScheduledEventDependencyData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventDependencyData>(this.baseUrl + 'api/ScheduledEventDependency/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduledEventDependency(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventDependencyStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ScheduledEventDependencyQueryParameters | any): string {

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

    public userIsSchedulerScheduledEventDependencyReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerScheduledEventDependencyReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ScheduledEventDependencies
        //
        if (userIsSchedulerScheduledEventDependencyReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerScheduledEventDependencyReader = user.readPermission >= 1;
            } else {
                userIsSchedulerScheduledEventDependencyReader = false;
            }
        }

        return userIsSchedulerScheduledEventDependencyReader;
    }


    public userIsSchedulerScheduledEventDependencyWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerScheduledEventDependencyWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ScheduledEventDependencies
        //
        if (userIsSchedulerScheduledEventDependencyWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerScheduledEventDependencyWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerScheduledEventDependencyWriter = false;
          }      
        }

        return userIsSchedulerScheduledEventDependencyWriter;
    }

    public GetScheduledEventDependencyChangeHistoriesForScheduledEventDependency(scheduledEventDependencyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventDependencyChangeHistoryData[]> {
        return this.scheduledEventDependencyChangeHistoryService.GetScheduledEventDependencyChangeHistoryList({
            scheduledEventDependencyId: scheduledEventDependencyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ScheduledEventDependencyData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ScheduledEventDependencyData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ScheduledEventDependencyTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveScheduledEventDependency(raw: any): ScheduledEventDependencyData {
    if (!raw) return raw;

    //
    // Create a ScheduledEventDependencyData object instance with correct prototype
    //
    const revived = Object.create(ScheduledEventDependencyData.prototype) as ScheduledEventDependencyData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._scheduledEventDependencyChangeHistories = null;
    (revived as any)._scheduledEventDependencyChangeHistoriesPromise = null;
    (revived as any)._scheduledEventDependencyChangeHistoriesSubject = new BehaviorSubject<ScheduledEventDependencyChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadScheduledEventDependencyXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ScheduledEventDependencyChangeHistories$ = (revived as any)._scheduledEventDependencyChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventDependencyChangeHistories === null && (revived as any)._scheduledEventDependencyChangeHistoriesPromise === null) {
                (revived as any).loadScheduledEventDependencyChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventDependencyChangeHistoriesCount$ = ScheduledEventDependencyChangeHistoryService.Instance.GetScheduledEventDependencyChangeHistoriesRowCount({scheduledEventDependencyId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduledEventDependencyData> | null>(null);

    (revived as any).CurrentVersionInfo$ = (revived as any)._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if ((revived as any)._currentVersionInfo === null && (revived as any)._currentVersionInfoPromise === null) {
                (revived as any).loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    return revived;
  }

  private ReviveScheduledEventDependencyList(rawList: any[]): ScheduledEventDependencyData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveScheduledEventDependency(raw));
  }

}
