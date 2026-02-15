/*

   GENERATED SERVICE FOR THE BRICKCONNECTION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickConnection table.

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
import { ProjectData } from './project.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BrickConnectionQueryParameters {
    projectId: bigint | number | null | undefined = null;
    sourcePlacedBrickId: bigint | number | null | undefined = null;
    sourceConnectorId: bigint | number | null | undefined = null;
    targetPlacedBrickId: bigint | number | null | undefined = null;
    targetConnectorId: bigint | number | null | undefined = null;
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
export class BrickConnectionSubmitData {
    id!: bigint | number;
    projectId!: bigint | number;
    sourcePlacedBrickId: bigint | number | null = null;
    sourceConnectorId: bigint | number | null = null;
    targetPlacedBrickId: bigint | number | null = null;
    targetConnectorId: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BrickConnectionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickConnectionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickConnection.BrickConnectionChildren$` — use with `| async` in templates
//        • Promise:    `brickConnection.BrickConnectionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickConnection.BrickConnectionChildren$ | async"`), or
//        • Access the promise getter (`brickConnection.BrickConnectionChildren` or `await brickConnection.BrickConnectionChildren`)
//    - Simply reading `brickConnection.BrickConnectionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickConnection.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickConnectionData {
    id!: bigint | number;
    projectId!: bigint | number;
    sourcePlacedBrickId!: bigint | number;
    sourceConnectorId!: bigint | number;
    targetPlacedBrickId!: bigint | number;
    targetConnectorId!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    project: ProjectData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any BrickConnectionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickConnection.Reload();
  //
  //  Non Async:
  //
  //     brickConnection[0].Reload().then(x => {
  //        this.brickConnection = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickConnectionService.Instance.GetBrickConnection(this.id, includeRelations)
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
     * Updates the state of this BrickConnectionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickConnectionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickConnectionSubmitData {
        return BrickConnectionService.Instance.ConvertToBrickConnectionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickConnectionService extends SecureEndpointBase {

    private static _instance: BrickConnectionService;
    private listCache: Map<string, Observable<Array<BrickConnectionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickConnectionBasicListData>>>;
    private recordCache: Map<string, Observable<BrickConnectionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickConnectionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickConnectionBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickConnectionData>>();

        BrickConnectionService._instance = this;
    }

    public static get Instance(): BrickConnectionService {
      return BrickConnectionService._instance;
    }


    public ClearListCaches(config: BrickConnectionQueryParameters | null = null) {

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


    public ConvertToBrickConnectionSubmitData(data: BrickConnectionData): BrickConnectionSubmitData {

        let output = new BrickConnectionSubmitData();

        output.id = data.id;
        output.projectId = data.projectId;
        output.sourcePlacedBrickId = data.sourcePlacedBrickId;
        output.sourceConnectorId = data.sourceConnectorId;
        output.targetPlacedBrickId = data.targetPlacedBrickId;
        output.targetConnectorId = data.targetConnectorId;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBrickConnection(id: bigint | number, includeRelations: boolean = true) : Observable<BrickConnectionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickConnection$ = this.requestBrickConnection(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickConnection", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickConnection$);

            return brickConnection$;
        }

        return this.recordCache.get(configHash) as Observable<BrickConnectionData>;
    }

    private requestBrickConnection(id: bigint | number, includeRelations: boolean = true) : Observable<BrickConnectionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickConnectionData>(this.baseUrl + 'api/BrickConnection/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickConnection(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickConnection(id, includeRelations));
            }));
    }

    public GetBrickConnectionList(config: BrickConnectionQueryParameters | any = null) : Observable<Array<BrickConnectionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickConnectionList$ = this.requestBrickConnectionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickConnection list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickConnectionList$);

            return brickConnectionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickConnectionData>>;
    }


    private requestBrickConnectionList(config: BrickConnectionQueryParameters | any) : Observable <Array<BrickConnectionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickConnectionData>>(this.baseUrl + 'api/BrickConnections', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickConnectionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickConnectionList(config));
            }));
    }

    public GetBrickConnectionsRowCount(config: BrickConnectionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickConnectionsRowCount$ = this.requestBrickConnectionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickConnections row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickConnectionsRowCount$);

            return brickConnectionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickConnectionsRowCount(config: BrickConnectionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickConnections/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickConnectionsRowCount(config));
            }));
    }

    public GetBrickConnectionsBasicListData(config: BrickConnectionQueryParameters | any = null) : Observable<Array<BrickConnectionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickConnectionsBasicListData$ = this.requestBrickConnectionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickConnections basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickConnectionsBasicListData$);

            return brickConnectionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickConnectionBasicListData>>;
    }


    private requestBrickConnectionsBasicListData(config: BrickConnectionQueryParameters | any) : Observable<Array<BrickConnectionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickConnectionBasicListData>>(this.baseUrl + 'api/BrickConnections/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickConnectionsBasicListData(config));
            }));

    }


    public PutBrickConnection(id: bigint | number, brickConnection: BrickConnectionSubmitData) : Observable<BrickConnectionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickConnectionData>(this.baseUrl + 'api/BrickConnection/' + id.toString(), brickConnection, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickConnection(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickConnection(id, brickConnection));
            }));
    }


    public PostBrickConnection(brickConnection: BrickConnectionSubmitData) : Observable<BrickConnectionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickConnectionData>(this.baseUrl + 'api/BrickConnection', brickConnection, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickConnection(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickConnection(brickConnection));
            }));
    }

  
    public DeleteBrickConnection(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickConnection/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickConnection(id));
            }));
    }


    private getConfigHash(config: BrickConnectionQueryParameters | any): string {

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

    public userIsBMCBrickConnectionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickConnectionReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickConnections
        //
        if (userIsBMCBrickConnectionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickConnectionReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickConnectionReader = false;
            }
        }

        return userIsBMCBrickConnectionReader;
    }


    public userIsBMCBrickConnectionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickConnectionWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickConnections
        //
        if (userIsBMCBrickConnectionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickConnectionWriter = user.writePermission >= 1;
          } else {
            userIsBMCBrickConnectionWriter = false;
          }      
        }

        return userIsBMCBrickConnectionWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BrickConnectionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickConnectionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickConnectionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickConnection(raw: any): BrickConnectionData {
    if (!raw) return raw;

    //
    // Create a BrickConnectionData object instance with correct prototype
    //
    const revived = Object.create(BrickConnectionData.prototype) as BrickConnectionData;

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
    // 2. But private methods (loadBrickConnectionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBrickConnectionList(rawList: any[]): BrickConnectionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickConnection(raw));
  }

}
