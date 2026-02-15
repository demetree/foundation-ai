/*

   GENERATED SERVICE FOR THE USERACHIEVEMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserAchievement table.

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
import { AchievementData } from './achievement.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserAchievementQueryParameters {
    achievementId: bigint | number | null | undefined = null;
    earnedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    isDisplayed: boolean | null | undefined = null;
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
export class UserAchievementSubmitData {
    id!: bigint | number;
    achievementId!: bigint | number;
    earnedDate!: string;      // ISO 8601 (full datetime)
    isDisplayed!: boolean;
    active!: boolean;
    deleted!: boolean;
}


export class UserAchievementBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserAchievementChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userAchievement.UserAchievementChildren$` — use with `| async` in templates
//        • Promise:    `userAchievement.UserAchievementChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userAchievement.UserAchievementChildren$ | async"`), or
//        • Access the promise getter (`userAchievement.UserAchievementChildren` or `await userAchievement.UserAchievementChildren`)
//    - Simply reading `userAchievement.UserAchievementChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userAchievement.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserAchievementData {
    id!: bigint | number;
    achievementId!: bigint | number;
    earnedDate!: string;      // ISO 8601 (full datetime)
    isDisplayed!: boolean;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    achievement: AchievementData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //

  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any UserAchievementData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userAchievement.Reload();
  //
  //  Non Async:
  //
  //     userAchievement[0].Reload().then(x => {
  //        this.userAchievement = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserAchievementService.Instance.GetUserAchievement(this.id, includeRelations)
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
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //


    /**
     * Updates the state of this UserAchievementData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserAchievementData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserAchievementSubmitData {
        return UserAchievementService.Instance.ConvertToUserAchievementSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserAchievementService extends SecureEndpointBase {

    private static _instance: UserAchievementService;
    private listCache: Map<string, Observable<Array<UserAchievementData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserAchievementBasicListData>>>;
    private recordCache: Map<string, Observable<UserAchievementData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserAchievementData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserAchievementBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserAchievementData>>();

        UserAchievementService._instance = this;
    }

    public static get Instance(): UserAchievementService {
      return UserAchievementService._instance;
    }


    public ClearListCaches(config: UserAchievementQueryParameters | null = null) {

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


    public ConvertToUserAchievementSubmitData(data: UserAchievementData): UserAchievementSubmitData {

        let output = new UserAchievementSubmitData();

        output.id = data.id;
        output.achievementId = data.achievementId;
        output.earnedDate = data.earnedDate;
        output.isDisplayed = data.isDisplayed;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserAchievement(id: bigint | number, includeRelations: boolean = true) : Observable<UserAchievementData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userAchievement$ = this.requestUserAchievement(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserAchievement", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userAchievement$);

            return userAchievement$;
        }

        return this.recordCache.get(configHash) as Observable<UserAchievementData>;
    }

    private requestUserAchievement(id: bigint | number, includeRelations: boolean = true) : Observable<UserAchievementData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserAchievementData>(this.baseUrl + 'api/UserAchievement/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserAchievement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserAchievement(id, includeRelations));
            }));
    }

    public GetUserAchievementList(config: UserAchievementQueryParameters | any = null) : Observable<Array<UserAchievementData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userAchievementList$ = this.requestUserAchievementList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserAchievement list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userAchievementList$);

            return userAchievementList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserAchievementData>>;
    }


    private requestUserAchievementList(config: UserAchievementQueryParameters | any) : Observable <Array<UserAchievementData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserAchievementData>>(this.baseUrl + 'api/UserAchievements', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserAchievementList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserAchievementList(config));
            }));
    }

    public GetUserAchievementsRowCount(config: UserAchievementQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userAchievementsRowCount$ = this.requestUserAchievementsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserAchievements row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userAchievementsRowCount$);

            return userAchievementsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserAchievementsRowCount(config: UserAchievementQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserAchievements/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserAchievementsRowCount(config));
            }));
    }

    public GetUserAchievementsBasicListData(config: UserAchievementQueryParameters | any = null) : Observable<Array<UserAchievementBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userAchievementsBasicListData$ = this.requestUserAchievementsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserAchievements basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userAchievementsBasicListData$);

            return userAchievementsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserAchievementBasicListData>>;
    }


    private requestUserAchievementsBasicListData(config: UserAchievementQueryParameters | any) : Observable<Array<UserAchievementBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserAchievementBasicListData>>(this.baseUrl + 'api/UserAchievements/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserAchievementsBasicListData(config));
            }));

    }


    public PutUserAchievement(id: bigint | number, userAchievement: UserAchievementSubmitData) : Observable<UserAchievementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserAchievementData>(this.baseUrl + 'api/UserAchievement/' + id.toString(), userAchievement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserAchievement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserAchievement(id, userAchievement));
            }));
    }


    public PostUserAchievement(userAchievement: UserAchievementSubmitData) : Observable<UserAchievementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserAchievementData>(this.baseUrl + 'api/UserAchievement', userAchievement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserAchievement(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserAchievement(userAchievement));
            }));
    }

  
    public DeleteUserAchievement(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserAchievement/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserAchievement(id));
            }));
    }


    private getConfigHash(config: UserAchievementQueryParameters | any): string {

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

    public userIsBMCUserAchievementReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCUserAchievementReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.UserAchievements
        //
        if (userIsBMCUserAchievementReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCUserAchievementReader = user.readPermission >= 1;
            } else {
                userIsBMCUserAchievementReader = false;
            }
        }

        return userIsBMCUserAchievementReader;
    }


    public userIsBMCUserAchievementWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCUserAchievementWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.UserAchievements
        //
        if (userIsBMCUserAchievementWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCUserAchievementWriter = user.writePermission >= 1;
          } else {
            userIsBMCUserAchievementWriter = false;
          }      
        }

        return userIsBMCUserAchievementWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full UserAchievementData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserAchievementData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserAchievementTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserAchievement(raw: any): UserAchievementData {
    if (!raw) return raw;

    //
    // Create a UserAchievementData object instance with correct prototype
    //
    const revived = Object.create(UserAchievementData.prototype) as UserAchievementData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //

    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadUserAchievementXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveUserAchievementList(rawList: any[]): UserAchievementData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserAchievement(raw));
  }

}
