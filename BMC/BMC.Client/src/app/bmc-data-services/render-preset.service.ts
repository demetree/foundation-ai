/*

   GENERATED SERVICE FOR THE RENDERPRESET TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the RenderPreset table.

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
import { ProjectRenderService, ProjectRenderData } from './project-render.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class RenderPresetQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    resolutionWidth: bigint | number | null | undefined = null;
    resolutionHeight: bigint | number | null | undefined = null;
    backgroundColorHex: string | null | undefined = null;
    enableShadows: boolean | null | undefined = null;
    enableReflections: boolean | null | undefined = null;
    lightingMode: string | null | undefined = null;
    antiAliasLevel: bigint | number | null | undefined = null;
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
export class RenderPresetSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    resolutionWidth: bigint | number | null = null;
    resolutionHeight: bigint | number | null = null;
    backgroundColorHex: string | null = null;
    enableShadows!: boolean;
    enableReflections!: boolean;
    lightingMode: string | null = null;
    antiAliasLevel: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class RenderPresetBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. RenderPresetChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `renderPreset.RenderPresetChildren$` — use with `| async` in templates
//        • Promise:    `renderPreset.RenderPresetChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="renderPreset.RenderPresetChildren$ | async"`), or
//        • Access the promise getter (`renderPreset.RenderPresetChildren` or `await renderPreset.RenderPresetChildren`)
//    - Simply reading `renderPreset.RenderPresetChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await renderPreset.Reload()` to refresh the entire object and clear all lazy caches.
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
export class RenderPresetData {
    id!: bigint | number;
    name!: string;
    description!: string;
    resolutionWidth!: bigint | number;
    resolutionHeight!: bigint | number;
    backgroundColorHex!: string | null;
    enableShadows!: boolean;
    enableReflections!: boolean;
    lightingMode!: string | null;
    antiAliasLevel!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _projectRenders: ProjectRenderData[] | null = null;
    private _projectRendersPromise: Promise<ProjectRenderData[]> | null  = null;
    private _projectRendersSubject = new BehaviorSubject<ProjectRenderData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ProjectRenders$ = this._projectRendersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._projectRenders === null && this._projectRendersPromise === null) {
            this.loadProjectRenders(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _projectRendersCount$: Observable<bigint | number> | null = null;
    public get ProjectRendersCount$(): Observable<bigint | number> {
        if (this._projectRendersCount$ === null) {
            this._projectRendersCount$ = ProjectRenderService.Instance.GetProjectRendersRowCount({renderPresetId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._projectRendersCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any RenderPresetData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.renderPreset.Reload();
  //
  //  Non Async:
  //
  //     renderPreset[0].Reload().then(x => {
  //        this.renderPreset = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      RenderPresetService.Instance.GetRenderPreset(this.id, includeRelations)
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
     this._projectRenders = null;
     this._projectRendersPromise = null;
     this._projectRendersSubject.next(null);
     this._projectRendersCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ProjectRenders for this RenderPreset.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.renderPreset.ProjectRenders.then(renderPresets => { ... })
     *   or
     *   await this.renderPreset.renderPresets
     *
    */
    public get ProjectRenders(): Promise<ProjectRenderData[]> {
        if (this._projectRenders !== null) {
            return Promise.resolve(this._projectRenders);
        }

        if (this._projectRendersPromise !== null) {
            return this._projectRendersPromise;
        }

        // Start the load
        this.loadProjectRenders();

        return this._projectRendersPromise!;
    }



    private loadProjectRenders(): void {

        this._projectRendersPromise = lastValueFrom(
            RenderPresetService.Instance.GetProjectRendersForRenderPreset(this.id)
        )
        .then(ProjectRenders => {
            this._projectRenders = ProjectRenders ?? [];
            this._projectRendersSubject.next(this._projectRenders);
            return this._projectRenders;
         })
        .catch(err => {
            this._projectRenders = [];
            this._projectRendersSubject.next(this._projectRenders);
            throw err;
        })
        .finally(() => {
            this._projectRendersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ProjectRender. Call after mutations to force refresh.
     */
    public ClearProjectRendersCache(): void {
        this._projectRenders = null;
        this._projectRendersPromise = null;
        this._projectRendersSubject.next(this._projectRenders);      // Emit to observable
    }

    public get HasProjectRenders(): Promise<boolean> {
        return this.ProjectRenders.then(projectRenders => projectRenders.length > 0);
    }




    /**
     * Updates the state of this RenderPresetData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this RenderPresetData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): RenderPresetSubmitData {
        return RenderPresetService.Instance.ConvertToRenderPresetSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class RenderPresetService extends SecureEndpointBase {

    private static _instance: RenderPresetService;
    private listCache: Map<string, Observable<Array<RenderPresetData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<RenderPresetBasicListData>>>;
    private recordCache: Map<string, Observable<RenderPresetData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private projectRenderService: ProjectRenderService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<RenderPresetData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<RenderPresetBasicListData>>>();
        this.recordCache = new Map<string, Observable<RenderPresetData>>();

        RenderPresetService._instance = this;
    }

    public static get Instance(): RenderPresetService {
      return RenderPresetService._instance;
    }


    public ClearListCaches(config: RenderPresetQueryParameters | null = null) {

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


    public ConvertToRenderPresetSubmitData(data: RenderPresetData): RenderPresetSubmitData {

        let output = new RenderPresetSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.resolutionWidth = data.resolutionWidth;
        output.resolutionHeight = data.resolutionHeight;
        output.backgroundColorHex = data.backgroundColorHex;
        output.enableShadows = data.enableShadows;
        output.enableReflections = data.enableReflections;
        output.lightingMode = data.lightingMode;
        output.antiAliasLevel = data.antiAliasLevel;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetRenderPreset(id: bigint | number, includeRelations: boolean = true) : Observable<RenderPresetData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const renderPreset$ = this.requestRenderPreset(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RenderPreset", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, renderPreset$);

            return renderPreset$;
        }

        return this.recordCache.get(configHash) as Observable<RenderPresetData>;
    }

    private requestRenderPreset(id: bigint | number, includeRelations: boolean = true) : Observable<RenderPresetData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RenderPresetData>(this.baseUrl + 'api/RenderPreset/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveRenderPreset(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestRenderPreset(id, includeRelations));
            }));
    }

    public GetRenderPresetList(config: RenderPresetQueryParameters | any = null) : Observable<Array<RenderPresetData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const renderPresetList$ = this.requestRenderPresetList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RenderPreset list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, renderPresetList$);

            return renderPresetList$;
        }

        return this.listCache.get(configHash) as Observable<Array<RenderPresetData>>;
    }


    private requestRenderPresetList(config: RenderPresetQueryParameters | any) : Observable <Array<RenderPresetData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RenderPresetData>>(this.baseUrl + 'api/RenderPresets', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveRenderPresetList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestRenderPresetList(config));
            }));
    }

    public GetRenderPresetsRowCount(config: RenderPresetQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const renderPresetsRowCount$ = this.requestRenderPresetsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RenderPresets row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, renderPresetsRowCount$);

            return renderPresetsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestRenderPresetsRowCount(config: RenderPresetQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/RenderPresets/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRenderPresetsRowCount(config));
            }));
    }

    public GetRenderPresetsBasicListData(config: RenderPresetQueryParameters | any = null) : Observable<Array<RenderPresetBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const renderPresetsBasicListData$ = this.requestRenderPresetsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RenderPresets basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, renderPresetsBasicListData$);

            return renderPresetsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<RenderPresetBasicListData>>;
    }


    private requestRenderPresetsBasicListData(config: RenderPresetQueryParameters | any) : Observable<Array<RenderPresetBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RenderPresetBasicListData>>(this.baseUrl + 'api/RenderPresets/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRenderPresetsBasicListData(config));
            }));

    }


    public PutRenderPreset(id: bigint | number, renderPreset: RenderPresetSubmitData) : Observable<RenderPresetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RenderPresetData>(this.baseUrl + 'api/RenderPreset/' + id.toString(), renderPreset, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRenderPreset(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutRenderPreset(id, renderPreset));
            }));
    }


    public PostRenderPreset(renderPreset: RenderPresetSubmitData) : Observable<RenderPresetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<RenderPresetData>(this.baseUrl + 'api/RenderPreset', renderPreset, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRenderPreset(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostRenderPreset(renderPreset));
            }));
    }

  
    public DeleteRenderPreset(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/RenderPreset/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteRenderPreset(id));
            }));
    }


    private getConfigHash(config: RenderPresetQueryParameters | any): string {

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

    public userIsBMCRenderPresetReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCRenderPresetReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.RenderPresets
        //
        if (userIsBMCRenderPresetReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCRenderPresetReader = user.readPermission >= 1;
            } else {
                userIsBMCRenderPresetReader = false;
            }
        }

        return userIsBMCRenderPresetReader;
    }


    public userIsBMCRenderPresetWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCRenderPresetWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.RenderPresets
        //
        if (userIsBMCRenderPresetWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCRenderPresetWriter = user.writePermission >= 1;
          } else {
            userIsBMCRenderPresetWriter = false;
          }      
        }

        return userIsBMCRenderPresetWriter;
    }

    public GetProjectRendersForRenderPreset(renderPresetId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ProjectRenderData[]> {
        return this.projectRenderService.GetProjectRenderList({
            renderPresetId: renderPresetId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full RenderPresetData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the RenderPresetData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when RenderPresetTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveRenderPreset(raw: any): RenderPresetData {
    if (!raw) return raw;

    //
    // Create a RenderPresetData object instance with correct prototype
    //
    const revived = Object.create(RenderPresetData.prototype) as RenderPresetData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._projectRenders = null;
    (revived as any)._projectRendersPromise = null;
    (revived as any)._projectRendersSubject = new BehaviorSubject<ProjectRenderData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadRenderPresetXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ProjectRenders$ = (revived as any)._projectRendersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._projectRenders === null && (revived as any)._projectRendersPromise === null) {
                (revived as any).loadProjectRenders();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._projectRendersCount$ = null;



    return revived;
  }

  private ReviveRenderPresetList(rawList: any[]): RenderPresetData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveRenderPreset(raw));
  }

}
