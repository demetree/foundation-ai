/*

   GENERATED SERVICE FOR THE BRICKSETTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickSetTransaction table.

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
export class BrickSetTransactionQueryParameters {
    transactionDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    direction: string | null | undefined = null;
    methodName: string | null | undefined = null;
    requestSummary: string | null | undefined = null;
    success: boolean | null | undefined = null;
    errorMessage: string | null | undefined = null;
    triggeredBy: string | null | undefined = null;
    recordCount: bigint | number | null | undefined = null;
    apiCallsRemaining: bigint | number | null | undefined = null;
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
export class BrickSetTransactionSubmitData {
    id!: bigint | number;
    transactionDate: string | null = null;     // ISO 8601 (full datetime)
    direction!: string;
    methodName!: string;
    requestSummary: string | null = null;
    success!: boolean;
    errorMessage: string | null = null;
    triggeredBy!: string;
    recordCount: bigint | number | null = null;
    apiCallsRemaining: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BrickSetTransactionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickSetTransactionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickSetTransaction.BrickSetTransactionChildren$` — use with `| async` in templates
//        • Promise:    `brickSetTransaction.BrickSetTransactionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickSetTransaction.BrickSetTransactionChildren$ | async"`), or
//        • Access the promise getter (`brickSetTransaction.BrickSetTransactionChildren` or `await brickSetTransaction.BrickSetTransactionChildren`)
//    - Simply reading `brickSetTransaction.BrickSetTransactionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickSetTransaction.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickSetTransactionData {
    id!: bigint | number;
    transactionDate!: string | null;   // ISO 8601 (full datetime)
    direction!: string;
    methodName!: string;
    requestSummary!: string | null;
    success!: boolean;
    errorMessage!: string | null;
    triggeredBy!: string;
    recordCount!: bigint | number;
    apiCallsRemaining!: bigint | number;
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
  // Promise based reload method to allow rebuilding of any BrickSetTransactionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickSetTransaction.Reload();
  //
  //  Non Async:
  //
  //     brickSetTransaction[0].Reload().then(x => {
  //        this.brickSetTransaction = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickSetTransactionService.Instance.GetBrickSetTransaction(this.id, includeRelations)
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
     * Updates the state of this BrickSetTransactionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickSetTransactionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickSetTransactionSubmitData {
        return BrickSetTransactionService.Instance.ConvertToBrickSetTransactionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickSetTransactionService extends SecureEndpointBase {

    private static _instance: BrickSetTransactionService;
    private listCache: Map<string, Observable<Array<BrickSetTransactionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickSetTransactionBasicListData>>>;
    private recordCache: Map<string, Observable<BrickSetTransactionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickSetTransactionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickSetTransactionBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickSetTransactionData>>();

        BrickSetTransactionService._instance = this;
    }

    public static get Instance(): BrickSetTransactionService {
      return BrickSetTransactionService._instance;
    }


    public ClearListCaches(config: BrickSetTransactionQueryParameters | null = null) {

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


    public ConvertToBrickSetTransactionSubmitData(data: BrickSetTransactionData): BrickSetTransactionSubmitData {

        let output = new BrickSetTransactionSubmitData();

        output.id = data.id;
        output.transactionDate = data.transactionDate;
        output.direction = data.direction;
        output.methodName = data.methodName;
        output.requestSummary = data.requestSummary;
        output.success = data.success;
        output.errorMessage = data.errorMessage;
        output.triggeredBy = data.triggeredBy;
        output.recordCount = data.recordCount;
        output.apiCallsRemaining = data.apiCallsRemaining;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBrickSetTransaction(id: bigint | number, includeRelations: boolean = true) : Observable<BrickSetTransactionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickSetTransaction$ = this.requestBrickSetTransaction(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickSetTransaction", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickSetTransaction$);

            return brickSetTransaction$;
        }

        return this.recordCache.get(configHash) as Observable<BrickSetTransactionData>;
    }

    private requestBrickSetTransaction(id: bigint | number, includeRelations: boolean = true) : Observable<BrickSetTransactionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickSetTransactionData>(this.baseUrl + 'api/BrickSetTransaction/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickSetTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickSetTransaction(id, includeRelations));
            }));
    }

    public GetBrickSetTransactionList(config: BrickSetTransactionQueryParameters | any = null) : Observable<Array<BrickSetTransactionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickSetTransactionList$ = this.requestBrickSetTransactionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickSetTransaction list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickSetTransactionList$);

            return brickSetTransactionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickSetTransactionData>>;
    }


    private requestBrickSetTransactionList(config: BrickSetTransactionQueryParameters | any) : Observable <Array<BrickSetTransactionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickSetTransactionData>>(this.baseUrl + 'api/BrickSetTransactions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickSetTransactionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickSetTransactionList(config));
            }));
    }

    public GetBrickSetTransactionsRowCount(config: BrickSetTransactionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickSetTransactionsRowCount$ = this.requestBrickSetTransactionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickSetTransactions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickSetTransactionsRowCount$);

            return brickSetTransactionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickSetTransactionsRowCount(config: BrickSetTransactionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickSetTransactions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickSetTransactionsRowCount(config));
            }));
    }

    public GetBrickSetTransactionsBasicListData(config: BrickSetTransactionQueryParameters | any = null) : Observable<Array<BrickSetTransactionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickSetTransactionsBasicListData$ = this.requestBrickSetTransactionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickSetTransactions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickSetTransactionsBasicListData$);

            return brickSetTransactionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickSetTransactionBasicListData>>;
    }


    private requestBrickSetTransactionsBasicListData(config: BrickSetTransactionQueryParameters | any) : Observable<Array<BrickSetTransactionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickSetTransactionBasicListData>>(this.baseUrl + 'api/BrickSetTransactions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickSetTransactionsBasicListData(config));
            }));

    }


    public PutBrickSetTransaction(id: bigint | number, brickSetTransaction: BrickSetTransactionSubmitData) : Observable<BrickSetTransactionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickSetTransactionData>(this.baseUrl + 'api/BrickSetTransaction/' + id.toString(), brickSetTransaction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickSetTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickSetTransaction(id, brickSetTransaction));
            }));
    }


    public PostBrickSetTransaction(brickSetTransaction: BrickSetTransactionSubmitData) : Observable<BrickSetTransactionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickSetTransactionData>(this.baseUrl + 'api/BrickSetTransaction', brickSetTransaction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickSetTransaction(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickSetTransaction(brickSetTransaction));
            }));
    }

  
    public DeleteBrickSetTransaction(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickSetTransaction/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickSetTransaction(id));
            }));
    }


    private getConfigHash(config: BrickSetTransactionQueryParameters | any): string {

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

    public userIsBMCBrickSetTransactionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickSetTransactionReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickSetTransactions
        //
        if (userIsBMCBrickSetTransactionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickSetTransactionReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickSetTransactionReader = false;
            }
        }

        return userIsBMCBrickSetTransactionReader;
    }


    public userIsBMCBrickSetTransactionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickSetTransactionWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickSetTransactions
        //
        if (userIsBMCBrickSetTransactionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickSetTransactionWriter = user.writePermission >= 255;
          } else {
            userIsBMCBrickSetTransactionWriter = false;
          }      
        }

        return userIsBMCBrickSetTransactionWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BrickSetTransactionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickSetTransactionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickSetTransactionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickSetTransaction(raw: any): BrickSetTransactionData {
    if (!raw) return raw;

    //
    // Create a BrickSetTransactionData object instance with correct prototype
    //
    const revived = Object.create(BrickSetTransactionData.prototype) as BrickSetTransactionData;

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
    // 2. But private methods (loadBrickSetTransactionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBrickSetTransactionList(rawList: any[]): BrickSetTransactionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickSetTransaction(raw));
  }

}
