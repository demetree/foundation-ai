/*

   GENERATED SERVICE FOR THE TELEMETRYDATABASEHEALTH TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the TelemetryDatabaseHealth table.

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
import { TelemetrySnapshotData } from './telemetry-snapshot.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class TelemetryDatabaseHealthQueryParameters {
    telemetrySnapshotId: bigint | number | null | undefined = null;
    databaseName: string | null | undefined = null;
    isConnected: boolean | null | undefined = null;
    status: string | null | undefined = null;
    server: string | null | undefined = null;
    provider: string | null | undefined = null;
    errorMessage: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class TelemetryDatabaseHealthSubmitData {
    id!: bigint | number;
    telemetrySnapshotId!: bigint | number;
    databaseName!: string;
    isConnected!: boolean;
    status: string | null = null;
    server: string | null = null;
    provider: string | null = null;
    errorMessage: string | null = null;
}


export class TelemetryDatabaseHealthBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TelemetryDatabaseHealthChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `telemetryDatabaseHealth.TelemetryDatabaseHealthChildren$` — use with `| async` in templates
//        • Promise:    `telemetryDatabaseHealth.TelemetryDatabaseHealthChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="telemetryDatabaseHealth.TelemetryDatabaseHealthChildren$ | async"`), or
//        • Access the promise getter (`telemetryDatabaseHealth.TelemetryDatabaseHealthChildren` or `await telemetryDatabaseHealth.TelemetryDatabaseHealthChildren`)
//    - Simply reading `telemetryDatabaseHealth.TelemetryDatabaseHealthChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await telemetryDatabaseHealth.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TelemetryDatabaseHealthData {
    id!: bigint | number;
    telemetrySnapshotId!: bigint | number;
    databaseName!: string;
    isConnected!: boolean;
    status!: string | null;
    server!: string | null;
    provider!: string | null;
    errorMessage!: string | null;
    telemetrySnapshot: TelemetrySnapshotData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any TelemetryDatabaseHealthData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.telemetryDatabaseHealth.Reload();
  //
  //  Non Async:
  //
  //     telemetryDatabaseHealth[0].Reload().then(x => {
  //        this.telemetryDatabaseHealth = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TelemetryDatabaseHealthService.Instance.GetTelemetryDatabaseHealth(this.id, includeRelations)
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
     * Updates the state of this TelemetryDatabaseHealthData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TelemetryDatabaseHealthData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TelemetryDatabaseHealthSubmitData {
        return TelemetryDatabaseHealthService.Instance.ConvertToTelemetryDatabaseHealthSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TelemetryDatabaseHealthService extends SecureEndpointBase {

    private static _instance: TelemetryDatabaseHealthService;
    private listCache: Map<string, Observable<Array<TelemetryDatabaseHealthData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TelemetryDatabaseHealthBasicListData>>>;
    private recordCache: Map<string, Observable<TelemetryDatabaseHealthData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TelemetryDatabaseHealthData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TelemetryDatabaseHealthBasicListData>>>();
        this.recordCache = new Map<string, Observable<TelemetryDatabaseHealthData>>();

        TelemetryDatabaseHealthService._instance = this;
    }

    public static get Instance(): TelemetryDatabaseHealthService {
      return TelemetryDatabaseHealthService._instance;
    }


    public ClearListCaches(config: TelemetryDatabaseHealthQueryParameters | null = null) {

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


    public ConvertToTelemetryDatabaseHealthSubmitData(data: TelemetryDatabaseHealthData): TelemetryDatabaseHealthSubmitData {

        let output = new TelemetryDatabaseHealthSubmitData();

        output.id = data.id;
        output.telemetrySnapshotId = data.telemetrySnapshotId;
        output.databaseName = data.databaseName;
        output.isConnected = data.isConnected;
        output.status = data.status;
        output.server = data.server;
        output.provider = data.provider;
        output.errorMessage = data.errorMessage;

        return output;
    }

    public GetTelemetryDatabaseHealth(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetryDatabaseHealthData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const telemetryDatabaseHealth$ = this.requestTelemetryDatabaseHealth(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryDatabaseHealth", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, telemetryDatabaseHealth$);

            return telemetryDatabaseHealth$;
        }

        return this.recordCache.get(configHash) as Observable<TelemetryDatabaseHealthData>;
    }

    private requestTelemetryDatabaseHealth(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetryDatabaseHealthData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TelemetryDatabaseHealthData>(this.baseUrl + 'api/TelemetryDatabaseHealth/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTelemetryDatabaseHealth(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryDatabaseHealth(id, includeRelations));
            }));
    }

    public GetTelemetryDatabaseHealthList(config: TelemetryDatabaseHealthQueryParameters | any = null) : Observable<Array<TelemetryDatabaseHealthData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const telemetryDatabaseHealthList$ = this.requestTelemetryDatabaseHealthList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryDatabaseHealth list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, telemetryDatabaseHealthList$);

            return telemetryDatabaseHealthList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TelemetryDatabaseHealthData>>;
    }


    private requestTelemetryDatabaseHealthList(config: TelemetryDatabaseHealthQueryParameters | any) : Observable <Array<TelemetryDatabaseHealthData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetryDatabaseHealthData>>(this.baseUrl + 'api/TelemetryDatabaseHealths', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTelemetryDatabaseHealthList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryDatabaseHealthList(config));
            }));
    }

    public GetTelemetryDatabaseHealthsRowCount(config: TelemetryDatabaseHealthQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const telemetryDatabaseHealthsRowCount$ = this.requestTelemetryDatabaseHealthsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryDatabaseHealths row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, telemetryDatabaseHealthsRowCount$);

            return telemetryDatabaseHealthsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTelemetryDatabaseHealthsRowCount(config: TelemetryDatabaseHealthQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/TelemetryDatabaseHealths/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryDatabaseHealthsRowCount(config));
            }));
    }

    public GetTelemetryDatabaseHealthsBasicListData(config: TelemetryDatabaseHealthQueryParameters | any = null) : Observable<Array<TelemetryDatabaseHealthBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const telemetryDatabaseHealthsBasicListData$ = this.requestTelemetryDatabaseHealthsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryDatabaseHealths basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, telemetryDatabaseHealthsBasicListData$);

            return telemetryDatabaseHealthsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TelemetryDatabaseHealthBasicListData>>;
    }


    private requestTelemetryDatabaseHealthsBasicListData(config: TelemetryDatabaseHealthQueryParameters | any) : Observable<Array<TelemetryDatabaseHealthBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetryDatabaseHealthBasicListData>>(this.baseUrl + 'api/TelemetryDatabaseHealths/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryDatabaseHealthsBasicListData(config));
            }));

    }


    public PutTelemetryDatabaseHealth(id: bigint | number, telemetryDatabaseHealth: TelemetryDatabaseHealthSubmitData) : Observable<TelemetryDatabaseHealthData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TelemetryDatabaseHealthData>(this.baseUrl + 'api/TelemetryDatabaseHealth/' + id.toString(), telemetryDatabaseHealth, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetryDatabaseHealth(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTelemetryDatabaseHealth(id, telemetryDatabaseHealth));
            }));
    }


    public PostTelemetryDatabaseHealth(telemetryDatabaseHealth: TelemetryDatabaseHealthSubmitData) : Observable<TelemetryDatabaseHealthData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TelemetryDatabaseHealthData>(this.baseUrl + 'api/TelemetryDatabaseHealth', telemetryDatabaseHealth, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetryDatabaseHealth(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTelemetryDatabaseHealth(telemetryDatabaseHealth));
            }));
    }

  
    public DeleteTelemetryDatabaseHealth(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/TelemetryDatabaseHealth/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTelemetryDatabaseHealth(id));
            }));
    }


    private getConfigHash(config: TelemetryDatabaseHealthQueryParameters | any): string {

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

    public userIsTelemetryTelemetryDatabaseHealthReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsTelemetryTelemetryDatabaseHealthReader = this.authService.isTelemetryReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Telemetry.TelemetryDatabaseHealths
        //
        if (userIsTelemetryTelemetryDatabaseHealthReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsTelemetryTelemetryDatabaseHealthReader = user.readPermission >= 0;
            } else {
                userIsTelemetryTelemetryDatabaseHealthReader = false;
            }
        }

        return userIsTelemetryTelemetryDatabaseHealthReader;
    }


    public userIsTelemetryTelemetryDatabaseHealthWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsTelemetryTelemetryDatabaseHealthWriter = this.authService.isTelemetryReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Telemetry.TelemetryDatabaseHealths
        //
        if (userIsTelemetryTelemetryDatabaseHealthWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsTelemetryTelemetryDatabaseHealthWriter = user.writePermission >= 0;
          } else {
            userIsTelemetryTelemetryDatabaseHealthWriter = false;
          }      
        }

        return userIsTelemetryTelemetryDatabaseHealthWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full TelemetryDatabaseHealthData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TelemetryDatabaseHealthData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TelemetryDatabaseHealthTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTelemetryDatabaseHealth(raw: any): TelemetryDatabaseHealthData {
    if (!raw) return raw;

    //
    // Create a TelemetryDatabaseHealthData object instance with correct prototype
    //
    const revived = Object.create(TelemetryDatabaseHealthData.prototype) as TelemetryDatabaseHealthData;

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
    // 2. But private methods (loadTelemetryDatabaseHealthXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveTelemetryDatabaseHealthList(rawList: any[]): TelemetryDatabaseHealthData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTelemetryDatabaseHealth(raw));
  }

}
