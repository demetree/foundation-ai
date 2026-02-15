/*

   GENERATED SERVICE FOR THE BUILDSTEPANNOTATION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BuildStepAnnotation table.

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
import { BuildManualStepData } from './build-manual-step.service';
import { BuildStepAnnotationTypeData } from './build-step-annotation-type.service';
import { PlacedBrickData } from './placed-brick.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BuildStepAnnotationQueryParameters {
    buildManualStepId: bigint | number | null | undefined = null;
    buildStepAnnotationTypeId: bigint | number | null | undefined = null;
    positionX: number | null | undefined = null;
    positionY: number | null | undefined = null;
    width: number | null | undefined = null;
    height: number | null | undefined = null;
    text: string | null | undefined = null;
    placedBrickId: bigint | number | null | undefined = null;
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
export class BuildStepAnnotationSubmitData {
    id!: bigint | number;
    buildManualStepId!: bigint | number;
    buildStepAnnotationTypeId!: bigint | number;
    positionX: number | null = null;
    positionY: number | null = null;
    width: number | null = null;
    height: number | null = null;
    text: string | null = null;
    placedBrickId: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BuildStepAnnotationBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BuildStepAnnotationChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `buildStepAnnotation.BuildStepAnnotationChildren$` — use with `| async` in templates
//        • Promise:    `buildStepAnnotation.BuildStepAnnotationChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="buildStepAnnotation.BuildStepAnnotationChildren$ | async"`), or
//        • Access the promise getter (`buildStepAnnotation.BuildStepAnnotationChildren` or `await buildStepAnnotation.BuildStepAnnotationChildren`)
//    - Simply reading `buildStepAnnotation.BuildStepAnnotationChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await buildStepAnnotation.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BuildStepAnnotationData {
    id!: bigint | number;
    buildManualStepId!: bigint | number;
    buildStepAnnotationTypeId!: bigint | number;
    positionX!: number | null;
    positionY!: number | null;
    width!: number | null;
    height!: number | null;
    text!: string | null;
    placedBrickId!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    buildManualStep: BuildManualStepData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    buildStepAnnotationType: BuildStepAnnotationTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    placedBrick: PlacedBrickData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //

  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any BuildStepAnnotationData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.buildStepAnnotation.Reload();
  //
  //  Non Async:
  //
  //     buildStepAnnotation[0].Reload().then(x => {
  //        this.buildStepAnnotation = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BuildStepAnnotationService.Instance.GetBuildStepAnnotation(this.id, includeRelations)
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
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //


    /**
     * Updates the state of this BuildStepAnnotationData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BuildStepAnnotationData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BuildStepAnnotationSubmitData {
        return BuildStepAnnotationService.Instance.ConvertToBuildStepAnnotationSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BuildStepAnnotationService extends SecureEndpointBase {

    private static _instance: BuildStepAnnotationService;
    private listCache: Map<string, Observable<Array<BuildStepAnnotationData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BuildStepAnnotationBasicListData>>>;
    private recordCache: Map<string, Observable<BuildStepAnnotationData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BuildStepAnnotationData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BuildStepAnnotationBasicListData>>>();
        this.recordCache = new Map<string, Observable<BuildStepAnnotationData>>();

        BuildStepAnnotationService._instance = this;
    }

    public static get Instance(): BuildStepAnnotationService {
      return BuildStepAnnotationService._instance;
    }


    public ClearListCaches(config: BuildStepAnnotationQueryParameters | null = null) {

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


    public ConvertToBuildStepAnnotationSubmitData(data: BuildStepAnnotationData): BuildStepAnnotationSubmitData {

        let output = new BuildStepAnnotationSubmitData();

        output.id = data.id;
        output.buildManualStepId = data.buildManualStepId;
        output.buildStepAnnotationTypeId = data.buildStepAnnotationTypeId;
        output.positionX = data.positionX;
        output.positionY = data.positionY;
        output.width = data.width;
        output.height = data.height;
        output.text = data.text;
        output.placedBrickId = data.placedBrickId;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBuildStepAnnotation(id: bigint | number, includeRelations: boolean = true) : Observable<BuildStepAnnotationData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const buildStepAnnotation$ = this.requestBuildStepAnnotation(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildStepAnnotation", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, buildStepAnnotation$);

            return buildStepAnnotation$;
        }

        return this.recordCache.get(configHash) as Observable<BuildStepAnnotationData>;
    }

    private requestBuildStepAnnotation(id: bigint | number, includeRelations: boolean = true) : Observable<BuildStepAnnotationData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BuildStepAnnotationData>(this.baseUrl + 'api/BuildStepAnnotation/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBuildStepAnnotation(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildStepAnnotation(id, includeRelations));
            }));
    }

    public GetBuildStepAnnotationList(config: BuildStepAnnotationQueryParameters | any = null) : Observable<Array<BuildStepAnnotationData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const buildStepAnnotationList$ = this.requestBuildStepAnnotationList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildStepAnnotation list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, buildStepAnnotationList$);

            return buildStepAnnotationList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BuildStepAnnotationData>>;
    }


    private requestBuildStepAnnotationList(config: BuildStepAnnotationQueryParameters | any) : Observable <Array<BuildStepAnnotationData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildStepAnnotationData>>(this.baseUrl + 'api/BuildStepAnnotations', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBuildStepAnnotationList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildStepAnnotationList(config));
            }));
    }

    public GetBuildStepAnnotationsRowCount(config: BuildStepAnnotationQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const buildStepAnnotationsRowCount$ = this.requestBuildStepAnnotationsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildStepAnnotations row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, buildStepAnnotationsRowCount$);

            return buildStepAnnotationsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBuildStepAnnotationsRowCount(config: BuildStepAnnotationQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BuildStepAnnotations/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildStepAnnotationsRowCount(config));
            }));
    }

    public GetBuildStepAnnotationsBasicListData(config: BuildStepAnnotationQueryParameters | any = null) : Observable<Array<BuildStepAnnotationBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const buildStepAnnotationsBasicListData$ = this.requestBuildStepAnnotationsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildStepAnnotations basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, buildStepAnnotationsBasicListData$);

            return buildStepAnnotationsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BuildStepAnnotationBasicListData>>;
    }


    private requestBuildStepAnnotationsBasicListData(config: BuildStepAnnotationQueryParameters | any) : Observable<Array<BuildStepAnnotationBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildStepAnnotationBasicListData>>(this.baseUrl + 'api/BuildStepAnnotations/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildStepAnnotationsBasicListData(config));
            }));

    }


    public PutBuildStepAnnotation(id: bigint | number, buildStepAnnotation: BuildStepAnnotationSubmitData) : Observable<BuildStepAnnotationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BuildStepAnnotationData>(this.baseUrl + 'api/BuildStepAnnotation/' + id.toString(), buildStepAnnotation, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildStepAnnotation(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBuildStepAnnotation(id, buildStepAnnotation));
            }));
    }


    public PostBuildStepAnnotation(buildStepAnnotation: BuildStepAnnotationSubmitData) : Observable<BuildStepAnnotationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BuildStepAnnotationData>(this.baseUrl + 'api/BuildStepAnnotation', buildStepAnnotation, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildStepAnnotation(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBuildStepAnnotation(buildStepAnnotation));
            }));
    }

  
    public DeleteBuildStepAnnotation(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BuildStepAnnotation/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBuildStepAnnotation(id));
            }));
    }


    private getConfigHash(config: BuildStepAnnotationQueryParameters | any): string {

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

    public userIsBMCBuildStepAnnotationReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBuildStepAnnotationReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BuildStepAnnotations
        //
        if (userIsBMCBuildStepAnnotationReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBuildStepAnnotationReader = user.readPermission >= 1;
            } else {
                userIsBMCBuildStepAnnotationReader = false;
            }
        }

        return userIsBMCBuildStepAnnotationReader;
    }


    public userIsBMCBuildStepAnnotationWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBuildStepAnnotationWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BuildStepAnnotations
        //
        if (userIsBMCBuildStepAnnotationWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBuildStepAnnotationWriter = user.writePermission >= 20;
          } else {
            userIsBMCBuildStepAnnotationWriter = false;
          }      
        }

        return userIsBMCBuildStepAnnotationWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BuildStepAnnotationData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BuildStepAnnotationData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BuildStepAnnotationTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBuildStepAnnotation(raw: any): BuildStepAnnotationData {
    if (!raw) return raw;

    //
    // Create a BuildStepAnnotationData object instance with correct prototype
    //
    const revived = Object.create(BuildStepAnnotationData.prototype) as BuildStepAnnotationData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //

    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadBuildStepAnnotationXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBuildStepAnnotationList(rawList: any[]): BuildStepAnnotationData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBuildStepAnnotation(raw));
  }

}
