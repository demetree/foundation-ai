/*

   GENERATED SERVICE FOR THE SCHEDULEOVERRIDETYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ScheduleOverrideType table.

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
import { ScheduleOverrideService, ScheduleOverrideData } from './schedule-override.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ScheduleOverrideTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class ScheduleOverrideTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ScheduleOverrideTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ScheduleOverrideTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `scheduleOverrideType.ScheduleOverrideTypeChildren$` — use with `| async` in templates
//        • Promise:    `scheduleOverrideType.ScheduleOverrideTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="scheduleOverrideType.ScheduleOverrideTypeChildren$ | async"`), or
//        • Access the promise getter (`scheduleOverrideType.ScheduleOverrideTypeChildren` or `await scheduleOverrideType.ScheduleOverrideTypeChildren`)
//    - Simply reading `scheduleOverrideType.ScheduleOverrideTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await scheduleOverrideType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ScheduleOverrideTypeData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _scheduleOverrides: ScheduleOverrideData[] | null = null;
    private _scheduleOverridesPromise: Promise<ScheduleOverrideData[]> | null  = null;
    private _scheduleOverridesSubject = new BehaviorSubject<ScheduleOverrideData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ScheduleOverrides$ = this._scheduleOverridesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduleOverrides === null && this._scheduleOverridesPromise === null) {
            this.loadScheduleOverrides(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduleOverridesCount$ = ScheduleOverrideService.Instance.GetScheduleOverridesRowCount({scheduleOverrideTypeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ScheduleOverrideTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.scheduleOverrideType.Reload();
  //
  //  Non Async:
  //
  //     scheduleOverrideType[0].Reload().then(x => {
  //        this.scheduleOverrideType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ScheduleOverrideTypeService.Instance.GetScheduleOverrideType(this.id, includeRelations)
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
     this._scheduleOverrides = null;
     this._scheduleOverridesPromise = null;
     this._scheduleOverridesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ScheduleOverrides for this ScheduleOverrideType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduleOverrideType.ScheduleOverrides.then(scheduleOverrideTypes => { ... })
     *   or
     *   await this.scheduleOverrideType.scheduleOverrideTypes
     *
    */
    public get ScheduleOverrides(): Promise<ScheduleOverrideData[]> {
        if (this._scheduleOverrides !== null) {
            return Promise.resolve(this._scheduleOverrides);
        }

        if (this._scheduleOverridesPromise !== null) {
            return this._scheduleOverridesPromise;
        }

        // Start the load
        this.loadScheduleOverrides();

        return this._scheduleOverridesPromise!;
    }



    private loadScheduleOverrides(): void {

        this._scheduleOverridesPromise = lastValueFrom(
            ScheduleOverrideTypeService.Instance.GetScheduleOverridesForScheduleOverrideType(this.id)
        )
        .then(ScheduleOverrides => {
            this._scheduleOverrides = ScheduleOverrides ?? [];
            this._scheduleOverridesSubject.next(this._scheduleOverrides);
            return this._scheduleOverrides;
         })
        .catch(err => {
            this._scheduleOverrides = [];
            this._scheduleOverridesSubject.next(this._scheduleOverrides);
            throw err;
        })
        .finally(() => {
            this._scheduleOverridesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduleOverride. Call after mutations to force refresh.
     */
    public ClearScheduleOverridesCache(): void {
        this._scheduleOverrides = null;
        this._scheduleOverridesPromise = null;
        this._scheduleOverridesSubject.next(this._scheduleOverrides);      // Emit to observable
    }

    public get HasScheduleOverrides(): Promise<boolean> {
        return this.ScheduleOverrides.then(scheduleOverrides => scheduleOverrides.length > 0);
    }




    /**
     * Updates the state of this ScheduleOverrideTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ScheduleOverrideTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ScheduleOverrideTypeSubmitData {
        return ScheduleOverrideTypeService.Instance.ConvertToScheduleOverrideTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ScheduleOverrideTypeService extends SecureEndpointBase {

    private static _instance: ScheduleOverrideTypeService;
    private listCache: Map<string, Observable<Array<ScheduleOverrideTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ScheduleOverrideTypeBasicListData>>>;
    private recordCache: Map<string, Observable<ScheduleOverrideTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private scheduleOverrideService: ScheduleOverrideService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ScheduleOverrideTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ScheduleOverrideTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<ScheduleOverrideTypeData>>();

        ScheduleOverrideTypeService._instance = this;
    }

    public static get Instance(): ScheduleOverrideTypeService {
      return ScheduleOverrideTypeService._instance;
    }


    public ClearListCaches(config: ScheduleOverrideTypeQueryParameters | null = null) {

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


    public ConvertToScheduleOverrideTypeSubmitData(data: ScheduleOverrideTypeData): ScheduleOverrideTypeSubmitData {

        let output = new ScheduleOverrideTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetScheduleOverrideType(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduleOverrideTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const scheduleOverrideType$ = this.requestScheduleOverrideType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleOverrideType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, scheduleOverrideType$);

            return scheduleOverrideType$;
        }

        return this.recordCache.get(configHash) as Observable<ScheduleOverrideTypeData>;
    }

    private requestScheduleOverrideType(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduleOverrideTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduleOverrideTypeData>(this.baseUrl + 'api/ScheduleOverrideType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveScheduleOverrideType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleOverrideType(id, includeRelations));
            }));
    }

    public GetScheduleOverrideTypeList(config: ScheduleOverrideTypeQueryParameters | any = null) : Observable<Array<ScheduleOverrideTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const scheduleOverrideTypeList$ = this.requestScheduleOverrideTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleOverrideType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, scheduleOverrideTypeList$);

            return scheduleOverrideTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ScheduleOverrideTypeData>>;
    }


    private requestScheduleOverrideTypeList(config: ScheduleOverrideTypeQueryParameters | any) : Observable <Array<ScheduleOverrideTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduleOverrideTypeData>>(this.baseUrl + 'api/ScheduleOverrideTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveScheduleOverrideTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleOverrideTypeList(config));
            }));
    }

    public GetScheduleOverrideTypesRowCount(config: ScheduleOverrideTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const scheduleOverrideTypesRowCount$ = this.requestScheduleOverrideTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleOverrideTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, scheduleOverrideTypesRowCount$);

            return scheduleOverrideTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestScheduleOverrideTypesRowCount(config: ScheduleOverrideTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ScheduleOverrideTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleOverrideTypesRowCount(config));
            }));
    }

    public GetScheduleOverrideTypesBasicListData(config: ScheduleOverrideTypeQueryParameters | any = null) : Observable<Array<ScheduleOverrideTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const scheduleOverrideTypesBasicListData$ = this.requestScheduleOverrideTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduleOverrideTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, scheduleOverrideTypesBasicListData$);

            return scheduleOverrideTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ScheduleOverrideTypeBasicListData>>;
    }


    private requestScheduleOverrideTypesBasicListData(config: ScheduleOverrideTypeQueryParameters | any) : Observable<Array<ScheduleOverrideTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduleOverrideTypeBasicListData>>(this.baseUrl + 'api/ScheduleOverrideTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduleOverrideTypesBasicListData(config));
            }));

    }


    public PutScheduleOverrideType(id: bigint | number, scheduleOverrideType: ScheduleOverrideTypeSubmitData) : Observable<ScheduleOverrideTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduleOverrideTypeData>(this.baseUrl + 'api/ScheduleOverrideType/' + id.toString(), scheduleOverrideType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduleOverrideType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutScheduleOverrideType(id, scheduleOverrideType));
            }));
    }


    public PostScheduleOverrideType(scheduleOverrideType: ScheduleOverrideTypeSubmitData) : Observable<ScheduleOverrideTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ScheduleOverrideTypeData>(this.baseUrl + 'api/ScheduleOverrideType', scheduleOverrideType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduleOverrideType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostScheduleOverrideType(scheduleOverrideType));
            }));
    }

  
    public DeleteScheduleOverrideType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ScheduleOverrideType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteScheduleOverrideType(id));
            }));
    }


    private getConfigHash(config: ScheduleOverrideTypeQueryParameters | any): string {

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

    public userIsAlertingScheduleOverrideTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingScheduleOverrideTypeReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.ScheduleOverrideTypes
        //
        if (userIsAlertingScheduleOverrideTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingScheduleOverrideTypeReader = user.readPermission >= 1;
            } else {
                userIsAlertingScheduleOverrideTypeReader = false;
            }
        }

        return userIsAlertingScheduleOverrideTypeReader;
    }


    public userIsAlertingScheduleOverrideTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingScheduleOverrideTypeWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.ScheduleOverrideTypes
        //
        if (userIsAlertingScheduleOverrideTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingScheduleOverrideTypeWriter = user.writePermission >= 255;
          } else {
            userIsAlertingScheduleOverrideTypeWriter = false;
          }      
        }

        return userIsAlertingScheduleOverrideTypeWriter;
    }

    public GetScheduleOverridesForScheduleOverrideType(scheduleOverrideTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduleOverrideData[]> {
        return this.scheduleOverrideService.GetScheduleOverrideList({
            scheduleOverrideTypeId: scheduleOverrideTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ScheduleOverrideTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ScheduleOverrideTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ScheduleOverrideTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveScheduleOverrideType(raw: any): ScheduleOverrideTypeData {
    if (!raw) return raw;

    //
    // Create a ScheduleOverrideTypeData object instance with correct prototype
    //
    const revived = Object.create(ScheduleOverrideTypeData.prototype) as ScheduleOverrideTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._scheduleOverrides = null;
    (revived as any)._scheduleOverridesPromise = null;
    (revived as any)._scheduleOverridesSubject = new BehaviorSubject<ScheduleOverrideData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadScheduleOverrideTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ScheduleOverrides$ = (revived as any)._scheduleOverridesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduleOverrides === null && (revived as any)._scheduleOverridesPromise === null) {
                (revived as any).loadScheduleOverrides();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduleOverridesCount$ = ScheduleOverrideService.Instance.GetScheduleOverridesRowCount({scheduleOverrideTypeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveScheduleOverrideTypeList(rawList: any[]): ScheduleOverrideTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveScheduleOverrideType(raw));
  }

}
