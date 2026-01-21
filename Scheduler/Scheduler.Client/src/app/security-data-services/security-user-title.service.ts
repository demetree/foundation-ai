/*

   GENERATED SERVICE FOR THE SECURITYUSERTITLE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SecurityUserTitle table.

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
import { SecurityUserService, SecurityUserData } from './security-user.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SecurityUserTitleQueryParameters {
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
export class SecurityUserTitleSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class SecurityUserTitleBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SecurityUserTitleChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `securityUserTitle.SecurityUserTitleChildren$` — use with `| async` in templates
//        • Promise:    `securityUserTitle.SecurityUserTitleChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="securityUserTitle.SecurityUserTitleChildren$ | async"`), or
//        • Access the promise getter (`securityUserTitle.SecurityUserTitleChildren` or `await securityUserTitle.SecurityUserTitleChildren`)
//    - Simply reading `securityUserTitle.SecurityUserTitleChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await securityUserTitle.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SecurityUserTitleData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _securityUsers: SecurityUserData[] | null = null;
    private _securityUsersPromise: Promise<SecurityUserData[]> | null  = null;
    private _securityUsersSubject = new BehaviorSubject<SecurityUserData[] | null>(null);

                

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

  
    public SecurityUsersCount$ = SecurityUserService.Instance.GetSecurityUsersRowCount({securityUserTitleId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SecurityUserTitleData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.securityUserTitle.Reload();
  //
  //  Non Async:
  //
  //     securityUserTitle[0].Reload().then(x => {
  //        this.securityUserTitle = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SecurityUserTitleService.Instance.GetSecurityUserTitle(this.id, includeRelations)
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

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the SecurityUsers for this SecurityUserTitle.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityUserTitle.SecurityUsers.then(securityUserTitles => { ... })
     *   or
     *   await this.securityUserTitle.securityUserTitles
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
            SecurityUserTitleService.Instance.GetSecurityUsersForSecurityUserTitle(this.id)
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
     * Updates the state of this SecurityUserTitleData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SecurityUserTitleData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SecurityUserTitleSubmitData {
        return SecurityUserTitleService.Instance.ConvertToSecurityUserTitleSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SecurityUserTitleService extends SecureEndpointBase {

    private static _instance: SecurityUserTitleService;
    private listCache: Map<string, Observable<Array<SecurityUserTitleData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SecurityUserTitleBasicListData>>>;
    private recordCache: Map<string, Observable<SecurityUserTitleData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private securityUserService: SecurityUserService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SecurityUserTitleData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SecurityUserTitleBasicListData>>>();
        this.recordCache = new Map<string, Observable<SecurityUserTitleData>>();

        SecurityUserTitleService._instance = this;
    }

    public static get Instance(): SecurityUserTitleService {
      return SecurityUserTitleService._instance;
    }


    public ClearListCaches(config: SecurityUserTitleQueryParameters | null = null) {

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


    public ConvertToSecurityUserTitleSubmitData(data: SecurityUserTitleData): SecurityUserTitleSubmitData {

        let output = new SecurityUserTitleSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSecurityUserTitle(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityUserTitleData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const securityUserTitle$ = this.requestSecurityUserTitle(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUserTitle", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, securityUserTitle$);

            return securityUserTitle$;
        }

        return this.recordCache.get(configHash) as Observable<SecurityUserTitleData>;
    }

    private requestSecurityUserTitle(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityUserTitleData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SecurityUserTitleData>(this.baseUrl + 'api/SecurityUserTitle/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSecurityUserTitle(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserTitle(id, includeRelations));
            }));
    }

    public GetSecurityUserTitleList(config: SecurityUserTitleQueryParameters | any = null) : Observable<Array<SecurityUserTitleData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const securityUserTitleList$ = this.requestSecurityUserTitleList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUserTitle list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, securityUserTitleList$);

            return securityUserTitleList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SecurityUserTitleData>>;
    }


    private requestSecurityUserTitleList(config: SecurityUserTitleQueryParameters | any) : Observable <Array<SecurityUserTitleData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityUserTitleData>>(this.baseUrl + 'api/SecurityUserTitles', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSecurityUserTitleList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserTitleList(config));
            }));
    }

    public GetSecurityUserTitlesRowCount(config: SecurityUserTitleQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const securityUserTitlesRowCount$ = this.requestSecurityUserTitlesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUserTitles row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, securityUserTitlesRowCount$);

            return securityUserTitlesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSecurityUserTitlesRowCount(config: SecurityUserTitleQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SecurityUserTitles/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserTitlesRowCount(config));
            }));
    }

    public GetSecurityUserTitlesBasicListData(config: SecurityUserTitleQueryParameters | any = null) : Observable<Array<SecurityUserTitleBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const securityUserTitlesBasicListData$ = this.requestSecurityUserTitlesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUserTitles basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, securityUserTitlesBasicListData$);

            return securityUserTitlesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SecurityUserTitleBasicListData>>;
    }


    private requestSecurityUserTitlesBasicListData(config: SecurityUserTitleQueryParameters | any) : Observable<Array<SecurityUserTitleBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityUserTitleBasicListData>>(this.baseUrl + 'api/SecurityUserTitles/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserTitlesBasicListData(config));
            }));

    }


    public PutSecurityUserTitle(id: bigint | number, securityUserTitle: SecurityUserTitleSubmitData) : Observable<SecurityUserTitleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SecurityUserTitleData>(this.baseUrl + 'api/SecurityUserTitle/' + id.toString(), securityUserTitle, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityUserTitle(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSecurityUserTitle(id, securityUserTitle));
            }));
    }


    public PostSecurityUserTitle(securityUserTitle: SecurityUserTitleSubmitData) : Observable<SecurityUserTitleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SecurityUserTitleData>(this.baseUrl + 'api/SecurityUserTitle', securityUserTitle, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityUserTitle(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSecurityUserTitle(securityUserTitle));
            }));
    }

  
    public DeleteSecurityUserTitle(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SecurityUserTitle/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSecurityUserTitle(id));
            }));
    }


    private getConfigHash(config: SecurityUserTitleQueryParameters | any): string {

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

    public userIsSecuritySecurityUserTitleReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecuritySecurityUserTitleReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.SecurityUserTitles
        //
        if (userIsSecuritySecurityUserTitleReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecuritySecurityUserTitleReader = user.readPermission >= 0;
            } else {
                userIsSecuritySecurityUserTitleReader = false;
            }
        }

        return userIsSecuritySecurityUserTitleReader;
    }


    public userIsSecuritySecurityUserTitleWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecuritySecurityUserTitleWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.SecurityUserTitles
        //
        if (userIsSecuritySecurityUserTitleWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecuritySecurityUserTitleWriter = user.writePermission >= 50;
          } else {
            userIsSecuritySecurityUserTitleWriter = false;
          }      
        }

        return userIsSecuritySecurityUserTitleWriter;
    }

    public GetSecurityUsersForSecurityUserTitle(securityUserTitleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityUserData[]> {
        return this.securityUserService.GetSecurityUserList({
            securityUserTitleId: securityUserTitleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SecurityUserTitleData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SecurityUserTitleData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SecurityUserTitleTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSecurityUserTitle(raw: any): SecurityUserTitleData {
    if (!raw) return raw;

    //
    // Create a SecurityUserTitleData object instance with correct prototype
    //
    const revived = Object.create(SecurityUserTitleData.prototype) as SecurityUserTitleData;

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


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSecurityUserTitleXYZ, etc.) are not accessible via the typed variable
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

    (revived as any).SecurityUsersCount$ = SecurityUserService.Instance.GetSecurityUsersRowCount({securityUserTitleId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveSecurityUserTitleList(rawList: any[]): SecurityUserTitleData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSecurityUserTitle(raw));
  }

}
