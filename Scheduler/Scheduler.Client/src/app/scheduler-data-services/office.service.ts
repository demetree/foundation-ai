/*

   GENERATED SERVICE FOR THE OFFICE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Office table.

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
import { OfficeTypeData } from './office-type.service';
import { TimeZoneData } from './time-zone.service';
import { CurrencyData } from './currency.service';
import { StateProvinceData } from './state-province.service';
import { CountryData } from './country.service';
import { OfficeChangeHistoryService, OfficeChangeHistoryData } from './office-change-history.service';
import { OfficeContactService, OfficeContactData } from './office-contact.service';
import { CalendarService, CalendarData } from './calendar.service';
import { SchedulingTargetService, SchedulingTargetData } from './scheduling-target.service';
import { ResourceService, ResourceData } from './resource.service';
import { RateSheetService, RateSheetData } from './rate-sheet.service';
import { CrewService, CrewData } from './crew.service';
import { ScheduledEventService, ScheduledEventData } from './scheduled-event.service';
import { GiftService, GiftData } from './gift.service';
import { VolunteerGroupService, VolunteerGroupData } from './volunteer-group.service';
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
export class OfficeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    officeTypeId: bigint | number | null | undefined = null;
    timeZoneId: bigint | number | null | undefined = null;
    currencyId: bigint | number | null | undefined = null;
    addressLine1: string | null | undefined = null;
    addressLine2: string | null | undefined = null;
    city: string | null | undefined = null;
    postalCode: string | null | undefined = null;
    stateProvinceId: bigint | number | null | undefined = null;
    countryId: bigint | number | null | undefined = null;
    phone: string | null | undefined = null;
    email: string | null | undefined = null;
    latitude: number | null | undefined = null;
    longitude: number | null | undefined = null;
    notes: string | null | undefined = null;
    externalId: string | null | undefined = null;
    color: string | null | undefined = null;
    attributes: string | null | undefined = null;
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
export class OfficeSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    officeTypeId!: bigint | number;
    timeZoneId!: bigint | number;
    currencyId!: bigint | number;
    addressLine1!: string;
    addressLine2: string | null = null;
    city!: string;
    postalCode: string | null = null;
    stateProvinceId!: bigint | number;
    countryId!: bigint | number;
    phone: string | null = null;
    email: string | null = null;
    latitude: number | null = null;
    longitude: number | null = null;
    notes: string | null = null;
    externalId: string | null = null;
    color: string | null = null;
    attributes: string | null = null;
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

export class OfficeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. OfficeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `office.OfficeChildren$` — use with `| async` in templates
//        • Promise:    `office.OfficeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="office.OfficeChildren$ | async"`), or
//        • Access the promise getter (`office.OfficeChildren` or `await office.OfficeChildren`)
//    - Simply reading `office.OfficeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await office.Reload()` to refresh the entire object and clear all lazy caches.
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
export class OfficeData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    officeTypeId!: bigint | number;
    timeZoneId!: bigint | number;
    currencyId!: bigint | number;
    addressLine1!: string;
    addressLine2!: string | null;
    city!: string;
    postalCode!: string | null;
    stateProvinceId!: bigint | number;
    countryId!: bigint | number;
    phone!: string | null;
    email!: string | null;
    latitude!: number | null;
    longitude!: number | null;
    notes!: string | null;
    externalId!: string | null;
    color!: string | null;
    attributes!: string | null;
    avatarFileName!: string | null;
    avatarSize!: bigint | number;
    avatarData!: string | null;
    avatarMimeType!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    country: CountryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    currency: CurrencyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    officeType: OfficeTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    stateProvince: StateProvinceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    timeZone: TimeZoneData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _officeChangeHistories: OfficeChangeHistoryData[] | null = null;
    private _officeChangeHistoriesPromise: Promise<OfficeChangeHistoryData[]> | null  = null;
    private _officeChangeHistoriesSubject = new BehaviorSubject<OfficeChangeHistoryData[] | null>(null);

                
    private _officeContacts: OfficeContactData[] | null = null;
    private _officeContactsPromise: Promise<OfficeContactData[]> | null  = null;
    private _officeContactsSubject = new BehaviorSubject<OfficeContactData[] | null>(null);

                
    private _calendars: CalendarData[] | null = null;
    private _calendarsPromise: Promise<CalendarData[]> | null  = null;
    private _calendarsSubject = new BehaviorSubject<CalendarData[] | null>(null);

                
    private _schedulingTargets: SchedulingTargetData[] | null = null;
    private _schedulingTargetsPromise: Promise<SchedulingTargetData[]> | null  = null;
    private _schedulingTargetsSubject = new BehaviorSubject<SchedulingTargetData[] | null>(null);

                
    private _resources: ResourceData[] | null = null;
    private _resourcesPromise: Promise<ResourceData[]> | null  = null;
    private _resourcesSubject = new BehaviorSubject<ResourceData[] | null>(null);

                
    private _rateSheets: RateSheetData[] | null = null;
    private _rateSheetsPromise: Promise<RateSheetData[]> | null  = null;
    private _rateSheetsSubject = new BehaviorSubject<RateSheetData[] | null>(null);

                
    private _crews: CrewData[] | null = null;
    private _crewsPromise: Promise<CrewData[]> | null  = null;
    private _crewsSubject = new BehaviorSubject<CrewData[] | null>(null);

                
    private _scheduledEvents: ScheduledEventData[] | null = null;
    private _scheduledEventsPromise: Promise<ScheduledEventData[]> | null  = null;
    private _scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

                
    private _gifts: GiftData[] | null = null;
    private _giftsPromise: Promise<GiftData[]> | null  = null;
    private _giftsSubject = new BehaviorSubject<GiftData[] | null>(null);

                
    private _volunteerGroups: VolunteerGroupData[] | null = null;
    private _volunteerGroupsPromise: Promise<VolunteerGroupData[]> | null  = null;
    private _volunteerGroupsSubject = new BehaviorSubject<VolunteerGroupData[] | null>(null);

                
    private _eventResourceAssignments: EventResourceAssignmentData[] | null = null;
    private _eventResourceAssignmentsPromise: Promise<EventResourceAssignmentData[]> | null  = null;
    private _eventResourceAssignmentsSubject = new BehaviorSubject<EventResourceAssignmentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<OfficeData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<OfficeData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<OfficeData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public OfficeChangeHistories$ = this._officeChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._officeChangeHistories === null && this._officeChangeHistoriesPromise === null) {
            this.loadOfficeChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public OfficeChangeHistoriesCount$ = OfficeChangeHistoryService.Instance.GetOfficeChangeHistoriesRowCount({officeId: this.id,
      active: true,
      deleted: false
    });



    public OfficeContacts$ = this._officeContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._officeContacts === null && this._officeContactsPromise === null) {
            this.loadOfficeContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public OfficeContactsCount$ = OfficeContactService.Instance.GetOfficeContactsRowCount({officeId: this.id,
      active: true,
      deleted: false
    });



    public Calendars$ = this._calendarsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._calendars === null && this._calendarsPromise === null) {
            this.loadCalendars(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public CalendarsCount$ = CalendarService.Instance.GetCalendarsRowCount({officeId: this.id,
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

  
    public SchedulingTargetsCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({officeId: this.id,
      active: true,
      deleted: false
    });



    public Resources$ = this._resourcesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resources === null && this._resourcesPromise === null) {
            this.loadResources(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ResourcesCount$ = ResourceService.Instance.GetResourcesRowCount({officeId: this.id,
      active: true,
      deleted: false
    });



    public RateSheets$ = this._rateSheetsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._rateSheets === null && this._rateSheetsPromise === null) {
            this.loadRateSheets(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public RateSheetsCount$ = RateSheetService.Instance.GetRateSheetsRowCount({officeId: this.id,
      active: true,
      deleted: false
    });



    public Crews$ = this._crewsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._crews === null && this._crewsPromise === null) {
            this.loadCrews(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public CrewsCount$ = CrewService.Instance.GetCrewsRowCount({officeId: this.id,
      active: true,
      deleted: false
    });



    public ScheduledEvents$ = this._scheduledEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEvents === null && this._scheduledEventsPromise === null) {
            this.loadScheduledEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({officeId: this.id,
      active: true,
      deleted: false
    });



    public Gifts$ = this._giftsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._gifts === null && this._giftsPromise === null) {
            this.loadGifts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public GiftsCount$ = GiftService.Instance.GetGiftsRowCount({officeId: this.id,
      active: true,
      deleted: false
    });



    public VolunteerGroups$ = this._volunteerGroupsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._volunteerGroups === null && this._volunteerGroupsPromise === null) {
            this.loadVolunteerGroups(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public VolunteerGroupsCount$ = VolunteerGroupService.Instance.GetVolunteerGroupsRowCount({officeId: this.id,
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

  
    public EventResourceAssignmentsCount$ = EventResourceAssignmentService.Instance.GetEventResourceAssignmentsRowCount({officeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any OfficeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.office.Reload();
  //
  //  Non Async:
  //
  //     office[0].Reload().then(x => {
  //        this.office = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      OfficeService.Instance.GetOffice(this.id, includeRelations)
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
     this._officeChangeHistories = null;
     this._officeChangeHistoriesPromise = null;
     this._officeChangeHistoriesSubject.next(null);

     this._officeContacts = null;
     this._officeContactsPromise = null;
     this._officeContactsSubject.next(null);

     this._calendars = null;
     this._calendarsPromise = null;
     this._calendarsSubject.next(null);

     this._schedulingTargets = null;
     this._schedulingTargetsPromise = null;
     this._schedulingTargetsSubject.next(null);

     this._resources = null;
     this._resourcesPromise = null;
     this._resourcesSubject.next(null);

     this._rateSheets = null;
     this._rateSheetsPromise = null;
     this._rateSheetsSubject.next(null);

     this._crews = null;
     this._crewsPromise = null;
     this._crewsSubject.next(null);

     this._scheduledEvents = null;
     this._scheduledEventsPromise = null;
     this._scheduledEventsSubject.next(null);

     this._gifts = null;
     this._giftsPromise = null;
     this._giftsSubject.next(null);

     this._volunteerGroups = null;
     this._volunteerGroupsPromise = null;
     this._volunteerGroupsSubject.next(null);

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
     * Gets the OfficeChangeHistories for this Office.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.office.OfficeChangeHistories.then(offices => { ... })
     *   or
     *   await this.office.offices
     *
    */
    public get OfficeChangeHistories(): Promise<OfficeChangeHistoryData[]> {
        if (this._officeChangeHistories !== null) {
            return Promise.resolve(this._officeChangeHistories);
        }

        if (this._officeChangeHistoriesPromise !== null) {
            return this._officeChangeHistoriesPromise;
        }

        // Start the load
        this.loadOfficeChangeHistories();

        return this._officeChangeHistoriesPromise!;
    }



    private loadOfficeChangeHistories(): void {

        this._officeChangeHistoriesPromise = lastValueFrom(
            OfficeService.Instance.GetOfficeChangeHistoriesForOffice(this.id)
        )
        .then(OfficeChangeHistories => {
            this._officeChangeHistories = OfficeChangeHistories ?? [];
            this._officeChangeHistoriesSubject.next(this._officeChangeHistories);
            return this._officeChangeHistories;
         })
        .catch(err => {
            this._officeChangeHistories = [];
            this._officeChangeHistoriesSubject.next(this._officeChangeHistories);
            throw err;
        })
        .finally(() => {
            this._officeChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached OfficeChangeHistory. Call after mutations to force refresh.
     */
    public ClearOfficeChangeHistoriesCache(): void {
        this._officeChangeHistories = null;
        this._officeChangeHistoriesPromise = null;
        this._officeChangeHistoriesSubject.next(this._officeChangeHistories);      // Emit to observable
    }

    public get HasOfficeChangeHistories(): Promise<boolean> {
        return this.OfficeChangeHistories.then(officeChangeHistories => officeChangeHistories.length > 0);
    }


    /**
     *
     * Gets the OfficeContacts for this Office.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.office.OfficeContacts.then(offices => { ... })
     *   or
     *   await this.office.offices
     *
    */
    public get OfficeContacts(): Promise<OfficeContactData[]> {
        if (this._officeContacts !== null) {
            return Promise.resolve(this._officeContacts);
        }

        if (this._officeContactsPromise !== null) {
            return this._officeContactsPromise;
        }

        // Start the load
        this.loadOfficeContacts();

        return this._officeContactsPromise!;
    }



    private loadOfficeContacts(): void {

        this._officeContactsPromise = lastValueFrom(
            OfficeService.Instance.GetOfficeContactsForOffice(this.id)
        )
        .then(OfficeContacts => {
            this._officeContacts = OfficeContacts ?? [];
            this._officeContactsSubject.next(this._officeContacts);
            return this._officeContacts;
         })
        .catch(err => {
            this._officeContacts = [];
            this._officeContactsSubject.next(this._officeContacts);
            throw err;
        })
        .finally(() => {
            this._officeContactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached OfficeContact. Call after mutations to force refresh.
     */
    public ClearOfficeContactsCache(): void {
        this._officeContacts = null;
        this._officeContactsPromise = null;
        this._officeContactsSubject.next(this._officeContacts);      // Emit to observable
    }

    public get HasOfficeContacts(): Promise<boolean> {
        return this.OfficeContacts.then(officeContacts => officeContacts.length > 0);
    }


    /**
     *
     * Gets the Calendars for this Office.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.office.Calendars.then(offices => { ... })
     *   or
     *   await this.office.offices
     *
    */
    public get Calendars(): Promise<CalendarData[]> {
        if (this._calendars !== null) {
            return Promise.resolve(this._calendars);
        }

        if (this._calendarsPromise !== null) {
            return this._calendarsPromise;
        }

        // Start the load
        this.loadCalendars();

        return this._calendarsPromise!;
    }



    private loadCalendars(): void {

        this._calendarsPromise = lastValueFrom(
            OfficeService.Instance.GetCalendarsForOffice(this.id)
        )
        .then(Calendars => {
            this._calendars = Calendars ?? [];
            this._calendarsSubject.next(this._calendars);
            return this._calendars;
         })
        .catch(err => {
            this._calendars = [];
            this._calendarsSubject.next(this._calendars);
            throw err;
        })
        .finally(() => {
            this._calendarsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Calendar. Call after mutations to force refresh.
     */
    public ClearCalendarsCache(): void {
        this._calendars = null;
        this._calendarsPromise = null;
        this._calendarsSubject.next(this._calendars);      // Emit to observable
    }

    public get HasCalendars(): Promise<boolean> {
        return this.Calendars.then(calendars => calendars.length > 0);
    }


    /**
     *
     * Gets the SchedulingTargets for this Office.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.office.SchedulingTargets.then(offices => { ... })
     *   or
     *   await this.office.offices
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
            OfficeService.Instance.GetSchedulingTargetsForOffice(this.id)
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
     * Gets the Resources for this Office.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.office.Resources.then(offices => { ... })
     *   or
     *   await this.office.offices
     *
    */
    public get Resources(): Promise<ResourceData[]> {
        if (this._resources !== null) {
            return Promise.resolve(this._resources);
        }

        if (this._resourcesPromise !== null) {
            return this._resourcesPromise;
        }

        // Start the load
        this.loadResources();

        return this._resourcesPromise!;
    }



    private loadResources(): void {

        this._resourcesPromise = lastValueFrom(
            OfficeService.Instance.GetResourcesForOffice(this.id)
        )
        .then(Resources => {
            this._resources = Resources ?? [];
            this._resourcesSubject.next(this._resources);
            return this._resources;
         })
        .catch(err => {
            this._resources = [];
            this._resourcesSubject.next(this._resources);
            throw err;
        })
        .finally(() => {
            this._resourcesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Resource. Call after mutations to force refresh.
     */
    public ClearResourcesCache(): void {
        this._resources = null;
        this._resourcesPromise = null;
        this._resourcesSubject.next(this._resources);      // Emit to observable
    }

    public get HasResources(): Promise<boolean> {
        return this.Resources.then(resources => resources.length > 0);
    }


    /**
     *
     * Gets the RateSheets for this Office.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.office.RateSheets.then(offices => { ... })
     *   or
     *   await this.office.offices
     *
    */
    public get RateSheets(): Promise<RateSheetData[]> {
        if (this._rateSheets !== null) {
            return Promise.resolve(this._rateSheets);
        }

        if (this._rateSheetsPromise !== null) {
            return this._rateSheetsPromise;
        }

        // Start the load
        this.loadRateSheets();

        return this._rateSheetsPromise!;
    }



    private loadRateSheets(): void {

        this._rateSheetsPromise = lastValueFrom(
            OfficeService.Instance.GetRateSheetsForOffice(this.id)
        )
        .then(RateSheets => {
            this._rateSheets = RateSheets ?? [];
            this._rateSheetsSubject.next(this._rateSheets);
            return this._rateSheets;
         })
        .catch(err => {
            this._rateSheets = [];
            this._rateSheetsSubject.next(this._rateSheets);
            throw err;
        })
        .finally(() => {
            this._rateSheetsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached RateSheet. Call after mutations to force refresh.
     */
    public ClearRateSheetsCache(): void {
        this._rateSheets = null;
        this._rateSheetsPromise = null;
        this._rateSheetsSubject.next(this._rateSheets);      // Emit to observable
    }

    public get HasRateSheets(): Promise<boolean> {
        return this.RateSheets.then(rateSheets => rateSheets.length > 0);
    }


    /**
     *
     * Gets the Crews for this Office.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.office.Crews.then(offices => { ... })
     *   or
     *   await this.office.offices
     *
    */
    public get Crews(): Promise<CrewData[]> {
        if (this._crews !== null) {
            return Promise.resolve(this._crews);
        }

        if (this._crewsPromise !== null) {
            return this._crewsPromise;
        }

        // Start the load
        this.loadCrews();

        return this._crewsPromise!;
    }



    private loadCrews(): void {

        this._crewsPromise = lastValueFrom(
            OfficeService.Instance.GetCrewsForOffice(this.id)
        )
        .then(Crews => {
            this._crews = Crews ?? [];
            this._crewsSubject.next(this._crews);
            return this._crews;
         })
        .catch(err => {
            this._crews = [];
            this._crewsSubject.next(this._crews);
            throw err;
        })
        .finally(() => {
            this._crewsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Crew. Call after mutations to force refresh.
     */
    public ClearCrewsCache(): void {
        this._crews = null;
        this._crewsPromise = null;
        this._crewsSubject.next(this._crews);      // Emit to observable
    }

    public get HasCrews(): Promise<boolean> {
        return this.Crews.then(crews => crews.length > 0);
    }


    /**
     *
     * Gets the ScheduledEvents for this Office.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.office.ScheduledEvents.then(offices => { ... })
     *   or
     *   await this.office.offices
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
            OfficeService.Instance.GetScheduledEventsForOffice(this.id)
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
     * Gets the Gifts for this Office.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.office.Gifts.then(offices => { ... })
     *   or
     *   await this.office.offices
     *
    */
    public get Gifts(): Promise<GiftData[]> {
        if (this._gifts !== null) {
            return Promise.resolve(this._gifts);
        }

        if (this._giftsPromise !== null) {
            return this._giftsPromise;
        }

        // Start the load
        this.loadGifts();

        return this._giftsPromise!;
    }



    private loadGifts(): void {

        this._giftsPromise = lastValueFrom(
            OfficeService.Instance.GetGiftsForOffice(this.id)
        )
        .then(Gifts => {
            this._gifts = Gifts ?? [];
            this._giftsSubject.next(this._gifts);
            return this._gifts;
         })
        .catch(err => {
            this._gifts = [];
            this._giftsSubject.next(this._gifts);
            throw err;
        })
        .finally(() => {
            this._giftsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Gift. Call after mutations to force refresh.
     */
    public ClearGiftsCache(): void {
        this._gifts = null;
        this._giftsPromise = null;
        this._giftsSubject.next(this._gifts);      // Emit to observable
    }

    public get HasGifts(): Promise<boolean> {
        return this.Gifts.then(gifts => gifts.length > 0);
    }


    /**
     *
     * Gets the VolunteerGroups for this Office.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.office.VolunteerGroups.then(offices => { ... })
     *   or
     *   await this.office.offices
     *
    */
    public get VolunteerGroups(): Promise<VolunteerGroupData[]> {
        if (this._volunteerGroups !== null) {
            return Promise.resolve(this._volunteerGroups);
        }

        if (this._volunteerGroupsPromise !== null) {
            return this._volunteerGroupsPromise;
        }

        // Start the load
        this.loadVolunteerGroups();

        return this._volunteerGroupsPromise!;
    }



    private loadVolunteerGroups(): void {

        this._volunteerGroupsPromise = lastValueFrom(
            OfficeService.Instance.GetVolunteerGroupsForOffice(this.id)
        )
        .then(VolunteerGroups => {
            this._volunteerGroups = VolunteerGroups ?? [];
            this._volunteerGroupsSubject.next(this._volunteerGroups);
            return this._volunteerGroups;
         })
        .catch(err => {
            this._volunteerGroups = [];
            this._volunteerGroupsSubject.next(this._volunteerGroups);
            throw err;
        })
        .finally(() => {
            this._volunteerGroupsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached VolunteerGroup. Call after mutations to force refresh.
     */
    public ClearVolunteerGroupsCache(): void {
        this._volunteerGroups = null;
        this._volunteerGroupsPromise = null;
        this._volunteerGroupsSubject.next(this._volunteerGroups);      // Emit to observable
    }

    public get HasVolunteerGroups(): Promise<boolean> {
        return this.VolunteerGroups.then(volunteerGroups => volunteerGroups.length > 0);
    }


    /**
     *
     * Gets the EventResourceAssignments for this Office.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.office.EventResourceAssignments.then(offices => { ... })
     *   or
     *   await this.office.offices
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
            OfficeService.Instance.GetEventResourceAssignmentsForOffice(this.id)
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
    //   Template: {{ (office.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await office.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<OfficeData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<OfficeData>> {
        const info = await lastValueFrom(
            OfficeService.Instance.GetOfficeChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this OfficeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this OfficeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): OfficeSubmitData {
        return OfficeService.Instance.ConvertToOfficeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class OfficeService extends SecureEndpointBase {

    private static _instance: OfficeService;
    private listCache: Map<string, Observable<Array<OfficeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<OfficeBasicListData>>>;
    private recordCache: Map<string, Observable<OfficeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private officeChangeHistoryService: OfficeChangeHistoryService,
        private officeContactService: OfficeContactService,
        private calendarService: CalendarService,
        private schedulingTargetService: SchedulingTargetService,
        private resourceService: ResourceService,
        private rateSheetService: RateSheetService,
        private crewService: CrewService,
        private scheduledEventService: ScheduledEventService,
        private giftService: GiftService,
        private volunteerGroupService: VolunteerGroupService,
        private eventResourceAssignmentService: EventResourceAssignmentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<OfficeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<OfficeBasicListData>>>();
        this.recordCache = new Map<string, Observable<OfficeData>>();

        OfficeService._instance = this;
    }

    public static get Instance(): OfficeService {
      return OfficeService._instance;
    }


    public ClearListCaches(config: OfficeQueryParameters | null = null) {

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


    public ConvertToOfficeSubmitData(data: OfficeData): OfficeSubmitData {

        let output = new OfficeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.officeTypeId = data.officeTypeId;
        output.timeZoneId = data.timeZoneId;
        output.currencyId = data.currencyId;
        output.addressLine1 = data.addressLine1;
        output.addressLine2 = data.addressLine2;
        output.city = data.city;
        output.postalCode = data.postalCode;
        output.stateProvinceId = data.stateProvinceId;
        output.countryId = data.countryId;
        output.phone = data.phone;
        output.email = data.email;
        output.latitude = data.latitude;
        output.longitude = data.longitude;
        output.notes = data.notes;
        output.externalId = data.externalId;
        output.color = data.color;
        output.attributes = data.attributes;
        output.avatarFileName = data.avatarFileName;
        output.avatarSize = data.avatarSize;
        output.avatarData = data.avatarData;
        output.avatarMimeType = data.avatarMimeType;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetOffice(id: bigint | number, includeRelations: boolean = true) : Observable<OfficeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const office$ = this.requestOffice(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Office", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, office$);

            return office$;
        }

        return this.recordCache.get(configHash) as Observable<OfficeData>;
    }

    private requestOffice(id: bigint | number, includeRelations: boolean = true) : Observable<OfficeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<OfficeData>(this.baseUrl + 'api/Office/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveOffice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestOffice(id, includeRelations));
            }));
    }

    public GetOfficeList(config: OfficeQueryParameters | any = null) : Observable<Array<OfficeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const officeList$ = this.requestOfficeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Office list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, officeList$);

            return officeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<OfficeData>>;
    }


    private requestOfficeList(config: OfficeQueryParameters | any) : Observable <Array<OfficeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<OfficeData>>(this.baseUrl + 'api/Offices', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveOfficeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeList(config));
            }));
    }

    public GetOfficesRowCount(config: OfficeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const officesRowCount$ = this.requestOfficesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Offices row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, officesRowCount$);

            return officesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestOfficesRowCount(config: OfficeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Offices/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestOfficesRowCount(config));
            }));
    }

    public GetOfficesBasicListData(config: OfficeQueryParameters | any = null) : Observable<Array<OfficeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const officesBasicListData$ = this.requestOfficesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Offices basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, officesBasicListData$);

            return officesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<OfficeBasicListData>>;
    }


    private requestOfficesBasicListData(config: OfficeQueryParameters | any) : Observable<Array<OfficeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<OfficeBasicListData>>(this.baseUrl + 'api/Offices/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestOfficesBasicListData(config));
            }));

    }


    public PutOffice(id: bigint | number, office: OfficeSubmitData) : Observable<OfficeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<OfficeData>(this.baseUrl + 'api/Office/' + id.toString(), office, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOffice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutOffice(id, office));
            }));
    }


    public PostOffice(office: OfficeSubmitData) : Observable<OfficeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<OfficeData>(this.baseUrl + 'api/Office', office, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOffice(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostOffice(office));
            }));
    }

  
    public DeleteOffice(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Office/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteOffice(id));
            }));
    }

    public RollbackOffice(id: bigint | number, versionNumber: bigint | number) : Observable<OfficeData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<OfficeData>(this.baseUrl + 'api/Office/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOffice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackOffice(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Office.
     */
    public GetOfficeChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<OfficeData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<OfficeData>>(this.baseUrl + 'api/Office/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetOfficeChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Office.
     */
    public GetOfficeAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<OfficeData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<OfficeData>[]>(this.baseUrl + 'api/Office/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetOfficeAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Office.
     */
    public GetOfficeVersion(id: bigint | number, version: number): Observable<OfficeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<OfficeData>(this.baseUrl + 'api/Office/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveOffice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetOfficeVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Office at a specific point in time.
     */
    public GetOfficeStateAtTime(id: bigint | number, time: string): Observable<OfficeData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<OfficeData>(this.baseUrl + 'api/Office/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveOffice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetOfficeStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: OfficeQueryParameters | any): string {

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

    public userIsSchedulerOfficeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerOfficeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Offices
        //
        if (userIsSchedulerOfficeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerOfficeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerOfficeReader = false;
            }
        }

        return userIsSchedulerOfficeReader;
    }


    public userIsSchedulerOfficeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerOfficeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Offices
        //
        if (userIsSchedulerOfficeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerOfficeWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerOfficeWriter = false;
          }      
        }

        return userIsSchedulerOfficeWriter;
    }

    public GetOfficeChangeHistoriesForOffice(officeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<OfficeChangeHistoryData[]> {
        return this.officeChangeHistoryService.GetOfficeChangeHistoryList({
            officeId: officeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetOfficeContactsForOffice(officeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<OfficeContactData[]> {
        return this.officeContactService.GetOfficeContactList({
            officeId: officeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetCalendarsForOffice(officeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CalendarData[]> {
        return this.calendarService.GetCalendarList({
            officeId: officeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetsForOffice(officeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetData[]> {
        return this.schedulingTargetService.GetSchedulingTargetList({
            officeId: officeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetResourcesForOffice(officeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceData[]> {
        return this.resourceService.GetResourceList({
            officeId: officeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetRateSheetsForOffice(officeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<RateSheetData[]> {
        return this.rateSheetService.GetRateSheetList({
            officeId: officeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetCrewsForOffice(officeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CrewData[]> {
        return this.crewService.GetCrewList({
            officeId: officeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventsForOffice(officeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventData[]> {
        return this.scheduledEventService.GetScheduledEventList({
            officeId: officeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetGiftsForOffice(officeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GiftData[]> {
        return this.giftService.GetGiftList({
            officeId: officeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetVolunteerGroupsForOffice(officeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<VolunteerGroupData[]> {
        return this.volunteerGroupService.GetVolunteerGroupList({
            officeId: officeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventResourceAssignmentsForOffice(officeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventResourceAssignmentData[]> {
        return this.eventResourceAssignmentService.GetEventResourceAssignmentList({
            officeId: officeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full OfficeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the OfficeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when OfficeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveOffice(raw: any): OfficeData {
    if (!raw) return raw;

    //
    // Create a OfficeData object instance with correct prototype
    //
    const revived = Object.create(OfficeData.prototype) as OfficeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._officeChangeHistories = null;
    (revived as any)._officeChangeHistoriesPromise = null;
    (revived as any)._officeChangeHistoriesSubject = new BehaviorSubject<OfficeChangeHistoryData[] | null>(null);

    (revived as any)._officeContacts = null;
    (revived as any)._officeContactsPromise = null;
    (revived as any)._officeContactsSubject = new BehaviorSubject<OfficeContactData[] | null>(null);

    (revived as any)._calendars = null;
    (revived as any)._calendarsPromise = null;
    (revived as any)._calendarsSubject = new BehaviorSubject<CalendarData[] | null>(null);

    (revived as any)._schedulingTargets = null;
    (revived as any)._schedulingTargetsPromise = null;
    (revived as any)._schedulingTargetsSubject = new BehaviorSubject<SchedulingTargetData[] | null>(null);

    (revived as any)._resources = null;
    (revived as any)._resourcesPromise = null;
    (revived as any)._resourcesSubject = new BehaviorSubject<ResourceData[] | null>(null);

    (revived as any)._rateSheets = null;
    (revived as any)._rateSheetsPromise = null;
    (revived as any)._rateSheetsSubject = new BehaviorSubject<RateSheetData[] | null>(null);

    (revived as any)._crews = null;
    (revived as any)._crewsPromise = null;
    (revived as any)._crewsSubject = new BehaviorSubject<CrewData[] | null>(null);

    (revived as any)._scheduledEvents = null;
    (revived as any)._scheduledEventsPromise = null;
    (revived as any)._scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

    (revived as any)._gifts = null;
    (revived as any)._giftsPromise = null;
    (revived as any)._giftsSubject = new BehaviorSubject<GiftData[] | null>(null);

    (revived as any)._volunteerGroups = null;
    (revived as any)._volunteerGroupsPromise = null;
    (revived as any)._volunteerGroupsSubject = new BehaviorSubject<VolunteerGroupData[] | null>(null);

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
    // 2. But private methods (loadOfficeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).OfficeChangeHistories$ = (revived as any)._officeChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._officeChangeHistories === null && (revived as any)._officeChangeHistoriesPromise === null) {
                (revived as any).loadOfficeChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).OfficeChangeHistoriesCount$ = OfficeChangeHistoryService.Instance.GetOfficeChangeHistoriesRowCount({officeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).OfficeContacts$ = (revived as any)._officeContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._officeContacts === null && (revived as any)._officeContactsPromise === null) {
                (revived as any).loadOfficeContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).OfficeContactsCount$ = OfficeContactService.Instance.GetOfficeContactsRowCount({officeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Calendars$ = (revived as any)._calendarsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._calendars === null && (revived as any)._calendarsPromise === null) {
                (revived as any).loadCalendars();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).CalendarsCount$ = CalendarService.Instance.GetCalendarsRowCount({officeId: (revived as any).id,
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

    (revived as any).SchedulingTargetsCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({officeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Resources$ = (revived as any)._resourcesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resources === null && (revived as any)._resourcesPromise === null) {
                (revived as any).loadResources();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ResourcesCount$ = ResourceService.Instance.GetResourcesRowCount({officeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).RateSheets$ = (revived as any)._rateSheetsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._rateSheets === null && (revived as any)._rateSheetsPromise === null) {
                (revived as any).loadRateSheets();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).RateSheetsCount$ = RateSheetService.Instance.GetRateSheetsRowCount({officeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Crews$ = (revived as any)._crewsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._crews === null && (revived as any)._crewsPromise === null) {
                (revived as any).loadCrews();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).CrewsCount$ = CrewService.Instance.GetCrewsRowCount({officeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduledEvents$ = (revived as any)._scheduledEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEvents === null && (revived as any)._scheduledEventsPromise === null) {
                (revived as any).loadScheduledEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({officeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Gifts$ = (revived as any)._giftsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._gifts === null && (revived as any)._giftsPromise === null) {
                (revived as any).loadGifts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).GiftsCount$ = GiftService.Instance.GetGiftsRowCount({officeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).VolunteerGroups$ = (revived as any)._volunteerGroupsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._volunteerGroups === null && (revived as any)._volunteerGroupsPromise === null) {
                (revived as any).loadVolunteerGroups();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).VolunteerGroupsCount$ = VolunteerGroupService.Instance.GetVolunteerGroupsRowCount({officeId: (revived as any).id,
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

    (revived as any).EventResourceAssignmentsCount$ = EventResourceAssignmentService.Instance.GetEventResourceAssignmentsRowCount({officeId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<OfficeData> | null>(null);

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

  private ReviveOfficeList(rawList: any[]): OfficeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveOffice(raw));
  }

}
