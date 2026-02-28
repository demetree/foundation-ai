/*

   GENERATED SERVICE FOR THE USERSETLIST TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserSetList table.

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
import { UserSetListChangeHistoryService, UserSetListChangeHistoryData } from './user-set-list-change-history.service';
import { UserSetListItemService, UserSetListItemData } from './user-set-list-item.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserSetListQueryParameters {
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
export class UserSetListSubmitData {
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

export class UserSetListBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserSetListChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userSetList.UserSetListChildren$` — use with `| async` in templates
//        • Promise:    `userSetList.UserSetListChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userSetList.UserSetListChildren$ | async"`), or
//        • Access the promise getter (`userSetList.UserSetListChildren` or `await userSetList.UserSetListChildren`)
//    - Simply reading `userSetList.UserSetListChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userSetList.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserSetListData {
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
    private _userSetListChangeHistories: UserSetListChangeHistoryData[] | null = null;
    private _userSetListChangeHistoriesPromise: Promise<UserSetListChangeHistoryData[]> | null  = null;
    private _userSetListChangeHistoriesSubject = new BehaviorSubject<UserSetListChangeHistoryData[] | null>(null);

                
    private _userSetListItems: UserSetListItemData[] | null = null;
    private _userSetListItemsPromise: Promise<UserSetListItemData[]> | null  = null;
    private _userSetListItemsSubject = new BehaviorSubject<UserSetListItemData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<UserSetListData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<UserSetListData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserSetListData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public UserSetListChangeHistories$ = this._userSetListChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userSetListChangeHistories === null && this._userSetListChangeHistoriesPromise === null) {
            this.loadUserSetListChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userSetListChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get UserSetListChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._userSetListChangeHistoriesCount$ === null) {
            this._userSetListChangeHistoriesCount$ = UserSetListChangeHistoryService.Instance.GetUserSetListChangeHistoriesRowCount({userSetListId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userSetListChangeHistoriesCount$;
    }



    public UserSetListItems$ = this._userSetListItemsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userSetListItems === null && this._userSetListItemsPromise === null) {
            this.loadUserSetListItems(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userSetListItemsCount$: Observable<bigint | number> | null = null;
    public get UserSetListItemsCount$(): Observable<bigint | number> {
        if (this._userSetListItemsCount$ === null) {
            this._userSetListItemsCount$ = UserSetListItemService.Instance.GetUserSetListItemsRowCount({userSetListId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userSetListItemsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any UserSetListData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userSetList.Reload();
  //
  //  Non Async:
  //
  //     userSetList[0].Reload().then(x => {
  //        this.userSetList = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserSetListService.Instance.GetUserSetList(this.id, includeRelations)
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
     this._userSetListChangeHistories = null;
     this._userSetListChangeHistoriesPromise = null;
     this._userSetListChangeHistoriesSubject.next(null);
     this._userSetListChangeHistoriesCount$ = null;

     this._userSetListItems = null;
     this._userSetListItemsPromise = null;
     this._userSetListItemsSubject.next(null);
     this._userSetListItemsCount$ = null;

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
     * Gets the UserSetListChangeHistories for this UserSetList.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userSetList.UserSetListChangeHistories.then(userSetLists => { ... })
     *   or
     *   await this.userSetList.userSetLists
     *
    */
    public get UserSetListChangeHistories(): Promise<UserSetListChangeHistoryData[]> {
        if (this._userSetListChangeHistories !== null) {
            return Promise.resolve(this._userSetListChangeHistories);
        }

        if (this._userSetListChangeHistoriesPromise !== null) {
            return this._userSetListChangeHistoriesPromise;
        }

        // Start the load
        this.loadUserSetListChangeHistories();

        return this._userSetListChangeHistoriesPromise!;
    }



    private loadUserSetListChangeHistories(): void {

        this._userSetListChangeHistoriesPromise = lastValueFrom(
            UserSetListService.Instance.GetUserSetListChangeHistoriesForUserSetList(this.id)
        )
        .then(UserSetListChangeHistories => {
            this._userSetListChangeHistories = UserSetListChangeHistories ?? [];
            this._userSetListChangeHistoriesSubject.next(this._userSetListChangeHistories);
            return this._userSetListChangeHistories;
         })
        .catch(err => {
            this._userSetListChangeHistories = [];
            this._userSetListChangeHistoriesSubject.next(this._userSetListChangeHistories);
            throw err;
        })
        .finally(() => {
            this._userSetListChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserSetListChangeHistory. Call after mutations to force refresh.
     */
    public ClearUserSetListChangeHistoriesCache(): void {
        this._userSetListChangeHistories = null;
        this._userSetListChangeHistoriesPromise = null;
        this._userSetListChangeHistoriesSubject.next(this._userSetListChangeHistories);      // Emit to observable
    }

    public get HasUserSetListChangeHistories(): Promise<boolean> {
        return this.UserSetListChangeHistories.then(userSetListChangeHistories => userSetListChangeHistories.length > 0);
    }


    /**
     *
     * Gets the UserSetListItems for this UserSetList.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userSetList.UserSetListItems.then(userSetLists => { ... })
     *   or
     *   await this.userSetList.userSetLists
     *
    */
    public get UserSetListItems(): Promise<UserSetListItemData[]> {
        if (this._userSetListItems !== null) {
            return Promise.resolve(this._userSetListItems);
        }

        if (this._userSetListItemsPromise !== null) {
            return this._userSetListItemsPromise;
        }

        // Start the load
        this.loadUserSetListItems();

        return this._userSetListItemsPromise!;
    }



    private loadUserSetListItems(): void {

        this._userSetListItemsPromise = lastValueFrom(
            UserSetListService.Instance.GetUserSetListItemsForUserSetList(this.id)
        )
        .then(UserSetListItems => {
            this._userSetListItems = UserSetListItems ?? [];
            this._userSetListItemsSubject.next(this._userSetListItems);
            return this._userSetListItems;
         })
        .catch(err => {
            this._userSetListItems = [];
            this._userSetListItemsSubject.next(this._userSetListItems);
            throw err;
        })
        .finally(() => {
            this._userSetListItemsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserSetListItem. Call after mutations to force refresh.
     */
    public ClearUserSetListItemsCache(): void {
        this._userSetListItems = null;
        this._userSetListItemsPromise = null;
        this._userSetListItemsSubject.next(this._userSetListItems);      // Emit to observable
    }

    public get HasUserSetListItems(): Promise<boolean> {
        return this.UserSetListItems.then(userSetListItems => userSetListItems.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (userSetList.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await userSetList.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<UserSetListData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<UserSetListData>> {
        const info = await lastValueFrom(
            UserSetListService.Instance.GetUserSetListChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this UserSetListData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserSetListData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserSetListSubmitData {
        return UserSetListService.Instance.ConvertToUserSetListSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserSetListService extends SecureEndpointBase {

    private static _instance: UserSetListService;
    private listCache: Map<string, Observable<Array<UserSetListData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserSetListBasicListData>>>;
    private recordCache: Map<string, Observable<UserSetListData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private userSetListChangeHistoryService: UserSetListChangeHistoryService,
        private userSetListItemService: UserSetListItemService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserSetListData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserSetListBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserSetListData>>();

        UserSetListService._instance = this;
    }

    public static get Instance(): UserSetListService {
      return UserSetListService._instance;
    }


    public ClearListCaches(config: UserSetListQueryParameters | null = null) {

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


    public ConvertToUserSetListSubmitData(data: UserSetListData): UserSetListSubmitData {

        let output = new UserSetListSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.isBuildable = data.isBuildable;
        output.rebrickableListId = data.rebrickableListId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserSetList(id: bigint | number, includeRelations: boolean = true) : Observable<UserSetListData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userSetList$ = this.requestUserSetList(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserSetList", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userSetList$);

            return userSetList$;
        }

        return this.recordCache.get(configHash) as Observable<UserSetListData>;
    }

    private requestUserSetList(id: bigint | number, includeRelations: boolean = true) : Observable<UserSetListData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserSetListData>(this.baseUrl + 'api/UserSetList/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserSetList(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserSetList(id, includeRelations));
            }));
    }

    public GetUserSetListList(config: UserSetListQueryParameters | any = null) : Observable<Array<UserSetListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userSetListList$ = this.requestUserSetListList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserSetList list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userSetListList$);

            return userSetListList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserSetListData>>;
    }


    private requestUserSetListList(config: UserSetListQueryParameters | any) : Observable <Array<UserSetListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserSetListData>>(this.baseUrl + 'api/UserSetLists', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserSetListList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserSetListList(config));
            }));
    }

    public GetUserSetListsRowCount(config: UserSetListQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userSetListsRowCount$ = this.requestUserSetListsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserSetLists row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userSetListsRowCount$);

            return userSetListsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserSetListsRowCount(config: UserSetListQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserSetLists/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserSetListsRowCount(config));
            }));
    }

    public GetUserSetListsBasicListData(config: UserSetListQueryParameters | any = null) : Observable<Array<UserSetListBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userSetListsBasicListData$ = this.requestUserSetListsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserSetLists basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userSetListsBasicListData$);

            return userSetListsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserSetListBasicListData>>;
    }


    private requestUserSetListsBasicListData(config: UserSetListQueryParameters | any) : Observable<Array<UserSetListBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserSetListBasicListData>>(this.baseUrl + 'api/UserSetLists/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserSetListsBasicListData(config));
            }));

    }


    public PutUserSetList(id: bigint | number, userSetList: UserSetListSubmitData) : Observable<UserSetListData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserSetListData>(this.baseUrl + 'api/UserSetList/' + id.toString(), userSetList, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserSetList(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserSetList(id, userSetList));
            }));
    }


    public PostUserSetList(userSetList: UserSetListSubmitData) : Observable<UserSetListData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserSetListData>(this.baseUrl + 'api/UserSetList', userSetList, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserSetList(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserSetList(userSetList));
            }));
    }

  
    public DeleteUserSetList(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserSetList/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserSetList(id));
            }));
    }

    public RollbackUserSetList(id: bigint | number, versionNumber: bigint | number) : Observable<UserSetListData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserSetListData>(this.baseUrl + 'api/UserSetList/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserSetList(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackUserSetList(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a UserSetList.
     */
    public GetUserSetListChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<UserSetListData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserSetListData>>(this.baseUrl + 'api/UserSetList/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserSetListChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a UserSetList.
     */
    public GetUserSetListAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<UserSetListData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserSetListData>[]>(this.baseUrl + 'api/UserSetList/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserSetListAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a UserSetList.
     */
    public GetUserSetListVersion(id: bigint | number, version: number): Observable<UserSetListData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserSetListData>(this.baseUrl + 'api/UserSetList/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserSetList(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserSetListVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a UserSetList at a specific point in time.
     */
    public GetUserSetListStateAtTime(id: bigint | number, time: string): Observable<UserSetListData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserSetListData>(this.baseUrl + 'api/UserSetList/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserSetList(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserSetListStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: UserSetListQueryParameters | any): string {

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

    public userIsBMCUserSetListReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCUserSetListReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.UserSetLists
        //
        if (userIsBMCUserSetListReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCUserSetListReader = user.readPermission >= 1;
            } else {
                userIsBMCUserSetListReader = false;
            }
        }

        return userIsBMCUserSetListReader;
    }


    public userIsBMCUserSetListWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCUserSetListWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.UserSetLists
        //
        if (userIsBMCUserSetListWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCUserSetListWriter = user.writePermission >= 10;
          } else {
            userIsBMCUserSetListWriter = false;
          }      
        }

        return userIsBMCUserSetListWriter;
    }

    public GetUserSetListChangeHistoriesForUserSetList(userSetListId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserSetListChangeHistoryData[]> {
        return this.userSetListChangeHistoryService.GetUserSetListChangeHistoryList({
            userSetListId: userSetListId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserSetListItemsForUserSetList(userSetListId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserSetListItemData[]> {
        return this.userSetListItemService.GetUserSetListItemList({
            userSetListId: userSetListId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full UserSetListData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserSetListData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserSetListTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserSetList(raw: any): UserSetListData {
    if (!raw) return raw;

    //
    // Create a UserSetListData object instance with correct prototype
    //
    const revived = Object.create(UserSetListData.prototype) as UserSetListData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._userSetListChangeHistories = null;
    (revived as any)._userSetListChangeHistoriesPromise = null;
    (revived as any)._userSetListChangeHistoriesSubject = new BehaviorSubject<UserSetListChangeHistoryData[] | null>(null);

    (revived as any)._userSetListItems = null;
    (revived as any)._userSetListItemsPromise = null;
    (revived as any)._userSetListItemsSubject = new BehaviorSubject<UserSetListItemData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadUserSetListXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).UserSetListChangeHistories$ = (revived as any)._userSetListChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userSetListChangeHistories === null && (revived as any)._userSetListChangeHistoriesPromise === null) {
                (revived as any).loadUserSetListChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userSetListChangeHistoriesCount$ = null;


    (revived as any).UserSetListItems$ = (revived as any)._userSetListItemsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userSetListItems === null && (revived as any)._userSetListItemsPromise === null) {
                (revived as any).loadUserSetListItems();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userSetListItemsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserSetListData> | null>(null);

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

  private ReviveUserSetListList(rawList: any[]): UserSetListData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserSetList(raw));
  }

}
