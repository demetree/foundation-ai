/*

   GENERATED SERVICE FOR THE SECURITYTEAM TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SecurityTeam table.

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
import { SecurityUserService, SecurityUserData } from './security-user.service';
import { SecurityTeamUserService, SecurityTeamUserData } from './security-team-user.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SecurityTeamQueryParameters {
    securityDepartmentId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class SecurityTeamSubmitData {
    id!: bigint | number;
    securityDepartmentId!: bigint | number;
    name!: string;
    description: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class SecurityTeamBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SecurityTeamChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `securityTeam.SecurityTeamChildren$` — use with `| async` in templates
//        • Promise:    `securityTeam.SecurityTeamChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="securityTeam.SecurityTeamChildren$ | async"`), or
//        • Access the promise getter (`securityTeam.SecurityTeamChildren` or `await securityTeam.SecurityTeamChildren`)
//    - Simply reading `securityTeam.SecurityTeamChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await securityTeam.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SecurityTeamData {
    id!: bigint | number;
    securityDepartmentId!: bigint | number;
    name!: string;
    description!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    securityDepartment: SecurityDepartmentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _securityUsers: SecurityUserData[] | null = null;
    private _securityUsersPromise: Promise<SecurityUserData[]> | null  = null;
    private _securityUsersSubject = new BehaviorSubject<SecurityUserData[] | null>(null);

                
    private _securityTeamUsers: SecurityTeamUserData[] | null = null;
    private _securityTeamUsersPromise: Promise<SecurityTeamUserData[]> | null  = null;
    private _securityTeamUsersSubject = new BehaviorSubject<SecurityTeamUserData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SecurityUsers$ = this._securityUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityUsers === null && this._securityUsersPromise === null) {
            this.loadSecurityUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityUsersCount$ = SecurityUserService.Instance.GetSecurityUsersRowCount({securityTeamId: this.id,
      active: true,
      deleted: false
    });



    public SecurityTeamUsers$ = this._securityTeamUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityTeamUsers === null && this._securityTeamUsersPromise === null) {
            this.loadSecurityTeamUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityTeamUsersCount$ = SecurityTeamUserService.Instance.GetSecurityTeamUsersRowCount({securityTeamId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SecurityTeamData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.securityTeam.Reload();
  //
  //  Non Async:
  //
  //     securityTeam[0].Reload().then(x => {
  //        this.securityTeam = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SecurityTeamService.Instance.GetSecurityTeam(this.id, includeRelations)
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
     this._securityUsers = null;
     this._securityUsersPromise = null;
     this._securityUsersSubject.next(null);

     this._securityTeamUsers = null;
     this._securityTeamUsersPromise = null;
     this._securityTeamUsersSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the SecurityUsers for this SecurityTeam.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityTeam.SecurityUsers.then(securityTeams => { ... })
     *   or
     *   await this.securityTeam.securityTeams
     *
    */
    public get SecurityUsers(): Promise<SecurityUserData[]> {
        if (this._securityUsers !== null) {
            return Promise.resolve(this._securityUsers);
        }

        if (this._securityUsersPromise !== null) {
            return this._securityUsersPromise;
        }

        // Start the load
        this.loadSecurityUsers();

        return this._securityUsersPromise!;
    }



    private loadSecurityUsers(): void {

        this._securityUsersPromise = lastValueFrom(
            SecurityTeamService.Instance.GetSecurityUsersForSecurityTeam(this.id)
        )
        .then(SecurityUsers => {
            this._securityUsers = SecurityUsers ?? [];
            this._securityUsersSubject.next(this._securityUsers);
            return this._securityUsers;
         })
        .catch(err => {
            this._securityUsers = [];
            this._securityUsersSubject.next(this._securityUsers);
            throw err;
        })
        .finally(() => {
            this._securityUsersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SecurityUser. Call after mutations to force refresh.
     */
    public ClearSecurityUsersCache(): void {
        this._securityUsers = null;
        this._securityUsersPromise = null;
        this._securityUsersSubject.next(this._securityUsers);      // Emit to observable
    }

    public get HasSecurityUsers(): Promise<boolean> {
        return this.SecurityUsers.then(securityUsers => securityUsers.length > 0);
    }


    /**
     *
     * Gets the SecurityTeamUsers for this SecurityTeam.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityTeam.SecurityTeamUsers.then(securityTeams => { ... })
     *   or
     *   await this.securityTeam.securityTeams
     *
    */
    public get SecurityTeamUsers(): Promise<SecurityTeamUserData[]> {
        if (this._securityTeamUsers !== null) {
            return Promise.resolve(this._securityTeamUsers);
        }

        if (this._securityTeamUsersPromise !== null) {
            return this._securityTeamUsersPromise;
        }

        // Start the load
        this.loadSecurityTeamUsers();

        return this._securityTeamUsersPromise!;
    }



    private loadSecurityTeamUsers(): void {

        this._securityTeamUsersPromise = lastValueFrom(
            SecurityTeamService.Instance.GetSecurityTeamUsersForSecurityTeam(this.id)
        )
        .then(SecurityTeamUsers => {
            this._securityTeamUsers = SecurityTeamUsers ?? [];
            this._securityTeamUsersSubject.next(this._securityTeamUsers);
            return this._securityTeamUsers;
         })
        .catch(err => {
            this._securityTeamUsers = [];
            this._securityTeamUsersSubject.next(this._securityTeamUsers);
            throw err;
        })
        .finally(() => {
            this._securityTeamUsersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SecurityTeamUser. Call after mutations to force refresh.
     */
    public ClearSecurityTeamUsersCache(): void {
        this._securityTeamUsers = null;
        this._securityTeamUsersPromise = null;
        this._securityTeamUsersSubject.next(this._securityTeamUsers);      // Emit to observable
    }

    public get HasSecurityTeamUsers(): Promise<boolean> {
        return this.SecurityTeamUsers.then(securityTeamUsers => securityTeamUsers.length > 0);
    }




    /**
     * Updates the state of this SecurityTeamData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SecurityTeamData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SecurityTeamSubmitData {
        return SecurityTeamService.Instance.ConvertToSecurityTeamSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SecurityTeamService extends SecureEndpointBase {

    private static _instance: SecurityTeamService;
    private listCache: Map<string, Observable<Array<SecurityTeamData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SecurityTeamBasicListData>>>;
    private recordCache: Map<string, Observable<SecurityTeamData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private securityUserService: SecurityUserService,
        private securityTeamUserService: SecurityTeamUserService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SecurityTeamData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SecurityTeamBasicListData>>>();
        this.recordCache = new Map<string, Observable<SecurityTeamData>>();

        SecurityTeamService._instance = this;
    }

    public static get Instance(): SecurityTeamService {
      return SecurityTeamService._instance;
    }


    public ClearListCaches(config: SecurityTeamQueryParameters | null = null) {

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


    public ConvertToSecurityTeamSubmitData(data: SecurityTeamData): SecurityTeamSubmitData {

        let output = new SecurityTeamSubmitData();

        output.id = data.id;
        output.securityDepartmentId = data.securityDepartmentId;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSecurityTeam(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityTeamData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const securityTeam$ = this.requestSecurityTeam(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityTeam", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, securityTeam$);

            return securityTeam$;
        }

        return this.recordCache.get(configHash) as Observable<SecurityTeamData>;
    }

    private requestSecurityTeam(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityTeamData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SecurityTeamData>(this.baseUrl + 'api/SecurityTeam/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSecurityTeam(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityTeam(id, includeRelations));
            }));
    }

    public GetSecurityTeamList(config: SecurityTeamQueryParameters | any = null) : Observable<Array<SecurityTeamData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const securityTeamList$ = this.requestSecurityTeamList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityTeam list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, securityTeamList$);

            return securityTeamList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SecurityTeamData>>;
    }


    private requestSecurityTeamList(config: SecurityTeamQueryParameters | any) : Observable <Array<SecurityTeamData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityTeamData>>(this.baseUrl + 'api/SecurityTeams', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSecurityTeamList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityTeamList(config));
            }));
    }

    public GetSecurityTeamsRowCount(config: SecurityTeamQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const securityTeamsRowCount$ = this.requestSecurityTeamsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityTeams row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, securityTeamsRowCount$);

            return securityTeamsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSecurityTeamsRowCount(config: SecurityTeamQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SecurityTeams/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityTeamsRowCount(config));
            }));
    }

    public GetSecurityTeamsBasicListData(config: SecurityTeamQueryParameters | any = null) : Observable<Array<SecurityTeamBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const securityTeamsBasicListData$ = this.requestSecurityTeamsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityTeams basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, securityTeamsBasicListData$);

            return securityTeamsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SecurityTeamBasicListData>>;
    }


    private requestSecurityTeamsBasicListData(config: SecurityTeamQueryParameters | any) : Observable<Array<SecurityTeamBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityTeamBasicListData>>(this.baseUrl + 'api/SecurityTeams/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityTeamsBasicListData(config));
            }));

    }


    public PutSecurityTeam(id: bigint | number, securityTeam: SecurityTeamSubmitData) : Observable<SecurityTeamData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SecurityTeamData>(this.baseUrl + 'api/SecurityTeam/' + id.toString(), securityTeam, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityTeam(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSecurityTeam(id, securityTeam));
            }));
    }


    public PostSecurityTeam(securityTeam: SecurityTeamSubmitData) : Observable<SecurityTeamData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SecurityTeamData>(this.baseUrl + 'api/SecurityTeam', securityTeam, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityTeam(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSecurityTeam(securityTeam));
            }));
    }

  
    public DeleteSecurityTeam(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SecurityTeam/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSecurityTeam(id));
            }));
    }


    private getConfigHash(config: SecurityTeamQueryParameters | any): string {

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

    public userIsSecuritySecurityTeamReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecuritySecurityTeamReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.SecurityTeams
        //
        if (userIsSecuritySecurityTeamReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecuritySecurityTeamReader = user.readPermission >= 0;
            } else {
                userIsSecuritySecurityTeamReader = false;
            }
        }

        return userIsSecuritySecurityTeamReader;
    }


    public userIsSecuritySecurityTeamWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecuritySecurityTeamWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.SecurityTeams
        //
        if (userIsSecuritySecurityTeamWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecuritySecurityTeamWriter = user.writePermission >= 50;
          } else {
            userIsSecuritySecurityTeamWriter = false;
          }      
        }

        return userIsSecuritySecurityTeamWriter;
    }

    public GetSecurityUsersForSecurityTeam(securityTeamId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityUserData[]> {
        return this.securityUserService.GetSecurityUserList({
            securityTeamId: securityTeamId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityTeamUsersForSecurityTeam(securityTeamId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityTeamUserData[]> {
        return this.securityTeamUserService.GetSecurityTeamUserList({
            securityTeamId: securityTeamId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SecurityTeamData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SecurityTeamData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SecurityTeamTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSecurityTeam(raw: any): SecurityTeamData {
    if (!raw) return raw;

    //
    // Create a SecurityTeamData object instance with correct prototype
    //
    const revived = Object.create(SecurityTeamData.prototype) as SecurityTeamData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._securityUsers = null;
    (revived as any)._securityUsersPromise = null;
    (revived as any)._securityUsersSubject = new BehaviorSubject<SecurityUserData[] | null>(null);

    (revived as any)._securityTeamUsers = null;
    (revived as any)._securityTeamUsersPromise = null;
    (revived as any)._securityTeamUsersSubject = new BehaviorSubject<SecurityTeamUserData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSecurityTeamXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SecurityUsers$ = (revived as any)._securityUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityUsers === null && (revived as any)._securityUsersPromise === null) {
                (revived as any).loadSecurityUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityUsersCount$ = SecurityUserService.Instance.GetSecurityUsersRowCount({securityTeamId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SecurityTeamUsers$ = (revived as any)._securityTeamUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityTeamUsers === null && (revived as any)._securityTeamUsersPromise === null) {
                (revived as any).loadSecurityTeamUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityTeamUsersCount$ = SecurityTeamUserService.Instance.GetSecurityTeamUsersRowCount({securityTeamId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveSecurityTeamList(rawList: any[]): SecurityTeamData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSecurityTeam(raw));
  }

}
