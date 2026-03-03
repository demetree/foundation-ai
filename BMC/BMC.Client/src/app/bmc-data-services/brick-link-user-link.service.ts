/*

   GENERATED SERVICE FOR THE BRICKLINKUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickLinkUserLink table.

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
export class BrickLinkUserLinkQueryParameters {
    encryptedTokenValue: string | null | undefined = null;
    encryptedTokenSecret: string | null | undefined = null;
    syncEnabled: boolean | null | undefined = null;
    syncDirection: string | null | undefined = null;
    lastSyncDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    lastPullDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    lastPushDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    lastSyncError: string | null | undefined = null;
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
export class BrickLinkUserLinkSubmitData {
    id!: bigint | number;
    encryptedTokenValue: string | null = null;
    encryptedTokenSecret: string | null = null;
    syncEnabled!: boolean;
    syncDirection: string | null = null;
    lastSyncDate: string | null = null;     // ISO 8601 (full datetime)
    lastPullDate: string | null = null;     // ISO 8601 (full datetime)
    lastPushDate: string | null = null;     // ISO 8601 (full datetime)
    lastSyncError: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BrickLinkUserLinkBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickLinkUserLinkChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickLinkUserLink.BrickLinkUserLinkChildren$` — use with `| async` in templates
//        • Promise:    `brickLinkUserLink.BrickLinkUserLinkChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickLinkUserLink.BrickLinkUserLinkChildren$ | async"`), or
//        • Access the promise getter (`brickLinkUserLink.BrickLinkUserLinkChildren` or `await brickLinkUserLink.BrickLinkUserLinkChildren`)
//    - Simply reading `brickLinkUserLink.BrickLinkUserLinkChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickLinkUserLink.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickLinkUserLinkData {
    id!: bigint | number;
    encryptedTokenValue!: string | null;
    encryptedTokenSecret!: string | null;
    syncEnabled!: boolean;
    syncDirection!: string | null;
    lastSyncDate!: string | null;   // ISO 8601 (full datetime)
    lastPullDate!: string | null;   // ISO 8601 (full datetime)
    lastPushDate!: string | null;   // ISO 8601 (full datetime)
    lastSyncError!: string | null;
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
  // Promise based reload method to allow rebuilding of any BrickLinkUserLinkData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickLinkUserLink.Reload();
  //
  //  Non Async:
  //
  //     brickLinkUserLink[0].Reload().then(x => {
  //        this.brickLinkUserLink = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickLinkUserLinkService.Instance.GetBrickLinkUserLink(this.id, includeRelations)
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
     * Updates the state of this BrickLinkUserLinkData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickLinkUserLinkData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickLinkUserLinkSubmitData {
        return BrickLinkUserLinkService.Instance.ConvertToBrickLinkUserLinkSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickLinkUserLinkService extends SecureEndpointBase {

    private static _instance: BrickLinkUserLinkService;
    private listCache: Map<string, Observable<Array<BrickLinkUserLinkData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickLinkUserLinkBasicListData>>>;
    private recordCache: Map<string, Observable<BrickLinkUserLinkData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickLinkUserLinkData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickLinkUserLinkBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickLinkUserLinkData>>();

        BrickLinkUserLinkService._instance = this;
    }

    public static get Instance(): BrickLinkUserLinkService {
      return BrickLinkUserLinkService._instance;
    }


    public ClearListCaches(config: BrickLinkUserLinkQueryParameters | null = null) {

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


    public ConvertToBrickLinkUserLinkSubmitData(data: BrickLinkUserLinkData): BrickLinkUserLinkSubmitData {

        let output = new BrickLinkUserLinkSubmitData();

        output.id = data.id;
        output.encryptedTokenValue = data.encryptedTokenValue;
        output.encryptedTokenSecret = data.encryptedTokenSecret;
        output.syncEnabled = data.syncEnabled;
        output.syncDirection = data.syncDirection;
        output.lastSyncDate = data.lastSyncDate;
        output.lastPullDate = data.lastPullDate;
        output.lastPushDate = data.lastPushDate;
        output.lastSyncError = data.lastSyncError;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBrickLinkUserLink(id: bigint | number, includeRelations: boolean = true) : Observable<BrickLinkUserLinkData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickLinkUserLink$ = this.requestBrickLinkUserLink(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickLinkUserLink", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickLinkUserLink$);

            return brickLinkUserLink$;
        }

        return this.recordCache.get(configHash) as Observable<BrickLinkUserLinkData>;
    }

    private requestBrickLinkUserLink(id: bigint | number, includeRelations: boolean = true) : Observable<BrickLinkUserLinkData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickLinkUserLinkData>(this.baseUrl + 'api/BrickLinkUserLink/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickLinkUserLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickLinkUserLink(id, includeRelations));
            }));
    }

    public GetBrickLinkUserLinkList(config: BrickLinkUserLinkQueryParameters | any = null) : Observable<Array<BrickLinkUserLinkData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickLinkUserLinkList$ = this.requestBrickLinkUserLinkList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickLinkUserLink list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickLinkUserLinkList$);

            return brickLinkUserLinkList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickLinkUserLinkData>>;
    }


    private requestBrickLinkUserLinkList(config: BrickLinkUserLinkQueryParameters | any) : Observable <Array<BrickLinkUserLinkData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickLinkUserLinkData>>(this.baseUrl + 'api/BrickLinkUserLinks', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickLinkUserLinkList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickLinkUserLinkList(config));
            }));
    }

    public GetBrickLinkUserLinksRowCount(config: BrickLinkUserLinkQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickLinkUserLinksRowCount$ = this.requestBrickLinkUserLinksRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickLinkUserLinks row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickLinkUserLinksRowCount$);

            return brickLinkUserLinksRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickLinkUserLinksRowCount(config: BrickLinkUserLinkQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickLinkUserLinks/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickLinkUserLinksRowCount(config));
            }));
    }

    public GetBrickLinkUserLinksBasicListData(config: BrickLinkUserLinkQueryParameters | any = null) : Observable<Array<BrickLinkUserLinkBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickLinkUserLinksBasicListData$ = this.requestBrickLinkUserLinksBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickLinkUserLinks basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickLinkUserLinksBasicListData$);

            return brickLinkUserLinksBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickLinkUserLinkBasicListData>>;
    }


    private requestBrickLinkUserLinksBasicListData(config: BrickLinkUserLinkQueryParameters | any) : Observable<Array<BrickLinkUserLinkBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickLinkUserLinkBasicListData>>(this.baseUrl + 'api/BrickLinkUserLinks/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickLinkUserLinksBasicListData(config));
            }));

    }


    public PutBrickLinkUserLink(id: bigint | number, brickLinkUserLink: BrickLinkUserLinkSubmitData) : Observable<BrickLinkUserLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickLinkUserLinkData>(this.baseUrl + 'api/BrickLinkUserLink/' + id.toString(), brickLinkUserLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickLinkUserLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickLinkUserLink(id, brickLinkUserLink));
            }));
    }


    public PostBrickLinkUserLink(brickLinkUserLink: BrickLinkUserLinkSubmitData) : Observable<BrickLinkUserLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickLinkUserLinkData>(this.baseUrl + 'api/BrickLinkUserLink', brickLinkUserLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickLinkUserLink(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickLinkUserLink(brickLinkUserLink));
            }));
    }

  
    public DeleteBrickLinkUserLink(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickLinkUserLink/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickLinkUserLink(id));
            }));
    }


    private getConfigHash(config: BrickLinkUserLinkQueryParameters | any): string {

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

    public userIsBMCBrickLinkUserLinkReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickLinkUserLinkReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickLinkUserLinks
        //
        if (userIsBMCBrickLinkUserLinkReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickLinkUserLinkReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickLinkUserLinkReader = false;
            }
        }

        return userIsBMCBrickLinkUserLinkReader;
    }


    public userIsBMCBrickLinkUserLinkWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickLinkUserLinkWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickLinkUserLinks
        //
        if (userIsBMCBrickLinkUserLinkWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickLinkUserLinkWriter = user.writePermission >= 10;
          } else {
            userIsBMCBrickLinkUserLinkWriter = false;
          }      
        }

        return userIsBMCBrickLinkUserLinkWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BrickLinkUserLinkData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickLinkUserLinkData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickLinkUserLinkTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickLinkUserLink(raw: any): BrickLinkUserLinkData {
    if (!raw) return raw;

    //
    // Create a BrickLinkUserLinkData object instance with correct prototype
    //
    const revived = Object.create(BrickLinkUserLinkData.prototype) as BrickLinkUserLinkData;

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
    // 2. But private methods (loadBrickLinkUserLinkXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBrickLinkUserLinkList(rawList: any[]): BrickLinkUserLinkData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickLinkUserLink(raw));
  }

}
