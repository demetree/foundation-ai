/*

   GENERATED SERVICE FOR THE TELEMETRYNETWORKHEALTH TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the TelemetryNetworkHealth table.

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
export class TelemetryNetworkHealthQueryParameters {
    telemetrySnapshotId: bigint | number | null | undefined = null;
    interfaceName: string | null | undefined = null;
    interfaceDescription: string | null | undefined = null;
    linkSpeedMbps: number | null | undefined = null;
    bytesSentTotal: bigint | number | null | undefined = null;
    bytesReceivedTotal: bigint | number | null | undefined = null;
    bytesSentPerSecond: number | null | undefined = null;
    bytesReceivedPerSecond: number | null | undefined = null;
    utilizationPercent: number | null | undefined = null;
    status: string | null | undefined = null;
    isActive: boolean | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class TelemetryNetworkHealthSubmitData {
    id!: bigint | number;
    telemetrySnapshotId!: bigint | number;
    interfaceName!: string;
    interfaceDescription: string | null = null;
    linkSpeedMbps: number | null = null;
    bytesSentTotal: bigint | number | null = null;
    bytesReceivedTotal: bigint | number | null = null;
    bytesSentPerSecond: number | null = null;
    bytesReceivedPerSecond: number | null = null;
    utilizationPercent: number | null = null;
    status: string | null = null;
    isActive!: boolean;
}


export class TelemetryNetworkHealthBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TelemetryNetworkHealthChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `telemetryNetworkHealth.TelemetryNetworkHealthChildren$` — use with `| async` in templates
//        • Promise:    `telemetryNetworkHealth.TelemetryNetworkHealthChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="telemetryNetworkHealth.TelemetryNetworkHealthChildren$ | async"`), or
//        • Access the promise getter (`telemetryNetworkHealth.TelemetryNetworkHealthChildren` or `await telemetryNetworkHealth.TelemetryNetworkHealthChildren`)
//    - Simply reading `telemetryNetworkHealth.TelemetryNetworkHealthChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await telemetryNetworkHealth.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TelemetryNetworkHealthData {
    id!: bigint | number;
    telemetrySnapshotId!: bigint | number;
    interfaceName!: string;
    interfaceDescription!: string | null;
    linkSpeedMbps!: number | null;
    bytesSentTotal!: bigint | number;
    bytesReceivedTotal!: bigint | number;
    bytesSentPerSecond!: number | null;
    bytesReceivedPerSecond!: number | null;
    utilizationPercent!: number | null;
    status!: string | null;
    isActive!: boolean;
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
  // Promise based reload method to allow rebuilding of any TelemetryNetworkHealthData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.telemetryNetworkHealth.Reload();
  //
  //  Non Async:
  //
  //     telemetryNetworkHealth[0].Reload().then(x => {
  //        this.telemetryNetworkHealth = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TelemetryNetworkHealthService.Instance.GetTelemetryNetworkHealth(this.id, includeRelations)
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
     * Updates the state of this TelemetryNetworkHealthData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TelemetryNetworkHealthData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TelemetryNetworkHealthSubmitData {
        return TelemetryNetworkHealthService.Instance.ConvertToTelemetryNetworkHealthSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TelemetryNetworkHealthService extends SecureEndpointBase {

    private static _instance: TelemetryNetworkHealthService;
    private listCache: Map<string, Observable<Array<TelemetryNetworkHealthData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TelemetryNetworkHealthBasicListData>>>;
    private recordCache: Map<string, Observable<TelemetryNetworkHealthData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TelemetryNetworkHealthData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TelemetryNetworkHealthBasicListData>>>();
        this.recordCache = new Map<string, Observable<TelemetryNetworkHealthData>>();

        TelemetryNetworkHealthService._instance = this;
    }

    public static get Instance(): TelemetryNetworkHealthService {
      return TelemetryNetworkHealthService._instance;
    }


    public ClearListCaches(config: TelemetryNetworkHealthQueryParameters | null = null) {

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


    public ConvertToTelemetryNetworkHealthSubmitData(data: TelemetryNetworkHealthData): TelemetryNetworkHealthSubmitData {

        let output = new TelemetryNetworkHealthSubmitData();

        output.id = data.id;
        output.telemetrySnapshotId = data.telemetrySnapshotId;
        output.interfaceName = data.interfaceName;
        output.interfaceDescription = data.interfaceDescription;
        output.linkSpeedMbps = data.linkSpeedMbps;
        output.bytesSentTotal = data.bytesSentTotal;
        output.bytesReceivedTotal = data.bytesReceivedTotal;
        output.bytesSentPerSecond = data.bytesSentPerSecond;
        output.bytesReceivedPerSecond = data.bytesReceivedPerSecond;
        output.utilizationPercent = data.utilizationPercent;
        output.status = data.status;
        output.isActive = data.isActive;

        return output;
    }

    public GetTelemetryNetworkHealth(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetryNetworkHealthData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const telemetryNetworkHealth$ = this.requestTelemetryNetworkHealth(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryNetworkHealth", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, telemetryNetworkHealth$);

            return telemetryNetworkHealth$;
        }

        return this.recordCache.get(configHash) as Observable<TelemetryNetworkHealthData>;
    }

    private requestTelemetryNetworkHealth(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetryNetworkHealthData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TelemetryNetworkHealthData>(this.baseUrl + 'api/TelemetryNetworkHealth/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTelemetryNetworkHealth(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryNetworkHealth(id, includeRelations));
            }));
    }

    public GetTelemetryNetworkHealthList(config: TelemetryNetworkHealthQueryParameters | any = null) : Observable<Array<TelemetryNetworkHealthData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const telemetryNetworkHealthList$ = this.requestTelemetryNetworkHealthList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryNetworkHealth list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, telemetryNetworkHealthList$);

            return telemetryNetworkHealthList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TelemetryNetworkHealthData>>;
    }


    private requestTelemetryNetworkHealthList(config: TelemetryNetworkHealthQueryParameters | any) : Observable <Array<TelemetryNetworkHealthData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetryNetworkHealthData>>(this.baseUrl + 'api/TelemetryNetworkHealths', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTelemetryNetworkHealthList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryNetworkHealthList(config));
            }));
    }

    public GetTelemetryNetworkHealthsRowCount(config: TelemetryNetworkHealthQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const telemetryNetworkHealthsRowCount$ = this.requestTelemetryNetworkHealthsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryNetworkHealths row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, telemetryNetworkHealthsRowCount$);

            return telemetryNetworkHealthsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTelemetryNetworkHealthsRowCount(config: TelemetryNetworkHealthQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/TelemetryNetworkHealths/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryNetworkHealthsRowCount(config));
            }));
    }

    public GetTelemetryNetworkHealthsBasicListData(config: TelemetryNetworkHealthQueryParameters | any = null) : Observable<Array<TelemetryNetworkHealthBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const telemetryNetworkHealthsBasicListData$ = this.requestTelemetryNetworkHealthsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryNetworkHealths basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, telemetryNetworkHealthsBasicListData$);

            return telemetryNetworkHealthsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TelemetryNetworkHealthBasicListData>>;
    }


    private requestTelemetryNetworkHealthsBasicListData(config: TelemetryNetworkHealthQueryParameters | any) : Observable<Array<TelemetryNetworkHealthBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetryNetworkHealthBasicListData>>(this.baseUrl + 'api/TelemetryNetworkHealths/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryNetworkHealthsBasicListData(config));
            }));

    }


    public PutTelemetryNetworkHealth(id: bigint | number, telemetryNetworkHealth: TelemetryNetworkHealthSubmitData) : Observable<TelemetryNetworkHealthData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TelemetryNetworkHealthData>(this.baseUrl + 'api/TelemetryNetworkHealth/' + id.toString(), telemetryNetworkHealth, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetryNetworkHealth(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTelemetryNetworkHealth(id, telemetryNetworkHealth));
            }));
    }


    public PostTelemetryNetworkHealth(telemetryNetworkHealth: TelemetryNetworkHealthSubmitData) : Observable<TelemetryNetworkHealthData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TelemetryNetworkHealthData>(this.baseUrl + 'api/TelemetryNetworkHealth', telemetryNetworkHealth, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetryNetworkHealth(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTelemetryNetworkHealth(telemetryNetworkHealth));
            }));
    }

  
    public DeleteTelemetryNetworkHealth(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/TelemetryNetworkHealth/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTelemetryNetworkHealth(id));
            }));
    }


    private getConfigHash(config: TelemetryNetworkHealthQueryParameters | any): string {

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

    public userIsTelemetryTelemetryNetworkHealthReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsTelemetryTelemetryNetworkHealthReader = this.authService.isTelemetryReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Telemetry.TelemetryNetworkHealths
        //
        if (userIsTelemetryTelemetryNetworkHealthReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsTelemetryTelemetryNetworkHealthReader = user.readPermission >= 0;
            } else {
                userIsTelemetryTelemetryNetworkHealthReader = false;
            }
        }

        return userIsTelemetryTelemetryNetworkHealthReader;
    }


    public userIsTelemetryTelemetryNetworkHealthWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsTelemetryTelemetryNetworkHealthWriter = this.authService.isTelemetryReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Telemetry.TelemetryNetworkHealths
        //
        if (userIsTelemetryTelemetryNetworkHealthWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsTelemetryTelemetryNetworkHealthWriter = user.writePermission >= 0;
          } else {
            userIsTelemetryTelemetryNetworkHealthWriter = false;
          }      
        }

        return userIsTelemetryTelemetryNetworkHealthWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full TelemetryNetworkHealthData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TelemetryNetworkHealthData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TelemetryNetworkHealthTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTelemetryNetworkHealth(raw: any): TelemetryNetworkHealthData {
    if (!raw) return raw;

    //
    // Create a TelemetryNetworkHealthData object instance with correct prototype
    //
    const revived = Object.create(TelemetryNetworkHealthData.prototype) as TelemetryNetworkHealthData;

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
    // 2. But private methods (loadTelemetryNetworkHealthXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveTelemetryNetworkHealthList(rawList: any[]): TelemetryNetworkHealthData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTelemetryNetworkHealth(raw));
  }

}
