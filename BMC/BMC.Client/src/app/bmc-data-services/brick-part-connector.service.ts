/*

   GENERATED SERVICE FOR THE BRICKPARTCONNECTOR TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickPartConnector table.

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
import { BrickPartData } from './brick-part.service';
import { ConnectorTypeData } from './connector-type.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BrickPartConnectorQueryParameters {
    brickPartId: bigint | number | null | undefined = null;
    connectorTypeId: bigint | number | null | undefined = null;
    positionX: number | null | undefined = null;
    positionY: number | null | undefined = null;
    positionZ: number | null | undefined = null;
    orientationX: number | null | undefined = null;
    orientationY: number | null | undefined = null;
    orientationZ: number | null | undefined = null;
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
export class BrickPartConnectorSubmitData {
    id!: bigint | number;
    brickPartId!: bigint | number;
    connectorTypeId!: bigint | number;
    positionX: number | null = null;
    positionY: number | null = null;
    positionZ: number | null = null;
    orientationX: number | null = null;
    orientationY: number | null = null;
    orientationZ: number | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BrickPartConnectorBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickPartConnectorChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickPartConnector.BrickPartConnectorChildren$` — use with `| async` in templates
//        • Promise:    `brickPartConnector.BrickPartConnectorChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickPartConnector.BrickPartConnectorChildren$ | async"`), or
//        • Access the promise getter (`brickPartConnector.BrickPartConnectorChildren` or `await brickPartConnector.BrickPartConnectorChildren`)
//    - Simply reading `brickPartConnector.BrickPartConnectorChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickPartConnector.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickPartConnectorData {
    id!: bigint | number;
    brickPartId!: bigint | number;
    connectorTypeId!: bigint | number;
    positionX!: number | null;
    positionY!: number | null;
    positionZ!: number | null;
    orientationX!: number | null;
    orientationY!: number | null;
    orientationZ!: number | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    brickPart: BrickPartData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    connectorType: ConnectorTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //

  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any BrickPartConnectorData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickPartConnector.Reload();
  //
  //  Non Async:
  //
  //     brickPartConnector[0].Reload().then(x => {
  //        this.brickPartConnector = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickPartConnectorService.Instance.GetBrickPartConnector(this.id, includeRelations)
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
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //


    /**
     * Updates the state of this BrickPartConnectorData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickPartConnectorData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickPartConnectorSubmitData {
        return BrickPartConnectorService.Instance.ConvertToBrickPartConnectorSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickPartConnectorService extends SecureEndpointBase {

    private static _instance: BrickPartConnectorService;
    private listCache: Map<string, Observable<Array<BrickPartConnectorData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickPartConnectorBasicListData>>>;
    private recordCache: Map<string, Observable<BrickPartConnectorData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickPartConnectorData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickPartConnectorBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickPartConnectorData>>();

        BrickPartConnectorService._instance = this;
    }

    public static get Instance(): BrickPartConnectorService {
      return BrickPartConnectorService._instance;
    }


    public ClearListCaches(config: BrickPartConnectorQueryParameters | null = null) {

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


    public ConvertToBrickPartConnectorSubmitData(data: BrickPartConnectorData): BrickPartConnectorSubmitData {

        let output = new BrickPartConnectorSubmitData();

        output.id = data.id;
        output.brickPartId = data.brickPartId;
        output.connectorTypeId = data.connectorTypeId;
        output.positionX = data.positionX;
        output.positionY = data.positionY;
        output.positionZ = data.positionZ;
        output.orientationX = data.orientationX;
        output.orientationY = data.orientationY;
        output.orientationZ = data.orientationZ;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBrickPartConnector(id: bigint | number, includeRelations: boolean = true) : Observable<BrickPartConnectorData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickPartConnector$ = this.requestBrickPartConnector(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickPartConnector", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickPartConnector$);

            return brickPartConnector$;
        }

        return this.recordCache.get(configHash) as Observable<BrickPartConnectorData>;
    }

    private requestBrickPartConnector(id: bigint | number, includeRelations: boolean = true) : Observable<BrickPartConnectorData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickPartConnectorData>(this.baseUrl + 'api/BrickPartConnector/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickPartConnector(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickPartConnector(id, includeRelations));
            }));
    }

    public GetBrickPartConnectorList(config: BrickPartConnectorQueryParameters | any = null) : Observable<Array<BrickPartConnectorData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickPartConnectorList$ = this.requestBrickPartConnectorList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickPartConnector list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickPartConnectorList$);

            return brickPartConnectorList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickPartConnectorData>>;
    }


    private requestBrickPartConnectorList(config: BrickPartConnectorQueryParameters | any) : Observable <Array<BrickPartConnectorData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickPartConnectorData>>(this.baseUrl + 'api/BrickPartConnectors', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickPartConnectorList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickPartConnectorList(config));
            }));
    }

    public GetBrickPartConnectorsRowCount(config: BrickPartConnectorQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickPartConnectorsRowCount$ = this.requestBrickPartConnectorsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickPartConnectors row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickPartConnectorsRowCount$);

            return brickPartConnectorsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickPartConnectorsRowCount(config: BrickPartConnectorQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickPartConnectors/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickPartConnectorsRowCount(config));
            }));
    }

    public GetBrickPartConnectorsBasicListData(config: BrickPartConnectorQueryParameters | any = null) : Observable<Array<BrickPartConnectorBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickPartConnectorsBasicListData$ = this.requestBrickPartConnectorsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickPartConnectors basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickPartConnectorsBasicListData$);

            return brickPartConnectorsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickPartConnectorBasicListData>>;
    }


    private requestBrickPartConnectorsBasicListData(config: BrickPartConnectorQueryParameters | any) : Observable<Array<BrickPartConnectorBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickPartConnectorBasicListData>>(this.baseUrl + 'api/BrickPartConnectors/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickPartConnectorsBasicListData(config));
            }));

    }


    public PutBrickPartConnector(id: bigint | number, brickPartConnector: BrickPartConnectorSubmitData) : Observable<BrickPartConnectorData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickPartConnectorData>(this.baseUrl + 'api/BrickPartConnector/' + id.toString(), brickPartConnector, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickPartConnector(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickPartConnector(id, brickPartConnector));
            }));
    }


    public PostBrickPartConnector(brickPartConnector: BrickPartConnectorSubmitData) : Observable<BrickPartConnectorData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickPartConnectorData>(this.baseUrl + 'api/BrickPartConnector', brickPartConnector, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickPartConnector(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickPartConnector(brickPartConnector));
            }));
    }

  
    public DeleteBrickPartConnector(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickPartConnector/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickPartConnector(id));
            }));
    }


    private getConfigHash(config: BrickPartConnectorQueryParameters | any): string {

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

    public userIsBMCBrickPartConnectorReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickPartConnectorReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickPartConnectors
        //
        if (userIsBMCBrickPartConnectorReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickPartConnectorReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickPartConnectorReader = false;
            }
        }

        return userIsBMCBrickPartConnectorReader;
    }


    public userIsBMCBrickPartConnectorWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickPartConnectorWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickPartConnectors
        //
        if (userIsBMCBrickPartConnectorWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickPartConnectorWriter = user.writePermission >= 50;
          } else {
            userIsBMCBrickPartConnectorWriter = false;
          }      
        }

        return userIsBMCBrickPartConnectorWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BrickPartConnectorData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickPartConnectorData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickPartConnectorTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickPartConnector(raw: any): BrickPartConnectorData {
    if (!raw) return raw;

    //
    // Create a BrickPartConnectorData object instance with correct prototype
    //
    const revived = Object.create(BrickPartConnectorData.prototype) as BrickPartConnectorData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //

    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadBrickPartConnectorXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBrickPartConnectorList(rawList: any[]): BrickPartConnectorData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickPartConnector(raw));
  }

}
