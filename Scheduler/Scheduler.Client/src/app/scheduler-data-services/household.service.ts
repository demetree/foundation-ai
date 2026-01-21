/*

   GENERATED SERVICE FOR THE HOUSEHOLD TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Household table.

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
import { SchedulingTargetData } from './scheduling-target.service';
import { IconData } from './icon.service';
import { HouseholdChangeHistoryService, HouseholdChangeHistoryData } from './household-change-history.service';
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
export class HouseholdQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    schedulingTargetId: bigint | number | null | undefined = null;
    formalSalutation: string | null | undefined = null;
    informalSalutation: string | null | undefined = null;
    addressee: string | null | undefined = null;
    totalHouseholdGiving: number | null | undefined = null;
    lastHouseholdGiftDate: string | null | undefined = null;        // ISO 8601
    notes: string | null | undefined = null;
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
export class HouseholdSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    schedulingTargetId: bigint | number | null = null;
    formalSalutation: string | null = null;
    informalSalutation: string | null = null;
    addressee: string | null = null;
    totalHouseholdGiving!: number;
    lastHouseholdGiftDate: string | null = null;     // ISO 8601
    notes: string | null = null;
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

export class HouseholdBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. HouseholdChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `household.HouseholdChildren$` — use with `| async` in templates
//        • Promise:    `household.HouseholdChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="household.HouseholdChildren$ | async"`), or
//        • Access the promise getter (`household.HouseholdChildren` or `await household.HouseholdChildren`)
//    - Simply reading `household.HouseholdChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await household.Reload()` to refresh the entire object and clear all lazy caches.
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
export class HouseholdData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    schedulingTargetId!: bigint | number;
    formalSalutation!: string | null;
    informalSalutation!: string | null;
    addressee!: string | null;
    totalHouseholdGiving!: number;
    lastHouseholdGiftDate!: string | null;   // ISO 8601
    notes!: string | null;
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
    schedulingTarget: SchedulingTargetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _householdChangeHistories: HouseholdChangeHistoryData[] | null = null;
    private _householdChangeHistoriesPromise: Promise<HouseholdChangeHistoryData[]> | null  = null;
    private _householdChangeHistoriesSubject = new BehaviorSubject<HouseholdChangeHistoryData[] | null>(null);

                
    private _constituents: ConstituentData[] | null = null;
    private _constituentsPromise: Promise<ConstituentData[]> | null  = null;
    private _constituentsSubject = new BehaviorSubject<ConstituentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<HouseholdData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<HouseholdData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<HouseholdData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public HouseholdChangeHistories$ = this._householdChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._householdChangeHistories === null && this._householdChangeHistoriesPromise === null) {
            this.loadHouseholdChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public HouseholdChangeHistoriesCount$ = HouseholdChangeHistoryService.Instance.GetHouseholdChangeHistoriesRowCount({householdId: this.id,
      active: true,
      deleted: false
    });



    public Constituents$ = this._constituentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._constituents === null && this._constituentsPromise === null) {
            this.loadConstituents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ConstituentsCount$ = ConstituentService.Instance.GetConstituentsRowCount({householdId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any HouseholdData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.household.Reload();
  //
  //  Non Async:
  //
  //     household[0].Reload().then(x => {
  //        this.household = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      HouseholdService.Instance.GetHousehold(this.id, includeRelations)
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
     this._householdChangeHistories = null;
     this._householdChangeHistoriesPromise = null;
     this._householdChangeHistoriesSubject.next(null);

     this._constituents = null;
     this._constituentsPromise = null;
     this._constituentsSubject.next(null);

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
     * Gets the HouseholdChangeHistories for this Household.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.household.HouseholdChangeHistories.then(households => { ... })
     *   or
     *   await this.household.households
     *
    */
    public get HouseholdChangeHistories(): Promise<HouseholdChangeHistoryData[]> {
        if (this._householdChangeHistories !== null) {
            return Promise.resolve(this._householdChangeHistories);
        }

        if (this._householdChangeHistoriesPromise !== null) {
            return this._householdChangeHistoriesPromise;
        }

        // Start the load
        this.loadHouseholdChangeHistories();

        return this._householdChangeHistoriesPromise!;
    }



    private loadHouseholdChangeHistories(): void {

        this._householdChangeHistoriesPromise = lastValueFrom(
            HouseholdService.Instance.GetHouseholdChangeHistoriesForHousehold(this.id)
        )
        .then(HouseholdChangeHistories => {
            this._householdChangeHistories = HouseholdChangeHistories ?? [];
            this._householdChangeHistoriesSubject.next(this._householdChangeHistories);
            return this._householdChangeHistories;
         })
        .catch(err => {
            this._householdChangeHistories = [];
            this._householdChangeHistoriesSubject.next(this._householdChangeHistories);
            throw err;
        })
        .finally(() => {
            this._householdChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached HouseholdChangeHistory. Call after mutations to force refresh.
     */
    public ClearHouseholdChangeHistoriesCache(): void {
        this._householdChangeHistories = null;
        this._householdChangeHistoriesPromise = null;
        this._householdChangeHistoriesSubject.next(this._householdChangeHistories);      // Emit to observable
    }

    public get HasHouseholdChangeHistories(): Promise<boolean> {
        return this.HouseholdChangeHistories.then(householdChangeHistories => householdChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Constituents for this Household.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.household.Constituents.then(households => { ... })
     *   or
     *   await this.household.households
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
            HouseholdService.Instance.GetConstituentsForHousehold(this.id)
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
    //   Template: {{ (household.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await household.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<HouseholdData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<HouseholdData>> {
        const info = await lastValueFrom(
            HouseholdService.Instance.GetHouseholdChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this HouseholdData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this HouseholdData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): HouseholdSubmitData {
        return HouseholdService.Instance.ConvertToHouseholdSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class HouseholdService extends SecureEndpointBase {

    private static _instance: HouseholdService;
    private listCache: Map<string, Observable<Array<HouseholdData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<HouseholdBasicListData>>>;
    private recordCache: Map<string, Observable<HouseholdData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private householdChangeHistoryService: HouseholdChangeHistoryService,
        private constituentService: ConstituentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<HouseholdData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<HouseholdBasicListData>>>();
        this.recordCache = new Map<string, Observable<HouseholdData>>();

        HouseholdService._instance = this;
    }

    public static get Instance(): HouseholdService {
      return HouseholdService._instance;
    }


    public ClearListCaches(config: HouseholdQueryParameters | null = null) {

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


    public ConvertToHouseholdSubmitData(data: HouseholdData): HouseholdSubmitData {

        let output = new HouseholdSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.schedulingTargetId = data.schedulingTargetId;
        output.formalSalutation = data.formalSalutation;
        output.informalSalutation = data.informalSalutation;
        output.addressee = data.addressee;
        output.totalHouseholdGiving = data.totalHouseholdGiving;
        output.lastHouseholdGiftDate = data.lastHouseholdGiftDate;
        output.notes = data.notes;
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

    public GetHousehold(id: bigint | number, includeRelations: boolean = true) : Observable<HouseholdData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const household$ = this.requestHousehold(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Household", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, household$);

            return household$;
        }

        return this.recordCache.get(configHash) as Observable<HouseholdData>;
    }

    private requestHousehold(id: bigint | number, includeRelations: boolean = true) : Observable<HouseholdData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<HouseholdData>(this.baseUrl + 'api/Household/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveHousehold(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestHousehold(id, includeRelations));
            }));
    }

    public GetHouseholdList(config: HouseholdQueryParameters | any = null) : Observable<Array<HouseholdData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const householdList$ = this.requestHouseholdList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Household list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, householdList$);

            return householdList$;
        }

        return this.listCache.get(configHash) as Observable<Array<HouseholdData>>;
    }


    private requestHouseholdList(config: HouseholdQueryParameters | any) : Observable <Array<HouseholdData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<HouseholdData>>(this.baseUrl + 'api/Households', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveHouseholdList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestHouseholdList(config));
            }));
    }

    public GetHouseholdsRowCount(config: HouseholdQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const householdsRowCount$ = this.requestHouseholdsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Households row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, householdsRowCount$);

            return householdsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestHouseholdsRowCount(config: HouseholdQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Households/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestHouseholdsRowCount(config));
            }));
    }

    public GetHouseholdsBasicListData(config: HouseholdQueryParameters | any = null) : Observable<Array<HouseholdBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const householdsBasicListData$ = this.requestHouseholdsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Households basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, householdsBasicListData$);

            return householdsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<HouseholdBasicListData>>;
    }


    private requestHouseholdsBasicListData(config: HouseholdQueryParameters | any) : Observable<Array<HouseholdBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<HouseholdBasicListData>>(this.baseUrl + 'api/Households/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestHouseholdsBasicListData(config));
            }));

    }


    public PutHousehold(id: bigint | number, household: HouseholdSubmitData) : Observable<HouseholdData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<HouseholdData>(this.baseUrl + 'api/Household/' + id.toString(), household, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveHousehold(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutHousehold(id, household));
            }));
    }


    public PostHousehold(household: HouseholdSubmitData) : Observable<HouseholdData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<HouseholdData>(this.baseUrl + 'api/Household', household, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveHousehold(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostHousehold(household));
            }));
    }

  
    public DeleteHousehold(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Household/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteHousehold(id));
            }));
    }

    public RollbackHousehold(id: bigint | number, versionNumber: bigint | number) : Observable<HouseholdData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<HouseholdData>(this.baseUrl + 'api/Household/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveHousehold(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackHousehold(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Household.
     */
    public GetHouseholdChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<HouseholdData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<HouseholdData>>(this.baseUrl + 'api/Household/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetHouseholdChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Household.
     */
    public GetHouseholdAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<HouseholdData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<HouseholdData>[]>(this.baseUrl + 'api/Household/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetHouseholdAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Household.
     */
    public GetHouseholdVersion(id: bigint | number, version: number): Observable<HouseholdData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<HouseholdData>(this.baseUrl + 'api/Household/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveHousehold(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetHouseholdVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Household at a specific point in time.
     */
    public GetHouseholdStateAtTime(id: bigint | number, time: string): Observable<HouseholdData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<HouseholdData>(this.baseUrl + 'api/Household/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveHousehold(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetHouseholdStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: HouseholdQueryParameters | any): string {

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

    public userIsSchedulerHouseholdReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerHouseholdReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Households
        //
        if (userIsSchedulerHouseholdReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerHouseholdReader = user.readPermission >= 1;
            } else {
                userIsSchedulerHouseholdReader = false;
            }
        }

        return userIsSchedulerHouseholdReader;
    }


    public userIsSchedulerHouseholdWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerHouseholdWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Households
        //
        if (userIsSchedulerHouseholdWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerHouseholdWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerHouseholdWriter = false;
          }      
        }

        return userIsSchedulerHouseholdWriter;
    }

    public GetHouseholdChangeHistoriesForHousehold(householdId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<HouseholdChangeHistoryData[]> {
        return this.householdChangeHistoryService.GetHouseholdChangeHistoryList({
            householdId: householdId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConstituentsForHousehold(householdId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConstituentData[]> {
        return this.constituentService.GetConstituentList({
            householdId: householdId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full HouseholdData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the HouseholdData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when HouseholdTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveHousehold(raw: any): HouseholdData {
    if (!raw) return raw;

    //
    // Create a HouseholdData object instance with correct prototype
    //
    const revived = Object.create(HouseholdData.prototype) as HouseholdData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._householdChangeHistories = null;
    (revived as any)._householdChangeHistoriesPromise = null;
    (revived as any)._householdChangeHistoriesSubject = new BehaviorSubject<HouseholdChangeHistoryData[] | null>(null);

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
    // 2. But private methods (loadHouseholdXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).HouseholdChangeHistories$ = (revived as any)._householdChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._householdChangeHistories === null && (revived as any)._householdChangeHistoriesPromise === null) {
                (revived as any).loadHouseholdChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).HouseholdChangeHistoriesCount$ = HouseholdChangeHistoryService.Instance.GetHouseholdChangeHistoriesRowCount({householdId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Constituents$ = (revived as any)._constituentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._constituents === null && (revived as any)._constituentsPromise === null) {
                (revived as any).loadConstituents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ConstituentsCount$ = ConstituentService.Instance.GetConstituentsRowCount({householdId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveHouseholdList(rawList: any[]): HouseholdData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveHousehold(raw));
  }

}
