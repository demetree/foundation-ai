/*

   GENERATED SERVICE FOR THE PERIODSTATUS TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PeriodStatus table.

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
import { FiscalPeriodService, FiscalPeriodData } from './fiscal-period.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PeriodStatusQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    color: string | null | undefined = null;
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
export class PeriodStatusSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    color: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class PeriodStatusBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PeriodStatusChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `periodStatus.PeriodStatusChildren$` — use with `| async` in templates
//        • Promise:    `periodStatus.PeriodStatusChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="periodStatus.PeriodStatusChildren$ | async"`), or
//        • Access the promise getter (`periodStatus.PeriodStatusChildren` or `await periodStatus.PeriodStatusChildren`)
//    - Simply reading `periodStatus.PeriodStatusChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await periodStatus.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PeriodStatusData {
    id!: bigint | number;
    name!: string;
    description!: string;
    color!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _fiscalPeriods: FiscalPeriodData[] | null = null;
    private _fiscalPeriodsPromise: Promise<FiscalPeriodData[]> | null  = null;
    private _fiscalPeriodsSubject = new BehaviorSubject<FiscalPeriodData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public FiscalPeriods$ = this._fiscalPeriodsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._fiscalPeriods === null && this._fiscalPeriodsPromise === null) {
            this.loadFiscalPeriods(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _fiscalPeriodsCount$: Observable<bigint | number> | null = null;
    public get FiscalPeriodsCount$(): Observable<bigint | number> {
        if (this._fiscalPeriodsCount$ === null) {
            this._fiscalPeriodsCount$ = FiscalPeriodService.Instance.GetFiscalPeriodsRowCount({periodStatusId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._fiscalPeriodsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PeriodStatusData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.periodStatus.Reload();
  //
  //  Non Async:
  //
  //     periodStatus[0].Reload().then(x => {
  //        this.periodStatus = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PeriodStatusService.Instance.GetPeriodStatus(this.id, includeRelations)
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
     this._fiscalPeriods = null;
     this._fiscalPeriodsPromise = null;
     this._fiscalPeriodsSubject.next(null);
     this._fiscalPeriodsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the FiscalPeriods for this PeriodStatus.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.periodStatus.FiscalPeriods.then(periodStatuses => { ... })
     *   or
     *   await this.periodStatus.periodStatuses
     *
    */
    public get FiscalPeriods(): Promise<FiscalPeriodData[]> {
        if (this._fiscalPeriods !== null) {
            return Promise.resolve(this._fiscalPeriods);
        }

        if (this._fiscalPeriodsPromise !== null) {
            return this._fiscalPeriodsPromise;
        }

        // Start the load
        this.loadFiscalPeriods();

        return this._fiscalPeriodsPromise!;
    }



    private loadFiscalPeriods(): void {

        this._fiscalPeriodsPromise = lastValueFrom(
            PeriodStatusService.Instance.GetFiscalPeriodsForPeriodStatus(this.id)
        )
        .then(FiscalPeriods => {
            this._fiscalPeriods = FiscalPeriods ?? [];
            this._fiscalPeriodsSubject.next(this._fiscalPeriods);
            return this._fiscalPeriods;
         })
        .catch(err => {
            this._fiscalPeriods = [];
            this._fiscalPeriodsSubject.next(this._fiscalPeriods);
            throw err;
        })
        .finally(() => {
            this._fiscalPeriodsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached FiscalPeriod. Call after mutations to force refresh.
     */
    public ClearFiscalPeriodsCache(): void {
        this._fiscalPeriods = null;
        this._fiscalPeriodsPromise = null;
        this._fiscalPeriodsSubject.next(this._fiscalPeriods);      // Emit to observable
    }

    public get HasFiscalPeriods(): Promise<boolean> {
        return this.FiscalPeriods.then(fiscalPeriods => fiscalPeriods.length > 0);
    }




    /**
     * Updates the state of this PeriodStatusData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PeriodStatusData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PeriodStatusSubmitData {
        return PeriodStatusService.Instance.ConvertToPeriodStatusSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PeriodStatusService extends SecureEndpointBase {

    private static _instance: PeriodStatusService;
    private listCache: Map<string, Observable<Array<PeriodStatusData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PeriodStatusBasicListData>>>;
    private recordCache: Map<string, Observable<PeriodStatusData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private fiscalPeriodService: FiscalPeriodService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PeriodStatusData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PeriodStatusBasicListData>>>();
        this.recordCache = new Map<string, Observable<PeriodStatusData>>();

        PeriodStatusService._instance = this;
    }

    public static get Instance(): PeriodStatusService {
      return PeriodStatusService._instance;
    }


    public ClearListCaches(config: PeriodStatusQueryParameters | null = null) {

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


    public ConvertToPeriodStatusSubmitData(data: PeriodStatusData): PeriodStatusSubmitData {

        let output = new PeriodStatusSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.color = data.color;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPeriodStatus(id: bigint | number, includeRelations: boolean = true) : Observable<PeriodStatusData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const periodStatus$ = this.requestPeriodStatus(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PeriodStatus", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, periodStatus$);

            return periodStatus$;
        }

        return this.recordCache.get(configHash) as Observable<PeriodStatusData>;
    }

    private requestPeriodStatus(id: bigint | number, includeRelations: boolean = true) : Observable<PeriodStatusData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PeriodStatusData>(this.baseUrl + 'api/PeriodStatus/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePeriodStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPeriodStatus(id, includeRelations));
            }));
    }

    public GetPeriodStatusList(config: PeriodStatusQueryParameters | any = null) : Observable<Array<PeriodStatusData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const periodStatusList$ = this.requestPeriodStatusList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PeriodStatus list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, periodStatusList$);

            return periodStatusList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PeriodStatusData>>;
    }


    private requestPeriodStatusList(config: PeriodStatusQueryParameters | any) : Observable <Array<PeriodStatusData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PeriodStatusData>>(this.baseUrl + 'api/PeriodStatuses', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePeriodStatusList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPeriodStatusList(config));
            }));
    }

    public GetPeriodStatusesRowCount(config: PeriodStatusQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const periodStatusesRowCount$ = this.requestPeriodStatusesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PeriodStatuses row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, periodStatusesRowCount$);

            return periodStatusesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPeriodStatusesRowCount(config: PeriodStatusQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PeriodStatuses/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPeriodStatusesRowCount(config));
            }));
    }

    public GetPeriodStatusesBasicListData(config: PeriodStatusQueryParameters | any = null) : Observable<Array<PeriodStatusBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const periodStatusesBasicListData$ = this.requestPeriodStatusesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PeriodStatuses basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, periodStatusesBasicListData$);

            return periodStatusesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PeriodStatusBasicListData>>;
    }


    private requestPeriodStatusesBasicListData(config: PeriodStatusQueryParameters | any) : Observable<Array<PeriodStatusBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PeriodStatusBasicListData>>(this.baseUrl + 'api/PeriodStatuses/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPeriodStatusesBasicListData(config));
            }));

    }


    public PutPeriodStatus(id: bigint | number, periodStatus: PeriodStatusSubmitData) : Observable<PeriodStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PeriodStatusData>(this.baseUrl + 'api/PeriodStatus/' + id.toString(), periodStatus, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePeriodStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPeriodStatus(id, periodStatus));
            }));
    }


    public PostPeriodStatus(periodStatus: PeriodStatusSubmitData) : Observable<PeriodStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PeriodStatusData>(this.baseUrl + 'api/PeriodStatus', periodStatus, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePeriodStatus(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPeriodStatus(periodStatus));
            }));
    }

  
    public DeletePeriodStatus(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PeriodStatus/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePeriodStatus(id));
            }));
    }


    private getConfigHash(config: PeriodStatusQueryParameters | any): string {

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

    public userIsSchedulerPeriodStatusReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerPeriodStatusReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.PeriodStatuses
        //
        if (userIsSchedulerPeriodStatusReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerPeriodStatusReader = user.readPermission >= 1;
            } else {
                userIsSchedulerPeriodStatusReader = false;
            }
        }

        return userIsSchedulerPeriodStatusReader;
    }


    public userIsSchedulerPeriodStatusWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerPeriodStatusWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.PeriodStatuses
        //
        if (userIsSchedulerPeriodStatusWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerPeriodStatusWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerPeriodStatusWriter = false;
          }      
        }

        return userIsSchedulerPeriodStatusWriter;
    }

    public GetFiscalPeriodsForPeriodStatus(periodStatusId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FiscalPeriodData[]> {
        return this.fiscalPeriodService.GetFiscalPeriodList({
            periodStatusId: periodStatusId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PeriodStatusData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PeriodStatusData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PeriodStatusTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePeriodStatus(raw: any): PeriodStatusData {
    if (!raw) return raw;

    //
    // Create a PeriodStatusData object instance with correct prototype
    //
    const revived = Object.create(PeriodStatusData.prototype) as PeriodStatusData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._fiscalPeriods = null;
    (revived as any)._fiscalPeriodsPromise = null;
    (revived as any)._fiscalPeriodsSubject = new BehaviorSubject<FiscalPeriodData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadPeriodStatusXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).FiscalPeriods$ = (revived as any)._fiscalPeriodsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._fiscalPeriods === null && (revived as any)._fiscalPeriodsPromise === null) {
                (revived as any).loadFiscalPeriods();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._fiscalPeriodsCount$ = null;



    return revived;
  }

  private RevivePeriodStatusList(rawList: any[]): PeriodStatusData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePeriodStatus(raw));
  }

}
