/*

   GENERATED SERVICE FOR THE DEPENDENCYTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the DependencyType table.

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
import { ScheduledEventDependencyService, ScheduledEventDependencyData } from './scheduled-event-dependency.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class DependencyTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class DependencyTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class DependencyTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. DependencyTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `dependencyType.DependencyTypeChildren$` — use with `| async` in templates
//        • Promise:    `dependencyType.DependencyTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="dependencyType.DependencyTypeChildren$ | async"`), or
//        • Access the promise getter (`dependencyType.DependencyTypeChildren` or `await dependencyType.DependencyTypeChildren`)
//    - Simply reading `dependencyType.DependencyTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await dependencyType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class DependencyTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence!: bigint | number;
    color!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _scheduledEventDependencies: ScheduledEventDependencyData[] | null = null;
    private _scheduledEventDependenciesPromise: Promise<ScheduledEventDependencyData[]> | null  = null;
    private _scheduledEventDependenciesSubject = new BehaviorSubject<ScheduledEventDependencyData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ScheduledEventDependencies$ = this._scheduledEventDependenciesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventDependencies === null && this._scheduledEventDependenciesPromise === null) {
            this.loadScheduledEventDependencies(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventDependenciesCount$ = DependencyTypeService.Instance.GetDependencyTypesRowCount({dependencyTypeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any DependencyTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.dependencyType.Reload();
  //
  //  Non Async:
  //
  //     dependencyType[0].Reload().then(x => {
  //        this.dependencyType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      DependencyTypeService.Instance.GetDependencyType(this.id, includeRelations)
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
     this._scheduledEventDependencies = null;
     this._scheduledEventDependenciesPromise = null;
     this._scheduledEventDependenciesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ScheduledEventDependencies for this DependencyType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.dependencyType.ScheduledEventDependencies.then(scheduledEventDependencies => { ... })
     *   or
     *   await this.dependencyType.ScheduledEventDependencies
     *
    */
    public get ScheduledEventDependencies(): Promise<ScheduledEventDependencyData[]> {
        if (this._scheduledEventDependencies !== null) {
            return Promise.resolve(this._scheduledEventDependencies);
        }

        if (this._scheduledEventDependenciesPromise !== null) {
            return this._scheduledEventDependenciesPromise;
        }

        // Start the load
        this.loadScheduledEventDependencies();

        return this._scheduledEventDependenciesPromise!;
    }



    private loadScheduledEventDependencies(): void {

        this._scheduledEventDependenciesPromise = lastValueFrom(
            DependencyTypeService.Instance.GetScheduledEventDependenciesForDependencyType(this.id)
        )
        .then(scheduledEventDependencies => {
            this._scheduledEventDependencies = scheduledEventDependencies ?? [];
            this._scheduledEventDependenciesSubject.next(this._scheduledEventDependencies);
            return this._scheduledEventDependencies;
         })
        .catch(err => {
            this._scheduledEventDependencies = [];
            this._scheduledEventDependenciesSubject.next(this._scheduledEventDependencies);
            throw err;
        })
        .finally(() => {
            this._scheduledEventDependenciesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventDependency. Call after mutations to force refresh.
     */
    public ClearScheduledEventDependenciesCache(): void {
        this._scheduledEventDependencies = null;
        this._scheduledEventDependenciesPromise = null;
        this._scheduledEventDependenciesSubject.next(this._scheduledEventDependencies);      // Emit to observable
    }

    public get HasScheduledEventDependencies(): Promise<boolean> {
        return this.ScheduledEventDependencies.then(scheduledEventDependencies => scheduledEventDependencies.length > 0);
    }




    /**
     * Updates the state of this DependencyTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this DependencyTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): DependencyTypeSubmitData {
        return DependencyTypeService.Instance.ConvertToDependencyTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class DependencyTypeService extends SecureEndpointBase {

    private static _instance: DependencyTypeService;
    private listCache: Map<string, Observable<Array<DependencyTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<DependencyTypeBasicListData>>>;
    private recordCache: Map<string, Observable<DependencyTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private scheduledEventDependencyService: ScheduledEventDependencyService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<DependencyTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<DependencyTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<DependencyTypeData>>();

        DependencyTypeService._instance = this;
    }

    public static get Instance(): DependencyTypeService {
      return DependencyTypeService._instance;
    }


    public ClearListCaches(config: DependencyTypeQueryParameters | null = null) {

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


    public ConvertToDependencyTypeSubmitData(data: DependencyTypeData): DependencyTypeSubmitData {

        let output = new DependencyTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetDependencyType(id: bigint | number, includeRelations: boolean = true) : Observable<DependencyTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const dependencyType$ = this.requestDependencyType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DependencyType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, dependencyType$);

            return dependencyType$;
        }

        return this.recordCache.get(configHash) as Observable<DependencyTypeData>;
    }

    private requestDependencyType(id: bigint | number, includeRelations: boolean = true) : Observable<DependencyTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DependencyTypeData>(this.baseUrl + 'api/DependencyType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveDependencyType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestDependencyType(id, includeRelations));
            }));
    }

    public GetDependencyTypeList(config: DependencyTypeQueryParameters | any = null) : Observable<Array<DependencyTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const dependencyTypeList$ = this.requestDependencyTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DependencyType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, dependencyTypeList$);

            return dependencyTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<DependencyTypeData>>;
    }


    private requestDependencyTypeList(config: DependencyTypeQueryParameters | any) : Observable <Array<DependencyTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DependencyTypeData>>(this.baseUrl + 'api/DependencyTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveDependencyTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestDependencyTypeList(config));
            }));
    }

    public GetDependencyTypesRowCount(config: DependencyTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const dependencyTypesRowCount$ = this.requestDependencyTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DependencyTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, dependencyTypesRowCount$);

            return dependencyTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestDependencyTypesRowCount(config: DependencyTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/DependencyTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDependencyTypesRowCount(config));
            }));
    }

    public GetDependencyTypesBasicListData(config: DependencyTypeQueryParameters | any = null) : Observable<Array<DependencyTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const dependencyTypesBasicListData$ = this.requestDependencyTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DependencyTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, dependencyTypesBasicListData$);

            return dependencyTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<DependencyTypeBasicListData>>;
    }


    private requestDependencyTypesBasicListData(config: DependencyTypeQueryParameters | any) : Observable<Array<DependencyTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DependencyTypeBasicListData>>(this.baseUrl + 'api/DependencyTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDependencyTypesBasicListData(config));
            }));

    }


    public PutDependencyType(id: bigint | number, dependencyType: DependencyTypeSubmitData) : Observable<DependencyTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DependencyTypeData>(this.baseUrl + 'api/DependencyType/' + id.toString(), dependencyType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDependencyType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutDependencyType(id, dependencyType));
            }));
    }


    public PostDependencyType(dependencyType: DependencyTypeSubmitData) : Observable<DependencyTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<DependencyTypeData>(this.baseUrl + 'api/DependencyType', dependencyType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDependencyType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostDependencyType(dependencyType));
            }));
    }

  
    public DeleteDependencyType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/DependencyType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteDependencyType(id));
            }));
    }


    private getConfigHash(config: DependencyTypeQueryParameters | any): string {

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

    public userIsSchedulerDependencyTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerDependencyTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.DependencyTypes
        //
        if (userIsSchedulerDependencyTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerDependencyTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerDependencyTypeReader = false;
            }
        }

        return userIsSchedulerDependencyTypeReader;
    }


    public userIsSchedulerDependencyTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerDependencyTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.DependencyTypes
        //
        if (userIsSchedulerDependencyTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerDependencyTypeWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerDependencyTypeWriter = false;
          }      
        }

        return userIsSchedulerDependencyTypeWriter;
    }

    public GetScheduledEventDependenciesForDependencyType(dependencyTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventDependencyData[]> {
        return this.scheduledEventDependencyService.GetScheduledEventDependencyList({
            dependencyTypeId: dependencyTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full DependencyTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the DependencyTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when DependencyTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveDependencyType(raw: any): DependencyTypeData {
    if (!raw) return raw;

    //
    // Create a DependencyTypeData object instance with correct prototype
    //
    const revived = Object.create(DependencyTypeData.prototype) as DependencyTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._scheduledEventDependencies = null;
    (revived as any)._scheduledEventDependenciesPromise = null;
    (revived as any)._scheduledEventDependenciesSubject = new BehaviorSubject<ScheduledEventDependencyData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadDependencyTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ScheduledEventDependencies$ = (revived as any)._scheduledEventDependenciesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventDependencies === null && (revived as any)._scheduledEventDependenciesPromise === null) {
                (revived as any).loadScheduledEventDependencies();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventDependenciesCount$ = ScheduledEventDependencyService.Instance.GetScheduledEventDependenciesRowCount({dependencyTypeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveDependencyTypeList(rawList: any[]): DependencyTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveDependencyType(raw));
  }

}
