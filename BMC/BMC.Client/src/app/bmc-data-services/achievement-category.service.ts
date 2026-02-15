/*

   GENERATED SERVICE FOR THE ACHIEVEMENTCATEGORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AchievementCategory table.

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
import { AchievementService, AchievementData } from './achievement.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AchievementCategoryQueryParameters {
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
export class AchievementCategorySubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    iconCssClass: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class AchievementCategoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AchievementCategoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `achievementCategory.AchievementCategoryChildren$` — use with `| async` in templates
//        • Promise:    `achievementCategory.AchievementCategoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="achievementCategory.AchievementCategoryChildren$ | async"`), or
//        • Access the promise getter (`achievementCategory.AchievementCategoryChildren` or `await achievementCategory.AchievementCategoryChildren`)
//    - Simply reading `achievementCategory.AchievementCategoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await achievementCategory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AchievementCategoryData {
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
    private _achievements: AchievementData[] | null = null;
    private _achievementsPromise: Promise<AchievementData[]> | null  = null;
    private _achievementsSubject = new BehaviorSubject<AchievementData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Achievements$ = this._achievementsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._achievements === null && this._achievementsPromise === null) {
            this.loadAchievements(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public AchievementsCount$ = AchievementService.Instance.GetAchievementsRowCount({achievementCategoryId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AchievementCategoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.achievementCategory.Reload();
  //
  //  Non Async:
  //
  //     achievementCategory[0].Reload().then(x => {
  //        this.achievementCategory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AchievementCategoryService.Instance.GetAchievementCategory(this.id, includeRelations)
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
     this._achievements = null;
     this._achievementsPromise = null;
     this._achievementsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Achievements for this AchievementCategory.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.achievementCategory.Achievements.then(achievementCategories => { ... })
     *   or
     *   await this.achievementCategory.achievementCategories
     *
    */
    public get Achievements(): Promise<AchievementData[]> {
        if (this._achievements !== null) {
            return Promise.resolve(this._achievements);
        }

        if (this._achievementsPromise !== null) {
            return this._achievementsPromise;
        }

        // Start the load
        this.loadAchievements();

        return this._achievementsPromise!;
    }



    private loadAchievements(): void {

        this._achievementsPromise = lastValueFrom(
            AchievementCategoryService.Instance.GetAchievementsForAchievementCategory(this.id)
        )
        .then(Achievements => {
            this._achievements = Achievements ?? [];
            this._achievementsSubject.next(this._achievements);
            return this._achievements;
         })
        .catch(err => {
            this._achievements = [];
            this._achievementsSubject.next(this._achievements);
            throw err;
        })
        .finally(() => {
            this._achievementsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Achievement. Call after mutations to force refresh.
     */
    public ClearAchievementsCache(): void {
        this._achievements = null;
        this._achievementsPromise = null;
        this._achievementsSubject.next(this._achievements);      // Emit to observable
    }

    public get HasAchievements(): Promise<boolean> {
        return this.Achievements.then(achievements => achievements.length > 0);
    }




    /**
     * Updates the state of this AchievementCategoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AchievementCategoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AchievementCategorySubmitData {
        return AchievementCategoryService.Instance.ConvertToAchievementCategorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AchievementCategoryService extends SecureEndpointBase {

    private static _instance: AchievementCategoryService;
    private listCache: Map<string, Observable<Array<AchievementCategoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AchievementCategoryBasicListData>>>;
    private recordCache: Map<string, Observable<AchievementCategoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private achievementService: AchievementService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AchievementCategoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AchievementCategoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<AchievementCategoryData>>();

        AchievementCategoryService._instance = this;
    }

    public static get Instance(): AchievementCategoryService {
      return AchievementCategoryService._instance;
    }


    public ClearListCaches(config: AchievementCategoryQueryParameters | null = null) {

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


    public ConvertToAchievementCategorySubmitData(data: AchievementCategoryData): AchievementCategorySubmitData {

        let output = new AchievementCategorySubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.iconCssClass = data.iconCssClass;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetAchievementCategory(id: bigint | number, includeRelations: boolean = true) : Observable<AchievementCategoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const achievementCategory$ = this.requestAchievementCategory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AchievementCategory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, achievementCategory$);

            return achievementCategory$;
        }

        return this.recordCache.get(configHash) as Observable<AchievementCategoryData>;
    }

    private requestAchievementCategory(id: bigint | number, includeRelations: boolean = true) : Observable<AchievementCategoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AchievementCategoryData>(this.baseUrl + 'api/AchievementCategory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAchievementCategory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAchievementCategory(id, includeRelations));
            }));
    }

    public GetAchievementCategoryList(config: AchievementCategoryQueryParameters | any = null) : Observable<Array<AchievementCategoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const achievementCategoryList$ = this.requestAchievementCategoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AchievementCategory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, achievementCategoryList$);

            return achievementCategoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AchievementCategoryData>>;
    }


    private requestAchievementCategoryList(config: AchievementCategoryQueryParameters | any) : Observable <Array<AchievementCategoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AchievementCategoryData>>(this.baseUrl + 'api/AchievementCategories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAchievementCategoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAchievementCategoryList(config));
            }));
    }

    public GetAchievementCategoriesRowCount(config: AchievementCategoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const achievementCategoriesRowCount$ = this.requestAchievementCategoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AchievementCategories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, achievementCategoriesRowCount$);

            return achievementCategoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAchievementCategoriesRowCount(config: AchievementCategoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AchievementCategories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAchievementCategoriesRowCount(config));
            }));
    }

    public GetAchievementCategoriesBasicListData(config: AchievementCategoryQueryParameters | any = null) : Observable<Array<AchievementCategoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const achievementCategoriesBasicListData$ = this.requestAchievementCategoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AchievementCategories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, achievementCategoriesBasicListData$);

            return achievementCategoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AchievementCategoryBasicListData>>;
    }


    private requestAchievementCategoriesBasicListData(config: AchievementCategoryQueryParameters | any) : Observable<Array<AchievementCategoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AchievementCategoryBasicListData>>(this.baseUrl + 'api/AchievementCategories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAchievementCategoriesBasicListData(config));
            }));

    }


    public PutAchievementCategory(id: bigint | number, achievementCategory: AchievementCategorySubmitData) : Observable<AchievementCategoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AchievementCategoryData>(this.baseUrl + 'api/AchievementCategory/' + id.toString(), achievementCategory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAchievementCategory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAchievementCategory(id, achievementCategory));
            }));
    }


    public PostAchievementCategory(achievementCategory: AchievementCategorySubmitData) : Observable<AchievementCategoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AchievementCategoryData>(this.baseUrl + 'api/AchievementCategory', achievementCategory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAchievementCategory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAchievementCategory(achievementCategory));
            }));
    }

  
    public DeleteAchievementCategory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AchievementCategory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAchievementCategory(id));
            }));
    }


    private getConfigHash(config: AchievementCategoryQueryParameters | any): string {

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

    public userIsBMCAchievementCategoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCAchievementCategoryReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.AchievementCategories
        //
        if (userIsBMCAchievementCategoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCAchievementCategoryReader = user.readPermission >= 1;
            } else {
                userIsBMCAchievementCategoryReader = false;
            }
        }

        return userIsBMCAchievementCategoryReader;
    }


    public userIsBMCAchievementCategoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCAchievementCategoryWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.AchievementCategories
        //
        if (userIsBMCAchievementCategoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCAchievementCategoryWriter = user.writePermission >= 255;
          } else {
            userIsBMCAchievementCategoryWriter = false;
          }      
        }

        return userIsBMCAchievementCategoryWriter;
    }

    public GetAchievementsForAchievementCategory(achievementCategoryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AchievementData[]> {
        return this.achievementService.GetAchievementList({
            achievementCategoryId: achievementCategoryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AchievementCategoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AchievementCategoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AchievementCategoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAchievementCategory(raw: any): AchievementCategoryData {
    if (!raw) return raw;

    //
    // Create a AchievementCategoryData object instance with correct prototype
    //
    const revived = Object.create(AchievementCategoryData.prototype) as AchievementCategoryData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._achievements = null;
    (revived as any)._achievementsPromise = null;
    (revived as any)._achievementsSubject = new BehaviorSubject<AchievementData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAchievementCategoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Achievements$ = (revived as any)._achievementsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._achievements === null && (revived as any)._achievementsPromise === null) {
                (revived as any).loadAchievements();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).AchievementsCount$ = AchievementService.Instance.GetAchievementsRowCount({achievementCategoryId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveAchievementCategoryList(rawList: any[]): AchievementCategoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAchievementCategory(raw));
  }

}
