/*

   GENERATED SERVICE FOR THE SCHEDULINGTARGETCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SchedulingTargetChangeHistory table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SchedulingTargetChangeHistoryQueryParameters {
    schedulingTargetId: bigint | number | null | undefined = null;
    versionNumber: bigint | number | null | undefined = null;
    timeStamp: string | null | undefined = null;        // ISO 8601
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
export class SchedulingTargetChangeHistorySubmitData {
    id!: bigint | number;
    schedulingTargetId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
}


export class SchedulingTargetChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SchedulingTargetChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `schedulingTargetChangeHistory.SchedulingTargetChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `schedulingTargetChangeHistory.SchedulingTargetChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="schedulingTargetChangeHistory.SchedulingTargetChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`schedulingTargetChangeHistory.SchedulingTargetChangeHistoryChildren` or `await schedulingTargetChangeHistory.SchedulingTargetChangeHistoryChildren`)
//    - Simply reading `schedulingTargetChangeHistory.SchedulingTargetChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await schedulingTargetChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SchedulingTargetChangeHistoryData {
    id!: bigint | number;
    schedulingTargetId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
    schedulingTarget: SchedulingTargetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any SchedulingTargetChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.schedulingTargetChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     schedulingTargetChangeHistory[0].Reload().then(x => {
  //        this.schedulingTargetChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SchedulingTargetChangeHistoryService.Instance.GetSchedulingTargetChangeHistory(this.id, includeRelations)
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
     * Updates the state of this SchedulingTargetChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SchedulingTargetChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SchedulingTargetChangeHistorySubmitData {
        return SchedulingTargetChangeHistoryService.Instance.ConvertToSchedulingTargetChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SchedulingTargetChangeHistoryService extends SecureEndpointBase {

    private static _instance: SchedulingTargetChangeHistoryService;
    private listCache: Map<string, Observable<Array<SchedulingTargetChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SchedulingTargetChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<SchedulingTargetChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SchedulingTargetChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SchedulingTargetChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<SchedulingTargetChangeHistoryData>>();

        SchedulingTargetChangeHistoryService._instance = this;
    }

    public static get Instance(): SchedulingTargetChangeHistoryService {
      return SchedulingTargetChangeHistoryService._instance;
    }


    public ClearListCaches(config: SchedulingTargetChangeHistoryQueryParameters | null = null) {

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


    public ConvertToSchedulingTargetChangeHistorySubmitData(data: SchedulingTargetChangeHistoryData): SchedulingTargetChangeHistorySubmitData {

        let output = new SchedulingTargetChangeHistorySubmitData();

        output.id = data.id;
        output.schedulingTargetId = data.schedulingTargetId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetSchedulingTargetChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const schedulingTargetChangeHistory$ = this.requestSchedulingTargetChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, schedulingTargetChangeHistory$);

            return schedulingTargetChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<SchedulingTargetChangeHistoryData>;
    }

    private requestSchedulingTargetChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetChangeHistoryData>(this.baseUrl + 'api/SchedulingTargetChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSchedulingTargetChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetChangeHistory(id, includeRelations));
            }));
    }

    public GetSchedulingTargetChangeHistoryList(config: SchedulingTargetChangeHistoryQueryParameters | any = null) : Observable<Array<SchedulingTargetChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const schedulingTargetChangeHistoryList$ = this.requestSchedulingTargetChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, schedulingTargetChangeHistoryList$);

            return schedulingTargetChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SchedulingTargetChangeHistoryData>>;
    }


    private requestSchedulingTargetChangeHistoryList(config: SchedulingTargetChangeHistoryQueryParameters | any) : Observable <Array<SchedulingTargetChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetChangeHistoryData>>(this.baseUrl + 'api/SchedulingTargetChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSchedulingTargetChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetChangeHistoryList(config));
            }));
    }

    public GetSchedulingTargetChangeHistoriesRowCount(config: SchedulingTargetChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const schedulingTargetChangeHistoriesRowCount$ = this.requestSchedulingTargetChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, schedulingTargetChangeHistoriesRowCount$);

            return schedulingTargetChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSchedulingTargetChangeHistoriesRowCount(config: SchedulingTargetChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SchedulingTargetChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetChangeHistoriesRowCount(config));
            }));
    }

    public GetSchedulingTargetChangeHistoriesBasicListData(config: SchedulingTargetChangeHistoryQueryParameters | any = null) : Observable<Array<SchedulingTargetChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const schedulingTargetChangeHistoriesBasicListData$ = this.requestSchedulingTargetChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, schedulingTargetChangeHistoriesBasicListData$);

            return schedulingTargetChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SchedulingTargetChangeHistoryBasicListData>>;
    }


    private requestSchedulingTargetChangeHistoriesBasicListData(config: SchedulingTargetChangeHistoryQueryParameters | any) : Observable<Array<SchedulingTargetChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetChangeHistoryBasicListData>>(this.baseUrl + 'api/SchedulingTargetChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetChangeHistoriesBasicListData(config));
            }));

    }


    public PutSchedulingTargetChangeHistory(id: bigint | number, schedulingTargetChangeHistory: SchedulingTargetChangeHistorySubmitData) : Observable<SchedulingTargetChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SchedulingTargetChangeHistoryData>(this.baseUrl + 'api/SchedulingTargetChangeHistory/' + id.toString(), schedulingTargetChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSchedulingTargetChangeHistory(id, schedulingTargetChangeHistory));
            }));
    }


    public PostSchedulingTargetChangeHistory(schedulingTargetChangeHistory: SchedulingTargetChangeHistorySubmitData) : Observable<SchedulingTargetChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SchedulingTargetChangeHistoryData>(this.baseUrl + 'api/SchedulingTargetChangeHistory', schedulingTargetChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSchedulingTargetChangeHistory(schedulingTargetChangeHistory));
            }));
    }

  
    public DeleteSchedulingTargetChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SchedulingTargetChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSchedulingTargetChangeHistory(id));
            }));
    }


    private getConfigHash(config: SchedulingTargetChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerSchedulingTargetChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerSchedulingTargetChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.SchedulingTargetChangeHistories
        //
        if (userIsSchedulerSchedulingTargetChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerSchedulingTargetChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerSchedulingTargetChangeHistoryReader = false;
            }
        }

        return userIsSchedulerSchedulingTargetChangeHistoryReader;
    }


    public userIsSchedulerSchedulingTargetChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerSchedulingTargetChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.SchedulingTargetChangeHistories
        //
        if (userIsSchedulerSchedulingTargetChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerSchedulingTargetChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerSchedulingTargetChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerSchedulingTargetChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full SchedulingTargetChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SchedulingTargetChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SchedulingTargetChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSchedulingTargetChangeHistory(raw: any): SchedulingTargetChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a SchedulingTargetChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(SchedulingTargetChangeHistoryData.prototype) as SchedulingTargetChangeHistoryData;

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
    // 2. But private methods (loadSchedulingTargetChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveSchedulingTargetChangeHistoryList(rawList: any[]): SchedulingTargetChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSchedulingTargetChangeHistory(raw));
  }

}
