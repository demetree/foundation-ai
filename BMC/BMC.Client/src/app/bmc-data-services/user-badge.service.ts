/*

   GENERATED SERVICE FOR THE USERBADGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserBadge table.

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
import { UserBadgeAssignmentService, UserBadgeAssignmentData } from './user-badge-assignment.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserBadgeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    iconCssClass: string | null | undefined = null;
    iconImagePath: string | null | undefined = null;
    badgeColor: string | null | undefined = null;
    isAutomatic: boolean | null | undefined = null;
    automaticCriteriaCode: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class UserBadgeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    iconCssClass: string | null = null;
    iconImagePath: string | null = null;
    badgeColor: string | null = null;
    isAutomatic!: boolean;
    automaticCriteriaCode: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class UserBadgeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserBadgeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userBadge.UserBadgeChildren$` — use with `| async` in templates
//        • Promise:    `userBadge.UserBadgeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userBadge.UserBadgeChildren$ | async"`), or
//        • Access the promise getter (`userBadge.UserBadgeChildren` or `await userBadge.UserBadgeChildren`)
//    - Simply reading `userBadge.UserBadgeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userBadge.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserBadgeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    iconCssClass!: string | null;
    iconImagePath!: string | null;
    badgeColor!: string | null;
    isAutomatic!: boolean;
    automaticCriteriaCode!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _userBadgeAssignments: UserBadgeAssignmentData[] | null = null;
    private _userBadgeAssignmentsPromise: Promise<UserBadgeAssignmentData[]> | null  = null;
    private _userBadgeAssignmentsSubject = new BehaviorSubject<UserBadgeAssignmentData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public UserBadgeAssignments$ = this._userBadgeAssignmentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userBadgeAssignments === null && this._userBadgeAssignmentsPromise === null) {
            this.loadUserBadgeAssignments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userBadgeAssignmentsCount$: Observable<bigint | number> | null = null;
    public get UserBadgeAssignmentsCount$(): Observable<bigint | number> {
        if (this._userBadgeAssignmentsCount$ === null) {
            this._userBadgeAssignmentsCount$ = UserBadgeAssignmentService.Instance.GetUserBadgeAssignmentsRowCount({userBadgeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userBadgeAssignmentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any UserBadgeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userBadge.Reload();
  //
  //  Non Async:
  //
  //     userBadge[0].Reload().then(x => {
  //        this.userBadge = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserBadgeService.Instance.GetUserBadge(this.id, includeRelations)
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
     this._userBadgeAssignments = null;
     this._userBadgeAssignmentsPromise = null;
     this._userBadgeAssignmentsSubject.next(null);
     this._userBadgeAssignmentsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the UserBadgeAssignments for this UserBadge.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userBadge.UserBadgeAssignments.then(userBadges => { ... })
     *   or
     *   await this.userBadge.userBadges
     *
    */
    public get UserBadgeAssignments(): Promise<UserBadgeAssignmentData[]> {
        if (this._userBadgeAssignments !== null) {
            return Promise.resolve(this._userBadgeAssignments);
        }

        if (this._userBadgeAssignmentsPromise !== null) {
            return this._userBadgeAssignmentsPromise;
        }

        // Start the load
        this.loadUserBadgeAssignments();

        return this._userBadgeAssignmentsPromise!;
    }



    private loadUserBadgeAssignments(): void {

        this._userBadgeAssignmentsPromise = lastValueFrom(
            UserBadgeService.Instance.GetUserBadgeAssignmentsForUserBadge(this.id)
        )
        .then(UserBadgeAssignments => {
            this._userBadgeAssignments = UserBadgeAssignments ?? [];
            this._userBadgeAssignmentsSubject.next(this._userBadgeAssignments);
            return this._userBadgeAssignments;
         })
        .catch(err => {
            this._userBadgeAssignments = [];
            this._userBadgeAssignmentsSubject.next(this._userBadgeAssignments);
            throw err;
        })
        .finally(() => {
            this._userBadgeAssignmentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserBadgeAssignment. Call after mutations to force refresh.
     */
    public ClearUserBadgeAssignmentsCache(): void {
        this._userBadgeAssignments = null;
        this._userBadgeAssignmentsPromise = null;
        this._userBadgeAssignmentsSubject.next(this._userBadgeAssignments);      // Emit to observable
    }

    public get HasUserBadgeAssignments(): Promise<boolean> {
        return this.UserBadgeAssignments.then(userBadgeAssignments => userBadgeAssignments.length > 0);
    }




    /**
     * Updates the state of this UserBadgeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserBadgeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserBadgeSubmitData {
        return UserBadgeService.Instance.ConvertToUserBadgeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserBadgeService extends SecureEndpointBase {

    private static _instance: UserBadgeService;
    private listCache: Map<string, Observable<Array<UserBadgeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserBadgeBasicListData>>>;
    private recordCache: Map<string, Observable<UserBadgeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private userBadgeAssignmentService: UserBadgeAssignmentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserBadgeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserBadgeBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserBadgeData>>();

        UserBadgeService._instance = this;
    }

    public static get Instance(): UserBadgeService {
      return UserBadgeService._instance;
    }


    public ClearListCaches(config: UserBadgeQueryParameters | null = null) {

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


    public ConvertToUserBadgeSubmitData(data: UserBadgeData): UserBadgeSubmitData {

        let output = new UserBadgeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.iconCssClass = data.iconCssClass;
        output.iconImagePath = data.iconImagePath;
        output.badgeColor = data.badgeColor;
        output.isAutomatic = data.isAutomatic;
        output.automaticCriteriaCode = data.automaticCriteriaCode;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserBadge(id: bigint | number, includeRelations: boolean = true) : Observable<UserBadgeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userBadge$ = this.requestUserBadge(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserBadge", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userBadge$);

            return userBadge$;
        }

        return this.recordCache.get(configHash) as Observable<UserBadgeData>;
    }

    private requestUserBadge(id: bigint | number, includeRelations: boolean = true) : Observable<UserBadgeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserBadgeData>(this.baseUrl + 'api/UserBadge/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserBadge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserBadge(id, includeRelations));
            }));
    }

    public GetUserBadgeList(config: UserBadgeQueryParameters | any = null) : Observable<Array<UserBadgeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userBadgeList$ = this.requestUserBadgeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserBadge list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userBadgeList$);

            return userBadgeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserBadgeData>>;
    }


    private requestUserBadgeList(config: UserBadgeQueryParameters | any) : Observable <Array<UserBadgeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserBadgeData>>(this.baseUrl + 'api/UserBadges', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserBadgeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserBadgeList(config));
            }));
    }

    public GetUserBadgesRowCount(config: UserBadgeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userBadgesRowCount$ = this.requestUserBadgesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserBadges row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userBadgesRowCount$);

            return userBadgesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserBadgesRowCount(config: UserBadgeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserBadges/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserBadgesRowCount(config));
            }));
    }

    public GetUserBadgesBasicListData(config: UserBadgeQueryParameters | any = null) : Observable<Array<UserBadgeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userBadgesBasicListData$ = this.requestUserBadgesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserBadges basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userBadgesBasicListData$);

            return userBadgesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserBadgeBasicListData>>;
    }


    private requestUserBadgesBasicListData(config: UserBadgeQueryParameters | any) : Observable<Array<UserBadgeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserBadgeBasicListData>>(this.baseUrl + 'api/UserBadges/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserBadgesBasicListData(config));
            }));

    }


    public PutUserBadge(id: bigint | number, userBadge: UserBadgeSubmitData) : Observable<UserBadgeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserBadgeData>(this.baseUrl + 'api/UserBadge/' + id.toString(), userBadge, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserBadge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserBadge(id, userBadge));
            }));
    }


    public PostUserBadge(userBadge: UserBadgeSubmitData) : Observable<UserBadgeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserBadgeData>(this.baseUrl + 'api/UserBadge', userBadge, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserBadge(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserBadge(userBadge));
            }));
    }

  
    public DeleteUserBadge(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserBadge/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserBadge(id));
            }));
    }


    private getConfigHash(config: UserBadgeQueryParameters | any): string {

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

    public userIsBMCUserBadgeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCUserBadgeReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.UserBadges
        //
        if (userIsBMCUserBadgeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCUserBadgeReader = user.readPermission >= 1;
            } else {
                userIsBMCUserBadgeReader = false;
            }
        }

        return userIsBMCUserBadgeReader;
    }


    public userIsBMCUserBadgeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCUserBadgeWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.UserBadges
        //
        if (userIsBMCUserBadgeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCUserBadgeWriter = user.writePermission >= 255;
          } else {
            userIsBMCUserBadgeWriter = false;
          }      
        }

        return userIsBMCUserBadgeWriter;
    }

    public GetUserBadgeAssignmentsForUserBadge(userBadgeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserBadgeAssignmentData[]> {
        return this.userBadgeAssignmentService.GetUserBadgeAssignmentList({
            userBadgeId: userBadgeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full UserBadgeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserBadgeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserBadgeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserBadge(raw: any): UserBadgeData {
    if (!raw) return raw;

    //
    // Create a UserBadgeData object instance with correct prototype
    //
    const revived = Object.create(UserBadgeData.prototype) as UserBadgeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._userBadgeAssignments = null;
    (revived as any)._userBadgeAssignmentsPromise = null;
    (revived as any)._userBadgeAssignmentsSubject = new BehaviorSubject<UserBadgeAssignmentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadUserBadgeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).UserBadgeAssignments$ = (revived as any)._userBadgeAssignmentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userBadgeAssignments === null && (revived as any)._userBadgeAssignmentsPromise === null) {
                (revived as any).loadUserBadgeAssignments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userBadgeAssignmentsCount$ = null;



    return revived;
  }

  private ReviveUserBadgeList(rawList: any[]): UserBadgeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserBadge(raw));
  }

}
