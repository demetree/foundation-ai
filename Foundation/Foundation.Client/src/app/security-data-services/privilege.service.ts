/*

   GENERATED SERVICE FOR THE PRIVILEGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Privilege table.

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
import { SecurityRoleService, SecurityRoleData } from './security-role.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PrivilegeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class PrivilegeSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
}


export class PrivilegeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PrivilegeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `privilege.PrivilegeChildren$` — use with `| async` in templates
//        • Promise:    `privilege.PrivilegeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="privilege.PrivilegeChildren$ | async"`), or
//        • Access the promise getter (`privilege.PrivilegeChildren` or `await privilege.PrivilegeChildren`)
//    - Simply reading `privilege.PrivilegeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await privilege.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PrivilegeData {
    id!: bigint | number;
    name!: string;
    description!: string | null;

    //
    // Private lazy-loading caches for related collections
    //
    private _securityRoles: SecurityRoleData[] | null = null;
    private _securityRolesPromise: Promise<SecurityRoleData[]> | null  = null;
    private _securityRolesSubject = new BehaviorSubject<SecurityRoleData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SecurityRoles$ = this._securityRolesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityRoles === null && this._securityRolesPromise === null) {
            this.loadSecurityRoles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityRolesCount$ = SecurityRoleService.Instance.GetSecurityRolesRowCount({privilegeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PrivilegeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.privilege.Reload();
  //
  //  Non Async:
  //
  //     privilege[0].Reload().then(x => {
  //        this.privilege = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PrivilegeService.Instance.GetPrivilege(this.id, includeRelations)
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
     this._securityRoles = null;
     this._securityRolesPromise = null;
     this._securityRolesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the SecurityRoles for this Privilege.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.privilege.SecurityRoles.then(privileges => { ... })
     *   or
     *   await this.privilege.privileges
     *
    */
    public get SecurityRoles(): Promise<SecurityRoleData[]> {
        if (this._securityRoles !== null) {
            return Promise.resolve(this._securityRoles);
        }

        if (this._securityRolesPromise !== null) {
            return this._securityRolesPromise;
        }

        // Start the load
        this.loadSecurityRoles();

        return this._securityRolesPromise!;
    }



    private loadSecurityRoles(): void {

        this._securityRolesPromise = lastValueFrom(
            PrivilegeService.Instance.GetSecurityRolesForPrivilege(this.id)
        )
        .then(SecurityRoles => {
            this._securityRoles = SecurityRoles ?? [];
            this._securityRolesSubject.next(this._securityRoles);
            return this._securityRoles;
         })
        .catch(err => {
            this._securityRoles = [];
            this._securityRolesSubject.next(this._securityRoles);
            throw err;
        })
        .finally(() => {
            this._securityRolesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SecurityRole. Call after mutations to force refresh.
     */
    public ClearSecurityRolesCache(): void {
        this._securityRoles = null;
        this._securityRolesPromise = null;
        this._securityRolesSubject.next(this._securityRoles);      // Emit to observable
    }

    public get HasSecurityRoles(): Promise<boolean> {
        return this.SecurityRoles.then(securityRoles => securityRoles.length > 0);
    }




    /**
     * Updates the state of this PrivilegeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PrivilegeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PrivilegeSubmitData {
        return PrivilegeService.Instance.ConvertToPrivilegeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PrivilegeService extends SecureEndpointBase {

    private static _instance: PrivilegeService;
    private listCache: Map<string, Observable<Array<PrivilegeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PrivilegeBasicListData>>>;
    private recordCache: Map<string, Observable<PrivilegeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private securityRoleService: SecurityRoleService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PrivilegeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PrivilegeBasicListData>>>();
        this.recordCache = new Map<string, Observable<PrivilegeData>>();

        PrivilegeService._instance = this;
    }

    public static get Instance(): PrivilegeService {
      return PrivilegeService._instance;
    }


    public ClearListCaches(config: PrivilegeQueryParameters | null = null) {

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


    public ConvertToPrivilegeSubmitData(data: PrivilegeData): PrivilegeSubmitData {

        let output = new PrivilegeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;

        return output;
    }

    public GetPrivilege(id: bigint | number, includeRelations: boolean = true) : Observable<PrivilegeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const privilege$ = this.requestPrivilege(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Privilege", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, privilege$);

            return privilege$;
        }

        return this.recordCache.get(configHash) as Observable<PrivilegeData>;
    }

    private requestPrivilege(id: bigint | number, includeRelations: boolean = true) : Observable<PrivilegeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PrivilegeData>(this.baseUrl + 'api/Privilege/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePrivilege(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPrivilege(id, includeRelations));
            }));
    }

    public GetPrivilegeList(config: PrivilegeQueryParameters | any = null) : Observable<Array<PrivilegeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const privilegeList$ = this.requestPrivilegeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Privilege list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, privilegeList$);

            return privilegeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PrivilegeData>>;
    }


    private requestPrivilegeList(config: PrivilegeQueryParameters | any) : Observable <Array<PrivilegeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PrivilegeData>>(this.baseUrl + 'api/Privileges', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePrivilegeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPrivilegeList(config));
            }));
    }

    public GetPrivilegesRowCount(config: PrivilegeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const privilegesRowCount$ = this.requestPrivilegesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Privileges row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, privilegesRowCount$);

            return privilegesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPrivilegesRowCount(config: PrivilegeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Privileges/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPrivilegesRowCount(config));
            }));
    }

    public GetPrivilegesBasicListData(config: PrivilegeQueryParameters | any = null) : Observable<Array<PrivilegeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const privilegesBasicListData$ = this.requestPrivilegesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Privileges basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, privilegesBasicListData$);

            return privilegesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PrivilegeBasicListData>>;
    }


    private requestPrivilegesBasicListData(config: PrivilegeQueryParameters | any) : Observable<Array<PrivilegeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PrivilegeBasicListData>>(this.baseUrl + 'api/Privileges/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPrivilegesBasicListData(config));
            }));

    }


    public PutPrivilege(id: bigint | number, privilege: PrivilegeSubmitData) : Observable<PrivilegeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PrivilegeData>(this.baseUrl + 'api/Privilege/' + id.toString(), privilege, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePrivilege(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPrivilege(id, privilege));
            }));
    }


    public PostPrivilege(privilege: PrivilegeSubmitData) : Observable<PrivilegeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PrivilegeData>(this.baseUrl + 'api/Privilege', privilege, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePrivilege(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPrivilege(privilege));
            }));
    }

  
    public DeletePrivilege(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Privilege/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePrivilege(id));
            }));
    }


    private getConfigHash(config: PrivilegeQueryParameters | any): string {

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

    public userIsSecurityPrivilegeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecurityPrivilegeReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.Privileges
        //
        if (userIsSecurityPrivilegeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecurityPrivilegeReader = user.readPermission >= 1;
            } else {
                userIsSecurityPrivilegeReader = false;
            }
        }

        return userIsSecurityPrivilegeReader;
    }


    public userIsSecurityPrivilegeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecurityPrivilegeWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.Privileges
        //
        if (userIsSecurityPrivilegeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecurityPrivilegeWriter = user.writePermission >= 255;
          } else {
            userIsSecurityPrivilegeWriter = false;
          }      
        }

        return userIsSecurityPrivilegeWriter;
    }

    public GetSecurityRolesForPrivilege(privilegeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityRoleData[]> {
        return this.securityRoleService.GetSecurityRoleList({
            privilegeId: privilegeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PrivilegeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PrivilegeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PrivilegeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePrivilege(raw: any): PrivilegeData {
    if (!raw) return raw;

    //
    // Create a PrivilegeData object instance with correct prototype
    //
    const revived = Object.create(PrivilegeData.prototype) as PrivilegeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._securityRoles = null;
    (revived as any)._securityRolesPromise = null;
    (revived as any)._securityRolesSubject = new BehaviorSubject<SecurityRoleData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadPrivilegeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SecurityRoles$ = (revived as any)._securityRolesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityRoles === null && (revived as any)._securityRolesPromise === null) {
                (revived as any).loadSecurityRoles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityRolesCount$ = SecurityRoleService.Instance.GetSecurityRolesRowCount({privilegeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private RevivePrivilegeList(rawList: any[]): PrivilegeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePrivilege(raw));
  }

}
