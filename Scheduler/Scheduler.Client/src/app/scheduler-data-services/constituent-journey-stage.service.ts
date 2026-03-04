/*

   GENERATED SERVICE FOR THE CONSTITUENTJOURNEYSTAGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ConstituentJourneyStage table.

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
import { ConstituentJourneyStageChangeHistoryService, ConstituentJourneyStageChangeHistoryData } from './constituent-journey-stage-change-history.service';
import { ConstituentService, ConstituentData } from './constituent.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ConstituentJourneyStageQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
    minLifetimeGiving: number | null | undefined = null;
    maxLifetimeGiving: number | null | undefined = null;
    minSingleGiftAmount: number | null | undefined = null;
    isDefault: boolean | null | undefined = null;
    minAnnualGiving: number | null | undefined = null;
    maxDaysSinceLastGift: bigint | number | null | undefined = null;
    minGiftCount: bigint | number | null | undefined = null;
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
export class ConstituentJourneyStageSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    sequence: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    minLifetimeGiving: number | null = null;
    maxLifetimeGiving: number | null = null;
    minSingleGiftAmount: number | null = null;
    isDefault!: boolean;
    minAnnualGiving: number | null = null;
    maxDaysSinceLastGift: bigint | number | null = null;
    minGiftCount: bigint | number | null = null;
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

export class ConstituentJourneyStageBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ConstituentJourneyStageChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `constituentJourneyStage.ConstituentJourneyStageChildren$` — use with `| async` in templates
//        • Promise:    `constituentJourneyStage.ConstituentJourneyStageChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="constituentJourneyStage.ConstituentJourneyStageChildren$ | async"`), or
//        • Access the promise getter (`constituentJourneyStage.ConstituentJourneyStageChildren` or `await constituentJourneyStage.ConstituentJourneyStageChildren`)
//    - Simply reading `constituentJourneyStage.ConstituentJourneyStageChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await constituentJourneyStage.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ConstituentJourneyStageData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    sequence!: bigint | number;
    iconId!: bigint | number;
    color!: string | null;
    minLifetimeGiving!: number | null;
    maxLifetimeGiving!: number | null;
    minSingleGiftAmount!: number | null;
    isDefault!: boolean;
    minAnnualGiving!: number | null;
    maxDaysSinceLastGift!: bigint | number;
    minGiftCount!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _constituentJourneyStageChangeHistories: ConstituentJourneyStageChangeHistoryData[] | null = null;
    private _constituentJourneyStageChangeHistoriesPromise: Promise<ConstituentJourneyStageChangeHistoryData[]> | null  = null;
    private _constituentJourneyStageChangeHistoriesSubject = new BehaviorSubject<ConstituentJourneyStageChangeHistoryData[] | null>(null);

                
    private _constituents: ConstituentData[] | null = null;
    private _constituentsPromise: Promise<ConstituentData[]> | null  = null;
    private _constituentsSubject = new BehaviorSubject<ConstituentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ConstituentJourneyStageData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ConstituentJourneyStageData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ConstituentJourneyStageData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ConstituentJourneyStageChangeHistories$ = this._constituentJourneyStageChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._constituentJourneyStageChangeHistories === null && this._constituentJourneyStageChangeHistoriesPromise === null) {
            this.loadConstituentJourneyStageChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _constituentJourneyStageChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ConstituentJourneyStageChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._constituentJourneyStageChangeHistoriesCount$ === null) {
            this._constituentJourneyStageChangeHistoriesCount$ = ConstituentJourneyStageChangeHistoryService.Instance.GetConstituentJourneyStageChangeHistoriesRowCount({constituentJourneyStageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._constituentJourneyStageChangeHistoriesCount$;
    }



    public Constituents$ = this._constituentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._constituents === null && this._constituentsPromise === null) {
            this.loadConstituents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _constituentsCount$: Observable<bigint | number> | null = null;
    public get ConstituentsCount$(): Observable<bigint | number> {
        if (this._constituentsCount$ === null) {
            this._constituentsCount$ = ConstituentService.Instance.GetConstituentsRowCount({constituentJourneyStageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._constituentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ConstituentJourneyStageData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.constituentJourneyStage.Reload();
  //
  //  Non Async:
  //
  //     constituentJourneyStage[0].Reload().then(x => {
  //        this.constituentJourneyStage = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ConstituentJourneyStageService.Instance.GetConstituentJourneyStage(this.id, includeRelations)
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
     this._constituentJourneyStageChangeHistories = null;
     this._constituentJourneyStageChangeHistoriesPromise = null;
     this._constituentJourneyStageChangeHistoriesSubject.next(null);
     this._constituentJourneyStageChangeHistoriesCount$ = null;

     this._constituents = null;
     this._constituentsPromise = null;
     this._constituentsSubject.next(null);
     this._constituentsCount$ = null;

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
     * Gets the ConstituentJourneyStageChangeHistories for this ConstituentJourneyStage.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.constituentJourneyStage.ConstituentJourneyStageChangeHistories.then(constituentJourneyStages => { ... })
     *   or
     *   await this.constituentJourneyStage.constituentJourneyStages
     *
    */
    public get ConstituentJourneyStageChangeHistories(): Promise<ConstituentJourneyStageChangeHistoryData[]> {
        if (this._constituentJourneyStageChangeHistories !== null) {
            return Promise.resolve(this._constituentJourneyStageChangeHistories);
        }

        if (this._constituentJourneyStageChangeHistoriesPromise !== null) {
            return this._constituentJourneyStageChangeHistoriesPromise;
        }

        // Start the load
        this.loadConstituentJourneyStageChangeHistories();

        return this._constituentJourneyStageChangeHistoriesPromise!;
    }



    private loadConstituentJourneyStageChangeHistories(): void {

        this._constituentJourneyStageChangeHistoriesPromise = lastValueFrom(
            ConstituentJourneyStageService.Instance.GetConstituentJourneyStageChangeHistoriesForConstituentJourneyStage(this.id)
        )
        .then(ConstituentJourneyStageChangeHistories => {
            this._constituentJourneyStageChangeHistories = ConstituentJourneyStageChangeHistories ?? [];
            this._constituentJourneyStageChangeHistoriesSubject.next(this._constituentJourneyStageChangeHistories);
            return this._constituentJourneyStageChangeHistories;
         })
        .catch(err => {
            this._constituentJourneyStageChangeHistories = [];
            this._constituentJourneyStageChangeHistoriesSubject.next(this._constituentJourneyStageChangeHistories);
            throw err;
        })
        .finally(() => {
            this._constituentJourneyStageChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConstituentJourneyStageChangeHistory. Call after mutations to force refresh.
     */
    public ClearConstituentJourneyStageChangeHistoriesCache(): void {
        this._constituentJourneyStageChangeHistories = null;
        this._constituentJourneyStageChangeHistoriesPromise = null;
        this._constituentJourneyStageChangeHistoriesSubject.next(this._constituentJourneyStageChangeHistories);      // Emit to observable
    }

    public get HasConstituentJourneyStageChangeHistories(): Promise<boolean> {
        return this.ConstituentJourneyStageChangeHistories.then(constituentJourneyStageChangeHistories => constituentJourneyStageChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Constituents for this ConstituentJourneyStage.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.constituentJourneyStage.Constituents.then(constituentJourneyStages => { ... })
     *   or
     *   await this.constituentJourneyStage.constituentJourneyStages
     *
    */
    public get Constituents(): Promise<ConstituentData[]> {
        if (this._constituents !== null) {
            return Promise.resolve(this._constituents);
        }

        if (this._constituentsPromise !== null) {
            return this._constituentsPromise;
        }

        // Start the load
        this.loadConstituents();

        return this._constituentsPromise!;
    }



    private loadConstituents(): void {

        this._constituentsPromise = lastValueFrom(
            ConstituentJourneyStageService.Instance.GetConstituentsForConstituentJourneyStage(this.id)
        )
        .then(Constituents => {
            this._constituents = Constituents ?? [];
            this._constituentsSubject.next(this._constituents);
            return this._constituents;
         })
        .catch(err => {
            this._constituents = [];
            this._constituentsSubject.next(this._constituents);
            throw err;
        })
        .finally(() => {
            this._constituentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Constituent. Call after mutations to force refresh.
     */
    public ClearConstituentsCache(): void {
        this._constituents = null;
        this._constituentsPromise = null;
        this._constituentsSubject.next(this._constituents);      // Emit to observable
    }

    public get HasConstituents(): Promise<boolean> {
        return this.Constituents.then(constituents => constituents.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (constituentJourneyStage.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await constituentJourneyStage.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ConstituentJourneyStageData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ConstituentJourneyStageData>> {
        const info = await lastValueFrom(
            ConstituentJourneyStageService.Instance.GetConstituentJourneyStageChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ConstituentJourneyStageData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ConstituentJourneyStageData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ConstituentJourneyStageSubmitData {
        return ConstituentJourneyStageService.Instance.ConvertToConstituentJourneyStageSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ConstituentJourneyStageService extends SecureEndpointBase {

    private static _instance: ConstituentJourneyStageService;
    private listCache: Map<string, Observable<Array<ConstituentJourneyStageData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ConstituentJourneyStageBasicListData>>>;
    private recordCache: Map<string, Observable<ConstituentJourneyStageData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private constituentJourneyStageChangeHistoryService: ConstituentJourneyStageChangeHistoryService,
        private constituentService: ConstituentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ConstituentJourneyStageData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ConstituentJourneyStageBasicListData>>>();
        this.recordCache = new Map<string, Observable<ConstituentJourneyStageData>>();

        ConstituentJourneyStageService._instance = this;
    }

    public static get Instance(): ConstituentJourneyStageService {
      return ConstituentJourneyStageService._instance;
    }


    public ClearListCaches(config: ConstituentJourneyStageQueryParameters | null = null) {

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


    public ConvertToConstituentJourneyStageSubmitData(data: ConstituentJourneyStageData): ConstituentJourneyStageSubmitData {

        let output = new ConstituentJourneyStageSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.iconId = data.iconId;
        output.color = data.color;
        output.minLifetimeGiving = data.minLifetimeGiving;
        output.maxLifetimeGiving = data.maxLifetimeGiving;
        output.minSingleGiftAmount = data.minSingleGiftAmount;
        output.isDefault = data.isDefault;
        output.minAnnualGiving = data.minAnnualGiving;
        output.maxDaysSinceLastGift = data.maxDaysSinceLastGift;
        output.minGiftCount = data.minGiftCount;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetConstituentJourneyStage(id: bigint | number, includeRelations: boolean = true) : Observable<ConstituentJourneyStageData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const constituentJourneyStage$ = this.requestConstituentJourneyStage(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConstituentJourneyStage", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, constituentJourneyStage$);

            return constituentJourneyStage$;
        }

        return this.recordCache.get(configHash) as Observable<ConstituentJourneyStageData>;
    }

    private requestConstituentJourneyStage(id: bigint | number, includeRelations: boolean = true) : Observable<ConstituentJourneyStageData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConstituentJourneyStageData>(this.baseUrl + 'api/ConstituentJourneyStage/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveConstituentJourneyStage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestConstituentJourneyStage(id, includeRelations));
            }));
    }

    public GetConstituentJourneyStageList(config: ConstituentJourneyStageQueryParameters | any = null) : Observable<Array<ConstituentJourneyStageData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const constituentJourneyStageList$ = this.requestConstituentJourneyStageList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConstituentJourneyStage list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, constituentJourneyStageList$);

            return constituentJourneyStageList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ConstituentJourneyStageData>>;
    }


    private requestConstituentJourneyStageList(config: ConstituentJourneyStageQueryParameters | any) : Observable <Array<ConstituentJourneyStageData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConstituentJourneyStageData>>(this.baseUrl + 'api/ConstituentJourneyStages', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveConstituentJourneyStageList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestConstituentJourneyStageList(config));
            }));
    }

    public GetConstituentJourneyStagesRowCount(config: ConstituentJourneyStageQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const constituentJourneyStagesRowCount$ = this.requestConstituentJourneyStagesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConstituentJourneyStages row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, constituentJourneyStagesRowCount$);

            return constituentJourneyStagesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestConstituentJourneyStagesRowCount(config: ConstituentJourneyStageQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ConstituentJourneyStages/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConstituentJourneyStagesRowCount(config));
            }));
    }

    public GetConstituentJourneyStagesBasicListData(config: ConstituentJourneyStageQueryParameters | any = null) : Observable<Array<ConstituentJourneyStageBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const constituentJourneyStagesBasicListData$ = this.requestConstituentJourneyStagesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConstituentJourneyStages basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, constituentJourneyStagesBasicListData$);

            return constituentJourneyStagesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ConstituentJourneyStageBasicListData>>;
    }


    private requestConstituentJourneyStagesBasicListData(config: ConstituentJourneyStageQueryParameters | any) : Observable<Array<ConstituentJourneyStageBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConstituentJourneyStageBasicListData>>(this.baseUrl + 'api/ConstituentJourneyStages/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConstituentJourneyStagesBasicListData(config));
            }));

    }


    public PutConstituentJourneyStage(id: bigint | number, constituentJourneyStage: ConstituentJourneyStageSubmitData) : Observable<ConstituentJourneyStageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConstituentJourneyStageData>(this.baseUrl + 'api/ConstituentJourneyStage/' + id.toString(), constituentJourneyStage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConstituentJourneyStage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutConstituentJourneyStage(id, constituentJourneyStage));
            }));
    }


    public PostConstituentJourneyStage(constituentJourneyStage: ConstituentJourneyStageSubmitData) : Observable<ConstituentJourneyStageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ConstituentJourneyStageData>(this.baseUrl + 'api/ConstituentJourneyStage', constituentJourneyStage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConstituentJourneyStage(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostConstituentJourneyStage(constituentJourneyStage));
            }));
    }

  
    public DeleteConstituentJourneyStage(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ConstituentJourneyStage/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteConstituentJourneyStage(id));
            }));
    }

    public RollbackConstituentJourneyStage(id: bigint | number, versionNumber: bigint | number) : Observable<ConstituentJourneyStageData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConstituentJourneyStageData>(this.baseUrl + 'api/ConstituentJourneyStage/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConstituentJourneyStage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackConstituentJourneyStage(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ConstituentJourneyStage.
     */
    public GetConstituentJourneyStageChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ConstituentJourneyStageData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ConstituentJourneyStageData>>(this.baseUrl + 'api/ConstituentJourneyStage/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetConstituentJourneyStageChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ConstituentJourneyStage.
     */
    public GetConstituentJourneyStageAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ConstituentJourneyStageData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ConstituentJourneyStageData>[]>(this.baseUrl + 'api/ConstituentJourneyStage/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetConstituentJourneyStageAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ConstituentJourneyStage.
     */
    public GetConstituentJourneyStageVersion(id: bigint | number, version: number): Observable<ConstituentJourneyStageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConstituentJourneyStageData>(this.baseUrl + 'api/ConstituentJourneyStage/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveConstituentJourneyStage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetConstituentJourneyStageVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ConstituentJourneyStage at a specific point in time.
     */
    public GetConstituentJourneyStageStateAtTime(id: bigint | number, time: string): Observable<ConstituentJourneyStageData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConstituentJourneyStageData>(this.baseUrl + 'api/ConstituentJourneyStage/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveConstituentJourneyStage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetConstituentJourneyStageStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ConstituentJourneyStageQueryParameters | any): string {

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

    public userIsSchedulerConstituentJourneyStageReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerConstituentJourneyStageReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ConstituentJourneyStages
        //
        if (userIsSchedulerConstituentJourneyStageReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerConstituentJourneyStageReader = user.readPermission >= 1;
            } else {
                userIsSchedulerConstituentJourneyStageReader = false;
            }
        }

        return userIsSchedulerConstituentJourneyStageReader;
    }


    public userIsSchedulerConstituentJourneyStageWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerConstituentJourneyStageWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ConstituentJourneyStages
        //
        if (userIsSchedulerConstituentJourneyStageWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerConstituentJourneyStageWriter = user.writePermission >= 60;
          } else {
            userIsSchedulerConstituentJourneyStageWriter = false;
          }      
        }

        return userIsSchedulerConstituentJourneyStageWriter;
    }

    public GetConstituentJourneyStageChangeHistoriesForConstituentJourneyStage(constituentJourneyStageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConstituentJourneyStageChangeHistoryData[]> {
        return this.constituentJourneyStageChangeHistoryService.GetConstituentJourneyStageChangeHistoryList({
            constituentJourneyStageId: constituentJourneyStageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConstituentsForConstituentJourneyStage(constituentJourneyStageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConstituentData[]> {
        return this.constituentService.GetConstituentList({
            constituentJourneyStageId: constituentJourneyStageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ConstituentJourneyStageData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ConstituentJourneyStageData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ConstituentJourneyStageTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveConstituentJourneyStage(raw: any): ConstituentJourneyStageData {
    if (!raw) return raw;

    //
    // Create a ConstituentJourneyStageData object instance with correct prototype
    //
    const revived = Object.create(ConstituentJourneyStageData.prototype) as ConstituentJourneyStageData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._constituentJourneyStageChangeHistories = null;
    (revived as any)._constituentJourneyStageChangeHistoriesPromise = null;
    (revived as any)._constituentJourneyStageChangeHistoriesSubject = new BehaviorSubject<ConstituentJourneyStageChangeHistoryData[] | null>(null);

    (revived as any)._constituents = null;
    (revived as any)._constituentsPromise = null;
    (revived as any)._constituentsSubject = new BehaviorSubject<ConstituentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadConstituentJourneyStageXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ConstituentJourneyStageChangeHistories$ = (revived as any)._constituentJourneyStageChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._constituentJourneyStageChangeHistories === null && (revived as any)._constituentJourneyStageChangeHistoriesPromise === null) {
                (revived as any).loadConstituentJourneyStageChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._constituentJourneyStageChangeHistoriesCount$ = null;


    (revived as any).Constituents$ = (revived as any)._constituentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._constituents === null && (revived as any)._constituentsPromise === null) {
                (revived as any).loadConstituents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._constituentsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ConstituentJourneyStageData> | null>(null);

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

  private ReviveConstituentJourneyStageList(rawList: any[]): ConstituentJourneyStageData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveConstituentJourneyStage(raw));
  }

}
