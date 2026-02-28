/*

   GENERATED SERVICE FOR THE USERPARTLISTITEM TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserPartListItem table.

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
import { UserPartListData } from './user-part-list.service';
import { BrickPartData } from './brick-part.service';
import { BrickColourData } from './brick-colour.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserPartListItemQueryParameters {
    userPartListId: bigint | number | null | undefined = null;
    brickPartId: bigint | number | null | undefined = null;
    brickColourId: bigint | number | null | undefined = null;
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
export class UserPartListItemSubmitData {
    id!: bigint | number;
    userPartListId!: bigint | number;
    brickPartId!: bigint | number;
    brickColourId!: bigint | number;
    quantity!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class UserPartListItemBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserPartListItemChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userPartListItem.UserPartListItemChildren$` — use with `| async` in templates
//        • Promise:    `userPartListItem.UserPartListItemChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userPartListItem.UserPartListItemChildren$ | async"`), or
//        • Access the promise getter (`userPartListItem.UserPartListItemChildren` or `await userPartListItem.UserPartListItemChildren`)
//    - Simply reading `userPartListItem.UserPartListItemChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userPartListItem.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserPartListItemData {
    id!: bigint | number;
    userPartListId!: bigint | number;
    brickPartId!: bigint | number;
    brickColourId!: bigint | number;
    quantity!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    brickColour: BrickColourData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    brickPart: BrickPartData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    userPartList: UserPartListData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any UserPartListItemData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userPartListItem.Reload();
  //
  //  Non Async:
  //
  //     userPartListItem[0].Reload().then(x => {
  //        this.userPartListItem = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserPartListItemService.Instance.GetUserPartListItem(this.id, includeRelations)
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
     * Updates the state of this UserPartListItemData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserPartListItemData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserPartListItemSubmitData {
        return UserPartListItemService.Instance.ConvertToUserPartListItemSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserPartListItemService extends SecureEndpointBase {

    private static _instance: UserPartListItemService;
    private listCache: Map<string, Observable<Array<UserPartListItemData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserPartListItemBasicListData>>>;
    private recordCache: Map<string, Observable<UserPartListItemData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserPartListItemData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserPartListItemBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserPartListItemData>>();

        UserPartListItemService._instance = this;
    }

    public static get Instance(): UserPartListItemService {
      return UserPartListItemService._instance;
    }


    public ClearListCaches(config: UserPartListItemQueryParameters | null = null) {

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


    public ConvertToUserPartListItemSubmitData(data: UserPartListItemData): UserPartListItemSubmitData {

        let output = new UserPartListItemSubmitData();

        output.id = data.id;
        output.userPartListId = data.userPartListId;
        output.brickPartId = data.brickPartId;
        output.brickColourId = data.brickColourId;
        output.quantity = data.quantity;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserPartListItem(id: bigint | number, includeRelations: boolean = true) : Observable<UserPartListItemData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userPartListItem$ = this.requestUserPartListItem(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserPartListItem", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userPartListItem$);

            return userPartListItem$;
        }

        return this.recordCache.get(configHash) as Observable<UserPartListItemData>;
    }

    private requestUserPartListItem(id: bigint | number, includeRelations: boolean = true) : Observable<UserPartListItemData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserPartListItemData>(this.baseUrl + 'api/UserPartListItem/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserPartListItem(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserPartListItem(id, includeRelations));
            }));
    }

    public GetUserPartListItemList(config: UserPartListItemQueryParameters | any = null) : Observable<Array<UserPartListItemData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userPartListItemList$ = this.requestUserPartListItemList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserPartListItem list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userPartListItemList$);

            return userPartListItemList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserPartListItemData>>;
    }


    private requestUserPartListItemList(config: UserPartListItemQueryParameters | any) : Observable <Array<UserPartListItemData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserPartListItemData>>(this.baseUrl + 'api/UserPartListItems', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserPartListItemList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserPartListItemList(config));
            }));
    }

    public GetUserPartListItemsRowCount(config: UserPartListItemQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userPartListItemsRowCount$ = this.requestUserPartListItemsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserPartListItems row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userPartListItemsRowCount$);

            return userPartListItemsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserPartListItemsRowCount(config: UserPartListItemQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserPartListItems/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserPartListItemsRowCount(config));
            }));
    }

    public GetUserPartListItemsBasicListData(config: UserPartListItemQueryParameters | any = null) : Observable<Array<UserPartListItemBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userPartListItemsBasicListData$ = this.requestUserPartListItemsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserPartListItems basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userPartListItemsBasicListData$);

            return userPartListItemsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserPartListItemBasicListData>>;
    }


    private requestUserPartListItemsBasicListData(config: UserPartListItemQueryParameters | any) : Observable<Array<UserPartListItemBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserPartListItemBasicListData>>(this.baseUrl + 'api/UserPartListItems/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserPartListItemsBasicListData(config));
            }));

    }


    public PutUserPartListItem(id: bigint | number, userPartListItem: UserPartListItemSubmitData) : Observable<UserPartListItemData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserPartListItemData>(this.baseUrl + 'api/UserPartListItem/' + id.toString(), userPartListItem, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserPartListItem(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserPartListItem(id, userPartListItem));
            }));
    }


    public PostUserPartListItem(userPartListItem: UserPartListItemSubmitData) : Observable<UserPartListItemData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserPartListItemData>(this.baseUrl + 'api/UserPartListItem', userPartListItem, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserPartListItem(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserPartListItem(userPartListItem));
            }));
    }

  
    public DeleteUserPartListItem(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserPartListItem/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserPartListItem(id));
            }));
    }


    private getConfigHash(config: UserPartListItemQueryParameters | any): string {

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

    public userIsBMCUserPartListItemReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCUserPartListItemReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.UserPartListItems
        //
        if (userIsBMCUserPartListItemReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCUserPartListItemReader = user.readPermission >= 1;
            } else {
                userIsBMCUserPartListItemReader = false;
            }
        }

        return userIsBMCUserPartListItemReader;
    }


    public userIsBMCUserPartListItemWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCUserPartListItemWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.UserPartListItems
        //
        if (userIsBMCUserPartListItemWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCUserPartListItemWriter = user.writePermission >= 10;
          } else {
            userIsBMCUserPartListItemWriter = false;
          }      
        }

        return userIsBMCUserPartListItemWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full UserPartListItemData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserPartListItemData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserPartListItemTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserPartListItem(raw: any): UserPartListItemData {
    if (!raw) return raw;

    //
    // Create a UserPartListItemData object instance with correct prototype
    //
    const revived = Object.create(UserPartListItemData.prototype) as UserPartListItemData;

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
    // 2. But private methods (loadUserPartListItemXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveUserPartListItemList(rawList: any[]): UserPartListItemData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserPartListItem(raw));
  }

}
