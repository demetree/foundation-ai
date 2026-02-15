/*

   GENERATED SERVICE FOR THE BUILDMANUALCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BuildManualChangeHistory table.

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
import { BuildManualData } from './build-manual.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BuildManualChangeHistoryQueryParameters {
    buildManualId: bigint | number | null | undefined = null;
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
export class BuildManualChangeHistorySubmitData {
    id!: bigint | number;
    buildManualId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
}


export class BuildManualChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BuildManualChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `buildManualChangeHistory.BuildManualChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `buildManualChangeHistory.BuildManualChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="buildManualChangeHistory.BuildManualChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`buildManualChangeHistory.BuildManualChangeHistoryChildren` or `await buildManualChangeHistory.BuildManualChangeHistoryChildren`)
//    - Simply reading `buildManualChangeHistory.BuildManualChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await buildManualChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BuildManualChangeHistoryData {
    id!: bigint | number;
    buildManualId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
    buildManual: BuildManualData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any BuildManualChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.buildManualChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     buildManualChangeHistory[0].Reload().then(x => {
  //        this.buildManualChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BuildManualChangeHistoryService.Instance.GetBuildManualChangeHistory(this.id, includeRelations)
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
     * Updates the state of this BuildManualChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BuildManualChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BuildManualChangeHistorySubmitData {
        return BuildManualChangeHistoryService.Instance.ConvertToBuildManualChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BuildManualChangeHistoryService extends SecureEndpointBase {

    private static _instance: BuildManualChangeHistoryService;
    private listCache: Map<string, Observable<Array<BuildManualChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BuildManualChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<BuildManualChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BuildManualChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BuildManualChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<BuildManualChangeHistoryData>>();

        BuildManualChangeHistoryService._instance = this;
    }

    public static get Instance(): BuildManualChangeHistoryService {
      return BuildManualChangeHistoryService._instance;
    }


    public ClearListCaches(config: BuildManualChangeHistoryQueryParameters | null = null) {

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


    public ConvertToBuildManualChangeHistorySubmitData(data: BuildManualChangeHistoryData): BuildManualChangeHistorySubmitData {

        let output = new BuildManualChangeHistorySubmitData();

        output.id = data.id;
        output.buildManualId = data.buildManualId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetBuildManualChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<BuildManualChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const buildManualChangeHistory$ = this.requestBuildManualChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildManualChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, buildManualChangeHistory$);

            return buildManualChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<BuildManualChangeHistoryData>;
    }

    private requestBuildManualChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<BuildManualChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BuildManualChangeHistoryData>(this.baseUrl + 'api/BuildManualChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBuildManualChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualChangeHistory(id, includeRelations));
            }));
    }

    public GetBuildManualChangeHistoryList(config: BuildManualChangeHistoryQueryParameters | any = null) : Observable<Array<BuildManualChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const buildManualChangeHistoryList$ = this.requestBuildManualChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildManualChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, buildManualChangeHistoryList$);

            return buildManualChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BuildManualChangeHistoryData>>;
    }


    private requestBuildManualChangeHistoryList(config: BuildManualChangeHistoryQueryParameters | any) : Observable <Array<BuildManualChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildManualChangeHistoryData>>(this.baseUrl + 'api/BuildManualChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBuildManualChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualChangeHistoryList(config));
            }));
    }

    public GetBuildManualChangeHistoriesRowCount(config: BuildManualChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const buildManualChangeHistoriesRowCount$ = this.requestBuildManualChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildManualChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, buildManualChangeHistoriesRowCount$);

            return buildManualChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBuildManualChangeHistoriesRowCount(config: BuildManualChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BuildManualChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualChangeHistoriesRowCount(config));
            }));
    }

    public GetBuildManualChangeHistoriesBasicListData(config: BuildManualChangeHistoryQueryParameters | any = null) : Observable<Array<BuildManualChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const buildManualChangeHistoriesBasicListData$ = this.requestBuildManualChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildManualChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, buildManualChangeHistoriesBasicListData$);

            return buildManualChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BuildManualChangeHistoryBasicListData>>;
    }


    private requestBuildManualChangeHistoriesBasicListData(config: BuildManualChangeHistoryQueryParameters | any) : Observable<Array<BuildManualChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildManualChangeHistoryBasicListData>>(this.baseUrl + 'api/BuildManualChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualChangeHistoriesBasicListData(config));
            }));

    }


    public PutBuildManualChangeHistory(id: bigint | number, buildManualChangeHistory: BuildManualChangeHistorySubmitData) : Observable<BuildManualChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BuildManualChangeHistoryData>(this.baseUrl + 'api/BuildManualChangeHistory/' + id.toString(), buildManualChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildManualChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBuildManualChangeHistory(id, buildManualChangeHistory));
            }));
    }


    public PostBuildManualChangeHistory(buildManualChangeHistory: BuildManualChangeHistorySubmitData) : Observable<BuildManualChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BuildManualChangeHistoryData>(this.baseUrl + 'api/BuildManualChangeHistory', buildManualChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildManualChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBuildManualChangeHistory(buildManualChangeHistory));
            }));
    }

  
    public DeleteBuildManualChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BuildManualChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBuildManualChangeHistory(id));
            }));
    }


    private getConfigHash(config: BuildManualChangeHistoryQueryParameters | any): string {

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

    public userIsBMCBuildManualChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBuildManualChangeHistoryReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BuildManualChangeHistories
        //
        if (userIsBMCBuildManualChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBuildManualChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsBMCBuildManualChangeHistoryReader = false;
            }
        }

        return userIsBMCBuildManualChangeHistoryReader;
    }


    public userIsBMCBuildManualChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBuildManualChangeHistoryWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BuildManualChangeHistories
        //
        if (userIsBMCBuildManualChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBuildManualChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsBMCBuildManualChangeHistoryWriter = false;
          }      
        }

        return userIsBMCBuildManualChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BuildManualChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BuildManualChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BuildManualChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBuildManualChangeHistory(raw: any): BuildManualChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a BuildManualChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(BuildManualChangeHistoryData.prototype) as BuildManualChangeHistoryData;

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
    // 2. But private methods (loadBuildManualChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBuildManualChangeHistoryList(rawList: any[]): BuildManualChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBuildManualChangeHistory(raw));
  }

}
