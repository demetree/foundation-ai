/*

   GENERATED SERVICE FOR THE INTEGRATIONCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the IntegrationChangeHistory table.

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
import { IntegrationData } from './integration.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class IntegrationChangeHistoryQueryParameters {
    integrationId: bigint | number | null | undefined = null;
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
export class IntegrationChangeHistorySubmitData {
    id!: bigint | number;
    integrationId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
}


export class IntegrationChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. IntegrationChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `integrationChangeHistory.IntegrationChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `integrationChangeHistory.IntegrationChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="integrationChangeHistory.IntegrationChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`integrationChangeHistory.IntegrationChangeHistoryChildren` or `await integrationChangeHistory.IntegrationChangeHistoryChildren`)
//    - Simply reading `integrationChangeHistory.IntegrationChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await integrationChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class IntegrationChangeHistoryData {
    id!: bigint | number;
    integrationId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
    integration: IntegrationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any IntegrationChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.integrationChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     integrationChangeHistory[0].Reload().then(x => {
  //        this.integrationChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      IntegrationChangeHistoryService.Instance.GetIntegrationChangeHistory(this.id, includeRelations)
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
     * Updates the state of this IntegrationChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this IntegrationChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): IntegrationChangeHistorySubmitData {
        return IntegrationChangeHistoryService.Instance.ConvertToIntegrationChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class IntegrationChangeHistoryService extends SecureEndpointBase {

    private static _instance: IntegrationChangeHistoryService;
    private listCache: Map<string, Observable<Array<IntegrationChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<IntegrationChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<IntegrationChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<IntegrationChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<IntegrationChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<IntegrationChangeHistoryData>>();

        IntegrationChangeHistoryService._instance = this;
    }

    public static get Instance(): IntegrationChangeHistoryService {
      return IntegrationChangeHistoryService._instance;
    }


    public ClearListCaches(config: IntegrationChangeHistoryQueryParameters | null = null) {

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


    public ConvertToIntegrationChangeHistorySubmitData(data: IntegrationChangeHistoryData): IntegrationChangeHistorySubmitData {

        let output = new IntegrationChangeHistorySubmitData();

        output.id = data.id;
        output.integrationId = data.integrationId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetIntegrationChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<IntegrationChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const integrationChangeHistory$ = this.requestIntegrationChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IntegrationChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, integrationChangeHistory$);

            return integrationChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<IntegrationChangeHistoryData>;
    }

    private requestIntegrationChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<IntegrationChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IntegrationChangeHistoryData>(this.baseUrl + 'api/IntegrationChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveIntegrationChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestIntegrationChangeHistory(id, includeRelations));
            }));
    }

    public GetIntegrationChangeHistoryList(config: IntegrationChangeHistoryQueryParameters | any = null) : Observable<Array<IntegrationChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const integrationChangeHistoryList$ = this.requestIntegrationChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IntegrationChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, integrationChangeHistoryList$);

            return integrationChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<IntegrationChangeHistoryData>>;
    }


    private requestIntegrationChangeHistoryList(config: IntegrationChangeHistoryQueryParameters | any) : Observable <Array<IntegrationChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IntegrationChangeHistoryData>>(this.baseUrl + 'api/IntegrationChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveIntegrationChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestIntegrationChangeHistoryList(config));
            }));
    }

    public GetIntegrationChangeHistoriesRowCount(config: IntegrationChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const integrationChangeHistoriesRowCount$ = this.requestIntegrationChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IntegrationChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, integrationChangeHistoriesRowCount$);

            return integrationChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestIntegrationChangeHistoriesRowCount(config: IntegrationChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/IntegrationChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIntegrationChangeHistoriesRowCount(config));
            }));
    }

    public GetIntegrationChangeHistoriesBasicListData(config: IntegrationChangeHistoryQueryParameters | any = null) : Observable<Array<IntegrationChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const integrationChangeHistoriesBasicListData$ = this.requestIntegrationChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IntegrationChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, integrationChangeHistoriesBasicListData$);

            return integrationChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<IntegrationChangeHistoryBasicListData>>;
    }


    private requestIntegrationChangeHistoriesBasicListData(config: IntegrationChangeHistoryQueryParameters | any) : Observable<Array<IntegrationChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IntegrationChangeHistoryBasicListData>>(this.baseUrl + 'api/IntegrationChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIntegrationChangeHistoriesBasicListData(config));
            }));

    }


    public PutIntegrationChangeHistory(id: bigint | number, integrationChangeHistory: IntegrationChangeHistorySubmitData) : Observable<IntegrationChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IntegrationChangeHistoryData>(this.baseUrl + 'api/IntegrationChangeHistory/' + id.toString(), integrationChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIntegrationChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutIntegrationChangeHistory(id, integrationChangeHistory));
            }));
    }


    public PostIntegrationChangeHistory(integrationChangeHistory: IntegrationChangeHistorySubmitData) : Observable<IntegrationChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<IntegrationChangeHistoryData>(this.baseUrl + 'api/IntegrationChangeHistory', integrationChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIntegrationChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostIntegrationChangeHistory(integrationChangeHistory));
            }));
    }

  
    public DeleteIntegrationChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/IntegrationChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteIntegrationChangeHistory(id));
            }));
    }


    private getConfigHash(config: IntegrationChangeHistoryQueryParameters | any): string {

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

    public userIsAlertingIntegrationChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingIntegrationChangeHistoryReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.IntegrationChangeHistories
        //
        if (userIsAlertingIntegrationChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingIntegrationChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsAlertingIntegrationChangeHistoryReader = false;
            }
        }

        return userIsAlertingIntegrationChangeHistoryReader;
    }


    public userIsAlertingIntegrationChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingIntegrationChangeHistoryWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.IntegrationChangeHistories
        //
        if (userIsAlertingIntegrationChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingIntegrationChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsAlertingIntegrationChangeHistoryWriter = false;
          }      
        }

        return userIsAlertingIntegrationChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full IntegrationChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the IntegrationChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when IntegrationChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveIntegrationChangeHistory(raw: any): IntegrationChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a IntegrationChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(IntegrationChangeHistoryData.prototype) as IntegrationChangeHistoryData;

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
    // 2. But private methods (loadIntegrationChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveIntegrationChangeHistoryList(rawList: any[]): IntegrationChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveIntegrationChangeHistory(raw));
  }

}
