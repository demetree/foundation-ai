/*

   GENERATED SERVICE FOR THE APIREQUESTLOG TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ApiRequestLog table.

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
import { ApiKeyData } from './api-key.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ApiRequestLogQueryParameters {
    apiKeyId: bigint | number | null | undefined = null;
    endpoint: string | null | undefined = null;
    httpMethod: string | null | undefined = null;
    responseStatus: bigint | number | null | undefined = null;
    requestDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    durationMs: bigint | number | null | undefined = null;
    clientIpAddress: string | null | undefined = null;
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
export class ApiRequestLogSubmitData {
    id!: bigint | number;
    apiKeyId!: bigint | number;
    endpoint!: string;
    httpMethod!: string;
    responseStatus!: bigint | number;
    requestDate!: string;      // ISO 8601 (full datetime)
    durationMs: bigint | number | null = null;
    clientIpAddress: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ApiRequestLogBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ApiRequestLogChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `apiRequestLog.ApiRequestLogChildren$` — use with `| async` in templates
//        • Promise:    `apiRequestLog.ApiRequestLogChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="apiRequestLog.ApiRequestLogChildren$ | async"`), or
//        • Access the promise getter (`apiRequestLog.ApiRequestLogChildren` or `await apiRequestLog.ApiRequestLogChildren`)
//    - Simply reading `apiRequestLog.ApiRequestLogChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await apiRequestLog.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ApiRequestLogData {
    id!: bigint | number;
    apiKeyId!: bigint | number;
    endpoint!: string;
    httpMethod!: string;
    responseStatus!: bigint | number;
    requestDate!: string;      // ISO 8601 (full datetime)
    durationMs!: bigint | number;
    clientIpAddress!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    apiKey: ApiKeyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ApiRequestLogData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.apiRequestLog.Reload();
  //
  //  Non Async:
  //
  //     apiRequestLog[0].Reload().then(x => {
  //        this.apiRequestLog = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ApiRequestLogService.Instance.GetApiRequestLog(this.id, includeRelations)
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
     * Updates the state of this ApiRequestLogData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ApiRequestLogData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ApiRequestLogSubmitData {
        return ApiRequestLogService.Instance.ConvertToApiRequestLogSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ApiRequestLogService extends SecureEndpointBase {

    private static _instance: ApiRequestLogService;
    private listCache: Map<string, Observable<Array<ApiRequestLogData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ApiRequestLogBasicListData>>>;
    private recordCache: Map<string, Observable<ApiRequestLogData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ApiRequestLogData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ApiRequestLogBasicListData>>>();
        this.recordCache = new Map<string, Observable<ApiRequestLogData>>();

        ApiRequestLogService._instance = this;
    }

    public static get Instance(): ApiRequestLogService {
      return ApiRequestLogService._instance;
    }


    public ClearListCaches(config: ApiRequestLogQueryParameters | null = null) {

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


    public ConvertToApiRequestLogSubmitData(data: ApiRequestLogData): ApiRequestLogSubmitData {

        let output = new ApiRequestLogSubmitData();

        output.id = data.id;
        output.apiKeyId = data.apiKeyId;
        output.endpoint = data.endpoint;
        output.httpMethod = data.httpMethod;
        output.responseStatus = data.responseStatus;
        output.requestDate = data.requestDate;
        output.durationMs = data.durationMs;
        output.clientIpAddress = data.clientIpAddress;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetApiRequestLog(id: bigint | number, includeRelations: boolean = true) : Observable<ApiRequestLogData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const apiRequestLog$ = this.requestApiRequestLog(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ApiRequestLog", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, apiRequestLog$);

            return apiRequestLog$;
        }

        return this.recordCache.get(configHash) as Observable<ApiRequestLogData>;
    }

    private requestApiRequestLog(id: bigint | number, includeRelations: boolean = true) : Observable<ApiRequestLogData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ApiRequestLogData>(this.baseUrl + 'api/ApiRequestLog/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveApiRequestLog(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestApiRequestLog(id, includeRelations));
            }));
    }

    public GetApiRequestLogList(config: ApiRequestLogQueryParameters | any = null) : Observable<Array<ApiRequestLogData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const apiRequestLogList$ = this.requestApiRequestLogList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ApiRequestLog list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, apiRequestLogList$);

            return apiRequestLogList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ApiRequestLogData>>;
    }


    private requestApiRequestLogList(config: ApiRequestLogQueryParameters | any) : Observable <Array<ApiRequestLogData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ApiRequestLogData>>(this.baseUrl + 'api/ApiRequestLogs', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveApiRequestLogList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestApiRequestLogList(config));
            }));
    }

    public GetApiRequestLogsRowCount(config: ApiRequestLogQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const apiRequestLogsRowCount$ = this.requestApiRequestLogsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ApiRequestLogs row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, apiRequestLogsRowCount$);

            return apiRequestLogsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestApiRequestLogsRowCount(config: ApiRequestLogQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ApiRequestLogs/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestApiRequestLogsRowCount(config));
            }));
    }

    public GetApiRequestLogsBasicListData(config: ApiRequestLogQueryParameters | any = null) : Observable<Array<ApiRequestLogBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const apiRequestLogsBasicListData$ = this.requestApiRequestLogsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ApiRequestLogs basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, apiRequestLogsBasicListData$);

            return apiRequestLogsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ApiRequestLogBasicListData>>;
    }


    private requestApiRequestLogsBasicListData(config: ApiRequestLogQueryParameters | any) : Observable<Array<ApiRequestLogBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ApiRequestLogBasicListData>>(this.baseUrl + 'api/ApiRequestLogs/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestApiRequestLogsBasicListData(config));
            }));

    }


    public PutApiRequestLog(id: bigint | number, apiRequestLog: ApiRequestLogSubmitData) : Observable<ApiRequestLogData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ApiRequestLogData>(this.baseUrl + 'api/ApiRequestLog/' + id.toString(), apiRequestLog, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveApiRequestLog(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutApiRequestLog(id, apiRequestLog));
            }));
    }


    public PostApiRequestLog(apiRequestLog: ApiRequestLogSubmitData) : Observable<ApiRequestLogData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ApiRequestLogData>(this.baseUrl + 'api/ApiRequestLog', apiRequestLog, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveApiRequestLog(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostApiRequestLog(apiRequestLog));
            }));
    }

  
    public DeleteApiRequestLog(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ApiRequestLog/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteApiRequestLog(id));
            }));
    }


    private getConfigHash(config: ApiRequestLogQueryParameters | any): string {

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

    public userIsBMCApiRequestLogReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCApiRequestLogReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ApiRequestLogs
        //
        if (userIsBMCApiRequestLogReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCApiRequestLogReader = user.readPermission >= 100;
            } else {
                userIsBMCApiRequestLogReader = false;
            }
        }

        return userIsBMCApiRequestLogReader;
    }


    public userIsBMCApiRequestLogWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCApiRequestLogWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ApiRequestLogs
        //
        if (userIsBMCApiRequestLogWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCApiRequestLogWriter = user.writePermission >= 255;
          } else {
            userIsBMCApiRequestLogWriter = false;
          }      
        }

        return userIsBMCApiRequestLogWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ApiRequestLogData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ApiRequestLogData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ApiRequestLogTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveApiRequestLog(raw: any): ApiRequestLogData {
    if (!raw) return raw;

    //
    // Create a ApiRequestLogData object instance with correct prototype
    //
    const revived = Object.create(ApiRequestLogData.prototype) as ApiRequestLogData;

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
    // 2. But private methods (loadApiRequestLogXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveApiRequestLogList(rawList: any[]): ApiRequestLogData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveApiRequestLog(raw));
  }

}
