/*

   GENERATED SERVICE FOR THE SHIFTPATTERN TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ShiftPattern table.

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
import { TimeZoneData } from './time-zone.service';
import { ShiftPatternChangeHistoryService, ShiftPatternChangeHistoryData } from './shift-pattern-change-history.service';
import { ShiftPatternDayService, ShiftPatternDayData } from './shift-pattern-day.service';
import { ResourceService, ResourceData } from './resource.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ShiftPatternQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    timeZoneId: bigint | number | null | undefined = null;
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
export class ShiftPatternSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    timeZoneId: bigint | number | null = null;
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

export class ShiftPatternBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ShiftPatternChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `shiftPattern.ShiftPatternChildren$` — use with `| async` in templates
//        • Promise:    `shiftPattern.ShiftPatternChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="shiftPattern.ShiftPatternChildren$ | async"`), or
//        • Access the promise getter (`shiftPattern.ShiftPatternChildren` or `await shiftPattern.ShiftPatternChildren`)
//    - Simply reading `shiftPattern.ShiftPatternChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await shiftPattern.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ShiftPatternData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    timeZoneId!: bigint | number;
    color!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    timeZone: TimeZoneData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _shiftPatternChangeHistories: ShiftPatternChangeHistoryData[] | null = null;
    private _shiftPatternChangeHistoriesPromise: Promise<ShiftPatternChangeHistoryData[]> | null  = null;
    private _shiftPatternChangeHistoriesSubject = new BehaviorSubject<ShiftPatternChangeHistoryData[] | null>(null);

                
    private _shiftPatternDays: ShiftPatternDayData[] | null = null;
    private _shiftPatternDaysPromise: Promise<ShiftPatternDayData[]> | null  = null;
    private _shiftPatternDaysSubject = new BehaviorSubject<ShiftPatternDayData[] | null>(null);

                
    private _resources: ResourceData[] | null = null;
    private _resourcesPromise: Promise<ResourceData[]> | null  = null;
    private _resourcesSubject = new BehaviorSubject<ResourceData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ShiftPatternData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ShiftPatternData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ShiftPatternData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ShiftPatternChangeHistories$ = this._shiftPatternChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._shiftPatternChangeHistories === null && this._shiftPatternChangeHistoriesPromise === null) {
            this.loadShiftPatternChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ShiftPatternChangeHistoriesCount$ = ShiftPatternChangeHistoryService.Instance.GetShiftPatternChangeHistoriesRowCount({shiftPatternId: this.id,
      active: true,
      deleted: false
    });



    public ShiftPatternDays$ = this._shiftPatternDaysSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._shiftPatternDays === null && this._shiftPatternDaysPromise === null) {
            this.loadShiftPatternDays(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ShiftPatternDaysCount$ = ShiftPatternDayService.Instance.GetShiftPatternDaysRowCount({shiftPatternId: this.id,
      active: true,
      deleted: false
    });



    public Resources$ = this._resourcesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resources === null && this._resourcesPromise === null) {
            this.loadResources(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ResourcesCount$ = ResourceService.Instance.GetResourcesRowCount({shiftPatternId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ShiftPatternData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.shiftPattern.Reload();
  //
  //  Non Async:
  //
  //     shiftPattern[0].Reload().then(x => {
  //        this.shiftPattern = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ShiftPatternService.Instance.GetShiftPattern(this.id, includeRelations)
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
     this._shiftPatternChangeHistories = null;
     this._shiftPatternChangeHistoriesPromise = null;
     this._shiftPatternChangeHistoriesSubject.next(null);

     this._shiftPatternDays = null;
     this._shiftPatternDaysPromise = null;
     this._shiftPatternDaysSubject.next(null);

     this._resources = null;
     this._resourcesPromise = null;
     this._resourcesSubject.next(null);

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
     * Gets the ShiftPatternChangeHistories for this ShiftPattern.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.shiftPattern.ShiftPatternChangeHistories.then(shiftPatterns => { ... })
     *   or
     *   await this.shiftPattern.shiftPatterns
     *
    */
    public get ShiftPatternChangeHistories(): Promise<ShiftPatternChangeHistoryData[]> {
        if (this._shiftPatternChangeHistories !== null) {
            return Promise.resolve(this._shiftPatternChangeHistories);
        }

        if (this._shiftPatternChangeHistoriesPromise !== null) {
            return this._shiftPatternChangeHistoriesPromise;
        }

        // Start the load
        this.loadShiftPatternChangeHistories();

        return this._shiftPatternChangeHistoriesPromise!;
    }



    private loadShiftPatternChangeHistories(): void {

        this._shiftPatternChangeHistoriesPromise = lastValueFrom(
            ShiftPatternService.Instance.GetShiftPatternChangeHistoriesForShiftPattern(this.id)
        )
        .then(ShiftPatternChangeHistories => {
            this._shiftPatternChangeHistories = ShiftPatternChangeHistories ?? [];
            this._shiftPatternChangeHistoriesSubject.next(this._shiftPatternChangeHistories);
            return this._shiftPatternChangeHistories;
         })
        .catch(err => {
            this._shiftPatternChangeHistories = [];
            this._shiftPatternChangeHistoriesSubject.next(this._shiftPatternChangeHistories);
            throw err;
        })
        .finally(() => {
            this._shiftPatternChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ShiftPatternChangeHistory. Call after mutations to force refresh.
     */
    public ClearShiftPatternChangeHistoriesCache(): void {
        this._shiftPatternChangeHistories = null;
        this._shiftPatternChangeHistoriesPromise = null;
        this._shiftPatternChangeHistoriesSubject.next(this._shiftPatternChangeHistories);      // Emit to observable
    }

    public get HasShiftPatternChangeHistories(): Promise<boolean> {
        return this.ShiftPatternChangeHistories.then(shiftPatternChangeHistories => shiftPatternChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ShiftPatternDays for this ShiftPattern.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.shiftPattern.ShiftPatternDays.then(shiftPatterns => { ... })
     *   or
     *   await this.shiftPattern.shiftPatterns
     *
    */
    public get ShiftPatternDays(): Promise<ShiftPatternDayData[]> {
        if (this._shiftPatternDays !== null) {
            return Promise.resolve(this._shiftPatternDays);
        }

        if (this._shiftPatternDaysPromise !== null) {
            return this._shiftPatternDaysPromise;
        }

        // Start the load
        this.loadShiftPatternDays();

        return this._shiftPatternDaysPromise!;
    }



    private loadShiftPatternDays(): void {

        this._shiftPatternDaysPromise = lastValueFrom(
            ShiftPatternService.Instance.GetShiftPatternDaysForShiftPattern(this.id)
        )
        .then(ShiftPatternDays => {
            this._shiftPatternDays = ShiftPatternDays ?? [];
            this._shiftPatternDaysSubject.next(this._shiftPatternDays);
            return this._shiftPatternDays;
         })
        .catch(err => {
            this._shiftPatternDays = [];
            this._shiftPatternDaysSubject.next(this._shiftPatternDays);
            throw err;
        })
        .finally(() => {
            this._shiftPatternDaysPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ShiftPatternDay. Call after mutations to force refresh.
     */
    public ClearShiftPatternDaysCache(): void {
        this._shiftPatternDays = null;
        this._shiftPatternDaysPromise = null;
        this._shiftPatternDaysSubject.next(this._shiftPatternDays);      // Emit to observable
    }

    public get HasShiftPatternDays(): Promise<boolean> {
        return this.ShiftPatternDays.then(shiftPatternDays => shiftPatternDays.length > 0);
    }


    /**
     *
     * Gets the Resources for this ShiftPattern.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.shiftPattern.Resources.then(shiftPatterns => { ... })
     *   or
     *   await this.shiftPattern.shiftPatterns
     *
    */
    public get Resources(): Promise<ResourceData[]> {
        if (this._resources !== null) {
            return Promise.resolve(this._resources);
        }

        if (this._resourcesPromise !== null) {
            return this._resourcesPromise;
        }

        // Start the load
        this.loadResources();

        return this._resourcesPromise!;
    }



    private loadResources(): void {

        this._resourcesPromise = lastValueFrom(
            ShiftPatternService.Instance.GetResourcesForShiftPattern(this.id)
        )
        .then(Resources => {
            this._resources = Resources ?? [];
            this._resourcesSubject.next(this._resources);
            return this._resources;
         })
        .catch(err => {
            this._resources = [];
            this._resourcesSubject.next(this._resources);
            throw err;
        })
        .finally(() => {
            this._resourcesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Resource. Call after mutations to force refresh.
     */
    public ClearResourcesCache(): void {
        this._resources = null;
        this._resourcesPromise = null;
        this._resourcesSubject.next(this._resources);      // Emit to observable
    }

    public get HasResources(): Promise<boolean> {
        return this.Resources.then(resources => resources.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (shiftPattern.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await shiftPattern.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ShiftPatternData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ShiftPatternData>> {
        const info = await lastValueFrom(
            ShiftPatternService.Instance.GetShiftPatternChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ShiftPatternData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ShiftPatternData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ShiftPatternSubmitData {
        return ShiftPatternService.Instance.ConvertToShiftPatternSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ShiftPatternService extends SecureEndpointBase {

    private static _instance: ShiftPatternService;
    private listCache: Map<string, Observable<Array<ShiftPatternData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ShiftPatternBasicListData>>>;
    private recordCache: Map<string, Observable<ShiftPatternData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private shiftPatternChangeHistoryService: ShiftPatternChangeHistoryService,
        private shiftPatternDayService: ShiftPatternDayService,
        private resourceService: ResourceService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ShiftPatternData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ShiftPatternBasicListData>>>();
        this.recordCache = new Map<string, Observable<ShiftPatternData>>();

        ShiftPatternService._instance = this;
    }

    public static get Instance(): ShiftPatternService {
      return ShiftPatternService._instance;
    }


    public ClearListCaches(config: ShiftPatternQueryParameters | null = null) {

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


    public ConvertToShiftPatternSubmitData(data: ShiftPatternData): ShiftPatternSubmitData {

        let output = new ShiftPatternSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.timeZoneId = data.timeZoneId;
        output.color = data.color;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetShiftPattern(id: bigint | number, includeRelations: boolean = true) : Observable<ShiftPatternData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const shiftPattern$ = this.requestShiftPattern(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ShiftPattern", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, shiftPattern$);

            return shiftPattern$;
        }

        return this.recordCache.get(configHash) as Observable<ShiftPatternData>;
    }

    private requestShiftPattern(id: bigint | number, includeRelations: boolean = true) : Observable<ShiftPatternData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ShiftPatternData>(this.baseUrl + 'api/ShiftPattern/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveShiftPattern(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestShiftPattern(id, includeRelations));
            }));
    }

    public GetShiftPatternList(config: ShiftPatternQueryParameters | any = null) : Observable<Array<ShiftPatternData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const shiftPatternList$ = this.requestShiftPatternList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ShiftPattern list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, shiftPatternList$);

            return shiftPatternList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ShiftPatternData>>;
    }


    private requestShiftPatternList(config: ShiftPatternQueryParameters | any) : Observable <Array<ShiftPatternData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ShiftPatternData>>(this.baseUrl + 'api/ShiftPatterns', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveShiftPatternList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestShiftPatternList(config));
            }));
    }

    public GetShiftPatternsRowCount(config: ShiftPatternQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const shiftPatternsRowCount$ = this.requestShiftPatternsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ShiftPatterns row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, shiftPatternsRowCount$);

            return shiftPatternsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestShiftPatternsRowCount(config: ShiftPatternQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ShiftPatterns/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestShiftPatternsRowCount(config));
            }));
    }

    public GetShiftPatternsBasicListData(config: ShiftPatternQueryParameters | any = null) : Observable<Array<ShiftPatternBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const shiftPatternsBasicListData$ = this.requestShiftPatternsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ShiftPatterns basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, shiftPatternsBasicListData$);

            return shiftPatternsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ShiftPatternBasicListData>>;
    }


    private requestShiftPatternsBasicListData(config: ShiftPatternQueryParameters | any) : Observable<Array<ShiftPatternBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ShiftPatternBasicListData>>(this.baseUrl + 'api/ShiftPatterns/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestShiftPatternsBasicListData(config));
            }));

    }


    public PutShiftPattern(id: bigint | number, shiftPattern: ShiftPatternSubmitData) : Observable<ShiftPatternData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ShiftPatternData>(this.baseUrl + 'api/ShiftPattern/' + id.toString(), shiftPattern, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveShiftPattern(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutShiftPattern(id, shiftPattern));
            }));
    }


    public PostShiftPattern(shiftPattern: ShiftPatternSubmitData) : Observable<ShiftPatternData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ShiftPatternData>(this.baseUrl + 'api/ShiftPattern', shiftPattern, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveShiftPattern(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostShiftPattern(shiftPattern));
            }));
    }

  
    public DeleteShiftPattern(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ShiftPattern/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteShiftPattern(id));
            }));
    }

    public RollbackShiftPattern(id: bigint | number, versionNumber: bigint | number) : Observable<ShiftPatternData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ShiftPatternData>(this.baseUrl + 'api/ShiftPattern/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveShiftPattern(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackShiftPattern(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ShiftPattern.
     */
    public GetShiftPatternChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ShiftPatternData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ShiftPatternData>>(this.baseUrl + 'api/ShiftPattern/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetShiftPatternChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ShiftPattern.
     */
    public GetShiftPatternAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ShiftPatternData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ShiftPatternData>[]>(this.baseUrl + 'api/ShiftPattern/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetShiftPatternAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ShiftPattern.
     */
    public GetShiftPatternVersion(id: bigint | number, version: number): Observable<ShiftPatternData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ShiftPatternData>(this.baseUrl + 'api/ShiftPattern/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveShiftPattern(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetShiftPatternVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ShiftPattern at a specific point in time.
     */
    public GetShiftPatternStateAtTime(id: bigint | number, time: string): Observable<ShiftPatternData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ShiftPatternData>(this.baseUrl + 'api/ShiftPattern/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveShiftPattern(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetShiftPatternStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ShiftPatternQueryParameters | any): string {

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

    public userIsSchedulerShiftPatternReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerShiftPatternReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ShiftPatterns
        //
        if (userIsSchedulerShiftPatternReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerShiftPatternReader = user.readPermission >= 1;
            } else {
                userIsSchedulerShiftPatternReader = false;
            }
        }

        return userIsSchedulerShiftPatternReader;
    }


    public userIsSchedulerShiftPatternWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerShiftPatternWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ShiftPatterns
        //
        if (userIsSchedulerShiftPatternWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerShiftPatternWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerShiftPatternWriter = false;
          }      
        }

        return userIsSchedulerShiftPatternWriter;
    }

    public GetShiftPatternChangeHistoriesForShiftPattern(shiftPatternId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ShiftPatternChangeHistoryData[]> {
        return this.shiftPatternChangeHistoryService.GetShiftPatternChangeHistoryList({
            shiftPatternId: shiftPatternId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetShiftPatternDaysForShiftPattern(shiftPatternId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ShiftPatternDayData[]> {
        return this.shiftPatternDayService.GetShiftPatternDayList({
            shiftPatternId: shiftPatternId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetResourcesForShiftPattern(shiftPatternId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceData[]> {
        return this.resourceService.GetResourceList({
            shiftPatternId: shiftPatternId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ShiftPatternData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ShiftPatternData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ShiftPatternTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveShiftPattern(raw: any): ShiftPatternData {
    if (!raw) return raw;

    //
    // Create a ShiftPatternData object instance with correct prototype
    //
    const revived = Object.create(ShiftPatternData.prototype) as ShiftPatternData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._shiftPatternChangeHistories = null;
    (revived as any)._shiftPatternChangeHistoriesPromise = null;
    (revived as any)._shiftPatternChangeHistoriesSubject = new BehaviorSubject<ShiftPatternChangeHistoryData[] | null>(null);

    (revived as any)._shiftPatternDays = null;
    (revived as any)._shiftPatternDaysPromise = null;
    (revived as any)._shiftPatternDaysSubject = new BehaviorSubject<ShiftPatternDayData[] | null>(null);

    (revived as any)._resources = null;
    (revived as any)._resourcesPromise = null;
    (revived as any)._resourcesSubject = new BehaviorSubject<ResourceData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadShiftPatternXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ShiftPatternChangeHistories$ = (revived as any)._shiftPatternChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._shiftPatternChangeHistories === null && (revived as any)._shiftPatternChangeHistoriesPromise === null) {
                (revived as any).loadShiftPatternChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ShiftPatternChangeHistoriesCount$ = ShiftPatternChangeHistoryService.Instance.GetShiftPatternChangeHistoriesRowCount({shiftPatternId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ShiftPatternDays$ = (revived as any)._shiftPatternDaysSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._shiftPatternDays === null && (revived as any)._shiftPatternDaysPromise === null) {
                (revived as any).loadShiftPatternDays();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ShiftPatternDaysCount$ = ShiftPatternDayService.Instance.GetShiftPatternDaysRowCount({shiftPatternId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Resources$ = (revived as any)._resourcesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resources === null && (revived as any)._resourcesPromise === null) {
                (revived as any).loadResources();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ResourcesCount$ = ResourceService.Instance.GetResourcesRowCount({shiftPatternId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveShiftPatternList(rawList: any[]): ShiftPatternData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveShiftPattern(raw));
  }

}
