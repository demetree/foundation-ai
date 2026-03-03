/*

   GENERATED SERVICE FOR THE BRICKECONOMYUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickEconomyUserLink table.

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
export class BrickEconomyUserLinkQueryParameters {
    encryptedApiKey: string | null | undefined = null;
    syncEnabled: boolean | null | undefined = null;
    lastSyncDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    lastSyncError: string | null | undefined = null;
    dailyQuotaUsed: bigint | number | null | undefined = null;
    quotaResetDate: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class BrickEconomyUserLinkSubmitData {
    id!: bigint | number;
    encryptedApiKey: string | null = null;
    syncEnabled!: boolean;
    lastSyncDate: string | null = null;     // ISO 8601 (full datetime)
    lastSyncError: string | null = null;
    dailyQuotaUsed: bigint | number | null = null;
    quotaResetDate: string | null = null;     // ISO 8601 (full datetime)
    active!: boolean;
    deleted!: boolean;
}


export class BrickEconomyUserLinkBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickEconomyUserLinkChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickEconomyUserLink.BrickEconomyUserLinkChildren$` — use with `| async` in templates
//        • Promise:    `brickEconomyUserLink.BrickEconomyUserLinkChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickEconomyUserLink.BrickEconomyUserLinkChildren$ | async"`), or
//        • Access the promise getter (`brickEconomyUserLink.BrickEconomyUserLinkChildren` or `await brickEconomyUserLink.BrickEconomyUserLinkChildren`)
//    - Simply reading `brickEconomyUserLink.BrickEconomyUserLinkChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickEconomyUserLink.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickEconomyUserLinkData {
    id!: bigint | number;
    encryptedApiKey!: string | null;
    syncEnabled!: boolean;
    lastSyncDate!: string | null;   // ISO 8601 (full datetime)
    lastSyncError!: string | null;
    dailyQuotaUsed!: bigint | number;
    quotaResetDate!: string | null;   // ISO 8601 (full datetime)
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
  // Promise based reload method to allow rebuilding of any BrickEconomyUserLinkData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickEconomyUserLink.Reload();
  //
  //  Non Async:
  //
  //     brickEconomyUserLink[0].Reload().then(x => {
  //        this.brickEconomyUserLink = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickEconomyUserLinkService.Instance.GetBrickEconomyUserLink(this.id, includeRelations)
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
     * Updates the state of this BrickEconomyUserLinkData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickEconomyUserLinkData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickEconomyUserLinkSubmitData {
        return BrickEconomyUserLinkService.Instance.ConvertToBrickEconomyUserLinkSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickEconomyUserLinkService extends SecureEndpointBase {

    private static _instance: BrickEconomyUserLinkService;
    private listCache: Map<string, Observable<Array<BrickEconomyUserLinkData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickEconomyUserLinkBasicListData>>>;
    private recordCache: Map<string, Observable<BrickEconomyUserLinkData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickEconomyUserLinkData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickEconomyUserLinkBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickEconomyUserLinkData>>();

        BrickEconomyUserLinkService._instance = this;
    }

    public static get Instance(): BrickEconomyUserLinkService {
      return BrickEconomyUserLinkService._instance;
    }


    public ClearListCaches(config: BrickEconomyUserLinkQueryParameters | null = null) {

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


    public ConvertToBrickEconomyUserLinkSubmitData(data: BrickEconomyUserLinkData): BrickEconomyUserLinkSubmitData {

        let output = new BrickEconomyUserLinkSubmitData();

        output.id = data.id;
        output.encryptedApiKey = data.encryptedApiKey;
        output.syncEnabled = data.syncEnabled;
        output.lastSyncDate = data.lastSyncDate;
        output.lastSyncError = data.lastSyncError;
        output.dailyQuotaUsed = data.dailyQuotaUsed;
        output.quotaResetDate = data.quotaResetDate;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBrickEconomyUserLink(id: bigint | number, includeRelations: boolean = true) : Observable<BrickEconomyUserLinkData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickEconomyUserLink$ = this.requestBrickEconomyUserLink(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickEconomyUserLink", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickEconomyUserLink$);

            return brickEconomyUserLink$;
        }

        return this.recordCache.get(configHash) as Observable<BrickEconomyUserLinkData>;
    }

    private requestBrickEconomyUserLink(id: bigint | number, includeRelations: boolean = true) : Observable<BrickEconomyUserLinkData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickEconomyUserLinkData>(this.baseUrl + 'api/BrickEconomyUserLink/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickEconomyUserLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickEconomyUserLink(id, includeRelations));
            }));
    }

    public GetBrickEconomyUserLinkList(config: BrickEconomyUserLinkQueryParameters | any = null) : Observable<Array<BrickEconomyUserLinkData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickEconomyUserLinkList$ = this.requestBrickEconomyUserLinkList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickEconomyUserLink list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickEconomyUserLinkList$);

            return brickEconomyUserLinkList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickEconomyUserLinkData>>;
    }


    private requestBrickEconomyUserLinkList(config: BrickEconomyUserLinkQueryParameters | any) : Observable <Array<BrickEconomyUserLinkData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickEconomyUserLinkData>>(this.baseUrl + 'api/BrickEconomyUserLinks', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickEconomyUserLinkList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickEconomyUserLinkList(config));
            }));
    }

    public GetBrickEconomyUserLinksRowCount(config: BrickEconomyUserLinkQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickEconomyUserLinksRowCount$ = this.requestBrickEconomyUserLinksRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickEconomyUserLinks row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickEconomyUserLinksRowCount$);

            return brickEconomyUserLinksRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickEconomyUserLinksRowCount(config: BrickEconomyUserLinkQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickEconomyUserLinks/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickEconomyUserLinksRowCount(config));
            }));
    }

    public GetBrickEconomyUserLinksBasicListData(config: BrickEconomyUserLinkQueryParameters | any = null) : Observable<Array<BrickEconomyUserLinkBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickEconomyUserLinksBasicListData$ = this.requestBrickEconomyUserLinksBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickEconomyUserLinks basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickEconomyUserLinksBasicListData$);

            return brickEconomyUserLinksBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickEconomyUserLinkBasicListData>>;
    }


    private requestBrickEconomyUserLinksBasicListData(config: BrickEconomyUserLinkQueryParameters | any) : Observable<Array<BrickEconomyUserLinkBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickEconomyUserLinkBasicListData>>(this.baseUrl + 'api/BrickEconomyUserLinks/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickEconomyUserLinksBasicListData(config));
            }));

    }


    public PutBrickEconomyUserLink(id: bigint | number, brickEconomyUserLink: BrickEconomyUserLinkSubmitData) : Observable<BrickEconomyUserLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickEconomyUserLinkData>(this.baseUrl + 'api/BrickEconomyUserLink/' + id.toString(), brickEconomyUserLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickEconomyUserLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickEconomyUserLink(id, brickEconomyUserLink));
            }));
    }


    public PostBrickEconomyUserLink(brickEconomyUserLink: BrickEconomyUserLinkSubmitData) : Observable<BrickEconomyUserLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickEconomyUserLinkData>(this.baseUrl + 'api/BrickEconomyUserLink', brickEconomyUserLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickEconomyUserLink(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickEconomyUserLink(brickEconomyUserLink));
            }));
    }

  
    public DeleteBrickEconomyUserLink(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickEconomyUserLink/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickEconomyUserLink(id));
            }));
    }


    private getConfigHash(config: BrickEconomyUserLinkQueryParameters | any): string {

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

    public userIsBMCBrickEconomyUserLinkReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickEconomyUserLinkReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickEconomyUserLinks
        //
        if (userIsBMCBrickEconomyUserLinkReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickEconomyUserLinkReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickEconomyUserLinkReader = false;
            }
        }

        return userIsBMCBrickEconomyUserLinkReader;
    }


    public userIsBMCBrickEconomyUserLinkWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickEconomyUserLinkWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickEconomyUserLinks
        //
        if (userIsBMCBrickEconomyUserLinkWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickEconomyUserLinkWriter = user.writePermission >= 10;
          } else {
            userIsBMCBrickEconomyUserLinkWriter = false;
          }      
        }

        return userIsBMCBrickEconomyUserLinkWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BrickEconomyUserLinkData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickEconomyUserLinkData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickEconomyUserLinkTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickEconomyUserLink(raw: any): BrickEconomyUserLinkData {
    if (!raw) return raw;

    //
    // Create a BrickEconomyUserLinkData object instance with correct prototype
    //
    const revived = Object.create(BrickEconomyUserLinkData.prototype) as BrickEconomyUserLinkData;

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
    // 2. But private methods (loadBrickEconomyUserLinkXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBrickEconomyUserLinkList(rawList: any[]): BrickEconomyUserLinkData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickEconomyUserLink(raw));
  }

}
