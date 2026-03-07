/*

   GENERATED SERVICE FOR THE PAYMENTTYPECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PaymentTypeChangeHistory table.

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
import { PaymentTypeData } from './payment-type.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PaymentTypeChangeHistoryQueryParameters {
    paymentTypeId: bigint | number | null | undefined = null;
    versionNumber: bigint | number | null | undefined = null;
    timeStamp: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class PaymentTypeChangeHistorySubmitData {
    id!: bigint | number;
    paymentTypeId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
}


export class PaymentTypeChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PaymentTypeChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `paymentTypeChangeHistory.PaymentTypeChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `paymentTypeChangeHistory.PaymentTypeChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="paymentTypeChangeHistory.PaymentTypeChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`paymentTypeChangeHistory.PaymentTypeChangeHistoryChildren` or `await paymentTypeChangeHistory.PaymentTypeChangeHistoryChildren`)
//    - Simply reading `paymentTypeChangeHistory.PaymentTypeChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await paymentTypeChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PaymentTypeChangeHistoryData {
    id!: bigint | number;
    paymentTypeId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
    paymentType: PaymentTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any PaymentTypeChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.paymentTypeChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     paymentTypeChangeHistory[0].Reload().then(x => {
  //        this.paymentTypeChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PaymentTypeChangeHistoryService.Instance.GetPaymentTypeChangeHistory(this.id, includeRelations)
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
     * Updates the state of this PaymentTypeChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PaymentTypeChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PaymentTypeChangeHistorySubmitData {
        return PaymentTypeChangeHistoryService.Instance.ConvertToPaymentTypeChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PaymentTypeChangeHistoryService extends SecureEndpointBase {

    private static _instance: PaymentTypeChangeHistoryService;
    private listCache: Map<string, Observable<Array<PaymentTypeChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PaymentTypeChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<PaymentTypeChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PaymentTypeChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PaymentTypeChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<PaymentTypeChangeHistoryData>>();

        PaymentTypeChangeHistoryService._instance = this;
    }

    public static get Instance(): PaymentTypeChangeHistoryService {
      return PaymentTypeChangeHistoryService._instance;
    }


    public ClearListCaches(config: PaymentTypeChangeHistoryQueryParameters | null = null) {

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


    public ConvertToPaymentTypeChangeHistorySubmitData(data: PaymentTypeChangeHistoryData): PaymentTypeChangeHistorySubmitData {

        let output = new PaymentTypeChangeHistorySubmitData();

        output.id = data.id;
        output.paymentTypeId = data.paymentTypeId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetPaymentTypeChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<PaymentTypeChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const paymentTypeChangeHistory$ = this.requestPaymentTypeChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PaymentTypeChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, paymentTypeChangeHistory$);

            return paymentTypeChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<PaymentTypeChangeHistoryData>;
    }

    private requestPaymentTypeChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<PaymentTypeChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PaymentTypeChangeHistoryData>(this.baseUrl + 'api/PaymentTypeChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePaymentTypeChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentTypeChangeHistory(id, includeRelations));
            }));
    }

    public GetPaymentTypeChangeHistoryList(config: PaymentTypeChangeHistoryQueryParameters | any = null) : Observable<Array<PaymentTypeChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const paymentTypeChangeHistoryList$ = this.requestPaymentTypeChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PaymentTypeChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, paymentTypeChangeHistoryList$);

            return paymentTypeChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PaymentTypeChangeHistoryData>>;
    }


    private requestPaymentTypeChangeHistoryList(config: PaymentTypeChangeHistoryQueryParameters | any) : Observable <Array<PaymentTypeChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PaymentTypeChangeHistoryData>>(this.baseUrl + 'api/PaymentTypeChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePaymentTypeChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentTypeChangeHistoryList(config));
            }));
    }

    public GetPaymentTypeChangeHistoriesRowCount(config: PaymentTypeChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const paymentTypeChangeHistoriesRowCount$ = this.requestPaymentTypeChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PaymentTypeChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, paymentTypeChangeHistoriesRowCount$);

            return paymentTypeChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPaymentTypeChangeHistoriesRowCount(config: PaymentTypeChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PaymentTypeChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentTypeChangeHistoriesRowCount(config));
            }));
    }

    public GetPaymentTypeChangeHistoriesBasicListData(config: PaymentTypeChangeHistoryQueryParameters | any = null) : Observable<Array<PaymentTypeChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const paymentTypeChangeHistoriesBasicListData$ = this.requestPaymentTypeChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PaymentTypeChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, paymentTypeChangeHistoriesBasicListData$);

            return paymentTypeChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PaymentTypeChangeHistoryBasicListData>>;
    }


    private requestPaymentTypeChangeHistoriesBasicListData(config: PaymentTypeChangeHistoryQueryParameters | any) : Observable<Array<PaymentTypeChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PaymentTypeChangeHistoryBasicListData>>(this.baseUrl + 'api/PaymentTypeChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPaymentTypeChangeHistoriesBasicListData(config));
            }));

    }


    public PutPaymentTypeChangeHistory(id: bigint | number, paymentTypeChangeHistory: PaymentTypeChangeHistorySubmitData) : Observable<PaymentTypeChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PaymentTypeChangeHistoryData>(this.baseUrl + 'api/PaymentTypeChangeHistory/' + id.toString(), paymentTypeChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentTypeChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPaymentTypeChangeHistory(id, paymentTypeChangeHistory));
            }));
    }


    public PostPaymentTypeChangeHistory(paymentTypeChangeHistory: PaymentTypeChangeHistorySubmitData) : Observable<PaymentTypeChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PaymentTypeChangeHistoryData>(this.baseUrl + 'api/PaymentTypeChangeHistory', paymentTypeChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePaymentTypeChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPaymentTypeChangeHistory(paymentTypeChangeHistory));
            }));
    }

  
    public DeletePaymentTypeChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PaymentTypeChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePaymentTypeChangeHistory(id));
            }));
    }


    private getConfigHash(config: PaymentTypeChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerPaymentTypeChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerPaymentTypeChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.PaymentTypeChangeHistories
        //
        if (userIsSchedulerPaymentTypeChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerPaymentTypeChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerPaymentTypeChangeHistoryReader = false;
            }
        }

        return userIsSchedulerPaymentTypeChangeHistoryReader;
    }


    public userIsSchedulerPaymentTypeChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerPaymentTypeChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.PaymentTypeChangeHistories
        //
        if (userIsSchedulerPaymentTypeChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerPaymentTypeChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerPaymentTypeChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerPaymentTypeChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full PaymentTypeChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PaymentTypeChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PaymentTypeChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePaymentTypeChangeHistory(raw: any): PaymentTypeChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a PaymentTypeChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(PaymentTypeChangeHistoryData.prototype) as PaymentTypeChangeHistoryData;

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
    // 2. But private methods (loadPaymentTypeChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private RevivePaymentTypeChangeHistoryList(rawList: any[]): PaymentTypeChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePaymentTypeChangeHistory(raw));
  }

}
