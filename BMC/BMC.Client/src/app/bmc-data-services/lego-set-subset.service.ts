/*

   GENERATED SERVICE FOR THE LEGOSETSUBSET TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the LegoSetSubset table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class LegoSetSubsetQueryParameters {
    parentLegoSetId: bigint | number | null | undefined = null;
    childLegoSetId: bigint | number | null | undefined = null;
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
export class LegoSetSubsetSubmitData {
    id!: bigint | number;
    parentLegoSetId!: bigint | number;
    childLegoSetId!: bigint | number;
    quantity: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class LegoSetSubsetBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. LegoSetSubsetChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `legoSetSubset.LegoSetSubsetChildren$` — use with `| async` in templates
//        • Promise:    `legoSetSubset.LegoSetSubsetChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="legoSetSubset.LegoSetSubsetChildren$ | async"`), or
//        • Access the promise getter (`legoSetSubset.LegoSetSubsetChildren` or `await legoSetSubset.LegoSetSubsetChildren`)
//    - Simply reading `legoSetSubset.LegoSetSubsetChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await legoSetSubset.Reload()` to refresh the entire object and clear all lazy caches.
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
export class LegoSetSubsetData {
    id!: bigint | number;
    parentLegoSetId!: bigint | number;
    childLegoSetId!: bigint | number;
    quantity!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    parentLegoSet: LegoSetData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)
    childLegoSet: LegoSetData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any LegoSetSubsetData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.legoSetSubset.Reload();
  //
  //  Non Async:
  //
  //     legoSetSubset[0].Reload().then(x => {
  //        this.legoSetSubset = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      LegoSetSubsetService.Instance.GetLegoSetSubset(this.id, includeRelations)
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
     * Updates the state of this LegoSetSubsetData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this LegoSetSubsetData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): LegoSetSubsetSubmitData {
        return LegoSetSubsetService.Instance.ConvertToLegoSetSubsetSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class LegoSetSubsetService extends SecureEndpointBase {

    private static _instance: LegoSetSubsetService;
    private listCache: Map<string, Observable<Array<LegoSetSubsetData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<LegoSetSubsetBasicListData>>>;
    private recordCache: Map<string, Observable<LegoSetSubsetData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<LegoSetSubsetData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<LegoSetSubsetBasicListData>>>();
        this.recordCache = new Map<string, Observable<LegoSetSubsetData>>();

        LegoSetSubsetService._instance = this;
    }

    public static get Instance(): LegoSetSubsetService {
      return LegoSetSubsetService._instance;
    }


    public ClearListCaches(config: LegoSetSubsetQueryParameters | null = null) {

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


    public ConvertToLegoSetSubsetSubmitData(data: LegoSetSubsetData): LegoSetSubsetSubmitData {

        let output = new LegoSetSubsetSubmitData();

        output.id = data.id;
        output.parentLegoSetId = data.parentLegoSetId;
        output.childLegoSetId = data.childLegoSetId;
        output.quantity = data.quantity;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetLegoSetSubset(id: bigint | number, includeRelations: boolean = true) : Observable<LegoSetSubsetData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const legoSetSubset$ = this.requestLegoSetSubset(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LegoSetSubset", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, legoSetSubset$);

            return legoSetSubset$;
        }

        return this.recordCache.get(configHash) as Observable<LegoSetSubsetData>;
    }

    private requestLegoSetSubset(id: bigint | number, includeRelations: boolean = true) : Observable<LegoSetSubsetData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<LegoSetSubsetData>(this.baseUrl + 'api/LegoSetSubset/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveLegoSetSubset(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetSubset(id, includeRelations));
            }));
    }

    public GetLegoSetSubsetList(config: LegoSetSubsetQueryParameters | any = null) : Observable<Array<LegoSetSubsetData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const legoSetSubsetList$ = this.requestLegoSetSubsetList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LegoSetSubset list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, legoSetSubsetList$);

            return legoSetSubsetList$;
        }

        return this.listCache.get(configHash) as Observable<Array<LegoSetSubsetData>>;
    }


    private requestLegoSetSubsetList(config: LegoSetSubsetQueryParameters | any) : Observable <Array<LegoSetSubsetData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LegoSetSubsetData>>(this.baseUrl + 'api/LegoSetSubsets', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveLegoSetSubsetList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetSubsetList(config));
            }));
    }

    public GetLegoSetSubsetsRowCount(config: LegoSetSubsetQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const legoSetSubsetsRowCount$ = this.requestLegoSetSubsetsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LegoSetSubsets row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, legoSetSubsetsRowCount$);

            return legoSetSubsetsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestLegoSetSubsetsRowCount(config: LegoSetSubsetQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/LegoSetSubsets/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetSubsetsRowCount(config));
            }));
    }

    public GetLegoSetSubsetsBasicListData(config: LegoSetSubsetQueryParameters | any = null) : Observable<Array<LegoSetSubsetBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const legoSetSubsetsBasicListData$ = this.requestLegoSetSubsetsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LegoSetSubsets basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, legoSetSubsetsBasicListData$);

            return legoSetSubsetsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<LegoSetSubsetBasicListData>>;
    }


    private requestLegoSetSubsetsBasicListData(config: LegoSetSubsetQueryParameters | any) : Observable<Array<LegoSetSubsetBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LegoSetSubsetBasicListData>>(this.baseUrl + 'api/LegoSetSubsets/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetSubsetsBasicListData(config));
            }));

    }


    public PutLegoSetSubset(id: bigint | number, legoSetSubset: LegoSetSubsetSubmitData) : Observable<LegoSetSubsetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<LegoSetSubsetData>(this.baseUrl + 'api/LegoSetSubset/' + id.toString(), legoSetSubset, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLegoSetSubset(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutLegoSetSubset(id, legoSetSubset));
            }));
    }


    public PostLegoSetSubset(legoSetSubset: LegoSetSubsetSubmitData) : Observable<LegoSetSubsetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<LegoSetSubsetData>(this.baseUrl + 'api/LegoSetSubset', legoSetSubset, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLegoSetSubset(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostLegoSetSubset(legoSetSubset));
            }));
    }

  
    public DeleteLegoSetSubset(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/LegoSetSubset/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteLegoSetSubset(id));
            }));
    }


    private getConfigHash(config: LegoSetSubsetQueryParameters | any): string {

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

    public userIsBMCLegoSetSubsetReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCLegoSetSubsetReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.LegoSetSubsets
        //
        if (userIsBMCLegoSetSubsetReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCLegoSetSubsetReader = user.readPermission >= 1;
            } else {
                userIsBMCLegoSetSubsetReader = false;
            }
        }

        return userIsBMCLegoSetSubsetReader;
    }


    public userIsBMCLegoSetSubsetWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCLegoSetSubsetWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.LegoSetSubsets
        //
        if (userIsBMCLegoSetSubsetWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCLegoSetSubsetWriter = user.writePermission >= 255;
          } else {
            userIsBMCLegoSetSubsetWriter = false;
          }      
        }

        return userIsBMCLegoSetSubsetWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full LegoSetSubsetData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the LegoSetSubsetData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when LegoSetSubsetTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveLegoSetSubset(raw: any): LegoSetSubsetData {
    if (!raw) return raw;

    //
    // Create a LegoSetSubsetData object instance with correct prototype
    //
    const revived = Object.create(LegoSetSubsetData.prototype) as LegoSetSubsetData;

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
    // 2. But private methods (loadLegoSetSubsetXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveLegoSetSubsetList(rawList: any[]): LegoSetSubsetData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveLegoSetSubset(raw));
  }

}
