/*

   GENERATED SERVICE FOR THE BRICKCATEGORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickCategory table.

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
export class BrickCategoryQueryParameters {
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
export class BrickCategorySubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BrickCategoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickCategoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickCategory.BrickCategoryChildren$` — use with `| async` in templates
//        • Promise:    `brickCategory.BrickCategoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickCategory.BrickCategoryChildren$ | async"`), or
//        • Access the promise getter (`brickCategory.BrickCategoryChildren` or `await brickCategory.BrickCategoryChildren`)
//    - Simply reading `brickCategory.BrickCategoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickCategory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickCategoryData {
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

  
    public BrickPartsCount$ = BrickPartService.Instance.GetBrickPartsRowCount({brickCategoryId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any BrickCategoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickCategory.Reload();
  //
  //  Non Async:
  //
  //     brickCategory[0].Reload().then(x => {
  //        this.brickCategory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickCategoryService.Instance.GetBrickCategory(this.id, includeRelations)
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

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the BrickParts for this BrickCategory.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickCategory.BrickParts.then(brickCategories => { ... })
     *   or
     *   await this.brickCategory.brickCategories
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
            BrickCategoryService.Instance.GetBrickPartsForBrickCategory(this.id)
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
     * Updates the state of this BrickCategoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickCategoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickCategorySubmitData {
        return BrickCategoryService.Instance.ConvertToBrickCategorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickCategoryService extends SecureEndpointBase {

    private static _instance: BrickCategoryService;
    private listCache: Map<string, Observable<Array<BrickCategoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickCategoryBasicListData>>>;
    private recordCache: Map<string, Observable<BrickCategoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private brickPartService: BrickPartService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickCategoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickCategoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickCategoryData>>();

        BrickCategoryService._instance = this;
    }

    public static get Instance(): BrickCategoryService {
      return BrickCategoryService._instance;
    }


    public ClearListCaches(config: BrickCategoryQueryParameters | null = null) {

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


    public ConvertToBrickCategorySubmitData(data: BrickCategoryData): BrickCategorySubmitData {

        let output = new BrickCategorySubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBrickCategory(id: bigint | number, includeRelations: boolean = true) : Observable<BrickCategoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickCategory$ = this.requestBrickCategory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickCategory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickCategory$);

            return brickCategory$;
        }

        return this.recordCache.get(configHash) as Observable<BrickCategoryData>;
    }

    private requestBrickCategory(id: bigint | number, includeRelations: boolean = true) : Observable<BrickCategoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickCategoryData>(this.baseUrl + 'api/BrickCategory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickCategory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickCategory(id, includeRelations));
            }));
    }

    public GetBrickCategoryList(config: BrickCategoryQueryParameters | any = null) : Observable<Array<BrickCategoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickCategoryList$ = this.requestBrickCategoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickCategory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickCategoryList$);

            return brickCategoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickCategoryData>>;
    }


    private requestBrickCategoryList(config: BrickCategoryQueryParameters | any) : Observable <Array<BrickCategoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickCategoryData>>(this.baseUrl + 'api/BrickCategories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickCategoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickCategoryList(config));
            }));
    }

    public GetBrickCategoriesRowCount(config: BrickCategoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickCategoriesRowCount$ = this.requestBrickCategoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickCategories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickCategoriesRowCount$);

            return brickCategoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickCategoriesRowCount(config: BrickCategoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickCategories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickCategoriesRowCount(config));
            }));
    }

    public GetBrickCategoriesBasicListData(config: BrickCategoryQueryParameters | any = null) : Observable<Array<BrickCategoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickCategoriesBasicListData$ = this.requestBrickCategoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickCategories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickCategoriesBasicListData$);

            return brickCategoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickCategoryBasicListData>>;
    }


    private requestBrickCategoriesBasicListData(config: BrickCategoryQueryParameters | any) : Observable<Array<BrickCategoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickCategoryBasicListData>>(this.baseUrl + 'api/BrickCategories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickCategoriesBasicListData(config));
            }));

    }


    public PutBrickCategory(id: bigint | number, brickCategory: BrickCategorySubmitData) : Observable<BrickCategoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickCategoryData>(this.baseUrl + 'api/BrickCategory/' + id.toString(), brickCategory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickCategory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickCategory(id, brickCategory));
            }));
    }


    public PostBrickCategory(brickCategory: BrickCategorySubmitData) : Observable<BrickCategoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickCategoryData>(this.baseUrl + 'api/BrickCategory', brickCategory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickCategory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickCategory(brickCategory));
            }));
    }

  
    public DeleteBrickCategory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickCategory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickCategory(id));
            }));
    }


    private getConfigHash(config: BrickCategoryQueryParameters | any): string {

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

    public userIsBMCBrickCategoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickCategoryReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickCategories
        //
        if (userIsBMCBrickCategoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickCategoryReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickCategoryReader = false;
            }
        }

        return userIsBMCBrickCategoryReader;
    }


    public userIsBMCBrickCategoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickCategoryWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickCategories
        //
        if (userIsBMCBrickCategoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickCategoryWriter = user.writePermission >= 255;
          } else {
            userIsBMCBrickCategoryWriter = false;
          }      
        }

        return userIsBMCBrickCategoryWriter;
    }

    public GetBrickPartsForBrickCategory(brickCategoryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickPartData[]> {
        return this.brickPartService.GetBrickPartList({
            brickCategoryId: brickCategoryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full BrickCategoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickCategoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickCategoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickCategory(raw: any): BrickCategoryData {
    if (!raw) return raw;

    //
    // Create a BrickCategoryData object instance with correct prototype
    //
    const revived = Object.create(BrickCategoryData.prototype) as BrickCategoryData;

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
    // 2. But private methods (loadBrickCategoryXYZ, etc.) are not accessible via the typed variable
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

    (revived as any).BrickPartsCount$ = BrickPartService.Instance.GetBrickPartsRowCount({brickCategoryId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveBrickCategoryList(rawList: any[]): BrickCategoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickCategory(raw));
  }

}
