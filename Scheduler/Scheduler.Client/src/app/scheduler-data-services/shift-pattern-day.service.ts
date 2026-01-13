/*

   GENERATED SERVICE FOR THE SHIFTPATTERNDAY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ShiftPatternDay table.

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
import { ShiftPatternData } from './shift-pattern.service';
import { ShiftPatternDayChangeHistoryService, ShiftPatternDayChangeHistoryData } from './shift-pattern-day-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ShiftPatternDayQueryParameters {
    shiftPatternId: bigint | number | null | undefined = null;
    dayOfWeek: bigint | number | null | undefined = null;
    startTime: string | null | undefined = null;        // ISO 8601
    hours: number | null | undefined = null;
    label: string | null | undefined = null;
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
export class ShiftPatternDaySubmitData {
    id!: bigint | number;
    shiftPatternId!: bigint | number;
    dayOfWeek!: bigint | number;
    startTime!: string;      // ISO 8601
    hours!: number;
    label: string | null = null;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class ShiftPatternDayBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ShiftPatternDayChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `shiftPatternDay.ShiftPatternDayChildren$` — use with `| async` in templates
//        • Promise:    `shiftPatternDay.ShiftPatternDayChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="shiftPatternDay.ShiftPatternDayChildren$ | async"`), or
//        • Access the promise getter (`shiftPatternDay.ShiftPatternDayChildren` or `await shiftPatternDay.ShiftPatternDayChildren`)
//    - Simply reading `shiftPatternDay.ShiftPatternDayChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await shiftPatternDay.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ShiftPatternDayData {
    id!: bigint | number;
    shiftPatternId!: bigint | number;
    dayOfWeek!: bigint | number;
    startTime!: string;      // ISO 8601
    hours!: number;
    label!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    shiftPattern: ShiftPatternData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _shiftPatternDayChangeHistories: ShiftPatternDayChangeHistoryData[] | null = null;
    private _shiftPatternDayChangeHistoriesPromise: Promise<ShiftPatternDayChangeHistoryData[]> | null  = null;
    private _shiftPatternDayChangeHistoriesSubject = new BehaviorSubject<ShiftPatternDayChangeHistoryData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ShiftPatternDayChangeHistories$ = this._shiftPatternDayChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._shiftPatternDayChangeHistories === null && this._shiftPatternDayChangeHistoriesPromise === null) {
            this.loadShiftPatternDayChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ShiftPatternDayChangeHistoriesCount$ = ShiftPatternDayChangeHistoryService.Instance.GetShiftPatternDayChangeHistoriesRowCount({shiftPatternDayId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ShiftPatternDayData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.shiftPatternDay.Reload();
  //
  //  Non Async:
  //
  //     shiftPatternDay[0].Reload().then(x => {
  //        this.shiftPatternDay = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ShiftPatternDayService.Instance.GetShiftPatternDay(this.id, includeRelations)
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
     this._shiftPatternDayChangeHistories = null;
     this._shiftPatternDayChangeHistoriesPromise = null;
     this._shiftPatternDayChangeHistoriesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ShiftPatternDayChangeHistories for this ShiftPatternDay.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.shiftPatternDay.ShiftPatternDayChangeHistories.then(shiftPatternDays => { ... })
     *   or
     *   await this.shiftPatternDay.shiftPatternDays
     *
    */
    public get ShiftPatternDayChangeHistories(): Promise<ShiftPatternDayChangeHistoryData[]> {
        if (this._shiftPatternDayChangeHistories !== null) {
            return Promise.resolve(this._shiftPatternDayChangeHistories);
        }

        if (this._shiftPatternDayChangeHistoriesPromise !== null) {
            return this._shiftPatternDayChangeHistoriesPromise;
        }

        // Start the load
        this.loadShiftPatternDayChangeHistories();

        return this._shiftPatternDayChangeHistoriesPromise!;
    }



    private loadShiftPatternDayChangeHistories(): void {

        this._shiftPatternDayChangeHistoriesPromise = lastValueFrom(
            ShiftPatternDayService.Instance.GetShiftPatternDayChangeHistoriesForShiftPatternDay(this.id)
        )
        .then(ShiftPatternDayChangeHistories => {
            this._shiftPatternDayChangeHistories = ShiftPatternDayChangeHistories ?? [];
            this._shiftPatternDayChangeHistoriesSubject.next(this._shiftPatternDayChangeHistories);
            return this._shiftPatternDayChangeHistories;
         })
        .catch(err => {
            this._shiftPatternDayChangeHistories = [];
            this._shiftPatternDayChangeHistoriesSubject.next(this._shiftPatternDayChangeHistories);
            throw err;
        })
        .finally(() => {
            this._shiftPatternDayChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ShiftPatternDayChangeHistory. Call after mutations to force refresh.
     */
    public ClearShiftPatternDayChangeHistoriesCache(): void {
        this._shiftPatternDayChangeHistories = null;
        this._shiftPatternDayChangeHistoriesPromise = null;
        this._shiftPatternDayChangeHistoriesSubject.next(this._shiftPatternDayChangeHistories);      // Emit to observable
    }

    public get HasShiftPatternDayChangeHistories(): Promise<boolean> {
        return this.ShiftPatternDayChangeHistories.then(shiftPatternDayChangeHistories => shiftPatternDayChangeHistories.length > 0);
    }




    /**
     * Updates the state of this ShiftPatternDayData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ShiftPatternDayData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ShiftPatternDaySubmitData {
        return ShiftPatternDayService.Instance.ConvertToShiftPatternDaySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ShiftPatternDayService extends SecureEndpointBase {

    private static _instance: ShiftPatternDayService;
    private listCache: Map<string, Observable<Array<ShiftPatternDayData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ShiftPatternDayBasicListData>>>;
    private recordCache: Map<string, Observable<ShiftPatternDayData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private shiftPatternDayChangeHistoryService: ShiftPatternDayChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ShiftPatternDayData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ShiftPatternDayBasicListData>>>();
        this.recordCache = new Map<string, Observable<ShiftPatternDayData>>();

        ShiftPatternDayService._instance = this;
    }

    public static get Instance(): ShiftPatternDayService {
      return ShiftPatternDayService._instance;
    }


    public ClearListCaches(config: ShiftPatternDayQueryParameters | null = null) {

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


    public ConvertToShiftPatternDaySubmitData(data: ShiftPatternDayData): ShiftPatternDaySubmitData {

        let output = new ShiftPatternDaySubmitData();

        output.id = data.id;
        output.shiftPatternId = data.shiftPatternId;
        output.dayOfWeek = data.dayOfWeek;
        output.startTime = data.startTime;
        output.hours = data.hours;
        output.label = data.label;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetShiftPatternDay(id: bigint | number, includeRelations: boolean = true) : Observable<ShiftPatternDayData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const shiftPatternDay$ = this.requestShiftPatternDay(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ShiftPatternDay", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, shiftPatternDay$);

            return shiftPatternDay$;
        }

        return this.recordCache.get(configHash) as Observable<ShiftPatternDayData>;
    }

    private requestShiftPatternDay(id: bigint | number, includeRelations: boolean = true) : Observable<ShiftPatternDayData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ShiftPatternDayData>(this.baseUrl + 'api/ShiftPatternDay/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveShiftPatternDay(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestShiftPatternDay(id, includeRelations));
            }));
    }

    public GetShiftPatternDayList(config: ShiftPatternDayQueryParameters | any = null) : Observable<Array<ShiftPatternDayData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const shiftPatternDayList$ = this.requestShiftPatternDayList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ShiftPatternDay list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, shiftPatternDayList$);

            return shiftPatternDayList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ShiftPatternDayData>>;
    }


    private requestShiftPatternDayList(config: ShiftPatternDayQueryParameters | any) : Observable <Array<ShiftPatternDayData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ShiftPatternDayData>>(this.baseUrl + 'api/ShiftPatternDays', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveShiftPatternDayList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestShiftPatternDayList(config));
            }));
    }

    public GetShiftPatternDaysRowCount(config: ShiftPatternDayQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const shiftPatternDaysRowCount$ = this.requestShiftPatternDaysRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ShiftPatternDays row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, shiftPatternDaysRowCount$);

            return shiftPatternDaysRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestShiftPatternDaysRowCount(config: ShiftPatternDayQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ShiftPatternDays/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestShiftPatternDaysRowCount(config));
            }));
    }

    public GetShiftPatternDaysBasicListData(config: ShiftPatternDayQueryParameters | any = null) : Observable<Array<ShiftPatternDayBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const shiftPatternDaysBasicListData$ = this.requestShiftPatternDaysBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ShiftPatternDays basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, shiftPatternDaysBasicListData$);

            return shiftPatternDaysBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ShiftPatternDayBasicListData>>;
    }


    private requestShiftPatternDaysBasicListData(config: ShiftPatternDayQueryParameters | any) : Observable<Array<ShiftPatternDayBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ShiftPatternDayBasicListData>>(this.baseUrl + 'api/ShiftPatternDays/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestShiftPatternDaysBasicListData(config));
            }));

    }


    public PutShiftPatternDay(id: bigint | number, shiftPatternDay: ShiftPatternDaySubmitData) : Observable<ShiftPatternDayData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ShiftPatternDayData>(this.baseUrl + 'api/ShiftPatternDay/' + id.toString(), shiftPatternDay, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveShiftPatternDay(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutShiftPatternDay(id, shiftPatternDay));
            }));
    }


    public PostShiftPatternDay(shiftPatternDay: ShiftPatternDaySubmitData) : Observable<ShiftPatternDayData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ShiftPatternDayData>(this.baseUrl + 'api/ShiftPatternDay', shiftPatternDay, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveShiftPatternDay(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostShiftPatternDay(shiftPatternDay));
            }));
    }

  
    public DeleteShiftPatternDay(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ShiftPatternDay/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteShiftPatternDay(id));
            }));
    }

    public RollbackShiftPatternDay(id: bigint | number, versionNumber: bigint | number) : Observable<ShiftPatternDayData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ShiftPatternDayData>(this.baseUrl + 'api/ShiftPatternDay/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveShiftPatternDay(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackShiftPatternDay(id, versionNumber));
        }));
    }

    private getConfigHash(config: ShiftPatternDayQueryParameters | any): string {

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

    public userIsSchedulerShiftPatternDayReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerShiftPatternDayReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ShiftPatternDays
        //
        if (userIsSchedulerShiftPatternDayReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerShiftPatternDayReader = user.readPermission >= 0;
            } else {
                userIsSchedulerShiftPatternDayReader = false;
            }
        }

        return userIsSchedulerShiftPatternDayReader;
    }


    public userIsSchedulerShiftPatternDayWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerShiftPatternDayWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ShiftPatternDays
        //
        if (userIsSchedulerShiftPatternDayWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerShiftPatternDayWriter = user.writePermission >= 0;
          } else {
            userIsSchedulerShiftPatternDayWriter = false;
          }      
        }

        return userIsSchedulerShiftPatternDayWriter;
    }

    public GetShiftPatternDayChangeHistoriesForShiftPatternDay(shiftPatternDayId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ShiftPatternDayChangeHistoryData[]> {
        return this.shiftPatternDayChangeHistoryService.GetShiftPatternDayChangeHistoryList({
            shiftPatternDayId: shiftPatternDayId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ShiftPatternDayData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ShiftPatternDayData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ShiftPatternDayTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveShiftPatternDay(raw: any): ShiftPatternDayData {
    if (!raw) return raw;

    //
    // Create a ShiftPatternDayData object instance with correct prototype
    //
    const revived = Object.create(ShiftPatternDayData.prototype) as ShiftPatternDayData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._shiftPatternDayChangeHistories = null;
    (revived as any)._shiftPatternDayChangeHistoriesPromise = null;
    (revived as any)._shiftPatternDayChangeHistoriesSubject = new BehaviorSubject<ShiftPatternDayChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadShiftPatternDayXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ShiftPatternDayChangeHistories$ = (revived as any)._shiftPatternDayChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._shiftPatternDayChangeHistories === null && (revived as any)._shiftPatternDayChangeHistoriesPromise === null) {
                (revived as any).loadShiftPatternDayChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ShiftPatternDayChangeHistoriesCount$ = ShiftPatternDayChangeHistoryService.Instance.GetShiftPatternDayChangeHistoriesRowCount({shiftPatternDayId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveShiftPatternDayList(rawList: any[]): ShiftPatternDayData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveShiftPatternDay(raw));
  }

}
