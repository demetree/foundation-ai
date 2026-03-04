/*

   GENERATED SERVICE FOR THE TIMEZONE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the TimeZone table.

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
import { ContactService, ContactData } from './contact.service';
import { OfficeService, OfficeData } from './office.service';
import { ClientService, ClientData } from './client.service';
import { TenantProfileService, TenantProfileData } from './tenant-profile.service';
import { SchedulingTargetService, SchedulingTargetData } from './scheduling-target.service';
import { ShiftPatternService, ShiftPatternData } from './shift-pattern.service';
import { ResourceService, ResourceData } from './resource.service';
import { ResourceAvailabilityService, ResourceAvailabilityData } from './resource-availability.service';
import { ResourceShiftService, ResourceShiftData } from './resource-shift.service';
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
export class TimeZoneQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    ianaTimeZone: string | null | undefined = null;
    abbreviation: string | null | undefined = null;
    abbreviationDaylightSavings: string | null | undefined = null;
    supportsDaylightSavings: boolean | null | undefined = null;
    standardUTCOffsetHours: number | null | undefined = null;
    dstUTCOffsetHours: number | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class TimeZoneSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    ianaTimeZone!: string;
    abbreviation!: string;
    abbreviationDaylightSavings!: string;
    supportsDaylightSavings!: boolean;
    standardUTCOffsetHours!: number;
    dstUTCOffsetHours!: number;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class TimeZoneBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TimeZoneChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `timeZone.TimeZoneChildren$` — use with `| async` in templates
//        • Promise:    `timeZone.TimeZoneChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="timeZone.TimeZoneChildren$ | async"`), or
//        • Access the promise getter (`timeZone.TimeZoneChildren` or `await timeZone.TimeZoneChildren`)
//    - Simply reading `timeZone.TimeZoneChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await timeZone.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TimeZoneData {
    id!: bigint | number;
    name!: string;
    description!: string;
    ianaTimeZone!: string;
    abbreviation!: string;
    abbreviationDaylightSavings!: string;
    supportsDaylightSavings!: boolean;
    standardUTCOffsetHours!: number;
    dstUTCOffsetHours!: number;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _contacts: ContactData[] | null = null;
    private _contactsPromise: Promise<ContactData[]> | null  = null;
    private _contactsSubject = new BehaviorSubject<ContactData[] | null>(null);

                
    private _offices: OfficeData[] | null = null;
    private _officesPromise: Promise<OfficeData[]> | null  = null;
    private _officesSubject = new BehaviorSubject<OfficeData[] | null>(null);

                
    private _clients: ClientData[] | null = null;
    private _clientsPromise: Promise<ClientData[]> | null  = null;
    private _clientsSubject = new BehaviorSubject<ClientData[] | null>(null);

                
    private _tenantProfiles: TenantProfileData[] | null = null;
    private _tenantProfilesPromise: Promise<TenantProfileData[]> | null  = null;
    private _tenantProfilesSubject = new BehaviorSubject<TenantProfileData[] | null>(null);

                
    private _schedulingTargets: SchedulingTargetData[] | null = null;
    private _schedulingTargetsPromise: Promise<SchedulingTargetData[]> | null  = null;
    private _schedulingTargetsSubject = new BehaviorSubject<SchedulingTargetData[] | null>(null);

                
    private _shiftPatterns: ShiftPatternData[] | null = null;
    private _shiftPatternsPromise: Promise<ShiftPatternData[]> | null  = null;
    private _shiftPatternsSubject = new BehaviorSubject<ShiftPatternData[] | null>(null);

                
    private _resources: ResourceData[] | null = null;
    private _resourcesPromise: Promise<ResourceData[]> | null  = null;
    private _resourcesSubject = new BehaviorSubject<ResourceData[] | null>(null);

                
    private _resourceAvailabilities: ResourceAvailabilityData[] | null = null;
    private _resourceAvailabilitiesPromise: Promise<ResourceAvailabilityData[]> | null  = null;
    private _resourceAvailabilitiesSubject = new BehaviorSubject<ResourceAvailabilityData[] | null>(null);

                
    private _resourceShifts: ResourceShiftData[] | null = null;
    private _resourceShiftsPromise: Promise<ResourceShiftData[]> | null  = null;
    private _resourceShiftsSubject = new BehaviorSubject<ResourceShiftData[] | null>(null);

                
    private _scheduledEvents: ScheduledEventData[] | null = null;
    private _scheduledEventsPromise: Promise<ScheduledEventData[]> | null  = null;
    private _scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Contacts$ = this._contactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contacts === null && this._contactsPromise === null) {
            this.loadContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _contactsCount$: Observable<bigint | number> | null = null;
    public get ContactsCount$(): Observable<bigint | number> {
        if (this._contactsCount$ === null) {
            this._contactsCount$ = ContactService.Instance.GetContactsRowCount({timeZoneId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._contactsCount$;
    }



    public Offices$ = this._officesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._offices === null && this._officesPromise === null) {
            this.loadOffices(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _officesCount$: Observable<bigint | number> | null = null;
    public get OfficesCount$(): Observable<bigint | number> {
        if (this._officesCount$ === null) {
            this._officesCount$ = OfficeService.Instance.GetOfficesRowCount({timeZoneId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._officesCount$;
    }



    public Clients$ = this._clientsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._clients === null && this._clientsPromise === null) {
            this.loadClients(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _clientsCount$: Observable<bigint | number> | null = null;
    public get ClientsCount$(): Observable<bigint | number> {
        if (this._clientsCount$ === null) {
            this._clientsCount$ = ClientService.Instance.GetClientsRowCount({timeZoneId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._clientsCount$;
    }



    public TenantProfiles$ = this._tenantProfilesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._tenantProfiles === null && this._tenantProfilesPromise === null) {
            this.loadTenantProfiles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _tenantProfilesCount$: Observable<bigint | number> | null = null;
    public get TenantProfilesCount$(): Observable<bigint | number> {
        if (this._tenantProfilesCount$ === null) {
            this._tenantProfilesCount$ = TenantProfileService.Instance.GetTenantProfilesRowCount({timeZoneId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._tenantProfilesCount$;
    }



    public SchedulingTargets$ = this._schedulingTargetsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargets === null && this._schedulingTargetsPromise === null) {
            this.loadSchedulingTargets(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _schedulingTargetsCount$: Observable<bigint | number> | null = null;
    public get SchedulingTargetsCount$(): Observable<bigint | number> {
        if (this._schedulingTargetsCount$ === null) {
            this._schedulingTargetsCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({timeZoneId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._schedulingTargetsCount$;
    }



    public ShiftPatterns$ = this._shiftPatternsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._shiftPatterns === null && this._shiftPatternsPromise === null) {
            this.loadShiftPatterns(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _shiftPatternsCount$: Observable<bigint | number> | null = null;
    public get ShiftPatternsCount$(): Observable<bigint | number> {
        if (this._shiftPatternsCount$ === null) {
            this._shiftPatternsCount$ = ShiftPatternService.Instance.GetShiftPatternsRowCount({timeZoneId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._shiftPatternsCount$;
    }



    public Resources$ = this._resourcesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resources === null && this._resourcesPromise === null) {
            this.loadResources(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _resourcesCount$: Observable<bigint | number> | null = null;
    public get ResourcesCount$(): Observable<bigint | number> {
        if (this._resourcesCount$ === null) {
            this._resourcesCount$ = ResourceService.Instance.GetResourcesRowCount({timeZoneId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._resourcesCount$;
    }



    public ResourceAvailabilities$ = this._resourceAvailabilitiesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resourceAvailabilities === null && this._resourceAvailabilitiesPromise === null) {
            this.loadResourceAvailabilities(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _resourceAvailabilitiesCount$: Observable<bigint | number> | null = null;
    public get ResourceAvailabilitiesCount$(): Observable<bigint | number> {
        if (this._resourceAvailabilitiesCount$ === null) {
            this._resourceAvailabilitiesCount$ = ResourceAvailabilityService.Instance.GetResourceAvailabilitiesRowCount({timeZoneId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._resourceAvailabilitiesCount$;
    }



    public ResourceShifts$ = this._resourceShiftsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resourceShifts === null && this._resourceShiftsPromise === null) {
            this.loadResourceShifts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _resourceShiftsCount$: Observable<bigint | number> | null = null;
    public get ResourceShiftsCount$(): Observable<bigint | number> {
        if (this._resourceShiftsCount$ === null) {
            this._resourceShiftsCount$ = ResourceShiftService.Instance.GetResourceShiftsRowCount({timeZoneId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._resourceShiftsCount$;
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
            this._scheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({timeZoneId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._scheduledEventsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any TimeZoneData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.timeZone.Reload();
  //
  //  Non Async:
  //
  //     timeZone[0].Reload().then(x => {
  //        this.timeZone = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TimeZoneService.Instance.GetTimeZone(this.id, includeRelations)
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
     this._contacts = null;
     this._contactsPromise = null;
     this._contactsSubject.next(null);
     this._contactsCount$ = null;

     this._offices = null;
     this._officesPromise = null;
     this._officesSubject.next(null);
     this._officesCount$ = null;

     this._clients = null;
     this._clientsPromise = null;
     this._clientsSubject.next(null);
     this._clientsCount$ = null;

     this._tenantProfiles = null;
     this._tenantProfilesPromise = null;
     this._tenantProfilesSubject.next(null);
     this._tenantProfilesCount$ = null;

     this._schedulingTargets = null;
     this._schedulingTargetsPromise = null;
     this._schedulingTargetsSubject.next(null);
     this._schedulingTargetsCount$ = null;

     this._shiftPatterns = null;
     this._shiftPatternsPromise = null;
     this._shiftPatternsSubject.next(null);
     this._shiftPatternsCount$ = null;

     this._resources = null;
     this._resourcesPromise = null;
     this._resourcesSubject.next(null);
     this._resourcesCount$ = null;

     this._resourceAvailabilities = null;
     this._resourceAvailabilitiesPromise = null;
     this._resourceAvailabilitiesSubject.next(null);
     this._resourceAvailabilitiesCount$ = null;

     this._resourceShifts = null;
     this._resourceShiftsPromise = null;
     this._resourceShiftsSubject.next(null);
     this._resourceShiftsCount$ = null;

     this._scheduledEvents = null;
     this._scheduledEventsPromise = null;
     this._scheduledEventsSubject.next(null);
     this._scheduledEventsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Contacts for this TimeZone.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.timeZone.Contacts.then(timeZones => { ... })
     *   or
     *   await this.timeZone.timeZones
     *
    */
    public get Contacts(): Promise<ContactData[]> {
        if (this._contacts !== null) {
            return Promise.resolve(this._contacts);
        }

        if (this._contactsPromise !== null) {
            return this._contactsPromise;
        }

        // Start the load
        this.loadContacts();

        return this._contactsPromise!;
    }



    private loadContacts(): void {

        this._contactsPromise = lastValueFrom(
            TimeZoneService.Instance.GetContactsForTimeZone(this.id)
        )
        .then(Contacts => {
            this._contacts = Contacts ?? [];
            this._contactsSubject.next(this._contacts);
            return this._contacts;
         })
        .catch(err => {
            this._contacts = [];
            this._contactsSubject.next(this._contacts);
            throw err;
        })
        .finally(() => {
            this._contactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Contact. Call after mutations to force refresh.
     */
    public ClearContactsCache(): void {
        this._contacts = null;
        this._contactsPromise = null;
        this._contactsSubject.next(this._contacts);      // Emit to observable
    }

    public get HasContacts(): Promise<boolean> {
        return this.Contacts.then(contacts => contacts.length > 0);
    }


    /**
     *
     * Gets the Offices for this TimeZone.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.timeZone.Offices.then(timeZones => { ... })
     *   or
     *   await this.timeZone.timeZones
     *
    */
    public get Offices(): Promise<OfficeData[]> {
        if (this._offices !== null) {
            return Promise.resolve(this._offices);
        }

        if (this._officesPromise !== null) {
            return this._officesPromise;
        }

        // Start the load
        this.loadOffices();

        return this._officesPromise!;
    }



    private loadOffices(): void {

        this._officesPromise = lastValueFrom(
            TimeZoneService.Instance.GetOfficesForTimeZone(this.id)
        )
        .then(Offices => {
            this._offices = Offices ?? [];
            this._officesSubject.next(this._offices);
            return this._offices;
         })
        .catch(err => {
            this._offices = [];
            this._officesSubject.next(this._offices);
            throw err;
        })
        .finally(() => {
            this._officesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Office. Call after mutations to force refresh.
     */
    public ClearOfficesCache(): void {
        this._offices = null;
        this._officesPromise = null;
        this._officesSubject.next(this._offices);      // Emit to observable
    }

    public get HasOffices(): Promise<boolean> {
        return this.Offices.then(offices => offices.length > 0);
    }


    /**
     *
     * Gets the Clients for this TimeZone.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.timeZone.Clients.then(timeZones => { ... })
     *   or
     *   await this.timeZone.timeZones
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
            TimeZoneService.Instance.GetClientsForTimeZone(this.id)
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
     * Gets the TenantProfiles for this TimeZone.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.timeZone.TenantProfiles.then(timeZones => { ... })
     *   or
     *   await this.timeZone.timeZones
     *
    */
    public get TenantProfiles(): Promise<TenantProfileData[]> {
        if (this._tenantProfiles !== null) {
            return Promise.resolve(this._tenantProfiles);
        }

        if (this._tenantProfilesPromise !== null) {
            return this._tenantProfilesPromise;
        }

        // Start the load
        this.loadTenantProfiles();

        return this._tenantProfilesPromise!;
    }



    private loadTenantProfiles(): void {

        this._tenantProfilesPromise = lastValueFrom(
            TimeZoneService.Instance.GetTenantProfilesForTimeZone(this.id)
        )
        .then(TenantProfiles => {
            this._tenantProfiles = TenantProfiles ?? [];
            this._tenantProfilesSubject.next(this._tenantProfiles);
            return this._tenantProfiles;
         })
        .catch(err => {
            this._tenantProfiles = [];
            this._tenantProfilesSubject.next(this._tenantProfiles);
            throw err;
        })
        .finally(() => {
            this._tenantProfilesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached TenantProfile. Call after mutations to force refresh.
     */
    public ClearTenantProfilesCache(): void {
        this._tenantProfiles = null;
        this._tenantProfilesPromise = null;
        this._tenantProfilesSubject.next(this._tenantProfiles);      // Emit to observable
    }

    public get HasTenantProfiles(): Promise<boolean> {
        return this.TenantProfiles.then(tenantProfiles => tenantProfiles.length > 0);
    }


    /**
     *
     * Gets the SchedulingTargets for this TimeZone.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.timeZone.SchedulingTargets.then(timeZones => { ... })
     *   or
     *   await this.timeZone.timeZones
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
            TimeZoneService.Instance.GetSchedulingTargetsForTimeZone(this.id)
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
     * Gets the ShiftPatterns for this TimeZone.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.timeZone.ShiftPatterns.then(timeZones => { ... })
     *   or
     *   await this.timeZone.timeZones
     *
    */
    public get ShiftPatterns(): Promise<ShiftPatternData[]> {
        if (this._shiftPatterns !== null) {
            return Promise.resolve(this._shiftPatterns);
        }

        if (this._shiftPatternsPromise !== null) {
            return this._shiftPatternsPromise;
        }

        // Start the load
        this.loadShiftPatterns();

        return this._shiftPatternsPromise!;
    }



    private loadShiftPatterns(): void {

        this._shiftPatternsPromise = lastValueFrom(
            TimeZoneService.Instance.GetShiftPatternsForTimeZone(this.id)
        )
        .then(ShiftPatterns => {
            this._shiftPatterns = ShiftPatterns ?? [];
            this._shiftPatternsSubject.next(this._shiftPatterns);
            return this._shiftPatterns;
         })
        .catch(err => {
            this._shiftPatterns = [];
            this._shiftPatternsSubject.next(this._shiftPatterns);
            throw err;
        })
        .finally(() => {
            this._shiftPatternsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ShiftPattern. Call after mutations to force refresh.
     */
    public ClearShiftPatternsCache(): void {
        this._shiftPatterns = null;
        this._shiftPatternsPromise = null;
        this._shiftPatternsSubject.next(this._shiftPatterns);      // Emit to observable
    }

    public get HasShiftPatterns(): Promise<boolean> {
        return this.ShiftPatterns.then(shiftPatterns => shiftPatterns.length > 0);
    }


    /**
     *
     * Gets the Resources for this TimeZone.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.timeZone.Resources.then(timeZones => { ... })
     *   or
     *   await this.timeZone.timeZones
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
            TimeZoneService.Instance.GetResourcesForTimeZone(this.id)
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
     * Gets the ResourceAvailabilities for this TimeZone.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.timeZone.ResourceAvailabilities.then(timeZones => { ... })
     *   or
     *   await this.timeZone.timeZones
     *
    */
    public get ResourceAvailabilities(): Promise<ResourceAvailabilityData[]> {
        if (this._resourceAvailabilities !== null) {
            return Promise.resolve(this._resourceAvailabilities);
        }

        if (this._resourceAvailabilitiesPromise !== null) {
            return this._resourceAvailabilitiesPromise;
        }

        // Start the load
        this.loadResourceAvailabilities();

        return this._resourceAvailabilitiesPromise!;
    }



    private loadResourceAvailabilities(): void {

        this._resourceAvailabilitiesPromise = lastValueFrom(
            TimeZoneService.Instance.GetResourceAvailabilitiesForTimeZone(this.id)
        )
        .then(ResourceAvailabilities => {
            this._resourceAvailabilities = ResourceAvailabilities ?? [];
            this._resourceAvailabilitiesSubject.next(this._resourceAvailabilities);
            return this._resourceAvailabilities;
         })
        .catch(err => {
            this._resourceAvailabilities = [];
            this._resourceAvailabilitiesSubject.next(this._resourceAvailabilities);
            throw err;
        })
        .finally(() => {
            this._resourceAvailabilitiesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ResourceAvailability. Call after mutations to force refresh.
     */
    public ClearResourceAvailabilitiesCache(): void {
        this._resourceAvailabilities = null;
        this._resourceAvailabilitiesPromise = null;
        this._resourceAvailabilitiesSubject.next(this._resourceAvailabilities);      // Emit to observable
    }

    public get HasResourceAvailabilities(): Promise<boolean> {
        return this.ResourceAvailabilities.then(resourceAvailabilities => resourceAvailabilities.length > 0);
    }


    /**
     *
     * Gets the ResourceShifts for this TimeZone.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.timeZone.ResourceShifts.then(timeZones => { ... })
     *   or
     *   await this.timeZone.timeZones
     *
    */
    public get ResourceShifts(): Promise<ResourceShiftData[]> {
        if (this._resourceShifts !== null) {
            return Promise.resolve(this._resourceShifts);
        }

        if (this._resourceShiftsPromise !== null) {
            return this._resourceShiftsPromise;
        }

        // Start the load
        this.loadResourceShifts();

        return this._resourceShiftsPromise!;
    }



    private loadResourceShifts(): void {

        this._resourceShiftsPromise = lastValueFrom(
            TimeZoneService.Instance.GetResourceShiftsForTimeZone(this.id)
        )
        .then(ResourceShifts => {
            this._resourceShifts = ResourceShifts ?? [];
            this._resourceShiftsSubject.next(this._resourceShifts);
            return this._resourceShifts;
         })
        .catch(err => {
            this._resourceShifts = [];
            this._resourceShiftsSubject.next(this._resourceShifts);
            throw err;
        })
        .finally(() => {
            this._resourceShiftsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ResourceShift. Call after mutations to force refresh.
     */
    public ClearResourceShiftsCache(): void {
        this._resourceShifts = null;
        this._resourceShiftsPromise = null;
        this._resourceShiftsSubject.next(this._resourceShifts);      // Emit to observable
    }

    public get HasResourceShifts(): Promise<boolean> {
        return this.ResourceShifts.then(resourceShifts => resourceShifts.length > 0);
    }


    /**
     *
     * Gets the ScheduledEvents for this TimeZone.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.timeZone.ScheduledEvents.then(timeZones => { ... })
     *   or
     *   await this.timeZone.timeZones
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
            TimeZoneService.Instance.GetScheduledEventsForTimeZone(this.id)
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
     * Updates the state of this TimeZoneData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TimeZoneData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TimeZoneSubmitData {
        return TimeZoneService.Instance.ConvertToTimeZoneSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TimeZoneService extends SecureEndpointBase {

    private static _instance: TimeZoneService;
    private listCache: Map<string, Observable<Array<TimeZoneData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TimeZoneBasicListData>>>;
    private recordCache: Map<string, Observable<TimeZoneData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private contactService: ContactService,
        private officeService: OfficeService,
        private clientService: ClientService,
        private tenantProfileService: TenantProfileService,
        private schedulingTargetService: SchedulingTargetService,
        private shiftPatternService: ShiftPatternService,
        private resourceService: ResourceService,
        private resourceAvailabilityService: ResourceAvailabilityService,
        private resourceShiftService: ResourceShiftService,
        private scheduledEventService: ScheduledEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TimeZoneData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TimeZoneBasicListData>>>();
        this.recordCache = new Map<string, Observable<TimeZoneData>>();

        TimeZoneService._instance = this;
    }

    public static get Instance(): TimeZoneService {
      return TimeZoneService._instance;
    }


    public ClearListCaches(config: TimeZoneQueryParameters | null = null) {

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


    public ConvertToTimeZoneSubmitData(data: TimeZoneData): TimeZoneSubmitData {

        let output = new TimeZoneSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.ianaTimeZone = data.ianaTimeZone;
        output.abbreviation = data.abbreviation;
        output.abbreviationDaylightSavings = data.abbreviationDaylightSavings;
        output.supportsDaylightSavings = data.supportsDaylightSavings;
        output.standardUTCOffsetHours = data.standardUTCOffsetHours;
        output.dstUTCOffsetHours = data.dstUTCOffsetHours;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetTimeZone(id: bigint | number, includeRelations: boolean = true) : Observable<TimeZoneData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const timeZone$ = this.requestTimeZone(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TimeZone", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, timeZone$);

            return timeZone$;
        }

        return this.recordCache.get(configHash) as Observable<TimeZoneData>;
    }

    private requestTimeZone(id: bigint | number, includeRelations: boolean = true) : Observable<TimeZoneData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TimeZoneData>(this.baseUrl + 'api/TimeZone/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTimeZone(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTimeZone(id, includeRelations));
            }));
    }

    public GetTimeZoneList(config: TimeZoneQueryParameters | any = null) : Observable<Array<TimeZoneData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const timeZoneList$ = this.requestTimeZoneList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TimeZone list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, timeZoneList$);

            return timeZoneList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TimeZoneData>>;
    }


    private requestTimeZoneList(config: TimeZoneQueryParameters | any) : Observable <Array<TimeZoneData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TimeZoneData>>(this.baseUrl + 'api/TimeZones', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTimeZoneList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTimeZoneList(config));
            }));
    }

    public GetTimeZonesRowCount(config: TimeZoneQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const timeZonesRowCount$ = this.requestTimeZonesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TimeZones row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, timeZonesRowCount$);

            return timeZonesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTimeZonesRowCount(config: TimeZoneQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/TimeZones/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTimeZonesRowCount(config));
            }));
    }

    public GetTimeZonesBasicListData(config: TimeZoneQueryParameters | any = null) : Observable<Array<TimeZoneBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const timeZonesBasicListData$ = this.requestTimeZonesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TimeZones basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, timeZonesBasicListData$);

            return timeZonesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TimeZoneBasicListData>>;
    }


    private requestTimeZonesBasicListData(config: TimeZoneQueryParameters | any) : Observable<Array<TimeZoneBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TimeZoneBasicListData>>(this.baseUrl + 'api/TimeZones/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTimeZonesBasicListData(config));
            }));

    }


    public PutTimeZone(id: bigint | number, timeZone: TimeZoneSubmitData) : Observable<TimeZoneData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TimeZoneData>(this.baseUrl + 'api/TimeZone/' + id.toString(), timeZone, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTimeZone(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTimeZone(id, timeZone));
            }));
    }


    public PostTimeZone(timeZone: TimeZoneSubmitData) : Observable<TimeZoneData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TimeZoneData>(this.baseUrl + 'api/TimeZone', timeZone, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTimeZone(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTimeZone(timeZone));
            }));
    }

  
    public DeleteTimeZone(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/TimeZone/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTimeZone(id));
            }));
    }


    private getConfigHash(config: TimeZoneQueryParameters | any): string {

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

    public userIsSchedulerTimeZoneReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerTimeZoneReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.TimeZones
        //
        if (userIsSchedulerTimeZoneReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerTimeZoneReader = user.readPermission >= 1;
            } else {
                userIsSchedulerTimeZoneReader = false;
            }
        }

        return userIsSchedulerTimeZoneReader;
    }


    public userIsSchedulerTimeZoneWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerTimeZoneWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.TimeZones
        //
        if (userIsSchedulerTimeZoneWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerTimeZoneWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerTimeZoneWriter = false;
          }      
        }

        return userIsSchedulerTimeZoneWriter;
    }

    public GetContactsForTimeZone(timeZoneId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactData[]> {
        return this.contactService.GetContactList({
            timeZoneId: timeZoneId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetOfficesForTimeZone(timeZoneId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<OfficeData[]> {
        return this.officeService.GetOfficeList({
            timeZoneId: timeZoneId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetClientsForTimeZone(timeZoneId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ClientData[]> {
        return this.clientService.GetClientList({
            timeZoneId: timeZoneId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetTenantProfilesForTimeZone(timeZoneId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TenantProfileData[]> {
        return this.tenantProfileService.GetTenantProfileList({
            timeZoneId: timeZoneId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetsForTimeZone(timeZoneId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetData[]> {
        return this.schedulingTargetService.GetSchedulingTargetList({
            timeZoneId: timeZoneId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetShiftPatternsForTimeZone(timeZoneId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ShiftPatternData[]> {
        return this.shiftPatternService.GetShiftPatternList({
            timeZoneId: timeZoneId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetResourcesForTimeZone(timeZoneId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceData[]> {
        return this.resourceService.GetResourceList({
            timeZoneId: timeZoneId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetResourceAvailabilitiesForTimeZone(timeZoneId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceAvailabilityData[]> {
        return this.resourceAvailabilityService.GetResourceAvailabilityList({
            timeZoneId: timeZoneId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetResourceShiftsForTimeZone(timeZoneId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceShiftData[]> {
        return this.resourceShiftService.GetResourceShiftList({
            timeZoneId: timeZoneId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventsForTimeZone(timeZoneId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventData[]> {
        return this.scheduledEventService.GetScheduledEventList({
            timeZoneId: timeZoneId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full TimeZoneData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TimeZoneData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TimeZoneTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTimeZone(raw: any): TimeZoneData {
    if (!raw) return raw;

    //
    // Create a TimeZoneData object instance with correct prototype
    //
    const revived = Object.create(TimeZoneData.prototype) as TimeZoneData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._contacts = null;
    (revived as any)._contactsPromise = null;
    (revived as any)._contactsSubject = new BehaviorSubject<ContactData[] | null>(null);

    (revived as any)._offices = null;
    (revived as any)._officesPromise = null;
    (revived as any)._officesSubject = new BehaviorSubject<OfficeData[] | null>(null);

    (revived as any)._clients = null;
    (revived as any)._clientsPromise = null;
    (revived as any)._clientsSubject = new BehaviorSubject<ClientData[] | null>(null);

    (revived as any)._tenantProfiles = null;
    (revived as any)._tenantProfilesPromise = null;
    (revived as any)._tenantProfilesSubject = new BehaviorSubject<TenantProfileData[] | null>(null);

    (revived as any)._schedulingTargets = null;
    (revived as any)._schedulingTargetsPromise = null;
    (revived as any)._schedulingTargetsSubject = new BehaviorSubject<SchedulingTargetData[] | null>(null);

    (revived as any)._shiftPatterns = null;
    (revived as any)._shiftPatternsPromise = null;
    (revived as any)._shiftPatternsSubject = new BehaviorSubject<ShiftPatternData[] | null>(null);

    (revived as any)._resources = null;
    (revived as any)._resourcesPromise = null;
    (revived as any)._resourcesSubject = new BehaviorSubject<ResourceData[] | null>(null);

    (revived as any)._resourceAvailabilities = null;
    (revived as any)._resourceAvailabilitiesPromise = null;
    (revived as any)._resourceAvailabilitiesSubject = new BehaviorSubject<ResourceAvailabilityData[] | null>(null);

    (revived as any)._resourceShifts = null;
    (revived as any)._resourceShiftsPromise = null;
    (revived as any)._resourceShiftsSubject = new BehaviorSubject<ResourceShiftData[] | null>(null);

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
    // 2. But private methods (loadTimeZoneXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Contacts$ = (revived as any)._contactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contacts === null && (revived as any)._contactsPromise === null) {
                (revived as any).loadContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._contactsCount$ = null;


    (revived as any).Offices$ = (revived as any)._officesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._offices === null && (revived as any)._officesPromise === null) {
                (revived as any).loadOffices();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._officesCount$ = null;


    (revived as any).Clients$ = (revived as any)._clientsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._clients === null && (revived as any)._clientsPromise === null) {
                (revived as any).loadClients();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._clientsCount$ = null;


    (revived as any).TenantProfiles$ = (revived as any)._tenantProfilesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._tenantProfiles === null && (revived as any)._tenantProfilesPromise === null) {
                (revived as any).loadTenantProfiles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._tenantProfilesCount$ = null;


    (revived as any).SchedulingTargets$ = (revived as any)._schedulingTargetsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargets === null && (revived as any)._schedulingTargetsPromise === null) {
                (revived as any).loadSchedulingTargets();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._schedulingTargetsCount$ = null;


    (revived as any).ShiftPatterns$ = (revived as any)._shiftPatternsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._shiftPatterns === null && (revived as any)._shiftPatternsPromise === null) {
                (revived as any).loadShiftPatterns();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._shiftPatternsCount$ = null;


    (revived as any).Resources$ = (revived as any)._resourcesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resources === null && (revived as any)._resourcesPromise === null) {
                (revived as any).loadResources();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._resourcesCount$ = null;


    (revived as any).ResourceAvailabilities$ = (revived as any)._resourceAvailabilitiesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resourceAvailabilities === null && (revived as any)._resourceAvailabilitiesPromise === null) {
                (revived as any).loadResourceAvailabilities();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._resourceAvailabilitiesCount$ = null;


    (revived as any).ResourceShifts$ = (revived as any)._resourceShiftsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resourceShifts === null && (revived as any)._resourceShiftsPromise === null) {
                (revived as any).loadResourceShifts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._resourceShiftsCount$ = null;


    (revived as any).ScheduledEvents$ = (revived as any)._scheduledEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEvents === null && (revived as any)._scheduledEventsPromise === null) {
                (revived as any).loadScheduledEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._scheduledEventsCount$ = null;



    return revived;
  }

  private ReviveTimeZoneList(rawList: any[]): TimeZoneData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTimeZone(raw));
  }

}
