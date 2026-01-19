/*

   GENERATED SERVICE FOR THE ENTITYDATATOKEN TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the EntityDataToken table.

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
import { SecurityUserData } from './security-user.service';
import { ModuleData } from './module.service';
import { EntityDataTokenEventService, EntityDataTokenEventData } from './entity-data-token-event.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class EntityDataTokenQueryParameters {
    securityUserId: bigint | number | null | undefined = null;
    moduleId: bigint | number | null | undefined = null;
    entity: string | null | undefined = null;
    sessionId: string | null | undefined = null;
    authenticationToken: string | null | undefined = null;
    token: string | null | undefined = null;
    timeStamp: string | null | undefined = null;        // ISO 8601
    comments: string | null | undefined = null;
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
export class EntityDataTokenSubmitData {
    id!: bigint | number;
    securityUserId!: bigint | number;
    moduleId!: bigint | number;
    entity!: string;
    sessionId!: string;
    authenticationToken!: string;
    token!: string;
    timeStamp!: string;      // ISO 8601
    comments: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class EntityDataTokenBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. EntityDataTokenChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `entityDataToken.EntityDataTokenChildren$` — use with `| async` in templates
//        • Promise:    `entityDataToken.EntityDataTokenChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="entityDataToken.EntityDataTokenChildren$ | async"`), or
//        • Access the promise getter (`entityDataToken.EntityDataTokenChildren` or `await entityDataToken.EntityDataTokenChildren`)
//    - Simply reading `entityDataToken.EntityDataTokenChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await entityDataToken.Reload()` to refresh the entire object and clear all lazy caches.
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
export class EntityDataTokenData {
    id!: bigint | number;
    securityUserId!: bigint | number;
    moduleId!: bigint | number;
    entity!: string;
    sessionId!: string;
    authenticationToken!: string;
    token!: string;
    timeStamp!: string;      // ISO 8601
    comments!: string | null;
    active!: boolean;
    deleted!: boolean;
    module: ModuleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    securityUser: SecurityUserData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _entityDataTokenEvents: EntityDataTokenEventData[] | null = null;
    private _entityDataTokenEventsPromise: Promise<EntityDataTokenEventData[]> | null  = null;
    private _entityDataTokenEventsSubject = new BehaviorSubject<EntityDataTokenEventData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public EntityDataTokenEvents$ = this._entityDataTokenEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._entityDataTokenEvents === null && this._entityDataTokenEventsPromise === null) {
            this.loadEntityDataTokenEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public EntityDataTokenEventsCount$ = EntityDataTokenEventService.Instance.GetEntityDataTokenEventsRowCount({entityDataTokenId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any EntityDataTokenData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.entityDataToken.Reload();
  //
  //  Non Async:
  //
  //     entityDataToken[0].Reload().then(x => {
  //        this.entityDataToken = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      EntityDataTokenService.Instance.GetEntityDataToken(this.id, includeRelations)
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
     this._entityDataTokenEvents = null;
     this._entityDataTokenEventsPromise = null;
     this._entityDataTokenEventsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the EntityDataTokenEvents for this EntityDataToken.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.entityDataToken.EntityDataTokenEvents.then(entityDataTokens => { ... })
     *   or
     *   await this.entityDataToken.entityDataTokens
     *
    */
    public get EntityDataTokenEvents(): Promise<EntityDataTokenEventData[]> {
        if (this._entityDataTokenEvents !== null) {
            return Promise.resolve(this._entityDataTokenEvents);
        }

        if (this._entityDataTokenEventsPromise !== null) {
            return this._entityDataTokenEventsPromise;
        }

        // Start the load
        this.loadEntityDataTokenEvents();

        return this._entityDataTokenEventsPromise!;
    }



    private loadEntityDataTokenEvents(): void {

        this._entityDataTokenEventsPromise = lastValueFrom(
            EntityDataTokenService.Instance.GetEntityDataTokenEventsForEntityDataToken(this.id)
        )
        .then(EntityDataTokenEvents => {
            this._entityDataTokenEvents = EntityDataTokenEvents ?? [];
            this._entityDataTokenEventsSubject.next(this._entityDataTokenEvents);
            return this._entityDataTokenEvents;
         })
        .catch(err => {
            this._entityDataTokenEvents = [];
            this._entityDataTokenEventsSubject.next(this._entityDataTokenEvents);
            throw err;
        })
        .finally(() => {
            this._entityDataTokenEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EntityDataTokenEvent. Call after mutations to force refresh.
     */
    public ClearEntityDataTokenEventsCache(): void {
        this._entityDataTokenEvents = null;
        this._entityDataTokenEventsPromise = null;
        this._entityDataTokenEventsSubject.next(this._entityDataTokenEvents);      // Emit to observable
    }

    public get HasEntityDataTokenEvents(): Promise<boolean> {
        return this.EntityDataTokenEvents.then(entityDataTokenEvents => entityDataTokenEvents.length > 0);
    }




    /**
     * Updates the state of this EntityDataTokenData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this EntityDataTokenData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): EntityDataTokenSubmitData {
        return EntityDataTokenService.Instance.ConvertToEntityDataTokenSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class EntityDataTokenService extends SecureEndpointBase {

    private static _instance: EntityDataTokenService;
    private listCache: Map<string, Observable<Array<EntityDataTokenData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<EntityDataTokenBasicListData>>>;
    private recordCache: Map<string, Observable<EntityDataTokenData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private entityDataTokenEventService: EntityDataTokenEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<EntityDataTokenData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<EntityDataTokenBasicListData>>>();
        this.recordCache = new Map<string, Observable<EntityDataTokenData>>();

        EntityDataTokenService._instance = this;
    }

    public static get Instance(): EntityDataTokenService {
      return EntityDataTokenService._instance;
    }


    public ClearListCaches(config: EntityDataTokenQueryParameters | null = null) {

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


    public ConvertToEntityDataTokenSubmitData(data: EntityDataTokenData): EntityDataTokenSubmitData {

        let output = new EntityDataTokenSubmitData();

        output.id = data.id;
        output.securityUserId = data.securityUserId;
        output.moduleId = data.moduleId;
        output.entity = data.entity;
        output.sessionId = data.sessionId;
        output.authenticationToken = data.authenticationToken;
        output.token = data.token;
        output.timeStamp = data.timeStamp;
        output.comments = data.comments;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetEntityDataToken(id: bigint | number, includeRelations: boolean = true) : Observable<EntityDataTokenData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const entityDataToken$ = this.requestEntityDataToken(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EntityDataToken", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, entityDataToken$);

            return entityDataToken$;
        }

        return this.recordCache.get(configHash) as Observable<EntityDataTokenData>;
    }

    private requestEntityDataToken(id: bigint | number, includeRelations: boolean = true) : Observable<EntityDataTokenData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EntityDataTokenData>(this.baseUrl + 'api/EntityDataToken/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveEntityDataToken(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestEntityDataToken(id, includeRelations));
            }));
    }

    public GetEntityDataTokenList(config: EntityDataTokenQueryParameters | any = null) : Observable<Array<EntityDataTokenData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const entityDataTokenList$ = this.requestEntityDataTokenList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EntityDataToken list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, entityDataTokenList$);

            return entityDataTokenList$;
        }

        return this.listCache.get(configHash) as Observable<Array<EntityDataTokenData>>;
    }


    private requestEntityDataTokenList(config: EntityDataTokenQueryParameters | any) : Observable <Array<EntityDataTokenData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EntityDataTokenData>>(this.baseUrl + 'api/EntityDataTokens', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveEntityDataTokenList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestEntityDataTokenList(config));
            }));
    }

    public GetEntityDataTokensRowCount(config: EntityDataTokenQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const entityDataTokensRowCount$ = this.requestEntityDataTokensRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EntityDataTokens row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, entityDataTokensRowCount$);

            return entityDataTokensRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestEntityDataTokensRowCount(config: EntityDataTokenQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/EntityDataTokens/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEntityDataTokensRowCount(config));
            }));
    }

    public GetEntityDataTokensBasicListData(config: EntityDataTokenQueryParameters | any = null) : Observable<Array<EntityDataTokenBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const entityDataTokensBasicListData$ = this.requestEntityDataTokensBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EntityDataTokens basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, entityDataTokensBasicListData$);

            return entityDataTokensBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<EntityDataTokenBasicListData>>;
    }


    private requestEntityDataTokensBasicListData(config: EntityDataTokenQueryParameters | any) : Observable<Array<EntityDataTokenBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EntityDataTokenBasicListData>>(this.baseUrl + 'api/EntityDataTokens/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEntityDataTokensBasicListData(config));
            }));

    }


    public PutEntityDataToken(id: bigint | number, entityDataToken: EntityDataTokenSubmitData) : Observable<EntityDataTokenData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EntityDataTokenData>(this.baseUrl + 'api/EntityDataToken/' + id.toString(), entityDataToken, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEntityDataToken(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutEntityDataToken(id, entityDataToken));
            }));
    }


    public PostEntityDataToken(entityDataToken: EntityDataTokenSubmitData) : Observable<EntityDataTokenData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<EntityDataTokenData>(this.baseUrl + 'api/EntityDataToken', entityDataToken, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEntityDataToken(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostEntityDataToken(entityDataToken));
            }));
    }

  
    public DeleteEntityDataToken(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/EntityDataToken/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteEntityDataToken(id));
            }));
    }


    private getConfigHash(config: EntityDataTokenQueryParameters | any): string {

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

    public userIsSecurityEntityDataTokenReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecurityEntityDataTokenReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.EntityDataTokens
        //
        if (userIsSecurityEntityDataTokenReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecurityEntityDataTokenReader = user.readPermission >= 0;
            } else {
                userIsSecurityEntityDataTokenReader = false;
            }
        }

        return userIsSecurityEntityDataTokenReader;
    }


    public userIsSecurityEntityDataTokenWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecurityEntityDataTokenWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.EntityDataTokens
        //
        if (userIsSecurityEntityDataTokenWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecurityEntityDataTokenWriter = user.writePermission >= 100;
          } else {
            userIsSecurityEntityDataTokenWriter = false;
          }      
        }

        return userIsSecurityEntityDataTokenWriter;
    }

    public GetEntityDataTokenEventsForEntityDataToken(entityDataTokenId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EntityDataTokenEventData[]> {
        return this.entityDataTokenEventService.GetEntityDataTokenEventList({
            entityDataTokenId: entityDataTokenId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full EntityDataTokenData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the EntityDataTokenData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when EntityDataTokenTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveEntityDataToken(raw: any): EntityDataTokenData {
    if (!raw) return raw;

    //
    // Create a EntityDataTokenData object instance with correct prototype
    //
    const revived = Object.create(EntityDataTokenData.prototype) as EntityDataTokenData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._entityDataTokenEvents = null;
    (revived as any)._entityDataTokenEventsPromise = null;
    (revived as any)._entityDataTokenEventsSubject = new BehaviorSubject<EntityDataTokenEventData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadEntityDataTokenXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).EntityDataTokenEvents$ = (revived as any)._entityDataTokenEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._entityDataTokenEvents === null && (revived as any)._entityDataTokenEventsPromise === null) {
                (revived as any).loadEntityDataTokenEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).EntityDataTokenEventsCount$ = EntityDataTokenEventService.Instance.GetEntityDataTokenEventsRowCount({entityDataTokenId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveEntityDataTokenList(rawList: any[]): EntityDataTokenData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveEntityDataToken(raw));
  }

}
