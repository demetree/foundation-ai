/*

   GENERATED SERVICE FOR THE TELEMETRYDISKHEALTH TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the TelemetryDiskHealth table.

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
export class TelemetryDiskHealthQueryParameters {
    telemetrySnapshotId: bigint | number | null | undefined = null;
    driveName: string | null | undefined = null;
    driveLabel: string | null | undefined = null;
    totalGB: number | null | undefined = null;
    freeGB: number | null | undefined = null;
    freePercent: number | null | undefined = null;
    usedPercent: number | null | undefined = null;
    status: string | null | undefined = null;
    isApplicationDrive: boolean | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class TelemetryDiskHealthSubmitData {
    id!: bigint | number;
    telemetrySnapshotId!: bigint | number;
    driveName!: string;
    driveLabel: string | null = null;
    totalGB: number | null = null;
    freeGB: number | null = null;
    freePercent: number | null = null;
    usedPercent: number | null = null;
    status: string | null = null;
    isApplicationDrive!: boolean;
}


export class TelemetryDiskHealthBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TelemetryDiskHealthChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `telemetryDiskHealth.TelemetryDiskHealthChildren$` — use with `| async` in templates
//        • Promise:    `telemetryDiskHealth.TelemetryDiskHealthChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="telemetryDiskHealth.TelemetryDiskHealthChildren$ | async"`), or
//        • Access the promise getter (`telemetryDiskHealth.TelemetryDiskHealthChildren` or `await telemetryDiskHealth.TelemetryDiskHealthChildren`)
//    - Simply reading `telemetryDiskHealth.TelemetryDiskHealthChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await telemetryDiskHealth.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TelemetryDiskHealthData {
    id!: bigint | number;
    telemetrySnapshotId!: bigint | number;
    driveName!: string;
    driveLabel!: string | null;
    totalGB!: number | null;
    freeGB!: number | null;
    freePercent!: number | null;
    usedPercent!: number | null;
    status!: string | null;
    isApplicationDrive!: boolean;
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
  // Promise based reload method to allow rebuilding of any TelemetryDiskHealthData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.telemetryDiskHealth.Reload();
  //
  //  Non Async:
  //
  //     telemetryDiskHealth[0].Reload().then(x => {
  //        this.telemetryDiskHealth = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TelemetryDiskHealthService.Instance.GetTelemetryDiskHealth(this.id, includeRelations)
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
     * Updates the state of this TelemetryDiskHealthData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TelemetryDiskHealthData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TelemetryDiskHealthSubmitData {
        return TelemetryDiskHealthService.Instance.ConvertToTelemetryDiskHealthSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TelemetryDiskHealthService extends SecureEndpointBase {

    private static _instance: TelemetryDiskHealthService;
    private listCache: Map<string, Observable<Array<TelemetryDiskHealthData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TelemetryDiskHealthBasicListData>>>;
    private recordCache: Map<string, Observable<TelemetryDiskHealthData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TelemetryDiskHealthData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TelemetryDiskHealthBasicListData>>>();
        this.recordCache = new Map<string, Observable<TelemetryDiskHealthData>>();

        TelemetryDiskHealthService._instance = this;
    }

    public static get Instance(): TelemetryDiskHealthService {
      return TelemetryDiskHealthService._instance;
    }


    public ClearListCaches(config: TelemetryDiskHealthQueryParameters | null = null) {

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


    public ConvertToTelemetryDiskHealthSubmitData(data: TelemetryDiskHealthData): TelemetryDiskHealthSubmitData {

        let output = new TelemetryDiskHealthSubmitData();

        output.id = data.id;
        output.telemetrySnapshotId = data.telemetrySnapshotId;
        output.driveName = data.driveName;
        output.driveLabel = data.driveLabel;
        output.totalGB = data.totalGB;
        output.freeGB = data.freeGB;
        output.freePercent = data.freePercent;
        output.usedPercent = data.usedPercent;
        output.status = data.status;
        output.isApplicationDrive = data.isApplicationDrive;

        return output;
    }

    public GetTelemetryDiskHealth(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetryDiskHealthData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const telemetryDiskHealth$ = this.requestTelemetryDiskHealth(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryDiskHealth", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, telemetryDiskHealth$);

            return telemetryDiskHealth$;
        }

        return this.recordCache.get(configHash) as Observable<TelemetryDiskHealthData>;
    }

    private requestTelemetryDiskHealth(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetryDiskHealthData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TelemetryDiskHealthData>(this.baseUrl + 'api/TelemetryDiskHealth/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTelemetryDiskHealth(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryDiskHealth(id, includeRelations));
            }));
    }

    public GetTelemetryDiskHealthList(config: TelemetryDiskHealthQueryParameters | any = null) : Observable<Array<TelemetryDiskHealthData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const telemetryDiskHealthList$ = this.requestTelemetryDiskHealthList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryDiskHealth list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, telemetryDiskHealthList$);

            return telemetryDiskHealthList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TelemetryDiskHealthData>>;
    }


    private requestTelemetryDiskHealthList(config: TelemetryDiskHealthQueryParameters | any) : Observable <Array<TelemetryDiskHealthData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetryDiskHealthData>>(this.baseUrl + 'api/TelemetryDiskHealths', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTelemetryDiskHealthList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryDiskHealthList(config));
            }));
    }

    public GetTelemetryDiskHealthsRowCount(config: TelemetryDiskHealthQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const telemetryDiskHealthsRowCount$ = this.requestTelemetryDiskHealthsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryDiskHealths row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, telemetryDiskHealthsRowCount$);

            return telemetryDiskHealthsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTelemetryDiskHealthsRowCount(config: TelemetryDiskHealthQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/TelemetryDiskHealths/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryDiskHealthsRowCount(config));
            }));
    }

    public GetTelemetryDiskHealthsBasicListData(config: TelemetryDiskHealthQueryParameters | any = null) : Observable<Array<TelemetryDiskHealthBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const telemetryDiskHealthsBasicListData$ = this.requestTelemetryDiskHealthsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryDiskHealths basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, telemetryDiskHealthsBasicListData$);

            return telemetryDiskHealthsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TelemetryDiskHealthBasicListData>>;
    }


    private requestTelemetryDiskHealthsBasicListData(config: TelemetryDiskHealthQueryParameters | any) : Observable<Array<TelemetryDiskHealthBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetryDiskHealthBasicListData>>(this.baseUrl + 'api/TelemetryDiskHealths/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryDiskHealthsBasicListData(config));
            }));

    }


    public PutTelemetryDiskHealth(id: bigint | number, telemetryDiskHealth: TelemetryDiskHealthSubmitData) : Observable<TelemetryDiskHealthData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TelemetryDiskHealthData>(this.baseUrl + 'api/TelemetryDiskHealth/' + id.toString(), telemetryDiskHealth, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetryDiskHealth(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTelemetryDiskHealth(id, telemetryDiskHealth));
            }));
    }


    public PostTelemetryDiskHealth(telemetryDiskHealth: TelemetryDiskHealthSubmitData) : Observable<TelemetryDiskHealthData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TelemetryDiskHealthData>(this.baseUrl + 'api/TelemetryDiskHealth', telemetryDiskHealth, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetryDiskHealth(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTelemetryDiskHealth(telemetryDiskHealth));
            }));
    }

  
    public DeleteTelemetryDiskHealth(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/TelemetryDiskHealth/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTelemetryDiskHealth(id));
            }));
    }


    private getConfigHash(config: TelemetryDiskHealthQueryParameters | any): string {

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

    public userIsTelemetryTelemetryDiskHealthReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsTelemetryTelemetryDiskHealthReader = this.authService.isTelemetryReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Telemetry.TelemetryDiskHealths
        //
        if (userIsTelemetryTelemetryDiskHealthReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsTelemetryTelemetryDiskHealthReader = user.readPermission >= 0;
            } else {
                userIsTelemetryTelemetryDiskHealthReader = false;
            }
        }

        return userIsTelemetryTelemetryDiskHealthReader;
    }


    public userIsTelemetryTelemetryDiskHealthWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsTelemetryTelemetryDiskHealthWriter = this.authService.isTelemetryReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Telemetry.TelemetryDiskHealths
        //
        if (userIsTelemetryTelemetryDiskHealthWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsTelemetryTelemetryDiskHealthWriter = user.writePermission >= 0;
          } else {
            userIsTelemetryTelemetryDiskHealthWriter = false;
          }      
        }

        return userIsTelemetryTelemetryDiskHealthWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full TelemetryDiskHealthData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TelemetryDiskHealthData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TelemetryDiskHealthTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTelemetryDiskHealth(raw: any): TelemetryDiskHealthData {
    if (!raw) return raw;

    //
    // Create a TelemetryDiskHealthData object instance with correct prototype
    //
    const revived = Object.create(TelemetryDiskHealthData.prototype) as TelemetryDiskHealthData;

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
    // 2. But private methods (loadTelemetryDiskHealthXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveTelemetryDiskHealthList(rawList: any[]): TelemetryDiskHealthData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTelemetryDiskHealth(raw));
  }

}
