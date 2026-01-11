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
export class AuditResourceQueryParameters {
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
export class AuditResourceSubmitData {
    id!: bigint | number;
    name!: string;
    comments: string | null = null;
    firstAccess: string | null = null;     // ISO 8601
}


export class AuditResourceBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AuditResourceChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `auditResource.AuditResourceChildren$` — use with `| async` in templates
//        • Promise:    `auditResource.AuditResourceChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="auditResource.AuditResourceChildren$ | async"`), or
//        • Access the promise getter (`auditResource.AuditResourceChildren` or `await auditResource.AuditResourceChildren`)
//    - Simply reading `auditResource.AuditResourceChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await auditResource.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AuditResourceData {
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

  
    public AuditEventsCount$ = AuditEventService.Instance.GetAuditEventsRowCount({auditResourceId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AuditResourceData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.auditResource.Reload();
  //
  //  Non Async:
  //
  //     auditResource[0].Reload().then(x => {
  //        this.auditResource = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AuditResourceService.Instance.GetAuditResource(this.id, includeRelations)
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
     * Gets the AuditEvents for this AuditResource.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.auditResource.AuditEvents.then(auditEvents => { ... })
     *   or
     *   await this.auditResource.AuditEvents
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
            AuditResourceService.Instance.GetAuditEventsForAuditResource(this.id)
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
     * Updates the state of this AuditResourceData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AuditResourceData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AuditResourceSubmitData {
        return AuditResourceService.Instance.ConvertToAuditResourceSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AuditResourceService extends SecureEndpointBase {

    private static _instance: AuditResourceService;
    private listCache: Map<string, Observable<Array<AuditResourceData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AuditResourceBasicListData>>>;
    private recordCache: Map<string, Observable<AuditResourceData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private auditEventService: AuditEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AuditResourceData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AuditResourceBasicListData>>>();
        this.recordCache = new Map<string, Observable<AuditResourceData>>();

        AuditResourceService._instance = this;
    }

    public static get Instance(): AuditResourceService {
      return AuditResourceService._instance;
    }


    public ClearListCaches(config: AuditResourceQueryParameters | null = null) {

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


    public ConvertToAuditResourceSubmitData(data: AuditResourceData): AuditResourceSubmitData {

        let output = new AuditResourceSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.comments = data.comments;
        output.firstAccess = data.firstAccess;

        return output;
    }

    public GetAuditResource(id: bigint | number, includeRelations: boolean = true) : Observable<AuditResourceData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const auditResource$ = this.requestAuditResource(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditResource", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, auditResource$);

            return auditResource$;
        }

        return this.recordCache.get(configHash) as Observable<AuditResourceData>;
    }

    private requestAuditResource(id: bigint | number, includeRelations: boolean = true) : Observable<AuditResourceData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AuditResourceData>(this.baseUrl + 'api/AuditResource/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAuditResource(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditResource(id, includeRelations));
            }));
    }

    public GetAuditResourceList(config: AuditResourceQueryParameters | any = null) : Observable<Array<AuditResourceData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const auditResourceList$ = this.requestAuditResourceList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditResource list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, auditResourceList$);

            return auditResourceList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AuditResourceData>>;
    }


    private requestAuditResourceList(config: AuditResourceQueryParameters | any) : Observable <Array<AuditResourceData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditResourceData>>(this.baseUrl + 'api/AuditResources', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAuditResourceList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditResourceList(config));
            }));
    }

    public GetAuditResourcesRowCount(config: AuditResourceQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const auditResourcesRowCount$ = this.requestAuditResourcesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditResources row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, auditResourcesRowCount$);

            return auditResourcesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAuditResourcesRowCount(config: AuditResourceQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AuditResources/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditResourcesRowCount(config));
            }));
    }

    public GetAuditResourcesBasicListData(config: AuditResourceQueryParameters | any = null) : Observable<Array<AuditResourceBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const auditResourcesBasicListData$ = this.requestAuditResourcesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditResources basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, auditResourcesBasicListData$);

            return auditResourcesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AuditResourceBasicListData>>;
    }


    private requestAuditResourcesBasicListData(config: AuditResourceQueryParameters | any) : Observable<Array<AuditResourceBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditResourceBasicListData>>(this.baseUrl + 'api/AuditResources/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditResourcesBasicListData(config));
            }));

    }


    public PutAuditResource(id: bigint | number, auditResource: AuditResourceSubmitData) : Observable<AuditResourceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AuditResourceData>(this.baseUrl + 'api/AuditResource/' + id.toString(), auditResource, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditResource(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAuditResource(id, auditResource));
            }));
    }


    public PostAuditResource(auditResource: AuditResourceSubmitData) : Observable<AuditResourceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AuditResourceData>(this.baseUrl + 'api/AuditResource', auditResource, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditResource(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAuditResource(auditResource));
            }));
    }

  
    public DeleteAuditResource(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AuditResource/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAuditResource(id));
            }));
    }


    private getConfigHash(config: AuditResourceQueryParameters | any): string {

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

    public userIsAuditorAuditResourceReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAuditorAuditResourceReader = this.authService.isAuditorReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Auditor.AuditResources
        //
        if (userIsAuditorAuditResourceReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAuditorAuditResourceReader = user.readPermission >= 0;
            } else {
                userIsAuditorAuditResourceReader = false;
            }
        }

        return userIsAuditorAuditResourceReader;
    }


    public userIsAuditorAuditResourceWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAuditorAuditResourceWriter = this.authService.isAuditorReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Auditor.AuditResources
        //
        if (userIsAuditorAuditResourceWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAuditorAuditResourceWriter = user.writePermission >= 0;
          } else {
            userIsAuditorAuditResourceWriter = false;
          }      
        }

        return userIsAuditorAuditResourceWriter;
    }

    public GetAuditEventsForAuditResource(auditResourceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AuditEventData[]> {
        return this.auditEventService.GetAuditEventList({
            auditResourceId: auditResourceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AuditResourceData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AuditResourceData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AuditResourceTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAuditResource(raw: any): AuditResourceData {
    if (!raw) return raw;

    //
    // Create a AuditResourceData object instance with correct prototype
    //
    const revived = Object.create(AuditResourceData.prototype) as AuditResourceData;

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
    // 2. But private methods (loadAuditResourceXYZ, etc.) are not accessible via the typed variable
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

    (revived as any).AuditEventsCount$ = AuditEventService.Instance.GetAuditEventsRowCount({auditResourceId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveAuditResourceList(rawList: any[]): AuditResourceData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAuditResource(raw));
  }

}
