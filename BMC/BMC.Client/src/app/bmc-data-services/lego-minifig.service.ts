/*

   GENERATED SERVICE FOR THE LEGOMINIFIG TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the LegoMinifig table.

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
import { LegoSetMinifigService, LegoSetMinifigData } from './lego-set-minifig.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class LegoMinifigQueryParameters {
    name: string | null | undefined = null;
    figNumber: string | null | undefined = null;
    partCount: bigint | number | null | undefined = null;
    imageUrl: string | null | undefined = null;
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
export class LegoMinifigSubmitData {
    id!: bigint | number;
    name!: string;
    figNumber!: string;
    partCount!: bigint | number;
    imageUrl: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class LegoMinifigBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. LegoMinifigChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `legoMinifig.LegoMinifigChildren$` — use with `| async` in templates
//        • Promise:    `legoMinifig.LegoMinifigChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="legoMinifig.LegoMinifigChildren$ | async"`), or
//        • Access the promise getter (`legoMinifig.LegoMinifigChildren` or `await legoMinifig.LegoMinifigChildren`)
//    - Simply reading `legoMinifig.LegoMinifigChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await legoMinifig.Reload()` to refresh the entire object and clear all lazy caches.
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
export class LegoMinifigData {
    id!: bigint | number;
    name!: string;
    figNumber!: string;
    partCount!: bigint | number;
    imageUrl!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _legoSetMinifigs: LegoSetMinifigData[] | null = null;
    private _legoSetMinifigsPromise: Promise<LegoSetMinifigData[]> | null  = null;
    private _legoSetMinifigsSubject = new BehaviorSubject<LegoSetMinifigData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public LegoSetMinifigs$ = this._legoSetMinifigsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._legoSetMinifigs === null && this._legoSetMinifigsPromise === null) {
            this.loadLegoSetMinifigs(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _legoSetMinifigsCount$: Observable<bigint | number> | null = null;
    public get LegoSetMinifigsCount$(): Observable<bigint | number> {
        if (this._legoSetMinifigsCount$ === null) {
            this._legoSetMinifigsCount$ = LegoSetMinifigService.Instance.GetLegoSetMinifigsRowCount({legoMinifigId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._legoSetMinifigsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any LegoMinifigData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.legoMinifig.Reload();
  //
  //  Non Async:
  //
  //     legoMinifig[0].Reload().then(x => {
  //        this.legoMinifig = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      LegoMinifigService.Instance.GetLegoMinifig(this.id, includeRelations)
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
     this._legoSetMinifigs = null;
     this._legoSetMinifigsPromise = null;
     this._legoSetMinifigsSubject.next(null);
     this._legoSetMinifigsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the LegoSetMinifigs for this LegoMinifig.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.legoMinifig.LegoSetMinifigs.then(legoMinifigs => { ... })
     *   or
     *   await this.legoMinifig.legoMinifigs
     *
    */
    public get LegoSetMinifigs(): Promise<LegoSetMinifigData[]> {
        if (this._legoSetMinifigs !== null) {
            return Promise.resolve(this._legoSetMinifigs);
        }

        if (this._legoSetMinifigsPromise !== null) {
            return this._legoSetMinifigsPromise;
        }

        // Start the load
        this.loadLegoSetMinifigs();

        return this._legoSetMinifigsPromise!;
    }



    private loadLegoSetMinifigs(): void {

        this._legoSetMinifigsPromise = lastValueFrom(
            LegoMinifigService.Instance.GetLegoSetMinifigsForLegoMinifig(this.id)
        )
        .then(LegoSetMinifigs => {
            this._legoSetMinifigs = LegoSetMinifigs ?? [];
            this._legoSetMinifigsSubject.next(this._legoSetMinifigs);
            return this._legoSetMinifigs;
         })
        .catch(err => {
            this._legoSetMinifigs = [];
            this._legoSetMinifigsSubject.next(this._legoSetMinifigs);
            throw err;
        })
        .finally(() => {
            this._legoSetMinifigsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached LegoSetMinifig. Call after mutations to force refresh.
     */
    public ClearLegoSetMinifigsCache(): void {
        this._legoSetMinifigs = null;
        this._legoSetMinifigsPromise = null;
        this._legoSetMinifigsSubject.next(this._legoSetMinifigs);      // Emit to observable
    }

    public get HasLegoSetMinifigs(): Promise<boolean> {
        return this.LegoSetMinifigs.then(legoSetMinifigs => legoSetMinifigs.length > 0);
    }




    /**
     * Updates the state of this LegoMinifigData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this LegoMinifigData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): LegoMinifigSubmitData {
        return LegoMinifigService.Instance.ConvertToLegoMinifigSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class LegoMinifigService extends SecureEndpointBase {

    private static _instance: LegoMinifigService;
    private listCache: Map<string, Observable<Array<LegoMinifigData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<LegoMinifigBasicListData>>>;
    private recordCache: Map<string, Observable<LegoMinifigData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private legoSetMinifigService: LegoSetMinifigService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<LegoMinifigData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<LegoMinifigBasicListData>>>();
        this.recordCache = new Map<string, Observable<LegoMinifigData>>();

        LegoMinifigService._instance = this;
    }

    public static get Instance(): LegoMinifigService {
      return LegoMinifigService._instance;
    }


    public ClearListCaches(config: LegoMinifigQueryParameters | null = null) {

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


    public ConvertToLegoMinifigSubmitData(data: LegoMinifigData): LegoMinifigSubmitData {

        let output = new LegoMinifigSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.figNumber = data.figNumber;
        output.partCount = data.partCount;
        output.imageUrl = data.imageUrl;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetLegoMinifig(id: bigint | number, includeRelations: boolean = true) : Observable<LegoMinifigData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const legoMinifig$ = this.requestLegoMinifig(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LegoMinifig", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, legoMinifig$);

            return legoMinifig$;
        }

        return this.recordCache.get(configHash) as Observable<LegoMinifigData>;
    }

    private requestLegoMinifig(id: bigint | number, includeRelations: boolean = true) : Observable<LegoMinifigData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<LegoMinifigData>(this.baseUrl + 'api/LegoMinifig/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveLegoMinifig(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestLegoMinifig(id, includeRelations));
            }));
    }

    public GetLegoMinifigList(config: LegoMinifigQueryParameters | any = null) : Observable<Array<LegoMinifigData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const legoMinifigList$ = this.requestLegoMinifigList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LegoMinifig list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, legoMinifigList$);

            return legoMinifigList$;
        }

        return this.listCache.get(configHash) as Observable<Array<LegoMinifigData>>;
    }


    private requestLegoMinifigList(config: LegoMinifigQueryParameters | any) : Observable <Array<LegoMinifigData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LegoMinifigData>>(this.baseUrl + 'api/LegoMinifigs', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveLegoMinifigList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestLegoMinifigList(config));
            }));
    }

    public GetLegoMinifigsRowCount(config: LegoMinifigQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const legoMinifigsRowCount$ = this.requestLegoMinifigsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LegoMinifigs row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, legoMinifigsRowCount$);

            return legoMinifigsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestLegoMinifigsRowCount(config: LegoMinifigQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/LegoMinifigs/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLegoMinifigsRowCount(config));
            }));
    }

    public GetLegoMinifigsBasicListData(config: LegoMinifigQueryParameters | any = null) : Observable<Array<LegoMinifigBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const legoMinifigsBasicListData$ = this.requestLegoMinifigsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LegoMinifigs basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, legoMinifigsBasicListData$);

            return legoMinifigsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<LegoMinifigBasicListData>>;
    }


    private requestLegoMinifigsBasicListData(config: LegoMinifigQueryParameters | any) : Observable<Array<LegoMinifigBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LegoMinifigBasicListData>>(this.baseUrl + 'api/LegoMinifigs/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLegoMinifigsBasicListData(config));
            }));

    }


    public PutLegoMinifig(id: bigint | number, legoMinifig: LegoMinifigSubmitData) : Observable<LegoMinifigData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<LegoMinifigData>(this.baseUrl + 'api/LegoMinifig/' + id.toString(), legoMinifig, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLegoMinifig(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutLegoMinifig(id, legoMinifig));
            }));
    }


    public PostLegoMinifig(legoMinifig: LegoMinifigSubmitData) : Observable<LegoMinifigData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<LegoMinifigData>(this.baseUrl + 'api/LegoMinifig', legoMinifig, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLegoMinifig(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostLegoMinifig(legoMinifig));
            }));
    }

  
    public DeleteLegoMinifig(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/LegoMinifig/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteLegoMinifig(id));
            }));
    }


    private getConfigHash(config: LegoMinifigQueryParameters | any): string {

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

    public userIsBMCLegoMinifigReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCLegoMinifigReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.LegoMinifigs
        //
        if (userIsBMCLegoMinifigReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCLegoMinifigReader = user.readPermission >= 1;
            } else {
                userIsBMCLegoMinifigReader = false;
            }
        }

        return userIsBMCLegoMinifigReader;
    }


    public userIsBMCLegoMinifigWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCLegoMinifigWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.LegoMinifigs
        //
        if (userIsBMCLegoMinifigWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCLegoMinifigWriter = user.writePermission >= 255;
          } else {
            userIsBMCLegoMinifigWriter = false;
          }      
        }

        return userIsBMCLegoMinifigWriter;
    }

    public GetLegoSetMinifigsForLegoMinifig(legoMinifigId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<LegoSetMinifigData[]> {
        return this.legoSetMinifigService.GetLegoSetMinifigList({
            legoMinifigId: legoMinifigId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full LegoMinifigData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the LegoMinifigData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when LegoMinifigTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveLegoMinifig(raw: any): LegoMinifigData {
    if (!raw) return raw;

    //
    // Create a LegoMinifigData object instance with correct prototype
    //
    const revived = Object.create(LegoMinifigData.prototype) as LegoMinifigData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._legoSetMinifigs = null;
    (revived as any)._legoSetMinifigsPromise = null;
    (revived as any)._legoSetMinifigsSubject = new BehaviorSubject<LegoSetMinifigData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadLegoMinifigXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).LegoSetMinifigs$ = (revived as any)._legoSetMinifigsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._legoSetMinifigs === null && (revived as any)._legoSetMinifigsPromise === null) {
                (revived as any).loadLegoSetMinifigs();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._legoSetMinifigsCount$ = null;



    return revived;
  }

  private ReviveLegoMinifigList(rawList: any[]): LegoMinifigData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveLegoMinifig(raw));
  }

}
