/*

   GENERATED SERVICE FOR THE USERNOTIFICATIONPREFERENCE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserNotificationPreference table.

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
import { UserNotificationPreferenceChangeHistoryService, UserNotificationPreferenceChangeHistoryData } from './user-notification-preference-change-history.service';
import { UserNotificationChannelPreferenceService, UserNotificationChannelPreferenceData } from './user-notification-channel-preference.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserNotificationPreferenceQueryParameters {
    securityUserObjectGuid: string | null | undefined = null;
    timeZoneId: string | null | undefined = null;
    quietHoursStart: string | null | undefined = null;
    quietHoursEnd: string | null | undefined = null;
    isDoNotDisturb: boolean | null | undefined = null;
    isDoNotDisturbPermanent: boolean | null | undefined = null;
    doNotDisturbUntil: string | null | undefined = null;        // ISO 8601
    customSettingsJson: string | null | undefined = null;
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
export class UserNotificationPreferenceSubmitData {
    id!: bigint | number;
    securityUserObjectGuid!: string;
    timeZoneId: string | null = null;
    quietHoursStart: string | null = null;
    quietHoursEnd: string | null = null;
    isDoNotDisturb!: boolean;
    isDoNotDisturbPermanent!: boolean;
    doNotDisturbUntil: string | null = null;     // ISO 8601
    customSettingsJson: string | null = null;
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

export class UserNotificationPreferenceBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserNotificationPreferenceChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userNotificationPreference.UserNotificationPreferenceChildren$` — use with `| async` in templates
//        • Promise:    `userNotificationPreference.UserNotificationPreferenceChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userNotificationPreference.UserNotificationPreferenceChildren$ | async"`), or
//        • Access the promise getter (`userNotificationPreference.UserNotificationPreferenceChildren` or `await userNotificationPreference.UserNotificationPreferenceChildren`)
//    - Simply reading `userNotificationPreference.UserNotificationPreferenceChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userNotificationPreference.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserNotificationPreferenceData {
    id!: bigint | number;
    securityUserObjectGuid!: string;
    timeZoneId!: string | null;
    quietHoursStart!: string | null;
    quietHoursEnd!: string | null;
    isDoNotDisturb!: boolean;
    isDoNotDisturbPermanent!: boolean;
    doNotDisturbUntil!: string | null;   // ISO 8601
    customSettingsJson!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _userNotificationPreferenceChangeHistories: UserNotificationPreferenceChangeHistoryData[] | null = null;
    private _userNotificationPreferenceChangeHistoriesPromise: Promise<UserNotificationPreferenceChangeHistoryData[]> | null  = null;
    private _userNotificationPreferenceChangeHistoriesSubject = new BehaviorSubject<UserNotificationPreferenceChangeHistoryData[] | null>(null);

                
    private _userNotificationChannelPreferences: UserNotificationChannelPreferenceData[] | null = null;
    private _userNotificationChannelPreferencesPromise: Promise<UserNotificationChannelPreferenceData[]> | null  = null;
    private _userNotificationChannelPreferencesSubject = new BehaviorSubject<UserNotificationChannelPreferenceData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<UserNotificationPreferenceData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<UserNotificationPreferenceData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserNotificationPreferenceData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public UserNotificationPreferenceChangeHistories$ = this._userNotificationPreferenceChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userNotificationPreferenceChangeHistories === null && this._userNotificationPreferenceChangeHistoriesPromise === null) {
            this.loadUserNotificationPreferenceChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public UserNotificationPreferenceChangeHistoriesCount$ = UserNotificationPreferenceChangeHistoryService.Instance.GetUserNotificationPreferenceChangeHistoriesRowCount({userNotificationPreferenceId: this.id,
      active: true,
      deleted: false
    });



    public UserNotificationChannelPreferences$ = this._userNotificationChannelPreferencesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userNotificationChannelPreferences === null && this._userNotificationChannelPreferencesPromise === null) {
            this.loadUserNotificationChannelPreferences(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public UserNotificationChannelPreferencesCount$ = UserNotificationChannelPreferenceService.Instance.GetUserNotificationChannelPreferencesRowCount({userNotificationPreferenceId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any UserNotificationPreferenceData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userNotificationPreference.Reload();
  //
  //  Non Async:
  //
  //     userNotificationPreference[0].Reload().then(x => {
  //        this.userNotificationPreference = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserNotificationPreferenceService.Instance.GetUserNotificationPreference(this.id, includeRelations)
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
     this._userNotificationPreferenceChangeHistories = null;
     this._userNotificationPreferenceChangeHistoriesPromise = null;
     this._userNotificationPreferenceChangeHistoriesSubject.next(null);

     this._userNotificationChannelPreferences = null;
     this._userNotificationChannelPreferencesPromise = null;
     this._userNotificationChannelPreferencesSubject.next(null);

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
     * Gets the UserNotificationPreferenceChangeHistories for this UserNotificationPreference.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userNotificationPreference.UserNotificationPreferenceChangeHistories.then(userNotificationPreferences => { ... })
     *   or
     *   await this.userNotificationPreference.userNotificationPreferences
     *
    */
    public get UserNotificationPreferenceChangeHistories(): Promise<UserNotificationPreferenceChangeHistoryData[]> {
        if (this._userNotificationPreferenceChangeHistories !== null) {
            return Promise.resolve(this._userNotificationPreferenceChangeHistories);
        }

        if (this._userNotificationPreferenceChangeHistoriesPromise !== null) {
            return this._userNotificationPreferenceChangeHistoriesPromise;
        }

        // Start the load
        this.loadUserNotificationPreferenceChangeHistories();

        return this._userNotificationPreferenceChangeHistoriesPromise!;
    }



    private loadUserNotificationPreferenceChangeHistories(): void {

        this._userNotificationPreferenceChangeHistoriesPromise = lastValueFrom(
            UserNotificationPreferenceService.Instance.GetUserNotificationPreferenceChangeHistoriesForUserNotificationPreference(this.id)
        )
        .then(UserNotificationPreferenceChangeHistories => {
            this._userNotificationPreferenceChangeHistories = UserNotificationPreferenceChangeHistories ?? [];
            this._userNotificationPreferenceChangeHistoriesSubject.next(this._userNotificationPreferenceChangeHistories);
            return this._userNotificationPreferenceChangeHistories;
         })
        .catch(err => {
            this._userNotificationPreferenceChangeHistories = [];
            this._userNotificationPreferenceChangeHistoriesSubject.next(this._userNotificationPreferenceChangeHistories);
            throw err;
        })
        .finally(() => {
            this._userNotificationPreferenceChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserNotificationPreferenceChangeHistory. Call after mutations to force refresh.
     */
    public ClearUserNotificationPreferenceChangeHistoriesCache(): void {
        this._userNotificationPreferenceChangeHistories = null;
        this._userNotificationPreferenceChangeHistoriesPromise = null;
        this._userNotificationPreferenceChangeHistoriesSubject.next(this._userNotificationPreferenceChangeHistories);      // Emit to observable
    }

    public get HasUserNotificationPreferenceChangeHistories(): Promise<boolean> {
        return this.UserNotificationPreferenceChangeHistories.then(userNotificationPreferenceChangeHistories => userNotificationPreferenceChangeHistories.length > 0);
    }


    /**
     *
     * Gets the UserNotificationChannelPreferences for this UserNotificationPreference.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userNotificationPreference.UserNotificationChannelPreferences.then(userNotificationPreferences => { ... })
     *   or
     *   await this.userNotificationPreference.userNotificationPreferences
     *
    */
    public get UserNotificationChannelPreferences(): Promise<UserNotificationChannelPreferenceData[]> {
        if (this._userNotificationChannelPreferences !== null) {
            return Promise.resolve(this._userNotificationChannelPreferences);
        }

        if (this._userNotificationChannelPreferencesPromise !== null) {
            return this._userNotificationChannelPreferencesPromise;
        }

        // Start the load
        this.loadUserNotificationChannelPreferences();

        return this._userNotificationChannelPreferencesPromise!;
    }



    private loadUserNotificationChannelPreferences(): void {

        this._userNotificationChannelPreferencesPromise = lastValueFrom(
            UserNotificationPreferenceService.Instance.GetUserNotificationChannelPreferencesForUserNotificationPreference(this.id)
        )
        .then(UserNotificationChannelPreferences => {
            this._userNotificationChannelPreferences = UserNotificationChannelPreferences ?? [];
            this._userNotificationChannelPreferencesSubject.next(this._userNotificationChannelPreferences);
            return this._userNotificationChannelPreferences;
         })
        .catch(err => {
            this._userNotificationChannelPreferences = [];
            this._userNotificationChannelPreferencesSubject.next(this._userNotificationChannelPreferences);
            throw err;
        })
        .finally(() => {
            this._userNotificationChannelPreferencesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserNotificationChannelPreference. Call after mutations to force refresh.
     */
    public ClearUserNotificationChannelPreferencesCache(): void {
        this._userNotificationChannelPreferences = null;
        this._userNotificationChannelPreferencesPromise = null;
        this._userNotificationChannelPreferencesSubject.next(this._userNotificationChannelPreferences);      // Emit to observable
    }

    public get HasUserNotificationChannelPreferences(): Promise<boolean> {
        return this.UserNotificationChannelPreferences.then(userNotificationChannelPreferences => userNotificationChannelPreferences.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (userNotificationPreference.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await userNotificationPreference.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<UserNotificationPreferenceData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<UserNotificationPreferenceData>> {
        const info = await lastValueFrom(
            UserNotificationPreferenceService.Instance.GetUserNotificationPreferenceChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this UserNotificationPreferenceData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserNotificationPreferenceData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserNotificationPreferenceSubmitData {
        return UserNotificationPreferenceService.Instance.ConvertToUserNotificationPreferenceSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserNotificationPreferenceService extends SecureEndpointBase {

    private static _instance: UserNotificationPreferenceService;
    private listCache: Map<string, Observable<Array<UserNotificationPreferenceData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserNotificationPreferenceBasicListData>>>;
    private recordCache: Map<string, Observable<UserNotificationPreferenceData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private userNotificationPreferenceChangeHistoryService: UserNotificationPreferenceChangeHistoryService,
        private userNotificationChannelPreferenceService: UserNotificationChannelPreferenceService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserNotificationPreferenceData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserNotificationPreferenceBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserNotificationPreferenceData>>();

        UserNotificationPreferenceService._instance = this;
    }

    public static get Instance(): UserNotificationPreferenceService {
      return UserNotificationPreferenceService._instance;
    }


    public ClearListCaches(config: UserNotificationPreferenceQueryParameters | null = null) {

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


    public ConvertToUserNotificationPreferenceSubmitData(data: UserNotificationPreferenceData): UserNotificationPreferenceSubmitData {

        let output = new UserNotificationPreferenceSubmitData();

        output.id = data.id;
        output.securityUserObjectGuid = data.securityUserObjectGuid;
        output.timeZoneId = data.timeZoneId;
        output.quietHoursStart = data.quietHoursStart;
        output.quietHoursEnd = data.quietHoursEnd;
        output.isDoNotDisturb = data.isDoNotDisturb;
        output.isDoNotDisturbPermanent = data.isDoNotDisturbPermanent;
        output.doNotDisturbUntil = data.doNotDisturbUntil;
        output.customSettingsJson = data.customSettingsJson;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserNotificationPreference(id: bigint | number, includeRelations: boolean = true) : Observable<UserNotificationPreferenceData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userNotificationPreference$ = this.requestUserNotificationPreference(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserNotificationPreference", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userNotificationPreference$);

            return userNotificationPreference$;
        }

        return this.recordCache.get(configHash) as Observable<UserNotificationPreferenceData>;
    }

    private requestUserNotificationPreference(id: bigint | number, includeRelations: boolean = true) : Observable<UserNotificationPreferenceData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserNotificationPreferenceData>(this.baseUrl + 'api/UserNotificationPreference/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserNotificationPreference(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserNotificationPreference(id, includeRelations));
            }));
    }

    public GetUserNotificationPreferenceList(config: UserNotificationPreferenceQueryParameters | any = null) : Observable<Array<UserNotificationPreferenceData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userNotificationPreferenceList$ = this.requestUserNotificationPreferenceList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserNotificationPreference list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userNotificationPreferenceList$);

            return userNotificationPreferenceList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserNotificationPreferenceData>>;
    }


    private requestUserNotificationPreferenceList(config: UserNotificationPreferenceQueryParameters | any) : Observable <Array<UserNotificationPreferenceData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserNotificationPreferenceData>>(this.baseUrl + 'api/UserNotificationPreferences', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserNotificationPreferenceList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserNotificationPreferenceList(config));
            }));
    }

    public GetUserNotificationPreferencesRowCount(config: UserNotificationPreferenceQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userNotificationPreferencesRowCount$ = this.requestUserNotificationPreferencesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserNotificationPreferences row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userNotificationPreferencesRowCount$);

            return userNotificationPreferencesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserNotificationPreferencesRowCount(config: UserNotificationPreferenceQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserNotificationPreferences/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserNotificationPreferencesRowCount(config));
            }));
    }

    public GetUserNotificationPreferencesBasicListData(config: UserNotificationPreferenceQueryParameters | any = null) : Observable<Array<UserNotificationPreferenceBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userNotificationPreferencesBasicListData$ = this.requestUserNotificationPreferencesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserNotificationPreferences basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userNotificationPreferencesBasicListData$);

            return userNotificationPreferencesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserNotificationPreferenceBasicListData>>;
    }


    private requestUserNotificationPreferencesBasicListData(config: UserNotificationPreferenceQueryParameters | any) : Observable<Array<UserNotificationPreferenceBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserNotificationPreferenceBasicListData>>(this.baseUrl + 'api/UserNotificationPreferences/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserNotificationPreferencesBasicListData(config));
            }));

    }


    public PutUserNotificationPreference(id: bigint | number, userNotificationPreference: UserNotificationPreferenceSubmitData) : Observable<UserNotificationPreferenceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserNotificationPreferenceData>(this.baseUrl + 'api/UserNotificationPreference/' + id.toString(), userNotificationPreference, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserNotificationPreference(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserNotificationPreference(id, userNotificationPreference));
            }));
    }


    public PostUserNotificationPreference(userNotificationPreference: UserNotificationPreferenceSubmitData) : Observable<UserNotificationPreferenceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserNotificationPreferenceData>(this.baseUrl + 'api/UserNotificationPreference', userNotificationPreference, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserNotificationPreference(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserNotificationPreference(userNotificationPreference));
            }));
    }

  
    public DeleteUserNotificationPreference(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserNotificationPreference/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserNotificationPreference(id));
            }));
    }

    public RollbackUserNotificationPreference(id: bigint | number, versionNumber: bigint | number) : Observable<UserNotificationPreferenceData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserNotificationPreferenceData>(this.baseUrl + 'api/UserNotificationPreference/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserNotificationPreference(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackUserNotificationPreference(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a UserNotificationPreference.
     */
    public GetUserNotificationPreferenceChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<UserNotificationPreferenceData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserNotificationPreferenceData>>(this.baseUrl + 'api/UserNotificationPreference/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserNotificationPreferenceChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a UserNotificationPreference.
     */
    public GetUserNotificationPreferenceAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<UserNotificationPreferenceData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserNotificationPreferenceData>[]>(this.baseUrl + 'api/UserNotificationPreference/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserNotificationPreferenceAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a UserNotificationPreference.
     */
    public GetUserNotificationPreferenceVersion(id: bigint | number, version: number): Observable<UserNotificationPreferenceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserNotificationPreferenceData>(this.baseUrl + 'api/UserNotificationPreference/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserNotificationPreference(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserNotificationPreferenceVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a UserNotificationPreference at a specific point in time.
     */
    public GetUserNotificationPreferenceStateAtTime(id: bigint | number, time: string): Observable<UserNotificationPreferenceData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserNotificationPreferenceData>(this.baseUrl + 'api/UserNotificationPreference/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserNotificationPreference(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserNotificationPreferenceStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: UserNotificationPreferenceQueryParameters | any): string {

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

    public userIsAlertingUserNotificationPreferenceReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingUserNotificationPreferenceReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.UserNotificationPreferences
        //
        if (userIsAlertingUserNotificationPreferenceReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingUserNotificationPreferenceReader = user.readPermission >= 1;
            } else {
                userIsAlertingUserNotificationPreferenceReader = false;
            }
        }

        return userIsAlertingUserNotificationPreferenceReader;
    }


    public userIsAlertingUserNotificationPreferenceWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingUserNotificationPreferenceWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.UserNotificationPreferences
        //
        if (userIsAlertingUserNotificationPreferenceWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingUserNotificationPreferenceWriter = user.writePermission >= 50;
          } else {
            userIsAlertingUserNotificationPreferenceWriter = false;
          }      
        }

        return userIsAlertingUserNotificationPreferenceWriter;
    }

    public GetUserNotificationPreferenceChangeHistoriesForUserNotificationPreference(userNotificationPreferenceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserNotificationPreferenceChangeHistoryData[]> {
        return this.userNotificationPreferenceChangeHistoryService.GetUserNotificationPreferenceChangeHistoryList({
            userNotificationPreferenceId: userNotificationPreferenceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserNotificationChannelPreferencesForUserNotificationPreference(userNotificationPreferenceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserNotificationChannelPreferenceData[]> {
        return this.userNotificationChannelPreferenceService.GetUserNotificationChannelPreferenceList({
            userNotificationPreferenceId: userNotificationPreferenceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full UserNotificationPreferenceData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserNotificationPreferenceData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserNotificationPreferenceTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserNotificationPreference(raw: any): UserNotificationPreferenceData {
    if (!raw) return raw;

    //
    // Create a UserNotificationPreferenceData object instance with correct prototype
    //
    const revived = Object.create(UserNotificationPreferenceData.prototype) as UserNotificationPreferenceData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._userNotificationPreferenceChangeHistories = null;
    (revived as any)._userNotificationPreferenceChangeHistoriesPromise = null;
    (revived as any)._userNotificationPreferenceChangeHistoriesSubject = new BehaviorSubject<UserNotificationPreferenceChangeHistoryData[] | null>(null);

    (revived as any)._userNotificationChannelPreferences = null;
    (revived as any)._userNotificationChannelPreferencesPromise = null;
    (revived as any)._userNotificationChannelPreferencesSubject = new BehaviorSubject<UserNotificationChannelPreferenceData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadUserNotificationPreferenceXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).UserNotificationPreferenceChangeHistories$ = (revived as any)._userNotificationPreferenceChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userNotificationPreferenceChangeHistories === null && (revived as any)._userNotificationPreferenceChangeHistoriesPromise === null) {
                (revived as any).loadUserNotificationPreferenceChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).UserNotificationPreferenceChangeHistoriesCount$ = UserNotificationPreferenceChangeHistoryService.Instance.GetUserNotificationPreferenceChangeHistoriesRowCount({userNotificationPreferenceId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).UserNotificationChannelPreferences$ = (revived as any)._userNotificationChannelPreferencesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userNotificationChannelPreferences === null && (revived as any)._userNotificationChannelPreferencesPromise === null) {
                (revived as any).loadUserNotificationChannelPreferences();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).UserNotificationChannelPreferencesCount$ = UserNotificationChannelPreferenceService.Instance.GetUserNotificationChannelPreferencesRowCount({userNotificationPreferenceId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserNotificationPreferenceData> | null>(null);

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

  private ReviveUserNotificationPreferenceList(rawList: any[]): UserNotificationPreferenceData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserNotificationPreference(raw));
  }

}
