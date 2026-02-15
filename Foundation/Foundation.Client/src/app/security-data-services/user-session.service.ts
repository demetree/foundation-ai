/*

   GENERATED SERVICE FOR THE USERSESSION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserSession table.

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
import { SecurityUserData } from './security-user.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserSessionQueryParameters {
    securityUserId: bigint | number | null | undefined = null;
    objectGuid: string | null | undefined = null;
    tokenId: string | null | undefined = null;
    sessionStart: string | null | undefined = null;        // ISO 8601 (full datetime)
    expiresAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    ipAddress: string | null | undefined = null;
    userAgent: string | null | undefined = null;
    loginMethod: string | null | undefined = null;
    clientApplication: string | null | undefined = null;
    isRevoked: boolean | null | undefined = null;
    revokedAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    revokedBy: string | null | undefined = null;
    revokedReason: string | null | undefined = null;
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
export class UserSessionSubmitData {
    id!: bigint | number;
    securityUserId!: bigint | number;
    tokenId: string | null = null;
    sessionStart!: string;      // ISO 8601 (full datetime)
    expiresAt!: string;      // ISO 8601 (full datetime)
    ipAddress: string | null = null;
    userAgent: string | null = null;
    loginMethod: string | null = null;
    clientApplication: string | null = null;
    isRevoked!: boolean;
    revokedAt: string | null = null;     // ISO 8601 (full datetime)
    revokedBy: string | null = null;
    revokedReason: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class UserSessionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserSessionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userSession.UserSessionChildren$` — use with `| async` in templates
//        • Promise:    `userSession.UserSessionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userSession.UserSessionChildren$ | async"`), or
//        • Access the promise getter (`userSession.UserSessionChildren` or `await userSession.UserSessionChildren`)
//    - Simply reading `userSession.UserSessionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userSession.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserSessionData {
    id!: bigint | number;
    securityUserId!: bigint | number;
    objectGuid!: string;
    tokenId!: string | null;
    sessionStart!: string;      // ISO 8601 (full datetime)
    expiresAt!: string;      // ISO 8601 (full datetime)
    ipAddress!: string | null;
    userAgent!: string | null;
    loginMethod!: string | null;
    clientApplication!: string | null;
    isRevoked!: boolean;
    revokedAt!: string | null;   // ISO 8601 (full datetime)
    revokedBy!: string | null;
    revokedReason!: string | null;
    active!: boolean;
    deleted!: boolean;
    securityUser: SecurityUserData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any UserSessionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userSession.Reload();
  //
  //  Non Async:
  //
  //     userSession[0].Reload().then(x => {
  //        this.userSession = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserSessionService.Instance.GetUserSession(this.id, includeRelations)
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
     * Updates the state of this UserSessionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserSessionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserSessionSubmitData {
        return UserSessionService.Instance.ConvertToUserSessionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserSessionService extends SecureEndpointBase {

    private static _instance: UserSessionService;
    private listCache: Map<string, Observable<Array<UserSessionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserSessionBasicListData>>>;
    private recordCache: Map<string, Observable<UserSessionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserSessionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserSessionBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserSessionData>>();

        UserSessionService._instance = this;
    }

    public static get Instance(): UserSessionService {
      return UserSessionService._instance;
    }


    public ClearListCaches(config: UserSessionQueryParameters | null = null) {

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


    public ConvertToUserSessionSubmitData(data: UserSessionData): UserSessionSubmitData {

        let output = new UserSessionSubmitData();

        output.id = data.id;
        output.securityUserId = data.securityUserId;
        output.tokenId = data.tokenId;
        output.sessionStart = data.sessionStart;
        output.expiresAt = data.expiresAt;
        output.ipAddress = data.ipAddress;
        output.userAgent = data.userAgent;
        output.loginMethod = data.loginMethod;
        output.clientApplication = data.clientApplication;
        output.isRevoked = data.isRevoked;
        output.revokedAt = data.revokedAt;
        output.revokedBy = data.revokedBy;
        output.revokedReason = data.revokedReason;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserSession(id: bigint | number, includeRelations: boolean = true) : Observable<UserSessionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userSession$ = this.requestUserSession(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserSession", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userSession$);

            return userSession$;
        }

        return this.recordCache.get(configHash) as Observable<UserSessionData>;
    }

    private requestUserSession(id: bigint | number, includeRelations: boolean = true) : Observable<UserSessionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserSessionData>(this.baseUrl + 'api/UserSession/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserSession(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserSession(id, includeRelations));
            }));
    }

    public GetUserSessionList(config: UserSessionQueryParameters | any = null) : Observable<Array<UserSessionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userSessionList$ = this.requestUserSessionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserSession list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userSessionList$);

            return userSessionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserSessionData>>;
    }


    private requestUserSessionList(config: UserSessionQueryParameters | any) : Observable <Array<UserSessionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserSessionData>>(this.baseUrl + 'api/UserSessions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserSessionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserSessionList(config));
            }));
    }

    public GetUserSessionsRowCount(config: UserSessionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userSessionsRowCount$ = this.requestUserSessionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserSessions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userSessionsRowCount$);

            return userSessionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserSessionsRowCount(config: UserSessionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserSessions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserSessionsRowCount(config));
            }));
    }

    public GetUserSessionsBasicListData(config: UserSessionQueryParameters | any = null) : Observable<Array<UserSessionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userSessionsBasicListData$ = this.requestUserSessionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserSessions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userSessionsBasicListData$);

            return userSessionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserSessionBasicListData>>;
    }


    private requestUserSessionsBasicListData(config: UserSessionQueryParameters | any) : Observable<Array<UserSessionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserSessionBasicListData>>(this.baseUrl + 'api/UserSessions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserSessionsBasicListData(config));
            }));

    }


    public PutUserSession(id: bigint | number, userSession: UserSessionSubmitData) : Observable<UserSessionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserSessionData>(this.baseUrl + 'api/UserSession/' + id.toString(), userSession, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserSession(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserSession(id, userSession));
            }));
    }


    public PostUserSession(userSession: UserSessionSubmitData) : Observable<UserSessionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserSessionData>(this.baseUrl + 'api/UserSession', userSession, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserSession(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserSession(userSession));
            }));
    }

  
    public DeleteUserSession(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserSession/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserSession(id));
            }));
    }


    private getConfigHash(config: UserSessionQueryParameters | any): string {

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

    public userIsSecurityUserSessionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecurityUserSessionReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.UserSessions
        //
        if (userIsSecurityUserSessionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecurityUserSessionReader = user.readPermission >= 1;
            } else {
                userIsSecurityUserSessionReader = false;
            }
        }

        return userIsSecurityUserSessionReader;
    }


    public userIsSecurityUserSessionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecurityUserSessionWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.UserSessions
        //
        if (userIsSecurityUserSessionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecurityUserSessionWriter = user.writePermission >= 50;
          } else {
            userIsSecurityUserSessionWriter = false;
          }      
        }

        return userIsSecurityUserSessionWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full UserSessionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserSessionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserSessionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserSession(raw: any): UserSessionData {
    if (!raw) return raw;

    //
    // Create a UserSessionData object instance with correct prototype
    //
    const revived = Object.create(UserSessionData.prototype) as UserSessionData;

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
    // 2. But private methods (loadUserSessionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveUserSessionList(rawList: any[]): UserSessionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserSession(raw));
  }

}
