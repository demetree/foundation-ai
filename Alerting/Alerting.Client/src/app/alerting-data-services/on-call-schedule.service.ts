/*

   GENERATED SERVICE FOR THE ONCALLSCHEDULE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the OnCallSchedule table.

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
import { OnCallScheduleChangeHistoryService, OnCallScheduleChangeHistoryData } from './on-call-schedule-change-history.service';
import { ScheduleLayerService, ScheduleLayerData } from './schedule-layer.service';
import { ScheduleOverrideService, ScheduleOverrideData } from './schedule-override.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class OnCallScheduleQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    timeZoneId: string | null | undefined = null;
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
export class OnCallScheduleSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    timeZoneId!: string;
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

export class OnCallScheduleBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. OnCallScheduleChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `onCallSchedule.OnCallScheduleChildren$` — use with `| async` in templates
//        • Promise:    `onCallSchedule.OnCallScheduleChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="onCallSchedule.OnCallScheduleChildren$ | async"`), or
//        • Access the promise getter (`onCallSchedule.OnCallScheduleChildren` or `await onCallSchedule.OnCallScheduleChildren`)
//    - Simply reading `onCallSchedule.OnCallScheduleChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await onCallSchedule.Reload()` to refresh the entire object and clear all lazy caches.
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
export class OnCallScheduleData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    timeZoneId!: string;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _onCallScheduleChangeHistories: OnCallScheduleChangeHistoryData[] | null = null;
    private _onCallScheduleChangeHistoriesPromise: Promise<OnCallScheduleChangeHistoryData[]> | null  = null;
    private _onCallScheduleChangeHistoriesSubject = new BehaviorSubject<OnCallScheduleChangeHistoryData[] | null>(null);

                
    private _scheduleLayers: ScheduleLayerData[] | null = null;
    private _scheduleLayersPromise: Promise<ScheduleLayerData[]> | null  = null;
    private _scheduleLayersSubject = new BehaviorSubject<ScheduleLayerData[] | null>(null);

                
    private _scheduleOverrides: ScheduleOverrideData[] | null = null;
    private _scheduleOverridesPromise: Promise<ScheduleOverrideData[]> | null  = null;
    private _scheduleOverridesSubject = new BehaviorSubject<ScheduleOverrideData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<OnCallScheduleData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<OnCallScheduleData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<OnCallScheduleData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public OnCallScheduleChangeHistories$ = this._onCallScheduleChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._onCallScheduleChangeHistories === null && this._onCallScheduleChangeHistoriesPromise === null) {
            this.loadOnCallScheduleChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public OnCallScheduleChangeHistoriesCount$ = OnCallScheduleChangeHistoryService.Instance.GetOnCallScheduleChangeHistoriesRowCount({onCallScheduleId: this.id,
      active: true,
      deleted: false
    });



    public ScheduleLayers$ = this._scheduleLayersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduleLayers === null && this._scheduleLayersPromise === null) {
            this.loadScheduleLayers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduleLayersCount$ = ScheduleLayerService.Instance.GetScheduleLayersRowCount({onCallScheduleId: this.id,
      active: true,
      deleted: false
    });



    public ScheduleOverrides$ = this._scheduleOverridesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduleOverrides === null && this._scheduleOverridesPromise === null) {
            this.loadScheduleOverrides(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduleOverridesCount$ = ScheduleOverrideService.Instance.GetScheduleOverridesRowCount({onCallScheduleId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any OnCallScheduleData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.onCallSchedule.Reload();
  //
  //  Non Async:
  //
  //     onCallSchedule[0].Reload().then(x => {
  //        this.onCallSchedule = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      OnCallScheduleService.Instance.GetOnCallSchedule(this.id, includeRelations)
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
     this._onCallScheduleChangeHistories = null;
     this._onCallScheduleChangeHistoriesPromise = null;
     this._onCallScheduleChangeHistoriesSubject.next(null);

     this._scheduleLayers = null;
     this._scheduleLayersPromise = null;
     this._scheduleLayersSubject.next(null);

     this._scheduleOverrides = null;
     this._scheduleOverridesPromise = null;
     this._scheduleOverridesSubject.next(null);

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
     * Gets the OnCallScheduleChangeHistories for this OnCallSchedule.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.onCallSchedule.OnCallScheduleChangeHistories.then(onCallSchedules => { ... })
     *   or
     *   await this.onCallSchedule.onCallSchedules
     *
    */
    public get OnCallScheduleChangeHistories(): Promise<OnCallScheduleChangeHistoryData[]> {
        if (this._onCallScheduleChangeHistories !== null) {
            return Promise.resolve(this._onCallScheduleChangeHistories);
        }

        if (this._onCallScheduleChangeHistoriesPromise !== null) {
            return this._onCallScheduleChangeHistoriesPromise;
        }

        // Start the load
        this.loadOnCallScheduleChangeHistories();

        return this._onCallScheduleChangeHistoriesPromise!;
    }



    private loadOnCallScheduleChangeHistories(): void {

        this._onCallScheduleChangeHistoriesPromise = lastValueFrom(
            OnCallScheduleService.Instance.GetOnCallScheduleChangeHistoriesForOnCallSchedule(this.id)
        )
        .then(OnCallScheduleChangeHistories => {
            this._onCallScheduleChangeHistories = OnCallScheduleChangeHistories ?? [];
            this._onCallScheduleChangeHistoriesSubject.next(this._onCallScheduleChangeHistories);
            return this._onCallScheduleChangeHistories;
         })
        .catch(err => {
            this._onCallScheduleChangeHistories = [];
            this._onCallScheduleChangeHistoriesSubject.next(this._onCallScheduleChangeHistories);
            throw err;
        })
        .finally(() => {
            this._onCallScheduleChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached OnCallScheduleChangeHistory. Call after mutations to force refresh.
     */
    public ClearOnCallScheduleChangeHistoriesCache(): void {
        this._onCallScheduleChangeHistories = null;
        this._onCallScheduleChangeHistoriesPromise = null;
        this._onCallScheduleChangeHistoriesSubject.next(this._onCallScheduleChangeHistories);      // Emit to observable
    }

    public get HasOnCallScheduleChangeHistories(): Promise<boolean> {
        return this.OnCallScheduleChangeHistories.then(onCallScheduleChangeHistories => onCallScheduleChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ScheduleLayers for this OnCallSchedule.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.onCallSchedule.ScheduleLayers.then(onCallSchedules => { ... })
     *   or
     *   await this.onCallSchedule.onCallSchedules
     *
    */
    public get ScheduleLayers(): Promise<ScheduleLayerData[]> {
        if (this._scheduleLayers !== null) {
            return Promise.resolve(this._scheduleLayers);
        }

        if (this._scheduleLayersPromise !== null) {
            return this._scheduleLayersPromise;
        }

        // Start the load
        this.loadScheduleLayers();

        return this._scheduleLayersPromise!;
    }



    private loadScheduleLayers(): void {

        this._scheduleLayersPromise = lastValueFrom(
            OnCallScheduleService.Instance.GetScheduleLayersForOnCallSchedule(this.id)
        )
        .then(ScheduleLayers => {
            this._scheduleLayers = ScheduleLayers ?? [];
            this._scheduleLayersSubject.next(this._scheduleLayers);
            return this._scheduleLayers;
         })
        .catch(err => {
            this._scheduleLayers = [];
            this._scheduleLayersSubject.next(this._scheduleLayers);
            throw err;
        })
        .finally(() => {
            this._scheduleLayersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduleLayer. Call after mutations to force refresh.
     */
    public ClearScheduleLayersCache(): void {
        this._scheduleLayers = null;
        this._scheduleLayersPromise = null;
        this._scheduleLayersSubject.next(this._scheduleLayers);      // Emit to observable
    }

    public get HasScheduleLayers(): Promise<boolean> {
        return this.ScheduleLayers.then(scheduleLayers => scheduleLayers.length > 0);
    }


    /**
     *
     * Gets the ScheduleOverrides for this OnCallSchedule.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.onCallSchedule.ScheduleOverrides.then(onCallSchedules => { ... })
     *   or
     *   await this.onCallSchedule.onCallSchedules
     *
    */
    public get ScheduleOverrides(): Promise<ScheduleOverrideData[]> {
        if (this._scheduleOverrides !== null) {
            return Promise.resolve(this._scheduleOverrides);
        }

        if (this._scheduleOverridesPromise !== null) {
            return this._scheduleOverridesPromise;
        }

        // Start the load
        this.loadScheduleOverrides();

        return this._scheduleOverridesPromise!;
    }



    private loadScheduleOverrides(): void {

        this._scheduleOverridesPromise = lastValueFrom(
            OnCallScheduleService.Instance.GetScheduleOverridesForOnCallSchedule(this.id)
        )
        .then(ScheduleOverrides => {
            this._scheduleOverrides = ScheduleOverrides ?? [];
            this._scheduleOverridesSubject.next(this._scheduleOverrides);
            return this._scheduleOverrides;
         })
        .catch(err => {
            this._scheduleOverrides = [];
            this._scheduleOverridesSubject.next(this._scheduleOverrides);
            throw err;
        })
        .finally(() => {
            this._scheduleOverridesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduleOverride. Call after mutations to force refresh.
     */
    public ClearScheduleOverridesCache(): void {
        this._scheduleOverrides = null;
        this._scheduleOverridesPromise = null;
        this._scheduleOverridesSubject.next(this._scheduleOverrides);      // Emit to observable
    }

    public get HasScheduleOverrides(): Promise<boolean> {
        return this.ScheduleOverrides.then(scheduleOverrides => scheduleOverrides.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (onCallSchedule.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await onCallSchedule.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<OnCallScheduleData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<OnCallScheduleData>> {
        const info = await lastValueFrom(
            OnCallScheduleService.Instance.GetOnCallScheduleChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this OnCallScheduleData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this OnCallScheduleData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): OnCallScheduleSubmitData {
        return OnCallScheduleService.Instance.ConvertToOnCallScheduleSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class OnCallScheduleService extends SecureEndpointBase {

    private static _instance: OnCallScheduleService;
    private listCache: Map<string, Observable<Array<OnCallScheduleData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<OnCallScheduleBasicListData>>>;
    private recordCache: Map<string, Observable<OnCallScheduleData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private onCallScheduleChangeHistoryService: OnCallScheduleChangeHistoryService,
        private scheduleLayerService: ScheduleLayerService,
        private scheduleOverrideService: ScheduleOverrideService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<OnCallScheduleData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<OnCallScheduleBasicListData>>>();
        this.recordCache = new Map<string, Observable<OnCallScheduleData>>();

        OnCallScheduleService._instance = this;
    }

    public static get Instance(): OnCallScheduleService {
      return OnCallScheduleService._instance;
    }


    public ClearListCaches(config: OnCallScheduleQueryParameters | null = null) {

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


    public ConvertToOnCallScheduleSubmitData(data: OnCallScheduleData): OnCallScheduleSubmitData {

        let output = new OnCallScheduleSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.timeZoneId = data.timeZoneId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetOnCallSchedule(id: bigint | number, includeRelations: boolean = true) : Observable<OnCallScheduleData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const onCallSchedule$ = this.requestOnCallSchedule(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get OnCallSchedule", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, onCallSchedule$);

            return onCallSchedule$;
        }

        return this.recordCache.get(configHash) as Observable<OnCallScheduleData>;
    }

    private requestOnCallSchedule(id: bigint | number, includeRelations: boolean = true) : Observable<OnCallScheduleData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<OnCallScheduleData>(this.baseUrl + 'api/OnCallSchedule/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveOnCallSchedule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestOnCallSchedule(id, includeRelations));
            }));
    }

    public GetOnCallScheduleList(config: OnCallScheduleQueryParameters | any = null) : Observable<Array<OnCallScheduleData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const onCallScheduleList$ = this.requestOnCallScheduleList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get OnCallSchedule list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, onCallScheduleList$);

            return onCallScheduleList$;
        }

        return this.listCache.get(configHash) as Observable<Array<OnCallScheduleData>>;
    }


    private requestOnCallScheduleList(config: OnCallScheduleQueryParameters | any) : Observable <Array<OnCallScheduleData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<OnCallScheduleData>>(this.baseUrl + 'api/OnCallSchedules', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveOnCallScheduleList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestOnCallScheduleList(config));
            }));
    }

    public GetOnCallSchedulesRowCount(config: OnCallScheduleQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const onCallSchedulesRowCount$ = this.requestOnCallSchedulesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get OnCallSchedules row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, onCallSchedulesRowCount$);

            return onCallSchedulesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestOnCallSchedulesRowCount(config: OnCallScheduleQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/OnCallSchedules/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestOnCallSchedulesRowCount(config));
            }));
    }

    public GetOnCallSchedulesBasicListData(config: OnCallScheduleQueryParameters | any = null) : Observable<Array<OnCallScheduleBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const onCallSchedulesBasicListData$ = this.requestOnCallSchedulesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get OnCallSchedules basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, onCallSchedulesBasicListData$);

            return onCallSchedulesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<OnCallScheduleBasicListData>>;
    }


    private requestOnCallSchedulesBasicListData(config: OnCallScheduleQueryParameters | any) : Observable<Array<OnCallScheduleBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<OnCallScheduleBasicListData>>(this.baseUrl + 'api/OnCallSchedules/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestOnCallSchedulesBasicListData(config));
            }));

    }


    public PutOnCallSchedule(id: bigint | number, onCallSchedule: OnCallScheduleSubmitData) : Observable<OnCallScheduleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<OnCallScheduleData>(this.baseUrl + 'api/OnCallSchedule/' + id.toString(), onCallSchedule, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOnCallSchedule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutOnCallSchedule(id, onCallSchedule));
            }));
    }


    public PostOnCallSchedule(onCallSchedule: OnCallScheduleSubmitData) : Observable<OnCallScheduleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<OnCallScheduleData>(this.baseUrl + 'api/OnCallSchedule', onCallSchedule, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOnCallSchedule(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostOnCallSchedule(onCallSchedule));
            }));
    }

  
    public DeleteOnCallSchedule(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/OnCallSchedule/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteOnCallSchedule(id));
            }));
    }

    public RollbackOnCallSchedule(id: bigint | number, versionNumber: bigint | number) : Observable<OnCallScheduleData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<OnCallScheduleData>(this.baseUrl + 'api/OnCallSchedule/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOnCallSchedule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackOnCallSchedule(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a OnCallSchedule.
     */
    public GetOnCallScheduleChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<OnCallScheduleData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<OnCallScheduleData>>(this.baseUrl + 'api/OnCallSchedule/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetOnCallScheduleChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a OnCallSchedule.
     */
    public GetOnCallScheduleAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<OnCallScheduleData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<OnCallScheduleData>[]>(this.baseUrl + 'api/OnCallSchedule/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetOnCallScheduleAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a OnCallSchedule.
     */
    public GetOnCallScheduleVersion(id: bigint | number, version: number): Observable<OnCallScheduleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<OnCallScheduleData>(this.baseUrl + 'api/OnCallSchedule/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveOnCallSchedule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetOnCallScheduleVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a OnCallSchedule at a specific point in time.
     */
    public GetOnCallScheduleStateAtTime(id: bigint | number, time: string): Observable<OnCallScheduleData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<OnCallScheduleData>(this.baseUrl + 'api/OnCallSchedule/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveOnCallSchedule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetOnCallScheduleStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: OnCallScheduleQueryParameters | any): string {

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

    public userIsAlertingOnCallScheduleReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingOnCallScheduleReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.OnCallSchedules
        //
        if (userIsAlertingOnCallScheduleReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingOnCallScheduleReader = user.readPermission >= 1;
            } else {
                userIsAlertingOnCallScheduleReader = false;
            }
        }

        return userIsAlertingOnCallScheduleReader;
    }


    public userIsAlertingOnCallScheduleWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingOnCallScheduleWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.OnCallSchedules
        //
        if (userIsAlertingOnCallScheduleWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingOnCallScheduleWriter = user.writePermission >= 100;
          } else {
            userIsAlertingOnCallScheduleWriter = false;
          }      
        }

        return userIsAlertingOnCallScheduleWriter;
    }

    public GetOnCallScheduleChangeHistoriesForOnCallSchedule(onCallScheduleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<OnCallScheduleChangeHistoryData[]> {
        return this.onCallScheduleChangeHistoryService.GetOnCallScheduleChangeHistoryList({
            onCallScheduleId: onCallScheduleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduleLayersForOnCallSchedule(onCallScheduleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduleLayerData[]> {
        return this.scheduleLayerService.GetScheduleLayerList({
            onCallScheduleId: onCallScheduleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduleOverridesForOnCallSchedule(onCallScheduleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduleOverrideData[]> {
        return this.scheduleOverrideService.GetScheduleOverrideList({
            onCallScheduleId: onCallScheduleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full OnCallScheduleData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the OnCallScheduleData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when OnCallScheduleTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveOnCallSchedule(raw: any): OnCallScheduleData {
    if (!raw) return raw;

    //
    // Create a OnCallScheduleData object instance with correct prototype
    //
    const revived = Object.create(OnCallScheduleData.prototype) as OnCallScheduleData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._onCallScheduleChangeHistories = null;
    (revived as any)._onCallScheduleChangeHistoriesPromise = null;
    (revived as any)._onCallScheduleChangeHistoriesSubject = new BehaviorSubject<OnCallScheduleChangeHistoryData[] | null>(null);

    (revived as any)._scheduleLayers = null;
    (revived as any)._scheduleLayersPromise = null;
    (revived as any)._scheduleLayersSubject = new BehaviorSubject<ScheduleLayerData[] | null>(null);

    (revived as any)._scheduleOverrides = null;
    (revived as any)._scheduleOverridesPromise = null;
    (revived as any)._scheduleOverridesSubject = new BehaviorSubject<ScheduleOverrideData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadOnCallScheduleXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).OnCallScheduleChangeHistories$ = (revived as any)._onCallScheduleChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._onCallScheduleChangeHistories === null && (revived as any)._onCallScheduleChangeHistoriesPromise === null) {
                (revived as any).loadOnCallScheduleChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).OnCallScheduleChangeHistoriesCount$ = OnCallScheduleChangeHistoryService.Instance.GetOnCallScheduleChangeHistoriesRowCount({onCallScheduleId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduleLayers$ = (revived as any)._scheduleLayersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduleLayers === null && (revived as any)._scheduleLayersPromise === null) {
                (revived as any).loadScheduleLayers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduleLayersCount$ = ScheduleLayerService.Instance.GetScheduleLayersRowCount({onCallScheduleId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduleOverrides$ = (revived as any)._scheduleOverridesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduleOverrides === null && (revived as any)._scheduleOverridesPromise === null) {
                (revived as any).loadScheduleOverrides();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduleOverridesCount$ = ScheduleOverrideService.Instance.GetScheduleOverridesRowCount({onCallScheduleId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<OnCallScheduleData> | null>(null);

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

  private ReviveOnCallScheduleList(rawList: any[]): OnCallScheduleData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveOnCallSchedule(raw));
  }

}
