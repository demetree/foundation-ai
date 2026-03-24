/*

   GENERATED SERVICE FOR THE EVENTNOTIFICATIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the EventNotificationType table.

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
import { EventNotificationSubscriptionService, EventNotificationSubscriptionData } from './event-notification-subscription.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class EventNotificationTypeQueryParameters {
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
export class EventNotificationTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class EventNotificationTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. EventNotificationTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `eventNotificationType.EventNotificationTypeChildren$` — use with `| async` in templates
//        • Promise:    `eventNotificationType.EventNotificationTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="eventNotificationType.EventNotificationTypeChildren$ | async"`), or
//        • Access the promise getter (`eventNotificationType.EventNotificationTypeChildren` or `await eventNotificationType.EventNotificationTypeChildren`)
//    - Simply reading `eventNotificationType.EventNotificationTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await eventNotificationType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class EventNotificationTypeData {
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
    private _eventNotificationSubscriptions: EventNotificationSubscriptionData[] | null = null;
    private _eventNotificationSubscriptionsPromise: Promise<EventNotificationSubscriptionData[]> | null  = null;
    private _eventNotificationSubscriptionsSubject = new BehaviorSubject<EventNotificationSubscriptionData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public EventNotificationSubscriptions$ = this._eventNotificationSubscriptionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventNotificationSubscriptions === null && this._eventNotificationSubscriptionsPromise === null) {
            this.loadEventNotificationSubscriptions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _eventNotificationSubscriptionsCount$: Observable<bigint | number> | null = null;
    public get EventNotificationSubscriptionsCount$(): Observable<bigint | number> {
        if (this._eventNotificationSubscriptionsCount$ === null) {
            this._eventNotificationSubscriptionsCount$ = EventNotificationSubscriptionService.Instance.GetEventNotificationSubscriptionsRowCount({eventNotificationTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._eventNotificationSubscriptionsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any EventNotificationTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.eventNotificationType.Reload();
  //
  //  Non Async:
  //
  //     eventNotificationType[0].Reload().then(x => {
  //        this.eventNotificationType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      EventNotificationTypeService.Instance.GetEventNotificationType(this.id, includeRelations)
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
     this._eventNotificationSubscriptions = null;
     this._eventNotificationSubscriptionsPromise = null;
     this._eventNotificationSubscriptionsSubject.next(null);
     this._eventNotificationSubscriptionsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the EventNotificationSubscriptions for this EventNotificationType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.eventNotificationType.EventNotificationSubscriptions.then(eventNotificationTypes => { ... })
     *   or
     *   await this.eventNotificationType.eventNotificationTypes
     *
    */
    public get EventNotificationSubscriptions(): Promise<EventNotificationSubscriptionData[]> {
        if (this._eventNotificationSubscriptions !== null) {
            return Promise.resolve(this._eventNotificationSubscriptions);
        }

        if (this._eventNotificationSubscriptionsPromise !== null) {
            return this._eventNotificationSubscriptionsPromise;
        }

        // Start the load
        this.loadEventNotificationSubscriptions();

        return this._eventNotificationSubscriptionsPromise!;
    }



    private loadEventNotificationSubscriptions(): void {

        this._eventNotificationSubscriptionsPromise = lastValueFrom(
            EventNotificationTypeService.Instance.GetEventNotificationSubscriptionsForEventNotificationType(this.id)
        )
        .then(EventNotificationSubscriptions => {
            this._eventNotificationSubscriptions = EventNotificationSubscriptions ?? [];
            this._eventNotificationSubscriptionsSubject.next(this._eventNotificationSubscriptions);
            return this._eventNotificationSubscriptions;
         })
        .catch(err => {
            this._eventNotificationSubscriptions = [];
            this._eventNotificationSubscriptionsSubject.next(this._eventNotificationSubscriptions);
            throw err;
        })
        .finally(() => {
            this._eventNotificationSubscriptionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventNotificationSubscription. Call after mutations to force refresh.
     */
    public ClearEventNotificationSubscriptionsCache(): void {
        this._eventNotificationSubscriptions = null;
        this._eventNotificationSubscriptionsPromise = null;
        this._eventNotificationSubscriptionsSubject.next(this._eventNotificationSubscriptions);      // Emit to observable
    }

    public get HasEventNotificationSubscriptions(): Promise<boolean> {
        return this.EventNotificationSubscriptions.then(eventNotificationSubscriptions => eventNotificationSubscriptions.length > 0);
    }




    /**
     * Updates the state of this EventNotificationTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this EventNotificationTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): EventNotificationTypeSubmitData {
        return EventNotificationTypeService.Instance.ConvertToEventNotificationTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class EventNotificationTypeService extends SecureEndpointBase {

    private static _instance: EventNotificationTypeService;
    private listCache: Map<string, Observable<Array<EventNotificationTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<EventNotificationTypeBasicListData>>>;
    private recordCache: Map<string, Observable<EventNotificationTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private eventNotificationSubscriptionService: EventNotificationSubscriptionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<EventNotificationTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<EventNotificationTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<EventNotificationTypeData>>();

        EventNotificationTypeService._instance = this;
    }

    public static get Instance(): EventNotificationTypeService {
      return EventNotificationTypeService._instance;
    }


    public ClearListCaches(config: EventNotificationTypeQueryParameters | null = null) {

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


    public ConvertToEventNotificationTypeSubmitData(data: EventNotificationTypeData): EventNotificationTypeSubmitData {

        let output = new EventNotificationTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetEventNotificationType(id: bigint | number, includeRelations: boolean = true) : Observable<EventNotificationTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const eventNotificationType$ = this.requestEventNotificationType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EventNotificationType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, eventNotificationType$);

            return eventNotificationType$;
        }

        return this.recordCache.get(configHash) as Observable<EventNotificationTypeData>;
    }

    private requestEventNotificationType(id: bigint | number, includeRelations: boolean = true) : Observable<EventNotificationTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventNotificationTypeData>(this.baseUrl + 'api/EventNotificationType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveEventNotificationType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestEventNotificationType(id, includeRelations));
            }));
    }

    public GetEventNotificationTypeList(config: EventNotificationTypeQueryParameters | any = null) : Observable<Array<EventNotificationTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const eventNotificationTypeList$ = this.requestEventNotificationTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EventNotificationType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, eventNotificationTypeList$);

            return eventNotificationTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<EventNotificationTypeData>>;
    }


    private requestEventNotificationTypeList(config: EventNotificationTypeQueryParameters | any) : Observable <Array<EventNotificationTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EventNotificationTypeData>>(this.baseUrl + 'api/EventNotificationTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveEventNotificationTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestEventNotificationTypeList(config));
            }));
    }

    public GetEventNotificationTypesRowCount(config: EventNotificationTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const eventNotificationTypesRowCount$ = this.requestEventNotificationTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EventNotificationTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, eventNotificationTypesRowCount$);

            return eventNotificationTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestEventNotificationTypesRowCount(config: EventNotificationTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/EventNotificationTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEventNotificationTypesRowCount(config));
            }));
    }

    public GetEventNotificationTypesBasicListData(config: EventNotificationTypeQueryParameters | any = null) : Observable<Array<EventNotificationTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const eventNotificationTypesBasicListData$ = this.requestEventNotificationTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EventNotificationTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, eventNotificationTypesBasicListData$);

            return eventNotificationTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<EventNotificationTypeBasicListData>>;
    }


    private requestEventNotificationTypesBasicListData(config: EventNotificationTypeQueryParameters | any) : Observable<Array<EventNotificationTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EventNotificationTypeBasicListData>>(this.baseUrl + 'api/EventNotificationTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEventNotificationTypesBasicListData(config));
            }));

    }


    public PutEventNotificationType(id: bigint | number, eventNotificationType: EventNotificationTypeSubmitData) : Observable<EventNotificationTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EventNotificationTypeData>(this.baseUrl + 'api/EventNotificationType/' + id.toString(), eventNotificationType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventNotificationType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutEventNotificationType(id, eventNotificationType));
            }));
    }


    public PostEventNotificationType(eventNotificationType: EventNotificationTypeSubmitData) : Observable<EventNotificationTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<EventNotificationTypeData>(this.baseUrl + 'api/EventNotificationType', eventNotificationType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventNotificationType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostEventNotificationType(eventNotificationType));
            }));
    }

  
    public DeleteEventNotificationType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/EventNotificationType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteEventNotificationType(id));
            }));
    }


    private getConfigHash(config: EventNotificationTypeQueryParameters | any): string {

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

    public userIsSchedulerEventNotificationTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerEventNotificationTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.EventNotificationTypes
        //
        if (userIsSchedulerEventNotificationTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerEventNotificationTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerEventNotificationTypeReader = false;
            }
        }

        return userIsSchedulerEventNotificationTypeReader;
    }


    public userIsSchedulerEventNotificationTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerEventNotificationTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.EventNotificationTypes
        //
        if (userIsSchedulerEventNotificationTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerEventNotificationTypeWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerEventNotificationTypeWriter = false;
          }      
        }

        return userIsSchedulerEventNotificationTypeWriter;
    }

    public GetEventNotificationSubscriptionsForEventNotificationType(eventNotificationTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventNotificationSubscriptionData[]> {
        return this.eventNotificationSubscriptionService.GetEventNotificationSubscriptionList({
            eventNotificationTypeId: eventNotificationTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full EventNotificationTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the EventNotificationTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when EventNotificationTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveEventNotificationType(raw: any): EventNotificationTypeData {
    if (!raw) return raw;

    //
    // Create a EventNotificationTypeData object instance with correct prototype
    //
    const revived = Object.create(EventNotificationTypeData.prototype) as EventNotificationTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._eventNotificationSubscriptions = null;
    (revived as any)._eventNotificationSubscriptionsPromise = null;
    (revived as any)._eventNotificationSubscriptionsSubject = new BehaviorSubject<EventNotificationSubscriptionData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadEventNotificationTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).EventNotificationSubscriptions$ = (revived as any)._eventNotificationSubscriptionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventNotificationSubscriptions === null && (revived as any)._eventNotificationSubscriptionsPromise === null) {
                (revived as any).loadEventNotificationSubscriptions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._eventNotificationSubscriptionsCount$ = null;



    return revived;
  }

  private ReviveEventNotificationTypeList(rawList: any[]): EventNotificationTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveEventNotificationType(raw));
  }

}
