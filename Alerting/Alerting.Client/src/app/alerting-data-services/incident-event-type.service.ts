/*

   GENERATED SERVICE FOR THE INCIDENTEVENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the IncidentEventType table.

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
import { IncidentTimelineEventService, IncidentTimelineEventData } from './incident-timeline-event.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class IncidentEventTypeQueryParameters {
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
export class IncidentEventTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class IncidentEventTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. IncidentEventTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `incidentEventType.IncidentEventTypeChildren$` — use with `| async` in templates
//        • Promise:    `incidentEventType.IncidentEventTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="incidentEventType.IncidentEventTypeChildren$ | async"`), or
//        • Access the promise getter (`incidentEventType.IncidentEventTypeChildren` or `await incidentEventType.IncidentEventTypeChildren`)
//    - Simply reading `incidentEventType.IncidentEventTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await incidentEventType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class IncidentEventTypeData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _incidentTimelineEvents: IncidentTimelineEventData[] | null = null;
    private _incidentTimelineEventsPromise: Promise<IncidentTimelineEventData[]> | null  = null;
    private _incidentTimelineEventsSubject = new BehaviorSubject<IncidentTimelineEventData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public IncidentTimelineEvents$ = this._incidentTimelineEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._incidentTimelineEvents === null && this._incidentTimelineEventsPromise === null) {
            this.loadIncidentTimelineEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public IncidentTimelineEventsCount$ = IncidentTimelineEventService.Instance.GetIncidentTimelineEventsRowCount({incidentEventTypeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any IncidentEventTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.incidentEventType.Reload();
  //
  //  Non Async:
  //
  //     incidentEventType[0].Reload().then(x => {
  //        this.incidentEventType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      IncidentEventTypeService.Instance.GetIncidentEventType(this.id, includeRelations)
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
     this._incidentTimelineEvents = null;
     this._incidentTimelineEventsPromise = null;
     this._incidentTimelineEventsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the IncidentTimelineEvents for this IncidentEventType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.incidentEventType.IncidentTimelineEvents.then(incidentEventTypes => { ... })
     *   or
     *   await this.incidentEventType.incidentEventTypes
     *
    */
    public get IncidentTimelineEvents(): Promise<IncidentTimelineEventData[]> {
        if (this._incidentTimelineEvents !== null) {
            return Promise.resolve(this._incidentTimelineEvents);
        }

        if (this._incidentTimelineEventsPromise !== null) {
            return this._incidentTimelineEventsPromise;
        }

        // Start the load
        this.loadIncidentTimelineEvents();

        return this._incidentTimelineEventsPromise!;
    }



    private loadIncidentTimelineEvents(): void {

        this._incidentTimelineEventsPromise = lastValueFrom(
            IncidentEventTypeService.Instance.GetIncidentTimelineEventsForIncidentEventType(this.id)
        )
        .then(IncidentTimelineEvents => {
            this._incidentTimelineEvents = IncidentTimelineEvents ?? [];
            this._incidentTimelineEventsSubject.next(this._incidentTimelineEvents);
            return this._incidentTimelineEvents;
         })
        .catch(err => {
            this._incidentTimelineEvents = [];
            this._incidentTimelineEventsSubject.next(this._incidentTimelineEvents);
            throw err;
        })
        .finally(() => {
            this._incidentTimelineEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached IncidentTimelineEvent. Call after mutations to force refresh.
     */
    public ClearIncidentTimelineEventsCache(): void {
        this._incidentTimelineEvents = null;
        this._incidentTimelineEventsPromise = null;
        this._incidentTimelineEventsSubject.next(this._incidentTimelineEvents);      // Emit to observable
    }

    public get HasIncidentTimelineEvents(): Promise<boolean> {
        return this.IncidentTimelineEvents.then(incidentTimelineEvents => incidentTimelineEvents.length > 0);
    }




    /**
     * Updates the state of this IncidentEventTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this IncidentEventTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): IncidentEventTypeSubmitData {
        return IncidentEventTypeService.Instance.ConvertToIncidentEventTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class IncidentEventTypeService extends SecureEndpointBase {

    private static _instance: IncidentEventTypeService;
    private listCache: Map<string, Observable<Array<IncidentEventTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<IncidentEventTypeBasicListData>>>;
    private recordCache: Map<string, Observable<IncidentEventTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private incidentTimelineEventService: IncidentTimelineEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<IncidentEventTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<IncidentEventTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<IncidentEventTypeData>>();

        IncidentEventTypeService._instance = this;
    }

    public static get Instance(): IncidentEventTypeService {
      return IncidentEventTypeService._instance;
    }


    public ClearListCaches(config: IncidentEventTypeQueryParameters | null = null) {

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


    public ConvertToIncidentEventTypeSubmitData(data: IncidentEventTypeData): IncidentEventTypeSubmitData {

        let output = new IncidentEventTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetIncidentEventType(id: bigint | number, includeRelations: boolean = true) : Observable<IncidentEventTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const incidentEventType$ = this.requestIncidentEventType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IncidentEventType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, incidentEventType$);

            return incidentEventType$;
        }

        return this.recordCache.get(configHash) as Observable<IncidentEventTypeData>;
    }

    private requestIncidentEventType(id: bigint | number, includeRelations: boolean = true) : Observable<IncidentEventTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentEventTypeData>(this.baseUrl + 'api/IncidentEventType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveIncidentEventType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentEventType(id, includeRelations));
            }));
    }

    public GetIncidentEventTypeList(config: IncidentEventTypeQueryParameters | any = null) : Observable<Array<IncidentEventTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const incidentEventTypeList$ = this.requestIncidentEventTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IncidentEventType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, incidentEventTypeList$);

            return incidentEventTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<IncidentEventTypeData>>;
    }


    private requestIncidentEventTypeList(config: IncidentEventTypeQueryParameters | any) : Observable <Array<IncidentEventTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IncidentEventTypeData>>(this.baseUrl + 'api/IncidentEventTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveIncidentEventTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentEventTypeList(config));
            }));
    }

    public GetIncidentEventTypesRowCount(config: IncidentEventTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const incidentEventTypesRowCount$ = this.requestIncidentEventTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IncidentEventTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, incidentEventTypesRowCount$);

            return incidentEventTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestIncidentEventTypesRowCount(config: IncidentEventTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/IncidentEventTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentEventTypesRowCount(config));
            }));
    }

    public GetIncidentEventTypesBasicListData(config: IncidentEventTypeQueryParameters | any = null) : Observable<Array<IncidentEventTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const incidentEventTypesBasicListData$ = this.requestIncidentEventTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IncidentEventTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, incidentEventTypesBasicListData$);

            return incidentEventTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<IncidentEventTypeBasicListData>>;
    }


    private requestIncidentEventTypesBasicListData(config: IncidentEventTypeQueryParameters | any) : Observable<Array<IncidentEventTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IncidentEventTypeBasicListData>>(this.baseUrl + 'api/IncidentEventTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentEventTypesBasicListData(config));
            }));

    }


    public PutIncidentEventType(id: bigint | number, incidentEventType: IncidentEventTypeSubmitData) : Observable<IncidentEventTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IncidentEventTypeData>(this.baseUrl + 'api/IncidentEventType/' + id.toString(), incidentEventType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncidentEventType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutIncidentEventType(id, incidentEventType));
            }));
    }


    public PostIncidentEventType(incidentEventType: IncidentEventTypeSubmitData) : Observable<IncidentEventTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<IncidentEventTypeData>(this.baseUrl + 'api/IncidentEventType', incidentEventType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncidentEventType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostIncidentEventType(incidentEventType));
            }));
    }

  
    public DeleteIncidentEventType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/IncidentEventType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteIncidentEventType(id));
            }));
    }


    private getConfigHash(config: IncidentEventTypeQueryParameters | any): string {

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

    public userIsAlertingIncidentEventTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingIncidentEventTypeReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.IncidentEventTypes
        //
        if (userIsAlertingIncidentEventTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingIncidentEventTypeReader = user.readPermission >= 0;
            } else {
                userIsAlertingIncidentEventTypeReader = false;
            }
        }

        return userIsAlertingIncidentEventTypeReader;
    }


    public userIsAlertingIncidentEventTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingIncidentEventTypeWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.IncidentEventTypes
        //
        if (userIsAlertingIncidentEventTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingIncidentEventTypeWriter = user.writePermission >= 0;
          } else {
            userIsAlertingIncidentEventTypeWriter = false;
          }      
        }

        return userIsAlertingIncidentEventTypeWriter;
    }

    public GetIncidentTimelineEventsForIncidentEventType(incidentEventTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IncidentTimelineEventData[]> {
        return this.incidentTimelineEventService.GetIncidentTimelineEventList({
            incidentEventTypeId: incidentEventTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full IncidentEventTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the IncidentEventTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when IncidentEventTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveIncidentEventType(raw: any): IncidentEventTypeData {
    if (!raw) return raw;

    //
    // Create a IncidentEventTypeData object instance with correct prototype
    //
    const revived = Object.create(IncidentEventTypeData.prototype) as IncidentEventTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._incidentTimelineEvents = null;
    (revived as any)._incidentTimelineEventsPromise = null;
    (revived as any)._incidentTimelineEventsSubject = new BehaviorSubject<IncidentTimelineEventData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadIncidentEventTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).IncidentTimelineEvents$ = (revived as any)._incidentTimelineEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._incidentTimelineEvents === null && (revived as any)._incidentTimelineEventsPromise === null) {
                (revived as any).loadIncidentTimelineEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).IncidentTimelineEventsCount$ = IncidentTimelineEventService.Instance.GetIncidentTimelineEventsRowCount({incidentEventTypeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveIncidentEventTypeList(rawList: any[]): IncidentEventTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveIncidentEventType(raw));
  }

}
