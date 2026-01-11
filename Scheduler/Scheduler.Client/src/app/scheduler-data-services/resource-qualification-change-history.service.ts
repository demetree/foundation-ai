/*

   GENERATED SERVICE FOR THE RESOURCEQUALIFICATIONCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ResourceQualificationChangeHistory table.

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
import { ResourceQualificationData } from './resource-qualification.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ResourceQualificationChangeHistoryQueryParameters {
    resourceQualificationId: bigint | number | null | undefined = null;
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
export class ResourceQualificationChangeHistorySubmitData {
    id!: bigint | number;
    resourceQualificationId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
}


export class ResourceQualificationChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ResourceQualificationChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `resourceQualificationChangeHistory.ResourceQualificationChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `resourceQualificationChangeHistory.ResourceQualificationChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="resourceQualificationChangeHistory.ResourceQualificationChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`resourceQualificationChangeHistory.ResourceQualificationChangeHistoryChildren` or `await resourceQualificationChangeHistory.ResourceQualificationChangeHistoryChildren`)
//    - Simply reading `resourceQualificationChangeHistory.ResourceQualificationChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await resourceQualificationChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ResourceQualificationChangeHistoryData {
    id!: bigint | number;
    resourceQualificationId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
    resourceQualification: ResourceQualificationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ResourceQualificationChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.resourceQualificationChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     resourceQualificationChangeHistory[0].Reload().then(x => {
  //        this.resourceQualificationChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ResourceQualificationChangeHistoryService.Instance.GetResourceQualificationChangeHistory(this.id, includeRelations)
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
     * Updates the state of this ResourceQualificationChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ResourceQualificationChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ResourceQualificationChangeHistorySubmitData {
        return ResourceQualificationChangeHistoryService.Instance.ConvertToResourceQualificationChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ResourceQualificationChangeHistoryService extends SecureEndpointBase {

    private static _instance: ResourceQualificationChangeHistoryService;
    private listCache: Map<string, Observable<Array<ResourceQualificationChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ResourceQualificationChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<ResourceQualificationChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ResourceQualificationChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ResourceQualificationChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<ResourceQualificationChangeHistoryData>>();

        ResourceQualificationChangeHistoryService._instance = this;
    }

    public static get Instance(): ResourceQualificationChangeHistoryService {
      return ResourceQualificationChangeHistoryService._instance;
    }


    public ClearListCaches(config: ResourceQualificationChangeHistoryQueryParameters | null = null) {

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


    public ConvertToResourceQualificationChangeHistorySubmitData(data: ResourceQualificationChangeHistoryData): ResourceQualificationChangeHistorySubmitData {

        let output = new ResourceQualificationChangeHistorySubmitData();

        output.id = data.id;
        output.resourceQualificationId = data.resourceQualificationId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetResourceQualificationChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceQualificationChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const resourceQualificationChangeHistory$ = this.requestResourceQualificationChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceQualificationChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, resourceQualificationChangeHistory$);

            return resourceQualificationChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<ResourceQualificationChangeHistoryData>;
    }

    private requestResourceQualificationChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceQualificationChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceQualificationChangeHistoryData>(this.baseUrl + 'api/ResourceQualificationChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveResourceQualificationChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceQualificationChangeHistory(id, includeRelations));
            }));
    }

    public GetResourceQualificationChangeHistoryList(config: ResourceQualificationChangeHistoryQueryParameters | any = null) : Observable<Array<ResourceQualificationChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const resourceQualificationChangeHistoryList$ = this.requestResourceQualificationChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceQualificationChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, resourceQualificationChangeHistoryList$);

            return resourceQualificationChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ResourceQualificationChangeHistoryData>>;
    }


    private requestResourceQualificationChangeHistoryList(config: ResourceQualificationChangeHistoryQueryParameters | any) : Observable <Array<ResourceQualificationChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceQualificationChangeHistoryData>>(this.baseUrl + 'api/ResourceQualificationChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveResourceQualificationChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceQualificationChangeHistoryList(config));
            }));
    }

    public GetResourceQualificationChangeHistoriesRowCount(config: ResourceQualificationChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const resourceQualificationChangeHistoriesRowCount$ = this.requestResourceQualificationChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceQualificationChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, resourceQualificationChangeHistoriesRowCount$);

            return resourceQualificationChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestResourceQualificationChangeHistoriesRowCount(config: ResourceQualificationChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ResourceQualificationChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceQualificationChangeHistoriesRowCount(config));
            }));
    }

    public GetResourceQualificationChangeHistoriesBasicListData(config: ResourceQualificationChangeHistoryQueryParameters | any = null) : Observable<Array<ResourceQualificationChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const resourceQualificationChangeHistoriesBasicListData$ = this.requestResourceQualificationChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceQualificationChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, resourceQualificationChangeHistoriesBasicListData$);

            return resourceQualificationChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ResourceQualificationChangeHistoryBasicListData>>;
    }


    private requestResourceQualificationChangeHistoriesBasicListData(config: ResourceQualificationChangeHistoryQueryParameters | any) : Observable<Array<ResourceQualificationChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceQualificationChangeHistoryBasicListData>>(this.baseUrl + 'api/ResourceQualificationChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceQualificationChangeHistoriesBasicListData(config));
            }));

    }


    public PutResourceQualificationChangeHistory(id: bigint | number, resourceQualificationChangeHistory: ResourceQualificationChangeHistorySubmitData) : Observable<ResourceQualificationChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceQualificationChangeHistoryData>(this.baseUrl + 'api/ResourceQualificationChangeHistory/' + id.toString(), resourceQualificationChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceQualificationChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutResourceQualificationChangeHistory(id, resourceQualificationChangeHistory));
            }));
    }


    public PostResourceQualificationChangeHistory(resourceQualificationChangeHistory: ResourceQualificationChangeHistorySubmitData) : Observable<ResourceQualificationChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ResourceQualificationChangeHistoryData>(this.baseUrl + 'api/ResourceQualificationChangeHistory', resourceQualificationChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceQualificationChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostResourceQualificationChangeHistory(resourceQualificationChangeHistory));
            }));
    }

  
    public DeleteResourceQualificationChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ResourceQualificationChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteResourceQualificationChangeHistory(id));
            }));
    }


    private getConfigHash(config: ResourceQualificationChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerResourceQualificationChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerResourceQualificationChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ResourceQualificationChangeHistories
        //
        if (userIsSchedulerResourceQualificationChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerResourceQualificationChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerResourceQualificationChangeHistoryReader = false;
            }
        }

        return userIsSchedulerResourceQualificationChangeHistoryReader;
    }


    public userIsSchedulerResourceQualificationChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerResourceQualificationChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ResourceQualificationChangeHistories
        //
        if (userIsSchedulerResourceQualificationChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerResourceQualificationChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerResourceQualificationChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerResourceQualificationChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ResourceQualificationChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ResourceQualificationChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ResourceQualificationChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveResourceQualificationChangeHistory(raw: any): ResourceQualificationChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a ResourceQualificationChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(ResourceQualificationChangeHistoryData.prototype) as ResourceQualificationChangeHistoryData;

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
    // 2. But private methods (loadResourceQualificationChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveResourceQualificationChangeHistoryList(rawList: any[]): ResourceQualificationChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveResourceQualificationChangeHistory(raw));
  }

}
