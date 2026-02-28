/*

   GENERATED SERVICE FOR THE USERSETLISTITEM TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserSetListItem table.

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
import { UserSetListData } from './user-set-list.service';
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
export class UserSetListItemQueryParameters {
    userSetListId: bigint | number | null | undefined = null;
    legoSetId: bigint | number | null | undefined = null;
    quantity: bigint | number | null | undefined = null;
    includeSpares: boolean | null | undefined = null;
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
export class UserSetListItemSubmitData {
    id!: bigint | number;
    userSetListId!: bigint | number;
    legoSetId!: bigint | number;
    quantity!: bigint | number;
    includeSpares!: boolean;
    active!: boolean;
    deleted!: boolean;
}


export class UserSetListItemBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserSetListItemChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userSetListItem.UserSetListItemChildren$` — use with `| async` in templates
//        • Promise:    `userSetListItem.UserSetListItemChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userSetListItem.UserSetListItemChildren$ | async"`), or
//        • Access the promise getter (`userSetListItem.UserSetListItemChildren` or `await userSetListItem.UserSetListItemChildren`)
//    - Simply reading `userSetListItem.UserSetListItemChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userSetListItem.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserSetListItemData {
    id!: bigint | number;
    userSetListId!: bigint | number;
    legoSetId!: bigint | number;
    quantity!: bigint | number;
    includeSpares!: boolean;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    legoSet: LegoSetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    userSetList: UserSetListData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any UserSetListItemData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userSetListItem.Reload();
  //
  //  Non Async:
  //
  //     userSetListItem[0].Reload().then(x => {
  //        this.userSetListItem = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserSetListItemService.Instance.GetUserSetListItem(this.id, includeRelations)
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
     * Updates the state of this UserSetListItemData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserSetListItemData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserSetListItemSubmitData {
        return UserSetListItemService.Instance.ConvertToUserSetListItemSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserSetListItemService extends SecureEndpointBase {

    private static _instance: UserSetListItemService;
    private listCache: Map<string, Observable<Array<UserSetListItemData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserSetListItemBasicListData>>>;
    private recordCache: Map<string, Observable<UserSetListItemData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserSetListItemData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserSetListItemBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserSetListItemData>>();

        UserSetListItemService._instance = this;
    }

    public static get Instance(): UserSetListItemService {
      return UserSetListItemService._instance;
    }


    public ClearListCaches(config: UserSetListItemQueryParameters | null = null) {

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


    public ConvertToUserSetListItemSubmitData(data: UserSetListItemData): UserSetListItemSubmitData {

        let output = new UserSetListItemSubmitData();

        output.id = data.id;
        output.userSetListId = data.userSetListId;
        output.legoSetId = data.legoSetId;
        output.quantity = data.quantity;
        output.includeSpares = data.includeSpares;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserSetListItem(id: bigint | number, includeRelations: boolean = true) : Observable<UserSetListItemData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userSetListItem$ = this.requestUserSetListItem(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserSetListItem", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userSetListItem$);

            return userSetListItem$;
        }

        return this.recordCache.get(configHash) as Observable<UserSetListItemData>;
    }

    private requestUserSetListItem(id: bigint | number, includeRelations: boolean = true) : Observable<UserSetListItemData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserSetListItemData>(this.baseUrl + 'api/UserSetListItem/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserSetListItem(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserSetListItem(id, includeRelations));
            }));
    }

    public GetUserSetListItemList(config: UserSetListItemQueryParameters | any = null) : Observable<Array<UserSetListItemData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userSetListItemList$ = this.requestUserSetListItemList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserSetListItem list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userSetListItemList$);

            return userSetListItemList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserSetListItemData>>;
    }


    private requestUserSetListItemList(config: UserSetListItemQueryParameters | any) : Observable <Array<UserSetListItemData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserSetListItemData>>(this.baseUrl + 'api/UserSetListItems', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserSetListItemList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserSetListItemList(config));
            }));
    }

    public GetUserSetListItemsRowCount(config: UserSetListItemQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userSetListItemsRowCount$ = this.requestUserSetListItemsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserSetListItems row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userSetListItemsRowCount$);

            return userSetListItemsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserSetListItemsRowCount(config: UserSetListItemQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserSetListItems/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserSetListItemsRowCount(config));
            }));
    }

    public GetUserSetListItemsBasicListData(config: UserSetListItemQueryParameters | any = null) : Observable<Array<UserSetListItemBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userSetListItemsBasicListData$ = this.requestUserSetListItemsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserSetListItems basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userSetListItemsBasicListData$);

            return userSetListItemsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserSetListItemBasicListData>>;
    }


    private requestUserSetListItemsBasicListData(config: UserSetListItemQueryParameters | any) : Observable<Array<UserSetListItemBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserSetListItemBasicListData>>(this.baseUrl + 'api/UserSetListItems/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserSetListItemsBasicListData(config));
            }));

    }


    public PutUserSetListItem(id: bigint | number, userSetListItem: UserSetListItemSubmitData) : Observable<UserSetListItemData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserSetListItemData>(this.baseUrl + 'api/UserSetListItem/' + id.toString(), userSetListItem, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserSetListItem(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserSetListItem(id, userSetListItem));
            }));
    }


    public PostUserSetListItem(userSetListItem: UserSetListItemSubmitData) : Observable<UserSetListItemData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserSetListItemData>(this.baseUrl + 'api/UserSetListItem', userSetListItem, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserSetListItem(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserSetListItem(userSetListItem));
            }));
    }

  
    public DeleteUserSetListItem(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserSetListItem/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserSetListItem(id));
            }));
    }


    private getConfigHash(config: UserSetListItemQueryParameters | any): string {

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

    public userIsBMCUserSetListItemReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCUserSetListItemReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.UserSetListItems
        //
        if (userIsBMCUserSetListItemReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCUserSetListItemReader = user.readPermission >= 1;
            } else {
                userIsBMCUserSetListItemReader = false;
            }
        }

        return userIsBMCUserSetListItemReader;
    }


    public userIsBMCUserSetListItemWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCUserSetListItemWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.UserSetListItems
        //
        if (userIsBMCUserSetListItemWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCUserSetListItemWriter = user.writePermission >= 10;
          } else {
            userIsBMCUserSetListItemWriter = false;
          }      
        }

        return userIsBMCUserSetListItemWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full UserSetListItemData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserSetListItemData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserSetListItemTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserSetListItem(raw: any): UserSetListItemData {
    if (!raw) return raw;

    //
    // Create a UserSetListItemData object instance with correct prototype
    //
    const revived = Object.create(UserSetListItemData.prototype) as UserSetListItemData;

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
    // 2. But private methods (loadUserSetListItemXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveUserSetListItemList(rawList: any[]): UserSetListItemData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserSetListItem(raw));
  }

}
