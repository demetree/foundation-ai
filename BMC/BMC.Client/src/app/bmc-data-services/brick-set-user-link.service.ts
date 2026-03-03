/*

   GENERATED SERVICE FOR THE BRICKSETUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickSetUserLink table.

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
export class BrickSetUserLinkQueryParameters {
    brickSetUsername: string | null | undefined = null;
    encryptedUserHash: string | null | undefined = null;
    encryptedPassword: string | null | undefined = null;
    syncEnabled: boolean | null | undefined = null;
    syncDirection: string | null | undefined = null;
    lastSyncDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    lastPullDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    lastPushDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    lastSyncError: string | null | undefined = null;
    userHashStoredDate: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class BrickSetUserLinkSubmitData {
    id!: bigint | number;
    brickSetUsername!: string;
    encryptedUserHash!: string;
    encryptedPassword: string | null = null;
    syncEnabled!: boolean;
    syncDirection!: string;
    lastSyncDate: string | null = null;     // ISO 8601 (full datetime)
    lastPullDate: string | null = null;     // ISO 8601 (full datetime)
    lastPushDate: string | null = null;     // ISO 8601 (full datetime)
    lastSyncError: string | null = null;
    userHashStoredDate: string | null = null;     // ISO 8601 (full datetime)
    active!: boolean;
    deleted!: boolean;
}


export class BrickSetUserLinkBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickSetUserLinkChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickSetUserLink.BrickSetUserLinkChildren$` — use with `| async` in templates
//        • Promise:    `brickSetUserLink.BrickSetUserLinkChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickSetUserLink.BrickSetUserLinkChildren$ | async"`), or
//        • Access the promise getter (`brickSetUserLink.BrickSetUserLinkChildren` or `await brickSetUserLink.BrickSetUserLinkChildren`)
//    - Simply reading `brickSetUserLink.BrickSetUserLinkChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickSetUserLink.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickSetUserLinkData {
    id!: bigint | number;
    brickSetUsername!: string;
    encryptedUserHash!: string;
    encryptedPassword!: string | null;
    syncEnabled!: boolean;
    syncDirection!: string;
    lastSyncDate!: string | null;   // ISO 8601 (full datetime)
    lastPullDate!: string | null;   // ISO 8601 (full datetime)
    lastPushDate!: string | null;   // ISO 8601 (full datetime)
    lastSyncError!: string | null;
    userHashStoredDate!: string | null;   // ISO 8601 (full datetime)
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
  // Promise based reload method to allow rebuilding of any BrickSetUserLinkData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickSetUserLink.Reload();
  //
  //  Non Async:
  //
  //     brickSetUserLink[0].Reload().then(x => {
  //        this.brickSetUserLink = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickSetUserLinkService.Instance.GetBrickSetUserLink(this.id, includeRelations)
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
     * Updates the state of this BrickSetUserLinkData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickSetUserLinkData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickSetUserLinkSubmitData {
        return BrickSetUserLinkService.Instance.ConvertToBrickSetUserLinkSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickSetUserLinkService extends SecureEndpointBase {

    private static _instance: BrickSetUserLinkService;
    private listCache: Map<string, Observable<Array<BrickSetUserLinkData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickSetUserLinkBasicListData>>>;
    private recordCache: Map<string, Observable<BrickSetUserLinkData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickSetUserLinkData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickSetUserLinkBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickSetUserLinkData>>();

        BrickSetUserLinkService._instance = this;
    }

    public static get Instance(): BrickSetUserLinkService {
      return BrickSetUserLinkService._instance;
    }


    public ClearListCaches(config: BrickSetUserLinkQueryParameters | null = null) {

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


    public ConvertToBrickSetUserLinkSubmitData(data: BrickSetUserLinkData): BrickSetUserLinkSubmitData {

        let output = new BrickSetUserLinkSubmitData();

        output.id = data.id;
        output.brickSetUsername = data.brickSetUsername;
        output.encryptedUserHash = data.encryptedUserHash;
        output.encryptedPassword = data.encryptedPassword;
        output.syncEnabled = data.syncEnabled;
        output.syncDirection = data.syncDirection;
        output.lastSyncDate = data.lastSyncDate;
        output.lastPullDate = data.lastPullDate;
        output.lastPushDate = data.lastPushDate;
        output.lastSyncError = data.lastSyncError;
        output.userHashStoredDate = data.userHashStoredDate;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBrickSetUserLink(id: bigint | number, includeRelations: boolean = true) : Observable<BrickSetUserLinkData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickSetUserLink$ = this.requestBrickSetUserLink(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickSetUserLink", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickSetUserLink$);

            return brickSetUserLink$;
        }

        return this.recordCache.get(configHash) as Observable<BrickSetUserLinkData>;
    }

    private requestBrickSetUserLink(id: bigint | number, includeRelations: boolean = true) : Observable<BrickSetUserLinkData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickSetUserLinkData>(this.baseUrl + 'api/BrickSetUserLink/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickSetUserLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickSetUserLink(id, includeRelations));
            }));
    }

    public GetBrickSetUserLinkList(config: BrickSetUserLinkQueryParameters | any = null) : Observable<Array<BrickSetUserLinkData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickSetUserLinkList$ = this.requestBrickSetUserLinkList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickSetUserLink list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickSetUserLinkList$);

            return brickSetUserLinkList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickSetUserLinkData>>;
    }


    private requestBrickSetUserLinkList(config: BrickSetUserLinkQueryParameters | any) : Observable <Array<BrickSetUserLinkData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickSetUserLinkData>>(this.baseUrl + 'api/BrickSetUserLinks', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickSetUserLinkList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickSetUserLinkList(config));
            }));
    }

    public GetBrickSetUserLinksRowCount(config: BrickSetUserLinkQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickSetUserLinksRowCount$ = this.requestBrickSetUserLinksRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickSetUserLinks row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickSetUserLinksRowCount$);

            return brickSetUserLinksRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickSetUserLinksRowCount(config: BrickSetUserLinkQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickSetUserLinks/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickSetUserLinksRowCount(config));
            }));
    }

    public GetBrickSetUserLinksBasicListData(config: BrickSetUserLinkQueryParameters | any = null) : Observable<Array<BrickSetUserLinkBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickSetUserLinksBasicListData$ = this.requestBrickSetUserLinksBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickSetUserLinks basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickSetUserLinksBasicListData$);

            return brickSetUserLinksBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickSetUserLinkBasicListData>>;
    }


    private requestBrickSetUserLinksBasicListData(config: BrickSetUserLinkQueryParameters | any) : Observable<Array<BrickSetUserLinkBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickSetUserLinkBasicListData>>(this.baseUrl + 'api/BrickSetUserLinks/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickSetUserLinksBasicListData(config));
            }));

    }


    public PutBrickSetUserLink(id: bigint | number, brickSetUserLink: BrickSetUserLinkSubmitData) : Observable<BrickSetUserLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickSetUserLinkData>(this.baseUrl + 'api/BrickSetUserLink/' + id.toString(), brickSetUserLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickSetUserLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickSetUserLink(id, brickSetUserLink));
            }));
    }


    public PostBrickSetUserLink(brickSetUserLink: BrickSetUserLinkSubmitData) : Observable<BrickSetUserLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickSetUserLinkData>(this.baseUrl + 'api/BrickSetUserLink', brickSetUserLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickSetUserLink(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickSetUserLink(brickSetUserLink));
            }));
    }

  
    public DeleteBrickSetUserLink(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickSetUserLink/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickSetUserLink(id));
            }));
    }


    private getConfigHash(config: BrickSetUserLinkQueryParameters | any): string {

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

    public userIsBMCBrickSetUserLinkReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickSetUserLinkReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickSetUserLinks
        //
        if (userIsBMCBrickSetUserLinkReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickSetUserLinkReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickSetUserLinkReader = false;
            }
        }

        return userIsBMCBrickSetUserLinkReader;
    }


    public userIsBMCBrickSetUserLinkWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickSetUserLinkWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickSetUserLinks
        //
        if (userIsBMCBrickSetUserLinkWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickSetUserLinkWriter = user.writePermission >= 10;
          } else {
            userIsBMCBrickSetUserLinkWriter = false;
          }      
        }

        return userIsBMCBrickSetUserLinkWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BrickSetUserLinkData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickSetUserLinkData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickSetUserLinkTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickSetUserLink(raw: any): BrickSetUserLinkData {
    if (!raw) return raw;

    //
    // Create a BrickSetUserLinkData object instance with correct prototype
    //
    const revived = Object.create(BrickSetUserLinkData.prototype) as BrickSetUserLinkData;

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
    // 2. But private methods (loadBrickSetUserLinkXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBrickSetUserLinkList(rawList: any[]): BrickSetUserLinkData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickSetUserLink(raw));
  }

}
