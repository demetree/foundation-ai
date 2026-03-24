/*

   GENERATED SERVICE FOR THE EVENTNOTIFICATIONSUBSCRIPTION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the EventNotificationSubscription table.

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
import { ResourceData } from './resource.service';
import { ContactData } from './contact.service';
import { EventNotificationTypeData } from './event-notification-type.service';
import { EventNotificationSubscriptionChangeHistoryService, EventNotificationSubscriptionChangeHistoryData } from './event-notification-subscription-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class EventNotificationSubscriptionQueryParameters {
    resourceId: bigint | number | null | undefined = null;
    contactId: bigint | number | null | undefined = null;
    eventNotificationTypeId: bigint | number | null | undefined = null;
    triggerEvents: bigint | number | null | undefined = null;
    recipientAddress: string | null | undefined = null;
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
export class EventNotificationSubscriptionSubmitData {
    id!: bigint | number;
    resourceId: bigint | number | null = null;
    contactId: bigint | number | null = null;
    eventNotificationTypeId!: bigint | number;
    triggerEvents!: bigint | number;
    recipientAddress!: string;
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

export class EventNotificationSubscriptionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. EventNotificationSubscriptionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `eventNotificationSubscription.EventNotificationSubscriptionChildren$` — use with `| async` in templates
//        • Promise:    `eventNotificationSubscription.EventNotificationSubscriptionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="eventNotificationSubscription.EventNotificationSubscriptionChildren$ | async"`), or
//        • Access the promise getter (`eventNotificationSubscription.EventNotificationSubscriptionChildren` or `await eventNotificationSubscription.EventNotificationSubscriptionChildren`)
//    - Simply reading `eventNotificationSubscription.EventNotificationSubscriptionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await eventNotificationSubscription.Reload()` to refresh the entire object and clear all lazy caches.
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
export class EventNotificationSubscriptionData {
    id!: bigint | number;
    resourceId!: bigint | number;
    contactId!: bigint | number;
    eventNotificationTypeId!: bigint | number;
    triggerEvents!: bigint | number;
    recipientAddress!: string;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    eventNotificationType: EventNotificationTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _eventNotificationSubscriptionChangeHistories: EventNotificationSubscriptionChangeHistoryData[] | null = null;
    private _eventNotificationSubscriptionChangeHistoriesPromise: Promise<EventNotificationSubscriptionChangeHistoryData[]> | null  = null;
    private _eventNotificationSubscriptionChangeHistoriesSubject = new BehaviorSubject<EventNotificationSubscriptionChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<EventNotificationSubscriptionData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<EventNotificationSubscriptionData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<EventNotificationSubscriptionData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public EventNotificationSubscriptionChangeHistories$ = this._eventNotificationSubscriptionChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventNotificationSubscriptionChangeHistories === null && this._eventNotificationSubscriptionChangeHistoriesPromise === null) {
            this.loadEventNotificationSubscriptionChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _eventNotificationSubscriptionChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get EventNotificationSubscriptionChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._eventNotificationSubscriptionChangeHistoriesCount$ === null) {
            this._eventNotificationSubscriptionChangeHistoriesCount$ = EventNotificationSubscriptionChangeHistoryService.Instance.GetEventNotificationSubscriptionChangeHistoriesRowCount({eventNotificationSubscriptionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._eventNotificationSubscriptionChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any EventNotificationSubscriptionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.eventNotificationSubscription.Reload();
  //
  //  Non Async:
  //
  //     eventNotificationSubscription[0].Reload().then(x => {
  //        this.eventNotificationSubscription = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      EventNotificationSubscriptionService.Instance.GetEventNotificationSubscription(this.id, includeRelations)
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
     this._eventNotificationSubscriptionChangeHistories = null;
     this._eventNotificationSubscriptionChangeHistoriesPromise = null;
     this._eventNotificationSubscriptionChangeHistoriesSubject.next(null);
     this._eventNotificationSubscriptionChangeHistoriesCount$ = null;

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
     * Gets the EventNotificationSubscriptionChangeHistories for this EventNotificationSubscription.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.eventNotificationSubscription.EventNotificationSubscriptionChangeHistories.then(eventNotificationSubscriptions => { ... })
     *   or
     *   await this.eventNotificationSubscription.eventNotificationSubscriptions
     *
    */
    public get EventNotificationSubscriptionChangeHistories(): Promise<EventNotificationSubscriptionChangeHistoryData[]> {
        if (this._eventNotificationSubscriptionChangeHistories !== null) {
            return Promise.resolve(this._eventNotificationSubscriptionChangeHistories);
        }

        if (this._eventNotificationSubscriptionChangeHistoriesPromise !== null) {
            return this._eventNotificationSubscriptionChangeHistoriesPromise;
        }

        // Start the load
        this.loadEventNotificationSubscriptionChangeHistories();

        return this._eventNotificationSubscriptionChangeHistoriesPromise!;
    }



    private loadEventNotificationSubscriptionChangeHistories(): void {

        this._eventNotificationSubscriptionChangeHistoriesPromise = lastValueFrom(
            EventNotificationSubscriptionService.Instance.GetEventNotificationSubscriptionChangeHistoriesForEventNotificationSubscription(this.id)
        )
        .then(EventNotificationSubscriptionChangeHistories => {
            this._eventNotificationSubscriptionChangeHistories = EventNotificationSubscriptionChangeHistories ?? [];
            this._eventNotificationSubscriptionChangeHistoriesSubject.next(this._eventNotificationSubscriptionChangeHistories);
            return this._eventNotificationSubscriptionChangeHistories;
         })
        .catch(err => {
            this._eventNotificationSubscriptionChangeHistories = [];
            this._eventNotificationSubscriptionChangeHistoriesSubject.next(this._eventNotificationSubscriptionChangeHistories);
            throw err;
        })
        .finally(() => {
            this._eventNotificationSubscriptionChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventNotificationSubscriptionChangeHistory. Call after mutations to force refresh.
     */
    public ClearEventNotificationSubscriptionChangeHistoriesCache(): void {
        this._eventNotificationSubscriptionChangeHistories = null;
        this._eventNotificationSubscriptionChangeHistoriesPromise = null;
        this._eventNotificationSubscriptionChangeHistoriesSubject.next(this._eventNotificationSubscriptionChangeHistories);      // Emit to observable
    }

    public get HasEventNotificationSubscriptionChangeHistories(): Promise<boolean> {
        return this.EventNotificationSubscriptionChangeHistories.then(eventNotificationSubscriptionChangeHistories => eventNotificationSubscriptionChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (eventNotificationSubscription.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await eventNotificationSubscription.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<EventNotificationSubscriptionData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<EventNotificationSubscriptionData>> {
        const info = await lastValueFrom(
            EventNotificationSubscriptionService.Instance.GetEventNotificationSubscriptionChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this EventNotificationSubscriptionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this EventNotificationSubscriptionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): EventNotificationSubscriptionSubmitData {
        return EventNotificationSubscriptionService.Instance.ConvertToEventNotificationSubscriptionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class EventNotificationSubscriptionService extends SecureEndpointBase {

    private static _instance: EventNotificationSubscriptionService;
    private listCache: Map<string, Observable<Array<EventNotificationSubscriptionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<EventNotificationSubscriptionBasicListData>>>;
    private recordCache: Map<string, Observable<EventNotificationSubscriptionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private eventNotificationSubscriptionChangeHistoryService: EventNotificationSubscriptionChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<EventNotificationSubscriptionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<EventNotificationSubscriptionBasicListData>>>();
        this.recordCache = new Map<string, Observable<EventNotificationSubscriptionData>>();

        EventNotificationSubscriptionService._instance = this;
    }

    public static get Instance(): EventNotificationSubscriptionService {
      return EventNotificationSubscriptionService._instance;
    }


    public ClearListCaches(config: EventNotificationSubscriptionQueryParameters | null = null) {

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


    public ConvertToEventNotificationSubscriptionSubmitData(data: EventNotificationSubscriptionData): EventNotificationSubscriptionSubmitData {

        let output = new EventNotificationSubscriptionSubmitData();

        output.id = data.id;
        output.resourceId = data.resourceId;
        output.contactId = data.contactId;
        output.eventNotificationTypeId = data.eventNotificationTypeId;
        output.triggerEvents = data.triggerEvents;
        output.recipientAddress = data.recipientAddress;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetEventNotificationSubscription(id: bigint | number, includeRelations: boolean = true) : Observable<EventNotificationSubscriptionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const eventNotificationSubscription$ = this.requestEventNotificationSubscription(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EventNotificationSubscription", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, eventNotificationSubscription$);

            return eventNotificationSubscription$;
        }

        return this.recordCache.get(configHash) as Observable<EventNotificationSubscriptionData>;
    }

    private requestEventNotificationSubscription(id: bigint | number, includeRelations: boolean = true) : Observable<EventNotificationSubscriptionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventNotificationSubscriptionData>(this.baseUrl + 'api/EventNotificationSubscription/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveEventNotificationSubscription(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestEventNotificationSubscription(id, includeRelations));
            }));
    }

    public GetEventNotificationSubscriptionList(config: EventNotificationSubscriptionQueryParameters | any = null) : Observable<Array<EventNotificationSubscriptionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const eventNotificationSubscriptionList$ = this.requestEventNotificationSubscriptionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EventNotificationSubscription list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, eventNotificationSubscriptionList$);

            return eventNotificationSubscriptionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<EventNotificationSubscriptionData>>;
    }


    private requestEventNotificationSubscriptionList(config: EventNotificationSubscriptionQueryParameters | any) : Observable <Array<EventNotificationSubscriptionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EventNotificationSubscriptionData>>(this.baseUrl + 'api/EventNotificationSubscriptions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveEventNotificationSubscriptionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestEventNotificationSubscriptionList(config));
            }));
    }

    public GetEventNotificationSubscriptionsRowCount(config: EventNotificationSubscriptionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const eventNotificationSubscriptionsRowCount$ = this.requestEventNotificationSubscriptionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EventNotificationSubscriptions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, eventNotificationSubscriptionsRowCount$);

            return eventNotificationSubscriptionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestEventNotificationSubscriptionsRowCount(config: EventNotificationSubscriptionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/EventNotificationSubscriptions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEventNotificationSubscriptionsRowCount(config));
            }));
    }

    public GetEventNotificationSubscriptionsBasicListData(config: EventNotificationSubscriptionQueryParameters | any = null) : Observable<Array<EventNotificationSubscriptionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const eventNotificationSubscriptionsBasicListData$ = this.requestEventNotificationSubscriptionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EventNotificationSubscriptions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, eventNotificationSubscriptionsBasicListData$);

            return eventNotificationSubscriptionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<EventNotificationSubscriptionBasicListData>>;
    }


    private requestEventNotificationSubscriptionsBasicListData(config: EventNotificationSubscriptionQueryParameters | any) : Observable<Array<EventNotificationSubscriptionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EventNotificationSubscriptionBasicListData>>(this.baseUrl + 'api/EventNotificationSubscriptions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEventNotificationSubscriptionsBasicListData(config));
            }));

    }


    public PutEventNotificationSubscription(id: bigint | number, eventNotificationSubscription: EventNotificationSubscriptionSubmitData) : Observable<EventNotificationSubscriptionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EventNotificationSubscriptionData>(this.baseUrl + 'api/EventNotificationSubscription/' + id.toString(), eventNotificationSubscription, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventNotificationSubscription(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutEventNotificationSubscription(id, eventNotificationSubscription));
            }));
    }


    public PostEventNotificationSubscription(eventNotificationSubscription: EventNotificationSubscriptionSubmitData) : Observable<EventNotificationSubscriptionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<EventNotificationSubscriptionData>(this.baseUrl + 'api/EventNotificationSubscription', eventNotificationSubscription, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventNotificationSubscription(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostEventNotificationSubscription(eventNotificationSubscription));
            }));
    }

  
    public DeleteEventNotificationSubscription(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/EventNotificationSubscription/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteEventNotificationSubscription(id));
            }));
    }

    public RollbackEventNotificationSubscription(id: bigint | number, versionNumber: bigint | number) : Observable<EventNotificationSubscriptionData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EventNotificationSubscriptionData>(this.baseUrl + 'api/EventNotificationSubscription/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventNotificationSubscription(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackEventNotificationSubscription(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a EventNotificationSubscription.
     */
    public GetEventNotificationSubscriptionChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<EventNotificationSubscriptionData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<EventNotificationSubscriptionData>>(this.baseUrl + 'api/EventNotificationSubscription/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetEventNotificationSubscriptionChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a EventNotificationSubscription.
     */
    public GetEventNotificationSubscriptionAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<EventNotificationSubscriptionData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<EventNotificationSubscriptionData>[]>(this.baseUrl + 'api/EventNotificationSubscription/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetEventNotificationSubscriptionAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a EventNotificationSubscription.
     */
    public GetEventNotificationSubscriptionVersion(id: bigint | number, version: number): Observable<EventNotificationSubscriptionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventNotificationSubscriptionData>(this.baseUrl + 'api/EventNotificationSubscription/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveEventNotificationSubscription(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetEventNotificationSubscriptionVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a EventNotificationSubscription at a specific point in time.
     */
    public GetEventNotificationSubscriptionStateAtTime(id: bigint | number, time: string): Observable<EventNotificationSubscriptionData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventNotificationSubscriptionData>(this.baseUrl + 'api/EventNotificationSubscription/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveEventNotificationSubscription(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetEventNotificationSubscriptionStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: EventNotificationSubscriptionQueryParameters | any): string {

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

    public userIsSchedulerEventNotificationSubscriptionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerEventNotificationSubscriptionReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.EventNotificationSubscriptions
        //
        if (userIsSchedulerEventNotificationSubscriptionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerEventNotificationSubscriptionReader = user.readPermission >= 1;
            } else {
                userIsSchedulerEventNotificationSubscriptionReader = false;
            }
        }

        return userIsSchedulerEventNotificationSubscriptionReader;
    }


    public userIsSchedulerEventNotificationSubscriptionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerEventNotificationSubscriptionWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.EventNotificationSubscriptions
        //
        if (userIsSchedulerEventNotificationSubscriptionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerEventNotificationSubscriptionWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerEventNotificationSubscriptionWriter = false;
          }      
        }

        return userIsSchedulerEventNotificationSubscriptionWriter;
    }

    public GetEventNotificationSubscriptionChangeHistoriesForEventNotificationSubscription(eventNotificationSubscriptionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventNotificationSubscriptionChangeHistoryData[]> {
        return this.eventNotificationSubscriptionChangeHistoryService.GetEventNotificationSubscriptionChangeHistoryList({
            eventNotificationSubscriptionId: eventNotificationSubscriptionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full EventNotificationSubscriptionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the EventNotificationSubscriptionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when EventNotificationSubscriptionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveEventNotificationSubscription(raw: any): EventNotificationSubscriptionData {
    if (!raw) return raw;

    //
    // Create a EventNotificationSubscriptionData object instance with correct prototype
    //
    const revived = Object.create(EventNotificationSubscriptionData.prototype) as EventNotificationSubscriptionData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._eventNotificationSubscriptionChangeHistories = null;
    (revived as any)._eventNotificationSubscriptionChangeHistoriesPromise = null;
    (revived as any)._eventNotificationSubscriptionChangeHistoriesSubject = new BehaviorSubject<EventNotificationSubscriptionChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadEventNotificationSubscriptionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).EventNotificationSubscriptionChangeHistories$ = (revived as any)._eventNotificationSubscriptionChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventNotificationSubscriptionChangeHistories === null && (revived as any)._eventNotificationSubscriptionChangeHistoriesPromise === null) {
                (revived as any).loadEventNotificationSubscriptionChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._eventNotificationSubscriptionChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<EventNotificationSubscriptionData> | null>(null);

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

  private ReviveEventNotificationSubscriptionList(rawList: any[]): EventNotificationSubscriptionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveEventNotificationSubscription(raw));
  }

}
