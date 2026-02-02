/*

   GENERATED SERVICE FOR THE NOTIFICATIONCHANNELTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the NotificationChannelType table.

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
import { UserNotificationChannelPreferenceService, UserNotificationChannelPreferenceData } from './user-notification-channel-preference.service';
import { NotificationDeliveryAttemptService, NotificationDeliveryAttemptData } from './notification-delivery-attempt.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class NotificationChannelTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    defaultPriority: bigint | number | null | undefined = null;
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
export class NotificationChannelTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    defaultPriority!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class NotificationChannelTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. NotificationChannelTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `notificationChannelType.NotificationChannelTypeChildren$` — use with `| async` in templates
//        • Promise:    `notificationChannelType.NotificationChannelTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="notificationChannelType.NotificationChannelTypeChildren$ | async"`), or
//        • Access the promise getter (`notificationChannelType.NotificationChannelTypeChildren` or `await notificationChannelType.NotificationChannelTypeChildren`)
//    - Simply reading `notificationChannelType.NotificationChannelTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await notificationChannelType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class NotificationChannelTypeData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    defaultPriority!: bigint | number;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _userNotificationChannelPreferences: UserNotificationChannelPreferenceData[] | null = null;
    private _userNotificationChannelPreferencesPromise: Promise<UserNotificationChannelPreferenceData[]> | null  = null;
    private _userNotificationChannelPreferencesSubject = new BehaviorSubject<UserNotificationChannelPreferenceData[] | null>(null);

                
    private _notificationDeliveryAttempts: NotificationDeliveryAttemptData[] | null = null;
    private _notificationDeliveryAttemptsPromise: Promise<NotificationDeliveryAttemptData[]> | null  = null;
    private _notificationDeliveryAttemptsSubject = new BehaviorSubject<NotificationDeliveryAttemptData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public UserNotificationChannelPreferences$ = this._userNotificationChannelPreferencesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userNotificationChannelPreferences === null && this._userNotificationChannelPreferencesPromise === null) {
            this.loadUserNotificationChannelPreferences(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public UserNotificationChannelPreferencesCount$ = UserNotificationChannelPreferenceService.Instance.GetUserNotificationChannelPreferencesRowCount({notificationChannelTypeId: this.id,
      active: true,
      deleted: false
    });



    public NotificationDeliveryAttempts$ = this._notificationDeliveryAttemptsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._notificationDeliveryAttempts === null && this._notificationDeliveryAttemptsPromise === null) {
            this.loadNotificationDeliveryAttempts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public NotificationDeliveryAttemptsCount$ = NotificationDeliveryAttemptService.Instance.GetNotificationDeliveryAttemptsRowCount({notificationChannelTypeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any NotificationChannelTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.notificationChannelType.Reload();
  //
  //  Non Async:
  //
  //     notificationChannelType[0].Reload().then(x => {
  //        this.notificationChannelType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      NotificationChannelTypeService.Instance.GetNotificationChannelType(this.id, includeRelations)
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
     this._userNotificationChannelPreferences = null;
     this._userNotificationChannelPreferencesPromise = null;
     this._userNotificationChannelPreferencesSubject.next(null);

     this._notificationDeliveryAttempts = null;
     this._notificationDeliveryAttemptsPromise = null;
     this._notificationDeliveryAttemptsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the UserNotificationChannelPreferences for this NotificationChannelType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.notificationChannelType.UserNotificationChannelPreferences.then(notificationChannelTypes => { ... })
     *   or
     *   await this.notificationChannelType.notificationChannelTypes
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
            NotificationChannelTypeService.Instance.GetUserNotificationChannelPreferencesForNotificationChannelType(this.id)
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


    /**
     *
     * Gets the NotificationDeliveryAttempts for this NotificationChannelType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.notificationChannelType.NotificationDeliveryAttempts.then(notificationChannelTypes => { ... })
     *   or
     *   await this.notificationChannelType.notificationChannelTypes
     *
    */
    public get NotificationDeliveryAttempts(): Promise<NotificationDeliveryAttemptData[]> {
        if (this._notificationDeliveryAttempts !== null) {
            return Promise.resolve(this._notificationDeliveryAttempts);
        }

        if (this._notificationDeliveryAttemptsPromise !== null) {
            return this._notificationDeliveryAttemptsPromise;
        }

        // Start the load
        this.loadNotificationDeliveryAttempts();

        return this._notificationDeliveryAttemptsPromise!;
    }



    private loadNotificationDeliveryAttempts(): void {

        this._notificationDeliveryAttemptsPromise = lastValueFrom(
            NotificationChannelTypeService.Instance.GetNotificationDeliveryAttemptsForNotificationChannelType(this.id)
        )
        .then(NotificationDeliveryAttempts => {
            this._notificationDeliveryAttempts = NotificationDeliveryAttempts ?? [];
            this._notificationDeliveryAttemptsSubject.next(this._notificationDeliveryAttempts);
            return this._notificationDeliveryAttempts;
         })
        .catch(err => {
            this._notificationDeliveryAttempts = [];
            this._notificationDeliveryAttemptsSubject.next(this._notificationDeliveryAttempts);
            throw err;
        })
        .finally(() => {
            this._notificationDeliveryAttemptsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached NotificationDeliveryAttempt. Call after mutations to force refresh.
     */
    public ClearNotificationDeliveryAttemptsCache(): void {
        this._notificationDeliveryAttempts = null;
        this._notificationDeliveryAttemptsPromise = null;
        this._notificationDeliveryAttemptsSubject.next(this._notificationDeliveryAttempts);      // Emit to observable
    }

    public get HasNotificationDeliveryAttempts(): Promise<boolean> {
        return this.NotificationDeliveryAttempts.then(notificationDeliveryAttempts => notificationDeliveryAttempts.length > 0);
    }




    /**
     * Updates the state of this NotificationChannelTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this NotificationChannelTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): NotificationChannelTypeSubmitData {
        return NotificationChannelTypeService.Instance.ConvertToNotificationChannelTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class NotificationChannelTypeService extends SecureEndpointBase {

    private static _instance: NotificationChannelTypeService;
    private listCache: Map<string, Observable<Array<NotificationChannelTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<NotificationChannelTypeBasicListData>>>;
    private recordCache: Map<string, Observable<NotificationChannelTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private userNotificationChannelPreferenceService: UserNotificationChannelPreferenceService,
        private notificationDeliveryAttemptService: NotificationDeliveryAttemptService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<NotificationChannelTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<NotificationChannelTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<NotificationChannelTypeData>>();

        NotificationChannelTypeService._instance = this;
    }

    public static get Instance(): NotificationChannelTypeService {
      return NotificationChannelTypeService._instance;
    }


    public ClearListCaches(config: NotificationChannelTypeQueryParameters | null = null) {

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


    public ConvertToNotificationChannelTypeSubmitData(data: NotificationChannelTypeData): NotificationChannelTypeSubmitData {

        let output = new NotificationChannelTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.defaultPriority = data.defaultPriority;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetNotificationChannelType(id: bigint | number, includeRelations: boolean = true) : Observable<NotificationChannelTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const notificationChannelType$ = this.requestNotificationChannelType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get NotificationChannelType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, notificationChannelType$);

            return notificationChannelType$;
        }

        return this.recordCache.get(configHash) as Observable<NotificationChannelTypeData>;
    }

    private requestNotificationChannelType(id: bigint | number, includeRelations: boolean = true) : Observable<NotificationChannelTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<NotificationChannelTypeData>(this.baseUrl + 'api/NotificationChannelType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveNotificationChannelType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationChannelType(id, includeRelations));
            }));
    }

    public GetNotificationChannelTypeList(config: NotificationChannelTypeQueryParameters | any = null) : Observable<Array<NotificationChannelTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const notificationChannelTypeList$ = this.requestNotificationChannelTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get NotificationChannelType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, notificationChannelTypeList$);

            return notificationChannelTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<NotificationChannelTypeData>>;
    }


    private requestNotificationChannelTypeList(config: NotificationChannelTypeQueryParameters | any) : Observable <Array<NotificationChannelTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<NotificationChannelTypeData>>(this.baseUrl + 'api/NotificationChannelTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveNotificationChannelTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationChannelTypeList(config));
            }));
    }

    public GetNotificationChannelTypesRowCount(config: NotificationChannelTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const notificationChannelTypesRowCount$ = this.requestNotificationChannelTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get NotificationChannelTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, notificationChannelTypesRowCount$);

            return notificationChannelTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestNotificationChannelTypesRowCount(config: NotificationChannelTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/NotificationChannelTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationChannelTypesRowCount(config));
            }));
    }

    public GetNotificationChannelTypesBasicListData(config: NotificationChannelTypeQueryParameters | any = null) : Observable<Array<NotificationChannelTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const notificationChannelTypesBasicListData$ = this.requestNotificationChannelTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get NotificationChannelTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, notificationChannelTypesBasicListData$);

            return notificationChannelTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<NotificationChannelTypeBasicListData>>;
    }


    private requestNotificationChannelTypesBasicListData(config: NotificationChannelTypeQueryParameters | any) : Observable<Array<NotificationChannelTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<NotificationChannelTypeBasicListData>>(this.baseUrl + 'api/NotificationChannelTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationChannelTypesBasicListData(config));
            }));

    }


    public PutNotificationChannelType(id: bigint | number, notificationChannelType: NotificationChannelTypeSubmitData) : Observable<NotificationChannelTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<NotificationChannelTypeData>(this.baseUrl + 'api/NotificationChannelType/' + id.toString(), notificationChannelType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotificationChannelType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutNotificationChannelType(id, notificationChannelType));
            }));
    }


    public PostNotificationChannelType(notificationChannelType: NotificationChannelTypeSubmitData) : Observable<NotificationChannelTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<NotificationChannelTypeData>(this.baseUrl + 'api/NotificationChannelType', notificationChannelType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotificationChannelType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostNotificationChannelType(notificationChannelType));
            }));
    }

  
    public DeleteNotificationChannelType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/NotificationChannelType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteNotificationChannelType(id));
            }));
    }


    private getConfigHash(config: NotificationChannelTypeQueryParameters | any): string {

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

    public userIsAlertingNotificationChannelTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingNotificationChannelTypeReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.NotificationChannelTypes
        //
        if (userIsAlertingNotificationChannelTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingNotificationChannelTypeReader = user.readPermission >= 1;
            } else {
                userIsAlertingNotificationChannelTypeReader = false;
            }
        }

        return userIsAlertingNotificationChannelTypeReader;
    }


    public userIsAlertingNotificationChannelTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingNotificationChannelTypeWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.NotificationChannelTypes
        //
        if (userIsAlertingNotificationChannelTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingNotificationChannelTypeWriter = user.writePermission >= 255;
          } else {
            userIsAlertingNotificationChannelTypeWriter = false;
          }      
        }

        return userIsAlertingNotificationChannelTypeWriter;
    }

    public GetUserNotificationChannelPreferencesForNotificationChannelType(notificationChannelTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserNotificationChannelPreferenceData[]> {
        return this.userNotificationChannelPreferenceService.GetUserNotificationChannelPreferenceList({
            notificationChannelTypeId: notificationChannelTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetNotificationDeliveryAttemptsForNotificationChannelType(notificationChannelTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<NotificationDeliveryAttemptData[]> {
        return this.notificationDeliveryAttemptService.GetNotificationDeliveryAttemptList({
            notificationChannelTypeId: notificationChannelTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full NotificationChannelTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the NotificationChannelTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when NotificationChannelTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveNotificationChannelType(raw: any): NotificationChannelTypeData {
    if (!raw) return raw;

    //
    // Create a NotificationChannelTypeData object instance with correct prototype
    //
    const revived = Object.create(NotificationChannelTypeData.prototype) as NotificationChannelTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._userNotificationChannelPreferences = null;
    (revived as any)._userNotificationChannelPreferencesPromise = null;
    (revived as any)._userNotificationChannelPreferencesSubject = new BehaviorSubject<UserNotificationChannelPreferenceData[] | null>(null);

    (revived as any)._notificationDeliveryAttempts = null;
    (revived as any)._notificationDeliveryAttemptsPromise = null;
    (revived as any)._notificationDeliveryAttemptsSubject = new BehaviorSubject<NotificationDeliveryAttemptData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadNotificationChannelTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).UserNotificationChannelPreferences$ = (revived as any)._userNotificationChannelPreferencesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userNotificationChannelPreferences === null && (revived as any)._userNotificationChannelPreferencesPromise === null) {
                (revived as any).loadUserNotificationChannelPreferences();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).UserNotificationChannelPreferencesCount$ = UserNotificationChannelPreferenceService.Instance.GetUserNotificationChannelPreferencesRowCount({notificationChannelTypeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).NotificationDeliveryAttempts$ = (revived as any)._notificationDeliveryAttemptsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._notificationDeliveryAttempts === null && (revived as any)._notificationDeliveryAttemptsPromise === null) {
                (revived as any).loadNotificationDeliveryAttempts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).NotificationDeliveryAttemptsCount$ = NotificationDeliveryAttemptService.Instance.GetNotificationDeliveryAttemptsRowCount({notificationChannelTypeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveNotificationChannelTypeList(rawList: any[]): NotificationChannelTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveNotificationChannelType(raw));
  }

}
