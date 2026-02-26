/*

   GENERATED SERVICE FOR THE AUDITMODULE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AuditModule table.

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
import { AuditModuleEntityService, AuditModuleEntityData } from './audit-module-entity.service';
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
export class AuditModuleQueryParameters {
    name: string | null | undefined = null;
    comments: string | null | undefined = null;
    firstAccess: string | null | undefined = null;        // ISO 8601 (full datetime)
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class AuditModuleSubmitData {
    id!: bigint | number;
    name!: string;
    comments: string | null = null;
    firstAccess: string | null = null;     // ISO 8601 (full datetime)
}


export class AuditModuleBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AuditModuleChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `auditModule.AuditModuleChildren$` — use with `| async` in templates
//        • Promise:    `auditModule.AuditModuleChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="auditModule.AuditModuleChildren$ | async"`), or
//        • Access the promise getter (`auditModule.AuditModuleChildren` or `await auditModule.AuditModuleChildren`)
//    - Simply reading `auditModule.AuditModuleChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await auditModule.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AuditModuleData {
    id!: bigint | number;
    name!: string;
    comments!: string | null;
    firstAccess!: string | null;   // ISO 8601 (full datetime)

    //
    // Private lazy-loading caches for related collections
    //
    private _auditModuleEntities: AuditModuleEntityData[] | null = null;
    private _auditModuleEntitiesPromise: Promise<AuditModuleEntityData[]> | null  = null;
    private _auditModuleEntitiesSubject = new BehaviorSubject<AuditModuleEntityData[] | null>(null);

                
    private _auditEvents: AuditEventData[] | null = null;
    private _auditEventsPromise: Promise<AuditEventData[]> | null  = null;
    private _auditEventsSubject = new BehaviorSubject<AuditEventData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public AuditModuleEntities$ = this._auditModuleEntitiesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._auditModuleEntities === null && this._auditModuleEntitiesPromise === null) {
            this.loadAuditModuleEntities(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _auditModuleEntitiesCount$: Observable<bigint | number> | null = null;
    public get AuditModuleEntitiesCount$(): Observable<bigint | number> {
        if (this._auditModuleEntitiesCount$ === null) {
            this._auditModuleEntitiesCount$ = AuditModuleEntityService.Instance.GetAuditModuleEntitiesRowCount({auditModuleId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._auditModuleEntitiesCount$;
    }



    public AuditEvents$ = this._auditEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._auditEvents === null && this._auditEventsPromise === null) {
            this.loadAuditEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _auditEventsCount$: Observable<bigint | number> | null = null;
    public get AuditEventsCount$(): Observable<bigint | number> {
        if (this._auditEventsCount$ === null) {
            this._auditEventsCount$ = AuditEventService.Instance.GetAuditEventsRowCount({auditModuleId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._auditEventsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AuditModuleData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.auditModule.Reload();
  //
  //  Non Async:
  //
  //     auditModule[0].Reload().then(x => {
  //        this.auditModule = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AuditModuleService.Instance.GetAuditModule(this.id, includeRelations)
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
     this._auditModuleEntities = null;
     this._auditModuleEntitiesPromise = null;
     this._auditModuleEntitiesSubject.next(null);
     this._auditModuleEntitiesCount$ = null;

     this._auditEvents = null;
     this._auditEventsPromise = null;
     this._auditEventsSubject.next(null);
     this._auditEventsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the AuditModuleEntities for this AuditModule.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.auditModule.AuditModuleEntities.then(auditModules => { ... })
     *   or
     *   await this.auditModule.auditModules
     *
    */
    public get AuditModuleEntities(): Promise<AuditModuleEntityData[]> {
        if (this._auditModuleEntities !== null) {
            return Promise.resolve(this._auditModuleEntities);
        }

        if (this._auditModuleEntitiesPromise !== null) {
            return this._auditModuleEntitiesPromise;
        }

        // Start the load
        this.loadAuditModuleEntities();

        return this._auditModuleEntitiesPromise!;
    }



    private loadAuditModuleEntities(): void {

        this._auditModuleEntitiesPromise = lastValueFrom(
            AuditModuleService.Instance.GetAuditModuleEntitiesForAuditModule(this.id)
        )
        .then(AuditModuleEntities => {
            this._auditModuleEntities = AuditModuleEntities ?? [];
            this._auditModuleEntitiesSubject.next(this._auditModuleEntities);
            return this._auditModuleEntities;
         })
        .catch(err => {
            this._auditModuleEntities = [];
            this._auditModuleEntitiesSubject.next(this._auditModuleEntities);
            throw err;
        })
        .finally(() => {
            this._auditModuleEntitiesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached AuditModuleEntity. Call after mutations to force refresh.
     */
    public ClearAuditModuleEntitiesCache(): void {
        this._auditModuleEntities = null;
        this._auditModuleEntitiesPromise = null;
        this._auditModuleEntitiesSubject.next(this._auditModuleEntities);      // Emit to observable
    }

    public get HasAuditModuleEntities(): Promise<boolean> {
        return this.AuditModuleEntities.then(auditModuleEntities => auditModuleEntities.length > 0);
    }


    /**
     *
     * Gets the AuditEvents for this AuditModule.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.auditModule.AuditEvents.then(auditModules => { ... })
     *   or
     *   await this.auditModule.auditModules
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
            AuditModuleService.Instance.GetAuditEventsForAuditModule(this.id)
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
     * Updates the state of this AuditModuleData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AuditModuleData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AuditModuleSubmitData {
        return AuditModuleService.Instance.ConvertToAuditModuleSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AuditModuleService extends SecureEndpointBase {

    private static _instance: AuditModuleService;
    private listCache: Map<string, Observable<Array<AuditModuleData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AuditModuleBasicListData>>>;
    private recordCache: Map<string, Observable<AuditModuleData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private auditModuleEntityService: AuditModuleEntityService,
        private auditEventService: AuditEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AuditModuleData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AuditModuleBasicListData>>>();
        this.recordCache = new Map<string, Observable<AuditModuleData>>();

        AuditModuleService._instance = this;
    }

    public static get Instance(): AuditModuleService {
      return AuditModuleService._instance;
    }


    public ClearListCaches(config: AuditModuleQueryParameters | null = null) {

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


    public ConvertToAuditModuleSubmitData(data: AuditModuleData): AuditModuleSubmitData {

        let output = new AuditModuleSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.comments = data.comments;
        output.firstAccess = data.firstAccess;

        return output;
    }

    public GetAuditModule(id: bigint | number, includeRelations: boolean = true) : Observable<AuditModuleData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const auditModule$ = this.requestAuditModule(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditModule", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, auditModule$);

            return auditModule$;
        }

        return this.recordCache.get(configHash) as Observable<AuditModuleData>;
    }

    private requestAuditModule(id: bigint | number, includeRelations: boolean = true) : Observable<AuditModuleData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AuditModuleData>(this.baseUrl + 'api/AuditModule/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAuditModule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditModule(id, includeRelations));
            }));
    }

    public GetAuditModuleList(config: AuditModuleQueryParameters | any = null) : Observable<Array<AuditModuleData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const auditModuleList$ = this.requestAuditModuleList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditModule list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, auditModuleList$);

            return auditModuleList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AuditModuleData>>;
    }


    private requestAuditModuleList(config: AuditModuleQueryParameters | any) : Observable <Array<AuditModuleData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditModuleData>>(this.baseUrl + 'api/AuditModules', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAuditModuleList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditModuleList(config));
            }));
    }

    public GetAuditModulesRowCount(config: AuditModuleQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const auditModulesRowCount$ = this.requestAuditModulesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditModules row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, auditModulesRowCount$);

            return auditModulesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAuditModulesRowCount(config: AuditModuleQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AuditModules/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditModulesRowCount(config));
            }));
    }

    public GetAuditModulesBasicListData(config: AuditModuleQueryParameters | any = null) : Observable<Array<AuditModuleBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const auditModulesBasicListData$ = this.requestAuditModulesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditModules basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, auditModulesBasicListData$);

            return auditModulesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AuditModuleBasicListData>>;
    }


    private requestAuditModulesBasicListData(config: AuditModuleQueryParameters | any) : Observable<Array<AuditModuleBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditModuleBasicListData>>(this.baseUrl + 'api/AuditModules/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditModulesBasicListData(config));
            }));

    }


    public PutAuditModule(id: bigint | number, auditModule: AuditModuleSubmitData) : Observable<AuditModuleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AuditModuleData>(this.baseUrl + 'api/AuditModule/' + id.toString(), auditModule, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditModule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAuditModule(id, auditModule));
            }));
    }


    public PostAuditModule(auditModule: AuditModuleSubmitData) : Observable<AuditModuleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AuditModuleData>(this.baseUrl + 'api/AuditModule', auditModule, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditModule(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAuditModule(auditModule));
            }));
    }

  
    public DeleteAuditModule(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AuditModule/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAuditModule(id));
            }));
    }


    private getConfigHash(config: AuditModuleQueryParameters | any): string {

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

    public userIsAuditorAuditModuleReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAuditorAuditModuleReader = this.authService.isAuditorReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Auditor.AuditModules
        //
        if (userIsAuditorAuditModuleReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAuditorAuditModuleReader = user.readPermission >= 0;
            } else {
                userIsAuditorAuditModuleReader = false;
            }
        }

        return userIsAuditorAuditModuleReader;
    }


    public userIsAuditorAuditModuleWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAuditorAuditModuleWriter = this.authService.isAuditorReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Auditor.AuditModules
        //
        if (userIsAuditorAuditModuleWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAuditorAuditModuleWriter = user.writePermission >= 0;
          } else {
            userIsAuditorAuditModuleWriter = false;
          }      
        }

        return userIsAuditorAuditModuleWriter;
    }

    public GetAuditModuleEntitiesForAuditModule(auditModuleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AuditModuleEntityData[]> {
        return this.auditModuleEntityService.GetAuditModuleEntityList({
            auditModuleId: auditModuleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetAuditEventsForAuditModule(auditModuleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AuditEventData[]> {
        return this.auditEventService.GetAuditEventList({
            auditModuleId: auditModuleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AuditModuleData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AuditModuleData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AuditModuleTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAuditModule(raw: any): AuditModuleData {
    if (!raw) return raw;

    //
    // Create a AuditModuleData object instance with correct prototype
    //
    const revived = Object.create(AuditModuleData.prototype) as AuditModuleData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._auditModuleEntities = null;
    (revived as any)._auditModuleEntitiesPromise = null;
    (revived as any)._auditModuleEntitiesSubject = new BehaviorSubject<AuditModuleEntityData[] | null>(null);

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
    // 2. But private methods (loadAuditModuleXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).AuditModuleEntities$ = (revived as any)._auditModuleEntitiesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._auditModuleEntities === null && (revived as any)._auditModuleEntitiesPromise === null) {
                (revived as any).loadAuditModuleEntities();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._auditModuleEntitiesCount$ = null;


    (revived as any).AuditEvents$ = (revived as any)._auditEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._auditEvents === null && (revived as any)._auditEventsPromise === null) {
                (revived as any).loadAuditEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._auditEventsCount$ = null;



    return revived;
  }

  private ReviveAuditModuleList(rawList: any[]): AuditModuleData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAuditModule(raw));
  }

}
