/*

   GENERATED SERVICE FOR THE COLOURFINISH TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ColourFinish table.

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
import { BrickColourService, BrickColourData } from './brick-colour.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ColourFinishQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    requiresEnvironmentMap: boolean | null | undefined = null;
    isMatte: boolean | null | undefined = null;
    defaultAlpha: bigint | number | null | undefined = null;
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
export class ColourFinishSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    requiresEnvironmentMap!: boolean;
    isMatte!: boolean;
    defaultAlpha: bigint | number | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ColourFinishBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ColourFinishChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `colourFinish.ColourFinishChildren$` — use with `| async` in templates
//        • Promise:    `colourFinish.ColourFinishChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="colourFinish.ColourFinishChildren$ | async"`), or
//        • Access the promise getter (`colourFinish.ColourFinishChildren` or `await colourFinish.ColourFinishChildren`)
//    - Simply reading `colourFinish.ColourFinishChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await colourFinish.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ColourFinishData {
    id!: bigint | number;
    name!: string;
    description!: string;
    requiresEnvironmentMap!: boolean;
    isMatte!: boolean;
    defaultAlpha!: bigint | number;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _brickColours: BrickColourData[] | null = null;
    private _brickColoursPromise: Promise<BrickColourData[]> | null  = null;
    private _brickColoursSubject = new BehaviorSubject<BrickColourData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public BrickColours$ = this._brickColoursSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickColours === null && this._brickColoursPromise === null) {
            this.loadBrickColours(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public BrickColoursCount$ = BrickColourService.Instance.GetBrickColoursRowCount({colourFinishId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ColourFinishData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.colourFinish.Reload();
  //
  //  Non Async:
  //
  //     colourFinish[0].Reload().then(x => {
  //        this.colourFinish = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ColourFinishService.Instance.GetColourFinish(this.id, includeRelations)
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
     this._brickColours = null;
     this._brickColoursPromise = null;
     this._brickColoursSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the BrickColours for this ColourFinish.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.colourFinish.BrickColours.then(colourFinishes => { ... })
     *   or
     *   await this.colourFinish.colourFinishes
     *
    */
    public get BrickColours(): Promise<BrickColourData[]> {
        if (this._brickColours !== null) {
            return Promise.resolve(this._brickColours);
        }

        if (this._brickColoursPromise !== null) {
            return this._brickColoursPromise;
        }

        // Start the load
        this.loadBrickColours();

        return this._brickColoursPromise!;
    }



    private loadBrickColours(): void {

        this._brickColoursPromise = lastValueFrom(
            ColourFinishService.Instance.GetBrickColoursForColourFinish(this.id)
        )
        .then(BrickColours => {
            this._brickColours = BrickColours ?? [];
            this._brickColoursSubject.next(this._brickColours);
            return this._brickColours;
         })
        .catch(err => {
            this._brickColours = [];
            this._brickColoursSubject.next(this._brickColours);
            throw err;
        })
        .finally(() => {
            this._brickColoursPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BrickColour. Call after mutations to force refresh.
     */
    public ClearBrickColoursCache(): void {
        this._brickColours = null;
        this._brickColoursPromise = null;
        this._brickColoursSubject.next(this._brickColours);      // Emit to observable
    }

    public get HasBrickColours(): Promise<boolean> {
        return this.BrickColours.then(brickColours => brickColours.length > 0);
    }




    /**
     * Updates the state of this ColourFinishData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ColourFinishData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ColourFinishSubmitData {
        return ColourFinishService.Instance.ConvertToColourFinishSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ColourFinishService extends SecureEndpointBase {

    private static _instance: ColourFinishService;
    private listCache: Map<string, Observable<Array<ColourFinishData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ColourFinishBasicListData>>>;
    private recordCache: Map<string, Observable<ColourFinishData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private brickColourService: BrickColourService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ColourFinishData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ColourFinishBasicListData>>>();
        this.recordCache = new Map<string, Observable<ColourFinishData>>();

        ColourFinishService._instance = this;
    }

    public static get Instance(): ColourFinishService {
      return ColourFinishService._instance;
    }


    public ClearListCaches(config: ColourFinishQueryParameters | null = null) {

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


    public ConvertToColourFinishSubmitData(data: ColourFinishData): ColourFinishSubmitData {

        let output = new ColourFinishSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.requiresEnvironmentMap = data.requiresEnvironmentMap;
        output.isMatte = data.isMatte;
        output.defaultAlpha = data.defaultAlpha;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetColourFinish(id: bigint | number, includeRelations: boolean = true) : Observable<ColourFinishData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const colourFinish$ = this.requestColourFinish(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ColourFinish", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, colourFinish$);

            return colourFinish$;
        }

        return this.recordCache.get(configHash) as Observable<ColourFinishData>;
    }

    private requestColourFinish(id: bigint | number, includeRelations: boolean = true) : Observable<ColourFinishData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ColourFinishData>(this.baseUrl + 'api/ColourFinish/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveColourFinish(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestColourFinish(id, includeRelations));
            }));
    }

    public GetColourFinishList(config: ColourFinishQueryParameters | any = null) : Observable<Array<ColourFinishData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const colourFinishList$ = this.requestColourFinishList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ColourFinish list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, colourFinishList$);

            return colourFinishList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ColourFinishData>>;
    }


    private requestColourFinishList(config: ColourFinishQueryParameters | any) : Observable <Array<ColourFinishData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ColourFinishData>>(this.baseUrl + 'api/ColourFinishes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveColourFinishList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestColourFinishList(config));
            }));
    }

    public GetColourFinishesRowCount(config: ColourFinishQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const colourFinishesRowCount$ = this.requestColourFinishesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ColourFinishes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, colourFinishesRowCount$);

            return colourFinishesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestColourFinishesRowCount(config: ColourFinishQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ColourFinishes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestColourFinishesRowCount(config));
            }));
    }

    public GetColourFinishesBasicListData(config: ColourFinishQueryParameters | any = null) : Observable<Array<ColourFinishBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const colourFinishesBasicListData$ = this.requestColourFinishesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ColourFinishes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, colourFinishesBasicListData$);

            return colourFinishesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ColourFinishBasicListData>>;
    }


    private requestColourFinishesBasicListData(config: ColourFinishQueryParameters | any) : Observable<Array<ColourFinishBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ColourFinishBasicListData>>(this.baseUrl + 'api/ColourFinishes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestColourFinishesBasicListData(config));
            }));

    }


    public PutColourFinish(id: bigint | number, colourFinish: ColourFinishSubmitData) : Observable<ColourFinishData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ColourFinishData>(this.baseUrl + 'api/ColourFinish/' + id.toString(), colourFinish, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveColourFinish(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutColourFinish(id, colourFinish));
            }));
    }


    public PostColourFinish(colourFinish: ColourFinishSubmitData) : Observable<ColourFinishData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ColourFinishData>(this.baseUrl + 'api/ColourFinish', colourFinish, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveColourFinish(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostColourFinish(colourFinish));
            }));
    }

  
    public DeleteColourFinish(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ColourFinish/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteColourFinish(id));
            }));
    }


    private getConfigHash(config: ColourFinishQueryParameters | any): string {

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

    public userIsBMCColourFinishReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCColourFinishReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ColourFinishes
        //
        if (userIsBMCColourFinishReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCColourFinishReader = user.readPermission >= 1;
            } else {
                userIsBMCColourFinishReader = false;
            }
        }

        return userIsBMCColourFinishReader;
    }


    public userIsBMCColourFinishWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCColourFinishWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ColourFinishes
        //
        if (userIsBMCColourFinishWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCColourFinishWriter = user.writePermission >= 255;
          } else {
            userIsBMCColourFinishWriter = false;
          }      
        }

        return userIsBMCColourFinishWriter;
    }

    public GetBrickColoursForColourFinish(colourFinishId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickColourData[]> {
        return this.brickColourService.GetBrickColourList({
            colourFinishId: colourFinishId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ColourFinishData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ColourFinishData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ColourFinishTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveColourFinish(raw: any): ColourFinishData {
    if (!raw) return raw;

    //
    // Create a ColourFinishData object instance with correct prototype
    //
    const revived = Object.create(ColourFinishData.prototype) as ColourFinishData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._brickColours = null;
    (revived as any)._brickColoursPromise = null;
    (revived as any)._brickColoursSubject = new BehaviorSubject<BrickColourData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadColourFinishXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).BrickColours$ = (revived as any)._brickColoursSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickColours === null && (revived as any)._brickColoursPromise === null) {
                (revived as any).loadBrickColours();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).BrickColoursCount$ = BrickColourService.Instance.GetBrickColoursRowCount({colourFinishId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveColourFinishList(rawList: any[]): ColourFinishData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveColourFinish(raw));
  }

}
