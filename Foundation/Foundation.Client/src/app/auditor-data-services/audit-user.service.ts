import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, BehaviorSubject, catchError, throwError, lastValueFrom, map  } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';
import { AuditEventService, AuditEventData } from './audit-event.service';
import { ExternalCommunicationService, ExternalCommunicationData } from './external-communication.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AuditUserQueryParameters {
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
export class AuditUserSubmitData {
    id!: bigint | number;
    name!: string;
    comments: string | null = null;
    firstAccess: string | null = null;     // ISO 8601
}


export class AuditUserBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AuditUserChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `auditUser.AuditUserChildren$` — use with `| async` in templates
//        • Promise:    `auditUser.AuditUserChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="auditUser.AuditUserChildren$ | async"`), or
//        • Access the promise getter (`auditUser.AuditUserChildren` or `await auditUser.AuditUserChildren`)
//    - Simply reading `auditUser.AuditUserChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await auditUser.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AuditUserData {
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

    private _externalCommunications: ExternalCommunicationData[] | null = null;
    private _externalCommunicationsPromise: Promise<ExternalCommunicationData[]> | null  = null;
    private _externalCommunicationsSubject = new BehaviorSubject<ExternalCommunicationData[] | null>(null);


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

  
    public AuditEventsCount$ = AuditEventService.Instance.GetAuditEventsRowCount({auditUserId: this.id,
      active: true,
      deleted: false
    });



    public ExternalCommunications$ = this._externalCommunicationsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._externalCommunications === null && this._externalCommunicationsPromise === null) {
            this.loadExternalCommunications(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ExternalCommunicationsCount$ = ExternalCommunicationService.Instance.GetExternalCommunicationsRowCount({auditUserId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AuditUserData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.auditUser.Reload();
  //
  //  Non Async:
  //
  //     auditUser[0].Reload().then(x => {
  //        this.auditUser = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AuditUserService.Instance.GetAuditUser(this.id, includeRelations)
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

     this._externalCommunications = null;
     this._externalCommunicationsPromise = null;
     this._externalCommunicationsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the AuditEvents for this AuditUser.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.auditUser.AuditEvents.then(auditEvents => { ... })
     *   or
     *   await this.auditUser.AuditEvents
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
            AuditUserService.Instance.GetAuditEventsForAuditUser(this.id)
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
     *
     * Gets the ExternalCommunications for this AuditUser.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.auditUser.ExternalCommunications.then(externalCommunications => { ... })
     *   or
     *   await this.auditUser.ExternalCommunications
     *
    */
    public get ExternalCommunications(): Promise<ExternalCommunicationData[]> {
        if (this._externalCommunications !== null) {
            return Promise.resolve(this._externalCommunications);
        }

        if (this._externalCommunicationsPromise !== null) {
            return this._externalCommunicationsPromise;
        }

        // Start the load
        this.loadExternalCommunications();

        return this._externalCommunicationsPromise!;
    }



    private loadExternalCommunications(): void {

        this._externalCommunicationsPromise = lastValueFrom(
            AuditUserService.Instance.GetExternalCommunicationsForAuditUser(this.id)
        )
        .then(externalCommunications => {
            this._externalCommunications = externalCommunications ?? [];
            this._externalCommunicationsSubject.next(this._externalCommunications);
            return this._externalCommunications;
         })
        .catch(err => {
            this._externalCommunications = [];
            this._externalCommunicationsSubject.next(this._externalCommunications);
            throw err;
        })
        .finally(() => {
            this._externalCommunicationsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearExternalCommunicationsCache(): void {
        this._externalCommunications = null;
        this._externalCommunicationsPromise = null;
        this._externalCommunicationsSubject.next(this._externalCommunications);      // Emit to observable
    }

    public get HasExternalCommunications(): Promise<boolean> {
        return this.ExternalCommunications.then(externalCommunications => externalCommunications.length > 0);
    }




    /**
     * Updates the state of this AuditUserData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AuditUserData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AuditUserSubmitData {
        return AuditUserService.Instance.ConvertToAuditUserSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AuditUserService extends SecureEndpointBase {

    private static _instance: AuditUserService;
    private listCache: Map<string, Observable<Array<AuditUserData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AuditUserBasicListData>>>;
    private recordCache: Map<string, Observable<AuditUserData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private auditEventService: AuditEventService,
        private externalCommunicationService: ExternalCommunicationService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AuditUserData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AuditUserBasicListData>>>();
        this.recordCache = new Map<string, Observable<AuditUserData>>();

        AuditUserService._instance = this;
    }

    public static get Instance(): AuditUserService {
      return AuditUserService._instance;
    }


    public ClearListCaches(config: AuditUserQueryParameters | null = null) {

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


    public ConvertToAuditUserSubmitData(data: AuditUserData): AuditUserSubmitData {

        let output = new AuditUserSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.comments = data.comments;
        output.firstAccess = data.firstAccess;

        return output;
    }

    public GetAuditUser(id: bigint | number, includeRelations: boolean = true) : Observable<AuditUserData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const auditUser$ = this.requestAuditUser(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditUser", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, auditUser$);

            return auditUser$;
        }

        return this.recordCache.get(configHash) as Observable<AuditUserData>;
    }

    private requestAuditUser(id: bigint | number, includeRelations: boolean = true) : Observable<AuditUserData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AuditUserData>(this.baseUrl + 'api/AuditUser/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAuditUser(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditUser(id, includeRelations));
            }));
    }

    public GetAuditUserList(config: AuditUserQueryParameters | any = null) : Observable<Array<AuditUserData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const auditUserList$ = this.requestAuditUserList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditUser list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, auditUserList$);

            return auditUserList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AuditUserData>>;
    }


    private requestAuditUserList(config: AuditUserQueryParameters | any) : Observable <Array<AuditUserData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditUserData>>(this.baseUrl + 'api/AuditUsers', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAuditUserList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditUserList(config));
            }));
    }

    public GetAuditUsersRowCount(config: AuditUserQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const auditUsersRowCount$ = this.requestAuditUsersRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditUsers row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, auditUsersRowCount$);

            return auditUsersRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAuditUsersRowCount(config: AuditUserQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AuditUsers/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditUsersRowCount(config));
            }));
    }

    public GetAuditUsersBasicListData(config: AuditUserQueryParameters | any = null) : Observable<Array<AuditUserBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const auditUsersBasicListData$ = this.requestAuditUsersBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditUsers basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, auditUsersBasicListData$);

            return auditUsersBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AuditUserBasicListData>>;
    }


    private requestAuditUsersBasicListData(config: AuditUserQueryParameters | any) : Observable<Array<AuditUserBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditUserBasicListData>>(this.baseUrl + 'api/AuditUsers/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditUsersBasicListData(config));
            }));

    }


    public PutAuditUser(id: bigint | number, auditUser: AuditUserSubmitData) : Observable<AuditUserData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AuditUserData>(this.baseUrl + 'api/AuditUser/' + id.toString(), auditUser, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditUser(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAuditUser(id, auditUser));
            }));
    }


    public PostAuditUser(auditUser: AuditUserSubmitData) : Observable<AuditUserData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AuditUserData>(this.baseUrl + 'api/AuditUser', auditUser, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditUser(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAuditUser(auditUser));
            }));
    }

  
    public DeleteAuditUser(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AuditUser/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAuditUser(id));
            }));
    }


    private getConfigHash(config: AuditUserQueryParameters | any): string {

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

    public userIsAuditorAuditUserReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAuditorAuditUserReader = this.authService.isAuditorReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Auditor.AuditUsers
        //
        if (userIsAuditorAuditUserReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAuditorAuditUserReader = user.readPermission >= 0;
            } else {
                userIsAuditorAuditUserReader = false;
            }
        }

        return userIsAuditorAuditUserReader;
    }


    public userIsAuditorAuditUserWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAuditorAuditUserWriter = this.authService.isAuditorReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Auditor.AuditUsers
        //
        if (userIsAuditorAuditUserWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAuditorAuditUserWriter = user.writePermission >= 0;
          } else {
            userIsAuditorAuditUserWriter = false;
          }      
        }

        return userIsAuditorAuditUserWriter;
    }

    public GetAuditEventsForAuditUser(auditUserId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AuditEventData[]> {
        return this.auditEventService.GetAuditEventList({
            auditUserId: auditUserId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetExternalCommunicationsForAuditUser(auditUserId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ExternalCommunicationData[]> {
        return this.externalCommunicationService.GetExternalCommunicationList({
            auditUserId: auditUserId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AuditUserData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AuditUserData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AuditUserTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAuditUser(raw: any): AuditUserData {
    if (!raw) return raw;

    //
    // Create a AuditUserData object instance with correct prototype
    //
    const revived = Object.create(AuditUserData.prototype) as AuditUserData;

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

    (revived as any)._externalCommunications = null;
    (revived as any)._externalCommunicationsPromise = null;
    (revived as any)._externalCommunicationsSubject = new BehaviorSubject<ExternalCommunicationData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAuditUserXYZ, etc.) are not accessible via the typed variable
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

    (revived as any).AuditEventsCount$ = AuditEventService.Instance.GetAuditEventsRowCount({auditUserId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ExternalCommunications$ = (revived as any)._externalCommunicationsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._externalCommunications === null && (revived as any)._externalCommunicationsPromise === null) {
                (revived as any).loadExternalCommunications();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ExternalCommunicationsCount$ = ExternalCommunicationService.Instance.GetExternalCommunicationsRowCount({auditUserId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveAuditUserList(rawList: any[]): AuditUserData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAuditUser(raw));
  }

}
