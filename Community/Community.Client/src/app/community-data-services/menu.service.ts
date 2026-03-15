/*

   GENERATED SERVICE FOR THE MENU TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Menu table.

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
import { MenuItemService, MenuItemData } from './menu-item.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class MenuQueryParameters {
    Id: bigint | number | null | undefined = null;
    Name: string | null | undefined = null;
    Location: string | null | undefined = null;
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
export class MenuSubmitData {
    Id: bigint | number | null = null;
    Name: string | null = null;
    Location: string | null = null;
    Active: boolean | null = null;
    Deleted: boolean | null = null;
}


export class MenuBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. MenuChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `menu.MenuChildren$` — use with `| async` in templates
//        • Promise:    `menu.MenuChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="menu.MenuChildren$ | async"`), or
//        • Access the promise getter (`menu.MenuChildren` or `await menu.MenuChildren`)
//    - Simply reading `menu.MenuChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await menu.Reload()` to refresh the entire object and clear all lazy caches.
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
export class MenuData {
    Id!: bigint | number;
    Name!: string | null;
    Location!: string | null;
    ObjectGuid!: string | null;
    Active!: boolean | null;
    Deleted!: boolean | null;

    //
    // Private lazy-loading caches for related collections
    //
    private _menuItems: MenuItemData[] | null = null;
    private _menuItemsPromise: Promise<MenuItemData[]> | null  = null;
    private _menuItemsSubject = new BehaviorSubject<MenuItemData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public MenuItems$ = this._menuItemsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._menuItems === null && this._menuItemsPromise === null) {
            this.loadMenuItems(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _menuItemsCount$: Observable<bigint | number> | null = null;
    public get MenuItemsCount$(): Observable<bigint | number> {
        if (this._menuItemsCount$ === null) {
            this._menuItemsCount$ = MenuItemService.Instance.GetMenuItemsRowCount({menuId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._menuItemsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any MenuData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.menu.Reload();
  //
  //  Non Async:
  //
  //     menu[0].Reload().then(x => {
  //        this.menu = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      MenuService.Instance.GetMenu(this.id, includeRelations)
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
     this._menuItems = null;
     this._menuItemsPromise = null;
     this._menuItemsSubject.next(null);
     this._menuItemsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the MenuItems for this Menu.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.menu.MenuItems.then(menus => { ... })
     *   or
     *   await this.menu.menus
     *
    */
    public get MenuItems(): Promise<MenuItemData[]> {
        if (this._menuItems !== null) {
            return Promise.resolve(this._menuItems);
        }

        if (this._menuItemsPromise !== null) {
            return this._menuItemsPromise;
        }

        // Start the load
        this.loadMenuItems();

        return this._menuItemsPromise!;
    }



    private loadMenuItems(): void {

        this._menuItemsPromise = lastValueFrom(
            MenuService.Instance.GetMenuItemsForMenu(this.id)
        )
        .then(MenuItems => {
            this._menuItems = MenuItems ?? [];
            this._menuItemsSubject.next(this._menuItems);
            return this._menuItems;
         })
        .catch(err => {
            this._menuItems = [];
            this._menuItemsSubject.next(this._menuItems);
            throw err;
        })
        .finally(() => {
            this._menuItemsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached MenuItem. Call after mutations to force refresh.
     */
    public ClearMenuItemsCache(): void {
        this._menuItems = null;
        this._menuItemsPromise = null;
        this._menuItemsSubject.next(this._menuItems);      // Emit to observable
    }

    public get HasMenuItems(): Promise<boolean> {
        return this.MenuItems.then(menuItems => menuItems.length > 0);
    }




    /**
     * Updates the state of this MenuData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this MenuData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): MenuSubmitData {
        return MenuService.Instance.ConvertToMenuSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class MenuService extends SecureEndpointBase {

    private static _instance: MenuService;
    private listCache: Map<string, Observable<Array<MenuData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<MenuBasicListData>>>;
    private recordCache: Map<string, Observable<MenuData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private menuItemService: MenuItemService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<MenuData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<MenuBasicListData>>>();
        this.recordCache = new Map<string, Observable<MenuData>>();

        MenuService._instance = this;
    }

    public static get Instance(): MenuService {
      return MenuService._instance;
    }


    public ClearListCaches(config: MenuQueryParameters | null = null) {

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


    public ConvertToMenuSubmitData(data: MenuData): MenuSubmitData {

        let output = new MenuSubmitData();

        output.Id = data.Id;
        output.Name = data.Name;
        output.Location = data.Location;
        output.Active = data.Active;
        output.Deleted = data.Deleted;

        return output;
    }

    public GetMenu(id: bigint | number, includeRelations: boolean = true) : Observable<MenuData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const menu$ = this.requestMenu(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Menu", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, menu$);

            return menu$;
        }

        return this.recordCache.get(configHash) as Observable<MenuData>;
    }

    private requestMenu(id: bigint | number, includeRelations: boolean = true) : Observable<MenuData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<MenuData>(this.baseUrl + 'api/Menu/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveMenu(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestMenu(id, includeRelations));
            }));
    }

    public GetMenuList(config: MenuQueryParameters | any = null) : Observable<Array<MenuData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const menuList$ = this.requestMenuList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Menu list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, menuList$);

            return menuList$;
        }

        return this.listCache.get(configHash) as Observable<Array<MenuData>>;
    }


    private requestMenuList(config: MenuQueryParameters | any) : Observable <Array<MenuData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MenuData>>(this.baseUrl + 'api/Menus', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveMenuList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestMenuList(config));
            }));
    }

    public GetMenusRowCount(config: MenuQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const menusRowCount$ = this.requestMenusRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Menus row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, menusRowCount$);

            return menusRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestMenusRowCount(config: MenuQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Menus/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMenusRowCount(config));
            }));
    }

    public GetMenusBasicListData(config: MenuQueryParameters | any = null) : Observable<Array<MenuBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const menusBasicListData$ = this.requestMenusBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Menus basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, menusBasicListData$);

            return menusBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<MenuBasicListData>>;
    }


    private requestMenusBasicListData(config: MenuQueryParameters | any) : Observable<Array<MenuBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MenuBasicListData>>(this.baseUrl + 'api/Menus/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMenusBasicListData(config));
            }));

    }


    public PutMenu(id: bigint | number, menu: MenuSubmitData) : Observable<MenuData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<MenuData>(this.baseUrl + 'api/Menu/' + id.toString(), menu, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMenu(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutMenu(id, menu));
            }));
    }


    public PostMenu(menu: MenuSubmitData) : Observable<MenuData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<MenuData>(this.baseUrl + 'api/Menu', menu, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMenu(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostMenu(menu));
            }));
    }

  
    public DeleteMenu(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Menu/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteMenu(id));
            }));
    }


    private getConfigHash(config: MenuQueryParameters | any): string {

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

    public userIsCommunityMenuReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityMenuReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.Menus
        //
        if (userIsCommunityMenuReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityMenuReader = user.readPermission >= 1;
            } else {
                userIsCommunityMenuReader = false;
            }
        }

        return userIsCommunityMenuReader;
    }


    public userIsCommunityMenuWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityMenuWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.Menus
        //
        if (userIsCommunityMenuWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityMenuWriter = user.writePermission >= 50;
          } else {
            userIsCommunityMenuWriter = false;
          }      
        }

        return userIsCommunityMenuWriter;
    }

    public GetMenuItemsForMenu(menuId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MenuItemData[]> {
        return this.menuItemService.GetMenuItemList({
            menuId: menuId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full MenuData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the MenuData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when MenuTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveMenu(raw: any): MenuData {
    if (!raw) return raw;

    //
    // Create a MenuData object instance with correct prototype
    //
    const revived = Object.create(MenuData.prototype) as MenuData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._menuItems = null;
    (revived as any)._menuItemsPromise = null;
    (revived as any)._menuItemsSubject = new BehaviorSubject<MenuItemData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadMenuXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).MenuItems$ = (revived as any)._menuItemsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._menuItems === null && (revived as any)._menuItemsPromise === null) {
                (revived as any).loadMenuItems();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._menuItemsCount$ = null;



    return revived;
  }

  private ReviveMenuList(rawList: any[]): MenuData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveMenu(raw));
  }

}
