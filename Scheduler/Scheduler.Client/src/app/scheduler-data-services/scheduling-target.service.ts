/*

   GENERATED SERVICE FOR THE SCHEDULINGTARGET TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SchedulingTarget table.

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
import { SchedulingTargetTypeData } from './scheduling-target-type.service';
import { TimeZoneData } from './time-zone.service';
import { CalendarData } from './calendar.service';
import { SchedulingTargetChangeHistoryService, SchedulingTargetChangeHistoryData } from './scheduling-target-change-history.service';
import { SchedulingTargetContactService, SchedulingTargetContactData } from './scheduling-target-contact.service';
import { SchedulingTargetAddressService, SchedulingTargetAddressData } from './scheduling-target-address.service';
import { SchedulingTargetQualificationRequirementService, SchedulingTargetQualificationRequirementData } from './scheduling-target-qualification-requirement.service';
import { RateSheetService, RateSheetData } from './rate-sheet.service';
import { ScheduledEventService, ScheduledEventData } from './scheduled-event.service';
import { HouseholdService, HouseholdData } from './household.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SchedulingTargetQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    officeId: bigint | number | null | undefined = null;
    clientId: bigint | number | null | undefined = null;
    schedulingTargetTypeId: bigint | number | null | undefined = null;
    timeZoneId: bigint | number | null | undefined = null;
    calendarId: bigint | number | null | undefined = null;
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
export class SchedulingTargetSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    officeId: bigint | number | null = null;
    clientId!: bigint | number;
    schedulingTargetTypeId!: bigint | number;
    timeZoneId!: bigint | number;
    calendarId: bigint | number | null = null;
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


export class SchedulingTargetBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SchedulingTargetChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `schedulingTarget.SchedulingTargetChildren$` — use with `| async` in templates
//        • Promise:    `schedulingTarget.SchedulingTargetChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="schedulingTarget.SchedulingTargetChildren$ | async"`), or
//        • Access the promise getter (`schedulingTarget.SchedulingTargetChildren` or `await schedulingTarget.SchedulingTargetChildren`)
//    - Simply reading `schedulingTarget.SchedulingTargetChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await schedulingTarget.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SchedulingTargetData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    officeId!: bigint | number;
    clientId!: bigint | number;
    schedulingTargetTypeId!: bigint | number;
    timeZoneId!: bigint | number;
    calendarId!: bigint | number;
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
    calendar: CalendarData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    client: ClientData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    office: OfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    schedulingTargetType: SchedulingTargetTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    timeZone: TimeZoneData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _schedulingTargetChangeHistories: SchedulingTargetChangeHistoryData[] | null = null;
    private _schedulingTargetChangeHistoriesPromise: Promise<SchedulingTargetChangeHistoryData[]> | null  = null;
    private _schedulingTargetChangeHistoriesSubject = new BehaviorSubject<SchedulingTargetChangeHistoryData[] | null>(null);

    private _schedulingTargetContacts: SchedulingTargetContactData[] | null = null;
    private _schedulingTargetContactsPromise: Promise<SchedulingTargetContactData[]> | null  = null;
    private _schedulingTargetContactsSubject = new BehaviorSubject<SchedulingTargetContactData[] | null>(null);

    private _schedulingTargetAddresses: SchedulingTargetAddressData[] | null = null;
    private _schedulingTargetAddressesPromise: Promise<SchedulingTargetAddressData[]> | null  = null;
    private _schedulingTargetAddressesSubject = new BehaviorSubject<SchedulingTargetAddressData[] | null>(null);

    private _schedulingTargetQualificationRequirements: SchedulingTargetQualificationRequirementData[] | null = null;
    private _schedulingTargetQualificationRequirementsPromise: Promise<SchedulingTargetQualificationRequirementData[]> | null  = null;
    private _schedulingTargetQualificationRequirementsSubject = new BehaviorSubject<SchedulingTargetQualificationRequirementData[] | null>(null);

    private _rateSheets: RateSheetData[] | null = null;
    private _rateSheetsPromise: Promise<RateSheetData[]> | null  = null;
    private _rateSheetsSubject = new BehaviorSubject<RateSheetData[] | null>(null);

    private _scheduledEvents: ScheduledEventData[] | null = null;
    private _scheduledEventsPromise: Promise<ScheduledEventData[]> | null  = null;
    private _scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

    private _households: HouseholdData[] | null = null;
    private _householdsPromise: Promise<HouseholdData[]> | null  = null;
    private _householdsSubject = new BehaviorSubject<HouseholdData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SchedulingTargetChangeHistories$ = this._schedulingTargetChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetChangeHistories === null && this._schedulingTargetChangeHistoriesPromise === null) {
            this.loadSchedulingTargetChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SchedulingTargetChangeHistoriesCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({schedulingTargetId: this.id,
      active: true,
      deleted: false
    });



    public SchedulingTargetContacts$ = this._schedulingTargetContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetContacts === null && this._schedulingTargetContactsPromise === null) {
            this.loadSchedulingTargetContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SchedulingTargetContactsCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({schedulingTargetId: this.id,
      active: true,
      deleted: false
    });



    public SchedulingTargetAddresses$ = this._schedulingTargetAddressesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetAddresses === null && this._schedulingTargetAddressesPromise === null) {
            this.loadSchedulingTargetAddresses(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SchedulingTargetAddressesCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({schedulingTargetId: this.id,
      active: true,
      deleted: false
    });



    public SchedulingTargetQualificationRequirements$ = this._schedulingTargetQualificationRequirementsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetQualificationRequirements === null && this._schedulingTargetQualificationRequirementsPromise === null) {
            this.loadSchedulingTargetQualificationRequirements(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SchedulingTargetQualificationRequirementsCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({schedulingTargetId: this.id,
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

  
    public RateSheetsCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({schedulingTargetId: this.id,
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

  
    public ScheduledEventsCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({schedulingTargetId: this.id,
      active: true,
      deleted: false
    });



    public Households$ = this._householdsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._households === null && this._householdsPromise === null) {
            this.loadHouseholds(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public HouseholdsCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({schedulingTargetId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SchedulingTargetData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.schedulingTarget.Reload();
  //
  //  Non Async:
  //
  //     schedulingTarget[0].Reload().then(x => {
  //        this.schedulingTarget = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SchedulingTargetService.Instance.GetSchedulingTarget(this.id, includeRelations)
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
     this._schedulingTargetChangeHistories = null;
     this._schedulingTargetChangeHistoriesPromise = null;
     this._schedulingTargetChangeHistoriesSubject.next(null);

     this._schedulingTargetContacts = null;
     this._schedulingTargetContactsPromise = null;
     this._schedulingTargetContactsSubject.next(null);

     this._schedulingTargetAddresses = null;
     this._schedulingTargetAddressesPromise = null;
     this._schedulingTargetAddressesSubject.next(null);

     this._schedulingTargetQualificationRequirements = null;
     this._schedulingTargetQualificationRequirementsPromise = null;
     this._schedulingTargetQualificationRequirementsSubject.next(null);

     this._rateSheets = null;
     this._rateSheetsPromise = null;
     this._rateSheetsSubject.next(null);

     this._scheduledEvents = null;
     this._scheduledEventsPromise = null;
     this._scheduledEventsSubject.next(null);

     this._households = null;
     this._householdsPromise = null;
     this._householdsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the SchedulingTargetChangeHistories for this SchedulingTarget.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.schedulingTarget.SchedulingTargetChangeHistories.then(schedulingTargetChangeHistories => { ... })
     *   or
     *   await this.schedulingTarget.SchedulingTargetChangeHistories
     *
    */
    public get SchedulingTargetChangeHistories(): Promise<SchedulingTargetChangeHistoryData[]> {
        if (this._schedulingTargetChangeHistories !== null) {
            return Promise.resolve(this._schedulingTargetChangeHistories);
        }

        if (this._schedulingTargetChangeHistoriesPromise !== null) {
            return this._schedulingTargetChangeHistoriesPromise;
        }

        // Start the load
        this.loadSchedulingTargetChangeHistories();

        return this._schedulingTargetChangeHistoriesPromise!;
    }



    private loadSchedulingTargetChangeHistories(): void {

        this._schedulingTargetChangeHistoriesPromise = lastValueFrom(
            SchedulingTargetService.Instance.GetSchedulingTargetChangeHistoriesForSchedulingTarget(this.id)
        )
        .then(schedulingTargetChangeHistories => {
            this._schedulingTargetChangeHistories = schedulingTargetChangeHistories ?? [];
            this._schedulingTargetChangeHistoriesSubject.next(this._schedulingTargetChangeHistories);
            return this._schedulingTargetChangeHistories;
         })
        .catch(err => {
            this._schedulingTargetChangeHistories = [];
            this._schedulingTargetChangeHistoriesSubject.next(this._schedulingTargetChangeHistories);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTargetChangeHistory. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetChangeHistoriesCache(): void {
        this._schedulingTargetChangeHistories = null;
        this._schedulingTargetChangeHistoriesPromise = null;
        this._schedulingTargetChangeHistoriesSubject.next(this._schedulingTargetChangeHistories);      // Emit to observable
    }

    public get HasSchedulingTargetChangeHistories(): Promise<boolean> {
        return this.SchedulingTargetChangeHistories.then(schedulingTargetChangeHistories => schedulingTargetChangeHistories.length > 0);
    }


    /**
     *
     * Gets the SchedulingTargetContacts for this SchedulingTarget.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.schedulingTarget.SchedulingTargetContacts.then(schedulingTargetContacts => { ... })
     *   or
     *   await this.schedulingTarget.SchedulingTargetContacts
     *
    */
    public get SchedulingTargetContacts(): Promise<SchedulingTargetContactData[]> {
        if (this._schedulingTargetContacts !== null) {
            return Promise.resolve(this._schedulingTargetContacts);
        }

        if (this._schedulingTargetContactsPromise !== null) {
            return this._schedulingTargetContactsPromise;
        }

        // Start the load
        this.loadSchedulingTargetContacts();

        return this._schedulingTargetContactsPromise!;
    }



    private loadSchedulingTargetContacts(): void {

        this._schedulingTargetContactsPromise = lastValueFrom(
            SchedulingTargetService.Instance.GetSchedulingTargetContactsForSchedulingTarget(this.id)
        )
        .then(schedulingTargetContacts => {
            this._schedulingTargetContacts = schedulingTargetContacts ?? [];
            this._schedulingTargetContactsSubject.next(this._schedulingTargetContacts);
            return this._schedulingTargetContacts;
         })
        .catch(err => {
            this._schedulingTargetContacts = [];
            this._schedulingTargetContactsSubject.next(this._schedulingTargetContacts);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetContactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTargetContact. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetContactsCache(): void {
        this._schedulingTargetContacts = null;
        this._schedulingTargetContactsPromise = null;
        this._schedulingTargetContactsSubject.next(this._schedulingTargetContacts);      // Emit to observable
    }

    public get HasSchedulingTargetContacts(): Promise<boolean> {
        return this.SchedulingTargetContacts.then(schedulingTargetContacts => schedulingTargetContacts.length > 0);
    }


    /**
     *
     * Gets the SchedulingTargetAddresses for this SchedulingTarget.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.schedulingTarget.SchedulingTargetAddresses.then(schedulingTargetAddresses => { ... })
     *   or
     *   await this.schedulingTarget.SchedulingTargetAddresses
     *
    */
    public get SchedulingTargetAddresses(): Promise<SchedulingTargetAddressData[]> {
        if (this._schedulingTargetAddresses !== null) {
            return Promise.resolve(this._schedulingTargetAddresses);
        }

        if (this._schedulingTargetAddressesPromise !== null) {
            return this._schedulingTargetAddressesPromise;
        }

        // Start the load
        this.loadSchedulingTargetAddresses();

        return this._schedulingTargetAddressesPromise!;
    }



    private loadSchedulingTargetAddresses(): void {

        this._schedulingTargetAddressesPromise = lastValueFrom(
            SchedulingTargetService.Instance.GetSchedulingTargetAddressesForSchedulingTarget(this.id)
        )
        .then(schedulingTargetAddresses => {
            this._schedulingTargetAddresses = schedulingTargetAddresses ?? [];
            this._schedulingTargetAddressesSubject.next(this._schedulingTargetAddresses);
            return this._schedulingTargetAddresses;
         })
        .catch(err => {
            this._schedulingTargetAddresses = [];
            this._schedulingTargetAddressesSubject.next(this._schedulingTargetAddresses);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetAddressesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTargetAddress. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetAddressesCache(): void {
        this._schedulingTargetAddresses = null;
        this._schedulingTargetAddressesPromise = null;
        this._schedulingTargetAddressesSubject.next(this._schedulingTargetAddresses);      // Emit to observable
    }

    public get HasSchedulingTargetAddresses(): Promise<boolean> {
        return this.SchedulingTargetAddresses.then(schedulingTargetAddresses => schedulingTargetAddresses.length > 0);
    }


    /**
     *
     * Gets the SchedulingTargetQualificationRequirements for this SchedulingTarget.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.schedulingTarget.SchedulingTargetQualificationRequirements.then(schedulingTargetQualificationRequirements => { ... })
     *   or
     *   await this.schedulingTarget.SchedulingTargetQualificationRequirements
     *
    */
    public get SchedulingTargetQualificationRequirements(): Promise<SchedulingTargetQualificationRequirementData[]> {
        if (this._schedulingTargetQualificationRequirements !== null) {
            return Promise.resolve(this._schedulingTargetQualificationRequirements);
        }

        if (this._schedulingTargetQualificationRequirementsPromise !== null) {
            return this._schedulingTargetQualificationRequirementsPromise;
        }

        // Start the load
        this.loadSchedulingTargetQualificationRequirements();

        return this._schedulingTargetQualificationRequirementsPromise!;
    }



    private loadSchedulingTargetQualificationRequirements(): void {

        this._schedulingTargetQualificationRequirementsPromise = lastValueFrom(
            SchedulingTargetService.Instance.GetSchedulingTargetQualificationRequirementsForSchedulingTarget(this.id)
        )
        .then(schedulingTargetQualificationRequirements => {
            this._schedulingTargetQualificationRequirements = schedulingTargetQualificationRequirements ?? [];
            this._schedulingTargetQualificationRequirementsSubject.next(this._schedulingTargetQualificationRequirements);
            return this._schedulingTargetQualificationRequirements;
         })
        .catch(err => {
            this._schedulingTargetQualificationRequirements = [];
            this._schedulingTargetQualificationRequirementsSubject.next(this._schedulingTargetQualificationRequirements);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetQualificationRequirementsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTargetQualificationRequirement. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetQualificationRequirementsCache(): void {
        this._schedulingTargetQualificationRequirements = null;
        this._schedulingTargetQualificationRequirementsPromise = null;
        this._schedulingTargetQualificationRequirementsSubject.next(this._schedulingTargetQualificationRequirements);      // Emit to observable
    }

    public get HasSchedulingTargetQualificationRequirements(): Promise<boolean> {
        return this.SchedulingTargetQualificationRequirements.then(schedulingTargetQualificationRequirements => schedulingTargetQualificationRequirements.length > 0);
    }


    /**
     *
     * Gets the RateSheets for this SchedulingTarget.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.schedulingTarget.RateSheets.then(rateSheets => { ... })
     *   or
     *   await this.schedulingTarget.RateSheets
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
            SchedulingTargetService.Instance.GetRateSheetsForSchedulingTarget(this.id)
        )
        .then(rateSheets => {
            this._rateSheets = rateSheets ?? [];
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
     * Gets the ScheduledEvents for this SchedulingTarget.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.schedulingTarget.ScheduledEvents.then(scheduledEvents => { ... })
     *   or
     *   await this.schedulingTarget.ScheduledEvents
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
            SchedulingTargetService.Instance.GetScheduledEventsForSchedulingTarget(this.id)
        )
        .then(scheduledEvents => {
            this._scheduledEvents = scheduledEvents ?? [];
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
     * Gets the Households for this SchedulingTarget.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.schedulingTarget.Households.then(households => { ... })
     *   or
     *   await this.schedulingTarget.Households
     *
    */
    public get Households(): Promise<HouseholdData[]> {
        if (this._households !== null) {
            return Promise.resolve(this._households);
        }

        if (this._householdsPromise !== null) {
            return this._householdsPromise;
        }

        // Start the load
        this.loadHouseholds();

        return this._householdsPromise!;
    }



    private loadHouseholds(): void {

        this._householdsPromise = lastValueFrom(
            SchedulingTargetService.Instance.GetHouseholdsForSchedulingTarget(this.id)
        )
        .then(households => {
            this._households = households ?? [];
            this._householdsSubject.next(this._households);
            return this._households;
         })
        .catch(err => {
            this._households = [];
            this._householdsSubject.next(this._households);
            throw err;
        })
        .finally(() => {
            this._householdsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Household. Call after mutations to force refresh.
     */
    public ClearHouseholdsCache(): void {
        this._households = null;
        this._householdsPromise = null;
        this._householdsSubject.next(this._households);      // Emit to observable
    }

    public get HasHouseholds(): Promise<boolean> {
        return this.Households.then(households => households.length > 0);
    }




    /**
     * Updates the state of this SchedulingTargetData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SchedulingTargetData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SchedulingTargetSubmitData {
        return SchedulingTargetService.Instance.ConvertToSchedulingTargetSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SchedulingTargetService extends SecureEndpointBase {

    private static _instance: SchedulingTargetService;
    private listCache: Map<string, Observable<Array<SchedulingTargetData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SchedulingTargetBasicListData>>>;
    private recordCache: Map<string, Observable<SchedulingTargetData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private schedulingTargetChangeHistoryService: SchedulingTargetChangeHistoryService,
        private schedulingTargetContactService: SchedulingTargetContactService,
        private schedulingTargetAddressService: SchedulingTargetAddressService,
        private schedulingTargetQualificationRequirementService: SchedulingTargetQualificationRequirementService,
        private rateSheetService: RateSheetService,
        private scheduledEventService: ScheduledEventService,
        private householdService: HouseholdService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SchedulingTargetData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SchedulingTargetBasicListData>>>();
        this.recordCache = new Map<string, Observable<SchedulingTargetData>>();

        SchedulingTargetService._instance = this;
    }

    public static get Instance(): SchedulingTargetService {
      return SchedulingTargetService._instance;
    }


    public ClearListCaches(config: SchedulingTargetQueryParameters | null = null) {

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


    public ConvertToSchedulingTargetSubmitData(data: SchedulingTargetData): SchedulingTargetSubmitData {

        let output = new SchedulingTargetSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.officeId = data.officeId;
        output.clientId = data.clientId;
        output.schedulingTargetTypeId = data.schedulingTargetTypeId;
        output.timeZoneId = data.timeZoneId;
        output.calendarId = data.calendarId;
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

    public GetSchedulingTarget(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const schedulingTarget$ = this.requestSchedulingTarget(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTarget", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, schedulingTarget$);

            return schedulingTarget$;
        }

        return this.recordCache.get(configHash) as Observable<SchedulingTargetData>;
    }

    private requestSchedulingTarget(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetData>(this.baseUrl + 'api/SchedulingTarget/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSchedulingTarget(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTarget(id, includeRelations));
            }));
    }

    public GetSchedulingTargetList(config: SchedulingTargetQueryParameters | any = null) : Observable<Array<SchedulingTargetData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const schedulingTargetList$ = this.requestSchedulingTargetList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTarget list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, schedulingTargetList$);

            return schedulingTargetList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SchedulingTargetData>>;
    }


    private requestSchedulingTargetList(config: SchedulingTargetQueryParameters | any) : Observable <Array<SchedulingTargetData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetData>>(this.baseUrl + 'api/SchedulingTargets', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSchedulingTargetList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetList(config));
            }));
    }

    public GetSchedulingTargetsRowCount(config: SchedulingTargetQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const schedulingTargetsRowCount$ = this.requestSchedulingTargetsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargets row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, schedulingTargetsRowCount$);

            return schedulingTargetsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSchedulingTargetsRowCount(config: SchedulingTargetQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SchedulingTargets/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetsRowCount(config));
            }));
    }

    public GetSchedulingTargetsBasicListData(config: SchedulingTargetQueryParameters | any = null) : Observable<Array<SchedulingTargetBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const schedulingTargetsBasicListData$ = this.requestSchedulingTargetsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargets basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, schedulingTargetsBasicListData$);

            return schedulingTargetsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SchedulingTargetBasicListData>>;
    }


    private requestSchedulingTargetsBasicListData(config: SchedulingTargetQueryParameters | any) : Observable<Array<SchedulingTargetBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetBasicListData>>(this.baseUrl + 'api/SchedulingTargets/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetsBasicListData(config));
            }));

    }


    public PutSchedulingTarget(id: bigint | number, schedulingTarget: SchedulingTargetSubmitData) : Observable<SchedulingTargetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SchedulingTargetData>(this.baseUrl + 'api/SchedulingTarget/' + id.toString(), schedulingTarget, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTarget(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSchedulingTarget(id, schedulingTarget));
            }));
    }


    public PostSchedulingTarget(schedulingTarget: SchedulingTargetSubmitData) : Observable<SchedulingTargetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SchedulingTargetData>(this.baseUrl + 'api/SchedulingTarget', schedulingTarget, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTarget(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSchedulingTarget(schedulingTarget));
            }));
    }

  
    public DeleteSchedulingTarget(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SchedulingTarget/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSchedulingTarget(id));
            }));
    }

    public RollbackSchedulingTarget(id: bigint | number, versionNumber: bigint | number) : Observable<SchedulingTargetData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SchedulingTargetData>(this.baseUrl + 'api/SchedulingTarget/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTarget(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackSchedulingTarget(id, versionNumber));
        }));
    }

    private getConfigHash(config: SchedulingTargetQueryParameters | any): string {

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

    public userIsSchedulerSchedulingTargetReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerSchedulingTargetReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.SchedulingTargets
        //
        if (userIsSchedulerSchedulingTargetReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerSchedulingTargetReader = user.readPermission >= 1;
            } else {
                userIsSchedulerSchedulingTargetReader = false;
            }
        }

        return userIsSchedulerSchedulingTargetReader;
    }


    public userIsSchedulerSchedulingTargetWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerSchedulingTargetWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.SchedulingTargets
        //
        if (userIsSchedulerSchedulingTargetWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerSchedulingTargetWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerSchedulingTargetWriter = false;
          }      
        }

        return userIsSchedulerSchedulingTargetWriter;
    }

    public GetSchedulingTargetChangeHistoriesForSchedulingTarget(schedulingTargetId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetChangeHistoryData[]> {
        return this.schedulingTargetChangeHistoryService.GetSchedulingTargetChangeHistoryList({
            schedulingTargetId: schedulingTargetId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetContactsForSchedulingTarget(schedulingTargetId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetContactData[]> {
        return this.schedulingTargetContactService.GetSchedulingTargetContactList({
            schedulingTargetId: schedulingTargetId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetAddressesForSchedulingTarget(schedulingTargetId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetAddressData[]> {
        return this.schedulingTargetAddressService.GetSchedulingTargetAddressList({
            schedulingTargetId: schedulingTargetId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetQualificationRequirementsForSchedulingTarget(schedulingTargetId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetQualificationRequirementData[]> {
        return this.schedulingTargetQualificationRequirementService.GetSchedulingTargetQualificationRequirementList({
            schedulingTargetId: schedulingTargetId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetRateSheetsForSchedulingTarget(schedulingTargetId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<RateSheetData[]> {
        return this.rateSheetService.GetRateSheetList({
            schedulingTargetId: schedulingTargetId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventsForSchedulingTarget(schedulingTargetId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventData[]> {
        return this.scheduledEventService.GetScheduledEventList({
            schedulingTargetId: schedulingTargetId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetHouseholdsForSchedulingTarget(schedulingTargetId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<HouseholdData[]> {
        return this.householdService.GetHouseholdList({
            schedulingTargetId: schedulingTargetId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SchedulingTargetData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SchedulingTargetData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SchedulingTargetTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSchedulingTarget(raw: any): SchedulingTargetData {
    if (!raw) return raw;

    //
    // Create a SchedulingTargetData object instance with correct prototype
    //
    const revived = Object.create(SchedulingTargetData.prototype) as SchedulingTargetData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._schedulingTargetChangeHistories = null;
    (revived as any)._schedulingTargetChangeHistoriesPromise = null;
    (revived as any)._schedulingTargetChangeHistoriesSubject = new BehaviorSubject<SchedulingTargetChangeHistoryData[] | null>(null);

    (revived as any)._schedulingTargetContacts = null;
    (revived as any)._schedulingTargetContactsPromise = null;
    (revived as any)._schedulingTargetContactsSubject = new BehaviorSubject<SchedulingTargetContactData[] | null>(null);

    (revived as any)._schedulingTargetAddresses = null;
    (revived as any)._schedulingTargetAddressesPromise = null;
    (revived as any)._schedulingTargetAddressesSubject = new BehaviorSubject<SchedulingTargetAddressData[] | null>(null);

    (revived as any)._schedulingTargetQualificationRequirements = null;
    (revived as any)._schedulingTargetQualificationRequirementsPromise = null;
    (revived as any)._schedulingTargetQualificationRequirementsSubject = new BehaviorSubject<SchedulingTargetQualificationRequirementData[] | null>(null);

    (revived as any)._rateSheets = null;
    (revived as any)._rateSheetsPromise = null;
    (revived as any)._rateSheetsSubject = new BehaviorSubject<RateSheetData[] | null>(null);

    (revived as any)._scheduledEvents = null;
    (revived as any)._scheduledEventsPromise = null;
    (revived as any)._scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

    (revived as any)._households = null;
    (revived as any)._householdsPromise = null;
    (revived as any)._householdsSubject = new BehaviorSubject<HouseholdData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSchedulingTargetXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SchedulingTargetChangeHistories$ = (revived as any)._schedulingTargetChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetChangeHistories === null && (revived as any)._schedulingTargetChangeHistoriesPromise === null) {
                (revived as any).loadSchedulingTargetChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SchedulingTargetChangeHistoriesCount$ = SchedulingTargetChangeHistoryService.Instance.GetSchedulingTargetChangeHistoriesRowCount({schedulingTargetId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SchedulingTargetContacts$ = (revived as any)._schedulingTargetContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetContacts === null && (revived as any)._schedulingTargetContactsPromise === null) {
                (revived as any).loadSchedulingTargetContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SchedulingTargetContactsCount$ = SchedulingTargetContactService.Instance.GetSchedulingTargetContactsRowCount({schedulingTargetId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SchedulingTargetAddresses$ = (revived as any)._schedulingTargetAddressesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetAddresses === null && (revived as any)._schedulingTargetAddressesPromise === null) {
                (revived as any).loadSchedulingTargetAddresses();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SchedulingTargetAddressesCount$ = SchedulingTargetAddressService.Instance.GetSchedulingTargetAddressesRowCount({schedulingTargetId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SchedulingTargetQualificationRequirements$ = (revived as any)._schedulingTargetQualificationRequirementsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetQualificationRequirements === null && (revived as any)._schedulingTargetQualificationRequirementsPromise === null) {
                (revived as any).loadSchedulingTargetQualificationRequirements();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SchedulingTargetQualificationRequirementsCount$ = SchedulingTargetQualificationRequirementService.Instance.GetSchedulingTargetQualificationRequirementsRowCount({schedulingTargetId: (revived as any).id,
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

    (revived as any).RateSheetsCount$ = RateSheetService.Instance.GetRateSheetsRowCount({schedulingTargetId: (revived as any).id,
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

    (revived as any).ScheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({schedulingTargetId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Households$ = (revived as any)._householdsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._households === null && (revived as any)._householdsPromise === null) {
                (revived as any).loadHouseholds();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).HouseholdsCount$ = HouseholdService.Instance.GetHouseholdsRowCount({schedulingTargetId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveSchedulingTargetList(rawList: any[]): SchedulingTargetData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSchedulingTarget(raw));
  }

}
