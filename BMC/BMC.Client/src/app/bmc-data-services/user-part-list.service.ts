/*

   GENERATED SERVICE FOR THE USERPARTLIST TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserPartList table.

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
import { UserPartListChangeHistoryService, UserPartListChangeHistoryData } from './user-part-list-change-history.service';
import { UserPartListItemService, UserPartListItemData } from './user-part-list-item.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserPartListQueryParameters {
    name: string | null | undefined = null;
    isBuildable: boolean | null | undefined = null;
    rebrickableListId: bigint | number | null | undefined = null;
    versionNumber: bigint | number | null | undefined = null;
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
export class UserPartListSubmitData {
    id!: bigint | number;
    name!: string;
    isBuildable!: boolean;
    rebrickableListId: bigint | number | null = null;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
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

export class UserPartListBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserPartListChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userPartList.UserPartListChildren$` — use with `| async` in templates
//        • Promise:    `userPartList.UserPartListChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userPartList.UserPartListChildren$ | async"`), or
//        • Access the promise getter (`userPartList.UserPartListChildren` or `await userPartList.UserPartListChildren`)
//    - Simply reading `userPartList.UserPartListChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userPartList.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserPartListData {
    id!: bigint | number;
    name!: string;
    isBuildable!: boolean;
    rebrickableListId!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _userPartListChangeHistories: UserPartListChangeHistoryData[] | null = null;
    private _userPartListChangeHistoriesPromise: Promise<UserPartListChangeHistoryData[]> | null  = null;
    private _userPartListChangeHistoriesSubject = new BehaviorSubject<UserPartListChangeHistoryData[] | null>(null);

                
    private _userPartListItems: UserPartListItemData[] | null = null;
    private _userPartListItemsPromise: Promise<UserPartListItemData[]> | null  = null;
    private _userPartListItemsSubject = new BehaviorSubject<UserPartListItemData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<UserPartListData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<UserPartListData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserPartListData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public UserPartListChangeHistories$ = this._userPartListChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userPartListChangeHistories === null && this._userPartListChangeHistoriesPromise === null) {
            this.loadUserPartListChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userPartListChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get UserPartListChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._userPartListChangeHistoriesCount$ === null) {
            this._userPartListChangeHistoriesCount$ = UserPartListChangeHistoryService.Instance.GetUserPartListChangeHistoriesRowCount({userPartListId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userPartListChangeHistoriesCount$;
    }



    public UserPartListItems$ = this._userPartListItemsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userPartListItems === null && this._userPartListItemsPromise === null) {
            this.loadUserPartListItems(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userPartListItemsCount$: Observable<bigint | number> | null = null;
    public get UserPartListItemsCount$(): Observable<bigint | number> {
        if (this._userPartListItemsCount$ === null) {
            this._userPartListItemsCount$ = UserPartListItemService.Instance.GetUserPartListItemsRowCount({userPartListId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userPartListItemsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any UserPartListData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userPartList.Reload();
  //
  //  Non Async:
  //
  //     userPartList[0].Reload().then(x => {
  //        this.userPartList = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserPartListService.Instance.GetUserPartList(this.id, includeRelations)
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
     this._userPartListChangeHistories = null;
     this._userPartListChangeHistoriesPromise = null;
     this._userPartListChangeHistoriesSubject.next(null);
     this._userPartListChangeHistoriesCount$ = null;

     this._userPartListItems = null;
     this._userPartListItemsPromise = null;
     this._userPartListItemsSubject.next(null);
     this._userPartListItemsCount$ = null;

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
     * Gets the UserPartListChangeHistories for this UserPartList.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userPartList.UserPartListChangeHistories.then(userPartLists => { ... })
     *   or
     *   await this.userPartList.userPartLists
     *
    */
    public get UserPartListChangeHistories(): Promise<UserPartListChangeHistoryData[]> {
        if (this._userPartListChangeHistories !== null) {
            return Promise.resolve(this._userPartListChangeHistories);
        }

        if (this._userPartListChangeHistoriesPromise !== null) {
            return this._userPartListChangeHistoriesPromise;
        }

        // Start the load
        this.loadUserPartListChangeHistories();

        return this._userPartListChangeHistoriesPromise!;
    }



    private loadUserPartListChangeHistories(): void {

        this._userPartListChangeHistoriesPromise = lastValueFrom(
            UserPartListService.Instance.GetUserPartListChangeHistoriesForUserPartList(this.id)
        )
        .then(UserPartListChangeHistories => {
            this._userPartListChangeHistories = UserPartListChangeHistories ?? [];
            this._userPartListChangeHistoriesSubject.next(this._userPartListChangeHistories);
            return this._userPartListChangeHistories;
         })
        .catch(err => {
            this._userPartListChangeHistories = [];
            this._userPartListChangeHistoriesSubject.next(this._userPartListChangeHistories);
            throw err;
        })
        .finally(() => {
            this._userPartListChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserPartListChangeHistory. Call after mutations to force refresh.
     */
    public ClearUserPartListChangeHistoriesCache(): void {
        this._userPartListChangeHistories = null;
        this._userPartListChangeHistoriesPromise = null;
        this._userPartListChangeHistoriesSubject.next(this._userPartListChangeHistories);      // Emit to observable
    }

    public get HasUserPartListChangeHistories(): Promise<boolean> {
        return this.UserPartListChangeHistories.then(userPartListChangeHistories => userPartListChangeHistories.length > 0);
    }


    /**
     *
     * Gets the UserPartListItems for this UserPartList.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userPartList.UserPartListItems.then(userPartLists => { ... })
     *   or
     *   await this.userPartList.userPartLists
     *
    */
    public get UserPartListItems(): Promise<UserPartListItemData[]> {
        if (this._userPartListItems !== null) {
            return Promise.resolve(this._userPartListItems);
        }

        if (this._userPartListItemsPromise !== null) {
            return this._userPartListItemsPromise;
        }

        // Start the load
        this.loadUserPartListItems();

        return this._userPartListItemsPromise!;
    }



    private loadUserPartListItems(): void {

        this._userPartListItemsPromise = lastValueFrom(
            UserPartListService.Instance.GetUserPartListItemsForUserPartList(this.id)
        )
        .then(UserPartListItems => {
            this._userPartListItems = UserPartListItems ?? [];
            this._userPartListItemsSubject.next(this._userPartListItems);
            return this._userPartListItems;
         })
        .catch(err => {
            this._userPartListItems = [];
            this._userPartListItemsSubject.next(this._userPartListItems);
            throw err;
        })
        .finally(() => {
            this._userPartListItemsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserPartListItem. Call after mutations to force refresh.
     */
    public ClearUserPartListItemsCache(): void {
        this._userPartListItems = null;
        this._userPartListItemsPromise = null;
        this._userPartListItemsSubject.next(this._userPartListItems);      // Emit to observable
    }

    public get HasUserPartListItems(): Promise<boolean> {
        return this.UserPartListItems.then(userPartListItems => userPartListItems.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (userPartList.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await userPartList.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<UserPartListData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<UserPartListData>> {
        const info = await lastValueFrom(
            UserPartListService.Instance.GetUserPartListChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this UserPartListData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserPartListData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserPartListSubmitData {
        return UserPartListService.Instance.ConvertToUserPartListSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserPartListService extends SecureEndpointBase {

    private static _instance: UserPartListService;
    private listCache: Map<string, Observable<Array<UserPartListData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserPartListBasicListData>>>;
    private recordCache: Map<string, Observable<UserPartListData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private userPartListChangeHistoryService: UserPartListChangeHistoryService,
        private userPartListItemService: UserPartListItemService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserPartListData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserPartListBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserPartListData>>();

        UserPartListService._instance = this;
    }

    public static get Instance(): UserPartListService {
      return UserPartListService._instance;
    }


    public ClearListCaches(config: UserPartListQueryParameters | null = null) {

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


    public ConvertToUserPartListSubmitData(data: UserPartListData): UserPartListSubmitData {

        let output = new UserPartListSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.isBuildable = data.isBuildable;
        output.rebrickableListId = data.rebrickableListId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserPartList(id: bigint | number, includeRelations: boolean = true) : Observable<UserPartListData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userPartList$ = this.requestUserPartList(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserPartList", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userPartList$);

            return userPartList$;
        }

        return this.recordCache.get(configHash) as Observable<UserPartListData>;
    }

    private requestUserPartList(id: bigint | number, includeRelations: boolean = true) : Observable<UserPartListData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserPartListData>(this.baseUrl + 'api/UserPartList/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserPartList(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserPartList(id, includeRelations));
            }));
    }

    public GetUserPartListList(config: UserPartListQueryParameters | any = null) : Observable<Array<UserPartListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userPartListList$ = this.requestUserPartListList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserPartList list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userPartListList$);

            return userPartListList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserPartListData>>;
    }


    private requestUserPartListList(config: UserPartListQueryParameters | any) : Observable <Array<UserPartListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserPartListData>>(this.baseUrl + 'api/UserPartLists', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserPartListList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserPartListList(config));
            }));
    }

    public GetUserPartListsRowCount(config: UserPartListQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userPartListsRowCount$ = this.requestUserPartListsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserPartLists row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userPartListsRowCount$);

            return userPartListsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserPartListsRowCount(config: UserPartListQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserPartLists/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserPartListsRowCount(config));
            }));
    }

    public GetUserPartListsBasicListData(config: UserPartListQueryParameters | any = null) : Observable<Array<UserPartListBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userPartListsBasicListData$ = this.requestUserPartListsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserPartLists basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userPartListsBasicListData$);

            return userPartListsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserPartListBasicListData>>;
    }


    private requestUserPartListsBasicListData(config: UserPartListQueryParameters | any) : Observable<Array<UserPartListBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserPartListBasicListData>>(this.baseUrl + 'api/UserPartLists/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserPartListsBasicListData(config));
            }));

    }


    public PutUserPartList(id: bigint | number, userPartList: UserPartListSubmitData) : Observable<UserPartListData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserPartListData>(this.baseUrl + 'api/UserPartList/' + id.toString(), userPartList, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserPartList(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserPartList(id, userPartList));
            }));
    }


    public PostUserPartList(userPartList: UserPartListSubmitData) : Observable<UserPartListData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserPartListData>(this.baseUrl + 'api/UserPartList', userPartList, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserPartList(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserPartList(userPartList));
            }));
    }

  
    public DeleteUserPartList(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserPartList/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserPartList(id));
            }));
    }

    public RollbackUserPartList(id: bigint | number, versionNumber: bigint | number) : Observable<UserPartListData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserPartListData>(this.baseUrl + 'api/UserPartList/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserPartList(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackUserPartList(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a UserPartList.
     */
    public GetUserPartListChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<UserPartListData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserPartListData>>(this.baseUrl + 'api/UserPartList/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserPartListChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a UserPartList.
     */
    public GetUserPartListAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<UserPartListData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserPartListData>[]>(this.baseUrl + 'api/UserPartList/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserPartListAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a UserPartList.
     */
    public GetUserPartListVersion(id: bigint | number, version: number): Observable<UserPartListData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserPartListData>(this.baseUrl + 'api/UserPartList/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserPartList(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserPartListVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a UserPartList at a specific point in time.
     */
    public GetUserPartListStateAtTime(id: bigint | number, time: string): Observable<UserPartListData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserPartListData>(this.baseUrl + 'api/UserPartList/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserPartList(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserPartListStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: UserPartListQueryParameters | any): string {

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

    public userIsBMCUserPartListReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCUserPartListReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.UserPartLists
        //
        if (userIsBMCUserPartListReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCUserPartListReader = user.readPermission >= 1;
            } else {
                userIsBMCUserPartListReader = false;
            }
        }

        return userIsBMCUserPartListReader;
    }


    public userIsBMCUserPartListWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCUserPartListWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.UserPartLists
        //
        if (userIsBMCUserPartListWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCUserPartListWriter = user.writePermission >= 10;
          } else {
            userIsBMCUserPartListWriter = false;
          }      
        }

        return userIsBMCUserPartListWriter;
    }

    public GetUserPartListChangeHistoriesForUserPartList(userPartListId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserPartListChangeHistoryData[]> {
        return this.userPartListChangeHistoryService.GetUserPartListChangeHistoryList({
            userPartListId: userPartListId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserPartListItemsForUserPartList(userPartListId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserPartListItemData[]> {
        return this.userPartListItemService.GetUserPartListItemList({
            userPartListId: userPartListId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full UserPartListData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserPartListData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserPartListTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserPartList(raw: any): UserPartListData {
    if (!raw) return raw;

    //
    // Create a UserPartListData object instance with correct prototype
    //
    const revived = Object.create(UserPartListData.prototype) as UserPartListData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._userPartListChangeHistories = null;
    (revived as any)._userPartListChangeHistoriesPromise = null;
    (revived as any)._userPartListChangeHistoriesSubject = new BehaviorSubject<UserPartListChangeHistoryData[] | null>(null);

    (revived as any)._userPartListItems = null;
    (revived as any)._userPartListItemsPromise = null;
    (revived as any)._userPartListItemsSubject = new BehaviorSubject<UserPartListItemData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadUserPartListXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).UserPartListChangeHistories$ = (revived as any)._userPartListChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userPartListChangeHistories === null && (revived as any)._userPartListChangeHistoriesPromise === null) {
                (revived as any).loadUserPartListChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userPartListChangeHistoriesCount$ = null;


    (revived as any).UserPartListItems$ = (revived as any)._userPartListItemsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userPartListItems === null && (revived as any)._userPartListItemsPromise === null) {
                (revived as any).loadUserPartListItems();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userPartListItemsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserPartListData> | null>(null);

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

  private ReviveUserPartListList(rawList: any[]): UserPartListData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserPartList(raw));
  }

}
