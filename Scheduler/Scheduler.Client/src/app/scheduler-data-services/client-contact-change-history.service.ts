/*

   GENERATED SERVICE FOR THE CLIENTCONTACTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ClientContactChangeHistory table.

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
import { ClientContactData } from './client-contact.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ClientContactChangeHistoryQueryParameters {
    clientContactId: bigint | number | null | undefined = null;
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
export class ClientContactChangeHistorySubmitData {
    id!: bigint | number;
    clientContactId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
}


export class ClientContactChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ClientContactChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `clientContactChangeHistory.ClientContactChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `clientContactChangeHistory.ClientContactChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="clientContactChangeHistory.ClientContactChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`clientContactChangeHistory.ClientContactChangeHistoryChildren` or `await clientContactChangeHistory.ClientContactChangeHistoryChildren`)
//    - Simply reading `clientContactChangeHistory.ClientContactChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await clientContactChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ClientContactChangeHistoryData {
    id!: bigint | number;
    clientContactId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
    clientContact: ClientContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ClientContactChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.clientContactChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     clientContactChangeHistory[0].Reload().then(x => {
  //        this.clientContactChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ClientContactChangeHistoryService.Instance.GetClientContactChangeHistory(this.id, includeRelations)
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
     * Updates the state of this ClientContactChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ClientContactChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ClientContactChangeHistorySubmitData {
        return ClientContactChangeHistoryService.Instance.ConvertToClientContactChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ClientContactChangeHistoryService extends SecureEndpointBase {

    private static _instance: ClientContactChangeHistoryService;
    private listCache: Map<string, Observable<Array<ClientContactChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ClientContactChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<ClientContactChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ClientContactChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ClientContactChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<ClientContactChangeHistoryData>>();

        ClientContactChangeHistoryService._instance = this;
    }

    public static get Instance(): ClientContactChangeHistoryService {
      return ClientContactChangeHistoryService._instance;
    }


    public ClearListCaches(config: ClientContactChangeHistoryQueryParameters | null = null) {

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


    public ConvertToClientContactChangeHistorySubmitData(data: ClientContactChangeHistoryData): ClientContactChangeHistorySubmitData {

        let output = new ClientContactChangeHistorySubmitData();

        output.id = data.id;
        output.clientContactId = data.clientContactId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetClientContactChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ClientContactChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const clientContactChangeHistory$ = this.requestClientContactChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ClientContactChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, clientContactChangeHistory$);

            return clientContactChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<ClientContactChangeHistoryData>;
    }

    private requestClientContactChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<ClientContactChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ClientContactChangeHistoryData>(this.baseUrl + 'api/ClientContactChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveClientContactChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestClientContactChangeHistory(id, includeRelations));
            }));
    }

    public GetClientContactChangeHistoryList(config: ClientContactChangeHistoryQueryParameters | any = null) : Observable<Array<ClientContactChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const clientContactChangeHistoryList$ = this.requestClientContactChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ClientContactChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, clientContactChangeHistoryList$);

            return clientContactChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ClientContactChangeHistoryData>>;
    }


    private requestClientContactChangeHistoryList(config: ClientContactChangeHistoryQueryParameters | any) : Observable <Array<ClientContactChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ClientContactChangeHistoryData>>(this.baseUrl + 'api/ClientContactChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveClientContactChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestClientContactChangeHistoryList(config));
            }));
    }

    public GetClientContactChangeHistoriesRowCount(config: ClientContactChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const clientContactChangeHistoriesRowCount$ = this.requestClientContactChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ClientContactChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, clientContactChangeHistoriesRowCount$);

            return clientContactChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestClientContactChangeHistoriesRowCount(config: ClientContactChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ClientContactChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestClientContactChangeHistoriesRowCount(config));
            }));
    }

    public GetClientContactChangeHistoriesBasicListData(config: ClientContactChangeHistoryQueryParameters | any = null) : Observable<Array<ClientContactChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const clientContactChangeHistoriesBasicListData$ = this.requestClientContactChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ClientContactChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, clientContactChangeHistoriesBasicListData$);

            return clientContactChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ClientContactChangeHistoryBasicListData>>;
    }


    private requestClientContactChangeHistoriesBasicListData(config: ClientContactChangeHistoryQueryParameters | any) : Observable<Array<ClientContactChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ClientContactChangeHistoryBasicListData>>(this.baseUrl + 'api/ClientContactChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestClientContactChangeHistoriesBasicListData(config));
            }));

    }


    public PutClientContactChangeHistory(id: bigint | number, clientContactChangeHistory: ClientContactChangeHistorySubmitData) : Observable<ClientContactChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ClientContactChangeHistoryData>(this.baseUrl + 'api/ClientContactChangeHistory/' + id.toString(), clientContactChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveClientContactChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutClientContactChangeHistory(id, clientContactChangeHistory));
            }));
    }


    public PostClientContactChangeHistory(clientContactChangeHistory: ClientContactChangeHistorySubmitData) : Observable<ClientContactChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ClientContactChangeHistoryData>(this.baseUrl + 'api/ClientContactChangeHistory', clientContactChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveClientContactChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostClientContactChangeHistory(clientContactChangeHistory));
            }));
    }

  
    public DeleteClientContactChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ClientContactChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteClientContactChangeHistory(id));
            }));
    }


    private getConfigHash(config: ClientContactChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerClientContactChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerClientContactChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ClientContactChangeHistories
        //
        if (userIsSchedulerClientContactChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerClientContactChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerClientContactChangeHistoryReader = false;
            }
        }

        return userIsSchedulerClientContactChangeHistoryReader;
    }


    public userIsSchedulerClientContactChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerClientContactChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ClientContactChangeHistories
        //
        if (userIsSchedulerClientContactChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerClientContactChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerClientContactChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerClientContactChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ClientContactChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ClientContactChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ClientContactChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveClientContactChangeHistory(raw: any): ClientContactChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a ClientContactChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(ClientContactChangeHistoryData.prototype) as ClientContactChangeHistoryData;

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
    // 2. But private methods (loadClientContactChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveClientContactChangeHistoryList(rawList: any[]): ClientContactChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveClientContactChangeHistory(raw));
  }

}
