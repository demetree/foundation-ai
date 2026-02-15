/*

   GENERATED SERVICE FOR THE TELEMETRYLOGERROR TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the TelemetryLogError table.

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
import { TelemetryApplicationData } from './telemetry-application.service';
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
export class TelemetryLogErrorQueryParameters {
    telemetryApplicationId: bigint | number | null | undefined = null;
    telemetrySnapshotId: bigint | number | null | undefined = null;
    capturedAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    logFileName: string | null | undefined = null;
    logTimestamp: string | null | undefined = null;        // ISO 8601 (full datetime)
    level: string | null | undefined = null;
    message: string | null | undefined = null;
    exception: string | null | undefined = null;
    occurrenceCount: bigint | number | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class TelemetryLogErrorSubmitData {
    id!: bigint | number;
    telemetryApplicationId!: bigint | number;
    telemetrySnapshotId: bigint | number | null = null;
    capturedAt!: string;      // ISO 8601 (full datetime)
    logFileName: string | null = null;
    logTimestamp: string | null = null;     // ISO 8601 (full datetime)
    level: string | null = null;
    message: string | null = null;
    exception: string | null = null;
    occurrenceCount!: bigint | number;
}


export class TelemetryLogErrorBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TelemetryLogErrorChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `telemetryLogError.TelemetryLogErrorChildren$` — use with `| async` in templates
//        • Promise:    `telemetryLogError.TelemetryLogErrorChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="telemetryLogError.TelemetryLogErrorChildren$ | async"`), or
//        • Access the promise getter (`telemetryLogError.TelemetryLogErrorChildren` or `await telemetryLogError.TelemetryLogErrorChildren`)
//    - Simply reading `telemetryLogError.TelemetryLogErrorChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await telemetryLogError.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TelemetryLogErrorData {
    id!: bigint | number;
    telemetryApplicationId!: bigint | number;
    telemetrySnapshotId!: bigint | number;
    capturedAt!: string;      // ISO 8601 (full datetime)
    logFileName!: string | null;
    logTimestamp!: string | null;   // ISO 8601 (full datetime)
    level!: string | null;
    message!: string | null;
    exception!: string | null;
    occurrenceCount!: bigint | number;
    telemetryApplication: TelemetryApplicationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
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
  // Promise based reload method to allow rebuilding of any TelemetryLogErrorData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.telemetryLogError.Reload();
  //
  //  Non Async:
  //
  //     telemetryLogError[0].Reload().then(x => {
  //        this.telemetryLogError = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TelemetryLogErrorService.Instance.GetTelemetryLogError(this.id, includeRelations)
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
     * Updates the state of this TelemetryLogErrorData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TelemetryLogErrorData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TelemetryLogErrorSubmitData {
        return TelemetryLogErrorService.Instance.ConvertToTelemetryLogErrorSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TelemetryLogErrorService extends SecureEndpointBase {

    private static _instance: TelemetryLogErrorService;
    private listCache: Map<string, Observable<Array<TelemetryLogErrorData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TelemetryLogErrorBasicListData>>>;
    private recordCache: Map<string, Observable<TelemetryLogErrorData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TelemetryLogErrorData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TelemetryLogErrorBasicListData>>>();
        this.recordCache = new Map<string, Observable<TelemetryLogErrorData>>();

        TelemetryLogErrorService._instance = this;
    }

    public static get Instance(): TelemetryLogErrorService {
      return TelemetryLogErrorService._instance;
    }


    public ClearListCaches(config: TelemetryLogErrorQueryParameters | null = null) {

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


    public ConvertToTelemetryLogErrorSubmitData(data: TelemetryLogErrorData): TelemetryLogErrorSubmitData {

        let output = new TelemetryLogErrorSubmitData();

        output.id = data.id;
        output.telemetryApplicationId = data.telemetryApplicationId;
        output.telemetrySnapshotId = data.telemetrySnapshotId;
        output.capturedAt = data.capturedAt;
        output.logFileName = data.logFileName;
        output.logTimestamp = data.logTimestamp;
        output.level = data.level;
        output.message = data.message;
        output.exception = data.exception;
        output.occurrenceCount = data.occurrenceCount;

        return output;
    }

    public GetTelemetryLogError(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetryLogErrorData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const telemetryLogError$ = this.requestTelemetryLogError(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryLogError", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, telemetryLogError$);

            return telemetryLogError$;
        }

        return this.recordCache.get(configHash) as Observable<TelemetryLogErrorData>;
    }

    private requestTelemetryLogError(id: bigint | number, includeRelations: boolean = true) : Observable<TelemetryLogErrorData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TelemetryLogErrorData>(this.baseUrl + 'api/TelemetryLogError/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTelemetryLogError(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryLogError(id, includeRelations));
            }));
    }

    public GetTelemetryLogErrorList(config: TelemetryLogErrorQueryParameters | any = null) : Observable<Array<TelemetryLogErrorData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const telemetryLogErrorList$ = this.requestTelemetryLogErrorList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryLogError list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, telemetryLogErrorList$);

            return telemetryLogErrorList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TelemetryLogErrorData>>;
    }


    private requestTelemetryLogErrorList(config: TelemetryLogErrorQueryParameters | any) : Observable <Array<TelemetryLogErrorData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetryLogErrorData>>(this.baseUrl + 'api/TelemetryLogErrors', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTelemetryLogErrorList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryLogErrorList(config));
            }));
    }

    public GetTelemetryLogErrorsRowCount(config: TelemetryLogErrorQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const telemetryLogErrorsRowCount$ = this.requestTelemetryLogErrorsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryLogErrors row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, telemetryLogErrorsRowCount$);

            return telemetryLogErrorsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTelemetryLogErrorsRowCount(config: TelemetryLogErrorQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/TelemetryLogErrors/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryLogErrorsRowCount(config));
            }));
    }

    public GetTelemetryLogErrorsBasicListData(config: TelemetryLogErrorQueryParameters | any = null) : Observable<Array<TelemetryLogErrorBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const telemetryLogErrorsBasicListData$ = this.requestTelemetryLogErrorsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TelemetryLogErrors basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, telemetryLogErrorsBasicListData$);

            return telemetryLogErrorsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TelemetryLogErrorBasicListData>>;
    }


    private requestTelemetryLogErrorsBasicListData(config: TelemetryLogErrorQueryParameters | any) : Observable<Array<TelemetryLogErrorBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TelemetryLogErrorBasicListData>>(this.baseUrl + 'api/TelemetryLogErrors/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTelemetryLogErrorsBasicListData(config));
            }));

    }


    public PutTelemetryLogError(id: bigint | number, telemetryLogError: TelemetryLogErrorSubmitData) : Observable<TelemetryLogErrorData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TelemetryLogErrorData>(this.baseUrl + 'api/TelemetryLogError/' + id.toString(), telemetryLogError, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetryLogError(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTelemetryLogError(id, telemetryLogError));
            }));
    }


    public PostTelemetryLogError(telemetryLogError: TelemetryLogErrorSubmitData) : Observable<TelemetryLogErrorData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TelemetryLogErrorData>(this.baseUrl + 'api/TelemetryLogError', telemetryLogError, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTelemetryLogError(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTelemetryLogError(telemetryLogError));
            }));
    }

  
    public DeleteTelemetryLogError(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/TelemetryLogError/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTelemetryLogError(id));
            }));
    }


    private getConfigHash(config: TelemetryLogErrorQueryParameters | any): string {

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

    public userIsTelemetryTelemetryLogErrorReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsTelemetryTelemetryLogErrorReader = this.authService.isTelemetryReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Telemetry.TelemetryLogErrors
        //
        if (userIsTelemetryTelemetryLogErrorReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsTelemetryTelemetryLogErrorReader = user.readPermission >= 0;
            } else {
                userIsTelemetryTelemetryLogErrorReader = false;
            }
        }

        return userIsTelemetryTelemetryLogErrorReader;
    }


    public userIsTelemetryTelemetryLogErrorWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsTelemetryTelemetryLogErrorWriter = this.authService.isTelemetryReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Telemetry.TelemetryLogErrors
        //
        if (userIsTelemetryTelemetryLogErrorWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsTelemetryTelemetryLogErrorWriter = user.writePermission >= 0;
          } else {
            userIsTelemetryTelemetryLogErrorWriter = false;
          }      
        }

        return userIsTelemetryTelemetryLogErrorWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full TelemetryLogErrorData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TelemetryLogErrorData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TelemetryLogErrorTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTelemetryLogError(raw: any): TelemetryLogErrorData {
    if (!raw) return raw;

    //
    // Create a TelemetryLogErrorData object instance with correct prototype
    //
    const revived = Object.create(TelemetryLogErrorData.prototype) as TelemetryLogErrorData;

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
    // 2. But private methods (loadTelemetryLogErrorXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveTelemetryLogErrorList(rawList: any[]): TelemetryLogErrorData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTelemetryLogError(raw));
  }

}
