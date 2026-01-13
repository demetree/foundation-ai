/*

   GENERATED SERVICE FOR THE RATETYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the RateType table.

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
import { ChargeTypeService, ChargeTypeData } from './charge-type.service';
import { RateSheetService, RateSheetData } from './rate-sheet.service';
import { EventChargeService, EventChargeData } from './event-charge.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class RateTypeQueryParameters {
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
export class RateTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class RateTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. RateTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `rateType.RateTypeChildren$` — use with `| async` in templates
//        • Promise:    `rateType.RateTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="rateType.RateTypeChildren$ | async"`), or
//        • Access the promise getter (`rateType.RateTypeChildren` or `await rateType.RateTypeChildren`)
//    - Simply reading `rateType.RateTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await rateType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class RateTypeData {
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
    private _chargeTypes: ChargeTypeData[] | null = null;
    private _chargeTypesPromise: Promise<ChargeTypeData[]> | null  = null;
    private _chargeTypesSubject = new BehaviorSubject<ChargeTypeData[] | null>(null);

                
    private _rateSheets: RateSheetData[] | null = null;
    private _rateSheetsPromise: Promise<RateSheetData[]> | null  = null;
    private _rateSheetsSubject = new BehaviorSubject<RateSheetData[] | null>(null);

                
    private _eventCharges: EventChargeData[] | null = null;
    private _eventChargesPromise: Promise<EventChargeData[]> | null  = null;
    private _eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ChargeTypes$ = this._chargeTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._chargeTypes === null && this._chargeTypesPromise === null) {
            this.loadChargeTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ChargeTypesCount$ = ChargeTypeService.Instance.GetChargeTypesRowCount({rateTypeId: this.id,
      active: true,
      deleted: false
    });



    public RateSheets$ = this._rateSheetsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._rateSheets === null && this._rateSheetsPromise === null) {
            this.loadRateSheets(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public RateSheetsCount$ = RateSheetService.Instance.GetRateSheetsRowCount({rateTypeId: this.id,
      active: true,
      deleted: false
    });



    public EventCharges$ = this._eventChargesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventCharges === null && this._eventChargesPromise === null) {
            this.loadEventCharges(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public EventChargesCount$ = EventChargeService.Instance.GetEventChargesRowCount({rateTypeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any RateTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.rateType.Reload();
  //
  //  Non Async:
  //
  //     rateType[0].Reload().then(x => {
  //        this.rateType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      RateTypeService.Instance.GetRateType(this.id, includeRelations)
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
     this._chargeTypes = null;
     this._chargeTypesPromise = null;
     this._chargeTypesSubject.next(null);

     this._rateSheets = null;
     this._rateSheetsPromise = null;
     this._rateSheetsSubject.next(null);

     this._eventCharges = null;
     this._eventChargesPromise = null;
     this._eventChargesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ChargeTypes for this RateType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.rateType.ChargeTypes.then(rateTypes => { ... })
     *   or
     *   await this.rateType.rateTypes
     *
    */
    public get ChargeTypes(): Promise<ChargeTypeData[]> {
        if (this._chargeTypes !== null) {
            return Promise.resolve(this._chargeTypes);
        }

        if (this._chargeTypesPromise !== null) {
            return this._chargeTypesPromise;
        }

        // Start the load
        this.loadChargeTypes();

        return this._chargeTypesPromise!;
    }



    private loadChargeTypes(): void {

        this._chargeTypesPromise = lastValueFrom(
            RateTypeService.Instance.GetChargeTypesForRateType(this.id)
        )
        .then(ChargeTypes => {
            this._chargeTypes = ChargeTypes ?? [];
            this._chargeTypesSubject.next(this._chargeTypes);
            return this._chargeTypes;
         })
        .catch(err => {
            this._chargeTypes = [];
            this._chargeTypesSubject.next(this._chargeTypes);
            throw err;
        })
        .finally(() => {
            this._chargeTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ChargeType. Call after mutations to force refresh.
     */
    public ClearChargeTypesCache(): void {
        this._chargeTypes = null;
        this._chargeTypesPromise = null;
        this._chargeTypesSubject.next(this._chargeTypes);      // Emit to observable
    }

    public get HasChargeTypes(): Promise<boolean> {
        return this.ChargeTypes.then(chargeTypes => chargeTypes.length > 0);
    }


    /**
     *
     * Gets the RateSheets for this RateType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.rateType.RateSheets.then(rateTypes => { ... })
     *   or
     *   await this.rateType.rateTypes
     *
    */
    public get RateSheets(): Promise<RateSheetData[]> {
        if (this._rateSheets !== null) {
            return Promise.resolve(this._rateSheets);
        }

        if (this._rateSheetsPromise !== null) {
            return this._rateSheetsPromise;
        }

        // Start the load
        this.loadRateSheets();

        return this._rateSheetsPromise!;
    }



    private loadRateSheets(): void {

        this._rateSheetsPromise = lastValueFrom(
            RateTypeService.Instance.GetRateSheetsForRateType(this.id)
        )
        .then(RateSheets => {
            this._rateSheets = RateSheets ?? [];
            this._rateSheetsSubject.next(this._rateSheets);
            return this._rateSheets;
         })
        .catch(err => {
            this._rateSheets = [];
            this._rateSheetsSubject.next(this._rateSheets);
            throw err;
        })
        .finally(() => {
            this._rateSheetsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached RateSheet. Call after mutations to force refresh.
     */
    public ClearRateSheetsCache(): void {
        this._rateSheets = null;
        this._rateSheetsPromise = null;
        this._rateSheetsSubject.next(this._rateSheets);      // Emit to observable
    }

    public get HasRateSheets(): Promise<boolean> {
        return this.RateSheets.then(rateSheets => rateSheets.length > 0);
    }


    /**
     *
     * Gets the EventCharges for this RateType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.rateType.EventCharges.then(rateTypes => { ... })
     *   or
     *   await this.rateType.rateTypes
     *
    */
    public get EventCharges(): Promise<EventChargeData[]> {
        if (this._eventCharges !== null) {
            return Promise.resolve(this._eventCharges);
        }

        if (this._eventChargesPromise !== null) {
            return this._eventChargesPromise;
        }

        // Start the load
        this.loadEventCharges();

        return this._eventChargesPromise!;
    }



    private loadEventCharges(): void {

        this._eventChargesPromise = lastValueFrom(
            RateTypeService.Instance.GetEventChargesForRateType(this.id)
        )
        .then(EventCharges => {
            this._eventCharges = EventCharges ?? [];
            this._eventChargesSubject.next(this._eventCharges);
            return this._eventCharges;
         })
        .catch(err => {
            this._eventCharges = [];
            this._eventChargesSubject.next(this._eventCharges);
            throw err;
        })
        .finally(() => {
            this._eventChargesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventCharge. Call after mutations to force refresh.
     */
    public ClearEventChargesCache(): void {
        this._eventCharges = null;
        this._eventChargesPromise = null;
        this._eventChargesSubject.next(this._eventCharges);      // Emit to observable
    }

    public get HasEventCharges(): Promise<boolean> {
        return this.EventCharges.then(eventCharges => eventCharges.length > 0);
    }




    /**
     * Updates the state of this RateTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this RateTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): RateTypeSubmitData {
        return RateTypeService.Instance.ConvertToRateTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class RateTypeService extends SecureEndpointBase {

    private static _instance: RateTypeService;
    private listCache: Map<string, Observable<Array<RateTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<RateTypeBasicListData>>>;
    private recordCache: Map<string, Observable<RateTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private chargeTypeService: ChargeTypeService,
        private rateSheetService: RateSheetService,
        private eventChargeService: EventChargeService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<RateTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<RateTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<RateTypeData>>();

        RateTypeService._instance = this;
    }

    public static get Instance(): RateTypeService {
      return RateTypeService._instance;
    }


    public ClearListCaches(config: RateTypeQueryParameters | null = null) {

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


    public ConvertToRateTypeSubmitData(data: RateTypeData): RateTypeSubmitData {

        let output = new RateTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetRateType(id: bigint | number, includeRelations: boolean = true) : Observable<RateTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const rateType$ = this.requestRateType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RateType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, rateType$);

            return rateType$;
        }

        return this.recordCache.get(configHash) as Observable<RateTypeData>;
    }

    private requestRateType(id: bigint | number, includeRelations: boolean = true) : Observable<RateTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RateTypeData>(this.baseUrl + 'api/RateType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveRateType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestRateType(id, includeRelations));
            }));
    }

    public GetRateTypeList(config: RateTypeQueryParameters | any = null) : Observable<Array<RateTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const rateTypeList$ = this.requestRateTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RateType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, rateTypeList$);

            return rateTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<RateTypeData>>;
    }


    private requestRateTypeList(config: RateTypeQueryParameters | any) : Observable <Array<RateTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RateTypeData>>(this.baseUrl + 'api/RateTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveRateTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestRateTypeList(config));
            }));
    }

    public GetRateTypesRowCount(config: RateTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const rateTypesRowCount$ = this.requestRateTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RateTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, rateTypesRowCount$);

            return rateTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestRateTypesRowCount(config: RateTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/RateTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRateTypesRowCount(config));
            }));
    }

    public GetRateTypesBasicListData(config: RateTypeQueryParameters | any = null) : Observable<Array<RateTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const rateTypesBasicListData$ = this.requestRateTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RateTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, rateTypesBasicListData$);

            return rateTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<RateTypeBasicListData>>;
    }


    private requestRateTypesBasicListData(config: RateTypeQueryParameters | any) : Observable<Array<RateTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RateTypeBasicListData>>(this.baseUrl + 'api/RateTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRateTypesBasicListData(config));
            }));

    }


    public PutRateType(id: bigint | number, rateType: RateTypeSubmitData) : Observable<RateTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RateTypeData>(this.baseUrl + 'api/RateType/' + id.toString(), rateType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRateType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutRateType(id, rateType));
            }));
    }


    public PostRateType(rateType: RateTypeSubmitData) : Observable<RateTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<RateTypeData>(this.baseUrl + 'api/RateType', rateType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRateType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostRateType(rateType));
            }));
    }

  
    public DeleteRateType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/RateType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteRateType(id));
            }));
    }


    private getConfigHash(config: RateTypeQueryParameters | any): string {

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

    public userIsSchedulerRateTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerRateTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.RateTypes
        //
        if (userIsSchedulerRateTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerRateTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerRateTypeReader = false;
            }
        }

        return userIsSchedulerRateTypeReader;
    }


    public userIsSchedulerRateTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerRateTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.RateTypes
        //
        if (userIsSchedulerRateTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerRateTypeWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerRateTypeWriter = false;
          }      
        }

        return userIsSchedulerRateTypeWriter;
    }

    public GetChargeTypesForRateType(rateTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ChargeTypeData[]> {
        return this.chargeTypeService.GetChargeTypeList({
            rateTypeId: rateTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetRateSheetsForRateType(rateTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<RateSheetData[]> {
        return this.rateSheetService.GetRateSheetList({
            rateTypeId: rateTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventChargesForRateType(rateTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventChargeData[]> {
        return this.eventChargeService.GetEventChargeList({
            rateTypeId: rateTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full RateTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the RateTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when RateTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveRateType(raw: any): RateTypeData {
    if (!raw) return raw;

    //
    // Create a RateTypeData object instance with correct prototype
    //
    const revived = Object.create(RateTypeData.prototype) as RateTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._chargeTypes = null;
    (revived as any)._chargeTypesPromise = null;
    (revived as any)._chargeTypesSubject = new BehaviorSubject<ChargeTypeData[] | null>(null);

    (revived as any)._rateSheets = null;
    (revived as any)._rateSheetsPromise = null;
    (revived as any)._rateSheetsSubject = new BehaviorSubject<RateSheetData[] | null>(null);

    (revived as any)._eventCharges = null;
    (revived as any)._eventChargesPromise = null;
    (revived as any)._eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadRateTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ChargeTypes$ = (revived as any)._chargeTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._chargeTypes === null && (revived as any)._chargeTypesPromise === null) {
                (revived as any).loadChargeTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ChargeTypesCount$ = ChargeTypeService.Instance.GetChargeTypesRowCount({rateTypeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).RateSheets$ = (revived as any)._rateSheetsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._rateSheets === null && (revived as any)._rateSheetsPromise === null) {
                (revived as any).loadRateSheets();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).RateSheetsCount$ = RateSheetService.Instance.GetRateSheetsRowCount({rateTypeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).EventCharges$ = (revived as any)._eventChargesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventCharges === null && (revived as any)._eventChargesPromise === null) {
                (revived as any).loadEventCharges();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).EventChargesCount$ = EventChargeService.Instance.GetEventChargesRowCount({rateTypeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveRateTypeList(rawList: any[]): RateTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveRateType(raw));
  }

}
