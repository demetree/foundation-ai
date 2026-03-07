/*

   GENERATED SERVICE FOR THE CHARGETYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ChargeType table.

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
import { RateTypeData } from './rate-type.service';
import { CurrencyData } from './currency.service';
import { FinancialCategoryData } from './financial-category.service';
import { TaxCodeData } from './tax-code.service';
import { ChargeTypeChangeHistoryService, ChargeTypeChangeHistoryData } from './charge-type-change-history.service';
import { ScheduledEventTemplateChargeService, ScheduledEventTemplateChargeData } from './scheduled-event-template-charge.service';
import { EventChargeService, EventChargeData } from './event-charge.service';
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
export class ChargeTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    externalId: string | null | undefined = null;
    isRevenue: boolean | null | undefined = null;
    isTaxable: boolean | null | undefined = null;
    defaultAmount: number | null | undefined = null;
    defaultDescription: string | null | undefined = null;
    rateTypeId: bigint | number | null | undefined = null;
    currencyId: bigint | number | null | undefined = null;
    financialCategoryId: bigint | number | null | undefined = null;
    taxCodeId: bigint | number | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class ChargeTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    externalId: string | null = null;
    isRevenue!: boolean;
    isTaxable: boolean | null = null;
    defaultAmount: number | null = null;
    defaultDescription: string | null = null;
    rateTypeId: bigint | number | null = null;
    currencyId!: bigint | number;
    financialCategoryId: bigint | number | null = null;
    taxCodeId: bigint | number | null = null;
    sequence: bigint | number | null = null;
    color: string | null = null;
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

export class ChargeTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ChargeTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `chargeType.ChargeTypeChildren$` — use with `| async` in templates
//        • Promise:    `chargeType.ChargeTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="chargeType.ChargeTypeChildren$ | async"`), or
//        • Access the promise getter (`chargeType.ChargeTypeChildren` or `await chargeType.ChargeTypeChildren`)
//    - Simply reading `chargeType.ChargeTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await chargeType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ChargeTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    externalId!: string | null;
    isRevenue!: boolean;
    isTaxable!: boolean | null;
    defaultAmount!: number | null;
    defaultDescription!: string | null;
    rateTypeId!: bigint | number;
    currencyId!: bigint | number;
    financialCategoryId!: bigint | number;
    taxCodeId!: bigint | number;
    sequence!: bigint | number;
    color!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    currency: CurrencyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    financialCategory: FinancialCategoryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    rateType: RateTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    taxCode: TaxCodeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _chargeTypeChangeHistories: ChargeTypeChangeHistoryData[] | null = null;
    private _chargeTypeChangeHistoriesPromise: Promise<ChargeTypeChangeHistoryData[]> | null  = null;
    private _chargeTypeChangeHistoriesSubject = new BehaviorSubject<ChargeTypeChangeHistoryData[] | null>(null);

                
    private _scheduledEventTemplateCharges: ScheduledEventTemplateChargeData[] | null = null;
    private _scheduledEventTemplateChargesPromise: Promise<ScheduledEventTemplateChargeData[]> | null  = null;
    private _scheduledEventTemplateChargesSubject = new BehaviorSubject<ScheduledEventTemplateChargeData[] | null>(null);

                
    private _eventCharges: EventChargeData[] | null = null;
    private _eventChargesPromise: Promise<EventChargeData[]> | null  = null;
    private _eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);

                
    private _eventResourceAssignments: EventResourceAssignmentData[] | null = null;
    private _eventResourceAssignmentsPromise: Promise<EventResourceAssignmentData[]> | null  = null;
    private _eventResourceAssignmentsSubject = new BehaviorSubject<EventResourceAssignmentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ChargeTypeData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ChargeTypeData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ChargeTypeData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ChargeTypeChangeHistories$ = this._chargeTypeChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._chargeTypeChangeHistories === null && this._chargeTypeChangeHistoriesPromise === null) {
            this.loadChargeTypeChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _chargeTypeChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ChargeTypeChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._chargeTypeChangeHistoriesCount$ === null) {
            this._chargeTypeChangeHistoriesCount$ = ChargeTypeChangeHistoryService.Instance.GetChargeTypeChangeHistoriesRowCount({chargeTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._chargeTypeChangeHistoriesCount$;
    }



    public ScheduledEventTemplateCharges$ = this._scheduledEventTemplateChargesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventTemplateCharges === null && this._scheduledEventTemplateChargesPromise === null) {
            this.loadScheduledEventTemplateCharges(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _scheduledEventTemplateChargesCount$: Observable<bigint | number> | null = null;
    public get ScheduledEventTemplateChargesCount$(): Observable<bigint | number> {
        if (this._scheduledEventTemplateChargesCount$ === null) {
            this._scheduledEventTemplateChargesCount$ = ScheduledEventTemplateChargeService.Instance.GetScheduledEventTemplateChargesRowCount({chargeTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._scheduledEventTemplateChargesCount$;
    }



    public EventCharges$ = this._eventChargesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventCharges === null && this._eventChargesPromise === null) {
            this.loadEventCharges(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _eventChargesCount$: Observable<bigint | number> | null = null;
    public get EventChargesCount$(): Observable<bigint | number> {
        if (this._eventChargesCount$ === null) {
            this._eventChargesCount$ = EventChargeService.Instance.GetEventChargesRowCount({chargeTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._eventChargesCount$;
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
            this._eventResourceAssignmentsCount$ = EventResourceAssignmentService.Instance.GetEventResourceAssignmentsRowCount({chargeTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._eventResourceAssignmentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ChargeTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.chargeType.Reload();
  //
  //  Non Async:
  //
  //     chargeType[0].Reload().then(x => {
  //        this.chargeType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ChargeTypeService.Instance.GetChargeType(this.id, includeRelations)
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
     this._chargeTypeChangeHistories = null;
     this._chargeTypeChangeHistoriesPromise = null;
     this._chargeTypeChangeHistoriesSubject.next(null);
     this._chargeTypeChangeHistoriesCount$ = null;

     this._scheduledEventTemplateCharges = null;
     this._scheduledEventTemplateChargesPromise = null;
     this._scheduledEventTemplateChargesSubject.next(null);
     this._scheduledEventTemplateChargesCount$ = null;

     this._eventCharges = null;
     this._eventChargesPromise = null;
     this._eventChargesSubject.next(null);
     this._eventChargesCount$ = null;

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
     * Gets the ChargeTypeChangeHistories for this ChargeType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.chargeType.ChargeTypeChangeHistories.then(chargeTypes => { ... })
     *   or
     *   await this.chargeType.chargeTypes
     *
    */
    public get ChargeTypeChangeHistories(): Promise<ChargeTypeChangeHistoryData[]> {
        if (this._chargeTypeChangeHistories !== null) {
            return Promise.resolve(this._chargeTypeChangeHistories);
        }

        if (this._chargeTypeChangeHistoriesPromise !== null) {
            return this._chargeTypeChangeHistoriesPromise;
        }

        // Start the load
        this.loadChargeTypeChangeHistories();

        return this._chargeTypeChangeHistoriesPromise!;
    }



    private loadChargeTypeChangeHistories(): void {

        this._chargeTypeChangeHistoriesPromise = lastValueFrom(
            ChargeTypeService.Instance.GetChargeTypeChangeHistoriesForChargeType(this.id)
        )
        .then(ChargeTypeChangeHistories => {
            this._chargeTypeChangeHistories = ChargeTypeChangeHistories ?? [];
            this._chargeTypeChangeHistoriesSubject.next(this._chargeTypeChangeHistories);
            return this._chargeTypeChangeHistories;
         })
        .catch(err => {
            this._chargeTypeChangeHistories = [];
            this._chargeTypeChangeHistoriesSubject.next(this._chargeTypeChangeHistories);
            throw err;
        })
        .finally(() => {
            this._chargeTypeChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ChargeTypeChangeHistory. Call after mutations to force refresh.
     */
    public ClearChargeTypeChangeHistoriesCache(): void {
        this._chargeTypeChangeHistories = null;
        this._chargeTypeChangeHistoriesPromise = null;
        this._chargeTypeChangeHistoriesSubject.next(this._chargeTypeChangeHistories);      // Emit to observable
    }

    public get HasChargeTypeChangeHistories(): Promise<boolean> {
        return this.ChargeTypeChangeHistories.then(chargeTypeChangeHistories => chargeTypeChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ScheduledEventTemplateCharges for this ChargeType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.chargeType.ScheduledEventTemplateCharges.then(chargeTypes => { ... })
     *   or
     *   await this.chargeType.chargeTypes
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
            ChargeTypeService.Instance.GetScheduledEventTemplateChargesForChargeType(this.id)
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
     * Gets the EventCharges for this ChargeType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.chargeType.EventCharges.then(chargeTypes => { ... })
     *   or
     *   await this.chargeType.chargeTypes
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
            ChargeTypeService.Instance.GetEventChargesForChargeType(this.id)
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
     * Gets the EventResourceAssignments for this ChargeType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.chargeType.EventResourceAssignments.then(chargeTypes => { ... })
     *   or
     *   await this.chargeType.chargeTypes
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
            ChargeTypeService.Instance.GetEventResourceAssignmentsForChargeType(this.id)
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
    //   Template: {{ (chargeType.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await chargeType.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ChargeTypeData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ChargeTypeData>> {
        const info = await lastValueFrom(
            ChargeTypeService.Instance.GetChargeTypeChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ChargeTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ChargeTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ChargeTypeSubmitData {
        return ChargeTypeService.Instance.ConvertToChargeTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ChargeTypeService extends SecureEndpointBase {

    private static _instance: ChargeTypeService;
    private listCache: Map<string, Observable<Array<ChargeTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ChargeTypeBasicListData>>>;
    private recordCache: Map<string, Observable<ChargeTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private chargeTypeChangeHistoryService: ChargeTypeChangeHistoryService,
        private scheduledEventTemplateChargeService: ScheduledEventTemplateChargeService,
        private eventChargeService: EventChargeService,
        private eventResourceAssignmentService: EventResourceAssignmentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ChargeTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ChargeTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<ChargeTypeData>>();

        ChargeTypeService._instance = this;
    }

    public static get Instance(): ChargeTypeService {
      return ChargeTypeService._instance;
    }


    public ClearListCaches(config: ChargeTypeQueryParameters | null = null) {

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


    public ConvertToChargeTypeSubmitData(data: ChargeTypeData): ChargeTypeSubmitData {

        let output = new ChargeTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.externalId = data.externalId;
        output.isRevenue = data.isRevenue;
        output.isTaxable = data.isTaxable;
        output.defaultAmount = data.defaultAmount;
        output.defaultDescription = data.defaultDescription;
        output.rateTypeId = data.rateTypeId;
        output.currencyId = data.currencyId;
        output.financialCategoryId = data.financialCategoryId;
        output.taxCodeId = data.taxCodeId;
        output.sequence = data.sequence;
        output.color = data.color;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetChargeType(id: bigint | number, includeRelations: boolean = true) : Observable<ChargeTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const chargeType$ = this.requestChargeType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ChargeType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, chargeType$);

            return chargeType$;
        }

        return this.recordCache.get(configHash) as Observable<ChargeTypeData>;
    }

    private requestChargeType(id: bigint | number, includeRelations: boolean = true) : Observable<ChargeTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ChargeTypeData>(this.baseUrl + 'api/ChargeType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveChargeType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestChargeType(id, includeRelations));
            }));
    }

    public GetChargeTypeList(config: ChargeTypeQueryParameters | any = null) : Observable<Array<ChargeTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const chargeTypeList$ = this.requestChargeTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ChargeType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, chargeTypeList$);

            return chargeTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ChargeTypeData>>;
    }


    private requestChargeTypeList(config: ChargeTypeQueryParameters | any) : Observable <Array<ChargeTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ChargeTypeData>>(this.baseUrl + 'api/ChargeTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveChargeTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestChargeTypeList(config));
            }));
    }

    public GetChargeTypesRowCount(config: ChargeTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const chargeTypesRowCount$ = this.requestChargeTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ChargeTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, chargeTypesRowCount$);

            return chargeTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestChargeTypesRowCount(config: ChargeTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ChargeTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestChargeTypesRowCount(config));
            }));
    }

    public GetChargeTypesBasicListData(config: ChargeTypeQueryParameters | any = null) : Observable<Array<ChargeTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const chargeTypesBasicListData$ = this.requestChargeTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ChargeTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, chargeTypesBasicListData$);

            return chargeTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ChargeTypeBasicListData>>;
    }


    private requestChargeTypesBasicListData(config: ChargeTypeQueryParameters | any) : Observable<Array<ChargeTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ChargeTypeBasicListData>>(this.baseUrl + 'api/ChargeTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestChargeTypesBasicListData(config));
            }));

    }


    public PutChargeType(id: bigint | number, chargeType: ChargeTypeSubmitData) : Observable<ChargeTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ChargeTypeData>(this.baseUrl + 'api/ChargeType/' + id.toString(), chargeType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveChargeType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutChargeType(id, chargeType));
            }));
    }


    public PostChargeType(chargeType: ChargeTypeSubmitData) : Observable<ChargeTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ChargeTypeData>(this.baseUrl + 'api/ChargeType', chargeType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveChargeType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostChargeType(chargeType));
            }));
    }

  
    public DeleteChargeType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ChargeType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteChargeType(id));
            }));
    }

    public RollbackChargeType(id: bigint | number, versionNumber: bigint | number) : Observable<ChargeTypeData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ChargeTypeData>(this.baseUrl + 'api/ChargeType/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveChargeType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackChargeType(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ChargeType.
     */
    public GetChargeTypeChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ChargeTypeData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ChargeTypeData>>(this.baseUrl + 'api/ChargeType/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetChargeTypeChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ChargeType.
     */
    public GetChargeTypeAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ChargeTypeData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ChargeTypeData>[]>(this.baseUrl + 'api/ChargeType/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetChargeTypeAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ChargeType.
     */
    public GetChargeTypeVersion(id: bigint | number, version: number): Observable<ChargeTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ChargeTypeData>(this.baseUrl + 'api/ChargeType/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveChargeType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetChargeTypeVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ChargeType at a specific point in time.
     */
    public GetChargeTypeStateAtTime(id: bigint | number, time: string): Observable<ChargeTypeData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ChargeTypeData>(this.baseUrl + 'api/ChargeType/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveChargeType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetChargeTypeStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ChargeTypeQueryParameters | any): string {

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

    public userIsSchedulerChargeTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerChargeTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ChargeTypes
        //
        if (userIsSchedulerChargeTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerChargeTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerChargeTypeReader = false;
            }
        }

        return userIsSchedulerChargeTypeReader;
    }


    public userIsSchedulerChargeTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerChargeTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ChargeTypes
        //
        if (userIsSchedulerChargeTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerChargeTypeWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerChargeTypeWriter = false;
          }      
        }

        return userIsSchedulerChargeTypeWriter;
    }

    public GetChargeTypeChangeHistoriesForChargeType(chargeTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ChargeTypeChangeHistoryData[]> {
        return this.chargeTypeChangeHistoryService.GetChargeTypeChangeHistoryList({
            chargeTypeId: chargeTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventTemplateChargesForChargeType(chargeTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventTemplateChargeData[]> {
        return this.scheduledEventTemplateChargeService.GetScheduledEventTemplateChargeList({
            chargeTypeId: chargeTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventChargesForChargeType(chargeTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventChargeData[]> {
        return this.eventChargeService.GetEventChargeList({
            chargeTypeId: chargeTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventResourceAssignmentsForChargeType(chargeTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventResourceAssignmentData[]> {
        return this.eventResourceAssignmentService.GetEventResourceAssignmentList({
            chargeTypeId: chargeTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ChargeTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ChargeTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ChargeTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveChargeType(raw: any): ChargeTypeData {
    if (!raw) return raw;

    //
    // Create a ChargeTypeData object instance with correct prototype
    //
    const revived = Object.create(ChargeTypeData.prototype) as ChargeTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._chargeTypeChangeHistories = null;
    (revived as any)._chargeTypeChangeHistoriesPromise = null;
    (revived as any)._chargeTypeChangeHistoriesSubject = new BehaviorSubject<ChargeTypeChangeHistoryData[] | null>(null);

    (revived as any)._scheduledEventTemplateCharges = null;
    (revived as any)._scheduledEventTemplateChargesPromise = null;
    (revived as any)._scheduledEventTemplateChargesSubject = new BehaviorSubject<ScheduledEventTemplateChargeData[] | null>(null);

    (revived as any)._eventCharges = null;
    (revived as any)._eventChargesPromise = null;
    (revived as any)._eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);

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
    // 2. But private methods (loadChargeTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ChargeTypeChangeHistories$ = (revived as any)._chargeTypeChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._chargeTypeChangeHistories === null && (revived as any)._chargeTypeChangeHistoriesPromise === null) {
                (revived as any).loadChargeTypeChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._chargeTypeChangeHistoriesCount$ = null;


    (revived as any).ScheduledEventTemplateCharges$ = (revived as any)._scheduledEventTemplateChargesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventTemplateCharges === null && (revived as any)._scheduledEventTemplateChargesPromise === null) {
                (revived as any).loadScheduledEventTemplateCharges();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._scheduledEventTemplateChargesCount$ = null;


    (revived as any).EventCharges$ = (revived as any)._eventChargesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventCharges === null && (revived as any)._eventChargesPromise === null) {
                (revived as any).loadEventCharges();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._eventChargesCount$ = null;


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
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ChargeTypeData> | null>(null);

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

  private ReviveChargeTypeList(rawList: any[]): ChargeTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveChargeType(raw));
  }

}
