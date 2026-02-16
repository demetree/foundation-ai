/*

   GENERATED SERVICE FOR THE INCIDENTNOTIFICATION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the IncidentNotification table.

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
import { IncidentData } from './incident.service';
import { EscalationRuleData } from './escalation-rule.service';
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
export class IncidentNotificationQueryParameters {
    incidentId: bigint | number | null | undefined = null;
    escalationRuleId: bigint | number | null | undefined = null;
    userObjectGuid: string | null | undefined = null;
    firstNotifiedAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    lastNotifiedAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    acknowledgedAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    acknowledgedByObjectGuid: string | null | undefined = null;
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
export class IncidentNotificationSubmitData {
    id!: bigint | number;
    incidentId!: bigint | number;
    escalationRuleId: bigint | number | null = null;
    userObjectGuid!: string;
    firstNotifiedAt!: string;      // ISO 8601 (full datetime)
    lastNotifiedAt: string | null = null;     // ISO 8601 (full datetime)
    acknowledgedAt: string | null = null;     // ISO 8601 (full datetime)
    acknowledgedByObjectGuid: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class IncidentNotificationBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. IncidentNotificationChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `incidentNotification.IncidentNotificationChildren$` — use with `| async` in templates
//        • Promise:    `incidentNotification.IncidentNotificationChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="incidentNotification.IncidentNotificationChildren$ | async"`), or
//        • Access the promise getter (`incidentNotification.IncidentNotificationChildren` or `await incidentNotification.IncidentNotificationChildren`)
//    - Simply reading `incidentNotification.IncidentNotificationChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await incidentNotification.Reload()` to refresh the entire object and clear all lazy caches.
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
export class IncidentNotificationData {
    id!: bigint | number;
    incidentId!: bigint | number;
    escalationRuleId!: bigint | number;
    userObjectGuid!: string;
    firstNotifiedAt!: string;      // ISO 8601 (full datetime)
    lastNotifiedAt!: string | null;   // ISO 8601 (full datetime)
    acknowledgedAt!: string | null;   // ISO 8601 (full datetime)
    acknowledgedByObjectGuid!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    escalationRule: EscalationRuleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    incident: IncidentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _notificationDeliveryAttempts: NotificationDeliveryAttemptData[] | null = null;
    private _notificationDeliveryAttemptsPromise: Promise<NotificationDeliveryAttemptData[]> | null  = null;
    private _notificationDeliveryAttemptsSubject = new BehaviorSubject<NotificationDeliveryAttemptData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public NotificationDeliveryAttempts$ = this._notificationDeliveryAttemptsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._notificationDeliveryAttempts === null && this._notificationDeliveryAttemptsPromise === null) {
            this.loadNotificationDeliveryAttempts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public NotificationDeliveryAttemptsCount$ = NotificationDeliveryAttemptService.Instance.GetNotificationDeliveryAttemptsRowCount({incidentNotificationId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any IncidentNotificationData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.incidentNotification.Reload();
  //
  //  Non Async:
  //
  //     incidentNotification[0].Reload().then(x => {
  //        this.incidentNotification = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      IncidentNotificationService.Instance.GetIncidentNotification(this.id, includeRelations)
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
     * Gets the NotificationDeliveryAttempts for this IncidentNotification.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.incidentNotification.NotificationDeliveryAttempts.then(incidentNotifications => { ... })
     *   or
     *   await this.incidentNotification.incidentNotifications
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
            IncidentNotificationService.Instance.GetNotificationDeliveryAttemptsForIncidentNotification(this.id)
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
     * Updates the state of this IncidentNotificationData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this IncidentNotificationData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): IncidentNotificationSubmitData {
        return IncidentNotificationService.Instance.ConvertToIncidentNotificationSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class IncidentNotificationService extends SecureEndpointBase {

    private static _instance: IncidentNotificationService;
    private listCache: Map<string, Observable<Array<IncidentNotificationData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<IncidentNotificationBasicListData>>>;
    private recordCache: Map<string, Observable<IncidentNotificationData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private notificationDeliveryAttemptService: NotificationDeliveryAttemptService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<IncidentNotificationData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<IncidentNotificationBasicListData>>>();
        this.recordCache = new Map<string, Observable<IncidentNotificationData>>();

        IncidentNotificationService._instance = this;
    }

    public static get Instance(): IncidentNotificationService {
      return IncidentNotificationService._instance;
    }


    public ClearListCaches(config: IncidentNotificationQueryParameters | null = null) {

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


    public ConvertToIncidentNotificationSubmitData(data: IncidentNotificationData): IncidentNotificationSubmitData {

        let output = new IncidentNotificationSubmitData();

        output.id = data.id;
        output.incidentId = data.incidentId;
        output.escalationRuleId = data.escalationRuleId;
        output.userObjectGuid = data.userObjectGuid;
        output.firstNotifiedAt = data.firstNotifiedAt;
        output.lastNotifiedAt = data.lastNotifiedAt;
        output.acknowledgedAt = data.acknowledgedAt;
        output.acknowledgedByObjectGuid = data.acknowledgedByObjectGuid;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetIncidentNotification(id: bigint | number, includeRelations: boolean = true) : Observable<IncidentNotificationData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const incidentNotification$ = this.requestIncidentNotification(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IncidentNotification", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, incidentNotification$);

            return incidentNotification$;
        }

        return this.recordCache.get(configHash) as Observable<IncidentNotificationData>;
    }

    private requestIncidentNotification(id: bigint | number, includeRelations: boolean = true) : Observable<IncidentNotificationData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentNotificationData>(this.baseUrl + 'api/IncidentNotification/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveIncidentNotification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentNotification(id, includeRelations));
            }));
    }

    public GetIncidentNotificationList(config: IncidentNotificationQueryParameters | any = null) : Observable<Array<IncidentNotificationData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const incidentNotificationList$ = this.requestIncidentNotificationList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IncidentNotification list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, incidentNotificationList$);

            return incidentNotificationList$;
        }

        return this.listCache.get(configHash) as Observable<Array<IncidentNotificationData>>;
    }


    private requestIncidentNotificationList(config: IncidentNotificationQueryParameters | any) : Observable <Array<IncidentNotificationData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IncidentNotificationData>>(this.baseUrl + 'api/IncidentNotifications', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveIncidentNotificationList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentNotificationList(config));
            }));
    }

    public GetIncidentNotificationsRowCount(config: IncidentNotificationQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const incidentNotificationsRowCount$ = this.requestIncidentNotificationsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IncidentNotifications row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, incidentNotificationsRowCount$);

            return incidentNotificationsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestIncidentNotificationsRowCount(config: IncidentNotificationQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/IncidentNotifications/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentNotificationsRowCount(config));
            }));
    }

    public GetIncidentNotificationsBasicListData(config: IncidentNotificationQueryParameters | any = null) : Observable<Array<IncidentNotificationBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const incidentNotificationsBasicListData$ = this.requestIncidentNotificationsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IncidentNotifications basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, incidentNotificationsBasicListData$);

            return incidentNotificationsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<IncidentNotificationBasicListData>>;
    }


    private requestIncidentNotificationsBasicListData(config: IncidentNotificationQueryParameters | any) : Observable<Array<IncidentNotificationBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IncidentNotificationBasicListData>>(this.baseUrl + 'api/IncidentNotifications/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentNotificationsBasicListData(config));
            }));

    }


    public PutIncidentNotification(id: bigint | number, incidentNotification: IncidentNotificationSubmitData) : Observable<IncidentNotificationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IncidentNotificationData>(this.baseUrl + 'api/IncidentNotification/' + id.toString(), incidentNotification, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncidentNotification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutIncidentNotification(id, incidentNotification));
            }));
    }


    public PostIncidentNotification(incidentNotification: IncidentNotificationSubmitData) : Observable<IncidentNotificationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<IncidentNotificationData>(this.baseUrl + 'api/IncidentNotification', incidentNotification, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncidentNotification(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostIncidentNotification(incidentNotification));
            }));
    }

  
    public DeleteIncidentNotification(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/IncidentNotification/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteIncidentNotification(id));
            }));
    }


    private getConfigHash(config: IncidentNotificationQueryParameters | any): string {

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

    public userIsAlertingIncidentNotificationReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingIncidentNotificationReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.IncidentNotifications
        //
        if (userIsAlertingIncidentNotificationReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingIncidentNotificationReader = user.readPermission >= 1;
            } else {
                userIsAlertingIncidentNotificationReader = false;
            }
        }

        return userIsAlertingIncidentNotificationReader;
    }


    public userIsAlertingIncidentNotificationWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingIncidentNotificationWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.IncidentNotifications
        //
        if (userIsAlertingIncidentNotificationWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingIncidentNotificationWriter = user.writePermission >= 255;
          } else {
            userIsAlertingIncidentNotificationWriter = false;
          }      
        }

        return userIsAlertingIncidentNotificationWriter;
    }

    public GetNotificationDeliveryAttemptsForIncidentNotification(incidentNotificationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<NotificationDeliveryAttemptData[]> {
        return this.notificationDeliveryAttemptService.GetNotificationDeliveryAttemptList({
            incidentNotificationId: incidentNotificationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full IncidentNotificationData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the IncidentNotificationData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when IncidentNotificationTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveIncidentNotification(raw: any): IncidentNotificationData {
    if (!raw) return raw;

    //
    // Create a IncidentNotificationData object instance with correct prototype
    //
    const revived = Object.create(IncidentNotificationData.prototype) as IncidentNotificationData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
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
    // 2. But private methods (loadIncidentNotificationXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).NotificationDeliveryAttempts$ = (revived as any)._notificationDeliveryAttemptsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._notificationDeliveryAttempts === null && (revived as any)._notificationDeliveryAttemptsPromise === null) {
                (revived as any).loadNotificationDeliveryAttempts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).NotificationDeliveryAttemptsCount$ = NotificationDeliveryAttemptService.Instance.GetNotificationDeliveryAttemptsRowCount({incidentNotificationId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveIncidentNotificationList(rawList: any[]): IncidentNotificationData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveIncidentNotification(raw));
  }

}
