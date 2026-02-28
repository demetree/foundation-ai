/*

   GENERATED SERVICE FOR THE CONNECTORTYPECOMPATIBILITY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ConnectorTypeCompatibility table.

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
export class ConnectorTypeCompatibilityQueryParameters {
    maleConnectorTypeId: bigint | number | null | undefined = null;
    femaleConnectorTypeId: bigint | number | null | undefined = null;
    connectionStrength: string | null | undefined = null;
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
export class ConnectorTypeCompatibilitySubmitData {
    id!: bigint | number;
    maleConnectorTypeId!: bigint | number;
    femaleConnectorTypeId!: bigint | number;
    connectionStrength!: string;
    active!: boolean;
    deleted!: boolean;
}


export class ConnectorTypeCompatibilityBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ConnectorTypeCompatibilityChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `connectorTypeCompatibility.ConnectorTypeCompatibilityChildren$` — use with `| async` in templates
//        • Promise:    `connectorTypeCompatibility.ConnectorTypeCompatibilityChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="connectorTypeCompatibility.ConnectorTypeCompatibilityChildren$ | async"`), or
//        • Access the promise getter (`connectorTypeCompatibility.ConnectorTypeCompatibilityChildren` or `await connectorTypeCompatibility.ConnectorTypeCompatibilityChildren`)
//    - Simply reading `connectorTypeCompatibility.ConnectorTypeCompatibilityChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await connectorTypeCompatibility.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ConnectorTypeCompatibilityData {
    id!: bigint | number;
    maleConnectorTypeId!: bigint | number;
    femaleConnectorTypeId!: bigint | number;
    connectionStrength!: string;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    maleConnectorType: ConnectorTypeData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)
    femaleConnectorType: ConnectorTypeData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ConnectorTypeCompatibilityData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.connectorTypeCompatibility.Reload();
  //
  //  Non Async:
  //
  //     connectorTypeCompatibility[0].Reload().then(x => {
  //        this.connectorTypeCompatibility = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ConnectorTypeCompatibilityService.Instance.GetConnectorTypeCompatibility(this.id, includeRelations)
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
     * Updates the state of this ConnectorTypeCompatibilityData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ConnectorTypeCompatibilityData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ConnectorTypeCompatibilitySubmitData {
        return ConnectorTypeCompatibilityService.Instance.ConvertToConnectorTypeCompatibilitySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ConnectorTypeCompatibilityService extends SecureEndpointBase {

    private static _instance: ConnectorTypeCompatibilityService;
    private listCache: Map<string, Observable<Array<ConnectorTypeCompatibilityData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ConnectorTypeCompatibilityBasicListData>>>;
    private recordCache: Map<string, Observable<ConnectorTypeCompatibilityData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ConnectorTypeCompatibilityData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ConnectorTypeCompatibilityBasicListData>>>();
        this.recordCache = new Map<string, Observable<ConnectorTypeCompatibilityData>>();

        ConnectorTypeCompatibilityService._instance = this;
    }

    public static get Instance(): ConnectorTypeCompatibilityService {
      return ConnectorTypeCompatibilityService._instance;
    }


    public ClearListCaches(config: ConnectorTypeCompatibilityQueryParameters | null = null) {

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


    public ConvertToConnectorTypeCompatibilitySubmitData(data: ConnectorTypeCompatibilityData): ConnectorTypeCompatibilitySubmitData {

        let output = new ConnectorTypeCompatibilitySubmitData();

        output.id = data.id;
        output.maleConnectorTypeId = data.maleConnectorTypeId;
        output.femaleConnectorTypeId = data.femaleConnectorTypeId;
        output.connectionStrength = data.connectionStrength;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetConnectorTypeCompatibility(id: bigint | number, includeRelations: boolean = true) : Observable<ConnectorTypeCompatibilityData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const connectorTypeCompatibility$ = this.requestConnectorTypeCompatibility(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConnectorTypeCompatibility", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, connectorTypeCompatibility$);

            return connectorTypeCompatibility$;
        }

        return this.recordCache.get(configHash) as Observable<ConnectorTypeCompatibilityData>;
    }

    private requestConnectorTypeCompatibility(id: bigint | number, includeRelations: boolean = true) : Observable<ConnectorTypeCompatibilityData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConnectorTypeCompatibilityData>(this.baseUrl + 'api/ConnectorTypeCompatibility/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveConnectorTypeCompatibility(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestConnectorTypeCompatibility(id, includeRelations));
            }));
    }

    public GetConnectorTypeCompatibilityList(config: ConnectorTypeCompatibilityQueryParameters | any = null) : Observable<Array<ConnectorTypeCompatibilityData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const connectorTypeCompatibilityList$ = this.requestConnectorTypeCompatibilityList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConnectorTypeCompatibility list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, connectorTypeCompatibilityList$);

            return connectorTypeCompatibilityList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ConnectorTypeCompatibilityData>>;
    }


    private requestConnectorTypeCompatibilityList(config: ConnectorTypeCompatibilityQueryParameters | any) : Observable <Array<ConnectorTypeCompatibilityData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConnectorTypeCompatibilityData>>(this.baseUrl + 'api/ConnectorTypeCompatibilities', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveConnectorTypeCompatibilityList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestConnectorTypeCompatibilityList(config));
            }));
    }

    public GetConnectorTypeCompatibilitiesRowCount(config: ConnectorTypeCompatibilityQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const connectorTypeCompatibilitiesRowCount$ = this.requestConnectorTypeCompatibilitiesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConnectorTypeCompatibilities row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, connectorTypeCompatibilitiesRowCount$);

            return connectorTypeCompatibilitiesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestConnectorTypeCompatibilitiesRowCount(config: ConnectorTypeCompatibilityQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ConnectorTypeCompatibilities/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConnectorTypeCompatibilitiesRowCount(config));
            }));
    }

    public GetConnectorTypeCompatibilitiesBasicListData(config: ConnectorTypeCompatibilityQueryParameters | any = null) : Observable<Array<ConnectorTypeCompatibilityBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const connectorTypeCompatibilitiesBasicListData$ = this.requestConnectorTypeCompatibilitiesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConnectorTypeCompatibilities basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, connectorTypeCompatibilitiesBasicListData$);

            return connectorTypeCompatibilitiesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ConnectorTypeCompatibilityBasicListData>>;
    }


    private requestConnectorTypeCompatibilitiesBasicListData(config: ConnectorTypeCompatibilityQueryParameters | any) : Observable<Array<ConnectorTypeCompatibilityBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConnectorTypeCompatibilityBasicListData>>(this.baseUrl + 'api/ConnectorTypeCompatibilities/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConnectorTypeCompatibilitiesBasicListData(config));
            }));

    }


    public PutConnectorTypeCompatibility(id: bigint | number, connectorTypeCompatibility: ConnectorTypeCompatibilitySubmitData) : Observable<ConnectorTypeCompatibilityData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConnectorTypeCompatibilityData>(this.baseUrl + 'api/ConnectorTypeCompatibility/' + id.toString(), connectorTypeCompatibility, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConnectorTypeCompatibility(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutConnectorTypeCompatibility(id, connectorTypeCompatibility));
            }));
    }


    public PostConnectorTypeCompatibility(connectorTypeCompatibility: ConnectorTypeCompatibilitySubmitData) : Observable<ConnectorTypeCompatibilityData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ConnectorTypeCompatibilityData>(this.baseUrl + 'api/ConnectorTypeCompatibility', connectorTypeCompatibility, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConnectorTypeCompatibility(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostConnectorTypeCompatibility(connectorTypeCompatibility));
            }));
    }

  
    public DeleteConnectorTypeCompatibility(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ConnectorTypeCompatibility/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteConnectorTypeCompatibility(id));
            }));
    }


    private getConfigHash(config: ConnectorTypeCompatibilityQueryParameters | any): string {

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

    public userIsBMCConnectorTypeCompatibilityReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCConnectorTypeCompatibilityReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ConnectorTypeCompatibilities
        //
        if (userIsBMCConnectorTypeCompatibilityReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCConnectorTypeCompatibilityReader = user.readPermission >= 1;
            } else {
                userIsBMCConnectorTypeCompatibilityReader = false;
            }
        }

        return userIsBMCConnectorTypeCompatibilityReader;
    }


    public userIsBMCConnectorTypeCompatibilityWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCConnectorTypeCompatibilityWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ConnectorTypeCompatibilities
        //
        if (userIsBMCConnectorTypeCompatibilityWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCConnectorTypeCompatibilityWriter = user.writePermission >= 255;
          } else {
            userIsBMCConnectorTypeCompatibilityWriter = false;
          }      
        }

        return userIsBMCConnectorTypeCompatibilityWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ConnectorTypeCompatibilityData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ConnectorTypeCompatibilityData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ConnectorTypeCompatibilityTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveConnectorTypeCompatibility(raw: any): ConnectorTypeCompatibilityData {
    if (!raw) return raw;

    //
    // Create a ConnectorTypeCompatibilityData object instance with correct prototype
    //
    const revived = Object.create(ConnectorTypeCompatibilityData.prototype) as ConnectorTypeCompatibilityData;

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
    // 2. But private methods (loadConnectorTypeCompatibilityXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveConnectorTypeCompatibilityList(rawList: any[]): ConnectorTypeCompatibilityData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveConnectorTypeCompatibility(raw));
  }

}
