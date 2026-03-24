/*

   GENERATED SERVICE FOR THE PUSHDELIVERYLOG TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PushDeliveryLog table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PushDeliveryLogQueryParameters {
    userId: bigint | number | null | undefined = null;
    providerId: string | null | undefined = null;
    destination: string | null | undefined = null;
    sourceType: string | null | undefined = null;
    sourceNotificationId: bigint | number | null | undefined = null;
    sourceConversationMessageId: bigint | number | null | undefined = null;
    success: boolean | null | undefined = null;
    externalId: string | null | undefined = null;
    errorMessage: string | null | undefined = null;
    attemptNumber: bigint | number | null | undefined = null;
    dateTimeCreated: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class PushDeliveryLogSubmitData {
    id!: bigint | number;
    userId!: bigint | number;
    providerId!: string;
    destination: string | null = null;
    sourceType: string | null = null;
    sourceNotificationId: bigint | number | null = null;
    sourceConversationMessageId: bigint | number | null = null;
    success!: boolean;
    externalId: string | null = null;
    errorMessage: string | null = null;
    attemptNumber!: bigint | number;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    active!: boolean;
    deleted!: boolean;
}


export class PushDeliveryLogBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PushDeliveryLogChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `pushDeliveryLog.PushDeliveryLogChildren$` — use with `| async` in templates
//        • Promise:    `pushDeliveryLog.PushDeliveryLogChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="pushDeliveryLog.PushDeliveryLogChildren$ | async"`), or
//        • Access the promise getter (`pushDeliveryLog.PushDeliveryLogChildren` or `await pushDeliveryLog.PushDeliveryLogChildren`)
//    - Simply reading `pushDeliveryLog.PushDeliveryLogChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await pushDeliveryLog.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PushDeliveryLogData {
    id!: bigint | number;
    userId!: bigint | number;
    providerId!: string;
    destination!: string | null;
    sourceType!: string | null;
    sourceNotificationId!: bigint | number;
    sourceConversationMessageId!: bigint | number;
    success!: boolean;
    externalId!: string | null;
    errorMessage!: string | null;
    attemptNumber!: bigint | number;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

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
  // Promise based reload method to allow rebuilding of any PushDeliveryLogData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.pushDeliveryLog.Reload();
  //
  //  Non Async:
  //
  //     pushDeliveryLog[0].Reload().then(x => {
  //        this.pushDeliveryLog = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PushDeliveryLogService.Instance.GetPushDeliveryLog(this.id, includeRelations)
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
     * Updates the state of this PushDeliveryLogData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PushDeliveryLogData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PushDeliveryLogSubmitData {
        return PushDeliveryLogService.Instance.ConvertToPushDeliveryLogSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PushDeliveryLogService extends SecureEndpointBase {

    private static _instance: PushDeliveryLogService;
    private listCache: Map<string, Observable<Array<PushDeliveryLogData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PushDeliveryLogBasicListData>>>;
    private recordCache: Map<string, Observable<PushDeliveryLogData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PushDeliveryLogData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PushDeliveryLogBasicListData>>>();
        this.recordCache = new Map<string, Observable<PushDeliveryLogData>>();

        PushDeliveryLogService._instance = this;
    }

    public static get Instance(): PushDeliveryLogService {
      return PushDeliveryLogService._instance;
    }


    public ClearListCaches(config: PushDeliveryLogQueryParameters | null = null) {

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


    public ConvertToPushDeliveryLogSubmitData(data: PushDeliveryLogData): PushDeliveryLogSubmitData {

        let output = new PushDeliveryLogSubmitData();

        output.id = data.id;
        output.userId = data.userId;
        output.providerId = data.providerId;
        output.destination = data.destination;
        output.sourceType = data.sourceType;
        output.sourceNotificationId = data.sourceNotificationId;
        output.sourceConversationMessageId = data.sourceConversationMessageId;
        output.success = data.success;
        output.externalId = data.externalId;
        output.errorMessage = data.errorMessage;
        output.attemptNumber = data.attemptNumber;
        output.dateTimeCreated = data.dateTimeCreated;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPushDeliveryLog(id: bigint | number, includeRelations: boolean = true) : Observable<PushDeliveryLogData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const pushDeliveryLog$ = this.requestPushDeliveryLog(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PushDeliveryLog", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, pushDeliveryLog$);

            return pushDeliveryLog$;
        }

        return this.recordCache.get(configHash) as Observable<PushDeliveryLogData>;
    }

    private requestPushDeliveryLog(id: bigint | number, includeRelations: boolean = true) : Observable<PushDeliveryLogData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PushDeliveryLogData>(this.baseUrl + 'api/PushDeliveryLog/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePushDeliveryLog(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPushDeliveryLog(id, includeRelations));
            }));
    }

    public GetPushDeliveryLogList(config: PushDeliveryLogQueryParameters | any = null) : Observable<Array<PushDeliveryLogData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const pushDeliveryLogList$ = this.requestPushDeliveryLogList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PushDeliveryLog list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, pushDeliveryLogList$);

            return pushDeliveryLogList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PushDeliveryLogData>>;
    }


    private requestPushDeliveryLogList(config: PushDeliveryLogQueryParameters | any) : Observable <Array<PushDeliveryLogData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PushDeliveryLogData>>(this.baseUrl + 'api/PushDeliveryLogs', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePushDeliveryLogList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPushDeliveryLogList(config));
            }));
    }

    public GetPushDeliveryLogsRowCount(config: PushDeliveryLogQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const pushDeliveryLogsRowCount$ = this.requestPushDeliveryLogsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PushDeliveryLogs row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, pushDeliveryLogsRowCount$);

            return pushDeliveryLogsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPushDeliveryLogsRowCount(config: PushDeliveryLogQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PushDeliveryLogs/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPushDeliveryLogsRowCount(config));
            }));
    }

    public GetPushDeliveryLogsBasicListData(config: PushDeliveryLogQueryParameters | any = null) : Observable<Array<PushDeliveryLogBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const pushDeliveryLogsBasicListData$ = this.requestPushDeliveryLogsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PushDeliveryLogs basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, pushDeliveryLogsBasicListData$);

            return pushDeliveryLogsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PushDeliveryLogBasicListData>>;
    }


    private requestPushDeliveryLogsBasicListData(config: PushDeliveryLogQueryParameters | any) : Observable<Array<PushDeliveryLogBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PushDeliveryLogBasicListData>>(this.baseUrl + 'api/PushDeliveryLogs/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPushDeliveryLogsBasicListData(config));
            }));

    }


    public PutPushDeliveryLog(id: bigint | number, pushDeliveryLog: PushDeliveryLogSubmitData) : Observable<PushDeliveryLogData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PushDeliveryLogData>(this.baseUrl + 'api/PushDeliveryLog/' + id.toString(), pushDeliveryLog, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePushDeliveryLog(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPushDeliveryLog(id, pushDeliveryLog));
            }));
    }


    public PostPushDeliveryLog(pushDeliveryLog: PushDeliveryLogSubmitData) : Observable<PushDeliveryLogData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PushDeliveryLogData>(this.baseUrl + 'api/PushDeliveryLog', pushDeliveryLog, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePushDeliveryLog(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPushDeliveryLog(pushDeliveryLog));
            }));
    }

  
    public DeletePushDeliveryLog(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PushDeliveryLog/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePushDeliveryLog(id));
            }));
    }


    private getConfigHash(config: PushDeliveryLogQueryParameters | any): string {

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

    public userIsSchedulerPushDeliveryLogReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerPushDeliveryLogReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.PushDeliveryLogs
        //
        if (userIsSchedulerPushDeliveryLogReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerPushDeliveryLogReader = user.readPermission >= 50;
            } else {
                userIsSchedulerPushDeliveryLogReader = false;
            }
        }

        return userIsSchedulerPushDeliveryLogReader;
    }


    public userIsSchedulerPushDeliveryLogWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerPushDeliveryLogWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.PushDeliveryLogs
        //
        if (userIsSchedulerPushDeliveryLogWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerPushDeliveryLogWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerPushDeliveryLogWriter = false;
          }      
        }

        return userIsSchedulerPushDeliveryLogWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full PushDeliveryLogData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PushDeliveryLogData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PushDeliveryLogTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePushDeliveryLog(raw: any): PushDeliveryLogData {
    if (!raw) return raw;

    //
    // Create a PushDeliveryLogData object instance with correct prototype
    //
    const revived = Object.create(PushDeliveryLogData.prototype) as PushDeliveryLogData;

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
    // 2. But private methods (loadPushDeliveryLogXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private RevivePushDeliveryLogList(rawList: any[]): PushDeliveryLogData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePushDeliveryLog(raw));
  }

}
