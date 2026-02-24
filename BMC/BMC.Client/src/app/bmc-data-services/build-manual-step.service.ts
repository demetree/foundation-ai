/*

   GENERATED SERVICE FOR THE BUILDMANUALSTEP TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BuildManualStep table.

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
import { BuildManualPageData } from './build-manual-page.service';
import { BuildStepPartService, BuildStepPartData } from './build-step-part.service';
import { BuildStepAnnotationService, BuildStepAnnotationData } from './build-step-annotation.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BuildManualStepQueryParameters {
    buildManualPageId: bigint | number | null | undefined = null;
    stepNumber: bigint | number | null | undefined = null;
    cameraPositionX: number | null | undefined = null;
    cameraPositionY: number | null | undefined = null;
    cameraPositionZ: number | null | undefined = null;
    cameraTargetX: number | null | undefined = null;
    cameraTargetY: number | null | undefined = null;
    cameraTargetZ: number | null | undefined = null;
    cameraZoom: number | null | undefined = null;
    showExplodedView: boolean | null | undefined = null;
    explodedDistance: number | null | undefined = null;
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
export class BuildManualStepSubmitData {
    id!: bigint | number;
    buildManualPageId!: bigint | number;
    stepNumber: bigint | number | null = null;
    cameraPositionX: number | null = null;
    cameraPositionY: number | null = null;
    cameraPositionZ: number | null = null;
    cameraTargetX: number | null = null;
    cameraTargetY: number | null = null;
    cameraTargetZ: number | null = null;
    cameraZoom: number | null = null;
    showExplodedView!: boolean;
    explodedDistance: number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BuildManualStepBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BuildManualStepChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `buildManualStep.BuildManualStepChildren$` — use with `| async` in templates
//        • Promise:    `buildManualStep.BuildManualStepChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="buildManualStep.BuildManualStepChildren$ | async"`), or
//        • Access the promise getter (`buildManualStep.BuildManualStepChildren` or `await buildManualStep.BuildManualStepChildren`)
//    - Simply reading `buildManualStep.BuildManualStepChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await buildManualStep.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BuildManualStepData {
    id!: bigint | number;
    buildManualPageId!: bigint | number;
    stepNumber!: bigint | number;
    cameraPositionX!: number | null;
    cameraPositionY!: number | null;
    cameraPositionZ!: number | null;
    cameraTargetX!: number | null;
    cameraTargetY!: number | null;
    cameraTargetZ!: number | null;
    cameraZoom!: number | null;
    showExplodedView!: boolean;
    explodedDistance!: number | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    buildManualPage: BuildManualPageData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _buildStepParts: BuildStepPartData[] | null = null;
    private _buildStepPartsPromise: Promise<BuildStepPartData[]> | null  = null;
    private _buildStepPartsSubject = new BehaviorSubject<BuildStepPartData[] | null>(null);

                
    private _buildStepAnnotations: BuildStepAnnotationData[] | null = null;
    private _buildStepAnnotationsPromise: Promise<BuildStepAnnotationData[]> | null  = null;
    private _buildStepAnnotationsSubject = new BehaviorSubject<BuildStepAnnotationData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public BuildStepParts$ = this._buildStepPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._buildStepParts === null && this._buildStepPartsPromise === null) {
            this.loadBuildStepParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _buildStepPartsCount$: Observable<bigint | number> | null = null;
    public get BuildStepPartsCount$(): Observable<bigint | number> {
        if (this._buildStepPartsCount$ === null) {
            this._buildStepPartsCount$ = BuildStepPartService.Instance.GetBuildStepPartsRowCount({buildManualStepId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._buildStepPartsCount$;
    }



    public BuildStepAnnotations$ = this._buildStepAnnotationsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._buildStepAnnotations === null && this._buildStepAnnotationsPromise === null) {
            this.loadBuildStepAnnotations(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _buildStepAnnotationsCount$: Observable<bigint | number> | null = null;
    public get BuildStepAnnotationsCount$(): Observable<bigint | number> {
        if (this._buildStepAnnotationsCount$ === null) {
            this._buildStepAnnotationsCount$ = BuildStepAnnotationService.Instance.GetBuildStepAnnotationsRowCount({buildManualStepId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._buildStepAnnotationsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any BuildManualStepData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.buildManualStep.Reload();
  //
  //  Non Async:
  //
  //     buildManualStep[0].Reload().then(x => {
  //        this.buildManualStep = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BuildManualStepService.Instance.GetBuildManualStep(this.id, includeRelations)
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
     this._buildStepParts = null;
     this._buildStepPartsPromise = null;
     this._buildStepPartsSubject.next(null);
     this._buildStepPartsCount$ = null;

     this._buildStepAnnotations = null;
     this._buildStepAnnotationsPromise = null;
     this._buildStepAnnotationsSubject.next(null);
     this._buildStepAnnotationsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the BuildStepParts for this BuildManualStep.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.buildManualStep.BuildStepParts.then(buildManualSteps => { ... })
     *   or
     *   await this.buildManualStep.buildManualSteps
     *
    */
    public get BuildStepParts(): Promise<BuildStepPartData[]> {
        if (this._buildStepParts !== null) {
            return Promise.resolve(this._buildStepParts);
        }

        if (this._buildStepPartsPromise !== null) {
            return this._buildStepPartsPromise;
        }

        // Start the load
        this.loadBuildStepParts();

        return this._buildStepPartsPromise!;
    }



    private loadBuildStepParts(): void {

        this._buildStepPartsPromise = lastValueFrom(
            BuildManualStepService.Instance.GetBuildStepPartsForBuildManualStep(this.id)
        )
        .then(BuildStepParts => {
            this._buildStepParts = BuildStepParts ?? [];
            this._buildStepPartsSubject.next(this._buildStepParts);
            return this._buildStepParts;
         })
        .catch(err => {
            this._buildStepParts = [];
            this._buildStepPartsSubject.next(this._buildStepParts);
            throw err;
        })
        .finally(() => {
            this._buildStepPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BuildStepPart. Call after mutations to force refresh.
     */
    public ClearBuildStepPartsCache(): void {
        this._buildStepParts = null;
        this._buildStepPartsPromise = null;
        this._buildStepPartsSubject.next(this._buildStepParts);      // Emit to observable
    }

    public get HasBuildStepParts(): Promise<boolean> {
        return this.BuildStepParts.then(buildStepParts => buildStepParts.length > 0);
    }


    /**
     *
     * Gets the BuildStepAnnotations for this BuildManualStep.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.buildManualStep.BuildStepAnnotations.then(buildManualSteps => { ... })
     *   or
     *   await this.buildManualStep.buildManualSteps
     *
    */
    public get BuildStepAnnotations(): Promise<BuildStepAnnotationData[]> {
        if (this._buildStepAnnotations !== null) {
            return Promise.resolve(this._buildStepAnnotations);
        }

        if (this._buildStepAnnotationsPromise !== null) {
            return this._buildStepAnnotationsPromise;
        }

        // Start the load
        this.loadBuildStepAnnotations();

        return this._buildStepAnnotationsPromise!;
    }



    private loadBuildStepAnnotations(): void {

        this._buildStepAnnotationsPromise = lastValueFrom(
            BuildManualStepService.Instance.GetBuildStepAnnotationsForBuildManualStep(this.id)
        )
        .then(BuildStepAnnotations => {
            this._buildStepAnnotations = BuildStepAnnotations ?? [];
            this._buildStepAnnotationsSubject.next(this._buildStepAnnotations);
            return this._buildStepAnnotations;
         })
        .catch(err => {
            this._buildStepAnnotations = [];
            this._buildStepAnnotationsSubject.next(this._buildStepAnnotations);
            throw err;
        })
        .finally(() => {
            this._buildStepAnnotationsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BuildStepAnnotation. Call after mutations to force refresh.
     */
    public ClearBuildStepAnnotationsCache(): void {
        this._buildStepAnnotations = null;
        this._buildStepAnnotationsPromise = null;
        this._buildStepAnnotationsSubject.next(this._buildStepAnnotations);      // Emit to observable
    }

    public get HasBuildStepAnnotations(): Promise<boolean> {
        return this.BuildStepAnnotations.then(buildStepAnnotations => buildStepAnnotations.length > 0);
    }




    /**
     * Updates the state of this BuildManualStepData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BuildManualStepData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BuildManualStepSubmitData {
        return BuildManualStepService.Instance.ConvertToBuildManualStepSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BuildManualStepService extends SecureEndpointBase {

    private static _instance: BuildManualStepService;
    private listCache: Map<string, Observable<Array<BuildManualStepData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BuildManualStepBasicListData>>>;
    private recordCache: Map<string, Observable<BuildManualStepData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private buildStepPartService: BuildStepPartService,
        private buildStepAnnotationService: BuildStepAnnotationService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BuildManualStepData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BuildManualStepBasicListData>>>();
        this.recordCache = new Map<string, Observable<BuildManualStepData>>();

        BuildManualStepService._instance = this;
    }

    public static get Instance(): BuildManualStepService {
      return BuildManualStepService._instance;
    }


    public ClearListCaches(config: BuildManualStepQueryParameters | null = null) {

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


    public ConvertToBuildManualStepSubmitData(data: BuildManualStepData): BuildManualStepSubmitData {

        let output = new BuildManualStepSubmitData();

        output.id = data.id;
        output.buildManualPageId = data.buildManualPageId;
        output.stepNumber = data.stepNumber;
        output.cameraPositionX = data.cameraPositionX;
        output.cameraPositionY = data.cameraPositionY;
        output.cameraPositionZ = data.cameraPositionZ;
        output.cameraTargetX = data.cameraTargetX;
        output.cameraTargetY = data.cameraTargetY;
        output.cameraTargetZ = data.cameraTargetZ;
        output.cameraZoom = data.cameraZoom;
        output.showExplodedView = data.showExplodedView;
        output.explodedDistance = data.explodedDistance;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBuildManualStep(id: bigint | number, includeRelations: boolean = true) : Observable<BuildManualStepData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const buildManualStep$ = this.requestBuildManualStep(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildManualStep", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, buildManualStep$);

            return buildManualStep$;
        }

        return this.recordCache.get(configHash) as Observable<BuildManualStepData>;
    }

    private requestBuildManualStep(id: bigint | number, includeRelations: boolean = true) : Observable<BuildManualStepData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BuildManualStepData>(this.baseUrl + 'api/BuildManualStep/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBuildManualStep(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualStep(id, includeRelations));
            }));
    }

    public GetBuildManualStepList(config: BuildManualStepQueryParameters | any = null) : Observable<Array<BuildManualStepData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const buildManualStepList$ = this.requestBuildManualStepList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildManualStep list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, buildManualStepList$);

            return buildManualStepList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BuildManualStepData>>;
    }


    private requestBuildManualStepList(config: BuildManualStepQueryParameters | any) : Observable <Array<BuildManualStepData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildManualStepData>>(this.baseUrl + 'api/BuildManualSteps', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBuildManualStepList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualStepList(config));
            }));
    }

    public GetBuildManualStepsRowCount(config: BuildManualStepQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const buildManualStepsRowCount$ = this.requestBuildManualStepsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildManualSteps row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, buildManualStepsRowCount$);

            return buildManualStepsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBuildManualStepsRowCount(config: BuildManualStepQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BuildManualSteps/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualStepsRowCount(config));
            }));
    }

    public GetBuildManualStepsBasicListData(config: BuildManualStepQueryParameters | any = null) : Observable<Array<BuildManualStepBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const buildManualStepsBasicListData$ = this.requestBuildManualStepsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildManualSteps basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, buildManualStepsBasicListData$);

            return buildManualStepsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BuildManualStepBasicListData>>;
    }


    private requestBuildManualStepsBasicListData(config: BuildManualStepQueryParameters | any) : Observable<Array<BuildManualStepBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildManualStepBasicListData>>(this.baseUrl + 'api/BuildManualSteps/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualStepsBasicListData(config));
            }));

    }


    public PutBuildManualStep(id: bigint | number, buildManualStep: BuildManualStepSubmitData) : Observable<BuildManualStepData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BuildManualStepData>(this.baseUrl + 'api/BuildManualStep/' + id.toString(), buildManualStep, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildManualStep(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBuildManualStep(id, buildManualStep));
            }));
    }


    public PostBuildManualStep(buildManualStep: BuildManualStepSubmitData) : Observable<BuildManualStepData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BuildManualStepData>(this.baseUrl + 'api/BuildManualStep', buildManualStep, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildManualStep(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBuildManualStep(buildManualStep));
            }));
    }

  
    public DeleteBuildManualStep(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BuildManualStep/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBuildManualStep(id));
            }));
    }


    private getConfigHash(config: BuildManualStepQueryParameters | any): string {

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

    public userIsBMCBuildManualStepReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBuildManualStepReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BuildManualSteps
        //
        if (userIsBMCBuildManualStepReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBuildManualStepReader = user.readPermission >= 1;
            } else {
                userIsBMCBuildManualStepReader = false;
            }
        }

        return userIsBMCBuildManualStepReader;
    }


    public userIsBMCBuildManualStepWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBuildManualStepWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BuildManualSteps
        //
        if (userIsBMCBuildManualStepWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBuildManualStepWriter = user.writePermission >= 20;
          } else {
            userIsBMCBuildManualStepWriter = false;
          }      
        }

        return userIsBMCBuildManualStepWriter;
    }

    public GetBuildStepPartsForBuildManualStep(buildManualStepId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BuildStepPartData[]> {
        return this.buildStepPartService.GetBuildStepPartList({
            buildManualStepId: buildManualStepId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBuildStepAnnotationsForBuildManualStep(buildManualStepId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BuildStepAnnotationData[]> {
        return this.buildStepAnnotationService.GetBuildStepAnnotationList({
            buildManualStepId: buildManualStepId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full BuildManualStepData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BuildManualStepData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BuildManualStepTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBuildManualStep(raw: any): BuildManualStepData {
    if (!raw) return raw;

    //
    // Create a BuildManualStepData object instance with correct prototype
    //
    const revived = Object.create(BuildManualStepData.prototype) as BuildManualStepData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._buildStepParts = null;
    (revived as any)._buildStepPartsPromise = null;
    (revived as any)._buildStepPartsSubject = new BehaviorSubject<BuildStepPartData[] | null>(null);

    (revived as any)._buildStepAnnotations = null;
    (revived as any)._buildStepAnnotationsPromise = null;
    (revived as any)._buildStepAnnotationsSubject = new BehaviorSubject<BuildStepAnnotationData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadBuildManualStepXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).BuildStepParts$ = (revived as any)._buildStepPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._buildStepParts === null && (revived as any)._buildStepPartsPromise === null) {
                (revived as any).loadBuildStepParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._buildStepPartsCount$ = null;


    (revived as any).BuildStepAnnotations$ = (revived as any)._buildStepAnnotationsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._buildStepAnnotations === null && (revived as any)._buildStepAnnotationsPromise === null) {
                (revived as any).loadBuildStepAnnotations();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._buildStepAnnotationsCount$ = null;



    return revived;
  }

  private ReviveBuildManualStepList(rawList: any[]): BuildManualStepData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBuildManualStep(raw));
  }

}
