/*

   GENERATED SERVICE FOR THE CALENDAR TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Calendar table.

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
import { OfficeData } from './office.service';
import { IconData } from './icon.service';
import { CalendarChangeHistoryService, CalendarChangeHistoryData } from './calendar-change-history.service';
import { ClientService, ClientData } from './client.service';
import { SchedulingTargetService, SchedulingTargetData } from './scheduling-target.service';
import { EventCalendarService, EventCalendarData } from './event-calendar.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class CalendarQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    officeId: bigint | number | null | undefined = null;
    isDefault: boolean | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
    objectGuid: string | null | undefined = null;
    active: boolean | null | undefined = null;
    deleted: boolean | null | undefined = null;
    versionNumber: bigint | number | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class CalendarSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    officeId: bigint | number | null = null;
    isDefault: boolean | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
    versionNumber!: bigint | number;
}


export class CalendarBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. CalendarChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `calendar.CalendarChildren$` — use with `| async` in templates
//        • Promise:    `calendar.CalendarChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="calendar.CalendarChildren$ | async"`), or
//        • Access the promise getter (`calendar.CalendarChildren` or `await calendar.CalendarChildren`)
//    - Simply reading `calendar.CalendarChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await calendar.Reload()` to refresh the entire object and clear all lazy caches.
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
export class CalendarData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    officeId!: bigint | number;
    isDefault!: boolean | null;
    iconId!: bigint | number;
    color!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    versionNumber!: bigint | number;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    office: OfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _calendarChangeHistories: CalendarChangeHistoryData[] | null = null;
    private _calendarChangeHistoriesPromise: Promise<CalendarChangeHistoryData[]> | null  = null;
    private _calendarChangeHistoriesSubject = new BehaviorSubject<CalendarChangeHistoryData[] | null>(null);

                
    private _clients: ClientData[] | null = null;
    private _clientsPromise: Promise<ClientData[]> | null  = null;
    private _clientsSubject = new BehaviorSubject<ClientData[] | null>(null);

                
    private _schedulingTargets: SchedulingTargetData[] | null = null;
    private _schedulingTargetsPromise: Promise<SchedulingTargetData[]> | null  = null;
    private _schedulingTargetsSubject = new BehaviorSubject<SchedulingTargetData[] | null>(null);

                
    private _eventCalendars: EventCalendarData[] | null = null;
    private _eventCalendarsPromise: Promise<EventCalendarData[]> | null  = null;
    private _eventCalendarsSubject = new BehaviorSubject<EventCalendarData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public CalendarChangeHistories$ = this._calendarChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._calendarChangeHistories === null && this._calendarChangeHistoriesPromise === null) {
            this.loadCalendarChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public CalendarChangeHistoriesCount$ = CalendarChangeHistoryService.Instance.GetCalendarChangeHistoriesRowCount({calendarId: this.id,
      active: true,
      deleted: false
    });



    public Clients$ = this._clientsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._clients === null && this._clientsPromise === null) {
            this.loadClients(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ClientsCount$ = ClientService.Instance.GetClientsRowCount({calendarId: this.id,
      active: true,
      deleted: false
    });



    public SchedulingTargets$ = this._schedulingTargetsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargets === null && this._schedulingTargetsPromise === null) {
            this.loadSchedulingTargets(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SchedulingTargetsCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({calendarId: this.id,
      active: true,
      deleted: false
    });



    public EventCalendars$ = this._eventCalendarsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventCalendars === null && this._eventCalendarsPromise === null) {
            this.loadEventCalendars(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public EventCalendarsCount$ = EventCalendarService.Instance.GetEventCalendarsRowCount({calendarId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any CalendarData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.calendar.Reload();
  //
  //  Non Async:
  //
  //     calendar[0].Reload().then(x => {
  //        this.calendar = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      CalendarService.Instance.GetCalendar(this.id, includeRelations)
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
     this._calendarChangeHistories = null;
     this._calendarChangeHistoriesPromise = null;
     this._calendarChangeHistoriesSubject.next(null);

     this._clients = null;
     this._clientsPromise = null;
     this._clientsSubject.next(null);

     this._schedulingTargets = null;
     this._schedulingTargetsPromise = null;
     this._schedulingTargetsSubject.next(null);

     this._eventCalendars = null;
     this._eventCalendarsPromise = null;
     this._eventCalendarsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the CalendarChangeHistories for this Calendar.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.calendar.CalendarChangeHistories.then(calendars => { ... })
     *   or
     *   await this.calendar.calendars
     *
    */
    public get CalendarChangeHistories(): Promise<CalendarChangeHistoryData[]> {
        if (this._calendarChangeHistories !== null) {
            return Promise.resolve(this._calendarChangeHistories);
        }

        if (this._calendarChangeHistoriesPromise !== null) {
            return this._calendarChangeHistoriesPromise;
        }

        // Start the load
        this.loadCalendarChangeHistories();

        return this._calendarChangeHistoriesPromise!;
    }



    private loadCalendarChangeHistories(): void {

        this._calendarChangeHistoriesPromise = lastValueFrom(
            CalendarService.Instance.GetCalendarChangeHistoriesForCalendar(this.id)
        )
        .then(CalendarChangeHistories => {
            this._calendarChangeHistories = CalendarChangeHistories ?? [];
            this._calendarChangeHistoriesSubject.next(this._calendarChangeHistories);
            return this._calendarChangeHistories;
         })
        .catch(err => {
            this._calendarChangeHistories = [];
            this._calendarChangeHistoriesSubject.next(this._calendarChangeHistories);
            throw err;
        })
        .finally(() => {
            this._calendarChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached CalendarChangeHistory. Call after mutations to force refresh.
     */
    public ClearCalendarChangeHistoriesCache(): void {
        this._calendarChangeHistories = null;
        this._calendarChangeHistoriesPromise = null;
        this._calendarChangeHistoriesSubject.next(this._calendarChangeHistories);      // Emit to observable
    }

    public get HasCalendarChangeHistories(): Promise<boolean> {
        return this.CalendarChangeHistories.then(calendarChangeHistories => calendarChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Clients for this Calendar.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.calendar.Clients.then(calendars => { ... })
     *   or
     *   await this.calendar.calendars
     *
    */
    public get Clients(): Promise<ClientData[]> {
        if (this._clients !== null) {
            return Promise.resolve(this._clients);
        }

        if (this._clientsPromise !== null) {
            return this._clientsPromise;
        }

        // Start the load
        this.loadClients();

        return this._clientsPromise!;
    }



    private loadClients(): void {

        this._clientsPromise = lastValueFrom(
            CalendarService.Instance.GetClientsForCalendar(this.id)
        )
        .then(Clients => {
            this._clients = Clients ?? [];
            this._clientsSubject.next(this._clients);
            return this._clients;
         })
        .catch(err => {
            this._clients = [];
            this._clientsSubject.next(this._clients);
            throw err;
        })
        .finally(() => {
            this._clientsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Client. Call after mutations to force refresh.
     */
    public ClearClientsCache(): void {
        this._clients = null;
        this._clientsPromise = null;
        this._clientsSubject.next(this._clients);      // Emit to observable
    }

    public get HasClients(): Promise<boolean> {
        return this.Clients.then(clients => clients.length > 0);
    }


    /**
     *
     * Gets the SchedulingTargets for this Calendar.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.calendar.SchedulingTargets.then(calendars => { ... })
     *   or
     *   await this.calendar.calendars
     *
    */
    public get SchedulingTargets(): Promise<SchedulingTargetData[]> {
        if (this._schedulingTargets !== null) {
            return Promise.resolve(this._schedulingTargets);
        }

        if (this._schedulingTargetsPromise !== null) {
            return this._schedulingTargetsPromise;
        }

        // Start the load
        this.loadSchedulingTargets();

        return this._schedulingTargetsPromise!;
    }



    private loadSchedulingTargets(): void {

        this._schedulingTargetsPromise = lastValueFrom(
            CalendarService.Instance.GetSchedulingTargetsForCalendar(this.id)
        )
        .then(SchedulingTargets => {
            this._schedulingTargets = SchedulingTargets ?? [];
            this._schedulingTargetsSubject.next(this._schedulingTargets);
            return this._schedulingTargets;
         })
        .catch(err => {
            this._schedulingTargets = [];
            this._schedulingTargetsSubject.next(this._schedulingTargets);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTarget. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetsCache(): void {
        this._schedulingTargets = null;
        this._schedulingTargetsPromise = null;
        this._schedulingTargetsSubject.next(this._schedulingTargets);      // Emit to observable
    }

    public get HasSchedulingTargets(): Promise<boolean> {
        return this.SchedulingTargets.then(schedulingTargets => schedulingTargets.length > 0);
    }


    /**
     *
     * Gets the EventCalendars for this Calendar.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.calendar.EventCalendars.then(calendars => { ... })
     *   or
     *   await this.calendar.calendars
     *
    */
    public get EventCalendars(): Promise<EventCalendarData[]> {
        if (this._eventCalendars !== null) {
            return Promise.resolve(this._eventCalendars);
        }

        if (this._eventCalendarsPromise !== null) {
            return this._eventCalendarsPromise;
        }

        // Start the load
        this.loadEventCalendars();

        return this._eventCalendarsPromise!;
    }



    private loadEventCalendars(): void {

        this._eventCalendarsPromise = lastValueFrom(
            CalendarService.Instance.GetEventCalendarsForCalendar(this.id)
        )
        .then(EventCalendars => {
            this._eventCalendars = EventCalendars ?? [];
            this._eventCalendarsSubject.next(this._eventCalendars);
            return this._eventCalendars;
         })
        .catch(err => {
            this._eventCalendars = [];
            this._eventCalendarsSubject.next(this._eventCalendars);
            throw err;
        })
        .finally(() => {
            this._eventCalendarsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventCalendar. Call after mutations to force refresh.
     */
    public ClearEventCalendarsCache(): void {
        this._eventCalendars = null;
        this._eventCalendarsPromise = null;
        this._eventCalendarsSubject.next(this._eventCalendars);      // Emit to observable
    }

    public get HasEventCalendars(): Promise<boolean> {
        return this.EventCalendars.then(eventCalendars => eventCalendars.length > 0);
    }




    /**
     * Updates the state of this CalendarData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this CalendarData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): CalendarSubmitData {
        return CalendarService.Instance.ConvertToCalendarSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class CalendarService extends SecureEndpointBase {

    private static _instance: CalendarService;
    private listCache: Map<string, Observable<Array<CalendarData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<CalendarBasicListData>>>;
    private recordCache: Map<string, Observable<CalendarData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private calendarChangeHistoryService: CalendarChangeHistoryService,
        private clientService: ClientService,
        private schedulingTargetService: SchedulingTargetService,
        private eventCalendarService: EventCalendarService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<CalendarData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<CalendarBasicListData>>>();
        this.recordCache = new Map<string, Observable<CalendarData>>();

        CalendarService._instance = this;
    }

    public static get Instance(): CalendarService {
      return CalendarService._instance;
    }


    public ClearListCaches(config: CalendarQueryParameters | null = null) {

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


    public ConvertToCalendarSubmitData(data: CalendarData): CalendarSubmitData {

        let output = new CalendarSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.officeId = data.officeId;
        output.isDefault = data.isDefault;
        output.iconId = data.iconId;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;
        output.versionNumber = data.versionNumber;

        return output;
    }

    public GetCalendar(id: bigint | number, includeRelations: boolean = true) : Observable<CalendarData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const calendar$ = this.requestCalendar(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Calendar", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, calendar$);

            return calendar$;
        }

        return this.recordCache.get(configHash) as Observable<CalendarData>;
    }

    private requestCalendar(id: bigint | number, includeRelations: boolean = true) : Observable<CalendarData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CalendarData>(this.baseUrl + 'api/Calendar/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveCalendar(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestCalendar(id, includeRelations));
            }));
    }

    public GetCalendarList(config: CalendarQueryParameters | any = null) : Observable<Array<CalendarData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const calendarList$ = this.requestCalendarList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Calendar list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, calendarList$);

            return calendarList$;
        }

        return this.listCache.get(configHash) as Observable<Array<CalendarData>>;
    }


    private requestCalendarList(config: CalendarQueryParameters | any) : Observable <Array<CalendarData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CalendarData>>(this.baseUrl + 'api/Calendars', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveCalendarList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestCalendarList(config));
            }));
    }

    public GetCalendarsRowCount(config: CalendarQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const calendarsRowCount$ = this.requestCalendarsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Calendars row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, calendarsRowCount$);

            return calendarsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestCalendarsRowCount(config: CalendarQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Calendars/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCalendarsRowCount(config));
            }));
    }

    public GetCalendarsBasicListData(config: CalendarQueryParameters | any = null) : Observable<Array<CalendarBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const calendarsBasicListData$ = this.requestCalendarsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Calendars basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, calendarsBasicListData$);

            return calendarsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<CalendarBasicListData>>;
    }


    private requestCalendarsBasicListData(config: CalendarQueryParameters | any) : Observable<Array<CalendarBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CalendarBasicListData>>(this.baseUrl + 'api/Calendars/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCalendarsBasicListData(config));
            }));

    }


    public PutCalendar(id: bigint | number, calendar: CalendarSubmitData) : Observable<CalendarData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CalendarData>(this.baseUrl + 'api/Calendar/' + id.toString(), calendar, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCalendar(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutCalendar(id, calendar));
            }));
    }


    public PostCalendar(calendar: CalendarSubmitData) : Observable<CalendarData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<CalendarData>(this.baseUrl + 'api/Calendar', calendar, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCalendar(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostCalendar(calendar));
            }));
    }

  
    public DeleteCalendar(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Calendar/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteCalendar(id));
            }));
    }

    public RollbackCalendar(id: bigint | number, versionNumber: bigint | number) : Observable<CalendarData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CalendarData>(this.baseUrl + 'api/Calendar/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCalendar(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackCalendar(id, versionNumber));
        }));
    }

    private getConfigHash(config: CalendarQueryParameters | any): string {

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

    public userIsSchedulerCalendarReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerCalendarReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Calendars
        //
        if (userIsSchedulerCalendarReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerCalendarReader = user.readPermission >= 1;
            } else {
                userIsSchedulerCalendarReader = false;
            }
        }

        return userIsSchedulerCalendarReader;
    }


    public userIsSchedulerCalendarWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerCalendarWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Calendars
        //
        if (userIsSchedulerCalendarWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerCalendarWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerCalendarWriter = false;
          }      
        }

        return userIsSchedulerCalendarWriter;
    }

    public GetCalendarChangeHistoriesForCalendar(calendarId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CalendarChangeHistoryData[]> {
        return this.calendarChangeHistoryService.GetCalendarChangeHistoryList({
            calendarId: calendarId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetClientsForCalendar(calendarId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ClientData[]> {
        return this.clientService.GetClientList({
            calendarId: calendarId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetsForCalendar(calendarId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetData[]> {
        return this.schedulingTargetService.GetSchedulingTargetList({
            calendarId: calendarId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventCalendarsForCalendar(calendarId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventCalendarData[]> {
        return this.eventCalendarService.GetEventCalendarList({
            calendarId: calendarId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full CalendarData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the CalendarData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when CalendarTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveCalendar(raw: any): CalendarData {
    if (!raw) return raw;

    //
    // Create a CalendarData object instance with correct prototype
    //
    const revived = Object.create(CalendarData.prototype) as CalendarData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._calendarChangeHistories = null;
    (revived as any)._calendarChangeHistoriesPromise = null;
    (revived as any)._calendarChangeHistoriesSubject = new BehaviorSubject<CalendarChangeHistoryData[] | null>(null);

    (revived as any)._clients = null;
    (revived as any)._clientsPromise = null;
    (revived as any)._clientsSubject = new BehaviorSubject<ClientData[] | null>(null);

    (revived as any)._schedulingTargets = null;
    (revived as any)._schedulingTargetsPromise = null;
    (revived as any)._schedulingTargetsSubject = new BehaviorSubject<SchedulingTargetData[] | null>(null);

    (revived as any)._eventCalendars = null;
    (revived as any)._eventCalendarsPromise = null;
    (revived as any)._eventCalendarsSubject = new BehaviorSubject<EventCalendarData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadCalendarXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).CalendarChangeHistories$ = (revived as any)._calendarChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._calendarChangeHistories === null && (revived as any)._calendarChangeHistoriesPromise === null) {
                (revived as any).loadCalendarChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).CalendarChangeHistoriesCount$ = CalendarChangeHistoryService.Instance.GetCalendarChangeHistoriesRowCount({calendarId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Clients$ = (revived as any)._clientsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._clients === null && (revived as any)._clientsPromise === null) {
                (revived as any).loadClients();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ClientsCount$ = ClientService.Instance.GetClientsRowCount({calendarId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SchedulingTargets$ = (revived as any)._schedulingTargetsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargets === null && (revived as any)._schedulingTargetsPromise === null) {
                (revived as any).loadSchedulingTargets();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SchedulingTargetsCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({calendarId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).EventCalendars$ = (revived as any)._eventCalendarsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventCalendars === null && (revived as any)._eventCalendarsPromise === null) {
                (revived as any).loadEventCalendars();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).EventCalendarsCount$ = EventCalendarService.Instance.GetEventCalendarsRowCount({calendarId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveCalendarList(rawList: any[]): CalendarData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveCalendar(raw));
  }

}
