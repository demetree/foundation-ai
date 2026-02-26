/*

   GENERATED SERVICE FOR THE SECURITYDEPARTMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SecurityDepartment table.

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
import { SecurityOrganizationData } from './security-organization.service';
import { SecurityTeamService, SecurityTeamData } from './security-team.service';
import { SecurityUserService, SecurityUserData } from './security-user.service';
import { SecurityDepartmentUserService, SecurityDepartmentUserData } from './security-department-user.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SecurityDepartmentQueryParameters {
    securityOrganizationId: bigint | number | null | undefined = null;
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
export class SecurityDepartmentSubmitData {
    id!: bigint | number;
    securityOrganizationId!: bigint | number;
    name!: string;
    description: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class SecurityDepartmentBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SecurityDepartmentChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `securityDepartment.SecurityDepartmentChildren$` — use with `| async` in templates
//        • Promise:    `securityDepartment.SecurityDepartmentChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="securityDepartment.SecurityDepartmentChildren$ | async"`), or
//        • Access the promise getter (`securityDepartment.SecurityDepartmentChildren` or `await securityDepartment.SecurityDepartmentChildren`)
//    - Simply reading `securityDepartment.SecurityDepartmentChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await securityDepartment.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SecurityDepartmentData {
    id!: bigint | number;
    securityOrganizationId!: bigint | number;
    name!: string;
    description!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    securityOrganization: SecurityOrganizationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _securityTeams: SecurityTeamData[] | null = null;
    private _securityTeamsPromise: Promise<SecurityTeamData[]> | null  = null;
    private _securityTeamsSubject = new BehaviorSubject<SecurityTeamData[] | null>(null);

                
    private _securityUsers: SecurityUserData[] | null = null;
    private _securityUsersPromise: Promise<SecurityUserData[]> | null  = null;
    private _securityUsersSubject = new BehaviorSubject<SecurityUserData[] | null>(null);

                
    private _securityDepartmentUsers: SecurityDepartmentUserData[] | null = null;
    private _securityDepartmentUsersPromise: Promise<SecurityDepartmentUserData[]> | null  = null;
    private _securityDepartmentUsersSubject = new BehaviorSubject<SecurityDepartmentUserData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SecurityTeams$ = this._securityTeamsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityTeams === null && this._securityTeamsPromise === null) {
            this.loadSecurityTeams(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _securityTeamsCount$: Observable<bigint | number> | null = null;
    public get SecurityTeamsCount$(): Observable<bigint | number> {
        if (this._securityTeamsCount$ === null) {
            this._securityTeamsCount$ = SecurityTeamService.Instance.GetSecurityTeamsRowCount({securityDepartmentId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._securityTeamsCount$;
    }



    public SecurityUsers$ = this._securityUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityUsers === null && this._securityUsersPromise === null) {
            this.loadSecurityUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _securityUsersCount$: Observable<bigint | number> | null = null;
    public get SecurityUsersCount$(): Observable<bigint | number> {
        if (this._securityUsersCount$ === null) {
            this._securityUsersCount$ = SecurityUserService.Instance.GetSecurityUsersRowCount({securityDepartmentId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._securityUsersCount$;
    }



    public SecurityDepartmentUsers$ = this._securityDepartmentUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityDepartmentUsers === null && this._securityDepartmentUsersPromise === null) {
            this.loadSecurityDepartmentUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _securityDepartmentUsersCount$: Observable<bigint | number> | null = null;
    public get SecurityDepartmentUsersCount$(): Observable<bigint | number> {
        if (this._securityDepartmentUsersCount$ === null) {
            this._securityDepartmentUsersCount$ = SecurityDepartmentUserService.Instance.GetSecurityDepartmentUsersRowCount({securityDepartmentId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._securityDepartmentUsersCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SecurityDepartmentData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.securityDepartment.Reload();
  //
  //  Non Async:
  //
  //     securityDepartment[0].Reload().then(x => {
  //        this.securityDepartment = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SecurityDepartmentService.Instance.GetSecurityDepartment(this.id, includeRelations)
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
     this._securityTeams = null;
     this._securityTeamsPromise = null;
     this._securityTeamsSubject.next(null);
     this._securityTeamsCount$ = null;

     this._securityUsers = null;
     this._securityUsersPromise = null;
     this._securityUsersSubject.next(null);
     this._securityUsersCount$ = null;

     this._securityDepartmentUsers = null;
     this._securityDepartmentUsersPromise = null;
     this._securityDepartmentUsersSubject.next(null);
     this._securityDepartmentUsersCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the SecurityTeams for this SecurityDepartment.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityDepartment.SecurityTeams.then(securityDepartments => { ... })
     *   or
     *   await this.securityDepartment.securityDepartments
     *
    */
    public get SecurityTeams(): Promise<SecurityTeamData[]> {
        if (this._securityTeams !== null) {
            return Promise.resolve(this._securityTeams);
        }

        if (this._securityTeamsPromise !== null) {
            return this._securityTeamsPromise;
        }

        // Start the load
        this.loadSecurityTeams();

        return this._securityTeamsPromise!;
    }



    private loadSecurityTeams(): void {

        this._securityTeamsPromise = lastValueFrom(
            SecurityDepartmentService.Instance.GetSecurityTeamsForSecurityDepartment(this.id)
        )
        .then(SecurityTeams => {
            this._securityTeams = SecurityTeams ?? [];
            this._securityTeamsSubject.next(this._securityTeams);
            return this._securityTeams;
         })
        .catch(err => {
            this._securityTeams = [];
            this._securityTeamsSubject.next(this._securityTeams);
            throw err;
        })
        .finally(() => {
            this._securityTeamsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SecurityTeam. Call after mutations to force refresh.
     */
    public ClearSecurityTeamsCache(): void {
        this._securityTeams = null;
        this._securityTeamsPromise = null;
        this._securityTeamsSubject.next(this._securityTeams);      // Emit to observable
    }

    public get HasSecurityTeams(): Promise<boolean> {
        return this.SecurityTeams.then(securityTeams => securityTeams.length > 0);
    }


    /**
     *
     * Gets the SecurityUsers for this SecurityDepartment.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityDepartment.SecurityUsers.then(securityDepartments => { ... })
     *   or
     *   await this.securityDepartment.securityDepartments
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
            SecurityDepartmentService.Instance.GetSecurityUsersForSecurityDepartment(this.id)
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
     * Gets the SecurityDepartmentUsers for this SecurityDepartment.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityDepartment.SecurityDepartmentUsers.then(securityDepartments => { ... })
     *   or
     *   await this.securityDepartment.securityDepartments
     *
    */
    public get SecurityDepartmentUsers(): Promise<SecurityDepartmentUserData[]> {
        if (this._securityDepartmentUsers !== null) {
            return Promise.resolve(this._securityDepartmentUsers);
        }

        if (this._securityDepartmentUsersPromise !== null) {
            return this._securityDepartmentUsersPromise;
        }

        // Start the load
        this.loadSecurityDepartmentUsers();

        return this._securityDepartmentUsersPromise!;
    }



    private loadSecurityDepartmentUsers(): void {

        this._securityDepartmentUsersPromise = lastValueFrom(
            SecurityDepartmentService.Instance.GetSecurityDepartmentUsersForSecurityDepartment(this.id)
        )
        .then(SecurityDepartmentUsers => {
            this._securityDepartmentUsers = SecurityDepartmentUsers ?? [];
            this._securityDepartmentUsersSubject.next(this._securityDepartmentUsers);
            return this._securityDepartmentUsers;
         })
        .catch(err => {
            this._securityDepartmentUsers = [];
            this._securityDepartmentUsersSubject.next(this._securityDepartmentUsers);
            throw err;
        })
        .finally(() => {
            this._securityDepartmentUsersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SecurityDepartmentUser. Call after mutations to force refresh.
     */
    public ClearSecurityDepartmentUsersCache(): void {
        this._securityDepartmentUsers = null;
        this._securityDepartmentUsersPromise = null;
        this._securityDepartmentUsersSubject.next(this._securityDepartmentUsers);      // Emit to observable
    }

    public get HasSecurityDepartmentUsers(): Promise<boolean> {
        return this.SecurityDepartmentUsers.then(securityDepartmentUsers => securityDepartmentUsers.length > 0);
    }




    /**
     * Updates the state of this SecurityDepartmentData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SecurityDepartmentData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SecurityDepartmentSubmitData {
        return SecurityDepartmentService.Instance.ConvertToSecurityDepartmentSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SecurityDepartmentService extends SecureEndpointBase {

    private static _instance: SecurityDepartmentService;
    private listCache: Map<string, Observable<Array<SecurityDepartmentData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SecurityDepartmentBasicListData>>>;
    private recordCache: Map<string, Observable<SecurityDepartmentData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private securityTeamService: SecurityTeamService,
        private securityUserService: SecurityUserService,
        private securityDepartmentUserService: SecurityDepartmentUserService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SecurityDepartmentData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SecurityDepartmentBasicListData>>>();
        this.recordCache = new Map<string, Observable<SecurityDepartmentData>>();

        SecurityDepartmentService._instance = this;
    }

    public static get Instance(): SecurityDepartmentService {
      return SecurityDepartmentService._instance;
    }


    public ClearListCaches(config: SecurityDepartmentQueryParameters | null = null) {

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


    public ConvertToSecurityDepartmentSubmitData(data: SecurityDepartmentData): SecurityDepartmentSubmitData {

        let output = new SecurityDepartmentSubmitData();

        output.id = data.id;
        output.securityOrganizationId = data.securityOrganizationId;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSecurityDepartment(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityDepartmentData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const securityDepartment$ = this.requestSecurityDepartment(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityDepartment", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, securityDepartment$);

            return securityDepartment$;
        }

        return this.recordCache.get(configHash) as Observable<SecurityDepartmentData>;
    }

    private requestSecurityDepartment(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityDepartmentData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SecurityDepartmentData>(this.baseUrl + 'api/SecurityDepartment/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSecurityDepartment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityDepartment(id, includeRelations));
            }));
    }

    public GetSecurityDepartmentList(config: SecurityDepartmentQueryParameters | any = null) : Observable<Array<SecurityDepartmentData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const securityDepartmentList$ = this.requestSecurityDepartmentList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityDepartment list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, securityDepartmentList$);

            return securityDepartmentList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SecurityDepartmentData>>;
    }


    private requestSecurityDepartmentList(config: SecurityDepartmentQueryParameters | any) : Observable <Array<SecurityDepartmentData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityDepartmentData>>(this.baseUrl + 'api/SecurityDepartments', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSecurityDepartmentList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityDepartmentList(config));
            }));
    }

    public GetSecurityDepartmentsRowCount(config: SecurityDepartmentQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const securityDepartmentsRowCount$ = this.requestSecurityDepartmentsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityDepartments row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, securityDepartmentsRowCount$);

            return securityDepartmentsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSecurityDepartmentsRowCount(config: SecurityDepartmentQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SecurityDepartments/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityDepartmentsRowCount(config));
            }));
    }

    public GetSecurityDepartmentsBasicListData(config: SecurityDepartmentQueryParameters | any = null) : Observable<Array<SecurityDepartmentBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const securityDepartmentsBasicListData$ = this.requestSecurityDepartmentsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityDepartments basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, securityDepartmentsBasicListData$);

            return securityDepartmentsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SecurityDepartmentBasicListData>>;
    }


    private requestSecurityDepartmentsBasicListData(config: SecurityDepartmentQueryParameters | any) : Observable<Array<SecurityDepartmentBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityDepartmentBasicListData>>(this.baseUrl + 'api/SecurityDepartments/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityDepartmentsBasicListData(config));
            }));

    }


    public PutSecurityDepartment(id: bigint | number, securityDepartment: SecurityDepartmentSubmitData) : Observable<SecurityDepartmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SecurityDepartmentData>(this.baseUrl + 'api/SecurityDepartment/' + id.toString(), securityDepartment, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityDepartment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSecurityDepartment(id, securityDepartment));
            }));
    }


    public PostSecurityDepartment(securityDepartment: SecurityDepartmentSubmitData) : Observable<SecurityDepartmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SecurityDepartmentData>(this.baseUrl + 'api/SecurityDepartment', securityDepartment, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityDepartment(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSecurityDepartment(securityDepartment));
            }));
    }

  
    public DeleteSecurityDepartment(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SecurityDepartment/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSecurityDepartment(id));
            }));
    }


    private getConfigHash(config: SecurityDepartmentQueryParameters | any): string {

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

    public userIsSecuritySecurityDepartmentReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecuritySecurityDepartmentReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.SecurityDepartments
        //
        if (userIsSecuritySecurityDepartmentReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecuritySecurityDepartmentReader = user.readPermission >= 1;
            } else {
                userIsSecuritySecurityDepartmentReader = false;
            }
        }

        return userIsSecuritySecurityDepartmentReader;
    }


    public userIsSecuritySecurityDepartmentWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecuritySecurityDepartmentWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.SecurityDepartments
        //
        if (userIsSecuritySecurityDepartmentWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecuritySecurityDepartmentWriter = user.writePermission >= 100;
          } else {
            userIsSecuritySecurityDepartmentWriter = false;
          }      
        }

        return userIsSecuritySecurityDepartmentWriter;
    }

    public GetSecurityTeamsForSecurityDepartment(securityDepartmentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityTeamData[]> {
        return this.securityTeamService.GetSecurityTeamList({
            securityDepartmentId: securityDepartmentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityUsersForSecurityDepartment(securityDepartmentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityUserData[]> {
        return this.securityUserService.GetSecurityUserList({
            securityDepartmentId: securityDepartmentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityDepartmentUsersForSecurityDepartment(securityDepartmentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityDepartmentUserData[]> {
        return this.securityDepartmentUserService.GetSecurityDepartmentUserList({
            securityDepartmentId: securityDepartmentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SecurityDepartmentData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SecurityDepartmentData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SecurityDepartmentTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSecurityDepartment(raw: any): SecurityDepartmentData {
    if (!raw) return raw;

    //
    // Create a SecurityDepartmentData object instance with correct prototype
    //
    const revived = Object.create(SecurityDepartmentData.prototype) as SecurityDepartmentData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._securityTeams = null;
    (revived as any)._securityTeamsPromise = null;
    (revived as any)._securityTeamsSubject = new BehaviorSubject<SecurityTeamData[] | null>(null);

    (revived as any)._securityUsers = null;
    (revived as any)._securityUsersPromise = null;
    (revived as any)._securityUsersSubject = new BehaviorSubject<SecurityUserData[] | null>(null);

    (revived as any)._securityDepartmentUsers = null;
    (revived as any)._securityDepartmentUsersPromise = null;
    (revived as any)._securityDepartmentUsersSubject = new BehaviorSubject<SecurityDepartmentUserData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSecurityDepartmentXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SecurityTeams$ = (revived as any)._securityTeamsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityTeams === null && (revived as any)._securityTeamsPromise === null) {
                (revived as any).loadSecurityTeams();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._securityTeamsCount$ = null;


    (revived as any).SecurityUsers$ = (revived as any)._securityUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityUsers === null && (revived as any)._securityUsersPromise === null) {
                (revived as any).loadSecurityUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._securityUsersCount$ = null;


    (revived as any).SecurityDepartmentUsers$ = (revived as any)._securityDepartmentUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityDepartmentUsers === null && (revived as any)._securityDepartmentUsersPromise === null) {
                (revived as any).loadSecurityDepartmentUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._securityDepartmentUsersCount$ = null;



    return revived;
  }

  private ReviveSecurityDepartmentList(rawList: any[]): SecurityDepartmentData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSecurityDepartment(raw));
  }

}
