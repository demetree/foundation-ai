/*

   GENERATED SERVICE FOR THE SECURITYUSERPASSWORDRESETTOKEN TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SecurityUserPasswordResetToken table.

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
export class SecurityUserPasswordResetTokenQueryParameters {
    securityUserId: bigint | number | null | undefined = null;
    token: string | null | undefined = null;
    timeStamp: string | null | undefined = null;        // ISO 8601
    expiry: string | null | undefined = null;        // ISO 8601
    systemInitiated: boolean | null | undefined = null;
    completed: boolean | null | undefined = null;
    comments: string | null | undefined = null;
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
export class SecurityUserPasswordResetTokenSubmitData {
    id!: bigint | number;
    securityUserId!: bigint | number;
    token!: string;
    timeStamp!: string;      // ISO 8601
    expiry!: string;      // ISO 8601
    systemInitiated!: boolean;
    completed!: boolean;
    comments: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class SecurityUserPasswordResetTokenBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SecurityUserPasswordResetTokenChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `securityUserPasswordResetToken.SecurityUserPasswordResetTokenChildren$` — use with `| async` in templates
//        • Promise:    `securityUserPasswordResetToken.SecurityUserPasswordResetTokenChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="securityUserPasswordResetToken.SecurityUserPasswordResetTokenChildren$ | async"`), or
//        • Access the promise getter (`securityUserPasswordResetToken.SecurityUserPasswordResetTokenChildren` or `await securityUserPasswordResetToken.SecurityUserPasswordResetTokenChildren`)
//    - Simply reading `securityUserPasswordResetToken.SecurityUserPasswordResetTokenChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await securityUserPasswordResetToken.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SecurityUserPasswordResetTokenData {
    id!: bigint | number;
    securityUserId!: bigint | number;
    token!: string;
    timeStamp!: string;      // ISO 8601
    expiry!: string;      // ISO 8601
    systemInitiated!: boolean;
    completed!: boolean;
    comments!: string | null;
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
  // Promise based reload method to allow rebuilding of any SecurityUserPasswordResetTokenData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.securityUserPasswordResetToken.Reload();
  //
  //  Non Async:
  //
  //     securityUserPasswordResetToken[0].Reload().then(x => {
  //        this.securityUserPasswordResetToken = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SecurityUserPasswordResetTokenService.Instance.GetSecurityUserPasswordResetToken(this.id, includeRelations)
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
     * Updates the state of this SecurityUserPasswordResetTokenData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SecurityUserPasswordResetTokenData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SecurityUserPasswordResetTokenSubmitData {
        return SecurityUserPasswordResetTokenService.Instance.ConvertToSecurityUserPasswordResetTokenSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SecurityUserPasswordResetTokenService extends SecureEndpointBase {

    private static _instance: SecurityUserPasswordResetTokenService;
    private listCache: Map<string, Observable<Array<SecurityUserPasswordResetTokenData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SecurityUserPasswordResetTokenBasicListData>>>;
    private recordCache: Map<string, Observable<SecurityUserPasswordResetTokenData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SecurityUserPasswordResetTokenData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SecurityUserPasswordResetTokenBasicListData>>>();
        this.recordCache = new Map<string, Observable<SecurityUserPasswordResetTokenData>>();

        SecurityUserPasswordResetTokenService._instance = this;
    }

    public static get Instance(): SecurityUserPasswordResetTokenService {
      return SecurityUserPasswordResetTokenService._instance;
    }


    public ClearListCaches(config: SecurityUserPasswordResetTokenQueryParameters | null = null) {

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


    public ConvertToSecurityUserPasswordResetTokenSubmitData(data: SecurityUserPasswordResetTokenData): SecurityUserPasswordResetTokenSubmitData {

        let output = new SecurityUserPasswordResetTokenSubmitData();

        output.id = data.id;
        output.securityUserId = data.securityUserId;
        output.token = data.token;
        output.timeStamp = data.timeStamp;
        output.expiry = data.expiry;
        output.systemInitiated = data.systemInitiated;
        output.completed = data.completed;
        output.comments = data.comments;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSecurityUserPasswordResetToken(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityUserPasswordResetTokenData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const securityUserPasswordResetToken$ = this.requestSecurityUserPasswordResetToken(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUserPasswordResetToken", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, securityUserPasswordResetToken$);

            return securityUserPasswordResetToken$;
        }

        return this.recordCache.get(configHash) as Observable<SecurityUserPasswordResetTokenData>;
    }

    private requestSecurityUserPasswordResetToken(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityUserPasswordResetTokenData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SecurityUserPasswordResetTokenData>(this.baseUrl + 'api/SecurityUserPasswordResetToken/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSecurityUserPasswordResetToken(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserPasswordResetToken(id, includeRelations));
            }));
    }

    public GetSecurityUserPasswordResetTokenList(config: SecurityUserPasswordResetTokenQueryParameters | any = null) : Observable<Array<SecurityUserPasswordResetTokenData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const securityUserPasswordResetTokenList$ = this.requestSecurityUserPasswordResetTokenList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUserPasswordResetToken list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, securityUserPasswordResetTokenList$);

            return securityUserPasswordResetTokenList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SecurityUserPasswordResetTokenData>>;
    }


    private requestSecurityUserPasswordResetTokenList(config: SecurityUserPasswordResetTokenQueryParameters | any) : Observable <Array<SecurityUserPasswordResetTokenData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityUserPasswordResetTokenData>>(this.baseUrl + 'api/SecurityUserPasswordResetTokens', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSecurityUserPasswordResetTokenList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserPasswordResetTokenList(config));
            }));
    }

    public GetSecurityUserPasswordResetTokensRowCount(config: SecurityUserPasswordResetTokenQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const securityUserPasswordResetTokensRowCount$ = this.requestSecurityUserPasswordResetTokensRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUserPasswordResetTokens row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, securityUserPasswordResetTokensRowCount$);

            return securityUserPasswordResetTokensRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSecurityUserPasswordResetTokensRowCount(config: SecurityUserPasswordResetTokenQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SecurityUserPasswordResetTokens/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserPasswordResetTokensRowCount(config));
            }));
    }

    public GetSecurityUserPasswordResetTokensBasicListData(config: SecurityUserPasswordResetTokenQueryParameters | any = null) : Observable<Array<SecurityUserPasswordResetTokenBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const securityUserPasswordResetTokensBasicListData$ = this.requestSecurityUserPasswordResetTokensBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUserPasswordResetTokens basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, securityUserPasswordResetTokensBasicListData$);

            return securityUserPasswordResetTokensBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SecurityUserPasswordResetTokenBasicListData>>;
    }


    private requestSecurityUserPasswordResetTokensBasicListData(config: SecurityUserPasswordResetTokenQueryParameters | any) : Observable<Array<SecurityUserPasswordResetTokenBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityUserPasswordResetTokenBasicListData>>(this.baseUrl + 'api/SecurityUserPasswordResetTokens/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserPasswordResetTokensBasicListData(config));
            }));

    }


    public PutSecurityUserPasswordResetToken(id: bigint | number, securityUserPasswordResetToken: SecurityUserPasswordResetTokenSubmitData) : Observable<SecurityUserPasswordResetTokenData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SecurityUserPasswordResetTokenData>(this.baseUrl + 'api/SecurityUserPasswordResetToken/' + id.toString(), securityUserPasswordResetToken, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityUserPasswordResetToken(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSecurityUserPasswordResetToken(id, securityUserPasswordResetToken));
            }));
    }


    public PostSecurityUserPasswordResetToken(securityUserPasswordResetToken: SecurityUserPasswordResetTokenSubmitData) : Observable<SecurityUserPasswordResetTokenData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SecurityUserPasswordResetTokenData>(this.baseUrl + 'api/SecurityUserPasswordResetToken', securityUserPasswordResetToken, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityUserPasswordResetToken(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSecurityUserPasswordResetToken(securityUserPasswordResetToken));
            }));
    }

  
    public DeleteSecurityUserPasswordResetToken(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SecurityUserPasswordResetToken/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSecurityUserPasswordResetToken(id));
            }));
    }


    private getConfigHash(config: SecurityUserPasswordResetTokenQueryParameters | any): string {

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

    public userIsSecuritySecurityUserPasswordResetTokenReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecuritySecurityUserPasswordResetTokenReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.SecurityUserPasswordResetTokens
        //
        if (userIsSecuritySecurityUserPasswordResetTokenReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecuritySecurityUserPasswordResetTokenReader = user.readPermission >= 0;
            } else {
                userIsSecuritySecurityUserPasswordResetTokenReader = false;
            }
        }

        return userIsSecuritySecurityUserPasswordResetTokenReader;
    }


    public userIsSecuritySecurityUserPasswordResetTokenWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecuritySecurityUserPasswordResetTokenWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.SecurityUserPasswordResetTokens
        //
        if (userIsSecuritySecurityUserPasswordResetTokenWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecuritySecurityUserPasswordResetTokenWriter = user.writePermission >= 0;
          } else {
            userIsSecuritySecurityUserPasswordResetTokenWriter = false;
          }      
        }

        return userIsSecuritySecurityUserPasswordResetTokenWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full SecurityUserPasswordResetTokenData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SecurityUserPasswordResetTokenData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SecurityUserPasswordResetTokenTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSecurityUserPasswordResetToken(raw: any): SecurityUserPasswordResetTokenData {
    if (!raw) return raw;

    //
    // Create a SecurityUserPasswordResetTokenData object instance with correct prototype
    //
    const revived = Object.create(SecurityUserPasswordResetTokenData.prototype) as SecurityUserPasswordResetTokenData;

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
    // 2. But private methods (loadSecurityUserPasswordResetTokenXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveSecurityUserPasswordResetTokenList(rawList: any[]): SecurityUserPasswordResetTokenData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSecurityUserPasswordResetToken(raw));
  }

}
