/*

   GENERATED SERVICE FOR THE USERPROFILESTAT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserProfileStat table.

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
import { UserProfileData } from './user-profile.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserProfileStatQueryParameters {
    userProfileId: bigint | number | null | undefined = null;
    totalPartsOwned: bigint | number | null | undefined = null;
    totalUniquePartsOwned: bigint | number | null | undefined = null;
    totalSetsOwned: bigint | number | null | undefined = null;
    totalMocsPublished: bigint | number | null | undefined = null;
    totalFollowers: bigint | number | null | undefined = null;
    totalFollowing: bigint | number | null | undefined = null;
    totalLikesReceived: bigint | number | null | undefined = null;
    totalAchievementPoints: bigint | number | null | undefined = null;
    lastCalculatedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class UserProfileStatSubmitData {
    id!: bigint | number;
    userProfileId!: bigint | number;
    totalPartsOwned!: bigint | number;
    totalUniquePartsOwned!: bigint | number;
    totalSetsOwned!: bigint | number;
    totalMocsPublished!: bigint | number;
    totalFollowers!: bigint | number;
    totalFollowing!: bigint | number;
    totalLikesReceived!: bigint | number;
    totalAchievementPoints!: bigint | number;
    lastCalculatedDate: string | null = null;     // ISO 8601 (full datetime)
    active!: boolean;
    deleted!: boolean;
}


export class UserProfileStatBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserProfileStatChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userProfileStat.UserProfileStatChildren$` — use with `| async` in templates
//        • Promise:    `userProfileStat.UserProfileStatChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userProfileStat.UserProfileStatChildren$ | async"`), or
//        • Access the promise getter (`userProfileStat.UserProfileStatChildren` or `await userProfileStat.UserProfileStatChildren`)
//    - Simply reading `userProfileStat.UserProfileStatChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userProfileStat.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserProfileStatData {
    id!: bigint | number;
    userProfileId!: bigint | number;
    totalPartsOwned!: bigint | number;
    totalUniquePartsOwned!: bigint | number;
    totalSetsOwned!: bigint | number;
    totalMocsPublished!: bigint | number;
    totalFollowers!: bigint | number;
    totalFollowing!: bigint | number;
    totalLikesReceived!: bigint | number;
    totalAchievementPoints!: bigint | number;
    lastCalculatedDate!: string | null;   // ISO 8601 (full datetime)
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    userProfile: UserProfileData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any UserProfileStatData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userProfileStat.Reload();
  //
  //  Non Async:
  //
  //     userProfileStat[0].Reload().then(x => {
  //        this.userProfileStat = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserProfileStatService.Instance.GetUserProfileStat(this.id, includeRelations)
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
     * Updates the state of this UserProfileStatData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserProfileStatData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserProfileStatSubmitData {
        return UserProfileStatService.Instance.ConvertToUserProfileStatSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserProfileStatService extends SecureEndpointBase {

    private static _instance: UserProfileStatService;
    private listCache: Map<string, Observable<Array<UserProfileStatData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserProfileStatBasicListData>>>;
    private recordCache: Map<string, Observable<UserProfileStatData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserProfileStatData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserProfileStatBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserProfileStatData>>();

        UserProfileStatService._instance = this;
    }

    public static get Instance(): UserProfileStatService {
      return UserProfileStatService._instance;
    }


    public ClearListCaches(config: UserProfileStatQueryParameters | null = null) {

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


    public ConvertToUserProfileStatSubmitData(data: UserProfileStatData): UserProfileStatSubmitData {

        let output = new UserProfileStatSubmitData();

        output.id = data.id;
        output.userProfileId = data.userProfileId;
        output.totalPartsOwned = data.totalPartsOwned;
        output.totalUniquePartsOwned = data.totalUniquePartsOwned;
        output.totalSetsOwned = data.totalSetsOwned;
        output.totalMocsPublished = data.totalMocsPublished;
        output.totalFollowers = data.totalFollowers;
        output.totalFollowing = data.totalFollowing;
        output.totalLikesReceived = data.totalLikesReceived;
        output.totalAchievementPoints = data.totalAchievementPoints;
        output.lastCalculatedDate = data.lastCalculatedDate;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserProfileStat(id: bigint | number, includeRelations: boolean = true) : Observable<UserProfileStatData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userProfileStat$ = this.requestUserProfileStat(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserProfileStat", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userProfileStat$);

            return userProfileStat$;
        }

        return this.recordCache.get(configHash) as Observable<UserProfileStatData>;
    }

    private requestUserProfileStat(id: bigint | number, includeRelations: boolean = true) : Observable<UserProfileStatData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserProfileStatData>(this.baseUrl + 'api/UserProfileStat/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserProfileStat(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserProfileStat(id, includeRelations));
            }));
    }

    public GetUserProfileStatList(config: UserProfileStatQueryParameters | any = null) : Observable<Array<UserProfileStatData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userProfileStatList$ = this.requestUserProfileStatList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserProfileStat list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userProfileStatList$);

            return userProfileStatList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserProfileStatData>>;
    }


    private requestUserProfileStatList(config: UserProfileStatQueryParameters | any) : Observable <Array<UserProfileStatData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserProfileStatData>>(this.baseUrl + 'api/UserProfileStats', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserProfileStatList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserProfileStatList(config));
            }));
    }

    public GetUserProfileStatsRowCount(config: UserProfileStatQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userProfileStatsRowCount$ = this.requestUserProfileStatsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserProfileStats row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userProfileStatsRowCount$);

            return userProfileStatsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserProfileStatsRowCount(config: UserProfileStatQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserProfileStats/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserProfileStatsRowCount(config));
            }));
    }

    public GetUserProfileStatsBasicListData(config: UserProfileStatQueryParameters | any = null) : Observable<Array<UserProfileStatBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userProfileStatsBasicListData$ = this.requestUserProfileStatsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserProfileStats basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userProfileStatsBasicListData$);

            return userProfileStatsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserProfileStatBasicListData>>;
    }


    private requestUserProfileStatsBasicListData(config: UserProfileStatQueryParameters | any) : Observable<Array<UserProfileStatBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserProfileStatBasicListData>>(this.baseUrl + 'api/UserProfileStats/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserProfileStatsBasicListData(config));
            }));

    }


    public PutUserProfileStat(id: bigint | number, userProfileStat: UserProfileStatSubmitData) : Observable<UserProfileStatData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserProfileStatData>(this.baseUrl + 'api/UserProfileStat/' + id.toString(), userProfileStat, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserProfileStat(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserProfileStat(id, userProfileStat));
            }));
    }


    public PostUserProfileStat(userProfileStat: UserProfileStatSubmitData) : Observable<UserProfileStatData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserProfileStatData>(this.baseUrl + 'api/UserProfileStat', userProfileStat, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserProfileStat(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserProfileStat(userProfileStat));
            }));
    }

  
    public DeleteUserProfileStat(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserProfileStat/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserProfileStat(id));
            }));
    }


    private getConfigHash(config: UserProfileStatQueryParameters | any): string {

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

    public userIsBMCUserProfileStatReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCUserProfileStatReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.UserProfileStats
        //
        if (userIsBMCUserProfileStatReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCUserProfileStatReader = user.readPermission >= 1;
            } else {
                userIsBMCUserProfileStatReader = false;
            }
        }

        return userIsBMCUserProfileStatReader;
    }


    public userIsBMCUserProfileStatWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCUserProfileStatWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.UserProfileStats
        //
        if (userIsBMCUserProfileStatWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCUserProfileStatWriter = user.writePermission >= 255;
          } else {
            userIsBMCUserProfileStatWriter = false;
          }      
        }

        return userIsBMCUserProfileStatWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full UserProfileStatData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserProfileStatData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserProfileStatTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserProfileStat(raw: any): UserProfileStatData {
    if (!raw) return raw;

    //
    // Create a UserProfileStatData object instance with correct prototype
    //
    const revived = Object.create(UserProfileStatData.prototype) as UserProfileStatData;

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
    // 2. But private methods (loadUserProfileStatXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveUserProfileStatList(rawList: any[]): UserProfileStatData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserProfileStat(raw));
  }

}
