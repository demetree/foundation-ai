/*

   GENERATED SERVICE FOR THE SCHEDULEOVERRIDECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ScheduleOverrideChangeHistory table.

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
import { ScheduleOverrideData } from './schedule-override.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ScheduleOverrideChangeHistoryQueryParameters {
    scheduleOverrideId: bigint | number | null | undefined = null;
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
export class ScheduleOverrideChangeHistorySubmitData {
    id!: bigint | number;
    scheduleOverrideId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
}


export class ScheduleOverrideChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ScheduleOverrideChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `scheduleOverrideChangeHistory.ScheduleOverrideChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `scheduleOverrideChangeHistory.ScheduleOverrideChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="scheduleOverrideChangeHistory.ScheduleOverrideChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`scheduleOverrideChangeHistory.ScheduleOverrideChangeHistoryChildren` or `await scheduleOverrideChangeHistory.ScheduleOverrideChangeHistoryChildren`)
//    - Simply reading `scheduleOverrideChangeHistory.ScheduleOverrideChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await scheduleOverrideChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ScheduleOverrideChangeHistoryData {
    id!: bigint | number;
    scheduleOverrideId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
    scheduleOverride: ScheduleOverrideData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ScheduleOverrideChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.scheduleOverrideChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     scheduleOverrideChangeHistory[0].Reload().then(x => {
  //        this.scheduleOverrideChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ScheduleOverrideChangeHistoryService.Instance.GetScheduleOverrideChangeHistory(this.id, includeRelations)
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
     * Updates the state of this ScheduleOverrideChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ScheduleOverrideChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ScheduleOverrideChangeHistorySubmitData {
        return ScheduleOverrideChangeHistoryService.Instance.ConvertToScheduleOverrideChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ScheduleOverrideChangeHistoryService extends SecureEndpointBase {

    private static _instance: ScheduleOverrideChangeHistoryService;
    private listCache: Map<string, Observable<Array<ScheduleOverrideChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ScheduleOverrideChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<ScheduleOverrideChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ScheduleOverrideChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ScheduleOverrideChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<ScheduleOverrideChangeHistoryData>>();

        ScheduleOverrideChangeHistoryService._instance = this;
    }

    public static get Instance(): ScheduleOverrideChangeHistoryService {
      return ScheduleOverrideChangeHistoryService._instance;
    }


    public ClearListCaches(config: ScheduleOverrideChangeHistoryQueryParameters | null = null) {

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


    public ConvertToScheduleOverrideChangeHistorySubmitData(data: ScheduleOverrideChangeHistoryData): ScheduleOverrideChangeHistorySubmitData {

        let output = new ScheduleOverrideChangeHistorySubmitData();

        output.id = data.id;
        output.scheduleOverrideId = data.scheduleOverrideId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetScheduleOverrideChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduleOverrideChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const scheduleOverrideChangeHistory$ = this.requestScheduleOverrideChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleOverrideChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, scheduleOverrideChangeHistory$);

            return scheduleOverrideChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<ScheduleOverrideChangeHistoryData>;
    }

    private requestScheduleOverrideChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduleOverrideChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduleOverrideChangeHistoryData>(this.baseUrl + 'api/ScheduleOverrideChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveScheduleOverrideChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleOverrideChangeHistory(id, includeRelations));
            }));
    }

    public GetScheduleOverrideChangeHistoryList(config: ScheduleOverrideChangeHistoryQueryParameters | any = null) : Observable<Array<ScheduleOverrideChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const scheduleOverrideChangeHistoryList$ = this.requestScheduleOverrideChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleOverrideChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, scheduleOverrideChangeHistoryList$);

            return scheduleOverrideChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ScheduleOverrideChangeHistoryData>>;
    }


    private requestScheduleOverrideChangeHistoryList(config: ScheduleOverrideChangeHistoryQueryParameters | any) : Observable <Array<ScheduleOverrideChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduleOverrideChangeHistoryData>>(this.baseUrl + 'api/ScheduleOverrideChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveScheduleOverrideChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleOverrideChangeHistoryList(config));
            }));
    }

    public GetScheduleOverrideChangeHistoriesRowCount(config: ScheduleOverrideChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const scheduleOverrideChangeHistoriesRowCount$ = this.requestScheduleOverrideChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleOverrideChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, scheduleOverrideChangeHistoriesRowCount$);

            return scheduleOverrideChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestScheduleOverrideChangeHistoriesRowCount(config: ScheduleOverrideChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ScheduleOverrideChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleOverrideChangeHistoriesRowCount(config));
            }));
    }

    public GetScheduleOverrideChangeHistoriesBasicListData(config: ScheduleOverrideChangeHistoryQueryParameters | any = null) : Observable<Array<ScheduleOverrideChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const scheduleOverrideChangeHistoriesBasicListData$ = this.requestScheduleOverrideChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleOverrideChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, scheduleOverrideChangeHistoriesBasicListData$);

            return scheduleOverrideChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ScheduleOverrideChangeHistoryBasicListData>>;
    }


    private requestScheduleOverrideChangeHistoriesBasicListData(config: ScheduleOverrideChangeHistoryQueryParameters | any) : Observable<Array<ScheduleOverrideChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduleOverrideChangeHistoryBasicListData>>(this.baseUrl + 'api/ScheduleOverrideChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleOverrideChangeHistoriesBasicListData(config));
            }));

    }


    public PutScheduleOverrideChangeHistory(id: bigint | number, scheduleOverrideChangeHistory: ScheduleOverrideChangeHistorySubmitData) : Observable<ScheduleOverrideChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduleOverrideChangeHistoryData>(this.baseUrl + 'api/ScheduleOverrideChangeHistory/' + id.toString(), scheduleOverrideChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduleOverrideChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutScheduleOverrideChangeHistory(id, scheduleOverrideChangeHistory));
            }));
    }


    public PostScheduleOverrideChangeHistory(scheduleOverrideChangeHistory: ScheduleOverrideChangeHistorySubmitData) : Observable<ScheduleOverrideChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ScheduleOverrideChangeHistoryData>(this.baseUrl + 'api/ScheduleOverrideChangeHistory', scheduleOverrideChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduleOverrideChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostScheduleOverrideChangeHistory(scheduleOverrideChangeHistory));
            }));
    }

  
    public DeleteScheduleOverrideChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ScheduleOverrideChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteScheduleOverrideChangeHistory(id));
            }));
    }


    private getConfigHash(config: ScheduleOverrideChangeHistoryQueryParameters | any): string {

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

    public userIsAlertingScheduleOverrideChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingScheduleOverrideChangeHistoryReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.ScheduleOverrideChangeHistories
        //
        if (userIsAlertingScheduleOverrideChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingScheduleOverrideChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsAlertingScheduleOverrideChangeHistoryReader = false;
            }
        }

        return userIsAlertingScheduleOverrideChangeHistoryReader;
    }


    public userIsAlertingScheduleOverrideChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingScheduleOverrideChangeHistoryWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.ScheduleOverrideChangeHistories
        //
        if (userIsAlertingScheduleOverrideChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingScheduleOverrideChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsAlertingScheduleOverrideChangeHistoryWriter = false;
          }      
        }

        return userIsAlertingScheduleOverrideChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ScheduleOverrideChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ScheduleOverrideChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ScheduleOverrideChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveScheduleOverrideChangeHistory(raw: any): ScheduleOverrideChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a ScheduleOverrideChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(ScheduleOverrideChangeHistoryData.prototype) as ScheduleOverrideChangeHistoryData;

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
    // 2. But private methods (loadScheduleOverrideChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveScheduleOverrideChangeHistoryList(rawList: any[]): ScheduleOverrideChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveScheduleOverrideChangeHistory(raw));
  }

}
