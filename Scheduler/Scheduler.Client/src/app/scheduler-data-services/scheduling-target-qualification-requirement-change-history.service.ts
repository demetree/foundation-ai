/*

   GENERATED SERVICE FOR THE SCHEDULINGTARGETQUALIFICATIONREQUIREMENTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SchedulingTargetQualificationRequirementChangeHistory table.

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
import { SchedulingTargetQualificationRequirementData } from './scheduling-target-qualification-requirement.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SchedulingTargetQualificationRequirementChangeHistoryQueryParameters {
    schedulingTargetQualificationRequirementId: bigint | number | null | undefined = null;
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
export class SchedulingTargetQualificationRequirementChangeHistorySubmitData {
    id!: bigint | number;
    schedulingTargetQualificationRequirementId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
}


export class SchedulingTargetQualificationRequirementChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SchedulingTargetQualificationRequirementChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `schedulingTargetQualificationRequirementChangeHistory.SchedulingTargetQualificationRequirementChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `schedulingTargetQualificationRequirementChangeHistory.SchedulingTargetQualificationRequirementChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="schedulingTargetQualificationRequirementChangeHistory.SchedulingTargetQualificationRequirementChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`schedulingTargetQualificationRequirementChangeHistory.SchedulingTargetQualificationRequirementChangeHistoryChildren` or `await schedulingTargetQualificationRequirementChangeHistory.SchedulingTargetQualificationRequirementChangeHistoryChildren`)
//    - Simply reading `schedulingTargetQualificationRequirementChangeHistory.SchedulingTargetQualificationRequirementChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await schedulingTargetQualificationRequirementChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SchedulingTargetQualificationRequirementChangeHistoryData {
    id!: bigint | number;
    schedulingTargetQualificationRequirementId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
    schedulingTargetQualificationRequirement: SchedulingTargetQualificationRequirementData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any SchedulingTargetQualificationRequirementChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.schedulingTargetQualificationRequirementChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     schedulingTargetQualificationRequirementChangeHistory[0].Reload().then(x => {
  //        this.schedulingTargetQualificationRequirementChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SchedulingTargetQualificationRequirementChangeHistoryService.Instance.GetSchedulingTargetQualificationRequirementChangeHistory(this.id, includeRelations)
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
     * Updates the state of this SchedulingTargetQualificationRequirementChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SchedulingTargetQualificationRequirementChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SchedulingTargetQualificationRequirementChangeHistorySubmitData {
        return SchedulingTargetQualificationRequirementChangeHistoryService.Instance.ConvertToSchedulingTargetQualificationRequirementChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SchedulingTargetQualificationRequirementChangeHistoryService extends SecureEndpointBase {

    private static _instance: SchedulingTargetQualificationRequirementChangeHistoryService;
    private listCache: Map<string, Observable<Array<SchedulingTargetQualificationRequirementChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SchedulingTargetQualificationRequirementChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<SchedulingTargetQualificationRequirementChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SchedulingTargetQualificationRequirementChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SchedulingTargetQualificationRequirementChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<SchedulingTargetQualificationRequirementChangeHistoryData>>();

        SchedulingTargetQualificationRequirementChangeHistoryService._instance = this;
    }

    public static get Instance(): SchedulingTargetQualificationRequirementChangeHistoryService {
      return SchedulingTargetQualificationRequirementChangeHistoryService._instance;
    }


    public ClearListCaches(config: SchedulingTargetQualificationRequirementChangeHistoryQueryParameters | null = null) {

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


    public ConvertToSchedulingTargetQualificationRequirementChangeHistorySubmitData(data: SchedulingTargetQualificationRequirementChangeHistoryData): SchedulingTargetQualificationRequirementChangeHistorySubmitData {

        let output = new SchedulingTargetQualificationRequirementChangeHistorySubmitData();

        output.id = data.id;
        output.schedulingTargetQualificationRequirementId = data.schedulingTargetQualificationRequirementId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetSchedulingTargetQualificationRequirementChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetQualificationRequirementChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const schedulingTargetQualificationRequirementChangeHistory$ = this.requestSchedulingTargetQualificationRequirementChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetQualificationRequirementChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, schedulingTargetQualificationRequirementChangeHistory$);

            return schedulingTargetQualificationRequirementChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<SchedulingTargetQualificationRequirementChangeHistoryData>;
    }

    private requestSchedulingTargetQualificationRequirementChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetQualificationRequirementChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetQualificationRequirementChangeHistoryData>(this.baseUrl + 'api/SchedulingTargetQualificationRequirementChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSchedulingTargetQualificationRequirementChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetQualificationRequirementChangeHistory(id, includeRelations));
            }));
    }

    public GetSchedulingTargetQualificationRequirementChangeHistoryList(config: SchedulingTargetQualificationRequirementChangeHistoryQueryParameters | any = null) : Observable<Array<SchedulingTargetQualificationRequirementChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const schedulingTargetQualificationRequirementChangeHistoryList$ = this.requestSchedulingTargetQualificationRequirementChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetQualificationRequirementChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, schedulingTargetQualificationRequirementChangeHistoryList$);

            return schedulingTargetQualificationRequirementChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SchedulingTargetQualificationRequirementChangeHistoryData>>;
    }


    private requestSchedulingTargetQualificationRequirementChangeHistoryList(config: SchedulingTargetQualificationRequirementChangeHistoryQueryParameters | any) : Observable <Array<SchedulingTargetQualificationRequirementChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetQualificationRequirementChangeHistoryData>>(this.baseUrl + 'api/SchedulingTargetQualificationRequirementChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSchedulingTargetQualificationRequirementChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetQualificationRequirementChangeHistoryList(config));
            }));
    }

    public GetSchedulingTargetQualificationRequirementChangeHistoriesRowCount(config: SchedulingTargetQualificationRequirementChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const schedulingTargetQualificationRequirementChangeHistoriesRowCount$ = this.requestSchedulingTargetQualificationRequirementChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetQualificationRequirementChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, schedulingTargetQualificationRequirementChangeHistoriesRowCount$);

            return schedulingTargetQualificationRequirementChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSchedulingTargetQualificationRequirementChangeHistoriesRowCount(config: SchedulingTargetQualificationRequirementChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SchedulingTargetQualificationRequirementChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetQualificationRequirementChangeHistoriesRowCount(config));
            }));
    }

    public GetSchedulingTargetQualificationRequirementChangeHistoriesBasicListData(config: SchedulingTargetQualificationRequirementChangeHistoryQueryParameters | any = null) : Observable<Array<SchedulingTargetQualificationRequirementChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const schedulingTargetQualificationRequirementChangeHistoriesBasicListData$ = this.requestSchedulingTargetQualificationRequirementChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetQualificationRequirementChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, schedulingTargetQualificationRequirementChangeHistoriesBasicListData$);

            return schedulingTargetQualificationRequirementChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SchedulingTargetQualificationRequirementChangeHistoryBasicListData>>;
    }


    private requestSchedulingTargetQualificationRequirementChangeHistoriesBasicListData(config: SchedulingTargetQualificationRequirementChangeHistoryQueryParameters | any) : Observable<Array<SchedulingTargetQualificationRequirementChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetQualificationRequirementChangeHistoryBasicListData>>(this.baseUrl + 'api/SchedulingTargetQualificationRequirementChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetQualificationRequirementChangeHistoriesBasicListData(config));
            }));

    }


    public PutSchedulingTargetQualificationRequirementChangeHistory(id: bigint | number, schedulingTargetQualificationRequirementChangeHistory: SchedulingTargetQualificationRequirementChangeHistorySubmitData) : Observable<SchedulingTargetQualificationRequirementChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SchedulingTargetQualificationRequirementChangeHistoryData>(this.baseUrl + 'api/SchedulingTargetQualificationRequirementChangeHistory/' + id.toString(), schedulingTargetQualificationRequirementChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetQualificationRequirementChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSchedulingTargetQualificationRequirementChangeHistory(id, schedulingTargetQualificationRequirementChangeHistory));
            }));
    }


    public PostSchedulingTargetQualificationRequirementChangeHistory(schedulingTargetQualificationRequirementChangeHistory: SchedulingTargetQualificationRequirementChangeHistorySubmitData) : Observable<SchedulingTargetQualificationRequirementChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SchedulingTargetQualificationRequirementChangeHistoryData>(this.baseUrl + 'api/SchedulingTargetQualificationRequirementChangeHistory', schedulingTargetQualificationRequirementChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetQualificationRequirementChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSchedulingTargetQualificationRequirementChangeHistory(schedulingTargetQualificationRequirementChangeHistory));
            }));
    }

  
    public DeleteSchedulingTargetQualificationRequirementChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SchedulingTargetQualificationRequirementChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSchedulingTargetQualificationRequirementChangeHistory(id));
            }));
    }


    private getConfigHash(config: SchedulingTargetQualificationRequirementChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.SchedulingTargetQualificationRequirementChangeHistories
        //
        if (userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryReader = false;
            }
        }

        return userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryReader;
    }


    public userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.SchedulingTargetQualificationRequirementChangeHistories
        //
        if (userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full SchedulingTargetQualificationRequirementChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SchedulingTargetQualificationRequirementChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SchedulingTargetQualificationRequirementChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSchedulingTargetQualificationRequirementChangeHistory(raw: any): SchedulingTargetQualificationRequirementChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a SchedulingTargetQualificationRequirementChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(SchedulingTargetQualificationRequirementChangeHistoryData.prototype) as SchedulingTargetQualificationRequirementChangeHistoryData;

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
    // 2. But private methods (loadSchedulingTargetQualificationRequirementChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveSchedulingTargetQualificationRequirementChangeHistoryList(rawList: any[]): SchedulingTargetQualificationRequirementChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSchedulingTargetQualificationRequirementChangeHistory(raw));
  }

}
