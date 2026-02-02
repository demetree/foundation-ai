/*

   GENERATED SERVICE FOR THE USERNOTIFICATIONCHANNELPREFERENCE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserNotificationChannelPreference table.

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
import { UserNotificationPreferenceData } from './user-notification-preference.service';
import { NotificationChannelTypeData } from './notification-channel-type.service';
import { UserNotificationChannelPreferenceChangeHistoryService, UserNotificationChannelPreferenceChangeHistoryData } from './user-notification-channel-preference-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserNotificationChannelPreferenceQueryParameters {
    userNotificationPreferenceId: bigint | number | null | undefined = null;
    notificationChannelTypeId: bigint | number | null | undefined = null;
    isEnabled: boolean | null | undefined = null;
    priorityOverride: bigint | number | null | undefined = null;
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
export class UserNotificationChannelPreferenceSubmitData {
    id!: bigint | number;
    userNotificationPreferenceId!: bigint | number;
    notificationChannelTypeId!: bigint | number;
    isEnabled!: boolean;
    priorityOverride: bigint | number | null = null;
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

export class UserNotificationChannelPreferenceBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserNotificationChannelPreferenceChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userNotificationChannelPreference.UserNotificationChannelPreferenceChildren$` — use with `| async` in templates
//        • Promise:    `userNotificationChannelPreference.UserNotificationChannelPreferenceChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userNotificationChannelPreference.UserNotificationChannelPreferenceChildren$ | async"`), or
//        • Access the promise getter (`userNotificationChannelPreference.UserNotificationChannelPreferenceChildren` or `await userNotificationChannelPreference.UserNotificationChannelPreferenceChildren`)
//    - Simply reading `userNotificationChannelPreference.UserNotificationChannelPreferenceChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userNotificationChannelPreference.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserNotificationChannelPreferenceData {
    id!: bigint | number;
    userNotificationPreferenceId!: bigint | number;
    notificationChannelTypeId!: bigint | number;
    isEnabled!: boolean;
    priorityOverride!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    notificationChannelType: NotificationChannelTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    userNotificationPreference: UserNotificationPreferenceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _userNotificationChannelPreferenceChangeHistories: UserNotificationChannelPreferenceChangeHistoryData[] | null = null;
    private _userNotificationChannelPreferenceChangeHistoriesPromise: Promise<UserNotificationChannelPreferenceChangeHistoryData[]> | null  = null;
    private _userNotificationChannelPreferenceChangeHistoriesSubject = new BehaviorSubject<UserNotificationChannelPreferenceChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<UserNotificationChannelPreferenceData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<UserNotificationChannelPreferenceData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserNotificationChannelPreferenceData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public UserNotificationChannelPreferenceChangeHistories$ = this._userNotificationChannelPreferenceChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userNotificationChannelPreferenceChangeHistories === null && this._userNotificationChannelPreferenceChangeHistoriesPromise === null) {
            this.loadUserNotificationChannelPreferenceChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public UserNotificationChannelPreferenceChangeHistoriesCount$ = UserNotificationChannelPreferenceChangeHistoryService.Instance.GetUserNotificationChannelPreferenceChangeHistoriesRowCount({userNotificationChannelPreferenceId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any UserNotificationChannelPreferenceData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userNotificationChannelPreference.Reload();
  //
  //  Non Async:
  //
  //     userNotificationChannelPreference[0].Reload().then(x => {
  //        this.userNotificationChannelPreference = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserNotificationChannelPreferenceService.Instance.GetUserNotificationChannelPreference(this.id, includeRelations)
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
     this._userNotificationChannelPreferenceChangeHistories = null;
     this._userNotificationChannelPreferenceChangeHistoriesPromise = null;
     this._userNotificationChannelPreferenceChangeHistoriesSubject.next(null);

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
     * Gets the UserNotificationChannelPreferenceChangeHistories for this UserNotificationChannelPreference.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userNotificationChannelPreference.UserNotificationChannelPreferenceChangeHistories.then(userNotificationChannelPreferences => { ... })
     *   or
     *   await this.userNotificationChannelPreference.userNotificationChannelPreferences
     *
    */
    public get UserNotificationChannelPreferenceChangeHistories(): Promise<UserNotificationChannelPreferenceChangeHistoryData[]> {
        if (this._userNotificationChannelPreferenceChangeHistories !== null) {
            return Promise.resolve(this._userNotificationChannelPreferenceChangeHistories);
        }

        if (this._userNotificationChannelPreferenceChangeHistoriesPromise !== null) {
            return this._userNotificationChannelPreferenceChangeHistoriesPromise;
        }

        // Start the load
        this.loadUserNotificationChannelPreferenceChangeHistories();

        return this._userNotificationChannelPreferenceChangeHistoriesPromise!;
    }



    private loadUserNotificationChannelPreferenceChangeHistories(): void {

        this._userNotificationChannelPreferenceChangeHistoriesPromise = lastValueFrom(
            UserNotificationChannelPreferenceService.Instance.GetUserNotificationChannelPreferenceChangeHistoriesForUserNotificationChannelPreference(this.id)
        )
        .then(UserNotificationChannelPreferenceChangeHistories => {
            this._userNotificationChannelPreferenceChangeHistories = UserNotificationChannelPreferenceChangeHistories ?? [];
            this._userNotificationChannelPreferenceChangeHistoriesSubject.next(this._userNotificationChannelPreferenceChangeHistories);
            return this._userNotificationChannelPreferenceChangeHistories;
         })
        .catch(err => {
            this._userNotificationChannelPreferenceChangeHistories = [];
            this._userNotificationChannelPreferenceChangeHistoriesSubject.next(this._userNotificationChannelPreferenceChangeHistories);
            throw err;
        })
        .finally(() => {
            this._userNotificationChannelPreferenceChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserNotificationChannelPreferenceChangeHistory. Call after mutations to force refresh.
     */
    public ClearUserNotificationChannelPreferenceChangeHistoriesCache(): void {
        this._userNotificationChannelPreferenceChangeHistories = null;
        this._userNotificationChannelPreferenceChangeHistoriesPromise = null;
        this._userNotificationChannelPreferenceChangeHistoriesSubject.next(this._userNotificationChannelPreferenceChangeHistories);      // Emit to observable
    }

    public get HasUserNotificationChannelPreferenceChangeHistories(): Promise<boolean> {
        return this.UserNotificationChannelPreferenceChangeHistories.then(userNotificationChannelPreferenceChangeHistories => userNotificationChannelPreferenceChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (userNotificationChannelPreference.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await userNotificationChannelPreference.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<UserNotificationChannelPreferenceData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<UserNotificationChannelPreferenceData>> {
        const info = await lastValueFrom(
            UserNotificationChannelPreferenceService.Instance.GetUserNotificationChannelPreferenceChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this UserNotificationChannelPreferenceData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserNotificationChannelPreferenceData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserNotificationChannelPreferenceSubmitData {
        return UserNotificationChannelPreferenceService.Instance.ConvertToUserNotificationChannelPreferenceSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserNotificationChannelPreferenceService extends SecureEndpointBase {

    private static _instance: UserNotificationChannelPreferenceService;
    private listCache: Map<string, Observable<Array<UserNotificationChannelPreferenceData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserNotificationChannelPreferenceBasicListData>>>;
    private recordCache: Map<string, Observable<UserNotificationChannelPreferenceData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private userNotificationChannelPreferenceChangeHistoryService: UserNotificationChannelPreferenceChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserNotificationChannelPreferenceData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserNotificationChannelPreferenceBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserNotificationChannelPreferenceData>>();

        UserNotificationChannelPreferenceService._instance = this;
    }

    public static get Instance(): UserNotificationChannelPreferenceService {
      return UserNotificationChannelPreferenceService._instance;
    }


    public ClearListCaches(config: UserNotificationChannelPreferenceQueryParameters | null = null) {

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


    public ConvertToUserNotificationChannelPreferenceSubmitData(data: UserNotificationChannelPreferenceData): UserNotificationChannelPreferenceSubmitData {

        let output = new UserNotificationChannelPreferenceSubmitData();

        output.id = data.id;
        output.userNotificationPreferenceId = data.userNotificationPreferenceId;
        output.notificationChannelTypeId = data.notificationChannelTypeId;
        output.isEnabled = data.isEnabled;
        output.priorityOverride = data.priorityOverride;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserNotificationChannelPreference(id: bigint | number, includeRelations: boolean = true) : Observable<UserNotificationChannelPreferenceData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userNotificationChannelPreference$ = this.requestUserNotificationChannelPreference(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserNotificationChannelPreference", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userNotificationChannelPreference$);

            return userNotificationChannelPreference$;
        }

        return this.recordCache.get(configHash) as Observable<UserNotificationChannelPreferenceData>;
    }

    private requestUserNotificationChannelPreference(id: bigint | number, includeRelations: boolean = true) : Observable<UserNotificationChannelPreferenceData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserNotificationChannelPreferenceData>(this.baseUrl + 'api/UserNotificationChannelPreference/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserNotificationChannelPreference(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserNotificationChannelPreference(id, includeRelations));
            }));
    }

    public GetUserNotificationChannelPreferenceList(config: UserNotificationChannelPreferenceQueryParameters | any = null) : Observable<Array<UserNotificationChannelPreferenceData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userNotificationChannelPreferenceList$ = this.requestUserNotificationChannelPreferenceList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserNotificationChannelPreference list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userNotificationChannelPreferenceList$);

            return userNotificationChannelPreferenceList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserNotificationChannelPreferenceData>>;
    }


    private requestUserNotificationChannelPreferenceList(config: UserNotificationChannelPreferenceQueryParameters | any) : Observable <Array<UserNotificationChannelPreferenceData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserNotificationChannelPreferenceData>>(this.baseUrl + 'api/UserNotificationChannelPreferences', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserNotificationChannelPreferenceList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserNotificationChannelPreferenceList(config));
            }));
    }

    public GetUserNotificationChannelPreferencesRowCount(config: UserNotificationChannelPreferenceQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userNotificationChannelPreferencesRowCount$ = this.requestUserNotificationChannelPreferencesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserNotificationChannelPreferences row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userNotificationChannelPreferencesRowCount$);

            return userNotificationChannelPreferencesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserNotificationChannelPreferencesRowCount(config: UserNotificationChannelPreferenceQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserNotificationChannelPreferences/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserNotificationChannelPreferencesRowCount(config));
            }));
    }

    public GetUserNotificationChannelPreferencesBasicListData(config: UserNotificationChannelPreferenceQueryParameters | any = null) : Observable<Array<UserNotificationChannelPreferenceBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userNotificationChannelPreferencesBasicListData$ = this.requestUserNotificationChannelPreferencesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserNotificationChannelPreferences basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userNotificationChannelPreferencesBasicListData$);

            return userNotificationChannelPreferencesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserNotificationChannelPreferenceBasicListData>>;
    }


    private requestUserNotificationChannelPreferencesBasicListData(config: UserNotificationChannelPreferenceQueryParameters | any) : Observable<Array<UserNotificationChannelPreferenceBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserNotificationChannelPreferenceBasicListData>>(this.baseUrl + 'api/UserNotificationChannelPreferences/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserNotificationChannelPreferencesBasicListData(config));
            }));

    }


    public PutUserNotificationChannelPreference(id: bigint | number, userNotificationChannelPreference: UserNotificationChannelPreferenceSubmitData) : Observable<UserNotificationChannelPreferenceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserNotificationChannelPreferenceData>(this.baseUrl + 'api/UserNotificationChannelPreference/' + id.toString(), userNotificationChannelPreference, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserNotificationChannelPreference(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserNotificationChannelPreference(id, userNotificationChannelPreference));
            }));
    }


    public PostUserNotificationChannelPreference(userNotificationChannelPreference: UserNotificationChannelPreferenceSubmitData) : Observable<UserNotificationChannelPreferenceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserNotificationChannelPreferenceData>(this.baseUrl + 'api/UserNotificationChannelPreference', userNotificationChannelPreference, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserNotificationChannelPreference(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserNotificationChannelPreference(userNotificationChannelPreference));
            }));
    }

  
    public DeleteUserNotificationChannelPreference(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserNotificationChannelPreference/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserNotificationChannelPreference(id));
            }));
    }

    public RollbackUserNotificationChannelPreference(id: bigint | number, versionNumber: bigint | number) : Observable<UserNotificationChannelPreferenceData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserNotificationChannelPreferenceData>(this.baseUrl + 'api/UserNotificationChannelPreference/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserNotificationChannelPreference(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackUserNotificationChannelPreference(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a UserNotificationChannelPreference.
     */
    public GetUserNotificationChannelPreferenceChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<UserNotificationChannelPreferenceData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserNotificationChannelPreferenceData>>(this.baseUrl + 'api/UserNotificationChannelPreference/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserNotificationChannelPreferenceChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a UserNotificationChannelPreference.
     */
    public GetUserNotificationChannelPreferenceAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<UserNotificationChannelPreferenceData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserNotificationChannelPreferenceData>[]>(this.baseUrl + 'api/UserNotificationChannelPreference/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserNotificationChannelPreferenceAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a UserNotificationChannelPreference.
     */
    public GetUserNotificationChannelPreferenceVersion(id: bigint | number, version: number): Observable<UserNotificationChannelPreferenceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserNotificationChannelPreferenceData>(this.baseUrl + 'api/UserNotificationChannelPreference/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserNotificationChannelPreference(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserNotificationChannelPreferenceVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a UserNotificationChannelPreference at a specific point in time.
     */
    public GetUserNotificationChannelPreferenceStateAtTime(id: bigint | number, time: string): Observable<UserNotificationChannelPreferenceData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserNotificationChannelPreferenceData>(this.baseUrl + 'api/UserNotificationChannelPreference/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserNotificationChannelPreference(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserNotificationChannelPreferenceStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: UserNotificationChannelPreferenceQueryParameters | any): string {

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

    public userIsAlertingUserNotificationChannelPreferenceReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingUserNotificationChannelPreferenceReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.UserNotificationChannelPreferences
        //
        if (userIsAlertingUserNotificationChannelPreferenceReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingUserNotificationChannelPreferenceReader = user.readPermission >= 1;
            } else {
                userIsAlertingUserNotificationChannelPreferenceReader = false;
            }
        }

        return userIsAlertingUserNotificationChannelPreferenceReader;
    }


    public userIsAlertingUserNotificationChannelPreferenceWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingUserNotificationChannelPreferenceWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.UserNotificationChannelPreferences
        //
        if (userIsAlertingUserNotificationChannelPreferenceWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingUserNotificationChannelPreferenceWriter = user.writePermission >= 50;
          } else {
            userIsAlertingUserNotificationChannelPreferenceWriter = false;
          }      
        }

        return userIsAlertingUserNotificationChannelPreferenceWriter;
    }

    public GetUserNotificationChannelPreferenceChangeHistoriesForUserNotificationChannelPreference(userNotificationChannelPreferenceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserNotificationChannelPreferenceChangeHistoryData[]> {
        return this.userNotificationChannelPreferenceChangeHistoryService.GetUserNotificationChannelPreferenceChangeHistoryList({
            userNotificationChannelPreferenceId: userNotificationChannelPreferenceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full UserNotificationChannelPreferenceData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserNotificationChannelPreferenceData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserNotificationChannelPreferenceTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserNotificationChannelPreference(raw: any): UserNotificationChannelPreferenceData {
    if (!raw) return raw;

    //
    // Create a UserNotificationChannelPreferenceData object instance with correct prototype
    //
    const revived = Object.create(UserNotificationChannelPreferenceData.prototype) as UserNotificationChannelPreferenceData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._userNotificationChannelPreferenceChangeHistories = null;
    (revived as any)._userNotificationChannelPreferenceChangeHistoriesPromise = null;
    (revived as any)._userNotificationChannelPreferenceChangeHistoriesSubject = new BehaviorSubject<UserNotificationChannelPreferenceChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadUserNotificationChannelPreferenceXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).UserNotificationChannelPreferenceChangeHistories$ = (revived as any)._userNotificationChannelPreferenceChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userNotificationChannelPreferenceChangeHistories === null && (revived as any)._userNotificationChannelPreferenceChangeHistoriesPromise === null) {
                (revived as any).loadUserNotificationChannelPreferenceChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).UserNotificationChannelPreferenceChangeHistoriesCount$ = UserNotificationChannelPreferenceChangeHistoryService.Instance.GetUserNotificationChannelPreferenceChangeHistoriesRowCount({userNotificationChannelPreferenceId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserNotificationChannelPreferenceData> | null>(null);

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

  private ReviveUserNotificationChannelPreferenceList(rawList: any[]): UserNotificationChannelPreferenceData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserNotificationChannelPreference(raw));
  }

}
