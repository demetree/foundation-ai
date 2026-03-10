/*

   GENERATED SERVICE FOR THE PAYMENTTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PaymentTransaction table.

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
import { PaymentMethodData } from './payment-method.service';
import { PaymentProviderData } from './payment-provider.service';
import { ScheduledEventData } from './scheduled-event.service';
import { FinancialTransactionData } from './financial-transaction.service';
import { EventChargeData } from './event-charge.service';
import { CurrencyData } from './currency.service';
import { PaymentTransactionChangeHistoryService, PaymentTransactionChangeHistoryData } from './payment-transaction-change-history.service';
import { ReceiptService, ReceiptData } from './receipt.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PaymentTransactionQueryParameters {
    paymentMethodId: bigint | number | null | undefined = null;
    paymentProviderId: bigint | number | null | undefined = null;
    scheduledEventId: bigint | number | null | undefined = null;
    financialTransactionId: bigint | number | null | undefined = null;
    eventChargeId: bigint | number | null | undefined = null;
    transactionDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    amount: number | null | undefined = null;
    processingFee: number | null | undefined = null;
    netAmount: number | null | undefined = null;
    currencyId: bigint | number | null | undefined = null;
    status: string | null | undefined = null;
    providerTransactionId: string | null | undefined = null;
    providerResponse: string | null | undefined = null;
    payerName: string | null | undefined = null;
    payerEmail: string | null | undefined = null;
    payerPhone: string | null | undefined = null;
    receiptNumber: string | null | undefined = null;
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
export class PaymentTransactionSubmitData {
    id!: bigint | number;
    paymentMethodId!: bigint | number;
    paymentProviderId: bigint | number | null = null;
    scheduledEventId: bigint | number | null = null;
    financialTransactionId: bigint | number | null = null;
    eventChargeId: bigint | number | null = null;
    transactionDate!: string;      // ISO 8601 (full datetime)
    amount!: number;
    processingFee!: number;
    netAmount!: number;
    currencyId!: bigint | number;
    status!: string;
    providerTransactionId: string | null = null;
    providerResponse: string | null = null;
    payerName: string | null = null;
    payerEmail: string | null = null;
    payerPhone: string | null = null;
    receiptNumber: string | null = null;
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

export class PaymentTransactionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PaymentTransactionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `paymentTransaction.PaymentTransactionChildren$` — use with `| async` in templates
//        • Promise:    `paymentTransaction.PaymentTransactionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="paymentTransaction.PaymentTransactionChildren$ | async"`), or
//        • Access the promise getter (`paymentTransaction.PaymentTransactionChildren` or `await paymentTransaction.PaymentTransactionChildren`)
//    - Simply reading `paymentTransaction.PaymentTransactionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await paymentTransaction.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PaymentTransactionData {
    id!: bigint | number;
    paymentMethodId!: bigint | number;
    paymentProviderId!: bigint | number;
    scheduledEventId!: bigint | number;
    financialTransactionId!: bigint | number;
    eventChargeId!: bigint | number;
    transactionDate!: string;      // ISO 8601 (full datetime)
    amount!: number;
    processingFee!: number;
    netAmount!: number;
    currencyId!: bigint | number;
    status!: string;
    providerTransactionId!: string | null;
    providerResponse!: string | null;
    payerName!: string | null;
    payerEmail!: string | null;
    payerPhone!: string | null;
    receiptNumber!: string | null;
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    currency: CurrencyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    eventCharge: EventChargeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    financialTransaction: FinancialTransactionData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    paymentMethod: PaymentMethodData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    paymentProvider: PaymentProviderData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduledEvent: ScheduledEventData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _paymentTransactionChangeHistories: PaymentTransactionChangeHistoryData[] | null = null;
    private _paymentTransactionChangeHistoriesPromise: Promise<PaymentTransactionChangeHistoryData[]> | null  = null;
    private _paymentTransactionChangeHistoriesSubject = new BehaviorSubject<PaymentTransactionChangeHistoryData[] | null>(null);

                
    private _receipts: ReceiptData[] | null = null;
    private _receiptsPromise: Promise<ReceiptData[]> | null  = null;
    private _receiptsSubject = new BehaviorSubject<ReceiptData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<PaymentTransactionData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<PaymentTransactionData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PaymentTransactionData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public PaymentTransactionChangeHistories$ = this._paymentTransactionChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._paymentTransactionChangeHistories === null && this._paymentTransactionChangeHistoriesPromise === null) {
            this.loadPaymentTransactionChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _paymentTransactionChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get PaymentTransactionChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._paymentTransactionChangeHistoriesCount$ === null) {
            this._paymentTransactionChangeHistoriesCount$ = PaymentTransactionChangeHistoryService.Instance.GetPaymentTransactionChangeHistoriesRowCount({paymentTransactionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._paymentTransactionChangeHistoriesCount$;
    }



    public Receipts$ = this._receiptsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._receipts === null && this._receiptsPromise === null) {
            this.loadReceipts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _receiptsCount$: Observable<bigint | number> | null = null;
    public get ReceiptsCount$(): Observable<bigint | number> {
        if (this._receiptsCount$ === null) {
            this._receiptsCount$ = ReceiptService.Instance.GetReceiptsRowCount({paymentTransactionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._receiptsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PaymentTransactionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.paymentTransaction.Reload();
  //
  //  Non Async:
  //
  //     paymentTransaction[0].Reload().then(x => {
  //        this.paymentTransaction = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PaymentTransactionService.Instance.GetPaymentTransaction(this.id, includeRelations)
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
     this._paymentTransactionChangeHistories = null;
     this._paymentTransactionChangeHistoriesPromise = null;
     this._paymentTransactionChangeHistoriesSubject.next(null);
     this._paymentTransactionChangeHistoriesCount$ = null;

     this._receipts = null;
     this._receiptsPromise = null;
     this._receiptsSubject.next(null);
     this._receiptsCount$ = null;

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
     * Gets the PaymentTransactionChangeHistories for this PaymentTransaction.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.paymentTransaction.PaymentTransactionChangeHistories.then(paymentTransactions => { ... })
     *   or
     *   await this.paymentTransaction.paymentTransactions
     *
    */
    public get PaymentTransactionChangeHistories(): Promise<PaymentTransactionChangeHistoryData[]> {
        if (this._paymentTransactionChangeHistories !== null) {
            return Promise.resolve(this._paymentTransactionChangeHistories);
        }

        if (this._paymentTransactionChangeHistoriesPromise !== null) {
            return this._paymentTransactionChangeHistoriesPromise;
        }

        // Start the load
        this.loadPaymentTransactionChangeHistories();

        return this._paymentTransactionChangeHistoriesPromise!;
    }



    private loadPaymentTransactionChangeHistories(): void {

        this._paymentTransactionChangeHistoriesPromise = lastValueFrom(
            PaymentTransactionService.Instance.GetPaymentTransactionChangeHistoriesForPaymentTransaction(this.id)
        )
        .then(PaymentTransactionChangeHistories => {
            this._paymentTransactionChangeHistories = PaymentTransactionChangeHistories ?? [];
            this._paymentTransactionChangeHistoriesSubject.next(this._paymentTransactionChangeHistories);
            return this._paymentTransactionChangeHistories;
         })
        .catch(err => {
            this._paymentTransactionChangeHistories = [];
            this._paymentTransactionChangeHistoriesSubject.next(this._paymentTransactionChangeHistories);
            throw err;
        })
        .finally(() => {
            this._paymentTransactionChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PaymentTransactionChangeHistory. Call after mutations to force refresh.
     */
    public ClearPaymentTransactionChangeHistoriesCache(): void {
        this._paymentTransactionChangeHistories = null;
        this._paymentTransactionChangeHistoriesPromise = null;
        this._paymentTransactionChangeHistoriesSubject.next(this._paymentTransactionChangeHistories);      // Emit to observable
    }

    public get HasPaymentTransactionChangeHistories(): Promise<boolean> {
        return this.PaymentTransactionChangeHistories.then(paymentTransactionChangeHistories => paymentTransactionChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Receipts for this PaymentTransaction.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.paymentTransaction.Receipts.then(paymentTransactions => { ... })
     *   or
     *   await this.paymentTransaction.paymentTransactions
     *
    */
    public get Receipts(): Promise<ReceiptData[]> {
        if (this._receipts !== null) {
            return Promise.resolve(this._receipts);
        }

        if (this._receiptsPromise !== null) {
            return this._receiptsPromise;
        }

        // Start the load
        this.loadReceipts();

        return this._receiptsPromise!;
    }



    private loadReceipts(): void {

        this._receiptsPromise = lastValueFrom(
            PaymentTransactionService.Instance.GetReceiptsForPaymentTransaction(this.id)
        )
        .then(Receipts => {
            this._receipts = Receipts ?? [];
            this._receiptsSubject.next(this._receipts);
            return this._receipts;
         })
        .catch(err => {
            this._receipts = [];
            this._receiptsSubject.next(this._receipts);
            throw err;
        })
        .finally(() => {
            this._receiptsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Receipt. Call after mutations to force refresh.
     */
    public ClearReceiptsCache(): void {
        this._receipts = null;
        this._receiptsPromise = null;
        this._receiptsSubject.next(this._receipts);      // Emit to observable
    }

    public get HasReceipts(): Promise<boolean> {
        return this.Receipts.then(receipts => receipts.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (paymentTransaction.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await paymentTransaction.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<PaymentTransactionData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<PaymentTransactionData>> {
        const info = await lastValueFrom(
            PaymentTransactionService.Instance.GetPaymentTransactionChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this PaymentTransactionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PaymentTransactionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PaymentTransactionSubmitData {
        return PaymentTransactionService.Instance.ConvertToPaymentTransactionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PaymentTransactionService extends SecureEndpointBase {

    private static _instance: PaymentTransactionService;
    private listCache: Map<string, Observable<Array<PaymentTransactionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PaymentTransactionBasicListData>>>;
    private recordCache: Map<string, Observable<PaymentTransactionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private paymentTransactionChangeHistoryService: PaymentTransactionChangeHistoryService,
        private receiptService: ReceiptService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PaymentTransactionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PaymentTransactionBasicListData>>>();
        this.recordCache = new Map<string, Observable<PaymentTransactionData>>();

        PaymentTransactionService._instance = this;
    }

    public static get Instance(): PaymentTransactionService {
      return PaymentTransactionService._instance;
    }


    public ClearListCaches(config: PaymentTransactionQueryParameters | null = null) {

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


    public ConvertToPaymentTransactionSubmitData(data: PaymentTransactionData): PaymentTransactionSubmitData {

        let output = new PaymentTransactionSubmitData();

        output.id = data.id;
        output.paymentMethodId = data.paymentMethodId;
        output.paymentProviderId = data.paymentProviderId;
        output.scheduledEventId = data.scheduledEventId;
        output.financialTransactionId = data.financialTransactionId;
        output.eventChargeId = data.eventChargeId;
        output.transactionDate = data.transactionDate;
        output.amount = data.amount;
        output.processingFee = data.processingFee;
        output.netAmount = data.netAmount;
        output.currencyId = data.currencyId;
        output.status = data.status;
        output.providerTransactionId = data.providerTransactionId;
        output.providerResponse = data.providerResponse;
        output.payerName = data.payerName;
        output.payerEmail = data.payerEmail;
        output.payerPhone = data.payerPhone;
        output.receiptNumber = data.receiptNumber;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPaymentTransaction(id: bigint | number, includeRelations: boolean = true) : Observable<PaymentTransactionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const paymentTransaction$ = this.requestPaymentTransaction(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PaymentTransaction", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, paymentTransaction$);

            return paymentTransaction$;
        }

        return this.recordCache.get(configHash) as Observable<PaymentTransactionData>;
    }

    private requestPaymentTransaction(id: bigint | number, includeRelations: boolean = true) : Observable<PaymentTransactionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PaymentTransactionData>(this.baseUrl + 'api/PaymentTransaction/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePaymentTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentTransaction(id, includeRelations));
            }));
    }

    public GetPaymentTransactionList(config: PaymentTransactionQueryParameters | any = null) : Observable<Array<PaymentTransactionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const paymentTransactionList$ = this.requestPaymentTransactionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PaymentTransaction list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, paymentTransactionList$);

            return paymentTransactionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PaymentTransactionData>>;
    }


    private requestPaymentTransactionList(config: PaymentTransactionQueryParameters | any) : Observable <Array<PaymentTransactionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PaymentTransactionData>>(this.baseUrl + 'api/PaymentTransactions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePaymentTransactionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentTransactionList(config));
            }));
    }

    public GetPaymentTransactionsRowCount(config: PaymentTransactionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const paymentTransactionsRowCount$ = this.requestPaymentTransactionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PaymentTransactions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, paymentTransactionsRowCount$);

            return paymentTransactionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPaymentTransactionsRowCount(config: PaymentTransactionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PaymentTransactions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentTransactionsRowCount(config));
            }));
    }

    public GetPaymentTransactionsBasicListData(config: PaymentTransactionQueryParameters | any = null) : Observable<Array<PaymentTransactionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const paymentTransactionsBasicListData$ = this.requestPaymentTransactionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PaymentTransactions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, paymentTransactionsBasicListData$);

            return paymentTransactionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PaymentTransactionBasicListData>>;
    }


    private requestPaymentTransactionsBasicListData(config: PaymentTransactionQueryParameters | any) : Observable<Array<PaymentTransactionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PaymentTransactionBasicListData>>(this.baseUrl + 'api/PaymentTransactions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentTransactionsBasicListData(config));
            }));

    }


    public PutPaymentTransaction(id: bigint | number, paymentTransaction: PaymentTransactionSubmitData) : Observable<PaymentTransactionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PaymentTransactionData>(this.baseUrl + 'api/PaymentTransaction/' + id.toString(), paymentTransaction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPaymentTransaction(id, paymentTransaction));
            }));
    }


    public PostPaymentTransaction(paymentTransaction: PaymentTransactionSubmitData) : Observable<PaymentTransactionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PaymentTransactionData>(this.baseUrl + 'api/PaymentTransaction', paymentTransaction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentTransaction(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPaymentTransaction(paymentTransaction));
            }));
    }

  
    public DeletePaymentTransaction(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PaymentTransaction/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePaymentTransaction(id));
            }));
    }

    public RollbackPaymentTransaction(id: bigint | number, versionNumber: bigint | number) : Observable<PaymentTransactionData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PaymentTransactionData>(this.baseUrl + 'api/PaymentTransaction/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackPaymentTransaction(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a PaymentTransaction.
     */
    public GetPaymentTransactionChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<PaymentTransactionData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PaymentTransactionData>>(this.baseUrl + 'api/PaymentTransaction/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPaymentTransactionChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a PaymentTransaction.
     */
    public GetPaymentTransactionAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<PaymentTransactionData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PaymentTransactionData>[]>(this.baseUrl + 'api/PaymentTransaction/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPaymentTransactionAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a PaymentTransaction.
     */
    public GetPaymentTransactionVersion(id: bigint | number, version: number): Observable<PaymentTransactionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PaymentTransactionData>(this.baseUrl + 'api/PaymentTransaction/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePaymentTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPaymentTransactionVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a PaymentTransaction at a specific point in time.
     */
    public GetPaymentTransactionStateAtTime(id: bigint | number, time: string): Observable<PaymentTransactionData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PaymentTransactionData>(this.baseUrl + 'api/PaymentTransaction/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePaymentTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPaymentTransactionStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: PaymentTransactionQueryParameters | any): string {

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

    public userIsSchedulerPaymentTransactionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerPaymentTransactionReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.PaymentTransactions
        //
        if (userIsSchedulerPaymentTransactionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerPaymentTransactionReader = user.readPermission >= 1;
            } else {
                userIsSchedulerPaymentTransactionReader = false;
            }
        }

        return userIsSchedulerPaymentTransactionReader;
    }


    public userIsSchedulerPaymentTransactionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerPaymentTransactionWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.PaymentTransactions
        //
        if (userIsSchedulerPaymentTransactionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerPaymentTransactionWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerPaymentTransactionWriter = false;
          }      
        }

        return userIsSchedulerPaymentTransactionWriter;
    }

    public GetPaymentTransactionChangeHistoriesForPaymentTransaction(paymentTransactionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PaymentTransactionChangeHistoryData[]> {
        return this.paymentTransactionChangeHistoryService.GetPaymentTransactionChangeHistoryList({
            paymentTransactionId: paymentTransactionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetReceiptsForPaymentTransaction(paymentTransactionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ReceiptData[]> {
        return this.receiptService.GetReceiptList({
            paymentTransactionId: paymentTransactionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PaymentTransactionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PaymentTransactionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PaymentTransactionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePaymentTransaction(raw: any): PaymentTransactionData {
    if (!raw) return raw;

    //
    // Create a PaymentTransactionData object instance with correct prototype
    //
    const revived = Object.create(PaymentTransactionData.prototype) as PaymentTransactionData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._paymentTransactionChangeHistories = null;
    (revived as any)._paymentTransactionChangeHistoriesPromise = null;
    (revived as any)._paymentTransactionChangeHistoriesSubject = new BehaviorSubject<PaymentTransactionChangeHistoryData[] | null>(null);

    (revived as any)._receipts = null;
    (revived as any)._receiptsPromise = null;
    (revived as any)._receiptsSubject = new BehaviorSubject<ReceiptData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadPaymentTransactionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).PaymentTransactionChangeHistories$ = (revived as any)._paymentTransactionChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._paymentTransactionChangeHistories === null && (revived as any)._paymentTransactionChangeHistoriesPromise === null) {
                (revived as any).loadPaymentTransactionChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._paymentTransactionChangeHistoriesCount$ = null;


    (revived as any).Receipts$ = (revived as any)._receiptsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._receipts === null && (revived as any)._receiptsPromise === null) {
                (revived as any).loadReceipts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._receiptsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PaymentTransactionData> | null>(null);

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

  private RevivePaymentTransactionList(rawList: any[]): PaymentTransactionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePaymentTransaction(raw));
  }

}
