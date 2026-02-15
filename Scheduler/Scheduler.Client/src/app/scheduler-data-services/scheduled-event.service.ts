/*

   GENERATED SERVICE FOR THE SCHEDULEDEVENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ScheduledEvent table.

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
import { ClientData } from './client.service';
import { ScheduledEventTemplateData } from './scheduled-event-template.service';
import { RecurrenceRuleData } from './recurrence-rule.service';
import { SchedulingTargetData } from './scheduling-target.service';
import { TimeZoneData } from './time-zone.service';
import { EventStatusData } from './event-status.service';
import { ResourceData } from './resource.service';
import { CrewData } from './crew.service';
import { PriorityData } from './priority.service';
import { BookingSourceTypeData } from './booking-source-type.service';
import { ScheduledEventChangeHistoryService, ScheduledEventChangeHistoryData } from './scheduled-event-change-history.service';
import { EventChargeService, EventChargeData } from './event-charge.service';
import { ContactInteractionService, ContactInteractionData } from './contact-interaction.service';
import { EventCalendarService, EventCalendarData } from './event-calendar.service';
import { ScheduledEventDependencyService, ScheduledEventDependencyData } from './scheduled-event-dependency.service';
import { ScheduledEventQualificationRequirementService, ScheduledEventQualificationRequirementData } from './scheduled-event-qualification-requirement.service';
import { RecurrenceExceptionService, RecurrenceExceptionData } from './recurrence-exception.service';
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
export class ScheduledEventQueryParameters {
    officeId: bigint | number | null | undefined = null;
    clientId: bigint | number | null | undefined = null;
    scheduledEventTemplateId: bigint | number | null | undefined = null;
    recurrenceRuleId: bigint | number | null | undefined = null;
    schedulingTargetId: bigint | number | null | undefined = null;
    timeZoneId: bigint | number | null | undefined = null;
    parentScheduledEventId: bigint | number | null | undefined = null;
    recurrenceInstanceDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    isAllDay: boolean | null | undefined = null;
    startDateTime: string | null | undefined = null;        // ISO 8601 (full datetime)
    endDateTime: string | null | undefined = null;        // ISO 8601 (full datetime)
    location: string | null | undefined = null;
    eventStatusId: bigint | number | null | undefined = null;
    resourceId: bigint | number | null | undefined = null;
    crewId: bigint | number | null | undefined = null;
    priorityId: bigint | number | null | undefined = null;
    bookingSourceTypeId: bigint | number | null | undefined = null;
    partySize: bigint | number | null | undefined = null;
    notes: string | null | undefined = null;
    color: string | null | undefined = null;
    externalId: string | null | undefined = null;
    attributes: string | null | undefined = null;
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
export class ScheduledEventSubmitData {
    id!: bigint | number;
    officeId: bigint | number | null = null;
    clientId: bigint | number | null = null;
    scheduledEventTemplateId: bigint | number | null = null;
    recurrenceRuleId: bigint | number | null = null;
    schedulingTargetId: bigint | number | null = null;
    timeZoneId: bigint | number | null = null;
    parentScheduledEventId: bigint | number | null = null;
    recurrenceInstanceDate: string | null = null;     // ISO 8601 (full datetime)
    name!: string;
    description: string | null = null;
    isAllDay: boolean | null = null;
    startDateTime!: string;      // ISO 8601 (full datetime)
    endDateTime!: string;      // ISO 8601 (full datetime)
    location: string | null = null;
    eventStatusId!: bigint | number;
    resourceId: bigint | number | null = null;
    crewId: bigint | number | null = null;
    priorityId: bigint | number | null = null;
    bookingSourceTypeId: bigint | number | null = null;
    partySize: bigint | number | null = null;
    notes: string | null = null;
    color: string | null = null;
    externalId: string | null = null;
    attributes: string | null = null;
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

export class ScheduledEventBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ScheduledEventChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `scheduledEvent.ScheduledEventChildren$` — use with `| async` in templates
//        • Promise:    `scheduledEvent.ScheduledEventChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="scheduledEvent.ScheduledEventChildren$ | async"`), or
//        • Access the promise getter (`scheduledEvent.ScheduledEventChildren` or `await scheduledEvent.ScheduledEventChildren`)
//    - Simply reading `scheduledEvent.ScheduledEventChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await scheduledEvent.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ScheduledEventData {
    id!: bigint | number;
    officeId!: bigint | number;
    clientId!: bigint | number;
    scheduledEventTemplateId!: bigint | number;
    recurrenceRuleId!: bigint | number;
    schedulingTargetId!: bigint | number;
    timeZoneId!: bigint | number;
    parentScheduledEventId!: bigint | number;
    recurrenceInstanceDate!: string | null;   // ISO 8601 (full datetime)
    name!: string;
    description!: string | null;
    isAllDay!: boolean | null;
    startDateTime!: string;      // ISO 8601 (full datetime)
    endDateTime!: string;      // ISO 8601 (full datetime)
    location!: string | null;
    eventStatusId!: bigint | number;
    resourceId!: bigint | number;
    crewId!: bigint | number;
    priorityId!: bigint | number;
    bookingSourceTypeId!: bigint | number;
    partySize!: bigint | number;
    notes!: string | null;
    color!: string | null;
    externalId!: string | null;
    attributes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    bookingSourceType: BookingSourceTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    client: ClientData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    crew: CrewData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    eventStatus: EventStatusData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    office: OfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    priority: PriorityData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    recurrenceRule: RecurrenceRuleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduledEventTemplate: ScheduledEventTemplateData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    schedulingTarget: SchedulingTargetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    timeZone: TimeZoneData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    parentScheduledEvent: ScheduledEventData | null | undefined = null;            // Self referencing navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _scheduledEventChangeHistories: ScheduledEventChangeHistoryData[] | null = null;
    private _scheduledEventChangeHistoriesPromise: Promise<ScheduledEventChangeHistoryData[]> | null  = null;
    private _scheduledEventChangeHistoriesSubject = new BehaviorSubject<ScheduledEventChangeHistoryData[] | null>(null);

                
    private _eventCharges: EventChargeData[] | null = null;
    private _eventChargesPromise: Promise<EventChargeData[]> | null  = null;
    private _eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);

                
    private _contactInteractions: ContactInteractionData[] | null = null;
    private _contactInteractionsPromise: Promise<ContactInteractionData[]> | null  = null;
    private _contactInteractionsSubject = new BehaviorSubject<ContactInteractionData[] | null>(null);

                
    private _eventCalendars: EventCalendarData[] | null = null;
    private _eventCalendarsPromise: Promise<EventCalendarData[]> | null  = null;
    private _eventCalendarsSubject = new BehaviorSubject<EventCalendarData[] | null>(null);

                
    private _scheduledEventDependencyPredecessorEvents: ScheduledEventDependencyData[] | null = null;
    private _scheduledEventDependencyPredecessorEventsPromise: Promise<ScheduledEventDependencyData[]> | null  = null;
    private _scheduledEventDependencyPredecessorEventsSubject = new BehaviorSubject<ScheduledEventDependencyData[] | null>(null);
                    
    private _scheduledEventDependencySuccessorEvents: ScheduledEventDependencyData[] | null = null;
    private _scheduledEventDependencySuccessorEventsPromise: Promise<ScheduledEventDependencyData[]> | null  = null;
    private _scheduledEventDependencySuccessorEventsSubject = new BehaviorSubject<ScheduledEventDependencyData[] | null>(null);
                    
    private _scheduledEventQualificationRequirements: ScheduledEventQualificationRequirementData[] | null = null;
    private _scheduledEventQualificationRequirementsPromise: Promise<ScheduledEventQualificationRequirementData[]> | null  = null;
    private _scheduledEventQualificationRequirementsSubject = new BehaviorSubject<ScheduledEventQualificationRequirementData[] | null>(null);

                
    private _recurrenceExceptions: RecurrenceExceptionData[] | null = null;
    private _recurrenceExceptionsPromise: Promise<RecurrenceExceptionData[]> | null  = null;
    private _recurrenceExceptionsSubject = new BehaviorSubject<RecurrenceExceptionData[] | null>(null);

                
    private _eventResourceAssignments: EventResourceAssignmentData[] | null = null;
    private _eventResourceAssignmentsPromise: Promise<EventResourceAssignmentData[]> | null  = null;
    private _eventResourceAssignmentsSubject = new BehaviorSubject<EventResourceAssignmentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ScheduledEventData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ScheduledEventData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduledEventData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ScheduledEventChangeHistories$ = this._scheduledEventChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventChangeHistories === null && this._scheduledEventChangeHistoriesPromise === null) {
            this.loadScheduledEventChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventChangeHistoriesCount$ = ScheduledEventChangeHistoryService.Instance.GetScheduledEventChangeHistoriesRowCount({scheduledEventId: this.id,
      active: true,
      deleted: false
    });



    public EventCharges$ = this._eventChargesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventCharges === null && this._eventChargesPromise === null) {
            this.loadEventCharges(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public EventChargesCount$ = EventChargeService.Instance.GetEventChargesRowCount({scheduledEventId: this.id,
      active: true,
      deleted: false
    });



    public ContactInteractions$ = this._contactInteractionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactInteractions === null && this._contactInteractionsPromise === null) {
            this.loadContactInteractions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ContactInteractionsCount$ = ContactInteractionService.Instance.GetContactInteractionsRowCount({scheduledEventId: this.id,
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

  
    public EventCalendarsCount$ = EventCalendarService.Instance.GetEventCalendarsRowCount({scheduledEventId: this.id,
      active: true,
      deleted: false
    });



    public ScheduledEventDependencyPredecessorEvents$ = this._scheduledEventDependencyPredecessorEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventDependencyPredecessorEvents === null && this._scheduledEventDependencyPredecessorEventsPromise === null) {
            this.loadScheduledEventDependencyPredecessorEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventDependencyPredecessorEventsCount$ = ScheduledEventDependencyService.Instance.GetScheduledEventDependenciesRowCount({predecessorEventId: this.id,
      active: true,
      deleted: false
    });


    public ScheduledEventDependencySuccessorEvents$ = this._scheduledEventDependencySuccessorEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventDependencySuccessorEvents === null && this._scheduledEventDependencySuccessorEventsPromise === null) {
            this.loadScheduledEventDependencySuccessorEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventDependencySuccessorEventsCount$ = ScheduledEventDependencyService.Instance.GetScheduledEventDependenciesRowCount({successorEventId: this.id,
      active: true,
      deleted: false
    });


    public ScheduledEventQualificationRequirements$ = this._scheduledEventQualificationRequirementsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventQualificationRequirements === null && this._scheduledEventQualificationRequirementsPromise === null) {
            this.loadScheduledEventQualificationRequirements(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventQualificationRequirementsCount$ = ScheduledEventQualificationRequirementService.Instance.GetScheduledEventQualificationRequirementsRowCount({scheduledEventId: this.id,
      active: true,
      deleted: false
    });



    public RecurrenceExceptions$ = this._recurrenceExceptionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._recurrenceExceptions === null && this._recurrenceExceptionsPromise === null) {
            this.loadRecurrenceExceptions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public RecurrenceExceptionsCount$ = RecurrenceExceptionService.Instance.GetRecurrenceExceptionsRowCount({scheduledEventId: this.id,
      active: true,
      deleted: false
    });



    public EventResourceAssignments$ = this._eventResourceAssignmentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventResourceAssignments === null && this._eventResourceAssignmentsPromise === null) {
            this.loadEventResourceAssignments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public EventResourceAssignmentsCount$ = EventResourceAssignmentService.Instance.GetEventResourceAssignmentsRowCount({scheduledEventId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ScheduledEventData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.scheduledEvent.Reload();
  //
  //  Non Async:
  //
  //     scheduledEvent[0].Reload().then(x => {
  //        this.scheduledEvent = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ScheduledEventService.Instance.GetScheduledEvent(this.id, includeRelations)
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
     this._scheduledEventChangeHistories = null;
     this._scheduledEventChangeHistoriesPromise = null;
     this._scheduledEventChangeHistoriesSubject.next(null);

     this._eventCharges = null;
     this._eventChargesPromise = null;
     this._eventChargesSubject.next(null);

     this._contactInteractions = null;
     this._contactInteractionsPromise = null;
     this._contactInteractionsSubject.next(null);

     this._eventCalendars = null;
     this._eventCalendarsPromise = null;
     this._eventCalendarsSubject.next(null);

     this._scheduledEventDependencyPredecessorEvents = null;
     this._scheduledEventDependencyPredecessorEventsPromise = null;
     this._scheduledEventDependencyPredecessorEventsSubject.next(null);

     this._scheduledEventDependencySuccessorEvents = null;
     this._scheduledEventDependencySuccessorEventsPromise = null;
     this._scheduledEventDependencySuccessorEventsSubject.next(null);

     this._scheduledEventQualificationRequirements = null;
     this._scheduledEventQualificationRequirementsPromise = null;
     this._scheduledEventQualificationRequirementsSubject.next(null);

     this._recurrenceExceptions = null;
     this._recurrenceExceptionsPromise = null;
     this._recurrenceExceptionsSubject.next(null);

     this._eventResourceAssignments = null;
     this._eventResourceAssignmentsPromise = null;
     this._eventResourceAssignmentsSubject.next(null);

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
     * Gets the ScheduledEventChangeHistories for this ScheduledEvent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEvent.ScheduledEventChangeHistories.then(scheduledEvents => { ... })
     *   or
     *   await this.scheduledEvent.scheduledEvents
     *
    */
    public get ScheduledEventChangeHistories(): Promise<ScheduledEventChangeHistoryData[]> {
        if (this._scheduledEventChangeHistories !== null) {
            return Promise.resolve(this._scheduledEventChangeHistories);
        }

        if (this._scheduledEventChangeHistoriesPromise !== null) {
            return this._scheduledEventChangeHistoriesPromise;
        }

        // Start the load
        this.loadScheduledEventChangeHistories();

        return this._scheduledEventChangeHistoriesPromise!;
    }



    private loadScheduledEventChangeHistories(): void {

        this._scheduledEventChangeHistoriesPromise = lastValueFrom(
            ScheduledEventService.Instance.GetScheduledEventChangeHistoriesForScheduledEvent(this.id)
        )
        .then(ScheduledEventChangeHistories => {
            this._scheduledEventChangeHistories = ScheduledEventChangeHistories ?? [];
            this._scheduledEventChangeHistoriesSubject.next(this._scheduledEventChangeHistories);
            return this._scheduledEventChangeHistories;
         })
        .catch(err => {
            this._scheduledEventChangeHistories = [];
            this._scheduledEventChangeHistoriesSubject.next(this._scheduledEventChangeHistories);
            throw err;
        })
        .finally(() => {
            this._scheduledEventChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventChangeHistory. Call after mutations to force refresh.
     */
    public ClearScheduledEventChangeHistoriesCache(): void {
        this._scheduledEventChangeHistories = null;
        this._scheduledEventChangeHistoriesPromise = null;
        this._scheduledEventChangeHistoriesSubject.next(this._scheduledEventChangeHistories);      // Emit to observable
    }

    public get HasScheduledEventChangeHistories(): Promise<boolean> {
        return this.ScheduledEventChangeHistories.then(scheduledEventChangeHistories => scheduledEventChangeHistories.length > 0);
    }


    /**
     *
     * Gets the EventCharges for this ScheduledEvent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEvent.EventCharges.then(scheduledEvents => { ... })
     *   or
     *   await this.scheduledEvent.scheduledEvents
     *
    */
    public get EventCharges(): Promise<EventChargeData[]> {
        if (this._eventCharges !== null) {
            return Promise.resolve(this._eventCharges);
        }

        if (this._eventChargesPromise !== null) {
            return this._eventChargesPromise;
        }

        // Start the load
        this.loadEventCharges();

        return this._eventChargesPromise!;
    }



    private loadEventCharges(): void {

        this._eventChargesPromise = lastValueFrom(
            ScheduledEventService.Instance.GetEventChargesForScheduledEvent(this.id)
        )
        .then(EventCharges => {
            this._eventCharges = EventCharges ?? [];
            this._eventChargesSubject.next(this._eventCharges);
            return this._eventCharges;
         })
        .catch(err => {
            this._eventCharges = [];
            this._eventChargesSubject.next(this._eventCharges);
            throw err;
        })
        .finally(() => {
            this._eventChargesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventCharge. Call after mutations to force refresh.
     */
    public ClearEventChargesCache(): void {
        this._eventCharges = null;
        this._eventChargesPromise = null;
        this._eventChargesSubject.next(this._eventCharges);      // Emit to observable
    }

    public get HasEventCharges(): Promise<boolean> {
        return this.EventCharges.then(eventCharges => eventCharges.length > 0);
    }


    /**
     *
     * Gets the ContactInteractions for this ScheduledEvent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEvent.ContactInteractions.then(scheduledEvents => { ... })
     *   or
     *   await this.scheduledEvent.scheduledEvents
     *
    */
    public get ContactInteractions(): Promise<ContactInteractionData[]> {
        if (this._contactInteractions !== null) {
            return Promise.resolve(this._contactInteractions);
        }

        if (this._contactInteractionsPromise !== null) {
            return this._contactInteractionsPromise;
        }

        // Start the load
        this.loadContactInteractions();

        return this._contactInteractionsPromise!;
    }



    private loadContactInteractions(): void {

        this._contactInteractionsPromise = lastValueFrom(
            ScheduledEventService.Instance.GetContactInteractionsForScheduledEvent(this.id)
        )
        .then(ContactInteractions => {
            this._contactInteractions = ContactInteractions ?? [];
            this._contactInteractionsSubject.next(this._contactInteractions);
            return this._contactInteractions;
         })
        .catch(err => {
            this._contactInteractions = [];
            this._contactInteractionsSubject.next(this._contactInteractions);
            throw err;
        })
        .finally(() => {
            this._contactInteractionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContactInteraction. Call after mutations to force refresh.
     */
    public ClearContactInteractionsCache(): void {
        this._contactInteractions = null;
        this._contactInteractionsPromise = null;
        this._contactInteractionsSubject.next(this._contactInteractions);      // Emit to observable
    }

    public get HasContactInteractions(): Promise<boolean> {
        return this.ContactInteractions.then(contactInteractions => contactInteractions.length > 0);
    }


    /**
     *
     * Gets the EventCalendars for this ScheduledEvent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEvent.EventCalendars.then(scheduledEvents => { ... })
     *   or
     *   await this.scheduledEvent.scheduledEvents
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
            ScheduledEventService.Instance.GetEventCalendarsForScheduledEvent(this.id)
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
     *
     * Gets the ScheduledEventDependencyPredecessorEvents for this ScheduledEvent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEvent.ScheduledEventDependencyPredecessorEvents.then(predecessorEvents => { ... })
     *   or
     *   await this.scheduledEvent.predecessorEvents
     *
    */
    public get ScheduledEventDependencyPredecessorEvents(): Promise<ScheduledEventDependencyData[]> {
        if (this._scheduledEventDependencyPredecessorEvents !== null) {
            return Promise.resolve(this._scheduledEventDependencyPredecessorEvents);
        }

        if (this._scheduledEventDependencyPredecessorEventsPromise !== null) {
            return this._scheduledEventDependencyPredecessorEventsPromise;
        }

        // Start the load
        this.loadScheduledEventDependencyPredecessorEvents();

        return this._scheduledEventDependencyPredecessorEventsPromise!;
    }



    private loadScheduledEventDependencyPredecessorEvents(): void {

        this._scheduledEventDependencyPredecessorEventsPromise = lastValueFrom(
            ScheduledEventService.Instance.GetScheduledEventDependencyPredecessorEventsForScheduledEvent(this.id)
        )
        .then(ScheduledEventDependencyPredecessorEvents => {
            this._scheduledEventDependencyPredecessorEvents = ScheduledEventDependencyPredecessorEvents ?? [];
            this._scheduledEventDependencyPredecessorEventsSubject.next(this._scheduledEventDependencyPredecessorEvents);
            return this._scheduledEventDependencyPredecessorEvents;
         })
        .catch(err => {
            this._scheduledEventDependencyPredecessorEvents = [];
            this._scheduledEventDependencyPredecessorEventsSubject.next(this._scheduledEventDependencyPredecessorEvents);
            throw err;
        })
        .finally(() => {
            this._scheduledEventDependencyPredecessorEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventDependencyPredecessorEvent. Call after mutations to force refresh.
     */
    public ClearScheduledEventDependencyPredecessorEventsCache(): void {
        this._scheduledEventDependencyPredecessorEvents = null;
        this._scheduledEventDependencyPredecessorEventsPromise = null;
        this._scheduledEventDependencyPredecessorEventsSubject.next(this._scheduledEventDependencyPredecessorEvents);      // Emit to observable
    }

    public get HasScheduledEventDependencyPredecessorEvents(): Promise<boolean> {
        return this.ScheduledEventDependencyPredecessorEvents.then(scheduledEventDependencyPredecessorEvents => scheduledEventDependencyPredecessorEvents.length > 0);
    }


    /**
     *
     * Gets the ScheduledEventDependencySuccessorEvents for this ScheduledEvent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEvent.ScheduledEventDependencySuccessorEvents.then(successorEvents => { ... })
     *   or
     *   await this.scheduledEvent.successorEvents
     *
    */
    public get ScheduledEventDependencySuccessorEvents(): Promise<ScheduledEventDependencyData[]> {
        if (this._scheduledEventDependencySuccessorEvents !== null) {
            return Promise.resolve(this._scheduledEventDependencySuccessorEvents);
        }

        if (this._scheduledEventDependencySuccessorEventsPromise !== null) {
            return this._scheduledEventDependencySuccessorEventsPromise;
        }

        // Start the load
        this.loadScheduledEventDependencySuccessorEvents();

        return this._scheduledEventDependencySuccessorEventsPromise!;
    }



    private loadScheduledEventDependencySuccessorEvents(): void {

        this._scheduledEventDependencySuccessorEventsPromise = lastValueFrom(
            ScheduledEventService.Instance.GetScheduledEventDependencySuccessorEventsForScheduledEvent(this.id)
        )
        .then(ScheduledEventDependencySuccessorEvents => {
            this._scheduledEventDependencySuccessorEvents = ScheduledEventDependencySuccessorEvents ?? [];
            this._scheduledEventDependencySuccessorEventsSubject.next(this._scheduledEventDependencySuccessorEvents);
            return this._scheduledEventDependencySuccessorEvents;
         })
        .catch(err => {
            this._scheduledEventDependencySuccessorEvents = [];
            this._scheduledEventDependencySuccessorEventsSubject.next(this._scheduledEventDependencySuccessorEvents);
            throw err;
        })
        .finally(() => {
            this._scheduledEventDependencySuccessorEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventDependencySuccessorEvent. Call after mutations to force refresh.
     */
    public ClearScheduledEventDependencySuccessorEventsCache(): void {
        this._scheduledEventDependencySuccessorEvents = null;
        this._scheduledEventDependencySuccessorEventsPromise = null;
        this._scheduledEventDependencySuccessorEventsSubject.next(this._scheduledEventDependencySuccessorEvents);      // Emit to observable
    }

    public get HasScheduledEventDependencySuccessorEvents(): Promise<boolean> {
        return this.ScheduledEventDependencySuccessorEvents.then(scheduledEventDependencySuccessorEvents => scheduledEventDependencySuccessorEvents.length > 0);
    }


    /**
     *
     * Gets the ScheduledEventQualificationRequirements for this ScheduledEvent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEvent.ScheduledEventQualificationRequirements.then(scheduledEvents => { ... })
     *   or
     *   await this.scheduledEvent.scheduledEvents
     *
    */
    public get ScheduledEventQualificationRequirements(): Promise<ScheduledEventQualificationRequirementData[]> {
        if (this._scheduledEventQualificationRequirements !== null) {
            return Promise.resolve(this._scheduledEventQualificationRequirements);
        }

        if (this._scheduledEventQualificationRequirementsPromise !== null) {
            return this._scheduledEventQualificationRequirementsPromise;
        }

        // Start the load
        this.loadScheduledEventQualificationRequirements();

        return this._scheduledEventQualificationRequirementsPromise!;
    }



    private loadScheduledEventQualificationRequirements(): void {

        this._scheduledEventQualificationRequirementsPromise = lastValueFrom(
            ScheduledEventService.Instance.GetScheduledEventQualificationRequirementsForScheduledEvent(this.id)
        )
        .then(ScheduledEventQualificationRequirements => {
            this._scheduledEventQualificationRequirements = ScheduledEventQualificationRequirements ?? [];
            this._scheduledEventQualificationRequirementsSubject.next(this._scheduledEventQualificationRequirements);
            return this._scheduledEventQualificationRequirements;
         })
        .catch(err => {
            this._scheduledEventQualificationRequirements = [];
            this._scheduledEventQualificationRequirementsSubject.next(this._scheduledEventQualificationRequirements);
            throw err;
        })
        .finally(() => {
            this._scheduledEventQualificationRequirementsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventQualificationRequirement. Call after mutations to force refresh.
     */
    public ClearScheduledEventQualificationRequirementsCache(): void {
        this._scheduledEventQualificationRequirements = null;
        this._scheduledEventQualificationRequirementsPromise = null;
        this._scheduledEventQualificationRequirementsSubject.next(this._scheduledEventQualificationRequirements);      // Emit to observable
    }

    public get HasScheduledEventQualificationRequirements(): Promise<boolean> {
        return this.ScheduledEventQualificationRequirements.then(scheduledEventQualificationRequirements => scheduledEventQualificationRequirements.length > 0);
    }


    /**
     *
     * Gets the RecurrenceExceptions for this ScheduledEvent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEvent.RecurrenceExceptions.then(scheduledEvents => { ... })
     *   or
     *   await this.scheduledEvent.scheduledEvents
     *
    */
    public get RecurrenceExceptions(): Promise<RecurrenceExceptionData[]> {
        if (this._recurrenceExceptions !== null) {
            return Promise.resolve(this._recurrenceExceptions);
        }

        if (this._recurrenceExceptionsPromise !== null) {
            return this._recurrenceExceptionsPromise;
        }

        // Start the load
        this.loadRecurrenceExceptions();

        return this._recurrenceExceptionsPromise!;
    }



    private loadRecurrenceExceptions(): void {

        this._recurrenceExceptionsPromise = lastValueFrom(
            ScheduledEventService.Instance.GetRecurrenceExceptionsForScheduledEvent(this.id)
        )
        .then(RecurrenceExceptions => {
            this._recurrenceExceptions = RecurrenceExceptions ?? [];
            this._recurrenceExceptionsSubject.next(this._recurrenceExceptions);
            return this._recurrenceExceptions;
         })
        .catch(err => {
            this._recurrenceExceptions = [];
            this._recurrenceExceptionsSubject.next(this._recurrenceExceptions);
            throw err;
        })
        .finally(() => {
            this._recurrenceExceptionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached RecurrenceException. Call after mutations to force refresh.
     */
    public ClearRecurrenceExceptionsCache(): void {
        this._recurrenceExceptions = null;
        this._recurrenceExceptionsPromise = null;
        this._recurrenceExceptionsSubject.next(this._recurrenceExceptions);      // Emit to observable
    }

    public get HasRecurrenceExceptions(): Promise<boolean> {
        return this.RecurrenceExceptions.then(recurrenceExceptions => recurrenceExceptions.length > 0);
    }


    /**
     *
     * Gets the EventResourceAssignments for this ScheduledEvent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEvent.EventResourceAssignments.then(scheduledEvents => { ... })
     *   or
     *   await this.scheduledEvent.scheduledEvents
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
            ScheduledEventService.Instance.GetEventResourceAssignmentsForScheduledEvent(this.id)
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
    //   Template: {{ (scheduledEvent.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await scheduledEvent.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ScheduledEventData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ScheduledEventData>> {
        const info = await lastValueFrom(
            ScheduledEventService.Instance.GetScheduledEventChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ScheduledEventData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ScheduledEventData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ScheduledEventSubmitData {
        return ScheduledEventService.Instance.ConvertToScheduledEventSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ScheduledEventService extends SecureEndpointBase {

    private static _instance: ScheduledEventService;
    private listCache: Map<string, Observable<Array<ScheduledEventData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ScheduledEventBasicListData>>>;
    private recordCache: Map<string, Observable<ScheduledEventData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private scheduledEventChangeHistoryService: ScheduledEventChangeHistoryService,
        private eventChargeService: EventChargeService,
        private contactInteractionService: ContactInteractionService,
        private eventCalendarService: EventCalendarService,
        private scheduledEventDependencyService: ScheduledEventDependencyService,
        private scheduledEventQualificationRequirementService: ScheduledEventQualificationRequirementService,
        private recurrenceExceptionService: RecurrenceExceptionService,
        private eventResourceAssignmentService: EventResourceAssignmentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ScheduledEventData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ScheduledEventBasicListData>>>();
        this.recordCache = new Map<string, Observable<ScheduledEventData>>();

        ScheduledEventService._instance = this;
    }

    public static get Instance(): ScheduledEventService {
      return ScheduledEventService._instance;
    }


    public ClearListCaches(config: ScheduledEventQueryParameters | null = null) {

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


    public ConvertToScheduledEventSubmitData(data: ScheduledEventData): ScheduledEventSubmitData {

        let output = new ScheduledEventSubmitData();

        output.id = data.id;
        output.officeId = data.officeId;
        output.clientId = data.clientId;
        output.scheduledEventTemplateId = data.scheduledEventTemplateId;
        output.recurrenceRuleId = data.recurrenceRuleId;
        output.schedulingTargetId = data.schedulingTargetId;
        output.timeZoneId = data.timeZoneId;
        output.parentScheduledEventId = data.parentScheduledEventId;
        output.recurrenceInstanceDate = data.recurrenceInstanceDate;
        output.name = data.name;
        output.description = data.description;
        output.isAllDay = data.isAllDay;
        output.startDateTime = data.startDateTime;
        output.endDateTime = data.endDateTime;
        output.location = data.location;
        output.eventStatusId = data.eventStatusId;
        output.resourceId = data.resourceId;
        output.crewId = data.crewId;
        output.priorityId = data.priorityId;
        output.bookingSourceTypeId = data.bookingSourceTypeId;
        output.partySize = data.partySize;
        output.notes = data.notes;
        output.color = data.color;
        output.externalId = data.externalId;
        output.attributes = data.attributes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetScheduledEvent(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const scheduledEvent$ = this.requestScheduledEvent(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEvent", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, scheduledEvent$);

            return scheduledEvent$;
        }

        return this.recordCache.get(configHash) as Observable<ScheduledEventData>;
    }

    private requestScheduledEvent(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventData>(this.baseUrl + 'api/ScheduledEvent/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveScheduledEvent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEvent(id, includeRelations));
            }));
    }

    public GetScheduledEventList(config: ScheduledEventQueryParameters | any = null) : Observable<Array<ScheduledEventData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const scheduledEventList$ = this.requestScheduledEventList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEvent list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, scheduledEventList$);

            return scheduledEventList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ScheduledEventData>>;
    }


    private requestScheduledEventList(config: ScheduledEventQueryParameters | any) : Observable <Array<ScheduledEventData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventData>>(this.baseUrl + 'api/ScheduledEvents', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveScheduledEventList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventList(config));
            }));
    }

    public GetScheduledEventsRowCount(config: ScheduledEventQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const scheduledEventsRowCount$ = this.requestScheduledEventsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEvents row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, scheduledEventsRowCount$);

            return scheduledEventsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestScheduledEventsRowCount(config: ScheduledEventQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ScheduledEvents/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventsRowCount(config));
            }));
    }

    public GetScheduledEventsBasicListData(config: ScheduledEventQueryParameters | any = null) : Observable<Array<ScheduledEventBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const scheduledEventsBasicListData$ = this.requestScheduledEventsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEvents basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, scheduledEventsBasicListData$);

            return scheduledEventsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ScheduledEventBasicListData>>;
    }


    private requestScheduledEventsBasicListData(config: ScheduledEventQueryParameters | any) : Observable<Array<ScheduledEventBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventBasicListData>>(this.baseUrl + 'api/ScheduledEvents/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventsBasicListData(config));
            }));

    }


    public PutScheduledEvent(id: bigint | number, scheduledEvent: ScheduledEventSubmitData) : Observable<ScheduledEventData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventData>(this.baseUrl + 'api/ScheduledEvent/' + id.toString(), scheduledEvent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEvent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutScheduledEvent(id, scheduledEvent));
            }));
    }


    public PostScheduledEvent(scheduledEvent: ScheduledEventSubmitData) : Observable<ScheduledEventData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ScheduledEventData>(this.baseUrl + 'api/ScheduledEvent', scheduledEvent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEvent(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostScheduledEvent(scheduledEvent));
            }));
    }

  
    public DeleteScheduledEvent(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ScheduledEvent/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteScheduledEvent(id));
            }));
    }

    public RollbackScheduledEvent(id: bigint | number, versionNumber: bigint | number) : Observable<ScheduledEventData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventData>(this.baseUrl + 'api/ScheduledEvent/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEvent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackScheduledEvent(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ScheduledEvent.
     */
    public GetScheduledEventChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ScheduledEventData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduledEventData>>(this.baseUrl + 'api/ScheduledEvent/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ScheduledEvent.
     */
    public GetScheduledEventAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ScheduledEventData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduledEventData>[]>(this.baseUrl + 'api/ScheduledEvent/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ScheduledEvent.
     */
    public GetScheduledEventVersion(id: bigint | number, version: number): Observable<ScheduledEventData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventData>(this.baseUrl + 'api/ScheduledEvent/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduledEvent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ScheduledEvent at a specific point in time.
     */
    public GetScheduledEventStateAtTime(id: bigint | number, time: string): Observable<ScheduledEventData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventData>(this.baseUrl + 'api/ScheduledEvent/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduledEvent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ScheduledEventQueryParameters | any): string {

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

    public userIsSchedulerScheduledEventReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerScheduledEventReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ScheduledEvents
        //
        if (userIsSchedulerScheduledEventReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerScheduledEventReader = user.readPermission >= 1;
            } else {
                userIsSchedulerScheduledEventReader = false;
            }
        }

        return userIsSchedulerScheduledEventReader;
    }


    public userIsSchedulerScheduledEventWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerScheduledEventWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ScheduledEvents
        //
        if (userIsSchedulerScheduledEventWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerScheduledEventWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerScheduledEventWriter = false;
          }      
        }

        return userIsSchedulerScheduledEventWriter;
    }

    public GetScheduledEventChangeHistoriesForScheduledEvent(scheduledEventId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventChangeHistoryData[]> {
        return this.scheduledEventChangeHistoryService.GetScheduledEventChangeHistoryList({
            scheduledEventId: scheduledEventId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventChargesForScheduledEvent(scheduledEventId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventChargeData[]> {
        return this.eventChargeService.GetEventChargeList({
            scheduledEventId: scheduledEventId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetContactInteractionsForScheduledEvent(scheduledEventId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactInteractionData[]> {
        return this.contactInteractionService.GetContactInteractionList({
            scheduledEventId: scheduledEventId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventCalendarsForScheduledEvent(scheduledEventId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventCalendarData[]> {
        return this.eventCalendarService.GetEventCalendarList({
            scheduledEventId: scheduledEventId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventDependencyPredecessorEventsForScheduledEvent(scheduledEventId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventDependencyData[]> {
        return this.scheduledEventDependencyService.GetScheduledEventDependencyList({
            predecessorEventId: scheduledEventId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventDependencySuccessorEventsForScheduledEvent(scheduledEventId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventDependencyData[]> {
        return this.scheduledEventDependencyService.GetScheduledEventDependencyList({
            successorEventId: scheduledEventId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventQualificationRequirementsForScheduledEvent(scheduledEventId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventQualificationRequirementData[]> {
        return this.scheduledEventQualificationRequirementService.GetScheduledEventQualificationRequirementList({
            scheduledEventId: scheduledEventId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetRecurrenceExceptionsForScheduledEvent(scheduledEventId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<RecurrenceExceptionData[]> {
        return this.recurrenceExceptionService.GetRecurrenceExceptionList({
            scheduledEventId: scheduledEventId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventResourceAssignmentsForScheduledEvent(scheduledEventId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventResourceAssignmentData[]> {
        return this.eventResourceAssignmentService.GetEventResourceAssignmentList({
            scheduledEventId: scheduledEventId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ScheduledEventData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ScheduledEventData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ScheduledEventTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveScheduledEvent(raw: any): ScheduledEventData {
    if (!raw) return raw;

    //
    // Create a ScheduledEventData object instance with correct prototype
    //
    const revived = Object.create(ScheduledEventData.prototype) as ScheduledEventData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._scheduledEvents = null;
    (revived as any)._scheduledEventsPromise = null;
    (revived as any)._scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

    (revived as any)._scheduledEventChangeHistories = null;
    (revived as any)._scheduledEventChangeHistoriesPromise = null;
    (revived as any)._scheduledEventChangeHistoriesSubject = new BehaviorSubject<ScheduledEventChangeHistoryData[] | null>(null);

    (revived as any)._eventCharges = null;
    (revived as any)._eventChargesPromise = null;
    (revived as any)._eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);

    (revived as any)._contactInteractions = null;
    (revived as any)._contactInteractionsPromise = null;
    (revived as any)._contactInteractionsSubject = new BehaviorSubject<ContactInteractionData[] | null>(null);

    (revived as any)._eventCalendars = null;
    (revived as any)._eventCalendarsPromise = null;
    (revived as any)._eventCalendarsSubject = new BehaviorSubject<EventCalendarData[] | null>(null);

    (revived as any)._scheduledEventDependencies = null;
    (revived as any)._scheduledEventDependenciesPromise = null;
    (revived as any)._scheduledEventDependenciesSubject = new BehaviorSubject<ScheduledEventDependencyData[] | null>(null);

    (revived as any)._scheduledEventQualificationRequirements = null;
    (revived as any)._scheduledEventQualificationRequirementsPromise = null;
    (revived as any)._scheduledEventQualificationRequirementsSubject = new BehaviorSubject<ScheduledEventQualificationRequirementData[] | null>(null);

    (revived as any)._recurrenceExceptions = null;
    (revived as any)._recurrenceExceptionsPromise = null;
    (revived as any)._recurrenceExceptionsSubject = new BehaviorSubject<RecurrenceExceptionData[] | null>(null);

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
    // 2. But private methods (loadScheduledEventXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ScheduledEvents$ = (revived as any)._scheduledEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEvents === null && (revived as any)._scheduledEventsPromise === null) {
                (revived as any).loadScheduledEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({scheduledEventId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduledEventChangeHistories$ = (revived as any)._scheduledEventChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventChangeHistories === null && (revived as any)._scheduledEventChangeHistoriesPromise === null) {
                (revived as any).loadScheduledEventChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventChangeHistoriesCount$ = ScheduledEventChangeHistoryService.Instance.GetScheduledEventChangeHistoriesRowCount({scheduledEventId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).EventCharges$ = (revived as any)._eventChargesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventCharges === null && (revived as any)._eventChargesPromise === null) {
                (revived as any).loadEventCharges();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).EventChargesCount$ = EventChargeService.Instance.GetEventChargesRowCount({scheduledEventId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ContactInteractions$ = (revived as any)._contactInteractionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactInteractions === null && (revived as any)._contactInteractionsPromise === null) {
                (revived as any).loadContactInteractions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ContactInteractionsCount$ = ContactInteractionService.Instance.GetContactInteractionsRowCount({scheduledEventId: (revived as any).id,
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

    (revived as any).EventCalendarsCount$ = EventCalendarService.Instance.GetEventCalendarsRowCount({scheduledEventId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduledEventDependencies$ = (revived as any)._scheduledEventDependenciesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventDependencies === null && (revived as any)._scheduledEventDependenciesPromise === null) {
                (revived as any).loadScheduledEventDependencies();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventDependenciesCount$ = ScheduledEventDependencyService.Instance.GetScheduledEventDependenciesRowCount({scheduledEventId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduledEventQualificationRequirements$ = (revived as any)._scheduledEventQualificationRequirementsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventQualificationRequirements === null && (revived as any)._scheduledEventQualificationRequirementsPromise === null) {
                (revived as any).loadScheduledEventQualificationRequirements();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventQualificationRequirementsCount$ = ScheduledEventQualificationRequirementService.Instance.GetScheduledEventQualificationRequirementsRowCount({scheduledEventId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).RecurrenceExceptions$ = (revived as any)._recurrenceExceptionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._recurrenceExceptions === null && (revived as any)._recurrenceExceptionsPromise === null) {
                (revived as any).loadRecurrenceExceptions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).RecurrenceExceptionsCount$ = RecurrenceExceptionService.Instance.GetRecurrenceExceptionsRowCount({scheduledEventId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).EventResourceAssignments$ = (revived as any)._eventResourceAssignmentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventResourceAssignments === null && (revived as any)._eventResourceAssignmentsPromise === null) {
                (revived as any).loadEventResourceAssignments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).EventResourceAssignmentsCount$ = EventResourceAssignmentService.Instance.GetEventResourceAssignmentsRowCount({scheduledEventId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduledEventData> | null>(null);

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

  private ReviveScheduledEventList(rawList: any[]): ScheduledEventData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveScheduledEvent(raw));
  }

}
