/*

   GENERATED SERVICE FOR THE LEGOSETMINIFIG TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the LegoSetMinifig table.

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
import { LegoSetData } from './lego-set.service';
import { LegoMinifigData } from './lego-minifig.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class LegoSetMinifigQueryParameters {
    legoSetId: bigint | number | null | undefined = null;
    legoMinifigId: bigint | number | null | undefined = null;
    quantity: bigint | number | null | undefined = null;
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
export class LegoSetMinifigSubmitData {
    id!: bigint | number;
    legoSetId!: bigint | number;
    legoMinifigId!: bigint | number;
    quantity: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class LegoSetMinifigBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. LegoSetMinifigChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `legoSetMinifig.LegoSetMinifigChildren$` — use with `| async` in templates
//        • Promise:    `legoSetMinifig.LegoSetMinifigChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="legoSetMinifig.LegoSetMinifigChildren$ | async"`), or
//        • Access the promise getter (`legoSetMinifig.LegoSetMinifigChildren` or `await legoSetMinifig.LegoSetMinifigChildren`)
//    - Simply reading `legoSetMinifig.LegoSetMinifigChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await legoSetMinifig.Reload()` to refresh the entire object and clear all lazy caches.
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
export class LegoSetMinifigData {
    id!: bigint | number;
    legoSetId!: bigint | number;
    legoMinifigId!: bigint | number;
    quantity!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    legoMinifig: LegoMinifigData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    legoSet: LegoSetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any LegoSetMinifigData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.legoSetMinifig.Reload();
  //
  //  Non Async:
  //
  //     legoSetMinifig[0].Reload().then(x => {
  //        this.legoSetMinifig = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      LegoSetMinifigService.Instance.GetLegoSetMinifig(this.id, includeRelations)
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
     * Updates the state of this LegoSetMinifigData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this LegoSetMinifigData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): LegoSetMinifigSubmitData {
        return LegoSetMinifigService.Instance.ConvertToLegoSetMinifigSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class LegoSetMinifigService extends SecureEndpointBase {

    private static _instance: LegoSetMinifigService;
    private listCache: Map<string, Observable<Array<LegoSetMinifigData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<LegoSetMinifigBasicListData>>>;
    private recordCache: Map<string, Observable<LegoSetMinifigData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<LegoSetMinifigData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<LegoSetMinifigBasicListData>>>();
        this.recordCache = new Map<string, Observable<LegoSetMinifigData>>();

        LegoSetMinifigService._instance = this;
    }

    public static get Instance(): LegoSetMinifigService {
      return LegoSetMinifigService._instance;
    }


    public ClearListCaches(config: LegoSetMinifigQueryParameters | null = null) {

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


    public ConvertToLegoSetMinifigSubmitData(data: LegoSetMinifigData): LegoSetMinifigSubmitData {

        let output = new LegoSetMinifigSubmitData();

        output.id = data.id;
        output.legoSetId = data.legoSetId;
        output.legoMinifigId = data.legoMinifigId;
        output.quantity = data.quantity;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetLegoSetMinifig(id: bigint | number, includeRelations: boolean = true) : Observable<LegoSetMinifigData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const legoSetMinifig$ = this.requestLegoSetMinifig(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LegoSetMinifig", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, legoSetMinifig$);

            return legoSetMinifig$;
        }

        return this.recordCache.get(configHash) as Observable<LegoSetMinifigData>;
    }

    private requestLegoSetMinifig(id: bigint | number, includeRelations: boolean = true) : Observable<LegoSetMinifigData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<LegoSetMinifigData>(this.baseUrl + 'api/LegoSetMinifig/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveLegoSetMinifig(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetMinifig(id, includeRelations));
            }));
    }

    public GetLegoSetMinifigList(config: LegoSetMinifigQueryParameters | any = null) : Observable<Array<LegoSetMinifigData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const legoSetMinifigList$ = this.requestLegoSetMinifigList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LegoSetMinifig list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, legoSetMinifigList$);

            return legoSetMinifigList$;
        }

        return this.listCache.get(configHash) as Observable<Array<LegoSetMinifigData>>;
    }


    private requestLegoSetMinifigList(config: LegoSetMinifigQueryParameters | any) : Observable <Array<LegoSetMinifigData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LegoSetMinifigData>>(this.baseUrl + 'api/LegoSetMinifigs', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveLegoSetMinifigList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetMinifigList(config));
            }));
    }

    public GetLegoSetMinifigsRowCount(config: LegoSetMinifigQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const legoSetMinifigsRowCount$ = this.requestLegoSetMinifigsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LegoSetMinifigs row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, legoSetMinifigsRowCount$);

            return legoSetMinifigsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestLegoSetMinifigsRowCount(config: LegoSetMinifigQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/LegoSetMinifigs/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetMinifigsRowCount(config));
            }));
    }

    public GetLegoSetMinifigsBasicListData(config: LegoSetMinifigQueryParameters | any = null) : Observable<Array<LegoSetMinifigBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const legoSetMinifigsBasicListData$ = this.requestLegoSetMinifigsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LegoSetMinifigs basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, legoSetMinifigsBasicListData$);

            return legoSetMinifigsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<LegoSetMinifigBasicListData>>;
    }


    private requestLegoSetMinifigsBasicListData(config: LegoSetMinifigQueryParameters | any) : Observable<Array<LegoSetMinifigBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LegoSetMinifigBasicListData>>(this.baseUrl + 'api/LegoSetMinifigs/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetMinifigsBasicListData(config));
            }));

    }


    public PutLegoSetMinifig(id: bigint | number, legoSetMinifig: LegoSetMinifigSubmitData) : Observable<LegoSetMinifigData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<LegoSetMinifigData>(this.baseUrl + 'api/LegoSetMinifig/' + id.toString(), legoSetMinifig, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLegoSetMinifig(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutLegoSetMinifig(id, legoSetMinifig));
            }));
    }


    public PostLegoSetMinifig(legoSetMinifig: LegoSetMinifigSubmitData) : Observable<LegoSetMinifigData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<LegoSetMinifigData>(this.baseUrl + 'api/LegoSetMinifig', legoSetMinifig, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLegoSetMinifig(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostLegoSetMinifig(legoSetMinifig));
            }));
    }

  
    public DeleteLegoSetMinifig(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/LegoSetMinifig/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteLegoSetMinifig(id));
            }));
    }


    private getConfigHash(config: LegoSetMinifigQueryParameters | any): string {

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

    public userIsBMCLegoSetMinifigReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCLegoSetMinifigReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.LegoSetMinifigs
        //
        if (userIsBMCLegoSetMinifigReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCLegoSetMinifigReader = user.readPermission >= 1;
            } else {
                userIsBMCLegoSetMinifigReader = false;
            }
        }

        return userIsBMCLegoSetMinifigReader;
    }


    public userIsBMCLegoSetMinifigWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCLegoSetMinifigWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.LegoSetMinifigs
        //
        if (userIsBMCLegoSetMinifigWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCLegoSetMinifigWriter = user.writePermission >= 255;
          } else {
            userIsBMCLegoSetMinifigWriter = false;
          }      
        }

        return userIsBMCLegoSetMinifigWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full LegoSetMinifigData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the LegoSetMinifigData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when LegoSetMinifigTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveLegoSetMinifig(raw: any): LegoSetMinifigData {
    if (!raw) return raw;

    //
    // Create a LegoSetMinifigData object instance with correct prototype
    //
    const revived = Object.create(LegoSetMinifigData.prototype) as LegoSetMinifigData;

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
    // 2. But private methods (loadLegoSetMinifigXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveLegoSetMinifigList(rawList: any[]): LegoSetMinifigData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveLegoSetMinifig(raw));
  }

}
