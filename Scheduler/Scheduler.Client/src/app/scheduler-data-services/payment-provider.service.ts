/*

   GENERATED SERVICE FOR THE PAYMENTPROVIDER TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PaymentProvider table.

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
import { PaymentProviderChangeHistoryService, PaymentProviderChangeHistoryData } from './payment-provider-change-history.service';
import { PaymentTransactionService, PaymentTransactionData } from './payment-transaction.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PaymentProviderQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    providerType: string | null | undefined = null;
    isActive: boolean | null | undefined = null;
    apiKeyEncrypted: string | null | undefined = null;
    merchantId: string | null | undefined = null;
    webhookSecret: string | null | undefined = null;
    processingFeePercent: number | null | undefined = null;
    processingFeeFixed: number | null | undefined = null;
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
export class PaymentProviderSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    providerType!: string;
    isActive!: boolean;
    apiKeyEncrypted: string | null = null;
    merchantId: string | null = null;
    webhookSecret: string | null = null;
    processingFeePercent: number | null = null;
    processingFeeFixed: number | null = null;
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

export class PaymentProviderBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PaymentProviderChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `paymentProvider.PaymentProviderChildren$` — use with `| async` in templates
//        • Promise:    `paymentProvider.PaymentProviderChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="paymentProvider.PaymentProviderChildren$ | async"`), or
//        • Access the promise getter (`paymentProvider.PaymentProviderChildren` or `await paymentProvider.PaymentProviderChildren`)
//    - Simply reading `paymentProvider.PaymentProviderChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await paymentProvider.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PaymentProviderData {
    id!: bigint | number;
    name!: string;
    description!: string;
    providerType!: string;
    isActive!: boolean;
    apiKeyEncrypted!: string | null;
    merchantId!: string | null;
    webhookSecret!: string | null;
    processingFeePercent!: number | null;
    processingFeeFixed!: number | null;
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _paymentProviderChangeHistories: PaymentProviderChangeHistoryData[] | null = null;
    private _paymentProviderChangeHistoriesPromise: Promise<PaymentProviderChangeHistoryData[]> | null  = null;
    private _paymentProviderChangeHistoriesSubject = new BehaviorSubject<PaymentProviderChangeHistoryData[] | null>(null);

                
    private _paymentTransactions: PaymentTransactionData[] | null = null;
    private _paymentTransactionsPromise: Promise<PaymentTransactionData[]> | null  = null;
    private _paymentTransactionsSubject = new BehaviorSubject<PaymentTransactionData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<PaymentProviderData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<PaymentProviderData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PaymentProviderData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public PaymentProviderChangeHistories$ = this._paymentProviderChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._paymentProviderChangeHistories === null && this._paymentProviderChangeHistoriesPromise === null) {
            this.loadPaymentProviderChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _paymentProviderChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get PaymentProviderChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._paymentProviderChangeHistoriesCount$ === null) {
            this._paymentProviderChangeHistoriesCount$ = PaymentProviderChangeHistoryService.Instance.GetPaymentProviderChangeHistoriesRowCount({paymentProviderId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._paymentProviderChangeHistoriesCount$;
    }



    public PaymentTransactions$ = this._paymentTransactionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._paymentTransactions === null && this._paymentTransactionsPromise === null) {
            this.loadPaymentTransactions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _paymentTransactionsCount$: Observable<bigint | number> | null = null;
    public get PaymentTransactionsCount$(): Observable<bigint | number> {
        if (this._paymentTransactionsCount$ === null) {
            this._paymentTransactionsCount$ = PaymentTransactionService.Instance.GetPaymentTransactionsRowCount({paymentProviderId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._paymentTransactionsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PaymentProviderData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.paymentProvider.Reload();
  //
  //  Non Async:
  //
  //     paymentProvider[0].Reload().then(x => {
  //        this.paymentProvider = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PaymentProviderService.Instance.GetPaymentProvider(this.id, includeRelations)
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
     this._paymentProviderChangeHistories = null;
     this._paymentProviderChangeHistoriesPromise = null;
     this._paymentProviderChangeHistoriesSubject.next(null);
     this._paymentProviderChangeHistoriesCount$ = null;

     this._paymentTransactions = null;
     this._paymentTransactionsPromise = null;
     this._paymentTransactionsSubject.next(null);
     this._paymentTransactionsCount$ = null;

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
     * Gets the PaymentProviderChangeHistories for this PaymentProvider.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.paymentProvider.PaymentProviderChangeHistories.then(paymentProviders => { ... })
     *   or
     *   await this.paymentProvider.paymentProviders
     *
    */
    public get PaymentProviderChangeHistories(): Promise<PaymentProviderChangeHistoryData[]> {
        if (this._paymentProviderChangeHistories !== null) {
            return Promise.resolve(this._paymentProviderChangeHistories);
        }

        if (this._paymentProviderChangeHistoriesPromise !== null) {
            return this._paymentProviderChangeHistoriesPromise;
        }

        // Start the load
        this.loadPaymentProviderChangeHistories();

        return this._paymentProviderChangeHistoriesPromise!;
    }



    private loadPaymentProviderChangeHistories(): void {

        this._paymentProviderChangeHistoriesPromise = lastValueFrom(
            PaymentProviderService.Instance.GetPaymentProviderChangeHistoriesForPaymentProvider(this.id)
        )
        .then(PaymentProviderChangeHistories => {
            this._paymentProviderChangeHistories = PaymentProviderChangeHistories ?? [];
            this._paymentProviderChangeHistoriesSubject.next(this._paymentProviderChangeHistories);
            return this._paymentProviderChangeHistories;
         })
        .catch(err => {
            this._paymentProviderChangeHistories = [];
            this._paymentProviderChangeHistoriesSubject.next(this._paymentProviderChangeHistories);
            throw err;
        })
        .finally(() => {
            this._paymentProviderChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PaymentProviderChangeHistory. Call after mutations to force refresh.
     */
    public ClearPaymentProviderChangeHistoriesCache(): void {
        this._paymentProviderChangeHistories = null;
        this._paymentProviderChangeHistoriesPromise = null;
        this._paymentProviderChangeHistoriesSubject.next(this._paymentProviderChangeHistories);      // Emit to observable
    }

    public get HasPaymentProviderChangeHistories(): Promise<boolean> {
        return this.PaymentProviderChangeHistories.then(paymentProviderChangeHistories => paymentProviderChangeHistories.length > 0);
    }


    /**
     *
     * Gets the PaymentTransactions for this PaymentProvider.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.paymentProvider.PaymentTransactions.then(paymentProviders => { ... })
     *   or
     *   await this.paymentProvider.paymentProviders
     *
    */
    public get PaymentTransactions(): Promise<PaymentTransactionData[]> {
        if (this._paymentTransactions !== null) {
            return Promise.resolve(this._paymentTransactions);
        }

        if (this._paymentTransactionsPromise !== null) {
            return this._paymentTransactionsPromise;
        }

        // Start the load
        this.loadPaymentTransactions();

        return this._paymentTransactionsPromise!;
    }



    private loadPaymentTransactions(): void {

        this._paymentTransactionsPromise = lastValueFrom(
            PaymentProviderService.Instance.GetPaymentTransactionsForPaymentProvider(this.id)
        )
        .then(PaymentTransactions => {
            this._paymentTransactions = PaymentTransactions ?? [];
            this._paymentTransactionsSubject.next(this._paymentTransactions);
            return this._paymentTransactions;
         })
        .catch(err => {
            this._paymentTransactions = [];
            this._paymentTransactionsSubject.next(this._paymentTransactions);
            throw err;
        })
        .finally(() => {
            this._paymentTransactionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PaymentTransaction. Call after mutations to force refresh.
     */
    public ClearPaymentTransactionsCache(): void {
        this._paymentTransactions = null;
        this._paymentTransactionsPromise = null;
        this._paymentTransactionsSubject.next(this._paymentTransactions);      // Emit to observable
    }

    public get HasPaymentTransactions(): Promise<boolean> {
        return this.PaymentTransactions.then(paymentTransactions => paymentTransactions.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (paymentProvider.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await paymentProvider.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<PaymentProviderData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<PaymentProviderData>> {
        const info = await lastValueFrom(
            PaymentProviderService.Instance.GetPaymentProviderChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this PaymentProviderData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PaymentProviderData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PaymentProviderSubmitData {
        return PaymentProviderService.Instance.ConvertToPaymentProviderSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PaymentProviderService extends SecureEndpointBase {

    private static _instance: PaymentProviderService;
    private listCache: Map<string, Observable<Array<PaymentProviderData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PaymentProviderBasicListData>>>;
    private recordCache: Map<string, Observable<PaymentProviderData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private paymentProviderChangeHistoryService: PaymentProviderChangeHistoryService,
        private paymentTransactionService: PaymentTransactionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PaymentProviderData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PaymentProviderBasicListData>>>();
        this.recordCache = new Map<string, Observable<PaymentProviderData>>();

        PaymentProviderService._instance = this;
    }

    public static get Instance(): PaymentProviderService {
      return PaymentProviderService._instance;
    }


    public ClearListCaches(config: PaymentProviderQueryParameters | null = null) {

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


    public ConvertToPaymentProviderSubmitData(data: PaymentProviderData): PaymentProviderSubmitData {

        let output = new PaymentProviderSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.providerType = data.providerType;
        output.isActive = data.isActive;
        output.apiKeyEncrypted = data.apiKeyEncrypted;
        output.merchantId = data.merchantId;
        output.webhookSecret = data.webhookSecret;
        output.processingFeePercent = data.processingFeePercent;
        output.processingFeeFixed = data.processingFeeFixed;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPaymentProvider(id: bigint | number, includeRelations: boolean = true) : Observable<PaymentProviderData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const paymentProvider$ = this.requestPaymentProvider(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PaymentProvider", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, paymentProvider$);

            return paymentProvider$;
        }

        return this.recordCache.get(configHash) as Observable<PaymentProviderData>;
    }

    private requestPaymentProvider(id: bigint | number, includeRelations: boolean = true) : Observable<PaymentProviderData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PaymentProviderData>(this.baseUrl + 'api/PaymentProvider/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePaymentProvider(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentProvider(id, includeRelations));
            }));
    }

    public GetPaymentProviderList(config: PaymentProviderQueryParameters | any = null) : Observable<Array<PaymentProviderData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const paymentProviderList$ = this.requestPaymentProviderList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PaymentProvider list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, paymentProviderList$);

            return paymentProviderList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PaymentProviderData>>;
    }


    private requestPaymentProviderList(config: PaymentProviderQueryParameters | any) : Observable <Array<PaymentProviderData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PaymentProviderData>>(this.baseUrl + 'api/PaymentProviders', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePaymentProviderList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentProviderList(config));
            }));
    }

    public GetPaymentProvidersRowCount(config: PaymentProviderQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const paymentProvidersRowCount$ = this.requestPaymentProvidersRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PaymentProviders row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, paymentProvidersRowCount$);

            return paymentProvidersRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPaymentProvidersRowCount(config: PaymentProviderQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PaymentProviders/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentProvidersRowCount(config));
            }));
    }

    public GetPaymentProvidersBasicListData(config: PaymentProviderQueryParameters | any = null) : Observable<Array<PaymentProviderBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const paymentProvidersBasicListData$ = this.requestPaymentProvidersBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PaymentProviders basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, paymentProvidersBasicListData$);

            return paymentProvidersBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PaymentProviderBasicListData>>;
    }


    private requestPaymentProvidersBasicListData(config: PaymentProviderQueryParameters | any) : Observable<Array<PaymentProviderBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PaymentProviderBasicListData>>(this.baseUrl + 'api/PaymentProviders/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentProvidersBasicListData(config));
            }));

    }


    public PutPaymentProvider(id: bigint | number, paymentProvider: PaymentProviderSubmitData) : Observable<PaymentProviderData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PaymentProviderData>(this.baseUrl + 'api/PaymentProvider/' + id.toString(), paymentProvider, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentProvider(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPaymentProvider(id, paymentProvider));
            }));
    }


    public PostPaymentProvider(paymentProvider: PaymentProviderSubmitData) : Observable<PaymentProviderData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PaymentProviderData>(this.baseUrl + 'api/PaymentProvider', paymentProvider, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentProvider(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPaymentProvider(paymentProvider));
            }));
    }

  
    public DeletePaymentProvider(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PaymentProvider/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePaymentProvider(id));
            }));
    }

    public RollbackPaymentProvider(id: bigint | number, versionNumber: bigint | number) : Observable<PaymentProviderData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PaymentProviderData>(this.baseUrl + 'api/PaymentProvider/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentProvider(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackPaymentProvider(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a PaymentProvider.
     */
    public GetPaymentProviderChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<PaymentProviderData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PaymentProviderData>>(this.baseUrl + 'api/PaymentProvider/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPaymentProviderChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a PaymentProvider.
     */
    public GetPaymentProviderAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<PaymentProviderData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PaymentProviderData>[]>(this.baseUrl + 'api/PaymentProvider/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPaymentProviderAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a PaymentProvider.
     */
    public GetPaymentProviderVersion(id: bigint | number, version: number): Observable<PaymentProviderData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PaymentProviderData>(this.baseUrl + 'api/PaymentProvider/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePaymentProvider(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPaymentProviderVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a PaymentProvider at a specific point in time.
     */
    public GetPaymentProviderStateAtTime(id: bigint | number, time: string): Observable<PaymentProviderData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PaymentProviderData>(this.baseUrl + 'api/PaymentProvider/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePaymentProvider(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPaymentProviderStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: PaymentProviderQueryParameters | any): string {

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

    public userIsSchedulerPaymentProviderReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerPaymentProviderReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.PaymentProviders
        //
        if (userIsSchedulerPaymentProviderReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerPaymentProviderReader = user.readPermission >= 1;
            } else {
                userIsSchedulerPaymentProviderReader = false;
            }
        }

        return userIsSchedulerPaymentProviderReader;
    }


    public userIsSchedulerPaymentProviderWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerPaymentProviderWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.PaymentProviders
        //
        if (userIsSchedulerPaymentProviderWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerPaymentProviderWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerPaymentProviderWriter = false;
          }      
        }

        return userIsSchedulerPaymentProviderWriter;
    }

    public GetPaymentProviderChangeHistoriesForPaymentProvider(paymentProviderId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PaymentProviderChangeHistoryData[]> {
        return this.paymentProviderChangeHistoryService.GetPaymentProviderChangeHistoryList({
            paymentProviderId: paymentProviderId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPaymentTransactionsForPaymentProvider(paymentProviderId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PaymentTransactionData[]> {
        return this.paymentTransactionService.GetPaymentTransactionList({
            paymentProviderId: paymentProviderId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PaymentProviderData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PaymentProviderData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PaymentProviderTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePaymentProvider(raw: any): PaymentProviderData {
    if (!raw) return raw;

    //
    // Create a PaymentProviderData object instance with correct prototype
    //
    const revived = Object.create(PaymentProviderData.prototype) as PaymentProviderData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._paymentProviderChangeHistories = null;
    (revived as any)._paymentProviderChangeHistoriesPromise = null;
    (revived as any)._paymentProviderChangeHistoriesSubject = new BehaviorSubject<PaymentProviderChangeHistoryData[] | null>(null);

    (revived as any)._paymentTransactions = null;
    (revived as any)._paymentTransactionsPromise = null;
    (revived as any)._paymentTransactionsSubject = new BehaviorSubject<PaymentTransactionData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadPaymentProviderXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).PaymentProviderChangeHistories$ = (revived as any)._paymentProviderChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._paymentProviderChangeHistories === null && (revived as any)._paymentProviderChangeHistoriesPromise === null) {
                (revived as any).loadPaymentProviderChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._paymentProviderChangeHistoriesCount$ = null;


    (revived as any).PaymentTransactions$ = (revived as any)._paymentTransactionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._paymentTransactions === null && (revived as any)._paymentTransactionsPromise === null) {
                (revived as any).loadPaymentTransactions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._paymentTransactionsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PaymentProviderData> | null>(null);

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

  private RevivePaymentProviderList(rawList: any[]): PaymentProviderData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePaymentProvider(raw));
  }

}
