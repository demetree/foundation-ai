/*

   GENERATED SERVICE FOR THE AUDITEVENTENTITYSTATE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AuditEventEntityState table.

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
import { AuditEventData } from './audit-event.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AuditEventEntityStateQueryParameters {
    auditEventId: bigint | number | null | undefined = null;
    beforeState: string | null | undefined = null;
    afterState: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class AuditEventEntityStateSubmitData {
    id!: bigint | number;
    auditEventId!: bigint | number;
    beforeState: string | null = null;
    afterState: string | null = null;
}


export class AuditEventEntityStateBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AuditEventEntityStateChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `auditEventEntityState.AuditEventEntityStateChildren$` — use with `| async` in templates
//        • Promise:    `auditEventEntityState.AuditEventEntityStateChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="auditEventEntityState.AuditEventEntityStateChildren$ | async"`), or
//        • Access the promise getter (`auditEventEntityState.AuditEventEntityStateChildren` or `await auditEventEntityState.AuditEventEntityStateChildren`)
//    - Simply reading `auditEventEntityState.AuditEventEntityStateChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await auditEventEntityState.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AuditEventEntityStateData {
    id!: bigint | number;
    auditEventId!: bigint | number;
    beforeState!: string | null;
    afterState!: string | null;
    auditEvent: AuditEventData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any AuditEventEntityStateData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.auditEventEntityState.Reload();
  //
  //  Non Async:
  //
  //     auditEventEntityState[0].Reload().then(x => {
  //        this.auditEventEntityState = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AuditEventEntityStateService.Instance.GetAuditEventEntityState(this.id, includeRelations)
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
     * Updates the state of this AuditEventEntityStateData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AuditEventEntityStateData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AuditEventEntityStateSubmitData {
        return AuditEventEntityStateService.Instance.ConvertToAuditEventEntityStateSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AuditEventEntityStateService extends SecureEndpointBase {

    private static _instance: AuditEventEntityStateService;
    private listCache: Map<string, Observable<Array<AuditEventEntityStateData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AuditEventEntityStateBasicListData>>>;
    private recordCache: Map<string, Observable<AuditEventEntityStateData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AuditEventEntityStateData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AuditEventEntityStateBasicListData>>>();
        this.recordCache = new Map<string, Observable<AuditEventEntityStateData>>();

        AuditEventEntityStateService._instance = this;
    }

    public static get Instance(): AuditEventEntityStateService {
      return AuditEventEntityStateService._instance;
    }


    public ClearListCaches(config: AuditEventEntityStateQueryParameters | null = null) {

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


    public ConvertToAuditEventEntityStateSubmitData(data: AuditEventEntityStateData): AuditEventEntityStateSubmitData {

        let output = new AuditEventEntityStateSubmitData();

        output.id = data.id;
        output.auditEventId = data.auditEventId;
        output.beforeState = data.beforeState;
        output.afterState = data.afterState;

        return output;
    }

    public GetAuditEventEntityState(id: bigint | number, includeRelations: boolean = true) : Observable<AuditEventEntityStateData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const auditEventEntityState$ = this.requestAuditEventEntityState(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditEventEntityState", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, auditEventEntityState$);

            return auditEventEntityState$;
        }

        return this.recordCache.get(configHash) as Observable<AuditEventEntityStateData>;
    }

    private requestAuditEventEntityState(id: bigint | number, includeRelations: boolean = true) : Observable<AuditEventEntityStateData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AuditEventEntityStateData>(this.baseUrl + 'api/AuditEventEntityState/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAuditEventEntityState(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditEventEntityState(id, includeRelations));
            }));
    }

    public GetAuditEventEntityStateList(config: AuditEventEntityStateQueryParameters | any = null) : Observable<Array<AuditEventEntityStateData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const auditEventEntityStateList$ = this.requestAuditEventEntityStateList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditEventEntityState list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, auditEventEntityStateList$);

            return auditEventEntityStateList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AuditEventEntityStateData>>;
    }


    private requestAuditEventEntityStateList(config: AuditEventEntityStateQueryParameters | any) : Observable <Array<AuditEventEntityStateData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditEventEntityStateData>>(this.baseUrl + 'api/AuditEventEntityStates', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAuditEventEntityStateList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditEventEntityStateList(config));
            }));
    }

    public GetAuditEventEntityStatesRowCount(config: AuditEventEntityStateQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const auditEventEntityStatesRowCount$ = this.requestAuditEventEntityStatesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditEventEntityStates row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, auditEventEntityStatesRowCount$);

            return auditEventEntityStatesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAuditEventEntityStatesRowCount(config: AuditEventEntityStateQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AuditEventEntityStates/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditEventEntityStatesRowCount(config));
            }));
    }

    public GetAuditEventEntityStatesBasicListData(config: AuditEventEntityStateQueryParameters | any = null) : Observable<Array<AuditEventEntityStateBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const auditEventEntityStatesBasicListData$ = this.requestAuditEventEntityStatesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditEventEntityStates basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, auditEventEntityStatesBasicListData$);

            return auditEventEntityStatesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AuditEventEntityStateBasicListData>>;
    }


    private requestAuditEventEntityStatesBasicListData(config: AuditEventEntityStateQueryParameters | any) : Observable<Array<AuditEventEntityStateBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditEventEntityStateBasicListData>>(this.baseUrl + 'api/AuditEventEntityStates/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditEventEntityStatesBasicListData(config));
            }));

    }


    public PutAuditEventEntityState(id: bigint | number, auditEventEntityState: AuditEventEntityStateSubmitData) : Observable<AuditEventEntityStateData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AuditEventEntityStateData>(this.baseUrl + 'api/AuditEventEntityState/' + id.toString(), auditEventEntityState, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditEventEntityState(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAuditEventEntityState(id, auditEventEntityState));
            }));
    }


    public PostAuditEventEntityState(auditEventEntityState: AuditEventEntityStateSubmitData) : Observable<AuditEventEntityStateData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AuditEventEntityStateData>(this.baseUrl + 'api/AuditEventEntityState', auditEventEntityState, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditEventEntityState(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAuditEventEntityState(auditEventEntityState));
            }));
    }

  
    public DeleteAuditEventEntityState(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AuditEventEntityState/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAuditEventEntityState(id));
            }));
    }


    private getConfigHash(config: AuditEventEntityStateQueryParameters | any): string {

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

    public userIsAuditorAuditEventEntityStateReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAuditorAuditEventEntityStateReader = this.authService.isAuditorReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Auditor.AuditEventEntityStates
        //
        if (userIsAuditorAuditEventEntityStateReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAuditorAuditEventEntityStateReader = user.readPermission >= 0;
            } else {
                userIsAuditorAuditEventEntityStateReader = false;
            }
        }

        return userIsAuditorAuditEventEntityStateReader;
    }


    public userIsAuditorAuditEventEntityStateWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAuditorAuditEventEntityStateWriter = this.authService.isAuditorReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Auditor.AuditEventEntityStates
        //
        if (userIsAuditorAuditEventEntityStateWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAuditorAuditEventEntityStateWriter = user.writePermission >= 0;
          } else {
            userIsAuditorAuditEventEntityStateWriter = false;
          }      
        }

        return userIsAuditorAuditEventEntityStateWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full AuditEventEntityStateData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AuditEventEntityStateData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AuditEventEntityStateTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAuditEventEntityState(raw: any): AuditEventEntityStateData {
    if (!raw) return raw;

    //
    // Create a AuditEventEntityStateData object instance with correct prototype
    //
    const revived = Object.create(AuditEventEntityStateData.prototype) as AuditEventEntityStateData;

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
    // 2. But private methods (loadAuditEventEntityStateXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveAuditEventEntityStateList(rawList: any[]): AuditEventEntityStateData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAuditEventEntityState(raw));
  }

}
