/*

   GENERATED SERVICE FOR THE AUDITACCESSTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AuditAccessType table.

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
import { AuditEventService, AuditEventData } from './audit-event.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AuditAccessTypeQueryParameters {
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
export class AuditAccessTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
}


export class AuditAccessTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AuditAccessTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `auditAccessType.AuditAccessTypeChildren$` — use with `| async` in templates
//        • Promise:    `auditAccessType.AuditAccessTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="auditAccessType.AuditAccessTypeChildren$ | async"`), or
//        • Access the promise getter (`auditAccessType.AuditAccessTypeChildren` or `await auditAccessType.AuditAccessTypeChildren`)
//    - Simply reading `auditAccessType.AuditAccessTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await auditAccessType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AuditAccessTypeData {
    id!: bigint | number;
    name!: string;
    description!: string | null;

    //
    // Private lazy-loading caches for related collections
    //
    private _auditEvents: AuditEventData[] | null = null;
    private _auditEventsPromise: Promise<AuditEventData[]> | null  = null;
    private _auditEventsSubject = new BehaviorSubject<AuditEventData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public AuditEvents$ = this._auditEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._auditEvents === null && this._auditEventsPromise === null) {
            this.loadAuditEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _auditEventsCount$: Observable<bigint | number> | null = null;
    public get AuditEventsCount$(): Observable<bigint | number> {
        if (this._auditEventsCount$ === null) {
            this._auditEventsCount$ = AuditEventService.Instance.GetAuditEventsRowCount({auditAccessTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._auditEventsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AuditAccessTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.auditAccessType.Reload();
  //
  //  Non Async:
  //
  //     auditAccessType[0].Reload().then(x => {
  //        this.auditAccessType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AuditAccessTypeService.Instance.GetAuditAccessType(this.id, includeRelations)
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
     this._auditEvents = null;
     this._auditEventsPromise = null;
     this._auditEventsSubject.next(null);
     this._auditEventsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the AuditEvents for this AuditAccessType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.auditAccessType.AuditEvents.then(auditAccessTypes => { ... })
     *   or
     *   await this.auditAccessType.auditAccessTypes
     *
    */
    public get AuditEvents(): Promise<AuditEventData[]> {
        if (this._auditEvents !== null) {
            return Promise.resolve(this._auditEvents);
        }

        if (this._auditEventsPromise !== null) {
            return this._auditEventsPromise;
        }

        // Start the load
        this.loadAuditEvents();

        return this._auditEventsPromise!;
    }



    private loadAuditEvents(): void {

        this._auditEventsPromise = lastValueFrom(
            AuditAccessTypeService.Instance.GetAuditEventsForAuditAccessType(this.id)
        )
        .then(AuditEvents => {
            this._auditEvents = AuditEvents ?? [];
            this._auditEventsSubject.next(this._auditEvents);
            return this._auditEvents;
         })
        .catch(err => {
            this._auditEvents = [];
            this._auditEventsSubject.next(this._auditEvents);
            throw err;
        })
        .finally(() => {
            this._auditEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached AuditEvent. Call after mutations to force refresh.
     */
    public ClearAuditEventsCache(): void {
        this._auditEvents = null;
        this._auditEventsPromise = null;
        this._auditEventsSubject.next(this._auditEvents);      // Emit to observable
    }

    public get HasAuditEvents(): Promise<boolean> {
        return this.AuditEvents.then(auditEvents => auditEvents.length > 0);
    }




    /**
     * Updates the state of this AuditAccessTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AuditAccessTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AuditAccessTypeSubmitData {
        return AuditAccessTypeService.Instance.ConvertToAuditAccessTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AuditAccessTypeService extends SecureEndpointBase {

    private static _instance: AuditAccessTypeService;
    private listCache: Map<string, Observable<Array<AuditAccessTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AuditAccessTypeBasicListData>>>;
    private recordCache: Map<string, Observable<AuditAccessTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private auditEventService: AuditEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AuditAccessTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AuditAccessTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<AuditAccessTypeData>>();

        AuditAccessTypeService._instance = this;
    }

    public static get Instance(): AuditAccessTypeService {
      return AuditAccessTypeService._instance;
    }


    public ClearListCaches(config: AuditAccessTypeQueryParameters | null = null) {

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


    public ConvertToAuditAccessTypeSubmitData(data: AuditAccessTypeData): AuditAccessTypeSubmitData {

        let output = new AuditAccessTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;

        return output;
    }

    public GetAuditAccessType(id: bigint | number, includeRelations: boolean = true) : Observable<AuditAccessTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const auditAccessType$ = this.requestAuditAccessType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditAccessType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, auditAccessType$);

            return auditAccessType$;
        }

        return this.recordCache.get(configHash) as Observable<AuditAccessTypeData>;
    }

    private requestAuditAccessType(id: bigint | number, includeRelations: boolean = true) : Observable<AuditAccessTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AuditAccessTypeData>(this.baseUrl + 'api/AuditAccessType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAuditAccessType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditAccessType(id, includeRelations));
            }));
    }

    public GetAuditAccessTypeList(config: AuditAccessTypeQueryParameters | any = null) : Observable<Array<AuditAccessTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const auditAccessTypeList$ = this.requestAuditAccessTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditAccessType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, auditAccessTypeList$);

            return auditAccessTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AuditAccessTypeData>>;
    }


    private requestAuditAccessTypeList(config: AuditAccessTypeQueryParameters | any) : Observable <Array<AuditAccessTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditAccessTypeData>>(this.baseUrl + 'api/AuditAccessTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAuditAccessTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditAccessTypeList(config));
            }));
    }

    public GetAuditAccessTypesRowCount(config: AuditAccessTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const auditAccessTypesRowCount$ = this.requestAuditAccessTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditAccessTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, auditAccessTypesRowCount$);

            return auditAccessTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAuditAccessTypesRowCount(config: AuditAccessTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AuditAccessTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditAccessTypesRowCount(config));
            }));
    }

    public GetAuditAccessTypesBasicListData(config: AuditAccessTypeQueryParameters | any = null) : Observable<Array<AuditAccessTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const auditAccessTypesBasicListData$ = this.requestAuditAccessTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditAccessTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, auditAccessTypesBasicListData$);

            return auditAccessTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AuditAccessTypeBasicListData>>;
    }


    private requestAuditAccessTypesBasicListData(config: AuditAccessTypeQueryParameters | any) : Observable<Array<AuditAccessTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditAccessTypeBasicListData>>(this.baseUrl + 'api/AuditAccessTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditAccessTypesBasicListData(config));
            }));

    }


    public PutAuditAccessType(id: bigint | number, auditAccessType: AuditAccessTypeSubmitData) : Observable<AuditAccessTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AuditAccessTypeData>(this.baseUrl + 'api/AuditAccessType/' + id.toString(), auditAccessType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditAccessType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAuditAccessType(id, auditAccessType));
            }));
    }


    public PostAuditAccessType(auditAccessType: AuditAccessTypeSubmitData) : Observable<AuditAccessTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AuditAccessTypeData>(this.baseUrl + 'api/AuditAccessType', auditAccessType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditAccessType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAuditAccessType(auditAccessType));
            }));
    }

  
    public DeleteAuditAccessType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AuditAccessType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAuditAccessType(id));
            }));
    }


    private getConfigHash(config: AuditAccessTypeQueryParameters | any): string {

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

    public userIsAuditorAuditAccessTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAuditorAuditAccessTypeReader = this.authService.isAuditorReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Auditor.AuditAccessTypes
        //
        if (userIsAuditorAuditAccessTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAuditorAuditAccessTypeReader = user.readPermission >= 0;
            } else {
                userIsAuditorAuditAccessTypeReader = false;
            }
        }

        return userIsAuditorAuditAccessTypeReader;
    }


    public userIsAuditorAuditAccessTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAuditorAuditAccessTypeWriter = this.authService.isAuditorReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Auditor.AuditAccessTypes
        //
        if (userIsAuditorAuditAccessTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAuditorAuditAccessTypeWriter = user.writePermission >= 0;
          } else {
            userIsAuditorAuditAccessTypeWriter = false;
          }      
        }

        return userIsAuditorAuditAccessTypeWriter;
    }

    public GetAuditEventsForAuditAccessType(auditAccessTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AuditEventData[]> {
        return this.auditEventService.GetAuditEventList({
            auditAccessTypeId: auditAccessTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AuditAccessTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AuditAccessTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AuditAccessTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAuditAccessType(raw: any): AuditAccessTypeData {
    if (!raw) return raw;

    //
    // Create a AuditAccessTypeData object instance with correct prototype
    //
    const revived = Object.create(AuditAccessTypeData.prototype) as AuditAccessTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._auditEvents = null;
    (revived as any)._auditEventsPromise = null;
    (revived as any)._auditEventsSubject = new BehaviorSubject<AuditEventData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAuditAccessTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).AuditEvents$ = (revived as any)._auditEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._auditEvents === null && (revived as any)._auditEventsPromise === null) {
                (revived as any).loadAuditEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._auditEventsCount$ = null;



    return revived;
  }

  private ReviveAuditAccessTypeList(rawList: any[]): AuditAccessTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAuditAccessType(raw));
  }

}
