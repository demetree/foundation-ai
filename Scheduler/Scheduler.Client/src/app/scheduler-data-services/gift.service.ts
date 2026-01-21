/*

   GENERATED SERVICE FOR THE GIFT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Gift table.

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
import { ConstituentData } from './constituent.service';
import { PledgeData } from './pledge.service';
import { FundData } from './fund.service';
import { CampaignData } from './campaign.service';
import { AppealData } from './appeal.service';
import { PaymentTypeData } from './payment-type.service';
import { BatchData } from './batch.service';
import { ReceiptTypeData } from './receipt-type.service';
import { TributeData } from './tribute.service';
import { GiftChangeHistoryService, GiftChangeHistoryData } from './gift-change-history.service';
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
export class GiftQueryParameters {
    officeId: bigint | number | null | undefined = null;
    constituentId: bigint | number | null | undefined = null;
    pledgeId: bigint | number | null | undefined = null;
    amount: number | null | undefined = null;
    receivedDate: string | null | undefined = null;        // ISO 8601
    postedDate: string | null | undefined = null;        // ISO 8601
    fundId: bigint | number | null | undefined = null;
    campaignId: bigint | number | null | undefined = null;
    appealId: bigint | number | null | undefined = null;
    paymentTypeId: bigint | number | null | undefined = null;
    referenceNumber: string | null | undefined = null;
    batchId: bigint | number | null | undefined = null;
    receiptTypeId: bigint | number | null | undefined = null;
    receiptDate: string | null | undefined = null;        // ISO 8601
    tributeId: bigint | number | null | undefined = null;
    notes: string | null | undefined = null;
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
export class GiftSubmitData {
    id!: bigint | number;
    officeId: bigint | number | null = null;
    constituentId!: bigint | number;
    pledgeId: bigint | number | null = null;
    amount!: number;
    receivedDate!: string;      // ISO 8601
    postedDate: string | null = null;     // ISO 8601
    fundId!: bigint | number;
    campaignId: bigint | number | null = null;
    appealId: bigint | number | null = null;
    paymentTypeId!: bigint | number;
    referenceNumber: string | null = null;
    batchId: bigint | number | null = null;
    receiptTypeId: bigint | number | null = null;
    receiptDate: string | null = null;     // ISO 8601
    tributeId: bigint | number | null = null;
    notes: string | null = null;
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

export class GiftBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. GiftChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `gift.GiftChildren$` — use with `| async` in templates
//        • Promise:    `gift.GiftChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="gift.GiftChildren$ | async"`), or
//        • Access the promise getter (`gift.GiftChildren` or `await gift.GiftChildren`)
//    - Simply reading `gift.GiftChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await gift.Reload()` to refresh the entire object and clear all lazy caches.
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
export class GiftData {
    id!: bigint | number;
    officeId!: bigint | number;
    constituentId!: bigint | number;
    pledgeId!: bigint | number;
    amount!: number;
    receivedDate!: string;      // ISO 8601
    postedDate!: string | null;   // ISO 8601
    fundId!: bigint | number;
    campaignId!: bigint | number;
    appealId!: bigint | number;
    paymentTypeId!: bigint | number;
    referenceNumber!: string | null;
    batchId!: bigint | number;
    receiptTypeId!: bigint | number;
    receiptDate!: string | null;   // ISO 8601
    tributeId!: bigint | number;
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    appeal: AppealData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    batch: BatchData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    campaign: CampaignData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    constituent: ConstituentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    fund: FundData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    office: OfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    paymentType: PaymentTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    pledge: PledgeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    receiptType: ReceiptTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    tribute: TributeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _giftChangeHistories: GiftChangeHistoryData[] | null = null;
    private _giftChangeHistoriesPromise: Promise<GiftChangeHistoryData[]> | null  = null;
    private _giftChangeHistoriesSubject = new BehaviorSubject<GiftChangeHistoryData[] | null>(null);

                
    private _softCredits: SoftCreditData[] | null = null;
    private _softCreditsPromise: Promise<SoftCreditData[]> | null  = null;
    private _softCreditsSubject = new BehaviorSubject<SoftCreditData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<GiftData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<GiftData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<GiftData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public GiftChangeHistories$ = this._giftChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._giftChangeHistories === null && this._giftChangeHistoriesPromise === null) {
            this.loadGiftChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public GiftChangeHistoriesCount$ = GiftChangeHistoryService.Instance.GetGiftChangeHistoriesRowCount({giftId: this.id,
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

  
    public SoftCreditsCount$ = SoftCreditService.Instance.GetSoftCreditsRowCount({giftId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any GiftData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.gift.Reload();
  //
  //  Non Async:
  //
  //     gift[0].Reload().then(x => {
  //        this.gift = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      GiftService.Instance.GetGift(this.id, includeRelations)
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
     this._giftChangeHistories = null;
     this._giftChangeHistoriesPromise = null;
     this._giftChangeHistoriesSubject.next(null);

     this._softCredits = null;
     this._softCreditsPromise = null;
     this._softCreditsSubject.next(null);

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
     * Gets the GiftChangeHistories for this Gift.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.gift.GiftChangeHistories.then(gifts => { ... })
     *   or
     *   await this.gift.gifts
     *
    */
    public get GiftChangeHistories(): Promise<GiftChangeHistoryData[]> {
        if (this._giftChangeHistories !== null) {
            return Promise.resolve(this._giftChangeHistories);
        }

        if (this._giftChangeHistoriesPromise !== null) {
            return this._giftChangeHistoriesPromise;
        }

        // Start the load
        this.loadGiftChangeHistories();

        return this._giftChangeHistoriesPromise!;
    }



    private loadGiftChangeHistories(): void {

        this._giftChangeHistoriesPromise = lastValueFrom(
            GiftService.Instance.GetGiftChangeHistoriesForGift(this.id)
        )
        .then(GiftChangeHistories => {
            this._giftChangeHistories = GiftChangeHistories ?? [];
            this._giftChangeHistoriesSubject.next(this._giftChangeHistories);
            return this._giftChangeHistories;
         })
        .catch(err => {
            this._giftChangeHistories = [];
            this._giftChangeHistoriesSubject.next(this._giftChangeHistories);
            throw err;
        })
        .finally(() => {
            this._giftChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached GiftChangeHistory. Call after mutations to force refresh.
     */
    public ClearGiftChangeHistoriesCache(): void {
        this._giftChangeHistories = null;
        this._giftChangeHistoriesPromise = null;
        this._giftChangeHistoriesSubject.next(this._giftChangeHistories);      // Emit to observable
    }

    public get HasGiftChangeHistories(): Promise<boolean> {
        return this.GiftChangeHistories.then(giftChangeHistories => giftChangeHistories.length > 0);
    }


    /**
     *
     * Gets the SoftCredits for this Gift.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.gift.SoftCredits.then(gifts => { ... })
     *   or
     *   await this.gift.gifts
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
            GiftService.Instance.GetSoftCreditsForGift(this.id)
        )
        .then(SoftCredits => {
            this._softCredits = SoftCredits ?? [];
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




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (gift.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await gift.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<GiftData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<GiftData>> {
        const info = await lastValueFrom(
            GiftService.Instance.GetGiftChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this GiftData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this GiftData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): GiftSubmitData {
        return GiftService.Instance.ConvertToGiftSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class GiftService extends SecureEndpointBase {

    private static _instance: GiftService;
    private listCache: Map<string, Observable<Array<GiftData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<GiftBasicListData>>>;
    private recordCache: Map<string, Observable<GiftData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private giftChangeHistoryService: GiftChangeHistoryService,
        private softCreditService: SoftCreditService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<GiftData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<GiftBasicListData>>>();
        this.recordCache = new Map<string, Observable<GiftData>>();

        GiftService._instance = this;
    }

    public static get Instance(): GiftService {
      return GiftService._instance;
    }


    public ClearListCaches(config: GiftQueryParameters | null = null) {

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


    public ConvertToGiftSubmitData(data: GiftData): GiftSubmitData {

        let output = new GiftSubmitData();

        output.id = data.id;
        output.officeId = data.officeId;
        output.constituentId = data.constituentId;
        output.pledgeId = data.pledgeId;
        output.amount = data.amount;
        output.receivedDate = data.receivedDate;
        output.postedDate = data.postedDate;
        output.fundId = data.fundId;
        output.campaignId = data.campaignId;
        output.appealId = data.appealId;
        output.paymentTypeId = data.paymentTypeId;
        output.referenceNumber = data.referenceNumber;
        output.batchId = data.batchId;
        output.receiptTypeId = data.receiptTypeId;
        output.receiptDate = data.receiptDate;
        output.tributeId = data.tributeId;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetGift(id: bigint | number, includeRelations: boolean = true) : Observable<GiftData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const gift$ = this.requestGift(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Gift", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, gift$);

            return gift$;
        }

        return this.recordCache.get(configHash) as Observable<GiftData>;
    }

    private requestGift(id: bigint | number, includeRelations: boolean = true) : Observable<GiftData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<GiftData>(this.baseUrl + 'api/Gift/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveGift(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestGift(id, includeRelations));
            }));
    }

    public GetGiftList(config: GiftQueryParameters | any = null) : Observable<Array<GiftData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const giftList$ = this.requestGiftList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Gift list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, giftList$);

            return giftList$;
        }

        return this.listCache.get(configHash) as Observable<Array<GiftData>>;
    }


    private requestGiftList(config: GiftQueryParameters | any) : Observable <Array<GiftData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<GiftData>>(this.baseUrl + 'api/Gifts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveGiftList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestGiftList(config));
            }));
    }

    public GetGiftsRowCount(config: GiftQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const giftsRowCount$ = this.requestGiftsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Gifts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, giftsRowCount$);

            return giftsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestGiftsRowCount(config: GiftQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Gifts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestGiftsRowCount(config));
            }));
    }

    public GetGiftsBasicListData(config: GiftQueryParameters | any = null) : Observable<Array<GiftBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const giftsBasicListData$ = this.requestGiftsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Gifts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, giftsBasicListData$);

            return giftsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<GiftBasicListData>>;
    }


    private requestGiftsBasicListData(config: GiftQueryParameters | any) : Observable<Array<GiftBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<GiftBasicListData>>(this.baseUrl + 'api/Gifts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestGiftsBasicListData(config));
            }));

    }


    public PutGift(id: bigint | number, gift: GiftSubmitData) : Observable<GiftData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<GiftData>(this.baseUrl + 'api/Gift/' + id.toString(), gift, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveGift(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutGift(id, gift));
            }));
    }


    public PostGift(gift: GiftSubmitData) : Observable<GiftData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<GiftData>(this.baseUrl + 'api/Gift', gift, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveGift(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostGift(gift));
            }));
    }

  
    public DeleteGift(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Gift/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteGift(id));
            }));
    }

    public RollbackGift(id: bigint | number, versionNumber: bigint | number) : Observable<GiftData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<GiftData>(this.baseUrl + 'api/Gift/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveGift(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackGift(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Gift.
     */
    public GetGiftChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<GiftData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<GiftData>>(this.baseUrl + 'api/Gift/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetGiftChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Gift.
     */
    public GetGiftAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<GiftData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<GiftData>[]>(this.baseUrl + 'api/Gift/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetGiftAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Gift.
     */
    public GetGiftVersion(id: bigint | number, version: number): Observable<GiftData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<GiftData>(this.baseUrl + 'api/Gift/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveGift(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetGiftVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Gift at a specific point in time.
     */
    public GetGiftStateAtTime(id: bigint | number, time: string): Observable<GiftData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<GiftData>(this.baseUrl + 'api/Gift/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveGift(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetGiftStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: GiftQueryParameters | any): string {

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

    public userIsSchedulerGiftReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerGiftReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Gifts
        //
        if (userIsSchedulerGiftReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerGiftReader = user.readPermission >= 1;
            } else {
                userIsSchedulerGiftReader = false;
            }
        }

        return userIsSchedulerGiftReader;
    }


    public userIsSchedulerGiftWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerGiftWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Gifts
        //
        if (userIsSchedulerGiftWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerGiftWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerGiftWriter = false;
          }      
        }

        return userIsSchedulerGiftWriter;
    }

    public GetGiftChangeHistoriesForGift(giftId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GiftChangeHistoryData[]> {
        return this.giftChangeHistoryService.GetGiftChangeHistoryList({
            giftId: giftId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSoftCreditsForGift(giftId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SoftCreditData[]> {
        return this.softCreditService.GetSoftCreditList({
            giftId: giftId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full GiftData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the GiftData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when GiftTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveGift(raw: any): GiftData {
    if (!raw) return raw;

    //
    // Create a GiftData object instance with correct prototype
    //
    const revived = Object.create(GiftData.prototype) as GiftData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._giftChangeHistories = null;
    (revived as any)._giftChangeHistoriesPromise = null;
    (revived as any)._giftChangeHistoriesSubject = new BehaviorSubject<GiftChangeHistoryData[] | null>(null);

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
    // 2. But private methods (loadGiftXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).GiftChangeHistories$ = (revived as any)._giftChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._giftChangeHistories === null && (revived as any)._giftChangeHistoriesPromise === null) {
                (revived as any).loadGiftChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).GiftChangeHistoriesCount$ = GiftChangeHistoryService.Instance.GetGiftChangeHistoriesRowCount({giftId: (revived as any).id,
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

    (revived as any).SoftCreditsCount$ = SoftCreditService.Instance.GetSoftCreditsRowCount({giftId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<GiftData> | null>(null);

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

  private ReviveGiftList(rawList: any[]): GiftData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveGift(raw));
  }

}
