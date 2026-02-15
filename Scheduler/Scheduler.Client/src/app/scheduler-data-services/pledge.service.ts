/*

   GENERATED SERVICE FOR THE PLEDGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Pledge table.

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
import { ConstituentData } from './constituent.service';
import { RecurrenceFrequencyData } from './recurrence-frequency.service';
import { FundData } from './fund.service';
import { CampaignData } from './campaign.service';
import { AppealData } from './appeal.service';
import { PledgeChangeHistoryService, PledgeChangeHistoryData } from './pledge-change-history.service';
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
export class PledgeQueryParameters {
    constituentId: bigint | number | null | undefined = null;
    totalAmount: number | null | undefined = null;
    balanceAmount: number | null | undefined = null;
    pledgeDate: string | null | undefined = null;        // ISO 8601
    startDate: string | null | undefined = null;        // ISO 8601
    endDate: string | null | undefined = null;        // ISO 8601
    recurrenceFrequencyId: bigint | number | null | undefined = null;
    fundId: bigint | number | null | undefined = null;
    campaignId: bigint | number | null | undefined = null;
    appealId: bigint | number | null | undefined = null;
    writeOffAmount: number | null | undefined = null;
    isWrittenOff: boolean | null | undefined = null;
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
export class PledgeSubmitData {
    id!: bigint | number;
    constituentId!: bigint | number;
    totalAmount!: number;
    balanceAmount!: number;
    pledgeDate!: string;      // ISO 8601
    startDate: string | null = null;     // ISO 8601
    endDate: string | null = null;     // ISO 8601
    recurrenceFrequencyId: bigint | number | null = null;
    fundId!: bigint | number;
    campaignId: bigint | number | null = null;
    appealId: bigint | number | null = null;
    writeOffAmount!: number;
    isWrittenOff!: boolean;
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

export class PledgeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PledgeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `pledge.PledgeChildren$` — use with `| async` in templates
//        • Promise:    `pledge.PledgeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="pledge.PledgeChildren$ | async"`), or
//        • Access the promise getter (`pledge.PledgeChildren` or `await pledge.PledgeChildren`)
//    - Simply reading `pledge.PledgeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await pledge.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PledgeData {
    id!: bigint | number;
    constituentId!: bigint | number;
    totalAmount!: number;
    balanceAmount!: number;
    pledgeDate!: string;      // ISO 8601
    startDate!: string | null;   // ISO 8601
    endDate!: string | null;   // ISO 8601
    recurrenceFrequencyId!: bigint | number;
    fundId!: bigint | number;
    campaignId!: bigint | number;
    appealId!: bigint | number;
    writeOffAmount!: number;
    isWrittenOff!: boolean;
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    appeal: AppealData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    campaign: CampaignData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    constituent: ConstituentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    fund: FundData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    recurrenceFrequency: RecurrenceFrequencyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _pledgeChangeHistories: PledgeChangeHistoryData[] | null = null;
    private _pledgeChangeHistoriesPromise: Promise<PledgeChangeHistoryData[]> | null  = null;
    private _pledgeChangeHistoriesSubject = new BehaviorSubject<PledgeChangeHistoryData[] | null>(null);

                
    private _gifts: GiftData[] | null = null;
    private _giftsPromise: Promise<GiftData[]> | null  = null;
    private _giftsSubject = new BehaviorSubject<GiftData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<PledgeData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<PledgeData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PledgeData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public PledgeChangeHistories$ = this._pledgeChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._pledgeChangeHistories === null && this._pledgeChangeHistoriesPromise === null) {
            this.loadPledgeChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public PledgeChangeHistoriesCount$ = PledgeChangeHistoryService.Instance.GetPledgeChangeHistoriesRowCount({pledgeId: this.id,
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

  
    public GiftsCount$ = GiftService.Instance.GetGiftsRowCount({pledgeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PledgeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.pledge.Reload();
  //
  //  Non Async:
  //
  //     pledge[0].Reload().then(x => {
  //        this.pledge = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PledgeService.Instance.GetPledge(this.id, includeRelations)
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
     this._pledgeChangeHistories = null;
     this._pledgeChangeHistoriesPromise = null;
     this._pledgeChangeHistoriesSubject.next(null);

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
     * Gets the PledgeChangeHistories for this Pledge.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.pledge.PledgeChangeHistories.then(pledges => { ... })
     *   or
     *   await this.pledge.pledges
     *
    */
    public get PledgeChangeHistories(): Promise<PledgeChangeHistoryData[]> {
        if (this._pledgeChangeHistories !== null) {
            return Promise.resolve(this._pledgeChangeHistories);
        }

        if (this._pledgeChangeHistoriesPromise !== null) {
            return this._pledgeChangeHistoriesPromise;
        }

        // Start the load
        this.loadPledgeChangeHistories();

        return this._pledgeChangeHistoriesPromise!;
    }



    private loadPledgeChangeHistories(): void {

        this._pledgeChangeHistoriesPromise = lastValueFrom(
            PledgeService.Instance.GetPledgeChangeHistoriesForPledge(this.id)
        )
        .then(PledgeChangeHistories => {
            this._pledgeChangeHistories = PledgeChangeHistories ?? [];
            this._pledgeChangeHistoriesSubject.next(this._pledgeChangeHistories);
            return this._pledgeChangeHistories;
         })
        .catch(err => {
            this._pledgeChangeHistories = [];
            this._pledgeChangeHistoriesSubject.next(this._pledgeChangeHistories);
            throw err;
        })
        .finally(() => {
            this._pledgeChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PledgeChangeHistory. Call after mutations to force refresh.
     */
    public ClearPledgeChangeHistoriesCache(): void {
        this._pledgeChangeHistories = null;
        this._pledgeChangeHistoriesPromise = null;
        this._pledgeChangeHistoriesSubject.next(this._pledgeChangeHistories);      // Emit to observable
    }

    public get HasPledgeChangeHistories(): Promise<boolean> {
        return this.PledgeChangeHistories.then(pledgeChangeHistories => pledgeChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Gifts for this Pledge.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.pledge.Gifts.then(pledges => { ... })
     *   or
     *   await this.pledge.pledges
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
            PledgeService.Instance.GetGiftsForPledge(this.id)
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
    //   Template: {{ (pledge.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await pledge.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<PledgeData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<PledgeData>> {
        const info = await lastValueFrom(
            PledgeService.Instance.GetPledgeChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this PledgeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PledgeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PledgeSubmitData {
        return PledgeService.Instance.ConvertToPledgeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PledgeService extends SecureEndpointBase {

    private static _instance: PledgeService;
    private listCache: Map<string, Observable<Array<PledgeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PledgeBasicListData>>>;
    private recordCache: Map<string, Observable<PledgeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private pledgeChangeHistoryService: PledgeChangeHistoryService,
        private giftService: GiftService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PledgeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PledgeBasicListData>>>();
        this.recordCache = new Map<string, Observable<PledgeData>>();

        PledgeService._instance = this;
    }

    public static get Instance(): PledgeService {
      return PledgeService._instance;
    }


    public ClearListCaches(config: PledgeQueryParameters | null = null) {

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


    public ConvertToPledgeSubmitData(data: PledgeData): PledgeSubmitData {

        let output = new PledgeSubmitData();

        output.id = data.id;
        output.constituentId = data.constituentId;
        output.totalAmount = data.totalAmount;
        output.balanceAmount = data.balanceAmount;
        output.pledgeDate = data.pledgeDate;
        output.startDate = data.startDate;
        output.endDate = data.endDate;
        output.recurrenceFrequencyId = data.recurrenceFrequencyId;
        output.fundId = data.fundId;
        output.campaignId = data.campaignId;
        output.appealId = data.appealId;
        output.writeOffAmount = data.writeOffAmount;
        output.isWrittenOff = data.isWrittenOff;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPledge(id: bigint | number, includeRelations: boolean = true) : Observable<PledgeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const pledge$ = this.requestPledge(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Pledge", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, pledge$);

            return pledge$;
        }

        return this.recordCache.get(configHash) as Observable<PledgeData>;
    }

    private requestPledge(id: bigint | number, includeRelations: boolean = true) : Observable<PledgeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PledgeData>(this.baseUrl + 'api/Pledge/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePledge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPledge(id, includeRelations));
            }));
    }

    public GetPledgeList(config: PledgeQueryParameters | any = null) : Observable<Array<PledgeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const pledgeList$ = this.requestPledgeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Pledge list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, pledgeList$);

            return pledgeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PledgeData>>;
    }


    private requestPledgeList(config: PledgeQueryParameters | any) : Observable <Array<PledgeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PledgeData>>(this.baseUrl + 'api/Pledges', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePledgeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPledgeList(config));
            }));
    }

    public GetPledgesRowCount(config: PledgeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const pledgesRowCount$ = this.requestPledgesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Pledges row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, pledgesRowCount$);

            return pledgesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPledgesRowCount(config: PledgeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Pledges/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPledgesRowCount(config));
            }));
    }

    public GetPledgesBasicListData(config: PledgeQueryParameters | any = null) : Observable<Array<PledgeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const pledgesBasicListData$ = this.requestPledgesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Pledges basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, pledgesBasicListData$);

            return pledgesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PledgeBasicListData>>;
    }


    private requestPledgesBasicListData(config: PledgeQueryParameters | any) : Observable<Array<PledgeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PledgeBasicListData>>(this.baseUrl + 'api/Pledges/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPledgesBasicListData(config));
            }));

    }


    public PutPledge(id: bigint | number, pledge: PledgeSubmitData) : Observable<PledgeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PledgeData>(this.baseUrl + 'api/Pledge/' + id.toString(), pledge, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePledge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPledge(id, pledge));
            }));
    }


    public PostPledge(pledge: PledgeSubmitData) : Observable<PledgeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PledgeData>(this.baseUrl + 'api/Pledge', pledge, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePledge(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPledge(pledge));
            }));
    }

  
    public DeletePledge(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Pledge/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePledge(id));
            }));
    }

    public RollbackPledge(id: bigint | number, versionNumber: bigint | number) : Observable<PledgeData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PledgeData>(this.baseUrl + 'api/Pledge/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePledge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackPledge(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Pledge.
     */
    public GetPledgeChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<PledgeData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PledgeData>>(this.baseUrl + 'api/Pledge/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPledgeChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Pledge.
     */
    public GetPledgeAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<PledgeData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PledgeData>[]>(this.baseUrl + 'api/Pledge/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPledgeAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Pledge.
     */
    public GetPledgeVersion(id: bigint | number, version: number): Observable<PledgeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PledgeData>(this.baseUrl + 'api/Pledge/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePledge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPledgeVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Pledge at a specific point in time.
     */
    public GetPledgeStateAtTime(id: bigint | number, time: string): Observable<PledgeData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PledgeData>(this.baseUrl + 'api/Pledge/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePledge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPledgeStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: PledgeQueryParameters | any): string {

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

    public userIsSchedulerPledgeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerPledgeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Pledges
        //
        if (userIsSchedulerPledgeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerPledgeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerPledgeReader = false;
            }
        }

        return userIsSchedulerPledgeReader;
    }


    public userIsSchedulerPledgeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerPledgeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Pledges
        //
        if (userIsSchedulerPledgeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerPledgeWriter = user.writePermission >= 60;
          } else {
            userIsSchedulerPledgeWriter = false;
          }      
        }

        return userIsSchedulerPledgeWriter;
    }

    public GetPledgeChangeHistoriesForPledge(pledgeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PledgeChangeHistoryData[]> {
        return this.pledgeChangeHistoryService.GetPledgeChangeHistoryList({
            pledgeId: pledgeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetGiftsForPledge(pledgeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GiftData[]> {
        return this.giftService.GetGiftList({
            pledgeId: pledgeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PledgeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PledgeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PledgeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePledge(raw: any): PledgeData {
    if (!raw) return raw;

    //
    // Create a PledgeData object instance with correct prototype
    //
    const revived = Object.create(PledgeData.prototype) as PledgeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._pledgeChangeHistories = null;
    (revived as any)._pledgeChangeHistoriesPromise = null;
    (revived as any)._pledgeChangeHistoriesSubject = new BehaviorSubject<PledgeChangeHistoryData[] | null>(null);

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
    // 2. But private methods (loadPledgeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).PledgeChangeHistories$ = (revived as any)._pledgeChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._pledgeChangeHistories === null && (revived as any)._pledgeChangeHistoriesPromise === null) {
                (revived as any).loadPledgeChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).PledgeChangeHistoriesCount$ = PledgeChangeHistoryService.Instance.GetPledgeChangeHistoriesRowCount({pledgeId: (revived as any).id,
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

    (revived as any).GiftsCount$ = GiftService.Instance.GetGiftsRowCount({pledgeId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PledgeData> | null>(null);

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

  private RevivePledgeList(rawList: any[]): PledgeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePledge(raw));
  }

}
