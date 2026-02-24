/*

   GENERATED SERVICE FOR THE USERPROFILELINKTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserProfileLinkType table.

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
import { UserProfileLinkService, UserProfileLinkData } from './user-profile-link.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserProfileLinkTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    iconCssClass: string | null | undefined = null;
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
export class UserProfileLinkTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    iconCssClass: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class UserProfileLinkTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserProfileLinkTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userProfileLinkType.UserProfileLinkTypeChildren$` — use with `| async` in templates
//        • Promise:    `userProfileLinkType.UserProfileLinkTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userProfileLinkType.UserProfileLinkTypeChildren$ | async"`), or
//        • Access the promise getter (`userProfileLinkType.UserProfileLinkTypeChildren` or `await userProfileLinkType.UserProfileLinkTypeChildren`)
//    - Simply reading `userProfileLinkType.UserProfileLinkTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userProfileLinkType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserProfileLinkTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    iconCssClass!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _userProfileLinks: UserProfileLinkData[] | null = null;
    private _userProfileLinksPromise: Promise<UserProfileLinkData[]> | null  = null;
    private _userProfileLinksSubject = new BehaviorSubject<UserProfileLinkData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public UserProfileLinks$ = this._userProfileLinksSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userProfileLinks === null && this._userProfileLinksPromise === null) {
            this.loadUserProfileLinks(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userProfileLinksCount$: Observable<bigint | number> | null = null;
    public get UserProfileLinksCount$(): Observable<bigint | number> {
        if (this._userProfileLinksCount$ === null) {
            this._userProfileLinksCount$ = UserProfileLinkService.Instance.GetUserProfileLinksRowCount({userProfileLinkTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userProfileLinksCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any UserProfileLinkTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userProfileLinkType.Reload();
  //
  //  Non Async:
  //
  //     userProfileLinkType[0].Reload().then(x => {
  //        this.userProfileLinkType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserProfileLinkTypeService.Instance.GetUserProfileLinkType(this.id, includeRelations)
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
     this._userProfileLinks = null;
     this._userProfileLinksPromise = null;
     this._userProfileLinksSubject.next(null);
     this._userProfileLinksCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the UserProfileLinks for this UserProfileLinkType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userProfileLinkType.UserProfileLinks.then(userProfileLinkTypes => { ... })
     *   or
     *   await this.userProfileLinkType.userProfileLinkTypes
     *
    */
    public get UserProfileLinks(): Promise<UserProfileLinkData[]> {
        if (this._userProfileLinks !== null) {
            return Promise.resolve(this._userProfileLinks);
        }

        if (this._userProfileLinksPromise !== null) {
            return this._userProfileLinksPromise;
        }

        // Start the load
        this.loadUserProfileLinks();

        return this._userProfileLinksPromise!;
    }



    private loadUserProfileLinks(): void {

        this._userProfileLinksPromise = lastValueFrom(
            UserProfileLinkTypeService.Instance.GetUserProfileLinksForUserProfileLinkType(this.id)
        )
        .then(UserProfileLinks => {
            this._userProfileLinks = UserProfileLinks ?? [];
            this._userProfileLinksSubject.next(this._userProfileLinks);
            return this._userProfileLinks;
         })
        .catch(err => {
            this._userProfileLinks = [];
            this._userProfileLinksSubject.next(this._userProfileLinks);
            throw err;
        })
        .finally(() => {
            this._userProfileLinksPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserProfileLink. Call after mutations to force refresh.
     */
    public ClearUserProfileLinksCache(): void {
        this._userProfileLinks = null;
        this._userProfileLinksPromise = null;
        this._userProfileLinksSubject.next(this._userProfileLinks);      // Emit to observable
    }

    public get HasUserProfileLinks(): Promise<boolean> {
        return this.UserProfileLinks.then(userProfileLinks => userProfileLinks.length > 0);
    }




    /**
     * Updates the state of this UserProfileLinkTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserProfileLinkTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserProfileLinkTypeSubmitData {
        return UserProfileLinkTypeService.Instance.ConvertToUserProfileLinkTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserProfileLinkTypeService extends SecureEndpointBase {

    private static _instance: UserProfileLinkTypeService;
    private listCache: Map<string, Observable<Array<UserProfileLinkTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserProfileLinkTypeBasicListData>>>;
    private recordCache: Map<string, Observable<UserProfileLinkTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private userProfileLinkService: UserProfileLinkService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserProfileLinkTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserProfileLinkTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserProfileLinkTypeData>>();

        UserProfileLinkTypeService._instance = this;
    }

    public static get Instance(): UserProfileLinkTypeService {
      return UserProfileLinkTypeService._instance;
    }


    public ClearListCaches(config: UserProfileLinkTypeQueryParameters | null = null) {

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


    public ConvertToUserProfileLinkTypeSubmitData(data: UserProfileLinkTypeData): UserProfileLinkTypeSubmitData {

        let output = new UserProfileLinkTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.iconCssClass = data.iconCssClass;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserProfileLinkType(id: bigint | number, includeRelations: boolean = true) : Observable<UserProfileLinkTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userProfileLinkType$ = this.requestUserProfileLinkType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserProfileLinkType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userProfileLinkType$);

            return userProfileLinkType$;
        }

        return this.recordCache.get(configHash) as Observable<UserProfileLinkTypeData>;
    }

    private requestUserProfileLinkType(id: bigint | number, includeRelations: boolean = true) : Observable<UserProfileLinkTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserProfileLinkTypeData>(this.baseUrl + 'api/UserProfileLinkType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserProfileLinkType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserProfileLinkType(id, includeRelations));
            }));
    }

    public GetUserProfileLinkTypeList(config: UserProfileLinkTypeQueryParameters | any = null) : Observable<Array<UserProfileLinkTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userProfileLinkTypeList$ = this.requestUserProfileLinkTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserProfileLinkType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userProfileLinkTypeList$);

            return userProfileLinkTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserProfileLinkTypeData>>;
    }


    private requestUserProfileLinkTypeList(config: UserProfileLinkTypeQueryParameters | any) : Observable <Array<UserProfileLinkTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserProfileLinkTypeData>>(this.baseUrl + 'api/UserProfileLinkTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserProfileLinkTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserProfileLinkTypeList(config));
            }));
    }

    public GetUserProfileLinkTypesRowCount(config: UserProfileLinkTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userProfileLinkTypesRowCount$ = this.requestUserProfileLinkTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserProfileLinkTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userProfileLinkTypesRowCount$);

            return userProfileLinkTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserProfileLinkTypesRowCount(config: UserProfileLinkTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserProfileLinkTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserProfileLinkTypesRowCount(config));
            }));
    }

    public GetUserProfileLinkTypesBasicListData(config: UserProfileLinkTypeQueryParameters | any = null) : Observable<Array<UserProfileLinkTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userProfileLinkTypesBasicListData$ = this.requestUserProfileLinkTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserProfileLinkTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userProfileLinkTypesBasicListData$);

            return userProfileLinkTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserProfileLinkTypeBasicListData>>;
    }


    private requestUserProfileLinkTypesBasicListData(config: UserProfileLinkTypeQueryParameters | any) : Observable<Array<UserProfileLinkTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserProfileLinkTypeBasicListData>>(this.baseUrl + 'api/UserProfileLinkTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserProfileLinkTypesBasicListData(config));
            }));

    }


    public PutUserProfileLinkType(id: bigint | number, userProfileLinkType: UserProfileLinkTypeSubmitData) : Observable<UserProfileLinkTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserProfileLinkTypeData>(this.baseUrl + 'api/UserProfileLinkType/' + id.toString(), userProfileLinkType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserProfileLinkType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserProfileLinkType(id, userProfileLinkType));
            }));
    }


    public PostUserProfileLinkType(userProfileLinkType: UserProfileLinkTypeSubmitData) : Observable<UserProfileLinkTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserProfileLinkTypeData>(this.baseUrl + 'api/UserProfileLinkType', userProfileLinkType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserProfileLinkType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserProfileLinkType(userProfileLinkType));
            }));
    }

  
    public DeleteUserProfileLinkType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserProfileLinkType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserProfileLinkType(id));
            }));
    }


    private getConfigHash(config: UserProfileLinkTypeQueryParameters | any): string {

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

    public userIsBMCUserProfileLinkTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCUserProfileLinkTypeReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.UserProfileLinkTypes
        //
        if (userIsBMCUserProfileLinkTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCUserProfileLinkTypeReader = user.readPermission >= 1;
            } else {
                userIsBMCUserProfileLinkTypeReader = false;
            }
        }

        return userIsBMCUserProfileLinkTypeReader;
    }


    public userIsBMCUserProfileLinkTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCUserProfileLinkTypeWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.UserProfileLinkTypes
        //
        if (userIsBMCUserProfileLinkTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCUserProfileLinkTypeWriter = user.writePermission >= 255;
          } else {
            userIsBMCUserProfileLinkTypeWriter = false;
          }      
        }

        return userIsBMCUserProfileLinkTypeWriter;
    }

    public GetUserProfileLinksForUserProfileLinkType(userProfileLinkTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserProfileLinkData[]> {
        return this.userProfileLinkService.GetUserProfileLinkList({
            userProfileLinkTypeId: userProfileLinkTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full UserProfileLinkTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserProfileLinkTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserProfileLinkTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserProfileLinkType(raw: any): UserProfileLinkTypeData {
    if (!raw) return raw;

    //
    // Create a UserProfileLinkTypeData object instance with correct prototype
    //
    const revived = Object.create(UserProfileLinkTypeData.prototype) as UserProfileLinkTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._userProfileLinks = null;
    (revived as any)._userProfileLinksPromise = null;
    (revived as any)._userProfileLinksSubject = new BehaviorSubject<UserProfileLinkData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadUserProfileLinkTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).UserProfileLinks$ = (revived as any)._userProfileLinksSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userProfileLinks === null && (revived as any)._userProfileLinksPromise === null) {
                (revived as any).loadUserProfileLinks();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userProfileLinksCount$ = null;



    return revived;
  }

  private ReviveUserProfileLinkTypeList(rawList: any[]): UserProfileLinkTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserProfileLinkType(raw));
  }

}
