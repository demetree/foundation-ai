/*

   GENERATED SERVICE FOR THE SECURITYGROUP TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SecurityGroup table.

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
import { SecurityUserSecurityGroupService, SecurityUserSecurityGroupData } from './security-user-security-group.service';
import { SecurityGroupSecurityRoleService, SecurityGroupSecurityRoleData } from './security-group-security-role.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SecurityGroupQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class SecurityGroupSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class SecurityGroupBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SecurityGroupChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `securityGroup.SecurityGroupChildren$` — use with `| async` in templates
//        • Promise:    `securityGroup.SecurityGroupChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="securityGroup.SecurityGroupChildren$ | async"`), or
//        • Access the promise getter (`securityGroup.SecurityGroupChildren` or `await securityGroup.SecurityGroupChildren`)
//    - Simply reading `securityGroup.SecurityGroupChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await securityGroup.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SecurityGroupData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _securityUserSecurityGroups: SecurityUserSecurityGroupData[] | null = null;
    private _securityUserSecurityGroupsPromise: Promise<SecurityUserSecurityGroupData[]> | null  = null;
    private _securityUserSecurityGroupsSubject = new BehaviorSubject<SecurityUserSecurityGroupData[] | null>(null);

                
    private _securityGroupSecurityRoles: SecurityGroupSecurityRoleData[] | null = null;
    private _securityGroupSecurityRolesPromise: Promise<SecurityGroupSecurityRoleData[]> | null  = null;
    private _securityGroupSecurityRolesSubject = new BehaviorSubject<SecurityGroupSecurityRoleData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SecurityUserSecurityGroups$ = this._securityUserSecurityGroupsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityUserSecurityGroups === null && this._securityUserSecurityGroupsPromise === null) {
            this.loadSecurityUserSecurityGroups(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityUserSecurityGroupsCount$ = SecurityUserSecurityGroupService.Instance.GetSecurityUserSecurityGroupsRowCount({securityGroupId: this.id,
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

  
    public SecurityGroupSecurityRolesCount$ = SecurityGroupSecurityRoleService.Instance.GetSecurityGroupSecurityRolesRowCount({securityGroupId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SecurityGroupData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.securityGroup.Reload();
  //
  //  Non Async:
  //
  //     securityGroup[0].Reload().then(x => {
  //        this.securityGroup = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SecurityGroupService.Instance.GetSecurityGroup(this.id, includeRelations)
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
     this._securityUserSecurityGroups = null;
     this._securityUserSecurityGroupsPromise = null;
     this._securityUserSecurityGroupsSubject.next(null);

     this._securityGroupSecurityRoles = null;
     this._securityGroupSecurityRolesPromise = null;
     this._securityGroupSecurityRolesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the SecurityUserSecurityGroups for this SecurityGroup.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityGroup.SecurityUserSecurityGroups.then(securityGroups => { ... })
     *   or
     *   await this.securityGroup.securityGroups
     *
    */
    public get SecurityUserSecurityGroups(): Promise<SecurityUserSecurityGroupData[]> {
        if (this._securityUserSecurityGroups !== null) {
            return Promise.resolve(this._securityUserSecurityGroups);
        }

        if (this._securityUserSecurityGroupsPromise !== null) {
            return this._securityUserSecurityGroupsPromise;
        }

        // Start the load
        this.loadSecurityUserSecurityGroups();

        return this._securityUserSecurityGroupsPromise!;
    }



    private loadSecurityUserSecurityGroups(): void {

        this._securityUserSecurityGroupsPromise = lastValueFrom(
            SecurityGroupService.Instance.GetSecurityUserSecurityGroupsForSecurityGroup(this.id)
        )
        .then(SecurityUserSecurityGroups => {
            this._securityUserSecurityGroups = SecurityUserSecurityGroups ?? [];
            this._securityUserSecurityGroupsSubject.next(this._securityUserSecurityGroups);
            return this._securityUserSecurityGroups;
         })
        .catch(err => {
            this._securityUserSecurityGroups = [];
            this._securityUserSecurityGroupsSubject.next(this._securityUserSecurityGroups);
            throw err;
        })
        .finally(() => {
            this._securityUserSecurityGroupsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SecurityUserSecurityGroup. Call after mutations to force refresh.
     */
    public ClearSecurityUserSecurityGroupsCache(): void {
        this._securityUserSecurityGroups = null;
        this._securityUserSecurityGroupsPromise = null;
        this._securityUserSecurityGroupsSubject.next(this._securityUserSecurityGroups);      // Emit to observable
    }

    public get HasSecurityUserSecurityGroups(): Promise<boolean> {
        return this.SecurityUserSecurityGroups.then(securityUserSecurityGroups => securityUserSecurityGroups.length > 0);
    }


    /**
     *
     * Gets the SecurityGroupSecurityRoles for this SecurityGroup.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityGroup.SecurityGroupSecurityRoles.then(securityGroups => { ... })
     *   or
     *   await this.securityGroup.securityGroups
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
            SecurityGroupService.Instance.GetSecurityGroupSecurityRolesForSecurityGroup(this.id)
        )
        .then(SecurityGroupSecurityRoles => {
            this._securityGroupSecurityRoles = SecurityGroupSecurityRoles ?? [];
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
     * Clears the cached SecurityGroupSecurityRole. Call after mutations to force refresh.
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
     * Updates the state of this SecurityGroupData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SecurityGroupData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SecurityGroupSubmitData {
        return SecurityGroupService.Instance.ConvertToSecurityGroupSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SecurityGroupService extends SecureEndpointBase {

    private static _instance: SecurityGroupService;
    private listCache: Map<string, Observable<Array<SecurityGroupData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SecurityGroupBasicListData>>>;
    private recordCache: Map<string, Observable<SecurityGroupData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private securityUserSecurityGroupService: SecurityUserSecurityGroupService,
        private securityGroupSecurityRoleService: SecurityGroupSecurityRoleService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SecurityGroupData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SecurityGroupBasicListData>>>();
        this.recordCache = new Map<string, Observable<SecurityGroupData>>();

        SecurityGroupService._instance = this;
    }

    public static get Instance(): SecurityGroupService {
      return SecurityGroupService._instance;
    }


    public ClearListCaches(config: SecurityGroupQueryParameters | null = null) {

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


    public ConvertToSecurityGroupSubmitData(data: SecurityGroupData): SecurityGroupSubmitData {

        let output = new SecurityGroupSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSecurityGroup(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityGroupData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const securityGroup$ = this.requestSecurityGroup(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityGroup", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, securityGroup$);

            return securityGroup$;
        }

        return this.recordCache.get(configHash) as Observable<SecurityGroupData>;
    }

    private requestSecurityGroup(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityGroupData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SecurityGroupData>(this.baseUrl + 'api/SecurityGroup/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSecurityGroup(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityGroup(id, includeRelations));
            }));
    }

    public GetSecurityGroupList(config: SecurityGroupQueryParameters | any = null) : Observable<Array<SecurityGroupData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const securityGroupList$ = this.requestSecurityGroupList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityGroup list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, securityGroupList$);

            return securityGroupList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SecurityGroupData>>;
    }


    private requestSecurityGroupList(config: SecurityGroupQueryParameters | any) : Observable <Array<SecurityGroupData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityGroupData>>(this.baseUrl + 'api/SecurityGroups', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSecurityGroupList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityGroupList(config));
            }));
    }

    public GetSecurityGroupsRowCount(config: SecurityGroupQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const securityGroupsRowCount$ = this.requestSecurityGroupsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityGroups row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, securityGroupsRowCount$);

            return securityGroupsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSecurityGroupsRowCount(config: SecurityGroupQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SecurityGroups/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityGroupsRowCount(config));
            }));
    }

    public GetSecurityGroupsBasicListData(config: SecurityGroupQueryParameters | any = null) : Observable<Array<SecurityGroupBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const securityGroupsBasicListData$ = this.requestSecurityGroupsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityGroups basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, securityGroupsBasicListData$);

            return securityGroupsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SecurityGroupBasicListData>>;
    }


    private requestSecurityGroupsBasicListData(config: SecurityGroupQueryParameters | any) : Observable<Array<SecurityGroupBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityGroupBasicListData>>(this.baseUrl + 'api/SecurityGroups/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityGroupsBasicListData(config));
            }));

    }


    public PutSecurityGroup(id: bigint | number, securityGroup: SecurityGroupSubmitData) : Observable<SecurityGroupData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SecurityGroupData>(this.baseUrl + 'api/SecurityGroup/' + id.toString(), securityGroup, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityGroup(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSecurityGroup(id, securityGroup));
            }));
    }


    public PostSecurityGroup(securityGroup: SecurityGroupSubmitData) : Observable<SecurityGroupData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SecurityGroupData>(this.baseUrl + 'api/SecurityGroup', securityGroup, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityGroup(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSecurityGroup(securityGroup));
            }));
    }

  
    public DeleteSecurityGroup(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SecurityGroup/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSecurityGroup(id));
            }));
    }


    private getConfigHash(config: SecurityGroupQueryParameters | any): string {

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

    public userIsSecuritySecurityGroupReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecuritySecurityGroupReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.SecurityGroups
        //
        if (userIsSecuritySecurityGroupReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecuritySecurityGroupReader = user.readPermission >= 1;
            } else {
                userIsSecuritySecurityGroupReader = false;
            }
        }

        return userIsSecuritySecurityGroupReader;
    }


    public userIsSecuritySecurityGroupWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecuritySecurityGroupWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.SecurityGroups
        //
        if (userIsSecuritySecurityGroupWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecuritySecurityGroupWriter = user.writePermission >= 150;
          } else {
            userIsSecuritySecurityGroupWriter = false;
          }      
        }

        return userIsSecuritySecurityGroupWriter;
    }

    public GetSecurityUserSecurityGroupsForSecurityGroup(securityGroupId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityUserSecurityGroupData[]> {
        return this.securityUserSecurityGroupService.GetSecurityUserSecurityGroupList({
            securityGroupId: securityGroupId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityGroupSecurityRolesForSecurityGroup(securityGroupId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityGroupSecurityRoleData[]> {
        return this.securityGroupSecurityRoleService.GetSecurityGroupSecurityRoleList({
            securityGroupId: securityGroupId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SecurityGroupData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SecurityGroupData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SecurityGroupTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSecurityGroup(raw: any): SecurityGroupData {
    if (!raw) return raw;

    //
    // Create a SecurityGroupData object instance with correct prototype
    //
    const revived = Object.create(SecurityGroupData.prototype) as SecurityGroupData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._securityUserSecurityGroups = null;
    (revived as any)._securityUserSecurityGroupsPromise = null;
    (revived as any)._securityUserSecurityGroupsSubject = new BehaviorSubject<SecurityUserSecurityGroupData[] | null>(null);

    (revived as any)._securityGroupSecurityRoles = null;
    (revived as any)._securityGroupSecurityRolesPromise = null;
    (revived as any)._securityGroupSecurityRolesSubject = new BehaviorSubject<SecurityGroupSecurityRoleData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSecurityGroupXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SecurityUserSecurityGroups$ = (revived as any)._securityUserSecurityGroupsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityUserSecurityGroups === null && (revived as any)._securityUserSecurityGroupsPromise === null) {
                (revived as any).loadSecurityUserSecurityGroups();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityUserSecurityGroupsCount$ = SecurityUserSecurityGroupService.Instance.GetSecurityUserSecurityGroupsRowCount({securityGroupId: (revived as any).id,
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

    (revived as any).SecurityGroupSecurityRolesCount$ = SecurityGroupSecurityRoleService.Instance.GetSecurityGroupSecurityRolesRowCount({securityGroupId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveSecurityGroupList(rawList: any[]): SecurityGroupData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSecurityGroup(raw));
  }

}
