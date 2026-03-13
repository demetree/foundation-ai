/*

   GENERATED SERVICE FOR THE MOCVERSIONCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the MocVersionChangeHistory table.

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
import { MocVersionData } from './moc-version.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class MocVersionChangeHistoryQueryParameters {
    mocVersionId: bigint | number | null | undefined = null;
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
export class MocVersionChangeHistorySubmitData {
    id!: bigint | number;
    mocVersionId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
}


export class MocVersionChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. MocVersionChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `mocVersionChangeHistory.MocVersionChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `mocVersionChangeHistory.MocVersionChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="mocVersionChangeHistory.MocVersionChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`mocVersionChangeHistory.MocVersionChangeHistoryChildren` or `await mocVersionChangeHistory.MocVersionChangeHistoryChildren`)
//    - Simply reading `mocVersionChangeHistory.MocVersionChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await mocVersionChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class MocVersionChangeHistoryData {
    id!: bigint | number;
    mocVersionId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
    mocVersion: MocVersionData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any MocVersionChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.mocVersionChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     mocVersionChangeHistory[0].Reload().then(x => {
  //        this.mocVersionChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      MocVersionChangeHistoryService.Instance.GetMocVersionChangeHistory(this.id, includeRelations)
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
     * Updates the state of this MocVersionChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this MocVersionChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): MocVersionChangeHistorySubmitData {
        return MocVersionChangeHistoryService.Instance.ConvertToMocVersionChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class MocVersionChangeHistoryService extends SecureEndpointBase {

    private static _instance: MocVersionChangeHistoryService;
    private listCache: Map<string, Observable<Array<MocVersionChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<MocVersionChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<MocVersionChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<MocVersionChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<MocVersionChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<MocVersionChangeHistoryData>>();

        MocVersionChangeHistoryService._instance = this;
    }

    public static get Instance(): MocVersionChangeHistoryService {
      return MocVersionChangeHistoryService._instance;
    }


    public ClearListCaches(config: MocVersionChangeHistoryQueryParameters | null = null) {

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


    public ConvertToMocVersionChangeHistorySubmitData(data: MocVersionChangeHistoryData): MocVersionChangeHistorySubmitData {

        let output = new MocVersionChangeHistorySubmitData();

        output.id = data.id;
        output.mocVersionId = data.mocVersionId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetMocVersionChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<MocVersionChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const mocVersionChangeHistory$ = this.requestMocVersionChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MocVersionChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, mocVersionChangeHistory$);

            return mocVersionChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<MocVersionChangeHistoryData>;
    }

    private requestMocVersionChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<MocVersionChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<MocVersionChangeHistoryData>(this.baseUrl + 'api/MocVersionChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveMocVersionChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestMocVersionChangeHistory(id, includeRelations));
            }));
    }

    public GetMocVersionChangeHistoryList(config: MocVersionChangeHistoryQueryParameters | any = null) : Observable<Array<MocVersionChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const mocVersionChangeHistoryList$ = this.requestMocVersionChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MocVersionChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, mocVersionChangeHistoryList$);

            return mocVersionChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<MocVersionChangeHistoryData>>;
    }


    private requestMocVersionChangeHistoryList(config: MocVersionChangeHistoryQueryParameters | any) : Observable <Array<MocVersionChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MocVersionChangeHistoryData>>(this.baseUrl + 'api/MocVersionChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveMocVersionChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestMocVersionChangeHistoryList(config));
            }));
    }

    public GetMocVersionChangeHistoriesRowCount(config: MocVersionChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const mocVersionChangeHistoriesRowCount$ = this.requestMocVersionChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MocVersionChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, mocVersionChangeHistoriesRowCount$);

            return mocVersionChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestMocVersionChangeHistoriesRowCount(config: MocVersionChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/MocVersionChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMocVersionChangeHistoriesRowCount(config));
            }));
    }

    public GetMocVersionChangeHistoriesBasicListData(config: MocVersionChangeHistoryQueryParameters | any = null) : Observable<Array<MocVersionChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const mocVersionChangeHistoriesBasicListData$ = this.requestMocVersionChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MocVersionChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, mocVersionChangeHistoriesBasicListData$);

            return mocVersionChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<MocVersionChangeHistoryBasicListData>>;
    }


    private requestMocVersionChangeHistoriesBasicListData(config: MocVersionChangeHistoryQueryParameters | any) : Observable<Array<MocVersionChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MocVersionChangeHistoryBasicListData>>(this.baseUrl + 'api/MocVersionChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMocVersionChangeHistoriesBasicListData(config));
            }));

    }


    public PutMocVersionChangeHistory(id: bigint | number, mocVersionChangeHistory: MocVersionChangeHistorySubmitData) : Observable<MocVersionChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<MocVersionChangeHistoryData>(this.baseUrl + 'api/MocVersionChangeHistory/' + id.toString(), mocVersionChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMocVersionChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutMocVersionChangeHistory(id, mocVersionChangeHistory));
            }));
    }


    public PostMocVersionChangeHistory(mocVersionChangeHistory: MocVersionChangeHistorySubmitData) : Observable<MocVersionChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<MocVersionChangeHistoryData>(this.baseUrl + 'api/MocVersionChangeHistory', mocVersionChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMocVersionChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostMocVersionChangeHistory(mocVersionChangeHistory));
            }));
    }

  
    public DeleteMocVersionChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/MocVersionChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteMocVersionChangeHistory(id));
            }));
    }


    private getConfigHash(config: MocVersionChangeHistoryQueryParameters | any): string {

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

    public userIsBMCMocVersionChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCMocVersionChangeHistoryReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.MocVersionChangeHistories
        //
        if (userIsBMCMocVersionChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCMocVersionChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsBMCMocVersionChangeHistoryReader = false;
            }
        }

        return userIsBMCMocVersionChangeHistoryReader;
    }


    public userIsBMCMocVersionChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCMocVersionChangeHistoryWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.MocVersionChangeHistories
        //
        if (userIsBMCMocVersionChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCMocVersionChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsBMCMocVersionChangeHistoryWriter = false;
          }      
        }

        return userIsBMCMocVersionChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full MocVersionChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the MocVersionChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when MocVersionChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveMocVersionChangeHistory(raw: any): MocVersionChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a MocVersionChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(MocVersionChangeHistoryData.prototype) as MocVersionChangeHistoryData;

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
    // 2. But private methods (loadMocVersionChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveMocVersionChangeHistoryList(rawList: any[]): MocVersionChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveMocVersionChangeHistory(raw));
  }

}
