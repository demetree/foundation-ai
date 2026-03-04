/*

   GENERATED SERVICE FOR THE OFFICETYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the OfficeType table.

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
import { OfficeService, OfficeData } from './office.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class OfficeTypeQueryParameters {
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
export class OfficeTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class OfficeTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. OfficeTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `officeType.OfficeTypeChildren$` — use with `| async` in templates
//        • Promise:    `officeType.OfficeTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="officeType.OfficeTypeChildren$ | async"`), or
//        • Access the promise getter (`officeType.OfficeTypeChildren` or `await officeType.OfficeTypeChildren`)
//    - Simply reading `officeType.OfficeTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await officeType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class OfficeTypeData {
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
    private _offices: OfficeData[] | null = null;
    private _officesPromise: Promise<OfficeData[]> | null  = null;
    private _officesSubject = new BehaviorSubject<OfficeData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Offices$ = this._officesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._offices === null && this._officesPromise === null) {
            this.loadOffices(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _officesCount$: Observable<bigint | number> | null = null;
    public get OfficesCount$(): Observable<bigint | number> {
        if (this._officesCount$ === null) {
            this._officesCount$ = OfficeService.Instance.GetOfficesRowCount({officeTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._officesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any OfficeTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.officeType.Reload();
  //
  //  Non Async:
  //
  //     officeType[0].Reload().then(x => {
  //        this.officeType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      OfficeTypeService.Instance.GetOfficeType(this.id, includeRelations)
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
     this._offices = null;
     this._officesPromise = null;
     this._officesSubject.next(null);
     this._officesCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Offices for this OfficeType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.officeType.Offices.then(officeTypes => { ... })
     *   or
     *   await this.officeType.officeTypes
     *
    */
    public get Offices(): Promise<OfficeData[]> {
        if (this._offices !== null) {
            return Promise.resolve(this._offices);
        }

        if (this._officesPromise !== null) {
            return this._officesPromise;
        }

        // Start the load
        this.loadOffices();

        return this._officesPromise!;
    }



    private loadOffices(): void {

        this._officesPromise = lastValueFrom(
            OfficeTypeService.Instance.GetOfficesForOfficeType(this.id)
        )
        .then(Offices => {
            this._offices = Offices ?? [];
            this._officesSubject.next(this._offices);
            return this._offices;
         })
        .catch(err => {
            this._offices = [];
            this._officesSubject.next(this._offices);
            throw err;
        })
        .finally(() => {
            this._officesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Office. Call after mutations to force refresh.
     */
    public ClearOfficesCache(): void {
        this._offices = null;
        this._officesPromise = null;
        this._officesSubject.next(this._offices);      // Emit to observable
    }

    public get HasOffices(): Promise<boolean> {
        return this.Offices.then(offices => offices.length > 0);
    }




    /**
     * Updates the state of this OfficeTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this OfficeTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): OfficeTypeSubmitData {
        return OfficeTypeService.Instance.ConvertToOfficeTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class OfficeTypeService extends SecureEndpointBase {

    private static _instance: OfficeTypeService;
    private listCache: Map<string, Observable<Array<OfficeTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<OfficeTypeBasicListData>>>;
    private recordCache: Map<string, Observable<OfficeTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private officeService: OfficeService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<OfficeTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<OfficeTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<OfficeTypeData>>();

        OfficeTypeService._instance = this;
    }

    public static get Instance(): OfficeTypeService {
      return OfficeTypeService._instance;
    }


    public ClearListCaches(config: OfficeTypeQueryParameters | null = null) {

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


    public ConvertToOfficeTypeSubmitData(data: OfficeTypeData): OfficeTypeSubmitData {

        let output = new OfficeTypeSubmitData();

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

    public GetOfficeType(id: bigint | number, includeRelations: boolean = true) : Observable<OfficeTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const officeType$ = this.requestOfficeType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get OfficeType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, officeType$);

            return officeType$;
        }

        return this.recordCache.get(configHash) as Observable<OfficeTypeData>;
    }

    private requestOfficeType(id: bigint | number, includeRelations: boolean = true) : Observable<OfficeTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<OfficeTypeData>(this.baseUrl + 'api/OfficeType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveOfficeType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeType(id, includeRelations));
            }));
    }

    public GetOfficeTypeList(config: OfficeTypeQueryParameters | any = null) : Observable<Array<OfficeTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const officeTypeList$ = this.requestOfficeTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get OfficeType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, officeTypeList$);

            return officeTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<OfficeTypeData>>;
    }


    private requestOfficeTypeList(config: OfficeTypeQueryParameters | any) : Observable <Array<OfficeTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<OfficeTypeData>>(this.baseUrl + 'api/OfficeTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveOfficeTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeTypeList(config));
            }));
    }

    public GetOfficeTypesRowCount(config: OfficeTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const officeTypesRowCount$ = this.requestOfficeTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get OfficeTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, officeTypesRowCount$);

            return officeTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestOfficeTypesRowCount(config: OfficeTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/OfficeTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeTypesRowCount(config));
            }));
    }

    public GetOfficeTypesBasicListData(config: OfficeTypeQueryParameters | any = null) : Observable<Array<OfficeTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const officeTypesBasicListData$ = this.requestOfficeTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get OfficeTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, officeTypesBasicListData$);

            return officeTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<OfficeTypeBasicListData>>;
    }


    private requestOfficeTypesBasicListData(config: OfficeTypeQueryParameters | any) : Observable<Array<OfficeTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<OfficeTypeBasicListData>>(this.baseUrl + 'api/OfficeTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeTypesBasicListData(config));
            }));

    }


    public PutOfficeType(id: bigint | number, officeType: OfficeTypeSubmitData) : Observable<OfficeTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<OfficeTypeData>(this.baseUrl + 'api/OfficeType/' + id.toString(), officeType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOfficeType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutOfficeType(id, officeType));
            }));
    }


    public PostOfficeType(officeType: OfficeTypeSubmitData) : Observable<OfficeTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<OfficeTypeData>(this.baseUrl + 'api/OfficeType', officeType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOfficeType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostOfficeType(officeType));
            }));
    }

  
    public DeleteOfficeType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/OfficeType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteOfficeType(id));
            }));
    }


    private getConfigHash(config: OfficeTypeQueryParameters | any): string {

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

    public userIsSchedulerOfficeTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerOfficeTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.OfficeTypes
        //
        if (userIsSchedulerOfficeTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerOfficeTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerOfficeTypeReader = false;
            }
        }

        return userIsSchedulerOfficeTypeReader;
    }


    public userIsSchedulerOfficeTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerOfficeTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.OfficeTypes
        //
        if (userIsSchedulerOfficeTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerOfficeTypeWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerOfficeTypeWriter = false;
          }      
        }

        return userIsSchedulerOfficeTypeWriter;
    }

    public GetOfficesForOfficeType(officeTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<OfficeData[]> {
        return this.officeService.GetOfficeList({
            officeTypeId: officeTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full OfficeTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the OfficeTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when OfficeTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveOfficeType(raw: any): OfficeTypeData {
    if (!raw) return raw;

    //
    // Create a OfficeTypeData object instance with correct prototype
    //
    const revived = Object.create(OfficeTypeData.prototype) as OfficeTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._offices = null;
    (revived as any)._officesPromise = null;
    (revived as any)._officesSubject = new BehaviorSubject<OfficeData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadOfficeTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Offices$ = (revived as any)._officesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._offices === null && (revived as any)._officesPromise === null) {
                (revived as any).loadOffices();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._officesCount$ = null;



    return revived;
  }

  private ReviveOfficeTypeList(rawList: any[]): OfficeTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveOfficeType(raw));
  }

}
