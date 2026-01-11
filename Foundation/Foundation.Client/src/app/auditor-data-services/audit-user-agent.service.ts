import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, BehaviorSubject, catchError, throwError, lastValueFrom, map  } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';
import { AuditEventService, AuditEventData } from './audit-event.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AuditUserAgentQueryParameters {
    name: string | null | undefined = null;
    comments: string | null | undefined = null;
    firstAccess: string | null | undefined = null;        // ISO 8601
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class AuditUserAgentSubmitData {
    id!: bigint | number;
    name!: string;
    comments: string | null = null;
    firstAccess: string | null = null;     // ISO 8601
}


export class AuditUserAgentBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AuditUserAgentChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `auditUserAgent.AuditUserAgentChildren$` — use with `| async` in templates
//        • Promise:    `auditUserAgent.AuditUserAgentChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="auditUserAgent.AuditUserAgentChildren$ | async"`), or
//        • Access the promise getter (`auditUserAgent.AuditUserAgentChildren` or `await auditUserAgent.AuditUserAgentChildren`)
//    - Simply reading `auditUserAgent.AuditUserAgentChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await auditUserAgent.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AuditUserAgentData {
    id!: bigint | number;
    name!: string;
    comments!: string | null;
    firstAccess!: string | null;   // ISO 8601

    //
    // Private lazy-loading caches for related collections
    //
    private _auditEvents: AuditEventData[] | null = null;
    private _auditEventsPromise: Promise<AuditEventData[]> | null  = null;
    private _auditEventsSubject = new BehaviorSubject<AuditEventData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public AuditEvents$ = this._auditEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._auditEvents === null && this._auditEventsPromise === null) {
            this.loadAuditEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public AuditEventsCount$ = AuditEventService.Instance.GetAuditEventsRowCount({auditUserAgentId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AuditUserAgentData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.auditUserAgent.Reload();
  //
  //  Non Async:
  //
  //     auditUserAgent[0].Reload().then(x => {
  //        this.auditUserAgent = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AuditUserAgentService.Instance.GetAuditUserAgent(this.id, includeRelations)
    );

    // Merge fresh data into this instance (preserves reference)
    this.UpdateFrom(fresh as this);

    // Clear all lazy caches to force re-load on next access
    this.clearAllLazyCaches();

    return this;
  }


  private clearAllLazyCaches(): void {
     // Reset every collection cache and notify subscribers
     this._auditEvents = null;
     this._auditEventsPromise = null;
     this._auditEventsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the AuditEvents for this AuditUserAgent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.auditUserAgent.AuditEvents.then(auditEvents => { ... })
     *   or
     *   await this.auditUserAgent.AuditEvents
     *
    */
    public get AuditEvents(): Promise<AuditEventData[]> {
        if (this._auditEvents !== null) {
            return Promise.resolve(this._auditEvents);
        }

        if (this._auditEventsPromise !== null) {
            return this._auditEventsPromise;
        }

        // Start the load
        this.loadAuditEvents();

        return this._auditEventsPromise!;
    }



    private loadAuditEvents(): void {

        this._auditEventsPromise = lastValueFrom(
            AuditUserAgentService.Instance.GetAuditEventsForAuditUserAgent(this.id)
        )
        .then(auditEvents => {
            this._auditEvents = auditEvents ?? [];
            this._auditEventsSubject.next(this._auditEvents);
            return this._auditEvents;
         })
        .catch(err => {
            this._auditEvents = [];
            this._auditEventsSubject.next(this._auditEvents);
            throw err;
        })
        .finally(() => {
            this._auditEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearAuditEventsCache(): void {
        this._auditEvents = null;
        this._auditEventsPromise = null;
        this._auditEventsSubject.next(this._auditEvents);      // Emit to observable
    }

    public get HasAuditEvents(): Promise<boolean> {
        return this.AuditEvents.then(auditEvents => auditEvents.length > 0);
    }




    /**
     * Updates the state of this AuditUserAgentData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AuditUserAgentData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AuditUserAgentSubmitData {
        return AuditUserAgentService.Instance.ConvertToAuditUserAgentSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AuditUserAgentService extends SecureEndpointBase {

    private static _instance: AuditUserAgentService;
    private listCache: Map<string, Observable<Array<AuditUserAgentData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AuditUserAgentBasicListData>>>;
    private recordCache: Map<string, Observable<AuditUserAgentData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private auditEventService: AuditEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AuditUserAgentData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AuditUserAgentBasicListData>>>();
        this.recordCache = new Map<string, Observable<AuditUserAgentData>>();

        AuditUserAgentService._instance = this;
    }

    public static get Instance(): AuditUserAgentService {
      return AuditUserAgentService._instance;
    }


    public ClearListCaches(config: AuditUserAgentQueryParameters | null = null) {

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


    public ConvertToAuditUserAgentSubmitData(data: AuditUserAgentData): AuditUserAgentSubmitData {

        let output = new AuditUserAgentSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.comments = data.comments;
        output.firstAccess = data.firstAccess;

        return output;
    }

    public GetAuditUserAgent(id: bigint | number, includeRelations: boolean = true) : Observable<AuditUserAgentData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const auditUserAgent$ = this.requestAuditUserAgent(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditUserAgent", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, auditUserAgent$);

            return auditUserAgent$;
        }

        return this.recordCache.get(configHash) as Observable<AuditUserAgentData>;
    }

    private requestAuditUserAgent(id: bigint | number, includeRelations: boolean = true) : Observable<AuditUserAgentData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AuditUserAgentData>(this.baseUrl + 'api/AuditUserAgent/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAuditUserAgent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditUserAgent(id, includeRelations));
            }));
    }

    public GetAuditUserAgentList(config: AuditUserAgentQueryParameters | any = null) : Observable<Array<AuditUserAgentData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const auditUserAgentList$ = this.requestAuditUserAgentList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditUserAgent list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, auditUserAgentList$);

            return auditUserAgentList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AuditUserAgentData>>;
    }


    private requestAuditUserAgentList(config: AuditUserAgentQueryParameters | any) : Observable <Array<AuditUserAgentData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditUserAgentData>>(this.baseUrl + 'api/AuditUserAgents', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAuditUserAgentList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditUserAgentList(config));
            }));
    }

    public GetAuditUserAgentsRowCount(config: AuditUserAgentQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const auditUserAgentsRowCount$ = this.requestAuditUserAgentsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditUserAgents row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, auditUserAgentsRowCount$);

            return auditUserAgentsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAuditUserAgentsRowCount(config: AuditUserAgentQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AuditUserAgents/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditUserAgentsRowCount(config));
            }));
    }

    public GetAuditUserAgentsBasicListData(config: AuditUserAgentQueryParameters | any = null) : Observable<Array<AuditUserAgentBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const auditUserAgentsBasicListData$ = this.requestAuditUserAgentsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditUserAgents basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, auditUserAgentsBasicListData$);

            return auditUserAgentsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AuditUserAgentBasicListData>>;
    }


    private requestAuditUserAgentsBasicListData(config: AuditUserAgentQueryParameters | any) : Observable<Array<AuditUserAgentBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditUserAgentBasicListData>>(this.baseUrl + 'api/AuditUserAgents/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditUserAgentsBasicListData(config));
            }));

    }


    public PutAuditUserAgent(id: bigint | number, auditUserAgent: AuditUserAgentSubmitData) : Observable<AuditUserAgentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AuditUserAgentData>(this.baseUrl + 'api/AuditUserAgent/' + id.toString(), auditUserAgent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditUserAgent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAuditUserAgent(id, auditUserAgent));
            }));
    }


    public PostAuditUserAgent(auditUserAgent: AuditUserAgentSubmitData) : Observable<AuditUserAgentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AuditUserAgentData>(this.baseUrl + 'api/AuditUserAgent', auditUserAgent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditUserAgent(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAuditUserAgent(auditUserAgent));
            }));
    }

  
    public DeleteAuditUserAgent(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AuditUserAgent/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAuditUserAgent(id));
            }));
    }


    private getConfigHash(config: AuditUserAgentQueryParameters | any): string {

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

    public userIsAuditorAuditUserAgentReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAuditorAuditUserAgentReader = this.authService.isAuditorReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Auditor.AuditUserAgents
        //
        if (userIsAuditorAuditUserAgentReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAuditorAuditUserAgentReader = user.readPermission >= 0;
            } else {
                userIsAuditorAuditUserAgentReader = false;
            }
        }

        return userIsAuditorAuditUserAgentReader;
    }


    public userIsAuditorAuditUserAgentWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAuditorAuditUserAgentWriter = this.authService.isAuditorReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Auditor.AuditUserAgents
        //
        if (userIsAuditorAuditUserAgentWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAuditorAuditUserAgentWriter = user.writePermission >= 0;
          } else {
            userIsAuditorAuditUserAgentWriter = false;
          }      
        }

        return userIsAuditorAuditUserAgentWriter;
    }

    public GetAuditEventsForAuditUserAgent(auditUserAgentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AuditEventData[]> {
        return this.auditEventService.GetAuditEventList({
            auditUserAgentId: auditUserAgentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AuditUserAgentData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AuditUserAgentData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AuditUserAgentTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAuditUserAgent(raw: any): AuditUserAgentData {
    if (!raw) return raw;

    //
    // Create a AuditUserAgentData object instance with correct prototype
    //
    const revived = Object.create(AuditUserAgentData.prototype) as AuditUserAgentData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._auditEvents = null;
    (revived as any)._auditEventsPromise = null;
    (revived as any)._auditEventsSubject = new BehaviorSubject<AuditEventData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAuditUserAgentXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).AuditEvents$ = (revived as any)._auditEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._auditEvents === null && (revived as any)._auditEventsPromise === null) {
                (revived as any).loadAuditEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).AuditEventsCount$ = AuditEventService.Instance.GetAuditEventsRowCount({auditUserAgentId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveAuditUserAgentList(rawList: any[]): AuditUserAgentData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAuditUserAgent(raw));
  }

}
