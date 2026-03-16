/*

   GENERATED SERVICE FOR THE POSTCATEGORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PostCategory table.

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
import { PostService, PostData } from './post.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PostCategoryQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    slug: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class PostCategorySubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    slug!: string;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class PostCategoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PostCategoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `postCategory.PostCategoryChildren$` — use with `| async` in templates
//        • Promise:    `postCategory.PostCategoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="postCategory.PostCategoryChildren$ | async"`), or
//        • Access the promise getter (`postCategory.PostCategoryChildren` or `await postCategory.PostCategoryChildren`)
//    - Simply reading `postCategory.PostCategoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await postCategory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PostCategoryData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    slug!: string;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _posts: PostData[] | null = null;
    private _postsPromise: Promise<PostData[]> | null  = null;
    private _postsSubject = new BehaviorSubject<PostData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Posts$ = this._postsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._posts === null && this._postsPromise === null) {
            this.loadPosts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _postsCount$: Observable<bigint | number> | null = null;
    public get PostsCount$(): Observable<bigint | number> {
        if (this._postsCount$ === null) {
            this._postsCount$ = PostService.Instance.GetPostsRowCount({postCategoryId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._postsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PostCategoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.postCategory.Reload();
  //
  //  Non Async:
  //
  //     postCategory[0].Reload().then(x => {
  //        this.postCategory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PostCategoryService.Instance.GetPostCategory(this.id, includeRelations)
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
     this._posts = null;
     this._postsPromise = null;
     this._postsSubject.next(null);
     this._postsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Posts for this PostCategory.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.postCategory.Posts.then(postCategories => { ... })
     *   or
     *   await this.postCategory.postCategories
     *
    */
    public get Posts(): Promise<PostData[]> {
        if (this._posts !== null) {
            return Promise.resolve(this._posts);
        }

        if (this._postsPromise !== null) {
            return this._postsPromise;
        }

        // Start the load
        this.loadPosts();

        return this._postsPromise!;
    }



    private loadPosts(): void {

        this._postsPromise = lastValueFrom(
            PostCategoryService.Instance.GetPostsForPostCategory(this.id)
        )
        .then(Posts => {
            this._posts = Posts ?? [];
            this._postsSubject.next(this._posts);
            return this._posts;
         })
        .catch(err => {
            this._posts = [];
            this._postsSubject.next(this._posts);
            throw err;
        })
        .finally(() => {
            this._postsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Post. Call after mutations to force refresh.
     */
    public ClearPostsCache(): void {
        this._posts = null;
        this._postsPromise = null;
        this._postsSubject.next(this._posts);      // Emit to observable
    }

    public get HasPosts(): Promise<boolean> {
        return this.Posts.then(posts => posts.length > 0);
    }




    /**
     * Updates the state of this PostCategoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PostCategoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PostCategorySubmitData {
        return PostCategoryService.Instance.ConvertToPostCategorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PostCategoryService extends SecureEndpointBase {

    private static _instance: PostCategoryService;
    private listCache: Map<string, Observable<Array<PostCategoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PostCategoryBasicListData>>>;
    private recordCache: Map<string, Observable<PostCategoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private postService: PostService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PostCategoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PostCategoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<PostCategoryData>>();

        PostCategoryService._instance = this;
    }

    public static get Instance(): PostCategoryService {
      return PostCategoryService._instance;
    }


    public ClearListCaches(config: PostCategoryQueryParameters | null = null) {

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


    public ConvertToPostCategorySubmitData(data: PostCategoryData): PostCategorySubmitData {

        let output = new PostCategorySubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.slug = data.slug;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPostCategory(id: bigint | number, includeRelations: boolean = true) : Observable<PostCategoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const postCategory$ = this.requestPostCategory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PostCategory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, postCategory$);

            return postCategory$;
        }

        return this.recordCache.get(configHash) as Observable<PostCategoryData>;
    }

    private requestPostCategory(id: bigint | number, includeRelations: boolean = true) : Observable<PostCategoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PostCategoryData>(this.baseUrl + 'api/PostCategory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePostCategory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPostCategory(id, includeRelations));
            }));
    }

    public GetPostCategoryList(config: PostCategoryQueryParameters | any = null) : Observable<Array<PostCategoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const postCategoryList$ = this.requestPostCategoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PostCategory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, postCategoryList$);

            return postCategoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PostCategoryData>>;
    }


    private requestPostCategoryList(config: PostCategoryQueryParameters | any) : Observable <Array<PostCategoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PostCategoryData>>(this.baseUrl + 'api/PostCategories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePostCategoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPostCategoryList(config));
            }));
    }

    public GetPostCategoriesRowCount(config: PostCategoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const postCategoriesRowCount$ = this.requestPostCategoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PostCategories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, postCategoriesRowCount$);

            return postCategoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPostCategoriesRowCount(config: PostCategoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PostCategories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPostCategoriesRowCount(config));
            }));
    }

    public GetPostCategoriesBasicListData(config: PostCategoryQueryParameters | any = null) : Observable<Array<PostCategoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const postCategoriesBasicListData$ = this.requestPostCategoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PostCategories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, postCategoriesBasicListData$);

            return postCategoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PostCategoryBasicListData>>;
    }


    private requestPostCategoriesBasicListData(config: PostCategoryQueryParameters | any) : Observable<Array<PostCategoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PostCategoryBasicListData>>(this.baseUrl + 'api/PostCategories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPostCategoriesBasicListData(config));
            }));

    }


    public PutPostCategory(id: bigint | number, postCategory: PostCategorySubmitData) : Observable<PostCategoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PostCategoryData>(this.baseUrl + 'api/PostCategory/' + id.toString(), postCategory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePostCategory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPostCategory(id, postCategory));
            }));
    }


    public PostPostCategory(postCategory: PostCategorySubmitData) : Observable<PostCategoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PostCategoryData>(this.baseUrl + 'api/PostCategory', postCategory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePostCategory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPostCategory(postCategory));
            }));
    }

  
    public DeletePostCategory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PostCategory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePostCategory(id));
            }));
    }


    private getConfigHash(config: PostCategoryQueryParameters | any): string {

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

    public userIsCommunityPostCategoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityPostCategoryReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.PostCategories
        //
        if (userIsCommunityPostCategoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityPostCategoryReader = user.readPermission >= 1;
            } else {
                userIsCommunityPostCategoryReader = false;
            }
        }

        return userIsCommunityPostCategoryReader;
    }


    public userIsCommunityPostCategoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityPostCategoryWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.PostCategories
        //
        if (userIsCommunityPostCategoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityPostCategoryWriter = user.writePermission >= 10;
          } else {
            userIsCommunityPostCategoryWriter = false;
          }      
        }

        return userIsCommunityPostCategoryWriter;
    }

    public GetPostsForPostCategory(postCategoryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PostData[]> {
        return this.postService.GetPostList({
            postCategoryId: postCategoryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PostCategoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PostCategoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PostCategoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePostCategory(raw: any): PostCategoryData {
    if (!raw) return raw;

    //
    // Create a PostCategoryData object instance with correct prototype
    //
    const revived = Object.create(PostCategoryData.prototype) as PostCategoryData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._posts = null;
    (revived as any)._postsPromise = null;
    (revived as any)._postsSubject = new BehaviorSubject<PostData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadPostCategoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Posts$ = (revived as any)._postsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._posts === null && (revived as any)._postsPromise === null) {
                (revived as any).loadPosts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._postsCount$ = null;



    return revived;
  }

  private RevivePostCategoryList(rawList: any[]): PostCategoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePostCategory(raw));
  }

}
