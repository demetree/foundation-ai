/*

   GENERATED SERVICE FOR THE AUDITSESSION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AuditSession table.

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
export class AuditSessionQueryParameters {
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
export class AuditSessionSubmitData {
    id!: bigint | number;
    name!: string;
    comments: string | null = null;
    firstAccess: string | null = null;     // ISO 8601
}


export class AuditSessionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AuditSessionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `auditSession.AuditSessionChildren$` — use with `| async` in templates
//        • Promise:    `auditSession.AuditSessionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="auditSession.AuditSessionChildren$ | async"`), or
//        • Access the promise getter (`auditSession.AuditSessionChildren` or `await auditSession.AuditSessionChildren`)
//    - Simply reading `auditSession.AuditSessionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await auditSession.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AuditSessionData {
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

  
    public AuditEventsCount$ = AuditEventService.Instance.GetAuditEventsRowCount({auditSessionId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AuditSessionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.auditSession.Reload();
  //
  //  Non Async:
  //
  //     auditSession[0].Reload().then(x => {
  //        this.auditSession = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AuditSessionService.Instance.GetAuditSession(this.id, includeRelations)
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
     * Gets the AuditEvents for this AuditSession.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.auditSession.AuditEvents.then(auditSessions => { ... })
     *   or
     *   await this.auditSession.auditSessions
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
            AuditSessionService.Instance.GetAuditEventsForAuditSession(this.id)
        )
        .then(AuditEvents => {
            this._auditEvents = AuditEvents ?? [];
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
     * Clears the cached AuditEvent. Call after mutations to force refresh.
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
     * Updates the state of this AuditSessionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AuditSessionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AuditSessionSubmitData {
        return AuditSessionService.Instance.ConvertToAuditSessionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AuditSessionService extends SecureEndpointBase {

    private static _instance: AuditSessionService;
    private listCache: Map<string, Observable<Array<AuditSessionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AuditSessionBasicListData>>>;
    private recordCache: Map<string, Observable<AuditSessionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private auditEventService: AuditEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AuditSessionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AuditSessionBasicListData>>>();
        this.recordCache = new Map<string, Observable<AuditSessionData>>();

        AuditSessionService._instance = this;
    }

    public static get Instance(): AuditSessionService {
      return AuditSessionService._instance;
    }


    public ClearListCaches(config: AuditSessionQueryParameters | null = null) {

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


    public ConvertToAuditSessionSubmitData(data: AuditSessionData): AuditSessionSubmitData {

        let output = new AuditSessionSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.comments = data.comments;
        output.firstAccess = data.firstAccess;

        return output;
    }

    public GetAuditSession(id: bigint | number, includeRelations: boolean = true) : Observable<AuditSessionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const auditSession$ = this.requestAuditSession(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditSession", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, auditSession$);

            return auditSession$;
        }

        return this.recordCache.get(configHash) as Observable<AuditSessionData>;
    }

    private requestAuditSession(id: bigint | number, includeRelations: boolean = true) : Observable<AuditSessionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AuditSessionData>(this.baseUrl + 'api/AuditSession/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAuditSession(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditSession(id, includeRelations));
            }));
    }

    public GetAuditSessionList(config: AuditSessionQueryParameters | any = null) : Observable<Array<AuditSessionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const auditSessionList$ = this.requestAuditSessionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditSession list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, auditSessionList$);

            return auditSessionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AuditSessionData>>;
    }


    private requestAuditSessionList(config: AuditSessionQueryParameters | any) : Observable <Array<AuditSessionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditSessionData>>(this.baseUrl + 'api/AuditSessions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAuditSessionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditSessionList(config));
            }));
    }

    public GetAuditSessionsRowCount(config: AuditSessionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const auditSessionsRowCount$ = this.requestAuditSessionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditSessions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, auditSessionsRowCount$);

            return auditSessionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAuditSessionsRowCount(config: AuditSessionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AuditSessions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditSessionsRowCount(config));
            }));
    }

    public GetAuditSessionsBasicListData(config: AuditSessionQueryParameters | any = null) : Observable<Array<AuditSessionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const auditSessionsBasicListData$ = this.requestAuditSessionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditSessions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, auditSessionsBasicListData$);

            return auditSessionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AuditSessionBasicListData>>;
    }


    private requestAuditSessionsBasicListData(config: AuditSessionQueryParameters | any) : Observable<Array<AuditSessionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditSessionBasicListData>>(this.baseUrl + 'api/AuditSessions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditSessionsBasicListData(config));
            }));

    }


    public PutAuditSession(id: bigint | number, auditSession: AuditSessionSubmitData) : Observable<AuditSessionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AuditSessionData>(this.baseUrl + 'api/AuditSession/' + id.toString(), auditSession, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditSession(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAuditSession(id, auditSession));
            }));
    }


    public PostAuditSession(auditSession: AuditSessionSubmitData) : Observable<AuditSessionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AuditSessionData>(this.baseUrl + 'api/AuditSession', auditSession, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditSession(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAuditSession(auditSession));
            }));
    }

  
    public DeleteAuditSession(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AuditSession/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAuditSession(id));
            }));
    }


    private getConfigHash(config: AuditSessionQueryParameters | any): string {

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

    public userIsAuditorAuditSessionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAuditorAuditSessionReader = this.authService.isAuditorReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Auditor.AuditSessions
        //
        if (userIsAuditorAuditSessionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAuditorAuditSessionReader = user.readPermission >= 0;
            } else {
                userIsAuditorAuditSessionReader = false;
            }
        }

        return userIsAuditorAuditSessionReader;
    }


    public userIsAuditorAuditSessionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAuditorAuditSessionWriter = this.authService.isAuditorReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Auditor.AuditSessions
        //
        if (userIsAuditorAuditSessionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAuditorAuditSessionWriter = user.writePermission >= 0;
          } else {
            userIsAuditorAuditSessionWriter = false;
          }      
        }

        return userIsAuditorAuditSessionWriter;
    }

    public GetAuditEventsForAuditSession(auditSessionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AuditEventData[]> {
        return this.auditEventService.GetAuditEventList({
            auditSessionId: auditSessionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AuditSessionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AuditSessionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AuditSessionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAuditSession(raw: any): AuditSessionData {
    if (!raw) return raw;

    //
    // Create a AuditSessionData object instance with correct prototype
    //
    const revived = Object.create(AuditSessionData.prototype) as AuditSessionData;

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
    // 2. But private methods (loadAuditSessionXYZ, etc.) are not accessible via the typed variable
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

    (revived as any).AuditEventsCount$ = AuditEventService.Instance.GetAuditEventsRowCount({auditSessionId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveAuditSessionList(rawList: any[]): AuditSessionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAuditSession(raw));
  }

}
