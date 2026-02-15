/*

   GENERATED SERVICE FOR THE GIFTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the GiftChangeHistory table.

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
import { GiftData } from './gift.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class GiftChangeHistoryQueryParameters {
    giftId: bigint | number | null | undefined = null;
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
export class GiftChangeHistorySubmitData {
    id!: bigint | number;
    giftId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
}


export class GiftChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. GiftChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `giftChangeHistory.GiftChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `giftChangeHistory.GiftChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="giftChangeHistory.GiftChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`giftChangeHistory.GiftChangeHistoryChildren` or `await giftChangeHistory.GiftChangeHistoryChildren`)
//    - Simply reading `giftChangeHistory.GiftChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await giftChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class GiftChangeHistoryData {
    id!: bigint | number;
    giftId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
    gift: GiftData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any GiftChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.giftChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     giftChangeHistory[0].Reload().then(x => {
  //        this.giftChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      GiftChangeHistoryService.Instance.GetGiftChangeHistory(this.id, includeRelations)
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
     * Updates the state of this GiftChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this GiftChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): GiftChangeHistorySubmitData {
        return GiftChangeHistoryService.Instance.ConvertToGiftChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class GiftChangeHistoryService extends SecureEndpointBase {

    private static _instance: GiftChangeHistoryService;
    private listCache: Map<string, Observable<Array<GiftChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<GiftChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<GiftChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<GiftChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<GiftChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<GiftChangeHistoryData>>();

        GiftChangeHistoryService._instance = this;
    }

    public static get Instance(): GiftChangeHistoryService {
      return GiftChangeHistoryService._instance;
    }


    public ClearListCaches(config: GiftChangeHistoryQueryParameters | null = null) {

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


    public ConvertToGiftChangeHistorySubmitData(data: GiftChangeHistoryData): GiftChangeHistorySubmitData {

        let output = new GiftChangeHistorySubmitData();

        output.id = data.id;
        output.giftId = data.giftId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetGiftChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<GiftChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const giftChangeHistory$ = this.requestGiftChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get GiftChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, giftChangeHistory$);

            return giftChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<GiftChangeHistoryData>;
    }

    private requestGiftChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<GiftChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<GiftChangeHistoryData>(this.baseUrl + 'api/GiftChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveGiftChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestGiftChangeHistory(id, includeRelations));
            }));
    }

    public GetGiftChangeHistoryList(config: GiftChangeHistoryQueryParameters | any = null) : Observable<Array<GiftChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const giftChangeHistoryList$ = this.requestGiftChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get GiftChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, giftChangeHistoryList$);

            return giftChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<GiftChangeHistoryData>>;
    }


    private requestGiftChangeHistoryList(config: GiftChangeHistoryQueryParameters | any) : Observable <Array<GiftChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<GiftChangeHistoryData>>(this.baseUrl + 'api/GiftChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveGiftChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestGiftChangeHistoryList(config));
            }));
    }

    public GetGiftChangeHistoriesRowCount(config: GiftChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const giftChangeHistoriesRowCount$ = this.requestGiftChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get GiftChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, giftChangeHistoriesRowCount$);

            return giftChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestGiftChangeHistoriesRowCount(config: GiftChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/GiftChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestGiftChangeHistoriesRowCount(config));
            }));
    }

    public GetGiftChangeHistoriesBasicListData(config: GiftChangeHistoryQueryParameters | any = null) : Observable<Array<GiftChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const giftChangeHistoriesBasicListData$ = this.requestGiftChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get GiftChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, giftChangeHistoriesBasicListData$);

            return giftChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<GiftChangeHistoryBasicListData>>;
    }


    private requestGiftChangeHistoriesBasicListData(config: GiftChangeHistoryQueryParameters | any) : Observable<Array<GiftChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<GiftChangeHistoryBasicListData>>(this.baseUrl + 'api/GiftChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestGiftChangeHistoriesBasicListData(config));
            }));

    }


    public PutGiftChangeHistory(id: bigint | number, giftChangeHistory: GiftChangeHistorySubmitData) : Observable<GiftChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<GiftChangeHistoryData>(this.baseUrl + 'api/GiftChangeHistory/' + id.toString(), giftChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveGiftChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutGiftChangeHistory(id, giftChangeHistory));
            }));
    }


    public PostGiftChangeHistory(giftChangeHistory: GiftChangeHistorySubmitData) : Observable<GiftChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<GiftChangeHistoryData>(this.baseUrl + 'api/GiftChangeHistory', giftChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveGiftChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostGiftChangeHistory(giftChangeHistory));
            }));
    }

  
    public DeleteGiftChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/GiftChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteGiftChangeHistory(id));
            }));
    }


    private getConfigHash(config: GiftChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerGiftChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerGiftChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.GiftChangeHistories
        //
        if (userIsSchedulerGiftChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerGiftChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerGiftChangeHistoryReader = false;
            }
        }

        return userIsSchedulerGiftChangeHistoryReader;
    }


    public userIsSchedulerGiftChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerGiftChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.GiftChangeHistories
        //
        if (userIsSchedulerGiftChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerGiftChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerGiftChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerGiftChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full GiftChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the GiftChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when GiftChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveGiftChangeHistory(raw: any): GiftChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a GiftChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(GiftChangeHistoryData.prototype) as GiftChangeHistoryData;

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
    // 2. But private methods (loadGiftChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveGiftChangeHistoryList(rawList: any[]): GiftChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveGiftChangeHistory(raw));
  }

}
