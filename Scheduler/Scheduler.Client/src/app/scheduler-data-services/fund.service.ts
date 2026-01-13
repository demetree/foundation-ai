/*

   GENERATED SERVICE FOR THE FUND TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Fund table.

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
import { IconData } from './icon.service';
import { FundChangeHistoryService, FundChangeHistoryData } from './fund-change-history.service';
import { PledgeService, PledgeData } from './pledge.service';
import { BatchService, BatchData } from './batch.service';
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
export class FundQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    glCode: string | null | undefined = null;
    isRestricted: boolean | null | undefined = null;
    goalAmount: number | null | undefined = null;
    notes: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class FundSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    glCode: string | null = null;
    isRestricted!: boolean;
    goalAmount: number | null = null;
    notes: string | null = null;
    sequence: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class FundBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. FundChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `fund.FundChildren$` — use with `| async` in templates
//        • Promise:    `fund.FundChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="fund.FundChildren$ | async"`), or
//        • Access the promise getter (`fund.FundChildren` or `await fund.FundChildren`)
//    - Simply reading `fund.FundChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await fund.Reload()` to refresh the entire object and clear all lazy caches.
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
export class FundData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    glCode!: string | null;
    isRestricted!: boolean;
    goalAmount!: number | null;
    notes!: string | null;
    sequence!: bigint | number;
    iconId!: bigint | number;
    color!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _fundChangeHistories: FundChangeHistoryData[] | null = null;
    private _fundChangeHistoriesPromise: Promise<FundChangeHistoryData[]> | null  = null;
    private _fundChangeHistoriesSubject = new BehaviorSubject<FundChangeHistoryData[] | null>(null);

                
    private _pledges: PledgeData[] | null = null;
    private _pledgesPromise: Promise<PledgeData[]> | null  = null;
    private _pledgesSubject = new BehaviorSubject<PledgeData[] | null>(null);

                
    private _batchDefaultFunds: BatchData[] | null = null;
    private _batchDefaultFundsPromise: Promise<BatchData[]> | null  = null;
    private _batchDefaultFundsSubject = new BehaviorSubject<BatchData[] | null>(null);
                    
    private _gifts: GiftData[] | null = null;
    private _giftsPromise: Promise<GiftData[]> | null  = null;
    private _giftsSubject = new BehaviorSubject<GiftData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public FundChangeHistories$ = this._fundChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._fundChangeHistories === null && this._fundChangeHistoriesPromise === null) {
            this.loadFundChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public FundChangeHistoriesCount$ = FundChangeHistoryService.Instance.GetFundChangeHistoriesRowCount({fundId: this.id,
      active: true,
      deleted: false
    });



    public Pledges$ = this._pledgesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._pledges === null && this._pledgesPromise === null) {
            this.loadPledges(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public PledgesCount$ = PledgeService.Instance.GetPledgesRowCount({fundId: this.id,
      active: true,
      deleted: false
    });



    public BatchDefaultFunds$ = this._batchDefaultFundsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._batchDefaultFunds === null && this._batchDefaultFundsPromise === null) {
            this.loadBatchDefaultFunds(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public BatchDefaultFundsCount$ = BatchService.Instance.GetBatchesRowCount({defaultFundId: this.id,
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

  
    public GiftsCount$ = GiftService.Instance.GetGiftsRowCount({fundId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any FundData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.fund.Reload();
  //
  //  Non Async:
  //
  //     fund[0].Reload().then(x => {
  //        this.fund = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      FundService.Instance.GetFund(this.id, includeRelations)
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
     this._fundChangeHistories = null;
     this._fundChangeHistoriesPromise = null;
     this._fundChangeHistoriesSubject.next(null);

     this._pledges = null;
     this._pledgesPromise = null;
     this._pledgesSubject.next(null);

     this._batchDefaultFunds = null;
     this._batchDefaultFundsPromise = null;
     this._batchDefaultFundsSubject.next(null);

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
     * Gets the FundChangeHistories for this Fund.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.fund.FundChangeHistories.then(funds => { ... })
     *   or
     *   await this.fund.funds
     *
    */
    public get FundChangeHistories(): Promise<FundChangeHistoryData[]> {
        if (this._fundChangeHistories !== null) {
            return Promise.resolve(this._fundChangeHistories);
        }

        if (this._fundChangeHistoriesPromise !== null) {
            return this._fundChangeHistoriesPromise;
        }

        // Start the load
        this.loadFundChangeHistories();

        return this._fundChangeHistoriesPromise!;
    }



    private loadFundChangeHistories(): void {

        this._fundChangeHistoriesPromise = lastValueFrom(
            FundService.Instance.GetFundChangeHistoriesForFund(this.id)
        )
        .then(FundChangeHistories => {
            this._fundChangeHistories = FundChangeHistories ?? [];
            this._fundChangeHistoriesSubject.next(this._fundChangeHistories);
            return this._fundChangeHistories;
         })
        .catch(err => {
            this._fundChangeHistories = [];
            this._fundChangeHistoriesSubject.next(this._fundChangeHistories);
            throw err;
        })
        .finally(() => {
            this._fundChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached FundChangeHistory. Call after mutations to force refresh.
     */
    public ClearFundChangeHistoriesCache(): void {
        this._fundChangeHistories = null;
        this._fundChangeHistoriesPromise = null;
        this._fundChangeHistoriesSubject.next(this._fundChangeHistories);      // Emit to observable
    }

    public get HasFundChangeHistories(): Promise<boolean> {
        return this.FundChangeHistories.then(fundChangeHistories => fundChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Pledges for this Fund.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.fund.Pledges.then(funds => { ... })
     *   or
     *   await this.fund.funds
     *
    */
    public get Pledges(): Promise<PledgeData[]> {
        if (this._pledges !== null) {
            return Promise.resolve(this._pledges);
        }

        if (this._pledgesPromise !== null) {
            return this._pledgesPromise;
        }

        // Start the load
        this.loadPledges();

        return this._pledgesPromise!;
    }



    private loadPledges(): void {

        this._pledgesPromise = lastValueFrom(
            FundService.Instance.GetPledgesForFund(this.id)
        )
        .then(Pledges => {
            this._pledges = Pledges ?? [];
            this._pledgesSubject.next(this._pledges);
            return this._pledges;
         })
        .catch(err => {
            this._pledges = [];
            this._pledgesSubject.next(this._pledges);
            throw err;
        })
        .finally(() => {
            this._pledgesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Pledge. Call after mutations to force refresh.
     */
    public ClearPledgesCache(): void {
        this._pledges = null;
        this._pledgesPromise = null;
        this._pledgesSubject.next(this._pledges);      // Emit to observable
    }

    public get HasPledges(): Promise<boolean> {
        return this.Pledges.then(pledges => pledges.length > 0);
    }


    /**
     *
     * Gets the BatchDefaultFunds for this Fund.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.fund.BatchDefaultFunds.then(defaultFunds => { ... })
     *   or
     *   await this.fund.defaultFunds
     *
    */
    public get BatchDefaultFunds(): Promise<BatchData[]> {
        if (this._batchDefaultFunds !== null) {
            return Promise.resolve(this._batchDefaultFunds);
        }

        if (this._batchDefaultFundsPromise !== null) {
            return this._batchDefaultFundsPromise;
        }

        // Start the load
        this.loadBatchDefaultFunds();

        return this._batchDefaultFundsPromise!;
    }



    private loadBatchDefaultFunds(): void {

        this._batchDefaultFundsPromise = lastValueFrom(
            FundService.Instance.GetBatchDefaultFundsForFund(this.id)
        )
        .then(BatchDefaultFunds => {
            this._batchDefaultFunds = BatchDefaultFunds ?? [];
            this._batchDefaultFundsSubject.next(this._batchDefaultFunds);
            return this._batchDefaultFunds;
         })
        .catch(err => {
            this._batchDefaultFunds = [];
            this._batchDefaultFundsSubject.next(this._batchDefaultFunds);
            throw err;
        })
        .finally(() => {
            this._batchDefaultFundsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BatchDefaultFund. Call after mutations to force refresh.
     */
    public ClearBatchDefaultFundsCache(): void {
        this._batchDefaultFunds = null;
        this._batchDefaultFundsPromise = null;
        this._batchDefaultFundsSubject.next(this._batchDefaultFunds);      // Emit to observable
    }

    public get HasBatchDefaultFunds(): Promise<boolean> {
        return this.BatchDefaultFunds.then(batchDefaultFunds => batchDefaultFunds.length > 0);
    }


    /**
     *
     * Gets the Gifts for this Fund.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.fund.Gifts.then(funds => { ... })
     *   or
     *   await this.fund.funds
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
            FundService.Instance.GetGiftsForFund(this.id)
        )
        .then(Gifts => {
            this._gifts = Gifts ?? [];
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
     * Updates the state of this FundData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this FundData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): FundSubmitData {
        return FundService.Instance.ConvertToFundSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class FundService extends SecureEndpointBase {

    private static _instance: FundService;
    private listCache: Map<string, Observable<Array<FundData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<FundBasicListData>>>;
    private recordCache: Map<string, Observable<FundData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private fundChangeHistoryService: FundChangeHistoryService,
        private pledgeService: PledgeService,
        private batchService: BatchService,
        private giftService: GiftService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<FundData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<FundBasicListData>>>();
        this.recordCache = new Map<string, Observable<FundData>>();

        FundService._instance = this;
    }

    public static get Instance(): FundService {
      return FundService._instance;
    }


    public ClearListCaches(config: FundQueryParameters | null = null) {

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


    public ConvertToFundSubmitData(data: FundData): FundSubmitData {

        let output = new FundSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.glCode = data.glCode;
        output.isRestricted = data.isRestricted;
        output.goalAmount = data.goalAmount;
        output.notes = data.notes;
        output.sequence = data.sequence;
        output.iconId = data.iconId;
        output.color = data.color;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetFund(id: bigint | number, includeRelations: boolean = true) : Observable<FundData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const fund$ = this.requestFund(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Fund", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, fund$);

            return fund$;
        }

        return this.recordCache.get(configHash) as Observable<FundData>;
    }

    private requestFund(id: bigint | number, includeRelations: boolean = true) : Observable<FundData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FundData>(this.baseUrl + 'api/Fund/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveFund(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestFund(id, includeRelations));
            }));
    }

    public GetFundList(config: FundQueryParameters | any = null) : Observable<Array<FundData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const fundList$ = this.requestFundList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Fund list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, fundList$);

            return fundList$;
        }

        return this.listCache.get(configHash) as Observable<Array<FundData>>;
    }


    private requestFundList(config: FundQueryParameters | any) : Observable <Array<FundData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<FundData>>(this.baseUrl + 'api/Funds', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveFundList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestFundList(config));
            }));
    }

    public GetFundsRowCount(config: FundQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const fundsRowCount$ = this.requestFundsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Funds row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, fundsRowCount$);

            return fundsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestFundsRowCount(config: FundQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Funds/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestFundsRowCount(config));
            }));
    }

    public GetFundsBasicListData(config: FundQueryParameters | any = null) : Observable<Array<FundBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const fundsBasicListData$ = this.requestFundsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Funds basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, fundsBasicListData$);

            return fundsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<FundBasicListData>>;
    }


    private requestFundsBasicListData(config: FundQueryParameters | any) : Observable<Array<FundBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<FundBasicListData>>(this.baseUrl + 'api/Funds/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestFundsBasicListData(config));
            }));

    }


    public PutFund(id: bigint | number, fund: FundSubmitData) : Observable<FundData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<FundData>(this.baseUrl + 'api/Fund/' + id.toString(), fund, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFund(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutFund(id, fund));
            }));
    }


    public PostFund(fund: FundSubmitData) : Observable<FundData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<FundData>(this.baseUrl + 'api/Fund', fund, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFund(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostFund(fund));
            }));
    }

  
    public DeleteFund(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Fund/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteFund(id));
            }));
    }

    public RollbackFund(id: bigint | number, versionNumber: bigint | number) : Observable<FundData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<FundData>(this.baseUrl + 'api/Fund/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFund(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackFund(id, versionNumber));
        }));
    }

    private getConfigHash(config: FundQueryParameters | any): string {

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

    public userIsSchedulerFundReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerFundReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Funds
        //
        if (userIsSchedulerFundReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerFundReader = user.readPermission >= 1;
            } else {
                userIsSchedulerFundReader = false;
            }
        }

        return userIsSchedulerFundReader;
    }


    public userIsSchedulerFundWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerFundWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Funds
        //
        if (userIsSchedulerFundWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerFundWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerFundWriter = false;
          }      
        }

        return userIsSchedulerFundWriter;
    }

    public GetFundChangeHistoriesForFund(fundId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FundChangeHistoryData[]> {
        return this.fundChangeHistoryService.GetFundChangeHistoryList({
            fundId: fundId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPledgesForFund(fundId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PledgeData[]> {
        return this.pledgeService.GetPledgeList({
            fundId: fundId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBatchDefaultFundsForFund(fundId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BatchData[]> {
        return this.batchService.GetBatchList({
            defaultFundId: fundId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetGiftsForFund(fundId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GiftData[]> {
        return this.giftService.GetGiftList({
            fundId: fundId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full FundData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the FundData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when FundTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveFund(raw: any): FundData {
    if (!raw) return raw;

    //
    // Create a FundData object instance with correct prototype
    //
    const revived = Object.create(FundData.prototype) as FundData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._fundChangeHistories = null;
    (revived as any)._fundChangeHistoriesPromise = null;
    (revived as any)._fundChangeHistoriesSubject = new BehaviorSubject<FundChangeHistoryData[] | null>(null);

    (revived as any)._pledges = null;
    (revived as any)._pledgesPromise = null;
    (revived as any)._pledgesSubject = new BehaviorSubject<PledgeData[] | null>(null);

    (revived as any)._batches = null;
    (revived as any)._batchesPromise = null;
    (revived as any)._batchesSubject = new BehaviorSubject<BatchData[] | null>(null);

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
    // 2. But private methods (loadFundXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).FundChangeHistories$ = (revived as any)._fundChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._fundChangeHistories === null && (revived as any)._fundChangeHistoriesPromise === null) {
                (revived as any).loadFundChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).FundChangeHistoriesCount$ = FundChangeHistoryService.Instance.GetFundChangeHistoriesRowCount({fundId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Pledges$ = (revived as any)._pledgesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._pledges === null && (revived as any)._pledgesPromise === null) {
                (revived as any).loadPledges();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).PledgesCount$ = PledgeService.Instance.GetPledgesRowCount({fundId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Batches$ = (revived as any)._batchesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._batches === null && (revived as any)._batchesPromise === null) {
                (revived as any).loadBatches();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).BatchesCount$ = BatchService.Instance.GetBatchesRowCount({fundId: (revived as any).id,
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

    (revived as any).GiftsCount$ = GiftService.Instance.GetGiftsRowCount({fundId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveFundList(rawList: any[]): FundData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveFund(raw));
  }

}
