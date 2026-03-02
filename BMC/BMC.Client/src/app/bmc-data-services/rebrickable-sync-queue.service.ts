/*

   GENERATED SERVICE FOR THE REBRICKABLESYNCQUEUE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the RebrickableSyncQueue table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class RebrickableSyncQueueQueryParameters {
    operationType: string | null | undefined = null;
    entityType: string | null | undefined = null;
    entityId: bigint | number | null | undefined = null;
    payload: string | null | undefined = null;
    status: string | null | undefined = null;
    createdDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    lastAttemptDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    completedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    attemptCount: bigint | number | null | undefined = null;
    maxAttempts: bigint | number | null | undefined = null;
    errorMessage: string | null | undefined = null;
    responseBody: string | null | undefined = null;
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
export class RebrickableSyncQueueSubmitData {
    id!: bigint | number;
    operationType!: string;
    entityType!: string;
    entityId!: bigint | number;
    payload: string | null = null;
    status!: string;
    createdDate: string | null = null;     // ISO 8601 (full datetime)
    lastAttemptDate: string | null = null;     // ISO 8601 (full datetime)
    completedDate: string | null = null;     // ISO 8601 (full datetime)
    attemptCount!: bigint | number;
    maxAttempts!: bigint | number;
    errorMessage: string | null = null;
    responseBody: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class RebrickableSyncQueueBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. RebrickableSyncQueueChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `rebrickableSyncQueue.RebrickableSyncQueueChildren$` — use with `| async` in templates
//        • Promise:    `rebrickableSyncQueue.RebrickableSyncQueueChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="rebrickableSyncQueue.RebrickableSyncQueueChildren$ | async"`), or
//        • Access the promise getter (`rebrickableSyncQueue.RebrickableSyncQueueChildren` or `await rebrickableSyncQueue.RebrickableSyncQueueChildren`)
//    - Simply reading `rebrickableSyncQueue.RebrickableSyncQueueChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await rebrickableSyncQueue.Reload()` to refresh the entire object and clear all lazy caches.
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
export class RebrickableSyncQueueData {
    id!: bigint | number;
    operationType!: string;
    entityType!: string;
    entityId!: bigint | number;
    payload!: string | null;
    status!: string;
    createdDate!: string | null;   // ISO 8601 (full datetime)
    lastAttemptDate!: string | null;   // ISO 8601 (full datetime)
    completedDate!: string | null;   // ISO 8601 (full datetime)
    attemptCount!: bigint | number;
    maxAttempts!: bigint | number;
    errorMessage!: string | null;
    responseBody!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

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
  // Promise based reload method to allow rebuilding of any RebrickableSyncQueueData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.rebrickableSyncQueue.Reload();
  //
  //  Non Async:
  //
  //     rebrickableSyncQueue[0].Reload().then(x => {
  //        this.rebrickableSyncQueue = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      RebrickableSyncQueueService.Instance.GetRebrickableSyncQueue(this.id, includeRelations)
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
     * Updates the state of this RebrickableSyncQueueData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this RebrickableSyncQueueData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): RebrickableSyncQueueSubmitData {
        return RebrickableSyncQueueService.Instance.ConvertToRebrickableSyncQueueSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class RebrickableSyncQueueService extends SecureEndpointBase {

    private static _instance: RebrickableSyncQueueService;
    private listCache: Map<string, Observable<Array<RebrickableSyncQueueData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<RebrickableSyncQueueBasicListData>>>;
    private recordCache: Map<string, Observable<RebrickableSyncQueueData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<RebrickableSyncQueueData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<RebrickableSyncQueueBasicListData>>>();
        this.recordCache = new Map<string, Observable<RebrickableSyncQueueData>>();

        RebrickableSyncQueueService._instance = this;
    }

    public static get Instance(): RebrickableSyncQueueService {
      return RebrickableSyncQueueService._instance;
    }


    public ClearListCaches(config: RebrickableSyncQueueQueryParameters | null = null) {

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


    public ConvertToRebrickableSyncQueueSubmitData(data: RebrickableSyncQueueData): RebrickableSyncQueueSubmitData {

        let output = new RebrickableSyncQueueSubmitData();

        output.id = data.id;
        output.operationType = data.operationType;
        output.entityType = data.entityType;
        output.entityId = data.entityId;
        output.payload = data.payload;
        output.status = data.status;
        output.createdDate = data.createdDate;
        output.lastAttemptDate = data.lastAttemptDate;
        output.completedDate = data.completedDate;
        output.attemptCount = data.attemptCount;
        output.maxAttempts = data.maxAttempts;
        output.errorMessage = data.errorMessage;
        output.responseBody = data.responseBody;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetRebrickableSyncQueue(id: bigint | number, includeRelations: boolean = true) : Observable<RebrickableSyncQueueData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const rebrickableSyncQueue$ = this.requestRebrickableSyncQueue(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RebrickableSyncQueue", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, rebrickableSyncQueue$);

            return rebrickableSyncQueue$;
        }

        return this.recordCache.get(configHash) as Observable<RebrickableSyncQueueData>;
    }

    private requestRebrickableSyncQueue(id: bigint | number, includeRelations: boolean = true) : Observable<RebrickableSyncQueueData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RebrickableSyncQueueData>(this.baseUrl + 'api/RebrickableSyncQueue/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveRebrickableSyncQueue(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestRebrickableSyncQueue(id, includeRelations));
            }));
    }

    public GetRebrickableSyncQueueList(config: RebrickableSyncQueueQueryParameters | any = null) : Observable<Array<RebrickableSyncQueueData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const rebrickableSyncQueueList$ = this.requestRebrickableSyncQueueList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RebrickableSyncQueue list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, rebrickableSyncQueueList$);

            return rebrickableSyncQueueList$;
        }

        return this.listCache.get(configHash) as Observable<Array<RebrickableSyncQueueData>>;
    }


    private requestRebrickableSyncQueueList(config: RebrickableSyncQueueQueryParameters | any) : Observable <Array<RebrickableSyncQueueData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RebrickableSyncQueueData>>(this.baseUrl + 'api/RebrickableSyncQueues', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveRebrickableSyncQueueList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestRebrickableSyncQueueList(config));
            }));
    }

    public GetRebrickableSyncQueuesRowCount(config: RebrickableSyncQueueQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const rebrickableSyncQueuesRowCount$ = this.requestRebrickableSyncQueuesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RebrickableSyncQueues row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, rebrickableSyncQueuesRowCount$);

            return rebrickableSyncQueuesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestRebrickableSyncQueuesRowCount(config: RebrickableSyncQueueQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/RebrickableSyncQueues/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRebrickableSyncQueuesRowCount(config));
            }));
    }

    public GetRebrickableSyncQueuesBasicListData(config: RebrickableSyncQueueQueryParameters | any = null) : Observable<Array<RebrickableSyncQueueBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const rebrickableSyncQueuesBasicListData$ = this.requestRebrickableSyncQueuesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RebrickableSyncQueues basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, rebrickableSyncQueuesBasicListData$);

            return rebrickableSyncQueuesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<RebrickableSyncQueueBasicListData>>;
    }


    private requestRebrickableSyncQueuesBasicListData(config: RebrickableSyncQueueQueryParameters | any) : Observable<Array<RebrickableSyncQueueBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RebrickableSyncQueueBasicListData>>(this.baseUrl + 'api/RebrickableSyncQueues/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRebrickableSyncQueuesBasicListData(config));
            }));

    }


    public PutRebrickableSyncQueue(id: bigint | number, rebrickableSyncQueue: RebrickableSyncQueueSubmitData) : Observable<RebrickableSyncQueueData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RebrickableSyncQueueData>(this.baseUrl + 'api/RebrickableSyncQueue/' + id.toString(), rebrickableSyncQueue, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRebrickableSyncQueue(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutRebrickableSyncQueue(id, rebrickableSyncQueue));
            }));
    }


    public PostRebrickableSyncQueue(rebrickableSyncQueue: RebrickableSyncQueueSubmitData) : Observable<RebrickableSyncQueueData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<RebrickableSyncQueueData>(this.baseUrl + 'api/RebrickableSyncQueue', rebrickableSyncQueue, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRebrickableSyncQueue(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostRebrickableSyncQueue(rebrickableSyncQueue));
            }));
    }

  
    public DeleteRebrickableSyncQueue(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/RebrickableSyncQueue/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteRebrickableSyncQueue(id));
            }));
    }


    private getConfigHash(config: RebrickableSyncQueueQueryParameters | any): string {

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

    public userIsBMCRebrickableSyncQueueReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCRebrickableSyncQueueReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.RebrickableSyncQueues
        //
        if (userIsBMCRebrickableSyncQueueReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCRebrickableSyncQueueReader = user.readPermission >= 1;
            } else {
                userIsBMCRebrickableSyncQueueReader = false;
            }
        }

        return userIsBMCRebrickableSyncQueueReader;
    }


    public userIsBMCRebrickableSyncQueueWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCRebrickableSyncQueueWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.RebrickableSyncQueues
        //
        if (userIsBMCRebrickableSyncQueueWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCRebrickableSyncQueueWriter = user.writePermission >= 255;
          } else {
            userIsBMCRebrickableSyncQueueWriter = false;
          }      
        }

        return userIsBMCRebrickableSyncQueueWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full RebrickableSyncQueueData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the RebrickableSyncQueueData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when RebrickableSyncQueueTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveRebrickableSyncQueue(raw: any): RebrickableSyncQueueData {
    if (!raw) return raw;

    //
    // Create a RebrickableSyncQueueData object instance with correct prototype
    //
    const revived = Object.create(RebrickableSyncQueueData.prototype) as RebrickableSyncQueueData;

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
    // 2. But private methods (loadRebrickableSyncQueueXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveRebrickableSyncQueueList(rawList: any[]): RebrickableSyncQueueData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveRebrickableSyncQueue(raw));
  }

}
