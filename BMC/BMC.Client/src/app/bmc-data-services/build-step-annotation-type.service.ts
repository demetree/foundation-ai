/*

   GENERATED SERVICE FOR THE BUILDSTEPANNOTATIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BuildStepAnnotationType table.

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
export class BuildStepAnnotationTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class BuildStepAnnotationTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BuildStepAnnotationTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BuildStepAnnotationTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `buildStepAnnotationType.BuildStepAnnotationTypeChildren$` — use with `| async` in templates
//        • Promise:    `buildStepAnnotationType.BuildStepAnnotationTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="buildStepAnnotationType.BuildStepAnnotationTypeChildren$ | async"`), or
//        • Access the promise getter (`buildStepAnnotationType.BuildStepAnnotationTypeChildren` or `await buildStepAnnotationType.BuildStepAnnotationTypeChildren`)
//    - Simply reading `buildStepAnnotationType.BuildStepAnnotationTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await buildStepAnnotationType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BuildStepAnnotationTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _buildStepAnnotations: BuildStepAnnotationData[] | null = null;
    private _buildStepAnnotationsPromise: Promise<BuildStepAnnotationData[]> | null  = null;
    private _buildStepAnnotationsSubject = new BehaviorSubject<BuildStepAnnotationData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
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
            this._buildStepAnnotationsCount$ = BuildStepAnnotationService.Instance.GetBuildStepAnnotationsRowCount({buildStepAnnotationTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._buildStepAnnotationsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any BuildStepAnnotationTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.buildStepAnnotationType.Reload();
  //
  //  Non Async:
  //
  //     buildStepAnnotationType[0].Reload().then(x => {
  //        this.buildStepAnnotationType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BuildStepAnnotationTypeService.Instance.GetBuildStepAnnotationType(this.id, includeRelations)
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
     * Gets the BuildStepAnnotations for this BuildStepAnnotationType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.buildStepAnnotationType.BuildStepAnnotations.then(buildStepAnnotationTypes => { ... })
     *   or
     *   await this.buildStepAnnotationType.buildStepAnnotationTypes
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
            BuildStepAnnotationTypeService.Instance.GetBuildStepAnnotationsForBuildStepAnnotationType(this.id)
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
     * Updates the state of this BuildStepAnnotationTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BuildStepAnnotationTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BuildStepAnnotationTypeSubmitData {
        return BuildStepAnnotationTypeService.Instance.ConvertToBuildStepAnnotationTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BuildStepAnnotationTypeService extends SecureEndpointBase {

    private static _instance: BuildStepAnnotationTypeService;
    private listCache: Map<string, Observable<Array<BuildStepAnnotationTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BuildStepAnnotationTypeBasicListData>>>;
    private recordCache: Map<string, Observable<BuildStepAnnotationTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private buildStepAnnotationService: BuildStepAnnotationService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BuildStepAnnotationTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BuildStepAnnotationTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<BuildStepAnnotationTypeData>>();

        BuildStepAnnotationTypeService._instance = this;
    }

    public static get Instance(): BuildStepAnnotationTypeService {
      return BuildStepAnnotationTypeService._instance;
    }


    public ClearListCaches(config: BuildStepAnnotationTypeQueryParameters | null = null) {

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


    public ConvertToBuildStepAnnotationTypeSubmitData(data: BuildStepAnnotationTypeData): BuildStepAnnotationTypeSubmitData {

        let output = new BuildStepAnnotationTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBuildStepAnnotationType(id: bigint | number, includeRelations: boolean = true) : Observable<BuildStepAnnotationTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const buildStepAnnotationType$ = this.requestBuildStepAnnotationType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildStepAnnotationType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, buildStepAnnotationType$);

            return buildStepAnnotationType$;
        }

        return this.recordCache.get(configHash) as Observable<BuildStepAnnotationTypeData>;
    }

    private requestBuildStepAnnotationType(id: bigint | number, includeRelations: boolean = true) : Observable<BuildStepAnnotationTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BuildStepAnnotationTypeData>(this.baseUrl + 'api/BuildStepAnnotationType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBuildStepAnnotationType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildStepAnnotationType(id, includeRelations));
            }));
    }

    public GetBuildStepAnnotationTypeList(config: BuildStepAnnotationTypeQueryParameters | any = null) : Observable<Array<BuildStepAnnotationTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const buildStepAnnotationTypeList$ = this.requestBuildStepAnnotationTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildStepAnnotationType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, buildStepAnnotationTypeList$);

            return buildStepAnnotationTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BuildStepAnnotationTypeData>>;
    }


    private requestBuildStepAnnotationTypeList(config: BuildStepAnnotationTypeQueryParameters | any) : Observable <Array<BuildStepAnnotationTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildStepAnnotationTypeData>>(this.baseUrl + 'api/BuildStepAnnotationTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBuildStepAnnotationTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildStepAnnotationTypeList(config));
            }));
    }

    public GetBuildStepAnnotationTypesRowCount(config: BuildStepAnnotationTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const buildStepAnnotationTypesRowCount$ = this.requestBuildStepAnnotationTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildStepAnnotationTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, buildStepAnnotationTypesRowCount$);

            return buildStepAnnotationTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBuildStepAnnotationTypesRowCount(config: BuildStepAnnotationTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BuildStepAnnotationTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildStepAnnotationTypesRowCount(config));
            }));
    }

    public GetBuildStepAnnotationTypesBasicListData(config: BuildStepAnnotationTypeQueryParameters | any = null) : Observable<Array<BuildStepAnnotationTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const buildStepAnnotationTypesBasicListData$ = this.requestBuildStepAnnotationTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildStepAnnotationTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, buildStepAnnotationTypesBasicListData$);

            return buildStepAnnotationTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BuildStepAnnotationTypeBasicListData>>;
    }


    private requestBuildStepAnnotationTypesBasicListData(config: BuildStepAnnotationTypeQueryParameters | any) : Observable<Array<BuildStepAnnotationTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildStepAnnotationTypeBasicListData>>(this.baseUrl + 'api/BuildStepAnnotationTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildStepAnnotationTypesBasicListData(config));
            }));

    }


    public PutBuildStepAnnotationType(id: bigint | number, buildStepAnnotationType: BuildStepAnnotationTypeSubmitData) : Observable<BuildStepAnnotationTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BuildStepAnnotationTypeData>(this.baseUrl + 'api/BuildStepAnnotationType/' + id.toString(), buildStepAnnotationType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildStepAnnotationType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBuildStepAnnotationType(id, buildStepAnnotationType));
            }));
    }


    public PostBuildStepAnnotationType(buildStepAnnotationType: BuildStepAnnotationTypeSubmitData) : Observable<BuildStepAnnotationTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BuildStepAnnotationTypeData>(this.baseUrl + 'api/BuildStepAnnotationType', buildStepAnnotationType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildStepAnnotationType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBuildStepAnnotationType(buildStepAnnotationType));
            }));
    }

  
    public DeleteBuildStepAnnotationType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BuildStepAnnotationType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBuildStepAnnotationType(id));
            }));
    }


    private getConfigHash(config: BuildStepAnnotationTypeQueryParameters | any): string {

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

    public userIsBMCBuildStepAnnotationTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBuildStepAnnotationTypeReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BuildStepAnnotationTypes
        //
        if (userIsBMCBuildStepAnnotationTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBuildStepAnnotationTypeReader = user.readPermission >= 1;
            } else {
                userIsBMCBuildStepAnnotationTypeReader = false;
            }
        }

        return userIsBMCBuildStepAnnotationTypeReader;
    }


    public userIsBMCBuildStepAnnotationTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBuildStepAnnotationTypeWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BuildStepAnnotationTypes
        //
        if (userIsBMCBuildStepAnnotationTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBuildStepAnnotationTypeWriter = user.writePermission >= 255;
          } else {
            userIsBMCBuildStepAnnotationTypeWriter = false;
          }      
        }

        return userIsBMCBuildStepAnnotationTypeWriter;
    }

    public GetBuildStepAnnotationsForBuildStepAnnotationType(buildStepAnnotationTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BuildStepAnnotationData[]> {
        return this.buildStepAnnotationService.GetBuildStepAnnotationList({
            buildStepAnnotationTypeId: buildStepAnnotationTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full BuildStepAnnotationTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BuildStepAnnotationTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BuildStepAnnotationTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBuildStepAnnotationType(raw: any): BuildStepAnnotationTypeData {
    if (!raw) return raw;

    //
    // Create a BuildStepAnnotationTypeData object instance with correct prototype
    //
    const revived = Object.create(BuildStepAnnotationTypeData.prototype) as BuildStepAnnotationTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
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
    // 2. But private methods (loadBuildStepAnnotationTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
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

  private ReviveBuildStepAnnotationTypeList(rawList: any[]): BuildStepAnnotationTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBuildStepAnnotationType(raw));
  }

}
