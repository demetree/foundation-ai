/*

   GENERATED SERVICE FOR THE RESOURCEAVAILABILITY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ResourceAvailability table.

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
import { ResourceAvailabilityChangeHistoryService, ResourceAvailabilityChangeHistoryData } from './resource-availability-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ResourceAvailabilityQueryParameters {
    resourceId: bigint | number | null | undefined = null;
    timeZoneId: bigint | number | null | undefined = null;
    startDateTime: string | null | undefined = null;        // ISO 8601
    endDateTime: string | null | undefined = null;        // ISO 8601
    reason: string | null | undefined = null;
    notes: string | null | undefined = null;
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
export class ResourceAvailabilitySubmitData {
    id!: bigint | number;
    resourceId!: bigint | number;
    timeZoneId: bigint | number | null = null;
    startDateTime!: string;      // ISO 8601
    endDateTime: string | null = null;     // ISO 8601
    reason: string | null = null;
    notes: string | null = null;
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

export class ResourceAvailabilityBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ResourceAvailabilityChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `resourceAvailability.ResourceAvailabilityChildren$` — use with `| async` in templates
//        • Promise:    `resourceAvailability.ResourceAvailabilityChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="resourceAvailability.ResourceAvailabilityChildren$ | async"`), or
//        • Access the promise getter (`resourceAvailability.ResourceAvailabilityChildren` or `await resourceAvailability.ResourceAvailabilityChildren`)
//    - Simply reading `resourceAvailability.ResourceAvailabilityChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await resourceAvailability.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ResourceAvailabilityData {
    id!: bigint | number;
    resourceId!: bigint | number;
    timeZoneId!: bigint | number;
    startDateTime!: string;      // ISO 8601
    endDateTime!: string | null;   // ISO 8601
    reason!: string | null;
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    timeZone: TimeZoneData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _resourceAvailabilityChangeHistories: ResourceAvailabilityChangeHistoryData[] | null = null;
    private _resourceAvailabilityChangeHistoriesPromise: Promise<ResourceAvailabilityChangeHistoryData[]> | null  = null;
    private _resourceAvailabilityChangeHistoriesSubject = new BehaviorSubject<ResourceAvailabilityChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ResourceAvailabilityData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ResourceAvailabilityData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ResourceAvailabilityData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ResourceAvailabilityChangeHistories$ = this._resourceAvailabilityChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resourceAvailabilityChangeHistories === null && this._resourceAvailabilityChangeHistoriesPromise === null) {
            this.loadResourceAvailabilityChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ResourceAvailabilityChangeHistoriesCount$ = ResourceAvailabilityChangeHistoryService.Instance.GetResourceAvailabilityChangeHistoriesRowCount({resourceAvailabilityId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ResourceAvailabilityData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.resourceAvailability.Reload();
  //
  //  Non Async:
  //
  //     resourceAvailability[0].Reload().then(x => {
  //        this.resourceAvailability = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ResourceAvailabilityService.Instance.GetResourceAvailability(this.id, includeRelations)
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
     this._resourceAvailabilityChangeHistories = null;
     this._resourceAvailabilityChangeHistoriesPromise = null;
     this._resourceAvailabilityChangeHistoriesSubject.next(null);

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
     * Gets the ResourceAvailabilityChangeHistories for this ResourceAvailability.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resourceAvailability.ResourceAvailabilityChangeHistories.then(resourceAvailabilities => { ... })
     *   or
     *   await this.resourceAvailability.resourceAvailabilities
     *
    */
    public get ResourceAvailabilityChangeHistories(): Promise<ResourceAvailabilityChangeHistoryData[]> {
        if (this._resourceAvailabilityChangeHistories !== null) {
            return Promise.resolve(this._resourceAvailabilityChangeHistories);
        }

        if (this._resourceAvailabilityChangeHistoriesPromise !== null) {
            return this._resourceAvailabilityChangeHistoriesPromise;
        }

        // Start the load
        this.loadResourceAvailabilityChangeHistories();

        return this._resourceAvailabilityChangeHistoriesPromise!;
    }



    private loadResourceAvailabilityChangeHistories(): void {

        this._resourceAvailabilityChangeHistoriesPromise = lastValueFrom(
            ResourceAvailabilityService.Instance.GetResourceAvailabilityChangeHistoriesForResourceAvailability(this.id)
        )
        .then(ResourceAvailabilityChangeHistories => {
            this._resourceAvailabilityChangeHistories = ResourceAvailabilityChangeHistories ?? [];
            this._resourceAvailabilityChangeHistoriesSubject.next(this._resourceAvailabilityChangeHistories);
            return this._resourceAvailabilityChangeHistories;
         })
        .catch(err => {
            this._resourceAvailabilityChangeHistories = [];
            this._resourceAvailabilityChangeHistoriesSubject.next(this._resourceAvailabilityChangeHistories);
            throw err;
        })
        .finally(() => {
            this._resourceAvailabilityChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ResourceAvailabilityChangeHistory. Call after mutations to force refresh.
     */
    public ClearResourceAvailabilityChangeHistoriesCache(): void {
        this._resourceAvailabilityChangeHistories = null;
        this._resourceAvailabilityChangeHistoriesPromise = null;
        this._resourceAvailabilityChangeHistoriesSubject.next(this._resourceAvailabilityChangeHistories);      // Emit to observable
    }

    public get HasResourceAvailabilityChangeHistories(): Promise<boolean> {
        return this.ResourceAvailabilityChangeHistories.then(resourceAvailabilityChangeHistories => resourceAvailabilityChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (resourceAvailability.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await resourceAvailability.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ResourceAvailabilityData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ResourceAvailabilityData>> {
        const info = await lastValueFrom(
            ResourceAvailabilityService.Instance.GetResourceAvailabilityChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ResourceAvailabilityData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ResourceAvailabilityData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ResourceAvailabilitySubmitData {
        return ResourceAvailabilityService.Instance.ConvertToResourceAvailabilitySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ResourceAvailabilityService extends SecureEndpointBase {

    private static _instance: ResourceAvailabilityService;
    private listCache: Map<string, Observable<Array<ResourceAvailabilityData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ResourceAvailabilityBasicListData>>>;
    private recordCache: Map<string, Observable<ResourceAvailabilityData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private resourceAvailabilityChangeHistoryService: ResourceAvailabilityChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ResourceAvailabilityData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ResourceAvailabilityBasicListData>>>();
        this.recordCache = new Map<string, Observable<ResourceAvailabilityData>>();

        ResourceAvailabilityService._instance = this;
    }

    public static get Instance(): ResourceAvailabilityService {
      return ResourceAvailabilityService._instance;
    }


    public ClearListCaches(config: ResourceAvailabilityQueryParameters | null = null) {

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


    public ConvertToResourceAvailabilitySubmitData(data: ResourceAvailabilityData): ResourceAvailabilitySubmitData {

        let output = new ResourceAvailabilitySubmitData();

        output.id = data.id;
        output.resourceId = data.resourceId;
        output.timeZoneId = data.timeZoneId;
        output.startDateTime = data.startDateTime;
        output.endDateTime = data.endDateTime;
        output.reason = data.reason;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetResourceAvailability(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceAvailabilityData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const resourceAvailability$ = this.requestResourceAvailability(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceAvailability", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, resourceAvailability$);

            return resourceAvailability$;
        }

        return this.recordCache.get(configHash) as Observable<ResourceAvailabilityData>;
    }

    private requestResourceAvailability(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceAvailabilityData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceAvailabilityData>(this.baseUrl + 'api/ResourceAvailability/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveResourceAvailability(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceAvailability(id, includeRelations));
            }));
    }

    public GetResourceAvailabilityList(config: ResourceAvailabilityQueryParameters | any = null) : Observable<Array<ResourceAvailabilityData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const resourceAvailabilityList$ = this.requestResourceAvailabilityList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceAvailability list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, resourceAvailabilityList$);

            return resourceAvailabilityList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ResourceAvailabilityData>>;
    }


    private requestResourceAvailabilityList(config: ResourceAvailabilityQueryParameters | any) : Observable <Array<ResourceAvailabilityData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceAvailabilityData>>(this.baseUrl + 'api/ResourceAvailabilities', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveResourceAvailabilityList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceAvailabilityList(config));
            }));
    }

    public GetResourceAvailabilitiesRowCount(config: ResourceAvailabilityQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const resourceAvailabilitiesRowCount$ = this.requestResourceAvailabilitiesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceAvailabilities row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, resourceAvailabilitiesRowCount$);

            return resourceAvailabilitiesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestResourceAvailabilitiesRowCount(config: ResourceAvailabilityQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ResourceAvailabilities/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceAvailabilitiesRowCount(config));
            }));
    }

    public GetResourceAvailabilitiesBasicListData(config: ResourceAvailabilityQueryParameters | any = null) : Observable<Array<ResourceAvailabilityBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const resourceAvailabilitiesBasicListData$ = this.requestResourceAvailabilitiesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceAvailabilities basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, resourceAvailabilitiesBasicListData$);

            return resourceAvailabilitiesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ResourceAvailabilityBasicListData>>;
    }


    private requestResourceAvailabilitiesBasicListData(config: ResourceAvailabilityQueryParameters | any) : Observable<Array<ResourceAvailabilityBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceAvailabilityBasicListData>>(this.baseUrl + 'api/ResourceAvailabilities/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceAvailabilitiesBasicListData(config));
            }));

    }


    public PutResourceAvailability(id: bigint | number, resourceAvailability: ResourceAvailabilitySubmitData) : Observable<ResourceAvailabilityData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceAvailabilityData>(this.baseUrl + 'api/ResourceAvailability/' + id.toString(), resourceAvailability, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceAvailability(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutResourceAvailability(id, resourceAvailability));
            }));
    }


    public PostResourceAvailability(resourceAvailability: ResourceAvailabilitySubmitData) : Observable<ResourceAvailabilityData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ResourceAvailabilityData>(this.baseUrl + 'api/ResourceAvailability', resourceAvailability, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceAvailability(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostResourceAvailability(resourceAvailability));
            }));
    }

  
    public DeleteResourceAvailability(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ResourceAvailability/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteResourceAvailability(id));
            }));
    }

    public RollbackResourceAvailability(id: bigint | number, versionNumber: bigint | number) : Observable<ResourceAvailabilityData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceAvailabilityData>(this.baseUrl + 'api/ResourceAvailability/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceAvailability(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackResourceAvailability(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ResourceAvailability.
     */
    public GetResourceAvailabilityChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ResourceAvailabilityData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ResourceAvailabilityData>>(this.baseUrl + 'api/ResourceAvailability/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetResourceAvailabilityChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ResourceAvailability.
     */
    public GetResourceAvailabilityAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ResourceAvailabilityData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ResourceAvailabilityData>[]>(this.baseUrl + 'api/ResourceAvailability/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetResourceAvailabilityAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ResourceAvailability.
     */
    public GetResourceAvailabilityVersion(id: bigint | number, version: number): Observable<ResourceAvailabilityData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceAvailabilityData>(this.baseUrl + 'api/ResourceAvailability/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveResourceAvailability(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetResourceAvailabilityVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ResourceAvailability at a specific point in time.
     */
    public GetResourceAvailabilityStateAtTime(id: bigint | number, time: string): Observable<ResourceAvailabilityData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceAvailabilityData>(this.baseUrl + 'api/ResourceAvailability/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveResourceAvailability(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetResourceAvailabilityStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ResourceAvailabilityQueryParameters | any): string {

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

    public userIsSchedulerResourceAvailabilityReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerResourceAvailabilityReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ResourceAvailabilities
        //
        if (userIsSchedulerResourceAvailabilityReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerResourceAvailabilityReader = user.readPermission >= 1;
            } else {
                userIsSchedulerResourceAvailabilityReader = false;
            }
        }

        return userIsSchedulerResourceAvailabilityReader;
    }


    public userIsSchedulerResourceAvailabilityWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerResourceAvailabilityWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ResourceAvailabilities
        //
        if (userIsSchedulerResourceAvailabilityWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerResourceAvailabilityWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerResourceAvailabilityWriter = false;
          }      
        }

        return userIsSchedulerResourceAvailabilityWriter;
    }

    public GetResourceAvailabilityChangeHistoriesForResourceAvailability(resourceAvailabilityId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceAvailabilityChangeHistoryData[]> {
        return this.resourceAvailabilityChangeHistoryService.GetResourceAvailabilityChangeHistoryList({
            resourceAvailabilityId: resourceAvailabilityId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ResourceAvailabilityData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ResourceAvailabilityData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ResourceAvailabilityTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveResourceAvailability(raw: any): ResourceAvailabilityData {
    if (!raw) return raw;

    //
    // Create a ResourceAvailabilityData object instance with correct prototype
    //
    const revived = Object.create(ResourceAvailabilityData.prototype) as ResourceAvailabilityData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._resourceAvailabilityChangeHistories = null;
    (revived as any)._resourceAvailabilityChangeHistoriesPromise = null;
    (revived as any)._resourceAvailabilityChangeHistoriesSubject = new BehaviorSubject<ResourceAvailabilityChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadResourceAvailabilityXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ResourceAvailabilityChangeHistories$ = (revived as any)._resourceAvailabilityChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resourceAvailabilityChangeHistories === null && (revived as any)._resourceAvailabilityChangeHistoriesPromise === null) {
                (revived as any).loadResourceAvailabilityChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ResourceAvailabilityChangeHistoriesCount$ = ResourceAvailabilityChangeHistoryService.Instance.GetResourceAvailabilityChangeHistoriesRowCount({resourceAvailabilityId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ResourceAvailabilityData> | null>(null);

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

  private ReviveResourceAvailabilityList(rawList: any[]): ResourceAvailabilityData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveResourceAvailability(raw));
  }

}
