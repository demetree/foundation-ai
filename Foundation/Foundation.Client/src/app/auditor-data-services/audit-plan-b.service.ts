import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, BehaviorSubject, catchError, throwError, lastValueFrom, map  } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AuditPlanBQueryParameters {
    startTime: string | null | undefined = null;        // ISO 8601
    stopTime: string | null | undefined = null;        // ISO 8601
    completedSuccessfully: boolean | null | undefined = null;
    user: string | null | undefined = null;
    session: string | null | undefined = null;
    type: string | null | undefined = null;
    accessType: string | null | undefined = null;
    source: string | null | undefined = null;
    userAgent: string | null | undefined = null;
    module: string | null | undefined = null;
    moduleEntity: string | null | undefined = null;
    resource: string | null | undefined = null;
    hostSystem: string | null | undefined = null;
    primaryKey: string | null | undefined = null;
    threadId: bigint | number | null | undefined = null;
    message: string | null | undefined = null;
    beforeState: string | null | undefined = null;
    afterState: string | null | undefined = null;
    errorMessage: string | null | undefined = null;
    exceptionText: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class AuditPlanBSubmitData {
    id!: bigint | number;
    startTime!: string;      // ISO 8601
    stopTime!: string;      // ISO 8601
    completedSuccessfully!: boolean;
    user: string | null = null;
    session: string | null = null;
    type: string | null = null;
    accessType: string | null = null;
    source: string | null = null;
    userAgent: string | null = null;
    module: string | null = null;
    moduleEntity: string | null = null;
    resource: string | null = null;
    hostSystem: string | null = null;
    primaryKey: string | null = null;
    threadId: bigint | number | null = null;
    message: string | null = null;
    beforeState: string | null = null;
    afterState: string | null = null;
    errorMessage: string | null = null;
    exceptionText: string | null = null;
}


export class AuditPlanBBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AuditPlanBChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `auditPlanB.AuditPlanBChildren$` — use with `| async` in templates
//        • Promise:    `auditPlanB.AuditPlanBChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="auditPlanB.AuditPlanBChildren$ | async"`), or
//        • Access the promise getter (`auditPlanB.AuditPlanBChildren` or `await auditPlanB.AuditPlanBChildren`)
//    - Simply reading `auditPlanB.AuditPlanBChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await auditPlanB.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AuditPlanBData {
    id!: bigint | number;
    startTime!: string;      // ISO 8601
    stopTime!: string;      // ISO 8601
    completedSuccessfully!: boolean;
    user!: string | null;
    session!: string | null;
    type!: string | null;
    accessType!: string | null;
    source!: string | null;
    userAgent!: string | null;
    module!: string | null;
    moduleEntity!: string | null;
    resource!: string | null;
    hostSystem!: string | null;
    primaryKey!: string | null;
    threadId!: bigint | number;
    message!: string | null;
    beforeState!: string | null;
    afterState!: string | null;
    errorMessage!: string | null;
    exceptionText!: string | null;

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
  // Promise based reload method to allow rebuilding of any AuditPlanBData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.auditPlanB.Reload();
  //
  //  Non Async:
  //
  //     auditPlanB[0].Reload().then(x => {
  //        this.auditPlanB = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AuditPlanBService.Instance.GetAuditPlanB(this.id, includeRelations)
    );

    // Merge fresh data into this instance (preserves reference)
    this.UpdateFrom(fresh as this);

    // Clear all lazy caches to force re-load on next access
    this.clearAllLazyCaches();

    return this;
  }


  private clearAllLazyCaches(): void {
     // Reset every collection cache and notify subscribers
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //


    /**
     * Updates the state of this AuditPlanBData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AuditPlanBData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AuditPlanBSubmitData {
        return AuditPlanBService.Instance.ConvertToAuditPlanBSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AuditPlanBService extends SecureEndpointBase {

    private static _instance: AuditPlanBService;
    private listCache: Map<string, Observable<Array<AuditPlanBData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AuditPlanBBasicListData>>>;
    private recordCache: Map<string, Observable<AuditPlanBData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AuditPlanBData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AuditPlanBBasicListData>>>();
        this.recordCache = new Map<string, Observable<AuditPlanBData>>();

        AuditPlanBService._instance = this;
    }

    public static get Instance(): AuditPlanBService {
      return AuditPlanBService._instance;
    }


    public ClearListCaches(config: AuditPlanBQueryParameters | null = null) {

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


    public ConvertToAuditPlanBSubmitData(data: AuditPlanBData): AuditPlanBSubmitData {

        let output = new AuditPlanBSubmitData();

        output.id = data.id;
        output.startTime = data.startTime;
        output.stopTime = data.stopTime;
        output.completedSuccessfully = data.completedSuccessfully;
        output.user = data.user;
        output.session = data.session;
        output.type = data.type;
        output.accessType = data.accessType;
        output.source = data.source;
        output.userAgent = data.userAgent;
        output.module = data.module;
        output.moduleEntity = data.moduleEntity;
        output.resource = data.resource;
        output.hostSystem = data.hostSystem;
        output.primaryKey = data.primaryKey;
        output.threadId = data.threadId;
        output.message = data.message;
        output.beforeState = data.beforeState;
        output.afterState = data.afterState;
        output.errorMessage = data.errorMessage;
        output.exceptionText = data.exceptionText;

        return output;
    }

    public GetAuditPlanB(id: bigint | number, includeRelations: boolean = true) : Observable<AuditPlanBData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const auditPlanB$ = this.requestAuditPlanB(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditPlanB", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, auditPlanB$);

            return auditPlanB$;
        }

        return this.recordCache.get(configHash) as Observable<AuditPlanBData>;
    }

    private requestAuditPlanB(id: bigint | number, includeRelations: boolean = true) : Observable<AuditPlanBData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AuditPlanBData>(this.baseUrl + 'api/AuditPlanB/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAuditPlanB(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditPlanB(id, includeRelations));
            }));
    }

    public GetAuditPlanBList(config: AuditPlanBQueryParameters | any = null) : Observable<Array<AuditPlanBData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const auditPlanBList$ = this.requestAuditPlanBList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditPlanB list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, auditPlanBList$);

            return auditPlanBList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AuditPlanBData>>;
    }


    private requestAuditPlanBList(config: AuditPlanBQueryParameters | any) : Observable <Array<AuditPlanBData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditPlanBData>>(this.baseUrl + 'api/AuditPlanBs', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAuditPlanBList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditPlanBList(config));
            }));
    }

    public GetAuditPlanBsRowCount(config: AuditPlanBQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const auditPlanBsRowCount$ = this.requestAuditPlanBsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditPlanBs row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, auditPlanBsRowCount$);

            return auditPlanBsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAuditPlanBsRowCount(config: AuditPlanBQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AuditPlanBs/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditPlanBsRowCount(config));
            }));
    }

    public GetAuditPlanBsBasicListData(config: AuditPlanBQueryParameters | any = null) : Observable<Array<AuditPlanBBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const auditPlanBsBasicListData$ = this.requestAuditPlanBsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditPlanBs basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, auditPlanBsBasicListData$);

            return auditPlanBsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AuditPlanBBasicListData>>;
    }


    private requestAuditPlanBsBasicListData(config: AuditPlanBQueryParameters | any) : Observable<Array<AuditPlanBBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditPlanBBasicListData>>(this.baseUrl + 'api/AuditPlanBs/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditPlanBsBasicListData(config));
            }));

    }


    public PutAuditPlanB(id: bigint | number, auditPlanB: AuditPlanBSubmitData) : Observable<AuditPlanBData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AuditPlanBData>(this.baseUrl + 'api/AuditPlanB/' + id.toString(), auditPlanB, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditPlanB(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAuditPlanB(id, auditPlanB));
            }));
    }


    public PostAuditPlanB(auditPlanB: AuditPlanBSubmitData) : Observable<AuditPlanBData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AuditPlanBData>(this.baseUrl + 'api/AuditPlanB', auditPlanB, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditPlanB(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAuditPlanB(auditPlanB));
            }));
    }

  
    public DeleteAuditPlanB(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AuditPlanB/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAuditPlanB(id));
            }));
    }


    private getConfigHash(config: AuditPlanBQueryParameters | any): string {

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

    public userIsAuditorAuditPlanBReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAuditorAuditPlanBReader = this.authService.isAuditorReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Auditor.AuditPlanBs
        //
        if (userIsAuditorAuditPlanBReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAuditorAuditPlanBReader = user.readPermission >= 0;
            } else {
                userIsAuditorAuditPlanBReader = false;
            }
        }

        return userIsAuditorAuditPlanBReader;
    }


    public userIsAuditorAuditPlanBWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAuditorAuditPlanBWriter = this.authService.isAuditorReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Auditor.AuditPlanBs
        //
        if (userIsAuditorAuditPlanBWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAuditorAuditPlanBWriter = user.writePermission >= 0;
          } else {
            userIsAuditorAuditPlanBWriter = false;
          }      
        }

        return userIsAuditorAuditPlanBWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full AuditPlanBData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AuditPlanBData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AuditPlanBTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAuditPlanB(raw: any): AuditPlanBData {
    if (!raw) return raw;

    //
    // Create a AuditPlanBData object instance with correct prototype
    //
    const revived = Object.create(AuditPlanBData.prototype) as AuditPlanBData;

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
    // 2. But private methods (loadAuditPlanBXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveAuditPlanBList(rawList: any[]): AuditPlanBData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAuditPlanB(raw));
  }

}
