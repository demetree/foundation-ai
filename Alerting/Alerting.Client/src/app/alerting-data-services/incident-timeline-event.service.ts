/*

   GENERATED SERVICE FOR THE INCIDENTTIMELINEEVENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the IncidentTimelineEvent table.

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
import { IncidentEventTypeData } from './incident-event-type.service';
import { WebhookDeliveryAttemptService, WebhookDeliveryAttemptData } from './webhook-delivery-attempt.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class IncidentTimelineEventQueryParameters {
    incidentId: bigint | number | null | undefined = null;
    incidentEventTypeId: bigint | number | null | undefined = null;
    timestamp: string | null | undefined = null;        // ISO 8601
    actorObjectGuid: string | null | undefined = null;
    detailsJson: string | null | undefined = null;
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
export class IncidentTimelineEventSubmitData {
    id!: bigint | number;
    incidentId!: bigint | number;
    incidentEventTypeId!: bigint | number;
    timestamp!: string;      // ISO 8601
    actorObjectGuid: string | null = null;
    detailsJson: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class IncidentTimelineEventBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. IncidentTimelineEventChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `incidentTimelineEvent.IncidentTimelineEventChildren$` — use with `| async` in templates
//        • Promise:    `incidentTimelineEvent.IncidentTimelineEventChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="incidentTimelineEvent.IncidentTimelineEventChildren$ | async"`), or
//        • Access the promise getter (`incidentTimelineEvent.IncidentTimelineEventChildren` or `await incidentTimelineEvent.IncidentTimelineEventChildren`)
//    - Simply reading `incidentTimelineEvent.IncidentTimelineEventChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await incidentTimelineEvent.Reload()` to refresh the entire object and clear all lazy caches.
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
export class IncidentTimelineEventData {
    id!: bigint | number;
    incidentId!: bigint | number;
    incidentEventTypeId!: bigint | number;
    timestamp!: string;      // ISO 8601
    actorObjectGuid!: string | null;
    detailsJson!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    incident: IncidentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    incidentEventType: IncidentEventTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _webhookDeliveryAttempts: WebhookDeliveryAttemptData[] | null = null;
    private _webhookDeliveryAttemptsPromise: Promise<WebhookDeliveryAttemptData[]> | null  = null;
    private _webhookDeliveryAttemptsSubject = new BehaviorSubject<WebhookDeliveryAttemptData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public WebhookDeliveryAttempts$ = this._webhookDeliveryAttemptsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._webhookDeliveryAttempts === null && this._webhookDeliveryAttemptsPromise === null) {
            this.loadWebhookDeliveryAttempts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public WebhookDeliveryAttemptsCount$ = WebhookDeliveryAttemptService.Instance.GetWebhookDeliveryAttemptsRowCount({incidentTimelineEventId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any IncidentTimelineEventData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.incidentTimelineEvent.Reload();
  //
  //  Non Async:
  //
  //     incidentTimelineEvent[0].Reload().then(x => {
  //        this.incidentTimelineEvent = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      IncidentTimelineEventService.Instance.GetIncidentTimelineEvent(this.id, includeRelations)
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
     this._webhookDeliveryAttempts = null;
     this._webhookDeliveryAttemptsPromise = null;
     this._webhookDeliveryAttemptsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the WebhookDeliveryAttempts for this IncidentTimelineEvent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.incidentTimelineEvent.WebhookDeliveryAttempts.then(incidentTimelineEvents => { ... })
     *   or
     *   await this.incidentTimelineEvent.incidentTimelineEvents
     *
    */
    public get WebhookDeliveryAttempts(): Promise<WebhookDeliveryAttemptData[]> {
        if (this._webhookDeliveryAttempts !== null) {
            return Promise.resolve(this._webhookDeliveryAttempts);
        }

        if (this._webhookDeliveryAttemptsPromise !== null) {
            return this._webhookDeliveryAttemptsPromise;
        }

        // Start the load
        this.loadWebhookDeliveryAttempts();

        return this._webhookDeliveryAttemptsPromise!;
    }



    private loadWebhookDeliveryAttempts(): void {

        this._webhookDeliveryAttemptsPromise = lastValueFrom(
            IncidentTimelineEventService.Instance.GetWebhookDeliveryAttemptsForIncidentTimelineEvent(this.id)
        )
        .then(WebhookDeliveryAttempts => {
            this._webhookDeliveryAttempts = WebhookDeliveryAttempts ?? [];
            this._webhookDeliveryAttemptsSubject.next(this._webhookDeliveryAttempts);
            return this._webhookDeliveryAttempts;
         })
        .catch(err => {
            this._webhookDeliveryAttempts = [];
            this._webhookDeliveryAttemptsSubject.next(this._webhookDeliveryAttempts);
            throw err;
        })
        .finally(() => {
            this._webhookDeliveryAttemptsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached WebhookDeliveryAttempt. Call after mutations to force refresh.
     */
    public ClearWebhookDeliveryAttemptsCache(): void {
        this._webhookDeliveryAttempts = null;
        this._webhookDeliveryAttemptsPromise = null;
        this._webhookDeliveryAttemptsSubject.next(this._webhookDeliveryAttempts);      // Emit to observable
    }

    public get HasWebhookDeliveryAttempts(): Promise<boolean> {
        return this.WebhookDeliveryAttempts.then(webhookDeliveryAttempts => webhookDeliveryAttempts.length > 0);
    }




    /**
     * Updates the state of this IncidentTimelineEventData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this IncidentTimelineEventData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): IncidentTimelineEventSubmitData {
        return IncidentTimelineEventService.Instance.ConvertToIncidentTimelineEventSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class IncidentTimelineEventService extends SecureEndpointBase {

    private static _instance: IncidentTimelineEventService;
    private listCache: Map<string, Observable<Array<IncidentTimelineEventData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<IncidentTimelineEventBasicListData>>>;
    private recordCache: Map<string, Observable<IncidentTimelineEventData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private webhookDeliveryAttemptService: WebhookDeliveryAttemptService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<IncidentTimelineEventData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<IncidentTimelineEventBasicListData>>>();
        this.recordCache = new Map<string, Observable<IncidentTimelineEventData>>();

        IncidentTimelineEventService._instance = this;
    }

    public static get Instance(): IncidentTimelineEventService {
      return IncidentTimelineEventService._instance;
    }


    public ClearListCaches(config: IncidentTimelineEventQueryParameters | null = null) {

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


    public ConvertToIncidentTimelineEventSubmitData(data: IncidentTimelineEventData): IncidentTimelineEventSubmitData {

        let output = new IncidentTimelineEventSubmitData();

        output.id = data.id;
        output.incidentId = data.incidentId;
        output.incidentEventTypeId = data.incidentEventTypeId;
        output.timestamp = data.timestamp;
        output.actorObjectGuid = data.actorObjectGuid;
        output.detailsJson = data.detailsJson;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetIncidentTimelineEvent(id: bigint | number, includeRelations: boolean = true) : Observable<IncidentTimelineEventData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const incidentTimelineEvent$ = this.requestIncidentTimelineEvent(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IncidentTimelineEvent", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, incidentTimelineEvent$);

            return incidentTimelineEvent$;
        }

        return this.recordCache.get(configHash) as Observable<IncidentTimelineEventData>;
    }

    private requestIncidentTimelineEvent(id: bigint | number, includeRelations: boolean = true) : Observable<IncidentTimelineEventData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentTimelineEventData>(this.baseUrl + 'api/IncidentTimelineEvent/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveIncidentTimelineEvent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentTimelineEvent(id, includeRelations));
            }));
    }

    public GetIncidentTimelineEventList(config: IncidentTimelineEventQueryParameters | any = null) : Observable<Array<IncidentTimelineEventData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const incidentTimelineEventList$ = this.requestIncidentTimelineEventList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IncidentTimelineEvent list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, incidentTimelineEventList$);

            return incidentTimelineEventList$;
        }

        return this.listCache.get(configHash) as Observable<Array<IncidentTimelineEventData>>;
    }


    private requestIncidentTimelineEventList(config: IncidentTimelineEventQueryParameters | any) : Observable <Array<IncidentTimelineEventData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IncidentTimelineEventData>>(this.baseUrl + 'api/IncidentTimelineEvents', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveIncidentTimelineEventList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentTimelineEventList(config));
            }));
    }

    public GetIncidentTimelineEventsRowCount(config: IncidentTimelineEventQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const incidentTimelineEventsRowCount$ = this.requestIncidentTimelineEventsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IncidentTimelineEvents row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, incidentTimelineEventsRowCount$);

            return incidentTimelineEventsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestIncidentTimelineEventsRowCount(config: IncidentTimelineEventQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/IncidentTimelineEvents/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentTimelineEventsRowCount(config));
            }));
    }

    public GetIncidentTimelineEventsBasicListData(config: IncidentTimelineEventQueryParameters | any = null) : Observable<Array<IncidentTimelineEventBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const incidentTimelineEventsBasicListData$ = this.requestIncidentTimelineEventsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IncidentTimelineEvents basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, incidentTimelineEventsBasicListData$);

            return incidentTimelineEventsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<IncidentTimelineEventBasicListData>>;
    }


    private requestIncidentTimelineEventsBasicListData(config: IncidentTimelineEventQueryParameters | any) : Observable<Array<IncidentTimelineEventBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IncidentTimelineEventBasicListData>>(this.baseUrl + 'api/IncidentTimelineEvents/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentTimelineEventsBasicListData(config));
            }));

    }


    public PutIncidentTimelineEvent(id: bigint | number, incidentTimelineEvent: IncidentTimelineEventSubmitData) : Observable<IncidentTimelineEventData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IncidentTimelineEventData>(this.baseUrl + 'api/IncidentTimelineEvent/' + id.toString(), incidentTimelineEvent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncidentTimelineEvent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutIncidentTimelineEvent(id, incidentTimelineEvent));
            }));
    }


    public PostIncidentTimelineEvent(incidentTimelineEvent: IncidentTimelineEventSubmitData) : Observable<IncidentTimelineEventData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<IncidentTimelineEventData>(this.baseUrl + 'api/IncidentTimelineEvent', incidentTimelineEvent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncidentTimelineEvent(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostIncidentTimelineEvent(incidentTimelineEvent));
            }));
    }

  
    public DeleteIncidentTimelineEvent(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/IncidentTimelineEvent/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteIncidentTimelineEvent(id));
            }));
    }


    private getConfigHash(config: IncidentTimelineEventQueryParameters | any): string {

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

    public userIsAlertingIncidentTimelineEventReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingIncidentTimelineEventReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.IncidentTimelineEvents
        //
        if (userIsAlertingIncidentTimelineEventReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingIncidentTimelineEventReader = user.readPermission >= 1;
            } else {
                userIsAlertingIncidentTimelineEventReader = false;
            }
        }

        return userIsAlertingIncidentTimelineEventReader;
    }


    public userIsAlertingIncidentTimelineEventWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingIncidentTimelineEventWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.IncidentTimelineEvents
        //
        if (userIsAlertingIncidentTimelineEventWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingIncidentTimelineEventWriter = user.writePermission >= 255;
          } else {
            userIsAlertingIncidentTimelineEventWriter = false;
          }      
        }

        return userIsAlertingIncidentTimelineEventWriter;
    }

    public GetWebhookDeliveryAttemptsForIncidentTimelineEvent(incidentTimelineEventId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<WebhookDeliveryAttemptData[]> {
        return this.webhookDeliveryAttemptService.GetWebhookDeliveryAttemptList({
            incidentTimelineEventId: incidentTimelineEventId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full IncidentTimelineEventData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the IncidentTimelineEventData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when IncidentTimelineEventTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveIncidentTimelineEvent(raw: any): IncidentTimelineEventData {
    if (!raw) return raw;

    //
    // Create a IncidentTimelineEventData object instance with correct prototype
    //
    const revived = Object.create(IncidentTimelineEventData.prototype) as IncidentTimelineEventData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._webhookDeliveryAttempts = null;
    (revived as any)._webhookDeliveryAttemptsPromise = null;
    (revived as any)._webhookDeliveryAttemptsSubject = new BehaviorSubject<WebhookDeliveryAttemptData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadIncidentTimelineEventXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).WebhookDeliveryAttempts$ = (revived as any)._webhookDeliveryAttemptsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._webhookDeliveryAttempts === null && (revived as any)._webhookDeliveryAttemptsPromise === null) {
                (revived as any).loadWebhookDeliveryAttempts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).WebhookDeliveryAttemptsCount$ = WebhookDeliveryAttemptService.Instance.GetWebhookDeliveryAttemptsRowCount({incidentTimelineEventId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveIncidentTimelineEventList(rawList: any[]): IncidentTimelineEventData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveIncidentTimelineEvent(raw));
  }

}
