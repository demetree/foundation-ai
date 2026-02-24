/*

   GENERATED SERVICE FOR THE ACHIEVEMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Achievement table.

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
import { AchievementCategoryData } from './achievement-category.service';
import { UserAchievementService, UserAchievementData } from './user-achievement.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AchievementQueryParameters {
    achievementCategoryId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    iconCssClass: string | null | undefined = null;
    iconImagePath: string | null | undefined = null;
    criteria: string | null | undefined = null;
    criteriaCode: string | null | undefined = null;
    pointValue: bigint | number | null | undefined = null;
    rarity: string | null | undefined = null;
    isActive: boolean | null | undefined = null;
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
export class AchievementSubmitData {
    id!: bigint | number;
    achievementCategoryId!: bigint | number;
    name!: string;
    description!: string;
    iconCssClass: string | null = null;
    iconImagePath: string | null = null;
    criteria: string | null = null;
    criteriaCode: string | null = null;
    pointValue!: bigint | number;
    rarity!: string;
    isActive!: boolean;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class AchievementBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AchievementChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `achievement.AchievementChildren$` — use with `| async` in templates
//        • Promise:    `achievement.AchievementChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="achievement.AchievementChildren$ | async"`), or
//        • Access the promise getter (`achievement.AchievementChildren` or `await achievement.AchievementChildren`)
//    - Simply reading `achievement.AchievementChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await achievement.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AchievementData {
    id!: bigint | number;
    achievementCategoryId!: bigint | number;
    name!: string;
    description!: string;
    iconCssClass!: string | null;
    iconImagePath!: string | null;
    criteria!: string | null;
    criteriaCode!: string | null;
    pointValue!: bigint | number;
    rarity!: string;
    isActive!: boolean;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    achievementCategory: AchievementCategoryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _userAchievements: UserAchievementData[] | null = null;
    private _userAchievementsPromise: Promise<UserAchievementData[]> | null  = null;
    private _userAchievementsSubject = new BehaviorSubject<UserAchievementData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public UserAchievements$ = this._userAchievementsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userAchievements === null && this._userAchievementsPromise === null) {
            this.loadUserAchievements(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userAchievementsCount$: Observable<bigint | number> | null = null;
    public get UserAchievementsCount$(): Observable<bigint | number> {
        if (this._userAchievementsCount$ === null) {
            this._userAchievementsCount$ = UserAchievementService.Instance.GetUserAchievementsRowCount({achievementId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userAchievementsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AchievementData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.achievement.Reload();
  //
  //  Non Async:
  //
  //     achievement[0].Reload().then(x => {
  //        this.achievement = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AchievementService.Instance.GetAchievement(this.id, includeRelations)
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
     this._userAchievements = null;
     this._userAchievementsPromise = null;
     this._userAchievementsSubject.next(null);
     this._userAchievementsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the UserAchievements for this Achievement.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.achievement.UserAchievements.then(achievements => { ... })
     *   or
     *   await this.achievement.achievements
     *
    */
    public get UserAchievements(): Promise<UserAchievementData[]> {
        if (this._userAchievements !== null) {
            return Promise.resolve(this._userAchievements);
        }

        if (this._userAchievementsPromise !== null) {
            return this._userAchievementsPromise;
        }

        // Start the load
        this.loadUserAchievements();

        return this._userAchievementsPromise!;
    }



    private loadUserAchievements(): void {

        this._userAchievementsPromise = lastValueFrom(
            AchievementService.Instance.GetUserAchievementsForAchievement(this.id)
        )
        .then(UserAchievements => {
            this._userAchievements = UserAchievements ?? [];
            this._userAchievementsSubject.next(this._userAchievements);
            return this._userAchievements;
         })
        .catch(err => {
            this._userAchievements = [];
            this._userAchievementsSubject.next(this._userAchievements);
            throw err;
        })
        .finally(() => {
            this._userAchievementsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserAchievement. Call after mutations to force refresh.
     */
    public ClearUserAchievementsCache(): void {
        this._userAchievements = null;
        this._userAchievementsPromise = null;
        this._userAchievementsSubject.next(this._userAchievements);      // Emit to observable
    }

    public get HasUserAchievements(): Promise<boolean> {
        return this.UserAchievements.then(userAchievements => userAchievements.length > 0);
    }




    /**
     * Updates the state of this AchievementData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AchievementData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AchievementSubmitData {
        return AchievementService.Instance.ConvertToAchievementSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AchievementService extends SecureEndpointBase {

    private static _instance: AchievementService;
    private listCache: Map<string, Observable<Array<AchievementData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AchievementBasicListData>>>;
    private recordCache: Map<string, Observable<AchievementData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private userAchievementService: UserAchievementService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AchievementData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AchievementBasicListData>>>();
        this.recordCache = new Map<string, Observable<AchievementData>>();

        AchievementService._instance = this;
    }

    public static get Instance(): AchievementService {
      return AchievementService._instance;
    }


    public ClearListCaches(config: AchievementQueryParameters | null = null) {

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


    public ConvertToAchievementSubmitData(data: AchievementData): AchievementSubmitData {

        let output = new AchievementSubmitData();

        output.id = data.id;
        output.achievementCategoryId = data.achievementCategoryId;
        output.name = data.name;
        output.description = data.description;
        output.iconCssClass = data.iconCssClass;
        output.iconImagePath = data.iconImagePath;
        output.criteria = data.criteria;
        output.criteriaCode = data.criteriaCode;
        output.pointValue = data.pointValue;
        output.rarity = data.rarity;
        output.isActive = data.isActive;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetAchievement(id: bigint | number, includeRelations: boolean = true) : Observable<AchievementData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const achievement$ = this.requestAchievement(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Achievement", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, achievement$);

            return achievement$;
        }

        return this.recordCache.get(configHash) as Observable<AchievementData>;
    }

    private requestAchievement(id: bigint | number, includeRelations: boolean = true) : Observable<AchievementData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AchievementData>(this.baseUrl + 'api/Achievement/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAchievement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAchievement(id, includeRelations));
            }));
    }

    public GetAchievementList(config: AchievementQueryParameters | any = null) : Observable<Array<AchievementData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const achievementList$ = this.requestAchievementList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Achievement list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, achievementList$);

            return achievementList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AchievementData>>;
    }


    private requestAchievementList(config: AchievementQueryParameters | any) : Observable <Array<AchievementData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AchievementData>>(this.baseUrl + 'api/Achievements', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAchievementList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAchievementList(config));
            }));
    }

    public GetAchievementsRowCount(config: AchievementQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const achievementsRowCount$ = this.requestAchievementsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Achievements row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, achievementsRowCount$);

            return achievementsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAchievementsRowCount(config: AchievementQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Achievements/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAchievementsRowCount(config));
            }));
    }

    public GetAchievementsBasicListData(config: AchievementQueryParameters | any = null) : Observable<Array<AchievementBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const achievementsBasicListData$ = this.requestAchievementsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Achievements basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, achievementsBasicListData$);

            return achievementsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AchievementBasicListData>>;
    }


    private requestAchievementsBasicListData(config: AchievementQueryParameters | any) : Observable<Array<AchievementBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AchievementBasicListData>>(this.baseUrl + 'api/Achievements/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAchievementsBasicListData(config));
            }));

    }


    public PutAchievement(id: bigint | number, achievement: AchievementSubmitData) : Observable<AchievementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AchievementData>(this.baseUrl + 'api/Achievement/' + id.toString(), achievement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAchievement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAchievement(id, achievement));
            }));
    }


    public PostAchievement(achievement: AchievementSubmitData) : Observable<AchievementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AchievementData>(this.baseUrl + 'api/Achievement', achievement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAchievement(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAchievement(achievement));
            }));
    }

  
    public DeleteAchievement(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Achievement/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAchievement(id));
            }));
    }


    private getConfigHash(config: AchievementQueryParameters | any): string {

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

    public userIsBMCAchievementReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCAchievementReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.Achievements
        //
        if (userIsBMCAchievementReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCAchievementReader = user.readPermission >= 1;
            } else {
                userIsBMCAchievementReader = false;
            }
        }

        return userIsBMCAchievementReader;
    }


    public userIsBMCAchievementWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCAchievementWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.Achievements
        //
        if (userIsBMCAchievementWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCAchievementWriter = user.writePermission >= 255;
          } else {
            userIsBMCAchievementWriter = false;
          }      
        }

        return userIsBMCAchievementWriter;
    }

    public GetUserAchievementsForAchievement(achievementId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserAchievementData[]> {
        return this.userAchievementService.GetUserAchievementList({
            achievementId: achievementId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AchievementData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AchievementData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AchievementTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAchievement(raw: any): AchievementData {
    if (!raw) return raw;

    //
    // Create a AchievementData object instance with correct prototype
    //
    const revived = Object.create(AchievementData.prototype) as AchievementData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._userAchievements = null;
    (revived as any)._userAchievementsPromise = null;
    (revived as any)._userAchievementsSubject = new BehaviorSubject<UserAchievementData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAchievementXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).UserAchievements$ = (revived as any)._userAchievementsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userAchievements === null && (revived as any)._userAchievementsPromise === null) {
                (revived as any).loadUserAchievements();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userAchievementsCount$ = null;



    return revived;
  }

  private ReviveAchievementList(rawList: any[]): AchievementData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAchievement(raw));
  }

}
