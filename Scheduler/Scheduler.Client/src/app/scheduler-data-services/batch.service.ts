/*

   GENERATED SERVICE FOR THE BATCH TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Batch table.

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
import { BatchStatusData } from './batch-status.service';
import { FundData } from './fund.service';
import { CampaignData } from './campaign.service';
import { AppealData } from './appeal.service';
import { BatchChangeHistoryService, BatchChangeHistoryData } from './batch-change-history.service';
import { GiftService, GiftData } from './gift.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BatchQueryParameters {
    batchNumber: string | null | undefined = null;
    description: string | null | undefined = null;
    dateOpened: string | null | undefined = null;        // ISO 8601
    datePosted: string | null | undefined = null;        // ISO 8601
    batchStatusId: bigint | number | null | undefined = null;
    controlAmount: number | null | undefined = null;
    controlCount: bigint | number | null | undefined = null;
    defaultFundId: bigint | number | null | undefined = null;
    defaultCampaignId: bigint | number | null | undefined = null;
    defaultAppealId: bigint | number | null | undefined = null;
    defaultDate: string | null | undefined = null;        // ISO 8601
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
export class BatchSubmitData {
    id!: bigint | number;
    batchNumber!: string;
    description: string | null = null;
    dateOpened!: string;      // ISO 8601
    datePosted: string | null = null;     // ISO 8601
    batchStatusId!: bigint | number;
    controlAmount!: number;
    controlCount!: bigint | number;
    defaultFundId: bigint | number | null = null;
    defaultCampaignId: bigint | number | null = null;
    defaultAppealId: bigint | number | null = null;
    defaultDate: string | null = null;     // ISO 8601
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class BatchBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BatchChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `batch.BatchChildren$` — use with `| async` in templates
//        • Promise:    `batch.BatchChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="batch.BatchChildren$ | async"`), or
//        • Access the promise getter (`batch.BatchChildren` or `await batch.BatchChildren`)
//    - Simply reading `batch.BatchChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await batch.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BatchData {
    id!: bigint | number;
    batchNumber!: string;
    description!: string | null;
    dateOpened!: string;      // ISO 8601
    datePosted!: string | null;   // ISO 8601
    batchStatusId!: bigint | number;
    controlAmount!: number;
    controlCount!: bigint | number;
    defaultFundId!: bigint | number;
    defaultCampaignId!: bigint | number;
    defaultAppealId!: bigint | number;
    defaultDate!: string | null;   // ISO 8601
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    batchStatus: BatchStatusData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    defaultFund: FundData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)
    defaultCampaign: CampaignData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)
    defaultAppeal: AppealData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _batchChangeHistories: BatchChangeHistoryData[] | null = null;
    private _batchChangeHistoriesPromise: Promise<BatchChangeHistoryData[]> | null  = null;
    private _batchChangeHistoriesSubject = new BehaviorSubject<BatchChangeHistoryData[] | null>(null);

    private _gifts: GiftData[] | null = null;
    private _giftsPromise: Promise<GiftData[]> | null  = null;
    private _giftsSubject = new BehaviorSubject<GiftData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public BatchChangeHistories$ = this._batchChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._batchChangeHistories === null && this._batchChangeHistoriesPromise === null) {
            this.loadBatchChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public BatchChangeHistoriesCount$ = BatchService.Instance.GetBatchesRowCount({batchId: this.id,
      active: true,
      deleted: false
    });



    public Gifts$ = this._giftsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._gifts === null && this._giftsPromise === null) {
            this.loadGifts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public GiftsCount$ = BatchService.Instance.GetBatchesRowCount({batchId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any BatchData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.batch.Reload();
  //
  //  Non Async:
  //
  //     batch[0].Reload().then(x => {
  //        this.batch = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BatchService.Instance.GetBatch(this.id, includeRelations)
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
     this._batchChangeHistories = null;
     this._batchChangeHistoriesPromise = null;
     this._batchChangeHistoriesSubject.next(null);

     this._gifts = null;
     this._giftsPromise = null;
     this._giftsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the BatchChangeHistories for this Batch.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.batch.BatchChangeHistories.then(batchChangeHistories => { ... })
     *   or
     *   await this.batch.BatchChangeHistories
     *
    */
    public get BatchChangeHistories(): Promise<BatchChangeHistoryData[]> {
        if (this._batchChangeHistories !== null) {
            return Promise.resolve(this._batchChangeHistories);
        }

        if (this._batchChangeHistoriesPromise !== null) {
            return this._batchChangeHistoriesPromise;
        }

        // Start the load
        this.loadBatchChangeHistories();

        return this._batchChangeHistoriesPromise!;
    }



    private loadBatchChangeHistories(): void {

        this._batchChangeHistoriesPromise = lastValueFrom(
            BatchService.Instance.GetBatchChangeHistoriesForBatch(this.id)
        )
        .then(batchChangeHistories => {
            this._batchChangeHistories = batchChangeHistories ?? [];
            this._batchChangeHistoriesSubject.next(this._batchChangeHistories);
            return this._batchChangeHistories;
         })
        .catch(err => {
            this._batchChangeHistories = [];
            this._batchChangeHistoriesSubject.next(this._batchChangeHistories);
            throw err;
        })
        .finally(() => {
            this._batchChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BatchChangeHistory. Call after mutations to force refresh.
     */
    public ClearBatchChangeHistoriesCache(): void {
        this._batchChangeHistories = null;
        this._batchChangeHistoriesPromise = null;
        this._batchChangeHistoriesSubject.next(this._batchChangeHistories);      // Emit to observable
    }

    public get HasBatchChangeHistories(): Promise<boolean> {
        return this.BatchChangeHistories.then(batchChangeHistories => batchChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Gifts for this Batch.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.batch.Gifts.then(gifts => { ... })
     *   or
     *   await this.batch.Gifts
     *
    */
    public get Gifts(): Promise<GiftData[]> {
        if (this._gifts !== null) {
            return Promise.resolve(this._gifts);
        }

        if (this._giftsPromise !== null) {
            return this._giftsPromise;
        }

        // Start the load
        this.loadGifts();

        return this._giftsPromise!;
    }



    private loadGifts(): void {

        this._giftsPromise = lastValueFrom(
            BatchService.Instance.GetGiftsForBatch(this.id)
        )
        .then(gifts => {
            this._gifts = gifts ?? [];
            this._giftsSubject.next(this._gifts);
            return this._gifts;
         })
        .catch(err => {
            this._gifts = [];
            this._giftsSubject.next(this._gifts);
            throw err;
        })
        .finally(() => {
            this._giftsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Gift. Call after mutations to force refresh.
     */
    public ClearGiftsCache(): void {
        this._gifts = null;
        this._giftsPromise = null;
        this._giftsSubject.next(this._gifts);      // Emit to observable
    }

    public get HasGifts(): Promise<boolean> {
        return this.Gifts.then(gifts => gifts.length > 0);
    }




    /**
     * Updates the state of this BatchData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BatchData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BatchSubmitData {
        return BatchService.Instance.ConvertToBatchSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BatchService extends SecureEndpointBase {

    private static _instance: BatchService;
    private listCache: Map<string, Observable<Array<BatchData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BatchBasicListData>>>;
    private recordCache: Map<string, Observable<BatchData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private batchChangeHistoryService: BatchChangeHistoryService,
        private giftService: GiftService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BatchData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BatchBasicListData>>>();
        this.recordCache = new Map<string, Observable<BatchData>>();

        BatchService._instance = this;
    }

    public static get Instance(): BatchService {
      return BatchService._instance;
    }


    public ClearListCaches(config: BatchQueryParameters | null = null) {

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


    public ConvertToBatchSubmitData(data: BatchData): BatchSubmitData {

        let output = new BatchSubmitData();

        output.id = data.id;
        output.batchNumber = data.batchNumber;
        output.description = data.description;
        output.dateOpened = data.dateOpened;
        output.datePosted = data.datePosted;
        output.batchStatusId = data.batchStatusId;
        output.controlAmount = data.controlAmount;
        output.controlCount = data.controlCount;
        output.defaultFundId = data.defaultFundId;
        output.defaultCampaignId = data.defaultCampaignId;
        output.defaultAppealId = data.defaultAppealId;
        output.defaultDate = data.defaultDate;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBatch(id: bigint | number, includeRelations: boolean = true) : Observable<BatchData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const batch$ = this.requestBatch(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Batch", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, batch$);

            return batch$;
        }

        return this.recordCache.get(configHash) as Observable<BatchData>;
    }

    private requestBatch(id: bigint | number, includeRelations: boolean = true) : Observable<BatchData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BatchData>(this.baseUrl + 'api/Batch/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBatch(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBatch(id, includeRelations));
            }));
    }

    public GetBatchList(config: BatchQueryParameters | any = null) : Observable<Array<BatchData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const batchList$ = this.requestBatchList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Batch list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, batchList$);

            return batchList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BatchData>>;
    }


    private requestBatchList(config: BatchQueryParameters | any) : Observable <Array<BatchData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BatchData>>(this.baseUrl + 'api/Batches', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBatchList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBatchList(config));
            }));
    }

    public GetBatchesRowCount(config: BatchQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const batchesRowCount$ = this.requestBatchesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Batches row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, batchesRowCount$);

            return batchesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBatchesRowCount(config: BatchQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Batches/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBatchesRowCount(config));
            }));
    }

    public GetBatchesBasicListData(config: BatchQueryParameters | any = null) : Observable<Array<BatchBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const batchesBasicListData$ = this.requestBatchesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Batches basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, batchesBasicListData$);

            return batchesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BatchBasicListData>>;
    }


    private requestBatchesBasicListData(config: BatchQueryParameters | any) : Observable<Array<BatchBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BatchBasicListData>>(this.baseUrl + 'api/Batches/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBatchesBasicListData(config));
            }));

    }


    public PutBatch(id: bigint | number, batch: BatchSubmitData) : Observable<BatchData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BatchData>(this.baseUrl + 'api/Batch/' + id.toString(), batch, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBatch(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBatch(id, batch));
            }));
    }


    public PostBatch(batch: BatchSubmitData) : Observable<BatchData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BatchData>(this.baseUrl + 'api/Batch', batch, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBatch(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBatch(batch));
            }));
    }

  
    public DeleteBatch(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Batch/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBatch(id));
            }));
    }

    public RollbackBatch(id: bigint | number, versionNumber: bigint | number) : Observable<BatchData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BatchData>(this.baseUrl + 'api/Batch/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBatch(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackBatch(id, versionNumber));
        }));
    }

    private getConfigHash(config: BatchQueryParameters | any): string {

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

    public userIsSchedulerBatchReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerBatchReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Batches
        //
        if (userIsSchedulerBatchReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerBatchReader = user.readPermission >= 1;
            } else {
                userIsSchedulerBatchReader = false;
            }
        }

        return userIsSchedulerBatchReader;
    }


    public userIsSchedulerBatchWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerBatchWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Batches
        //
        if (userIsSchedulerBatchWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerBatchWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerBatchWriter = false;
          }      
        }

        return userIsSchedulerBatchWriter;
    }

    public GetBatchChangeHistoriesForBatch(batchId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BatchChangeHistoryData[]> {
        return this.batchChangeHistoryService.GetBatchChangeHistoryList({
            batchId: batchId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetGiftsForBatch(batchId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GiftData[]> {
        return this.giftService.GetGiftList({
            batchId: batchId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full BatchData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BatchData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BatchTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBatch(raw: any): BatchData {
    if (!raw) return raw;

    //
    // Create a BatchData object instance with correct prototype
    //
    const revived = Object.create(BatchData.prototype) as BatchData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._batchChangeHistories = null;
    (revived as any)._batchChangeHistoriesPromise = null;
    (revived as any)._batchChangeHistoriesSubject = new BehaviorSubject<BatchChangeHistoryData[] | null>(null);

    (revived as any)._gifts = null;
    (revived as any)._giftsPromise = null;
    (revived as any)._giftsSubject = new BehaviorSubject<GiftData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadBatchXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).BatchChangeHistories$ = (revived as any)._batchChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._batchChangeHistories === null && (revived as any)._batchChangeHistoriesPromise === null) {
                (revived as any).loadBatchChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).BatchChangeHistoriesCount$ = BatchChangeHistoryService.Instance.GetBatchChangeHistoriesRowCount({batchId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Gifts$ = (revived as any)._giftsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._gifts === null && (revived as any)._giftsPromise === null) {
                (revived as any).loadGifts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).GiftsCount$ = GiftService.Instance.GetGiftsRowCount({batchId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveBatchList(rawList: any[]): BatchData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBatch(raw));
  }

}
