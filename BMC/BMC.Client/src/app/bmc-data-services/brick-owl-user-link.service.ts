/*

   GENERATED SERVICE FOR THE BRICKOWLUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickOwlUserLink table.

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
export class BrickOwlUserLinkQueryParameters {
    encryptedApiKey: string | null | undefined = null;
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
export class BrickOwlUserLinkSubmitData {
    id!: bigint | number;
    encryptedApiKey: string | null = null;
    syncEnabled!: boolean;
    syncDirection: string | null = null;
    lastSyncDate: string | null = null;     // ISO 8601 (full datetime)
    lastPullDate: string | null = null;     // ISO 8601 (full datetime)
    lastPushDate: string | null = null;     // ISO 8601 (full datetime)
    lastSyncError: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BrickOwlUserLinkBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickOwlUserLinkChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickOwlUserLink.BrickOwlUserLinkChildren$` — use with `| async` in templates
//        • Promise:    `brickOwlUserLink.BrickOwlUserLinkChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickOwlUserLink.BrickOwlUserLinkChildren$ | async"`), or
//        • Access the promise getter (`brickOwlUserLink.BrickOwlUserLinkChildren` or `await brickOwlUserLink.BrickOwlUserLinkChildren`)
//    - Simply reading `brickOwlUserLink.BrickOwlUserLinkChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickOwlUserLink.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickOwlUserLinkData {
    id!: bigint | number;
    encryptedApiKey!: string | null;
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
  // Promise based reload method to allow rebuilding of any BrickOwlUserLinkData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickOwlUserLink.Reload();
  //
  //  Non Async:
  //
  //     brickOwlUserLink[0].Reload().then(x => {
  //        this.brickOwlUserLink = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickOwlUserLinkService.Instance.GetBrickOwlUserLink(this.id, includeRelations)
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
     * Updates the state of this BrickOwlUserLinkData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickOwlUserLinkData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickOwlUserLinkSubmitData {
        return BrickOwlUserLinkService.Instance.ConvertToBrickOwlUserLinkSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickOwlUserLinkService extends SecureEndpointBase {

    private static _instance: BrickOwlUserLinkService;
    private listCache: Map<string, Observable<Array<BrickOwlUserLinkData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickOwlUserLinkBasicListData>>>;
    private recordCache: Map<string, Observable<BrickOwlUserLinkData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickOwlUserLinkData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickOwlUserLinkBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickOwlUserLinkData>>();

        BrickOwlUserLinkService._instance = this;
    }

    public static get Instance(): BrickOwlUserLinkService {
      return BrickOwlUserLinkService._instance;
    }


    public ClearListCaches(config: BrickOwlUserLinkQueryParameters | null = null) {

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


    public ConvertToBrickOwlUserLinkSubmitData(data: BrickOwlUserLinkData): BrickOwlUserLinkSubmitData {

        let output = new BrickOwlUserLinkSubmitData();

        output.id = data.id;
        output.encryptedApiKey = data.encryptedApiKey;
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

    public GetBrickOwlUserLink(id: bigint | number, includeRelations: boolean = true) : Observable<BrickOwlUserLinkData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickOwlUserLink$ = this.requestBrickOwlUserLink(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickOwlUserLink", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickOwlUserLink$);

            return brickOwlUserLink$;
        }

        return this.recordCache.get(configHash) as Observable<BrickOwlUserLinkData>;
    }

    private requestBrickOwlUserLink(id: bigint | number, includeRelations: boolean = true) : Observable<BrickOwlUserLinkData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickOwlUserLinkData>(this.baseUrl + 'api/BrickOwlUserLink/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickOwlUserLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickOwlUserLink(id, includeRelations));
            }));
    }

    public GetBrickOwlUserLinkList(config: BrickOwlUserLinkQueryParameters | any = null) : Observable<Array<BrickOwlUserLinkData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickOwlUserLinkList$ = this.requestBrickOwlUserLinkList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickOwlUserLink list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickOwlUserLinkList$);

            return brickOwlUserLinkList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickOwlUserLinkData>>;
    }


    private requestBrickOwlUserLinkList(config: BrickOwlUserLinkQueryParameters | any) : Observable <Array<BrickOwlUserLinkData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickOwlUserLinkData>>(this.baseUrl + 'api/BrickOwlUserLinks', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickOwlUserLinkList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickOwlUserLinkList(config));
            }));
    }

    public GetBrickOwlUserLinksRowCount(config: BrickOwlUserLinkQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickOwlUserLinksRowCount$ = this.requestBrickOwlUserLinksRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickOwlUserLinks row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickOwlUserLinksRowCount$);

            return brickOwlUserLinksRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickOwlUserLinksRowCount(config: BrickOwlUserLinkQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickOwlUserLinks/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickOwlUserLinksRowCount(config));
            }));
    }

    public GetBrickOwlUserLinksBasicListData(config: BrickOwlUserLinkQueryParameters | any = null) : Observable<Array<BrickOwlUserLinkBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickOwlUserLinksBasicListData$ = this.requestBrickOwlUserLinksBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickOwlUserLinks basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickOwlUserLinksBasicListData$);

            return brickOwlUserLinksBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickOwlUserLinkBasicListData>>;
    }


    private requestBrickOwlUserLinksBasicListData(config: BrickOwlUserLinkQueryParameters | any) : Observable<Array<BrickOwlUserLinkBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickOwlUserLinkBasicListData>>(this.baseUrl + 'api/BrickOwlUserLinks/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickOwlUserLinksBasicListData(config));
            }));

    }


    public PutBrickOwlUserLink(id: bigint | number, brickOwlUserLink: BrickOwlUserLinkSubmitData) : Observable<BrickOwlUserLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickOwlUserLinkData>(this.baseUrl + 'api/BrickOwlUserLink/' + id.toString(), brickOwlUserLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickOwlUserLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickOwlUserLink(id, brickOwlUserLink));
            }));
    }


    public PostBrickOwlUserLink(brickOwlUserLink: BrickOwlUserLinkSubmitData) : Observable<BrickOwlUserLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickOwlUserLinkData>(this.baseUrl + 'api/BrickOwlUserLink', brickOwlUserLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickOwlUserLink(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickOwlUserLink(brickOwlUserLink));
            }));
    }

  
    public DeleteBrickOwlUserLink(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickOwlUserLink/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickOwlUserLink(id));
            }));
    }


    private getConfigHash(config: BrickOwlUserLinkQueryParameters | any): string {

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

    public userIsBMCBrickOwlUserLinkReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickOwlUserLinkReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickOwlUserLinks
        //
        if (userIsBMCBrickOwlUserLinkReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickOwlUserLinkReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickOwlUserLinkReader = false;
            }
        }

        return userIsBMCBrickOwlUserLinkReader;
    }


    public userIsBMCBrickOwlUserLinkWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickOwlUserLinkWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickOwlUserLinks
        //
        if (userIsBMCBrickOwlUserLinkWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickOwlUserLinkWriter = user.writePermission >= 10;
          } else {
            userIsBMCBrickOwlUserLinkWriter = false;
          }      
        }

        return userIsBMCBrickOwlUserLinkWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BrickOwlUserLinkData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickOwlUserLinkData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickOwlUserLinkTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickOwlUserLink(raw: any): BrickOwlUserLinkData {
    if (!raw) return raw;

    //
    // Create a BrickOwlUserLinkData object instance with correct prototype
    //
    const revived = Object.create(BrickOwlUserLinkData.prototype) as BrickOwlUserLinkData;

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
    // 2. But private methods (loadBrickOwlUserLinkXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBrickOwlUserLinkList(rawList: any[]): BrickOwlUserLinkData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickOwlUserLink(raw));
  }

}
