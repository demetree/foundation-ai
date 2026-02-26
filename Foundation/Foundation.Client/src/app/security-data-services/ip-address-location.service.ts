/*

   GENERATED SERVICE FOR THE IPADDRESSLOCATION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the IpAddressLocation table.

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
import { LoginAttemptService, LoginAttemptData } from './login-attempt.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class IpAddressLocationQueryParameters {
    ipAddress: string | null | undefined = null;
    countryCode: string | null | undefined = null;
    countryName: string | null | undefined = null;
    city: string | null | undefined = null;
    latitude: number | null | undefined = null;
    longitude: number | null | undefined = null;
    lastLookupDate: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class IpAddressLocationSubmitData {
    id!: bigint | number;
    ipAddress!: string;
    countryCode: string | null = null;
    countryName: string | null = null;
    city: string | null = null;
    latitude: number | null = null;
    longitude: number | null = null;
    lastLookupDate!: string;      // ISO 8601 (full datetime)
    active!: boolean;
    deleted!: boolean;
}


export class IpAddressLocationBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. IpAddressLocationChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `ipAddressLocation.IpAddressLocationChildren$` — use with `| async` in templates
//        • Promise:    `ipAddressLocation.IpAddressLocationChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="ipAddressLocation.IpAddressLocationChildren$ | async"`), or
//        • Access the promise getter (`ipAddressLocation.IpAddressLocationChildren` or `await ipAddressLocation.IpAddressLocationChildren`)
//    - Simply reading `ipAddressLocation.IpAddressLocationChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await ipAddressLocation.Reload()` to refresh the entire object and clear all lazy caches.
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
export class IpAddressLocationData {
    id!: bigint | number;
    ipAddress!: string;
    countryCode!: string | null;
    countryName!: string | null;
    city!: string | null;
    latitude!: number | null;
    longitude!: number | null;
    lastLookupDate!: string;      // ISO 8601 (full datetime)
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _loginAttempts: LoginAttemptData[] | null = null;
    private _loginAttemptsPromise: Promise<LoginAttemptData[]> | null  = null;
    private _loginAttemptsSubject = new BehaviorSubject<LoginAttemptData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public LoginAttempts$ = this._loginAttemptsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._loginAttempts === null && this._loginAttemptsPromise === null) {
            this.loadLoginAttempts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _loginAttemptsCount$: Observable<bigint | number> | null = null;
    public get LoginAttemptsCount$(): Observable<bigint | number> {
        if (this._loginAttemptsCount$ === null) {
            this._loginAttemptsCount$ = LoginAttemptService.Instance.GetLoginAttemptsRowCount({ipAddressLocationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._loginAttemptsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any IpAddressLocationData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.ipAddressLocation.Reload();
  //
  //  Non Async:
  //
  //     ipAddressLocation[0].Reload().then(x => {
  //        this.ipAddressLocation = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      IpAddressLocationService.Instance.GetIpAddressLocation(this.id, includeRelations)
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
     this._loginAttempts = null;
     this._loginAttemptsPromise = null;
     this._loginAttemptsSubject.next(null);
     this._loginAttemptsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the LoginAttempts for this IpAddressLocation.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.ipAddressLocation.LoginAttempts.then(ipAddressLocations => { ... })
     *   or
     *   await this.ipAddressLocation.ipAddressLocations
     *
    */
    public get LoginAttempts(): Promise<LoginAttemptData[]> {
        if (this._loginAttempts !== null) {
            return Promise.resolve(this._loginAttempts);
        }

        if (this._loginAttemptsPromise !== null) {
            return this._loginAttemptsPromise;
        }

        // Start the load
        this.loadLoginAttempts();

        return this._loginAttemptsPromise!;
    }



    private loadLoginAttempts(): void {

        this._loginAttemptsPromise = lastValueFrom(
            IpAddressLocationService.Instance.GetLoginAttemptsForIpAddressLocation(this.id)
        )
        .then(LoginAttempts => {
            this._loginAttempts = LoginAttempts ?? [];
            this._loginAttemptsSubject.next(this._loginAttempts);
            return this._loginAttempts;
         })
        .catch(err => {
            this._loginAttempts = [];
            this._loginAttemptsSubject.next(this._loginAttempts);
            throw err;
        })
        .finally(() => {
            this._loginAttemptsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached LoginAttempt. Call after mutations to force refresh.
     */
    public ClearLoginAttemptsCache(): void {
        this._loginAttempts = null;
        this._loginAttemptsPromise = null;
        this._loginAttemptsSubject.next(this._loginAttempts);      // Emit to observable
    }

    public get HasLoginAttempts(): Promise<boolean> {
        return this.LoginAttempts.then(loginAttempts => loginAttempts.length > 0);
    }




    /**
     * Updates the state of this IpAddressLocationData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this IpAddressLocationData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): IpAddressLocationSubmitData {
        return IpAddressLocationService.Instance.ConvertToIpAddressLocationSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class IpAddressLocationService extends SecureEndpointBase {

    private static _instance: IpAddressLocationService;
    private listCache: Map<string, Observable<Array<IpAddressLocationData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<IpAddressLocationBasicListData>>>;
    private recordCache: Map<string, Observable<IpAddressLocationData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private loginAttemptService: LoginAttemptService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<IpAddressLocationData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<IpAddressLocationBasicListData>>>();
        this.recordCache = new Map<string, Observable<IpAddressLocationData>>();

        IpAddressLocationService._instance = this;
    }

    public static get Instance(): IpAddressLocationService {
      return IpAddressLocationService._instance;
    }


    public ClearListCaches(config: IpAddressLocationQueryParameters | null = null) {

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


    public ConvertToIpAddressLocationSubmitData(data: IpAddressLocationData): IpAddressLocationSubmitData {

        let output = new IpAddressLocationSubmitData();

        output.id = data.id;
        output.ipAddress = data.ipAddress;
        output.countryCode = data.countryCode;
        output.countryName = data.countryName;
        output.city = data.city;
        output.latitude = data.latitude;
        output.longitude = data.longitude;
        output.lastLookupDate = data.lastLookupDate;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetIpAddressLocation(id: bigint | number, includeRelations: boolean = true) : Observable<IpAddressLocationData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const ipAddressLocation$ = this.requestIpAddressLocation(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IpAddressLocation", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, ipAddressLocation$);

            return ipAddressLocation$;
        }

        return this.recordCache.get(configHash) as Observable<IpAddressLocationData>;
    }

    private requestIpAddressLocation(id: bigint | number, includeRelations: boolean = true) : Observable<IpAddressLocationData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IpAddressLocationData>(this.baseUrl + 'api/IpAddressLocation/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveIpAddressLocation(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestIpAddressLocation(id, includeRelations));
            }));
    }

    public GetIpAddressLocationList(config: IpAddressLocationQueryParameters | any = null) : Observable<Array<IpAddressLocationData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const ipAddressLocationList$ = this.requestIpAddressLocationList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IpAddressLocation list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, ipAddressLocationList$);

            return ipAddressLocationList$;
        }

        return this.listCache.get(configHash) as Observable<Array<IpAddressLocationData>>;
    }


    private requestIpAddressLocationList(config: IpAddressLocationQueryParameters | any) : Observable <Array<IpAddressLocationData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IpAddressLocationData>>(this.baseUrl + 'api/IpAddressLocations', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveIpAddressLocationList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestIpAddressLocationList(config));
            }));
    }

    public GetIpAddressLocationsRowCount(config: IpAddressLocationQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const ipAddressLocationsRowCount$ = this.requestIpAddressLocationsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IpAddressLocations row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, ipAddressLocationsRowCount$);

            return ipAddressLocationsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestIpAddressLocationsRowCount(config: IpAddressLocationQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/IpAddressLocations/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIpAddressLocationsRowCount(config));
            }));
    }

    public GetIpAddressLocationsBasicListData(config: IpAddressLocationQueryParameters | any = null) : Observable<Array<IpAddressLocationBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const ipAddressLocationsBasicListData$ = this.requestIpAddressLocationsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IpAddressLocations basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, ipAddressLocationsBasicListData$);

            return ipAddressLocationsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<IpAddressLocationBasicListData>>;
    }


    private requestIpAddressLocationsBasicListData(config: IpAddressLocationQueryParameters | any) : Observable<Array<IpAddressLocationBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IpAddressLocationBasicListData>>(this.baseUrl + 'api/IpAddressLocations/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIpAddressLocationsBasicListData(config));
            }));

    }


    public PutIpAddressLocation(id: bigint | number, ipAddressLocation: IpAddressLocationSubmitData) : Observable<IpAddressLocationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IpAddressLocationData>(this.baseUrl + 'api/IpAddressLocation/' + id.toString(), ipAddressLocation, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIpAddressLocation(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutIpAddressLocation(id, ipAddressLocation));
            }));
    }


    public PostIpAddressLocation(ipAddressLocation: IpAddressLocationSubmitData) : Observable<IpAddressLocationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<IpAddressLocationData>(this.baseUrl + 'api/IpAddressLocation', ipAddressLocation, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIpAddressLocation(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostIpAddressLocation(ipAddressLocation));
            }));
    }

  
    public DeleteIpAddressLocation(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/IpAddressLocation/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteIpAddressLocation(id));
            }));
    }


    private getConfigHash(config: IpAddressLocationQueryParameters | any): string {

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

    public userIsSecurityIpAddressLocationReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecurityIpAddressLocationReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.IpAddressLocations
        //
        if (userIsSecurityIpAddressLocationReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecurityIpAddressLocationReader = user.readPermission >= 1;
            } else {
                userIsSecurityIpAddressLocationReader = false;
            }
        }

        return userIsSecurityIpAddressLocationReader;
    }


    public userIsSecurityIpAddressLocationWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecurityIpAddressLocationWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.IpAddressLocations
        //
        if (userIsSecurityIpAddressLocationWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecurityIpAddressLocationWriter = user.writePermission >= 255;
          } else {
            userIsSecurityIpAddressLocationWriter = false;
          }      
        }

        return userIsSecurityIpAddressLocationWriter;
    }

    public GetLoginAttemptsForIpAddressLocation(ipAddressLocationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<LoginAttemptData[]> {
        return this.loginAttemptService.GetLoginAttemptList({
            ipAddressLocationId: ipAddressLocationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full IpAddressLocationData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the IpAddressLocationData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when IpAddressLocationTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveIpAddressLocation(raw: any): IpAddressLocationData {
    if (!raw) return raw;

    //
    // Create a IpAddressLocationData object instance with correct prototype
    //
    const revived = Object.create(IpAddressLocationData.prototype) as IpAddressLocationData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._loginAttempts = null;
    (revived as any)._loginAttemptsPromise = null;
    (revived as any)._loginAttemptsSubject = new BehaviorSubject<LoginAttemptData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadIpAddressLocationXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).LoginAttempts$ = (revived as any)._loginAttemptsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._loginAttempts === null && (revived as any)._loginAttemptsPromise === null) {
                (revived as any).loadLoginAttempts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._loginAttemptsCount$ = null;



    return revived;
  }

  private ReviveIpAddressLocationList(rawList: any[]): IpAddressLocationData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveIpAddressLocation(raw));
  }

}
