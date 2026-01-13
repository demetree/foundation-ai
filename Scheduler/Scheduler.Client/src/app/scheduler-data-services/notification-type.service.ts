/*

   GENERATED SERVICE FOR THE NOTIFICATIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the NotificationType table.

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
import { NotificationSubscriptionService, NotificationSubscriptionData } from './notification-subscription.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class NotificationTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class NotificationTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class NotificationTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. NotificationTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `notificationType.NotificationTypeChildren$` — use with `| async` in templates
//        • Promise:    `notificationType.NotificationTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="notificationType.NotificationTypeChildren$ | async"`), or
//        • Access the promise getter (`notificationType.NotificationTypeChildren` or `await notificationType.NotificationTypeChildren`)
//    - Simply reading `notificationType.NotificationTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await notificationType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class NotificationTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence!: bigint | number;
    color!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _notificationSubscriptions: NotificationSubscriptionData[] | null = null;
    private _notificationSubscriptionsPromise: Promise<NotificationSubscriptionData[]> | null  = null;
    private _notificationSubscriptionsSubject = new BehaviorSubject<NotificationSubscriptionData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public NotificationSubscriptions$ = this._notificationSubscriptionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._notificationSubscriptions === null && this._notificationSubscriptionsPromise === null) {
            this.loadNotificationSubscriptions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public NotificationSubscriptionsCount$ = NotificationSubscriptionService.Instance.GetNotificationSubscriptionsRowCount({notificationTypeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any NotificationTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.notificationType.Reload();
  //
  //  Non Async:
  //
  //     notificationType[0].Reload().then(x => {
  //        this.notificationType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      NotificationTypeService.Instance.GetNotificationType(this.id, includeRelations)
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
     this._notificationSubscriptions = null;
     this._notificationSubscriptionsPromise = null;
     this._notificationSubscriptionsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the NotificationSubscriptions for this NotificationType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.notificationType.NotificationSubscriptions.then(notificationTypes => { ... })
     *   or
     *   await this.notificationType.notificationTypes
     *
    */
    public get NotificationSubscriptions(): Promise<NotificationSubscriptionData[]> {
        if (this._notificationSubscriptions !== null) {
            return Promise.resolve(this._notificationSubscriptions);
        }

        if (this._notificationSubscriptionsPromise !== null) {
            return this._notificationSubscriptionsPromise;
        }

        // Start the load
        this.loadNotificationSubscriptions();

        return this._notificationSubscriptionsPromise!;
    }



    private loadNotificationSubscriptions(): void {

        this._notificationSubscriptionsPromise = lastValueFrom(
            NotificationTypeService.Instance.GetNotificationSubscriptionsForNotificationType(this.id)
        )
        .then(NotificationSubscriptions => {
            this._notificationSubscriptions = NotificationSubscriptions ?? [];
            this._notificationSubscriptionsSubject.next(this._notificationSubscriptions);
            return this._notificationSubscriptions;
         })
        .catch(err => {
            this._notificationSubscriptions = [];
            this._notificationSubscriptionsSubject.next(this._notificationSubscriptions);
            throw err;
        })
        .finally(() => {
            this._notificationSubscriptionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached NotificationSubscription. Call after mutations to force refresh.
     */
    public ClearNotificationSubscriptionsCache(): void {
        this._notificationSubscriptions = null;
        this._notificationSubscriptionsPromise = null;
        this._notificationSubscriptionsSubject.next(this._notificationSubscriptions);      // Emit to observable
    }

    public get HasNotificationSubscriptions(): Promise<boolean> {
        return this.NotificationSubscriptions.then(notificationSubscriptions => notificationSubscriptions.length > 0);
    }




    /**
     * Updates the state of this NotificationTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this NotificationTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): NotificationTypeSubmitData {
        return NotificationTypeService.Instance.ConvertToNotificationTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class NotificationTypeService extends SecureEndpointBase {

    private static _instance: NotificationTypeService;
    private listCache: Map<string, Observable<Array<NotificationTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<NotificationTypeBasicListData>>>;
    private recordCache: Map<string, Observable<NotificationTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private notificationSubscriptionService: NotificationSubscriptionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<NotificationTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<NotificationTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<NotificationTypeData>>();

        NotificationTypeService._instance = this;
    }

    public static get Instance(): NotificationTypeService {
      return NotificationTypeService._instance;
    }


    public ClearListCaches(config: NotificationTypeQueryParameters | null = null) {

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


    public ConvertToNotificationTypeSubmitData(data: NotificationTypeData): NotificationTypeSubmitData {

        let output = new NotificationTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetNotificationType(id: bigint | number, includeRelations: boolean = true) : Observable<NotificationTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const notificationType$ = this.requestNotificationType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get NotificationType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, notificationType$);

            return notificationType$;
        }

        return this.recordCache.get(configHash) as Observable<NotificationTypeData>;
    }

    private requestNotificationType(id: bigint | number, includeRelations: boolean = true) : Observable<NotificationTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<NotificationTypeData>(this.baseUrl + 'api/NotificationType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveNotificationType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationType(id, includeRelations));
            }));
    }

    public GetNotificationTypeList(config: NotificationTypeQueryParameters | any = null) : Observable<Array<NotificationTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const notificationTypeList$ = this.requestNotificationTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get NotificationType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, notificationTypeList$);

            return notificationTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<NotificationTypeData>>;
    }


    private requestNotificationTypeList(config: NotificationTypeQueryParameters | any) : Observable <Array<NotificationTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<NotificationTypeData>>(this.baseUrl + 'api/NotificationTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveNotificationTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationTypeList(config));
            }));
    }

    public GetNotificationTypesRowCount(config: NotificationTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const notificationTypesRowCount$ = this.requestNotificationTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get NotificationTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, notificationTypesRowCount$);

            return notificationTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestNotificationTypesRowCount(config: NotificationTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/NotificationTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationTypesRowCount(config));
            }));
    }

    public GetNotificationTypesBasicListData(config: NotificationTypeQueryParameters | any = null) : Observable<Array<NotificationTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const notificationTypesBasicListData$ = this.requestNotificationTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get NotificationTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, notificationTypesBasicListData$);

            return notificationTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<NotificationTypeBasicListData>>;
    }


    private requestNotificationTypesBasicListData(config: NotificationTypeQueryParameters | any) : Observable<Array<NotificationTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<NotificationTypeBasicListData>>(this.baseUrl + 'api/NotificationTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationTypesBasicListData(config));
            }));

    }


    public PutNotificationType(id: bigint | number, notificationType: NotificationTypeSubmitData) : Observable<NotificationTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<NotificationTypeData>(this.baseUrl + 'api/NotificationType/' + id.toString(), notificationType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotificationType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutNotificationType(id, notificationType));
            }));
    }


    public PostNotificationType(notificationType: NotificationTypeSubmitData) : Observable<NotificationTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<NotificationTypeData>(this.baseUrl + 'api/NotificationType', notificationType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotificationType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostNotificationType(notificationType));
            }));
    }

  
    public DeleteNotificationType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/NotificationType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteNotificationType(id));
            }));
    }


    private getConfigHash(config: NotificationTypeQueryParameters | any): string {

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

    public userIsSchedulerNotificationTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerNotificationTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.NotificationTypes
        //
        if (userIsSchedulerNotificationTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerNotificationTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerNotificationTypeReader = false;
            }
        }

        return userIsSchedulerNotificationTypeReader;
    }


    public userIsSchedulerNotificationTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerNotificationTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.NotificationTypes
        //
        if (userIsSchedulerNotificationTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerNotificationTypeWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerNotificationTypeWriter = false;
          }      
        }

        return userIsSchedulerNotificationTypeWriter;
    }

    public GetNotificationSubscriptionsForNotificationType(notificationTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<NotificationSubscriptionData[]> {
        return this.notificationSubscriptionService.GetNotificationSubscriptionList({
            notificationTypeId: notificationTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full NotificationTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the NotificationTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when NotificationTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveNotificationType(raw: any): NotificationTypeData {
    if (!raw) return raw;

    //
    // Create a NotificationTypeData object instance with correct prototype
    //
    const revived = Object.create(NotificationTypeData.prototype) as NotificationTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._notificationSubscriptions = null;
    (revived as any)._notificationSubscriptionsPromise = null;
    (revived as any)._notificationSubscriptionsSubject = new BehaviorSubject<NotificationSubscriptionData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadNotificationTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).NotificationSubscriptions$ = (revived as any)._notificationSubscriptionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._notificationSubscriptions === null && (revived as any)._notificationSubscriptionsPromise === null) {
                (revived as any).loadNotificationSubscriptions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).NotificationSubscriptionsCount$ = NotificationSubscriptionService.Instance.GetNotificationSubscriptionsRowCount({notificationTypeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveNotificationTypeList(rawList: any[]): NotificationTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveNotificationType(raw));
  }

}
