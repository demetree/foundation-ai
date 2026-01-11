/*

   GENERATED SERVICE FOR THE RESOURCECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ResourceChangeHistory table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ResourceChangeHistoryQueryParameters {
    resourceId: bigint | number | null | undefined = null;
    versionNumber: bigint | number | null | undefined = null;
    timeStamp: string | null | undefined = null;        // ISO 8601
    userId: bigint | number | null | undefined = null;
    data: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class ResourceChangeHistorySubmitData {
    id!: bigint | number;
    resourceId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
}


export class ResourceChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ResourceChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `resourceChangeHistory.ResourceChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `resourceChangeHistory.ResourceChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="resourceChangeHistory.ResourceChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`resourceChangeHistory.ResourceChangeHistoryChildren` or `await resourceChangeHistory.ResourceChangeHistoryChildren`)
//    - Simply reading `resourceChangeHistory.ResourceChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await resourceChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ResourceChangeHistoryData {
    id!: bigint | number;
    resourceId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //

  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ResourceChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.resourceChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     resourceChangeHistory[0].Reload().then(x => {
  //        this.resourceChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ResourceChangeHistoryService.Instance.GetResourceChangeHistory(this.id, includeRelations)
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
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //


    /**
     * Updates the state of this ResourceChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ResourceChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ResourceChangeHistorySubmitData {
        return ResourceChangeHistoryService.Instance.ConvertToResourceChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ResourceChangeHistoryService extends SecureEndpointBase {

    private static _instance: ResourceChangeHistoryService;
    private listCache: Map<string, Observable<Array<ResourceChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ResourceChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<ResourceChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ResourceChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ResourceChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<ResourceChangeHistoryData>>();

        ResourceChangeHistoryService._instance = this;
    }

    public static get Instance(): ResourceChangeHistoryService {
      return ResourceChangeHistoryService._instance;
    }


    public ClearListCaches(config: ResourceChangeHistoryQueryParameters | null = null) {

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


    public ConvertToResourceChangeHistorySubmitData(data: ResourceChangeHistoryData): ResourceChangeHistorySubmitData {

        let output = new ResourceChangeHistorySubmitData();

        output.id = data.id;
        output.resourceId = data.resourceId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetResourceChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const resourceChangeHistory$ = this.requestResourceChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, resourceChangeHistory$);

            return resourceChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<ResourceChangeHistoryData>;
    }

    private requestResourceChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceChangeHistoryData>(this.baseUrl + 'api/ResourceChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveResourceChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceChangeHistory(id, includeRelations));
            }));
    }

    public GetResourceChangeHistoryList(config: ResourceChangeHistoryQueryParameters | any = null) : Observable<Array<ResourceChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const resourceChangeHistoryList$ = this.requestResourceChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, resourceChangeHistoryList$);

            return resourceChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ResourceChangeHistoryData>>;
    }


    private requestResourceChangeHistoryList(config: ResourceChangeHistoryQueryParameters | any) : Observable <Array<ResourceChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceChangeHistoryData>>(this.baseUrl + 'api/ResourceChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveResourceChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceChangeHistoryList(config));
            }));
    }

    public GetResourceChangeHistoriesRowCount(config: ResourceChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const resourceChangeHistoriesRowCount$ = this.requestResourceChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, resourceChangeHistoriesRowCount$);

            return resourceChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestResourceChangeHistoriesRowCount(config: ResourceChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ResourceChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceChangeHistoriesRowCount(config));
            }));
    }

    public GetResourceChangeHistoriesBasicListData(config: ResourceChangeHistoryQueryParameters | any = null) : Observable<Array<ResourceChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const resourceChangeHistoriesBasicListData$ = this.requestResourceChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, resourceChangeHistoriesBasicListData$);

            return resourceChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ResourceChangeHistoryBasicListData>>;
    }


    private requestResourceChangeHistoriesBasicListData(config: ResourceChangeHistoryQueryParameters | any) : Observable<Array<ResourceChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceChangeHistoryBasicListData>>(this.baseUrl + 'api/ResourceChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceChangeHistoriesBasicListData(config));
            }));

    }


    public PutResourceChangeHistory(id: bigint | number, resourceChangeHistory: ResourceChangeHistorySubmitData) : Observable<ResourceChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceChangeHistoryData>(this.baseUrl + 'api/ResourceChangeHistory/' + id.toString(), resourceChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutResourceChangeHistory(id, resourceChangeHistory));
            }));
    }


    public PostResourceChangeHistory(resourceChangeHistory: ResourceChangeHistorySubmitData) : Observable<ResourceChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ResourceChangeHistoryData>(this.baseUrl + 'api/ResourceChangeHistory', resourceChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostResourceChangeHistory(resourceChangeHistory));
            }));
    }

  
    public DeleteResourceChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ResourceChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteResourceChangeHistory(id));
            }));
    }


    private getConfigHash(config: ResourceChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerResourceChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerResourceChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ResourceChangeHistories
        //
        if (userIsSchedulerResourceChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerResourceChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerResourceChangeHistoryReader = false;
            }
        }

        return userIsSchedulerResourceChangeHistoryReader;
    }


    public userIsSchedulerResourceChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerResourceChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ResourceChangeHistories
        //
        if (userIsSchedulerResourceChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerResourceChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerResourceChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerResourceChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ResourceChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ResourceChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ResourceChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveResourceChangeHistory(raw: any): ResourceChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a ResourceChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(ResourceChangeHistoryData.prototype) as ResourceChangeHistoryData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //

    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadResourceChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveResourceChangeHistoryList(rawList: any[]): ResourceChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveResourceChangeHistory(raw));
  }

}
