/*

   GENERATED SERVICE FOR THE ACTIVITYEVENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ActivityEventType table.

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
import { ActivityEventService, ActivityEventData } from './activity-event.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ActivityEventTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    iconCssClass: string | null | undefined = null;
    accentColor: string | null | undefined = null;
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
export class ActivityEventTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    iconCssClass: string | null = null;
    accentColor: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ActivityEventTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ActivityEventTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `activityEventType.ActivityEventTypeChildren$` — use with `| async` in templates
//        • Promise:    `activityEventType.ActivityEventTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="activityEventType.ActivityEventTypeChildren$ | async"`), or
//        • Access the promise getter (`activityEventType.ActivityEventTypeChildren` or `await activityEventType.ActivityEventTypeChildren`)
//    - Simply reading `activityEventType.ActivityEventTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await activityEventType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ActivityEventTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    iconCssClass!: string | null;
    accentColor!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _activityEvents: ActivityEventData[] | null = null;
    private _activityEventsPromise: Promise<ActivityEventData[]> | null  = null;
    private _activityEventsSubject = new BehaviorSubject<ActivityEventData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ActivityEvents$ = this._activityEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._activityEvents === null && this._activityEventsPromise === null) {
            this.loadActivityEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ActivityEventsCount$ = ActivityEventService.Instance.GetActivityEventsRowCount({activityEventTypeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ActivityEventTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.activityEventType.Reload();
  //
  //  Non Async:
  //
  //     activityEventType[0].Reload().then(x => {
  //        this.activityEventType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ActivityEventTypeService.Instance.GetActivityEventType(this.id, includeRelations)
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
     this._activityEvents = null;
     this._activityEventsPromise = null;
     this._activityEventsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ActivityEvents for this ActivityEventType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.activityEventType.ActivityEvents.then(activityEventTypes => { ... })
     *   or
     *   await this.activityEventType.activityEventTypes
     *
    */
    public get ActivityEvents(): Promise<ActivityEventData[]> {
        if (this._activityEvents !== null) {
            return Promise.resolve(this._activityEvents);
        }

        if (this._activityEventsPromise !== null) {
            return this._activityEventsPromise;
        }

        // Start the load
        this.loadActivityEvents();

        return this._activityEventsPromise!;
    }



    private loadActivityEvents(): void {

        this._activityEventsPromise = lastValueFrom(
            ActivityEventTypeService.Instance.GetActivityEventsForActivityEventType(this.id)
        )
        .then(ActivityEvents => {
            this._activityEvents = ActivityEvents ?? [];
            this._activityEventsSubject.next(this._activityEvents);
            return this._activityEvents;
         })
        .catch(err => {
            this._activityEvents = [];
            this._activityEventsSubject.next(this._activityEvents);
            throw err;
        })
        .finally(() => {
            this._activityEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ActivityEvent. Call after mutations to force refresh.
     */
    public ClearActivityEventsCache(): void {
        this._activityEvents = null;
        this._activityEventsPromise = null;
        this._activityEventsSubject.next(this._activityEvents);      // Emit to observable
    }

    public get HasActivityEvents(): Promise<boolean> {
        return this.ActivityEvents.then(activityEvents => activityEvents.length > 0);
    }




    /**
     * Updates the state of this ActivityEventTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ActivityEventTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ActivityEventTypeSubmitData {
        return ActivityEventTypeService.Instance.ConvertToActivityEventTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ActivityEventTypeService extends SecureEndpointBase {

    private static _instance: ActivityEventTypeService;
    private listCache: Map<string, Observable<Array<ActivityEventTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ActivityEventTypeBasicListData>>>;
    private recordCache: Map<string, Observable<ActivityEventTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private activityEventService: ActivityEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ActivityEventTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ActivityEventTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<ActivityEventTypeData>>();

        ActivityEventTypeService._instance = this;
    }

    public static get Instance(): ActivityEventTypeService {
      return ActivityEventTypeService._instance;
    }


    public ClearListCaches(config: ActivityEventTypeQueryParameters | null = null) {

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


    public ConvertToActivityEventTypeSubmitData(data: ActivityEventTypeData): ActivityEventTypeSubmitData {

        let output = new ActivityEventTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.iconCssClass = data.iconCssClass;
        output.accentColor = data.accentColor;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetActivityEventType(id: bigint | number, includeRelations: boolean = true) : Observable<ActivityEventTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const activityEventType$ = this.requestActivityEventType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ActivityEventType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, activityEventType$);

            return activityEventType$;
        }

        return this.recordCache.get(configHash) as Observable<ActivityEventTypeData>;
    }

    private requestActivityEventType(id: bigint | number, includeRelations: boolean = true) : Observable<ActivityEventTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ActivityEventTypeData>(this.baseUrl + 'api/ActivityEventType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveActivityEventType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestActivityEventType(id, includeRelations));
            }));
    }

    public GetActivityEventTypeList(config: ActivityEventTypeQueryParameters | any = null) : Observable<Array<ActivityEventTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const activityEventTypeList$ = this.requestActivityEventTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ActivityEventType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, activityEventTypeList$);

            return activityEventTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ActivityEventTypeData>>;
    }


    private requestActivityEventTypeList(config: ActivityEventTypeQueryParameters | any) : Observable <Array<ActivityEventTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ActivityEventTypeData>>(this.baseUrl + 'api/ActivityEventTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveActivityEventTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestActivityEventTypeList(config));
            }));
    }

    public GetActivityEventTypesRowCount(config: ActivityEventTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const activityEventTypesRowCount$ = this.requestActivityEventTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ActivityEventTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, activityEventTypesRowCount$);

            return activityEventTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestActivityEventTypesRowCount(config: ActivityEventTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ActivityEventTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestActivityEventTypesRowCount(config));
            }));
    }

    public GetActivityEventTypesBasicListData(config: ActivityEventTypeQueryParameters | any = null) : Observable<Array<ActivityEventTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const activityEventTypesBasicListData$ = this.requestActivityEventTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ActivityEventTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, activityEventTypesBasicListData$);

            return activityEventTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ActivityEventTypeBasicListData>>;
    }


    private requestActivityEventTypesBasicListData(config: ActivityEventTypeQueryParameters | any) : Observable<Array<ActivityEventTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ActivityEventTypeBasicListData>>(this.baseUrl + 'api/ActivityEventTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestActivityEventTypesBasicListData(config));
            }));

    }


    public PutActivityEventType(id: bigint | number, activityEventType: ActivityEventTypeSubmitData) : Observable<ActivityEventTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ActivityEventTypeData>(this.baseUrl + 'api/ActivityEventType/' + id.toString(), activityEventType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveActivityEventType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutActivityEventType(id, activityEventType));
            }));
    }


    public PostActivityEventType(activityEventType: ActivityEventTypeSubmitData) : Observable<ActivityEventTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ActivityEventTypeData>(this.baseUrl + 'api/ActivityEventType', activityEventType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveActivityEventType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostActivityEventType(activityEventType));
            }));
    }

  
    public DeleteActivityEventType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ActivityEventType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteActivityEventType(id));
            }));
    }


    private getConfigHash(config: ActivityEventTypeQueryParameters | any): string {

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

    public userIsBMCActivityEventTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCActivityEventTypeReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ActivityEventTypes
        //
        if (userIsBMCActivityEventTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCActivityEventTypeReader = user.readPermission >= 1;
            } else {
                userIsBMCActivityEventTypeReader = false;
            }
        }

        return userIsBMCActivityEventTypeReader;
    }


    public userIsBMCActivityEventTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCActivityEventTypeWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ActivityEventTypes
        //
        if (userIsBMCActivityEventTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCActivityEventTypeWriter = user.writePermission >= 255;
          } else {
            userIsBMCActivityEventTypeWriter = false;
          }      
        }

        return userIsBMCActivityEventTypeWriter;
    }

    public GetActivityEventsForActivityEventType(activityEventTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ActivityEventData[]> {
        return this.activityEventService.GetActivityEventList({
            activityEventTypeId: activityEventTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ActivityEventTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ActivityEventTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ActivityEventTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveActivityEventType(raw: any): ActivityEventTypeData {
    if (!raw) return raw;

    //
    // Create a ActivityEventTypeData object instance with correct prototype
    //
    const revived = Object.create(ActivityEventTypeData.prototype) as ActivityEventTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._activityEvents = null;
    (revived as any)._activityEventsPromise = null;
    (revived as any)._activityEventsSubject = new BehaviorSubject<ActivityEventData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadActivityEventTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ActivityEvents$ = (revived as any)._activityEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._activityEvents === null && (revived as any)._activityEventsPromise === null) {
                (revived as any).loadActivityEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ActivityEventsCount$ = ActivityEventService.Instance.GetActivityEventsRowCount({activityEventTypeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveActivityEventTypeList(rawList: any[]): ActivityEventTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveActivityEventType(raw));
  }

}
