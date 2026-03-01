/*

   GENERATED SERVICE FOR THE REBRICKABLETRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the RebrickableTransaction table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class RebrickableTransactionQueryParameters {
    transactionDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    direction: string | null | undefined = null;
    httpMethod: string | null | undefined = null;
    endpoint: string | null | undefined = null;
    requestSummary: string | null | undefined = null;
    responseStatusCode: bigint | number | null | undefined = null;
    responseBody: string | null | undefined = null;
    success: boolean | null | undefined = null;
    errorMessage: string | null | undefined = null;
    triggeredBy: string | null | undefined = null;
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
export class RebrickableTransactionSubmitData {
    id!: bigint | number;
    transactionDate: string | null = null;     // ISO 8601 (full datetime)
    direction!: string;
    httpMethod!: string;
    endpoint!: string;
    requestSummary: string | null = null;
    responseStatusCode: bigint | number | null = null;
    responseBody: string | null = null;
    success!: boolean;
    errorMessage: string | null = null;
    triggeredBy!: string;
    active!: boolean;
    deleted!: boolean;
}


export class RebrickableTransactionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. RebrickableTransactionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `rebrickableTransaction.RebrickableTransactionChildren$` — use with `| async` in templates
//        • Promise:    `rebrickableTransaction.RebrickableTransactionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="rebrickableTransaction.RebrickableTransactionChildren$ | async"`), or
//        • Access the promise getter (`rebrickableTransaction.RebrickableTransactionChildren` or `await rebrickableTransaction.RebrickableTransactionChildren`)
//    - Simply reading `rebrickableTransaction.RebrickableTransactionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await rebrickableTransaction.Reload()` to refresh the entire object and clear all lazy caches.
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
export class RebrickableTransactionData {
    id!: bigint | number;
    transactionDate!: string | null;   // ISO 8601 (full datetime)
    direction!: string;
    httpMethod!: string;
    endpoint!: string;
    requestSummary!: string | null;
    responseStatusCode!: bigint | number;
    responseBody!: string | null;
    success!: boolean;
    errorMessage!: string | null;
    triggeredBy!: string;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

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
  // Promise based reload method to allow rebuilding of any RebrickableTransactionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.rebrickableTransaction.Reload();
  //
  //  Non Async:
  //
  //     rebrickableTransaction[0].Reload().then(x => {
  //        this.rebrickableTransaction = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      RebrickableTransactionService.Instance.GetRebrickableTransaction(this.id, includeRelations)
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
     * Updates the state of this RebrickableTransactionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this RebrickableTransactionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): RebrickableTransactionSubmitData {
        return RebrickableTransactionService.Instance.ConvertToRebrickableTransactionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class RebrickableTransactionService extends SecureEndpointBase {

    private static _instance: RebrickableTransactionService;
    private listCache: Map<string, Observable<Array<RebrickableTransactionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<RebrickableTransactionBasicListData>>>;
    private recordCache: Map<string, Observable<RebrickableTransactionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<RebrickableTransactionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<RebrickableTransactionBasicListData>>>();
        this.recordCache = new Map<string, Observable<RebrickableTransactionData>>();

        RebrickableTransactionService._instance = this;
    }

    public static get Instance(): RebrickableTransactionService {
      return RebrickableTransactionService._instance;
    }


    public ClearListCaches(config: RebrickableTransactionQueryParameters | null = null) {

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


    public ConvertToRebrickableTransactionSubmitData(data: RebrickableTransactionData): RebrickableTransactionSubmitData {

        let output = new RebrickableTransactionSubmitData();

        output.id = data.id;
        output.transactionDate = data.transactionDate;
        output.direction = data.direction;
        output.httpMethod = data.httpMethod;
        output.endpoint = data.endpoint;
        output.requestSummary = data.requestSummary;
        output.responseStatusCode = data.responseStatusCode;
        output.responseBody = data.responseBody;
        output.success = data.success;
        output.errorMessage = data.errorMessage;
        output.triggeredBy = data.triggeredBy;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetRebrickableTransaction(id: bigint | number, includeRelations: boolean = true) : Observable<RebrickableTransactionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const rebrickableTransaction$ = this.requestRebrickableTransaction(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RebrickableTransaction", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, rebrickableTransaction$);

            return rebrickableTransaction$;
        }

        return this.recordCache.get(configHash) as Observable<RebrickableTransactionData>;
    }

    private requestRebrickableTransaction(id: bigint | number, includeRelations: boolean = true) : Observable<RebrickableTransactionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RebrickableTransactionData>(this.baseUrl + 'api/RebrickableTransaction/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveRebrickableTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestRebrickableTransaction(id, includeRelations));
            }));
    }

    public GetRebrickableTransactionList(config: RebrickableTransactionQueryParameters | any = null) : Observable<Array<RebrickableTransactionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const rebrickableTransactionList$ = this.requestRebrickableTransactionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RebrickableTransaction list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, rebrickableTransactionList$);

            return rebrickableTransactionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<RebrickableTransactionData>>;
    }


    private requestRebrickableTransactionList(config: RebrickableTransactionQueryParameters | any) : Observable <Array<RebrickableTransactionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RebrickableTransactionData>>(this.baseUrl + 'api/RebrickableTransactions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveRebrickableTransactionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestRebrickableTransactionList(config));
            }));
    }

    public GetRebrickableTransactionsRowCount(config: RebrickableTransactionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const rebrickableTransactionsRowCount$ = this.requestRebrickableTransactionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RebrickableTransactions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, rebrickableTransactionsRowCount$);

            return rebrickableTransactionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestRebrickableTransactionsRowCount(config: RebrickableTransactionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/RebrickableTransactions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRebrickableTransactionsRowCount(config));
            }));
    }

    public GetRebrickableTransactionsBasicListData(config: RebrickableTransactionQueryParameters | any = null) : Observable<Array<RebrickableTransactionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const rebrickableTransactionsBasicListData$ = this.requestRebrickableTransactionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RebrickableTransactions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, rebrickableTransactionsBasicListData$);

            return rebrickableTransactionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<RebrickableTransactionBasicListData>>;
    }


    private requestRebrickableTransactionsBasicListData(config: RebrickableTransactionQueryParameters | any) : Observable<Array<RebrickableTransactionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RebrickableTransactionBasicListData>>(this.baseUrl + 'api/RebrickableTransactions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRebrickableTransactionsBasicListData(config));
            }));

    }


    public PutRebrickableTransaction(id: bigint | number, rebrickableTransaction: RebrickableTransactionSubmitData) : Observable<RebrickableTransactionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RebrickableTransactionData>(this.baseUrl + 'api/RebrickableTransaction/' + id.toString(), rebrickableTransaction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRebrickableTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutRebrickableTransaction(id, rebrickableTransaction));
            }));
    }


    public PostRebrickableTransaction(rebrickableTransaction: RebrickableTransactionSubmitData) : Observable<RebrickableTransactionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<RebrickableTransactionData>(this.baseUrl + 'api/RebrickableTransaction', rebrickableTransaction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRebrickableTransaction(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostRebrickableTransaction(rebrickableTransaction));
            }));
    }

  
    public DeleteRebrickableTransaction(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/RebrickableTransaction/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteRebrickableTransaction(id));
            }));
    }


    private getConfigHash(config: RebrickableTransactionQueryParameters | any): string {

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

    public userIsBMCRebrickableTransactionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCRebrickableTransactionReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.RebrickableTransactions
        //
        if (userIsBMCRebrickableTransactionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCRebrickableTransactionReader = user.readPermission >= 1;
            } else {
                userIsBMCRebrickableTransactionReader = false;
            }
        }

        return userIsBMCRebrickableTransactionReader;
    }


    public userIsBMCRebrickableTransactionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCRebrickableTransactionWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.RebrickableTransactions
        //
        if (userIsBMCRebrickableTransactionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCRebrickableTransactionWriter = user.writePermission >= 10;
          } else {
            userIsBMCRebrickableTransactionWriter = false;
          }      
        }

        return userIsBMCRebrickableTransactionWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full RebrickableTransactionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the RebrickableTransactionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when RebrickableTransactionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveRebrickableTransaction(raw: any): RebrickableTransactionData {
    if (!raw) return raw;

    //
    // Create a RebrickableTransactionData object instance with correct prototype
    //
    const revived = Object.create(RebrickableTransactionData.prototype) as RebrickableTransactionData;

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
    // 2. But private methods (loadRebrickableTransactionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveRebrickableTransactionList(rawList: any[]): RebrickableTransactionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveRebrickableTransaction(raw));
  }

}
