/*

   GENERATED SERVICE FOR THE USERPROFILE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserProfile table.

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
import { UserProfileChangeHistoryService, UserProfileChangeHistoryData } from './user-profile-change-history.service';
import { UserProfileLinkService, UserProfileLinkData } from './user-profile-link.service';
import { UserProfileStatService, UserProfileStatData } from './user-profile-stat.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserProfileQueryParameters {
    displayName: string | null | undefined = null;
    bio: string | null | undefined = null;
    location: string | null | undefined = null;
    avatarFileName: string | null | undefined = null;
    avatarSize: bigint | number | null | undefined = null;
    avatarMimeType: string | null | undefined = null;
    bannerFileName: string | null | undefined = null;
    bannerSize: bigint | number | null | undefined = null;
    bannerMimeType: string | null | undefined = null;
    websiteUrl: string | null | undefined = null;
    isPublic: boolean | null | undefined = null;
    memberSinceDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    versionNumber: bigint | number | null | undefined = null;
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
export class UserProfileSubmitData {
    id!: bigint | number;
    displayName!: string;
    bio: string | null = null;
    location: string | null = null;
    avatarFileName: string | null = null;
    avatarSize: bigint | number | null = null;
    avatarData: string | null = null;
    avatarMimeType: string | null = null;
    bannerFileName: string | null = null;
    bannerSize: bigint | number | null = null;
    bannerData: string | null = null;
    bannerMimeType: string | null = null;
    websiteUrl: string | null = null;
    isPublic!: boolean;
    memberSinceDate: string | null = null;     // ISO 8601 (full datetime)
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}



//
// Version history information returned from version history API endpoints.
// Matches server-side VersionInformation<T> structure.
//
export interface VersionInformationUser {
    id: bigint | number;
    userName: string;
    firstName: string | null;
    middleName: string | null;
    lastName: string | null;
}

export interface VersionInformation<T> {
    timeStamp: string;           // ISO 8601
    user: VersionInformationUser;
    versionNumber: number;
    data: T | null;
}

export class UserProfileBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserProfileChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userProfile.UserProfileChildren$` — use with `| async` in templates
//        • Promise:    `userProfile.UserProfileChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userProfile.UserProfileChildren$ | async"`), or
//        • Access the promise getter (`userProfile.UserProfileChildren` or `await userProfile.UserProfileChildren`)
//    - Simply reading `userProfile.UserProfileChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userProfile.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserProfileData {
    id!: bigint | number;
    displayName!: string;
    bio!: string | null;
    location!: string | null;
    avatarFileName!: string | null;
    avatarSize!: bigint | number;
    avatarData!: string | null;
    avatarMimeType!: string | null;
    bannerFileName!: string | null;
    bannerSize!: bigint | number;
    bannerData!: string | null;
    bannerMimeType!: string | null;
    websiteUrl!: string | null;
    isPublic!: boolean;
    memberSinceDate!: string | null;   // ISO 8601 (full datetime)
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _userProfileChangeHistories: UserProfileChangeHistoryData[] | null = null;
    private _userProfileChangeHistoriesPromise: Promise<UserProfileChangeHistoryData[]> | null  = null;
    private _userProfileChangeHistoriesSubject = new BehaviorSubject<UserProfileChangeHistoryData[] | null>(null);

                
    private _userProfileLinks: UserProfileLinkData[] | null = null;
    private _userProfileLinksPromise: Promise<UserProfileLinkData[]> | null  = null;
    private _userProfileLinksSubject = new BehaviorSubject<UserProfileLinkData[] | null>(null);

                
    private _userProfileStats: UserProfileStatData[] | null = null;
    private _userProfileStatsPromise: Promise<UserProfileStatData[]> | null  = null;
    private _userProfileStatsSubject = new BehaviorSubject<UserProfileStatData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<UserProfileData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<UserProfileData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserProfileData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public UserProfileChangeHistories$ = this._userProfileChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userProfileChangeHistories === null && this._userProfileChangeHistoriesPromise === null) {
            this.loadUserProfileChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public UserProfileChangeHistoriesCount$ = UserProfileChangeHistoryService.Instance.GetUserProfileChangeHistoriesRowCount({userProfileId: this.id,
      active: true,
      deleted: false
    });



    public UserProfileLinks$ = this._userProfileLinksSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userProfileLinks === null && this._userProfileLinksPromise === null) {
            this.loadUserProfileLinks(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public UserProfileLinksCount$ = UserProfileLinkService.Instance.GetUserProfileLinksRowCount({userProfileId: this.id,
      active: true,
      deleted: false
    });



    public UserProfileStats$ = this._userProfileStatsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userProfileStats === null && this._userProfileStatsPromise === null) {
            this.loadUserProfileStats(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public UserProfileStatsCount$ = UserProfileStatService.Instance.GetUserProfileStatsRowCount({userProfileId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any UserProfileData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userProfile.Reload();
  //
  //  Non Async:
  //
  //     userProfile[0].Reload().then(x => {
  //        this.userProfile = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserProfileService.Instance.GetUserProfile(this.id, includeRelations)
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
     this._userProfileChangeHistories = null;
     this._userProfileChangeHistoriesPromise = null;
     this._userProfileChangeHistoriesSubject.next(null);

     this._userProfileLinks = null;
     this._userProfileLinksPromise = null;
     this._userProfileLinksSubject.next(null);

     this._userProfileStats = null;
     this._userProfileStatsPromise = null;
     this._userProfileStatsSubject.next(null);

     this._currentVersionInfo = null;
     this._currentVersionInfoPromise = null;
     this._currentVersionInfoSubject.next(null);
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the UserProfileChangeHistories for this UserProfile.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userProfile.UserProfileChangeHistories.then(userProfiles => { ... })
     *   or
     *   await this.userProfile.userProfiles
     *
    */
    public get UserProfileChangeHistories(): Promise<UserProfileChangeHistoryData[]> {
        if (this._userProfileChangeHistories !== null) {
            return Promise.resolve(this._userProfileChangeHistories);
        }

        if (this._userProfileChangeHistoriesPromise !== null) {
            return this._userProfileChangeHistoriesPromise;
        }

        // Start the load
        this.loadUserProfileChangeHistories();

        return this._userProfileChangeHistoriesPromise!;
    }



    private loadUserProfileChangeHistories(): void {

        this._userProfileChangeHistoriesPromise = lastValueFrom(
            UserProfileService.Instance.GetUserProfileChangeHistoriesForUserProfile(this.id)
        )
        .then(UserProfileChangeHistories => {
            this._userProfileChangeHistories = UserProfileChangeHistories ?? [];
            this._userProfileChangeHistoriesSubject.next(this._userProfileChangeHistories);
            return this._userProfileChangeHistories;
         })
        .catch(err => {
            this._userProfileChangeHistories = [];
            this._userProfileChangeHistoriesSubject.next(this._userProfileChangeHistories);
            throw err;
        })
        .finally(() => {
            this._userProfileChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserProfileChangeHistory. Call after mutations to force refresh.
     */
    public ClearUserProfileChangeHistoriesCache(): void {
        this._userProfileChangeHistories = null;
        this._userProfileChangeHistoriesPromise = null;
        this._userProfileChangeHistoriesSubject.next(this._userProfileChangeHistories);      // Emit to observable
    }

    public get HasUserProfileChangeHistories(): Promise<boolean> {
        return this.UserProfileChangeHistories.then(userProfileChangeHistories => userProfileChangeHistories.length > 0);
    }


    /**
     *
     * Gets the UserProfileLinks for this UserProfile.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userProfile.UserProfileLinks.then(userProfiles => { ... })
     *   or
     *   await this.userProfile.userProfiles
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
            UserProfileService.Instance.GetUserProfileLinksForUserProfile(this.id)
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
     *
     * Gets the UserProfileStats for this UserProfile.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userProfile.UserProfileStats.then(userProfiles => { ... })
     *   or
     *   await this.userProfile.userProfiles
     *
    */
    public get UserProfileStats(): Promise<UserProfileStatData[]> {
        if (this._userProfileStats !== null) {
            return Promise.resolve(this._userProfileStats);
        }

        if (this._userProfileStatsPromise !== null) {
            return this._userProfileStatsPromise;
        }

        // Start the load
        this.loadUserProfileStats();

        return this._userProfileStatsPromise!;
    }



    private loadUserProfileStats(): void {

        this._userProfileStatsPromise = lastValueFrom(
            UserProfileService.Instance.GetUserProfileStatsForUserProfile(this.id)
        )
        .then(UserProfileStats => {
            this._userProfileStats = UserProfileStats ?? [];
            this._userProfileStatsSubject.next(this._userProfileStats);
            return this._userProfileStats;
         })
        .catch(err => {
            this._userProfileStats = [];
            this._userProfileStatsSubject.next(this._userProfileStats);
            throw err;
        })
        .finally(() => {
            this._userProfileStatsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserProfileStat. Call after mutations to force refresh.
     */
    public ClearUserProfileStatsCache(): void {
        this._userProfileStats = null;
        this._userProfileStatsPromise = null;
        this._userProfileStatsSubject.next(this._userProfileStats);      // Emit to observable
    }

    public get HasUserProfileStats(): Promise<boolean> {
        return this.UserProfileStats.then(userProfileStats => userProfileStats.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (userProfile.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await userProfile.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<UserProfileData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<UserProfileData>> {
        const info = await lastValueFrom(
            UserProfileService.Instance.GetUserProfileChangeMetadata(this.id, this.versionNumber as number)
        );
        this._currentVersionInfo = info;
        this._currentVersionInfoSubject.next(info);
        return info;
    }


    public ClearCurrentVersionInfoCache(): void {
        this._currentVersionInfo = null;
        this._currentVersionInfoPromise = null;
        this._currentVersionInfoSubject.next(null);
    }



    /**
     * Updates the state of this UserProfileData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserProfileData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserProfileSubmitData {
        return UserProfileService.Instance.ConvertToUserProfileSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserProfileService extends SecureEndpointBase {

    private static _instance: UserProfileService;
    private listCache: Map<string, Observable<Array<UserProfileData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserProfileBasicListData>>>;
    private recordCache: Map<string, Observable<UserProfileData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private userProfileChangeHistoryService: UserProfileChangeHistoryService,
        private userProfileLinkService: UserProfileLinkService,
        private userProfileStatService: UserProfileStatService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserProfileData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserProfileBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserProfileData>>();

        UserProfileService._instance = this;
    }

    public static get Instance(): UserProfileService {
      return UserProfileService._instance;
    }


    public ClearListCaches(config: UserProfileQueryParameters | null = null) {

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


    public ConvertToUserProfileSubmitData(data: UserProfileData): UserProfileSubmitData {

        let output = new UserProfileSubmitData();

        output.id = data.id;
        output.displayName = data.displayName;
        output.bio = data.bio;
        output.location = data.location;
        output.avatarFileName = data.avatarFileName;
        output.avatarSize = data.avatarSize;
        output.avatarData = data.avatarData;
        output.avatarMimeType = data.avatarMimeType;
        output.bannerFileName = data.bannerFileName;
        output.bannerSize = data.bannerSize;
        output.bannerData = data.bannerData;
        output.bannerMimeType = data.bannerMimeType;
        output.websiteUrl = data.websiteUrl;
        output.isPublic = data.isPublic;
        output.memberSinceDate = data.memberSinceDate;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserProfile(id: bigint | number, includeRelations: boolean = true) : Observable<UserProfileData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userProfile$ = this.requestUserProfile(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserProfile", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userProfile$);

            return userProfile$;
        }

        return this.recordCache.get(configHash) as Observable<UserProfileData>;
    }

    private requestUserProfile(id: bigint | number, includeRelations: boolean = true) : Observable<UserProfileData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserProfileData>(this.baseUrl + 'api/UserProfile/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserProfile(id, includeRelations));
            }));
    }

    public GetUserProfileList(config: UserProfileQueryParameters | any = null) : Observable<Array<UserProfileData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userProfileList$ = this.requestUserProfileList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserProfile list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userProfileList$);

            return userProfileList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserProfileData>>;
    }


    private requestUserProfileList(config: UserProfileQueryParameters | any) : Observable <Array<UserProfileData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserProfileData>>(this.baseUrl + 'api/UserProfiles', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserProfileList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserProfileList(config));
            }));
    }

    public GetUserProfilesRowCount(config: UserProfileQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userProfilesRowCount$ = this.requestUserProfilesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserProfiles row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userProfilesRowCount$);

            return userProfilesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserProfilesRowCount(config: UserProfileQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserProfiles/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserProfilesRowCount(config));
            }));
    }

    public GetUserProfilesBasicListData(config: UserProfileQueryParameters | any = null) : Observable<Array<UserProfileBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userProfilesBasicListData$ = this.requestUserProfilesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserProfiles basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userProfilesBasicListData$);

            return userProfilesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserProfileBasicListData>>;
    }


    private requestUserProfilesBasicListData(config: UserProfileQueryParameters | any) : Observable<Array<UserProfileBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserProfileBasicListData>>(this.baseUrl + 'api/UserProfiles/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserProfilesBasicListData(config));
            }));

    }


    public PutUserProfile(id: bigint | number, userProfile: UserProfileSubmitData) : Observable<UserProfileData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserProfileData>(this.baseUrl + 'api/UserProfile/' + id.toString(), userProfile, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserProfile(id, userProfile));
            }));
    }


    public PostUserProfile(userProfile: UserProfileSubmitData) : Observable<UserProfileData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserProfileData>(this.baseUrl + 'api/UserProfile', userProfile, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserProfile(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserProfile(userProfile));
            }));
    }

  
    public DeleteUserProfile(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserProfile/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserProfile(id));
            }));
    }

    public RollbackUserProfile(id: bigint | number, versionNumber: bigint | number) : Observable<UserProfileData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserProfileData>(this.baseUrl + 'api/UserProfile/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackUserProfile(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a UserProfile.
     */
    public GetUserProfileChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<UserProfileData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserProfileData>>(this.baseUrl + 'api/UserProfile/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserProfileChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a UserProfile.
     */
    public GetUserProfileAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<UserProfileData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserProfileData>[]>(this.baseUrl + 'api/UserProfile/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserProfileAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a UserProfile.
     */
    public GetUserProfileVersion(id: bigint | number, version: number): Observable<UserProfileData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserProfileData>(this.baseUrl + 'api/UserProfile/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserProfileVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a UserProfile at a specific point in time.
     */
    public GetUserProfileStateAtTime(id: bigint | number, time: string): Observable<UserProfileData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserProfileData>(this.baseUrl + 'api/UserProfile/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserProfileStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: UserProfileQueryParameters | any): string {

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

    public userIsBMCUserProfileReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCUserProfileReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.UserProfiles
        //
        if (userIsBMCUserProfileReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCUserProfileReader = user.readPermission >= 1;
            } else {
                userIsBMCUserProfileReader = false;
            }
        }

        return userIsBMCUserProfileReader;
    }


    public userIsBMCUserProfileWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCUserProfileWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.UserProfiles
        //
        if (userIsBMCUserProfileWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCUserProfileWriter = user.writePermission >= 1;
          } else {
            userIsBMCUserProfileWriter = false;
          }      
        }

        return userIsBMCUserProfileWriter;
    }

    public GetUserProfileChangeHistoriesForUserProfile(userProfileId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserProfileChangeHistoryData[]> {
        return this.userProfileChangeHistoryService.GetUserProfileChangeHistoryList({
            userProfileId: userProfileId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserProfileLinksForUserProfile(userProfileId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserProfileLinkData[]> {
        return this.userProfileLinkService.GetUserProfileLinkList({
            userProfileId: userProfileId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserProfileStatsForUserProfile(userProfileId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserProfileStatData[]> {
        return this.userProfileStatService.GetUserProfileStatList({
            userProfileId: userProfileId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full UserProfileData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserProfileData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserProfileTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserProfile(raw: any): UserProfileData {
    if (!raw) return raw;

    //
    // Create a UserProfileData object instance with correct prototype
    //
    const revived = Object.create(UserProfileData.prototype) as UserProfileData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._userProfileChangeHistories = null;
    (revived as any)._userProfileChangeHistoriesPromise = null;
    (revived as any)._userProfileChangeHistoriesSubject = new BehaviorSubject<UserProfileChangeHistoryData[] | null>(null);

    (revived as any)._userProfileLinks = null;
    (revived as any)._userProfileLinksPromise = null;
    (revived as any)._userProfileLinksSubject = new BehaviorSubject<UserProfileLinkData[] | null>(null);

    (revived as any)._userProfileStats = null;
    (revived as any)._userProfileStatsPromise = null;
    (revived as any)._userProfileStatsSubject = new BehaviorSubject<UserProfileStatData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadUserProfileXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).UserProfileChangeHistories$ = (revived as any)._userProfileChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userProfileChangeHistories === null && (revived as any)._userProfileChangeHistoriesPromise === null) {
                (revived as any).loadUserProfileChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).UserProfileChangeHistoriesCount$ = UserProfileChangeHistoryService.Instance.GetUserProfileChangeHistoriesRowCount({userProfileId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).UserProfileLinks$ = (revived as any)._userProfileLinksSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userProfileLinks === null && (revived as any)._userProfileLinksPromise === null) {
                (revived as any).loadUserProfileLinks();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).UserProfileLinksCount$ = UserProfileLinkService.Instance.GetUserProfileLinksRowCount({userProfileId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).UserProfileStats$ = (revived as any)._userProfileStatsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userProfileStats === null && (revived as any)._userProfileStatsPromise === null) {
                (revived as any).loadUserProfileStats();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).UserProfileStatsCount$ = UserProfileStatService.Instance.GetUserProfileStatsRowCount({userProfileId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserProfileData> | null>(null);

    (revived as any).CurrentVersionInfo$ = (revived as any)._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if ((revived as any)._currentVersionInfo === null && (revived as any)._currentVersionInfoPromise === null) {
                (revived as any).loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    return revived;
  }

  private ReviveUserProfileList(rawList: any[]): UserProfileData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserProfile(raw));
  }

}
