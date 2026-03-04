/*

   GENERATED SERVICE FOR THE CREW TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Crew table.

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
import { CrewChangeHistoryService, CrewChangeHistoryData } from './crew-change-history.service';
import { CrewMemberService, CrewMemberData } from './crew-member.service';
import { ScheduledEventService, ScheduledEventData } from './scheduled-event.service';
import { EventResourceAssignmentService, EventResourceAssignmentData } from './event-resource-assignment.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class CrewQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    notes: string | null | undefined = null;
    officeId: bigint | number | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
    avatarFileName: string | null | undefined = null;
    avatarSize: bigint | number | null | undefined = null;
    avatarMimeType: string | null | undefined = null;
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
export class CrewSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    notes: string | null = null;
    officeId: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    avatarFileName: string | null = null;
    avatarSize: bigint | number | null = null;
    avatarData: string | null = null;
    avatarMimeType: string | null = null;
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

export class CrewBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. CrewChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `crew.CrewChildren$` — use with `| async` in templates
//        • Promise:    `crew.CrewChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="crew.CrewChildren$ | async"`), or
//        • Access the promise getter (`crew.CrewChildren` or `await crew.CrewChildren`)
//    - Simply reading `crew.CrewChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await crew.Reload()` to refresh the entire object and clear all lazy caches.
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
export class CrewData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    notes!: string | null;
    officeId!: bigint | number;
    iconId!: bigint | number;
    color!: string | null;
    avatarFileName!: string | null;
    avatarSize!: bigint | number;
    avatarData!: string | null;
    avatarMimeType!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    office: OfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _crewChangeHistories: CrewChangeHistoryData[] | null = null;
    private _crewChangeHistoriesPromise: Promise<CrewChangeHistoryData[]> | null  = null;
    private _crewChangeHistoriesSubject = new BehaviorSubject<CrewChangeHistoryData[] | null>(null);

                
    private _crewMembers: CrewMemberData[] | null = null;
    private _crewMembersPromise: Promise<CrewMemberData[]> | null  = null;
    private _crewMembersSubject = new BehaviorSubject<CrewMemberData[] | null>(null);

                
    private _scheduledEvents: ScheduledEventData[] | null = null;
    private _scheduledEventsPromise: Promise<ScheduledEventData[]> | null  = null;
    private _scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

                
    private _eventResourceAssignments: EventResourceAssignmentData[] | null = null;
    private _eventResourceAssignmentsPromise: Promise<EventResourceAssignmentData[]> | null  = null;
    private _eventResourceAssignmentsSubject = new BehaviorSubject<EventResourceAssignmentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<CrewData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<CrewData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<CrewData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public CrewChangeHistories$ = this._crewChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._crewChangeHistories === null && this._crewChangeHistoriesPromise === null) {
            this.loadCrewChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _crewChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get CrewChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._crewChangeHistoriesCount$ === null) {
            this._crewChangeHistoriesCount$ = CrewChangeHistoryService.Instance.GetCrewChangeHistoriesRowCount({crewId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._crewChangeHistoriesCount$;
    }



    public CrewMembers$ = this._crewMembersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._crewMembers === null && this._crewMembersPromise === null) {
            this.loadCrewMembers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _crewMembersCount$: Observable<bigint | number> | null = null;
    public get CrewMembersCount$(): Observable<bigint | number> {
        if (this._crewMembersCount$ === null) {
            this._crewMembersCount$ = CrewMemberService.Instance.GetCrewMembersRowCount({crewId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._crewMembersCount$;
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
            this._scheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({crewId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._scheduledEventsCount$;
    }



    public EventResourceAssignments$ = this._eventResourceAssignmentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventResourceAssignments === null && this._eventResourceAssignmentsPromise === null) {
            this.loadEventResourceAssignments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _eventResourceAssignmentsCount$: Observable<bigint | number> | null = null;
    public get EventResourceAssignmentsCount$(): Observable<bigint | number> {
        if (this._eventResourceAssignmentsCount$ === null) {
            this._eventResourceAssignmentsCount$ = EventResourceAssignmentService.Instance.GetEventResourceAssignmentsRowCount({crewId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._eventResourceAssignmentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any CrewData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.crew.Reload();
  //
  //  Non Async:
  //
  //     crew[0].Reload().then(x => {
  //        this.crew = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      CrewService.Instance.GetCrew(this.id, includeRelations)
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
     this._crewChangeHistories = null;
     this._crewChangeHistoriesPromise = null;
     this._crewChangeHistoriesSubject.next(null);
     this._crewChangeHistoriesCount$ = null;

     this._crewMembers = null;
     this._crewMembersPromise = null;
     this._crewMembersSubject.next(null);
     this._crewMembersCount$ = null;

     this._scheduledEvents = null;
     this._scheduledEventsPromise = null;
     this._scheduledEventsSubject.next(null);
     this._scheduledEventsCount$ = null;

     this._eventResourceAssignments = null;
     this._eventResourceAssignmentsPromise = null;
     this._eventResourceAssignmentsSubject.next(null);
     this._eventResourceAssignmentsCount$ = null;

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
     * Gets the CrewChangeHistories for this Crew.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.crew.CrewChangeHistories.then(crews => { ... })
     *   or
     *   await this.crew.crews
     *
    */
    public get CrewChangeHistories(): Promise<CrewChangeHistoryData[]> {
        if (this._crewChangeHistories !== null) {
            return Promise.resolve(this._crewChangeHistories);
        }

        if (this._crewChangeHistoriesPromise !== null) {
            return this._crewChangeHistoriesPromise;
        }

        // Start the load
        this.loadCrewChangeHistories();

        return this._crewChangeHistoriesPromise!;
    }



    private loadCrewChangeHistories(): void {

        this._crewChangeHistoriesPromise = lastValueFrom(
            CrewService.Instance.GetCrewChangeHistoriesForCrew(this.id)
        )
        .then(CrewChangeHistories => {
            this._crewChangeHistories = CrewChangeHistories ?? [];
            this._crewChangeHistoriesSubject.next(this._crewChangeHistories);
            return this._crewChangeHistories;
         })
        .catch(err => {
            this._crewChangeHistories = [];
            this._crewChangeHistoriesSubject.next(this._crewChangeHistories);
            throw err;
        })
        .finally(() => {
            this._crewChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached CrewChangeHistory. Call after mutations to force refresh.
     */
    public ClearCrewChangeHistoriesCache(): void {
        this._crewChangeHistories = null;
        this._crewChangeHistoriesPromise = null;
        this._crewChangeHistoriesSubject.next(this._crewChangeHistories);      // Emit to observable
    }

    public get HasCrewChangeHistories(): Promise<boolean> {
        return this.CrewChangeHistories.then(crewChangeHistories => crewChangeHistories.length > 0);
    }


    /**
     *
     * Gets the CrewMembers for this Crew.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.crew.CrewMembers.then(crews => { ... })
     *   or
     *   await this.crew.crews
     *
    */
    public get CrewMembers(): Promise<CrewMemberData[]> {
        if (this._crewMembers !== null) {
            return Promise.resolve(this._crewMembers);
        }

        if (this._crewMembersPromise !== null) {
            return this._crewMembersPromise;
        }

        // Start the load
        this.loadCrewMembers();

        return this._crewMembersPromise!;
    }



    private loadCrewMembers(): void {

        this._crewMembersPromise = lastValueFrom(
            CrewService.Instance.GetCrewMembersForCrew(this.id)
        )
        .then(CrewMembers => {
            this._crewMembers = CrewMembers ?? [];
            this._crewMembersSubject.next(this._crewMembers);
            return this._crewMembers;
         })
        .catch(err => {
            this._crewMembers = [];
            this._crewMembersSubject.next(this._crewMembers);
            throw err;
        })
        .finally(() => {
            this._crewMembersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached CrewMember. Call after mutations to force refresh.
     */
    public ClearCrewMembersCache(): void {
        this._crewMembers = null;
        this._crewMembersPromise = null;
        this._crewMembersSubject.next(this._crewMembers);      // Emit to observable
    }

    public get HasCrewMembers(): Promise<boolean> {
        return this.CrewMembers.then(crewMembers => crewMembers.length > 0);
    }


    /**
     *
     * Gets the ScheduledEvents for this Crew.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.crew.ScheduledEvents.then(crews => { ... })
     *   or
     *   await this.crew.crews
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
            CrewService.Instance.GetScheduledEventsForCrew(this.id)
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


    /**
     *
     * Gets the EventResourceAssignments for this Crew.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.crew.EventResourceAssignments.then(crews => { ... })
     *   or
     *   await this.crew.crews
     *
    */
    public get EventResourceAssignments(): Promise<EventResourceAssignmentData[]> {
        if (this._eventResourceAssignments !== null) {
            return Promise.resolve(this._eventResourceAssignments);
        }

        if (this._eventResourceAssignmentsPromise !== null) {
            return this._eventResourceAssignmentsPromise;
        }

        // Start the load
        this.loadEventResourceAssignments();

        return this._eventResourceAssignmentsPromise!;
    }



    private loadEventResourceAssignments(): void {

        this._eventResourceAssignmentsPromise = lastValueFrom(
            CrewService.Instance.GetEventResourceAssignmentsForCrew(this.id)
        )
        .then(EventResourceAssignments => {
            this._eventResourceAssignments = EventResourceAssignments ?? [];
            this._eventResourceAssignmentsSubject.next(this._eventResourceAssignments);
            return this._eventResourceAssignments;
         })
        .catch(err => {
            this._eventResourceAssignments = [];
            this._eventResourceAssignmentsSubject.next(this._eventResourceAssignments);
            throw err;
        })
        .finally(() => {
            this._eventResourceAssignmentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventResourceAssignment. Call after mutations to force refresh.
     */
    public ClearEventResourceAssignmentsCache(): void {
        this._eventResourceAssignments = null;
        this._eventResourceAssignmentsPromise = null;
        this._eventResourceAssignmentsSubject.next(this._eventResourceAssignments);      // Emit to observable
    }

    public get HasEventResourceAssignments(): Promise<boolean> {
        return this.EventResourceAssignments.then(eventResourceAssignments => eventResourceAssignments.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (crew.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await crew.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<CrewData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<CrewData>> {
        const info = await lastValueFrom(
            CrewService.Instance.GetCrewChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this CrewData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this CrewData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): CrewSubmitData {
        return CrewService.Instance.ConvertToCrewSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class CrewService extends SecureEndpointBase {

    private static _instance: CrewService;
    private listCache: Map<string, Observable<Array<CrewData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<CrewBasicListData>>>;
    private recordCache: Map<string, Observable<CrewData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private crewChangeHistoryService: CrewChangeHistoryService,
        private crewMemberService: CrewMemberService,
        private scheduledEventService: ScheduledEventService,
        private eventResourceAssignmentService: EventResourceAssignmentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<CrewData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<CrewBasicListData>>>();
        this.recordCache = new Map<string, Observable<CrewData>>();

        CrewService._instance = this;
    }

    public static get Instance(): CrewService {
      return CrewService._instance;
    }


    public ClearListCaches(config: CrewQueryParameters | null = null) {

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


    public ConvertToCrewSubmitData(data: CrewData): CrewSubmitData {

        let output = new CrewSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.notes = data.notes;
        output.officeId = data.officeId;
        output.iconId = data.iconId;
        output.color = data.color;
        output.avatarFileName = data.avatarFileName;
        output.avatarSize = data.avatarSize;
        output.avatarData = data.avatarData;
        output.avatarMimeType = data.avatarMimeType;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetCrew(id: bigint | number, includeRelations: boolean = true) : Observable<CrewData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const crew$ = this.requestCrew(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Crew", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, crew$);

            return crew$;
        }

        return this.recordCache.get(configHash) as Observable<CrewData>;
    }

    private requestCrew(id: bigint | number, includeRelations: boolean = true) : Observable<CrewData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CrewData>(this.baseUrl + 'api/Crew/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveCrew(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestCrew(id, includeRelations));
            }));
    }

    public GetCrewList(config: CrewQueryParameters | any = null) : Observable<Array<CrewData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const crewList$ = this.requestCrewList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Crew list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, crewList$);

            return crewList$;
        }

        return this.listCache.get(configHash) as Observable<Array<CrewData>>;
    }


    private requestCrewList(config: CrewQueryParameters | any) : Observable <Array<CrewData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CrewData>>(this.baseUrl + 'api/Crews', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveCrewList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestCrewList(config));
            }));
    }

    public GetCrewsRowCount(config: CrewQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const crewsRowCount$ = this.requestCrewsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Crews row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, crewsRowCount$);

            return crewsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestCrewsRowCount(config: CrewQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Crews/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCrewsRowCount(config));
            }));
    }

    public GetCrewsBasicListData(config: CrewQueryParameters | any = null) : Observable<Array<CrewBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const crewsBasicListData$ = this.requestCrewsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Crews basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, crewsBasicListData$);

            return crewsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<CrewBasicListData>>;
    }


    private requestCrewsBasicListData(config: CrewQueryParameters | any) : Observable<Array<CrewBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CrewBasicListData>>(this.baseUrl + 'api/Crews/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCrewsBasicListData(config));
            }));

    }


    public PutCrew(id: bigint | number, crew: CrewSubmitData) : Observable<CrewData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CrewData>(this.baseUrl + 'api/Crew/' + id.toString(), crew, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCrew(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutCrew(id, crew));
            }));
    }


    public PostCrew(crew: CrewSubmitData) : Observable<CrewData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<CrewData>(this.baseUrl + 'api/Crew', crew, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCrew(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostCrew(crew));
            }));
    }

  
    public DeleteCrew(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Crew/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteCrew(id));
            }));
    }

    public RollbackCrew(id: bigint | number, versionNumber: bigint | number) : Observable<CrewData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CrewData>(this.baseUrl + 'api/Crew/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCrew(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackCrew(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Crew.
     */
    public GetCrewChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<CrewData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<CrewData>>(this.baseUrl + 'api/Crew/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetCrewChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Crew.
     */
    public GetCrewAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<CrewData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<CrewData>[]>(this.baseUrl + 'api/Crew/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetCrewAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Crew.
     */
    public GetCrewVersion(id: bigint | number, version: number): Observable<CrewData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CrewData>(this.baseUrl + 'api/Crew/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveCrew(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetCrewVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Crew at a specific point in time.
     */
    public GetCrewStateAtTime(id: bigint | number, time: string): Observable<CrewData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CrewData>(this.baseUrl + 'api/Crew/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveCrew(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetCrewStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: CrewQueryParameters | any): string {

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

    public userIsSchedulerCrewReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerCrewReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Crews
        //
        if (userIsSchedulerCrewReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerCrewReader = user.readPermission >= 1;
            } else {
                userIsSchedulerCrewReader = false;
            }
        }

        return userIsSchedulerCrewReader;
    }


    public userIsSchedulerCrewWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerCrewWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Crews
        //
        if (userIsSchedulerCrewWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerCrewWriter = user.writePermission >= 30;
          } else {
            userIsSchedulerCrewWriter = false;
          }      
        }

        return userIsSchedulerCrewWriter;
    }

    public GetCrewChangeHistoriesForCrew(crewId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CrewChangeHistoryData[]> {
        return this.crewChangeHistoryService.GetCrewChangeHistoryList({
            crewId: crewId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetCrewMembersForCrew(crewId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CrewMemberData[]> {
        return this.crewMemberService.GetCrewMemberList({
            crewId: crewId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventsForCrew(crewId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventData[]> {
        return this.scheduledEventService.GetScheduledEventList({
            crewId: crewId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventResourceAssignmentsForCrew(crewId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventResourceAssignmentData[]> {
        return this.eventResourceAssignmentService.GetEventResourceAssignmentList({
            crewId: crewId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full CrewData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the CrewData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when CrewTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveCrew(raw: any): CrewData {
    if (!raw) return raw;

    //
    // Create a CrewData object instance with correct prototype
    //
    const revived = Object.create(CrewData.prototype) as CrewData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._crewChangeHistories = null;
    (revived as any)._crewChangeHistoriesPromise = null;
    (revived as any)._crewChangeHistoriesSubject = new BehaviorSubject<CrewChangeHistoryData[] | null>(null);

    (revived as any)._crewMembers = null;
    (revived as any)._crewMembersPromise = null;
    (revived as any)._crewMembersSubject = new BehaviorSubject<CrewMemberData[] | null>(null);

    (revived as any)._scheduledEvents = null;
    (revived as any)._scheduledEventsPromise = null;
    (revived as any)._scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

    (revived as any)._eventResourceAssignments = null;
    (revived as any)._eventResourceAssignmentsPromise = null;
    (revived as any)._eventResourceAssignmentsSubject = new BehaviorSubject<EventResourceAssignmentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadCrewXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).CrewChangeHistories$ = (revived as any)._crewChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._crewChangeHistories === null && (revived as any)._crewChangeHistoriesPromise === null) {
                (revived as any).loadCrewChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._crewChangeHistoriesCount$ = null;


    (revived as any).CrewMembers$ = (revived as any)._crewMembersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._crewMembers === null && (revived as any)._crewMembersPromise === null) {
                (revived as any).loadCrewMembers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._crewMembersCount$ = null;


    (revived as any).ScheduledEvents$ = (revived as any)._scheduledEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEvents === null && (revived as any)._scheduledEventsPromise === null) {
                (revived as any).loadScheduledEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._scheduledEventsCount$ = null;


    (revived as any).EventResourceAssignments$ = (revived as any)._eventResourceAssignmentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventResourceAssignments === null && (revived as any)._eventResourceAssignmentsPromise === null) {
                (revived as any).loadEventResourceAssignments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._eventResourceAssignmentsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<CrewData> | null>(null);

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

  private ReviveCrewList(rawList: any[]): CrewData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveCrew(raw));
  }

}
