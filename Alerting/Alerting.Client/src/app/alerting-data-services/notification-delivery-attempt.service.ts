/*

   GENERATED SERVICE FOR THE NOTIFICATIONDELIVERYATTEMPT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the NotificationDeliveryAttempt table.

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
import { IncidentNotificationData } from './incident-notification.service';
import { NotificationChannelTypeData } from './notification-channel-type.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class NotificationDeliveryAttemptQueryParameters {
    incidentNotificationId: bigint | number | null | undefined = null;
    notificationChannelTypeId: bigint | number | null | undefined = null;
    attemptNumber: bigint | number | null | undefined = null;
    attemptedAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    status: string | null | undefined = null;
    errorMessage: string | null | undefined = null;
    response: string | null | undefined = null;
    recipientAddress: string | null | undefined = null;
    subject: string | null | undefined = null;
    bodyContent: string | null | undefined = null;
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
export class NotificationDeliveryAttemptSubmitData {
    id!: bigint | number;
    incidentNotificationId!: bigint | number;
    notificationChannelTypeId!: bigint | number;
    attemptNumber!: bigint | number;
    attemptedAt!: string;      // ISO 8601 (full datetime)
    status!: string;
    errorMessage: string | null = null;
    response: string | null = null;
    recipientAddress: string | null = null;
    subject: string | null = null;
    bodyContent: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class NotificationDeliveryAttemptBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. NotificationDeliveryAttemptChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `notificationDeliveryAttempt.NotificationDeliveryAttemptChildren$` — use with `| async` in templates
//        • Promise:    `notificationDeliveryAttempt.NotificationDeliveryAttemptChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="notificationDeliveryAttempt.NotificationDeliveryAttemptChildren$ | async"`), or
//        • Access the promise getter (`notificationDeliveryAttempt.NotificationDeliveryAttemptChildren` or `await notificationDeliveryAttempt.NotificationDeliveryAttemptChildren`)
//    - Simply reading `notificationDeliveryAttempt.NotificationDeliveryAttemptChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await notificationDeliveryAttempt.Reload()` to refresh the entire object and clear all lazy caches.
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
export class NotificationDeliveryAttemptData {
    id!: bigint | number;
    incidentNotificationId!: bigint | number;
    notificationChannelTypeId!: bigint | number;
    attemptNumber!: bigint | number;
    attemptedAt!: string;      // ISO 8601 (full datetime)
    status!: string;
    errorMessage!: string | null;
    response!: string | null;
    recipientAddress!: string | null;
    subject!: string | null;
    bodyContent!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    incidentNotification: IncidentNotificationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    notificationChannelType: NotificationChannelTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any NotificationDeliveryAttemptData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.notificationDeliveryAttempt.Reload();
  //
  //  Non Async:
  //
  //     notificationDeliveryAttempt[0].Reload().then(x => {
  //        this.notificationDeliveryAttempt = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      NotificationDeliveryAttemptService.Instance.GetNotificationDeliveryAttempt(this.id, includeRelations)
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
     * Updates the state of this NotificationDeliveryAttemptData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this NotificationDeliveryAttemptData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): NotificationDeliveryAttemptSubmitData {
        return NotificationDeliveryAttemptService.Instance.ConvertToNotificationDeliveryAttemptSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class NotificationDeliveryAttemptService extends SecureEndpointBase {

    private static _instance: NotificationDeliveryAttemptService;
    private listCache: Map<string, Observable<Array<NotificationDeliveryAttemptData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<NotificationDeliveryAttemptBasicListData>>>;
    private recordCache: Map<string, Observable<NotificationDeliveryAttemptData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<NotificationDeliveryAttemptData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<NotificationDeliveryAttemptBasicListData>>>();
        this.recordCache = new Map<string, Observable<NotificationDeliveryAttemptData>>();

        NotificationDeliveryAttemptService._instance = this;
    }

    public static get Instance(): NotificationDeliveryAttemptService {
      return NotificationDeliveryAttemptService._instance;
    }


    public ClearListCaches(config: NotificationDeliveryAttemptQueryParameters | null = null) {

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


    public ConvertToNotificationDeliveryAttemptSubmitData(data: NotificationDeliveryAttemptData): NotificationDeliveryAttemptSubmitData {

        let output = new NotificationDeliveryAttemptSubmitData();

        output.id = data.id;
        output.incidentNotificationId = data.incidentNotificationId;
        output.notificationChannelTypeId = data.notificationChannelTypeId;
        output.attemptNumber = data.attemptNumber;
        output.attemptedAt = data.attemptedAt;
        output.status = data.status;
        output.errorMessage = data.errorMessage;
        output.response = data.response;
        output.recipientAddress = data.recipientAddress;
        output.subject = data.subject;
        output.bodyContent = data.bodyContent;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetNotificationDeliveryAttempt(id: bigint | number, includeRelations: boolean = true) : Observable<NotificationDeliveryAttemptData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const notificationDeliveryAttempt$ = this.requestNotificationDeliveryAttempt(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get NotificationDeliveryAttempt", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, notificationDeliveryAttempt$);

            return notificationDeliveryAttempt$;
        }

        return this.recordCache.get(configHash) as Observable<NotificationDeliveryAttemptData>;
    }

    private requestNotificationDeliveryAttempt(id: bigint | number, includeRelations: boolean = true) : Observable<NotificationDeliveryAttemptData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<NotificationDeliveryAttemptData>(this.baseUrl + 'api/NotificationDeliveryAttempt/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveNotificationDeliveryAttempt(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationDeliveryAttempt(id, includeRelations));
            }));
    }

    public GetNotificationDeliveryAttemptList(config: NotificationDeliveryAttemptQueryParameters | any = null) : Observable<Array<NotificationDeliveryAttemptData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const notificationDeliveryAttemptList$ = this.requestNotificationDeliveryAttemptList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get NotificationDeliveryAttempt list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, notificationDeliveryAttemptList$);

            return notificationDeliveryAttemptList$;
        }

        return this.listCache.get(configHash) as Observable<Array<NotificationDeliveryAttemptData>>;
    }


    private requestNotificationDeliveryAttemptList(config: NotificationDeliveryAttemptQueryParameters | any) : Observable <Array<NotificationDeliveryAttemptData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<NotificationDeliveryAttemptData>>(this.baseUrl + 'api/NotificationDeliveryAttempts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveNotificationDeliveryAttemptList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationDeliveryAttemptList(config));
            }));
    }

    public GetNotificationDeliveryAttemptsRowCount(config: NotificationDeliveryAttemptQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const notificationDeliveryAttemptsRowCount$ = this.requestNotificationDeliveryAttemptsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get NotificationDeliveryAttempts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, notificationDeliveryAttemptsRowCount$);

            return notificationDeliveryAttemptsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestNotificationDeliveryAttemptsRowCount(config: NotificationDeliveryAttemptQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/NotificationDeliveryAttempts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationDeliveryAttemptsRowCount(config));
            }));
    }

    public GetNotificationDeliveryAttemptsBasicListData(config: NotificationDeliveryAttemptQueryParameters | any = null) : Observable<Array<NotificationDeliveryAttemptBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const notificationDeliveryAttemptsBasicListData$ = this.requestNotificationDeliveryAttemptsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get NotificationDeliveryAttempts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, notificationDeliveryAttemptsBasicListData$);

            return notificationDeliveryAttemptsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<NotificationDeliveryAttemptBasicListData>>;
    }


    private requestNotificationDeliveryAttemptsBasicListData(config: NotificationDeliveryAttemptQueryParameters | any) : Observable<Array<NotificationDeliveryAttemptBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<NotificationDeliveryAttemptBasicListData>>(this.baseUrl + 'api/NotificationDeliveryAttempts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationDeliveryAttemptsBasicListData(config));
            }));

    }


    public PutNotificationDeliveryAttempt(id: bigint | number, notificationDeliveryAttempt: NotificationDeliveryAttemptSubmitData) : Observable<NotificationDeliveryAttemptData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<NotificationDeliveryAttemptData>(this.baseUrl + 'api/NotificationDeliveryAttempt/' + id.toString(), notificationDeliveryAttempt, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotificationDeliveryAttempt(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutNotificationDeliveryAttempt(id, notificationDeliveryAttempt));
            }));
    }


    public PostNotificationDeliveryAttempt(notificationDeliveryAttempt: NotificationDeliveryAttemptSubmitData) : Observable<NotificationDeliveryAttemptData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<NotificationDeliveryAttemptData>(this.baseUrl + 'api/NotificationDeliveryAttempt', notificationDeliveryAttempt, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotificationDeliveryAttempt(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostNotificationDeliveryAttempt(notificationDeliveryAttempt));
            }));
    }

  
    public DeleteNotificationDeliveryAttempt(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/NotificationDeliveryAttempt/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteNotificationDeliveryAttempt(id));
            }));
    }


    private getConfigHash(config: NotificationDeliveryAttemptQueryParameters | any): string {

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

    public userIsAlertingNotificationDeliveryAttemptReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingNotificationDeliveryAttemptReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.NotificationDeliveryAttempts
        //
        if (userIsAlertingNotificationDeliveryAttemptReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingNotificationDeliveryAttemptReader = user.readPermission >= 1;
            } else {
                userIsAlertingNotificationDeliveryAttemptReader = false;
            }
        }

        return userIsAlertingNotificationDeliveryAttemptReader;
    }


    public userIsAlertingNotificationDeliveryAttemptWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingNotificationDeliveryAttemptWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.NotificationDeliveryAttempts
        //
        if (userIsAlertingNotificationDeliveryAttemptWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingNotificationDeliveryAttemptWriter = user.writePermission >= 255;
          } else {
            userIsAlertingNotificationDeliveryAttemptWriter = false;
          }      
        }

        return userIsAlertingNotificationDeliveryAttemptWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full NotificationDeliveryAttemptData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the NotificationDeliveryAttemptData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when NotificationDeliveryAttemptTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveNotificationDeliveryAttempt(raw: any): NotificationDeliveryAttemptData {
    if (!raw) return raw;

    //
    // Create a NotificationDeliveryAttemptData object instance with correct prototype
    //
    const revived = Object.create(NotificationDeliveryAttemptData.prototype) as NotificationDeliveryAttemptData;

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
    // 2. But private methods (loadNotificationDeliveryAttemptXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveNotificationDeliveryAttemptList(rawList: any[]): NotificationDeliveryAttemptData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveNotificationDeliveryAttempt(raw));
  }

}
