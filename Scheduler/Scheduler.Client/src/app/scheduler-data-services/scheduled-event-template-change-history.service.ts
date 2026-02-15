/*

   GENERATED SERVICE FOR THE SCHEDULEDEVENTTEMPLATECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ScheduledEventTemplateChangeHistory table.

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
import { ScheduledEventTemplateData } from './scheduled-event-template.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ScheduledEventTemplateChangeHistoryQueryParameters {
    scheduledEventTemplateId: bigint | number | null | undefined = null;
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
export class ScheduledEventTemplateChangeHistorySubmitData {
    id!: bigint | number;
    scheduledEventTemplateId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
}


export class ScheduledEventTemplateChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ScheduledEventTemplateChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `scheduledEventTemplateChangeHistory.ScheduledEventTemplateChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `scheduledEventTemplateChangeHistory.ScheduledEventTemplateChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="scheduledEventTemplateChangeHistory.ScheduledEventTemplateChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`scheduledEventTemplateChangeHistory.ScheduledEventTemplateChangeHistoryChildren` or `await scheduledEventTemplateChangeHistory.ScheduledEventTemplateChangeHistoryChildren`)
//    - Simply reading `scheduledEventTemplateChangeHistory.ScheduledEventTemplateChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await scheduledEventTemplateChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ScheduledEventTemplateChangeHistoryData {
    id!: bigint | number;
    scheduledEventTemplateId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
    scheduledEventTemplate: ScheduledEventTemplateData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ScheduledEventTemplateChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.scheduledEventTemplateChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     scheduledEventTemplateChangeHistory[0].Reload().then(x => {
  //        this.scheduledEventTemplateChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ScheduledEventTemplateChangeHistoryService.Instance.GetScheduledEventTemplateChangeHistory(this.id, includeRelations)
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
     * Updates the state of this ScheduledEventTemplateChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ScheduledEventTemplateChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ScheduledEventTemplateChangeHistorySubmitData {
        return ScheduledEventTemplateChangeHistoryService.Instance.ConvertToScheduledEventTemplateChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ScheduledEventTemplateChangeHistoryService extends SecureEndpointBase {

    private static _instance: ScheduledEventTemplateChangeHistoryService;
    private listCache: Map<string, Observable<Array<ScheduledEventTemplateChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ScheduledEventTemplateChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<ScheduledEventTemplateChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ScheduledEventTemplateChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ScheduledEventTemplateChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<ScheduledEventTemplateChangeHistoryData>>();

        ScheduledEventTemplateChangeHistoryService._instance = this;
    }

    public static get Instance(): ScheduledEventTemplateChangeHistoryService {
      return ScheduledEventTemplateChangeHistoryService._instance;
    }


    public ClearListCaches(config: ScheduledEventTemplateChangeHistoryQueryParameters | null = null) {

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


    public ConvertToScheduledEventTemplateChangeHistorySubmitData(data: ScheduledEventTemplateChangeHistoryData): ScheduledEventTemplateChangeHistorySubmitData {

        let output = new ScheduledEventTemplateChangeHistorySubmitData();

        output.id = data.id;
        output.scheduledEventTemplateId = data.scheduledEventTemplateId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetScheduledEventTemplateChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventTemplateChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const scheduledEventTemplateChangeHistory$ = this.requestScheduledEventTemplateChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, scheduledEventTemplateChangeHistory$);

            return scheduledEventTemplateChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<ScheduledEventTemplateChangeHistoryData>;
    }

    private requestScheduledEventTemplateChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventTemplateChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventTemplateChangeHistoryData>(this.baseUrl + 'api/ScheduledEventTemplateChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveScheduledEventTemplateChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateChangeHistory(id, includeRelations));
            }));
    }

    public GetScheduledEventTemplateChangeHistoryList(config: ScheduledEventTemplateChangeHistoryQueryParameters | any = null) : Observable<Array<ScheduledEventTemplateChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const scheduledEventTemplateChangeHistoryList$ = this.requestScheduledEventTemplateChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, scheduledEventTemplateChangeHistoryList$);

            return scheduledEventTemplateChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ScheduledEventTemplateChangeHistoryData>>;
    }


    private requestScheduledEventTemplateChangeHistoryList(config: ScheduledEventTemplateChangeHistoryQueryParameters | any) : Observable <Array<ScheduledEventTemplateChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventTemplateChangeHistoryData>>(this.baseUrl + 'api/ScheduledEventTemplateChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveScheduledEventTemplateChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateChangeHistoryList(config));
            }));
    }

    public GetScheduledEventTemplateChangeHistoriesRowCount(config: ScheduledEventTemplateChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const scheduledEventTemplateChangeHistoriesRowCount$ = this.requestScheduledEventTemplateChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, scheduledEventTemplateChangeHistoriesRowCount$);

            return scheduledEventTemplateChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestScheduledEventTemplateChangeHistoriesRowCount(config: ScheduledEventTemplateChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ScheduledEventTemplateChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateChangeHistoriesRowCount(config));
            }));
    }

    public GetScheduledEventTemplateChangeHistoriesBasicListData(config: ScheduledEventTemplateChangeHistoryQueryParameters | any = null) : Observable<Array<ScheduledEventTemplateChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const scheduledEventTemplateChangeHistoriesBasicListData$ = this.requestScheduledEventTemplateChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, scheduledEventTemplateChangeHistoriesBasicListData$);

            return scheduledEventTemplateChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ScheduledEventTemplateChangeHistoryBasicListData>>;
    }


    private requestScheduledEventTemplateChangeHistoriesBasicListData(config: ScheduledEventTemplateChangeHistoryQueryParameters | any) : Observable<Array<ScheduledEventTemplateChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventTemplateChangeHistoryBasicListData>>(this.baseUrl + 'api/ScheduledEventTemplateChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateChangeHistoriesBasicListData(config));
            }));

    }


    public PutScheduledEventTemplateChangeHistory(id: bigint | number, scheduledEventTemplateChangeHistory: ScheduledEventTemplateChangeHistorySubmitData) : Observable<ScheduledEventTemplateChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventTemplateChangeHistoryData>(this.baseUrl + 'api/ScheduledEventTemplateChangeHistory/' + id.toString(), scheduledEventTemplateChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplateChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutScheduledEventTemplateChangeHistory(id, scheduledEventTemplateChangeHistory));
            }));
    }


    public PostScheduledEventTemplateChangeHistory(scheduledEventTemplateChangeHistory: ScheduledEventTemplateChangeHistorySubmitData) : Observable<ScheduledEventTemplateChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ScheduledEventTemplateChangeHistoryData>(this.baseUrl + 'api/ScheduledEventTemplateChangeHistory', scheduledEventTemplateChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplateChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostScheduledEventTemplateChangeHistory(scheduledEventTemplateChangeHistory));
            }));
    }

  
    public DeleteScheduledEventTemplateChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ScheduledEventTemplateChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteScheduledEventTemplateChangeHistory(id));
            }));
    }


    private getConfigHash(config: ScheduledEventTemplateChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerScheduledEventTemplateChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerScheduledEventTemplateChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ScheduledEventTemplateChangeHistories
        //
        if (userIsSchedulerScheduledEventTemplateChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerScheduledEventTemplateChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerScheduledEventTemplateChangeHistoryReader = false;
            }
        }

        return userIsSchedulerScheduledEventTemplateChangeHistoryReader;
    }


    public userIsSchedulerScheduledEventTemplateChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerScheduledEventTemplateChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ScheduledEventTemplateChangeHistories
        //
        if (userIsSchedulerScheduledEventTemplateChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerScheduledEventTemplateChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerScheduledEventTemplateChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerScheduledEventTemplateChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ScheduledEventTemplateChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ScheduledEventTemplateChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ScheduledEventTemplateChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveScheduledEventTemplateChangeHistory(raw: any): ScheduledEventTemplateChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a ScheduledEventTemplateChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(ScheduledEventTemplateChangeHistoryData.prototype) as ScheduledEventTemplateChangeHistoryData;

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
    // 2. But private methods (loadScheduledEventTemplateChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveScheduledEventTemplateChangeHistoryList(rawList: any[]): ScheduledEventTemplateChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveScheduledEventTemplateChangeHistory(raw));
  }

}
