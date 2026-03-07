/*

   GENERATED SERVICE FOR THE PAYMENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PaymentType table.

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
import { PaymentTypeChangeHistoryService, PaymentTypeChangeHistoryData } from './payment-type-change-history.service';
import { FinancialTransactionService, FinancialTransactionData } from './financial-transaction.service';
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
export class PaymentTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    color: string | null | undefined = null;
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
export class PaymentTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    color: string | null = null;
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

export class PaymentTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PaymentTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `paymentType.PaymentTypeChildren$` — use with `| async` in templates
//        • Promise:    `paymentType.PaymentTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="paymentType.PaymentTypeChildren$ | async"`), or
//        • Access the promise getter (`paymentType.PaymentTypeChildren` or `await paymentType.PaymentTypeChildren`)
//    - Simply reading `paymentType.PaymentTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await paymentType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PaymentTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    color!: string | null;
    sequence!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _paymentTypeChangeHistories: PaymentTypeChangeHistoryData[] | null = null;
    private _paymentTypeChangeHistoriesPromise: Promise<PaymentTypeChangeHistoryData[]> | null  = null;
    private _paymentTypeChangeHistoriesSubject = new BehaviorSubject<PaymentTypeChangeHistoryData[] | null>(null);

                
    private _financialTransactions: FinancialTransactionData[] | null = null;
    private _financialTransactionsPromise: Promise<FinancialTransactionData[]> | null  = null;
    private _financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

                
    private _gifts: GiftData[] | null = null;
    private _giftsPromise: Promise<GiftData[]> | null  = null;
    private _giftsSubject = new BehaviorSubject<GiftData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<PaymentTypeData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<PaymentTypeData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PaymentTypeData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public PaymentTypeChangeHistories$ = this._paymentTypeChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._paymentTypeChangeHistories === null && this._paymentTypeChangeHistoriesPromise === null) {
            this.loadPaymentTypeChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _paymentTypeChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get PaymentTypeChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._paymentTypeChangeHistoriesCount$ === null) {
            this._paymentTypeChangeHistoriesCount$ = PaymentTypeChangeHistoryService.Instance.GetPaymentTypeChangeHistoriesRowCount({paymentTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._paymentTypeChangeHistoriesCount$;
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
            this._financialTransactionsCount$ = FinancialTransactionService.Instance.GetFinancialTransactionsRowCount({paymentTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._financialTransactionsCount$;
    }



    public Gifts$ = this._giftsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._gifts === null && this._giftsPromise === null) {
            this.loadGifts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _giftsCount$: Observable<bigint | number> | null = null;
    public get GiftsCount$(): Observable<bigint | number> {
        if (this._giftsCount$ === null) {
            this._giftsCount$ = GiftService.Instance.GetGiftsRowCount({paymentTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._giftsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PaymentTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.paymentType.Reload();
  //
  //  Non Async:
  //
  //     paymentType[0].Reload().then(x => {
  //        this.paymentType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PaymentTypeService.Instance.GetPaymentType(this.id, includeRelations)
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
     this._paymentTypeChangeHistories = null;
     this._paymentTypeChangeHistoriesPromise = null;
     this._paymentTypeChangeHistoriesSubject.next(null);
     this._paymentTypeChangeHistoriesCount$ = null;

     this._financialTransactions = null;
     this._financialTransactionsPromise = null;
     this._financialTransactionsSubject.next(null);
     this._financialTransactionsCount$ = null;

     this._gifts = null;
     this._giftsPromise = null;
     this._giftsSubject.next(null);
     this._giftsCount$ = null;

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
     * Gets the PaymentTypeChangeHistories for this PaymentType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.paymentType.PaymentTypeChangeHistories.then(paymentTypes => { ... })
     *   or
     *   await this.paymentType.paymentTypes
     *
    */
    public get PaymentTypeChangeHistories(): Promise<PaymentTypeChangeHistoryData[]> {
        if (this._paymentTypeChangeHistories !== null) {
            return Promise.resolve(this._paymentTypeChangeHistories);
        }

        if (this._paymentTypeChangeHistoriesPromise !== null) {
            return this._paymentTypeChangeHistoriesPromise;
        }

        // Start the load
        this.loadPaymentTypeChangeHistories();

        return this._paymentTypeChangeHistoriesPromise!;
    }



    private loadPaymentTypeChangeHistories(): void {

        this._paymentTypeChangeHistoriesPromise = lastValueFrom(
            PaymentTypeService.Instance.GetPaymentTypeChangeHistoriesForPaymentType(this.id)
        )
        .then(PaymentTypeChangeHistories => {
            this._paymentTypeChangeHistories = PaymentTypeChangeHistories ?? [];
            this._paymentTypeChangeHistoriesSubject.next(this._paymentTypeChangeHistories);
            return this._paymentTypeChangeHistories;
         })
        .catch(err => {
            this._paymentTypeChangeHistories = [];
            this._paymentTypeChangeHistoriesSubject.next(this._paymentTypeChangeHistories);
            throw err;
        })
        .finally(() => {
            this._paymentTypeChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PaymentTypeChangeHistory. Call after mutations to force refresh.
     */
    public ClearPaymentTypeChangeHistoriesCache(): void {
        this._paymentTypeChangeHistories = null;
        this._paymentTypeChangeHistoriesPromise = null;
        this._paymentTypeChangeHistoriesSubject.next(this._paymentTypeChangeHistories);      // Emit to observable
    }

    public get HasPaymentTypeChangeHistories(): Promise<boolean> {
        return this.PaymentTypeChangeHistories.then(paymentTypeChangeHistories => paymentTypeChangeHistories.length > 0);
    }


    /**
     *
     * Gets the FinancialTransactions for this PaymentType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.paymentType.FinancialTransactions.then(paymentTypes => { ... })
     *   or
     *   await this.paymentType.paymentTypes
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
            PaymentTypeService.Instance.GetFinancialTransactionsForPaymentType(this.id)
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
     * Gets the Gifts for this PaymentType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.paymentType.Gifts.then(paymentTypes => { ... })
     *   or
     *   await this.paymentType.paymentTypes
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
            PaymentTypeService.Instance.GetGiftsForPaymentType(this.id)
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




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (paymentType.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await paymentType.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<PaymentTypeData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<PaymentTypeData>> {
        const info = await lastValueFrom(
            PaymentTypeService.Instance.GetPaymentTypeChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this PaymentTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PaymentTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PaymentTypeSubmitData {
        return PaymentTypeService.Instance.ConvertToPaymentTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PaymentTypeService extends SecureEndpointBase {

    private static _instance: PaymentTypeService;
    private listCache: Map<string, Observable<Array<PaymentTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PaymentTypeBasicListData>>>;
    private recordCache: Map<string, Observable<PaymentTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private paymentTypeChangeHistoryService: PaymentTypeChangeHistoryService,
        private financialTransactionService: FinancialTransactionService,
        private giftService: GiftService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PaymentTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PaymentTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<PaymentTypeData>>();

        PaymentTypeService._instance = this;
    }

    public static get Instance(): PaymentTypeService {
      return PaymentTypeService._instance;
    }


    public ClearListCaches(config: PaymentTypeQueryParameters | null = null) {

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


    public ConvertToPaymentTypeSubmitData(data: PaymentTypeData): PaymentTypeSubmitData {

        let output = new PaymentTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.color = data.color;
        output.sequence = data.sequence;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPaymentType(id: bigint | number, includeRelations: boolean = true) : Observable<PaymentTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const paymentType$ = this.requestPaymentType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PaymentType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, paymentType$);

            return paymentType$;
        }

        return this.recordCache.get(configHash) as Observable<PaymentTypeData>;
    }

    private requestPaymentType(id: bigint | number, includeRelations: boolean = true) : Observable<PaymentTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PaymentTypeData>(this.baseUrl + 'api/PaymentType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePaymentType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentType(id, includeRelations));
            }));
    }

    public GetPaymentTypeList(config: PaymentTypeQueryParameters | any = null) : Observable<Array<PaymentTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const paymentTypeList$ = this.requestPaymentTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PaymentType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, paymentTypeList$);

            return paymentTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PaymentTypeData>>;
    }


    private requestPaymentTypeList(config: PaymentTypeQueryParameters | any) : Observable <Array<PaymentTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PaymentTypeData>>(this.baseUrl + 'api/PaymentTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePaymentTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentTypeList(config));
            }));
    }

    public GetPaymentTypesRowCount(config: PaymentTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const paymentTypesRowCount$ = this.requestPaymentTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PaymentTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, paymentTypesRowCount$);

            return paymentTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPaymentTypesRowCount(config: PaymentTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PaymentTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentTypesRowCount(config));
            }));
    }

    public GetPaymentTypesBasicListData(config: PaymentTypeQueryParameters | any = null) : Observable<Array<PaymentTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const paymentTypesBasicListData$ = this.requestPaymentTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PaymentTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, paymentTypesBasicListData$);

            return paymentTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PaymentTypeBasicListData>>;
    }


    private requestPaymentTypesBasicListData(config: PaymentTypeQueryParameters | any) : Observable<Array<PaymentTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PaymentTypeBasicListData>>(this.baseUrl + 'api/PaymentTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentTypesBasicListData(config));
            }));

    }


    public PutPaymentType(id: bigint | number, paymentType: PaymentTypeSubmitData) : Observable<PaymentTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PaymentTypeData>(this.baseUrl + 'api/PaymentType/' + id.toString(), paymentType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPaymentType(id, paymentType));
            }));
    }


    public PostPaymentType(paymentType: PaymentTypeSubmitData) : Observable<PaymentTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PaymentTypeData>(this.baseUrl + 'api/PaymentType', paymentType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPaymentType(paymentType));
            }));
    }

  
    public DeletePaymentType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PaymentType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePaymentType(id));
            }));
    }

    public RollbackPaymentType(id: bigint | number, versionNumber: bigint | number) : Observable<PaymentTypeData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PaymentTypeData>(this.baseUrl + 'api/PaymentType/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackPaymentType(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a PaymentType.
     */
    public GetPaymentTypeChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<PaymentTypeData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PaymentTypeData>>(this.baseUrl + 'api/PaymentType/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPaymentTypeChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a PaymentType.
     */
    public GetPaymentTypeAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<PaymentTypeData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PaymentTypeData>[]>(this.baseUrl + 'api/PaymentType/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPaymentTypeAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a PaymentType.
     */
    public GetPaymentTypeVersion(id: bigint | number, version: number): Observable<PaymentTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PaymentTypeData>(this.baseUrl + 'api/PaymentType/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePaymentType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPaymentTypeVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a PaymentType at a specific point in time.
     */
    public GetPaymentTypeStateAtTime(id: bigint | number, time: string): Observable<PaymentTypeData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PaymentTypeData>(this.baseUrl + 'api/PaymentType/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePaymentType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPaymentTypeStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: PaymentTypeQueryParameters | any): string {

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

    public userIsSchedulerPaymentTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerPaymentTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.PaymentTypes
        //
        if (userIsSchedulerPaymentTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerPaymentTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerPaymentTypeReader = false;
            }
        }

        return userIsSchedulerPaymentTypeReader;
    }


    public userIsSchedulerPaymentTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerPaymentTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.PaymentTypes
        //
        if (userIsSchedulerPaymentTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerPaymentTypeWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerPaymentTypeWriter = false;
          }      
        }

        return userIsSchedulerPaymentTypeWriter;
    }

    public GetPaymentTypeChangeHistoriesForPaymentType(paymentTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PaymentTypeChangeHistoryData[]> {
        return this.paymentTypeChangeHistoryService.GetPaymentTypeChangeHistoryList({
            paymentTypeId: paymentTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetFinancialTransactionsForPaymentType(paymentTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FinancialTransactionData[]> {
        return this.financialTransactionService.GetFinancialTransactionList({
            paymentTypeId: paymentTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetGiftsForPaymentType(paymentTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GiftData[]> {
        return this.giftService.GetGiftList({
            paymentTypeId: paymentTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PaymentTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PaymentTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PaymentTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePaymentType(raw: any): PaymentTypeData {
    if (!raw) return raw;

    //
    // Create a PaymentTypeData object instance with correct prototype
    //
    const revived = Object.create(PaymentTypeData.prototype) as PaymentTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._paymentTypeChangeHistories = null;
    (revived as any)._paymentTypeChangeHistoriesPromise = null;
    (revived as any)._paymentTypeChangeHistoriesSubject = new BehaviorSubject<PaymentTypeChangeHistoryData[] | null>(null);

    (revived as any)._financialTransactions = null;
    (revived as any)._financialTransactionsPromise = null;
    (revived as any)._financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

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
    // 2. But private methods (loadPaymentTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).PaymentTypeChangeHistories$ = (revived as any)._paymentTypeChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._paymentTypeChangeHistories === null && (revived as any)._paymentTypeChangeHistoriesPromise === null) {
                (revived as any).loadPaymentTypeChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._paymentTypeChangeHistoriesCount$ = null;


    (revived as any).FinancialTransactions$ = (revived as any)._financialTransactionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._financialTransactions === null && (revived as any)._financialTransactionsPromise === null) {
                (revived as any).loadFinancialTransactions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._financialTransactionsCount$ = null;


    (revived as any).Gifts$ = (revived as any)._giftsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._gifts === null && (revived as any)._giftsPromise === null) {
                (revived as any).loadGifts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._giftsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PaymentTypeData> | null>(null);

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

  private RevivePaymentTypeList(rawList: any[]): PaymentTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePaymentType(raw));
  }

}
