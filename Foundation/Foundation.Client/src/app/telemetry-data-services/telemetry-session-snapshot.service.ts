/*

   GENERATED SERVICE FOR THE TELEMETRYSESSIONSNAPSHOT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the TelemetrySessionSnapshot table.

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
export class TelemetrySessionSnapshotQueryParameters {
    telemetrySnapshotId: bigint | number | null | undefined = null;
    activeSessionCount: bigint | number | null | undefined = null;
    expiredSessionCount: bigint | number | null | undefined = null;
    oldestSessionStart: string | null | undefined = null;        // ISO 8601 (full datetime)
    newestSessionStart: string | null | undefined = null;        // ISO 8601 (full datetime)
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class TelemetrySessionSnapshotSubmitData {
    id!: bigint | number;
    telemetrySnapshotId!: bigint | number;
    activeSessionCount!: bigint | number;
    expiredSessionCount: bigint | number | null = null;
    oldestSessionStart: string | null = null;     // ISO 8601 (full datetime)
    newestSessionStart: string | null = null;     // ISO 8601 (full datetime)
}


export class TelemetrySessionSnapshotBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TelemetrySessionSnapshotChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `telemetrySessionSnapshot.TelemetrySessionSnapshotChildren$` — use with `| async` in templates
//        • Promise:    `telemetrySessionSnapshot.TelemetrySessionSnapshotChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="telemetrySessionSnapshot.TelemetrySessionSnapshotChildren$ | async"`), or
//        • Access the promise getter (`telemetrySessionSnapshot.TelemetrySessionSnapshotChildren` or `await telemetrySessionSnapshot.TelemetrySessionSnapshotChildren`)
//    - Simply reading `telemetrySessionSnapshot.TelemetrySessionSnapshotChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await telemetrySessionSnapshot.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TelemetrySessionSnapshotData {
    id!: bigint | number;
    telemetrySnapshotId!: bigint | number;
    activeSessionCount!: bigint | number;
    expiredSessionCount!: bigint | number;
    oldestSessionStart!: string | null;   // ISO 8601 (full datetime)
    newestSessionStart!: string | null;   // ISO 8601 (full datetime)
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
  // Promise based reload method to allow rebuilding of any TelemetrySessionSnapshotData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.telemetrySessionSnapshot.Reload();
  //
  //  Non Async:
  //
  //     telemetrySessionSnapshot[0].Reload().then(x => {
  //        this.telemetrySessionSnapshot = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TelemetrySessionSnapshotService.Instance.GetTelemetrySessionSnapshot(this.id, includeRelations)
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
     * Updates the state of this TelemetrySessionSnapshotData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TelemetrySessionSnapshotData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TelemetrySessionSnapshotSubmitData {
        return TelemetrySessionSnapshotService.Instance.ConvertToTelemetrySessionSnapshotSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TelemetrySessionSnapshotService extends SecureEndpointBase {

    private static _instance: TelemetrySessionSnapshotService;
    private listCache: Map<string, Observable<Array<TelemetrySessionSnapshotData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TelemetrySessionSnapshotBasicListData>>>;
    private recordCache: Map<string, Observable<TelemetrySessionSnapshotData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TelemetrySessionSnapshotData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TelemetrySessionSnapshotBasicListData>>>();
        this.recordCache = new Map<string, Observable<TelemetrySessionSnapshotData>>();

        TelemetrySessionSnapshotService._instance = this;
    }

    public static get Instance(): TelemetrySessionSnapshotService {
      return TelemetrySessionSnapshotService._instance;
    }


    public ClearListCaches(config: TelemetrySessionSnapshotQueryParameters | null = null) {

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


    public ConvertToTelemetrySessionSnapshotSubmitData(data: TelemetrySessionSnapshotData): TelemetrySessionSnapshotSubmitData {

        let output = new TelemetrySessionSnapshotSubmitData();

        output.id = data.id;
        output.telemetrySnapshotId = data.telemetrySnapshotId;
        output.activeSessionCount = data.activeSessionCount;
        output.expiredSessionCount = data.expiredSessionCount;
        output.oldestSessionStart = data.oldestSessionStart;
        output.newestSessionStart = data.newestSessionStart;

        return output;
    }

    public GetTelemetrySessionSnapshot(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetrySessionSnapshotData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const telemetrySessionSnapshot$ = this.requestTelemetrySessionSnapshot(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetrySessionSnapshot", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, telemetrySessionSnapshot$);

            return telemetrySessionSnapshot$;
        }

        return this.recordCache.get(configHash) as Observable<TelemetrySessionSnapshotData>;
    }

    private requestTelemetrySessionSnapshot(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetrySessionSnapshotData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TelemetrySessionSnapshotData>(this.baseUrl + 'api/TelemetrySessionSnapshot/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTelemetrySessionSnapshot(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetrySessionSnapshot(id, includeRelations));
            }));
    }

    public GetTelemetrySessionSnapshotList(config: TelemetrySessionSnapshotQueryParameters | any = null) : Observable<Array<TelemetrySessionSnapshotData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const telemetrySessionSnapshotList$ = this.requestTelemetrySessionSnapshotList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetrySessionSnapshot list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, telemetrySessionSnapshotList$);

            return telemetrySessionSnapshotList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TelemetrySessionSnapshotData>>;
    }


    private requestTelemetrySessionSnapshotList(config: TelemetrySessionSnapshotQueryParameters | any) : Observable <Array<TelemetrySessionSnapshotData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetrySessionSnapshotData>>(this.baseUrl + 'api/TelemetrySessionSnapshots', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTelemetrySessionSnapshotList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetrySessionSnapshotList(config));
            }));
    }

    public GetTelemetrySessionSnapshotsRowCount(config: TelemetrySessionSnapshotQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const telemetrySessionSnapshotsRowCount$ = this.requestTelemetrySessionSnapshotsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetrySessionSnapshots row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, telemetrySessionSnapshotsRowCount$);

            return telemetrySessionSnapshotsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTelemetrySessionSnapshotsRowCount(config: TelemetrySessionSnapshotQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/TelemetrySessionSnapshots/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetrySessionSnapshotsRowCount(config));
            }));
    }

    public GetTelemetrySessionSnapshotsBasicListData(config: TelemetrySessionSnapshotQueryParameters | any = null) : Observable<Array<TelemetrySessionSnapshotBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const telemetrySessionSnapshotsBasicListData$ = this.requestTelemetrySessionSnapshotsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetrySessionSnapshots basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, telemetrySessionSnapshotsBasicListData$);

            return telemetrySessionSnapshotsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TelemetrySessionSnapshotBasicListData>>;
    }


    private requestTelemetrySessionSnapshotsBasicListData(config: TelemetrySessionSnapshotQueryParameters | any) : Observable<Array<TelemetrySessionSnapshotBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetrySessionSnapshotBasicListData>>(this.baseUrl + 'api/TelemetrySessionSnapshots/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetrySessionSnapshotsBasicListData(config));
            }));

    }


    public PutTelemetrySessionSnapshot(id: bigint | number, telemetrySessionSnapshot: TelemetrySessionSnapshotSubmitData) : Observable<TelemetrySessionSnapshotData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TelemetrySessionSnapshotData>(this.baseUrl + 'api/TelemetrySessionSnapshot/' + id.toString(), telemetrySessionSnapshot, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetrySessionSnapshot(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTelemetrySessionSnapshot(id, telemetrySessionSnapshot));
            }));
    }


    public PostTelemetrySessionSnapshot(telemetrySessionSnapshot: TelemetrySessionSnapshotSubmitData) : Observable<TelemetrySessionSnapshotData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TelemetrySessionSnapshotData>(this.baseUrl + 'api/TelemetrySessionSnapshot', telemetrySessionSnapshot, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetrySessionSnapshot(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTelemetrySessionSnapshot(telemetrySessionSnapshot));
            }));
    }

  
    public DeleteTelemetrySessionSnapshot(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/TelemetrySessionSnapshot/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTelemetrySessionSnapshot(id));
            }));
    }


    private getConfigHash(config: TelemetrySessionSnapshotQueryParameters | any): string {

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

    public userIsTelemetryTelemetrySessionSnapshotReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsTelemetryTelemetrySessionSnapshotReader = this.authService.isTelemetryReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Telemetry.TelemetrySessionSnapshots
        //
        if (userIsTelemetryTelemetrySessionSnapshotReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsTelemetryTelemetrySessionSnapshotReader = user.readPermission >= 0;
            } else {
                userIsTelemetryTelemetrySessionSnapshotReader = false;
            }
        }

        return userIsTelemetryTelemetrySessionSnapshotReader;
    }


    public userIsTelemetryTelemetrySessionSnapshotWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsTelemetryTelemetrySessionSnapshotWriter = this.authService.isTelemetryReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Telemetry.TelemetrySessionSnapshots
        //
        if (userIsTelemetryTelemetrySessionSnapshotWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsTelemetryTelemetrySessionSnapshotWriter = user.writePermission >= 0;
          } else {
            userIsTelemetryTelemetrySessionSnapshotWriter = false;
          }      
        }

        return userIsTelemetryTelemetrySessionSnapshotWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full TelemetrySessionSnapshotData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TelemetrySessionSnapshotData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TelemetrySessionSnapshotTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTelemetrySessionSnapshot(raw: any): TelemetrySessionSnapshotData {
    if (!raw) return raw;

    //
    // Create a TelemetrySessionSnapshotData object instance with correct prototype
    //
    const revived = Object.create(TelemetrySessionSnapshotData.prototype) as TelemetrySessionSnapshotData;

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
    // 2. But private methods (loadTelemetrySessionSnapshotXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveTelemetrySessionSnapshotList(rawList: any[]): TelemetrySessionSnapshotData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTelemetrySessionSnapshot(raw));
  }

}
