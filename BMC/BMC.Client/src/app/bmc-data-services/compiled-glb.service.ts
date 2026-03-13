/*

   GENERATED SERVICE FOR THE COMPILEDGLB TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the CompiledGlb table.

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
import { ProjectData } from './project.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class CompiledGlbQueryParameters {
    projectId: bigint | number | null | undefined = null;
    projectVersionNumber: bigint | number | null | undefined = null;
    includesEdgeLines: boolean | null | undefined = null;
    glbSizeBytes: bigint | number | null | undefined = null;
    triangleCount: bigint | number | null | undefined = null;
    stepCount: bigint | number | null | undefined = null;
    compiledAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    objectGuid: string | null | undefined = null;
    active: boolean | null | undefined = null;
    deleted: boolean | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class CompiledGlbSubmitData {
    id!: bigint | number;
    projectId!: bigint | number;
    projectVersionNumber!: bigint | number;
    includesEdgeLines!: boolean;
    glbData: string | null = null;
    glbSizeBytes!: bigint | number;
    triangleCount: bigint | number | null = null;
    stepCount: bigint | number | null = null;
    compiledAt!: string;      // ISO 8601 (full datetime)
    active!: boolean;
    deleted!: boolean;
}


export class CompiledGlbBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. CompiledGlbChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `compiledGlb.CompiledGlbChildren$` — use with `| async` in templates
//        • Promise:    `compiledGlb.CompiledGlbChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="compiledGlb.CompiledGlbChildren$ | async"`), or
//        • Access the promise getter (`compiledGlb.CompiledGlbChildren` or `await compiledGlb.CompiledGlbChildren`)
//    - Simply reading `compiledGlb.CompiledGlbChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await compiledGlb.Reload()` to refresh the entire object and clear all lazy caches.
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
export class CompiledGlbData {
    id!: bigint | number;
    projectId!: bigint | number;
    projectVersionNumber!: bigint | number;
    includesEdgeLines!: boolean;
    glbData!: string | null;
    glbSizeBytes!: bigint | number;
    triangleCount!: bigint | number;
    stepCount!: bigint | number;
    compiledAt!: string;      // ISO 8601 (full datetime)
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    project: ProjectData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any CompiledGlbData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.compiledGlb.Reload();
  //
  //  Non Async:
  //
  //     compiledGlb[0].Reload().then(x => {
  //        this.compiledGlb = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      CompiledGlbService.Instance.GetCompiledGlb(this.id, includeRelations)
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
     * Updates the state of this CompiledGlbData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this CompiledGlbData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): CompiledGlbSubmitData {
        return CompiledGlbService.Instance.ConvertToCompiledGlbSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class CompiledGlbService extends SecureEndpointBase {

    private static _instance: CompiledGlbService;
    private listCache: Map<string, Observable<Array<CompiledGlbData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<CompiledGlbBasicListData>>>;
    private recordCache: Map<string, Observable<CompiledGlbData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<CompiledGlbData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<CompiledGlbBasicListData>>>();
        this.recordCache = new Map<string, Observable<CompiledGlbData>>();

        CompiledGlbService._instance = this;
    }

    public static get Instance(): CompiledGlbService {
      return CompiledGlbService._instance;
    }


    public ClearListCaches(config: CompiledGlbQueryParameters | null = null) {

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


    public ConvertToCompiledGlbSubmitData(data: CompiledGlbData): CompiledGlbSubmitData {

        let output = new CompiledGlbSubmitData();

        output.id = data.id;
        output.projectId = data.projectId;
        output.projectVersionNumber = data.projectVersionNumber;
        output.includesEdgeLines = data.includesEdgeLines;
        output.glbData = data.glbData;
        output.glbSizeBytes = data.glbSizeBytes;
        output.triangleCount = data.triangleCount;
        output.stepCount = data.stepCount;
        output.compiledAt = data.compiledAt;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetCompiledGlb(id: bigint | number, includeRelations: boolean = true) : Observable<CompiledGlbData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const compiledGlb$ = this.requestCompiledGlb(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get CompiledGlb", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, compiledGlb$);

            return compiledGlb$;
        }

        return this.recordCache.get(configHash) as Observable<CompiledGlbData>;
    }

    private requestCompiledGlb(id: bigint | number, includeRelations: boolean = true) : Observable<CompiledGlbData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CompiledGlbData>(this.baseUrl + 'api/CompiledGlb/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveCompiledGlb(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestCompiledGlb(id, includeRelations));
            }));
    }

    public GetCompiledGlbList(config: CompiledGlbQueryParameters | any = null) : Observable<Array<CompiledGlbData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const compiledGlbList$ = this.requestCompiledGlbList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get CompiledGlb list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, compiledGlbList$);

            return compiledGlbList$;
        }

        return this.listCache.get(configHash) as Observable<Array<CompiledGlbData>>;
    }


    private requestCompiledGlbList(config: CompiledGlbQueryParameters | any) : Observable <Array<CompiledGlbData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CompiledGlbData>>(this.baseUrl + 'api/CompiledGlbs', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveCompiledGlbList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestCompiledGlbList(config));
            }));
    }

    public GetCompiledGlbsRowCount(config: CompiledGlbQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const compiledGlbsRowCount$ = this.requestCompiledGlbsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get CompiledGlbs row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, compiledGlbsRowCount$);

            return compiledGlbsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestCompiledGlbsRowCount(config: CompiledGlbQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/CompiledGlbs/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCompiledGlbsRowCount(config));
            }));
    }

    public GetCompiledGlbsBasicListData(config: CompiledGlbQueryParameters | any = null) : Observable<Array<CompiledGlbBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const compiledGlbsBasicListData$ = this.requestCompiledGlbsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get CompiledGlbs basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, compiledGlbsBasicListData$);

            return compiledGlbsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<CompiledGlbBasicListData>>;
    }


    private requestCompiledGlbsBasicListData(config: CompiledGlbQueryParameters | any) : Observable<Array<CompiledGlbBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CompiledGlbBasicListData>>(this.baseUrl + 'api/CompiledGlbs/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCompiledGlbsBasicListData(config));
            }));

    }


    public PutCompiledGlb(id: bigint | number, compiledGlb: CompiledGlbSubmitData) : Observable<CompiledGlbData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CompiledGlbData>(this.baseUrl + 'api/CompiledGlb/' + id.toString(), compiledGlb, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCompiledGlb(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutCompiledGlb(id, compiledGlb));
            }));
    }


    public PostCompiledGlb(compiledGlb: CompiledGlbSubmitData) : Observable<CompiledGlbData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<CompiledGlbData>(this.baseUrl + 'api/CompiledGlb', compiledGlb, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCompiledGlb(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostCompiledGlb(compiledGlb));
            }));
    }

  
    public DeleteCompiledGlb(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/CompiledGlb/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteCompiledGlb(id));
            }));
    }


    private getConfigHash(config: CompiledGlbQueryParameters | any): string {

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

    public userIsBMCCompiledGlbReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCCompiledGlbReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.CompiledGlbs
        //
        if (userIsBMCCompiledGlbReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCCompiledGlbReader = user.readPermission >= 1;
            } else {
                userIsBMCCompiledGlbReader = false;
            }
        }

        return userIsBMCCompiledGlbReader;
    }


    public userIsBMCCompiledGlbWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCCompiledGlbWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.CompiledGlbs
        //
        if (userIsBMCCompiledGlbWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCCompiledGlbWriter = user.writePermission >= 1;
          } else {
            userIsBMCCompiledGlbWriter = false;
          }      
        }

        return userIsBMCCompiledGlbWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full CompiledGlbData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the CompiledGlbData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when CompiledGlbTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveCompiledGlb(raw: any): CompiledGlbData {
    if (!raw) return raw;

    //
    // Create a CompiledGlbData object instance with correct prototype
    //
    const revived = Object.create(CompiledGlbData.prototype) as CompiledGlbData;

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
    // 2. But private methods (loadCompiledGlbXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveCompiledGlbList(rawList: any[]): CompiledGlbData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveCompiledGlb(raw));
  }

}
