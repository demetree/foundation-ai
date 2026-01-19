/*

   GENERATED SERVICE FOR THE MODULE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Module table.

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
import { ModuleSecurityRoleService, ModuleSecurityRoleData } from './module-security-role.service';
import { EntityDataTokenService, EntityDataTokenData } from './entity-data-token.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ModuleQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class ModuleSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ModuleBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ModuleChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `module.ModuleChildren$` — use with `| async` in templates
//        • Promise:    `module.ModuleChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="module.ModuleChildren$ | async"`), or
//        • Access the promise getter (`module.ModuleChildren` or `await module.ModuleChildren`)
//    - Simply reading `module.ModuleChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await module.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ModuleData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _moduleSecurityRoles: ModuleSecurityRoleData[] | null = null;
    private _moduleSecurityRolesPromise: Promise<ModuleSecurityRoleData[]> | null  = null;
    private _moduleSecurityRolesSubject = new BehaviorSubject<ModuleSecurityRoleData[] | null>(null);

                
    private _entityDataTokens: EntityDataTokenData[] | null = null;
    private _entityDataTokensPromise: Promise<EntityDataTokenData[]> | null  = null;
    private _entityDataTokensSubject = new BehaviorSubject<EntityDataTokenData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ModuleSecurityRoles$ = this._moduleSecurityRolesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._moduleSecurityRoles === null && this._moduleSecurityRolesPromise === null) {
            this.loadModuleSecurityRoles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ModuleSecurityRolesCount$ = ModuleSecurityRoleService.Instance.GetModuleSecurityRolesRowCount({moduleId: this.id,
      active: true,
      deleted: false
    });



    public EntityDataTokens$ = this._entityDataTokensSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._entityDataTokens === null && this._entityDataTokensPromise === null) {
            this.loadEntityDataTokens(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public EntityDataTokensCount$ = EntityDataTokenService.Instance.GetEntityDataTokensRowCount({moduleId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ModuleData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.module.Reload();
  //
  //  Non Async:
  //
  //     module[0].Reload().then(x => {
  //        this.module = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ModuleService.Instance.GetModule(this.id, includeRelations)
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
     this._moduleSecurityRoles = null;
     this._moduleSecurityRolesPromise = null;
     this._moduleSecurityRolesSubject.next(null);

     this._entityDataTokens = null;
     this._entityDataTokensPromise = null;
     this._entityDataTokensSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ModuleSecurityRoles for this Module.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.module.ModuleSecurityRoles.then(modules => { ... })
     *   or
     *   await this.module.modules
     *
    */
    public get ModuleSecurityRoles(): Promise<ModuleSecurityRoleData[]> {
        if (this._moduleSecurityRoles !== null) {
            return Promise.resolve(this._moduleSecurityRoles);
        }

        if (this._moduleSecurityRolesPromise !== null) {
            return this._moduleSecurityRolesPromise;
        }

        // Start the load
        this.loadModuleSecurityRoles();

        return this._moduleSecurityRolesPromise!;
    }



    private loadModuleSecurityRoles(): void {

        this._moduleSecurityRolesPromise = lastValueFrom(
            ModuleService.Instance.GetModuleSecurityRolesForModule(this.id)
        )
        .then(ModuleSecurityRoles => {
            this._moduleSecurityRoles = ModuleSecurityRoles ?? [];
            this._moduleSecurityRolesSubject.next(this._moduleSecurityRoles);
            return this._moduleSecurityRoles;
         })
        .catch(err => {
            this._moduleSecurityRoles = [];
            this._moduleSecurityRolesSubject.next(this._moduleSecurityRoles);
            throw err;
        })
        .finally(() => {
            this._moduleSecurityRolesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ModuleSecurityRole. Call after mutations to force refresh.
     */
    public ClearModuleSecurityRolesCache(): void {
        this._moduleSecurityRoles = null;
        this._moduleSecurityRolesPromise = null;
        this._moduleSecurityRolesSubject.next(this._moduleSecurityRoles);      // Emit to observable
    }

    public get HasModuleSecurityRoles(): Promise<boolean> {
        return this.ModuleSecurityRoles.then(moduleSecurityRoles => moduleSecurityRoles.length > 0);
    }


    /**
     *
     * Gets the EntityDataTokens for this Module.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.module.EntityDataTokens.then(modules => { ... })
     *   or
     *   await this.module.modules
     *
    */
    public get EntityDataTokens(): Promise<EntityDataTokenData[]> {
        if (this._entityDataTokens !== null) {
            return Promise.resolve(this._entityDataTokens);
        }

        if (this._entityDataTokensPromise !== null) {
            return this._entityDataTokensPromise;
        }

        // Start the load
        this.loadEntityDataTokens();

        return this._entityDataTokensPromise!;
    }



    private loadEntityDataTokens(): void {

        this._entityDataTokensPromise = lastValueFrom(
            ModuleService.Instance.GetEntityDataTokensForModule(this.id)
        )
        .then(EntityDataTokens => {
            this._entityDataTokens = EntityDataTokens ?? [];
            this._entityDataTokensSubject.next(this._entityDataTokens);
            return this._entityDataTokens;
         })
        .catch(err => {
            this._entityDataTokens = [];
            this._entityDataTokensSubject.next(this._entityDataTokens);
            throw err;
        })
        .finally(() => {
            this._entityDataTokensPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EntityDataToken. Call after mutations to force refresh.
     */
    public ClearEntityDataTokensCache(): void {
        this._entityDataTokens = null;
        this._entityDataTokensPromise = null;
        this._entityDataTokensSubject.next(this._entityDataTokens);      // Emit to observable
    }

    public get HasEntityDataTokens(): Promise<boolean> {
        return this.EntityDataTokens.then(entityDataTokens => entityDataTokens.length > 0);
    }




    /**
     * Updates the state of this ModuleData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ModuleData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ModuleSubmitData {
        return ModuleService.Instance.ConvertToModuleSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ModuleService extends SecureEndpointBase {

    private static _instance: ModuleService;
    private listCache: Map<string, Observable<Array<ModuleData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ModuleBasicListData>>>;
    private recordCache: Map<string, Observable<ModuleData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private moduleSecurityRoleService: ModuleSecurityRoleService,
        private entityDataTokenService: EntityDataTokenService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ModuleData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ModuleBasicListData>>>();
        this.recordCache = new Map<string, Observable<ModuleData>>();

        ModuleService._instance = this;
    }

    public static get Instance(): ModuleService {
      return ModuleService._instance;
    }


    public ClearListCaches(config: ModuleQueryParameters | null = null) {

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


    public ConvertToModuleSubmitData(data: ModuleData): ModuleSubmitData {

        let output = new ModuleSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetModule(id: bigint | number, includeRelations: boolean = true) : Observable<ModuleData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const module$ = this.requestModule(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Module", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, module$);

            return module$;
        }

        return this.recordCache.get(configHash) as Observable<ModuleData>;
    }

    private requestModule(id: bigint | number, includeRelations: boolean = true) : Observable<ModuleData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ModuleData>(this.baseUrl + 'api/Module/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveModule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestModule(id, includeRelations));
            }));
    }

    public GetModuleList(config: ModuleQueryParameters | any = null) : Observable<Array<ModuleData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const moduleList$ = this.requestModuleList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Module list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, moduleList$);

            return moduleList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ModuleData>>;
    }


    private requestModuleList(config: ModuleQueryParameters | any) : Observable <Array<ModuleData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ModuleData>>(this.baseUrl + 'api/Modules', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveModuleList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestModuleList(config));
            }));
    }

    public GetModulesRowCount(config: ModuleQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const modulesRowCount$ = this.requestModulesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Modules row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, modulesRowCount$);

            return modulesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestModulesRowCount(config: ModuleQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Modules/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestModulesRowCount(config));
            }));
    }

    public GetModulesBasicListData(config: ModuleQueryParameters | any = null) : Observable<Array<ModuleBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const modulesBasicListData$ = this.requestModulesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Modules basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, modulesBasicListData$);

            return modulesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ModuleBasicListData>>;
    }


    private requestModulesBasicListData(config: ModuleQueryParameters | any) : Observable<Array<ModuleBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ModuleBasicListData>>(this.baseUrl + 'api/Modules/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestModulesBasicListData(config));
            }));

    }


    public PutModule(id: bigint | number, module: ModuleSubmitData) : Observable<ModuleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ModuleData>(this.baseUrl + 'api/Module/' + id.toString(), module, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveModule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutModule(id, module));
            }));
    }


    public PostModule(module: ModuleSubmitData) : Observable<ModuleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ModuleData>(this.baseUrl + 'api/Module', module, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveModule(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostModule(module));
            }));
    }

  
    public DeleteModule(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Module/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteModule(id));
            }));
    }


    private getConfigHash(config: ModuleQueryParameters | any): string {

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

    public userIsSecurityModuleReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecurityModuleReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.Modules
        //
        if (userIsSecurityModuleReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecurityModuleReader = user.readPermission >= 0;
            } else {
                userIsSecurityModuleReader = false;
            }
        }

        return userIsSecurityModuleReader;
    }


    public userIsSecurityModuleWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecurityModuleWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.Modules
        //
        if (userIsSecurityModuleWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecurityModuleWriter = user.writePermission >= 0;
          } else {
            userIsSecurityModuleWriter = false;
          }      
        }

        return userIsSecurityModuleWriter;
    }

    public GetModuleSecurityRolesForModule(moduleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ModuleSecurityRoleData[]> {
        return this.moduleSecurityRoleService.GetModuleSecurityRoleList({
            moduleId: moduleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEntityDataTokensForModule(moduleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EntityDataTokenData[]> {
        return this.entityDataTokenService.GetEntityDataTokenList({
            moduleId: moduleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ModuleData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ModuleData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ModuleTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveModule(raw: any): ModuleData {
    if (!raw) return raw;

    //
    // Create a ModuleData object instance with correct prototype
    //
    const revived = Object.create(ModuleData.prototype) as ModuleData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._moduleSecurityRoles = null;
    (revived as any)._moduleSecurityRolesPromise = null;
    (revived as any)._moduleSecurityRolesSubject = new BehaviorSubject<ModuleSecurityRoleData[] | null>(null);

    (revived as any)._entityDataTokens = null;
    (revived as any)._entityDataTokensPromise = null;
    (revived as any)._entityDataTokensSubject = new BehaviorSubject<EntityDataTokenData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadModuleXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ModuleSecurityRoles$ = (revived as any)._moduleSecurityRolesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._moduleSecurityRoles === null && (revived as any)._moduleSecurityRolesPromise === null) {
                (revived as any).loadModuleSecurityRoles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ModuleSecurityRolesCount$ = ModuleSecurityRoleService.Instance.GetModuleSecurityRolesRowCount({moduleId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).EntityDataTokens$ = (revived as any)._entityDataTokensSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._entityDataTokens === null && (revived as any)._entityDataTokensPromise === null) {
                (revived as any).loadEntityDataTokens();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).EntityDataTokensCount$ = EntityDataTokenService.Instance.GetEntityDataTokensRowCount({moduleId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveModuleList(rawList: any[]): ModuleData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveModule(raw));
  }

}
