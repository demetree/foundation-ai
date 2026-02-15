/*

   GENERATED SERVICE FOR THE LEGOSET TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the LegoSet table.

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
import { LegoThemeData } from './lego-theme.service';
import { LegoSetPartService, LegoSetPartData } from './lego-set-part.service';
import { UserCollectionSetImportService, UserCollectionSetImportData } from './user-collection-set-import.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class LegoSetQueryParameters {
    name: string | null | undefined = null;
    setNumber: string | null | undefined = null;
    year: bigint | number | null | undefined = null;
    partCount: bigint | number | null | undefined = null;
    legoThemeId: bigint | number | null | undefined = null;
    imageUrl: string | null | undefined = null;
    brickLinkUrl: string | null | undefined = null;
    rebrickableUrl: string | null | undefined = null;
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
export class LegoSetSubmitData {
    id!: bigint | number;
    name!: string;
    setNumber!: string;
    year!: bigint | number;
    partCount!: bigint | number;
    legoThemeId: bigint | number | null = null;
    imageUrl: string | null = null;
    brickLinkUrl: string | null = null;
    rebrickableUrl: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class LegoSetBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. LegoSetChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `legoSet.LegoSetChildren$` — use with `| async` in templates
//        • Promise:    `legoSet.LegoSetChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="legoSet.LegoSetChildren$ | async"`), or
//        • Access the promise getter (`legoSet.LegoSetChildren` or `await legoSet.LegoSetChildren`)
//    - Simply reading `legoSet.LegoSetChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await legoSet.Reload()` to refresh the entire object and clear all lazy caches.
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
export class LegoSetData {
    id!: bigint | number;
    name!: string;
    setNumber!: string;
    year!: bigint | number;
    partCount!: bigint | number;
    legoThemeId!: bigint | number;
    imageUrl!: string | null;
    brickLinkUrl!: string | null;
    rebrickableUrl!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    legoTheme: LegoThemeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _legoSetParts: LegoSetPartData[] | null = null;
    private _legoSetPartsPromise: Promise<LegoSetPartData[]> | null  = null;
    private _legoSetPartsSubject = new BehaviorSubject<LegoSetPartData[] | null>(null);

                
    private _userCollectionSetImports: UserCollectionSetImportData[] | null = null;
    private _userCollectionSetImportsPromise: Promise<UserCollectionSetImportData[]> | null  = null;
    private _userCollectionSetImportsSubject = new BehaviorSubject<UserCollectionSetImportData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public LegoSetParts$ = this._legoSetPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._legoSetParts === null && this._legoSetPartsPromise === null) {
            this.loadLegoSetParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public LegoSetPartsCount$ = LegoSetPartService.Instance.GetLegoSetPartsRowCount({legoSetId: this.id,
      active: true,
      deleted: false
    });



    public UserCollectionSetImports$ = this._userCollectionSetImportsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userCollectionSetImports === null && this._userCollectionSetImportsPromise === null) {
            this.loadUserCollectionSetImports(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public UserCollectionSetImportsCount$ = UserCollectionSetImportService.Instance.GetUserCollectionSetImportsRowCount({legoSetId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any LegoSetData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.legoSet.Reload();
  //
  //  Non Async:
  //
  //     legoSet[0].Reload().then(x => {
  //        this.legoSet = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      LegoSetService.Instance.GetLegoSet(this.id, includeRelations)
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
     this._legoSetParts = null;
     this._legoSetPartsPromise = null;
     this._legoSetPartsSubject.next(null);

     this._userCollectionSetImports = null;
     this._userCollectionSetImportsPromise = null;
     this._userCollectionSetImportsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the LegoSetParts for this LegoSet.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.legoSet.LegoSetParts.then(legoSets => { ... })
     *   or
     *   await this.legoSet.legoSets
     *
    */
    public get LegoSetParts(): Promise<LegoSetPartData[]> {
        if (this._legoSetParts !== null) {
            return Promise.resolve(this._legoSetParts);
        }

        if (this._legoSetPartsPromise !== null) {
            return this._legoSetPartsPromise;
        }

        // Start the load
        this.loadLegoSetParts();

        return this._legoSetPartsPromise!;
    }



    private loadLegoSetParts(): void {

        this._legoSetPartsPromise = lastValueFrom(
            LegoSetService.Instance.GetLegoSetPartsForLegoSet(this.id)
        )
        .then(LegoSetParts => {
            this._legoSetParts = LegoSetParts ?? [];
            this._legoSetPartsSubject.next(this._legoSetParts);
            return this._legoSetParts;
         })
        .catch(err => {
            this._legoSetParts = [];
            this._legoSetPartsSubject.next(this._legoSetParts);
            throw err;
        })
        .finally(() => {
            this._legoSetPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached LegoSetPart. Call after mutations to force refresh.
     */
    public ClearLegoSetPartsCache(): void {
        this._legoSetParts = null;
        this._legoSetPartsPromise = null;
        this._legoSetPartsSubject.next(this._legoSetParts);      // Emit to observable
    }

    public get HasLegoSetParts(): Promise<boolean> {
        return this.LegoSetParts.then(legoSetParts => legoSetParts.length > 0);
    }


    /**
     *
     * Gets the UserCollectionSetImports for this LegoSet.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.legoSet.UserCollectionSetImports.then(legoSets => { ... })
     *   or
     *   await this.legoSet.legoSets
     *
    */
    public get UserCollectionSetImports(): Promise<UserCollectionSetImportData[]> {
        if (this._userCollectionSetImports !== null) {
            return Promise.resolve(this._userCollectionSetImports);
        }

        if (this._userCollectionSetImportsPromise !== null) {
            return this._userCollectionSetImportsPromise;
        }

        // Start the load
        this.loadUserCollectionSetImports();

        return this._userCollectionSetImportsPromise!;
    }



    private loadUserCollectionSetImports(): void {

        this._userCollectionSetImportsPromise = lastValueFrom(
            LegoSetService.Instance.GetUserCollectionSetImportsForLegoSet(this.id)
        )
        .then(UserCollectionSetImports => {
            this._userCollectionSetImports = UserCollectionSetImports ?? [];
            this._userCollectionSetImportsSubject.next(this._userCollectionSetImports);
            return this._userCollectionSetImports;
         })
        .catch(err => {
            this._userCollectionSetImports = [];
            this._userCollectionSetImportsSubject.next(this._userCollectionSetImports);
            throw err;
        })
        .finally(() => {
            this._userCollectionSetImportsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserCollectionSetImport. Call after mutations to force refresh.
     */
    public ClearUserCollectionSetImportsCache(): void {
        this._userCollectionSetImports = null;
        this._userCollectionSetImportsPromise = null;
        this._userCollectionSetImportsSubject.next(this._userCollectionSetImports);      // Emit to observable
    }

    public get HasUserCollectionSetImports(): Promise<boolean> {
        return this.UserCollectionSetImports.then(userCollectionSetImports => userCollectionSetImports.length > 0);
    }




    /**
     * Updates the state of this LegoSetData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this LegoSetData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): LegoSetSubmitData {
        return LegoSetService.Instance.ConvertToLegoSetSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class LegoSetService extends SecureEndpointBase {

    private static _instance: LegoSetService;
    private listCache: Map<string, Observable<Array<LegoSetData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<LegoSetBasicListData>>>;
    private recordCache: Map<string, Observable<LegoSetData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private legoSetPartService: LegoSetPartService,
        private userCollectionSetImportService: UserCollectionSetImportService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<LegoSetData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<LegoSetBasicListData>>>();
        this.recordCache = new Map<string, Observable<LegoSetData>>();

        LegoSetService._instance = this;
    }

    public static get Instance(): LegoSetService {
      return LegoSetService._instance;
    }


    public ClearListCaches(config: LegoSetQueryParameters | null = null) {

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


    public ConvertToLegoSetSubmitData(data: LegoSetData): LegoSetSubmitData {

        let output = new LegoSetSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.setNumber = data.setNumber;
        output.year = data.year;
        output.partCount = data.partCount;
        output.legoThemeId = data.legoThemeId;
        output.imageUrl = data.imageUrl;
        output.brickLinkUrl = data.brickLinkUrl;
        output.rebrickableUrl = data.rebrickableUrl;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetLegoSet(id: bigint | number, includeRelations: boolean = true) : Observable<LegoSetData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const legoSet$ = this.requestLegoSet(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LegoSet", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, legoSet$);

            return legoSet$;
        }

        return this.recordCache.get(configHash) as Observable<LegoSetData>;
    }

    private requestLegoSet(id: bigint | number, includeRelations: boolean = true) : Observable<LegoSetData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<LegoSetData>(this.baseUrl + 'api/LegoSet/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveLegoSet(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSet(id, includeRelations));
            }));
    }

    public GetLegoSetList(config: LegoSetQueryParameters | any = null) : Observable<Array<LegoSetData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const legoSetList$ = this.requestLegoSetList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LegoSet list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, legoSetList$);

            return legoSetList$;
        }

        return this.listCache.get(configHash) as Observable<Array<LegoSetData>>;
    }


    private requestLegoSetList(config: LegoSetQueryParameters | any) : Observable <Array<LegoSetData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LegoSetData>>(this.baseUrl + 'api/LegoSets', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveLegoSetList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetList(config));
            }));
    }

    public GetLegoSetsRowCount(config: LegoSetQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const legoSetsRowCount$ = this.requestLegoSetsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LegoSets row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, legoSetsRowCount$);

            return legoSetsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestLegoSetsRowCount(config: LegoSetQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/LegoSets/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetsRowCount(config));
            }));
    }

    public GetLegoSetsBasicListData(config: LegoSetQueryParameters | any = null) : Observable<Array<LegoSetBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const legoSetsBasicListData$ = this.requestLegoSetsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LegoSets basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, legoSetsBasicListData$);

            return legoSetsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<LegoSetBasicListData>>;
    }


    private requestLegoSetsBasicListData(config: LegoSetQueryParameters | any) : Observable<Array<LegoSetBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LegoSetBasicListData>>(this.baseUrl + 'api/LegoSets/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetsBasicListData(config));
            }));

    }


    public PutLegoSet(id: bigint | number, legoSet: LegoSetSubmitData) : Observable<LegoSetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<LegoSetData>(this.baseUrl + 'api/LegoSet/' + id.toString(), legoSet, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLegoSet(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutLegoSet(id, legoSet));
            }));
    }


    public PostLegoSet(legoSet: LegoSetSubmitData) : Observable<LegoSetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<LegoSetData>(this.baseUrl + 'api/LegoSet', legoSet, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLegoSet(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostLegoSet(legoSet));
            }));
    }

  
    public DeleteLegoSet(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/LegoSet/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteLegoSet(id));
            }));
    }


    private getConfigHash(config: LegoSetQueryParameters | any): string {

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

    public userIsBMCLegoSetReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCLegoSetReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.LegoSets
        //
        if (userIsBMCLegoSetReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCLegoSetReader = user.readPermission >= 1;
            } else {
                userIsBMCLegoSetReader = false;
            }
        }

        return userIsBMCLegoSetReader;
    }


    public userIsBMCLegoSetWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCLegoSetWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.LegoSets
        //
        if (userIsBMCLegoSetWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCLegoSetWriter = user.writePermission >= 255;
          } else {
            userIsBMCLegoSetWriter = false;
          }      
        }

        return userIsBMCLegoSetWriter;
    }

    public GetLegoSetPartsForLegoSet(legoSetId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<LegoSetPartData[]> {
        return this.legoSetPartService.GetLegoSetPartList({
            legoSetId: legoSetId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserCollectionSetImportsForLegoSet(legoSetId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserCollectionSetImportData[]> {
        return this.userCollectionSetImportService.GetUserCollectionSetImportList({
            legoSetId: legoSetId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full LegoSetData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the LegoSetData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when LegoSetTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveLegoSet(raw: any): LegoSetData {
    if (!raw) return raw;

    //
    // Create a LegoSetData object instance with correct prototype
    //
    const revived = Object.create(LegoSetData.prototype) as LegoSetData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._legoSetParts = null;
    (revived as any)._legoSetPartsPromise = null;
    (revived as any)._legoSetPartsSubject = new BehaviorSubject<LegoSetPartData[] | null>(null);

    (revived as any)._userCollectionSetImports = null;
    (revived as any)._userCollectionSetImportsPromise = null;
    (revived as any)._userCollectionSetImportsSubject = new BehaviorSubject<UserCollectionSetImportData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadLegoSetXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).LegoSetParts$ = (revived as any)._legoSetPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._legoSetParts === null && (revived as any)._legoSetPartsPromise === null) {
                (revived as any).loadLegoSetParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).LegoSetPartsCount$ = LegoSetPartService.Instance.GetLegoSetPartsRowCount({legoSetId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).UserCollectionSetImports$ = (revived as any)._userCollectionSetImportsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userCollectionSetImports === null && (revived as any)._userCollectionSetImportsPromise === null) {
                (revived as any).loadUserCollectionSetImports();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).UserCollectionSetImportsCount$ = UserCollectionSetImportService.Instance.GetUserCollectionSetImportsRowCount({legoSetId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveLegoSetList(rawList: any[]): LegoSetData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveLegoSet(raw));
  }

}
