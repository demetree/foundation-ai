/*

   GENERATED SERVICE FOR THE TAXCODE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the TaxCode table.

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
import { ChargeTypeService, ChargeTypeData } from './charge-type.service';
import { EventChargeService, EventChargeData } from './event-charge.service';
import { FinancialTransactionService, FinancialTransactionData } from './financial-transaction.service';
import { InvoiceService, InvoiceData } from './invoice.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class TaxCodeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    code: string | null | undefined = null;
    rate: number | null | undefined = null;
    isDefault: boolean | null | undefined = null;
    isExempt: boolean | null | undefined = null;
    externalTaxCodeId: string | null | undefined = null;
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
export class TaxCodeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    code!: string;
    rate!: number;
    isDefault!: boolean;
    isExempt!: boolean;
    externalTaxCodeId: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class TaxCodeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TaxCodeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `taxCode.TaxCodeChildren$` — use with `| async` in templates
//        • Promise:    `taxCode.TaxCodeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="taxCode.TaxCodeChildren$ | async"`), or
//        • Access the promise getter (`taxCode.TaxCodeChildren` or `await taxCode.TaxCodeChildren`)
//    - Simply reading `taxCode.TaxCodeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await taxCode.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TaxCodeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    code!: string;
    rate!: number;
    isDefault!: boolean;
    isExempt!: boolean;
    externalTaxCodeId!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _chargeTypes: ChargeTypeData[] | null = null;
    private _chargeTypesPromise: Promise<ChargeTypeData[]> | null  = null;
    private _chargeTypesSubject = new BehaviorSubject<ChargeTypeData[] | null>(null);

                
    private _eventCharges: EventChargeData[] | null = null;
    private _eventChargesPromise: Promise<EventChargeData[]> | null  = null;
    private _eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);

                
    private _financialTransactions: FinancialTransactionData[] | null = null;
    private _financialTransactionsPromise: Promise<FinancialTransactionData[]> | null  = null;
    private _financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

                
    private _invoices: InvoiceData[] | null = null;
    private _invoicesPromise: Promise<InvoiceData[]> | null  = null;
    private _invoicesSubject = new BehaviorSubject<InvoiceData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ChargeTypes$ = this._chargeTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._chargeTypes === null && this._chargeTypesPromise === null) {
            this.loadChargeTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _chargeTypesCount$: Observable<bigint | number> | null = null;
    public get ChargeTypesCount$(): Observable<bigint | number> {
        if (this._chargeTypesCount$ === null) {
            this._chargeTypesCount$ = ChargeTypeService.Instance.GetChargeTypesRowCount({taxCodeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._chargeTypesCount$;
    }



    public EventCharges$ = this._eventChargesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventCharges === null && this._eventChargesPromise === null) {
            this.loadEventCharges(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _eventChargesCount$: Observable<bigint | number> | null = null;
    public get EventChargesCount$(): Observable<bigint | number> {
        if (this._eventChargesCount$ === null) {
            this._eventChargesCount$ = EventChargeService.Instance.GetEventChargesRowCount({taxCodeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._eventChargesCount$;
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
            this._financialTransactionsCount$ = FinancialTransactionService.Instance.GetFinancialTransactionsRowCount({taxCodeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._financialTransactionsCount$;
    }



    public Invoices$ = this._invoicesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._invoices === null && this._invoicesPromise === null) {
            this.loadInvoices(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _invoicesCount$: Observable<bigint | number> | null = null;
    public get InvoicesCount$(): Observable<bigint | number> {
        if (this._invoicesCount$ === null) {
            this._invoicesCount$ = InvoiceService.Instance.GetInvoicesRowCount({taxCodeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._invoicesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any TaxCodeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.taxCode.Reload();
  //
  //  Non Async:
  //
  //     taxCode[0].Reload().then(x => {
  //        this.taxCode = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TaxCodeService.Instance.GetTaxCode(this.id, includeRelations)
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
     this._chargeTypes = null;
     this._chargeTypesPromise = null;
     this._chargeTypesSubject.next(null);
     this._chargeTypesCount$ = null;

     this._eventCharges = null;
     this._eventChargesPromise = null;
     this._eventChargesSubject.next(null);
     this._eventChargesCount$ = null;

     this._financialTransactions = null;
     this._financialTransactionsPromise = null;
     this._financialTransactionsSubject.next(null);
     this._financialTransactionsCount$ = null;

     this._invoices = null;
     this._invoicesPromise = null;
     this._invoicesSubject.next(null);
     this._invoicesCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ChargeTypes for this TaxCode.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.taxCode.ChargeTypes.then(taxCodes => { ... })
     *   or
     *   await this.taxCode.taxCodes
     *
    */
    public get ChargeTypes(): Promise<ChargeTypeData[]> {
        if (this._chargeTypes !== null) {
            return Promise.resolve(this._chargeTypes);
        }

        if (this._chargeTypesPromise !== null) {
            return this._chargeTypesPromise;
        }

        // Start the load
        this.loadChargeTypes();

        return this._chargeTypesPromise!;
    }



    private loadChargeTypes(): void {

        this._chargeTypesPromise = lastValueFrom(
            TaxCodeService.Instance.GetChargeTypesForTaxCode(this.id)
        )
        .then(ChargeTypes => {
            this._chargeTypes = ChargeTypes ?? [];
            this._chargeTypesSubject.next(this._chargeTypes);
            return this._chargeTypes;
         })
        .catch(err => {
            this._chargeTypes = [];
            this._chargeTypesSubject.next(this._chargeTypes);
            throw err;
        })
        .finally(() => {
            this._chargeTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ChargeType. Call after mutations to force refresh.
     */
    public ClearChargeTypesCache(): void {
        this._chargeTypes = null;
        this._chargeTypesPromise = null;
        this._chargeTypesSubject.next(this._chargeTypes);      // Emit to observable
    }

    public get HasChargeTypes(): Promise<boolean> {
        return this.ChargeTypes.then(chargeTypes => chargeTypes.length > 0);
    }


    /**
     *
     * Gets the EventCharges for this TaxCode.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.taxCode.EventCharges.then(taxCodes => { ... })
     *   or
     *   await this.taxCode.taxCodes
     *
    */
    public get EventCharges(): Promise<EventChargeData[]> {
        if (this._eventCharges !== null) {
            return Promise.resolve(this._eventCharges);
        }

        if (this._eventChargesPromise !== null) {
            return this._eventChargesPromise;
        }

        // Start the load
        this.loadEventCharges();

        return this._eventChargesPromise!;
    }



    private loadEventCharges(): void {

        this._eventChargesPromise = lastValueFrom(
            TaxCodeService.Instance.GetEventChargesForTaxCode(this.id)
        )
        .then(EventCharges => {
            this._eventCharges = EventCharges ?? [];
            this._eventChargesSubject.next(this._eventCharges);
            return this._eventCharges;
         })
        .catch(err => {
            this._eventCharges = [];
            this._eventChargesSubject.next(this._eventCharges);
            throw err;
        })
        .finally(() => {
            this._eventChargesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventCharge. Call after mutations to force refresh.
     */
    public ClearEventChargesCache(): void {
        this._eventCharges = null;
        this._eventChargesPromise = null;
        this._eventChargesSubject.next(this._eventCharges);      // Emit to observable
    }

    public get HasEventCharges(): Promise<boolean> {
        return this.EventCharges.then(eventCharges => eventCharges.length > 0);
    }


    /**
     *
     * Gets the FinancialTransactions for this TaxCode.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.taxCode.FinancialTransactions.then(taxCodes => { ... })
     *   or
     *   await this.taxCode.taxCodes
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
            TaxCodeService.Instance.GetFinancialTransactionsForTaxCode(this.id)
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
     * Gets the Invoices for this TaxCode.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.taxCode.Invoices.then(taxCodes => { ... })
     *   or
     *   await this.taxCode.taxCodes
     *
    */
    public get Invoices(): Promise<InvoiceData[]> {
        if (this._invoices !== null) {
            return Promise.resolve(this._invoices);
        }

        if (this._invoicesPromise !== null) {
            return this._invoicesPromise;
        }

        // Start the load
        this.loadInvoices();

        return this._invoicesPromise!;
    }



    private loadInvoices(): void {

        this._invoicesPromise = lastValueFrom(
            TaxCodeService.Instance.GetInvoicesForTaxCode(this.id)
        )
        .then(Invoices => {
            this._invoices = Invoices ?? [];
            this._invoicesSubject.next(this._invoices);
            return this._invoices;
         })
        .catch(err => {
            this._invoices = [];
            this._invoicesSubject.next(this._invoices);
            throw err;
        })
        .finally(() => {
            this._invoicesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Invoice. Call after mutations to force refresh.
     */
    public ClearInvoicesCache(): void {
        this._invoices = null;
        this._invoicesPromise = null;
        this._invoicesSubject.next(this._invoices);      // Emit to observable
    }

    public get HasInvoices(): Promise<boolean> {
        return this.Invoices.then(invoices => invoices.length > 0);
    }




    /**
     * Updates the state of this TaxCodeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TaxCodeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TaxCodeSubmitData {
        return TaxCodeService.Instance.ConvertToTaxCodeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TaxCodeService extends SecureEndpointBase {

    private static _instance: TaxCodeService;
    private listCache: Map<string, Observable<Array<TaxCodeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TaxCodeBasicListData>>>;
    private recordCache: Map<string, Observable<TaxCodeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private chargeTypeService: ChargeTypeService,
        private eventChargeService: EventChargeService,
        private financialTransactionService: FinancialTransactionService,
        private invoiceService: InvoiceService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TaxCodeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TaxCodeBasicListData>>>();
        this.recordCache = new Map<string, Observable<TaxCodeData>>();

        TaxCodeService._instance = this;
    }

    public static get Instance(): TaxCodeService {
      return TaxCodeService._instance;
    }


    public ClearListCaches(config: TaxCodeQueryParameters | null = null) {

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


    public ConvertToTaxCodeSubmitData(data: TaxCodeData): TaxCodeSubmitData {

        let output = new TaxCodeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.code = data.code;
        output.rate = data.rate;
        output.isDefault = data.isDefault;
        output.isExempt = data.isExempt;
        output.externalTaxCodeId = data.externalTaxCodeId;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetTaxCode(id: bigint | number, includeRelations: boolean = true) : Observable<TaxCodeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const taxCode$ = this.requestTaxCode(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TaxCode", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, taxCode$);

            return taxCode$;
        }

        return this.recordCache.get(configHash) as Observable<TaxCodeData>;
    }

    private requestTaxCode(id: bigint | number, includeRelations: boolean = true) : Observable<TaxCodeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TaxCodeData>(this.baseUrl + 'api/TaxCode/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTaxCode(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTaxCode(id, includeRelations));
            }));
    }

    public GetTaxCodeList(config: TaxCodeQueryParameters | any = null) : Observable<Array<TaxCodeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const taxCodeList$ = this.requestTaxCodeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TaxCode list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, taxCodeList$);

            return taxCodeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TaxCodeData>>;
    }


    private requestTaxCodeList(config: TaxCodeQueryParameters | any) : Observable <Array<TaxCodeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TaxCodeData>>(this.baseUrl + 'api/TaxCodes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTaxCodeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTaxCodeList(config));
            }));
    }

    public GetTaxCodesRowCount(config: TaxCodeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const taxCodesRowCount$ = this.requestTaxCodesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TaxCodes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, taxCodesRowCount$);

            return taxCodesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTaxCodesRowCount(config: TaxCodeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/TaxCodes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTaxCodesRowCount(config));
            }));
    }

    public GetTaxCodesBasicListData(config: TaxCodeQueryParameters | any = null) : Observable<Array<TaxCodeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const taxCodesBasicListData$ = this.requestTaxCodesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TaxCodes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, taxCodesBasicListData$);

            return taxCodesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TaxCodeBasicListData>>;
    }


    private requestTaxCodesBasicListData(config: TaxCodeQueryParameters | any) : Observable<Array<TaxCodeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TaxCodeBasicListData>>(this.baseUrl + 'api/TaxCodes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTaxCodesBasicListData(config));
            }));

    }


    public PutTaxCode(id: bigint | number, taxCode: TaxCodeSubmitData) : Observable<TaxCodeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TaxCodeData>(this.baseUrl + 'api/TaxCode/' + id.toString(), taxCode, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTaxCode(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTaxCode(id, taxCode));
            }));
    }


    public PostTaxCode(taxCode: TaxCodeSubmitData) : Observable<TaxCodeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TaxCodeData>(this.baseUrl + 'api/TaxCode', taxCode, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTaxCode(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTaxCode(taxCode));
            }));
    }

  
    public DeleteTaxCode(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/TaxCode/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTaxCode(id));
            }));
    }


    private getConfigHash(config: TaxCodeQueryParameters | any): string {

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

    public userIsSchedulerTaxCodeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerTaxCodeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.TaxCodes
        //
        if (userIsSchedulerTaxCodeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerTaxCodeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerTaxCodeReader = false;
            }
        }

        return userIsSchedulerTaxCodeReader;
    }


    public userIsSchedulerTaxCodeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerTaxCodeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.TaxCodes
        //
        if (userIsSchedulerTaxCodeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerTaxCodeWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerTaxCodeWriter = false;
          }      
        }

        return userIsSchedulerTaxCodeWriter;
    }

    public GetChargeTypesForTaxCode(taxCodeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ChargeTypeData[]> {
        return this.chargeTypeService.GetChargeTypeList({
            taxCodeId: taxCodeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventChargesForTaxCode(taxCodeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventChargeData[]> {
        return this.eventChargeService.GetEventChargeList({
            taxCodeId: taxCodeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetFinancialTransactionsForTaxCode(taxCodeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FinancialTransactionData[]> {
        return this.financialTransactionService.GetFinancialTransactionList({
            taxCodeId: taxCodeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetInvoicesForTaxCode(taxCodeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<InvoiceData[]> {
        return this.invoiceService.GetInvoiceList({
            taxCodeId: taxCodeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full TaxCodeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TaxCodeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TaxCodeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTaxCode(raw: any): TaxCodeData {
    if (!raw) return raw;

    //
    // Create a TaxCodeData object instance with correct prototype
    //
    const revived = Object.create(TaxCodeData.prototype) as TaxCodeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._chargeTypes = null;
    (revived as any)._chargeTypesPromise = null;
    (revived as any)._chargeTypesSubject = new BehaviorSubject<ChargeTypeData[] | null>(null);

    (revived as any)._eventCharges = null;
    (revived as any)._eventChargesPromise = null;
    (revived as any)._eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);

    (revived as any)._financialTransactions = null;
    (revived as any)._financialTransactionsPromise = null;
    (revived as any)._financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

    (revived as any)._invoices = null;
    (revived as any)._invoicesPromise = null;
    (revived as any)._invoicesSubject = new BehaviorSubject<InvoiceData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadTaxCodeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ChargeTypes$ = (revived as any)._chargeTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._chargeTypes === null && (revived as any)._chargeTypesPromise === null) {
                (revived as any).loadChargeTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._chargeTypesCount$ = null;


    (revived as any).EventCharges$ = (revived as any)._eventChargesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventCharges === null && (revived as any)._eventChargesPromise === null) {
                (revived as any).loadEventCharges();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._eventChargesCount$ = null;


    (revived as any).FinancialTransactions$ = (revived as any)._financialTransactionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._financialTransactions === null && (revived as any)._financialTransactionsPromise === null) {
                (revived as any).loadFinancialTransactions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._financialTransactionsCount$ = null;


    (revived as any).Invoices$ = (revived as any)._invoicesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._invoices === null && (revived as any)._invoicesPromise === null) {
                (revived as any).loadInvoices();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._invoicesCount$ = null;



    return revived;
  }

  private ReviveTaxCodeList(rawList: any[]): TaxCodeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTaxCode(raw));
  }

}
