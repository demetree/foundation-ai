/*

   GENERATED SERVICE FOR THE SECURITYDEPARTMENTUSER TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SecurityDepartmentUser table.

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
import { SecurityDepartmentData } from './security-department.service';
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
export class SecurityDepartmentUserQueryParameters {
    securityDepartmentId: bigint | number | null | undefined = null;
    securityUserId: bigint | number | null | undefined = null;
    canRead: boolean | null | undefined = null;
    canWrite: boolean | null | undefined = null;
    canChangeHierarchy: boolean | null | undefined = null;
    canChangeOwner: boolean | null | undefined = null;
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
export class SecurityDepartmentUserSubmitData {
    id!: bigint | number;
    securityDepartmentId!: bigint | number;
    securityUserId!: bigint | number;
    canRead!: boolean;
    canWrite!: boolean;
    canChangeHierarchy!: boolean;
    canChangeOwner!: boolean;
    active!: boolean;
    deleted!: boolean;
}


export class SecurityDepartmentUserBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SecurityDepartmentUserChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `securityDepartmentUser.SecurityDepartmentUserChildren$` — use with `| async` in templates
//        • Promise:    `securityDepartmentUser.SecurityDepartmentUserChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="securityDepartmentUser.SecurityDepartmentUserChildren$ | async"`), or
//        • Access the promise getter (`securityDepartmentUser.SecurityDepartmentUserChildren` or `await securityDepartmentUser.SecurityDepartmentUserChildren`)
//    - Simply reading `securityDepartmentUser.SecurityDepartmentUserChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await securityDepartmentUser.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SecurityDepartmentUserData {
    id!: bigint | number;
    securityDepartmentId!: bigint | number;
    securityUserId!: bigint | number;
    canRead!: boolean;
    canWrite!: boolean;
    canChangeHierarchy!: boolean;
    canChangeOwner!: boolean;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    securityDepartment: SecurityDepartmentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
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
  // Promise based reload method to allow rebuilding of any SecurityDepartmentUserData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.securityDepartmentUser.Reload();
  //
  //  Non Async:
  //
  //     securityDepartmentUser[0].Reload().then(x => {
  //        this.securityDepartmentUser = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SecurityDepartmentUserService.Instance.GetSecurityDepartmentUser(this.id, includeRelations)
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
     * Updates the state of this SecurityDepartmentUserData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SecurityDepartmentUserData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SecurityDepartmentUserSubmitData {
        return SecurityDepartmentUserService.Instance.ConvertToSecurityDepartmentUserSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SecurityDepartmentUserService extends SecureEndpointBase {

    private static _instance: SecurityDepartmentUserService;
    private listCache: Map<string, Observable<Array<SecurityDepartmentUserData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SecurityDepartmentUserBasicListData>>>;
    private recordCache: Map<string, Observable<SecurityDepartmentUserData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SecurityDepartmentUserData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SecurityDepartmentUserBasicListData>>>();
        this.recordCache = new Map<string, Observable<SecurityDepartmentUserData>>();

        SecurityDepartmentUserService._instance = this;
    }

    public static get Instance(): SecurityDepartmentUserService {
      return SecurityDepartmentUserService._instance;
    }


    public ClearListCaches(config: SecurityDepartmentUserQueryParameters | null = null) {

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


    public ConvertToSecurityDepartmentUserSubmitData(data: SecurityDepartmentUserData): SecurityDepartmentUserSubmitData {

        let output = new SecurityDepartmentUserSubmitData();

        output.id = data.id;
        output.securityDepartmentId = data.securityDepartmentId;
        output.securityUserId = data.securityUserId;
        output.canRead = data.canRead;
        output.canWrite = data.canWrite;
        output.canChangeHierarchy = data.canChangeHierarchy;
        output.canChangeOwner = data.canChangeOwner;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSecurityDepartmentUser(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityDepartmentUserData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const securityDepartmentUser$ = this.requestSecurityDepartmentUser(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityDepartmentUser", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, securityDepartmentUser$);

            return securityDepartmentUser$;
        }

        return this.recordCache.get(configHash) as Observable<SecurityDepartmentUserData>;
    }

    private requestSecurityDepartmentUser(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityDepartmentUserData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SecurityDepartmentUserData>(this.baseUrl + 'api/SecurityDepartmentUser/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSecurityDepartmentUser(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityDepartmentUser(id, includeRelations));
            }));
    }

    public GetSecurityDepartmentUserList(config: SecurityDepartmentUserQueryParameters | any = null) : Observable<Array<SecurityDepartmentUserData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const securityDepartmentUserList$ = this.requestSecurityDepartmentUserList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityDepartmentUser list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, securityDepartmentUserList$);

            return securityDepartmentUserList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SecurityDepartmentUserData>>;
    }


    private requestSecurityDepartmentUserList(config: SecurityDepartmentUserQueryParameters | any) : Observable <Array<SecurityDepartmentUserData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityDepartmentUserData>>(this.baseUrl + 'api/SecurityDepartmentUsers', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSecurityDepartmentUserList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityDepartmentUserList(config));
            }));
    }

    public GetSecurityDepartmentUsersRowCount(config: SecurityDepartmentUserQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const securityDepartmentUsersRowCount$ = this.requestSecurityDepartmentUsersRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityDepartmentUsers row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, securityDepartmentUsersRowCount$);

            return securityDepartmentUsersRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSecurityDepartmentUsersRowCount(config: SecurityDepartmentUserQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SecurityDepartmentUsers/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityDepartmentUsersRowCount(config));
            }));
    }

    public GetSecurityDepartmentUsersBasicListData(config: SecurityDepartmentUserQueryParameters | any = null) : Observable<Array<SecurityDepartmentUserBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const securityDepartmentUsersBasicListData$ = this.requestSecurityDepartmentUsersBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityDepartmentUsers basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, securityDepartmentUsersBasicListData$);

            return securityDepartmentUsersBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SecurityDepartmentUserBasicListData>>;
    }


    private requestSecurityDepartmentUsersBasicListData(config: SecurityDepartmentUserQueryParameters | any) : Observable<Array<SecurityDepartmentUserBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityDepartmentUserBasicListData>>(this.baseUrl + 'api/SecurityDepartmentUsers/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityDepartmentUsersBasicListData(config));
            }));

    }


    public PutSecurityDepartmentUser(id: bigint | number, securityDepartmentUser: SecurityDepartmentUserSubmitData) : Observable<SecurityDepartmentUserData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SecurityDepartmentUserData>(this.baseUrl + 'api/SecurityDepartmentUser/' + id.toString(), securityDepartmentUser, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityDepartmentUser(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSecurityDepartmentUser(id, securityDepartmentUser));
            }));
    }


    public PostSecurityDepartmentUser(securityDepartmentUser: SecurityDepartmentUserSubmitData) : Observable<SecurityDepartmentUserData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SecurityDepartmentUserData>(this.baseUrl + 'api/SecurityDepartmentUser', securityDepartmentUser, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityDepartmentUser(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSecurityDepartmentUser(securityDepartmentUser));
            }));
    }

  
    public DeleteSecurityDepartmentUser(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SecurityDepartmentUser/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSecurityDepartmentUser(id));
            }));
    }


    private getConfigHash(config: SecurityDepartmentUserQueryParameters | any): string {

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

    public userIsSecuritySecurityDepartmentUserReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecuritySecurityDepartmentUserReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.SecurityDepartmentUsers
        //
        if (userIsSecuritySecurityDepartmentUserReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecuritySecurityDepartmentUserReader = user.readPermission >= 1;
            } else {
                userIsSecuritySecurityDepartmentUserReader = false;
            }
        }

        return userIsSecuritySecurityDepartmentUserReader;
    }


    public userIsSecuritySecurityDepartmentUserWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecuritySecurityDepartmentUserWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.SecurityDepartmentUsers
        //
        if (userIsSecuritySecurityDepartmentUserWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecuritySecurityDepartmentUserWriter = user.writePermission >= 50;
          } else {
            userIsSecuritySecurityDepartmentUserWriter = false;
          }      
        }

        return userIsSecuritySecurityDepartmentUserWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full SecurityDepartmentUserData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SecurityDepartmentUserData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SecurityDepartmentUserTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSecurityDepartmentUser(raw: any): SecurityDepartmentUserData {
    if (!raw) return raw;

    //
    // Create a SecurityDepartmentUserData object instance with correct prototype
    //
    const revived = Object.create(SecurityDepartmentUserData.prototype) as SecurityDepartmentUserData;

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
    // 2. But private methods (loadSecurityDepartmentUserXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveSecurityDepartmentUserList(rawList: any[]): SecurityDepartmentUserData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSecurityDepartmentUser(raw));
  }

}
