/*

   GENERATED SERVICE FOR THE LEGOTHEME TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the LegoTheme table.

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
import { LegoSetService, LegoSetData } from './lego-set.service';
import { UserProfilePreferredThemeService, UserProfilePreferredThemeData } from './user-profile-preferred-theme.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class LegoThemeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    legoThemeId: bigint | number | null | undefined = null;
    rebrickableThemeId: bigint | number | null | undefined = null;
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
export class LegoThemeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    legoThemeId: bigint | number | null = null;
    rebrickableThemeId: bigint | number | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class LegoThemeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. LegoThemeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `legoTheme.LegoThemeChildren$` — use with `| async` in templates
//        • Promise:    `legoTheme.LegoThemeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="legoTheme.LegoThemeChildren$ | async"`), or
//        • Access the promise getter (`legoTheme.LegoThemeChildren` or `await legoTheme.LegoThemeChildren`)
//    - Simply reading `legoTheme.LegoThemeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await legoTheme.Reload()` to refresh the entire object and clear all lazy caches.
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
export class LegoThemeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    legoThemeId!: bigint | number;
    rebrickableThemeId!: bigint | number;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    legoTheme: LegoThemeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _legoSets: LegoSetData[] | null = null;
    private _legoSetsPromise: Promise<LegoSetData[]> | null  = null;
    private _legoSetsSubject = new BehaviorSubject<LegoSetData[] | null>(null);

                
    private _userProfilePreferredThemes: UserProfilePreferredThemeData[] | null = null;
    private _userProfilePreferredThemesPromise: Promise<UserProfilePreferredThemeData[]> | null  = null;
    private _userProfilePreferredThemesSubject = new BehaviorSubject<UserProfilePreferredThemeData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public LegoSets$ = this._legoSetsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._legoSets === null && this._legoSetsPromise === null) {
            this.loadLegoSets(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public LegoSetsCount$ = LegoSetService.Instance.GetLegoSetsRowCount({legoThemeId: this.id,
      active: true,
      deleted: false
    });



    public UserProfilePreferredThemes$ = this._userProfilePreferredThemesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userProfilePreferredThemes === null && this._userProfilePreferredThemesPromise === null) {
            this.loadUserProfilePreferredThemes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public UserProfilePreferredThemesCount$ = UserProfilePreferredThemeService.Instance.GetUserProfilePreferredThemesRowCount({legoThemeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any LegoThemeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.legoTheme.Reload();
  //
  //  Non Async:
  //
  //     legoTheme[0].Reload().then(x => {
  //        this.legoTheme = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      LegoThemeService.Instance.GetLegoTheme(this.id, includeRelations)
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
     this._legoSets = null;
     this._legoSetsPromise = null;
     this._legoSetsSubject.next(null);

     this._userProfilePreferredThemes = null;
     this._userProfilePreferredThemesPromise = null;
     this._userProfilePreferredThemesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the LegoSets for this LegoTheme.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.legoTheme.LegoSets.then(legoThemes => { ... })
     *   or
     *   await this.legoTheme.legoThemes
     *
    */
    public get LegoSets(): Promise<LegoSetData[]> {
        if (this._legoSets !== null) {
            return Promise.resolve(this._legoSets);
        }

        if (this._legoSetsPromise !== null) {
            return this._legoSetsPromise;
        }

        // Start the load
        this.loadLegoSets();

        return this._legoSetsPromise!;
    }



    private loadLegoSets(): void {

        this._legoSetsPromise = lastValueFrom(
            LegoThemeService.Instance.GetLegoSetsForLegoTheme(this.id)
        )
        .then(LegoSets => {
            this._legoSets = LegoSets ?? [];
            this._legoSetsSubject.next(this._legoSets);
            return this._legoSets;
         })
        .catch(err => {
            this._legoSets = [];
            this._legoSetsSubject.next(this._legoSets);
            throw err;
        })
        .finally(() => {
            this._legoSetsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached LegoSet. Call after mutations to force refresh.
     */
    public ClearLegoSetsCache(): void {
        this._legoSets = null;
        this._legoSetsPromise = null;
        this._legoSetsSubject.next(this._legoSets);      // Emit to observable
    }

    public get HasLegoSets(): Promise<boolean> {
        return this.LegoSets.then(legoSets => legoSets.length > 0);
    }


    /**
     *
     * Gets the UserProfilePreferredThemes for this LegoTheme.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.legoTheme.UserProfilePreferredThemes.then(legoThemes => { ... })
     *   or
     *   await this.legoTheme.legoThemes
     *
    */
    public get UserProfilePreferredThemes(): Promise<UserProfilePreferredThemeData[]> {
        if (this._userProfilePreferredThemes !== null) {
            return Promise.resolve(this._userProfilePreferredThemes);
        }

        if (this._userProfilePreferredThemesPromise !== null) {
            return this._userProfilePreferredThemesPromise;
        }

        // Start the load
        this.loadUserProfilePreferredThemes();

        return this._userProfilePreferredThemesPromise!;
    }



    private loadUserProfilePreferredThemes(): void {

        this._userProfilePreferredThemesPromise = lastValueFrom(
            LegoThemeService.Instance.GetUserProfilePreferredThemesForLegoTheme(this.id)
        )
        .then(UserProfilePreferredThemes => {
            this._userProfilePreferredThemes = UserProfilePreferredThemes ?? [];
            this._userProfilePreferredThemesSubject.next(this._userProfilePreferredThemes);
            return this._userProfilePreferredThemes;
         })
        .catch(err => {
            this._userProfilePreferredThemes = [];
            this._userProfilePreferredThemesSubject.next(this._userProfilePreferredThemes);
            throw err;
        })
        .finally(() => {
            this._userProfilePreferredThemesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserProfilePreferredTheme. Call after mutations to force refresh.
     */
    public ClearUserProfilePreferredThemesCache(): void {
        this._userProfilePreferredThemes = null;
        this._userProfilePreferredThemesPromise = null;
        this._userProfilePreferredThemesSubject.next(this._userProfilePreferredThemes);      // Emit to observable
    }

    public get HasUserProfilePreferredThemes(): Promise<boolean> {
        return this.UserProfilePreferredThemes.then(userProfilePreferredThemes => userProfilePreferredThemes.length > 0);
    }




    /**
     * Updates the state of this LegoThemeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this LegoThemeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): LegoThemeSubmitData {
        return LegoThemeService.Instance.ConvertToLegoThemeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class LegoThemeService extends SecureEndpointBase {

    private static _instance: LegoThemeService;
    private listCache: Map<string, Observable<Array<LegoThemeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<LegoThemeBasicListData>>>;
    private recordCache: Map<string, Observable<LegoThemeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private legoSetService: LegoSetService,
        private userProfilePreferredThemeService: UserProfilePreferredThemeService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<LegoThemeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<LegoThemeBasicListData>>>();
        this.recordCache = new Map<string, Observable<LegoThemeData>>();

        LegoThemeService._instance = this;
    }

    public static get Instance(): LegoThemeService {
      return LegoThemeService._instance;
    }


    public ClearListCaches(config: LegoThemeQueryParameters | null = null) {

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


    public ConvertToLegoThemeSubmitData(data: LegoThemeData): LegoThemeSubmitData {

        let output = new LegoThemeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.legoThemeId = data.legoThemeId;
        output.rebrickableThemeId = data.rebrickableThemeId;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetLegoTheme(id: bigint | number, includeRelations: boolean = true) : Observable<LegoThemeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const legoTheme$ = this.requestLegoTheme(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LegoTheme", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, legoTheme$);

            return legoTheme$;
        }

        return this.recordCache.get(configHash) as Observable<LegoThemeData>;
    }

    private requestLegoTheme(id: bigint | number, includeRelations: boolean = true) : Observable<LegoThemeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<LegoThemeData>(this.baseUrl + 'api/LegoTheme/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveLegoTheme(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestLegoTheme(id, includeRelations));
            }));
    }

    public GetLegoThemeList(config: LegoThemeQueryParameters | any = null) : Observable<Array<LegoThemeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const legoThemeList$ = this.requestLegoThemeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LegoTheme list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, legoThemeList$);

            return legoThemeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<LegoThemeData>>;
    }


    private requestLegoThemeList(config: LegoThemeQueryParameters | any) : Observable <Array<LegoThemeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LegoThemeData>>(this.baseUrl + 'api/LegoThemes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveLegoThemeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestLegoThemeList(config));
            }));
    }

    public GetLegoThemesRowCount(config: LegoThemeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const legoThemesRowCount$ = this.requestLegoThemesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LegoThemes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, legoThemesRowCount$);

            return legoThemesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestLegoThemesRowCount(config: LegoThemeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/LegoThemes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLegoThemesRowCount(config));
            }));
    }

    public GetLegoThemesBasicListData(config: LegoThemeQueryParameters | any = null) : Observable<Array<LegoThemeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const legoThemesBasicListData$ = this.requestLegoThemesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LegoThemes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, legoThemesBasicListData$);

            return legoThemesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<LegoThemeBasicListData>>;
    }


    private requestLegoThemesBasicListData(config: LegoThemeQueryParameters | any) : Observable<Array<LegoThemeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LegoThemeBasicListData>>(this.baseUrl + 'api/LegoThemes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLegoThemesBasicListData(config));
            }));

    }


    public PutLegoTheme(id: bigint | number, legoTheme: LegoThemeSubmitData) : Observable<LegoThemeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<LegoThemeData>(this.baseUrl + 'api/LegoTheme/' + id.toString(), legoTheme, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLegoTheme(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutLegoTheme(id, legoTheme));
            }));
    }


    public PostLegoTheme(legoTheme: LegoThemeSubmitData) : Observable<LegoThemeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<LegoThemeData>(this.baseUrl + 'api/LegoTheme', legoTheme, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLegoTheme(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostLegoTheme(legoTheme));
            }));
    }

  
    public DeleteLegoTheme(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/LegoTheme/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteLegoTheme(id));
            }));
    }


    private getConfigHash(config: LegoThemeQueryParameters | any): string {

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

    public userIsBMCLegoThemeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCLegoThemeReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.LegoThemes
        //
        if (userIsBMCLegoThemeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCLegoThemeReader = user.readPermission >= 1;
            } else {
                userIsBMCLegoThemeReader = false;
            }
        }

        return userIsBMCLegoThemeReader;
    }


    public userIsBMCLegoThemeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCLegoThemeWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.LegoThemes
        //
        if (userIsBMCLegoThemeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCLegoThemeWriter = user.writePermission >= 255;
          } else {
            userIsBMCLegoThemeWriter = false;
          }      
        }

        return userIsBMCLegoThemeWriter;
    }

    public GetLegoSetsForLegoTheme(legoThemeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<LegoSetData[]> {
        return this.legoSetService.GetLegoSetList({
            legoThemeId: legoThemeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserProfilePreferredThemesForLegoTheme(legoThemeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserProfilePreferredThemeData[]> {
        return this.userProfilePreferredThemeService.GetUserProfilePreferredThemeList({
            legoThemeId: legoThemeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full LegoThemeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the LegoThemeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when LegoThemeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveLegoTheme(raw: any): LegoThemeData {
    if (!raw) return raw;

    //
    // Create a LegoThemeData object instance with correct prototype
    //
    const revived = Object.create(LegoThemeData.prototype) as LegoThemeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._legoThemes = null;
    (revived as any)._legoThemesPromise = null;
    (revived as any)._legoThemesSubject = new BehaviorSubject<LegoThemeData[] | null>(null);

    (revived as any)._legoSets = null;
    (revived as any)._legoSetsPromise = null;
    (revived as any)._legoSetsSubject = new BehaviorSubject<LegoSetData[] | null>(null);

    (revived as any)._userProfilePreferredThemes = null;
    (revived as any)._userProfilePreferredThemesPromise = null;
    (revived as any)._userProfilePreferredThemesSubject = new BehaviorSubject<UserProfilePreferredThemeData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadLegoThemeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).LegoThemes$ = (revived as any)._legoThemesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._legoThemes === null && (revived as any)._legoThemesPromise === null) {
                (revived as any).loadLegoThemes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).LegoThemesCount$ = LegoThemeService.Instance.GetLegoThemesRowCount({legoThemeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).LegoSets$ = (revived as any)._legoSetsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._legoSets === null && (revived as any)._legoSetsPromise === null) {
                (revived as any).loadLegoSets();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).LegoSetsCount$ = LegoSetService.Instance.GetLegoSetsRowCount({legoThemeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).UserProfilePreferredThemes$ = (revived as any)._userProfilePreferredThemesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userProfilePreferredThemes === null && (revived as any)._userProfilePreferredThemesPromise === null) {
                (revived as any).loadUserProfilePreferredThemes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).UserProfilePreferredThemesCount$ = UserProfilePreferredThemeService.Instance.GetUserProfilePreferredThemesRowCount({legoThemeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveLegoThemeList(rawList: any[]): LegoThemeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveLegoTheme(raw));
  }

}
