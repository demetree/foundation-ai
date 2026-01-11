/*

   GENERATED SERVICE FOR THE TRIBUTE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Tribute table.

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
import { TributeTypeData } from './tribute-type.service';
import { ConstituentData } from './constituent.service';
import { IconData } from './icon.service';
import { TributeChangeHistoryService, TributeChangeHistoryData } from './tribute-change-history.service';
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
export class TributeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    tributeTypeId: bigint | number | null | undefined = null;
    defaultAcknowledgeeId: bigint | number | null | undefined = null;
    startDate: string | null | undefined = null;        // ISO 8601
    endDate: string | null | undefined = null;        // ISO 8601
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
export class TributeSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    tributeTypeId: bigint | number | null = null;
    defaultAcknowledgeeId: bigint | number | null = null;
    startDate: string | null = null;     // ISO 8601
    endDate: string | null = null;     // ISO 8601
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


export class TributeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TributeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `tribute.TributeChildren$` — use with `| async` in templates
//        • Promise:    `tribute.TributeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="tribute.TributeChildren$ | async"`), or
//        • Access the promise getter (`tribute.TributeChildren` or `await tribute.TributeChildren`)
//    - Simply reading `tribute.TributeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await tribute.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TributeData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    tributeTypeId!: bigint | number;
    defaultAcknowledgeeId!: bigint | number;
    startDate!: string | null;   // ISO 8601
    endDate!: string | null;   // ISO 8601
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
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    tributeType: TributeTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    defaultAcknowledgee: ConstituentData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _tributeChangeHistories: TributeChangeHistoryData[] | null = null;
    private _tributeChangeHistoriesPromise: Promise<TributeChangeHistoryData[]> | null  = null;
    private _tributeChangeHistoriesSubject = new BehaviorSubject<TributeChangeHistoryData[] | null>(null);

    private _gifts: GiftData[] | null = null;
    private _giftsPromise: Promise<GiftData[]> | null  = null;
    private _giftsSubject = new BehaviorSubject<GiftData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public TributeChangeHistories$ = this._tributeChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._tributeChangeHistories === null && this._tributeChangeHistoriesPromise === null) {
            this.loadTributeChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public TributeChangeHistoriesCount$ = TributeService.Instance.GetTributesRowCount({tributeId: this.id,
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

  
    public GiftsCount$ = TributeService.Instance.GetTributesRowCount({tributeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any TributeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.tribute.Reload();
  //
  //  Non Async:
  //
  //     tribute[0].Reload().then(x => {
  //        this.tribute = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TributeService.Instance.GetTribute(this.id, includeRelations)
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
     this._tributeChangeHistories = null;
     this._tributeChangeHistoriesPromise = null;
     this._tributeChangeHistoriesSubject.next(null);

     this._gifts = null;
     this._giftsPromise = null;
     this._giftsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the TributeChangeHistories for this Tribute.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.tribute.TributeChangeHistories.then(tributeChangeHistories => { ... })
     *   or
     *   await this.tribute.TributeChangeHistories
     *
    */
    public get TributeChangeHistories(): Promise<TributeChangeHistoryData[]> {
        if (this._tributeChangeHistories !== null) {
            return Promise.resolve(this._tributeChangeHistories);
        }

        if (this._tributeChangeHistoriesPromise !== null) {
            return this._tributeChangeHistoriesPromise;
        }

        // Start the load
        this.loadTributeChangeHistories();

        return this._tributeChangeHistoriesPromise!;
    }



    private loadTributeChangeHistories(): void {

        this._tributeChangeHistoriesPromise = lastValueFrom(
            TributeService.Instance.GetTributeChangeHistoriesForTribute(this.id)
        )
        .then(tributeChangeHistories => {
            this._tributeChangeHistories = tributeChangeHistories ?? [];
            this._tributeChangeHistoriesSubject.next(this._tributeChangeHistories);
            return this._tributeChangeHistories;
         })
        .catch(err => {
            this._tributeChangeHistories = [];
            this._tributeChangeHistoriesSubject.next(this._tributeChangeHistories);
            throw err;
        })
        .finally(() => {
            this._tributeChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached TributeChangeHistory. Call after mutations to force refresh.
     */
    public ClearTributeChangeHistoriesCache(): void {
        this._tributeChangeHistories = null;
        this._tributeChangeHistoriesPromise = null;
        this._tributeChangeHistoriesSubject.next(this._tributeChangeHistories);      // Emit to observable
    }

    public get HasTributeChangeHistories(): Promise<boolean> {
        return this.TributeChangeHistories.then(tributeChangeHistories => tributeChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Gifts for this Tribute.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.tribute.Gifts.then(gifts => { ... })
     *   or
     *   await this.tribute.Gifts
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
            TributeService.Instance.GetGiftsForTribute(this.id)
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
     * Updates the state of this TributeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TributeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TributeSubmitData {
        return TributeService.Instance.ConvertToTributeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TributeService extends SecureEndpointBase {

    private static _instance: TributeService;
    private listCache: Map<string, Observable<Array<TributeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TributeBasicListData>>>;
    private recordCache: Map<string, Observable<TributeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private tributeChangeHistoryService: TributeChangeHistoryService,
        private giftService: GiftService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TributeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TributeBasicListData>>>();
        this.recordCache = new Map<string, Observable<TributeData>>();

        TributeService._instance = this;
    }

    public static get Instance(): TributeService {
      return TributeService._instance;
    }


    public ClearListCaches(config: TributeQueryParameters | null = null) {

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


    public ConvertToTributeSubmitData(data: TributeData): TributeSubmitData {

        let output = new TributeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.tributeTypeId = data.tributeTypeId;
        output.defaultAcknowledgeeId = data.defaultAcknowledgeeId;
        output.startDate = data.startDate;
        output.endDate = data.endDate;
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

    public GetTribute(id: bigint | number, includeRelations: boolean = true) : Observable<TributeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const tribute$ = this.requestTribute(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Tribute", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, tribute$);

            return tribute$;
        }

        return this.recordCache.get(configHash) as Observable<TributeData>;
    }

    private requestTribute(id: bigint | number, includeRelations: boolean = true) : Observable<TributeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TributeData>(this.baseUrl + 'api/Tribute/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTribute(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTribute(id, includeRelations));
            }));
    }

    public GetTributeList(config: TributeQueryParameters | any = null) : Observable<Array<TributeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const tributeList$ = this.requestTributeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Tribute list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, tributeList$);

            return tributeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TributeData>>;
    }


    private requestTributeList(config: TributeQueryParameters | any) : Observable <Array<TributeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TributeData>>(this.baseUrl + 'api/Tributes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTributeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTributeList(config));
            }));
    }

    public GetTributesRowCount(config: TributeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const tributesRowCount$ = this.requestTributesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Tributes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, tributesRowCount$);

            return tributesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTributesRowCount(config: TributeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Tributes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTributesRowCount(config));
            }));
    }

    public GetTributesBasicListData(config: TributeQueryParameters | any = null) : Observable<Array<TributeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const tributesBasicListData$ = this.requestTributesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Tributes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, tributesBasicListData$);

            return tributesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TributeBasicListData>>;
    }


    private requestTributesBasicListData(config: TributeQueryParameters | any) : Observable<Array<TributeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TributeBasicListData>>(this.baseUrl + 'api/Tributes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTributesBasicListData(config));
            }));

    }


    public PutTribute(id: bigint | number, tribute: TributeSubmitData) : Observable<TributeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TributeData>(this.baseUrl + 'api/Tribute/' + id.toString(), tribute, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTribute(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTribute(id, tribute));
            }));
    }


    public PostTribute(tribute: TributeSubmitData) : Observable<TributeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TributeData>(this.baseUrl + 'api/Tribute', tribute, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTribute(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTribute(tribute));
            }));
    }

  
    public DeleteTribute(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Tribute/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTribute(id));
            }));
    }

    public RollbackTribute(id: bigint | number, versionNumber: bigint | number) : Observable<TributeData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TributeData>(this.baseUrl + 'api/Tribute/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTribute(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackTribute(id, versionNumber));
        }));
    }

    private getConfigHash(config: TributeQueryParameters | any): string {

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

    public userIsSchedulerTributeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerTributeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Tributes
        //
        if (userIsSchedulerTributeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerTributeReader = user.readPermission >= 0;
            } else {
                userIsSchedulerTributeReader = false;
            }
        }

        return userIsSchedulerTributeReader;
    }


    public userIsSchedulerTributeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerTributeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Tributes
        //
        if (userIsSchedulerTributeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerTributeWriter = user.writePermission >= 0;
          } else {
            userIsSchedulerTributeWriter = false;
          }      
        }

        return userIsSchedulerTributeWriter;
    }

    public GetTributeChangeHistoriesForTribute(tributeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TributeChangeHistoryData[]> {
        return this.tributeChangeHistoryService.GetTributeChangeHistoryList({
            tributeId: tributeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetGiftsForTribute(tributeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GiftData[]> {
        return this.giftService.GetGiftList({
            tributeId: tributeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full TributeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TributeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TributeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTribute(raw: any): TributeData {
    if (!raw) return raw;

    //
    // Create a TributeData object instance with correct prototype
    //
    const revived = Object.create(TributeData.prototype) as TributeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._tributeChangeHistories = null;
    (revived as any)._tributeChangeHistoriesPromise = null;
    (revived as any)._tributeChangeHistoriesSubject = new BehaviorSubject<TributeChangeHistoryData[] | null>(null);

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
    // 2. But private methods (loadTributeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).TributeChangeHistories$ = (revived as any)._tributeChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._tributeChangeHistories === null && (revived as any)._tributeChangeHistoriesPromise === null) {
                (revived as any).loadTributeChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).TributeChangeHistoriesCount$ = TributeChangeHistoryService.Instance.GetTributeChangeHistoriesRowCount({tributeId: (revived as any).id,
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

    (revived as any).GiftsCount$ = GiftService.Instance.GetGiftsRowCount({tributeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveTributeList(rawList: any[]): TributeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTribute(raw));
  }

}
