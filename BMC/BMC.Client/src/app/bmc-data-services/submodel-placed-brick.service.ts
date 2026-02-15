/*

   GENERATED SERVICE FOR THE SUBMODELPLACEDBRICK TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SubmodelPlacedBrick table.

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
import { SubmodelData } from './submodel.service';
import { PlacedBrickData } from './placed-brick.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SubmodelPlacedBrickQueryParameters {
    submodelId: bigint | number | null | undefined = null;
    placedBrickId: bigint | number | null | undefined = null;
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
export class SubmodelPlacedBrickSubmitData {
    id!: bigint | number;
    submodelId!: bigint | number;
    placedBrickId!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class SubmodelPlacedBrickBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SubmodelPlacedBrickChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `submodelPlacedBrick.SubmodelPlacedBrickChildren$` — use with `| async` in templates
//        • Promise:    `submodelPlacedBrick.SubmodelPlacedBrickChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="submodelPlacedBrick.SubmodelPlacedBrickChildren$ | async"`), or
//        • Access the promise getter (`submodelPlacedBrick.SubmodelPlacedBrickChildren` or `await submodelPlacedBrick.SubmodelPlacedBrickChildren`)
//    - Simply reading `submodelPlacedBrick.SubmodelPlacedBrickChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await submodelPlacedBrick.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SubmodelPlacedBrickData {
    id!: bigint | number;
    submodelId!: bigint | number;
    placedBrickId!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    placedBrick: PlacedBrickData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    submodel: SubmodelData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any SubmodelPlacedBrickData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.submodelPlacedBrick.Reload();
  //
  //  Non Async:
  //
  //     submodelPlacedBrick[0].Reload().then(x => {
  //        this.submodelPlacedBrick = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SubmodelPlacedBrickService.Instance.GetSubmodelPlacedBrick(this.id, includeRelations)
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
     * Updates the state of this SubmodelPlacedBrickData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SubmodelPlacedBrickData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SubmodelPlacedBrickSubmitData {
        return SubmodelPlacedBrickService.Instance.ConvertToSubmodelPlacedBrickSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SubmodelPlacedBrickService extends SecureEndpointBase {

    private static _instance: SubmodelPlacedBrickService;
    private listCache: Map<string, Observable<Array<SubmodelPlacedBrickData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SubmodelPlacedBrickBasicListData>>>;
    private recordCache: Map<string, Observable<SubmodelPlacedBrickData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SubmodelPlacedBrickData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SubmodelPlacedBrickBasicListData>>>();
        this.recordCache = new Map<string, Observable<SubmodelPlacedBrickData>>();

        SubmodelPlacedBrickService._instance = this;
    }

    public static get Instance(): SubmodelPlacedBrickService {
      return SubmodelPlacedBrickService._instance;
    }


    public ClearListCaches(config: SubmodelPlacedBrickQueryParameters | null = null) {

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


    public ConvertToSubmodelPlacedBrickSubmitData(data: SubmodelPlacedBrickData): SubmodelPlacedBrickSubmitData {

        let output = new SubmodelPlacedBrickSubmitData();

        output.id = data.id;
        output.submodelId = data.submodelId;
        output.placedBrickId = data.placedBrickId;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSubmodelPlacedBrick(id: bigint | number, includeRelations: boolean = true) : Observable<SubmodelPlacedBrickData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const submodelPlacedBrick$ = this.requestSubmodelPlacedBrick(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SubmodelPlacedBrick", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, submodelPlacedBrick$);

            return submodelPlacedBrick$;
        }

        return this.recordCache.get(configHash) as Observable<SubmodelPlacedBrickData>;
    }

    private requestSubmodelPlacedBrick(id: bigint | number, includeRelations: boolean = true) : Observable<SubmodelPlacedBrickData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SubmodelPlacedBrickData>(this.baseUrl + 'api/SubmodelPlacedBrick/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSubmodelPlacedBrick(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSubmodelPlacedBrick(id, includeRelations));
            }));
    }

    public GetSubmodelPlacedBrickList(config: SubmodelPlacedBrickQueryParameters | any = null) : Observable<Array<SubmodelPlacedBrickData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const submodelPlacedBrickList$ = this.requestSubmodelPlacedBrickList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SubmodelPlacedBrick list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, submodelPlacedBrickList$);

            return submodelPlacedBrickList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SubmodelPlacedBrickData>>;
    }


    private requestSubmodelPlacedBrickList(config: SubmodelPlacedBrickQueryParameters | any) : Observable <Array<SubmodelPlacedBrickData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SubmodelPlacedBrickData>>(this.baseUrl + 'api/SubmodelPlacedBricks', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSubmodelPlacedBrickList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSubmodelPlacedBrickList(config));
            }));
    }

    public GetSubmodelPlacedBricksRowCount(config: SubmodelPlacedBrickQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const submodelPlacedBricksRowCount$ = this.requestSubmodelPlacedBricksRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SubmodelPlacedBricks row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, submodelPlacedBricksRowCount$);

            return submodelPlacedBricksRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSubmodelPlacedBricksRowCount(config: SubmodelPlacedBrickQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SubmodelPlacedBricks/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSubmodelPlacedBricksRowCount(config));
            }));
    }

    public GetSubmodelPlacedBricksBasicListData(config: SubmodelPlacedBrickQueryParameters | any = null) : Observable<Array<SubmodelPlacedBrickBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const submodelPlacedBricksBasicListData$ = this.requestSubmodelPlacedBricksBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SubmodelPlacedBricks basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, submodelPlacedBricksBasicListData$);

            return submodelPlacedBricksBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SubmodelPlacedBrickBasicListData>>;
    }


    private requestSubmodelPlacedBricksBasicListData(config: SubmodelPlacedBrickQueryParameters | any) : Observable<Array<SubmodelPlacedBrickBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SubmodelPlacedBrickBasicListData>>(this.baseUrl + 'api/SubmodelPlacedBricks/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSubmodelPlacedBricksBasicListData(config));
            }));

    }


    public PutSubmodelPlacedBrick(id: bigint | number, submodelPlacedBrick: SubmodelPlacedBrickSubmitData) : Observable<SubmodelPlacedBrickData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SubmodelPlacedBrickData>(this.baseUrl + 'api/SubmodelPlacedBrick/' + id.toString(), submodelPlacedBrick, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSubmodelPlacedBrick(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSubmodelPlacedBrick(id, submodelPlacedBrick));
            }));
    }


    public PostSubmodelPlacedBrick(submodelPlacedBrick: SubmodelPlacedBrickSubmitData) : Observable<SubmodelPlacedBrickData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SubmodelPlacedBrickData>(this.baseUrl + 'api/SubmodelPlacedBrick', submodelPlacedBrick, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSubmodelPlacedBrick(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSubmodelPlacedBrick(submodelPlacedBrick));
            }));
    }

  
    public DeleteSubmodelPlacedBrick(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SubmodelPlacedBrick/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSubmodelPlacedBrick(id));
            }));
    }


    private getConfigHash(config: SubmodelPlacedBrickQueryParameters | any): string {

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

    public userIsBMCSubmodelPlacedBrickReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCSubmodelPlacedBrickReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.SubmodelPlacedBricks
        //
        if (userIsBMCSubmodelPlacedBrickReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCSubmodelPlacedBrickReader = user.readPermission >= 1;
            } else {
                userIsBMCSubmodelPlacedBrickReader = false;
            }
        }

        return userIsBMCSubmodelPlacedBrickReader;
    }


    public userIsBMCSubmodelPlacedBrickWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCSubmodelPlacedBrickWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.SubmodelPlacedBricks
        //
        if (userIsBMCSubmodelPlacedBrickWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCSubmodelPlacedBrickWriter = user.writePermission >= 1;
          } else {
            userIsBMCSubmodelPlacedBrickWriter = false;
          }      
        }

        return userIsBMCSubmodelPlacedBrickWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full SubmodelPlacedBrickData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SubmodelPlacedBrickData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SubmodelPlacedBrickTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSubmodelPlacedBrick(raw: any): SubmodelPlacedBrickData {
    if (!raw) return raw;

    //
    // Create a SubmodelPlacedBrickData object instance with correct prototype
    //
    const revived = Object.create(SubmodelPlacedBrickData.prototype) as SubmodelPlacedBrickData;

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
    // 2. But private methods (loadSubmodelPlacedBrickXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveSubmodelPlacedBrickList(rawList: any[]): SubmodelPlacedBrickData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSubmodelPlacedBrick(raw));
  }

}
