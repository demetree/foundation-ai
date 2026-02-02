/*

   GENERATED SERVICE FOR THE SCHEDULELAYER TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ScheduleLayer table.

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
import { ScheduleLayerChangeHistoryService, ScheduleLayerChangeHistoryData } from './schedule-layer-change-history.service';
import { ScheduleLayerMemberService, ScheduleLayerMemberData } from './schedule-layer-member.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ScheduleLayerQueryParameters {
    onCallScheduleId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    layerLevel: bigint | number | null | undefined = null;
    rotationStart: string | null | undefined = null;        // ISO 8601
    rotationDays: bigint | number | null | undefined = null;
    handoffTime: string | null | undefined = null;
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
export class ScheduleLayerSubmitData {
    id!: bigint | number;
    onCallScheduleId!: bigint | number;
    name!: string;
    description: string | null = null;
    layerLevel!: bigint | number;
    rotationStart!: string;      // ISO 8601
    rotationDays!: bigint | number;
    handoffTime!: string;
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

export class ScheduleLayerBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ScheduleLayerChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `scheduleLayer.ScheduleLayerChildren$` — use with `| async` in templates
//        • Promise:    `scheduleLayer.ScheduleLayerChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="scheduleLayer.ScheduleLayerChildren$ | async"`), or
//        • Access the promise getter (`scheduleLayer.ScheduleLayerChildren` or `await scheduleLayer.ScheduleLayerChildren`)
//    - Simply reading `scheduleLayer.ScheduleLayerChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await scheduleLayer.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ScheduleLayerData {
    id!: bigint | number;
    onCallScheduleId!: bigint | number;
    name!: string;
    description!: string | null;
    layerLevel!: bigint | number;
    rotationStart!: string;      // ISO 8601
    rotationDays!: bigint | number;
    handoffTime!: string;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    onCallSchedule: OnCallScheduleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _scheduleLayerChangeHistories: ScheduleLayerChangeHistoryData[] | null = null;
    private _scheduleLayerChangeHistoriesPromise: Promise<ScheduleLayerChangeHistoryData[]> | null  = null;
    private _scheduleLayerChangeHistoriesSubject = new BehaviorSubject<ScheduleLayerChangeHistoryData[] | null>(null);

                
    private _scheduleLayerMembers: ScheduleLayerMemberData[] | null = null;
    private _scheduleLayerMembersPromise: Promise<ScheduleLayerMemberData[]> | null  = null;
    private _scheduleLayerMembersSubject = new BehaviorSubject<ScheduleLayerMemberData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ScheduleLayerData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ScheduleLayerData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduleLayerData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ScheduleLayerChangeHistories$ = this._scheduleLayerChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduleLayerChangeHistories === null && this._scheduleLayerChangeHistoriesPromise === null) {
            this.loadScheduleLayerChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduleLayerChangeHistoriesCount$ = ScheduleLayerChangeHistoryService.Instance.GetScheduleLayerChangeHistoriesRowCount({scheduleLayerId: this.id,
      active: true,
      deleted: false
    });



    public ScheduleLayerMembers$ = this._scheduleLayerMembersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduleLayerMembers === null && this._scheduleLayerMembersPromise === null) {
            this.loadScheduleLayerMembers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduleLayerMembersCount$ = ScheduleLayerMemberService.Instance.GetScheduleLayerMembersRowCount({scheduleLayerId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ScheduleLayerData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.scheduleLayer.Reload();
  //
  //  Non Async:
  //
  //     scheduleLayer[0].Reload().then(x => {
  //        this.scheduleLayer = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ScheduleLayerService.Instance.GetScheduleLayer(this.id, includeRelations)
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
     this._scheduleLayerChangeHistories = null;
     this._scheduleLayerChangeHistoriesPromise = null;
     this._scheduleLayerChangeHistoriesSubject.next(null);

     this._scheduleLayerMembers = null;
     this._scheduleLayerMembersPromise = null;
     this._scheduleLayerMembersSubject.next(null);

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
     * Gets the ScheduleLayerChangeHistories for this ScheduleLayer.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduleLayer.ScheduleLayerChangeHistories.then(scheduleLayers => { ... })
     *   or
     *   await this.scheduleLayer.scheduleLayers
     *
    */
    public get ScheduleLayerChangeHistories(): Promise<ScheduleLayerChangeHistoryData[]> {
        if (this._scheduleLayerChangeHistories !== null) {
            return Promise.resolve(this._scheduleLayerChangeHistories);
        }

        if (this._scheduleLayerChangeHistoriesPromise !== null) {
            return this._scheduleLayerChangeHistoriesPromise;
        }

        // Start the load
        this.loadScheduleLayerChangeHistories();

        return this._scheduleLayerChangeHistoriesPromise!;
    }



    private loadScheduleLayerChangeHistories(): void {

        this._scheduleLayerChangeHistoriesPromise = lastValueFrom(
            ScheduleLayerService.Instance.GetScheduleLayerChangeHistoriesForScheduleLayer(this.id)
        )
        .then(ScheduleLayerChangeHistories => {
            this._scheduleLayerChangeHistories = ScheduleLayerChangeHistories ?? [];
            this._scheduleLayerChangeHistoriesSubject.next(this._scheduleLayerChangeHistories);
            return this._scheduleLayerChangeHistories;
         })
        .catch(err => {
            this._scheduleLayerChangeHistories = [];
            this._scheduleLayerChangeHistoriesSubject.next(this._scheduleLayerChangeHistories);
            throw err;
        })
        .finally(() => {
            this._scheduleLayerChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduleLayerChangeHistory. Call after mutations to force refresh.
     */
    public ClearScheduleLayerChangeHistoriesCache(): void {
        this._scheduleLayerChangeHistories = null;
        this._scheduleLayerChangeHistoriesPromise = null;
        this._scheduleLayerChangeHistoriesSubject.next(this._scheduleLayerChangeHistories);      // Emit to observable
    }

    public get HasScheduleLayerChangeHistories(): Promise<boolean> {
        return this.ScheduleLayerChangeHistories.then(scheduleLayerChangeHistories => scheduleLayerChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ScheduleLayerMembers for this ScheduleLayer.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduleLayer.ScheduleLayerMembers.then(scheduleLayers => { ... })
     *   or
     *   await this.scheduleLayer.scheduleLayers
     *
    */
    public get ScheduleLayerMembers(): Promise<ScheduleLayerMemberData[]> {
        if (this._scheduleLayerMembers !== null) {
            return Promise.resolve(this._scheduleLayerMembers);
        }

        if (this._scheduleLayerMembersPromise !== null) {
            return this._scheduleLayerMembersPromise;
        }

        // Start the load
        this.loadScheduleLayerMembers();

        return this._scheduleLayerMembersPromise!;
    }



    private loadScheduleLayerMembers(): void {

        this._scheduleLayerMembersPromise = lastValueFrom(
            ScheduleLayerService.Instance.GetScheduleLayerMembersForScheduleLayer(this.id)
        )
        .then(ScheduleLayerMembers => {
            this._scheduleLayerMembers = ScheduleLayerMembers ?? [];
            this._scheduleLayerMembersSubject.next(this._scheduleLayerMembers);
            return this._scheduleLayerMembers;
         })
        .catch(err => {
            this._scheduleLayerMembers = [];
            this._scheduleLayerMembersSubject.next(this._scheduleLayerMembers);
            throw err;
        })
        .finally(() => {
            this._scheduleLayerMembersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduleLayerMember. Call after mutations to force refresh.
     */
    public ClearScheduleLayerMembersCache(): void {
        this._scheduleLayerMembers = null;
        this._scheduleLayerMembersPromise = null;
        this._scheduleLayerMembersSubject.next(this._scheduleLayerMembers);      // Emit to observable
    }

    public get HasScheduleLayerMembers(): Promise<boolean> {
        return this.ScheduleLayerMembers.then(scheduleLayerMembers => scheduleLayerMembers.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (scheduleLayer.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await scheduleLayer.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ScheduleLayerData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ScheduleLayerData>> {
        const info = await lastValueFrom(
            ScheduleLayerService.Instance.GetScheduleLayerChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ScheduleLayerData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ScheduleLayerData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ScheduleLayerSubmitData {
        return ScheduleLayerService.Instance.ConvertToScheduleLayerSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ScheduleLayerService extends SecureEndpointBase {

    private static _instance: ScheduleLayerService;
    private listCache: Map<string, Observable<Array<ScheduleLayerData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ScheduleLayerBasicListData>>>;
    private recordCache: Map<string, Observable<ScheduleLayerData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private scheduleLayerChangeHistoryService: ScheduleLayerChangeHistoryService,
        private scheduleLayerMemberService: ScheduleLayerMemberService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ScheduleLayerData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ScheduleLayerBasicListData>>>();
        this.recordCache = new Map<string, Observable<ScheduleLayerData>>();

        ScheduleLayerService._instance = this;
    }

    public static get Instance(): ScheduleLayerService {
      return ScheduleLayerService._instance;
    }


    public ClearListCaches(config: ScheduleLayerQueryParameters | null = null) {

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


    public ConvertToScheduleLayerSubmitData(data: ScheduleLayerData): ScheduleLayerSubmitData {

        let output = new ScheduleLayerSubmitData();

        output.id = data.id;
        output.onCallScheduleId = data.onCallScheduleId;
        output.name = data.name;
        output.description = data.description;
        output.layerLevel = data.layerLevel;
        output.rotationStart = data.rotationStart;
        output.rotationDays = data.rotationDays;
        output.handoffTime = data.handoffTime;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetScheduleLayer(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduleLayerData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const scheduleLayer$ = this.requestScheduleLayer(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleLayer", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, scheduleLayer$);

            return scheduleLayer$;
        }

        return this.recordCache.get(configHash) as Observable<ScheduleLayerData>;
    }

    private requestScheduleLayer(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduleLayerData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduleLayerData>(this.baseUrl + 'api/ScheduleLayer/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveScheduleLayer(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleLayer(id, includeRelations));
            }));
    }

    public GetScheduleLayerList(config: ScheduleLayerQueryParameters | any = null) : Observable<Array<ScheduleLayerData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const scheduleLayerList$ = this.requestScheduleLayerList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleLayer list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, scheduleLayerList$);

            return scheduleLayerList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ScheduleLayerData>>;
    }


    private requestScheduleLayerList(config: ScheduleLayerQueryParameters | any) : Observable <Array<ScheduleLayerData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduleLayerData>>(this.baseUrl + 'api/ScheduleLayers', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveScheduleLayerList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleLayerList(config));
            }));
    }

    public GetScheduleLayersRowCount(config: ScheduleLayerQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const scheduleLayersRowCount$ = this.requestScheduleLayersRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleLayers row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, scheduleLayersRowCount$);

            return scheduleLayersRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestScheduleLayersRowCount(config: ScheduleLayerQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ScheduleLayers/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleLayersRowCount(config));
            }));
    }

    public GetScheduleLayersBasicListData(config: ScheduleLayerQueryParameters | any = null) : Observable<Array<ScheduleLayerBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const scheduleLayersBasicListData$ = this.requestScheduleLayersBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleLayers basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, scheduleLayersBasicListData$);

            return scheduleLayersBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ScheduleLayerBasicListData>>;
    }


    private requestScheduleLayersBasicListData(config: ScheduleLayerQueryParameters | any) : Observable<Array<ScheduleLayerBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduleLayerBasicListData>>(this.baseUrl + 'api/ScheduleLayers/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleLayersBasicListData(config));
            }));

    }


    public PutScheduleLayer(id: bigint | number, scheduleLayer: ScheduleLayerSubmitData) : Observable<ScheduleLayerData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduleLayerData>(this.baseUrl + 'api/ScheduleLayer/' + id.toString(), scheduleLayer, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduleLayer(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutScheduleLayer(id, scheduleLayer));
            }));
    }


    public PostScheduleLayer(scheduleLayer: ScheduleLayerSubmitData) : Observable<ScheduleLayerData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ScheduleLayerData>(this.baseUrl + 'api/ScheduleLayer', scheduleLayer, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduleLayer(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostScheduleLayer(scheduleLayer));
            }));
    }

  
    public DeleteScheduleLayer(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ScheduleLayer/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteScheduleLayer(id));
            }));
    }

    public RollbackScheduleLayer(id: bigint | number, versionNumber: bigint | number) : Observable<ScheduleLayerData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduleLayerData>(this.baseUrl + 'api/ScheduleLayer/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduleLayer(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackScheduleLayer(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ScheduleLayer.
     */
    public GetScheduleLayerChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ScheduleLayerData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduleLayerData>>(this.baseUrl + 'api/ScheduleLayer/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduleLayerChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ScheduleLayer.
     */
    public GetScheduleLayerAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ScheduleLayerData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduleLayerData>[]>(this.baseUrl + 'api/ScheduleLayer/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduleLayerAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ScheduleLayer.
     */
    public GetScheduleLayerVersion(id: bigint | number, version: number): Observable<ScheduleLayerData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduleLayerData>(this.baseUrl + 'api/ScheduleLayer/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduleLayer(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduleLayerVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ScheduleLayer at a specific point in time.
     */
    public GetScheduleLayerStateAtTime(id: bigint | number, time: string): Observable<ScheduleLayerData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduleLayerData>(this.baseUrl + 'api/ScheduleLayer/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduleLayer(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduleLayerStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ScheduleLayerQueryParameters | any): string {

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

    public userIsAlertingScheduleLayerReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingScheduleLayerReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.ScheduleLayers
        //
        if (userIsAlertingScheduleLayerReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingScheduleLayerReader = user.readPermission >= 1;
            } else {
                userIsAlertingScheduleLayerReader = false;
            }
        }

        return userIsAlertingScheduleLayerReader;
    }


    public userIsAlertingScheduleLayerWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingScheduleLayerWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.ScheduleLayers
        //
        if (userIsAlertingScheduleLayerWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingScheduleLayerWriter = user.writePermission >= 100;
          } else {
            userIsAlertingScheduleLayerWriter = false;
          }      
        }

        return userIsAlertingScheduleLayerWriter;
    }

    public GetScheduleLayerChangeHistoriesForScheduleLayer(scheduleLayerId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduleLayerChangeHistoryData[]> {
        return this.scheduleLayerChangeHistoryService.GetScheduleLayerChangeHistoryList({
            scheduleLayerId: scheduleLayerId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduleLayerMembersForScheduleLayer(scheduleLayerId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduleLayerMemberData[]> {
        return this.scheduleLayerMemberService.GetScheduleLayerMemberList({
            scheduleLayerId: scheduleLayerId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ScheduleLayerData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ScheduleLayerData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ScheduleLayerTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveScheduleLayer(raw: any): ScheduleLayerData {
    if (!raw) return raw;

    //
    // Create a ScheduleLayerData object instance with correct prototype
    //
    const revived = Object.create(ScheduleLayerData.prototype) as ScheduleLayerData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._scheduleLayerChangeHistories = null;
    (revived as any)._scheduleLayerChangeHistoriesPromise = null;
    (revived as any)._scheduleLayerChangeHistoriesSubject = new BehaviorSubject<ScheduleLayerChangeHistoryData[] | null>(null);

    (revived as any)._scheduleLayerMembers = null;
    (revived as any)._scheduleLayerMembersPromise = null;
    (revived as any)._scheduleLayerMembersSubject = new BehaviorSubject<ScheduleLayerMemberData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadScheduleLayerXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ScheduleLayerChangeHistories$ = (revived as any)._scheduleLayerChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduleLayerChangeHistories === null && (revived as any)._scheduleLayerChangeHistoriesPromise === null) {
                (revived as any).loadScheduleLayerChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduleLayerChangeHistoriesCount$ = ScheduleLayerChangeHistoryService.Instance.GetScheduleLayerChangeHistoriesRowCount({scheduleLayerId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduleLayerMembers$ = (revived as any)._scheduleLayerMembersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduleLayerMembers === null && (revived as any)._scheduleLayerMembersPromise === null) {
                (revived as any).loadScheduleLayerMembers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduleLayerMembersCount$ = ScheduleLayerMemberService.Instance.GetScheduleLayerMembersRowCount({scheduleLayerId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduleLayerData> | null>(null);

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

  private ReviveScheduleLayerList(rawList: any[]): ScheduleLayerData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveScheduleLayer(raw));
  }

}
