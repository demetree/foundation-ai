/*

   GENERATED SERVICE FOR THE APIKEY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ApiKey table.

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
import { ApiRequestLogService, ApiRequestLogData } from './api-request-log.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ApiKeyQueryParameters {
    keyHash: string | null | undefined = null;
    keyPrefix: string | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    isActive: boolean | null | undefined = null;
    createdDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    lastUsedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    expiresDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    rateLimitPerHour: bigint | number | null | undefined = null;
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
export class ApiKeySubmitData {
    id!: bigint | number;
    keyHash!: string;
    keyPrefix!: string;
    name!: string;
    description: string | null = null;
    isActive!: boolean;
    createdDate!: string;      // ISO 8601 (full datetime)
    lastUsedDate: string | null = null;     // ISO 8601 (full datetime)
    expiresDate: string | null = null;     // ISO 8601 (full datetime)
    rateLimitPerHour!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class ApiKeyBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ApiKeyChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `apiKey.ApiKeyChildren$` — use with `| async` in templates
//        • Promise:    `apiKey.ApiKeyChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="apiKey.ApiKeyChildren$ | async"`), or
//        • Access the promise getter (`apiKey.ApiKeyChildren` or `await apiKey.ApiKeyChildren`)
//    - Simply reading `apiKey.ApiKeyChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await apiKey.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ApiKeyData {
    id!: bigint | number;
    keyHash!: string;
    keyPrefix!: string;
    name!: string;
    description!: string | null;
    isActive!: boolean;
    createdDate!: string;      // ISO 8601 (full datetime)
    lastUsedDate!: string | null;   // ISO 8601 (full datetime)
    expiresDate!: string | null;   // ISO 8601 (full datetime)
    rateLimitPerHour!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _apiRequestLogs: ApiRequestLogData[] | null = null;
    private _apiRequestLogsPromise: Promise<ApiRequestLogData[]> | null  = null;
    private _apiRequestLogsSubject = new BehaviorSubject<ApiRequestLogData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ApiRequestLogs$ = this._apiRequestLogsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._apiRequestLogs === null && this._apiRequestLogsPromise === null) {
            this.loadApiRequestLogs(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ApiRequestLogsCount$ = ApiRequestLogService.Instance.GetApiRequestLogsRowCount({apiKeyId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ApiKeyData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.apiKey.Reload();
  //
  //  Non Async:
  //
  //     apiKey[0].Reload().then(x => {
  //        this.apiKey = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ApiKeyService.Instance.GetApiKey(this.id, includeRelations)
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
     this._apiRequestLogs = null;
     this._apiRequestLogsPromise = null;
     this._apiRequestLogsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ApiRequestLogs for this ApiKey.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.apiKey.ApiRequestLogs.then(apiKeys => { ... })
     *   or
     *   await this.apiKey.apiKeys
     *
    */
    public get ApiRequestLogs(): Promise<ApiRequestLogData[]> {
        if (this._apiRequestLogs !== null) {
            return Promise.resolve(this._apiRequestLogs);
        }

        if (this._apiRequestLogsPromise !== null) {
            return this._apiRequestLogsPromise;
        }

        // Start the load
        this.loadApiRequestLogs();

        return this._apiRequestLogsPromise!;
    }



    private loadApiRequestLogs(): void {

        this._apiRequestLogsPromise = lastValueFrom(
            ApiKeyService.Instance.GetApiRequestLogsForApiKey(this.id)
        )
        .then(ApiRequestLogs => {
            this._apiRequestLogs = ApiRequestLogs ?? [];
            this._apiRequestLogsSubject.next(this._apiRequestLogs);
            return this._apiRequestLogs;
         })
        .catch(err => {
            this._apiRequestLogs = [];
            this._apiRequestLogsSubject.next(this._apiRequestLogs);
            throw err;
        })
        .finally(() => {
            this._apiRequestLogsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ApiRequestLog. Call after mutations to force refresh.
     */
    public ClearApiRequestLogsCache(): void {
        this._apiRequestLogs = null;
        this._apiRequestLogsPromise = null;
        this._apiRequestLogsSubject.next(this._apiRequestLogs);      // Emit to observable
    }

    public get HasApiRequestLogs(): Promise<boolean> {
        return this.ApiRequestLogs.then(apiRequestLogs => apiRequestLogs.length > 0);
    }




    /**
     * Updates the state of this ApiKeyData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ApiKeyData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ApiKeySubmitData {
        return ApiKeyService.Instance.ConvertToApiKeySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ApiKeyService extends SecureEndpointBase {

    private static _instance: ApiKeyService;
    private listCache: Map<string, Observable<Array<ApiKeyData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ApiKeyBasicListData>>>;
    private recordCache: Map<string, Observable<ApiKeyData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private apiRequestLogService: ApiRequestLogService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ApiKeyData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ApiKeyBasicListData>>>();
        this.recordCache = new Map<string, Observable<ApiKeyData>>();

        ApiKeyService._instance = this;
    }

    public static get Instance(): ApiKeyService {
      return ApiKeyService._instance;
    }


    public ClearListCaches(config: ApiKeyQueryParameters | null = null) {

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


    public ConvertToApiKeySubmitData(data: ApiKeyData): ApiKeySubmitData {

        let output = new ApiKeySubmitData();

        output.id = data.id;
        output.keyHash = data.keyHash;
        output.keyPrefix = data.keyPrefix;
        output.name = data.name;
        output.description = data.description;
        output.isActive = data.isActive;
        output.createdDate = data.createdDate;
        output.lastUsedDate = data.lastUsedDate;
        output.expiresDate = data.expiresDate;
        output.rateLimitPerHour = data.rateLimitPerHour;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetApiKey(id: bigint | number, includeRelations: boolean = true) : Observable<ApiKeyData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const apiKey$ = this.requestApiKey(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ApiKey", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, apiKey$);

            return apiKey$;
        }

        return this.recordCache.get(configHash) as Observable<ApiKeyData>;
    }

    private requestApiKey(id: bigint | number, includeRelations: boolean = true) : Observable<ApiKeyData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ApiKeyData>(this.baseUrl + 'api/ApiKey/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveApiKey(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestApiKey(id, includeRelations));
            }));
    }

    public GetApiKeyList(config: ApiKeyQueryParameters | any = null) : Observable<Array<ApiKeyData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const apiKeyList$ = this.requestApiKeyList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ApiKey list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, apiKeyList$);

            return apiKeyList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ApiKeyData>>;
    }


    private requestApiKeyList(config: ApiKeyQueryParameters | any) : Observable <Array<ApiKeyData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ApiKeyData>>(this.baseUrl + 'api/ApiKeys', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveApiKeyList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestApiKeyList(config));
            }));
    }

    public GetApiKeysRowCount(config: ApiKeyQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const apiKeysRowCount$ = this.requestApiKeysRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ApiKeys row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, apiKeysRowCount$);

            return apiKeysRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestApiKeysRowCount(config: ApiKeyQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ApiKeys/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestApiKeysRowCount(config));
            }));
    }

    public GetApiKeysBasicListData(config: ApiKeyQueryParameters | any = null) : Observable<Array<ApiKeyBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const apiKeysBasicListData$ = this.requestApiKeysBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ApiKeys basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, apiKeysBasicListData$);

            return apiKeysBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ApiKeyBasicListData>>;
    }


    private requestApiKeysBasicListData(config: ApiKeyQueryParameters | any) : Observable<Array<ApiKeyBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ApiKeyBasicListData>>(this.baseUrl + 'api/ApiKeys/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestApiKeysBasicListData(config));
            }));

    }


    public PutApiKey(id: bigint | number, apiKey: ApiKeySubmitData) : Observable<ApiKeyData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ApiKeyData>(this.baseUrl + 'api/ApiKey/' + id.toString(), apiKey, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveApiKey(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutApiKey(id, apiKey));
            }));
    }


    public PostApiKey(apiKey: ApiKeySubmitData) : Observable<ApiKeyData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ApiKeyData>(this.baseUrl + 'api/ApiKey', apiKey, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveApiKey(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostApiKey(apiKey));
            }));
    }

  
    public DeleteApiKey(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ApiKey/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteApiKey(id));
            }));
    }


    private getConfigHash(config: ApiKeyQueryParameters | any): string {

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

    public userIsBMCApiKeyReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCApiKeyReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ApiKeys
        //
        if (userIsBMCApiKeyReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCApiKeyReader = user.readPermission >= 1;
            } else {
                userIsBMCApiKeyReader = false;
            }
        }

        return userIsBMCApiKeyReader;
    }


    public userIsBMCApiKeyWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCApiKeyWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ApiKeys
        //
        if (userIsBMCApiKeyWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCApiKeyWriter = user.writePermission >= 1;
          } else {
            userIsBMCApiKeyWriter = false;
          }      
        }

        return userIsBMCApiKeyWriter;
    }

    public GetApiRequestLogsForApiKey(apiKeyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ApiRequestLogData[]> {
        return this.apiRequestLogService.GetApiRequestLogList({
            apiKeyId: apiKeyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ApiKeyData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ApiKeyData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ApiKeyTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveApiKey(raw: any): ApiKeyData {
    if (!raw) return raw;

    //
    // Create a ApiKeyData object instance with correct prototype
    //
    const revived = Object.create(ApiKeyData.prototype) as ApiKeyData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._apiRequestLogs = null;
    (revived as any)._apiRequestLogsPromise = null;
    (revived as any)._apiRequestLogsSubject = new BehaviorSubject<ApiRequestLogData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadApiKeyXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ApiRequestLogs$ = (revived as any)._apiRequestLogsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._apiRequestLogs === null && (revived as any)._apiRequestLogsPromise === null) {
                (revived as any).loadApiRequestLogs();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ApiRequestLogsCount$ = ApiRequestLogService.Instance.GetApiRequestLogsRowCount({apiKeyId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveApiKeyList(rawList: any[]): ApiKeyData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveApiKey(raw));
  }

}
