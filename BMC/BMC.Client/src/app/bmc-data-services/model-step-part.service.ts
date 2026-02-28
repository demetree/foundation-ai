/*

   GENERATED SERVICE FOR THE MODELSTEPPART TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ModelStepPart table.

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
import { ModelBuildStepData } from './model-build-step.service';
import { BrickPartData } from './brick-part.service';
import { BrickColourData } from './brick-colour.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ModelStepPartQueryParameters {
    modelBuildStepId: bigint | number | null | undefined = null;
    brickPartId: bigint | number | null | undefined = null;
    brickColourId: bigint | number | null | undefined = null;
    partFileName: string | null | undefined = null;
    colorCode: bigint | number | null | undefined = null;
    positionX: number | null | undefined = null;
    positionY: number | null | undefined = null;
    positionZ: number | null | undefined = null;
    transformMatrix: string | null | undefined = null;
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
export class ModelStepPartSubmitData {
    id!: bigint | number;
    modelBuildStepId!: bigint | number;
    brickPartId: bigint | number | null = null;
    brickColourId: bigint | number | null = null;
    partFileName!: string;
    colorCode!: bigint | number;
    positionX: number | null = null;
    positionY: number | null = null;
    positionZ: number | null = null;
    transformMatrix!: string;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ModelStepPartBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ModelStepPartChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `modelStepPart.ModelStepPartChildren$` — use with `| async` in templates
//        • Promise:    `modelStepPart.ModelStepPartChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="modelStepPart.ModelStepPartChildren$ | async"`), or
//        • Access the promise getter (`modelStepPart.ModelStepPartChildren` or `await modelStepPart.ModelStepPartChildren`)
//    - Simply reading `modelStepPart.ModelStepPartChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await modelStepPart.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ModelStepPartData {
    id!: bigint | number;
    modelBuildStepId!: bigint | number;
    brickPartId!: bigint | number;
    brickColourId!: bigint | number;
    partFileName!: string;
    colorCode!: bigint | number;
    positionX!: number | null;
    positionY!: number | null;
    positionZ!: number | null;
    transformMatrix!: string;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    brickColour: BrickColourData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    brickPart: BrickPartData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    modelBuildStep: ModelBuildStepData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ModelStepPartData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.modelStepPart.Reload();
  //
  //  Non Async:
  //
  //     modelStepPart[0].Reload().then(x => {
  //        this.modelStepPart = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ModelStepPartService.Instance.GetModelStepPart(this.id, includeRelations)
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
     * Updates the state of this ModelStepPartData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ModelStepPartData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ModelStepPartSubmitData {
        return ModelStepPartService.Instance.ConvertToModelStepPartSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ModelStepPartService extends SecureEndpointBase {

    private static _instance: ModelStepPartService;
    private listCache: Map<string, Observable<Array<ModelStepPartData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ModelStepPartBasicListData>>>;
    private recordCache: Map<string, Observable<ModelStepPartData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ModelStepPartData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ModelStepPartBasicListData>>>();
        this.recordCache = new Map<string, Observable<ModelStepPartData>>();

        ModelStepPartService._instance = this;
    }

    public static get Instance(): ModelStepPartService {
      return ModelStepPartService._instance;
    }


    public ClearListCaches(config: ModelStepPartQueryParameters | null = null) {

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


    public ConvertToModelStepPartSubmitData(data: ModelStepPartData): ModelStepPartSubmitData {

        let output = new ModelStepPartSubmitData();

        output.id = data.id;
        output.modelBuildStepId = data.modelBuildStepId;
        output.brickPartId = data.brickPartId;
        output.brickColourId = data.brickColourId;
        output.partFileName = data.partFileName;
        output.colorCode = data.colorCode;
        output.positionX = data.positionX;
        output.positionY = data.positionY;
        output.positionZ = data.positionZ;
        output.transformMatrix = data.transformMatrix;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetModelStepPart(id: bigint | number, includeRelations: boolean = true) : Observable<ModelStepPartData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const modelStepPart$ = this.requestModelStepPart(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ModelStepPart", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, modelStepPart$);

            return modelStepPart$;
        }

        return this.recordCache.get(configHash) as Observable<ModelStepPartData>;
    }

    private requestModelStepPart(id: bigint | number, includeRelations: boolean = true) : Observable<ModelStepPartData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ModelStepPartData>(this.baseUrl + 'api/ModelStepPart/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveModelStepPart(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestModelStepPart(id, includeRelations));
            }));
    }

    public GetModelStepPartList(config: ModelStepPartQueryParameters | any = null) : Observable<Array<ModelStepPartData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const modelStepPartList$ = this.requestModelStepPartList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ModelStepPart list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, modelStepPartList$);

            return modelStepPartList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ModelStepPartData>>;
    }


    private requestModelStepPartList(config: ModelStepPartQueryParameters | any) : Observable <Array<ModelStepPartData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ModelStepPartData>>(this.baseUrl + 'api/ModelStepParts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveModelStepPartList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestModelStepPartList(config));
            }));
    }

    public GetModelStepPartsRowCount(config: ModelStepPartQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const modelStepPartsRowCount$ = this.requestModelStepPartsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ModelStepParts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, modelStepPartsRowCount$);

            return modelStepPartsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestModelStepPartsRowCount(config: ModelStepPartQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ModelStepParts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestModelStepPartsRowCount(config));
            }));
    }

    public GetModelStepPartsBasicListData(config: ModelStepPartQueryParameters | any = null) : Observable<Array<ModelStepPartBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const modelStepPartsBasicListData$ = this.requestModelStepPartsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ModelStepParts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, modelStepPartsBasicListData$);

            return modelStepPartsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ModelStepPartBasicListData>>;
    }


    private requestModelStepPartsBasicListData(config: ModelStepPartQueryParameters | any) : Observable<Array<ModelStepPartBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ModelStepPartBasicListData>>(this.baseUrl + 'api/ModelStepParts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestModelStepPartsBasicListData(config));
            }));

    }


    public PutModelStepPart(id: bigint | number, modelStepPart: ModelStepPartSubmitData) : Observable<ModelStepPartData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ModelStepPartData>(this.baseUrl + 'api/ModelStepPart/' + id.toString(), modelStepPart, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveModelStepPart(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutModelStepPart(id, modelStepPart));
            }));
    }


    public PostModelStepPart(modelStepPart: ModelStepPartSubmitData) : Observable<ModelStepPartData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ModelStepPartData>(this.baseUrl + 'api/ModelStepPart', modelStepPart, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveModelStepPart(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostModelStepPart(modelStepPart));
            }));
    }

  
    public DeleteModelStepPart(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ModelStepPart/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteModelStepPart(id));
            }));
    }


    private getConfigHash(config: ModelStepPartQueryParameters | any): string {

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

    public userIsBMCModelStepPartReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCModelStepPartReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ModelStepParts
        //
        if (userIsBMCModelStepPartReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCModelStepPartReader = user.readPermission >= 1;
            } else {
                userIsBMCModelStepPartReader = false;
            }
        }

        return userIsBMCModelStepPartReader;
    }


    public userIsBMCModelStepPartWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCModelStepPartWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ModelStepParts
        //
        if (userIsBMCModelStepPartWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCModelStepPartWriter = user.writePermission >= 1;
          } else {
            userIsBMCModelStepPartWriter = false;
          }      
        }

        return userIsBMCModelStepPartWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ModelStepPartData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ModelStepPartData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ModelStepPartTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveModelStepPart(raw: any): ModelStepPartData {
    if (!raw) return raw;

    //
    // Create a ModelStepPartData object instance with correct prototype
    //
    const revived = Object.create(ModelStepPartData.prototype) as ModelStepPartData;

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
    // 2. But private methods (loadModelStepPartXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveModelStepPartList(rawList: any[]): ModelStepPartData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveModelStepPart(raw));
  }

}
