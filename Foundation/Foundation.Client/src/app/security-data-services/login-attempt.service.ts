/*

   GENERATED SERVICE FOR THE LOGINATTEMPT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the LoginAttempt table.

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
export class LoginAttemptQueryParameters {
    timeStamp: string | null | undefined = null;        // ISO 8601 (full datetime)
    userName: string | null | undefined = null;
    passwordHash: bigint | number | null | undefined = null;
    resource: string | null | undefined = null;
    sessionId: string | null | undefined = null;
    ipAddress: string | null | undefined = null;
    userAgent: string | null | undefined = null;
    value: string | null | undefined = null;
    success: boolean | null | undefined = null;
    securityUserId: bigint | number | null | undefined = null;
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
export class LoginAttemptSubmitData {
    id!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userName: string | null = null;
    passwordHash: bigint | number | null = null;
    resource: string | null = null;
    sessionId: string | null = null;
    ipAddress: string | null = null;
    userAgent: string | null = null;
    value: string | null = null;
    success: boolean | null = null;
    securityUserId: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class LoginAttemptBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. LoginAttemptChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `loginAttempt.LoginAttemptChildren$` — use with `| async` in templates
//        • Promise:    `loginAttempt.LoginAttemptChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="loginAttempt.LoginAttemptChildren$ | async"`), or
//        • Access the promise getter (`loginAttempt.LoginAttemptChildren` or `await loginAttempt.LoginAttemptChildren`)
//    - Simply reading `loginAttempt.LoginAttemptChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await loginAttempt.Reload()` to refresh the entire object and clear all lazy caches.
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
export class LoginAttemptData {
    id!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userName!: string | null;
    passwordHash!: bigint | number;
    resource!: string | null;
    sessionId!: string | null;
    ipAddress!: string | null;
    userAgent!: string | null;
    value!: string | null;
    success!: boolean | null;
    securityUserId!: bigint | number;
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
  // Promise based reload method to allow rebuilding of any LoginAttemptData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.loginAttempt.Reload();
  //
  //  Non Async:
  //
  //     loginAttempt[0].Reload().then(x => {
  //        this.loginAttempt = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      LoginAttemptService.Instance.GetLoginAttempt(this.id, includeRelations)
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
     * Updates the state of this LoginAttemptData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this LoginAttemptData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): LoginAttemptSubmitData {
        return LoginAttemptService.Instance.ConvertToLoginAttemptSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class LoginAttemptService extends SecureEndpointBase {

    private static _instance: LoginAttemptService;
    private listCache: Map<string, Observable<Array<LoginAttemptData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<LoginAttemptBasicListData>>>;
    private recordCache: Map<string, Observable<LoginAttemptData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<LoginAttemptData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<LoginAttemptBasicListData>>>();
        this.recordCache = new Map<string, Observable<LoginAttemptData>>();

        LoginAttemptService._instance = this;
    }

    public static get Instance(): LoginAttemptService {
      return LoginAttemptService._instance;
    }


    public ClearListCaches(config: LoginAttemptQueryParameters | null = null) {

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


    public ConvertToLoginAttemptSubmitData(data: LoginAttemptData): LoginAttemptSubmitData {

        let output = new LoginAttemptSubmitData();

        output.id = data.id;
        output.timeStamp = data.timeStamp;
        output.userName = data.userName;
        output.passwordHash = data.passwordHash;
        output.resource = data.resource;
        output.sessionId = data.sessionId;
        output.ipAddress = data.ipAddress;
        output.userAgent = data.userAgent;
        output.value = data.value;
        output.success = data.success;
        output.securityUserId = data.securityUserId;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetLoginAttempt(id: bigint | number, includeRelations: boolean = true) : Observable<LoginAttemptData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const loginAttempt$ = this.requestLoginAttempt(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LoginAttempt", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, loginAttempt$);

            return loginAttempt$;
        }

        return this.recordCache.get(configHash) as Observable<LoginAttemptData>;
    }

    private requestLoginAttempt(id: bigint | number, includeRelations: boolean = true) : Observable<LoginAttemptData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<LoginAttemptData>(this.baseUrl + 'api/LoginAttempt/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveLoginAttempt(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestLoginAttempt(id, includeRelations));
            }));
    }

    public GetLoginAttemptList(config: LoginAttemptQueryParameters | any = null) : Observable<Array<LoginAttemptData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const loginAttemptList$ = this.requestLoginAttemptList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LoginAttempt list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, loginAttemptList$);

            return loginAttemptList$;
        }

        return this.listCache.get(configHash) as Observable<Array<LoginAttemptData>>;
    }


    private requestLoginAttemptList(config: LoginAttemptQueryParameters | any) : Observable <Array<LoginAttemptData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LoginAttemptData>>(this.baseUrl + 'api/LoginAttempts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveLoginAttemptList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestLoginAttemptList(config));
            }));
    }

    public GetLoginAttemptsRowCount(config: LoginAttemptQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const loginAttemptsRowCount$ = this.requestLoginAttemptsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LoginAttempts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, loginAttemptsRowCount$);

            return loginAttemptsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestLoginAttemptsRowCount(config: LoginAttemptQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/LoginAttempts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLoginAttemptsRowCount(config));
            }));
    }

    public GetLoginAttemptsBasicListData(config: LoginAttemptQueryParameters | any = null) : Observable<Array<LoginAttemptBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const loginAttemptsBasicListData$ = this.requestLoginAttemptsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LoginAttempts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, loginAttemptsBasicListData$);

            return loginAttemptsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<LoginAttemptBasicListData>>;
    }


    private requestLoginAttemptsBasicListData(config: LoginAttemptQueryParameters | any) : Observable<Array<LoginAttemptBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LoginAttemptBasicListData>>(this.baseUrl + 'api/LoginAttempts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLoginAttemptsBasicListData(config));
            }));

    }


    public PutLoginAttempt(id: bigint | number, loginAttempt: LoginAttemptSubmitData) : Observable<LoginAttemptData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<LoginAttemptData>(this.baseUrl + 'api/LoginAttempt/' + id.toString(), loginAttempt, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLoginAttempt(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutLoginAttempt(id, loginAttempt));
            }));
    }


    public PostLoginAttempt(loginAttempt: LoginAttemptSubmitData) : Observable<LoginAttemptData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<LoginAttemptData>(this.baseUrl + 'api/LoginAttempt', loginAttempt, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLoginAttempt(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostLoginAttempt(loginAttempt));
            }));
    }

  
    public DeleteLoginAttempt(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/LoginAttempt/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteLoginAttempt(id));
            }));
    }


    private getConfigHash(config: LoginAttemptQueryParameters | any): string {

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

    public userIsSecurityLoginAttemptReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecurityLoginAttemptReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.LoginAttempts
        //
        if (userIsSecurityLoginAttemptReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecurityLoginAttemptReader = user.readPermission >= 1;
            } else {
                userIsSecurityLoginAttemptReader = false;
            }
        }

        return userIsSecurityLoginAttemptReader;
    }


    public userIsSecurityLoginAttemptWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecurityLoginAttemptWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.LoginAttempts
        //
        if (userIsSecurityLoginAttemptWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecurityLoginAttemptWriter = user.writePermission >= 255;
          } else {
            userIsSecurityLoginAttemptWriter = false;
          }      
        }

        return userIsSecurityLoginAttemptWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full LoginAttemptData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the LoginAttemptData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when LoginAttemptTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveLoginAttempt(raw: any): LoginAttemptData {
    if (!raw) return raw;

    //
    // Create a LoginAttemptData object instance with correct prototype
    //
    const revived = Object.create(LoginAttemptData.prototype) as LoginAttemptData;

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
    // 2. But private methods (loadLoginAttemptXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveLoginAttemptList(rawList: any[]): LoginAttemptData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveLoginAttempt(raw));
  }

}
