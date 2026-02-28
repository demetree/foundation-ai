/*

   GENERATED SERVICE FOR THE MODELBUILDSTEP TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ModelBuildStep table.

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
import { ModelSubFileData } from './model-sub-file.service';
import { ModelStepPartService, ModelStepPartData } from './model-step-part.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ModelBuildStepQueryParameters {
    modelSubFileId: bigint | number | null | undefined = null;
    stepNumber: bigint | number | null | undefined = null;
    rotationType: string | null | undefined = null;
    rotationX: number | null | undefined = null;
    rotationY: number | null | undefined = null;
    rotationZ: number | null | undefined = null;
    description: string | null | undefined = null;
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
export class ModelBuildStepSubmitData {
    id!: bigint | number;
    modelSubFileId!: bigint | number;
    stepNumber!: bigint | number;
    rotationType: string | null = null;
    rotationX: number | null = null;
    rotationY: number | null = null;
    rotationZ: number | null = null;
    description: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ModelBuildStepBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ModelBuildStepChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `modelBuildStep.ModelBuildStepChildren$` — use with `| async` in templates
//        • Promise:    `modelBuildStep.ModelBuildStepChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="modelBuildStep.ModelBuildStepChildren$ | async"`), or
//        • Access the promise getter (`modelBuildStep.ModelBuildStepChildren` or `await modelBuildStep.ModelBuildStepChildren`)
//    - Simply reading `modelBuildStep.ModelBuildStepChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await modelBuildStep.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ModelBuildStepData {
    id!: bigint | number;
    modelSubFileId!: bigint | number;
    stepNumber!: bigint | number;
    rotationType!: string | null;
    rotationX!: number | null;
    rotationY!: number | null;
    rotationZ!: number | null;
    description!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    modelSubFile: ModelSubFileData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _modelStepParts: ModelStepPartData[] | null = null;
    private _modelStepPartsPromise: Promise<ModelStepPartData[]> | null  = null;
    private _modelStepPartsSubject = new BehaviorSubject<ModelStepPartData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ModelStepParts$ = this._modelStepPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._modelStepParts === null && this._modelStepPartsPromise === null) {
            this.loadModelStepParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _modelStepPartsCount$: Observable<bigint | number> | null = null;
    public get ModelStepPartsCount$(): Observable<bigint | number> {
        if (this._modelStepPartsCount$ === null) {
            this._modelStepPartsCount$ = ModelStepPartService.Instance.GetModelStepPartsRowCount({modelBuildStepId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._modelStepPartsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ModelBuildStepData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.modelBuildStep.Reload();
  //
  //  Non Async:
  //
  //     modelBuildStep[0].Reload().then(x => {
  //        this.modelBuildStep = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ModelBuildStepService.Instance.GetModelBuildStep(this.id, includeRelations)
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
     this._modelStepParts = null;
     this._modelStepPartsPromise = null;
     this._modelStepPartsSubject.next(null);
     this._modelStepPartsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ModelStepParts for this ModelBuildStep.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.modelBuildStep.ModelStepParts.then(modelBuildSteps => { ... })
     *   or
     *   await this.modelBuildStep.modelBuildSteps
     *
    */
    public get ModelStepParts(): Promise<ModelStepPartData[]> {
        if (this._modelStepParts !== null) {
            return Promise.resolve(this._modelStepParts);
        }

        if (this._modelStepPartsPromise !== null) {
            return this._modelStepPartsPromise;
        }

        // Start the load
        this.loadModelStepParts();

        return this._modelStepPartsPromise!;
    }



    private loadModelStepParts(): void {

        this._modelStepPartsPromise = lastValueFrom(
            ModelBuildStepService.Instance.GetModelStepPartsForModelBuildStep(this.id)
        )
        .then(ModelStepParts => {
            this._modelStepParts = ModelStepParts ?? [];
            this._modelStepPartsSubject.next(this._modelStepParts);
            return this._modelStepParts;
         })
        .catch(err => {
            this._modelStepParts = [];
            this._modelStepPartsSubject.next(this._modelStepParts);
            throw err;
        })
        .finally(() => {
            this._modelStepPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ModelStepPart. Call after mutations to force refresh.
     */
    public ClearModelStepPartsCache(): void {
        this._modelStepParts = null;
        this._modelStepPartsPromise = null;
        this._modelStepPartsSubject.next(this._modelStepParts);      // Emit to observable
    }

    public get HasModelStepParts(): Promise<boolean> {
        return this.ModelStepParts.then(modelStepParts => modelStepParts.length > 0);
    }




    /**
     * Updates the state of this ModelBuildStepData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ModelBuildStepData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ModelBuildStepSubmitData {
        return ModelBuildStepService.Instance.ConvertToModelBuildStepSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ModelBuildStepService extends SecureEndpointBase {

    private static _instance: ModelBuildStepService;
    private listCache: Map<string, Observable<Array<ModelBuildStepData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ModelBuildStepBasicListData>>>;
    private recordCache: Map<string, Observable<ModelBuildStepData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private modelStepPartService: ModelStepPartService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ModelBuildStepData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ModelBuildStepBasicListData>>>();
        this.recordCache = new Map<string, Observable<ModelBuildStepData>>();

        ModelBuildStepService._instance = this;
    }

    public static get Instance(): ModelBuildStepService {
      return ModelBuildStepService._instance;
    }


    public ClearListCaches(config: ModelBuildStepQueryParameters | null = null) {

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


    public ConvertToModelBuildStepSubmitData(data: ModelBuildStepData): ModelBuildStepSubmitData {

        let output = new ModelBuildStepSubmitData();

        output.id = data.id;
        output.modelSubFileId = data.modelSubFileId;
        output.stepNumber = data.stepNumber;
        output.rotationType = data.rotationType;
        output.rotationX = data.rotationX;
        output.rotationY = data.rotationY;
        output.rotationZ = data.rotationZ;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetModelBuildStep(id: bigint | number, includeRelations: boolean = true) : Observable<ModelBuildStepData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const modelBuildStep$ = this.requestModelBuildStep(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ModelBuildStep", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, modelBuildStep$);

            return modelBuildStep$;
        }

        return this.recordCache.get(configHash) as Observable<ModelBuildStepData>;
    }

    private requestModelBuildStep(id: bigint | number, includeRelations: boolean = true) : Observable<ModelBuildStepData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ModelBuildStepData>(this.baseUrl + 'api/ModelBuildStep/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveModelBuildStep(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestModelBuildStep(id, includeRelations));
            }));
    }

    public GetModelBuildStepList(config: ModelBuildStepQueryParameters | any = null) : Observable<Array<ModelBuildStepData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const modelBuildStepList$ = this.requestModelBuildStepList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ModelBuildStep list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, modelBuildStepList$);

            return modelBuildStepList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ModelBuildStepData>>;
    }


    private requestModelBuildStepList(config: ModelBuildStepQueryParameters | any) : Observable <Array<ModelBuildStepData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ModelBuildStepData>>(this.baseUrl + 'api/ModelBuildSteps', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveModelBuildStepList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestModelBuildStepList(config));
            }));
    }

    public GetModelBuildStepsRowCount(config: ModelBuildStepQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const modelBuildStepsRowCount$ = this.requestModelBuildStepsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ModelBuildSteps row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, modelBuildStepsRowCount$);

            return modelBuildStepsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestModelBuildStepsRowCount(config: ModelBuildStepQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ModelBuildSteps/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestModelBuildStepsRowCount(config));
            }));
    }

    public GetModelBuildStepsBasicListData(config: ModelBuildStepQueryParameters | any = null) : Observable<Array<ModelBuildStepBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const modelBuildStepsBasicListData$ = this.requestModelBuildStepsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ModelBuildSteps basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, modelBuildStepsBasicListData$);

            return modelBuildStepsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ModelBuildStepBasicListData>>;
    }


    private requestModelBuildStepsBasicListData(config: ModelBuildStepQueryParameters | any) : Observable<Array<ModelBuildStepBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ModelBuildStepBasicListData>>(this.baseUrl + 'api/ModelBuildSteps/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestModelBuildStepsBasicListData(config));
            }));

    }


    public PutModelBuildStep(id: bigint | number, modelBuildStep: ModelBuildStepSubmitData) : Observable<ModelBuildStepData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ModelBuildStepData>(this.baseUrl + 'api/ModelBuildStep/' + id.toString(), modelBuildStep, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveModelBuildStep(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutModelBuildStep(id, modelBuildStep));
            }));
    }


    public PostModelBuildStep(modelBuildStep: ModelBuildStepSubmitData) : Observable<ModelBuildStepData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ModelBuildStepData>(this.baseUrl + 'api/ModelBuildStep', modelBuildStep, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveModelBuildStep(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostModelBuildStep(modelBuildStep));
            }));
    }

  
    public DeleteModelBuildStep(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ModelBuildStep/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteModelBuildStep(id));
            }));
    }


    private getConfigHash(config: ModelBuildStepQueryParameters | any): string {

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

    public userIsBMCModelBuildStepReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCModelBuildStepReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ModelBuildSteps
        //
        if (userIsBMCModelBuildStepReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCModelBuildStepReader = user.readPermission >= 1;
            } else {
                userIsBMCModelBuildStepReader = false;
            }
        }

        return userIsBMCModelBuildStepReader;
    }


    public userIsBMCModelBuildStepWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCModelBuildStepWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ModelBuildSteps
        //
        if (userIsBMCModelBuildStepWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCModelBuildStepWriter = user.writePermission >= 1;
          } else {
            userIsBMCModelBuildStepWriter = false;
          }      
        }

        return userIsBMCModelBuildStepWriter;
    }

    public GetModelStepPartsForModelBuildStep(modelBuildStepId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ModelStepPartData[]> {
        return this.modelStepPartService.GetModelStepPartList({
            modelBuildStepId: modelBuildStepId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ModelBuildStepData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ModelBuildStepData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ModelBuildStepTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveModelBuildStep(raw: any): ModelBuildStepData {
    if (!raw) return raw;

    //
    // Create a ModelBuildStepData object instance with correct prototype
    //
    const revived = Object.create(ModelBuildStepData.prototype) as ModelBuildStepData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._modelStepParts = null;
    (revived as any)._modelStepPartsPromise = null;
    (revived as any)._modelStepPartsSubject = new BehaviorSubject<ModelStepPartData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadModelBuildStepXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ModelStepParts$ = (revived as any)._modelStepPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._modelStepParts === null && (revived as any)._modelStepPartsPromise === null) {
                (revived as any).loadModelStepParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._modelStepPartsCount$ = null;



    return revived;
  }

  private ReviveModelBuildStepList(rawList: any[]): ModelBuildStepData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveModelBuildStep(raw));
  }

}
