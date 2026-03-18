/*

   GENERATED SERVICE FOR THE EVENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the EventType table.

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
import { IconData } from './icon.service';
import { ChargeTypeData } from './charge-type.service';
import { EventTypeChangeHistoryService, EventTypeChangeHistoryData } from './event-type-change-history.service';
import { ScheduledEventService, ScheduledEventData } from './scheduled-event.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class EventTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    color: string | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    requiresRentalAgreement: boolean | null | undefined = null;
    requiresExternalContact: boolean | null | undefined = null;
    requiresPayment: boolean | null | undefined = null;
    requiresDeposit: boolean | null | undefined = null;
    requiresBarService: boolean | null | undefined = null;
    allowsTicketSales: boolean | null | undefined = null;
    isInternalEvent: boolean | null | undefined = null;
    defaultPrice: number | null | undefined = null;
    chargeTypeId: bigint | number | null | undefined = null;
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
export class EventTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    color: string | null = null;
    iconId: bigint | number | null = null;
    sequence: bigint | number | null = null;
    requiresRentalAgreement!: boolean;
    requiresExternalContact!: boolean;
    requiresPayment!: boolean;
    requiresDeposit!: boolean;
    requiresBarService!: boolean;
    allowsTicketSales!: boolean;
    isInternalEvent!: boolean;
    defaultPrice: number | null = null;
    chargeTypeId: bigint | number | null = null;
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

export class EventTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. EventTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `eventType.EventTypeChildren$` — use with `| async` in templates
//        • Promise:    `eventType.EventTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="eventType.EventTypeChildren$ | async"`), or
//        • Access the promise getter (`eventType.EventTypeChildren` or `await eventType.EventTypeChildren`)
//    - Simply reading `eventType.EventTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await eventType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class EventTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    color!: string | null;
    iconId!: bigint | number;
    sequence!: bigint | number;
    requiresRentalAgreement!: boolean;
    requiresExternalContact!: boolean;
    requiresPayment!: boolean;
    requiresDeposit!: boolean;
    requiresBarService!: boolean;
    allowsTicketSales!: boolean;
    isInternalEvent!: boolean;
    defaultPrice!: number | null;
    chargeTypeId!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    chargeType: ChargeTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _eventTypeChangeHistories: EventTypeChangeHistoryData[] | null = null;
    private _eventTypeChangeHistoriesPromise: Promise<EventTypeChangeHistoryData[]> | null  = null;
    private _eventTypeChangeHistoriesSubject = new BehaviorSubject<EventTypeChangeHistoryData[] | null>(null);

                
    private _scheduledEvents: ScheduledEventData[] | null = null;
    private _scheduledEventsPromise: Promise<ScheduledEventData[]> | null  = null;
    private _scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<EventTypeData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<EventTypeData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<EventTypeData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public EventTypeChangeHistories$ = this._eventTypeChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventTypeChangeHistories === null && this._eventTypeChangeHistoriesPromise === null) {
            this.loadEventTypeChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _eventTypeChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get EventTypeChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._eventTypeChangeHistoriesCount$ === null) {
            this._eventTypeChangeHistoriesCount$ = EventTypeChangeHistoryService.Instance.GetEventTypeChangeHistoriesRowCount({eventTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._eventTypeChangeHistoriesCount$;
    }



    public ScheduledEvents$ = this._scheduledEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEvents === null && this._scheduledEventsPromise === null) {
            this.loadScheduledEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _scheduledEventsCount$: Observable<bigint | number> | null = null;
    public get ScheduledEventsCount$(): Observable<bigint | number> {
        if (this._scheduledEventsCount$ === null) {
            this._scheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({eventTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._scheduledEventsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any EventTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.eventType.Reload();
  //
  //  Non Async:
  //
  //     eventType[0].Reload().then(x => {
  //        this.eventType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      EventTypeService.Instance.GetEventType(this.id, includeRelations)
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
     this._eventTypeChangeHistories = null;
     this._eventTypeChangeHistoriesPromise = null;
     this._eventTypeChangeHistoriesSubject.next(null);
     this._eventTypeChangeHistoriesCount$ = null;

     this._scheduledEvents = null;
     this._scheduledEventsPromise = null;
     this._scheduledEventsSubject.next(null);
     this._scheduledEventsCount$ = null;

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
     * Gets the EventTypeChangeHistories for this EventType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.eventType.EventTypeChangeHistories.then(eventTypes => { ... })
     *   or
     *   await this.eventType.eventTypes
     *
    */
    public get EventTypeChangeHistories(): Promise<EventTypeChangeHistoryData[]> {
        if (this._eventTypeChangeHistories !== null) {
            return Promise.resolve(this._eventTypeChangeHistories);
        }

        if (this._eventTypeChangeHistoriesPromise !== null) {
            return this._eventTypeChangeHistoriesPromise;
        }

        // Start the load
        this.loadEventTypeChangeHistories();

        return this._eventTypeChangeHistoriesPromise!;
    }



    private loadEventTypeChangeHistories(): void {

        this._eventTypeChangeHistoriesPromise = lastValueFrom(
            EventTypeService.Instance.GetEventTypeChangeHistoriesForEventType(this.id)
        )
        .then(EventTypeChangeHistories => {
            this._eventTypeChangeHistories = EventTypeChangeHistories ?? [];
            this._eventTypeChangeHistoriesSubject.next(this._eventTypeChangeHistories);
            return this._eventTypeChangeHistories;
         })
        .catch(err => {
            this._eventTypeChangeHistories = [];
            this._eventTypeChangeHistoriesSubject.next(this._eventTypeChangeHistories);
            throw err;
        })
        .finally(() => {
            this._eventTypeChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventTypeChangeHistory. Call after mutations to force refresh.
     */
    public ClearEventTypeChangeHistoriesCache(): void {
        this._eventTypeChangeHistories = null;
        this._eventTypeChangeHistoriesPromise = null;
        this._eventTypeChangeHistoriesSubject.next(this._eventTypeChangeHistories);      // Emit to observable
    }

    public get HasEventTypeChangeHistories(): Promise<boolean> {
        return this.EventTypeChangeHistories.then(eventTypeChangeHistories => eventTypeChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ScheduledEvents for this EventType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.eventType.ScheduledEvents.then(eventTypes => { ... })
     *   or
     *   await this.eventType.eventTypes
     *
    */
    public get ScheduledEvents(): Promise<ScheduledEventData[]> {
        if (this._scheduledEvents !== null) {
            return Promise.resolve(this._scheduledEvents);
        }

        if (this._scheduledEventsPromise !== null) {
            return this._scheduledEventsPromise;
        }

        // Start the load
        this.loadScheduledEvents();

        return this._scheduledEventsPromise!;
    }



    private loadScheduledEvents(): void {

        this._scheduledEventsPromise = lastValueFrom(
            EventTypeService.Instance.GetScheduledEventsForEventType(this.id)
        )
        .then(ScheduledEvents => {
            this._scheduledEvents = ScheduledEvents ?? [];
            this._scheduledEventsSubject.next(this._scheduledEvents);
            return this._scheduledEvents;
         })
        .catch(err => {
            this._scheduledEvents = [];
            this._scheduledEventsSubject.next(this._scheduledEvents);
            throw err;
        })
        .finally(() => {
            this._scheduledEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEvent. Call after mutations to force refresh.
     */
    public ClearScheduledEventsCache(): void {
        this._scheduledEvents = null;
        this._scheduledEventsPromise = null;
        this._scheduledEventsSubject.next(this._scheduledEvents);      // Emit to observable
    }

    public get HasScheduledEvents(): Promise<boolean> {
        return this.ScheduledEvents.then(scheduledEvents => scheduledEvents.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (eventType.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await eventType.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<EventTypeData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<EventTypeData>> {
        const info = await lastValueFrom(
            EventTypeService.Instance.GetEventTypeChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this EventTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this EventTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): EventTypeSubmitData {
        return EventTypeService.Instance.ConvertToEventTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class EventTypeService extends SecureEndpointBase {

    private static _instance: EventTypeService;
    private listCache: Map<string, Observable<Array<EventTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<EventTypeBasicListData>>>;
    private recordCache: Map<string, Observable<EventTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private eventTypeChangeHistoryService: EventTypeChangeHistoryService,
        private scheduledEventService: ScheduledEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<EventTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<EventTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<EventTypeData>>();

        EventTypeService._instance = this;
    }

    public static get Instance(): EventTypeService {
      return EventTypeService._instance;
    }


    public ClearListCaches(config: EventTypeQueryParameters | null = null) {

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


    public ConvertToEventTypeSubmitData(data: EventTypeData): EventTypeSubmitData {

        let output = new EventTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.color = data.color;
        output.iconId = data.iconId;
        output.sequence = data.sequence;
        output.requiresRentalAgreement = data.requiresRentalAgreement;
        output.requiresExternalContact = data.requiresExternalContact;
        output.requiresPayment = data.requiresPayment;
        output.requiresDeposit = data.requiresDeposit;
        output.requiresBarService = data.requiresBarService;
        output.allowsTicketSales = data.allowsTicketSales;
        output.isInternalEvent = data.isInternalEvent;
        output.defaultPrice = data.defaultPrice;
        output.chargeTypeId = data.chargeTypeId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetEventType(id: bigint | number, includeRelations: boolean = true) : Observable<EventTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const eventType$ = this.requestEventType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EventType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, eventType$);

            return eventType$;
        }

        return this.recordCache.get(configHash) as Observable<EventTypeData>;
    }

    private requestEventType(id: bigint | number, includeRelations: boolean = true) : Observable<EventTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventTypeData>(this.baseUrl + 'api/EventType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveEventType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestEventType(id, includeRelations));
            }));
    }

    public GetEventTypeList(config: EventTypeQueryParameters | any = null) : Observable<Array<EventTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const eventTypeList$ = this.requestEventTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EventType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, eventTypeList$);

            return eventTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<EventTypeData>>;
    }


    private requestEventTypeList(config: EventTypeQueryParameters | any) : Observable <Array<EventTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EventTypeData>>(this.baseUrl + 'api/EventTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveEventTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestEventTypeList(config));
            }));
    }

    public GetEventTypesRowCount(config: EventTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const eventTypesRowCount$ = this.requestEventTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EventTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, eventTypesRowCount$);

            return eventTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestEventTypesRowCount(config: EventTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/EventTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEventTypesRowCount(config));
            }));
    }

    public GetEventTypesBasicListData(config: EventTypeQueryParameters | any = null) : Observable<Array<EventTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const eventTypesBasicListData$ = this.requestEventTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EventTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, eventTypesBasicListData$);

            return eventTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<EventTypeBasicListData>>;
    }


    private requestEventTypesBasicListData(config: EventTypeQueryParameters | any) : Observable<Array<EventTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EventTypeBasicListData>>(this.baseUrl + 'api/EventTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEventTypesBasicListData(config));
            }));

    }


    public PutEventType(id: bigint | number, eventType: EventTypeSubmitData) : Observable<EventTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EventTypeData>(this.baseUrl + 'api/EventType/' + id.toString(), eventType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutEventType(id, eventType));
            }));
    }


    public PostEventType(eventType: EventTypeSubmitData) : Observable<EventTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<EventTypeData>(this.baseUrl + 'api/EventType', eventType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostEventType(eventType));
            }));
    }

  
    public DeleteEventType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/EventType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteEventType(id));
            }));
    }

    public RollbackEventType(id: bigint | number, versionNumber: bigint | number) : Observable<EventTypeData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EventTypeData>(this.baseUrl + 'api/EventType/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackEventType(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a EventType.
     */
    public GetEventTypeChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<EventTypeData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<EventTypeData>>(this.baseUrl + 'api/EventType/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetEventTypeChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a EventType.
     */
    public GetEventTypeAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<EventTypeData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<EventTypeData>[]>(this.baseUrl + 'api/EventType/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetEventTypeAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a EventType.
     */
    public GetEventTypeVersion(id: bigint | number, version: number): Observable<EventTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventTypeData>(this.baseUrl + 'api/EventType/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveEventType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetEventTypeVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a EventType at a specific point in time.
     */
    public GetEventTypeStateAtTime(id: bigint | number, time: string): Observable<EventTypeData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventTypeData>(this.baseUrl + 'api/EventType/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveEventType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetEventTypeStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: EventTypeQueryParameters | any): string {

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

    public userIsSchedulerEventTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerEventTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.EventTypes
        //
        if (userIsSchedulerEventTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerEventTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerEventTypeReader = false;
            }
        }

        return userIsSchedulerEventTypeReader;
    }


    public userIsSchedulerEventTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerEventTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.EventTypes
        //
        if (userIsSchedulerEventTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerEventTypeWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerEventTypeWriter = false;
          }      
        }

        return userIsSchedulerEventTypeWriter;
    }

    public GetEventTypeChangeHistoriesForEventType(eventTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventTypeChangeHistoryData[]> {
        return this.eventTypeChangeHistoryService.GetEventTypeChangeHistoryList({
            eventTypeId: eventTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventsForEventType(eventTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventData[]> {
        return this.scheduledEventService.GetScheduledEventList({
            eventTypeId: eventTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full EventTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the EventTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when EventTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveEventType(raw: any): EventTypeData {
    if (!raw) return raw;

    //
    // Create a EventTypeData object instance with correct prototype
    //
    const revived = Object.create(EventTypeData.prototype) as EventTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._eventTypeChangeHistories = null;
    (revived as any)._eventTypeChangeHistoriesPromise = null;
    (revived as any)._eventTypeChangeHistoriesSubject = new BehaviorSubject<EventTypeChangeHistoryData[] | null>(null);

    (revived as any)._scheduledEvents = null;
    (revived as any)._scheduledEventsPromise = null;
    (revived as any)._scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadEventTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).EventTypeChangeHistories$ = (revived as any)._eventTypeChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventTypeChangeHistories === null && (revived as any)._eventTypeChangeHistoriesPromise === null) {
                (revived as any).loadEventTypeChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._eventTypeChangeHistoriesCount$ = null;


    (revived as any).ScheduledEvents$ = (revived as any)._scheduledEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEvents === null && (revived as any)._scheduledEventsPromise === null) {
                (revived as any).loadScheduledEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._scheduledEventsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<EventTypeData> | null>(null);

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

  private ReviveEventTypeList(rawList: any[]): EventTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveEventType(raw));
  }

}
