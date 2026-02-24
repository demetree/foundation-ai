/*

   GENERATED SERVICE FOR THE PARTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PartType table.

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
import { BrickPartService, BrickPartData } from './brick-part.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PartTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    isUserVisible: boolean | null | undefined = null;
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
export class PartTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isUserVisible!: boolean;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class PartTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PartTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `partType.PartTypeChildren$` — use with `| async` in templates
//        • Promise:    `partType.PartTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="partType.PartTypeChildren$ | async"`), or
//        • Access the promise getter (`partType.PartTypeChildren` or `await partType.PartTypeChildren`)
//    - Simply reading `partType.PartTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await partType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PartTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isUserVisible!: boolean;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _brickParts: BrickPartData[] | null = null;
    private _brickPartsPromise: Promise<BrickPartData[]> | null  = null;
    private _brickPartsSubject = new BehaviorSubject<BrickPartData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public BrickParts$ = this._brickPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickParts === null && this._brickPartsPromise === null) {
            this.loadBrickParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _brickPartsCount$: Observable<bigint | number> | null = null;
    public get BrickPartsCount$(): Observable<bigint | number> {
        if (this._brickPartsCount$ === null) {
            this._brickPartsCount$ = BrickPartService.Instance.GetBrickPartsRowCount({partTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._brickPartsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PartTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.partType.Reload();
  //
  //  Non Async:
  //
  //     partType[0].Reload().then(x => {
  //        this.partType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PartTypeService.Instance.GetPartType(this.id, includeRelations)
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
     this._brickParts = null;
     this._brickPartsPromise = null;
     this._brickPartsSubject.next(null);
     this._brickPartsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the BrickParts for this PartType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.partType.BrickParts.then(partTypes => { ... })
     *   or
     *   await this.partType.partTypes
     *
    */
    public get BrickParts(): Promise<BrickPartData[]> {
        if (this._brickParts !== null) {
            return Promise.resolve(this._brickParts);
        }

        if (this._brickPartsPromise !== null) {
            return this._brickPartsPromise;
        }

        // Start the load
        this.loadBrickParts();

        return this._brickPartsPromise!;
    }



    private loadBrickParts(): void {

        this._brickPartsPromise = lastValueFrom(
            PartTypeService.Instance.GetBrickPartsForPartType(this.id)
        )
        .then(BrickParts => {
            this._brickParts = BrickParts ?? [];
            this._brickPartsSubject.next(this._brickParts);
            return this._brickParts;
         })
        .catch(err => {
            this._brickParts = [];
            this._brickPartsSubject.next(this._brickParts);
            throw err;
        })
        .finally(() => {
            this._brickPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BrickPart. Call after mutations to force refresh.
     */
    public ClearBrickPartsCache(): void {
        this._brickParts = null;
        this._brickPartsPromise = null;
        this._brickPartsSubject.next(this._brickParts);      // Emit to observable
    }

    public get HasBrickParts(): Promise<boolean> {
        return this.BrickParts.then(brickParts => brickParts.length > 0);
    }




    /**
     * Updates the state of this PartTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PartTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PartTypeSubmitData {
        return PartTypeService.Instance.ConvertToPartTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PartTypeService extends SecureEndpointBase {

    private static _instance: PartTypeService;
    private listCache: Map<string, Observable<Array<PartTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PartTypeBasicListData>>>;
    private recordCache: Map<string, Observable<PartTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private brickPartService: BrickPartService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PartTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PartTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<PartTypeData>>();

        PartTypeService._instance = this;
    }

    public static get Instance(): PartTypeService {
      return PartTypeService._instance;
    }


    public ClearListCaches(config: PartTypeQueryParameters | null = null) {

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


    public ConvertToPartTypeSubmitData(data: PartTypeData): PartTypeSubmitData {

        let output = new PartTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.isUserVisible = data.isUserVisible;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPartType(id: bigint | number, includeRelations: boolean = true) : Observable<PartTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const partType$ = this.requestPartType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PartType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, partType$);

            return partType$;
        }

        return this.recordCache.get(configHash) as Observable<PartTypeData>;
    }

    private requestPartType(id: bigint | number, includeRelations: boolean = true) : Observable<PartTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PartTypeData>(this.baseUrl + 'api/PartType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePartType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPartType(id, includeRelations));
            }));
    }

    public GetPartTypeList(config: PartTypeQueryParameters | any = null) : Observable<Array<PartTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const partTypeList$ = this.requestPartTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PartType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, partTypeList$);

            return partTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PartTypeData>>;
    }


    private requestPartTypeList(config: PartTypeQueryParameters | any) : Observable <Array<PartTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PartTypeData>>(this.baseUrl + 'api/PartTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePartTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPartTypeList(config));
            }));
    }

    public GetPartTypesRowCount(config: PartTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const partTypesRowCount$ = this.requestPartTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PartTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, partTypesRowCount$);

            return partTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPartTypesRowCount(config: PartTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PartTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPartTypesRowCount(config));
            }));
    }

    public GetPartTypesBasicListData(config: PartTypeQueryParameters | any = null) : Observable<Array<PartTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const partTypesBasicListData$ = this.requestPartTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PartTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, partTypesBasicListData$);

            return partTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PartTypeBasicListData>>;
    }


    private requestPartTypesBasicListData(config: PartTypeQueryParameters | any) : Observable<Array<PartTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PartTypeBasicListData>>(this.baseUrl + 'api/PartTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPartTypesBasicListData(config));
            }));

    }


    public PutPartType(id: bigint | number, partType: PartTypeSubmitData) : Observable<PartTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PartTypeData>(this.baseUrl + 'api/PartType/' + id.toString(), partType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePartType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPartType(id, partType));
            }));
    }


    public PostPartType(partType: PartTypeSubmitData) : Observable<PartTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PartTypeData>(this.baseUrl + 'api/PartType', partType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePartType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPartType(partType));
            }));
    }

  
    public DeletePartType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PartType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePartType(id));
            }));
    }


    private getConfigHash(config: PartTypeQueryParameters | any): string {

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

    public userIsBMCPartTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCPartTypeReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.PartTypes
        //
        if (userIsBMCPartTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCPartTypeReader = user.readPermission >= 1;
            } else {
                userIsBMCPartTypeReader = false;
            }
        }

        return userIsBMCPartTypeReader;
    }


    public userIsBMCPartTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCPartTypeWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.PartTypes
        //
        if (userIsBMCPartTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCPartTypeWriter = user.writePermission >= 255;
          } else {
            userIsBMCPartTypeWriter = false;
          }      
        }

        return userIsBMCPartTypeWriter;
    }

    public GetBrickPartsForPartType(partTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickPartData[]> {
        return this.brickPartService.GetBrickPartList({
            partTypeId: partTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PartTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PartTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PartTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePartType(raw: any): PartTypeData {
    if (!raw) return raw;

    //
    // Create a PartTypeData object instance with correct prototype
    //
    const revived = Object.create(PartTypeData.prototype) as PartTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._brickParts = null;
    (revived as any)._brickPartsPromise = null;
    (revived as any)._brickPartsSubject = new BehaviorSubject<BrickPartData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadPartTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).BrickParts$ = (revived as any)._brickPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickParts === null && (revived as any)._brickPartsPromise === null) {
                (revived as any).loadBrickParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._brickPartsCount$ = null;



    return revived;
  }

  private RevivePartTypeList(rawList: any[]): PartTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePartType(raw));
  }

}
