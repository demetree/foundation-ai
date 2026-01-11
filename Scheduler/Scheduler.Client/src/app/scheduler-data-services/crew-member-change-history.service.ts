/*

   GENERATED SERVICE FOR THE CREWMEMBERCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the CrewMemberChangeHistory table.

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
import { CrewMemberData } from './crew-member.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class CrewMemberChangeHistoryQueryParameters {
    crewMemberId: bigint | number | null | undefined = null;
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
export class CrewMemberChangeHistorySubmitData {
    id!: bigint | number;
    crewMemberId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
}


export class CrewMemberChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. CrewMemberChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `crewMemberChangeHistory.CrewMemberChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `crewMemberChangeHistory.CrewMemberChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="crewMemberChangeHistory.CrewMemberChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`crewMemberChangeHistory.CrewMemberChangeHistoryChildren` or `await crewMemberChangeHistory.CrewMemberChangeHistoryChildren`)
//    - Simply reading `crewMemberChangeHistory.CrewMemberChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await crewMemberChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class CrewMemberChangeHistoryData {
    id!: bigint | number;
    crewMemberId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
    crewMember: CrewMemberData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any CrewMemberChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.crewMemberChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     crewMemberChangeHistory[0].Reload().then(x => {
  //        this.crewMemberChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      CrewMemberChangeHistoryService.Instance.GetCrewMemberChangeHistory(this.id, includeRelations)
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
     * Updates the state of this CrewMemberChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this CrewMemberChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): CrewMemberChangeHistorySubmitData {
        return CrewMemberChangeHistoryService.Instance.ConvertToCrewMemberChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class CrewMemberChangeHistoryService extends SecureEndpointBase {

    private static _instance: CrewMemberChangeHistoryService;
    private listCache: Map<string, Observable<Array<CrewMemberChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<CrewMemberChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<CrewMemberChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<CrewMemberChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<CrewMemberChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<CrewMemberChangeHistoryData>>();

        CrewMemberChangeHistoryService._instance = this;
    }

    public static get Instance(): CrewMemberChangeHistoryService {
      return CrewMemberChangeHistoryService._instance;
    }


    public ClearListCaches(config: CrewMemberChangeHistoryQueryParameters | null = null) {

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


    public ConvertToCrewMemberChangeHistorySubmitData(data: CrewMemberChangeHistoryData): CrewMemberChangeHistorySubmitData {

        let output = new CrewMemberChangeHistorySubmitData();

        output.id = data.id;
        output.crewMemberId = data.crewMemberId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetCrewMemberChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<CrewMemberChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const crewMemberChangeHistory$ = this.requestCrewMemberChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get CrewMemberChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, crewMemberChangeHistory$);

            return crewMemberChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<CrewMemberChangeHistoryData>;
    }

    private requestCrewMemberChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<CrewMemberChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CrewMemberChangeHistoryData>(this.baseUrl + 'api/CrewMemberChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveCrewMemberChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestCrewMemberChangeHistory(id, includeRelations));
            }));
    }

    public GetCrewMemberChangeHistoryList(config: CrewMemberChangeHistoryQueryParameters | any = null) : Observable<Array<CrewMemberChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const crewMemberChangeHistoryList$ = this.requestCrewMemberChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get CrewMemberChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, crewMemberChangeHistoryList$);

            return crewMemberChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<CrewMemberChangeHistoryData>>;
    }


    private requestCrewMemberChangeHistoryList(config: CrewMemberChangeHistoryQueryParameters | any) : Observable <Array<CrewMemberChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CrewMemberChangeHistoryData>>(this.baseUrl + 'api/CrewMemberChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveCrewMemberChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestCrewMemberChangeHistoryList(config));
            }));
    }

    public GetCrewMemberChangeHistoriesRowCount(config: CrewMemberChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const crewMemberChangeHistoriesRowCount$ = this.requestCrewMemberChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get CrewMemberChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, crewMemberChangeHistoriesRowCount$);

            return crewMemberChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestCrewMemberChangeHistoriesRowCount(config: CrewMemberChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/CrewMemberChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCrewMemberChangeHistoriesRowCount(config));
            }));
    }

    public GetCrewMemberChangeHistoriesBasicListData(config: CrewMemberChangeHistoryQueryParameters | any = null) : Observable<Array<CrewMemberChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const crewMemberChangeHistoriesBasicListData$ = this.requestCrewMemberChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get CrewMemberChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, crewMemberChangeHistoriesBasicListData$);

            return crewMemberChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<CrewMemberChangeHistoryBasicListData>>;
    }


    private requestCrewMemberChangeHistoriesBasicListData(config: CrewMemberChangeHistoryQueryParameters | any) : Observable<Array<CrewMemberChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CrewMemberChangeHistoryBasicListData>>(this.baseUrl + 'api/CrewMemberChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCrewMemberChangeHistoriesBasicListData(config));
            }));

    }


    public PutCrewMemberChangeHistory(id: bigint | number, crewMemberChangeHistory: CrewMemberChangeHistorySubmitData) : Observable<CrewMemberChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CrewMemberChangeHistoryData>(this.baseUrl + 'api/CrewMemberChangeHistory/' + id.toString(), crewMemberChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCrewMemberChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutCrewMemberChangeHistory(id, crewMemberChangeHistory));
            }));
    }


    public PostCrewMemberChangeHistory(crewMemberChangeHistory: CrewMemberChangeHistorySubmitData) : Observable<CrewMemberChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<CrewMemberChangeHistoryData>(this.baseUrl + 'api/CrewMemberChangeHistory', crewMemberChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCrewMemberChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostCrewMemberChangeHistory(crewMemberChangeHistory));
            }));
    }

  
    public DeleteCrewMemberChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/CrewMemberChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteCrewMemberChangeHistory(id));
            }));
    }


    private getConfigHash(config: CrewMemberChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerCrewMemberChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerCrewMemberChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.CrewMemberChangeHistories
        //
        if (userIsSchedulerCrewMemberChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerCrewMemberChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerCrewMemberChangeHistoryReader = false;
            }
        }

        return userIsSchedulerCrewMemberChangeHistoryReader;
    }


    public userIsSchedulerCrewMemberChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerCrewMemberChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.CrewMemberChangeHistories
        //
        if (userIsSchedulerCrewMemberChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerCrewMemberChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerCrewMemberChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerCrewMemberChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full CrewMemberChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the CrewMemberChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when CrewMemberChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveCrewMemberChangeHistory(raw: any): CrewMemberChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a CrewMemberChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(CrewMemberChangeHistoryData.prototype) as CrewMemberChangeHistoryData;

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
    // 2. But private methods (loadCrewMemberChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveCrewMemberChangeHistoryList(rawList: any[]): CrewMemberChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveCrewMemberChangeHistory(raw));
  }

}
