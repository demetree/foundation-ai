import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, BehaviorSubject, catchError, throwError, lastValueFrom, map  } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';
import { PrivilegeData } from './privilege.service';
import { SecurityUserSecurityRoleService, SecurityUserSecurityRoleData } from './security-user-security-role.service';
import { SecurityGroupSecurityRoleService, SecurityGroupSecurityRoleData } from './security-group-security-role.service';
import { ModuleSecurityRoleService, ModuleSecurityRoleData } from './module-security-role.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SecurityRoleQueryParameters {
    privilegeId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class SecurityRoleSubmitData {
    id!: bigint | number;
    privilegeId!: bigint | number;
    name!: string;
    description: string | null = null;
    comments: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class SecurityRoleBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SecurityRoleChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `securityRole.SecurityRoleChildren$` — use with `| async` in templates
//        • Promise:    `securityRole.SecurityRoleChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="securityRole.SecurityRoleChildren$ | async"`), or
//        • Access the promise getter (`securityRole.SecurityRoleChildren` or `await securityRole.SecurityRoleChildren`)
//    - Simply reading `securityRole.SecurityRoleChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await securityRole.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SecurityRoleData {
    id!: bigint | number;
    privilegeId!: bigint | number;
    name!: string;
    description!: string | null;
    comments!: string | null;
    active!: boolean;
    deleted!: boolean;
    privilege: PrivilegeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _securityUserSecurityRoles: SecurityUserSecurityRoleData[] | null = null;
    private _securityUserSecurityRolesPromise: Promise<SecurityUserSecurityRoleData[]> | null  = null;
    private _securityUserSecurityRolesSubject = new BehaviorSubject<SecurityUserSecurityRoleData[] | null>(null);

    private _securityGroupSecurityRoles: SecurityGroupSecurityRoleData[] | null = null;
    private _securityGroupSecurityRolesPromise: Promise<SecurityGroupSecurityRoleData[]> | null  = null;
    private _securityGroupSecurityRolesSubject = new BehaviorSubject<SecurityGroupSecurityRoleData[] | null>(null);

    private _moduleSecurityRoles: ModuleSecurityRoleData[] | null = null;
    private _moduleSecurityRolesPromise: Promise<ModuleSecurityRoleData[]> | null  = null;
    private _moduleSecurityRolesSubject = new BehaviorSubject<ModuleSecurityRoleData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SecurityUserSecurityRoles$ = this._securityUserSecurityRolesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityUserSecurityRoles === null && this._securityUserSecurityRolesPromise === null) {
            this.loadSecurityUserSecurityRoles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityUserSecurityRolesCount$ = SecurityUserSecurityRoleService.Instance.GetSecurityUserSecurityRolesRowCount({securityRoleId: this.id,
      active: true,
      deleted: false
    });



    public SecurityGroupSecurityRoles$ = this._securityGroupSecurityRolesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityGroupSecurityRoles === null && this._securityGroupSecurityRolesPromise === null) {
            this.loadSecurityGroupSecurityRoles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityGroupSecurityRolesCount$ = SecurityGroupSecurityRoleService.Instance.GetSecurityGroupSecurityRolesRowCount({securityRoleId: this.id,
      active: true,
      deleted: false
    });



    public ModuleSecurityRoles$ = this._moduleSecurityRolesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._moduleSecurityRoles === null && this._moduleSecurityRolesPromise === null) {
            this.loadModuleSecurityRoles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ModuleSecurityRolesCount$ = ModuleSecurityRoleService.Instance.GetModuleSecurityRolesRowCount({securityRoleId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SecurityRoleData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.securityRole.Reload();
  //
  //  Non Async:
  //
  //     securityRole[0].Reload().then(x => {
  //        this.securityRole = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SecurityRoleService.Instance.GetSecurityRole(this.id, includeRelations)
    );

    // Merge fresh data into this instance (preserves reference)
    this.UpdateFrom(fresh as this);

    // Clear all lazy caches to force re-load on next access
    this.clearAllLazyCaches();

    return this;
  }


  private clearAllLazyCaches(): void {
     // Reset every collection cache and notify subscribers
     this._securityUserSecurityRoles = null;
     this._securityUserSecurityRolesPromise = null;
     this._securityUserSecurityRolesSubject.next(null);

     this._securityGroupSecurityRoles = null;
     this._securityGroupSecurityRolesPromise = null;
     this._securityGroupSecurityRolesSubject.next(null);

     this._moduleSecurityRoles = null;
     this._moduleSecurityRolesPromise = null;
     this._moduleSecurityRolesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the SecurityUserSecurityRoles for this SecurityRole.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityRole.SecurityUserSecurityRoles.then(securityUserSecurityRoles => { ... })
     *   or
     *   await this.securityRole.SecurityUserSecurityRoles
     *
    */
    public get SecurityUserSecurityRoles(): Promise<SecurityUserSecurityRoleData[]> {
        if (this._securityUserSecurityRoles !== null) {
            return Promise.resolve(this._securityUserSecurityRoles);
        }

        if (this._securityUserSecurityRolesPromise !== null) {
            return this._securityUserSecurityRolesPromise;
        }

        // Start the load
        this.loadSecurityUserSecurityRoles();

        return this._securityUserSecurityRolesPromise!;
    }



    private loadSecurityUserSecurityRoles(): void {

        this._securityUserSecurityRolesPromise = lastValueFrom(
            SecurityRoleService.Instance.GetSecurityUserSecurityRolesForSecurityRole(this.id)
        )
        .then(securityUserSecurityRoles => {
            this._securityUserSecurityRoles = securityUserSecurityRoles ?? [];
            this._securityUserSecurityRolesSubject.next(this._securityUserSecurityRoles);
            return this._securityUserSecurityRoles;
         })
        .catch(err => {
            this._securityUserSecurityRoles = [];
            this._securityUserSecurityRolesSubject.next(this._securityUserSecurityRoles);
            throw err;
        })
        .finally(() => {
            this._securityUserSecurityRolesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearSecurityUserSecurityRolesCache(): void {
        this._securityUserSecurityRoles = null;
        this._securityUserSecurityRolesPromise = null;
        this._securityUserSecurityRolesSubject.next(this._securityUserSecurityRoles);      // Emit to observable
    }

    public get HasSecurityUserSecurityRoles(): Promise<boolean> {
        return this.SecurityUserSecurityRoles.then(securityUserSecurityRoles => securityUserSecurityRoles.length > 0);
    }


    /**
     *
     * Gets the SecurityGroupSecurityRoles for this SecurityRole.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityRole.SecurityGroupSecurityRoles.then(securityGroupSecurityRoles => { ... })
     *   or
     *   await this.securityRole.SecurityGroupSecurityRoles
     *
    */
    public get SecurityGroupSecurityRoles(): Promise<SecurityGroupSecurityRoleData[]> {
        if (this._securityGroupSecurityRoles !== null) {
            return Promise.resolve(this._securityGroupSecurityRoles);
        }

        if (this._securityGroupSecurityRolesPromise !== null) {
            return this._securityGroupSecurityRolesPromise;
        }

        // Start the load
        this.loadSecurityGroupSecurityRoles();

        return this._securityGroupSecurityRolesPromise!;
    }



    private loadSecurityGroupSecurityRoles(): void {

        this._securityGroupSecurityRolesPromise = lastValueFrom(
            SecurityRoleService.Instance.GetSecurityGroupSecurityRolesForSecurityRole(this.id)
        )
        .then(securityGroupSecurityRoles => {
            this._securityGroupSecurityRoles = securityGroupSecurityRoles ?? [];
            this._securityGroupSecurityRolesSubject.next(this._securityGroupSecurityRoles);
            return this._securityGroupSecurityRoles;
         })
        .catch(err => {
            this._securityGroupSecurityRoles = [];
            this._securityGroupSecurityRolesSubject.next(this._securityGroupSecurityRoles);
            throw err;
        })
        .finally(() => {
            this._securityGroupSecurityRolesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearSecurityGroupSecurityRolesCache(): void {
        this._securityGroupSecurityRoles = null;
        this._securityGroupSecurityRolesPromise = null;
        this._securityGroupSecurityRolesSubject.next(this._securityGroupSecurityRoles);      // Emit to observable
    }

    public get HasSecurityGroupSecurityRoles(): Promise<boolean> {
        return this.SecurityGroupSecurityRoles.then(securityGroupSecurityRoles => securityGroupSecurityRoles.length > 0);
    }


    /**
     *
     * Gets the ModuleSecurityRoles for this SecurityRole.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityRole.ModuleSecurityRoles.then(moduleSecurityRoles => { ... })
     *   or
     *   await this.securityRole.ModuleSecurityRoles
     *
    */
    public get ModuleSecurityRoles(): Promise<ModuleSecurityRoleData[]> {
        if (this._moduleSecurityRoles !== null) {
            return Promise.resolve(this._moduleSecurityRoles);
        }

        if (this._moduleSecurityRolesPromise !== null) {
            return this._moduleSecurityRolesPromise;
        }

        // Start the load
        this.loadModuleSecurityRoles();

        return this._moduleSecurityRolesPromise!;
    }



    private loadModuleSecurityRoles(): void {

        this._moduleSecurityRolesPromise = lastValueFrom(
            SecurityRoleService.Instance.GetModuleSecurityRolesForSecurityRole(this.id)
        )
        .then(moduleSecurityRoles => {
            this._moduleSecurityRoles = moduleSecurityRoles ?? [];
            this._moduleSecurityRolesSubject.next(this._moduleSecurityRoles);
            return this._moduleSecurityRoles;
         })
        .catch(err => {
            this._moduleSecurityRoles = [];
            this._moduleSecurityRolesSubject.next(this._moduleSecurityRoles);
            throw err;
        })
        .finally(() => {
            this._moduleSecurityRolesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearModuleSecurityRolesCache(): void {
        this._moduleSecurityRoles = null;
        this._moduleSecurityRolesPromise = null;
        this._moduleSecurityRolesSubject.next(this._moduleSecurityRoles);      // Emit to observable
    }

    public get HasModuleSecurityRoles(): Promise<boolean> {
        return this.ModuleSecurityRoles.then(moduleSecurityRoles => moduleSecurityRoles.length > 0);
    }




    /**
     * Updates the state of this SecurityRoleData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SecurityRoleData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SecurityRoleSubmitData {
        return SecurityRoleService.Instance.ConvertToSecurityRoleSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SecurityRoleService extends SecureEndpointBase {

    private static _instance: SecurityRoleService;
    private listCache: Map<string, Observable<Array<SecurityRoleData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SecurityRoleBasicListData>>>;
    private recordCache: Map<string, Observable<SecurityRoleData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private securityUserSecurityRoleService: SecurityUserSecurityRoleService,
        private securityGroupSecurityRoleService: SecurityGroupSecurityRoleService,
        private moduleSecurityRoleService: ModuleSecurityRoleService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SecurityRoleData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SecurityRoleBasicListData>>>();
        this.recordCache = new Map<string, Observable<SecurityRoleData>>();

        SecurityRoleService._instance = this;
    }

    public static get Instance(): SecurityRoleService {
      return SecurityRoleService._instance;
    }


    public ClearListCaches(config: SecurityRoleQueryParameters | null = null) {

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


    public ConvertToSecurityRoleSubmitData(data: SecurityRoleData): SecurityRoleSubmitData {

        let output = new SecurityRoleSubmitData();

        output.id = data.id;
        output.privilegeId = data.privilegeId;
        output.name = data.name;
        output.description = data.description;
        output.comments = data.comments;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSecurityRole(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityRoleData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const securityRole$ = this.requestSecurityRole(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityRole", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, securityRole$);

            return securityRole$;
        }

        return this.recordCache.get(configHash) as Observable<SecurityRoleData>;
    }

    private requestSecurityRole(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityRoleData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SecurityRoleData>(this.baseUrl + 'api/SecurityRole/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSecurityRole(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityRole(id, includeRelations));
            }));
    }

    public GetSecurityRoleList(config: SecurityRoleQueryParameters | any = null) : Observable<Array<SecurityRoleData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const securityRoleList$ = this.requestSecurityRoleList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityRole list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, securityRoleList$);

            return securityRoleList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SecurityRoleData>>;
    }


    private requestSecurityRoleList(config: SecurityRoleQueryParameters | any) : Observable <Array<SecurityRoleData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityRoleData>>(this.baseUrl + 'api/SecurityRoles', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSecurityRoleList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityRoleList(config));
            }));
    }

    public GetSecurityRolesRowCount(config: SecurityRoleQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const securityRolesRowCount$ = this.requestSecurityRolesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityRoles row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, securityRolesRowCount$);

            return securityRolesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSecurityRolesRowCount(config: SecurityRoleQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SecurityRoles/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityRolesRowCount(config));
            }));
    }

    public GetSecurityRolesBasicListData(config: SecurityRoleQueryParameters | any = null) : Observable<Array<SecurityRoleBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const securityRolesBasicListData$ = this.requestSecurityRolesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityRoles basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, securityRolesBasicListData$);

            return securityRolesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SecurityRoleBasicListData>>;
    }


    private requestSecurityRolesBasicListData(config: SecurityRoleQueryParameters | any) : Observable<Array<SecurityRoleBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityRoleBasicListData>>(this.baseUrl + 'api/SecurityRoles/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityRolesBasicListData(config));
            }));

    }


    public PutSecurityRole(id: bigint | number, securityRole: SecurityRoleSubmitData) : Observable<SecurityRoleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SecurityRoleData>(this.baseUrl + 'api/SecurityRole/' + id.toString(), securityRole, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityRole(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSecurityRole(id, securityRole));
            }));
    }


    public PostSecurityRole(securityRole: SecurityRoleSubmitData) : Observable<SecurityRoleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SecurityRoleData>(this.baseUrl + 'api/SecurityRole', securityRole, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityRole(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSecurityRole(securityRole));
            }));
    }

  
    public DeleteSecurityRole(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SecurityRole/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSecurityRole(id));
            }));
    }


    private getConfigHash(config: SecurityRoleQueryParameters | any): string {

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

    public userIsSecuritySecurityRoleReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecuritySecurityRoleReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.SecurityRoles
        //
        if (userIsSecuritySecurityRoleReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecuritySecurityRoleReader = user.readPermission >= 0;
            } else {
                userIsSecuritySecurityRoleReader = false;
            }
        }

        return userIsSecuritySecurityRoleReader;
    }


    public userIsSecuritySecurityRoleWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecuritySecurityRoleWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.SecurityRoles
        //
        if (userIsSecuritySecurityRoleWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecuritySecurityRoleWriter = user.writePermission >= 0;
          } else {
            userIsSecuritySecurityRoleWriter = false;
          }      
        }

        return userIsSecuritySecurityRoleWriter;
    }

    public GetSecurityUserSecurityRolesForSecurityRole(securityRoleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityUserSecurityRoleData[]> {
        return this.securityUserSecurityRoleService.GetSecurityUserSecurityRoleList({
            securityRoleId: securityRoleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityGroupSecurityRolesForSecurityRole(securityRoleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityGroupSecurityRoleData[]> {
        return this.securityGroupSecurityRoleService.GetSecurityGroupSecurityRoleList({
            securityRoleId: securityRoleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetModuleSecurityRolesForSecurityRole(securityRoleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ModuleSecurityRoleData[]> {
        return this.moduleSecurityRoleService.GetModuleSecurityRoleList({
            securityRoleId: securityRoleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SecurityRoleData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SecurityRoleData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SecurityRoleTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSecurityRole(raw: any): SecurityRoleData {
    if (!raw) return raw;

    //
    // Create a SecurityRoleData object instance with correct prototype
    //
    const revived = Object.create(SecurityRoleData.prototype) as SecurityRoleData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._securityUserSecurityRoles = null;
    (revived as any)._securityUserSecurityRolesPromise = null;
    (revived as any)._securityUserSecurityRolesSubject = new BehaviorSubject<SecurityUserSecurityRoleData[] | null>(null);

    (revived as any)._securityGroupSecurityRoles = null;
    (revived as any)._securityGroupSecurityRolesPromise = null;
    (revived as any)._securityGroupSecurityRolesSubject = new BehaviorSubject<SecurityGroupSecurityRoleData[] | null>(null);

    (revived as any)._moduleSecurityRoles = null;
    (revived as any)._moduleSecurityRolesPromise = null;
    (revived as any)._moduleSecurityRolesSubject = new BehaviorSubject<ModuleSecurityRoleData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSecurityRoleXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SecurityUserSecurityRoles$ = (revived as any)._securityUserSecurityRolesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityUserSecurityRoles === null && (revived as any)._securityUserSecurityRolesPromise === null) {
                (revived as any).loadSecurityUserSecurityRoles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityUserSecurityRolesCount$ = SecurityUserSecurityRoleService.Instance.GetSecurityUserSecurityRolesRowCount({securityRoleId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SecurityGroupSecurityRoles$ = (revived as any)._securityGroupSecurityRolesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityGroupSecurityRoles === null && (revived as any)._securityGroupSecurityRolesPromise === null) {
                (revived as any).loadSecurityGroupSecurityRoles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityGroupSecurityRolesCount$ = SecurityGroupSecurityRoleService.Instance.GetSecurityGroupSecurityRolesRowCount({securityRoleId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ModuleSecurityRoles$ = (revived as any)._moduleSecurityRolesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._moduleSecurityRoles === null && (revived as any)._moduleSecurityRolesPromise === null) {
                (revived as any).loadModuleSecurityRoles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ModuleSecurityRolesCount$ = ModuleSecurityRoleService.Instance.GetModuleSecurityRolesRowCount({securityRoleId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveSecurityRoleList(rawList: any[]): SecurityRoleData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSecurityRole(raw));
  }

}
