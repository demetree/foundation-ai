/*

   GENERATED SERVICE FOR THE INCIDENTSTATUSTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the IncidentStatusType table.

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
import { IncidentService, IncidentData } from './incident.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class IncidentStatusTypeQueryParameters {
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
export class IncidentStatusTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class IncidentStatusTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. IncidentStatusTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `incidentStatusType.IncidentStatusTypeChildren$` — use with `| async` in templates
//        • Promise:    `incidentStatusType.IncidentStatusTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="incidentStatusType.IncidentStatusTypeChildren$ | async"`), or
//        • Access the promise getter (`incidentStatusType.IncidentStatusTypeChildren` or `await incidentStatusType.IncidentStatusTypeChildren`)
//    - Simply reading `incidentStatusType.IncidentStatusTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await incidentStatusType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class IncidentStatusTypeData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _incidents: IncidentData[] | null = null;
    private _incidentsPromise: Promise<IncidentData[]> | null  = null;
    private _incidentsSubject = new BehaviorSubject<IncidentData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Incidents$ = this._incidentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._incidents === null && this._incidentsPromise === null) {
            this.loadIncidents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public IncidentsCount$ = IncidentService.Instance.GetIncidentsRowCount({incidentStatusTypeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any IncidentStatusTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.incidentStatusType.Reload();
  //
  //  Non Async:
  //
  //     incidentStatusType[0].Reload().then(x => {
  //        this.incidentStatusType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      IncidentStatusTypeService.Instance.GetIncidentStatusType(this.id, includeRelations)
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
     this._incidents = null;
     this._incidentsPromise = null;
     this._incidentsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Incidents for this IncidentStatusType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.incidentStatusType.Incidents.then(incidentStatusTypes => { ... })
     *   or
     *   await this.incidentStatusType.incidentStatusTypes
     *
    */
    public get Incidents(): Promise<IncidentData[]> {
        if (this._incidents !== null) {
            return Promise.resolve(this._incidents);
        }

        if (this._incidentsPromise !== null) {
            return this._incidentsPromise;
        }

        // Start the load
        this.loadIncidents();

        return this._incidentsPromise!;
    }



    private loadIncidents(): void {

        this._incidentsPromise = lastValueFrom(
            IncidentStatusTypeService.Instance.GetIncidentsForIncidentStatusType(this.id)
        )
        .then(Incidents => {
            this._incidents = Incidents ?? [];
            this._incidentsSubject.next(this._incidents);
            return this._incidents;
         })
        .catch(err => {
            this._incidents = [];
            this._incidentsSubject.next(this._incidents);
            throw err;
        })
        .finally(() => {
            this._incidentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Incident. Call after mutations to force refresh.
     */
    public ClearIncidentsCache(): void {
        this._incidents = null;
        this._incidentsPromise = null;
        this._incidentsSubject.next(this._incidents);      // Emit to observable
    }

    public get HasIncidents(): Promise<boolean> {
        return this.Incidents.then(incidents => incidents.length > 0);
    }




    /**
     * Updates the state of this IncidentStatusTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this IncidentStatusTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): IncidentStatusTypeSubmitData {
        return IncidentStatusTypeService.Instance.ConvertToIncidentStatusTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class IncidentStatusTypeService extends SecureEndpointBase {

    private static _instance: IncidentStatusTypeService;
    private listCache: Map<string, Observable<Array<IncidentStatusTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<IncidentStatusTypeBasicListData>>>;
    private recordCache: Map<string, Observable<IncidentStatusTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private incidentService: IncidentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<IncidentStatusTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<IncidentStatusTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<IncidentStatusTypeData>>();

        IncidentStatusTypeService._instance = this;
    }

    public static get Instance(): IncidentStatusTypeService {
      return IncidentStatusTypeService._instance;
    }


    public ClearListCaches(config: IncidentStatusTypeQueryParameters | null = null) {

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


    public ConvertToIncidentStatusTypeSubmitData(data: IncidentStatusTypeData): IncidentStatusTypeSubmitData {

        let output = new IncidentStatusTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetIncidentStatusType(id: bigint | number, includeRelations: boolean = true) : Observable<IncidentStatusTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const incidentStatusType$ = this.requestIncidentStatusType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IncidentStatusType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, incidentStatusType$);

            return incidentStatusType$;
        }

        return this.recordCache.get(configHash) as Observable<IncidentStatusTypeData>;
    }

    private requestIncidentStatusType(id: bigint | number, includeRelations: boolean = true) : Observable<IncidentStatusTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentStatusTypeData>(this.baseUrl + 'api/IncidentStatusType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveIncidentStatusType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentStatusType(id, includeRelations));
            }));
    }

    public GetIncidentStatusTypeList(config: IncidentStatusTypeQueryParameters | any = null) : Observable<Array<IncidentStatusTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const incidentStatusTypeList$ = this.requestIncidentStatusTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IncidentStatusType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, incidentStatusTypeList$);

            return incidentStatusTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<IncidentStatusTypeData>>;
    }


    private requestIncidentStatusTypeList(config: IncidentStatusTypeQueryParameters | any) : Observable <Array<IncidentStatusTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IncidentStatusTypeData>>(this.baseUrl + 'api/IncidentStatusTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveIncidentStatusTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentStatusTypeList(config));
            }));
    }

    public GetIncidentStatusTypesRowCount(config: IncidentStatusTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const incidentStatusTypesRowCount$ = this.requestIncidentStatusTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IncidentStatusTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, incidentStatusTypesRowCount$);

            return incidentStatusTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestIncidentStatusTypesRowCount(config: IncidentStatusTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/IncidentStatusTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentStatusTypesRowCount(config));
            }));
    }

    public GetIncidentStatusTypesBasicListData(config: IncidentStatusTypeQueryParameters | any = null) : Observable<Array<IncidentStatusTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const incidentStatusTypesBasicListData$ = this.requestIncidentStatusTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IncidentStatusTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, incidentStatusTypesBasicListData$);

            return incidentStatusTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<IncidentStatusTypeBasicListData>>;
    }


    private requestIncidentStatusTypesBasicListData(config: IncidentStatusTypeQueryParameters | any) : Observable<Array<IncidentStatusTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IncidentStatusTypeBasicListData>>(this.baseUrl + 'api/IncidentStatusTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentStatusTypesBasicListData(config));
            }));

    }


    public PutIncidentStatusType(id: bigint | number, incidentStatusType: IncidentStatusTypeSubmitData) : Observable<IncidentStatusTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IncidentStatusTypeData>(this.baseUrl + 'api/IncidentStatusType/' + id.toString(), incidentStatusType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncidentStatusType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutIncidentStatusType(id, incidentStatusType));
            }));
    }


    public PostIncidentStatusType(incidentStatusType: IncidentStatusTypeSubmitData) : Observable<IncidentStatusTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<IncidentStatusTypeData>(this.baseUrl + 'api/IncidentStatusType', incidentStatusType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncidentStatusType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostIncidentStatusType(incidentStatusType));
            }));
    }

  
    public DeleteIncidentStatusType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/IncidentStatusType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteIncidentStatusType(id));
            }));
    }


    private getConfigHash(config: IncidentStatusTypeQueryParameters | any): string {

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

    public userIsAlertingIncidentStatusTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingIncidentStatusTypeReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.IncidentStatusTypes
        //
        if (userIsAlertingIncidentStatusTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingIncidentStatusTypeReader = user.readPermission >= 1;
            } else {
                userIsAlertingIncidentStatusTypeReader = false;
            }
        }

        return userIsAlertingIncidentStatusTypeReader;
    }


    public userIsAlertingIncidentStatusTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingIncidentStatusTypeWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.IncidentStatusTypes
        //
        if (userIsAlertingIncidentStatusTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingIncidentStatusTypeWriter = user.writePermission >= 255;
          } else {
            userIsAlertingIncidentStatusTypeWriter = false;
          }      
        }

        return userIsAlertingIncidentStatusTypeWriter;
    }

    public GetIncidentsForIncidentStatusType(incidentStatusTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IncidentData[]> {
        return this.incidentService.GetIncidentList({
            incidentStatusTypeId: incidentStatusTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full IncidentStatusTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the IncidentStatusTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when IncidentStatusTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveIncidentStatusType(raw: any): IncidentStatusTypeData {
    if (!raw) return raw;

    //
    // Create a IncidentStatusTypeData object instance with correct prototype
    //
    const revived = Object.create(IncidentStatusTypeData.prototype) as IncidentStatusTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._incidents = null;
    (revived as any)._incidentsPromise = null;
    (revived as any)._incidentsSubject = new BehaviorSubject<IncidentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadIncidentStatusTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Incidents$ = (revived as any)._incidentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._incidents === null && (revived as any)._incidentsPromise === null) {
                (revived as any).loadIncidents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).IncidentsCount$ = IncidentService.Instance.GetIncidentsRowCount({incidentStatusTypeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveIncidentStatusTypeList(rawList: any[]): IncidentStatusTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveIncidentStatusType(raw));
  }

}
