/*

   GENERATED SERVICE FOR THE SCHEDULEDEVENTTEMPLATE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ScheduledEventTemplate table.

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
import { SchedulingTargetTypeData } from './scheduling-target-type.service';
import { PriorityData } from './priority.service';
import { ScheduledEventTemplateChangeHistoryService, ScheduledEventTemplateChangeHistoryData } from './scheduled-event-template-change-history.service';
import { ScheduledEventTemplateChargeService, ScheduledEventTemplateChargeData } from './scheduled-event-template-charge.service';
import { ScheduledEventTemplateQualificationRequirementService, ScheduledEventTemplateQualificationRequirementData } from './scheduled-event-template-qualification-requirement.service';
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
export class ScheduledEventTemplateQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    defaultAllDay: boolean | null | undefined = null;
    defaultDurationMinutes: bigint | number | null | undefined = null;
    schedulingTargetTypeId: bigint | number | null | undefined = null;
    priorityId: bigint | number | null | undefined = null;
    defaultLocationPattern: string | null | undefined = null;
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
export class ScheduledEventTemplateSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    defaultAllDay!: boolean;
    defaultDurationMinutes!: bigint | number;
    schedulingTargetTypeId: bigint | number | null = null;
    priorityId: bigint | number | null = null;
    defaultLocationPattern: string | null = null;
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

export class ScheduledEventTemplateBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ScheduledEventTemplateChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `scheduledEventTemplate.ScheduledEventTemplateChildren$` — use with `| async` in templates
//        • Promise:    `scheduledEventTemplate.ScheduledEventTemplateChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="scheduledEventTemplate.ScheduledEventTemplateChildren$ | async"`), or
//        • Access the promise getter (`scheduledEventTemplate.ScheduledEventTemplateChildren` or `await scheduledEventTemplate.ScheduledEventTemplateChildren`)
//    - Simply reading `scheduledEventTemplate.ScheduledEventTemplateChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await scheduledEventTemplate.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ScheduledEventTemplateData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    defaultAllDay!: boolean;
    defaultDurationMinutes!: bigint | number;
    schedulingTargetTypeId!: bigint | number;
    priorityId!: bigint | number;
    defaultLocationPattern!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    priority: PriorityData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    schedulingTargetType: SchedulingTargetTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _scheduledEventTemplateChangeHistories: ScheduledEventTemplateChangeHistoryData[] | null = null;
    private _scheduledEventTemplateChangeHistoriesPromise: Promise<ScheduledEventTemplateChangeHistoryData[]> | null  = null;
    private _scheduledEventTemplateChangeHistoriesSubject = new BehaviorSubject<ScheduledEventTemplateChangeHistoryData[] | null>(null);

                
    private _scheduledEventTemplateCharges: ScheduledEventTemplateChargeData[] | null = null;
    private _scheduledEventTemplateChargesPromise: Promise<ScheduledEventTemplateChargeData[]> | null  = null;
    private _scheduledEventTemplateChargesSubject = new BehaviorSubject<ScheduledEventTemplateChargeData[] | null>(null);

                
    private _scheduledEventTemplateQualificationRequirements: ScheduledEventTemplateQualificationRequirementData[] | null = null;
    private _scheduledEventTemplateQualificationRequirementsPromise: Promise<ScheduledEventTemplateQualificationRequirementData[]> | null  = null;
    private _scheduledEventTemplateQualificationRequirementsSubject = new BehaviorSubject<ScheduledEventTemplateQualificationRequirementData[] | null>(null);

                
    private _scheduledEvents: ScheduledEventData[] | null = null;
    private _scheduledEventsPromise: Promise<ScheduledEventData[]> | null  = null;
    private _scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ScheduledEventTemplateData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ScheduledEventTemplateData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduledEventTemplateData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ScheduledEventTemplateChangeHistories$ = this._scheduledEventTemplateChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventTemplateChangeHistories === null && this._scheduledEventTemplateChangeHistoriesPromise === null) {
            this.loadScheduledEventTemplateChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventTemplateChangeHistoriesCount$ = ScheduledEventTemplateChangeHistoryService.Instance.GetScheduledEventTemplateChangeHistoriesRowCount({scheduledEventTemplateId: this.id,
      active: true,
      deleted: false
    });



    public ScheduledEventTemplateCharges$ = this._scheduledEventTemplateChargesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventTemplateCharges === null && this._scheduledEventTemplateChargesPromise === null) {
            this.loadScheduledEventTemplateCharges(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventTemplateChargesCount$ = ScheduledEventTemplateChargeService.Instance.GetScheduledEventTemplateChargesRowCount({scheduledEventTemplateId: this.id,
      active: true,
      deleted: false
    });



    public ScheduledEventTemplateQualificationRequirements$ = this._scheduledEventTemplateQualificationRequirementsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventTemplateQualificationRequirements === null && this._scheduledEventTemplateQualificationRequirementsPromise === null) {
            this.loadScheduledEventTemplateQualificationRequirements(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventTemplateQualificationRequirementsCount$ = ScheduledEventTemplateQualificationRequirementService.Instance.GetScheduledEventTemplateQualificationRequirementsRowCount({scheduledEventTemplateId: this.id,
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

  
    public ScheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({scheduledEventTemplateId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ScheduledEventTemplateData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.scheduledEventTemplate.Reload();
  //
  //  Non Async:
  //
  //     scheduledEventTemplate[0].Reload().then(x => {
  //        this.scheduledEventTemplate = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ScheduledEventTemplateService.Instance.GetScheduledEventTemplate(this.id, includeRelations)
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
     this._scheduledEventTemplateChangeHistories = null;
     this._scheduledEventTemplateChangeHistoriesPromise = null;
     this._scheduledEventTemplateChangeHistoriesSubject.next(null);

     this._scheduledEventTemplateCharges = null;
     this._scheduledEventTemplateChargesPromise = null;
     this._scheduledEventTemplateChargesSubject.next(null);

     this._scheduledEventTemplateQualificationRequirements = null;
     this._scheduledEventTemplateQualificationRequirementsPromise = null;
     this._scheduledEventTemplateQualificationRequirementsSubject.next(null);

     this._scheduledEvents = null;
     this._scheduledEventsPromise = null;
     this._scheduledEventsSubject.next(null);

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
     * Gets the ScheduledEventTemplateChangeHistories for this ScheduledEventTemplate.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEventTemplate.ScheduledEventTemplateChangeHistories.then(scheduledEventTemplates => { ... })
     *   or
     *   await this.scheduledEventTemplate.scheduledEventTemplates
     *
    */
    public get ScheduledEventTemplateChangeHistories(): Promise<ScheduledEventTemplateChangeHistoryData[]> {
        if (this._scheduledEventTemplateChangeHistories !== null) {
            return Promise.resolve(this._scheduledEventTemplateChangeHistories);
        }

        if (this._scheduledEventTemplateChangeHistoriesPromise !== null) {
            return this._scheduledEventTemplateChangeHistoriesPromise;
        }

        // Start the load
        this.loadScheduledEventTemplateChangeHistories();

        return this._scheduledEventTemplateChangeHistoriesPromise!;
    }



    private loadScheduledEventTemplateChangeHistories(): void {

        this._scheduledEventTemplateChangeHistoriesPromise = lastValueFrom(
            ScheduledEventTemplateService.Instance.GetScheduledEventTemplateChangeHistoriesForScheduledEventTemplate(this.id)
        )
        .then(ScheduledEventTemplateChangeHistories => {
            this._scheduledEventTemplateChangeHistories = ScheduledEventTemplateChangeHistories ?? [];
            this._scheduledEventTemplateChangeHistoriesSubject.next(this._scheduledEventTemplateChangeHistories);
            return this._scheduledEventTemplateChangeHistories;
         })
        .catch(err => {
            this._scheduledEventTemplateChangeHistories = [];
            this._scheduledEventTemplateChangeHistoriesSubject.next(this._scheduledEventTemplateChangeHistories);
            throw err;
        })
        .finally(() => {
            this._scheduledEventTemplateChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventTemplateChangeHistory. Call after mutations to force refresh.
     */
    public ClearScheduledEventTemplateChangeHistoriesCache(): void {
        this._scheduledEventTemplateChangeHistories = null;
        this._scheduledEventTemplateChangeHistoriesPromise = null;
        this._scheduledEventTemplateChangeHistoriesSubject.next(this._scheduledEventTemplateChangeHistories);      // Emit to observable
    }

    public get HasScheduledEventTemplateChangeHistories(): Promise<boolean> {
        return this.ScheduledEventTemplateChangeHistories.then(scheduledEventTemplateChangeHistories => scheduledEventTemplateChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ScheduledEventTemplateCharges for this ScheduledEventTemplate.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEventTemplate.ScheduledEventTemplateCharges.then(scheduledEventTemplates => { ... })
     *   or
     *   await this.scheduledEventTemplate.scheduledEventTemplates
     *
    */
    public get ScheduledEventTemplateCharges(): Promise<ScheduledEventTemplateChargeData[]> {
        if (this._scheduledEventTemplateCharges !== null) {
            return Promise.resolve(this._scheduledEventTemplateCharges);
        }

        if (this._scheduledEventTemplateChargesPromise !== null) {
            return this._scheduledEventTemplateChargesPromise;
        }

        // Start the load
        this.loadScheduledEventTemplateCharges();

        return this._scheduledEventTemplateChargesPromise!;
    }



    private loadScheduledEventTemplateCharges(): void {

        this._scheduledEventTemplateChargesPromise = lastValueFrom(
            ScheduledEventTemplateService.Instance.GetScheduledEventTemplateChargesForScheduledEventTemplate(this.id)
        )
        .then(ScheduledEventTemplateCharges => {
            this._scheduledEventTemplateCharges = ScheduledEventTemplateCharges ?? [];
            this._scheduledEventTemplateChargesSubject.next(this._scheduledEventTemplateCharges);
            return this._scheduledEventTemplateCharges;
         })
        .catch(err => {
            this._scheduledEventTemplateCharges = [];
            this._scheduledEventTemplateChargesSubject.next(this._scheduledEventTemplateCharges);
            throw err;
        })
        .finally(() => {
            this._scheduledEventTemplateChargesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventTemplateCharge. Call after mutations to force refresh.
     */
    public ClearScheduledEventTemplateChargesCache(): void {
        this._scheduledEventTemplateCharges = null;
        this._scheduledEventTemplateChargesPromise = null;
        this._scheduledEventTemplateChargesSubject.next(this._scheduledEventTemplateCharges);      // Emit to observable
    }

    public get HasScheduledEventTemplateCharges(): Promise<boolean> {
        return this.ScheduledEventTemplateCharges.then(scheduledEventTemplateCharges => scheduledEventTemplateCharges.length > 0);
    }


    /**
     *
     * Gets the ScheduledEventTemplateQualificationRequirements for this ScheduledEventTemplate.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEventTemplate.ScheduledEventTemplateQualificationRequirements.then(scheduledEventTemplates => { ... })
     *   or
     *   await this.scheduledEventTemplate.scheduledEventTemplates
     *
    */
    public get ScheduledEventTemplateQualificationRequirements(): Promise<ScheduledEventTemplateQualificationRequirementData[]> {
        if (this._scheduledEventTemplateQualificationRequirements !== null) {
            return Promise.resolve(this._scheduledEventTemplateQualificationRequirements);
        }

        if (this._scheduledEventTemplateQualificationRequirementsPromise !== null) {
            return this._scheduledEventTemplateQualificationRequirementsPromise;
        }

        // Start the load
        this.loadScheduledEventTemplateQualificationRequirements();

        return this._scheduledEventTemplateQualificationRequirementsPromise!;
    }



    private loadScheduledEventTemplateQualificationRequirements(): void {

        this._scheduledEventTemplateQualificationRequirementsPromise = lastValueFrom(
            ScheduledEventTemplateService.Instance.GetScheduledEventTemplateQualificationRequirementsForScheduledEventTemplate(this.id)
        )
        .then(ScheduledEventTemplateQualificationRequirements => {
            this._scheduledEventTemplateQualificationRequirements = ScheduledEventTemplateQualificationRequirements ?? [];
            this._scheduledEventTemplateQualificationRequirementsSubject.next(this._scheduledEventTemplateQualificationRequirements);
            return this._scheduledEventTemplateQualificationRequirements;
         })
        .catch(err => {
            this._scheduledEventTemplateQualificationRequirements = [];
            this._scheduledEventTemplateQualificationRequirementsSubject.next(this._scheduledEventTemplateQualificationRequirements);
            throw err;
        })
        .finally(() => {
            this._scheduledEventTemplateQualificationRequirementsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventTemplateQualificationRequirement. Call after mutations to force refresh.
     */
    public ClearScheduledEventTemplateQualificationRequirementsCache(): void {
        this._scheduledEventTemplateQualificationRequirements = null;
        this._scheduledEventTemplateQualificationRequirementsPromise = null;
        this._scheduledEventTemplateQualificationRequirementsSubject.next(this._scheduledEventTemplateQualificationRequirements);      // Emit to observable
    }

    public get HasScheduledEventTemplateQualificationRequirements(): Promise<boolean> {
        return this.ScheduledEventTemplateQualificationRequirements.then(scheduledEventTemplateQualificationRequirements => scheduledEventTemplateQualificationRequirements.length > 0);
    }


    /**
     *
     * Gets the ScheduledEvents for this ScheduledEventTemplate.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEventTemplate.ScheduledEvents.then(scheduledEventTemplates => { ... })
     *   or
     *   await this.scheduledEventTemplate.scheduledEventTemplates
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
            ScheduledEventTemplateService.Instance.GetScheduledEventsForScheduledEventTemplate(this.id)
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
    //   Template: {{ (scheduledEventTemplate.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await scheduledEventTemplate.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ScheduledEventTemplateData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ScheduledEventTemplateData>> {
        const info = await lastValueFrom(
            ScheduledEventTemplateService.Instance.GetScheduledEventTemplateChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ScheduledEventTemplateData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ScheduledEventTemplateData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ScheduledEventTemplateSubmitData {
        return ScheduledEventTemplateService.Instance.ConvertToScheduledEventTemplateSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ScheduledEventTemplateService extends SecureEndpointBase {

    private static _instance: ScheduledEventTemplateService;
    private listCache: Map<string, Observable<Array<ScheduledEventTemplateData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ScheduledEventTemplateBasicListData>>>;
    private recordCache: Map<string, Observable<ScheduledEventTemplateData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private scheduledEventTemplateChangeHistoryService: ScheduledEventTemplateChangeHistoryService,
        private scheduledEventTemplateChargeService: ScheduledEventTemplateChargeService,
        private scheduledEventTemplateQualificationRequirementService: ScheduledEventTemplateQualificationRequirementService,
        private scheduledEventService: ScheduledEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ScheduledEventTemplateData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ScheduledEventTemplateBasicListData>>>();
        this.recordCache = new Map<string, Observable<ScheduledEventTemplateData>>();

        ScheduledEventTemplateService._instance = this;
    }

    public static get Instance(): ScheduledEventTemplateService {
      return ScheduledEventTemplateService._instance;
    }


    public ClearListCaches(config: ScheduledEventTemplateQueryParameters | null = null) {

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


    public ConvertToScheduledEventTemplateSubmitData(data: ScheduledEventTemplateData): ScheduledEventTemplateSubmitData {

        let output = new ScheduledEventTemplateSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.defaultAllDay = data.defaultAllDay;
        output.defaultDurationMinutes = data.defaultDurationMinutes;
        output.schedulingTargetTypeId = data.schedulingTargetTypeId;
        output.priorityId = data.priorityId;
        output.defaultLocationPattern = data.defaultLocationPattern;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetScheduledEventTemplate(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventTemplateData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const scheduledEventTemplate$ = this.requestScheduledEventTemplate(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplate", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, scheduledEventTemplate$);

            return scheduledEventTemplate$;
        }

        return this.recordCache.get(configHash) as Observable<ScheduledEventTemplateData>;
    }

    private requestScheduledEventTemplate(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventTemplateData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventTemplateData>(this.baseUrl + 'api/ScheduledEventTemplate/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveScheduledEventTemplate(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplate(id, includeRelations));
            }));
    }

    public GetScheduledEventTemplateList(config: ScheduledEventTemplateQueryParameters | any = null) : Observable<Array<ScheduledEventTemplateData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const scheduledEventTemplateList$ = this.requestScheduledEventTemplateList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplate list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, scheduledEventTemplateList$);

            return scheduledEventTemplateList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ScheduledEventTemplateData>>;
    }


    private requestScheduledEventTemplateList(config: ScheduledEventTemplateQueryParameters | any) : Observable <Array<ScheduledEventTemplateData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventTemplateData>>(this.baseUrl + 'api/ScheduledEventTemplates', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveScheduledEventTemplateList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateList(config));
            }));
    }

    public GetScheduledEventTemplatesRowCount(config: ScheduledEventTemplateQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const scheduledEventTemplatesRowCount$ = this.requestScheduledEventTemplatesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplates row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, scheduledEventTemplatesRowCount$);

            return scheduledEventTemplatesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestScheduledEventTemplatesRowCount(config: ScheduledEventTemplateQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ScheduledEventTemplates/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplatesRowCount(config));
            }));
    }

    public GetScheduledEventTemplatesBasicListData(config: ScheduledEventTemplateQueryParameters | any = null) : Observable<Array<ScheduledEventTemplateBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const scheduledEventTemplatesBasicListData$ = this.requestScheduledEventTemplatesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplates basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, scheduledEventTemplatesBasicListData$);

            return scheduledEventTemplatesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ScheduledEventTemplateBasicListData>>;
    }


    private requestScheduledEventTemplatesBasicListData(config: ScheduledEventTemplateQueryParameters | any) : Observable<Array<ScheduledEventTemplateBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventTemplateBasicListData>>(this.baseUrl + 'api/ScheduledEventTemplates/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplatesBasicListData(config));
            }));

    }


    public PutScheduledEventTemplate(id: bigint | number, scheduledEventTemplate: ScheduledEventTemplateSubmitData) : Observable<ScheduledEventTemplateData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventTemplateData>(this.baseUrl + 'api/ScheduledEventTemplate/' + id.toString(), scheduledEventTemplate, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplate(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutScheduledEventTemplate(id, scheduledEventTemplate));
            }));
    }


    public PostScheduledEventTemplate(scheduledEventTemplate: ScheduledEventTemplateSubmitData) : Observable<ScheduledEventTemplateData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ScheduledEventTemplateData>(this.baseUrl + 'api/ScheduledEventTemplate', scheduledEventTemplate, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplate(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostScheduledEventTemplate(scheduledEventTemplate));
            }));
    }

  
    public DeleteScheduledEventTemplate(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ScheduledEventTemplate/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteScheduledEventTemplate(id));
            }));
    }

    public RollbackScheduledEventTemplate(id: bigint | number, versionNumber: bigint | number) : Observable<ScheduledEventTemplateData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventTemplateData>(this.baseUrl + 'api/ScheduledEventTemplate/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplate(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackScheduledEventTemplate(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ScheduledEventTemplate.
     */
    public GetScheduledEventTemplateChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ScheduledEventTemplateData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduledEventTemplateData>>(this.baseUrl + 'api/ScheduledEventTemplate/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventTemplateChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ScheduledEventTemplate.
     */
    public GetScheduledEventTemplateAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ScheduledEventTemplateData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduledEventTemplateData>[]>(this.baseUrl + 'api/ScheduledEventTemplate/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventTemplateAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ScheduledEventTemplate.
     */
    public GetScheduledEventTemplateVersion(id: bigint | number, version: number): Observable<ScheduledEventTemplateData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventTemplateData>(this.baseUrl + 'api/ScheduledEventTemplate/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduledEventTemplate(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventTemplateVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ScheduledEventTemplate at a specific point in time.
     */
    public GetScheduledEventTemplateStateAtTime(id: bigint | number, time: string): Observable<ScheduledEventTemplateData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventTemplateData>(this.baseUrl + 'api/ScheduledEventTemplate/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduledEventTemplate(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventTemplateStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ScheduledEventTemplateQueryParameters | any): string {

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

    public userIsSchedulerScheduledEventTemplateReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerScheduledEventTemplateReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ScheduledEventTemplates
        //
        if (userIsSchedulerScheduledEventTemplateReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerScheduledEventTemplateReader = user.readPermission >= 1;
            } else {
                userIsSchedulerScheduledEventTemplateReader = false;
            }
        }

        return userIsSchedulerScheduledEventTemplateReader;
    }


    public userIsSchedulerScheduledEventTemplateWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerScheduledEventTemplateWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ScheduledEventTemplates
        //
        if (userIsSchedulerScheduledEventTemplateWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerScheduledEventTemplateWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerScheduledEventTemplateWriter = false;
          }      
        }

        return userIsSchedulerScheduledEventTemplateWriter;
    }

    public GetScheduledEventTemplateChangeHistoriesForScheduledEventTemplate(scheduledEventTemplateId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventTemplateChangeHistoryData[]> {
        return this.scheduledEventTemplateChangeHistoryService.GetScheduledEventTemplateChangeHistoryList({
            scheduledEventTemplateId: scheduledEventTemplateId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventTemplateChargesForScheduledEventTemplate(scheduledEventTemplateId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventTemplateChargeData[]> {
        return this.scheduledEventTemplateChargeService.GetScheduledEventTemplateChargeList({
            scheduledEventTemplateId: scheduledEventTemplateId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventTemplateQualificationRequirementsForScheduledEventTemplate(scheduledEventTemplateId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventTemplateQualificationRequirementData[]> {
        return this.scheduledEventTemplateQualificationRequirementService.GetScheduledEventTemplateQualificationRequirementList({
            scheduledEventTemplateId: scheduledEventTemplateId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventsForScheduledEventTemplate(scheduledEventTemplateId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventData[]> {
        return this.scheduledEventService.GetScheduledEventList({
            scheduledEventTemplateId: scheduledEventTemplateId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ScheduledEventTemplateData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ScheduledEventTemplateData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ScheduledEventTemplateTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveScheduledEventTemplate(raw: any): ScheduledEventTemplateData {
    if (!raw) return raw;

    //
    // Create a ScheduledEventTemplateData object instance with correct prototype
    //
    const revived = Object.create(ScheduledEventTemplateData.prototype) as ScheduledEventTemplateData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._scheduledEventTemplateChangeHistories = null;
    (revived as any)._scheduledEventTemplateChangeHistoriesPromise = null;
    (revived as any)._scheduledEventTemplateChangeHistoriesSubject = new BehaviorSubject<ScheduledEventTemplateChangeHistoryData[] | null>(null);

    (revived as any)._scheduledEventTemplateCharges = null;
    (revived as any)._scheduledEventTemplateChargesPromise = null;
    (revived as any)._scheduledEventTemplateChargesSubject = new BehaviorSubject<ScheduledEventTemplateChargeData[] | null>(null);

    (revived as any)._scheduledEventTemplateQualificationRequirements = null;
    (revived as any)._scheduledEventTemplateQualificationRequirementsPromise = null;
    (revived as any)._scheduledEventTemplateQualificationRequirementsSubject = new BehaviorSubject<ScheduledEventTemplateQualificationRequirementData[] | null>(null);

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
    // 2. But private methods (loadScheduledEventTemplateXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ScheduledEventTemplateChangeHistories$ = (revived as any)._scheduledEventTemplateChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventTemplateChangeHistories === null && (revived as any)._scheduledEventTemplateChangeHistoriesPromise === null) {
                (revived as any).loadScheduledEventTemplateChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventTemplateChangeHistoriesCount$ = ScheduledEventTemplateChangeHistoryService.Instance.GetScheduledEventTemplateChangeHistoriesRowCount({scheduledEventTemplateId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduledEventTemplateCharges$ = (revived as any)._scheduledEventTemplateChargesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventTemplateCharges === null && (revived as any)._scheduledEventTemplateChargesPromise === null) {
                (revived as any).loadScheduledEventTemplateCharges();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventTemplateChargesCount$ = ScheduledEventTemplateChargeService.Instance.GetScheduledEventTemplateChargesRowCount({scheduledEventTemplateId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduledEventTemplateQualificationRequirements$ = (revived as any)._scheduledEventTemplateQualificationRequirementsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventTemplateQualificationRequirements === null && (revived as any)._scheduledEventTemplateQualificationRequirementsPromise === null) {
                (revived as any).loadScheduledEventTemplateQualificationRequirements();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventTemplateQualificationRequirementsCount$ = ScheduledEventTemplateQualificationRequirementService.Instance.GetScheduledEventTemplateQualificationRequirementsRowCount({scheduledEventTemplateId: (revived as any).id,
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

    (revived as any).ScheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({scheduledEventTemplateId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduledEventTemplateData> | null>(null);

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

  private ReviveScheduledEventTemplateList(rawList: any[]): ScheduledEventTemplateData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveScheduledEventTemplate(raw));
  }

}
