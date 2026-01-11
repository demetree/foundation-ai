/*

   GENERATED SERVICE FOR THE SCHEDULEDEVENTTEMPLATEQUALIFICATIONREQUIREMENTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ScheduledEventTemplateQualificationRequirementChangeHistory table.

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
import { ScheduledEventTemplateQualificationRequirementData } from './scheduled-event-template-qualification-requirement.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ScheduledEventTemplateQualificationRequirementChangeHistoryQueryParameters {
    scheduledEventTemplateQualificationRequirementId: bigint | number | null | undefined = null;
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
export class ScheduledEventTemplateQualificationRequirementChangeHistorySubmitData {
    id!: bigint | number;
    scheduledEventTemplateQualificationRequirementId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
}


export class ScheduledEventTemplateQualificationRequirementChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ScheduledEventTemplateQualificationRequirementChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `scheduledEventTemplateQualificationRequirementChangeHistory.ScheduledEventTemplateQualificationRequirementChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `scheduledEventTemplateQualificationRequirementChangeHistory.ScheduledEventTemplateQualificationRequirementChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="scheduledEventTemplateQualificationRequirementChangeHistory.ScheduledEventTemplateQualificationRequirementChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`scheduledEventTemplateQualificationRequirementChangeHistory.ScheduledEventTemplateQualificationRequirementChangeHistoryChildren` or `await scheduledEventTemplateQualificationRequirementChangeHistory.ScheduledEventTemplateQualificationRequirementChangeHistoryChildren`)
//    - Simply reading `scheduledEventTemplateQualificationRequirementChangeHistory.ScheduledEventTemplateQualificationRequirementChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await scheduledEventTemplateQualificationRequirementChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ScheduledEventTemplateQualificationRequirementChangeHistoryData {
    id!: bigint | number;
    scheduledEventTemplateQualificationRequirementId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
    scheduledEventTemplateQualificationRequirement: ScheduledEventTemplateQualificationRequirementData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ScheduledEventTemplateQualificationRequirementChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.scheduledEventTemplateQualificationRequirementChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     scheduledEventTemplateQualificationRequirementChangeHistory[0].Reload().then(x => {
  //        this.scheduledEventTemplateQualificationRequirementChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ScheduledEventTemplateQualificationRequirementChangeHistoryService.Instance.GetScheduledEventTemplateQualificationRequirementChangeHistory(this.id, includeRelations)
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
     * Updates the state of this ScheduledEventTemplateQualificationRequirementChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ScheduledEventTemplateQualificationRequirementChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ScheduledEventTemplateQualificationRequirementChangeHistorySubmitData {
        return ScheduledEventTemplateQualificationRequirementChangeHistoryService.Instance.ConvertToScheduledEventTemplateQualificationRequirementChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ScheduledEventTemplateQualificationRequirementChangeHistoryService extends SecureEndpointBase {

    private static _instance: ScheduledEventTemplateQualificationRequirementChangeHistoryService;
    private listCache: Map<string, Observable<Array<ScheduledEventTemplateQualificationRequirementChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ScheduledEventTemplateQualificationRequirementChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<ScheduledEventTemplateQualificationRequirementChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ScheduledEventTemplateQualificationRequirementChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ScheduledEventTemplateQualificationRequirementChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<ScheduledEventTemplateQualificationRequirementChangeHistoryData>>();

        ScheduledEventTemplateQualificationRequirementChangeHistoryService._instance = this;
    }

    public static get Instance(): ScheduledEventTemplateQualificationRequirementChangeHistoryService {
      return ScheduledEventTemplateQualificationRequirementChangeHistoryService._instance;
    }


    public ClearListCaches(config: ScheduledEventTemplateQualificationRequirementChangeHistoryQueryParameters | null = null) {

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


    public ConvertToScheduledEventTemplateQualificationRequirementChangeHistorySubmitData(data: ScheduledEventTemplateQualificationRequirementChangeHistoryData): ScheduledEventTemplateQualificationRequirementChangeHistorySubmitData {

        let output = new ScheduledEventTemplateQualificationRequirementChangeHistorySubmitData();

        output.id = data.id;
        output.scheduledEventTemplateQualificationRequirementId = data.scheduledEventTemplateQualificationRequirementId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetScheduledEventTemplateQualificationRequirementChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventTemplateQualificationRequirementChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const scheduledEventTemplateQualificationRequirementChangeHistory$ = this.requestScheduledEventTemplateQualificationRequirementChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateQualificationRequirementChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, scheduledEventTemplateQualificationRequirementChangeHistory$);

            return scheduledEventTemplateQualificationRequirementChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<ScheduledEventTemplateQualificationRequirementChangeHistoryData>;
    }

    private requestScheduledEventTemplateQualificationRequirementChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventTemplateQualificationRequirementChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventTemplateQualificationRequirementChangeHistoryData>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirementChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveScheduledEventTemplateQualificationRequirementChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateQualificationRequirementChangeHistory(id, includeRelations));
            }));
    }

    public GetScheduledEventTemplateQualificationRequirementChangeHistoryList(config: ScheduledEventTemplateQualificationRequirementChangeHistoryQueryParameters | any = null) : Observable<Array<ScheduledEventTemplateQualificationRequirementChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const scheduledEventTemplateQualificationRequirementChangeHistoryList$ = this.requestScheduledEventTemplateQualificationRequirementChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateQualificationRequirementChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, scheduledEventTemplateQualificationRequirementChangeHistoryList$);

            return scheduledEventTemplateQualificationRequirementChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ScheduledEventTemplateQualificationRequirementChangeHistoryData>>;
    }


    private requestScheduledEventTemplateQualificationRequirementChangeHistoryList(config: ScheduledEventTemplateQualificationRequirementChangeHistoryQueryParameters | any) : Observable <Array<ScheduledEventTemplateQualificationRequirementChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventTemplateQualificationRequirementChangeHistoryData>>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirementChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveScheduledEventTemplateQualificationRequirementChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateQualificationRequirementChangeHistoryList(config));
            }));
    }

    public GetScheduledEventTemplateQualificationRequirementChangeHistoriesRowCount(config: ScheduledEventTemplateQualificationRequirementChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const scheduledEventTemplateQualificationRequirementChangeHistoriesRowCount$ = this.requestScheduledEventTemplateQualificationRequirementChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateQualificationRequirementChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, scheduledEventTemplateQualificationRequirementChangeHistoriesRowCount$);

            return scheduledEventTemplateQualificationRequirementChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestScheduledEventTemplateQualificationRequirementChangeHistoriesRowCount(config: ScheduledEventTemplateQualificationRequirementChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirementChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateQualificationRequirementChangeHistoriesRowCount(config));
            }));
    }

    public GetScheduledEventTemplateQualificationRequirementChangeHistoriesBasicListData(config: ScheduledEventTemplateQualificationRequirementChangeHistoryQueryParameters | any = null) : Observable<Array<ScheduledEventTemplateQualificationRequirementChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const scheduledEventTemplateQualificationRequirementChangeHistoriesBasicListData$ = this.requestScheduledEventTemplateQualificationRequirementChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateQualificationRequirementChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, scheduledEventTemplateQualificationRequirementChangeHistoriesBasicListData$);

            return scheduledEventTemplateQualificationRequirementChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ScheduledEventTemplateQualificationRequirementChangeHistoryBasicListData>>;
    }


    private requestScheduledEventTemplateQualificationRequirementChangeHistoriesBasicListData(config: ScheduledEventTemplateQualificationRequirementChangeHistoryQueryParameters | any) : Observable<Array<ScheduledEventTemplateQualificationRequirementChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventTemplateQualificationRequirementChangeHistoryBasicListData>>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirementChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateQualificationRequirementChangeHistoriesBasicListData(config));
            }));

    }


    public PutScheduledEventTemplateQualificationRequirementChangeHistory(id: bigint | number, scheduledEventTemplateQualificationRequirementChangeHistory: ScheduledEventTemplateQualificationRequirementChangeHistorySubmitData) : Observable<ScheduledEventTemplateQualificationRequirementChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventTemplateQualificationRequirementChangeHistoryData>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirementChangeHistory/' + id.toString(), scheduledEventTemplateQualificationRequirementChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplateQualificationRequirementChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutScheduledEventTemplateQualificationRequirementChangeHistory(id, scheduledEventTemplateQualificationRequirementChangeHistory));
            }));
    }


    public PostScheduledEventTemplateQualificationRequirementChangeHistory(scheduledEventTemplateQualificationRequirementChangeHistory: ScheduledEventTemplateQualificationRequirementChangeHistorySubmitData) : Observable<ScheduledEventTemplateQualificationRequirementChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ScheduledEventTemplateQualificationRequirementChangeHistoryData>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirementChangeHistory', scheduledEventTemplateQualificationRequirementChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplateQualificationRequirementChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostScheduledEventTemplateQualificationRequirementChangeHistory(scheduledEventTemplateQualificationRequirementChangeHistory));
            }));
    }

  
    public DeleteScheduledEventTemplateQualificationRequirementChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirementChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteScheduledEventTemplateQualificationRequirementChangeHistory(id));
            }));
    }


    private getConfigHash(config: ScheduledEventTemplateQualificationRequirementChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ScheduledEventTemplateQualificationRequirementChangeHistories
        //
        if (userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryReader = false;
            }
        }

        return userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryReader;
    }


    public userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ScheduledEventTemplateQualificationRequirementChangeHistories
        //
        if (userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ScheduledEventTemplateQualificationRequirementChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ScheduledEventTemplateQualificationRequirementChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ScheduledEventTemplateQualificationRequirementChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveScheduledEventTemplateQualificationRequirementChangeHistory(raw: any): ScheduledEventTemplateQualificationRequirementChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a ScheduledEventTemplateQualificationRequirementChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(ScheduledEventTemplateQualificationRequirementChangeHistoryData.prototype) as ScheduledEventTemplateQualificationRequirementChangeHistoryData;

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
    // 2. But private methods (loadScheduledEventTemplateQualificationRequirementChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveScheduledEventTemplateQualificationRequirementChangeHistoryList(rawList: any[]): ScheduledEventTemplateQualificationRequirementChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveScheduledEventTemplateQualificationRequirementChangeHistory(raw));
  }

}
