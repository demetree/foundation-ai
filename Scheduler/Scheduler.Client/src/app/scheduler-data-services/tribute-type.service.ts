/*

   GENERATED SERVICE FOR THE TRIBUTETYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the TributeType table.

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
import { TributeService, TributeData } from './tribute.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class TributeTypeQueryParameters {
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
export class TributeTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class TributeTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TributeTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `tributeType.TributeTypeChildren$` — use with `| async` in templates
//        • Promise:    `tributeType.TributeTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="tributeType.TributeTypeChildren$ | async"`), or
//        • Access the promise getter (`tributeType.TributeTypeChildren` or `await tributeType.TributeTypeChildren`)
//    - Simply reading `tributeType.TributeTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await tributeType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TributeTypeData {
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
    private _tributes: TributeData[] | null = null;
    private _tributesPromise: Promise<TributeData[]> | null  = null;
    private _tributesSubject = new BehaviorSubject<TributeData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Tributes$ = this._tributesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._tributes === null && this._tributesPromise === null) {
            this.loadTributes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _tributesCount$: Observable<bigint | number> | null = null;
    public get TributesCount$(): Observable<bigint | number> {
        if (this._tributesCount$ === null) {
            this._tributesCount$ = TributeService.Instance.GetTributesRowCount({tributeTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._tributesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any TributeTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.tributeType.Reload();
  //
  //  Non Async:
  //
  //     tributeType[0].Reload().then(x => {
  //        this.tributeType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TributeTypeService.Instance.GetTributeType(this.id, includeRelations)
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
     this._tributes = null;
     this._tributesPromise = null;
     this._tributesSubject.next(null);
     this._tributesCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Tributes for this TributeType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.tributeType.Tributes.then(tributeTypes => { ... })
     *   or
     *   await this.tributeType.tributeTypes
     *
    */
    public get Tributes(): Promise<TributeData[]> {
        if (this._tributes !== null) {
            return Promise.resolve(this._tributes);
        }

        if (this._tributesPromise !== null) {
            return this._tributesPromise;
        }

        // Start the load
        this.loadTributes();

        return this._tributesPromise!;
    }



    private loadTributes(): void {

        this._tributesPromise = lastValueFrom(
            TributeTypeService.Instance.GetTributesForTributeType(this.id)
        )
        .then(Tributes => {
            this._tributes = Tributes ?? [];
            this._tributesSubject.next(this._tributes);
            return this._tributes;
         })
        .catch(err => {
            this._tributes = [];
            this._tributesSubject.next(this._tributes);
            throw err;
        })
        .finally(() => {
            this._tributesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Tribute. Call after mutations to force refresh.
     */
    public ClearTributesCache(): void {
        this._tributes = null;
        this._tributesPromise = null;
        this._tributesSubject.next(this._tributes);      // Emit to observable
    }

    public get HasTributes(): Promise<boolean> {
        return this.Tributes.then(tributes => tributes.length > 0);
    }




    /**
     * Updates the state of this TributeTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TributeTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TributeTypeSubmitData {
        return TributeTypeService.Instance.ConvertToTributeTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TributeTypeService extends SecureEndpointBase {

    private static _instance: TributeTypeService;
    private listCache: Map<string, Observable<Array<TributeTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TributeTypeBasicListData>>>;
    private recordCache: Map<string, Observable<TributeTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private tributeService: TributeService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TributeTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TributeTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<TributeTypeData>>();

        TributeTypeService._instance = this;
    }

    public static get Instance(): TributeTypeService {
      return TributeTypeService._instance;
    }


    public ClearListCaches(config: TributeTypeQueryParameters | null = null) {

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


    public ConvertToTributeTypeSubmitData(data: TributeTypeData): TributeTypeSubmitData {

        let output = new TributeTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetTributeType(id: bigint | number, includeRelations: boolean = true) : Observable<TributeTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const tributeType$ = this.requestTributeType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TributeType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, tributeType$);

            return tributeType$;
        }

        return this.recordCache.get(configHash) as Observable<TributeTypeData>;
    }

    private requestTributeType(id: bigint | number, includeRelations: boolean = true) : Observable<TributeTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TributeTypeData>(this.baseUrl + 'api/TributeType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTributeType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTributeType(id, includeRelations));
            }));
    }

    public GetTributeTypeList(config: TributeTypeQueryParameters | any = null) : Observable<Array<TributeTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const tributeTypeList$ = this.requestTributeTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TributeType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, tributeTypeList$);

            return tributeTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TributeTypeData>>;
    }


    private requestTributeTypeList(config: TributeTypeQueryParameters | any) : Observable <Array<TributeTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TributeTypeData>>(this.baseUrl + 'api/TributeTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTributeTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTributeTypeList(config));
            }));
    }

    public GetTributeTypesRowCount(config: TributeTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const tributeTypesRowCount$ = this.requestTributeTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TributeTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, tributeTypesRowCount$);

            return tributeTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTributeTypesRowCount(config: TributeTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/TributeTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTributeTypesRowCount(config));
            }));
    }

    public GetTributeTypesBasicListData(config: TributeTypeQueryParameters | any = null) : Observable<Array<TributeTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const tributeTypesBasicListData$ = this.requestTributeTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TributeTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, tributeTypesBasicListData$);

            return tributeTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TributeTypeBasicListData>>;
    }


    private requestTributeTypesBasicListData(config: TributeTypeQueryParameters | any) : Observable<Array<TributeTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TributeTypeBasicListData>>(this.baseUrl + 'api/TributeTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTributeTypesBasicListData(config));
            }));

    }


    public PutTributeType(id: bigint | number, tributeType: TributeTypeSubmitData) : Observable<TributeTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TributeTypeData>(this.baseUrl + 'api/TributeType/' + id.toString(), tributeType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTributeType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTributeType(id, tributeType));
            }));
    }


    public PostTributeType(tributeType: TributeTypeSubmitData) : Observable<TributeTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TributeTypeData>(this.baseUrl + 'api/TributeType', tributeType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTributeType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTributeType(tributeType));
            }));
    }

  
    public DeleteTributeType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/TributeType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTributeType(id));
            }));
    }


    private getConfigHash(config: TributeTypeQueryParameters | any): string {

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

    public userIsSchedulerTributeTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerTributeTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.TributeTypes
        //
        if (userIsSchedulerTributeTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerTributeTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerTributeTypeReader = false;
            }
        }

        return userIsSchedulerTributeTypeReader;
    }


    public userIsSchedulerTributeTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerTributeTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.TributeTypes
        //
        if (userIsSchedulerTributeTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerTributeTypeWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerTributeTypeWriter = false;
          }      
        }

        return userIsSchedulerTributeTypeWriter;
    }

    public GetTributesForTributeType(tributeTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TributeData[]> {
        return this.tributeService.GetTributeList({
            tributeTypeId: tributeTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full TributeTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TributeTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TributeTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTributeType(raw: any): TributeTypeData {
    if (!raw) return raw;

    //
    // Create a TributeTypeData object instance with correct prototype
    //
    const revived = Object.create(TributeTypeData.prototype) as TributeTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._tributes = null;
    (revived as any)._tributesPromise = null;
    (revived as any)._tributesSubject = new BehaviorSubject<TributeData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadTributeTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Tributes$ = (revived as any)._tributesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._tributes === null && (revived as any)._tributesPromise === null) {
                (revived as any).loadTributes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._tributesCount$ = null;



    return revived;
  }

  private ReviveTributeTypeList(rawList: any[]): TributeTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTributeType(raw));
  }

}
