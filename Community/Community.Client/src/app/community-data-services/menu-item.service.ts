/*

   GENERATED SERVICE FOR THE MENUITEM TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the MenuItem table.

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
import { MenuData } from './menu.service';
import { PageData } from './page.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class MenuItemQueryParameters {
    Id: bigint | number | null | undefined = null;
    MenuId: bigint | number | null | undefined = null;
    Label: string | null | undefined = null;
    Url: string | null | undefined = null;
    PageId: bigint | number | null | undefined = null;
    ParentMenuItemId: bigint | number | null | undefined = null;
    IconClass: string | null | undefined = null;
    OpenInNewTab: boolean | null | undefined = null;
    Sequence: bigint | number | null | undefined = null;
    ObjectGuid: string | null | undefined = null;
    Active: boolean | null | undefined = null;
    Deleted: boolean | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class MenuItemSubmitData {
    Id: bigint | number | null = null;
    MenuId: bigint | number | null = null;
    Label: string | null = null;
    Url: string | null = null;
    PageId: bigint | number | null = null;
    ParentMenuItemId: bigint | number | null = null;
    IconClass: string | null = null;
    OpenInNewTab: boolean | null = null;
    Sequence: bigint | number | null = null;
    Active: boolean | null = null;
    Deleted: boolean | null = null;
}


export class MenuItemBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. MenuItemChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `menuItem.MenuItemChildren$` — use with `| async` in templates
//        • Promise:    `menuItem.MenuItemChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="menuItem.MenuItemChildren$ | async"`), or
//        • Access the promise getter (`menuItem.MenuItemChildren` or `await menuItem.MenuItemChildren`)
//    - Simply reading `menuItem.MenuItemChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await menuItem.Reload()` to refresh the entire object and clear all lazy caches.
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
export class MenuItemData {
    Id!: bigint | number;
    MenuId!: bigint | number;
    Label!: string | null;
    Url!: string | null;
    PageId!: bigint | number;
    ParentMenuItemId!: bigint | number;
    IconClass!: string | null;
    OpenInNewTab!: boolean | null;
    Sequence!: bigint | number;
    ObjectGuid!: string | null;
    Active!: boolean | null;
    Deleted!: boolean | null;
    Menu: MenuData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    Page: PageData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    parentMenuItem: MenuItemData | null | undefined = null;            // Self referencing navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any MenuItemData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.menuItem.Reload();
  //
  //  Non Async:
  //
  //     menuItem[0].Reload().then(x => {
  //        this.menuItem = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      MenuItemService.Instance.GetMenuItem(this.id, includeRelations)
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
     * Updates the state of this MenuItemData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this MenuItemData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): MenuItemSubmitData {
        return MenuItemService.Instance.ConvertToMenuItemSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class MenuItemService extends SecureEndpointBase {

    private static _instance: MenuItemService;
    private listCache: Map<string, Observable<Array<MenuItemData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<MenuItemBasicListData>>>;
    private recordCache: Map<string, Observable<MenuItemData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<MenuItemData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<MenuItemBasicListData>>>();
        this.recordCache = new Map<string, Observable<MenuItemData>>();

        MenuItemService._instance = this;
    }

    public static get Instance(): MenuItemService {
      return MenuItemService._instance;
    }


    public ClearListCaches(config: MenuItemQueryParameters | null = null) {

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


    public ConvertToMenuItemSubmitData(data: MenuItemData): MenuItemSubmitData {

        let output = new MenuItemSubmitData();

        output.Id = data.Id;
        output.MenuId = data.MenuId;
        output.Label = data.Label;
        output.Url = data.Url;
        output.PageId = data.PageId;
        output.ParentMenuItemId = data.ParentMenuItemId;
        output.IconClass = data.IconClass;
        output.OpenInNewTab = data.OpenInNewTab;
        output.Sequence = data.Sequence;
        output.Active = data.Active;
        output.Deleted = data.Deleted;

        return output;
    }

    public GetMenuItem(id: bigint | number, includeRelations: boolean = true) : Observable<MenuItemData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const menuItem$ = this.requestMenuItem(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MenuItem", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, menuItem$);

            return menuItem$;
        }

        return this.recordCache.get(configHash) as Observable<MenuItemData>;
    }

    private requestMenuItem(id: bigint | number, includeRelations: boolean = true) : Observable<MenuItemData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<MenuItemData>(this.baseUrl + 'api/MenuItem/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveMenuItem(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestMenuItem(id, includeRelations));
            }));
    }

    public GetMenuItemList(config: MenuItemQueryParameters | any = null) : Observable<Array<MenuItemData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const menuItemList$ = this.requestMenuItemList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MenuItem list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, menuItemList$);

            return menuItemList$;
        }

        return this.listCache.get(configHash) as Observable<Array<MenuItemData>>;
    }


    private requestMenuItemList(config: MenuItemQueryParameters | any) : Observable <Array<MenuItemData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MenuItemData>>(this.baseUrl + 'api/MenuItems', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveMenuItemList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestMenuItemList(config));
            }));
    }

    public GetMenuItemsRowCount(config: MenuItemQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const menuItemsRowCount$ = this.requestMenuItemsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MenuItems row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, menuItemsRowCount$);

            return menuItemsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestMenuItemsRowCount(config: MenuItemQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/MenuItems/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMenuItemsRowCount(config));
            }));
    }

    public GetMenuItemsBasicListData(config: MenuItemQueryParameters | any = null) : Observable<Array<MenuItemBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const menuItemsBasicListData$ = this.requestMenuItemsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MenuItems basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, menuItemsBasicListData$);

            return menuItemsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<MenuItemBasicListData>>;
    }


    private requestMenuItemsBasicListData(config: MenuItemQueryParameters | any) : Observable<Array<MenuItemBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MenuItemBasicListData>>(this.baseUrl + 'api/MenuItems/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMenuItemsBasicListData(config));
            }));

    }


    public PutMenuItem(id: bigint | number, menuItem: MenuItemSubmitData) : Observable<MenuItemData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<MenuItemData>(this.baseUrl + 'api/MenuItem/' + id.toString(), menuItem, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMenuItem(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutMenuItem(id, menuItem));
            }));
    }


    public PostMenuItem(menuItem: MenuItemSubmitData) : Observable<MenuItemData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<MenuItemData>(this.baseUrl + 'api/MenuItem', menuItem, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMenuItem(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostMenuItem(menuItem));
            }));
    }

  
    public DeleteMenuItem(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/MenuItem/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteMenuItem(id));
            }));
    }


    private getConfigHash(config: MenuItemQueryParameters | any): string {

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

    public userIsCommunityMenuItemReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityMenuItemReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.MenuItems
        //
        if (userIsCommunityMenuItemReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityMenuItemReader = user.readPermission >= 1;
            } else {
                userIsCommunityMenuItemReader = false;
            }
        }

        return userIsCommunityMenuItemReader;
    }


    public userIsCommunityMenuItemWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityMenuItemWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.MenuItems
        //
        if (userIsCommunityMenuItemWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityMenuItemWriter = user.writePermission >= 50;
          } else {
            userIsCommunityMenuItemWriter = false;
          }      
        }

        return userIsCommunityMenuItemWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full MenuItemData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the MenuItemData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when MenuItemTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveMenuItem(raw: any): MenuItemData {
    if (!raw) return raw;

    //
    // Create a MenuItemData object instance with correct prototype
    //
    const revived = Object.create(MenuItemData.prototype) as MenuItemData;

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
    // 2. But private methods (loadMenuItemXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveMenuItemList(rawList: any[]): MenuItemData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveMenuItem(raw));
  }

}
