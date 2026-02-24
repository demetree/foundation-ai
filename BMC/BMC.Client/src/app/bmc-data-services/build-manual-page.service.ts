/*

   GENERATED SERVICE FOR THE BUILDMANUALPAGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BuildManualPage table.

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
import { BuildManualData } from './build-manual.service';
import { BuildManualStepService, BuildManualStepData } from './build-manual-step.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BuildManualPageQueryParameters {
    buildManualId: bigint | number | null | undefined = null;
    pageNum: bigint | number | null | undefined = null;
    title: string | null | undefined = null;
    notes: string | null | undefined = null;
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
export class BuildManualPageSubmitData {
    id!: bigint | number;
    buildManualId!: bigint | number;
    pageNum: bigint | number | null = null;
    title: string | null = null;
    notes: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BuildManualPageBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BuildManualPageChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `buildManualPage.BuildManualPageChildren$` — use with `| async` in templates
//        • Promise:    `buildManualPage.BuildManualPageChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="buildManualPage.BuildManualPageChildren$ | async"`), or
//        • Access the promise getter (`buildManualPage.BuildManualPageChildren` or `await buildManualPage.BuildManualPageChildren`)
//    - Simply reading `buildManualPage.BuildManualPageChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await buildManualPage.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BuildManualPageData {
    id!: bigint | number;
    buildManualId!: bigint | number;
    pageNum!: bigint | number;
    title!: string | null;
    notes!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    buildManual: BuildManualData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _buildManualSteps: BuildManualStepData[] | null = null;
    private _buildManualStepsPromise: Promise<BuildManualStepData[]> | null  = null;
    private _buildManualStepsSubject = new BehaviorSubject<BuildManualStepData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public BuildManualSteps$ = this._buildManualStepsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._buildManualSteps === null && this._buildManualStepsPromise === null) {
            this.loadBuildManualSteps(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _buildManualStepsCount$: Observable<bigint | number> | null = null;
    public get BuildManualStepsCount$(): Observable<bigint | number> {
        if (this._buildManualStepsCount$ === null) {
            this._buildManualStepsCount$ = BuildManualStepService.Instance.GetBuildManualStepsRowCount({buildManualPageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._buildManualStepsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any BuildManualPageData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.buildManualPage.Reload();
  //
  //  Non Async:
  //
  //     buildManualPage[0].Reload().then(x => {
  //        this.buildManualPage = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BuildManualPageService.Instance.GetBuildManualPage(this.id, includeRelations)
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
     this._buildManualSteps = null;
     this._buildManualStepsPromise = null;
     this._buildManualStepsSubject.next(null);
     this._buildManualStepsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the BuildManualSteps for this BuildManualPage.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.buildManualPage.BuildManualSteps.then(buildManualPages => { ... })
     *   or
     *   await this.buildManualPage.buildManualPages
     *
    */
    public get BuildManualSteps(): Promise<BuildManualStepData[]> {
        if (this._buildManualSteps !== null) {
            return Promise.resolve(this._buildManualSteps);
        }

        if (this._buildManualStepsPromise !== null) {
            return this._buildManualStepsPromise;
        }

        // Start the load
        this.loadBuildManualSteps();

        return this._buildManualStepsPromise!;
    }



    private loadBuildManualSteps(): void {

        this._buildManualStepsPromise = lastValueFrom(
            BuildManualPageService.Instance.GetBuildManualStepsForBuildManualPage(this.id)
        )
        .then(BuildManualSteps => {
            this._buildManualSteps = BuildManualSteps ?? [];
            this._buildManualStepsSubject.next(this._buildManualSteps);
            return this._buildManualSteps;
         })
        .catch(err => {
            this._buildManualSteps = [];
            this._buildManualStepsSubject.next(this._buildManualSteps);
            throw err;
        })
        .finally(() => {
            this._buildManualStepsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BuildManualStep. Call after mutations to force refresh.
     */
    public ClearBuildManualStepsCache(): void {
        this._buildManualSteps = null;
        this._buildManualStepsPromise = null;
        this._buildManualStepsSubject.next(this._buildManualSteps);      // Emit to observable
    }

    public get HasBuildManualSteps(): Promise<boolean> {
        return this.BuildManualSteps.then(buildManualSteps => buildManualSteps.length > 0);
    }




    /**
     * Updates the state of this BuildManualPageData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BuildManualPageData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BuildManualPageSubmitData {
        return BuildManualPageService.Instance.ConvertToBuildManualPageSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BuildManualPageService extends SecureEndpointBase {

    private static _instance: BuildManualPageService;
    private listCache: Map<string, Observable<Array<BuildManualPageData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BuildManualPageBasicListData>>>;
    private recordCache: Map<string, Observable<BuildManualPageData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private buildManualStepService: BuildManualStepService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BuildManualPageData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BuildManualPageBasicListData>>>();
        this.recordCache = new Map<string, Observable<BuildManualPageData>>();

        BuildManualPageService._instance = this;
    }

    public static get Instance(): BuildManualPageService {
      return BuildManualPageService._instance;
    }


    public ClearListCaches(config: BuildManualPageQueryParameters | null = null) {

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


    public ConvertToBuildManualPageSubmitData(data: BuildManualPageData): BuildManualPageSubmitData {

        let output = new BuildManualPageSubmitData();

        output.id = data.id;
        output.buildManualId = data.buildManualId;
        output.pageNum = data.pageNum;
        output.title = data.title;
        output.notes = data.notes;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBuildManualPage(id: bigint | number, includeRelations: boolean = true) : Observable<BuildManualPageData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const buildManualPage$ = this.requestBuildManualPage(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildManualPage", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, buildManualPage$);

            return buildManualPage$;
        }

        return this.recordCache.get(configHash) as Observable<BuildManualPageData>;
    }

    private requestBuildManualPage(id: bigint | number, includeRelations: boolean = true) : Observable<BuildManualPageData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BuildManualPageData>(this.baseUrl + 'api/BuildManualPage/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBuildManualPage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualPage(id, includeRelations));
            }));
    }

    public GetBuildManualPageList(config: BuildManualPageQueryParameters | any = null) : Observable<Array<BuildManualPageData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const buildManualPageList$ = this.requestBuildManualPageList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildManualPage list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, buildManualPageList$);

            return buildManualPageList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BuildManualPageData>>;
    }


    private requestBuildManualPageList(config: BuildManualPageQueryParameters | any) : Observable <Array<BuildManualPageData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildManualPageData>>(this.baseUrl + 'api/BuildManualPages', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBuildManualPageList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualPageList(config));
            }));
    }

    public GetBuildManualPagesRowCount(config: BuildManualPageQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const buildManualPagesRowCount$ = this.requestBuildManualPagesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildManualPages row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, buildManualPagesRowCount$);

            return buildManualPagesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBuildManualPagesRowCount(config: BuildManualPageQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BuildManualPages/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualPagesRowCount(config));
            }));
    }

    public GetBuildManualPagesBasicListData(config: BuildManualPageQueryParameters | any = null) : Observable<Array<BuildManualPageBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const buildManualPagesBasicListData$ = this.requestBuildManualPagesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildManualPages basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, buildManualPagesBasicListData$);

            return buildManualPagesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BuildManualPageBasicListData>>;
    }


    private requestBuildManualPagesBasicListData(config: BuildManualPageQueryParameters | any) : Observable<Array<BuildManualPageBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildManualPageBasicListData>>(this.baseUrl + 'api/BuildManualPages/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualPagesBasicListData(config));
            }));

    }


    public PutBuildManualPage(id: bigint | number, buildManualPage: BuildManualPageSubmitData) : Observable<BuildManualPageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BuildManualPageData>(this.baseUrl + 'api/BuildManualPage/' + id.toString(), buildManualPage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildManualPage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBuildManualPage(id, buildManualPage));
            }));
    }


    public PostBuildManualPage(buildManualPage: BuildManualPageSubmitData) : Observable<BuildManualPageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BuildManualPageData>(this.baseUrl + 'api/BuildManualPage', buildManualPage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildManualPage(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBuildManualPage(buildManualPage));
            }));
    }

  
    public DeleteBuildManualPage(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BuildManualPage/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBuildManualPage(id));
            }));
    }


    private getConfigHash(config: BuildManualPageQueryParameters | any): string {

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

    public userIsBMCBuildManualPageReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBuildManualPageReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BuildManualPages
        //
        if (userIsBMCBuildManualPageReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBuildManualPageReader = user.readPermission >= 1;
            } else {
                userIsBMCBuildManualPageReader = false;
            }
        }

        return userIsBMCBuildManualPageReader;
    }


    public userIsBMCBuildManualPageWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBuildManualPageWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BuildManualPages
        //
        if (userIsBMCBuildManualPageWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBuildManualPageWriter = user.writePermission >= 20;
          } else {
            userIsBMCBuildManualPageWriter = false;
          }      
        }

        return userIsBMCBuildManualPageWriter;
    }

    public GetBuildManualStepsForBuildManualPage(buildManualPageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BuildManualStepData[]> {
        return this.buildManualStepService.GetBuildManualStepList({
            buildManualPageId: buildManualPageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full BuildManualPageData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BuildManualPageData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BuildManualPageTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBuildManualPage(raw: any): BuildManualPageData {
    if (!raw) return raw;

    //
    // Create a BuildManualPageData object instance with correct prototype
    //
    const revived = Object.create(BuildManualPageData.prototype) as BuildManualPageData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._buildManualSteps = null;
    (revived as any)._buildManualStepsPromise = null;
    (revived as any)._buildManualStepsSubject = new BehaviorSubject<BuildManualStepData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadBuildManualPageXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).BuildManualSteps$ = (revived as any)._buildManualStepsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._buildManualSteps === null && (revived as any)._buildManualStepsPromise === null) {
                (revived as any).loadBuildManualSteps();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._buildManualStepsCount$ = null;



    return revived;
  }

  private ReviveBuildManualPageList(rawList: any[]): BuildManualPageData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBuildManualPage(raw));
  }

}
