/*

   GENERATED SERVICE FOR THE CONSTITUENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Constituent table.

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
import { ContactData } from './contact.service';
import { ClientData } from './client.service';
import { HouseholdData } from './household.service';
import { ConstituentJourneyStageData } from './constituent-journey-stage.service';
import { IconData } from './icon.service';
import { ConstituentChangeHistoryService, ConstituentChangeHistoryData } from './constituent-change-history.service';
import { PledgeService, PledgeData } from './pledge.service';
import { TributeService, TributeData } from './tribute.service';
import { GiftService, GiftData } from './gift.service';
import { SoftCreditService, SoftCreditData } from './soft-credit.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ConstituentQueryParameters {
    contactId: bigint | number | null | undefined = null;
    clientId: bigint | number | null | undefined = null;
    householdId: bigint | number | null | undefined = null;
    constituentNumber: string | null | undefined = null;
    doNotSolicit: boolean | null | undefined = null;
    doNotEmail: boolean | null | undefined = null;
    doNotMail: boolean | null | undefined = null;
    totalLifetimeGiving: number | null | undefined = null;
    totalYTDGiving: number | null | undefined = null;
    lastGiftDate: string | null | undefined = null;        // ISO 8601
    lastGiftAmount: number | null | undefined = null;
    largestGiftAmount: number | null | undefined = null;
    totalGiftCount: bigint | number | null | undefined = null;
    externalId: string | null | undefined = null;
    notes: string | null | undefined = null;
    constituentJourneyStageId: bigint | number | null | undefined = null;
    dateEnteredCurrentStage: string | null | undefined = null;        // ISO 8601
    attributes: string | null | undefined = null;
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
export class ConstituentSubmitData {
    id!: bigint | number;
    contactId: bigint | number | null = null;
    clientId: bigint | number | null = null;
    householdId: bigint | number | null = null;
    constituentNumber!: string;
    doNotSolicit!: boolean;
    doNotEmail!: boolean;
    doNotMail!: boolean;
    totalLifetimeGiving!: number;
    totalYTDGiving!: number;
    lastGiftDate: string | null = null;     // ISO 8601
    lastGiftAmount: number | null = null;
    largestGiftAmount: number | null = null;
    totalGiftCount: bigint | number | null = null;
    externalId: string | null = null;
    notes: string | null = null;
    constituentJourneyStageId: bigint | number | null = null;
    dateEnteredCurrentStage: string | null = null;     // ISO 8601
    attributes: string | null = null;
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


export class ConstituentBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ConstituentChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `constituent.ConstituentChildren$` — use with `| async` in templates
//        • Promise:    `constituent.ConstituentChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="constituent.ConstituentChildren$ | async"`), or
//        • Access the promise getter (`constituent.ConstituentChildren` or `await constituent.ConstituentChildren`)
//    - Simply reading `constituent.ConstituentChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await constituent.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ConstituentData {
    id!: bigint | number;
    contactId!: bigint | number;
    clientId!: bigint | number;
    householdId!: bigint | number;
    constituentNumber!: string;
    doNotSolicit!: boolean;
    doNotEmail!: boolean;
    doNotMail!: boolean;
    totalLifetimeGiving!: number;
    totalYTDGiving!: number;
    lastGiftDate!: string | null;   // ISO 8601
    lastGiftAmount!: number | null;
    largestGiftAmount!: number | null;
    totalGiftCount!: bigint | number;
    externalId!: string | null;
    notes!: string | null;
    constituentJourneyStageId!: bigint | number;
    dateEnteredCurrentStage!: string | null;   // ISO 8601
    attributes!: string | null;
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
    client: ClientData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    constituentJourneyStage: ConstituentJourneyStageData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    household: HouseholdData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _constituentChangeHistories: ConstituentChangeHistoryData[] | null = null;
    private _constituentChangeHistoriesPromise: Promise<ConstituentChangeHistoryData[]> | null  = null;
    private _constituentChangeHistoriesSubject = new BehaviorSubject<ConstituentChangeHistoryData[] | null>(null);

    private _pledges: PledgeData[] | null = null;
    private _pledgesPromise: Promise<PledgeData[]> | null  = null;
    private _pledgesSubject = new BehaviorSubject<PledgeData[] | null>(null);

    private _defaultAcknowledgees: TributeData[] | null = null;
    private _defaultAcknowledgeesPromise: Promise<TributeData[]> | null  = null;
    private _defaultAcknowledgeesSubject = new BehaviorSubject<TributeData[] | null>(null);

    private _gifts: GiftData[] | null = null;
    private _giftsPromise: Promise<GiftData[]> | null  = null;
    private _giftsSubject = new BehaviorSubject<GiftData[] | null>(null);

    private _softCredits: SoftCreditData[] | null = null;
    private _softCreditsPromise: Promise<SoftCreditData[]> | null  = null;
    private _softCreditsSubject = new BehaviorSubject<SoftCreditData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ConstituentChangeHistories$ = this._constituentChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._constituentChangeHistories === null && this._constituentChangeHistoriesPromise === null) {
            this.loadConstituentChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ConstituentChangeHistoriesCount$ = ConstituentService.Instance.GetConstituentsRowCount({constituentId: this.id,
      active: true,
      deleted: false
    });



    public Pledges$ = this._pledgesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._pledges === null && this._pledgesPromise === null) {
            this.loadPledges(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public PledgesCount$ = ConstituentService.Instance.GetConstituentsRowCount({constituentId: this.id,
      active: true,
      deleted: false
    });



    public DefaultAcknowledgees$ = this._defaultAcknowledgeesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._defaultAcknowledgees === null && this._defaultAcknowledgeesPromise === null) {
            this.loadDefaultAcknowledgees(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public DefaultAcknowledgeesCount$ = ConstituentService.Instance.GetConstituentsRowCount({constituentId: this.id,
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

  
    public GiftsCount$ = ConstituentService.Instance.GetConstituentsRowCount({constituentId: this.id,
      active: true,
      deleted: false
    });



    public SoftCredits$ = this._softCreditsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._softCredits === null && this._softCreditsPromise === null) {
            this.loadSoftCredits(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SoftCreditsCount$ = ConstituentService.Instance.GetConstituentsRowCount({constituentId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ConstituentData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.constituent.Reload();
  //
  //  Non Async:
  //
  //     constituent[0].Reload().then(x => {
  //        this.constituent = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ConstituentService.Instance.GetConstituent(this.id, includeRelations)
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
     this._constituentChangeHistories = null;
     this._constituentChangeHistoriesPromise = null;
     this._constituentChangeHistoriesSubject.next(null);

     this._pledges = null;
     this._pledgesPromise = null;
     this._pledgesSubject.next(null);

     this._defaultAcknowledgees = null;
     this._defaultAcknowledgeesPromise = null;
     this._defaultAcknowledgeesSubject.next(null);

     this._gifts = null;
     this._giftsPromise = null;
     this._giftsSubject.next(null);

     this._softCredits = null;
     this._softCreditsPromise = null;
     this._softCreditsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ConstituentChangeHistories for this Constituent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.constituent.ConstituentChangeHistories.then(constituentChangeHistories => { ... })
     *   or
     *   await this.constituent.ConstituentChangeHistories
     *
    */
    public get ConstituentChangeHistories(): Promise<ConstituentChangeHistoryData[]> {
        if (this._constituentChangeHistories !== null) {
            return Promise.resolve(this._constituentChangeHistories);
        }

        if (this._constituentChangeHistoriesPromise !== null) {
            return this._constituentChangeHistoriesPromise;
        }

        // Start the load
        this.loadConstituentChangeHistories();

        return this._constituentChangeHistoriesPromise!;
    }



    private loadConstituentChangeHistories(): void {

        this._constituentChangeHistoriesPromise = lastValueFrom(
            ConstituentService.Instance.GetConstituentChangeHistoriesForConstituent(this.id)
        )
        .then(constituentChangeHistories => {
            this._constituentChangeHistories = constituentChangeHistories ?? [];
            this._constituentChangeHistoriesSubject.next(this._constituentChangeHistories);
            return this._constituentChangeHistories;
         })
        .catch(err => {
            this._constituentChangeHistories = [];
            this._constituentChangeHistoriesSubject.next(this._constituentChangeHistories);
            throw err;
        })
        .finally(() => {
            this._constituentChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConstituentChangeHistory. Call after mutations to force refresh.
     */
    public ClearConstituentChangeHistoriesCache(): void {
        this._constituentChangeHistories = null;
        this._constituentChangeHistoriesPromise = null;
        this._constituentChangeHistoriesSubject.next(this._constituentChangeHistories);      // Emit to observable
    }

    public get HasConstituentChangeHistories(): Promise<boolean> {
        return this.ConstituentChangeHistories.then(constituentChangeHistories => constituentChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Pledges for this Constituent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.constituent.Pledges.then(pledges => { ... })
     *   or
     *   await this.constituent.Pledges
     *
    */
    public get Pledges(): Promise<PledgeData[]> {
        if (this._pledges !== null) {
            return Promise.resolve(this._pledges);
        }

        if (this._pledgesPromise !== null) {
            return this._pledgesPromise;
        }

        // Start the load
        this.loadPledges();

        return this._pledgesPromise!;
    }



    private loadPledges(): void {

        this._pledgesPromise = lastValueFrom(
            ConstituentService.Instance.GetPledgesForConstituent(this.id)
        )
        .then(pledges => {
            this._pledges = pledges ?? [];
            this._pledgesSubject.next(this._pledges);
            return this._pledges;
         })
        .catch(err => {
            this._pledges = [];
            this._pledgesSubject.next(this._pledges);
            throw err;
        })
        .finally(() => {
            this._pledgesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Pledge. Call after mutations to force refresh.
     */
    public ClearPledgesCache(): void {
        this._pledges = null;
        this._pledgesPromise = null;
        this._pledgesSubject.next(this._pledges);      // Emit to observable
    }

    public get HasPledges(): Promise<boolean> {
        return this.Pledges.then(pledges => pledges.length > 0);
    }


    /**
     *
     * Gets the defaultAcknowledgees for this Constituent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.constituent.defaultAcknowledgees.then(defaultAcknowledgees => { ... })
     *   or
     *   await this.constituent.defaultAcknowledgees
     *
    */
    public get defaultAcknowledgees(): Promise<TributeData[]> {
        if (this._defaultAcknowledgees !== null) {
            return Promise.resolve(this._defaultAcknowledgees);
        }

        if (this._defaultAcknowledgeesPromise !== null) {
            return this._defaultAcknowledgeesPromise;
        }

        // Start the load
        this.loadDefaultAcknowledgees();

        return this._defaultAcknowledgeesPromise!;
    }



    private loadDefaultAcknowledgees(): void {

        this._defaultAcknowledgeesPromise = lastValueFrom(
            ConstituentService.Instance.GetDefaultAcknowledgeesForConstituent(this.id)
        )
        .then(defaultAcknowledgees => {
            this._defaultAcknowledgees = defaultAcknowledgees ?? [];
            this._defaultAcknowledgeesSubject.next(this._defaultAcknowledgees);
            return this._defaultAcknowledgees;
         })
        .catch(err => {
            this._defaultAcknowledgees = [];
            this._defaultAcknowledgeesSubject.next(this._defaultAcknowledgees);
            throw err;
        })
        .finally(() => {
            this._defaultAcknowledgeesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached defaultAcknowledgee. Call after mutations to force refresh.
     */
    public ClearDefaultAcknowledgeesCache(): void {
        this._defaultAcknowledgees = null;
        this._defaultAcknowledgeesPromise = null;
        this._defaultAcknowledgeesSubject.next(this._defaultAcknowledgees);      // Emit to observable
    }

    public get HasDefaultAcknowledgees(): Promise<boolean> {
        return this.defaultAcknowledgees.then(defaultAcknowledgees => defaultAcknowledgees.length > 0);
    }


    /**
     *
     * Gets the Gifts for this Constituent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.constituent.Gifts.then(gifts => { ... })
     *   or
     *   await this.constituent.Gifts
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
            ConstituentService.Instance.GetGiftsForConstituent(this.id)
        )
        .then(gifts => {
            this._gifts = gifts ?? [];
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
     * Gets the SoftCredits for this Constituent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.constituent.SoftCredits.then(softCredits => { ... })
     *   or
     *   await this.constituent.SoftCredits
     *
    */
    public get SoftCredits(): Promise<SoftCreditData[]> {
        if (this._softCredits !== null) {
            return Promise.resolve(this._softCredits);
        }

        if (this._softCreditsPromise !== null) {
            return this._softCreditsPromise;
        }

        // Start the load
        this.loadSoftCredits();

        return this._softCreditsPromise!;
    }



    private loadSoftCredits(): void {

        this._softCreditsPromise = lastValueFrom(
            ConstituentService.Instance.GetSoftCreditsForConstituent(this.id)
        )
        .then(softCredits => {
            this._softCredits = softCredits ?? [];
            this._softCreditsSubject.next(this._softCredits);
            return this._softCredits;
         })
        .catch(err => {
            this._softCredits = [];
            this._softCreditsSubject.next(this._softCredits);
            throw err;
        })
        .finally(() => {
            this._softCreditsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SoftCredit. Call after mutations to force refresh.
     */
    public ClearSoftCreditsCache(): void {
        this._softCredits = null;
        this._softCreditsPromise = null;
        this._softCreditsSubject.next(this._softCredits);      // Emit to observable
    }

    public get HasSoftCredits(): Promise<boolean> {
        return this.SoftCredits.then(softCredits => softCredits.length > 0);
    }




    /**
     * Updates the state of this ConstituentData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ConstituentData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ConstituentSubmitData {
        return ConstituentService.Instance.ConvertToConstituentSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ConstituentService extends SecureEndpointBase {

    private static _instance: ConstituentService;
    private listCache: Map<string, Observable<Array<ConstituentData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ConstituentBasicListData>>>;
    private recordCache: Map<string, Observable<ConstituentData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private constituentChangeHistoryService: ConstituentChangeHistoryService,
        private pledgeService: PledgeService,
        private tributeService: TributeService,
        private giftService: GiftService,
        private softCreditService: SoftCreditService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ConstituentData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ConstituentBasicListData>>>();
        this.recordCache = new Map<string, Observable<ConstituentData>>();

        ConstituentService._instance = this;
    }

    public static get Instance(): ConstituentService {
      return ConstituentService._instance;
    }


    public ClearListCaches(config: ConstituentQueryParameters | null = null) {

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


    public ConvertToConstituentSubmitData(data: ConstituentData): ConstituentSubmitData {

        let output = new ConstituentSubmitData();

        output.id = data.id;
        output.contactId = data.contactId;
        output.clientId = data.clientId;
        output.householdId = data.householdId;
        output.constituentNumber = data.constituentNumber;
        output.doNotSolicit = data.doNotSolicit;
        output.doNotEmail = data.doNotEmail;
        output.doNotMail = data.doNotMail;
        output.totalLifetimeGiving = data.totalLifetimeGiving;
        output.totalYTDGiving = data.totalYTDGiving;
        output.lastGiftDate = data.lastGiftDate;
        output.lastGiftAmount = data.lastGiftAmount;
        output.largestGiftAmount = data.largestGiftAmount;
        output.totalGiftCount = data.totalGiftCount;
        output.externalId = data.externalId;
        output.notes = data.notes;
        output.constituentJourneyStageId = data.constituentJourneyStageId;
        output.dateEnteredCurrentStage = data.dateEnteredCurrentStage;
        output.attributes = data.attributes;
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

    public GetConstituent(id: bigint | number, includeRelations: boolean = true) : Observable<ConstituentData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const constituent$ = this.requestConstituent(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Constituent", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, constituent$);

            return constituent$;
        }

        return this.recordCache.get(configHash) as Observable<ConstituentData>;
    }

    private requestConstituent(id: bigint | number, includeRelations: boolean = true) : Observable<ConstituentData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConstituentData>(this.baseUrl + 'api/Constituent/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveConstituent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestConstituent(id, includeRelations));
            }));
    }

    public GetConstituentList(config: ConstituentQueryParameters | any = null) : Observable<Array<ConstituentData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const constituentList$ = this.requestConstituentList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Constituent list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, constituentList$);

            return constituentList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ConstituentData>>;
    }


    private requestConstituentList(config: ConstituentQueryParameters | any) : Observable <Array<ConstituentData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConstituentData>>(this.baseUrl + 'api/Constituents', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveConstituentList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestConstituentList(config));
            }));
    }

    public GetConstituentsRowCount(config: ConstituentQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const constituentsRowCount$ = this.requestConstituentsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Constituents row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, constituentsRowCount$);

            return constituentsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestConstituentsRowCount(config: ConstituentQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Constituents/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConstituentsRowCount(config));
            }));
    }

    public GetConstituentsBasicListData(config: ConstituentQueryParameters | any = null) : Observable<Array<ConstituentBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const constituentsBasicListData$ = this.requestConstituentsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Constituents basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, constituentsBasicListData$);

            return constituentsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ConstituentBasicListData>>;
    }


    private requestConstituentsBasicListData(config: ConstituentQueryParameters | any) : Observable<Array<ConstituentBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConstituentBasicListData>>(this.baseUrl + 'api/Constituents/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConstituentsBasicListData(config));
            }));

    }


    public PutConstituent(id: bigint | number, constituent: ConstituentSubmitData) : Observable<ConstituentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConstituentData>(this.baseUrl + 'api/Constituent/' + id.toString(), constituent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConstituent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutConstituent(id, constituent));
            }));
    }


    public PostConstituent(constituent: ConstituentSubmitData) : Observable<ConstituentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ConstituentData>(this.baseUrl + 'api/Constituent', constituent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConstituent(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostConstituent(constituent));
            }));
    }

  
    public DeleteConstituent(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Constituent/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteConstituent(id));
            }));
    }

    public RollbackConstituent(id: bigint | number, versionNumber: bigint | number) : Observable<ConstituentData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConstituentData>(this.baseUrl + 'api/Constituent/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConstituent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackConstituent(id, versionNumber));
        }));
    }

    private getConfigHash(config: ConstituentQueryParameters | any): string {

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

    public userIsSchedulerConstituentReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerConstituentReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Constituents
        //
        if (userIsSchedulerConstituentReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerConstituentReader = user.readPermission >= 1;
            } else {
                userIsSchedulerConstituentReader = false;
            }
        }

        return userIsSchedulerConstituentReader;
    }


    public userIsSchedulerConstituentWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerConstituentWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Constituents
        //
        if (userIsSchedulerConstituentWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerConstituentWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerConstituentWriter = false;
          }      
        }

        return userIsSchedulerConstituentWriter;
    }

    public GetConstituentChangeHistoriesForConstituent(constituentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConstituentChangeHistoryData[]> {
        return this.constituentChangeHistoryService.GetConstituentChangeHistoryList({
            constituentId: constituentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPledgesForConstituent(constituentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PledgeData[]> {
        return this.pledgeService.GetPledgeList({
            constituentId: constituentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetDefaultAcknowledgeesForConstituent(constituentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TributeData[]> {
        return this.tributeService.GetTributeList({
            defaultAcknowledgeeId: constituentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetGiftsForConstituent(constituentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GiftData[]> {
        return this.giftService.GetGiftList({
            constituentId: constituentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSoftCreditsForConstituent(constituentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SoftCreditData[]> {
        return this.softCreditService.GetSoftCreditList({
            constituentId: constituentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ConstituentData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ConstituentData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ConstituentTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveConstituent(raw: any): ConstituentData {
    if (!raw) return raw;

    //
    // Create a ConstituentData object instance with correct prototype
    //
    const revived = Object.create(ConstituentData.prototype) as ConstituentData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._constituentChangeHistories = null;
    (revived as any)._constituentChangeHistoriesPromise = null;
    (revived as any)._constituentChangeHistoriesSubject = new BehaviorSubject<ConstituentChangeHistoryData[] | null>(null);

    (revived as any)._pledges = null;
    (revived as any)._pledgesPromise = null;
    (revived as any)._pledgesSubject = new BehaviorSubject<PledgeData[] | null>(null);

    (revived as any)._tributes = null;
    (revived as any)._tributesPromise = null;
    (revived as any)._tributesSubject = new BehaviorSubject<TributeData[] | null>(null);

    (revived as any)._gifts = null;
    (revived as any)._giftsPromise = null;
    (revived as any)._giftsSubject = new BehaviorSubject<GiftData[] | null>(null);

    (revived as any)._softCredits = null;
    (revived as any)._softCreditsPromise = null;
    (revived as any)._softCreditsSubject = new BehaviorSubject<SoftCreditData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadConstituentXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ConstituentChangeHistories$ = (revived as any)._constituentChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._constituentChangeHistories === null && (revived as any)._constituentChangeHistoriesPromise === null) {
                (revived as any).loadConstituentChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ConstituentChangeHistoriesCount$ = ConstituentChangeHistoryService.Instance.GetConstituentChangeHistoriesRowCount({constituentId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Pledges$ = (revived as any)._pledgesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._pledges === null && (revived as any)._pledgesPromise === null) {
                (revived as any).loadPledges();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).PledgesCount$ = PledgeService.Instance.GetPledgesRowCount({constituentId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Tributes$ = (revived as any)._tributesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._tributes === null && (revived as any)._tributesPromise === null) {
                (revived as any).loadTributes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).TributesCount$ = TributeService.Instance.GetTributesRowCount({constituentId: (revived as any).id,
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

    (revived as any).GiftsCount$ = GiftService.Instance.GetGiftsRowCount({constituentId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SoftCredits$ = (revived as any)._softCreditsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._softCredits === null && (revived as any)._softCreditsPromise === null) {
                (revived as any).loadSoftCredits();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SoftCreditsCount$ = SoftCreditService.Instance.GetSoftCreditsRowCount({constituentId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveConstituentList(rawList: any[]): ConstituentData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveConstituent(raw));
  }

}
