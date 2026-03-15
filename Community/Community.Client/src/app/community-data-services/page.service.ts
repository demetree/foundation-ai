/*

   GENERATED SERVICE FOR THE PAGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Page table.

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
import { PageChangeHistoryService, PageChangeHistoryData } from './page-change-history.service';
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
export class PageQueryParameters {
    Id: bigint | number | null | undefined = null;
    Title: string | null | undefined = null;
    Slug: string | null | undefined = null;
    Body: string | null | undefined = null;
    MetaDescription: string | null | undefined = null;
    FeaturedImageUrl: string | null | undefined = null;
    IsPublished: boolean | null | undefined = null;
    PublishedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    SortOrder: bigint | number | null | undefined = null;
    VersionNumber: bigint | number | null | undefined = null;
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
export class PageSubmitData {
    Id: bigint | number | null = null;
    Title: string | null = null;
    Slug: string | null = null;
    Body: string | null = null;
    MetaDescription: string | null = null;
    FeaturedImageUrl: string | null = null;
    IsPublished: boolean | null = null;
    PublishedDate: string | null = null;     // ISO 8601 (full datetime)
    SortOrder: bigint | number | null = null;
    VersionNumber: bigint | number | null = null;
    Active: boolean | null = null;
    Deleted: boolean | null = null;
}



//
// Version history information returned from version history API endpoints.
// Matches server-side VersionInformation<T> structure.
//
export interface VersionInformationUser {
    id: bigint | number;
    userName: string;
    firstName: string | null;
    middleName: string | null;
    lastName: string | null;
}

export interface VersionInformation<T> {
    timeStamp: string;           // ISO 8601
    user: VersionInformationUser;
    versionNumber: number;
    data: T | null;
}

export class PageBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PageChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `page.PageChildren$` — use with `| async` in templates
//        • Promise:    `page.PageChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="page.PageChildren$ | async"`), or
//        • Access the promise getter (`page.PageChildren` or `await page.PageChildren`)
//    - Simply reading `page.PageChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await page.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PageData {
    Id!: bigint | number;
    Title!: string | null;
    Slug!: string | null;
    Body!: string | null;
    MetaDescription!: string | null;
    FeaturedImageUrl!: string | null;
    IsPublished!: boolean | null;
    PublishedDate!: string | null;   // ISO 8601 (full datetime)
    SortOrder!: bigint | number;
    VersionNumber!: bigint | number;
    ObjectGuid!: string | null;
    Active!: boolean | null;
    Deleted!: boolean | null;

    //
    // Private lazy-loading caches for related collections
    //
    private _pageChangeHistories: PageChangeHistoryData[] | null = null;
    private _pageChangeHistoriesPromise: Promise<PageChangeHistoryData[]> | null  = null;
    private _pageChangeHistoriesSubject = new BehaviorSubject<PageChangeHistoryData[] | null>(null);

                
    private _menuItems: MenuItemData[] | null = null;
    private _menuItemsPromise: Promise<MenuItemData[]> | null  = null;
    private _menuItemsSubject = new BehaviorSubject<MenuItemData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<PageData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<PageData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PageData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public PageChangeHistories$ = this._pageChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._pageChangeHistories === null && this._pageChangeHistoriesPromise === null) {
            this.loadPageChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _pageChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get PageChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._pageChangeHistoriesCount$ === null) {
            this._pageChangeHistoriesCount$ = PageChangeHistoryService.Instance.GetPageChangeHistoriesRowCount({pageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._pageChangeHistoriesCount$;
    }



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
            this._menuItemsCount$ = MenuItemService.Instance.GetMenuItemsRowCount({pageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._menuItemsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PageData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.page.Reload();
  //
  //  Non Async:
  //
  //     page[0].Reload().then(x => {
  //        this.page = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PageService.Instance.GetPage(this.id, includeRelations)
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
     this._pageChangeHistories = null;
     this._pageChangeHistoriesPromise = null;
     this._pageChangeHistoriesSubject.next(null);
     this._pageChangeHistoriesCount$ = null;

     this._menuItems = null;
     this._menuItemsPromise = null;
     this._menuItemsSubject.next(null);
     this._menuItemsCount$ = null;

     this._currentVersionInfo = null;
     this._currentVersionInfoPromise = null;
     this._currentVersionInfoSubject.next(null);
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the PageChangeHistories for this Page.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.page.PageChangeHistories.then(pages => { ... })
     *   or
     *   await this.page.pages
     *
    */
    public get PageChangeHistories(): Promise<PageChangeHistoryData[]> {
        if (this._pageChangeHistories !== null) {
            return Promise.resolve(this._pageChangeHistories);
        }

        if (this._pageChangeHistoriesPromise !== null) {
            return this._pageChangeHistoriesPromise;
        }

        // Start the load
        this.loadPageChangeHistories();

        return this._pageChangeHistoriesPromise!;
    }



    private loadPageChangeHistories(): void {

        this._pageChangeHistoriesPromise = lastValueFrom(
            PageService.Instance.GetPageChangeHistoriesForPage(this.id)
        )
        .then(PageChangeHistories => {
            this._pageChangeHistories = PageChangeHistories ?? [];
            this._pageChangeHistoriesSubject.next(this._pageChangeHistories);
            return this._pageChangeHistories;
         })
        .catch(err => {
            this._pageChangeHistories = [];
            this._pageChangeHistoriesSubject.next(this._pageChangeHistories);
            throw err;
        })
        .finally(() => {
            this._pageChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PageChangeHistory. Call after mutations to force refresh.
     */
    public ClearPageChangeHistoriesCache(): void {
        this._pageChangeHistories = null;
        this._pageChangeHistoriesPromise = null;
        this._pageChangeHistoriesSubject.next(this._pageChangeHistories);      // Emit to observable
    }

    public get HasPageChangeHistories(): Promise<boolean> {
        return this.PageChangeHistories.then(pageChangeHistories => pageChangeHistories.length > 0);
    }


    /**
     *
     * Gets the MenuItems for this Page.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.page.MenuItems.then(pages => { ... })
     *   or
     *   await this.page.pages
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
            PageService.Instance.GetMenuItemsForPage(this.id)
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




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (page.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await page.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<PageData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<PageData>> {
        const info = await lastValueFrom(
            PageService.Instance.GetPageChangeMetadata(this.id, this.versionNumber as number)
        );
        this._currentVersionInfo = info;
        this._currentVersionInfoSubject.next(info);
        return info;
    }


    public ClearCurrentVersionInfoCache(): void {
        this._currentVersionInfo = null;
        this._currentVersionInfoPromise = null;
        this._currentVersionInfoSubject.next(null);
    }



    /**
     * Updates the state of this PageData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PageData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PageSubmitData {
        return PageService.Instance.ConvertToPageSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PageService extends SecureEndpointBase {

    private static _instance: PageService;
    private listCache: Map<string, Observable<Array<PageData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PageBasicListData>>>;
    private recordCache: Map<string, Observable<PageData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private pageChangeHistoryService: PageChangeHistoryService,
        private menuItemService: MenuItemService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PageData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PageBasicListData>>>();
        this.recordCache = new Map<string, Observable<PageData>>();

        PageService._instance = this;
    }

    public static get Instance(): PageService {
      return PageService._instance;
    }


    public ClearListCaches(config: PageQueryParameters | null = null) {

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


    public ConvertToPageSubmitData(data: PageData): PageSubmitData {

        let output = new PageSubmitData();

        output.Id = data.Id;
        output.Title = data.Title;
        output.Slug = data.Slug;
        output.Body = data.Body;
        output.MetaDescription = data.MetaDescription;
        output.FeaturedImageUrl = data.FeaturedImageUrl;
        output.IsPublished = data.IsPublished;
        output.PublishedDate = data.PublishedDate;
        output.SortOrder = data.SortOrder;
        output.VersionNumber = data.VersionNumber;
        output.Active = data.Active;
        output.Deleted = data.Deleted;

        return output;
    }

    public GetPage(id: bigint | number, includeRelations: boolean = true) : Observable<PageData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const page$ = this.requestPage(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Page", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, page$);

            return page$;
        }

        return this.recordCache.get(configHash) as Observable<PageData>;
    }

    private requestPage(id: bigint | number, includeRelations: boolean = true) : Observable<PageData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PageData>(this.baseUrl + 'api/Page/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPage(id, includeRelations));
            }));
    }

    public GetPageList(config: PageQueryParameters | any = null) : Observable<Array<PageData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const pageList$ = this.requestPageList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Page list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, pageList$);

            return pageList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PageData>>;
    }


    private requestPageList(config: PageQueryParameters | any) : Observable <Array<PageData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PageData>>(this.baseUrl + 'api/Pages', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePageList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPageList(config));
            }));
    }

    public GetPagesRowCount(config: PageQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const pagesRowCount$ = this.requestPagesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Pages row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, pagesRowCount$);

            return pagesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPagesRowCount(config: PageQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Pages/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPagesRowCount(config));
            }));
    }

    public GetPagesBasicListData(config: PageQueryParameters | any = null) : Observable<Array<PageBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const pagesBasicListData$ = this.requestPagesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Pages basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, pagesBasicListData$);

            return pagesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PageBasicListData>>;
    }


    private requestPagesBasicListData(config: PageQueryParameters | any) : Observable<Array<PageBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PageBasicListData>>(this.baseUrl + 'api/Pages/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPagesBasicListData(config));
            }));

    }


    public PutPage(id: bigint | number, page: PageSubmitData) : Observable<PageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PageData>(this.baseUrl + 'api/Page/' + id.toString(), page, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPage(id, page));
            }));
    }


    public PostPage(page: PageSubmitData) : Observable<PageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PageData>(this.baseUrl + 'api/Page', page, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePage(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPage(page));
            }));
    }

  
    public DeletePage(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Page/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePage(id));
            }));
    }

    public RollbackPage(id: bigint | number, versionNumber: bigint | number) : Observable<PageData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PageData>(this.baseUrl + 'api/Page/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackPage(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Page.
     */
    public GetPageChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<PageData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PageData>>(this.baseUrl + 'api/Page/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPageChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Page.
     */
    public GetPageAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<PageData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PageData>[]>(this.baseUrl + 'api/Page/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPageAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Page.
     */
    public GetPageVersion(id: bigint | number, version: number): Observable<PageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PageData>(this.baseUrl + 'api/Page/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPageVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Page at a specific point in time.
     */
    public GetPageStateAtTime(id: bigint | number, time: string): Observable<PageData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PageData>(this.baseUrl + 'api/Page/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPageStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: PageQueryParameters | any): string {

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

    public userIsCommunityPageReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityPageReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.Pages
        //
        if (userIsCommunityPageReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityPageReader = user.readPermission >= 1;
            } else {
                userIsCommunityPageReader = false;
            }
        }

        return userIsCommunityPageReader;
    }


    public userIsCommunityPageWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityPageWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.Pages
        //
        if (userIsCommunityPageWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityPageWriter = user.writePermission >= 10;
          } else {
            userIsCommunityPageWriter = false;
          }      
        }

        return userIsCommunityPageWriter;
    }

    public GetPageChangeHistoriesForPage(pageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PageChangeHistoryData[]> {
        return this.pageChangeHistoryService.GetPageChangeHistoryList({
            pageId: pageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetMenuItemsForPage(pageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MenuItemData[]> {
        return this.menuItemService.GetMenuItemList({
            pageId: pageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PageData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PageData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PageTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePage(raw: any): PageData {
    if (!raw) return raw;

    //
    // Create a PageData object instance with correct prototype
    //
    const revived = Object.create(PageData.prototype) as PageData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._pageChangeHistories = null;
    (revived as any)._pageChangeHistoriesPromise = null;
    (revived as any)._pageChangeHistoriesSubject = new BehaviorSubject<PageChangeHistoryData[] | null>(null);

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
    // 2. But private methods (loadPageXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).PageChangeHistories$ = (revived as any)._pageChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._pageChangeHistories === null && (revived as any)._pageChangeHistoriesPromise === null) {
                (revived as any).loadPageChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._pageChangeHistoriesCount$ = null;


    (revived as any).MenuItems$ = (revived as any)._menuItemsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._menuItems === null && (revived as any)._menuItemsPromise === null) {
                (revived as any).loadMenuItems();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._menuItemsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PageData> | null>(null);

    (revived as any).CurrentVersionInfo$ = (revived as any)._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if ((revived as any)._currentVersionInfo === null && (revived as any)._currentVersionInfoPromise === null) {
                (revived as any).loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    return revived;
  }

  private RevivePageList(rawList: any[]): PageData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePage(raw));
  }

}
