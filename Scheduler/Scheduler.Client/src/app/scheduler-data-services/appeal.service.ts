/*

   GENERATED SERVICE FOR THE APPEAL TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Appeal table.

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
import { CampaignData } from './campaign.service';
import { IconData } from './icon.service';
import { AppealChangeHistoryService, AppealChangeHistoryData } from './appeal-change-history.service';
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
export class AppealQueryParameters {
    campaignId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    costPerUnit: number | null | undefined = null;
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
export class AppealSubmitData {
    id!: bigint | number;
    campaignId: bigint | number | null = null;
    name!: string;
    description: string | null = null;
    costPerUnit: number | null = null;
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

export class AppealBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AppealChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `appeal.AppealChildren$` — use with `| async` in templates
//        • Promise:    `appeal.AppealChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="appeal.AppealChildren$ | async"`), or
//        • Access the promise getter (`appeal.AppealChildren` or `await appeal.AppealChildren`)
//    - Simply reading `appeal.AppealChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await appeal.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AppealData {
    id!: bigint | number;
    campaignId!: bigint | number;
    name!: string;
    description!: string | null;
    costPerUnit!: number | null;
    notes!: string | null;
    iconId!: bigint | number;
    color!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    campaign: CampaignData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _appealChangeHistories: AppealChangeHistoryData[] | null = null;
    private _appealChangeHistoriesPromise: Promise<AppealChangeHistoryData[]> | null  = null;
    private _appealChangeHistoriesSubject = new BehaviorSubject<AppealChangeHistoryData[] | null>(null);

                
    private _pledges: PledgeData[] | null = null;
    private _pledgesPromise: Promise<PledgeData[]> | null  = null;
    private _pledgesSubject = new BehaviorSubject<PledgeData[] | null>(null);

                
    private _batchDefaultAppeals: BatchData[] | null = null;
    private _batchDefaultAppealsPromise: Promise<BatchData[]> | null  = null;
    private _batchDefaultAppealsSubject = new BehaviorSubject<BatchData[] | null>(null);
                    
    private _gifts: GiftData[] | null = null;
    private _giftsPromise: Promise<GiftData[]> | null  = null;
    private _giftsSubject = new BehaviorSubject<GiftData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<AppealData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<AppealData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<AppealData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public AppealChangeHistories$ = this._appealChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._appealChangeHistories === null && this._appealChangeHistoriesPromise === null) {
            this.loadAppealChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public AppealChangeHistoriesCount$ = AppealChangeHistoryService.Instance.GetAppealChangeHistoriesRowCount({appealId: this.id,
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

  
    public PledgesCount$ = PledgeService.Instance.GetPledgesRowCount({appealId: this.id,
      active: true,
      deleted: false
    });



    public BatchDefaultAppeals$ = this._batchDefaultAppealsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._batchDefaultAppeals === null && this._batchDefaultAppealsPromise === null) {
            this.loadBatchDefaultAppeals(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public BatchDefaultAppealsCount$ = BatchService.Instance.GetBatchesRowCount({defaultAppealId: this.id,
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

  
    public GiftsCount$ = GiftService.Instance.GetGiftsRowCount({appealId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AppealData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.appeal.Reload();
  //
  //  Non Async:
  //
  //     appeal[0].Reload().then(x => {
  //        this.appeal = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AppealService.Instance.GetAppeal(this.id, includeRelations)
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
     this._appealChangeHistories = null;
     this._appealChangeHistoriesPromise = null;
     this._appealChangeHistoriesSubject.next(null);

     this._pledges = null;
     this._pledgesPromise = null;
     this._pledgesSubject.next(null);

     this._batchDefaultAppeals = null;
     this._batchDefaultAppealsPromise = null;
     this._batchDefaultAppealsSubject.next(null);

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
     * Gets the AppealChangeHistories for this Appeal.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.appeal.AppealChangeHistories.then(appeals => { ... })
     *   or
     *   await this.appeal.appeals
     *
    */
    public get AppealChangeHistories(): Promise<AppealChangeHistoryData[]> {
        if (this._appealChangeHistories !== null) {
            return Promise.resolve(this._appealChangeHistories);
        }

        if (this._appealChangeHistoriesPromise !== null) {
            return this._appealChangeHistoriesPromise;
        }

        // Start the load
        this.loadAppealChangeHistories();

        return this._appealChangeHistoriesPromise!;
    }



    private loadAppealChangeHistories(): void {

        this._appealChangeHistoriesPromise = lastValueFrom(
            AppealService.Instance.GetAppealChangeHistoriesForAppeal(this.id)
        )
        .then(AppealChangeHistories => {
            this._appealChangeHistories = AppealChangeHistories ?? [];
            this._appealChangeHistoriesSubject.next(this._appealChangeHistories);
            return this._appealChangeHistories;
         })
        .catch(err => {
            this._appealChangeHistories = [];
            this._appealChangeHistoriesSubject.next(this._appealChangeHistories);
            throw err;
        })
        .finally(() => {
            this._appealChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached AppealChangeHistory. Call after mutations to force refresh.
     */
    public ClearAppealChangeHistoriesCache(): void {
        this._appealChangeHistories = null;
        this._appealChangeHistoriesPromise = null;
        this._appealChangeHistoriesSubject.next(this._appealChangeHistories);      // Emit to observable
    }

    public get HasAppealChangeHistories(): Promise<boolean> {
        return this.AppealChangeHistories.then(appealChangeHistories => appealChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Pledges for this Appeal.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.appeal.Pledges.then(appeals => { ... })
     *   or
     *   await this.appeal.appeals
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
            AppealService.Instance.GetPledgesForAppeal(this.id)
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
     * Gets the BatchDefaultAppeals for this Appeal.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.appeal.BatchDefaultAppeals.then(defaultAppeals => { ... })
     *   or
     *   await this.appeal.defaultAppeals
     *
    */
    public get BatchDefaultAppeals(): Promise<BatchData[]> {
        if (this._batchDefaultAppeals !== null) {
            return Promise.resolve(this._batchDefaultAppeals);
        }

        if (this._batchDefaultAppealsPromise !== null) {
            return this._batchDefaultAppealsPromise;
        }

        // Start the load
        this.loadBatchDefaultAppeals();

        return this._batchDefaultAppealsPromise!;
    }



    private loadBatchDefaultAppeals(): void {

        this._batchDefaultAppealsPromise = lastValueFrom(
            AppealService.Instance.GetBatchDefaultAppealsForAppeal(this.id)
        )
        .then(BatchDefaultAppeals => {
            this._batchDefaultAppeals = BatchDefaultAppeals ?? [];
            this._batchDefaultAppealsSubject.next(this._batchDefaultAppeals);
            return this._batchDefaultAppeals;
         })
        .catch(err => {
            this._batchDefaultAppeals = [];
            this._batchDefaultAppealsSubject.next(this._batchDefaultAppeals);
            throw err;
        })
        .finally(() => {
            this._batchDefaultAppealsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BatchDefaultAppeal. Call after mutations to force refresh.
     */
    public ClearBatchDefaultAppealsCache(): void {
        this._batchDefaultAppeals = null;
        this._batchDefaultAppealsPromise = null;
        this._batchDefaultAppealsSubject.next(this._batchDefaultAppeals);      // Emit to observable
    }

    public get HasBatchDefaultAppeals(): Promise<boolean> {
        return this.BatchDefaultAppeals.then(batchDefaultAppeals => batchDefaultAppeals.length > 0);
    }


    /**
     *
     * Gets the Gifts for this Appeal.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.appeal.Gifts.then(appeals => { ... })
     *   or
     *   await this.appeal.appeals
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
            AppealService.Instance.GetGiftsForAppeal(this.id)
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
    //   Template: {{ (appeal.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await appeal.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<AppealData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<AppealData>> {
        const info = await lastValueFrom(
            AppealService.Instance.GetAppealChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this AppealData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AppealData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AppealSubmitData {
        return AppealService.Instance.ConvertToAppealSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AppealService extends SecureEndpointBase {

    private static _instance: AppealService;
    private listCache: Map<string, Observable<Array<AppealData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AppealBasicListData>>>;
    private recordCache: Map<string, Observable<AppealData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private appealChangeHistoryService: AppealChangeHistoryService,
        private pledgeService: PledgeService,
        private batchService: BatchService,
        private giftService: GiftService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AppealData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AppealBasicListData>>>();
        this.recordCache = new Map<string, Observable<AppealData>>();

        AppealService._instance = this;
    }

    public static get Instance(): AppealService {
      return AppealService._instance;
    }


    public ClearListCaches(config: AppealQueryParameters | null = null) {

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


    public ConvertToAppealSubmitData(data: AppealData): AppealSubmitData {

        let output = new AppealSubmitData();

        output.id = data.id;
        output.campaignId = data.campaignId;
        output.name = data.name;
        output.description = data.description;
        output.costPerUnit = data.costPerUnit;
        output.notes = data.notes;
        output.iconId = data.iconId;
        output.color = data.color;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetAppeal(id: bigint | number, includeRelations: boolean = true) : Observable<AppealData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const appeal$ = this.requestAppeal(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Appeal", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, appeal$);

            return appeal$;
        }

        return this.recordCache.get(configHash) as Observable<AppealData>;
    }

    private requestAppeal(id: bigint | number, includeRelations: boolean = true) : Observable<AppealData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AppealData>(this.baseUrl + 'api/Appeal/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAppeal(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAppeal(id, includeRelations));
            }));
    }

    public GetAppealList(config: AppealQueryParameters | any = null) : Observable<Array<AppealData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const appealList$ = this.requestAppealList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Appeal list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, appealList$);

            return appealList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AppealData>>;
    }


    private requestAppealList(config: AppealQueryParameters | any) : Observable <Array<AppealData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AppealData>>(this.baseUrl + 'api/Appeals', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAppealList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAppealList(config));
            }));
    }

    public GetAppealsRowCount(config: AppealQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const appealsRowCount$ = this.requestAppealsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Appeals row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, appealsRowCount$);

            return appealsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAppealsRowCount(config: AppealQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Appeals/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAppealsRowCount(config));
            }));
    }

    public GetAppealsBasicListData(config: AppealQueryParameters | any = null) : Observable<Array<AppealBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const appealsBasicListData$ = this.requestAppealsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Appeals basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, appealsBasicListData$);

            return appealsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AppealBasicListData>>;
    }


    private requestAppealsBasicListData(config: AppealQueryParameters | any) : Observable<Array<AppealBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AppealBasicListData>>(this.baseUrl + 'api/Appeals/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAppealsBasicListData(config));
            }));

    }


    public PutAppeal(id: bigint | number, appeal: AppealSubmitData) : Observable<AppealData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AppealData>(this.baseUrl + 'api/Appeal/' + id.toString(), appeal, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAppeal(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAppeal(id, appeal));
            }));
    }


    public PostAppeal(appeal: AppealSubmitData) : Observable<AppealData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AppealData>(this.baseUrl + 'api/Appeal', appeal, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAppeal(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAppeal(appeal));
            }));
    }

  
    public DeleteAppeal(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Appeal/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAppeal(id));
            }));
    }

    public RollbackAppeal(id: bigint | number, versionNumber: bigint | number) : Observable<AppealData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AppealData>(this.baseUrl + 'api/Appeal/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAppeal(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackAppeal(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Appeal.
     */
    public GetAppealChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<AppealData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<AppealData>>(this.baseUrl + 'api/Appeal/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetAppealChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Appeal.
     */
    public GetAppealAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<AppealData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<AppealData>[]>(this.baseUrl + 'api/Appeal/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetAppealAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Appeal.
     */
    public GetAppealVersion(id: bigint | number, version: number): Observable<AppealData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AppealData>(this.baseUrl + 'api/Appeal/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveAppeal(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetAppealVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Appeal at a specific point in time.
     */
    public GetAppealStateAtTime(id: bigint | number, time: string): Observable<AppealData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AppealData>(this.baseUrl + 'api/Appeal/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveAppeal(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetAppealStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: AppealQueryParameters | any): string {

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

    public userIsSchedulerAppealReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerAppealReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Appeals
        //
        if (userIsSchedulerAppealReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerAppealReader = user.readPermission >= 1;
            } else {
                userIsSchedulerAppealReader = false;
            }
        }

        return userIsSchedulerAppealReader;
    }


    public userIsSchedulerAppealWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerAppealWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Appeals
        //
        if (userIsSchedulerAppealWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerAppealWriter = user.writePermission >= 60;
          } else {
            userIsSchedulerAppealWriter = false;
          }      
        }

        return userIsSchedulerAppealWriter;
    }

    public GetAppealChangeHistoriesForAppeal(appealId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AppealChangeHistoryData[]> {
        return this.appealChangeHistoryService.GetAppealChangeHistoryList({
            appealId: appealId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPledgesForAppeal(appealId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PledgeData[]> {
        return this.pledgeService.GetPledgeList({
            appealId: appealId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBatchDefaultAppealsForAppeal(appealId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BatchData[]> {
        return this.batchService.GetBatchList({
            defaultAppealId: appealId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetGiftsForAppeal(appealId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GiftData[]> {
        return this.giftService.GetGiftList({
            appealId: appealId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AppealData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AppealData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AppealTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAppeal(raw: any): AppealData {
    if (!raw) return raw;

    //
    // Create a AppealData object instance with correct prototype
    //
    const revived = Object.create(AppealData.prototype) as AppealData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._appealChangeHistories = null;
    (revived as any)._appealChangeHistoriesPromise = null;
    (revived as any)._appealChangeHistoriesSubject = new BehaviorSubject<AppealChangeHistoryData[] | null>(null);

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
    // 2. But private methods (loadAppealXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).AppealChangeHistories$ = (revived as any)._appealChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._appealChangeHistories === null && (revived as any)._appealChangeHistoriesPromise === null) {
                (revived as any).loadAppealChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).AppealChangeHistoriesCount$ = AppealChangeHistoryService.Instance.GetAppealChangeHistoriesRowCount({appealId: (revived as any).id,
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

    (revived as any).PledgesCount$ = PledgeService.Instance.GetPledgesRowCount({appealId: (revived as any).id,
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

    (revived as any).BatchesCount$ = BatchService.Instance.GetBatchesRowCount({appealId: (revived as any).id,
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

    (revived as any).GiftsCount$ = GiftService.Instance.GetGiftsRowCount({appealId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<AppealData> | null>(null);

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

  private ReviveAppealList(rawList: any[]): AppealData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAppeal(raw));
  }

}
