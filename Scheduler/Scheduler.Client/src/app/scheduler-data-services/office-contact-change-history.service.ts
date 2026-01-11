/*

   GENERATED SERVICE FOR THE OFFICECONTACTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the OfficeContactChangeHistory table.

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
import { OfficeContactData } from './office-contact.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class OfficeContactChangeHistoryQueryParameters {
    officeContactId: bigint | number | null | undefined = null;
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
export class OfficeContactChangeHistorySubmitData {
    id!: bigint | number;
    officeContactId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
}


export class OfficeContactChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. OfficeContactChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `officeContactChangeHistory.OfficeContactChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `officeContactChangeHistory.OfficeContactChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="officeContactChangeHistory.OfficeContactChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`officeContactChangeHistory.OfficeContactChangeHistoryChildren` or `await officeContactChangeHistory.OfficeContactChangeHistoryChildren`)
//    - Simply reading `officeContactChangeHistory.OfficeContactChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await officeContactChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class OfficeContactChangeHistoryData {
    id!: bigint | number;
    officeContactId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601
    userId!: bigint | number;
    data!: string;
    officeContact: OfficeContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any OfficeContactChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.officeContactChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     officeContactChangeHistory[0].Reload().then(x => {
  //        this.officeContactChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      OfficeContactChangeHistoryService.Instance.GetOfficeContactChangeHistory(this.id, includeRelations)
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
     * Updates the state of this OfficeContactChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this OfficeContactChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): OfficeContactChangeHistorySubmitData {
        return OfficeContactChangeHistoryService.Instance.ConvertToOfficeContactChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class OfficeContactChangeHistoryService extends SecureEndpointBase {

    private static _instance: OfficeContactChangeHistoryService;
    private listCache: Map<string, Observable<Array<OfficeContactChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<OfficeContactChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<OfficeContactChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<OfficeContactChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<OfficeContactChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<OfficeContactChangeHistoryData>>();

        OfficeContactChangeHistoryService._instance = this;
    }

    public static get Instance(): OfficeContactChangeHistoryService {
      return OfficeContactChangeHistoryService._instance;
    }


    public ClearListCaches(config: OfficeContactChangeHistoryQueryParameters | null = null) {

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


    public ConvertToOfficeContactChangeHistorySubmitData(data: OfficeContactChangeHistoryData): OfficeContactChangeHistorySubmitData {

        let output = new OfficeContactChangeHistorySubmitData();

        output.id = data.id;
        output.officeContactId = data.officeContactId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetOfficeContactChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<OfficeContactChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const officeContactChangeHistory$ = this.requestOfficeContactChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get OfficeContactChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, officeContactChangeHistory$);

            return officeContactChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<OfficeContactChangeHistoryData>;
    }

    private requestOfficeContactChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<OfficeContactChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<OfficeContactChangeHistoryData>(this.baseUrl + 'api/OfficeContactChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveOfficeContactChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeContactChangeHistory(id, includeRelations));
            }));
    }

    public GetOfficeContactChangeHistoryList(config: OfficeContactChangeHistoryQueryParameters | any = null) : Observable<Array<OfficeContactChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const officeContactChangeHistoryList$ = this.requestOfficeContactChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get OfficeContactChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, officeContactChangeHistoryList$);

            return officeContactChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<OfficeContactChangeHistoryData>>;
    }


    private requestOfficeContactChangeHistoryList(config: OfficeContactChangeHistoryQueryParameters | any) : Observable <Array<OfficeContactChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<OfficeContactChangeHistoryData>>(this.baseUrl + 'api/OfficeContactChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveOfficeContactChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeContactChangeHistoryList(config));
            }));
    }

    public GetOfficeContactChangeHistoriesRowCount(config: OfficeContactChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const officeContactChangeHistoriesRowCount$ = this.requestOfficeContactChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get OfficeContactChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, officeContactChangeHistoriesRowCount$);

            return officeContactChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestOfficeContactChangeHistoriesRowCount(config: OfficeContactChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/OfficeContactChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeContactChangeHistoriesRowCount(config));
            }));
    }

    public GetOfficeContactChangeHistoriesBasicListData(config: OfficeContactChangeHistoryQueryParameters | any = null) : Observable<Array<OfficeContactChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const officeContactChangeHistoriesBasicListData$ = this.requestOfficeContactChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get OfficeContactChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, officeContactChangeHistoriesBasicListData$);

            return officeContactChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<OfficeContactChangeHistoryBasicListData>>;
    }


    private requestOfficeContactChangeHistoriesBasicListData(config: OfficeContactChangeHistoryQueryParameters | any) : Observable<Array<OfficeContactChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<OfficeContactChangeHistoryBasicListData>>(this.baseUrl + 'api/OfficeContactChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeContactChangeHistoriesBasicListData(config));
            }));

    }


    public PutOfficeContactChangeHistory(id: bigint | number, officeContactChangeHistory: OfficeContactChangeHistorySubmitData) : Observable<OfficeContactChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<OfficeContactChangeHistoryData>(this.baseUrl + 'api/OfficeContactChangeHistory/' + id.toString(), officeContactChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOfficeContactChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutOfficeContactChangeHistory(id, officeContactChangeHistory));
            }));
    }


    public PostOfficeContactChangeHistory(officeContactChangeHistory: OfficeContactChangeHistorySubmitData) : Observable<OfficeContactChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<OfficeContactChangeHistoryData>(this.baseUrl + 'api/OfficeContactChangeHistory', officeContactChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOfficeContactChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostOfficeContactChangeHistory(officeContactChangeHistory));
            }));
    }

  
    public DeleteOfficeContactChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/OfficeContactChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteOfficeContactChangeHistory(id));
            }));
    }


    private getConfigHash(config: OfficeContactChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerOfficeContactChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerOfficeContactChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.OfficeContactChangeHistories
        //
        if (userIsSchedulerOfficeContactChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerOfficeContactChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerOfficeContactChangeHistoryReader = false;
            }
        }

        return userIsSchedulerOfficeContactChangeHistoryReader;
    }


    public userIsSchedulerOfficeContactChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerOfficeContactChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.OfficeContactChangeHistories
        //
        if (userIsSchedulerOfficeContactChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerOfficeContactChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerOfficeContactChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerOfficeContactChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full OfficeContactChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the OfficeContactChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when OfficeContactChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveOfficeContactChangeHistory(raw: any): OfficeContactChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a OfficeContactChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(OfficeContactChangeHistoryData.prototype) as OfficeContactChangeHistoryData;

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
    // 2. But private methods (loadOfficeContactChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveOfficeContactChangeHistoryList(rawList: any[]): OfficeContactChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveOfficeContactChangeHistory(raw));
  }

}
