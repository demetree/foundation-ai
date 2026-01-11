import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, BehaviorSubject, catchError, throwError, lastValueFrom, map  } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';
import { SecurityUserEventService, SecurityUserEventData } from './security-user-event.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SecurityUserEventTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class SecurityUserEventTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
}


export class SecurityUserEventTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SecurityUserEventTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `securityUserEventType.SecurityUserEventTypeChildren$` — use with `| async` in templates
//        • Promise:    `securityUserEventType.SecurityUserEventTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="securityUserEventType.SecurityUserEventTypeChildren$ | async"`), or
//        • Access the promise getter (`securityUserEventType.SecurityUserEventTypeChildren` or `await securityUserEventType.SecurityUserEventTypeChildren`)
//    - Simply reading `securityUserEventType.SecurityUserEventTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await securityUserEventType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SecurityUserEventTypeData {
    id!: bigint | number;
    name!: string;
    description!: string | null;

    //
    // Private lazy-loading caches for related collections
    //
    private _securityUserEvents: SecurityUserEventData[] | null = null;
    private _securityUserEventsPromise: Promise<SecurityUserEventData[]> | null  = null;
    private _securityUserEventsSubject = new BehaviorSubject<SecurityUserEventData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SecurityUserEvents$ = this._securityUserEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityUserEvents === null && this._securityUserEventsPromise === null) {
            this.loadSecurityUserEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityUserEventsCount$ = SecurityUserEventService.Instance.GetSecurityUserEventsRowCount({securityUserEventTypeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SecurityUserEventTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.securityUserEventType.Reload();
  //
  //  Non Async:
  //
  //     securityUserEventType[0].Reload().then(x => {
  //        this.securityUserEventType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SecurityUserEventTypeService.Instance.GetSecurityUserEventType(this.id, includeRelations)
    );

    // Merge fresh data into this instance (preserves reference)
    this.UpdateFrom(fresh as this);

    // Clear all lazy caches to force re-load on next access
    this.clearAllLazyCaches();

    return this;
  }


  private clearAllLazyCaches(): void {
     // Reset every collection cache and notify subscribers
     this._securityUserEvents = null;
     this._securityUserEventsPromise = null;
     this._securityUserEventsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the SecurityUserEvents for this SecurityUserEventType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityUserEventType.SecurityUserEvents.then(securityUserEvents => { ... })
     *   or
     *   await this.securityUserEventType.SecurityUserEvents
     *
    */
    public get SecurityUserEvents(): Promise<SecurityUserEventData[]> {
        if (this._securityUserEvents !== null) {
            return Promise.resolve(this._securityUserEvents);
        }

        if (this._securityUserEventsPromise !== null) {
            return this._securityUserEventsPromise;
        }

        // Start the load
        this.loadSecurityUserEvents();

        return this._securityUserEventsPromise!;
    }



    private loadSecurityUserEvents(): void {

        this._securityUserEventsPromise = lastValueFrom(
            SecurityUserEventTypeService.Instance.GetSecurityUserEventsForSecurityUserEventType(this.id)
        )
        .then(securityUserEvents => {
            this._securityUserEvents = securityUserEvents ?? [];
            this._securityUserEventsSubject.next(this._securityUserEvents);
            return this._securityUserEvents;
         })
        .catch(err => {
            this._securityUserEvents = [];
            this._securityUserEventsSubject.next(this._securityUserEvents);
            throw err;
        })
        .finally(() => {
            this._securityUserEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearSecurityUserEventsCache(): void {
        this._securityUserEvents = null;
        this._securityUserEventsPromise = null;
        this._securityUserEventsSubject.next(this._securityUserEvents);      // Emit to observable
    }

    public get HasSecurityUserEvents(): Promise<boolean> {
        return this.SecurityUserEvents.then(securityUserEvents => securityUserEvents.length > 0);
    }




    /**
     * Updates the state of this SecurityUserEventTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SecurityUserEventTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SecurityUserEventTypeSubmitData {
        return SecurityUserEventTypeService.Instance.ConvertToSecurityUserEventTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SecurityUserEventTypeService extends SecureEndpointBase {

    private static _instance: SecurityUserEventTypeService;
    private listCache: Map<string, Observable<Array<SecurityUserEventTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SecurityUserEventTypeBasicListData>>>;
    private recordCache: Map<string, Observable<SecurityUserEventTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private securityUserEventService: SecurityUserEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SecurityUserEventTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SecurityUserEventTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<SecurityUserEventTypeData>>();

        SecurityUserEventTypeService._instance = this;
    }

    public static get Instance(): SecurityUserEventTypeService {
      return SecurityUserEventTypeService._instance;
    }


    public ClearListCaches(config: SecurityUserEventTypeQueryParameters | null = null) {

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


    public ConvertToSecurityUserEventTypeSubmitData(data: SecurityUserEventTypeData): SecurityUserEventTypeSubmitData {

        let output = new SecurityUserEventTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;

        return output;
    }

    public GetSecurityUserEventType(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityUserEventTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const securityUserEventType$ = this.requestSecurityUserEventType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUserEventType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, securityUserEventType$);

            return securityUserEventType$;
        }

        return this.recordCache.get(configHash) as Observable<SecurityUserEventTypeData>;
    }

    private requestSecurityUserEventType(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityUserEventTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SecurityUserEventTypeData>(this.baseUrl + 'api/SecurityUserEventType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSecurityUserEventType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserEventType(id, includeRelations));
            }));
    }

    public GetSecurityUserEventTypeList(config: SecurityUserEventTypeQueryParameters | any = null) : Observable<Array<SecurityUserEventTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const securityUserEventTypeList$ = this.requestSecurityUserEventTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUserEventType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, securityUserEventTypeList$);

            return securityUserEventTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SecurityUserEventTypeData>>;
    }


    private requestSecurityUserEventTypeList(config: SecurityUserEventTypeQueryParameters | any) : Observable <Array<SecurityUserEventTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityUserEventTypeData>>(this.baseUrl + 'api/SecurityUserEventTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSecurityUserEventTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserEventTypeList(config));
            }));
    }

    public GetSecurityUserEventTypesRowCount(config: SecurityUserEventTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const securityUserEventTypesRowCount$ = this.requestSecurityUserEventTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUserEventTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, securityUserEventTypesRowCount$);

            return securityUserEventTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSecurityUserEventTypesRowCount(config: SecurityUserEventTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SecurityUserEventTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserEventTypesRowCount(config));
            }));
    }

    public GetSecurityUserEventTypesBasicListData(config: SecurityUserEventTypeQueryParameters | any = null) : Observable<Array<SecurityUserEventTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const securityUserEventTypesBasicListData$ = this.requestSecurityUserEventTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUserEventTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, securityUserEventTypesBasicListData$);

            return securityUserEventTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SecurityUserEventTypeBasicListData>>;
    }


    private requestSecurityUserEventTypesBasicListData(config: SecurityUserEventTypeQueryParameters | any) : Observable<Array<SecurityUserEventTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityUserEventTypeBasicListData>>(this.baseUrl + 'api/SecurityUserEventTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserEventTypesBasicListData(config));
            }));

    }


    public PutSecurityUserEventType(id: bigint | number, securityUserEventType: SecurityUserEventTypeSubmitData) : Observable<SecurityUserEventTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SecurityUserEventTypeData>(this.baseUrl + 'api/SecurityUserEventType/' + id.toString(), securityUserEventType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityUserEventType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSecurityUserEventType(id, securityUserEventType));
            }));
    }


    public PostSecurityUserEventType(securityUserEventType: SecurityUserEventTypeSubmitData) : Observable<SecurityUserEventTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SecurityUserEventTypeData>(this.baseUrl + 'api/SecurityUserEventType', securityUserEventType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityUserEventType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSecurityUserEventType(securityUserEventType));
            }));
    }

  
    public DeleteSecurityUserEventType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SecurityUserEventType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSecurityUserEventType(id));
            }));
    }


    private getConfigHash(config: SecurityUserEventTypeQueryParameters | any): string {

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

    public userIsSecuritySecurityUserEventTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecuritySecurityUserEventTypeReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.SecurityUserEventTypes
        //
        if (userIsSecuritySecurityUserEventTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecuritySecurityUserEventTypeReader = user.readPermission >= 0;
            } else {
                userIsSecuritySecurityUserEventTypeReader = false;
            }
        }

        return userIsSecuritySecurityUserEventTypeReader;
    }


    public userIsSecuritySecurityUserEventTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecuritySecurityUserEventTypeWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.SecurityUserEventTypes
        //
        if (userIsSecuritySecurityUserEventTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecuritySecurityUserEventTypeWriter = user.writePermission >= 0;
          } else {
            userIsSecuritySecurityUserEventTypeWriter = false;
          }      
        }

        return userIsSecuritySecurityUserEventTypeWriter;
    }

    public GetSecurityUserEventsForSecurityUserEventType(securityUserEventTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityUserEventData[]> {
        return this.securityUserEventService.GetSecurityUserEventList({
            securityUserEventTypeId: securityUserEventTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SecurityUserEventTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SecurityUserEventTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SecurityUserEventTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSecurityUserEventType(raw: any): SecurityUserEventTypeData {
    if (!raw) return raw;

    //
    // Create a SecurityUserEventTypeData object instance with correct prototype
    //
    const revived = Object.create(SecurityUserEventTypeData.prototype) as SecurityUserEventTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._securityUserEvents = null;
    (revived as any)._securityUserEventsPromise = null;
    (revived as any)._securityUserEventsSubject = new BehaviorSubject<SecurityUserEventData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSecurityUserEventTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SecurityUserEvents$ = (revived as any)._securityUserEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityUserEvents === null && (revived as any)._securityUserEventsPromise === null) {
                (revived as any).loadSecurityUserEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityUserEventsCount$ = SecurityUserEventService.Instance.GetSecurityUserEventsRowCount({securityUserEventTypeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveSecurityUserEventTypeList(rawList: any[]): SecurityUserEventTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSecurityUserEventType(raw));
  }

}
