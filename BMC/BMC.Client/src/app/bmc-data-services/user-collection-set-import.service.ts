/*

   GENERATED SERVICE FOR THE USERCOLLECTIONSETIMPORT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserCollectionSetImport table.

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
import { UserCollectionData } from './user-collection.service';
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
export class UserCollectionSetImportQueryParameters {
    userCollectionId: bigint | number | null | undefined = null;
    legoSetId: bigint | number | null | undefined = null;
    quantity: bigint | number | null | undefined = null;
    importedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class UserCollectionSetImportSubmitData {
    id!: bigint | number;
    userCollectionId!: bigint | number;
    legoSetId!: bigint | number;
    quantity: bigint | number | null = null;
    importedDate: string | null = null;     // ISO 8601 (full datetime)
    active!: boolean;
    deleted!: boolean;
}


export class UserCollectionSetImportBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserCollectionSetImportChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userCollectionSetImport.UserCollectionSetImportChildren$` — use with `| async` in templates
//        • Promise:    `userCollectionSetImport.UserCollectionSetImportChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userCollectionSetImport.UserCollectionSetImportChildren$ | async"`), or
//        • Access the promise getter (`userCollectionSetImport.UserCollectionSetImportChildren` or `await userCollectionSetImport.UserCollectionSetImportChildren`)
//    - Simply reading `userCollectionSetImport.UserCollectionSetImportChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userCollectionSetImport.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserCollectionSetImportData {
    id!: bigint | number;
    userCollectionId!: bigint | number;
    legoSetId!: bigint | number;
    quantity!: bigint | number;
    importedDate!: string | null;   // ISO 8601 (full datetime)
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    legoSet: LegoSetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    userCollection: UserCollectionData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any UserCollectionSetImportData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userCollectionSetImport.Reload();
  //
  //  Non Async:
  //
  //     userCollectionSetImport[0].Reload().then(x => {
  //        this.userCollectionSetImport = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserCollectionSetImportService.Instance.GetUserCollectionSetImport(this.id, includeRelations)
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
     * Updates the state of this UserCollectionSetImportData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserCollectionSetImportData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserCollectionSetImportSubmitData {
        return UserCollectionSetImportService.Instance.ConvertToUserCollectionSetImportSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserCollectionSetImportService extends SecureEndpointBase {

    private static _instance: UserCollectionSetImportService;
    private listCache: Map<string, Observable<Array<UserCollectionSetImportData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserCollectionSetImportBasicListData>>>;
    private recordCache: Map<string, Observable<UserCollectionSetImportData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserCollectionSetImportData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserCollectionSetImportBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserCollectionSetImportData>>();

        UserCollectionSetImportService._instance = this;
    }

    public static get Instance(): UserCollectionSetImportService {
      return UserCollectionSetImportService._instance;
    }


    public ClearListCaches(config: UserCollectionSetImportQueryParameters | null = null) {

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


    public ConvertToUserCollectionSetImportSubmitData(data: UserCollectionSetImportData): UserCollectionSetImportSubmitData {

        let output = new UserCollectionSetImportSubmitData();

        output.id = data.id;
        output.userCollectionId = data.userCollectionId;
        output.legoSetId = data.legoSetId;
        output.quantity = data.quantity;
        output.importedDate = data.importedDate;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserCollectionSetImport(id: bigint | number, includeRelations: boolean = true) : Observable<UserCollectionSetImportData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userCollectionSetImport$ = this.requestUserCollectionSetImport(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserCollectionSetImport", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userCollectionSetImport$);

            return userCollectionSetImport$;
        }

        return this.recordCache.get(configHash) as Observable<UserCollectionSetImportData>;
    }

    private requestUserCollectionSetImport(id: bigint | number, includeRelations: boolean = true) : Observable<UserCollectionSetImportData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserCollectionSetImportData>(this.baseUrl + 'api/UserCollectionSetImport/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserCollectionSetImport(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserCollectionSetImport(id, includeRelations));
            }));
    }

    public GetUserCollectionSetImportList(config: UserCollectionSetImportQueryParameters | any = null) : Observable<Array<UserCollectionSetImportData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userCollectionSetImportList$ = this.requestUserCollectionSetImportList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserCollectionSetImport list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userCollectionSetImportList$);

            return userCollectionSetImportList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserCollectionSetImportData>>;
    }


    private requestUserCollectionSetImportList(config: UserCollectionSetImportQueryParameters | any) : Observable <Array<UserCollectionSetImportData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserCollectionSetImportData>>(this.baseUrl + 'api/UserCollectionSetImports', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserCollectionSetImportList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserCollectionSetImportList(config));
            }));
    }

    public GetUserCollectionSetImportsRowCount(config: UserCollectionSetImportQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userCollectionSetImportsRowCount$ = this.requestUserCollectionSetImportsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserCollectionSetImports row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userCollectionSetImportsRowCount$);

            return userCollectionSetImportsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserCollectionSetImportsRowCount(config: UserCollectionSetImportQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserCollectionSetImports/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserCollectionSetImportsRowCount(config));
            }));
    }

    public GetUserCollectionSetImportsBasicListData(config: UserCollectionSetImportQueryParameters | any = null) : Observable<Array<UserCollectionSetImportBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userCollectionSetImportsBasicListData$ = this.requestUserCollectionSetImportsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserCollectionSetImports basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userCollectionSetImportsBasicListData$);

            return userCollectionSetImportsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserCollectionSetImportBasicListData>>;
    }


    private requestUserCollectionSetImportsBasicListData(config: UserCollectionSetImportQueryParameters | any) : Observable<Array<UserCollectionSetImportBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserCollectionSetImportBasicListData>>(this.baseUrl + 'api/UserCollectionSetImports/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserCollectionSetImportsBasicListData(config));
            }));

    }


    public PutUserCollectionSetImport(id: bigint | number, userCollectionSetImport: UserCollectionSetImportSubmitData) : Observable<UserCollectionSetImportData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserCollectionSetImportData>(this.baseUrl + 'api/UserCollectionSetImport/' + id.toString(), userCollectionSetImport, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserCollectionSetImport(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserCollectionSetImport(id, userCollectionSetImport));
            }));
    }


    public PostUserCollectionSetImport(userCollectionSetImport: UserCollectionSetImportSubmitData) : Observable<UserCollectionSetImportData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserCollectionSetImportData>(this.baseUrl + 'api/UserCollectionSetImport', userCollectionSetImport, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserCollectionSetImport(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserCollectionSetImport(userCollectionSetImport));
            }));
    }

  
    public DeleteUserCollectionSetImport(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserCollectionSetImport/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserCollectionSetImport(id));
            }));
    }


    private getConfigHash(config: UserCollectionSetImportQueryParameters | any): string {

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

    public userIsBMCUserCollectionSetImportReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCUserCollectionSetImportReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.UserCollectionSetImports
        //
        if (userIsBMCUserCollectionSetImportReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCUserCollectionSetImportReader = user.readPermission >= 1;
            } else {
                userIsBMCUserCollectionSetImportReader = false;
            }
        }

        return userIsBMCUserCollectionSetImportReader;
    }


    public userIsBMCUserCollectionSetImportWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCUserCollectionSetImportWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.UserCollectionSetImports
        //
        if (userIsBMCUserCollectionSetImportWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCUserCollectionSetImportWriter = user.writePermission >= 10;
          } else {
            userIsBMCUserCollectionSetImportWriter = false;
          }      
        }

        return userIsBMCUserCollectionSetImportWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full UserCollectionSetImportData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserCollectionSetImportData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserCollectionSetImportTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserCollectionSetImport(raw: any): UserCollectionSetImportData {
    if (!raw) return raw;

    //
    // Create a UserCollectionSetImportData object instance with correct prototype
    //
    const revived = Object.create(UserCollectionSetImportData.prototype) as UserCollectionSetImportData;

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
    // 2. But private methods (loadUserCollectionSetImportXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveUserCollectionSetImportList(rawList: any[]): UserCollectionSetImportData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserCollectionSetImport(raw));
  }

}
