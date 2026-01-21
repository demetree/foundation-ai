/*

   GENERATED SERVICE FOR THE EVENTRESOURCEASSIGNMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the EventResourceAssignment table.

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
import { ScheduledEventData } from './scheduled-event.service';
import { OfficeData } from './office.service';
import { ResourceData } from './resource.service';
import { CrewData } from './crew.service';
import { AssignmentRoleData } from './assignment-role.service';
import { AssignmentStatusData } from './assignment-status.service';
import { EventResourceAssignmentChangeHistoryService, EventResourceAssignmentChangeHistoryData } from './event-resource-assignment-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class EventResourceAssignmentQueryParameters {
    scheduledEventId: bigint | number | null | undefined = null;
    officeId: bigint | number | null | undefined = null;
    resourceId: bigint | number | null | undefined = null;
    crewId: bigint | number | null | undefined = null;
    assignmentRoleId: bigint | number | null | undefined = null;
    assignmentStatusId: bigint | number | null | undefined = null;
    assignmentStartDateTime: string | null | undefined = null;        // ISO 8601
    assignmentEndDateTime: string | null | undefined = null;        // ISO 8601
    notes: string | null | undefined = null;
    isTravelRequired: boolean | null | undefined = null;
    travelDurationMinutes: bigint | number | null | undefined = null;
    distanceKilometers: number | null | undefined = null;
    startLocation: string | null | undefined = null;
    actualStartDateTime: string | null | undefined = null;        // ISO 8601
    actualEndDateTime: string | null | undefined = null;        // ISO 8601
    actualNotes: string | null | undefined = null;
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
export class EventResourceAssignmentSubmitData {
    id!: bigint | number;
    scheduledEventId!: bigint | number;
    officeId: bigint | number | null = null;
    resourceId: bigint | number | null = null;
    crewId: bigint | number | null = null;
    assignmentRoleId: bigint | number | null = null;
    assignmentStatusId!: bigint | number;
    assignmentStartDateTime: string | null = null;     // ISO 8601
    assignmentEndDateTime: string | null = null;     // ISO 8601
    notes: string | null = null;
    isTravelRequired: boolean | null = null;
    travelDurationMinutes: bigint | number | null = null;
    distanceKilometers: number | null = null;
    startLocation: string | null = null;
    actualStartDateTime: string | null = null;     // ISO 8601
    actualEndDateTime: string | null = null;     // ISO 8601
    actualNotes: string | null = null;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}



//
// Version history information returned from version history API endpoints.
// Matches server-side VersionInformation<T> structure.
//
export interface VersionInformation<T> {
    timeStamp: string;           // ISO 8601
    userId: bigint | number;
    userName: string;
    versionNumber: number;
    data: T | null;
}

export class EventResourceAssignmentBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. EventResourceAssignmentChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `eventResourceAssignment.EventResourceAssignmentChildren$` — use with `| async` in templates
//        • Promise:    `eventResourceAssignment.EventResourceAssignmentChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="eventResourceAssignment.EventResourceAssignmentChildren$ | async"`), or
//        • Access the promise getter (`eventResourceAssignment.EventResourceAssignmentChildren` or `await eventResourceAssignment.EventResourceAssignmentChildren`)
//    - Simply reading `eventResourceAssignment.EventResourceAssignmentChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await eventResourceAssignment.Reload()` to refresh the entire object and clear all lazy caches.
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
export class EventResourceAssignmentData {
    id!: bigint | number;
    scheduledEventId!: bigint | number;
    officeId!: bigint | number;
    resourceId!: bigint | number;
    crewId!: bigint | number;
    assignmentRoleId!: bigint | number;
    assignmentStatusId!: bigint | number;
    assignmentStartDateTime!: string | null;   // ISO 8601
    assignmentEndDateTime!: string | null;   // ISO 8601
    notes!: string | null;
    isTravelRequired!: boolean | null;
    travelDurationMinutes!: bigint | number;
    distanceKilometers!: number | null;
    startLocation!: string | null;
    actualStartDateTime!: string | null;   // ISO 8601
    actualEndDateTime!: string | null;   // ISO 8601
    actualNotes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    assignmentRole: AssignmentRoleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    assignmentStatus: AssignmentStatusData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    crew: CrewData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    office: OfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduledEvent: ScheduledEventData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _eventResourceAssignmentChangeHistories: EventResourceAssignmentChangeHistoryData[] | null = null;
    private _eventResourceAssignmentChangeHistoriesPromise: Promise<EventResourceAssignmentChangeHistoryData[]> | null  = null;
    private _eventResourceAssignmentChangeHistoriesSubject = new BehaviorSubject<EventResourceAssignmentChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<EventResourceAssignmentData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<EventResourceAssignmentData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<EventResourceAssignmentData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public EventResourceAssignmentChangeHistories$ = this._eventResourceAssignmentChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventResourceAssignmentChangeHistories === null && this._eventResourceAssignmentChangeHistoriesPromise === null) {
            this.loadEventResourceAssignmentChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public EventResourceAssignmentChangeHistoriesCount$ = EventResourceAssignmentChangeHistoryService.Instance.GetEventResourceAssignmentChangeHistoriesRowCount({eventResourceAssignmentId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any EventResourceAssignmentData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.eventResourceAssignment.Reload();
  //
  //  Non Async:
  //
  //     eventResourceAssignment[0].Reload().then(x => {
  //        this.eventResourceAssignment = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      EventResourceAssignmentService.Instance.GetEventResourceAssignment(this.id, includeRelations)
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
     this._eventResourceAssignmentChangeHistories = null;
     this._eventResourceAssignmentChangeHistoriesPromise = null;
     this._eventResourceAssignmentChangeHistoriesSubject.next(null);

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
     * Gets the EventResourceAssignmentChangeHistories for this EventResourceAssignment.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.eventResourceAssignment.EventResourceAssignmentChangeHistories.then(eventResourceAssignments => { ... })
     *   or
     *   await this.eventResourceAssignment.eventResourceAssignments
     *
    */
    public get EventResourceAssignmentChangeHistories(): Promise<EventResourceAssignmentChangeHistoryData[]> {
        if (this._eventResourceAssignmentChangeHistories !== null) {
            return Promise.resolve(this._eventResourceAssignmentChangeHistories);
        }

        if (this._eventResourceAssignmentChangeHistoriesPromise !== null) {
            return this._eventResourceAssignmentChangeHistoriesPromise;
        }

        // Start the load
        this.loadEventResourceAssignmentChangeHistories();

        return this._eventResourceAssignmentChangeHistoriesPromise!;
    }



    private loadEventResourceAssignmentChangeHistories(): void {

        this._eventResourceAssignmentChangeHistoriesPromise = lastValueFrom(
            EventResourceAssignmentService.Instance.GetEventResourceAssignmentChangeHistoriesForEventResourceAssignment(this.id)
        )
        .then(EventResourceAssignmentChangeHistories => {
            this._eventResourceAssignmentChangeHistories = EventResourceAssignmentChangeHistories ?? [];
            this._eventResourceAssignmentChangeHistoriesSubject.next(this._eventResourceAssignmentChangeHistories);
            return this._eventResourceAssignmentChangeHistories;
         })
        .catch(err => {
            this._eventResourceAssignmentChangeHistories = [];
            this._eventResourceAssignmentChangeHistoriesSubject.next(this._eventResourceAssignmentChangeHistories);
            throw err;
        })
        .finally(() => {
            this._eventResourceAssignmentChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventResourceAssignmentChangeHistory. Call after mutations to force refresh.
     */
    public ClearEventResourceAssignmentChangeHistoriesCache(): void {
        this._eventResourceAssignmentChangeHistories = null;
        this._eventResourceAssignmentChangeHistoriesPromise = null;
        this._eventResourceAssignmentChangeHistoriesSubject.next(this._eventResourceAssignmentChangeHistories);      // Emit to observable
    }

    public get HasEventResourceAssignmentChangeHistories(): Promise<boolean> {
        return this.EventResourceAssignmentChangeHistories.then(eventResourceAssignmentChangeHistories => eventResourceAssignmentChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (eventResourceAssignment.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await eventResourceAssignment.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<EventResourceAssignmentData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<EventResourceAssignmentData>> {
        const info = await lastValueFrom(
            EventResourceAssignmentService.Instance.GetEventResourceAssignmentChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this EventResourceAssignmentData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this EventResourceAssignmentData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): EventResourceAssignmentSubmitData {
        return EventResourceAssignmentService.Instance.ConvertToEventResourceAssignmentSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class EventResourceAssignmentService extends SecureEndpointBase {

    private static _instance: EventResourceAssignmentService;
    private listCache: Map<string, Observable<Array<EventResourceAssignmentData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<EventResourceAssignmentBasicListData>>>;
    private recordCache: Map<string, Observable<EventResourceAssignmentData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private eventResourceAssignmentChangeHistoryService: EventResourceAssignmentChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<EventResourceAssignmentData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<EventResourceAssignmentBasicListData>>>();
        this.recordCache = new Map<string, Observable<EventResourceAssignmentData>>();

        EventResourceAssignmentService._instance = this;
    }

    public static get Instance(): EventResourceAssignmentService {
      return EventResourceAssignmentService._instance;
    }


    public ClearListCaches(config: EventResourceAssignmentQueryParameters | null = null) {

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


    public ConvertToEventResourceAssignmentSubmitData(data: EventResourceAssignmentData): EventResourceAssignmentSubmitData {

        let output = new EventResourceAssignmentSubmitData();

        output.id = data.id;
        output.scheduledEventId = data.scheduledEventId;
        output.officeId = data.officeId;
        output.resourceId = data.resourceId;
        output.crewId = data.crewId;
        output.assignmentRoleId = data.assignmentRoleId;
        output.assignmentStatusId = data.assignmentStatusId;
        output.assignmentStartDateTime = data.assignmentStartDateTime;
        output.assignmentEndDateTime = data.assignmentEndDateTime;
        output.notes = data.notes;
        output.isTravelRequired = data.isTravelRequired;
        output.travelDurationMinutes = data.travelDurationMinutes;
        output.distanceKilometers = data.distanceKilometers;
        output.startLocation = data.startLocation;
        output.actualStartDateTime = data.actualStartDateTime;
        output.actualEndDateTime = data.actualEndDateTime;
        output.actualNotes = data.actualNotes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetEventResourceAssignment(id: bigint | number, includeRelations: boolean = true) : Observable<EventResourceAssignmentData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const eventResourceAssignment$ = this.requestEventResourceAssignment(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EventResourceAssignment", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, eventResourceAssignment$);

            return eventResourceAssignment$;
        }

        return this.recordCache.get(configHash) as Observable<EventResourceAssignmentData>;
    }

    private requestEventResourceAssignment(id: bigint | number, includeRelations: boolean = true) : Observable<EventResourceAssignmentData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventResourceAssignmentData>(this.baseUrl + 'api/EventResourceAssignment/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveEventResourceAssignment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestEventResourceAssignment(id, includeRelations));
            }));
    }

    public GetEventResourceAssignmentList(config: EventResourceAssignmentQueryParameters | any = null) : Observable<Array<EventResourceAssignmentData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const eventResourceAssignmentList$ = this.requestEventResourceAssignmentList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EventResourceAssignment list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, eventResourceAssignmentList$);

            return eventResourceAssignmentList$;
        }

        return this.listCache.get(configHash) as Observable<Array<EventResourceAssignmentData>>;
    }


    private requestEventResourceAssignmentList(config: EventResourceAssignmentQueryParameters | any) : Observable <Array<EventResourceAssignmentData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EventResourceAssignmentData>>(this.baseUrl + 'api/EventResourceAssignments', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveEventResourceAssignmentList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestEventResourceAssignmentList(config));
            }));
    }

    public GetEventResourceAssignmentsRowCount(config: EventResourceAssignmentQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const eventResourceAssignmentsRowCount$ = this.requestEventResourceAssignmentsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EventResourceAssignments row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, eventResourceAssignmentsRowCount$);

            return eventResourceAssignmentsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestEventResourceAssignmentsRowCount(config: EventResourceAssignmentQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/EventResourceAssignments/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEventResourceAssignmentsRowCount(config));
            }));
    }

    public GetEventResourceAssignmentsBasicListData(config: EventResourceAssignmentQueryParameters | any = null) : Observable<Array<EventResourceAssignmentBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const eventResourceAssignmentsBasicListData$ = this.requestEventResourceAssignmentsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EventResourceAssignments basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, eventResourceAssignmentsBasicListData$);

            return eventResourceAssignmentsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<EventResourceAssignmentBasicListData>>;
    }


    private requestEventResourceAssignmentsBasicListData(config: EventResourceAssignmentQueryParameters | any) : Observable<Array<EventResourceAssignmentBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EventResourceAssignmentBasicListData>>(this.baseUrl + 'api/EventResourceAssignments/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEventResourceAssignmentsBasicListData(config));
            }));

    }


    public PutEventResourceAssignment(id: bigint | number, eventResourceAssignment: EventResourceAssignmentSubmitData) : Observable<EventResourceAssignmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EventResourceAssignmentData>(this.baseUrl + 'api/EventResourceAssignment/' + id.toString(), eventResourceAssignment, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventResourceAssignment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutEventResourceAssignment(id, eventResourceAssignment));
            }));
    }


    public PostEventResourceAssignment(eventResourceAssignment: EventResourceAssignmentSubmitData) : Observable<EventResourceAssignmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<EventResourceAssignmentData>(this.baseUrl + 'api/EventResourceAssignment', eventResourceAssignment, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventResourceAssignment(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostEventResourceAssignment(eventResourceAssignment));
            }));
    }

  
    public DeleteEventResourceAssignment(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/EventResourceAssignment/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteEventResourceAssignment(id));
            }));
    }

    public RollbackEventResourceAssignment(id: bigint | number, versionNumber: bigint | number) : Observable<EventResourceAssignmentData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EventResourceAssignmentData>(this.baseUrl + 'api/EventResourceAssignment/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventResourceAssignment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackEventResourceAssignment(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a EventResourceAssignment.
     */
    public GetEventResourceAssignmentChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<EventResourceAssignmentData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<EventResourceAssignmentData>>(this.baseUrl + 'api/EventResourceAssignment/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetEventResourceAssignmentChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a EventResourceAssignment.
     */
    public GetEventResourceAssignmentAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<EventResourceAssignmentData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<EventResourceAssignmentData>[]>(this.baseUrl + 'api/EventResourceAssignment/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetEventResourceAssignmentAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a EventResourceAssignment.
     */
    public GetEventResourceAssignmentVersion(id: bigint | number, version: number): Observable<EventResourceAssignmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventResourceAssignmentData>(this.baseUrl + 'api/EventResourceAssignment/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveEventResourceAssignment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetEventResourceAssignmentVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a EventResourceAssignment at a specific point in time.
     */
    public GetEventResourceAssignmentStateAtTime(id: bigint | number, time: string): Observable<EventResourceAssignmentData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventResourceAssignmentData>(this.baseUrl + 'api/EventResourceAssignment/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveEventResourceAssignment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetEventResourceAssignmentStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: EventResourceAssignmentQueryParameters | any): string {

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

    public userIsSchedulerEventResourceAssignmentReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerEventResourceAssignmentReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.EventResourceAssignments
        //
        if (userIsSchedulerEventResourceAssignmentReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerEventResourceAssignmentReader = user.readPermission >= 1;
            } else {
                userIsSchedulerEventResourceAssignmentReader = false;
            }
        }

        return userIsSchedulerEventResourceAssignmentReader;
    }


    public userIsSchedulerEventResourceAssignmentWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerEventResourceAssignmentWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.EventResourceAssignments
        //
        if (userIsSchedulerEventResourceAssignmentWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerEventResourceAssignmentWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerEventResourceAssignmentWriter = false;
          }      
        }

        return userIsSchedulerEventResourceAssignmentWriter;
    }

    public GetEventResourceAssignmentChangeHistoriesForEventResourceAssignment(eventResourceAssignmentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventResourceAssignmentChangeHistoryData[]> {
        return this.eventResourceAssignmentChangeHistoryService.GetEventResourceAssignmentChangeHistoryList({
            eventResourceAssignmentId: eventResourceAssignmentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full EventResourceAssignmentData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the EventResourceAssignmentData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when EventResourceAssignmentTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveEventResourceAssignment(raw: any): EventResourceAssignmentData {
    if (!raw) return raw;

    //
    // Create a EventResourceAssignmentData object instance with correct prototype
    //
    const revived = Object.create(EventResourceAssignmentData.prototype) as EventResourceAssignmentData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._eventResourceAssignmentChangeHistories = null;
    (revived as any)._eventResourceAssignmentChangeHistoriesPromise = null;
    (revived as any)._eventResourceAssignmentChangeHistoriesSubject = new BehaviorSubject<EventResourceAssignmentChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadEventResourceAssignmentXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).EventResourceAssignmentChangeHistories$ = (revived as any)._eventResourceAssignmentChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventResourceAssignmentChangeHistories === null && (revived as any)._eventResourceAssignmentChangeHistoriesPromise === null) {
                (revived as any).loadEventResourceAssignmentChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).EventResourceAssignmentChangeHistoriesCount$ = EventResourceAssignmentChangeHistoryService.Instance.GetEventResourceAssignmentChangeHistoriesRowCount({eventResourceAssignmentId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveEventResourceAssignmentList(rawList: any[]): EventResourceAssignmentData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveEventResourceAssignment(raw));
  }

}
