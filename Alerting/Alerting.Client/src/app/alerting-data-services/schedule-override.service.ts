/*

   GENERATED SERVICE FOR THE SCHEDULEOVERRIDE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ScheduleOverride table.

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
import { OnCallScheduleData } from './on-call-schedule.service';
import { ScheduleLayerData } from './schedule-layer.service';
import { ScheduleOverrideTypeData } from './schedule-override-type.service';
import { ScheduleOverrideChangeHistoryService, ScheduleOverrideChangeHistoryData } from './schedule-override-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ScheduleOverrideQueryParameters {
    onCallScheduleId: bigint | number | null | undefined = null;
    scheduleLayerId: bigint | number | null | undefined = null;
    startDateTime: string | null | undefined = null;        // ISO 8601
    endDateTime: string | null | undefined = null;        // ISO 8601
    scheduleOverrideTypeId: bigint | number | null | undefined = null;
    originalUserObjectGuid: string | null | undefined = null;
    replacementUserObjectGuid: string | null | undefined = null;
    reason: string | null | undefined = null;
    createdByUserObjectGuid: string | null | undefined = null;
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
export class ScheduleOverrideSubmitData {
    id!: bigint | number;
    onCallScheduleId!: bigint | number;
    scheduleLayerId: bigint | number | null = null;
    startDateTime!: string;      // ISO 8601
    endDateTime!: string;      // ISO 8601
    scheduleOverrideTypeId!: bigint | number;
    originalUserObjectGuid: string | null = null;
    replacementUserObjectGuid: string | null = null;
    reason: string | null = null;
    createdByUserObjectGuid!: string;
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

export class ScheduleOverrideBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ScheduleOverrideChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `scheduleOverride.ScheduleOverrideChildren$` — use with `| async` in templates
//        • Promise:    `scheduleOverride.ScheduleOverrideChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="scheduleOverride.ScheduleOverrideChildren$ | async"`), or
//        • Access the promise getter (`scheduleOverride.ScheduleOverrideChildren` or `await scheduleOverride.ScheduleOverrideChildren`)
//    - Simply reading `scheduleOverride.ScheduleOverrideChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await scheduleOverride.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ScheduleOverrideData {
    id!: bigint | number;
    onCallScheduleId!: bigint | number;
    scheduleLayerId!: bigint | number;
    startDateTime!: string;      // ISO 8601
    endDateTime!: string;      // ISO 8601
    scheduleOverrideTypeId!: bigint | number;
    originalUserObjectGuid!: string | null;
    replacementUserObjectGuid!: string | null;
    reason!: string | null;
    createdByUserObjectGuid!: string;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    onCallSchedule: OnCallScheduleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduleLayer: ScheduleLayerData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduleOverrideType: ScheduleOverrideTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _scheduleOverrideChangeHistories: ScheduleOverrideChangeHistoryData[] | null = null;
    private _scheduleOverrideChangeHistoriesPromise: Promise<ScheduleOverrideChangeHistoryData[]> | null  = null;
    private _scheduleOverrideChangeHistoriesSubject = new BehaviorSubject<ScheduleOverrideChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ScheduleOverrideData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ScheduleOverrideData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduleOverrideData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ScheduleOverrideChangeHistories$ = this._scheduleOverrideChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduleOverrideChangeHistories === null && this._scheduleOverrideChangeHistoriesPromise === null) {
            this.loadScheduleOverrideChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduleOverrideChangeHistoriesCount$ = ScheduleOverrideChangeHistoryService.Instance.GetScheduleOverrideChangeHistoriesRowCount({scheduleOverrideId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ScheduleOverrideData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.scheduleOverride.Reload();
  //
  //  Non Async:
  //
  //     scheduleOverride[0].Reload().then(x => {
  //        this.scheduleOverride = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ScheduleOverrideService.Instance.GetScheduleOverride(this.id, includeRelations)
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
     this._scheduleOverrideChangeHistories = null;
     this._scheduleOverrideChangeHistoriesPromise = null;
     this._scheduleOverrideChangeHistoriesSubject.next(null);

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
     * Gets the ScheduleOverrideChangeHistories for this ScheduleOverride.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduleOverride.ScheduleOverrideChangeHistories.then(scheduleOverrides => { ... })
     *   or
     *   await this.scheduleOverride.scheduleOverrides
     *
    */
    public get ScheduleOverrideChangeHistories(): Promise<ScheduleOverrideChangeHistoryData[]> {
        if (this._scheduleOverrideChangeHistories !== null) {
            return Promise.resolve(this._scheduleOverrideChangeHistories);
        }

        if (this._scheduleOverrideChangeHistoriesPromise !== null) {
            return this._scheduleOverrideChangeHistoriesPromise;
        }

        // Start the load
        this.loadScheduleOverrideChangeHistories();

        return this._scheduleOverrideChangeHistoriesPromise!;
    }



    private loadScheduleOverrideChangeHistories(): void {

        this._scheduleOverrideChangeHistoriesPromise = lastValueFrom(
            ScheduleOverrideService.Instance.GetScheduleOverrideChangeHistoriesForScheduleOverride(this.id)
        )
        .then(ScheduleOverrideChangeHistories => {
            this._scheduleOverrideChangeHistories = ScheduleOverrideChangeHistories ?? [];
            this._scheduleOverrideChangeHistoriesSubject.next(this._scheduleOverrideChangeHistories);
            return this._scheduleOverrideChangeHistories;
         })
        .catch(err => {
            this._scheduleOverrideChangeHistories = [];
            this._scheduleOverrideChangeHistoriesSubject.next(this._scheduleOverrideChangeHistories);
            throw err;
        })
        .finally(() => {
            this._scheduleOverrideChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduleOverrideChangeHistory. Call after mutations to force refresh.
     */
    public ClearScheduleOverrideChangeHistoriesCache(): void {
        this._scheduleOverrideChangeHistories = null;
        this._scheduleOverrideChangeHistoriesPromise = null;
        this._scheduleOverrideChangeHistoriesSubject.next(this._scheduleOverrideChangeHistories);      // Emit to observable
    }

    public get HasScheduleOverrideChangeHistories(): Promise<boolean> {
        return this.ScheduleOverrideChangeHistories.then(scheduleOverrideChangeHistories => scheduleOverrideChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (scheduleOverride.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await scheduleOverride.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ScheduleOverrideData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ScheduleOverrideData>> {
        const info = await lastValueFrom(
            ScheduleOverrideService.Instance.GetScheduleOverrideChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ScheduleOverrideData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ScheduleOverrideData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ScheduleOverrideSubmitData {
        return ScheduleOverrideService.Instance.ConvertToScheduleOverrideSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ScheduleOverrideService extends SecureEndpointBase {

    private static _instance: ScheduleOverrideService;
    private listCache: Map<string, Observable<Array<ScheduleOverrideData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ScheduleOverrideBasicListData>>>;
    private recordCache: Map<string, Observable<ScheduleOverrideData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private scheduleOverrideChangeHistoryService: ScheduleOverrideChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ScheduleOverrideData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ScheduleOverrideBasicListData>>>();
        this.recordCache = new Map<string, Observable<ScheduleOverrideData>>();

        ScheduleOverrideService._instance = this;
    }

    public static get Instance(): ScheduleOverrideService {
      return ScheduleOverrideService._instance;
    }


    public ClearListCaches(config: ScheduleOverrideQueryParameters | null = null) {

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


    public ConvertToScheduleOverrideSubmitData(data: ScheduleOverrideData): ScheduleOverrideSubmitData {

        let output = new ScheduleOverrideSubmitData();

        output.id = data.id;
        output.onCallScheduleId = data.onCallScheduleId;
        output.scheduleLayerId = data.scheduleLayerId;
        output.startDateTime = data.startDateTime;
        output.endDateTime = data.endDateTime;
        output.scheduleOverrideTypeId = data.scheduleOverrideTypeId;
        output.originalUserObjectGuid = data.originalUserObjectGuid;
        output.replacementUserObjectGuid = data.replacementUserObjectGuid;
        output.reason = data.reason;
        output.createdByUserObjectGuid = data.createdByUserObjectGuid;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetScheduleOverride(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduleOverrideData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const scheduleOverride$ = this.requestScheduleOverride(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleOverride", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, scheduleOverride$);

            return scheduleOverride$;
        }

        return this.recordCache.get(configHash) as Observable<ScheduleOverrideData>;
    }

    private requestScheduleOverride(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduleOverrideData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduleOverrideData>(this.baseUrl + 'api/ScheduleOverride/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveScheduleOverride(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleOverride(id, includeRelations));
            }));
    }

    public GetScheduleOverrideList(config: ScheduleOverrideQueryParameters | any = null) : Observable<Array<ScheduleOverrideData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const scheduleOverrideList$ = this.requestScheduleOverrideList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleOverride list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, scheduleOverrideList$);

            return scheduleOverrideList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ScheduleOverrideData>>;
    }


    private requestScheduleOverrideList(config: ScheduleOverrideQueryParameters | any) : Observable <Array<ScheduleOverrideData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduleOverrideData>>(this.baseUrl + 'api/ScheduleOverrides', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveScheduleOverrideList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleOverrideList(config));
            }));
    }

    public GetScheduleOverridesRowCount(config: ScheduleOverrideQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const scheduleOverridesRowCount$ = this.requestScheduleOverridesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleOverrides row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, scheduleOverridesRowCount$);

            return scheduleOverridesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestScheduleOverridesRowCount(config: ScheduleOverrideQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ScheduleOverrides/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleOverridesRowCount(config));
            }));
    }

    public GetScheduleOverridesBasicListData(config: ScheduleOverrideQueryParameters | any = null) : Observable<Array<ScheduleOverrideBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const scheduleOverridesBasicListData$ = this.requestScheduleOverridesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleOverrides basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, scheduleOverridesBasicListData$);

            return scheduleOverridesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ScheduleOverrideBasicListData>>;
    }


    private requestScheduleOverridesBasicListData(config: ScheduleOverrideQueryParameters | any) : Observable<Array<ScheduleOverrideBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduleOverrideBasicListData>>(this.baseUrl + 'api/ScheduleOverrides/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleOverridesBasicListData(config));
            }));

    }


    public PutScheduleOverride(id: bigint | number, scheduleOverride: ScheduleOverrideSubmitData) : Observable<ScheduleOverrideData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduleOverrideData>(this.baseUrl + 'api/ScheduleOverride/' + id.toString(), scheduleOverride, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduleOverride(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutScheduleOverride(id, scheduleOverride));
            }));
    }


    public PostScheduleOverride(scheduleOverride: ScheduleOverrideSubmitData) : Observable<ScheduleOverrideData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ScheduleOverrideData>(this.baseUrl + 'api/ScheduleOverride', scheduleOverride, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduleOverride(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostScheduleOverride(scheduleOverride));
            }));
    }

  
    public DeleteScheduleOverride(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ScheduleOverride/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteScheduleOverride(id));
            }));
    }

    public RollbackScheduleOverride(id: bigint | number, versionNumber: bigint | number) : Observable<ScheduleOverrideData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduleOverrideData>(this.baseUrl + 'api/ScheduleOverride/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduleOverride(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackScheduleOverride(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ScheduleOverride.
     */
    public GetScheduleOverrideChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ScheduleOverrideData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduleOverrideData>>(this.baseUrl + 'api/ScheduleOverride/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduleOverrideChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ScheduleOverride.
     */
    public GetScheduleOverrideAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ScheduleOverrideData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduleOverrideData>[]>(this.baseUrl + 'api/ScheduleOverride/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduleOverrideAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ScheduleOverride.
     */
    public GetScheduleOverrideVersion(id: bigint | number, version: number): Observable<ScheduleOverrideData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduleOverrideData>(this.baseUrl + 'api/ScheduleOverride/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduleOverride(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduleOverrideVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ScheduleOverride at a specific point in time.
     */
    public GetScheduleOverrideStateAtTime(id: bigint | number, time: string): Observable<ScheduleOverrideData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduleOverrideData>(this.baseUrl + 'api/ScheduleOverride/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduleOverride(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduleOverrideStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ScheduleOverrideQueryParameters | any): string {

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

    public userIsAlertingScheduleOverrideReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingScheduleOverrideReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.ScheduleOverrides
        //
        if (userIsAlertingScheduleOverrideReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingScheduleOverrideReader = user.readPermission >= 1;
            } else {
                userIsAlertingScheduleOverrideReader = false;
            }
        }

        return userIsAlertingScheduleOverrideReader;
    }


    public userIsAlertingScheduleOverrideWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingScheduleOverrideWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.ScheduleOverrides
        //
        if (userIsAlertingScheduleOverrideWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingScheduleOverrideWriter = user.writePermission >= 100;
          } else {
            userIsAlertingScheduleOverrideWriter = false;
          }      
        }

        return userIsAlertingScheduleOverrideWriter;
    }

    public GetScheduleOverrideChangeHistoriesForScheduleOverride(scheduleOverrideId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduleOverrideChangeHistoryData[]> {
        return this.scheduleOverrideChangeHistoryService.GetScheduleOverrideChangeHistoryList({
            scheduleOverrideId: scheduleOverrideId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ScheduleOverrideData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ScheduleOverrideData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ScheduleOverrideTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveScheduleOverride(raw: any): ScheduleOverrideData {
    if (!raw) return raw;

    //
    // Create a ScheduleOverrideData object instance with correct prototype
    //
    const revived = Object.create(ScheduleOverrideData.prototype) as ScheduleOverrideData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._scheduleOverrideChangeHistories = null;
    (revived as any)._scheduleOverrideChangeHistoriesPromise = null;
    (revived as any)._scheduleOverrideChangeHistoriesSubject = new BehaviorSubject<ScheduleOverrideChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadScheduleOverrideXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ScheduleOverrideChangeHistories$ = (revived as any)._scheduleOverrideChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduleOverrideChangeHistories === null && (revived as any)._scheduleOverrideChangeHistoriesPromise === null) {
                (revived as any).loadScheduleOverrideChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduleOverrideChangeHistoriesCount$ = ScheduleOverrideChangeHistoryService.Instance.GetScheduleOverrideChangeHistoriesRowCount({scheduleOverrideId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduleOverrideData> | null>(null);

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

  private ReviveScheduleOverrideList(rawList: any[]): ScheduleOverrideData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveScheduleOverride(raw));
  }

}
