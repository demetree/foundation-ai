/*

   GENERATED SERVICE FOR THE CONNECTORTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ConnectorType table.

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
import { ConnectorTypeCompatibilityService, ConnectorTypeCompatibilityData } from './connector-type-compatibility.service';
import { BrickPartConnectorService, BrickPartConnectorData } from './brick-part-connector.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ConnectorTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    degreesOfFreedom: bigint | number | null | undefined = null;
    allowsRotation: boolean | null | undefined = null;
    allowsSlide: boolean | null | undefined = null;
    minAngleDegrees: number | null | undefined = null;
    maxAngleDegrees: number | null | undefined = null;
    snapIncrementDegrees: number | null | undefined = null;
    clutchForceNewtons: number | null | undefined = null;
    maleOrFemale: string | null | undefined = null;
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
export class ConnectorTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    degreesOfFreedom: bigint | number | null = null;
    allowsRotation!: boolean;
    allowsSlide!: boolean;
    minAngleDegrees: number | null = null;
    maxAngleDegrees: number | null = null;
    snapIncrementDegrees: number | null = null;
    clutchForceNewtons: number | null = null;
    maleOrFemale: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ConnectorTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ConnectorTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `connectorType.ConnectorTypeChildren$` — use with `| async` in templates
//        • Promise:    `connectorType.ConnectorTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="connectorType.ConnectorTypeChildren$ | async"`), or
//        • Access the promise getter (`connectorType.ConnectorTypeChildren` or `await connectorType.ConnectorTypeChildren`)
//    - Simply reading `connectorType.ConnectorTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await connectorType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ConnectorTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    degreesOfFreedom!: bigint | number;
    allowsRotation!: boolean;
    allowsSlide!: boolean;
    minAngleDegrees!: number | null;
    maxAngleDegrees!: number | null;
    snapIncrementDegrees!: number | null;
    clutchForceNewtons!: number | null;
    maleOrFemale!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _connectorTypeCompatibilityMaleConnectorTypes: ConnectorTypeCompatibilityData[] | null = null;
    private _connectorTypeCompatibilityMaleConnectorTypesPromise: Promise<ConnectorTypeCompatibilityData[]> | null  = null;
    private _connectorTypeCompatibilityMaleConnectorTypesSubject = new BehaviorSubject<ConnectorTypeCompatibilityData[] | null>(null);
                    
    private _connectorTypeCompatibilityFemaleConnectorTypes: ConnectorTypeCompatibilityData[] | null = null;
    private _connectorTypeCompatibilityFemaleConnectorTypesPromise: Promise<ConnectorTypeCompatibilityData[]> | null  = null;
    private _connectorTypeCompatibilityFemaleConnectorTypesSubject = new BehaviorSubject<ConnectorTypeCompatibilityData[] | null>(null);
                    
    private _brickPartConnectors: BrickPartConnectorData[] | null = null;
    private _brickPartConnectorsPromise: Promise<BrickPartConnectorData[]> | null  = null;
    private _brickPartConnectorsSubject = new BehaviorSubject<BrickPartConnectorData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ConnectorTypeCompatibilityMaleConnectorTypes$ = this._connectorTypeCompatibilityMaleConnectorTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._connectorTypeCompatibilityMaleConnectorTypes === null && this._connectorTypeCompatibilityMaleConnectorTypesPromise === null) {
            this.loadConnectorTypeCompatibilityMaleConnectorTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _connectorTypeCompatibilityMaleConnectorTypesCount$: Observable<bigint | number> | null = null;
    public get ConnectorTypeCompatibilityMaleConnectorTypesCount$(): Observable<bigint | number> {
        if (this._connectorTypeCompatibilityMaleConnectorTypesCount$ === null) {
            this._connectorTypeCompatibilityMaleConnectorTypesCount$ = ConnectorTypeCompatibilityService.Instance.GetConnectorTypeCompatibilitiesRowCount({maleConnectorTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._connectorTypeCompatibilityMaleConnectorTypesCount$;
    }


    public ConnectorTypeCompatibilityFemaleConnectorTypes$ = this._connectorTypeCompatibilityFemaleConnectorTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._connectorTypeCompatibilityFemaleConnectorTypes === null && this._connectorTypeCompatibilityFemaleConnectorTypesPromise === null) {
            this.loadConnectorTypeCompatibilityFemaleConnectorTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _connectorTypeCompatibilityFemaleConnectorTypesCount$: Observable<bigint | number> | null = null;
    public get ConnectorTypeCompatibilityFemaleConnectorTypesCount$(): Observable<bigint | number> {
        if (this._connectorTypeCompatibilityFemaleConnectorTypesCount$ === null) {
            this._connectorTypeCompatibilityFemaleConnectorTypesCount$ = ConnectorTypeCompatibilityService.Instance.GetConnectorTypeCompatibilitiesRowCount({femaleConnectorTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._connectorTypeCompatibilityFemaleConnectorTypesCount$;
    }


    public BrickPartConnectors$ = this._brickPartConnectorsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickPartConnectors === null && this._brickPartConnectorsPromise === null) {
            this.loadBrickPartConnectors(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _brickPartConnectorsCount$: Observable<bigint | number> | null = null;
    public get BrickPartConnectorsCount$(): Observable<bigint | number> {
        if (this._brickPartConnectorsCount$ === null) {
            this._brickPartConnectorsCount$ = BrickPartConnectorService.Instance.GetBrickPartConnectorsRowCount({connectorTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._brickPartConnectorsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ConnectorTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.connectorType.Reload();
  //
  //  Non Async:
  //
  //     connectorType[0].Reload().then(x => {
  //        this.connectorType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ConnectorTypeService.Instance.GetConnectorType(this.id, includeRelations)
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
     this._connectorTypeCompatibilityMaleConnectorTypes = null;
     this._connectorTypeCompatibilityMaleConnectorTypesPromise = null;
     this._connectorTypeCompatibilityMaleConnectorTypesSubject.next(null);
     this._connectorTypeCompatibilityMaleConnectorTypesCount$ = null;

     this._connectorTypeCompatibilityFemaleConnectorTypes = null;
     this._connectorTypeCompatibilityFemaleConnectorTypesPromise = null;
     this._connectorTypeCompatibilityFemaleConnectorTypesSubject.next(null);
     this._connectorTypeCompatibilityFemaleConnectorTypesCount$ = null;

     this._brickPartConnectors = null;
     this._brickPartConnectorsPromise = null;
     this._brickPartConnectorsSubject.next(null);
     this._brickPartConnectorsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ConnectorTypeCompatibilityMaleConnectorTypes for this ConnectorType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.connectorType.ConnectorTypeCompatibilityMaleConnectorTypes.then(maleConnectorTypes => { ... })
     *   or
     *   await this.connectorType.maleConnectorTypes
     *
    */
    public get ConnectorTypeCompatibilityMaleConnectorTypes(): Promise<ConnectorTypeCompatibilityData[]> {
        if (this._connectorTypeCompatibilityMaleConnectorTypes !== null) {
            return Promise.resolve(this._connectorTypeCompatibilityMaleConnectorTypes);
        }

        if (this._connectorTypeCompatibilityMaleConnectorTypesPromise !== null) {
            return this._connectorTypeCompatibilityMaleConnectorTypesPromise;
        }

        // Start the load
        this.loadConnectorTypeCompatibilityMaleConnectorTypes();

        return this._connectorTypeCompatibilityMaleConnectorTypesPromise!;
    }



    private loadConnectorTypeCompatibilityMaleConnectorTypes(): void {

        this._connectorTypeCompatibilityMaleConnectorTypesPromise = lastValueFrom(
            ConnectorTypeService.Instance.GetConnectorTypeCompatibilityMaleConnectorTypesForConnectorType(this.id)
        )
        .then(ConnectorTypeCompatibilityMaleConnectorTypes => {
            this._connectorTypeCompatibilityMaleConnectorTypes = ConnectorTypeCompatibilityMaleConnectorTypes ?? [];
            this._connectorTypeCompatibilityMaleConnectorTypesSubject.next(this._connectorTypeCompatibilityMaleConnectorTypes);
            return this._connectorTypeCompatibilityMaleConnectorTypes;
         })
        .catch(err => {
            this._connectorTypeCompatibilityMaleConnectorTypes = [];
            this._connectorTypeCompatibilityMaleConnectorTypesSubject.next(this._connectorTypeCompatibilityMaleConnectorTypes);
            throw err;
        })
        .finally(() => {
            this._connectorTypeCompatibilityMaleConnectorTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConnectorTypeCompatibilityMaleConnectorType. Call after mutations to force refresh.
     */
    public ClearConnectorTypeCompatibilityMaleConnectorTypesCache(): void {
        this._connectorTypeCompatibilityMaleConnectorTypes = null;
        this._connectorTypeCompatibilityMaleConnectorTypesPromise = null;
        this._connectorTypeCompatibilityMaleConnectorTypesSubject.next(this._connectorTypeCompatibilityMaleConnectorTypes);      // Emit to observable
    }

    public get HasConnectorTypeCompatibilityMaleConnectorTypes(): Promise<boolean> {
        return this.ConnectorTypeCompatibilityMaleConnectorTypes.then(connectorTypeCompatibilityMaleConnectorTypes => connectorTypeCompatibilityMaleConnectorTypes.length > 0);
    }


    /**
     *
     * Gets the ConnectorTypeCompatibilityFemaleConnectorTypes for this ConnectorType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.connectorType.ConnectorTypeCompatibilityFemaleConnectorTypes.then(femaleConnectorTypes => { ... })
     *   or
     *   await this.connectorType.femaleConnectorTypes
     *
    */
    public get ConnectorTypeCompatibilityFemaleConnectorTypes(): Promise<ConnectorTypeCompatibilityData[]> {
        if (this._connectorTypeCompatibilityFemaleConnectorTypes !== null) {
            return Promise.resolve(this._connectorTypeCompatibilityFemaleConnectorTypes);
        }

        if (this._connectorTypeCompatibilityFemaleConnectorTypesPromise !== null) {
            return this._connectorTypeCompatibilityFemaleConnectorTypesPromise;
        }

        // Start the load
        this.loadConnectorTypeCompatibilityFemaleConnectorTypes();

        return this._connectorTypeCompatibilityFemaleConnectorTypesPromise!;
    }



    private loadConnectorTypeCompatibilityFemaleConnectorTypes(): void {

        this._connectorTypeCompatibilityFemaleConnectorTypesPromise = lastValueFrom(
            ConnectorTypeService.Instance.GetConnectorTypeCompatibilityFemaleConnectorTypesForConnectorType(this.id)
        )
        .then(ConnectorTypeCompatibilityFemaleConnectorTypes => {
            this._connectorTypeCompatibilityFemaleConnectorTypes = ConnectorTypeCompatibilityFemaleConnectorTypes ?? [];
            this._connectorTypeCompatibilityFemaleConnectorTypesSubject.next(this._connectorTypeCompatibilityFemaleConnectorTypes);
            return this._connectorTypeCompatibilityFemaleConnectorTypes;
         })
        .catch(err => {
            this._connectorTypeCompatibilityFemaleConnectorTypes = [];
            this._connectorTypeCompatibilityFemaleConnectorTypesSubject.next(this._connectorTypeCompatibilityFemaleConnectorTypes);
            throw err;
        })
        .finally(() => {
            this._connectorTypeCompatibilityFemaleConnectorTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConnectorTypeCompatibilityFemaleConnectorType. Call after mutations to force refresh.
     */
    public ClearConnectorTypeCompatibilityFemaleConnectorTypesCache(): void {
        this._connectorTypeCompatibilityFemaleConnectorTypes = null;
        this._connectorTypeCompatibilityFemaleConnectorTypesPromise = null;
        this._connectorTypeCompatibilityFemaleConnectorTypesSubject.next(this._connectorTypeCompatibilityFemaleConnectorTypes);      // Emit to observable
    }

    public get HasConnectorTypeCompatibilityFemaleConnectorTypes(): Promise<boolean> {
        return this.ConnectorTypeCompatibilityFemaleConnectorTypes.then(connectorTypeCompatibilityFemaleConnectorTypes => connectorTypeCompatibilityFemaleConnectorTypes.length > 0);
    }


    /**
     *
     * Gets the BrickPartConnectors for this ConnectorType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.connectorType.BrickPartConnectors.then(connectorTypes => { ... })
     *   or
     *   await this.connectorType.connectorTypes
     *
    */
    public get BrickPartConnectors(): Promise<BrickPartConnectorData[]> {
        if (this._brickPartConnectors !== null) {
            return Promise.resolve(this._brickPartConnectors);
        }

        if (this._brickPartConnectorsPromise !== null) {
            return this._brickPartConnectorsPromise;
        }

        // Start the load
        this.loadBrickPartConnectors();

        return this._brickPartConnectorsPromise!;
    }



    private loadBrickPartConnectors(): void {

        this._brickPartConnectorsPromise = lastValueFrom(
            ConnectorTypeService.Instance.GetBrickPartConnectorsForConnectorType(this.id)
        )
        .then(BrickPartConnectors => {
            this._brickPartConnectors = BrickPartConnectors ?? [];
            this._brickPartConnectorsSubject.next(this._brickPartConnectors);
            return this._brickPartConnectors;
         })
        .catch(err => {
            this._brickPartConnectors = [];
            this._brickPartConnectorsSubject.next(this._brickPartConnectors);
            throw err;
        })
        .finally(() => {
            this._brickPartConnectorsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BrickPartConnector. Call after mutations to force refresh.
     */
    public ClearBrickPartConnectorsCache(): void {
        this._brickPartConnectors = null;
        this._brickPartConnectorsPromise = null;
        this._brickPartConnectorsSubject.next(this._brickPartConnectors);      // Emit to observable
    }

    public get HasBrickPartConnectors(): Promise<boolean> {
        return this.BrickPartConnectors.then(brickPartConnectors => brickPartConnectors.length > 0);
    }




    /**
     * Updates the state of this ConnectorTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ConnectorTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ConnectorTypeSubmitData {
        return ConnectorTypeService.Instance.ConvertToConnectorTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ConnectorTypeService extends SecureEndpointBase {

    private static _instance: ConnectorTypeService;
    private listCache: Map<string, Observable<Array<ConnectorTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ConnectorTypeBasicListData>>>;
    private recordCache: Map<string, Observable<ConnectorTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private connectorTypeCompatibilityService: ConnectorTypeCompatibilityService,
        private brickPartConnectorService: BrickPartConnectorService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ConnectorTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ConnectorTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<ConnectorTypeData>>();

        ConnectorTypeService._instance = this;
    }

    public static get Instance(): ConnectorTypeService {
      return ConnectorTypeService._instance;
    }


    public ClearListCaches(config: ConnectorTypeQueryParameters | null = null) {

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


    public ConvertToConnectorTypeSubmitData(data: ConnectorTypeData): ConnectorTypeSubmitData {

        let output = new ConnectorTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.degreesOfFreedom = data.degreesOfFreedom;
        output.allowsRotation = data.allowsRotation;
        output.allowsSlide = data.allowsSlide;
        output.minAngleDegrees = data.minAngleDegrees;
        output.maxAngleDegrees = data.maxAngleDegrees;
        output.snapIncrementDegrees = data.snapIncrementDegrees;
        output.clutchForceNewtons = data.clutchForceNewtons;
        output.maleOrFemale = data.maleOrFemale;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetConnectorType(id: bigint | number, includeRelations: boolean = true) : Observable<ConnectorTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const connectorType$ = this.requestConnectorType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConnectorType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, connectorType$);

            return connectorType$;
        }

        return this.recordCache.get(configHash) as Observable<ConnectorTypeData>;
    }

    private requestConnectorType(id: bigint | number, includeRelations: boolean = true) : Observable<ConnectorTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConnectorTypeData>(this.baseUrl + 'api/ConnectorType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveConnectorType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestConnectorType(id, includeRelations));
            }));
    }

    public GetConnectorTypeList(config: ConnectorTypeQueryParameters | any = null) : Observable<Array<ConnectorTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const connectorTypeList$ = this.requestConnectorTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConnectorType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, connectorTypeList$);

            return connectorTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ConnectorTypeData>>;
    }


    private requestConnectorTypeList(config: ConnectorTypeQueryParameters | any) : Observable <Array<ConnectorTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConnectorTypeData>>(this.baseUrl + 'api/ConnectorTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveConnectorTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestConnectorTypeList(config));
            }));
    }

    public GetConnectorTypesRowCount(config: ConnectorTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const connectorTypesRowCount$ = this.requestConnectorTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConnectorTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, connectorTypesRowCount$);

            return connectorTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestConnectorTypesRowCount(config: ConnectorTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ConnectorTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConnectorTypesRowCount(config));
            }));
    }

    public GetConnectorTypesBasicListData(config: ConnectorTypeQueryParameters | any = null) : Observable<Array<ConnectorTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const connectorTypesBasicListData$ = this.requestConnectorTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConnectorTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, connectorTypesBasicListData$);

            return connectorTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ConnectorTypeBasicListData>>;
    }


    private requestConnectorTypesBasicListData(config: ConnectorTypeQueryParameters | any) : Observable<Array<ConnectorTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConnectorTypeBasicListData>>(this.baseUrl + 'api/ConnectorTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConnectorTypesBasicListData(config));
            }));

    }


    public PutConnectorType(id: bigint | number, connectorType: ConnectorTypeSubmitData) : Observable<ConnectorTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConnectorTypeData>(this.baseUrl + 'api/ConnectorType/' + id.toString(), connectorType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConnectorType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutConnectorType(id, connectorType));
            }));
    }


    public PostConnectorType(connectorType: ConnectorTypeSubmitData) : Observable<ConnectorTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ConnectorTypeData>(this.baseUrl + 'api/ConnectorType', connectorType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConnectorType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostConnectorType(connectorType));
            }));
    }

  
    public DeleteConnectorType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ConnectorType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteConnectorType(id));
            }));
    }


    private getConfigHash(config: ConnectorTypeQueryParameters | any): string {

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

    public userIsBMCConnectorTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCConnectorTypeReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ConnectorTypes
        //
        if (userIsBMCConnectorTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCConnectorTypeReader = user.readPermission >= 1;
            } else {
                userIsBMCConnectorTypeReader = false;
            }
        }

        return userIsBMCConnectorTypeReader;
    }


    public userIsBMCConnectorTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCConnectorTypeWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ConnectorTypes
        //
        if (userIsBMCConnectorTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCConnectorTypeWriter = user.writePermission >= 255;
          } else {
            userIsBMCConnectorTypeWriter = false;
          }      
        }

        return userIsBMCConnectorTypeWriter;
    }

    public GetConnectorTypeCompatibilityMaleConnectorTypesForConnectorType(connectorTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConnectorTypeCompatibilityData[]> {
        return this.connectorTypeCompatibilityService.GetConnectorTypeCompatibilityList({
            maleConnectorTypeId: connectorTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConnectorTypeCompatibilityFemaleConnectorTypesForConnectorType(connectorTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConnectorTypeCompatibilityData[]> {
        return this.connectorTypeCompatibilityService.GetConnectorTypeCompatibilityList({
            femaleConnectorTypeId: connectorTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBrickPartConnectorsForConnectorType(connectorTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickPartConnectorData[]> {
        return this.brickPartConnectorService.GetBrickPartConnectorList({
            connectorTypeId: connectorTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ConnectorTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ConnectorTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ConnectorTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveConnectorType(raw: any): ConnectorTypeData {
    if (!raw) return raw;

    //
    // Create a ConnectorTypeData object instance with correct prototype
    //
    const revived = Object.create(ConnectorTypeData.prototype) as ConnectorTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._connectorTypeCompatibilityMaleConnectorTypes = null;
    (revived as any)._connectorTypeCompatibilityMaleConnectorTypesPromise = null;
    (revived as any)._connectorTypeCompatibilityMaleConnectorTypesSubject = new BehaviorSubject<ConnectorTypeCompatibilityData[] | null>(null);

    (revived as any)._connectorTypeCompatibilityFemaleConnectorTypes = null;
    (revived as any)._connectorTypeCompatibilityFemaleConnectorTypesPromise = null;
    (revived as any)._connectorTypeCompatibilityFemaleConnectorTypesSubject = new BehaviorSubject<ConnectorTypeCompatibilityData[] | null>(null);

    (revived as any)._brickPartConnectors = null;
    (revived as any)._brickPartConnectorsPromise = null;
    (revived as any)._brickPartConnectorsSubject = new BehaviorSubject<BrickPartConnectorData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadConnectorTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ConnectorTypeCompatibilityMaleConnectorTypes$ = (revived as any)._connectorTypeCompatibilityMaleConnectorTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._connectorTypeCompatibilityMaleConnectorTypes === null && (revived as any)._connectorTypeCompatibilityMaleConnectorTypesPromise === null) {
                (revived as any).loadConnectorTypeCompatibilityMaleConnectorTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._connectorTypeCompatibilityMaleConnectorTypesCount$ = null;


    (revived as any).ConnectorTypeCompatibilityFemaleConnectorTypes$ = (revived as any)._connectorTypeCompatibilityFemaleConnectorTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._connectorTypeCompatibilityFemaleConnectorTypes === null && (revived as any)._connectorTypeCompatibilityFemaleConnectorTypesPromise === null) {
                (revived as any).loadConnectorTypeCompatibilityFemaleConnectorTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._connectorTypeCompatibilityFemaleConnectorTypesCount$ = null;


    (revived as any).BrickPartConnectors$ = (revived as any)._brickPartConnectorsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickPartConnectors === null && (revived as any)._brickPartConnectorsPromise === null) {
                (revived as any).loadBrickPartConnectors();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._brickPartConnectorsCount$ = null;



    return revived;
  }

  private ReviveConnectorTypeList(rawList: any[]): ConnectorTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveConnectorType(raw));
  }

}
