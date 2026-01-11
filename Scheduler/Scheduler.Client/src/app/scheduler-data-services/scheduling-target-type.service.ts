/*

   GENERATED SERVICE FOR THE SCHEDULINGTARGETTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SchedulingTargetType table.

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
import { IconData } from './icon.service';
import { SchedulingTargetService, SchedulingTargetData } from './scheduling-target.service';
import { ScheduledEventTemplateService, ScheduledEventTemplateData } from './scheduled-event-template.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SchedulingTargetTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
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
export class SchedulingTargetTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class SchedulingTargetTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SchedulingTargetTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `schedulingTargetType.SchedulingTargetTypeChildren$` — use with `| async` in templates
//        • Promise:    `schedulingTargetType.SchedulingTargetTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="schedulingTargetType.SchedulingTargetTypeChildren$ | async"`), or
//        • Access the promise getter (`schedulingTargetType.SchedulingTargetTypeChildren` or `await schedulingTargetType.SchedulingTargetTypeChildren`)
//    - Simply reading `schedulingTargetType.SchedulingTargetTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await schedulingTargetType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SchedulingTargetTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence!: bigint | number;
    iconId!: bigint | number;
    color!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _schedulingTargets: SchedulingTargetData[] | null = null;
    private _schedulingTargetsPromise: Promise<SchedulingTargetData[]> | null  = null;
    private _schedulingTargetsSubject = new BehaviorSubject<SchedulingTargetData[] | null>(null);

    private _scheduledEventTemplates: ScheduledEventTemplateData[] | null = null;
    private _scheduledEventTemplatesPromise: Promise<ScheduledEventTemplateData[]> | null  = null;
    private _scheduledEventTemplatesSubject = new BehaviorSubject<ScheduledEventTemplateData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SchedulingTargets$ = this._schedulingTargetsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargets === null && this._schedulingTargetsPromise === null) {
            this.loadSchedulingTargets(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SchedulingTargetsCount$ = SchedulingTargetTypeService.Instance.GetSchedulingTargetTypesRowCount({schedulingTargetTypeId: this.id,
      active: true,
      deleted: false
    });



    public ScheduledEventTemplates$ = this._scheduledEventTemplatesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventTemplates === null && this._scheduledEventTemplatesPromise === null) {
            this.loadScheduledEventTemplates(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventTemplatesCount$ = SchedulingTargetTypeService.Instance.GetSchedulingTargetTypesRowCount({schedulingTargetTypeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SchedulingTargetTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.schedulingTargetType.Reload();
  //
  //  Non Async:
  //
  //     schedulingTargetType[0].Reload().then(x => {
  //        this.schedulingTargetType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SchedulingTargetTypeService.Instance.GetSchedulingTargetType(this.id, includeRelations)
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
     this._schedulingTargets = null;
     this._schedulingTargetsPromise = null;
     this._schedulingTargetsSubject.next(null);

     this._scheduledEventTemplates = null;
     this._scheduledEventTemplatesPromise = null;
     this._scheduledEventTemplatesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the SchedulingTargets for this SchedulingTargetType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.schedulingTargetType.SchedulingTargets.then(schedulingTargets => { ... })
     *   or
     *   await this.schedulingTargetType.SchedulingTargets
     *
    */
    public get SchedulingTargets(): Promise<SchedulingTargetData[]> {
        if (this._schedulingTargets !== null) {
            return Promise.resolve(this._schedulingTargets);
        }

        if (this._schedulingTargetsPromise !== null) {
            return this._schedulingTargetsPromise;
        }

        // Start the load
        this.loadSchedulingTargets();

        return this._schedulingTargetsPromise!;
    }



    private loadSchedulingTargets(): void {

        this._schedulingTargetsPromise = lastValueFrom(
            SchedulingTargetTypeService.Instance.GetSchedulingTargetsForSchedulingTargetType(this.id)
        )
        .then(schedulingTargets => {
            this._schedulingTargets = schedulingTargets ?? [];
            this._schedulingTargetsSubject.next(this._schedulingTargets);
            return this._schedulingTargets;
         })
        .catch(err => {
            this._schedulingTargets = [];
            this._schedulingTargetsSubject.next(this._schedulingTargets);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTarget. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetsCache(): void {
        this._schedulingTargets = null;
        this._schedulingTargetsPromise = null;
        this._schedulingTargetsSubject.next(this._schedulingTargets);      // Emit to observable
    }

    public get HasSchedulingTargets(): Promise<boolean> {
        return this.SchedulingTargets.then(schedulingTargets => schedulingTargets.length > 0);
    }


    /**
     *
     * Gets the ScheduledEventTemplates for this SchedulingTargetType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.schedulingTargetType.ScheduledEventTemplates.then(scheduledEventTemplates => { ... })
     *   or
     *   await this.schedulingTargetType.ScheduledEventTemplates
     *
    */
    public get ScheduledEventTemplates(): Promise<ScheduledEventTemplateData[]> {
        if (this._scheduledEventTemplates !== null) {
            return Promise.resolve(this._scheduledEventTemplates);
        }

        if (this._scheduledEventTemplatesPromise !== null) {
            return this._scheduledEventTemplatesPromise;
        }

        // Start the load
        this.loadScheduledEventTemplates();

        return this._scheduledEventTemplatesPromise!;
    }



    private loadScheduledEventTemplates(): void {

        this._scheduledEventTemplatesPromise = lastValueFrom(
            SchedulingTargetTypeService.Instance.GetScheduledEventTemplatesForSchedulingTargetType(this.id)
        )
        .then(scheduledEventTemplates => {
            this._scheduledEventTemplates = scheduledEventTemplates ?? [];
            this._scheduledEventTemplatesSubject.next(this._scheduledEventTemplates);
            return this._scheduledEventTemplates;
         })
        .catch(err => {
            this._scheduledEventTemplates = [];
            this._scheduledEventTemplatesSubject.next(this._scheduledEventTemplates);
            throw err;
        })
        .finally(() => {
            this._scheduledEventTemplatesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventTemplate. Call after mutations to force refresh.
     */
    public ClearScheduledEventTemplatesCache(): void {
        this._scheduledEventTemplates = null;
        this._scheduledEventTemplatesPromise = null;
        this._scheduledEventTemplatesSubject.next(this._scheduledEventTemplates);      // Emit to observable
    }

    public get HasScheduledEventTemplates(): Promise<boolean> {
        return this.ScheduledEventTemplates.then(scheduledEventTemplates => scheduledEventTemplates.length > 0);
    }




    /**
     * Updates the state of this SchedulingTargetTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SchedulingTargetTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SchedulingTargetTypeSubmitData {
        return SchedulingTargetTypeService.Instance.ConvertToSchedulingTargetTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SchedulingTargetTypeService extends SecureEndpointBase {

    private static _instance: SchedulingTargetTypeService;
    private listCache: Map<string, Observable<Array<SchedulingTargetTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SchedulingTargetTypeBasicListData>>>;
    private recordCache: Map<string, Observable<SchedulingTargetTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private schedulingTargetService: SchedulingTargetService,
        private scheduledEventTemplateService: ScheduledEventTemplateService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SchedulingTargetTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SchedulingTargetTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<SchedulingTargetTypeData>>();

        SchedulingTargetTypeService._instance = this;
    }

    public static get Instance(): SchedulingTargetTypeService {
      return SchedulingTargetTypeService._instance;
    }


    public ClearListCaches(config: SchedulingTargetTypeQueryParameters | null = null) {

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


    public ConvertToSchedulingTargetTypeSubmitData(data: SchedulingTargetTypeData): SchedulingTargetTypeSubmitData {

        let output = new SchedulingTargetTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.iconId = data.iconId;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSchedulingTargetType(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const schedulingTargetType$ = this.requestSchedulingTargetType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, schedulingTargetType$);

            return schedulingTargetType$;
        }

        return this.recordCache.get(configHash) as Observable<SchedulingTargetTypeData>;
    }

    private requestSchedulingTargetType(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetTypeData>(this.baseUrl + 'api/SchedulingTargetType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSchedulingTargetType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetType(id, includeRelations));
            }));
    }

    public GetSchedulingTargetTypeList(config: SchedulingTargetTypeQueryParameters | any = null) : Observable<Array<SchedulingTargetTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const schedulingTargetTypeList$ = this.requestSchedulingTargetTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, schedulingTargetTypeList$);

            return schedulingTargetTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SchedulingTargetTypeData>>;
    }


    private requestSchedulingTargetTypeList(config: SchedulingTargetTypeQueryParameters | any) : Observable <Array<SchedulingTargetTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetTypeData>>(this.baseUrl + 'api/SchedulingTargetTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSchedulingTargetTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetTypeList(config));
            }));
    }

    public GetSchedulingTargetTypesRowCount(config: SchedulingTargetTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const schedulingTargetTypesRowCount$ = this.requestSchedulingTargetTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, schedulingTargetTypesRowCount$);

            return schedulingTargetTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSchedulingTargetTypesRowCount(config: SchedulingTargetTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SchedulingTargetTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetTypesRowCount(config));
            }));
    }

    public GetSchedulingTargetTypesBasicListData(config: SchedulingTargetTypeQueryParameters | any = null) : Observable<Array<SchedulingTargetTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const schedulingTargetTypesBasicListData$ = this.requestSchedulingTargetTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, schedulingTargetTypesBasicListData$);

            return schedulingTargetTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SchedulingTargetTypeBasicListData>>;
    }


    private requestSchedulingTargetTypesBasicListData(config: SchedulingTargetTypeQueryParameters | any) : Observable<Array<SchedulingTargetTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetTypeBasicListData>>(this.baseUrl + 'api/SchedulingTargetTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetTypesBasicListData(config));
            }));

    }


    public PutSchedulingTargetType(id: bigint | number, schedulingTargetType: SchedulingTargetTypeSubmitData) : Observable<SchedulingTargetTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SchedulingTargetTypeData>(this.baseUrl + 'api/SchedulingTargetType/' + id.toString(), schedulingTargetType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSchedulingTargetType(id, schedulingTargetType));
            }));
    }


    public PostSchedulingTargetType(schedulingTargetType: SchedulingTargetTypeSubmitData) : Observable<SchedulingTargetTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SchedulingTargetTypeData>(this.baseUrl + 'api/SchedulingTargetType', schedulingTargetType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSchedulingTargetType(schedulingTargetType));
            }));
    }

  
    public DeleteSchedulingTargetType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SchedulingTargetType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSchedulingTargetType(id));
            }));
    }


    private getConfigHash(config: SchedulingTargetTypeQueryParameters | any): string {

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

    public userIsSchedulerSchedulingTargetTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerSchedulingTargetTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.SchedulingTargetTypes
        //
        if (userIsSchedulerSchedulingTargetTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerSchedulingTargetTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerSchedulingTargetTypeReader = false;
            }
        }

        return userIsSchedulerSchedulingTargetTypeReader;
    }


    public userIsSchedulerSchedulingTargetTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerSchedulingTargetTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.SchedulingTargetTypes
        //
        if (userIsSchedulerSchedulingTargetTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerSchedulingTargetTypeWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerSchedulingTargetTypeWriter = false;
          }      
        }

        return userIsSchedulerSchedulingTargetTypeWriter;
    }

    public GetSchedulingTargetsForSchedulingTargetType(schedulingTargetTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetData[]> {
        return this.schedulingTargetService.GetSchedulingTargetList({
            schedulingTargetTypeId: schedulingTargetTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventTemplatesForSchedulingTargetType(schedulingTargetTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventTemplateData[]> {
        return this.scheduledEventTemplateService.GetScheduledEventTemplateList({
            schedulingTargetTypeId: schedulingTargetTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SchedulingTargetTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SchedulingTargetTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SchedulingTargetTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSchedulingTargetType(raw: any): SchedulingTargetTypeData {
    if (!raw) return raw;

    //
    // Create a SchedulingTargetTypeData object instance with correct prototype
    //
    const revived = Object.create(SchedulingTargetTypeData.prototype) as SchedulingTargetTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._schedulingTargets = null;
    (revived as any)._schedulingTargetsPromise = null;
    (revived as any)._schedulingTargetsSubject = new BehaviorSubject<SchedulingTargetData[] | null>(null);

    (revived as any)._scheduledEventTemplates = null;
    (revived as any)._scheduledEventTemplatesPromise = null;
    (revived as any)._scheduledEventTemplatesSubject = new BehaviorSubject<ScheduledEventTemplateData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSchedulingTargetTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SchedulingTargets$ = (revived as any)._schedulingTargetsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargets === null && (revived as any)._schedulingTargetsPromise === null) {
                (revived as any).loadSchedulingTargets();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SchedulingTargetsCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({schedulingTargetTypeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduledEventTemplates$ = (revived as any)._scheduledEventTemplatesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventTemplates === null && (revived as any)._scheduledEventTemplatesPromise === null) {
                (revived as any).loadScheduledEventTemplates();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventTemplatesCount$ = ScheduledEventTemplateService.Instance.GetScheduledEventTemplatesRowCount({schedulingTargetTypeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveSchedulingTargetTypeList(rawList: any[]): SchedulingTargetTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSchedulingTargetType(raw));
  }

}
