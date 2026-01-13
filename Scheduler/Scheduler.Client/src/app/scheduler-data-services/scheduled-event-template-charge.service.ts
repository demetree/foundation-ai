/*

   GENERATED SERVICE FOR THE SCHEDULEDEVENTTEMPLATECHARGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ScheduledEventTemplateCharge table.

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
import { ScheduledEventTemplateData } from './scheduled-event-template.service';
import { ChargeTypeData } from './charge-type.service';
import { ScheduledEventTemplateChargeChangeHistoryService, ScheduledEventTemplateChargeChangeHistoryData } from './scheduled-event-template-charge-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ScheduledEventTemplateChargeQueryParameters {
    scheduledEventTemplateId: bigint | number | null | undefined = null;
    chargeTypeId: bigint | number | null | undefined = null;
    defaultAmount: number | null | undefined = null;
    isRequired: boolean | null | undefined = null;
    versionNumber: bigint | number | null | undefined = null;
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
export class ScheduledEventTemplateChargeSubmitData {
    id!: bigint | number;
    scheduledEventTemplateId!: bigint | number;
    chargeTypeId!: bigint | number;
    defaultAmount!: number;
    isRequired!: boolean;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class ScheduledEventTemplateChargeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ScheduledEventTemplateChargeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `scheduledEventTemplateCharge.ScheduledEventTemplateChargeChildren$` — use with `| async` in templates
//        • Promise:    `scheduledEventTemplateCharge.ScheduledEventTemplateChargeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="scheduledEventTemplateCharge.ScheduledEventTemplateChargeChildren$ | async"`), or
//        • Access the promise getter (`scheduledEventTemplateCharge.ScheduledEventTemplateChargeChildren` or `await scheduledEventTemplateCharge.ScheduledEventTemplateChargeChildren`)
//    - Simply reading `scheduledEventTemplateCharge.ScheduledEventTemplateChargeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await scheduledEventTemplateCharge.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ScheduledEventTemplateChargeData {
    id!: bigint | number;
    scheduledEventTemplateId!: bigint | number;
    chargeTypeId!: bigint | number;
    defaultAmount!: number;
    isRequired!: boolean;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    chargeType: ChargeTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduledEventTemplate: ScheduledEventTemplateData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _scheduledEventTemplateChargeChangeHistories: ScheduledEventTemplateChargeChangeHistoryData[] | null = null;
    private _scheduledEventTemplateChargeChangeHistoriesPromise: Promise<ScheduledEventTemplateChargeChangeHistoryData[]> | null  = null;
    private _scheduledEventTemplateChargeChangeHistoriesSubject = new BehaviorSubject<ScheduledEventTemplateChargeChangeHistoryData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ScheduledEventTemplateChargeChangeHistories$ = this._scheduledEventTemplateChargeChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventTemplateChargeChangeHistories === null && this._scheduledEventTemplateChargeChangeHistoriesPromise === null) {
            this.loadScheduledEventTemplateChargeChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventTemplateChargeChangeHistoriesCount$ = ScheduledEventTemplateChargeChangeHistoryService.Instance.GetScheduledEventTemplateChargeChangeHistoriesRowCount({scheduledEventTemplateChargeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ScheduledEventTemplateChargeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.scheduledEventTemplateCharge.Reload();
  //
  //  Non Async:
  //
  //     scheduledEventTemplateCharge[0].Reload().then(x => {
  //        this.scheduledEventTemplateCharge = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ScheduledEventTemplateChargeService.Instance.GetScheduledEventTemplateCharge(this.id, includeRelations)
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
     this._scheduledEventTemplateChargeChangeHistories = null;
     this._scheduledEventTemplateChargeChangeHistoriesPromise = null;
     this._scheduledEventTemplateChargeChangeHistoriesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ScheduledEventTemplateChargeChangeHistories for this ScheduledEventTemplateCharge.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEventTemplateCharge.ScheduledEventTemplateChargeChangeHistories.then(scheduledEventTemplateCharges => { ... })
     *   or
     *   await this.scheduledEventTemplateCharge.scheduledEventTemplateCharges
     *
    */
    public get ScheduledEventTemplateChargeChangeHistories(): Promise<ScheduledEventTemplateChargeChangeHistoryData[]> {
        if (this._scheduledEventTemplateChargeChangeHistories !== null) {
            return Promise.resolve(this._scheduledEventTemplateChargeChangeHistories);
        }

        if (this._scheduledEventTemplateChargeChangeHistoriesPromise !== null) {
            return this._scheduledEventTemplateChargeChangeHistoriesPromise;
        }

        // Start the load
        this.loadScheduledEventTemplateChargeChangeHistories();

        return this._scheduledEventTemplateChargeChangeHistoriesPromise!;
    }



    private loadScheduledEventTemplateChargeChangeHistories(): void {

        this._scheduledEventTemplateChargeChangeHistoriesPromise = lastValueFrom(
            ScheduledEventTemplateChargeService.Instance.GetScheduledEventTemplateChargeChangeHistoriesForScheduledEventTemplateCharge(this.id)
        )
        .then(ScheduledEventTemplateChargeChangeHistories => {
            this._scheduledEventTemplateChargeChangeHistories = ScheduledEventTemplateChargeChangeHistories ?? [];
            this._scheduledEventTemplateChargeChangeHistoriesSubject.next(this._scheduledEventTemplateChargeChangeHistories);
            return this._scheduledEventTemplateChargeChangeHistories;
         })
        .catch(err => {
            this._scheduledEventTemplateChargeChangeHistories = [];
            this._scheduledEventTemplateChargeChangeHistoriesSubject.next(this._scheduledEventTemplateChargeChangeHistories);
            throw err;
        })
        .finally(() => {
            this._scheduledEventTemplateChargeChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventTemplateChargeChangeHistory. Call after mutations to force refresh.
     */
    public ClearScheduledEventTemplateChargeChangeHistoriesCache(): void {
        this._scheduledEventTemplateChargeChangeHistories = null;
        this._scheduledEventTemplateChargeChangeHistoriesPromise = null;
        this._scheduledEventTemplateChargeChangeHistoriesSubject.next(this._scheduledEventTemplateChargeChangeHistories);      // Emit to observable
    }

    public get HasScheduledEventTemplateChargeChangeHistories(): Promise<boolean> {
        return this.ScheduledEventTemplateChargeChangeHistories.then(scheduledEventTemplateChargeChangeHistories => scheduledEventTemplateChargeChangeHistories.length > 0);
    }




    /**
     * Updates the state of this ScheduledEventTemplateChargeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ScheduledEventTemplateChargeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ScheduledEventTemplateChargeSubmitData {
        return ScheduledEventTemplateChargeService.Instance.ConvertToScheduledEventTemplateChargeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ScheduledEventTemplateChargeService extends SecureEndpointBase {

    private static _instance: ScheduledEventTemplateChargeService;
    private listCache: Map<string, Observable<Array<ScheduledEventTemplateChargeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ScheduledEventTemplateChargeBasicListData>>>;
    private recordCache: Map<string, Observable<ScheduledEventTemplateChargeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private scheduledEventTemplateChargeChangeHistoryService: ScheduledEventTemplateChargeChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ScheduledEventTemplateChargeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ScheduledEventTemplateChargeBasicListData>>>();
        this.recordCache = new Map<string, Observable<ScheduledEventTemplateChargeData>>();

        ScheduledEventTemplateChargeService._instance = this;
    }

    public static get Instance(): ScheduledEventTemplateChargeService {
      return ScheduledEventTemplateChargeService._instance;
    }


    public ClearListCaches(config: ScheduledEventTemplateChargeQueryParameters | null = null) {

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


    public ConvertToScheduledEventTemplateChargeSubmitData(data: ScheduledEventTemplateChargeData): ScheduledEventTemplateChargeSubmitData {

        let output = new ScheduledEventTemplateChargeSubmitData();

        output.id = data.id;
        output.scheduledEventTemplateId = data.scheduledEventTemplateId;
        output.chargeTypeId = data.chargeTypeId;
        output.defaultAmount = data.defaultAmount;
        output.isRequired = data.isRequired;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetScheduledEventTemplateCharge(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventTemplateChargeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const scheduledEventTemplateCharge$ = this.requestScheduledEventTemplateCharge(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateCharge", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, scheduledEventTemplateCharge$);

            return scheduledEventTemplateCharge$;
        }

        return this.recordCache.get(configHash) as Observable<ScheduledEventTemplateChargeData>;
    }

    private requestScheduledEventTemplateCharge(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventTemplateChargeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventTemplateChargeData>(this.baseUrl + 'api/ScheduledEventTemplateCharge/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveScheduledEventTemplateCharge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateCharge(id, includeRelations));
            }));
    }

    public GetScheduledEventTemplateChargeList(config: ScheduledEventTemplateChargeQueryParameters | any = null) : Observable<Array<ScheduledEventTemplateChargeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const scheduledEventTemplateChargeList$ = this.requestScheduledEventTemplateChargeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateCharge list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, scheduledEventTemplateChargeList$);

            return scheduledEventTemplateChargeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ScheduledEventTemplateChargeData>>;
    }


    private requestScheduledEventTemplateChargeList(config: ScheduledEventTemplateChargeQueryParameters | any) : Observable <Array<ScheduledEventTemplateChargeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventTemplateChargeData>>(this.baseUrl + 'api/ScheduledEventTemplateCharges', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveScheduledEventTemplateChargeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateChargeList(config));
            }));
    }

    public GetScheduledEventTemplateChargesRowCount(config: ScheduledEventTemplateChargeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const scheduledEventTemplateChargesRowCount$ = this.requestScheduledEventTemplateChargesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateCharges row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, scheduledEventTemplateChargesRowCount$);

            return scheduledEventTemplateChargesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestScheduledEventTemplateChargesRowCount(config: ScheduledEventTemplateChargeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ScheduledEventTemplateCharges/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateChargesRowCount(config));
            }));
    }

    public GetScheduledEventTemplateChargesBasicListData(config: ScheduledEventTemplateChargeQueryParameters | any = null) : Observable<Array<ScheduledEventTemplateChargeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const scheduledEventTemplateChargesBasicListData$ = this.requestScheduledEventTemplateChargesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateCharges basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, scheduledEventTemplateChargesBasicListData$);

            return scheduledEventTemplateChargesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ScheduledEventTemplateChargeBasicListData>>;
    }


    private requestScheduledEventTemplateChargesBasicListData(config: ScheduledEventTemplateChargeQueryParameters | any) : Observable<Array<ScheduledEventTemplateChargeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventTemplateChargeBasicListData>>(this.baseUrl + 'api/ScheduledEventTemplateCharges/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateChargesBasicListData(config));
            }));

    }


    public PutScheduledEventTemplateCharge(id: bigint | number, scheduledEventTemplateCharge: ScheduledEventTemplateChargeSubmitData) : Observable<ScheduledEventTemplateChargeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventTemplateChargeData>(this.baseUrl + 'api/ScheduledEventTemplateCharge/' + id.toString(), scheduledEventTemplateCharge, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplateCharge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutScheduledEventTemplateCharge(id, scheduledEventTemplateCharge));
            }));
    }


    public PostScheduledEventTemplateCharge(scheduledEventTemplateCharge: ScheduledEventTemplateChargeSubmitData) : Observable<ScheduledEventTemplateChargeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ScheduledEventTemplateChargeData>(this.baseUrl + 'api/ScheduledEventTemplateCharge', scheduledEventTemplateCharge, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplateCharge(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostScheduledEventTemplateCharge(scheduledEventTemplateCharge));
            }));
    }

  
    public DeleteScheduledEventTemplateCharge(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ScheduledEventTemplateCharge/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteScheduledEventTemplateCharge(id));
            }));
    }

    public RollbackScheduledEventTemplateCharge(id: bigint | number, versionNumber: bigint | number) : Observable<ScheduledEventTemplateChargeData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventTemplateChargeData>(this.baseUrl + 'api/ScheduledEventTemplateCharge/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplateCharge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackScheduledEventTemplateCharge(id, versionNumber));
        }));
    }

    private getConfigHash(config: ScheduledEventTemplateChargeQueryParameters | any): string {

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

    public userIsSchedulerScheduledEventTemplateChargeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerScheduledEventTemplateChargeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ScheduledEventTemplateCharges
        //
        if (userIsSchedulerScheduledEventTemplateChargeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerScheduledEventTemplateChargeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerScheduledEventTemplateChargeReader = false;
            }
        }

        return userIsSchedulerScheduledEventTemplateChargeReader;
    }


    public userIsSchedulerScheduledEventTemplateChargeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerScheduledEventTemplateChargeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ScheduledEventTemplateCharges
        //
        if (userIsSchedulerScheduledEventTemplateChargeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerScheduledEventTemplateChargeWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerScheduledEventTemplateChargeWriter = false;
          }      
        }

        return userIsSchedulerScheduledEventTemplateChargeWriter;
    }

    public GetScheduledEventTemplateChargeChangeHistoriesForScheduledEventTemplateCharge(scheduledEventTemplateChargeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventTemplateChargeChangeHistoryData[]> {
        return this.scheduledEventTemplateChargeChangeHistoryService.GetScheduledEventTemplateChargeChangeHistoryList({
            scheduledEventTemplateChargeId: scheduledEventTemplateChargeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ScheduledEventTemplateChargeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ScheduledEventTemplateChargeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ScheduledEventTemplateChargeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveScheduledEventTemplateCharge(raw: any): ScheduledEventTemplateChargeData {
    if (!raw) return raw;

    //
    // Create a ScheduledEventTemplateChargeData object instance with correct prototype
    //
    const revived = Object.create(ScheduledEventTemplateChargeData.prototype) as ScheduledEventTemplateChargeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._scheduledEventTemplateChargeChangeHistories = null;
    (revived as any)._scheduledEventTemplateChargeChangeHistoriesPromise = null;
    (revived as any)._scheduledEventTemplateChargeChangeHistoriesSubject = new BehaviorSubject<ScheduledEventTemplateChargeChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadScheduledEventTemplateChargeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ScheduledEventTemplateChargeChangeHistories$ = (revived as any)._scheduledEventTemplateChargeChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventTemplateChargeChangeHistories === null && (revived as any)._scheduledEventTemplateChargeChangeHistoriesPromise === null) {
                (revived as any).loadScheduledEventTemplateChargeChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventTemplateChargeChangeHistoriesCount$ = ScheduledEventTemplateChargeChangeHistoryService.Instance.GetScheduledEventTemplateChargeChangeHistoriesRowCount({scheduledEventTemplateChargeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveScheduledEventTemplateChargeList(rawList: any[]): ScheduledEventTemplateChargeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveScheduledEventTemplateCharge(raw));
  }

}
