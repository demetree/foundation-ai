/*

   GENERATED SERVICE FOR THE FISCALPERIOD TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the FiscalPeriod table.

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
import { PeriodStatusData } from './period-status.service';
import { FiscalPeriodChangeHistoryService, FiscalPeriodChangeHistoryData } from './fiscal-period-change-history.service';
import { FinancialTransactionService, FinancialTransactionData } from './financial-transaction.service';
import { BudgetService, BudgetData } from './budget.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class FiscalPeriodQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    startDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    endDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    periodType: string | null | undefined = null;
    fiscalYear: bigint | number | null | undefined = null;
    periodNumber: bigint | number | null | undefined = null;
    periodStatusId: bigint | number | null | undefined = null;
    closedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    closedBy: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class FiscalPeriodSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    startDate!: string;      // ISO 8601 (full datetime)
    endDate!: string;      // ISO 8601 (full datetime)
    periodType!: string;
    fiscalYear!: bigint | number;
    periodNumber!: bigint | number;
    periodStatusId!: bigint | number;
    closedDate: string | null = null;     // ISO 8601 (full datetime)
    closedBy: string | null = null;
    sequence: bigint | number | null = null;
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

export class FiscalPeriodBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. FiscalPeriodChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `fiscalPeriod.FiscalPeriodChildren$` — use with `| async` in templates
//        • Promise:    `fiscalPeriod.FiscalPeriodChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="fiscalPeriod.FiscalPeriodChildren$ | async"`), or
//        • Access the promise getter (`fiscalPeriod.FiscalPeriodChildren` or `await fiscalPeriod.FiscalPeriodChildren`)
//    - Simply reading `fiscalPeriod.FiscalPeriodChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await fiscalPeriod.Reload()` to refresh the entire object and clear all lazy caches.
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
export class FiscalPeriodData {
    id!: bigint | number;
    name!: string;
    description!: string;
    startDate!: string;      // ISO 8601 (full datetime)
    endDate!: string;      // ISO 8601 (full datetime)
    periodType!: string;
    fiscalYear!: bigint | number;
    periodNumber!: bigint | number;
    periodStatusId!: bigint | number;
    closedDate!: string | null;   // ISO 8601 (full datetime)
    closedBy!: string | null;
    sequence!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    periodStatus: PeriodStatusData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _fiscalPeriodChangeHistories: FiscalPeriodChangeHistoryData[] | null = null;
    private _fiscalPeriodChangeHistoriesPromise: Promise<FiscalPeriodChangeHistoryData[]> | null  = null;
    private _fiscalPeriodChangeHistoriesSubject = new BehaviorSubject<FiscalPeriodChangeHistoryData[] | null>(null);

                
    private _financialTransactions: FinancialTransactionData[] | null = null;
    private _financialTransactionsPromise: Promise<FinancialTransactionData[]> | null  = null;
    private _financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

                
    private _budgets: BudgetData[] | null = null;
    private _budgetsPromise: Promise<BudgetData[]> | null  = null;
    private _budgetsSubject = new BehaviorSubject<BudgetData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<FiscalPeriodData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<FiscalPeriodData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<FiscalPeriodData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public FiscalPeriodChangeHistories$ = this._fiscalPeriodChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._fiscalPeriodChangeHistories === null && this._fiscalPeriodChangeHistoriesPromise === null) {
            this.loadFiscalPeriodChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _fiscalPeriodChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get FiscalPeriodChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._fiscalPeriodChangeHistoriesCount$ === null) {
            this._fiscalPeriodChangeHistoriesCount$ = FiscalPeriodChangeHistoryService.Instance.GetFiscalPeriodChangeHistoriesRowCount({fiscalPeriodId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._fiscalPeriodChangeHistoriesCount$;
    }



    public FinancialTransactions$ = this._financialTransactionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._financialTransactions === null && this._financialTransactionsPromise === null) {
            this.loadFinancialTransactions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _financialTransactionsCount$: Observable<bigint | number> | null = null;
    public get FinancialTransactionsCount$(): Observable<bigint | number> {
        if (this._financialTransactionsCount$ === null) {
            this._financialTransactionsCount$ = FinancialTransactionService.Instance.GetFinancialTransactionsRowCount({fiscalPeriodId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._financialTransactionsCount$;
    }



    public Budgets$ = this._budgetsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._budgets === null && this._budgetsPromise === null) {
            this.loadBudgets(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _budgetsCount$: Observable<bigint | number> | null = null;
    public get BudgetsCount$(): Observable<bigint | number> {
        if (this._budgetsCount$ === null) {
            this._budgetsCount$ = BudgetService.Instance.GetBudgetsRowCount({fiscalPeriodId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._budgetsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any FiscalPeriodData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.fiscalPeriod.Reload();
  //
  //  Non Async:
  //
  //     fiscalPeriod[0].Reload().then(x => {
  //        this.fiscalPeriod = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      FiscalPeriodService.Instance.GetFiscalPeriod(this.id, includeRelations)
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
     this._fiscalPeriodChangeHistories = null;
     this._fiscalPeriodChangeHistoriesPromise = null;
     this._fiscalPeriodChangeHistoriesSubject.next(null);
     this._fiscalPeriodChangeHistoriesCount$ = null;

     this._financialTransactions = null;
     this._financialTransactionsPromise = null;
     this._financialTransactionsSubject.next(null);
     this._financialTransactionsCount$ = null;

     this._budgets = null;
     this._budgetsPromise = null;
     this._budgetsSubject.next(null);
     this._budgetsCount$ = null;

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
     * Gets the FiscalPeriodChangeHistories for this FiscalPeriod.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.fiscalPeriod.FiscalPeriodChangeHistories.then(fiscalPeriods => { ... })
     *   or
     *   await this.fiscalPeriod.fiscalPeriods
     *
    */
    public get FiscalPeriodChangeHistories(): Promise<FiscalPeriodChangeHistoryData[]> {
        if (this._fiscalPeriodChangeHistories !== null) {
            return Promise.resolve(this._fiscalPeriodChangeHistories);
        }

        if (this._fiscalPeriodChangeHistoriesPromise !== null) {
            return this._fiscalPeriodChangeHistoriesPromise;
        }

        // Start the load
        this.loadFiscalPeriodChangeHistories();

        return this._fiscalPeriodChangeHistoriesPromise!;
    }



    private loadFiscalPeriodChangeHistories(): void {

        this._fiscalPeriodChangeHistoriesPromise = lastValueFrom(
            FiscalPeriodService.Instance.GetFiscalPeriodChangeHistoriesForFiscalPeriod(this.id)
        )
        .then(FiscalPeriodChangeHistories => {
            this._fiscalPeriodChangeHistories = FiscalPeriodChangeHistories ?? [];
            this._fiscalPeriodChangeHistoriesSubject.next(this._fiscalPeriodChangeHistories);
            return this._fiscalPeriodChangeHistories;
         })
        .catch(err => {
            this._fiscalPeriodChangeHistories = [];
            this._fiscalPeriodChangeHistoriesSubject.next(this._fiscalPeriodChangeHistories);
            throw err;
        })
        .finally(() => {
            this._fiscalPeriodChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached FiscalPeriodChangeHistory. Call after mutations to force refresh.
     */
    public ClearFiscalPeriodChangeHistoriesCache(): void {
        this._fiscalPeriodChangeHistories = null;
        this._fiscalPeriodChangeHistoriesPromise = null;
        this._fiscalPeriodChangeHistoriesSubject.next(this._fiscalPeriodChangeHistories);      // Emit to observable
    }

    public get HasFiscalPeriodChangeHistories(): Promise<boolean> {
        return this.FiscalPeriodChangeHistories.then(fiscalPeriodChangeHistories => fiscalPeriodChangeHistories.length > 0);
    }


    /**
     *
     * Gets the FinancialTransactions for this FiscalPeriod.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.fiscalPeriod.FinancialTransactions.then(fiscalPeriods => { ... })
     *   or
     *   await this.fiscalPeriod.fiscalPeriods
     *
    */
    public get FinancialTransactions(): Promise<FinancialTransactionData[]> {
        if (this._financialTransactions !== null) {
            return Promise.resolve(this._financialTransactions);
        }

        if (this._financialTransactionsPromise !== null) {
            return this._financialTransactionsPromise;
        }

        // Start the load
        this.loadFinancialTransactions();

        return this._financialTransactionsPromise!;
    }



    private loadFinancialTransactions(): void {

        this._financialTransactionsPromise = lastValueFrom(
            FiscalPeriodService.Instance.GetFinancialTransactionsForFiscalPeriod(this.id)
        )
        .then(FinancialTransactions => {
            this._financialTransactions = FinancialTransactions ?? [];
            this._financialTransactionsSubject.next(this._financialTransactions);
            return this._financialTransactions;
         })
        .catch(err => {
            this._financialTransactions = [];
            this._financialTransactionsSubject.next(this._financialTransactions);
            throw err;
        })
        .finally(() => {
            this._financialTransactionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached FinancialTransaction. Call after mutations to force refresh.
     */
    public ClearFinancialTransactionsCache(): void {
        this._financialTransactions = null;
        this._financialTransactionsPromise = null;
        this._financialTransactionsSubject.next(this._financialTransactions);      // Emit to observable
    }

    public get HasFinancialTransactions(): Promise<boolean> {
        return this.FinancialTransactions.then(financialTransactions => financialTransactions.length > 0);
    }


    /**
     *
     * Gets the Budgets for this FiscalPeriod.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.fiscalPeriod.Budgets.then(fiscalPeriods => { ... })
     *   or
     *   await this.fiscalPeriod.fiscalPeriods
     *
    */
    public get Budgets(): Promise<BudgetData[]> {
        if (this._budgets !== null) {
            return Promise.resolve(this._budgets);
        }

        if (this._budgetsPromise !== null) {
            return this._budgetsPromise;
        }

        // Start the load
        this.loadBudgets();

        return this._budgetsPromise!;
    }



    private loadBudgets(): void {

        this._budgetsPromise = lastValueFrom(
            FiscalPeriodService.Instance.GetBudgetsForFiscalPeriod(this.id)
        )
        .then(Budgets => {
            this._budgets = Budgets ?? [];
            this._budgetsSubject.next(this._budgets);
            return this._budgets;
         })
        .catch(err => {
            this._budgets = [];
            this._budgetsSubject.next(this._budgets);
            throw err;
        })
        .finally(() => {
            this._budgetsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Budget. Call after mutations to force refresh.
     */
    public ClearBudgetsCache(): void {
        this._budgets = null;
        this._budgetsPromise = null;
        this._budgetsSubject.next(this._budgets);      // Emit to observable
    }

    public get HasBudgets(): Promise<boolean> {
        return this.Budgets.then(budgets => budgets.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (fiscalPeriod.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await fiscalPeriod.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<FiscalPeriodData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<FiscalPeriodData>> {
        const info = await lastValueFrom(
            FiscalPeriodService.Instance.GetFiscalPeriodChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this FiscalPeriodData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this FiscalPeriodData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): FiscalPeriodSubmitData {
        return FiscalPeriodService.Instance.ConvertToFiscalPeriodSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class FiscalPeriodService extends SecureEndpointBase {

    private static _instance: FiscalPeriodService;
    private listCache: Map<string, Observable<Array<FiscalPeriodData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<FiscalPeriodBasicListData>>>;
    private recordCache: Map<string, Observable<FiscalPeriodData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private fiscalPeriodChangeHistoryService: FiscalPeriodChangeHistoryService,
        private financialTransactionService: FinancialTransactionService,
        private budgetService: BudgetService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<FiscalPeriodData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<FiscalPeriodBasicListData>>>();
        this.recordCache = new Map<string, Observable<FiscalPeriodData>>();

        FiscalPeriodService._instance = this;
    }

    public static get Instance(): FiscalPeriodService {
      return FiscalPeriodService._instance;
    }


    public ClearListCaches(config: FiscalPeriodQueryParameters | null = null) {

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


    public ConvertToFiscalPeriodSubmitData(data: FiscalPeriodData): FiscalPeriodSubmitData {

        let output = new FiscalPeriodSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.startDate = data.startDate;
        output.endDate = data.endDate;
        output.periodType = data.periodType;
        output.fiscalYear = data.fiscalYear;
        output.periodNumber = data.periodNumber;
        output.periodStatusId = data.periodStatusId;
        output.closedDate = data.closedDate;
        output.closedBy = data.closedBy;
        output.sequence = data.sequence;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetFiscalPeriod(id: bigint | number, includeRelations: boolean = true) : Observable<FiscalPeriodData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const fiscalPeriod$ = this.requestFiscalPeriod(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get FiscalPeriod", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, fiscalPeriod$);

            return fiscalPeriod$;
        }

        return this.recordCache.get(configHash) as Observable<FiscalPeriodData>;
    }

    private requestFiscalPeriod(id: bigint | number, includeRelations: boolean = true) : Observable<FiscalPeriodData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FiscalPeriodData>(this.baseUrl + 'api/FiscalPeriod/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveFiscalPeriod(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestFiscalPeriod(id, includeRelations));
            }));
    }

    public GetFiscalPeriodList(config: FiscalPeriodQueryParameters | any = null) : Observable<Array<FiscalPeriodData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const fiscalPeriodList$ = this.requestFiscalPeriodList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get FiscalPeriod list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, fiscalPeriodList$);

            return fiscalPeriodList$;
        }

        return this.listCache.get(configHash) as Observable<Array<FiscalPeriodData>>;
    }


    private requestFiscalPeriodList(config: FiscalPeriodQueryParameters | any) : Observable <Array<FiscalPeriodData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<FiscalPeriodData>>(this.baseUrl + 'api/FiscalPeriods', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveFiscalPeriodList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestFiscalPeriodList(config));
            }));
    }

    public GetFiscalPeriodsRowCount(config: FiscalPeriodQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const fiscalPeriodsRowCount$ = this.requestFiscalPeriodsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get FiscalPeriods row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, fiscalPeriodsRowCount$);

            return fiscalPeriodsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestFiscalPeriodsRowCount(config: FiscalPeriodQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/FiscalPeriods/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestFiscalPeriodsRowCount(config));
            }));
    }

    public GetFiscalPeriodsBasicListData(config: FiscalPeriodQueryParameters | any = null) : Observable<Array<FiscalPeriodBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const fiscalPeriodsBasicListData$ = this.requestFiscalPeriodsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get FiscalPeriods basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, fiscalPeriodsBasicListData$);

            return fiscalPeriodsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<FiscalPeriodBasicListData>>;
    }


    private requestFiscalPeriodsBasicListData(config: FiscalPeriodQueryParameters | any) : Observable<Array<FiscalPeriodBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<FiscalPeriodBasicListData>>(this.baseUrl + 'api/FiscalPeriods/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestFiscalPeriodsBasicListData(config));
            }));

    }


    public PutFiscalPeriod(id: bigint | number, fiscalPeriod: FiscalPeriodSubmitData) : Observable<FiscalPeriodData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<FiscalPeriodData>(this.baseUrl + 'api/FiscalPeriod/' + id.toString(), fiscalPeriod, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFiscalPeriod(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutFiscalPeriod(id, fiscalPeriod));
            }));
    }


    public PostFiscalPeriod(fiscalPeriod: FiscalPeriodSubmitData) : Observable<FiscalPeriodData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<FiscalPeriodData>(this.baseUrl + 'api/FiscalPeriod', fiscalPeriod, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFiscalPeriod(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostFiscalPeriod(fiscalPeriod));
            }));
    }

  
    public DeleteFiscalPeriod(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/FiscalPeriod/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteFiscalPeriod(id));
            }));
    }

    public RollbackFiscalPeriod(id: bigint | number, versionNumber: bigint | number) : Observable<FiscalPeriodData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<FiscalPeriodData>(this.baseUrl + 'api/FiscalPeriod/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFiscalPeriod(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackFiscalPeriod(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a FiscalPeriod.
     */
    public GetFiscalPeriodChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<FiscalPeriodData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<FiscalPeriodData>>(this.baseUrl + 'api/FiscalPeriod/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetFiscalPeriodChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a FiscalPeriod.
     */
    public GetFiscalPeriodAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<FiscalPeriodData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<FiscalPeriodData>[]>(this.baseUrl + 'api/FiscalPeriod/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetFiscalPeriodAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a FiscalPeriod.
     */
    public GetFiscalPeriodVersion(id: bigint | number, version: number): Observable<FiscalPeriodData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FiscalPeriodData>(this.baseUrl + 'api/FiscalPeriod/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveFiscalPeriod(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetFiscalPeriodVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a FiscalPeriod at a specific point in time.
     */
    public GetFiscalPeriodStateAtTime(id: bigint | number, time: string): Observable<FiscalPeriodData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FiscalPeriodData>(this.baseUrl + 'api/FiscalPeriod/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveFiscalPeriod(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetFiscalPeriodStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: FiscalPeriodQueryParameters | any): string {

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

    public userIsSchedulerFiscalPeriodReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerFiscalPeriodReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.FiscalPeriods
        //
        if (userIsSchedulerFiscalPeriodReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerFiscalPeriodReader = user.readPermission >= 1;
            } else {
                userIsSchedulerFiscalPeriodReader = false;
            }
        }

        return userIsSchedulerFiscalPeriodReader;
    }


    public userIsSchedulerFiscalPeriodWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerFiscalPeriodWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.FiscalPeriods
        //
        if (userIsSchedulerFiscalPeriodWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerFiscalPeriodWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerFiscalPeriodWriter = false;
          }      
        }

        return userIsSchedulerFiscalPeriodWriter;
    }

    public GetFiscalPeriodChangeHistoriesForFiscalPeriod(fiscalPeriodId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FiscalPeriodChangeHistoryData[]> {
        return this.fiscalPeriodChangeHistoryService.GetFiscalPeriodChangeHistoryList({
            fiscalPeriodId: fiscalPeriodId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetFinancialTransactionsForFiscalPeriod(fiscalPeriodId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FinancialTransactionData[]> {
        return this.financialTransactionService.GetFinancialTransactionList({
            fiscalPeriodId: fiscalPeriodId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBudgetsForFiscalPeriod(fiscalPeriodId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BudgetData[]> {
        return this.budgetService.GetBudgetList({
            fiscalPeriodId: fiscalPeriodId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full FiscalPeriodData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the FiscalPeriodData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when FiscalPeriodTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveFiscalPeriod(raw: any): FiscalPeriodData {
    if (!raw) return raw;

    //
    // Create a FiscalPeriodData object instance with correct prototype
    //
    const revived = Object.create(FiscalPeriodData.prototype) as FiscalPeriodData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._fiscalPeriodChangeHistories = null;
    (revived as any)._fiscalPeriodChangeHistoriesPromise = null;
    (revived as any)._fiscalPeriodChangeHistoriesSubject = new BehaviorSubject<FiscalPeriodChangeHistoryData[] | null>(null);

    (revived as any)._financialTransactions = null;
    (revived as any)._financialTransactionsPromise = null;
    (revived as any)._financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

    (revived as any)._budgets = null;
    (revived as any)._budgetsPromise = null;
    (revived as any)._budgetsSubject = new BehaviorSubject<BudgetData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadFiscalPeriodXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).FiscalPeriodChangeHistories$ = (revived as any)._fiscalPeriodChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._fiscalPeriodChangeHistories === null && (revived as any)._fiscalPeriodChangeHistoriesPromise === null) {
                (revived as any).loadFiscalPeriodChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._fiscalPeriodChangeHistoriesCount$ = null;


    (revived as any).FinancialTransactions$ = (revived as any)._financialTransactionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._financialTransactions === null && (revived as any)._financialTransactionsPromise === null) {
                (revived as any).loadFinancialTransactions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._financialTransactionsCount$ = null;


    (revived as any).Budgets$ = (revived as any)._budgetsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._budgets === null && (revived as any)._budgetsPromise === null) {
                (revived as any).loadBudgets();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._budgetsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<FiscalPeriodData> | null>(null);

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

  private ReviveFiscalPeriodList(rawList: any[]): FiscalPeriodData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveFiscalPeriod(raw));
  }

}
