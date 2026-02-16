/*

   GENERATED SERVICE FOR THE USERNOTIFICATIONCHANNELPREFERENCECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserNotificationChannelPreferenceChangeHistory table.

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
import { UserNotificationChannelPreferenceData } from './user-notification-channel-preference.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserNotificationChannelPreferenceChangeHistoryQueryParameters {
    userNotificationChannelPreferenceId: bigint | number | null | undefined = null;
    versionNumber: bigint | number | null | undefined = null;
    timeStamp: string | null | undefined = null;        // ISO 8601 (full datetime)
    userId: bigint | number | null | undefined = null;
    data: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class UserNotificationChannelPreferenceChangeHistorySubmitData {
    id!: bigint | number;
    userNotificationChannelPreferenceId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
}


export class UserNotificationChannelPreferenceChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserNotificationChannelPreferenceChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userNotificationChannelPreferenceChangeHistory.UserNotificationChannelPreferenceChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `userNotificationChannelPreferenceChangeHistory.UserNotificationChannelPreferenceChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userNotificationChannelPreferenceChangeHistory.UserNotificationChannelPreferenceChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`userNotificationChannelPreferenceChangeHistory.UserNotificationChannelPreferenceChangeHistoryChildren` or `await userNotificationChannelPreferenceChangeHistory.UserNotificationChannelPreferenceChangeHistoryChildren`)
//    - Simply reading `userNotificationChannelPreferenceChangeHistory.UserNotificationChannelPreferenceChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userNotificationChannelPreferenceChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserNotificationChannelPreferenceChangeHistoryData {
    id!: bigint | number;
    userNotificationChannelPreferenceId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
    userNotificationChannelPreference: UserNotificationChannelPreferenceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any UserNotificationChannelPreferenceChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userNotificationChannelPreferenceChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     userNotificationChannelPreferenceChangeHistory[0].Reload().then(x => {
  //        this.userNotificationChannelPreferenceChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserNotificationChannelPreferenceChangeHistoryService.Instance.GetUserNotificationChannelPreferenceChangeHistory(this.id, includeRelations)
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
     * Updates the state of this UserNotificationChannelPreferenceChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserNotificationChannelPreferenceChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserNotificationChannelPreferenceChangeHistorySubmitData {
        return UserNotificationChannelPreferenceChangeHistoryService.Instance.ConvertToUserNotificationChannelPreferenceChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserNotificationChannelPreferenceChangeHistoryService extends SecureEndpointBase {

    private static _instance: UserNotificationChannelPreferenceChangeHistoryService;
    private listCache: Map<string, Observable<Array<UserNotificationChannelPreferenceChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserNotificationChannelPreferenceChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<UserNotificationChannelPreferenceChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserNotificationChannelPreferenceChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserNotificationChannelPreferenceChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserNotificationChannelPreferenceChangeHistoryData>>();

        UserNotificationChannelPreferenceChangeHistoryService._instance = this;
    }

    public static get Instance(): UserNotificationChannelPreferenceChangeHistoryService {
      return UserNotificationChannelPreferenceChangeHistoryService._instance;
    }


    public ClearListCaches(config: UserNotificationChannelPreferenceChangeHistoryQueryParameters | null = null) {

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


    public ConvertToUserNotificationChannelPreferenceChangeHistorySubmitData(data: UserNotificationChannelPreferenceChangeHistoryData): UserNotificationChannelPreferenceChangeHistorySubmitData {

        let output = new UserNotificationChannelPreferenceChangeHistorySubmitData();

        output.id = data.id;
        output.userNotificationChannelPreferenceId = data.userNotificationChannelPreferenceId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetUserNotificationChannelPreferenceChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<UserNotificationChannelPreferenceChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userNotificationChannelPreferenceChangeHistory$ = this.requestUserNotificationChannelPreferenceChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserNotificationChannelPreferenceChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userNotificationChannelPreferenceChangeHistory$);

            return userNotificationChannelPreferenceChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<UserNotificationChannelPreferenceChangeHistoryData>;
    }

    private requestUserNotificationChannelPreferenceChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<UserNotificationChannelPreferenceChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserNotificationChannelPreferenceChangeHistoryData>(this.baseUrl + 'api/UserNotificationChannelPreferenceChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserNotificationChannelPreferenceChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserNotificationChannelPreferenceChangeHistory(id, includeRelations));
            }));
    }

    public GetUserNotificationChannelPreferenceChangeHistoryList(config: UserNotificationChannelPreferenceChangeHistoryQueryParameters | any = null) : Observable<Array<UserNotificationChannelPreferenceChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userNotificationChannelPreferenceChangeHistoryList$ = this.requestUserNotificationChannelPreferenceChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserNotificationChannelPreferenceChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userNotificationChannelPreferenceChangeHistoryList$);

            return userNotificationChannelPreferenceChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserNotificationChannelPreferenceChangeHistoryData>>;
    }


    private requestUserNotificationChannelPreferenceChangeHistoryList(config: UserNotificationChannelPreferenceChangeHistoryQueryParameters | any) : Observable <Array<UserNotificationChannelPreferenceChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserNotificationChannelPreferenceChangeHistoryData>>(this.baseUrl + 'api/UserNotificationChannelPreferenceChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserNotificationChannelPreferenceChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserNotificationChannelPreferenceChangeHistoryList(config));
            }));
    }

    public GetUserNotificationChannelPreferenceChangeHistoriesRowCount(config: UserNotificationChannelPreferenceChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userNotificationChannelPreferenceChangeHistoriesRowCount$ = this.requestUserNotificationChannelPreferenceChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserNotificationChannelPreferenceChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userNotificationChannelPreferenceChangeHistoriesRowCount$);

            return userNotificationChannelPreferenceChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserNotificationChannelPreferenceChangeHistoriesRowCount(config: UserNotificationChannelPreferenceChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserNotificationChannelPreferenceChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserNotificationChannelPreferenceChangeHistoriesRowCount(config));
            }));
    }

    public GetUserNotificationChannelPreferenceChangeHistoriesBasicListData(config: UserNotificationChannelPreferenceChangeHistoryQueryParameters | any = null) : Observable<Array<UserNotificationChannelPreferenceChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userNotificationChannelPreferenceChangeHistoriesBasicListData$ = this.requestUserNotificationChannelPreferenceChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserNotificationChannelPreferenceChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userNotificationChannelPreferenceChangeHistoriesBasicListData$);

            return userNotificationChannelPreferenceChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserNotificationChannelPreferenceChangeHistoryBasicListData>>;
    }


    private requestUserNotificationChannelPreferenceChangeHistoriesBasicListData(config: UserNotificationChannelPreferenceChangeHistoryQueryParameters | any) : Observable<Array<UserNotificationChannelPreferenceChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserNotificationChannelPreferenceChangeHistoryBasicListData>>(this.baseUrl + 'api/UserNotificationChannelPreferenceChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserNotificationChannelPreferenceChangeHistoriesBasicListData(config));
            }));

    }


    public PutUserNotificationChannelPreferenceChangeHistory(id: bigint | number, userNotificationChannelPreferenceChangeHistory: UserNotificationChannelPreferenceChangeHistorySubmitData) : Observable<UserNotificationChannelPreferenceChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserNotificationChannelPreferenceChangeHistoryData>(this.baseUrl + 'api/UserNotificationChannelPreferenceChangeHistory/' + id.toString(), userNotificationChannelPreferenceChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserNotificationChannelPreferenceChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserNotificationChannelPreferenceChangeHistory(id, userNotificationChannelPreferenceChangeHistory));
            }));
    }


    public PostUserNotificationChannelPreferenceChangeHistory(userNotificationChannelPreferenceChangeHistory: UserNotificationChannelPreferenceChangeHistorySubmitData) : Observable<UserNotificationChannelPreferenceChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserNotificationChannelPreferenceChangeHistoryData>(this.baseUrl + 'api/UserNotificationChannelPreferenceChangeHistory', userNotificationChannelPreferenceChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserNotificationChannelPreferenceChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserNotificationChannelPreferenceChangeHistory(userNotificationChannelPreferenceChangeHistory));
            }));
    }

  
    public DeleteUserNotificationChannelPreferenceChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserNotificationChannelPreferenceChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserNotificationChannelPreferenceChangeHistory(id));
            }));
    }


    private getConfigHash(config: UserNotificationChannelPreferenceChangeHistoryQueryParameters | any): string {

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

    public userIsAlertingUserNotificationChannelPreferenceChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingUserNotificationChannelPreferenceChangeHistoryReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.UserNotificationChannelPreferenceChangeHistories
        //
        if (userIsAlertingUserNotificationChannelPreferenceChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingUserNotificationChannelPreferenceChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsAlertingUserNotificationChannelPreferenceChangeHistoryReader = false;
            }
        }

        return userIsAlertingUserNotificationChannelPreferenceChangeHistoryReader;
    }


    public userIsAlertingUserNotificationChannelPreferenceChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingUserNotificationChannelPreferenceChangeHistoryWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.UserNotificationChannelPreferenceChangeHistories
        //
        if (userIsAlertingUserNotificationChannelPreferenceChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingUserNotificationChannelPreferenceChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsAlertingUserNotificationChannelPreferenceChangeHistoryWriter = false;
          }      
        }

        return userIsAlertingUserNotificationChannelPreferenceChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full UserNotificationChannelPreferenceChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserNotificationChannelPreferenceChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserNotificationChannelPreferenceChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserNotificationChannelPreferenceChangeHistory(raw: any): UserNotificationChannelPreferenceChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a UserNotificationChannelPreferenceChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(UserNotificationChannelPreferenceChangeHistoryData.prototype) as UserNotificationChannelPreferenceChangeHistoryData;

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
    // 2. But private methods (loadUserNotificationChannelPreferenceChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveUserNotificationChannelPreferenceChangeHistoryList(rawList: any[]): UserNotificationChannelPreferenceChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserNotificationChannelPreferenceChangeHistory(raw));
  }

}
