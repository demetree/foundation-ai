/*

   GENERATED SERVICE FOR THE RESOURCE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Resource table.

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
import { ResourceTypeData } from './resource-type.service';
import { ShiftPatternData } from './shift-pattern.service';
import { TimeZoneData } from './time-zone.service';
import { ResourceChangeHistoryService, ResourceChangeHistoryData } from './resource-change-history.service';
import { ResourceContactService, ResourceContactData } from './resource-contact.service';
import { RateSheetService, RateSheetData } from './rate-sheet.service';
import { ResourceQualificationService, ResourceQualificationData } from './resource-qualification.service';
import { ResourceAvailabilityService, ResourceAvailabilityData } from './resource-availability.service';
import { ResourceShiftService, ResourceShiftData } from './resource-shift.service';
import { CrewMemberService, CrewMemberData } from './crew-member.service';
import { ScheduledEventService, ScheduledEventData } from './scheduled-event.service';
import { EventChargeService, EventChargeData } from './event-charge.service';
import { NotificationSubscriptionService, NotificationSubscriptionData } from './notification-subscription.service';
import { VolunteerProfileService, VolunteerProfileData } from './volunteer-profile.service';
import { VolunteerGroupMemberService, VolunteerGroupMemberData } from './volunteer-group-member.service';
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
export class ResourceQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    officeId: bigint | number | null | undefined = null;
    resourceTypeId: bigint | number | null | undefined = null;
    shiftPatternId: bigint | number | null | undefined = null;
    timeZoneId: bigint | number | null | undefined = null;
    targetWeeklyWorkHours: number | null | undefined = null;
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
export class ResourceSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    officeId: bigint | number | null = null;
    resourceTypeId!: bigint | number;
    shiftPatternId: bigint | number | null = null;
    timeZoneId!: bigint | number;
    targetWeeklyWorkHours: number | null = null;
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

export class ResourceBasicListData {
    id!: bigint | number;
    name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ResourceChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `resource.ResourceChildren$` — use with `| async` in templates
//        • Promise:    `resource.ResourceChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="resource.ResourceChildren$ | async"`), or
//        • Access the promise getter (`resource.ResourceChildren` or `await resource.ResourceChildren`)
//    - Simply reading `resource.ResourceChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await resource.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ResourceData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    officeId!: bigint | number;
    resourceTypeId!: bigint | number;
    shiftPatternId!: bigint | number;
    timeZoneId!: bigint | number;
    targetWeeklyWorkHours!: number | null;
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
    office: OfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resourceType: ResourceTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    shiftPattern: ShiftPatternData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    timeZone: TimeZoneData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _resourceChangeHistories: ResourceChangeHistoryData[] | null = null;
    private _resourceChangeHistoriesPromise: Promise<ResourceChangeHistoryData[]> | null = null;
    private _resourceChangeHistoriesSubject = new BehaviorSubject<ResourceChangeHistoryData[] | null>(null);


    private _resourceContacts: ResourceContactData[] | null = null;
    private _resourceContactsPromise: Promise<ResourceContactData[]> | null = null;
    private _resourceContactsSubject = new BehaviorSubject<ResourceContactData[] | null>(null);


    private _rateSheets: RateSheetData[] | null = null;
    private _rateSheetsPromise: Promise<RateSheetData[]> | null = null;
    private _rateSheetsSubject = new BehaviorSubject<RateSheetData[] | null>(null);


    private _resourceQualifications: ResourceQualificationData[] | null = null;
    private _resourceQualificationsPromise: Promise<ResourceQualificationData[]> | null = null;
    private _resourceQualificationsSubject = new BehaviorSubject<ResourceQualificationData[] | null>(null);


    private _resourceAvailabilities: ResourceAvailabilityData[] | null = null;
    private _resourceAvailabilitiesPromise: Promise<ResourceAvailabilityData[]> | null = null;
    private _resourceAvailabilitiesSubject = new BehaviorSubject<ResourceAvailabilityData[] | null>(null);


    private _resourceShifts: ResourceShiftData[] | null = null;
    private _resourceShiftsPromise: Promise<ResourceShiftData[]> | null = null;
    private _resourceShiftsSubject = new BehaviorSubject<ResourceShiftData[] | null>(null);


    private _crewMembers: CrewMemberData[] | null = null;
    private _crewMembersPromise: Promise<CrewMemberData[]> | null = null;
    private _crewMembersSubject = new BehaviorSubject<CrewMemberData[] | null>(null);


    private _scheduledEvents: ScheduledEventData[] | null = null;
    private _scheduledEventsPromise: Promise<ScheduledEventData[]> | null = null;
    private _scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);


    private _eventCharges: EventChargeData[] | null = null;
    private _eventChargesPromise: Promise<EventChargeData[]> | null = null;
    private _eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);


    private _notificationSubscriptions: NotificationSubscriptionData[] | null = null;
    private _notificationSubscriptionsPromise: Promise<NotificationSubscriptionData[]> | null = null;
    private _notificationSubscriptionsSubject = new BehaviorSubject<NotificationSubscriptionData[] | null>(null);


    private _volunteerProfiles: VolunteerProfileData[] | null = null;
    private _volunteerProfilesPromise: Promise<VolunteerProfileData[]> | null = null;
    private _volunteerProfilesSubject = new BehaviorSubject<VolunteerProfileData[] | null>(null);


    private _volunteerGroupMembers: VolunteerGroupMemberData[] | null = null;
    private _volunteerGroupMembersPromise: Promise<VolunteerGroupMemberData[]> | null = null;
    private _volunteerGroupMembersSubject = new BehaviorSubject<VolunteerGroupMemberData[] | null>(null);


    private _eventResourceAssignments: EventResourceAssignmentData[] | null = null;
    private _eventResourceAssignmentsPromise: Promise<EventResourceAssignmentData[]> | null = null;
    private _eventResourceAssignmentsSubject = new BehaviorSubject<EventResourceAssignmentData[] | null>(null);




    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ResourceData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ResourceData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ResourceData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ResourceChangeHistories$ = this._resourceChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
            if (this._resourceChangeHistories === null && this._resourceChangeHistoriesPromise === null) {
                this.loadResourceChangeHistories(); // Private method to start fetch
            }
        }),
        shareReplay(1) // Cache last emit
    );


    public ResourceChangeHistoriesCount$ = ResourceChangeHistoryService.Instance.GetResourceChangeHistoriesRowCount({
        resourceId: this.id,
        active: true,
        deleted: false
    });



    public ResourceContacts$ = this._resourceContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
            if (this._resourceContacts === null && this._resourceContactsPromise === null) {
                this.loadResourceContacts(); // Private method to start fetch
            }
        }),
        shareReplay(1) // Cache last emit
    );


    public ResourceContactsCount$ = ResourceContactService.Instance.GetResourceContactsRowCount({
        resourceId: this.id,
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


    public RateSheetsCount$ = RateSheetService.Instance.GetRateSheetsRowCount({
        resourceId: this.id,
        active: true,
        deleted: false
    });



    public ResourceQualifications$ = this._resourceQualificationsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
            if (this._resourceQualifications === null && this._resourceQualificationsPromise === null) {
                this.loadResourceQualifications(); // Private method to start fetch
            }
        }),
        shareReplay(1) // Cache last emit
    );


    public ResourceQualificationsCount$ = ResourceQualificationService.Instance.GetResourceQualificationsRowCount({
        resourceId: this.id,
        active: true,
        deleted: false
    });



    public ResourceAvailabilities$ = this._resourceAvailabilitiesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
            if (this._resourceAvailabilities === null && this._resourceAvailabilitiesPromise === null) {
                this.loadResourceAvailabilities(); // Private method to start fetch
            }
        }),
        shareReplay(1) // Cache last emit
    );


    public ResourceAvailabilitiesCount$ = ResourceAvailabilityService.Instance.GetResourceAvailabilitiesRowCount({
        resourceId: this.id,
        active: true,
        deleted: false
    });



    public ResourceShifts$ = this._resourceShiftsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
            if (this._resourceShifts === null && this._resourceShiftsPromise === null) {
                this.loadResourceShifts(); // Private method to start fetch
            }
        }),
        shareReplay(1) // Cache last emit
    );


    public ResourceShiftsCount$ = ResourceShiftService.Instance.GetResourceShiftsRowCount({
        resourceId: this.id,
        active: true,
        deleted: false
    });



    public CrewMembers$ = this._crewMembersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
            if (this._crewMembers === null && this._crewMembersPromise === null) {
                this.loadCrewMembers(); // Private method to start fetch
            }
        }),
        shareReplay(1) // Cache last emit
    );


    public CrewMembersCount$ = CrewMemberService.Instance.GetCrewMembersRowCount({
        resourceId: this.id,
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


    public ScheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({
        resourceId: this.id,
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


    public EventChargesCount$ = EventChargeService.Instance.GetEventChargesRowCount({
        resourceId: this.id,
        active: true,
        deleted: false
    });



    public NotificationSubscriptions$ = this._notificationSubscriptionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
            if (this._notificationSubscriptions === null && this._notificationSubscriptionsPromise === null) {
                this.loadNotificationSubscriptions(); // Private method to start fetch
            }
        }),
        shareReplay(1) // Cache last emit
    );


    public NotificationSubscriptionsCount$ = NotificationSubscriptionService.Instance.GetNotificationSubscriptionsRowCount({
        resourceId: this.id,
        active: true,
        deleted: false
    });



    public VolunteerProfiles$ = this._volunteerProfilesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
            if (this._volunteerProfiles === null && this._volunteerProfilesPromise === null) {
                this.loadVolunteerProfiles(); // Private method to start fetch
            }
        }),
        shareReplay(1) // Cache last emit
    );


    public VolunteerProfilesCount$ = VolunteerProfileService.Instance.GetVolunteerProfilesRowCount({
        resourceId: this.id,
        active: true,
        deleted: false
    });



    public VolunteerGroupMembers$ = this._volunteerGroupMembersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
            if (this._volunteerGroupMembers === null && this._volunteerGroupMembersPromise === null) {
                this.loadVolunteerGroupMembers(); // Private method to start fetch
            }
        }),
        shareReplay(1) // Cache last emit
    );


    public VolunteerGroupMembersCount$ = VolunteerGroupMemberService.Instance.GetVolunteerGroupMembersRowCount({
        resourceId: this.id,
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


    public EventResourceAssignmentsCount$ = EventResourceAssignmentService.Instance.GetEventResourceAssignmentsRowCount({
        resourceId: this.id,
        active: true,
        deleted: false
    });




    //
    // Full reload — refreshes the entire object and clears all lazy caches 
    //
    // Promise based reload method to allow rebuilding of any ResourceData object with all of it's relations on demand.  Useful for navigating into nav property
    // objects and getting full state after put or post that may not have returned all nav properties.
    //
    // Usage examples:;
    //
    //  Async:
    //   await this.resource.Reload();
    //
    //  Non Async:
    //
    //     resource[0].Reload().then(x => {
    //        this.resource = x;
    //    });
    //
    public async Reload(includeRelations: boolean = true): Promise<this> {

        const fresh = await lastValueFrom(
            ResourceService.Instance.GetResource(this.id, includeRelations)
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
        this._resourceChangeHistories = null;
        this._resourceChangeHistoriesPromise = null;
        this._resourceChangeHistoriesSubject.next(null);

        this._resourceContacts = null;
        this._resourceContactsPromise = null;
        this._resourceContactsSubject.next(null);

        this._rateSheets = null;
        this._rateSheetsPromise = null;
        this._rateSheetsSubject.next(null);

        this._resourceQualifications = null;
        this._resourceQualificationsPromise = null;
        this._resourceQualificationsSubject.next(null);

        this._resourceAvailabilities = null;
        this._resourceAvailabilitiesPromise = null;
        this._resourceAvailabilitiesSubject.next(null);

        this._resourceShifts = null;
        this._resourceShiftsPromise = null;
        this._resourceShiftsSubject.next(null);

        this._crewMembers = null;
        this._crewMembersPromise = null;
        this._crewMembersSubject.next(null);

        this._scheduledEvents = null;
        this._scheduledEventsPromise = null;
        this._scheduledEventsSubject.next(null);

        this._eventCharges = null;
        this._eventChargesPromise = null;
        this._eventChargesSubject.next(null);

        this._notificationSubscriptions = null;
        this._notificationSubscriptionsPromise = null;
        this._notificationSubscriptionsSubject.next(null);

        this._volunteerProfiles = null;
        this._volunteerProfilesPromise = null;
        this._volunteerProfilesSubject.next(null);

        this._volunteerGroupMembers = null;
        this._volunteerGroupMembersPromise = null;
        this._volunteerGroupMembersSubject.next(null);

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
     * Gets the ResourceChangeHistories for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.ResourceChangeHistories.then(resources => { ... })
     *   or
     *   await this.resource.resources
     *
    */
    public get ResourceChangeHistories(): Promise<ResourceChangeHistoryData[]> {
        if (this._resourceChangeHistories !== null) {
            return Promise.resolve(this._resourceChangeHistories);
        }

        if (this._resourceChangeHistoriesPromise !== null) {
            return this._resourceChangeHistoriesPromise;
        }

        // Start the load
        this.loadResourceChangeHistories();

        return this._resourceChangeHistoriesPromise!;
    }



    private loadResourceChangeHistories(): void {

        this._resourceChangeHistoriesPromise = lastValueFrom(
            ResourceService.Instance.GetResourceChangeHistoriesForResource(this.id)
        )
            .then(ResourceChangeHistories => {
                this._resourceChangeHistories = ResourceChangeHistories ?? [];
                this._resourceChangeHistoriesSubject.next(this._resourceChangeHistories);
                return this._resourceChangeHistories;
            })
            .catch(err => {
                this._resourceChangeHistories = [];
                this._resourceChangeHistoriesSubject.next(this._resourceChangeHistories);
                throw err;
            })
            .finally(() => {
                this._resourceChangeHistoriesPromise = null; // Allow retry if needed
            });
    }

    /**
     * Clears the cached ResourceChangeHistory. Call after mutations to force refresh.
     */
    public ClearResourceChangeHistoriesCache(): void {
        this._resourceChangeHistories = null;
        this._resourceChangeHistoriesPromise = null;
        this._resourceChangeHistoriesSubject.next(this._resourceChangeHistories);      // Emit to observable
    }

    public get HasResourceChangeHistories(): Promise<boolean> {
        return this.ResourceChangeHistories.then(resourceChangeHistories => resourceChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ResourceContacts for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.ResourceContacts.then(resources => { ... })
     *   or
     *   await this.resource.resources
     *
    */
    public get ResourceContacts(): Promise<ResourceContactData[]> {
        if (this._resourceContacts !== null) {
            return Promise.resolve(this._resourceContacts);
        }

        if (this._resourceContactsPromise !== null) {
            return this._resourceContactsPromise;
        }

        // Start the load
        this.loadResourceContacts();

        return this._resourceContactsPromise!;
    }



    private loadResourceContacts(): void {

        this._resourceContactsPromise = lastValueFrom(
            ResourceService.Instance.GetResourceContactsForResource(this.id)
        )
            .then(ResourceContacts => {
                this._resourceContacts = ResourceContacts ?? [];
                this._resourceContactsSubject.next(this._resourceContacts);
                return this._resourceContacts;
            })
            .catch(err => {
                this._resourceContacts = [];
                this._resourceContactsSubject.next(this._resourceContacts);
                throw err;
            })
            .finally(() => {
                this._resourceContactsPromise = null; // Allow retry if needed
            });
    }

    /**
     * Clears the cached ResourceContact. Call after mutations to force refresh.
     */
    public ClearResourceContactsCache(): void {
        this._resourceContacts = null;
        this._resourceContactsPromise = null;
        this._resourceContactsSubject.next(this._resourceContacts);      // Emit to observable
    }

    public get HasResourceContacts(): Promise<boolean> {
        return this.ResourceContacts.then(resourceContacts => resourceContacts.length > 0);
    }


    /**
     *
     * Gets the RateSheets for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.RateSheets.then(resources => { ... })
     *   or
     *   await this.resource.resources
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
            ResourceService.Instance.GetRateSheetsForResource(this.id)
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
     * Gets the ResourceQualifications for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.ResourceQualifications.then(resources => { ... })
     *   or
     *   await this.resource.resources
     *
    */
    public get ResourceQualifications(): Promise<ResourceQualificationData[]> {
        if (this._resourceQualifications !== null) {
            return Promise.resolve(this._resourceQualifications);
        }

        if (this._resourceQualificationsPromise !== null) {
            return this._resourceQualificationsPromise;
        }

        // Start the load
        this.loadResourceQualifications();

        return this._resourceQualificationsPromise!;
    }



    private loadResourceQualifications(): void {

        this._resourceQualificationsPromise = lastValueFrom(
            ResourceService.Instance.GetResourceQualificationsForResource(this.id)
        )
            .then(ResourceQualifications => {
                this._resourceQualifications = ResourceQualifications ?? [];
                this._resourceQualificationsSubject.next(this._resourceQualifications);
                return this._resourceQualifications;
            })
            .catch(err => {
                this._resourceQualifications = [];
                this._resourceQualificationsSubject.next(this._resourceQualifications);
                throw err;
            })
            .finally(() => {
                this._resourceQualificationsPromise = null; // Allow retry if needed
            });
    }

    /**
     * Clears the cached ResourceQualification. Call after mutations to force refresh.
     */
    public ClearResourceQualificationsCache(): void {
        this._resourceQualifications = null;
        this._resourceQualificationsPromise = null;
        this._resourceQualificationsSubject.next(this._resourceQualifications);      // Emit to observable
    }

    public get HasResourceQualifications(): Promise<boolean> {
        return this.ResourceQualifications.then(resourceQualifications => resourceQualifications.length > 0);
    }


    /**
     *
     * Gets the ResourceAvailabilities for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.ResourceAvailabilities.then(resources => { ... })
     *   or
     *   await this.resource.resources
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
            ResourceService.Instance.GetResourceAvailabilitiesForResource(this.id)
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
     * Gets the ResourceShifts for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.ResourceShifts.then(resources => { ... })
     *   or
     *   await this.resource.resources
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
            ResourceService.Instance.GetResourceShiftsForResource(this.id)
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
     * Gets the CrewMembers for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.CrewMembers.then(resources => { ... })
     *   or
     *   await this.resource.resources
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
            ResourceService.Instance.GetCrewMembersForResource(this.id)
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
     * Gets the ScheduledEvents for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.ScheduledEvents.then(resources => { ... })
     *   or
     *   await this.resource.resources
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
            ResourceService.Instance.GetScheduledEventsForResource(this.id)
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
     * Gets the EventCharges for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.EventCharges.then(resources => { ... })
     *   or
     *   await this.resource.resources
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
            ResourceService.Instance.GetEventChargesForResource(this.id)
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
     * Gets the NotificationSubscriptions for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.NotificationSubscriptions.then(resources => { ... })
     *   or
     *   await this.resource.resources
     *
    */
    public get NotificationSubscriptions(): Promise<NotificationSubscriptionData[]> {
        if (this._notificationSubscriptions !== null) {
            return Promise.resolve(this._notificationSubscriptions);
        }

        if (this._notificationSubscriptionsPromise !== null) {
            return this._notificationSubscriptionsPromise;
        }

        // Start the load
        this.loadNotificationSubscriptions();

        return this._notificationSubscriptionsPromise!;
    }



    private loadNotificationSubscriptions(): void {

        this._notificationSubscriptionsPromise = lastValueFrom(
            ResourceService.Instance.GetNotificationSubscriptionsForResource(this.id)
        )
            .then(NotificationSubscriptions => {
                this._notificationSubscriptions = NotificationSubscriptions ?? [];
                this._notificationSubscriptionsSubject.next(this._notificationSubscriptions);
                return this._notificationSubscriptions;
            })
            .catch(err => {
                this._notificationSubscriptions = [];
                this._notificationSubscriptionsSubject.next(this._notificationSubscriptions);
                throw err;
            })
            .finally(() => {
                this._notificationSubscriptionsPromise = null; // Allow retry if needed
            });
    }

    /**
     * Clears the cached NotificationSubscription. Call after mutations to force refresh.
     */
    public ClearNotificationSubscriptionsCache(): void {
        this._notificationSubscriptions = null;
        this._notificationSubscriptionsPromise = null;
        this._notificationSubscriptionsSubject.next(this._notificationSubscriptions);      // Emit to observable
    }

    public get HasNotificationSubscriptions(): Promise<boolean> {
        return this.NotificationSubscriptions.then(notificationSubscriptions => notificationSubscriptions.length > 0);
    }


    /**
     *
     * Gets the VolunteerProfiles for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.VolunteerProfiles.then(resources => { ... })
     *   or
     *   await this.resource.resources
     *
    */
    public get VolunteerProfiles(): Promise<VolunteerProfileData[]> {
        if (this._volunteerProfiles !== null) {
            return Promise.resolve(this._volunteerProfiles);
        }

        if (this._volunteerProfilesPromise !== null) {
            return this._volunteerProfilesPromise;
        }

        // Start the load
        this.loadVolunteerProfiles();

        return this._volunteerProfilesPromise!;
    }



    private loadVolunteerProfiles(): void {

        this._volunteerProfilesPromise = lastValueFrom(
            ResourceService.Instance.GetVolunteerProfilesForResource(this.id)
        )
            .then(VolunteerProfiles => {
                this._volunteerProfiles = VolunteerProfiles ?? [];
                this._volunteerProfilesSubject.next(this._volunteerProfiles);
                return this._volunteerProfiles;
            })
            .catch(err => {
                this._volunteerProfiles = [];
                this._volunteerProfilesSubject.next(this._volunteerProfiles);
                throw err;
            })
            .finally(() => {
                this._volunteerProfilesPromise = null; // Allow retry if needed
            });
    }

    /**
     * Clears the cached VolunteerProfile. Call after mutations to force refresh.
     */
    public ClearVolunteerProfilesCache(): void {
        this._volunteerProfiles = null;
        this._volunteerProfilesPromise = null;
        this._volunteerProfilesSubject.next(this._volunteerProfiles);      // Emit to observable
    }

    public get HasVolunteerProfiles(): Promise<boolean> {
        return this.VolunteerProfiles.then(volunteerProfiles => volunteerProfiles.length > 0);
    }


    /**
     *
     * Gets the VolunteerGroupMembers for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.VolunteerGroupMembers.then(resources => { ... })
     *   or
     *   await this.resource.resources
     *
    */
    public get VolunteerGroupMembers(): Promise<VolunteerGroupMemberData[]> {
        if (this._volunteerGroupMembers !== null) {
            return Promise.resolve(this._volunteerGroupMembers);
        }

        if (this._volunteerGroupMembersPromise !== null) {
            return this._volunteerGroupMembersPromise;
        }

        // Start the load
        this.loadVolunteerGroupMembers();

        return this._volunteerGroupMembersPromise!;
    }



    private loadVolunteerGroupMembers(): void {

        this._volunteerGroupMembersPromise = lastValueFrom(
            ResourceService.Instance.GetVolunteerGroupMembersForResource(this.id)
        )
            .then(VolunteerGroupMembers => {
                this._volunteerGroupMembers = VolunteerGroupMembers ?? [];
                this._volunteerGroupMembersSubject.next(this._volunteerGroupMembers);
                return this._volunteerGroupMembers;
            })
            .catch(err => {
                this._volunteerGroupMembers = [];
                this._volunteerGroupMembersSubject.next(this._volunteerGroupMembers);
                throw err;
            })
            .finally(() => {
                this._volunteerGroupMembersPromise = null; // Allow retry if needed
            });
    }

    /**
     * Clears the cached VolunteerGroupMember. Call after mutations to force refresh.
     */
    public ClearVolunteerGroupMembersCache(): void {
        this._volunteerGroupMembers = null;
        this._volunteerGroupMembersPromise = null;
        this._volunteerGroupMembersSubject.next(this._volunteerGroupMembers);      // Emit to observable
    }

    public get HasVolunteerGroupMembers(): Promise<boolean> {
        return this.VolunteerGroupMembers.then(volunteerGroupMembers => volunteerGroupMembers.length > 0);
    }


    /**
     *
     * Gets the EventResourceAssignments for this Resource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resource.EventResourceAssignments.then(resources => { ... })
     *   or
     *   await this.resource.resources
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
            ResourceService.Instance.GetEventResourceAssignmentsForResource(this.id)
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
    //   Template: {{ (resource.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await resource.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ResourceData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ResourceData>> {
        const info = await lastValueFrom(
            ResourceService.Instance.GetResourceChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ResourceData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ResourceData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ResourceSubmitData {
        return ResourceService.Instance.ConvertToResourceSubmitData(this);
    }
}


@Injectable({
    providedIn: 'root'
})
export class ResourceService extends SecureEndpointBase {

    private static _instance: ResourceService;
    private listCache: Map<string, Observable<Array<ResourceData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ResourceBasicListData>>>;
    private recordCache: Map<string, Observable<ResourceData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private resourceChangeHistoryService: ResourceChangeHistoryService,
        private resourceContactService: ResourceContactService,
        private rateSheetService: RateSheetService,
        private resourceQualificationService: ResourceQualificationService,
        private resourceAvailabilityService: ResourceAvailabilityService,
        private resourceShiftService: ResourceShiftService,
        private crewMemberService: CrewMemberService,
        private scheduledEventService: ScheduledEventService,
        private eventChargeService: EventChargeService,
        private notificationSubscriptionService: NotificationSubscriptionService,
        private volunteerProfileService: VolunteerProfileService,
        private volunteerGroupMemberService: VolunteerGroupMemberService,
        private eventResourceAssignmentService: EventResourceAssignmentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ResourceData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ResourceBasicListData>>>();
        this.recordCache = new Map<string, Observable<ResourceData>>();

        ResourceService._instance = this;
    }

    public static get Instance(): ResourceService {
        return ResourceService._instance;
    }


    public ClearListCaches(config: ResourceQueryParameters | null = null) {

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


    public ConvertToResourceSubmitData(data: ResourceData): ResourceSubmitData {

        let output = new ResourceSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.officeId = data.officeId;
        output.resourceTypeId = data.resourceTypeId;
        output.shiftPatternId = data.shiftPatternId;
        output.timeZoneId = data.timeZoneId;
        output.targetWeeklyWorkHours = data.targetWeeklyWorkHours;
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

    public GetResource(id: bigint | number, includeRelations: boolean = true): Observable<ResourceData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const resource$ = this.requestResource(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Resource", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, resource$);

            return resource$;
        }

        return this.recordCache.get(configHash) as Observable<ResourceData>;
    }

    private requestResource(id: bigint | number, includeRelations: boolean = true): Observable<ResourceData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceData>(this.baseUrl + 'api/Resource/' + id.toString(), {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveResource(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestResource(id, includeRelations));
            }));
    }

    public GetResourceList(config: ResourceQueryParameters | any = null): Observable<Array<ResourceData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const resourceList$ = this.requestResourceList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Resource list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, resourceList$);

            return resourceList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ResourceData>>;
    }


    private requestResourceList(config: ResourceQueryParameters | any): Observable<Array<ResourceData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceData>>(this.baseUrl + 'api/Resources', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(rawList => this.ReviveResourceList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceList(config));
            }));
    }

    public GetResourcesRowCount(config: ResourceQueryParameters | any = null): Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const resourcesRowCount$ = this.requestResourcesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Resources row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, resourcesRowCount$);

            return resourcesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestResourcesRowCount(config: ResourceQueryParameters | any): Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Resources/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourcesRowCount(config));
            }));
    }

    public GetResourcesBasicListData(config: ResourceQueryParameters | any = null): Observable<Array<ResourceBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const resourcesBasicListData$ = this.requestResourcesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Resources basic list data", error);

                    return throwError(() => error);
                })
            );

            this.basicListDataCache.set(configHash, resourcesBasicListData$);

            return resourcesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ResourceBasicListData>>;
    }


    private requestResourcesBasicListData(config: ResourceQueryParameters | any): Observable<Array<ResourceBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceBasicListData>>(this.baseUrl + 'api/Resources/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourcesBasicListData(config));
            }));

    }


    public PutResource(id: bigint | number, resource: ResourceSubmitData): Observable<ResourceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceData>(this.baseUrl + 'api/Resource/' + id.toString(), resource, { headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResource(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutResource(id, resource));
            }));
    }


    public PostResource(resource: ResourceSubmitData): Observable<ResourceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ResourceData>(this.baseUrl + 'api/Resource', resource, { headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResource(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PostResource(resource));
            }));
    }


    public DeleteResource(id: bigint | number): Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Resource/' + id.toString(), { headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteResource(id));
            }));
    }

    public RollbackResource(id: bigint | number, versionNumber: bigint | number): Observable<ResourceData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceData>(this.baseUrl + 'api/Resource/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResource(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackResource(id, versionNumber));
            }));
    }


    /**
     * Gets version metadata for a specific version of a Resource.
     */
    public GetResourceChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ResourceData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ResourceData>>(this.baseUrl + 'api/Resource/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetResourceChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Resource.
     */
    public GetResourceAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ResourceData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ResourceData>[]>(this.baseUrl + 'api/Resource/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetResourceAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Resource.
     */
    public GetResourceVersion(id: bigint | number, version: number): Observable<ResourceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceData>(this.baseUrl + 'api/Resource/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveResource(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetResourceVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Resource at a specific point in time.
     */
    public GetResourceStateAtTime(id: bigint | number, time: string): Observable<ResourceData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceData>(this.baseUrl + 'api/Resource/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveResource(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetResourceStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ResourceQueryParameters | any): string {

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

    public userIsSchedulerResourceReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerResourceReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Resources
        //
        if (userIsSchedulerResourceReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerResourceReader = user.readPermission >= 1;
            } else {
                userIsSchedulerResourceReader = false;
            }
        }

        return userIsSchedulerResourceReader;
    }


    public userIsSchedulerResourceWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerResourceWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Resources
        //
        if (userIsSchedulerResourceWriter == true) {
            let user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerResourceWriter = user.writePermission >= 50;
            } else {
                userIsSchedulerResourceWriter = false;
            }
        }

        return userIsSchedulerResourceWriter;
    }

    public GetResourceChangeHistoriesForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceChangeHistoryData[]> {
        return this.resourceChangeHistoryService.GetResourceChangeHistoryList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetResourceContactsForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceContactData[]> {
        return this.resourceContactService.GetResourceContactList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetRateSheetsForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<RateSheetData[]> {
        return this.rateSheetService.GetRateSheetList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetResourceQualificationsForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceQualificationData[]> {
        return this.resourceQualificationService.GetResourceQualificationList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetResourceAvailabilitiesForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceAvailabilityData[]> {
        return this.resourceAvailabilityService.GetResourceAvailabilityList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetResourceShiftsForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceShiftData[]> {
        return this.resourceShiftService.GetResourceShiftList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetCrewMembersForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CrewMemberData[]> {
        return this.crewMemberService.GetCrewMemberList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventsForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventData[]> {
        return this.scheduledEventService.GetScheduledEventList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventChargesForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventChargeData[]> {
        return this.eventChargeService.GetEventChargeList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetNotificationSubscriptionsForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<NotificationSubscriptionData[]> {
        return this.notificationSubscriptionService.GetNotificationSubscriptionList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetVolunteerProfilesForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<VolunteerProfileData[]> {
        return this.volunteerProfileService.GetVolunteerProfileList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetVolunteerGroupMembersForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<VolunteerGroupMemberData[]> {
        return this.volunteerGroupMemberService.GetVolunteerGroupMemberList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventResourceAssignmentsForResource(resourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventResourceAssignmentData[]> {
        return this.eventResourceAssignmentService.GetEventResourceAssignmentList({
            resourceId: resourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    /**
      *
      * Revives a plain object from the server into a full ResourceData instance.
      *
      * This is critical for the lazy-loading pattern to work correctly.
      *
      * When the server returns JSON, it is a plain object with no prototype methods
      * or observable properties. This method:
      * 1. Re-attaches the ResourceData prototype
      * 2. Copies all properties from the raw object
      * 3. Re-initializes all private caches and BehaviorSubjects
      * 4. Re-creates all public observable properties ($ suffixed) with their
      *    original tap() triggers that initiate lazy loading on first subscription
      *
      * Without this, revived objects would not trigger loads when ResourceTags$ etc.
      * are subscribed to in templates.
      *
      */
    public ReviveResource(raw: any): ResourceData {
        if (!raw) return raw;

        //
        // Create a ResourceData object instance with correct prototype
        //
        const revived = Object.create(ResourceData.prototype) as ResourceData;

        //
        // Copy all raw properties
        //
        Object.assign(revived, raw);

        //
        // Explicitly initialize all private caches
        // This ensures the getters work correctly on revived objects
        //
        (revived as any)._resourceChangeHistories = null;
        (revived as any)._resourceChangeHistoriesPromise = null;
        (revived as any)._resourceChangeHistoriesSubject = new BehaviorSubject<ResourceChangeHistoryData[] | null>(null);

        (revived as any)._resourceContacts = null;
        (revived as any)._resourceContactsPromise = null;
        (revived as any)._resourceContactsSubject = new BehaviorSubject<ResourceContactData[] | null>(null);

        (revived as any)._rateSheets = null;
        (revived as any)._rateSheetsPromise = null;
        (revived as any)._rateSheetsSubject = new BehaviorSubject<RateSheetData[] | null>(null);

        (revived as any)._resourceQualifications = null;
        (revived as any)._resourceQualificationsPromise = null;
        (revived as any)._resourceQualificationsSubject = new BehaviorSubject<ResourceQualificationData[] | null>(null);

        (revived as any)._resourceAvailabilities = null;
        (revived as any)._resourceAvailabilitiesPromise = null;
        (revived as any)._resourceAvailabilitiesSubject = new BehaviorSubject<ResourceAvailabilityData[] | null>(null);

        (revived as any)._resourceShifts = null;
        (revived as any)._resourceShiftsPromise = null;
        (revived as any)._resourceShiftsSubject = new BehaviorSubject<ResourceShiftData[] | null>(null);

        (revived as any)._crewMembers = null;
        (revived as any)._crewMembersPromise = null;
        (revived as any)._crewMembersSubject = new BehaviorSubject<CrewMemberData[] | null>(null);

        (revived as any)._scheduledEvents = null;
        (revived as any)._scheduledEventsPromise = null;
        (revived as any)._scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

        (revived as any)._eventCharges = null;
        (revived as any)._eventChargesPromise = null;
        (revived as any)._eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);

        (revived as any)._notificationSubscriptions = null;
        (revived as any)._notificationSubscriptionsPromise = null;
        (revived as any)._notificationSubscriptionsSubject = new BehaviorSubject<NotificationSubscriptionData[] | null>(null);

        (revived as any)._volunteerProfiles = null;
        (revived as any)._volunteerProfilesPromise = null;
        (revived as any)._volunteerProfilesSubject = new BehaviorSubject<VolunteerProfileData[] | null>(null);

        (revived as any)._volunteerGroupMembers = null;
        (revived as any)._volunteerGroupMembersPromise = null;
        (revived as any)._volunteerGroupMembersSubject = new BehaviorSubject<VolunteerGroupMemberData[] | null>(null);

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
        // 2. But private methods (loadResourceXYZ, etc.) are not accessible via the typed variable
        // 3. This is a controlled revival context — safe and necessary
        //
        (revived as any).ResourceChangeHistories$ = (revived as any)._resourceChangeHistoriesSubject.asObservable().pipe(
            tap(() => {
                if ((revived as any)._resourceChangeHistories === null && (revived as any)._resourceChangeHistoriesPromise === null) {
                    (revived as any).loadResourceChangeHistories();        // Need to cast to any to invoke private load method
                }
            }),
            shareReplay(1)
        );

        (revived as any).ResourceChangeHistoriesCount$ = ResourceChangeHistoryService.Instance.GetResourceChangeHistoriesRowCount({
            resourceId: (revived as any).id,
            active: true,
            deleted: false
        });



        (revived as any).ResourceContacts$ = (revived as any)._resourceContactsSubject.asObservable().pipe(
            tap(() => {
                if ((revived as any)._resourceContacts === null && (revived as any)._resourceContactsPromise === null) {
                    (revived as any).loadResourceContacts();        // Need to cast to any to invoke private load method
                }
            }),
            shareReplay(1)
        );

        (revived as any).ResourceContactsCount$ = ResourceContactService.Instance.GetResourceContactsRowCount({
            resourceId: (revived as any).id,
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

        (revived as any).RateSheetsCount$ = RateSheetService.Instance.GetRateSheetsRowCount({
            resourceId: (revived as any).id,
            active: true,
            deleted: false
        });



        (revived as any).ResourceQualifications$ = (revived as any)._resourceQualificationsSubject.asObservable().pipe(
            tap(() => {
                if ((revived as any)._resourceQualifications === null && (revived as any)._resourceQualificationsPromise === null) {
                    (revived as any).loadResourceQualifications();        // Need to cast to any to invoke private load method
                }
            }),
            shareReplay(1)
        );

        (revived as any).ResourceQualificationsCount$ = ResourceQualificationService.Instance.GetResourceQualificationsRowCount({
            resourceId: (revived as any).id,
            active: true,
            deleted: false
        });



        (revived as any).ResourceAvailabilities$ = (revived as any)._resourceAvailabilitiesSubject.asObservable().pipe(
            tap(() => {
                if ((revived as any)._resourceAvailabilities === null && (revived as any)._resourceAvailabilitiesPromise === null) {
                    (revived as any).loadResourceAvailabilities();        // Need to cast to any to invoke private load method
                }
            }),
            shareReplay(1)
        );

        (revived as any).ResourceAvailabilitiesCount$ = ResourceAvailabilityService.Instance.GetResourceAvailabilitiesRowCount({
            resourceId: (revived as any).id,
            active: true,
            deleted: false
        });



        (revived as any).ResourceShifts$ = (revived as any)._resourceShiftsSubject.asObservable().pipe(
            tap(() => {
                if ((revived as any)._resourceShifts === null && (revived as any)._resourceShiftsPromise === null) {
                    (revived as any).loadResourceShifts();        // Need to cast to any to invoke private load method
                }
            }),
            shareReplay(1)
        );

        (revived as any).ResourceShiftsCount$ = ResourceShiftService.Instance.GetResourceShiftsRowCount({
            resourceId: (revived as any).id,
            active: true,
            deleted: false
        });



        (revived as any).CrewMembers$ = (revived as any)._crewMembersSubject.asObservable().pipe(
            tap(() => {
                if ((revived as any)._crewMembers === null && (revived as any)._crewMembersPromise === null) {
                    (revived as any).loadCrewMembers();        // Need to cast to any to invoke private load method
                }
            }),
            shareReplay(1)
        );

        (revived as any).CrewMembersCount$ = CrewMemberService.Instance.GetCrewMembersRowCount({
            resourceId: (revived as any).id,
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

        (revived as any).ScheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({
            resourceId: (revived as any).id,
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

        (revived as any).EventChargesCount$ = EventChargeService.Instance.GetEventChargesRowCount({
            resourceId: (revived as any).id,
            active: true,
            deleted: false
        });



        (revived as any).NotificationSubscriptions$ = (revived as any)._notificationSubscriptionsSubject.asObservable().pipe(
            tap(() => {
                if ((revived as any)._notificationSubscriptions === null && (revived as any)._notificationSubscriptionsPromise === null) {
                    (revived as any).loadNotificationSubscriptions();        // Need to cast to any to invoke private load method
                }
            }),
            shareReplay(1)
        );

        (revived as any).NotificationSubscriptionsCount$ = NotificationSubscriptionService.Instance.GetNotificationSubscriptionsRowCount({
            resourceId: (revived as any).id,
            active: true,
            deleted: false
        });



        (revived as any).VolunteerProfiles$ = (revived as any)._volunteerProfilesSubject.asObservable().pipe(
            tap(() => {
                if ((revived as any)._volunteerProfiles === null && (revived as any)._volunteerProfilesPromise === null) {
                    (revived as any).loadVolunteerProfiles();        // Need to cast to any to invoke private load method
                }
            }),
            shareReplay(1)
        );

        (revived as any).VolunteerProfilesCount$ = VolunteerProfileService.Instance.GetVolunteerProfilesRowCount({
            resourceId: (revived as any).id,
            active: true,
            deleted: false
        });



        (revived as any).VolunteerGroupMembers$ = (revived as any)._volunteerGroupMembersSubject.asObservable().pipe(
            tap(() => {
                if ((revived as any)._volunteerGroupMembers === null && (revived as any)._volunteerGroupMembersPromise === null) {
                    (revived as any).loadVolunteerGroupMembers();        // Need to cast to any to invoke private load method
                }
            }),
            shareReplay(1)
        );

        (revived as any).VolunteerGroupMembersCount$ = VolunteerGroupMemberService.Instance.GetVolunteerGroupMembersRowCount({
            resourceId: (revived as any).id,
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

        (revived as any).EventResourceAssignmentsCount$ = EventResourceAssignmentService.Instance.GetEventResourceAssignmentsRowCount({
            resourceId: (revived as any).id,
            active: true,
            deleted: false
        });




        //
        // Version history metadata cache and observable
        //
        (revived as any)._currentVersionInfo = null;
        (revived as any)._currentVersionInfoPromise = null;
        (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ResourceData> | null>(null);

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

    private ReviveResourceList(rawList: any[]): ResourceData[] {

        if (!rawList) {
            return [];
        }

        return rawList.map(raw => this.ReviveResource(raw));
    }

}
