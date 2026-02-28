/*

   GENERATED SERVICE FOR THE REBRICKABLEUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the RebrickableUserLink table.

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
export class RebrickableUserLinkQueryParameters {
    rebrickableUsername: string | null | undefined = null;
    encryptedApiToken: string | null | undefined = null;
    lastSyncDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    syncEnabled: boolean | null | undefined = null;
    syncDirectionFlags: string | null | undefined = null;
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
export class RebrickableUserLinkSubmitData {
    id!: bigint | number;
    rebrickableUsername!: string;
    encryptedApiToken!: string;
    lastSyncDate: string | null = null;     // ISO 8601 (full datetime)
    syncEnabled!: boolean;
    syncDirectionFlags!: string;
    active!: boolean;
    deleted!: boolean;
}


export class RebrickableUserLinkBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. RebrickableUserLinkChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `rebrickableUserLink.RebrickableUserLinkChildren$` — use with `| async` in templates
//        • Promise:    `rebrickableUserLink.RebrickableUserLinkChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="rebrickableUserLink.RebrickableUserLinkChildren$ | async"`), or
//        • Access the promise getter (`rebrickableUserLink.RebrickableUserLinkChildren` or `await rebrickableUserLink.RebrickableUserLinkChildren`)
//    - Simply reading `rebrickableUserLink.RebrickableUserLinkChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await rebrickableUserLink.Reload()` to refresh the entire object and clear all lazy caches.
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
export class RebrickableUserLinkData {
    id!: bigint | number;
    rebrickableUsername!: string;
    encryptedApiToken!: string;
    lastSyncDate!: string | null;   // ISO 8601 (full datetime)
    syncEnabled!: boolean;
    syncDirectionFlags!: string;
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
  // Promise based reload method to allow rebuilding of any RebrickableUserLinkData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.rebrickableUserLink.Reload();
  //
  //  Non Async:
  //
  //     rebrickableUserLink[0].Reload().then(x => {
  //        this.rebrickableUserLink = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      RebrickableUserLinkService.Instance.GetRebrickableUserLink(this.id, includeRelations)
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
     * Updates the state of this RebrickableUserLinkData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this RebrickableUserLinkData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): RebrickableUserLinkSubmitData {
        return RebrickableUserLinkService.Instance.ConvertToRebrickableUserLinkSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class RebrickableUserLinkService extends SecureEndpointBase {

    private static _instance: RebrickableUserLinkService;
    private listCache: Map<string, Observable<Array<RebrickableUserLinkData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<RebrickableUserLinkBasicListData>>>;
    private recordCache: Map<string, Observable<RebrickableUserLinkData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<RebrickableUserLinkData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<RebrickableUserLinkBasicListData>>>();
        this.recordCache = new Map<string, Observable<RebrickableUserLinkData>>();

        RebrickableUserLinkService._instance = this;
    }

    public static get Instance(): RebrickableUserLinkService {
      return RebrickableUserLinkService._instance;
    }


    public ClearListCaches(config: RebrickableUserLinkQueryParameters | null = null) {

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


    public ConvertToRebrickableUserLinkSubmitData(data: RebrickableUserLinkData): RebrickableUserLinkSubmitData {

        let output = new RebrickableUserLinkSubmitData();

        output.id = data.id;
        output.rebrickableUsername = data.rebrickableUsername;
        output.encryptedApiToken = data.encryptedApiToken;
        output.lastSyncDate = data.lastSyncDate;
        output.syncEnabled = data.syncEnabled;
        output.syncDirectionFlags = data.syncDirectionFlags;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetRebrickableUserLink(id: bigint | number, includeRelations: boolean = true) : Observable<RebrickableUserLinkData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const rebrickableUserLink$ = this.requestRebrickableUserLink(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RebrickableUserLink", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, rebrickableUserLink$);

            return rebrickableUserLink$;
        }

        return this.recordCache.get(configHash) as Observable<RebrickableUserLinkData>;
    }

    private requestRebrickableUserLink(id: bigint | number, includeRelations: boolean = true) : Observable<RebrickableUserLinkData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RebrickableUserLinkData>(this.baseUrl + 'api/RebrickableUserLink/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveRebrickableUserLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestRebrickableUserLink(id, includeRelations));
            }));
    }

    public GetRebrickableUserLinkList(config: RebrickableUserLinkQueryParameters | any = null) : Observable<Array<RebrickableUserLinkData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const rebrickableUserLinkList$ = this.requestRebrickableUserLinkList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RebrickableUserLink list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, rebrickableUserLinkList$);

            return rebrickableUserLinkList$;
        }

        return this.listCache.get(configHash) as Observable<Array<RebrickableUserLinkData>>;
    }


    private requestRebrickableUserLinkList(config: RebrickableUserLinkQueryParameters | any) : Observable <Array<RebrickableUserLinkData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RebrickableUserLinkData>>(this.baseUrl + 'api/RebrickableUserLinks', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveRebrickableUserLinkList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestRebrickableUserLinkList(config));
            }));
    }

    public GetRebrickableUserLinksRowCount(config: RebrickableUserLinkQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const rebrickableUserLinksRowCount$ = this.requestRebrickableUserLinksRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RebrickableUserLinks row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, rebrickableUserLinksRowCount$);

            return rebrickableUserLinksRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestRebrickableUserLinksRowCount(config: RebrickableUserLinkQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/RebrickableUserLinks/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRebrickableUserLinksRowCount(config));
            }));
    }

    public GetRebrickableUserLinksBasicListData(config: RebrickableUserLinkQueryParameters | any = null) : Observable<Array<RebrickableUserLinkBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const rebrickableUserLinksBasicListData$ = this.requestRebrickableUserLinksBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RebrickableUserLinks basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, rebrickableUserLinksBasicListData$);

            return rebrickableUserLinksBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<RebrickableUserLinkBasicListData>>;
    }


    private requestRebrickableUserLinksBasicListData(config: RebrickableUserLinkQueryParameters | any) : Observable<Array<RebrickableUserLinkBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RebrickableUserLinkBasicListData>>(this.baseUrl + 'api/RebrickableUserLinks/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRebrickableUserLinksBasicListData(config));
            }));

    }


    public PutRebrickableUserLink(id: bigint | number, rebrickableUserLink: RebrickableUserLinkSubmitData) : Observable<RebrickableUserLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RebrickableUserLinkData>(this.baseUrl + 'api/RebrickableUserLink/' + id.toString(), rebrickableUserLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRebrickableUserLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutRebrickableUserLink(id, rebrickableUserLink));
            }));
    }


    public PostRebrickableUserLink(rebrickableUserLink: RebrickableUserLinkSubmitData) : Observable<RebrickableUserLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<RebrickableUserLinkData>(this.baseUrl + 'api/RebrickableUserLink', rebrickableUserLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRebrickableUserLink(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostRebrickableUserLink(rebrickableUserLink));
            }));
    }

  
    public DeleteRebrickableUserLink(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/RebrickableUserLink/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteRebrickableUserLink(id));
            }));
    }


    private getConfigHash(config: RebrickableUserLinkQueryParameters | any): string {

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

    public userIsBMCRebrickableUserLinkReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCRebrickableUserLinkReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.RebrickableUserLinks
        //
        if (userIsBMCRebrickableUserLinkReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCRebrickableUserLinkReader = user.readPermission >= 1;
            } else {
                userIsBMCRebrickableUserLinkReader = false;
            }
        }

        return userIsBMCRebrickableUserLinkReader;
    }


    public userIsBMCRebrickableUserLinkWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCRebrickableUserLinkWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.RebrickableUserLinks
        //
        if (userIsBMCRebrickableUserLinkWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCRebrickableUserLinkWriter = user.writePermission >= 10;
          } else {
            userIsBMCRebrickableUserLinkWriter = false;
          }      
        }

        return userIsBMCRebrickableUserLinkWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full RebrickableUserLinkData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the RebrickableUserLinkData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when RebrickableUserLinkTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveRebrickableUserLink(raw: any): RebrickableUserLinkData {
    if (!raw) return raw;

    //
    // Create a RebrickableUserLinkData object instance with correct prototype
    //
    const revived = Object.create(RebrickableUserLinkData.prototype) as RebrickableUserLinkData;

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
    // 2. But private methods (loadRebrickableUserLinkXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveRebrickableUserLinkList(rawList: any[]): RebrickableUserLinkData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveRebrickableUserLink(raw));
  }

}
