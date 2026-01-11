/*

   GENERATED SERVICE FOR THE CHARGETYPECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ChargeTypeChangeHistory table.

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
import { ChargeTypeData } from './charge-type.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ChargeTypeChangeHistoryQueryParameters {
    chargeTypeId: bigint | number | null | undefined = null;
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
export class ChargeTypeChangeHistorySubmitData {
    id!: bigint | number;
    chargeTypeId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
}


export class ChargeTypeChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ChargeTypeChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `chargeTypeChangeHistory.ChargeTypeChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `chargeTypeChangeHistory.ChargeTypeChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="chargeTypeChangeHistory.ChargeTypeChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`chargeTypeChangeHistory.ChargeTypeChangeHistoryChildren` or `await chargeTypeChangeHistory.ChargeTypeChangeHistoryChildren`)
//    - Simply reading `chargeTypeChangeHistory.ChargeTypeChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await chargeTypeChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ChargeTypeChangeHistoryData {
    id!: bigint | number;
    chargeTypeId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
    chargeType: ChargeTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ChargeTypeChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.chargeTypeChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     chargeTypeChangeHistory[0].Reload().then(x => {
  //        this.chargeTypeChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ChargeTypeChangeHistoryService.Instance.GetChargeTypeChangeHistory(this.id, includeRelations)
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
     * Updates the state of this ChargeTypeChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ChargeTypeChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ChargeTypeChangeHistorySubmitData {
        return ChargeTypeChangeHistoryService.Instance.ConvertToChargeTypeChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ChargeTypeChangeHistoryService extends SecureEndpointBase {

    private static _instance: ChargeTypeChangeHistoryService;
    private listCache: Map<string, Observable<Array<ChargeTypeChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ChargeTypeChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<ChargeTypeChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ChargeTypeChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ChargeTypeChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<ChargeTypeChangeHistoryData>>();

        ChargeTypeChangeHistoryService._instance = this;
    }

    public static get Instance(): ChargeTypeChangeHistoryService {
      return ChargeTypeChangeHistoryService._instance;
    }


    public ClearListCaches(config: ChargeTypeChangeHistoryQueryParameters | null = null) {

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


    public ConvertToChargeTypeChangeHistorySubmitData(data: ChargeTypeChangeHistoryData): ChargeTypeChangeHistorySubmitData {

        let output = new ChargeTypeChangeHistorySubmitData();

        output.id = data.id;
        output.chargeTypeId = data.chargeTypeId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetChargeTypeChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ChargeTypeChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const chargeTypeChangeHistory$ = this.requestChargeTypeChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ChargeTypeChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, chargeTypeChangeHistory$);

            return chargeTypeChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<ChargeTypeChangeHistoryData>;
    }

    private requestChargeTypeChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ChargeTypeChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ChargeTypeChangeHistoryData>(this.baseUrl + 'api/ChargeTypeChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveChargeTypeChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestChargeTypeChangeHistory(id, includeRelations));
            }));
    }

    public GetChargeTypeChangeHistoryList(config: ChargeTypeChangeHistoryQueryParameters | any = null) : Observable<Array<ChargeTypeChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const chargeTypeChangeHistoryList$ = this.requestChargeTypeChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ChargeTypeChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, chargeTypeChangeHistoryList$);

            return chargeTypeChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ChargeTypeChangeHistoryData>>;
    }


    private requestChargeTypeChangeHistoryList(config: ChargeTypeChangeHistoryQueryParameters | any) : Observable <Array<ChargeTypeChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ChargeTypeChangeHistoryData>>(this.baseUrl + 'api/ChargeTypeChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveChargeTypeChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestChargeTypeChangeHistoryList(config));
            }));
    }

    public GetChargeTypeChangeHistoriesRowCount(config: ChargeTypeChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const chargeTypeChangeHistoriesRowCount$ = this.requestChargeTypeChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ChargeTypeChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, chargeTypeChangeHistoriesRowCount$);

            return chargeTypeChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestChargeTypeChangeHistoriesRowCount(config: ChargeTypeChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ChargeTypeChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestChargeTypeChangeHistoriesRowCount(config));
            }));
    }

    public GetChargeTypeChangeHistoriesBasicListData(config: ChargeTypeChangeHistoryQueryParameters | any = null) : Observable<Array<ChargeTypeChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const chargeTypeChangeHistoriesBasicListData$ = this.requestChargeTypeChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ChargeTypeChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, chargeTypeChangeHistoriesBasicListData$);

            return chargeTypeChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ChargeTypeChangeHistoryBasicListData>>;
    }


    private requestChargeTypeChangeHistoriesBasicListData(config: ChargeTypeChangeHistoryQueryParameters | any) : Observable<Array<ChargeTypeChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ChargeTypeChangeHistoryBasicListData>>(this.baseUrl + 'api/ChargeTypeChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestChargeTypeChangeHistoriesBasicListData(config));
            }));

    }


    public PutChargeTypeChangeHistory(id: bigint | number, chargeTypeChangeHistory: ChargeTypeChangeHistorySubmitData) : Observable<ChargeTypeChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ChargeTypeChangeHistoryData>(this.baseUrl + 'api/ChargeTypeChangeHistory/' + id.toString(), chargeTypeChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveChargeTypeChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutChargeTypeChangeHistory(id, chargeTypeChangeHistory));
            }));
    }


    public PostChargeTypeChangeHistory(chargeTypeChangeHistory: ChargeTypeChangeHistorySubmitData) : Observable<ChargeTypeChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ChargeTypeChangeHistoryData>(this.baseUrl + 'api/ChargeTypeChangeHistory', chargeTypeChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveChargeTypeChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostChargeTypeChangeHistory(chargeTypeChangeHistory));
            }));
    }

  
    public DeleteChargeTypeChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ChargeTypeChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteChargeTypeChangeHistory(id));
            }));
    }


    private getConfigHash(config: ChargeTypeChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerChargeTypeChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerChargeTypeChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ChargeTypeChangeHistories
        //
        if (userIsSchedulerChargeTypeChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerChargeTypeChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerChargeTypeChangeHistoryReader = false;
            }
        }

        return userIsSchedulerChargeTypeChangeHistoryReader;
    }


    public userIsSchedulerChargeTypeChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerChargeTypeChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ChargeTypeChangeHistories
        //
        if (userIsSchedulerChargeTypeChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerChargeTypeChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerChargeTypeChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerChargeTypeChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ChargeTypeChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ChargeTypeChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ChargeTypeChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveChargeTypeChangeHistory(raw: any): ChargeTypeChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a ChargeTypeChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(ChargeTypeChangeHistoryData.prototype) as ChargeTypeChangeHistoryData;

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
    // 2. But private methods (loadChargeTypeChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveChargeTypeChangeHistoryList(rawList: any[]): ChargeTypeChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveChargeTypeChangeHistory(raw));
  }

}
