/*

   GENERATED SERVICE FOR THE BRICKLINKTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickLinkTransaction table.

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
export class BrickLinkTransactionQueryParameters {
    transactionDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    direction: string | null | undefined = null;
    methodName: string | null | undefined = null;
    requestSummary: string | null | undefined = null;
    success: boolean | null | undefined = null;
    errorMessage: string | null | undefined = null;
    triggeredBy: string | null | undefined = null;
    recordCount: bigint | number | null | undefined = null;
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
export class BrickLinkTransactionSubmitData {
    id!: bigint | number;
    transactionDate: string | null = null;     // ISO 8601 (full datetime)
    direction!: string;
    methodName!: string;
    requestSummary: string | null = null;
    success!: boolean;
    errorMessage: string | null = null;
    triggeredBy!: string;
    recordCount: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BrickLinkTransactionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickLinkTransactionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickLinkTransaction.BrickLinkTransactionChildren$` — use with `| async` in templates
//        • Promise:    `brickLinkTransaction.BrickLinkTransactionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickLinkTransaction.BrickLinkTransactionChildren$ | async"`), or
//        • Access the promise getter (`brickLinkTransaction.BrickLinkTransactionChildren` or `await brickLinkTransaction.BrickLinkTransactionChildren`)
//    - Simply reading `brickLinkTransaction.BrickLinkTransactionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickLinkTransaction.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickLinkTransactionData {
    id!: bigint | number;
    transactionDate!: string | null;   // ISO 8601 (full datetime)
    direction!: string;
    methodName!: string;
    requestSummary!: string | null;
    success!: boolean;
    errorMessage!: string | null;
    triggeredBy!: string;
    recordCount!: bigint | number;
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
  // Promise based reload method to allow rebuilding of any BrickLinkTransactionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickLinkTransaction.Reload();
  //
  //  Non Async:
  //
  //     brickLinkTransaction[0].Reload().then(x => {
  //        this.brickLinkTransaction = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickLinkTransactionService.Instance.GetBrickLinkTransaction(this.id, includeRelations)
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
     * Updates the state of this BrickLinkTransactionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickLinkTransactionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickLinkTransactionSubmitData {
        return BrickLinkTransactionService.Instance.ConvertToBrickLinkTransactionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickLinkTransactionService extends SecureEndpointBase {

    private static _instance: BrickLinkTransactionService;
    private listCache: Map<string, Observable<Array<BrickLinkTransactionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickLinkTransactionBasicListData>>>;
    private recordCache: Map<string, Observable<BrickLinkTransactionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickLinkTransactionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickLinkTransactionBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickLinkTransactionData>>();

        BrickLinkTransactionService._instance = this;
    }

    public static get Instance(): BrickLinkTransactionService {
      return BrickLinkTransactionService._instance;
    }


    public ClearListCaches(config: BrickLinkTransactionQueryParameters | null = null) {

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


    public ConvertToBrickLinkTransactionSubmitData(data: BrickLinkTransactionData): BrickLinkTransactionSubmitData {

        let output = new BrickLinkTransactionSubmitData();

        output.id = data.id;
        output.transactionDate = data.transactionDate;
        output.direction = data.direction;
        output.methodName = data.methodName;
        output.requestSummary = data.requestSummary;
        output.success = data.success;
        output.errorMessage = data.errorMessage;
        output.triggeredBy = data.triggeredBy;
        output.recordCount = data.recordCount;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBrickLinkTransaction(id: bigint | number, includeRelations: boolean = true) : Observable<BrickLinkTransactionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickLinkTransaction$ = this.requestBrickLinkTransaction(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickLinkTransaction", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickLinkTransaction$);

            return brickLinkTransaction$;
        }

        return this.recordCache.get(configHash) as Observable<BrickLinkTransactionData>;
    }

    private requestBrickLinkTransaction(id: bigint | number, includeRelations: boolean = true) : Observable<BrickLinkTransactionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickLinkTransactionData>(this.baseUrl + 'api/BrickLinkTransaction/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickLinkTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickLinkTransaction(id, includeRelations));
            }));
    }

    public GetBrickLinkTransactionList(config: BrickLinkTransactionQueryParameters | any = null) : Observable<Array<BrickLinkTransactionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickLinkTransactionList$ = this.requestBrickLinkTransactionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickLinkTransaction list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickLinkTransactionList$);

            return brickLinkTransactionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickLinkTransactionData>>;
    }


    private requestBrickLinkTransactionList(config: BrickLinkTransactionQueryParameters | any) : Observable <Array<BrickLinkTransactionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickLinkTransactionData>>(this.baseUrl + 'api/BrickLinkTransactions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickLinkTransactionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickLinkTransactionList(config));
            }));
    }

    public GetBrickLinkTransactionsRowCount(config: BrickLinkTransactionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickLinkTransactionsRowCount$ = this.requestBrickLinkTransactionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickLinkTransactions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickLinkTransactionsRowCount$);

            return brickLinkTransactionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickLinkTransactionsRowCount(config: BrickLinkTransactionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickLinkTransactions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickLinkTransactionsRowCount(config));
            }));
    }

    public GetBrickLinkTransactionsBasicListData(config: BrickLinkTransactionQueryParameters | any = null) : Observable<Array<BrickLinkTransactionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickLinkTransactionsBasicListData$ = this.requestBrickLinkTransactionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickLinkTransactions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickLinkTransactionsBasicListData$);

            return brickLinkTransactionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickLinkTransactionBasicListData>>;
    }


    private requestBrickLinkTransactionsBasicListData(config: BrickLinkTransactionQueryParameters | any) : Observable<Array<BrickLinkTransactionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickLinkTransactionBasicListData>>(this.baseUrl + 'api/BrickLinkTransactions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickLinkTransactionsBasicListData(config));
            }));

    }


    public PutBrickLinkTransaction(id: bigint | number, brickLinkTransaction: BrickLinkTransactionSubmitData) : Observable<BrickLinkTransactionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickLinkTransactionData>(this.baseUrl + 'api/BrickLinkTransaction/' + id.toString(), brickLinkTransaction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickLinkTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickLinkTransaction(id, brickLinkTransaction));
            }));
    }


    public PostBrickLinkTransaction(brickLinkTransaction: BrickLinkTransactionSubmitData) : Observable<BrickLinkTransactionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickLinkTransactionData>(this.baseUrl + 'api/BrickLinkTransaction', brickLinkTransaction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickLinkTransaction(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickLinkTransaction(brickLinkTransaction));
            }));
    }

  
    public DeleteBrickLinkTransaction(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickLinkTransaction/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickLinkTransaction(id));
            }));
    }


    private getConfigHash(config: BrickLinkTransactionQueryParameters | any): string {

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

    public userIsBMCBrickLinkTransactionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickLinkTransactionReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickLinkTransactions
        //
        if (userIsBMCBrickLinkTransactionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickLinkTransactionReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickLinkTransactionReader = false;
            }
        }

        return userIsBMCBrickLinkTransactionReader;
    }


    public userIsBMCBrickLinkTransactionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickLinkTransactionWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickLinkTransactions
        //
        if (userIsBMCBrickLinkTransactionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickLinkTransactionWriter = user.writePermission >= 255;
          } else {
            userIsBMCBrickLinkTransactionWriter = false;
          }      
        }

        return userIsBMCBrickLinkTransactionWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BrickLinkTransactionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickLinkTransactionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickLinkTransactionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickLinkTransaction(raw: any): BrickLinkTransactionData {
    if (!raw) return raw;

    //
    // Create a BrickLinkTransactionData object instance with correct prototype
    //
    const revived = Object.create(BrickLinkTransactionData.prototype) as BrickLinkTransactionData;

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
    // 2. But private methods (loadBrickLinkTransactionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBrickLinkTransactionList(rawList: any[]): BrickLinkTransactionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickLinkTransaction(raw));
  }

}
