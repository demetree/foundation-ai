/*

   GENERATED SERVICE FOR THE PAYMENTMETHOD TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PaymentMethod table.

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
export class PaymentMethodQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    isElectronic: boolean | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class PaymentMethodSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isElectronic!: boolean;
    sequence: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class PaymentMethodBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PaymentMethodChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `paymentMethod.PaymentMethodChildren$` — use with `| async` in templates
//        • Promise:    `paymentMethod.PaymentMethodChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="paymentMethod.PaymentMethodChildren$ | async"`), or
//        • Access the promise getter (`paymentMethod.PaymentMethodChildren` or `await paymentMethod.PaymentMethodChildren`)
//    - Simply reading `paymentMethod.PaymentMethodChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await paymentMethod.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PaymentMethodData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isElectronic!: boolean;
    sequence!: bigint | number;
    color!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _paymentTransactions: PaymentTransactionData[] | null = null;
    private _paymentTransactionsPromise: Promise<PaymentTransactionData[]> | null  = null;
    private _paymentTransactionsSubject = new BehaviorSubject<PaymentTransactionData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
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
            this._paymentTransactionsCount$ = PaymentTransactionService.Instance.GetPaymentTransactionsRowCount({paymentMethodId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._paymentTransactionsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PaymentMethodData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.paymentMethod.Reload();
  //
  //  Non Async:
  //
  //     paymentMethod[0].Reload().then(x => {
  //        this.paymentMethod = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PaymentMethodService.Instance.GetPaymentMethod(this.id, includeRelations)
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
     this._paymentTransactions = null;
     this._paymentTransactionsPromise = null;
     this._paymentTransactionsSubject.next(null);
     this._paymentTransactionsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the PaymentTransactions for this PaymentMethod.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.paymentMethod.PaymentTransactions.then(paymentMethods => { ... })
     *   or
     *   await this.paymentMethod.paymentMethods
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
            PaymentMethodService.Instance.GetPaymentTransactionsForPaymentMethod(this.id)
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




    /**
     * Updates the state of this PaymentMethodData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PaymentMethodData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PaymentMethodSubmitData {
        return PaymentMethodService.Instance.ConvertToPaymentMethodSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PaymentMethodService extends SecureEndpointBase {

    private static _instance: PaymentMethodService;
    private listCache: Map<string, Observable<Array<PaymentMethodData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PaymentMethodBasicListData>>>;
    private recordCache: Map<string, Observable<PaymentMethodData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private paymentTransactionService: PaymentTransactionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PaymentMethodData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PaymentMethodBasicListData>>>();
        this.recordCache = new Map<string, Observable<PaymentMethodData>>();

        PaymentMethodService._instance = this;
    }

    public static get Instance(): PaymentMethodService {
      return PaymentMethodService._instance;
    }


    public ClearListCaches(config: PaymentMethodQueryParameters | null = null) {

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


    public ConvertToPaymentMethodSubmitData(data: PaymentMethodData): PaymentMethodSubmitData {

        let output = new PaymentMethodSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.isElectronic = data.isElectronic;
        output.sequence = data.sequence;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPaymentMethod(id: bigint | number, includeRelations: boolean = true) : Observable<PaymentMethodData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const paymentMethod$ = this.requestPaymentMethod(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PaymentMethod", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, paymentMethod$);

            return paymentMethod$;
        }

        return this.recordCache.get(configHash) as Observable<PaymentMethodData>;
    }

    private requestPaymentMethod(id: bigint | number, includeRelations: boolean = true) : Observable<PaymentMethodData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PaymentMethodData>(this.baseUrl + 'api/PaymentMethod/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePaymentMethod(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentMethod(id, includeRelations));
            }));
    }

    public GetPaymentMethodList(config: PaymentMethodQueryParameters | any = null) : Observable<Array<PaymentMethodData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const paymentMethodList$ = this.requestPaymentMethodList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PaymentMethod list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, paymentMethodList$);

            return paymentMethodList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PaymentMethodData>>;
    }


    private requestPaymentMethodList(config: PaymentMethodQueryParameters | any) : Observable <Array<PaymentMethodData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PaymentMethodData>>(this.baseUrl + 'api/PaymentMethods', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePaymentMethodList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentMethodList(config));
            }));
    }

    public GetPaymentMethodsRowCount(config: PaymentMethodQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const paymentMethodsRowCount$ = this.requestPaymentMethodsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PaymentMethods row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, paymentMethodsRowCount$);

            return paymentMethodsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPaymentMethodsRowCount(config: PaymentMethodQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PaymentMethods/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentMethodsRowCount(config));
            }));
    }

    public GetPaymentMethodsBasicListData(config: PaymentMethodQueryParameters | any = null) : Observable<Array<PaymentMethodBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const paymentMethodsBasicListData$ = this.requestPaymentMethodsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PaymentMethods basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, paymentMethodsBasicListData$);

            return paymentMethodsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PaymentMethodBasicListData>>;
    }


    private requestPaymentMethodsBasicListData(config: PaymentMethodQueryParameters | any) : Observable<Array<PaymentMethodBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PaymentMethodBasicListData>>(this.baseUrl + 'api/PaymentMethods/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentMethodsBasicListData(config));
            }));

    }


    public PutPaymentMethod(id: bigint | number, paymentMethod: PaymentMethodSubmitData) : Observable<PaymentMethodData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PaymentMethodData>(this.baseUrl + 'api/PaymentMethod/' + id.toString(), paymentMethod, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentMethod(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPaymentMethod(id, paymentMethod));
            }));
    }


    public PostPaymentMethod(paymentMethod: PaymentMethodSubmitData) : Observable<PaymentMethodData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PaymentMethodData>(this.baseUrl + 'api/PaymentMethod', paymentMethod, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentMethod(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPaymentMethod(paymentMethod));
            }));
    }

  
    public DeletePaymentMethod(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PaymentMethod/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePaymentMethod(id));
            }));
    }


    private getConfigHash(config: PaymentMethodQueryParameters | any): string {

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

    public userIsSchedulerPaymentMethodReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerPaymentMethodReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.PaymentMethods
        //
        if (userIsSchedulerPaymentMethodReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerPaymentMethodReader = user.readPermission >= 1;
            } else {
                userIsSchedulerPaymentMethodReader = false;
            }
        }

        return userIsSchedulerPaymentMethodReader;
    }


    public userIsSchedulerPaymentMethodWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerPaymentMethodWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.PaymentMethods
        //
        if (userIsSchedulerPaymentMethodWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerPaymentMethodWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerPaymentMethodWriter = false;
          }      
        }

        return userIsSchedulerPaymentMethodWriter;
    }

    public GetPaymentTransactionsForPaymentMethod(paymentMethodId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PaymentTransactionData[]> {
        return this.paymentTransactionService.GetPaymentTransactionList({
            paymentMethodId: paymentMethodId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PaymentMethodData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PaymentMethodData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PaymentMethodTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePaymentMethod(raw: any): PaymentMethodData {
    if (!raw) return raw;

    //
    // Create a PaymentMethodData object instance with correct prototype
    //
    const revived = Object.create(PaymentMethodData.prototype) as PaymentMethodData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
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
    // 2. But private methods (loadPaymentMethodXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).PaymentTransactions$ = (revived as any)._paymentTransactionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._paymentTransactions === null && (revived as any)._paymentTransactionsPromise === null) {
                (revived as any).loadPaymentTransactions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._paymentTransactionsCount$ = null;



    return revived;
  }

  private RevivePaymentMethodList(rawList: any[]): PaymentMethodData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePaymentMethod(raw));
  }

}
