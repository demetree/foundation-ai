/*

   GENERATED SERVICE FOR THE BATCHSTATUS TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BatchStatus table.

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
import { BatchService, BatchData } from './batch.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BatchStatusQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class BatchStatusSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BatchStatusBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BatchStatusChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `batchStatus.BatchStatusChildren$` — use with `| async` in templates
//        • Promise:    `batchStatus.BatchStatusChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="batchStatus.BatchStatusChildren$ | async"`), or
//        • Access the promise getter (`batchStatus.BatchStatusChildren` or `await batchStatus.BatchStatusChildren`)
//    - Simply reading `batchStatus.BatchStatusChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await batchStatus.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BatchStatusData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _batches: BatchData[] | null = null;
    private _batchesPromise: Promise<BatchData[]> | null  = null;
    private _batchesSubject = new BehaviorSubject<BatchData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Batches$ = this._batchesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._batches === null && this._batchesPromise === null) {
            this.loadBatches(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _batchesCount$: Observable<bigint | number> | null = null;
    public get BatchesCount$(): Observable<bigint | number> {
        if (this._batchesCount$ === null) {
            this._batchesCount$ = BatchService.Instance.GetBatchesRowCount({batchStatusId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._batchesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any BatchStatusData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.batchStatus.Reload();
  //
  //  Non Async:
  //
  //     batchStatus[0].Reload().then(x => {
  //        this.batchStatus = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BatchStatusService.Instance.GetBatchStatus(this.id, includeRelations)
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
     this._batches = null;
     this._batchesPromise = null;
     this._batchesSubject.next(null);
     this._batchesCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Batches for this BatchStatus.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.batchStatus.Batches.then(batchStatuses => { ... })
     *   or
     *   await this.batchStatus.batchStatuses
     *
    */
    public get Batches(): Promise<BatchData[]> {
        if (this._batches !== null) {
            return Promise.resolve(this._batches);
        }

        if (this._batchesPromise !== null) {
            return this._batchesPromise;
        }

        // Start the load
        this.loadBatches();

        return this._batchesPromise!;
    }



    private loadBatches(): void {

        this._batchesPromise = lastValueFrom(
            BatchStatusService.Instance.GetBatchesForBatchStatus(this.id)
        )
        .then(Batches => {
            this._batches = Batches ?? [];
            this._batchesSubject.next(this._batches);
            return this._batches;
         })
        .catch(err => {
            this._batches = [];
            this._batchesSubject.next(this._batches);
            throw err;
        })
        .finally(() => {
            this._batchesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Batch. Call after mutations to force refresh.
     */
    public ClearBatchesCache(): void {
        this._batches = null;
        this._batchesPromise = null;
        this._batchesSubject.next(this._batches);      // Emit to observable
    }

    public get HasBatches(): Promise<boolean> {
        return this.Batches.then(batches => batches.length > 0);
    }




    /**
     * Updates the state of this BatchStatusData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BatchStatusData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BatchStatusSubmitData {
        return BatchStatusService.Instance.ConvertToBatchStatusSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BatchStatusService extends SecureEndpointBase {

    private static _instance: BatchStatusService;
    private listCache: Map<string, Observable<Array<BatchStatusData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BatchStatusBasicListData>>>;
    private recordCache: Map<string, Observable<BatchStatusData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private batchService: BatchService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BatchStatusData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BatchStatusBasicListData>>>();
        this.recordCache = new Map<string, Observable<BatchStatusData>>();

        BatchStatusService._instance = this;
    }

    public static get Instance(): BatchStatusService {
      return BatchStatusService._instance;
    }


    public ClearListCaches(config: BatchStatusQueryParameters | null = null) {

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


    public ConvertToBatchStatusSubmitData(data: BatchStatusData): BatchStatusSubmitData {

        let output = new BatchStatusSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBatchStatus(id: bigint | number, includeRelations: boolean = true) : Observable<BatchStatusData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const batchStatus$ = this.requestBatchStatus(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BatchStatus", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, batchStatus$);

            return batchStatus$;
        }

        return this.recordCache.get(configHash) as Observable<BatchStatusData>;
    }

    private requestBatchStatus(id: bigint | number, includeRelations: boolean = true) : Observable<BatchStatusData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BatchStatusData>(this.baseUrl + 'api/BatchStatus/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBatchStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBatchStatus(id, includeRelations));
            }));
    }

    public GetBatchStatusList(config: BatchStatusQueryParameters | any = null) : Observable<Array<BatchStatusData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const batchStatusList$ = this.requestBatchStatusList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BatchStatus list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, batchStatusList$);

            return batchStatusList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BatchStatusData>>;
    }


    private requestBatchStatusList(config: BatchStatusQueryParameters | any) : Observable <Array<BatchStatusData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BatchStatusData>>(this.baseUrl + 'api/BatchStatuses', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBatchStatusList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBatchStatusList(config));
            }));
    }

    public GetBatchStatusesRowCount(config: BatchStatusQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const batchStatusesRowCount$ = this.requestBatchStatusesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BatchStatuses row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, batchStatusesRowCount$);

            return batchStatusesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBatchStatusesRowCount(config: BatchStatusQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BatchStatuses/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBatchStatusesRowCount(config));
            }));
    }

    public GetBatchStatusesBasicListData(config: BatchStatusQueryParameters | any = null) : Observable<Array<BatchStatusBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const batchStatusesBasicListData$ = this.requestBatchStatusesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BatchStatuses basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, batchStatusesBasicListData$);

            return batchStatusesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BatchStatusBasicListData>>;
    }


    private requestBatchStatusesBasicListData(config: BatchStatusQueryParameters | any) : Observable<Array<BatchStatusBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BatchStatusBasicListData>>(this.baseUrl + 'api/BatchStatuses/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBatchStatusesBasicListData(config));
            }));

    }


    public PutBatchStatus(id: bigint | number, batchStatus: BatchStatusSubmitData) : Observable<BatchStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BatchStatusData>(this.baseUrl + 'api/BatchStatus/' + id.toString(), batchStatus, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBatchStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBatchStatus(id, batchStatus));
            }));
    }


    public PostBatchStatus(batchStatus: BatchStatusSubmitData) : Observable<BatchStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BatchStatusData>(this.baseUrl + 'api/BatchStatus', batchStatus, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBatchStatus(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBatchStatus(batchStatus));
            }));
    }

  
    public DeleteBatchStatus(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BatchStatus/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBatchStatus(id));
            }));
    }


    private getConfigHash(config: BatchStatusQueryParameters | any): string {

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

    public userIsSchedulerBatchStatusReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerBatchStatusReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.BatchStatuses
        //
        if (userIsSchedulerBatchStatusReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerBatchStatusReader = user.readPermission >= 1;
            } else {
                userIsSchedulerBatchStatusReader = false;
            }
        }

        return userIsSchedulerBatchStatusReader;
    }


    public userIsSchedulerBatchStatusWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerBatchStatusWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.BatchStatuses
        //
        if (userIsSchedulerBatchStatusWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerBatchStatusWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerBatchStatusWriter = false;
          }      
        }

        return userIsSchedulerBatchStatusWriter;
    }

    public GetBatchesForBatchStatus(batchStatusId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BatchData[]> {
        return this.batchService.GetBatchList({
            batchStatusId: batchStatusId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full BatchStatusData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BatchStatusData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BatchStatusTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBatchStatus(raw: any): BatchStatusData {
    if (!raw) return raw;

    //
    // Create a BatchStatusData object instance with correct prototype
    //
    const revived = Object.create(BatchStatusData.prototype) as BatchStatusData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._batches = null;
    (revived as any)._batchesPromise = null;
    (revived as any)._batchesSubject = new BehaviorSubject<BatchData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadBatchStatusXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Batches$ = (revived as any)._batchesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._batches === null && (revived as any)._batchesPromise === null) {
                (revived as any).loadBatches();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._batchesCount$ = null;



    return revived;
  }

  private ReviveBatchStatusList(rawList: any[]): BatchStatusData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBatchStatus(raw));
  }

}
