/*

   GENERATED SERVICE FOR THE RATESHEETCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the RateSheetChangeHistory table.

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
import { RateSheetData } from './rate-sheet.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class RateSheetChangeHistoryQueryParameters {
    rateSheetId: bigint | number | null | undefined = null;
    versionNumber: bigint | number | null | undefined = null;
    timeStamp: string | null | undefined = null;        // ISO 8601 (full datetime)
    userId: bigint | number | null | undefined = null;
    data: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class RateSheetChangeHistorySubmitData {
    id!: bigint | number;
    rateSheetId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
}


export class RateSheetChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. RateSheetChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `rateSheetChangeHistory.RateSheetChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `rateSheetChangeHistory.RateSheetChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="rateSheetChangeHistory.RateSheetChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`rateSheetChangeHistory.RateSheetChangeHistoryChildren` or `await rateSheetChangeHistory.RateSheetChangeHistoryChildren`)
//    - Simply reading `rateSheetChangeHistory.RateSheetChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await rateSheetChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class RateSheetChangeHistoryData {
    id!: bigint | number;
    rateSheetId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
    rateSheet: RateSheetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //

  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any RateSheetChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.rateSheetChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     rateSheetChangeHistory[0].Reload().then(x => {
  //        this.rateSheetChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      RateSheetChangeHistoryService.Instance.GetRateSheetChangeHistory(this.id, includeRelations)
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
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //


    /**
     * Updates the state of this RateSheetChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this RateSheetChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): RateSheetChangeHistorySubmitData {
        return RateSheetChangeHistoryService.Instance.ConvertToRateSheetChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class RateSheetChangeHistoryService extends SecureEndpointBase {

    private static _instance: RateSheetChangeHistoryService;
    private listCache: Map<string, Observable<Array<RateSheetChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<RateSheetChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<RateSheetChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<RateSheetChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<RateSheetChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<RateSheetChangeHistoryData>>();

        RateSheetChangeHistoryService._instance = this;
    }

    public static get Instance(): RateSheetChangeHistoryService {
      return RateSheetChangeHistoryService._instance;
    }


    public ClearListCaches(config: RateSheetChangeHistoryQueryParameters | null = null) {

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


    public ConvertToRateSheetChangeHistorySubmitData(data: RateSheetChangeHistoryData): RateSheetChangeHistorySubmitData {

        let output = new RateSheetChangeHistorySubmitData();

        output.id = data.id;
        output.rateSheetId = data.rateSheetId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetRateSheetChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<RateSheetChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const rateSheetChangeHistory$ = this.requestRateSheetChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RateSheetChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, rateSheetChangeHistory$);

            return rateSheetChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<RateSheetChangeHistoryData>;
    }

    private requestRateSheetChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<RateSheetChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RateSheetChangeHistoryData>(this.baseUrl + 'api/RateSheetChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveRateSheetChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestRateSheetChangeHistory(id, includeRelations));
            }));
    }

    public GetRateSheetChangeHistoryList(config: RateSheetChangeHistoryQueryParameters | any = null) : Observable<Array<RateSheetChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const rateSheetChangeHistoryList$ = this.requestRateSheetChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RateSheetChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, rateSheetChangeHistoryList$);

            return rateSheetChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<RateSheetChangeHistoryData>>;
    }


    private requestRateSheetChangeHistoryList(config: RateSheetChangeHistoryQueryParameters | any) : Observable <Array<RateSheetChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RateSheetChangeHistoryData>>(this.baseUrl + 'api/RateSheetChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveRateSheetChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestRateSheetChangeHistoryList(config));
            }));
    }

    public GetRateSheetChangeHistoriesRowCount(config: RateSheetChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const rateSheetChangeHistoriesRowCount$ = this.requestRateSheetChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RateSheetChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, rateSheetChangeHistoriesRowCount$);

            return rateSheetChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestRateSheetChangeHistoriesRowCount(config: RateSheetChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/RateSheetChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRateSheetChangeHistoriesRowCount(config));
            }));
    }

    public GetRateSheetChangeHistoriesBasicListData(config: RateSheetChangeHistoryQueryParameters | any = null) : Observable<Array<RateSheetChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const rateSheetChangeHistoriesBasicListData$ = this.requestRateSheetChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RateSheetChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, rateSheetChangeHistoriesBasicListData$);

            return rateSheetChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<RateSheetChangeHistoryBasicListData>>;
    }


    private requestRateSheetChangeHistoriesBasicListData(config: RateSheetChangeHistoryQueryParameters | any) : Observable<Array<RateSheetChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RateSheetChangeHistoryBasicListData>>(this.baseUrl + 'api/RateSheetChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRateSheetChangeHistoriesBasicListData(config));
            }));

    }


    public PutRateSheetChangeHistory(id: bigint | number, rateSheetChangeHistory: RateSheetChangeHistorySubmitData) : Observable<RateSheetChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RateSheetChangeHistoryData>(this.baseUrl + 'api/RateSheetChangeHistory/' + id.toString(), rateSheetChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRateSheetChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutRateSheetChangeHistory(id, rateSheetChangeHistory));
            }));
    }


    public PostRateSheetChangeHistory(rateSheetChangeHistory: RateSheetChangeHistorySubmitData) : Observable<RateSheetChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<RateSheetChangeHistoryData>(this.baseUrl + 'api/RateSheetChangeHistory', rateSheetChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRateSheetChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostRateSheetChangeHistory(rateSheetChangeHistory));
            }));
    }

  
    public DeleteRateSheetChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/RateSheetChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteRateSheetChangeHistory(id));
            }));
    }


    private getConfigHash(config: RateSheetChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerRateSheetChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerRateSheetChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.RateSheetChangeHistories
        //
        if (userIsSchedulerRateSheetChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerRateSheetChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerRateSheetChangeHistoryReader = false;
            }
        }

        return userIsSchedulerRateSheetChangeHistoryReader;
    }


    public userIsSchedulerRateSheetChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerRateSheetChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.RateSheetChangeHistories
        //
        if (userIsSchedulerRateSheetChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerRateSheetChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerRateSheetChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerRateSheetChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full RateSheetChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the RateSheetChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when RateSheetChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveRateSheetChangeHistory(raw: any): RateSheetChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a RateSheetChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(RateSheetChangeHistoryData.prototype) as RateSheetChangeHistoryData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //

    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadRateSheetChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveRateSheetChangeHistoryList(rawList: any[]): RateSheetChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveRateSheetChangeHistory(raw));
  }

}
