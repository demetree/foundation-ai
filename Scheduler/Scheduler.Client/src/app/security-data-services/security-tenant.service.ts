/*

   GENERATED SERVICE FOR THE SECURITYTENANT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SecurityTenant table.

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
import { SecurityOrganizationService, SecurityOrganizationData } from './security-organization.service';
import { SecurityUserService, SecurityUserData } from './security-user.service';
import { SecurityTenantUserService, SecurityTenantUserData } from './security-tenant-user.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SecurityTenantQueryParameters {
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
export class SecurityTenantSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class SecurityTenantBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SecurityTenantChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `securityTenant.SecurityTenantChildren$` — use with `| async` in templates
//        • Promise:    `securityTenant.SecurityTenantChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="securityTenant.SecurityTenantChildren$ | async"`), or
//        • Access the promise getter (`securityTenant.SecurityTenantChildren` or `await securityTenant.SecurityTenantChildren`)
//    - Simply reading `securityTenant.SecurityTenantChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await securityTenant.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SecurityTenantData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _securityOrganizations: SecurityOrganizationData[] | null = null;
    private _securityOrganizationsPromise: Promise<SecurityOrganizationData[]> | null  = null;
    private _securityOrganizationsSubject = new BehaviorSubject<SecurityOrganizationData[] | null>(null);

                
    private _securityUsers: SecurityUserData[] | null = null;
    private _securityUsersPromise: Promise<SecurityUserData[]> | null  = null;
    private _securityUsersSubject = new BehaviorSubject<SecurityUserData[] | null>(null);

                
    private _securityTenantUsers: SecurityTenantUserData[] | null = null;
    private _securityTenantUsersPromise: Promise<SecurityTenantUserData[]> | null  = null;
    private _securityTenantUsersSubject = new BehaviorSubject<SecurityTenantUserData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SecurityOrganizations$ = this._securityOrganizationsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityOrganizations === null && this._securityOrganizationsPromise === null) {
            this.loadSecurityOrganizations(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityOrganizationsCount$ = SecurityOrganizationService.Instance.GetSecurityOrganizationsRowCount({securityTenantId: this.id,
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

  
    public SecurityUsersCount$ = SecurityUserService.Instance.GetSecurityUsersRowCount({securityTenantId: this.id,
      active: true,
      deleted: false
    });



    public SecurityTenantUsers$ = this._securityTenantUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityTenantUsers === null && this._securityTenantUsersPromise === null) {
            this.loadSecurityTenantUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityTenantUsersCount$ = SecurityTenantUserService.Instance.GetSecurityTenantUsersRowCount({securityTenantId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SecurityTenantData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.securityTenant.Reload();
  //
  //  Non Async:
  //
  //     securityTenant[0].Reload().then(x => {
  //        this.securityTenant = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SecurityTenantService.Instance.GetSecurityTenant(this.id, includeRelations)
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
     this._securityOrganizations = null;
     this._securityOrganizationsPromise = null;
     this._securityOrganizationsSubject.next(null);

     this._securityUsers = null;
     this._securityUsersPromise = null;
     this._securityUsersSubject.next(null);

     this._securityTenantUsers = null;
     this._securityTenantUsersPromise = null;
     this._securityTenantUsersSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the SecurityOrganizations for this SecurityTenant.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityTenant.SecurityOrganizations.then(securityTenants => { ... })
     *   or
     *   await this.securityTenant.securityTenants
     *
    */
    public get SecurityOrganizations(): Promise<SecurityOrganizationData[]> {
        if (this._securityOrganizations !== null) {
            return Promise.resolve(this._securityOrganizations);
        }

        if (this._securityOrganizationsPromise !== null) {
            return this._securityOrganizationsPromise;
        }

        // Start the load
        this.loadSecurityOrganizations();

        return this._securityOrganizationsPromise!;
    }



    private loadSecurityOrganizations(): void {

        this._securityOrganizationsPromise = lastValueFrom(
            SecurityTenantService.Instance.GetSecurityOrganizationsForSecurityTenant(this.id)
        )
        .then(SecurityOrganizations => {
            this._securityOrganizations = SecurityOrganizations ?? [];
            this._securityOrganizationsSubject.next(this._securityOrganizations);
            return this._securityOrganizations;
         })
        .catch(err => {
            this._securityOrganizations = [];
            this._securityOrganizationsSubject.next(this._securityOrganizations);
            throw err;
        })
        .finally(() => {
            this._securityOrganizationsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SecurityOrganization. Call after mutations to force refresh.
     */
    public ClearSecurityOrganizationsCache(): void {
        this._securityOrganizations = null;
        this._securityOrganizationsPromise = null;
        this._securityOrganizationsSubject.next(this._securityOrganizations);      // Emit to observable
    }

    public get HasSecurityOrganizations(): Promise<boolean> {
        return this.SecurityOrganizations.then(securityOrganizations => securityOrganizations.length > 0);
    }


    /**
     *
     * Gets the SecurityUsers for this SecurityTenant.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityTenant.SecurityUsers.then(securityTenants => { ... })
     *   or
     *   await this.securityTenant.securityTenants
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
            SecurityTenantService.Instance.GetSecurityUsersForSecurityTenant(this.id)
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
     * Gets the SecurityTenantUsers for this SecurityTenant.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityTenant.SecurityTenantUsers.then(securityTenants => { ... })
     *   or
     *   await this.securityTenant.securityTenants
     *
    */
    public get SecurityTenantUsers(): Promise<SecurityTenantUserData[]> {
        if (this._securityTenantUsers !== null) {
            return Promise.resolve(this._securityTenantUsers);
        }

        if (this._securityTenantUsersPromise !== null) {
            return this._securityTenantUsersPromise;
        }

        // Start the load
        this.loadSecurityTenantUsers();

        return this._securityTenantUsersPromise!;
    }



    private loadSecurityTenantUsers(): void {

        this._securityTenantUsersPromise = lastValueFrom(
            SecurityTenantService.Instance.GetSecurityTenantUsersForSecurityTenant(this.id)
        )
        .then(SecurityTenantUsers => {
            this._securityTenantUsers = SecurityTenantUsers ?? [];
            this._securityTenantUsersSubject.next(this._securityTenantUsers);
            return this._securityTenantUsers;
         })
        .catch(err => {
            this._securityTenantUsers = [];
            this._securityTenantUsersSubject.next(this._securityTenantUsers);
            throw err;
        })
        .finally(() => {
            this._securityTenantUsersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SecurityTenantUser. Call after mutations to force refresh.
     */
    public ClearSecurityTenantUsersCache(): void {
        this._securityTenantUsers = null;
        this._securityTenantUsersPromise = null;
        this._securityTenantUsersSubject.next(this._securityTenantUsers);      // Emit to observable
    }

    public get HasSecurityTenantUsers(): Promise<boolean> {
        return this.SecurityTenantUsers.then(securityTenantUsers => securityTenantUsers.length > 0);
    }




    /**
     * Updates the state of this SecurityTenantData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SecurityTenantData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SecurityTenantSubmitData {
        return SecurityTenantService.Instance.ConvertToSecurityTenantSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SecurityTenantService extends SecureEndpointBase {

    private static _instance: SecurityTenantService;
    private listCache: Map<string, Observable<Array<SecurityTenantData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SecurityTenantBasicListData>>>;
    private recordCache: Map<string, Observable<SecurityTenantData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private securityOrganizationService: SecurityOrganizationService,
        private securityUserService: SecurityUserService,
        private securityTenantUserService: SecurityTenantUserService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SecurityTenantData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SecurityTenantBasicListData>>>();
        this.recordCache = new Map<string, Observable<SecurityTenantData>>();

        SecurityTenantService._instance = this;
    }

    public static get Instance(): SecurityTenantService {
      return SecurityTenantService._instance;
    }


    public ClearListCaches(config: SecurityTenantQueryParameters | null = null) {

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


    public ConvertToSecurityTenantSubmitData(data: SecurityTenantData): SecurityTenantSubmitData {

        let output = new SecurityTenantSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSecurityTenant(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityTenantData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const securityTenant$ = this.requestSecurityTenant(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityTenant", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, securityTenant$);

            return securityTenant$;
        }

        return this.recordCache.get(configHash) as Observable<SecurityTenantData>;
    }

    private requestSecurityTenant(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityTenantData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SecurityTenantData>(this.baseUrl + 'api/SecurityTenant/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSecurityTenant(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityTenant(id, includeRelations));
            }));
    }

    public GetSecurityTenantList(config: SecurityTenantQueryParameters | any = null) : Observable<Array<SecurityTenantData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const securityTenantList$ = this.requestSecurityTenantList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityTenant list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, securityTenantList$);

            return securityTenantList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SecurityTenantData>>;
    }


    private requestSecurityTenantList(config: SecurityTenantQueryParameters | any) : Observable <Array<SecurityTenantData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityTenantData>>(this.baseUrl + 'api/SecurityTenants', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSecurityTenantList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityTenantList(config));
            }));
    }

    public GetSecurityTenantsRowCount(config: SecurityTenantQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const securityTenantsRowCount$ = this.requestSecurityTenantsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityTenants row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, securityTenantsRowCount$);

            return securityTenantsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSecurityTenantsRowCount(config: SecurityTenantQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SecurityTenants/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityTenantsRowCount(config));
            }));
    }

    public GetSecurityTenantsBasicListData(config: SecurityTenantQueryParameters | any = null) : Observable<Array<SecurityTenantBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const securityTenantsBasicListData$ = this.requestSecurityTenantsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityTenants basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, securityTenantsBasicListData$);

            return securityTenantsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SecurityTenantBasicListData>>;
    }


    private requestSecurityTenantsBasicListData(config: SecurityTenantQueryParameters | any) : Observable<Array<SecurityTenantBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityTenantBasicListData>>(this.baseUrl + 'api/SecurityTenants/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityTenantsBasicListData(config));
            }));

    }


    public PutSecurityTenant(id: bigint | number, securityTenant: SecurityTenantSubmitData) : Observable<SecurityTenantData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SecurityTenantData>(this.baseUrl + 'api/SecurityTenant/' + id.toString(), securityTenant, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityTenant(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSecurityTenant(id, securityTenant));
            }));
    }


    public PostSecurityTenant(securityTenant: SecurityTenantSubmitData) : Observable<SecurityTenantData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SecurityTenantData>(this.baseUrl + 'api/SecurityTenant', securityTenant, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityTenant(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSecurityTenant(securityTenant));
            }));
    }

  
    public DeleteSecurityTenant(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SecurityTenant/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSecurityTenant(id));
            }));
    }


    private getConfigHash(config: SecurityTenantQueryParameters | any): string {

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

    public userIsSecuritySecurityTenantReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecuritySecurityTenantReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.SecurityTenants
        //
        if (userIsSecuritySecurityTenantReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecuritySecurityTenantReader = user.readPermission >= 0;
            } else {
                userIsSecuritySecurityTenantReader = false;
            }
        }

        return userIsSecuritySecurityTenantReader;
    }


    public userIsSecuritySecurityTenantWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecuritySecurityTenantWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.SecurityTenants
        //
        if (userIsSecuritySecurityTenantWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecuritySecurityTenantWriter = user.writePermission >= 100;
          } else {
            userIsSecuritySecurityTenantWriter = false;
          }      
        }

        return userIsSecuritySecurityTenantWriter;
    }

    public GetSecurityOrganizationsForSecurityTenant(securityTenantId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityOrganizationData[]> {
        return this.securityOrganizationService.GetSecurityOrganizationList({
            securityTenantId: securityTenantId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityUsersForSecurityTenant(securityTenantId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityUserData[]> {
        return this.securityUserService.GetSecurityUserList({
            securityTenantId: securityTenantId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityTenantUsersForSecurityTenant(securityTenantId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityTenantUserData[]> {
        return this.securityTenantUserService.GetSecurityTenantUserList({
            securityTenantId: securityTenantId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SecurityTenantData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SecurityTenantData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SecurityTenantTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSecurityTenant(raw: any): SecurityTenantData {
    if (!raw) return raw;

    //
    // Create a SecurityTenantData object instance with correct prototype
    //
    const revived = Object.create(SecurityTenantData.prototype) as SecurityTenantData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._securityOrganizations = null;
    (revived as any)._securityOrganizationsPromise = null;
    (revived as any)._securityOrganizationsSubject = new BehaviorSubject<SecurityOrganizationData[] | null>(null);

    (revived as any)._securityUsers = null;
    (revived as any)._securityUsersPromise = null;
    (revived as any)._securityUsersSubject = new BehaviorSubject<SecurityUserData[] | null>(null);

    (revived as any)._securityTenantUsers = null;
    (revived as any)._securityTenantUsersPromise = null;
    (revived as any)._securityTenantUsersSubject = new BehaviorSubject<SecurityTenantUserData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSecurityTenantXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SecurityOrganizations$ = (revived as any)._securityOrganizationsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityOrganizations === null && (revived as any)._securityOrganizationsPromise === null) {
                (revived as any).loadSecurityOrganizations();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityOrganizationsCount$ = SecurityOrganizationService.Instance.GetSecurityOrganizationsRowCount({securityTenantId: (revived as any).id,
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

    (revived as any).SecurityUsersCount$ = SecurityUserService.Instance.GetSecurityUsersRowCount({securityTenantId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SecurityTenantUsers$ = (revived as any)._securityTenantUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityTenantUsers === null && (revived as any)._securityTenantUsersPromise === null) {
                (revived as any).loadSecurityTenantUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityTenantUsersCount$ = SecurityTenantUserService.Instance.GetSecurityTenantUsersRowCount({securityTenantId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveSecurityTenantList(rawList: any[]): SecurityTenantData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSecurityTenant(raw));
  }

}
