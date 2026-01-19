/*

   GENERATED SERVICE FOR THE SECURITYORGANIZATION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SecurityOrganization table.

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
import { SecurityTenantData } from './security-tenant.service';
import { SecurityDepartmentService, SecurityDepartmentData } from './security-department.service';
import { SecurityUserService, SecurityUserData } from './security-user.service';
import { SecurityOrganizationUserService, SecurityOrganizationUserData } from './security-organization-user.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SecurityOrganizationQueryParameters {
    securityTenantId: bigint | number | null | undefined = null;
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
export class SecurityOrganizationSubmitData {
    id!: bigint | number;
    securityTenantId!: bigint | number;
    name!: string;
    description: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class SecurityOrganizationBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SecurityOrganizationChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `securityOrganization.SecurityOrganizationChildren$` — use with `| async` in templates
//        • Promise:    `securityOrganization.SecurityOrganizationChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="securityOrganization.SecurityOrganizationChildren$ | async"`), or
//        • Access the promise getter (`securityOrganization.SecurityOrganizationChildren` or `await securityOrganization.SecurityOrganizationChildren`)
//    - Simply reading `securityOrganization.SecurityOrganizationChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await securityOrganization.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SecurityOrganizationData {
    id!: bigint | number;
    securityTenantId!: bigint | number;
    name!: string;
    description!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    securityTenant: SecurityTenantData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _securityDepartments: SecurityDepartmentData[] | null = null;
    private _securityDepartmentsPromise: Promise<SecurityDepartmentData[]> | null  = null;
    private _securityDepartmentsSubject = new BehaviorSubject<SecurityDepartmentData[] | null>(null);

                
    private _securityUsers: SecurityUserData[] | null = null;
    private _securityUsersPromise: Promise<SecurityUserData[]> | null  = null;
    private _securityUsersSubject = new BehaviorSubject<SecurityUserData[] | null>(null);

                
    private _securityOrganizationUsers: SecurityOrganizationUserData[] | null = null;
    private _securityOrganizationUsersPromise: Promise<SecurityOrganizationUserData[]> | null  = null;
    private _securityOrganizationUsersSubject = new BehaviorSubject<SecurityOrganizationUserData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SecurityDepartments$ = this._securityDepartmentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityDepartments === null && this._securityDepartmentsPromise === null) {
            this.loadSecurityDepartments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityDepartmentsCount$ = SecurityDepartmentService.Instance.GetSecurityDepartmentsRowCount({securityOrganizationId: this.id,
      active: true,
      deleted: false
    });



    public SecurityUsers$ = this._securityUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityUsers === null && this._securityUsersPromise === null) {
            this.loadSecurityUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityUsersCount$ = SecurityUserService.Instance.GetSecurityUsersRowCount({securityOrganizationId: this.id,
      active: true,
      deleted: false
    });



    public SecurityOrganizationUsers$ = this._securityOrganizationUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityOrganizationUsers === null && this._securityOrganizationUsersPromise === null) {
            this.loadSecurityOrganizationUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityOrganizationUsersCount$ = SecurityOrganizationUserService.Instance.GetSecurityOrganizationUsersRowCount({securityOrganizationId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SecurityOrganizationData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.securityOrganization.Reload();
  //
  //  Non Async:
  //
  //     securityOrganization[0].Reload().then(x => {
  //        this.securityOrganization = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SecurityOrganizationService.Instance.GetSecurityOrganization(this.id, includeRelations)
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
     this._securityDepartments = null;
     this._securityDepartmentsPromise = null;
     this._securityDepartmentsSubject.next(null);

     this._securityUsers = null;
     this._securityUsersPromise = null;
     this._securityUsersSubject.next(null);

     this._securityOrganizationUsers = null;
     this._securityOrganizationUsersPromise = null;
     this._securityOrganizationUsersSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the SecurityDepartments for this SecurityOrganization.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityOrganization.SecurityDepartments.then(securityOrganizations => { ... })
     *   or
     *   await this.securityOrganization.securityOrganizations
     *
    */
    public get SecurityDepartments(): Promise<SecurityDepartmentData[]> {
        if (this._securityDepartments !== null) {
            return Promise.resolve(this._securityDepartments);
        }

        if (this._securityDepartmentsPromise !== null) {
            return this._securityDepartmentsPromise;
        }

        // Start the load
        this.loadSecurityDepartments();

        return this._securityDepartmentsPromise!;
    }



    private loadSecurityDepartments(): void {

        this._securityDepartmentsPromise = lastValueFrom(
            SecurityOrganizationService.Instance.GetSecurityDepartmentsForSecurityOrganization(this.id)
        )
        .then(SecurityDepartments => {
            this._securityDepartments = SecurityDepartments ?? [];
            this._securityDepartmentsSubject.next(this._securityDepartments);
            return this._securityDepartments;
         })
        .catch(err => {
            this._securityDepartments = [];
            this._securityDepartmentsSubject.next(this._securityDepartments);
            throw err;
        })
        .finally(() => {
            this._securityDepartmentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SecurityDepartment. Call after mutations to force refresh.
     */
    public ClearSecurityDepartmentsCache(): void {
        this._securityDepartments = null;
        this._securityDepartmentsPromise = null;
        this._securityDepartmentsSubject.next(this._securityDepartments);      // Emit to observable
    }

    public get HasSecurityDepartments(): Promise<boolean> {
        return this.SecurityDepartments.then(securityDepartments => securityDepartments.length > 0);
    }


    /**
     *
     * Gets the SecurityUsers for this SecurityOrganization.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityOrganization.SecurityUsers.then(securityOrganizations => { ... })
     *   or
     *   await this.securityOrganization.securityOrganizations
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
            SecurityOrganizationService.Instance.GetSecurityUsersForSecurityOrganization(this.id)
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
     * Gets the SecurityOrganizationUsers for this SecurityOrganization.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityOrganization.SecurityOrganizationUsers.then(securityOrganizations => { ... })
     *   or
     *   await this.securityOrganization.securityOrganizations
     *
    */
    public get SecurityOrganizationUsers(): Promise<SecurityOrganizationUserData[]> {
        if (this._securityOrganizationUsers !== null) {
            return Promise.resolve(this._securityOrganizationUsers);
        }

        if (this._securityOrganizationUsersPromise !== null) {
            return this._securityOrganizationUsersPromise;
        }

        // Start the load
        this.loadSecurityOrganizationUsers();

        return this._securityOrganizationUsersPromise!;
    }



    private loadSecurityOrganizationUsers(): void {

        this._securityOrganizationUsersPromise = lastValueFrom(
            SecurityOrganizationService.Instance.GetSecurityOrganizationUsersForSecurityOrganization(this.id)
        )
        .then(SecurityOrganizationUsers => {
            this._securityOrganizationUsers = SecurityOrganizationUsers ?? [];
            this._securityOrganizationUsersSubject.next(this._securityOrganizationUsers);
            return this._securityOrganizationUsers;
         })
        .catch(err => {
            this._securityOrganizationUsers = [];
            this._securityOrganizationUsersSubject.next(this._securityOrganizationUsers);
            throw err;
        })
        .finally(() => {
            this._securityOrganizationUsersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SecurityOrganizationUser. Call after mutations to force refresh.
     */
    public ClearSecurityOrganizationUsersCache(): void {
        this._securityOrganizationUsers = null;
        this._securityOrganizationUsersPromise = null;
        this._securityOrganizationUsersSubject.next(this._securityOrganizationUsers);      // Emit to observable
    }

    public get HasSecurityOrganizationUsers(): Promise<boolean> {
        return this.SecurityOrganizationUsers.then(securityOrganizationUsers => securityOrganizationUsers.length > 0);
    }




    /**
     * Updates the state of this SecurityOrganizationData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SecurityOrganizationData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SecurityOrganizationSubmitData {
        return SecurityOrganizationService.Instance.ConvertToSecurityOrganizationSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SecurityOrganizationService extends SecureEndpointBase {

    private static _instance: SecurityOrganizationService;
    private listCache: Map<string, Observable<Array<SecurityOrganizationData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SecurityOrganizationBasicListData>>>;
    private recordCache: Map<string, Observable<SecurityOrganizationData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private securityDepartmentService: SecurityDepartmentService,
        private securityUserService: SecurityUserService,
        private securityOrganizationUserService: SecurityOrganizationUserService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SecurityOrganizationData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SecurityOrganizationBasicListData>>>();
        this.recordCache = new Map<string, Observable<SecurityOrganizationData>>();

        SecurityOrganizationService._instance = this;
    }

    public static get Instance(): SecurityOrganizationService {
      return SecurityOrganizationService._instance;
    }


    public ClearListCaches(config: SecurityOrganizationQueryParameters | null = null) {

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


    public ConvertToSecurityOrganizationSubmitData(data: SecurityOrganizationData): SecurityOrganizationSubmitData {

        let output = new SecurityOrganizationSubmitData();

        output.id = data.id;
        output.securityTenantId = data.securityTenantId;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSecurityOrganization(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityOrganizationData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const securityOrganization$ = this.requestSecurityOrganization(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityOrganization", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, securityOrganization$);

            return securityOrganization$;
        }

        return this.recordCache.get(configHash) as Observable<SecurityOrganizationData>;
    }

    private requestSecurityOrganization(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityOrganizationData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SecurityOrganizationData>(this.baseUrl + 'api/SecurityOrganization/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSecurityOrganization(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityOrganization(id, includeRelations));
            }));
    }

    public GetSecurityOrganizationList(config: SecurityOrganizationQueryParameters | any = null) : Observable<Array<SecurityOrganizationData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const securityOrganizationList$ = this.requestSecurityOrganizationList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityOrganization list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, securityOrganizationList$);

            return securityOrganizationList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SecurityOrganizationData>>;
    }


    private requestSecurityOrganizationList(config: SecurityOrganizationQueryParameters | any) : Observable <Array<SecurityOrganizationData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityOrganizationData>>(this.baseUrl + 'api/SecurityOrganizations', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSecurityOrganizationList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityOrganizationList(config));
            }));
    }

    public GetSecurityOrganizationsRowCount(config: SecurityOrganizationQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const securityOrganizationsRowCount$ = this.requestSecurityOrganizationsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityOrganizations row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, securityOrganizationsRowCount$);

            return securityOrganizationsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSecurityOrganizationsRowCount(config: SecurityOrganizationQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SecurityOrganizations/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityOrganizationsRowCount(config));
            }));
    }

    public GetSecurityOrganizationsBasicListData(config: SecurityOrganizationQueryParameters | any = null) : Observable<Array<SecurityOrganizationBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const securityOrganizationsBasicListData$ = this.requestSecurityOrganizationsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityOrganizations basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, securityOrganizationsBasicListData$);

            return securityOrganizationsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SecurityOrganizationBasicListData>>;
    }


    private requestSecurityOrganizationsBasicListData(config: SecurityOrganizationQueryParameters | any) : Observable<Array<SecurityOrganizationBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityOrganizationBasicListData>>(this.baseUrl + 'api/SecurityOrganizations/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityOrganizationsBasicListData(config));
            }));

    }


    public PutSecurityOrganization(id: bigint | number, securityOrganization: SecurityOrganizationSubmitData) : Observable<SecurityOrganizationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SecurityOrganizationData>(this.baseUrl + 'api/SecurityOrganization/' + id.toString(), securityOrganization, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityOrganization(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSecurityOrganization(id, securityOrganization));
            }));
    }


    public PostSecurityOrganization(securityOrganization: SecurityOrganizationSubmitData) : Observable<SecurityOrganizationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SecurityOrganizationData>(this.baseUrl + 'api/SecurityOrganization', securityOrganization, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityOrganization(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSecurityOrganization(securityOrganization));
            }));
    }

  
    public DeleteSecurityOrganization(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SecurityOrganization/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSecurityOrganization(id));
            }));
    }


    private getConfigHash(config: SecurityOrganizationQueryParameters | any): string {

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

    public userIsSecuritySecurityOrganizationReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecuritySecurityOrganizationReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.SecurityOrganizations
        //
        if (userIsSecuritySecurityOrganizationReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecuritySecurityOrganizationReader = user.readPermission >= 0;
            } else {
                userIsSecuritySecurityOrganizationReader = false;
            }
        }

        return userIsSecuritySecurityOrganizationReader;
    }


    public userIsSecuritySecurityOrganizationWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecuritySecurityOrganizationWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.SecurityOrganizations
        //
        if (userIsSecuritySecurityOrganizationWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecuritySecurityOrganizationWriter = user.writePermission >= 100;
          } else {
            userIsSecuritySecurityOrganizationWriter = false;
          }      
        }

        return userIsSecuritySecurityOrganizationWriter;
    }

    public GetSecurityDepartmentsForSecurityOrganization(securityOrganizationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityDepartmentData[]> {
        return this.securityDepartmentService.GetSecurityDepartmentList({
            securityOrganizationId: securityOrganizationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityUsersForSecurityOrganization(securityOrganizationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityUserData[]> {
        return this.securityUserService.GetSecurityUserList({
            securityOrganizationId: securityOrganizationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityOrganizationUsersForSecurityOrganization(securityOrganizationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityOrganizationUserData[]> {
        return this.securityOrganizationUserService.GetSecurityOrganizationUserList({
            securityOrganizationId: securityOrganizationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SecurityOrganizationData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SecurityOrganizationData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SecurityOrganizationTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSecurityOrganization(raw: any): SecurityOrganizationData {
    if (!raw) return raw;

    //
    // Create a SecurityOrganizationData object instance with correct prototype
    //
    const revived = Object.create(SecurityOrganizationData.prototype) as SecurityOrganizationData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._securityDepartments = null;
    (revived as any)._securityDepartmentsPromise = null;
    (revived as any)._securityDepartmentsSubject = new BehaviorSubject<SecurityDepartmentData[] | null>(null);

    (revived as any)._securityUsers = null;
    (revived as any)._securityUsersPromise = null;
    (revived as any)._securityUsersSubject = new BehaviorSubject<SecurityUserData[] | null>(null);

    (revived as any)._securityOrganizationUsers = null;
    (revived as any)._securityOrganizationUsersPromise = null;
    (revived as any)._securityOrganizationUsersSubject = new BehaviorSubject<SecurityOrganizationUserData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSecurityOrganizationXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SecurityDepartments$ = (revived as any)._securityDepartmentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityDepartments === null && (revived as any)._securityDepartmentsPromise === null) {
                (revived as any).loadSecurityDepartments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityDepartmentsCount$ = SecurityDepartmentService.Instance.GetSecurityDepartmentsRowCount({securityOrganizationId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SecurityUsers$ = (revived as any)._securityUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityUsers === null && (revived as any)._securityUsersPromise === null) {
                (revived as any).loadSecurityUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityUsersCount$ = SecurityUserService.Instance.GetSecurityUsersRowCount({securityOrganizationId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SecurityOrganizationUsers$ = (revived as any)._securityOrganizationUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityOrganizationUsers === null && (revived as any)._securityOrganizationUsersPromise === null) {
                (revived as any).loadSecurityOrganizationUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityOrganizationUsersCount$ = SecurityOrganizationUserService.Instance.GetSecurityOrganizationUsersRowCount({securityOrganizationId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveSecurityOrganizationList(rawList: any[]): SecurityOrganizationData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSecurityOrganization(raw));
  }

}
