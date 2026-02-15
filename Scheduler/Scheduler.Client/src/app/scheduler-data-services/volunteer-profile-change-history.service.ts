/*

   GENERATED SERVICE FOR THE VOLUNTEERPROFILECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the VolunteerProfileChangeHistory table.

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
import { VolunteerProfileData } from './volunteer-profile.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class VolunteerProfileChangeHistoryQueryParameters {
    volunteerProfileId: bigint | number | null | undefined = null;
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
export class VolunteerProfileChangeHistorySubmitData {
    id!: bigint | number;
    volunteerProfileId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
}


export class VolunteerProfileChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. VolunteerProfileChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `volunteerProfileChangeHistory.VolunteerProfileChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `volunteerProfileChangeHistory.VolunteerProfileChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="volunteerProfileChangeHistory.VolunteerProfileChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`volunteerProfileChangeHistory.VolunteerProfileChangeHistoryChildren` or `await volunteerProfileChangeHistory.VolunteerProfileChangeHistoryChildren`)
//    - Simply reading `volunteerProfileChangeHistory.VolunteerProfileChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await volunteerProfileChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class VolunteerProfileChangeHistoryData {
    id!: bigint | number;
    volunteerProfileId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
    volunteerProfile: VolunteerProfileData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any VolunteerProfileChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.volunteerProfileChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     volunteerProfileChangeHistory[0].Reload().then(x => {
  //        this.volunteerProfileChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      VolunteerProfileChangeHistoryService.Instance.GetVolunteerProfileChangeHistory(this.id, includeRelations)
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
     * Updates the state of this VolunteerProfileChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this VolunteerProfileChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): VolunteerProfileChangeHistorySubmitData {
        return VolunteerProfileChangeHistoryService.Instance.ConvertToVolunteerProfileChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class VolunteerProfileChangeHistoryService extends SecureEndpointBase {

    private static _instance: VolunteerProfileChangeHistoryService;
    private listCache: Map<string, Observable<Array<VolunteerProfileChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<VolunteerProfileChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<VolunteerProfileChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<VolunteerProfileChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<VolunteerProfileChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<VolunteerProfileChangeHistoryData>>();

        VolunteerProfileChangeHistoryService._instance = this;
    }

    public static get Instance(): VolunteerProfileChangeHistoryService {
      return VolunteerProfileChangeHistoryService._instance;
    }


    public ClearListCaches(config: VolunteerProfileChangeHistoryQueryParameters | null = null) {

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


    public ConvertToVolunteerProfileChangeHistorySubmitData(data: VolunteerProfileChangeHistoryData): VolunteerProfileChangeHistorySubmitData {

        let output = new VolunteerProfileChangeHistorySubmitData();

        output.id = data.id;
        output.volunteerProfileId = data.volunteerProfileId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetVolunteerProfileChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<VolunteerProfileChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const volunteerProfileChangeHistory$ = this.requestVolunteerProfileChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerProfileChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, volunteerProfileChangeHistory$);

            return volunteerProfileChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<VolunteerProfileChangeHistoryData>;
    }

    private requestVolunteerProfileChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<VolunteerProfileChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VolunteerProfileChangeHistoryData>(this.baseUrl + 'api/VolunteerProfileChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveVolunteerProfileChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerProfileChangeHistory(id, includeRelations));
            }));
    }

    public GetVolunteerProfileChangeHistoryList(config: VolunteerProfileChangeHistoryQueryParameters | any = null) : Observable<Array<VolunteerProfileChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const volunteerProfileChangeHistoryList$ = this.requestVolunteerProfileChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerProfileChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, volunteerProfileChangeHistoryList$);

            return volunteerProfileChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<VolunteerProfileChangeHistoryData>>;
    }


    private requestVolunteerProfileChangeHistoryList(config: VolunteerProfileChangeHistoryQueryParameters | any) : Observable <Array<VolunteerProfileChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<VolunteerProfileChangeHistoryData>>(this.baseUrl + 'api/VolunteerProfileChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveVolunteerProfileChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerProfileChangeHistoryList(config));
            }));
    }

    public GetVolunteerProfileChangeHistoriesRowCount(config: VolunteerProfileChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const volunteerProfileChangeHistoriesRowCount$ = this.requestVolunteerProfileChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerProfileChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, volunteerProfileChangeHistoriesRowCount$);

            return volunteerProfileChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestVolunteerProfileChangeHistoriesRowCount(config: VolunteerProfileChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/VolunteerProfileChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerProfileChangeHistoriesRowCount(config));
            }));
    }

    public GetVolunteerProfileChangeHistoriesBasicListData(config: VolunteerProfileChangeHistoryQueryParameters | any = null) : Observable<Array<VolunteerProfileChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const volunteerProfileChangeHistoriesBasicListData$ = this.requestVolunteerProfileChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerProfileChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, volunteerProfileChangeHistoriesBasicListData$);

            return volunteerProfileChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<VolunteerProfileChangeHistoryBasicListData>>;
    }


    private requestVolunteerProfileChangeHistoriesBasicListData(config: VolunteerProfileChangeHistoryQueryParameters | any) : Observable<Array<VolunteerProfileChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<VolunteerProfileChangeHistoryBasicListData>>(this.baseUrl + 'api/VolunteerProfileChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerProfileChangeHistoriesBasicListData(config));
            }));

    }


    public PutVolunteerProfileChangeHistory(id: bigint | number, volunteerProfileChangeHistory: VolunteerProfileChangeHistorySubmitData) : Observable<VolunteerProfileChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<VolunteerProfileChangeHistoryData>(this.baseUrl + 'api/VolunteerProfileChangeHistory/' + id.toString(), volunteerProfileChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveVolunteerProfileChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutVolunteerProfileChangeHistory(id, volunteerProfileChangeHistory));
            }));
    }


    public PostVolunteerProfileChangeHistory(volunteerProfileChangeHistory: VolunteerProfileChangeHistorySubmitData) : Observable<VolunteerProfileChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<VolunteerProfileChangeHistoryData>(this.baseUrl + 'api/VolunteerProfileChangeHistory', volunteerProfileChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveVolunteerProfileChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostVolunteerProfileChangeHistory(volunteerProfileChangeHistory));
            }));
    }

  
    public DeleteVolunteerProfileChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/VolunteerProfileChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteVolunteerProfileChangeHistory(id));
            }));
    }


    private getConfigHash(config: VolunteerProfileChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerVolunteerProfileChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerVolunteerProfileChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.VolunteerProfileChangeHistories
        //
        if (userIsSchedulerVolunteerProfileChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerVolunteerProfileChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerVolunteerProfileChangeHistoryReader = false;
            }
        }

        return userIsSchedulerVolunteerProfileChangeHistoryReader;
    }


    public userIsSchedulerVolunteerProfileChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerVolunteerProfileChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.VolunteerProfileChangeHistories
        //
        if (userIsSchedulerVolunteerProfileChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerVolunteerProfileChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerVolunteerProfileChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerVolunteerProfileChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full VolunteerProfileChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the VolunteerProfileChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when VolunteerProfileChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveVolunteerProfileChangeHistory(raw: any): VolunteerProfileChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a VolunteerProfileChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(VolunteerProfileChangeHistoryData.prototype) as VolunteerProfileChangeHistoryData;

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
    // 2. But private methods (loadVolunteerProfileChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveVolunteerProfileChangeHistoryList(rawList: any[]): VolunteerProfileChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveVolunteerProfileChangeHistory(raw));
  }

}
