/*

   GENERATED SERVICE FOR THE RESOURCESHIFT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ResourceShift table.

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
import { ResourceData } from './resource.service';
import { TimeZoneData } from './time-zone.service';
import { ResourceShiftChangeHistoryService, ResourceShiftChangeHistoryData } from './resource-shift-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ResourceShiftQueryParameters {
    resourceId: bigint | number | null | undefined = null;
    dayOfWeek: bigint | number | null | undefined = null;
    timeZoneId: bigint | number | null | undefined = null;
    startTime: string | null | undefined = null;        // ISO 8601
    hours: number | null | undefined = null;
    label: string | null | undefined = null;
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
export class ResourceShiftSubmitData {
    id!: bigint | number;
    resourceId!: bigint | number;
    dayOfWeek!: bigint | number;
    timeZoneId: bigint | number | null = null;
    startTime!: string;      // ISO 8601
    hours!: number;
    label: string | null = null;
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

export class ResourceShiftBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ResourceShiftChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `resourceShift.ResourceShiftChildren$` — use with `| async` in templates
//        • Promise:    `resourceShift.ResourceShiftChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="resourceShift.ResourceShiftChildren$ | async"`), or
//        • Access the promise getter (`resourceShift.ResourceShiftChildren` or `await resourceShift.ResourceShiftChildren`)
//    - Simply reading `resourceShift.ResourceShiftChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await resourceShift.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ResourceShiftData {
    id!: bigint | number;
    resourceId!: bigint | number;
    dayOfWeek!: bigint | number;
    timeZoneId!: bigint | number;
    startTime!: string;      // ISO 8601
    hours!: number;
    label!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    timeZone: TimeZoneData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _resourceShiftChangeHistories: ResourceShiftChangeHistoryData[] | null = null;
    private _resourceShiftChangeHistoriesPromise: Promise<ResourceShiftChangeHistoryData[]> | null  = null;
    private _resourceShiftChangeHistoriesSubject = new BehaviorSubject<ResourceShiftChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ResourceShiftData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ResourceShiftData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ResourceShiftData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ResourceShiftChangeHistories$ = this._resourceShiftChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resourceShiftChangeHistories === null && this._resourceShiftChangeHistoriesPromise === null) {
            this.loadResourceShiftChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ResourceShiftChangeHistoriesCount$ = ResourceShiftChangeHistoryService.Instance.GetResourceShiftChangeHistoriesRowCount({resourceShiftId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ResourceShiftData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.resourceShift.Reload();
  //
  //  Non Async:
  //
  //     resourceShift[0].Reload().then(x => {
  //        this.resourceShift = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ResourceShiftService.Instance.GetResourceShift(this.id, includeRelations)
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
     this._resourceShiftChangeHistories = null;
     this._resourceShiftChangeHistoriesPromise = null;
     this._resourceShiftChangeHistoriesSubject.next(null);

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
     * Gets the ResourceShiftChangeHistories for this ResourceShift.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resourceShift.ResourceShiftChangeHistories.then(resourceShifts => { ... })
     *   or
     *   await this.resourceShift.resourceShifts
     *
    */
    public get ResourceShiftChangeHistories(): Promise<ResourceShiftChangeHistoryData[]> {
        if (this._resourceShiftChangeHistories !== null) {
            return Promise.resolve(this._resourceShiftChangeHistories);
        }

        if (this._resourceShiftChangeHistoriesPromise !== null) {
            return this._resourceShiftChangeHistoriesPromise;
        }

        // Start the load
        this.loadResourceShiftChangeHistories();

        return this._resourceShiftChangeHistoriesPromise!;
    }



    private loadResourceShiftChangeHistories(): void {

        this._resourceShiftChangeHistoriesPromise = lastValueFrom(
            ResourceShiftService.Instance.GetResourceShiftChangeHistoriesForResourceShift(this.id)
        )
        .then(ResourceShiftChangeHistories => {
            this._resourceShiftChangeHistories = ResourceShiftChangeHistories ?? [];
            this._resourceShiftChangeHistoriesSubject.next(this._resourceShiftChangeHistories);
            return this._resourceShiftChangeHistories;
         })
        .catch(err => {
            this._resourceShiftChangeHistories = [];
            this._resourceShiftChangeHistoriesSubject.next(this._resourceShiftChangeHistories);
            throw err;
        })
        .finally(() => {
            this._resourceShiftChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ResourceShiftChangeHistory. Call after mutations to force refresh.
     */
    public ClearResourceShiftChangeHistoriesCache(): void {
        this._resourceShiftChangeHistories = null;
        this._resourceShiftChangeHistoriesPromise = null;
        this._resourceShiftChangeHistoriesSubject.next(this._resourceShiftChangeHistories);      // Emit to observable
    }

    public get HasResourceShiftChangeHistories(): Promise<boolean> {
        return this.ResourceShiftChangeHistories.then(resourceShiftChangeHistories => resourceShiftChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (resourceShift.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await resourceShift.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ResourceShiftData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ResourceShiftData>> {
        const info = await lastValueFrom(
            ResourceShiftService.Instance.GetResourceShiftChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ResourceShiftData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ResourceShiftData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ResourceShiftSubmitData {
        return ResourceShiftService.Instance.ConvertToResourceShiftSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ResourceShiftService extends SecureEndpointBase {

    private static _instance: ResourceShiftService;
    private listCache: Map<string, Observable<Array<ResourceShiftData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ResourceShiftBasicListData>>>;
    private recordCache: Map<string, Observable<ResourceShiftData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private resourceShiftChangeHistoryService: ResourceShiftChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ResourceShiftData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ResourceShiftBasicListData>>>();
        this.recordCache = new Map<string, Observable<ResourceShiftData>>();

        ResourceShiftService._instance = this;
    }

    public static get Instance(): ResourceShiftService {
      return ResourceShiftService._instance;
    }


    public ClearListCaches(config: ResourceShiftQueryParameters | null = null) {

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


    public ConvertToResourceShiftSubmitData(data: ResourceShiftData): ResourceShiftSubmitData {

        let output = new ResourceShiftSubmitData();

        output.id = data.id;
        output.resourceId = data.resourceId;
        output.dayOfWeek = data.dayOfWeek;
        output.timeZoneId = data.timeZoneId;
        output.startTime = data.startTime;
        output.hours = data.hours;
        output.label = data.label;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetResourceShift(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceShiftData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const resourceShift$ = this.requestResourceShift(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceShift", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, resourceShift$);

            return resourceShift$;
        }

        return this.recordCache.get(configHash) as Observable<ResourceShiftData>;
    }

    private requestResourceShift(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceShiftData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceShiftData>(this.baseUrl + 'api/ResourceShift/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveResourceShift(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceShift(id, includeRelations));
            }));
    }

    public GetResourceShiftList(config: ResourceShiftQueryParameters | any = null) : Observable<Array<ResourceShiftData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const resourceShiftList$ = this.requestResourceShiftList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceShift list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, resourceShiftList$);

            return resourceShiftList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ResourceShiftData>>;
    }


    private requestResourceShiftList(config: ResourceShiftQueryParameters | any) : Observable <Array<ResourceShiftData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceShiftData>>(this.baseUrl + 'api/ResourceShifts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveResourceShiftList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceShiftList(config));
            }));
    }

    public GetResourceShiftsRowCount(config: ResourceShiftQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const resourceShiftsRowCount$ = this.requestResourceShiftsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceShifts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, resourceShiftsRowCount$);

            return resourceShiftsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestResourceShiftsRowCount(config: ResourceShiftQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ResourceShifts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceShiftsRowCount(config));
            }));
    }

    public GetResourceShiftsBasicListData(config: ResourceShiftQueryParameters | any = null) : Observable<Array<ResourceShiftBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const resourceShiftsBasicListData$ = this.requestResourceShiftsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceShifts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, resourceShiftsBasicListData$);

            return resourceShiftsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ResourceShiftBasicListData>>;
    }


    private requestResourceShiftsBasicListData(config: ResourceShiftQueryParameters | any) : Observable<Array<ResourceShiftBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceShiftBasicListData>>(this.baseUrl + 'api/ResourceShifts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceShiftsBasicListData(config));
            }));

    }


    public PutResourceShift(id: bigint | number, resourceShift: ResourceShiftSubmitData) : Observable<ResourceShiftData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceShiftData>(this.baseUrl + 'api/ResourceShift/' + id.toString(), resourceShift, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceShift(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutResourceShift(id, resourceShift));
            }));
    }


    public PostResourceShift(resourceShift: ResourceShiftSubmitData) : Observable<ResourceShiftData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ResourceShiftData>(this.baseUrl + 'api/ResourceShift', resourceShift, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceShift(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostResourceShift(resourceShift));
            }));
    }

  
    public DeleteResourceShift(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ResourceShift/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteResourceShift(id));
            }));
    }

    public RollbackResourceShift(id: bigint | number, versionNumber: bigint | number) : Observable<ResourceShiftData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceShiftData>(this.baseUrl + 'api/ResourceShift/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceShift(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackResourceShift(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ResourceShift.
     */
    public GetResourceShiftChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ResourceShiftData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ResourceShiftData>>(this.baseUrl + 'api/ResourceShift/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetResourceShiftChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ResourceShift.
     */
    public GetResourceShiftAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ResourceShiftData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ResourceShiftData>[]>(this.baseUrl + 'api/ResourceShift/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetResourceShiftAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ResourceShift.
     */
    public GetResourceShiftVersion(id: bigint | number, version: number): Observable<ResourceShiftData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceShiftData>(this.baseUrl + 'api/ResourceShift/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveResourceShift(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetResourceShiftVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ResourceShift at a specific point in time.
     */
    public GetResourceShiftStateAtTime(id: bigint | number, time: string): Observable<ResourceShiftData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceShiftData>(this.baseUrl + 'api/ResourceShift/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveResourceShift(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetResourceShiftStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ResourceShiftQueryParameters | any): string {

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

    public userIsSchedulerResourceShiftReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerResourceShiftReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ResourceShifts
        //
        if (userIsSchedulerResourceShiftReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerResourceShiftReader = user.readPermission >= 1;
            } else {
                userIsSchedulerResourceShiftReader = false;
            }
        }

        return userIsSchedulerResourceShiftReader;
    }


    public userIsSchedulerResourceShiftWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerResourceShiftWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ResourceShifts
        //
        if (userIsSchedulerResourceShiftWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerResourceShiftWriter = user.writePermission >= 30;
          } else {
            userIsSchedulerResourceShiftWriter = false;
          }      
        }

        return userIsSchedulerResourceShiftWriter;
    }

    public GetResourceShiftChangeHistoriesForResourceShift(resourceShiftId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceShiftChangeHistoryData[]> {
        return this.resourceShiftChangeHistoryService.GetResourceShiftChangeHistoryList({
            resourceShiftId: resourceShiftId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ResourceShiftData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ResourceShiftData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ResourceShiftTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveResourceShift(raw: any): ResourceShiftData {
    if (!raw) return raw;

    //
    // Create a ResourceShiftData object instance with correct prototype
    //
    const revived = Object.create(ResourceShiftData.prototype) as ResourceShiftData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._resourceShiftChangeHistories = null;
    (revived as any)._resourceShiftChangeHistoriesPromise = null;
    (revived as any)._resourceShiftChangeHistoriesSubject = new BehaviorSubject<ResourceShiftChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadResourceShiftXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ResourceShiftChangeHistories$ = (revived as any)._resourceShiftChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resourceShiftChangeHistories === null && (revived as any)._resourceShiftChangeHistoriesPromise === null) {
                (revived as any).loadResourceShiftChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ResourceShiftChangeHistoriesCount$ = ResourceShiftChangeHistoryService.Instance.GetResourceShiftChangeHistoriesRowCount({resourceShiftId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ResourceShiftData> | null>(null);

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

  private ReviveResourceShiftList(rawList: any[]): ResourceShiftData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveResourceShift(raw));
  }

}
