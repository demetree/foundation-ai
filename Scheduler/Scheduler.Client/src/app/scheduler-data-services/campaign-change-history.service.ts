/*

   GENERATED SERVICE FOR THE CAMPAIGNCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the CampaignChangeHistory table.

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
import { CampaignData } from './campaign.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class CampaignChangeHistoryQueryParameters {
    campaignId: bigint | number | null | undefined = null;
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
export class CampaignChangeHistorySubmitData {
    id!: bigint | number;
    campaignId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
}


export class CampaignChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. CampaignChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `campaignChangeHistory.CampaignChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `campaignChangeHistory.CampaignChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="campaignChangeHistory.CampaignChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`campaignChangeHistory.CampaignChangeHistoryChildren` or `await campaignChangeHistory.CampaignChangeHistoryChildren`)
//    - Simply reading `campaignChangeHistory.CampaignChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await campaignChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class CampaignChangeHistoryData {
    id!: bigint | number;
    campaignId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
    campaign: CampaignData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any CampaignChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.campaignChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     campaignChangeHistory[0].Reload().then(x => {
  //        this.campaignChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      CampaignChangeHistoryService.Instance.GetCampaignChangeHistory(this.id, includeRelations)
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
     * Updates the state of this CampaignChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this CampaignChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): CampaignChangeHistorySubmitData {
        return CampaignChangeHistoryService.Instance.ConvertToCampaignChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class CampaignChangeHistoryService extends SecureEndpointBase {

    private static _instance: CampaignChangeHistoryService;
    private listCache: Map<string, Observable<Array<CampaignChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<CampaignChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<CampaignChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<CampaignChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<CampaignChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<CampaignChangeHistoryData>>();

        CampaignChangeHistoryService._instance = this;
    }

    public static get Instance(): CampaignChangeHistoryService {
      return CampaignChangeHistoryService._instance;
    }


    public ClearListCaches(config: CampaignChangeHistoryQueryParameters | null = null) {

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


    public ConvertToCampaignChangeHistorySubmitData(data: CampaignChangeHistoryData): CampaignChangeHistorySubmitData {

        let output = new CampaignChangeHistorySubmitData();

        output.id = data.id;
        output.campaignId = data.campaignId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetCampaignChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<CampaignChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const campaignChangeHistory$ = this.requestCampaignChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get CampaignChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, campaignChangeHistory$);

            return campaignChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<CampaignChangeHistoryData>;
    }

    private requestCampaignChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<CampaignChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CampaignChangeHistoryData>(this.baseUrl + 'api/CampaignChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveCampaignChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestCampaignChangeHistory(id, includeRelations));
            }));
    }

    public GetCampaignChangeHistoryList(config: CampaignChangeHistoryQueryParameters | any = null) : Observable<Array<CampaignChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const campaignChangeHistoryList$ = this.requestCampaignChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get CampaignChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, campaignChangeHistoryList$);

            return campaignChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<CampaignChangeHistoryData>>;
    }


    private requestCampaignChangeHistoryList(config: CampaignChangeHistoryQueryParameters | any) : Observable <Array<CampaignChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CampaignChangeHistoryData>>(this.baseUrl + 'api/CampaignChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveCampaignChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestCampaignChangeHistoryList(config));
            }));
    }

    public GetCampaignChangeHistoriesRowCount(config: CampaignChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const campaignChangeHistoriesRowCount$ = this.requestCampaignChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get CampaignChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, campaignChangeHistoriesRowCount$);

            return campaignChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestCampaignChangeHistoriesRowCount(config: CampaignChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/CampaignChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCampaignChangeHistoriesRowCount(config));
            }));
    }

    public GetCampaignChangeHistoriesBasicListData(config: CampaignChangeHistoryQueryParameters | any = null) : Observable<Array<CampaignChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const campaignChangeHistoriesBasicListData$ = this.requestCampaignChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get CampaignChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, campaignChangeHistoriesBasicListData$);

            return campaignChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<CampaignChangeHistoryBasicListData>>;
    }


    private requestCampaignChangeHistoriesBasicListData(config: CampaignChangeHistoryQueryParameters | any) : Observable<Array<CampaignChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CampaignChangeHistoryBasicListData>>(this.baseUrl + 'api/CampaignChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCampaignChangeHistoriesBasicListData(config));
            }));

    }


    public PutCampaignChangeHistory(id: bigint | number, campaignChangeHistory: CampaignChangeHistorySubmitData) : Observable<CampaignChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CampaignChangeHistoryData>(this.baseUrl + 'api/CampaignChangeHistory/' + id.toString(), campaignChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCampaignChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutCampaignChangeHistory(id, campaignChangeHistory));
            }));
    }


    public PostCampaignChangeHistory(campaignChangeHistory: CampaignChangeHistorySubmitData) : Observable<CampaignChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<CampaignChangeHistoryData>(this.baseUrl + 'api/CampaignChangeHistory', campaignChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCampaignChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostCampaignChangeHistory(campaignChangeHistory));
            }));
    }

  
    public DeleteCampaignChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/CampaignChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteCampaignChangeHistory(id));
            }));
    }


    private getConfigHash(config: CampaignChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerCampaignChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerCampaignChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.CampaignChangeHistories
        //
        if (userIsSchedulerCampaignChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerCampaignChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerCampaignChangeHistoryReader = false;
            }
        }

        return userIsSchedulerCampaignChangeHistoryReader;
    }


    public userIsSchedulerCampaignChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerCampaignChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.CampaignChangeHistories
        //
        if (userIsSchedulerCampaignChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerCampaignChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerCampaignChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerCampaignChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full CampaignChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the CampaignChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when CampaignChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveCampaignChangeHistory(raw: any): CampaignChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a CampaignChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(CampaignChangeHistoryData.prototype) as CampaignChangeHistoryData;

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
    // 2. But private methods (loadCampaignChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveCampaignChangeHistoryList(rawList: any[]): CampaignChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveCampaignChangeHistory(raw));
  }

}
