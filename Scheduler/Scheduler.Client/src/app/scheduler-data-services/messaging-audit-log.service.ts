/*

   GENERATED SERVICE FOR THE MESSAGINGAUDITLOG TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the MessagingAuditLog table.

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
export class MessagingAuditLogQueryParameters {
    performedByUserId: bigint | number | null | undefined = null;
    action: string | null | undefined = null;
    entityType: string | null | undefined = null;
    entityId: bigint | number | null | undefined = null;
    details: string | null | undefined = null;
    ipAddress: string | null | undefined = null;
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
export class MessagingAuditLogSubmitData {
    id!: bigint | number;
    performedByUserId!: bigint | number;
    action!: string;
    entityType: string | null = null;
    entityId: bigint | number | null = null;
    details: string | null = null;
    ipAddress: string | null = null;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    active!: boolean;
    deleted!: boolean;
}


export class MessagingAuditLogBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. MessagingAuditLogChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `messagingAuditLog.MessagingAuditLogChildren$` — use with `| async` in templates
//        • Promise:    `messagingAuditLog.MessagingAuditLogChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="messagingAuditLog.MessagingAuditLogChildren$ | async"`), or
//        • Access the promise getter (`messagingAuditLog.MessagingAuditLogChildren` or `await messagingAuditLog.MessagingAuditLogChildren`)
//    - Simply reading `messagingAuditLog.MessagingAuditLogChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await messagingAuditLog.Reload()` to refresh the entire object and clear all lazy caches.
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
export class MessagingAuditLogData {
    id!: bigint | number;
    performedByUserId!: bigint | number;
    action!: string;
    entityType!: string | null;
    entityId!: bigint | number;
    details!: string | null;
    ipAddress!: string | null;
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
  // Promise based reload method to allow rebuilding of any MessagingAuditLogData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.messagingAuditLog.Reload();
  //
  //  Non Async:
  //
  //     messagingAuditLog[0].Reload().then(x => {
  //        this.messagingAuditLog = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      MessagingAuditLogService.Instance.GetMessagingAuditLog(this.id, includeRelations)
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
     * Updates the state of this MessagingAuditLogData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this MessagingAuditLogData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): MessagingAuditLogSubmitData {
        return MessagingAuditLogService.Instance.ConvertToMessagingAuditLogSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class MessagingAuditLogService extends SecureEndpointBase {

    private static _instance: MessagingAuditLogService;
    private listCache: Map<string, Observable<Array<MessagingAuditLogData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<MessagingAuditLogBasicListData>>>;
    private recordCache: Map<string, Observable<MessagingAuditLogData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<MessagingAuditLogData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<MessagingAuditLogBasicListData>>>();
        this.recordCache = new Map<string, Observable<MessagingAuditLogData>>();

        MessagingAuditLogService._instance = this;
    }

    public static get Instance(): MessagingAuditLogService {
      return MessagingAuditLogService._instance;
    }


    public ClearListCaches(config: MessagingAuditLogQueryParameters | null = null) {

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


    public ConvertToMessagingAuditLogSubmitData(data: MessagingAuditLogData): MessagingAuditLogSubmitData {

        let output = new MessagingAuditLogSubmitData();

        output.id = data.id;
        output.performedByUserId = data.performedByUserId;
        output.action = data.action;
        output.entityType = data.entityType;
        output.entityId = data.entityId;
        output.details = data.details;
        output.ipAddress = data.ipAddress;
        output.dateTimeCreated = data.dateTimeCreated;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetMessagingAuditLog(id: bigint | number, includeRelations: boolean = true) : Observable<MessagingAuditLogData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const messagingAuditLog$ = this.requestMessagingAuditLog(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MessagingAuditLog", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, messagingAuditLog$);

            return messagingAuditLog$;
        }

        return this.recordCache.get(configHash) as Observable<MessagingAuditLogData>;
    }

    private requestMessagingAuditLog(id: bigint | number, includeRelations: boolean = true) : Observable<MessagingAuditLogData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<MessagingAuditLogData>(this.baseUrl + 'api/MessagingAuditLog/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveMessagingAuditLog(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestMessagingAuditLog(id, includeRelations));
            }));
    }

    public GetMessagingAuditLogList(config: MessagingAuditLogQueryParameters | any = null) : Observable<Array<MessagingAuditLogData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const messagingAuditLogList$ = this.requestMessagingAuditLogList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MessagingAuditLog list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, messagingAuditLogList$);

            return messagingAuditLogList$;
        }

        return this.listCache.get(configHash) as Observable<Array<MessagingAuditLogData>>;
    }


    private requestMessagingAuditLogList(config: MessagingAuditLogQueryParameters | any) : Observable <Array<MessagingAuditLogData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MessagingAuditLogData>>(this.baseUrl + 'api/MessagingAuditLogs', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveMessagingAuditLogList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestMessagingAuditLogList(config));
            }));
    }

    public GetMessagingAuditLogsRowCount(config: MessagingAuditLogQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const messagingAuditLogsRowCount$ = this.requestMessagingAuditLogsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MessagingAuditLogs row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, messagingAuditLogsRowCount$);

            return messagingAuditLogsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestMessagingAuditLogsRowCount(config: MessagingAuditLogQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/MessagingAuditLogs/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMessagingAuditLogsRowCount(config));
            }));
    }

    public GetMessagingAuditLogsBasicListData(config: MessagingAuditLogQueryParameters | any = null) : Observable<Array<MessagingAuditLogBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const messagingAuditLogsBasicListData$ = this.requestMessagingAuditLogsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MessagingAuditLogs basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, messagingAuditLogsBasicListData$);

            return messagingAuditLogsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<MessagingAuditLogBasicListData>>;
    }


    private requestMessagingAuditLogsBasicListData(config: MessagingAuditLogQueryParameters | any) : Observable<Array<MessagingAuditLogBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MessagingAuditLogBasicListData>>(this.baseUrl + 'api/MessagingAuditLogs/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMessagingAuditLogsBasicListData(config));
            }));

    }


    public PutMessagingAuditLog(id: bigint | number, messagingAuditLog: MessagingAuditLogSubmitData) : Observable<MessagingAuditLogData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<MessagingAuditLogData>(this.baseUrl + 'api/MessagingAuditLog/' + id.toString(), messagingAuditLog, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMessagingAuditLog(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutMessagingAuditLog(id, messagingAuditLog));
            }));
    }


    public PostMessagingAuditLog(messagingAuditLog: MessagingAuditLogSubmitData) : Observable<MessagingAuditLogData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<MessagingAuditLogData>(this.baseUrl + 'api/MessagingAuditLog', messagingAuditLog, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMessagingAuditLog(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostMessagingAuditLog(messagingAuditLog));
            }));
    }

  
    public DeleteMessagingAuditLog(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/MessagingAuditLog/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteMessagingAuditLog(id));
            }));
    }


    private getConfigHash(config: MessagingAuditLogQueryParameters | any): string {

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

    public userIsSchedulerMessagingAuditLogReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerMessagingAuditLogReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.MessagingAuditLogs
        //
        if (userIsSchedulerMessagingAuditLogReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerMessagingAuditLogReader = user.readPermission >= 50;
            } else {
                userIsSchedulerMessagingAuditLogReader = false;
            }
        }

        return userIsSchedulerMessagingAuditLogReader;
    }


    public userIsSchedulerMessagingAuditLogWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerMessagingAuditLogWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.MessagingAuditLogs
        //
        if (userIsSchedulerMessagingAuditLogWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerMessagingAuditLogWriter = user.writePermission >= 100;
          } else {
            userIsSchedulerMessagingAuditLogWriter = false;
          }      
        }

        return userIsSchedulerMessagingAuditLogWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full MessagingAuditLogData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the MessagingAuditLogData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when MessagingAuditLogTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveMessagingAuditLog(raw: any): MessagingAuditLogData {
    if (!raw) return raw;

    //
    // Create a MessagingAuditLogData object instance with correct prototype
    //
    const revived = Object.create(MessagingAuditLogData.prototype) as MessagingAuditLogData;

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
    // 2. But private methods (loadMessagingAuditLogXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveMessagingAuditLogList(rawList: any[]): MessagingAuditLogData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveMessagingAuditLog(raw));
  }

}
