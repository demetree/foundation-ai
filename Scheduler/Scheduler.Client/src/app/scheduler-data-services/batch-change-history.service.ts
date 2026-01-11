/*

   GENERATED SERVICE FOR THE BATCHCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BatchChangeHistory table.

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
import { BatchData } from './batch.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BatchChangeHistoryQueryParameters {
    batchId: bigint | number | null | undefined = null;
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
export class BatchChangeHistorySubmitData {
    id!: bigint | number;
    batchId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
}


export class BatchChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BatchChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `batchChangeHistory.BatchChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `batchChangeHistory.BatchChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="batchChangeHistory.BatchChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`batchChangeHistory.BatchChangeHistoryChildren` or `await batchChangeHistory.BatchChangeHistoryChildren`)
//    - Simply reading `batchChangeHistory.BatchChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await batchChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BatchChangeHistoryData {
    id!: bigint | number;
    batchId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
    batch: BatchData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any BatchChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.batchChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     batchChangeHistory[0].Reload().then(x => {
  //        this.batchChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BatchChangeHistoryService.Instance.GetBatchChangeHistory(this.id, includeRelations)
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
     * Updates the state of this BatchChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BatchChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BatchChangeHistorySubmitData {
        return BatchChangeHistoryService.Instance.ConvertToBatchChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BatchChangeHistoryService extends SecureEndpointBase {

    private static _instance: BatchChangeHistoryService;
    private listCache: Map<string, Observable<Array<BatchChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BatchChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<BatchChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BatchChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BatchChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<BatchChangeHistoryData>>();

        BatchChangeHistoryService._instance = this;
    }

    public static get Instance(): BatchChangeHistoryService {
      return BatchChangeHistoryService._instance;
    }


    public ClearListCaches(config: BatchChangeHistoryQueryParameters | null = null) {

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


    public ConvertToBatchChangeHistorySubmitData(data: BatchChangeHistoryData): BatchChangeHistorySubmitData {

        let output = new BatchChangeHistorySubmitData();

        output.id = data.id;
        output.batchId = data.batchId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetBatchChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<BatchChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const batchChangeHistory$ = this.requestBatchChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BatchChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, batchChangeHistory$);

            return batchChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<BatchChangeHistoryData>;
    }

    private requestBatchChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<BatchChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BatchChangeHistoryData>(this.baseUrl + 'api/BatchChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBatchChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBatchChangeHistory(id, includeRelations));
            }));
    }

    public GetBatchChangeHistoryList(config: BatchChangeHistoryQueryParameters | any = null) : Observable<Array<BatchChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const batchChangeHistoryList$ = this.requestBatchChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BatchChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, batchChangeHistoryList$);

            return batchChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BatchChangeHistoryData>>;
    }


    private requestBatchChangeHistoryList(config: BatchChangeHistoryQueryParameters | any) : Observable <Array<BatchChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BatchChangeHistoryData>>(this.baseUrl + 'api/BatchChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBatchChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBatchChangeHistoryList(config));
            }));
    }

    public GetBatchChangeHistoriesRowCount(config: BatchChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const batchChangeHistoriesRowCount$ = this.requestBatchChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BatchChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, batchChangeHistoriesRowCount$);

            return batchChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBatchChangeHistoriesRowCount(config: BatchChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BatchChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBatchChangeHistoriesRowCount(config));
            }));
    }

    public GetBatchChangeHistoriesBasicListData(config: BatchChangeHistoryQueryParameters | any = null) : Observable<Array<BatchChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const batchChangeHistoriesBasicListData$ = this.requestBatchChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BatchChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, batchChangeHistoriesBasicListData$);

            return batchChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BatchChangeHistoryBasicListData>>;
    }


    private requestBatchChangeHistoriesBasicListData(config: BatchChangeHistoryQueryParameters | any) : Observable<Array<BatchChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BatchChangeHistoryBasicListData>>(this.baseUrl + 'api/BatchChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBatchChangeHistoriesBasicListData(config));
            }));

    }


    public PutBatchChangeHistory(id: bigint | number, batchChangeHistory: BatchChangeHistorySubmitData) : Observable<BatchChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BatchChangeHistoryData>(this.baseUrl + 'api/BatchChangeHistory/' + id.toString(), batchChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBatchChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBatchChangeHistory(id, batchChangeHistory));
            }));
    }


    public PostBatchChangeHistory(batchChangeHistory: BatchChangeHistorySubmitData) : Observable<BatchChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BatchChangeHistoryData>(this.baseUrl + 'api/BatchChangeHistory', batchChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBatchChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBatchChangeHistory(batchChangeHistory));
            }));
    }

  
    public DeleteBatchChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BatchChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBatchChangeHistory(id));
            }));
    }


    private getConfigHash(config: BatchChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerBatchChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerBatchChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.BatchChangeHistories
        //
        if (userIsSchedulerBatchChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerBatchChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerBatchChangeHistoryReader = false;
            }
        }

        return userIsSchedulerBatchChangeHistoryReader;
    }


    public userIsSchedulerBatchChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerBatchChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.BatchChangeHistories
        //
        if (userIsSchedulerBatchChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerBatchChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerBatchChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerBatchChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BatchChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BatchChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BatchChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBatchChangeHistory(raw: any): BatchChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a BatchChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(BatchChangeHistoryData.prototype) as BatchChangeHistoryData;

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
    // 2. But private methods (loadBatchChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBatchChangeHistoryList(rawList: any[]): BatchChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBatchChangeHistory(raw));
  }

}
