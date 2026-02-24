/*

   GENERATED SERVICE FOR THE USERCOLLECTION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserCollection table.

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
import { UserCollectionChangeHistoryService, UserCollectionChangeHistoryData } from './user-collection-change-history.service';
import { UserCollectionPartService, UserCollectionPartData } from './user-collection-part.service';
import { UserWishlistItemService, UserWishlistItemData } from './user-wishlist-item.service';
import { UserCollectionSetImportService, UserCollectionSetImportData } from './user-collection-set-import.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserCollectionQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    isDefault: boolean | null | undefined = null;
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
export class UserCollectionSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isDefault!: boolean;
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

export class UserCollectionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserCollectionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userCollection.UserCollectionChildren$` — use with `| async` in templates
//        • Promise:    `userCollection.UserCollectionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userCollection.UserCollectionChildren$ | async"`), or
//        • Access the promise getter (`userCollection.UserCollectionChildren` or `await userCollection.UserCollectionChildren`)
//    - Simply reading `userCollection.UserCollectionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userCollection.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserCollectionData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isDefault!: boolean;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _userCollectionChangeHistories: UserCollectionChangeHistoryData[] | null = null;
    private _userCollectionChangeHistoriesPromise: Promise<UserCollectionChangeHistoryData[]> | null  = null;
    private _userCollectionChangeHistoriesSubject = new BehaviorSubject<UserCollectionChangeHistoryData[] | null>(null);

                
    private _userCollectionParts: UserCollectionPartData[] | null = null;
    private _userCollectionPartsPromise: Promise<UserCollectionPartData[]> | null  = null;
    private _userCollectionPartsSubject = new BehaviorSubject<UserCollectionPartData[] | null>(null);

                
    private _userWishlistItems: UserWishlistItemData[] | null = null;
    private _userWishlistItemsPromise: Promise<UserWishlistItemData[]> | null  = null;
    private _userWishlistItemsSubject = new BehaviorSubject<UserWishlistItemData[] | null>(null);

                
    private _userCollectionSetImports: UserCollectionSetImportData[] | null = null;
    private _userCollectionSetImportsPromise: Promise<UserCollectionSetImportData[]> | null  = null;
    private _userCollectionSetImportsSubject = new BehaviorSubject<UserCollectionSetImportData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<UserCollectionData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<UserCollectionData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserCollectionData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public UserCollectionChangeHistories$ = this._userCollectionChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userCollectionChangeHistories === null && this._userCollectionChangeHistoriesPromise === null) {
            this.loadUserCollectionChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userCollectionChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get UserCollectionChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._userCollectionChangeHistoriesCount$ === null) {
            this._userCollectionChangeHistoriesCount$ = UserCollectionChangeHistoryService.Instance.GetUserCollectionChangeHistoriesRowCount({userCollectionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userCollectionChangeHistoriesCount$;
    }



    public UserCollectionParts$ = this._userCollectionPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userCollectionParts === null && this._userCollectionPartsPromise === null) {
            this.loadUserCollectionParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userCollectionPartsCount$: Observable<bigint | number> | null = null;
    public get UserCollectionPartsCount$(): Observable<bigint | number> {
        if (this._userCollectionPartsCount$ === null) {
            this._userCollectionPartsCount$ = UserCollectionPartService.Instance.GetUserCollectionPartsRowCount({userCollectionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userCollectionPartsCount$;
    }



    public UserWishlistItems$ = this._userWishlistItemsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userWishlistItems === null && this._userWishlistItemsPromise === null) {
            this.loadUserWishlistItems(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userWishlistItemsCount$: Observable<bigint | number> | null = null;
    public get UserWishlistItemsCount$(): Observable<bigint | number> {
        if (this._userWishlistItemsCount$ === null) {
            this._userWishlistItemsCount$ = UserWishlistItemService.Instance.GetUserWishlistItemsRowCount({userCollectionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userWishlistItemsCount$;
    }



    public UserCollectionSetImports$ = this._userCollectionSetImportsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userCollectionSetImports === null && this._userCollectionSetImportsPromise === null) {
            this.loadUserCollectionSetImports(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userCollectionSetImportsCount$: Observable<bigint | number> | null = null;
    public get UserCollectionSetImportsCount$(): Observable<bigint | number> {
        if (this._userCollectionSetImportsCount$ === null) {
            this._userCollectionSetImportsCount$ = UserCollectionSetImportService.Instance.GetUserCollectionSetImportsRowCount({userCollectionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userCollectionSetImportsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any UserCollectionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userCollection.Reload();
  //
  //  Non Async:
  //
  //     userCollection[0].Reload().then(x => {
  //        this.userCollection = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserCollectionService.Instance.GetUserCollection(this.id, includeRelations)
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
     this._userCollectionChangeHistories = null;
     this._userCollectionChangeHistoriesPromise = null;
     this._userCollectionChangeHistoriesSubject.next(null);
     this._userCollectionChangeHistoriesCount$ = null;

     this._userCollectionParts = null;
     this._userCollectionPartsPromise = null;
     this._userCollectionPartsSubject.next(null);
     this._userCollectionPartsCount$ = null;

     this._userWishlistItems = null;
     this._userWishlistItemsPromise = null;
     this._userWishlistItemsSubject.next(null);
     this._userWishlistItemsCount$ = null;

     this._userCollectionSetImports = null;
     this._userCollectionSetImportsPromise = null;
     this._userCollectionSetImportsSubject.next(null);
     this._userCollectionSetImportsCount$ = null;

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
     * Gets the UserCollectionChangeHistories for this UserCollection.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userCollection.UserCollectionChangeHistories.then(userCollections => { ... })
     *   or
     *   await this.userCollection.userCollections
     *
    */
    public get UserCollectionChangeHistories(): Promise<UserCollectionChangeHistoryData[]> {
        if (this._userCollectionChangeHistories !== null) {
            return Promise.resolve(this._userCollectionChangeHistories);
        }

        if (this._userCollectionChangeHistoriesPromise !== null) {
            return this._userCollectionChangeHistoriesPromise;
        }

        // Start the load
        this.loadUserCollectionChangeHistories();

        return this._userCollectionChangeHistoriesPromise!;
    }



    private loadUserCollectionChangeHistories(): void {

        this._userCollectionChangeHistoriesPromise = lastValueFrom(
            UserCollectionService.Instance.GetUserCollectionChangeHistoriesForUserCollection(this.id)
        )
        .then(UserCollectionChangeHistories => {
            this._userCollectionChangeHistories = UserCollectionChangeHistories ?? [];
            this._userCollectionChangeHistoriesSubject.next(this._userCollectionChangeHistories);
            return this._userCollectionChangeHistories;
         })
        .catch(err => {
            this._userCollectionChangeHistories = [];
            this._userCollectionChangeHistoriesSubject.next(this._userCollectionChangeHistories);
            throw err;
        })
        .finally(() => {
            this._userCollectionChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserCollectionChangeHistory. Call after mutations to force refresh.
     */
    public ClearUserCollectionChangeHistoriesCache(): void {
        this._userCollectionChangeHistories = null;
        this._userCollectionChangeHistoriesPromise = null;
        this._userCollectionChangeHistoriesSubject.next(this._userCollectionChangeHistories);      // Emit to observable
    }

    public get HasUserCollectionChangeHistories(): Promise<boolean> {
        return this.UserCollectionChangeHistories.then(userCollectionChangeHistories => userCollectionChangeHistories.length > 0);
    }


    /**
     *
     * Gets the UserCollectionParts for this UserCollection.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userCollection.UserCollectionParts.then(userCollections => { ... })
     *   or
     *   await this.userCollection.userCollections
     *
    */
    public get UserCollectionParts(): Promise<UserCollectionPartData[]> {
        if (this._userCollectionParts !== null) {
            return Promise.resolve(this._userCollectionParts);
        }

        if (this._userCollectionPartsPromise !== null) {
            return this._userCollectionPartsPromise;
        }

        // Start the load
        this.loadUserCollectionParts();

        return this._userCollectionPartsPromise!;
    }



    private loadUserCollectionParts(): void {

        this._userCollectionPartsPromise = lastValueFrom(
            UserCollectionService.Instance.GetUserCollectionPartsForUserCollection(this.id)
        )
        .then(UserCollectionParts => {
            this._userCollectionParts = UserCollectionParts ?? [];
            this._userCollectionPartsSubject.next(this._userCollectionParts);
            return this._userCollectionParts;
         })
        .catch(err => {
            this._userCollectionParts = [];
            this._userCollectionPartsSubject.next(this._userCollectionParts);
            throw err;
        })
        .finally(() => {
            this._userCollectionPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserCollectionPart. Call after mutations to force refresh.
     */
    public ClearUserCollectionPartsCache(): void {
        this._userCollectionParts = null;
        this._userCollectionPartsPromise = null;
        this._userCollectionPartsSubject.next(this._userCollectionParts);      // Emit to observable
    }

    public get HasUserCollectionParts(): Promise<boolean> {
        return this.UserCollectionParts.then(userCollectionParts => userCollectionParts.length > 0);
    }


    /**
     *
     * Gets the UserWishlistItems for this UserCollection.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userCollection.UserWishlistItems.then(userCollections => { ... })
     *   or
     *   await this.userCollection.userCollections
     *
    */
    public get UserWishlistItems(): Promise<UserWishlistItemData[]> {
        if (this._userWishlistItems !== null) {
            return Promise.resolve(this._userWishlistItems);
        }

        if (this._userWishlistItemsPromise !== null) {
            return this._userWishlistItemsPromise;
        }

        // Start the load
        this.loadUserWishlistItems();

        return this._userWishlistItemsPromise!;
    }



    private loadUserWishlistItems(): void {

        this._userWishlistItemsPromise = lastValueFrom(
            UserCollectionService.Instance.GetUserWishlistItemsForUserCollection(this.id)
        )
        .then(UserWishlistItems => {
            this._userWishlistItems = UserWishlistItems ?? [];
            this._userWishlistItemsSubject.next(this._userWishlistItems);
            return this._userWishlistItems;
         })
        .catch(err => {
            this._userWishlistItems = [];
            this._userWishlistItemsSubject.next(this._userWishlistItems);
            throw err;
        })
        .finally(() => {
            this._userWishlistItemsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserWishlistItem. Call after mutations to force refresh.
     */
    public ClearUserWishlistItemsCache(): void {
        this._userWishlistItems = null;
        this._userWishlistItemsPromise = null;
        this._userWishlistItemsSubject.next(this._userWishlistItems);      // Emit to observable
    }

    public get HasUserWishlistItems(): Promise<boolean> {
        return this.UserWishlistItems.then(userWishlistItems => userWishlistItems.length > 0);
    }


    /**
     *
     * Gets the UserCollectionSetImports for this UserCollection.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userCollection.UserCollectionSetImports.then(userCollections => { ... })
     *   or
     *   await this.userCollection.userCollections
     *
    */
    public get UserCollectionSetImports(): Promise<UserCollectionSetImportData[]> {
        if (this._userCollectionSetImports !== null) {
            return Promise.resolve(this._userCollectionSetImports);
        }

        if (this._userCollectionSetImportsPromise !== null) {
            return this._userCollectionSetImportsPromise;
        }

        // Start the load
        this.loadUserCollectionSetImports();

        return this._userCollectionSetImportsPromise!;
    }



    private loadUserCollectionSetImports(): void {

        this._userCollectionSetImportsPromise = lastValueFrom(
            UserCollectionService.Instance.GetUserCollectionSetImportsForUserCollection(this.id)
        )
        .then(UserCollectionSetImports => {
            this._userCollectionSetImports = UserCollectionSetImports ?? [];
            this._userCollectionSetImportsSubject.next(this._userCollectionSetImports);
            return this._userCollectionSetImports;
         })
        .catch(err => {
            this._userCollectionSetImports = [];
            this._userCollectionSetImportsSubject.next(this._userCollectionSetImports);
            throw err;
        })
        .finally(() => {
            this._userCollectionSetImportsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserCollectionSetImport. Call after mutations to force refresh.
     */
    public ClearUserCollectionSetImportsCache(): void {
        this._userCollectionSetImports = null;
        this._userCollectionSetImportsPromise = null;
        this._userCollectionSetImportsSubject.next(this._userCollectionSetImports);      // Emit to observable
    }

    public get HasUserCollectionSetImports(): Promise<boolean> {
        return this.UserCollectionSetImports.then(userCollectionSetImports => userCollectionSetImports.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (userCollection.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await userCollection.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<UserCollectionData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<UserCollectionData>> {
        const info = await lastValueFrom(
            UserCollectionService.Instance.GetUserCollectionChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this UserCollectionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserCollectionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserCollectionSubmitData {
        return UserCollectionService.Instance.ConvertToUserCollectionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserCollectionService extends SecureEndpointBase {

    private static _instance: UserCollectionService;
    private listCache: Map<string, Observable<Array<UserCollectionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserCollectionBasicListData>>>;
    private recordCache: Map<string, Observable<UserCollectionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private userCollectionChangeHistoryService: UserCollectionChangeHistoryService,
        private userCollectionPartService: UserCollectionPartService,
        private userWishlistItemService: UserWishlistItemService,
        private userCollectionSetImportService: UserCollectionSetImportService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserCollectionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserCollectionBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserCollectionData>>();

        UserCollectionService._instance = this;
    }

    public static get Instance(): UserCollectionService {
      return UserCollectionService._instance;
    }


    public ClearListCaches(config: UserCollectionQueryParameters | null = null) {

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


    public ConvertToUserCollectionSubmitData(data: UserCollectionData): UserCollectionSubmitData {

        let output = new UserCollectionSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.isDefault = data.isDefault;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserCollection(id: bigint | number, includeRelations: boolean = true) : Observable<UserCollectionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userCollection$ = this.requestUserCollection(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserCollection", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userCollection$);

            return userCollection$;
        }

        return this.recordCache.get(configHash) as Observable<UserCollectionData>;
    }

    private requestUserCollection(id: bigint | number, includeRelations: boolean = true) : Observable<UserCollectionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserCollectionData>(this.baseUrl + 'api/UserCollection/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserCollection(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserCollection(id, includeRelations));
            }));
    }

    public GetUserCollectionList(config: UserCollectionQueryParameters | any = null) : Observable<Array<UserCollectionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userCollectionList$ = this.requestUserCollectionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserCollection list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userCollectionList$);

            return userCollectionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserCollectionData>>;
    }


    private requestUserCollectionList(config: UserCollectionQueryParameters | any) : Observable <Array<UserCollectionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserCollectionData>>(this.baseUrl + 'api/UserCollections', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserCollectionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserCollectionList(config));
            }));
    }

    public GetUserCollectionsRowCount(config: UserCollectionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userCollectionsRowCount$ = this.requestUserCollectionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserCollections row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userCollectionsRowCount$);

            return userCollectionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserCollectionsRowCount(config: UserCollectionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserCollections/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserCollectionsRowCount(config));
            }));
    }

    public GetUserCollectionsBasicListData(config: UserCollectionQueryParameters | any = null) : Observable<Array<UserCollectionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userCollectionsBasicListData$ = this.requestUserCollectionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserCollections basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userCollectionsBasicListData$);

            return userCollectionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserCollectionBasicListData>>;
    }


    private requestUserCollectionsBasicListData(config: UserCollectionQueryParameters | any) : Observable<Array<UserCollectionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserCollectionBasicListData>>(this.baseUrl + 'api/UserCollections/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserCollectionsBasicListData(config));
            }));

    }


    public PutUserCollection(id: bigint | number, userCollection: UserCollectionSubmitData) : Observable<UserCollectionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserCollectionData>(this.baseUrl + 'api/UserCollection/' + id.toString(), userCollection, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserCollection(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserCollection(id, userCollection));
            }));
    }


    public PostUserCollection(userCollection: UserCollectionSubmitData) : Observable<UserCollectionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserCollectionData>(this.baseUrl + 'api/UserCollection', userCollection, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserCollection(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserCollection(userCollection));
            }));
    }

  
    public DeleteUserCollection(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserCollection/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserCollection(id));
            }));
    }

    public RollbackUserCollection(id: bigint | number, versionNumber: bigint | number) : Observable<UserCollectionData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserCollectionData>(this.baseUrl + 'api/UserCollection/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserCollection(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackUserCollection(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a UserCollection.
     */
    public GetUserCollectionChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<UserCollectionData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserCollectionData>>(this.baseUrl + 'api/UserCollection/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserCollectionChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a UserCollection.
     */
    public GetUserCollectionAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<UserCollectionData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserCollectionData>[]>(this.baseUrl + 'api/UserCollection/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserCollectionAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a UserCollection.
     */
    public GetUserCollectionVersion(id: bigint | number, version: number): Observable<UserCollectionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserCollectionData>(this.baseUrl + 'api/UserCollection/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserCollection(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserCollectionVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a UserCollection at a specific point in time.
     */
    public GetUserCollectionStateAtTime(id: bigint | number, time: string): Observable<UserCollectionData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserCollectionData>(this.baseUrl + 'api/UserCollection/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserCollection(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserCollectionStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: UserCollectionQueryParameters | any): string {

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

    public userIsBMCUserCollectionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCUserCollectionReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.UserCollections
        //
        if (userIsBMCUserCollectionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCUserCollectionReader = user.readPermission >= 1;
            } else {
                userIsBMCUserCollectionReader = false;
            }
        }

        return userIsBMCUserCollectionReader;
    }


    public userIsBMCUserCollectionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCUserCollectionWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.UserCollections
        //
        if (userIsBMCUserCollectionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCUserCollectionWriter = user.writePermission >= 10;
          } else {
            userIsBMCUserCollectionWriter = false;
          }      
        }

        return userIsBMCUserCollectionWriter;
    }

    public GetUserCollectionChangeHistoriesForUserCollection(userCollectionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserCollectionChangeHistoryData[]> {
        return this.userCollectionChangeHistoryService.GetUserCollectionChangeHistoryList({
            userCollectionId: userCollectionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserCollectionPartsForUserCollection(userCollectionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserCollectionPartData[]> {
        return this.userCollectionPartService.GetUserCollectionPartList({
            userCollectionId: userCollectionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserWishlistItemsForUserCollection(userCollectionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserWishlistItemData[]> {
        return this.userWishlistItemService.GetUserWishlistItemList({
            userCollectionId: userCollectionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserCollectionSetImportsForUserCollection(userCollectionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserCollectionSetImportData[]> {
        return this.userCollectionSetImportService.GetUserCollectionSetImportList({
            userCollectionId: userCollectionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full UserCollectionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserCollectionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserCollectionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserCollection(raw: any): UserCollectionData {
    if (!raw) return raw;

    //
    // Create a UserCollectionData object instance with correct prototype
    //
    const revived = Object.create(UserCollectionData.prototype) as UserCollectionData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._userCollectionChangeHistories = null;
    (revived as any)._userCollectionChangeHistoriesPromise = null;
    (revived as any)._userCollectionChangeHistoriesSubject = new BehaviorSubject<UserCollectionChangeHistoryData[] | null>(null);

    (revived as any)._userCollectionParts = null;
    (revived as any)._userCollectionPartsPromise = null;
    (revived as any)._userCollectionPartsSubject = new BehaviorSubject<UserCollectionPartData[] | null>(null);

    (revived as any)._userWishlistItems = null;
    (revived as any)._userWishlistItemsPromise = null;
    (revived as any)._userWishlistItemsSubject = new BehaviorSubject<UserWishlistItemData[] | null>(null);

    (revived as any)._userCollectionSetImports = null;
    (revived as any)._userCollectionSetImportsPromise = null;
    (revived as any)._userCollectionSetImportsSubject = new BehaviorSubject<UserCollectionSetImportData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadUserCollectionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).UserCollectionChangeHistories$ = (revived as any)._userCollectionChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userCollectionChangeHistories === null && (revived as any)._userCollectionChangeHistoriesPromise === null) {
                (revived as any).loadUserCollectionChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userCollectionChangeHistoriesCount$ = null;


    (revived as any).UserCollectionParts$ = (revived as any)._userCollectionPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userCollectionParts === null && (revived as any)._userCollectionPartsPromise === null) {
                (revived as any).loadUserCollectionParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userCollectionPartsCount$ = null;


    (revived as any).UserWishlistItems$ = (revived as any)._userWishlistItemsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userWishlistItems === null && (revived as any)._userWishlistItemsPromise === null) {
                (revived as any).loadUserWishlistItems();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userWishlistItemsCount$ = null;


    (revived as any).UserCollectionSetImports$ = (revived as any)._userCollectionSetImportsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userCollectionSetImports === null && (revived as any)._userCollectionSetImportsPromise === null) {
                (revived as any).loadUserCollectionSetImports();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userCollectionSetImportsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserCollectionData> | null>(null);

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

  private ReviveUserCollectionList(rawList: any[]): UserCollectionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserCollection(raw));
  }

}
