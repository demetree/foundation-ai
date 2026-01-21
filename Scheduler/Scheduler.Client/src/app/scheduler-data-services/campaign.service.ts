/*

   GENERATED SERVICE FOR THE CAMPAIGN TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Campaign table.

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
import { IconData } from './icon.service';
import { CampaignChangeHistoryService, CampaignChangeHistoryData } from './campaign-change-history.service';
import { AppealService, AppealData } from './appeal.service';
import { PledgeService, PledgeData } from './pledge.service';
import { BatchService, BatchData } from './batch.service';
import { GiftService, GiftData } from './gift.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class CampaignQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    startDate: string | null | undefined = null;        // ISO 8601
    endDate: string | null | undefined = null;        // ISO 8601
    fundRaisingGoal: number | null | undefined = null;
    notes: string | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
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
export class CampaignSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    startDate: string | null = null;     // ISO 8601
    endDate: string | null = null;     // ISO 8601
    fundRaisingGoal: number | null = null;
    notes: string | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
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

export class CampaignBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. CampaignChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `campaign.CampaignChildren$` — use with `| async` in templates
//        • Promise:    `campaign.CampaignChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="campaign.CampaignChildren$ | async"`), or
//        • Access the promise getter (`campaign.CampaignChildren` or `await campaign.CampaignChildren`)
//    - Simply reading `campaign.CampaignChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await campaign.Reload()` to refresh the entire object and clear all lazy caches.
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
export class CampaignData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    startDate!: string | null;   // ISO 8601
    endDate!: string | null;   // ISO 8601
    fundRaisingGoal!: number | null;
    notes!: string | null;
    iconId!: bigint | number;
    color!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _campaignChangeHistories: CampaignChangeHistoryData[] | null = null;
    private _campaignChangeHistoriesPromise: Promise<CampaignChangeHistoryData[]> | null  = null;
    private _campaignChangeHistoriesSubject = new BehaviorSubject<CampaignChangeHistoryData[] | null>(null);

                
    private _appeals: AppealData[] | null = null;
    private _appealsPromise: Promise<AppealData[]> | null  = null;
    private _appealsSubject = new BehaviorSubject<AppealData[] | null>(null);

                
    private _pledges: PledgeData[] | null = null;
    private _pledgesPromise: Promise<PledgeData[]> | null  = null;
    private _pledgesSubject = new BehaviorSubject<PledgeData[] | null>(null);

                
    private _batchDefaultCampaigns: BatchData[] | null = null;
    private _batchDefaultCampaignsPromise: Promise<BatchData[]> | null  = null;
    private _batchDefaultCampaignsSubject = new BehaviorSubject<BatchData[] | null>(null);
                    
    private _gifts: GiftData[] | null = null;
    private _giftsPromise: Promise<GiftData[]> | null  = null;
    private _giftsSubject = new BehaviorSubject<GiftData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<CampaignData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<CampaignData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<CampaignData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public CampaignChangeHistories$ = this._campaignChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._campaignChangeHistories === null && this._campaignChangeHistoriesPromise === null) {
            this.loadCampaignChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public CampaignChangeHistoriesCount$ = CampaignChangeHistoryService.Instance.GetCampaignChangeHistoriesRowCount({campaignId: this.id,
      active: true,
      deleted: false
    });



    public Appeals$ = this._appealsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._appeals === null && this._appealsPromise === null) {
            this.loadAppeals(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public AppealsCount$ = AppealService.Instance.GetAppealsRowCount({campaignId: this.id,
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

  
    public PledgesCount$ = PledgeService.Instance.GetPledgesRowCount({campaignId: this.id,
      active: true,
      deleted: false
    });



    public BatchDefaultCampaigns$ = this._batchDefaultCampaignsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._batchDefaultCampaigns === null && this._batchDefaultCampaignsPromise === null) {
            this.loadBatchDefaultCampaigns(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public BatchDefaultCampaignsCount$ = BatchService.Instance.GetBatchesRowCount({defaultCampaignId: this.id,
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

  
    public GiftsCount$ = GiftService.Instance.GetGiftsRowCount({campaignId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any CampaignData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.campaign.Reload();
  //
  //  Non Async:
  //
  //     campaign[0].Reload().then(x => {
  //        this.campaign = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      CampaignService.Instance.GetCampaign(this.id, includeRelations)
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
     this._campaignChangeHistories = null;
     this._campaignChangeHistoriesPromise = null;
     this._campaignChangeHistoriesSubject.next(null);

     this._appeals = null;
     this._appealsPromise = null;
     this._appealsSubject.next(null);

     this._pledges = null;
     this._pledgesPromise = null;
     this._pledgesSubject.next(null);

     this._batchDefaultCampaigns = null;
     this._batchDefaultCampaignsPromise = null;
     this._batchDefaultCampaignsSubject.next(null);

     this._gifts = null;
     this._giftsPromise = null;
     this._giftsSubject.next(null);

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
     * Gets the CampaignChangeHistories for this Campaign.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.campaign.CampaignChangeHistories.then(campaigns => { ... })
     *   or
     *   await this.campaign.campaigns
     *
    */
    public get CampaignChangeHistories(): Promise<CampaignChangeHistoryData[]> {
        if (this._campaignChangeHistories !== null) {
            return Promise.resolve(this._campaignChangeHistories);
        }

        if (this._campaignChangeHistoriesPromise !== null) {
            return this._campaignChangeHistoriesPromise;
        }

        // Start the load
        this.loadCampaignChangeHistories();

        return this._campaignChangeHistoriesPromise!;
    }



    private loadCampaignChangeHistories(): void {

        this._campaignChangeHistoriesPromise = lastValueFrom(
            CampaignService.Instance.GetCampaignChangeHistoriesForCampaign(this.id)
        )
        .then(CampaignChangeHistories => {
            this._campaignChangeHistories = CampaignChangeHistories ?? [];
            this._campaignChangeHistoriesSubject.next(this._campaignChangeHistories);
            return this._campaignChangeHistories;
         })
        .catch(err => {
            this._campaignChangeHistories = [];
            this._campaignChangeHistoriesSubject.next(this._campaignChangeHistories);
            throw err;
        })
        .finally(() => {
            this._campaignChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached CampaignChangeHistory. Call after mutations to force refresh.
     */
    public ClearCampaignChangeHistoriesCache(): void {
        this._campaignChangeHistories = null;
        this._campaignChangeHistoriesPromise = null;
        this._campaignChangeHistoriesSubject.next(this._campaignChangeHistories);      // Emit to observable
    }

    public get HasCampaignChangeHistories(): Promise<boolean> {
        return this.CampaignChangeHistories.then(campaignChangeHistories => campaignChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Appeals for this Campaign.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.campaign.Appeals.then(campaigns => { ... })
     *   or
     *   await this.campaign.campaigns
     *
    */
    public get Appeals(): Promise<AppealData[]> {
        if (this._appeals !== null) {
            return Promise.resolve(this._appeals);
        }

        if (this._appealsPromise !== null) {
            return this._appealsPromise;
        }

        // Start the load
        this.loadAppeals();

        return this._appealsPromise!;
    }



    private loadAppeals(): void {

        this._appealsPromise = lastValueFrom(
            CampaignService.Instance.GetAppealsForCampaign(this.id)
        )
        .then(Appeals => {
            this._appeals = Appeals ?? [];
            this._appealsSubject.next(this._appeals);
            return this._appeals;
         })
        .catch(err => {
            this._appeals = [];
            this._appealsSubject.next(this._appeals);
            throw err;
        })
        .finally(() => {
            this._appealsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Appeal. Call after mutations to force refresh.
     */
    public ClearAppealsCache(): void {
        this._appeals = null;
        this._appealsPromise = null;
        this._appealsSubject.next(this._appeals);      // Emit to observable
    }

    public get HasAppeals(): Promise<boolean> {
        return this.Appeals.then(appeals => appeals.length > 0);
    }


    /**
     *
     * Gets the Pledges for this Campaign.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.campaign.Pledges.then(campaigns => { ... })
     *   or
     *   await this.campaign.campaigns
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
            CampaignService.Instance.GetPledgesForCampaign(this.id)
        )
        .then(Pledges => {
            this._pledges = Pledges ?? [];
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
     * Gets the BatchDefaultCampaigns for this Campaign.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.campaign.BatchDefaultCampaigns.then(defaultCampaigns => { ... })
     *   or
     *   await this.campaign.defaultCampaigns
     *
    */
    public get BatchDefaultCampaigns(): Promise<BatchData[]> {
        if (this._batchDefaultCampaigns !== null) {
            return Promise.resolve(this._batchDefaultCampaigns);
        }

        if (this._batchDefaultCampaignsPromise !== null) {
            return this._batchDefaultCampaignsPromise;
        }

        // Start the load
        this.loadBatchDefaultCampaigns();

        return this._batchDefaultCampaignsPromise!;
    }



    private loadBatchDefaultCampaigns(): void {

        this._batchDefaultCampaignsPromise = lastValueFrom(
            CampaignService.Instance.GetBatchDefaultCampaignsForCampaign(this.id)
        )
        .then(BatchDefaultCampaigns => {
            this._batchDefaultCampaigns = BatchDefaultCampaigns ?? [];
            this._batchDefaultCampaignsSubject.next(this._batchDefaultCampaigns);
            return this._batchDefaultCampaigns;
         })
        .catch(err => {
            this._batchDefaultCampaigns = [];
            this._batchDefaultCampaignsSubject.next(this._batchDefaultCampaigns);
            throw err;
        })
        .finally(() => {
            this._batchDefaultCampaignsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BatchDefaultCampaign. Call after mutations to force refresh.
     */
    public ClearBatchDefaultCampaignsCache(): void {
        this._batchDefaultCampaigns = null;
        this._batchDefaultCampaignsPromise = null;
        this._batchDefaultCampaignsSubject.next(this._batchDefaultCampaigns);      // Emit to observable
    }

    public get HasBatchDefaultCampaigns(): Promise<boolean> {
        return this.BatchDefaultCampaigns.then(batchDefaultCampaigns => batchDefaultCampaigns.length > 0);
    }


    /**
     *
     * Gets the Gifts for this Campaign.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.campaign.Gifts.then(campaigns => { ... })
     *   or
     *   await this.campaign.campaigns
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
            CampaignService.Instance.GetGiftsForCampaign(this.id)
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




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (campaign.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await campaign.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<CampaignData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<CampaignData>> {
        const info = await lastValueFrom(
            CampaignService.Instance.GetCampaignChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this CampaignData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this CampaignData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): CampaignSubmitData {
        return CampaignService.Instance.ConvertToCampaignSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class CampaignService extends SecureEndpointBase {

    private static _instance: CampaignService;
    private listCache: Map<string, Observable<Array<CampaignData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<CampaignBasicListData>>>;
    private recordCache: Map<string, Observable<CampaignData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private campaignChangeHistoryService: CampaignChangeHistoryService,
        private appealService: AppealService,
        private pledgeService: PledgeService,
        private batchService: BatchService,
        private giftService: GiftService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<CampaignData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<CampaignBasicListData>>>();
        this.recordCache = new Map<string, Observable<CampaignData>>();

        CampaignService._instance = this;
    }

    public static get Instance(): CampaignService {
      return CampaignService._instance;
    }


    public ClearListCaches(config: CampaignQueryParameters | null = null) {

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


    public ConvertToCampaignSubmitData(data: CampaignData): CampaignSubmitData {

        let output = new CampaignSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.startDate = data.startDate;
        output.endDate = data.endDate;
        output.fundRaisingGoal = data.fundRaisingGoal;
        output.notes = data.notes;
        output.iconId = data.iconId;
        output.color = data.color;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetCampaign(id: bigint | number, includeRelations: boolean = true) : Observable<CampaignData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const campaign$ = this.requestCampaign(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Campaign", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, campaign$);

            return campaign$;
        }

        return this.recordCache.get(configHash) as Observable<CampaignData>;
    }

    private requestCampaign(id: bigint | number, includeRelations: boolean = true) : Observable<CampaignData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CampaignData>(this.baseUrl + 'api/Campaign/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveCampaign(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestCampaign(id, includeRelations));
            }));
    }

    public GetCampaignList(config: CampaignQueryParameters | any = null) : Observable<Array<CampaignData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const campaignList$ = this.requestCampaignList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Campaign list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, campaignList$);

            return campaignList$;
        }

        return this.listCache.get(configHash) as Observable<Array<CampaignData>>;
    }


    private requestCampaignList(config: CampaignQueryParameters | any) : Observable <Array<CampaignData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CampaignData>>(this.baseUrl + 'api/Campaigns', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveCampaignList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestCampaignList(config));
            }));
    }

    public GetCampaignsRowCount(config: CampaignQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const campaignsRowCount$ = this.requestCampaignsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Campaigns row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, campaignsRowCount$);

            return campaignsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestCampaignsRowCount(config: CampaignQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Campaigns/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCampaignsRowCount(config));
            }));
    }

    public GetCampaignsBasicListData(config: CampaignQueryParameters | any = null) : Observable<Array<CampaignBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const campaignsBasicListData$ = this.requestCampaignsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Campaigns basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, campaignsBasicListData$);

            return campaignsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<CampaignBasicListData>>;
    }


    private requestCampaignsBasicListData(config: CampaignQueryParameters | any) : Observable<Array<CampaignBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CampaignBasicListData>>(this.baseUrl + 'api/Campaigns/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCampaignsBasicListData(config));
            }));

    }


    public PutCampaign(id: bigint | number, campaign: CampaignSubmitData) : Observable<CampaignData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CampaignData>(this.baseUrl + 'api/Campaign/' + id.toString(), campaign, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCampaign(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutCampaign(id, campaign));
            }));
    }


    public PostCampaign(campaign: CampaignSubmitData) : Observable<CampaignData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<CampaignData>(this.baseUrl + 'api/Campaign', campaign, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCampaign(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostCampaign(campaign));
            }));
    }

  
    public DeleteCampaign(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Campaign/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteCampaign(id));
            }));
    }

    public RollbackCampaign(id: bigint | number, versionNumber: bigint | number) : Observable<CampaignData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CampaignData>(this.baseUrl + 'api/Campaign/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCampaign(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackCampaign(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Campaign.
     */
    public GetCampaignChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<CampaignData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<CampaignData>>(this.baseUrl + 'api/Campaign/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetCampaignChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Campaign.
     */
    public GetCampaignAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<CampaignData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<CampaignData>[]>(this.baseUrl + 'api/Campaign/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetCampaignAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Campaign.
     */
    public GetCampaignVersion(id: bigint | number, version: number): Observable<CampaignData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CampaignData>(this.baseUrl + 'api/Campaign/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveCampaign(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetCampaignVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Campaign at a specific point in time.
     */
    public GetCampaignStateAtTime(id: bigint | number, time: string): Observable<CampaignData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CampaignData>(this.baseUrl + 'api/Campaign/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveCampaign(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetCampaignStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: CampaignQueryParameters | any): string {

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

    public userIsSchedulerCampaignReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerCampaignReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Campaigns
        //
        if (userIsSchedulerCampaignReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerCampaignReader = user.readPermission >= 1;
            } else {
                userIsSchedulerCampaignReader = false;
            }
        }

        return userIsSchedulerCampaignReader;
    }


    public userIsSchedulerCampaignWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerCampaignWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Campaigns
        //
        if (userIsSchedulerCampaignWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerCampaignWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerCampaignWriter = false;
          }      
        }

        return userIsSchedulerCampaignWriter;
    }

    public GetCampaignChangeHistoriesForCampaign(campaignId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CampaignChangeHistoryData[]> {
        return this.campaignChangeHistoryService.GetCampaignChangeHistoryList({
            campaignId: campaignId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetAppealsForCampaign(campaignId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AppealData[]> {
        return this.appealService.GetAppealList({
            campaignId: campaignId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPledgesForCampaign(campaignId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PledgeData[]> {
        return this.pledgeService.GetPledgeList({
            campaignId: campaignId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBatchDefaultCampaignsForCampaign(campaignId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BatchData[]> {
        return this.batchService.GetBatchList({
            defaultCampaignId: campaignId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetGiftsForCampaign(campaignId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GiftData[]> {
        return this.giftService.GetGiftList({
            campaignId: campaignId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full CampaignData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the CampaignData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when CampaignTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveCampaign(raw: any): CampaignData {
    if (!raw) return raw;

    //
    // Create a CampaignData object instance with correct prototype
    //
    const revived = Object.create(CampaignData.prototype) as CampaignData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._campaignChangeHistories = null;
    (revived as any)._campaignChangeHistoriesPromise = null;
    (revived as any)._campaignChangeHistoriesSubject = new BehaviorSubject<CampaignChangeHistoryData[] | null>(null);

    (revived as any)._appeals = null;
    (revived as any)._appealsPromise = null;
    (revived as any)._appealsSubject = new BehaviorSubject<AppealData[] | null>(null);

    (revived as any)._pledges = null;
    (revived as any)._pledgesPromise = null;
    (revived as any)._pledgesSubject = new BehaviorSubject<PledgeData[] | null>(null);

    (revived as any)._batches = null;
    (revived as any)._batchesPromise = null;
    (revived as any)._batchesSubject = new BehaviorSubject<BatchData[] | null>(null);

    (revived as any)._gifts = null;
    (revived as any)._giftsPromise = null;
    (revived as any)._giftsSubject = new BehaviorSubject<GiftData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadCampaignXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).CampaignChangeHistories$ = (revived as any)._campaignChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._campaignChangeHistories === null && (revived as any)._campaignChangeHistoriesPromise === null) {
                (revived as any).loadCampaignChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).CampaignChangeHistoriesCount$ = CampaignChangeHistoryService.Instance.GetCampaignChangeHistoriesRowCount({campaignId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Appeals$ = (revived as any)._appealsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._appeals === null && (revived as any)._appealsPromise === null) {
                (revived as any).loadAppeals();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).AppealsCount$ = AppealService.Instance.GetAppealsRowCount({campaignId: (revived as any).id,
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

    (revived as any).PledgesCount$ = PledgeService.Instance.GetPledgesRowCount({campaignId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Batches$ = (revived as any)._batchesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._batches === null && (revived as any)._batchesPromise === null) {
                (revived as any).loadBatches();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).BatchesCount$ = BatchService.Instance.GetBatchesRowCount({campaignId: (revived as any).id,
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

    (revived as any).GiftsCount$ = GiftService.Instance.GetGiftsRowCount({campaignId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveCampaignList(rawList: any[]): CampaignData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveCampaign(raw));
  }

}
