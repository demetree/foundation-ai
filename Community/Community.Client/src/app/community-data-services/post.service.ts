/*

   GENERATED SERVICE FOR THE POST TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Post table.

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
import { PostCategoryData } from './post-category.service';
import { PostChangeHistoryService, PostChangeHistoryData } from './post-change-history.service';
import { PostTagAssignmentService, PostTagAssignmentData } from './post-tag-assignment.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PostQueryParameters {
    Id: bigint | number | null | undefined = null;
    Title: string | null | undefined = null;
    Slug: string | null | undefined = null;
    Body: string | null | undefined = null;
    Excerpt: string | null | undefined = null;
    AuthorName: string | null | undefined = null;
    PostCategoryId: bigint | number | null | undefined = null;
    FeaturedImageUrl: string | null | undefined = null;
    MetaDescription: string | null | undefined = null;
    IsPublished: boolean | null | undefined = null;
    PublishedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    IsFeatured: boolean | null | undefined = null;
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
export class PostSubmitData {
    Id: bigint | number | null = null;
    Title: string | null = null;
    Slug: string | null = null;
    Body: string | null = null;
    Excerpt: string | null = null;
    AuthorName: string | null = null;
    PostCategoryId: bigint | number | null = null;
    FeaturedImageUrl: string | null = null;
    MetaDescription: string | null = null;
    IsPublished: boolean | null = null;
    PublishedDate: string | null = null;     // ISO 8601 (full datetime)
    IsFeatured: boolean | null = null;
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

export class PostBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PostChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `post.PostChildren$` — use with `| async` in templates
//        • Promise:    `post.PostChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="post.PostChildren$ | async"`), or
//        • Access the promise getter (`post.PostChildren` or `await post.PostChildren`)
//    - Simply reading `post.PostChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await post.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PostData {
    Id!: bigint | number;
    Title!: string | null;
    Slug!: string | null;
    Body!: string | null;
    Excerpt!: string | null;
    AuthorName!: string | null;
    PostCategoryId!: bigint | number;
    FeaturedImageUrl!: string | null;
    MetaDescription!: string | null;
    IsPublished!: boolean | null;
    PublishedDate!: string | null;   // ISO 8601 (full datetime)
    IsFeatured!: boolean | null;
    VersionNumber!: bigint | number;
    ObjectGuid!: string | null;
    Active!: boolean | null;
    Deleted!: boolean | null;
    PostCategory: PostCategoryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _postChangeHistories: PostChangeHistoryData[] | null = null;
    private _postChangeHistoriesPromise: Promise<PostChangeHistoryData[]> | null  = null;
    private _postChangeHistoriesSubject = new BehaviorSubject<PostChangeHistoryData[] | null>(null);

                
    private _postTagAssignments: PostTagAssignmentData[] | null = null;
    private _postTagAssignmentsPromise: Promise<PostTagAssignmentData[]> | null  = null;
    private _postTagAssignmentsSubject = new BehaviorSubject<PostTagAssignmentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<PostData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<PostData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PostData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public PostChangeHistories$ = this._postChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._postChangeHistories === null && this._postChangeHistoriesPromise === null) {
            this.loadPostChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _postChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get PostChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._postChangeHistoriesCount$ === null) {
            this._postChangeHistoriesCount$ = PostChangeHistoryService.Instance.GetPostChangeHistoriesRowCount({postId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._postChangeHistoriesCount$;
    }



    public PostTagAssignments$ = this._postTagAssignmentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._postTagAssignments === null && this._postTagAssignmentsPromise === null) {
            this.loadPostTagAssignments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _postTagAssignmentsCount$: Observable<bigint | number> | null = null;
    public get PostTagAssignmentsCount$(): Observable<bigint | number> {
        if (this._postTagAssignmentsCount$ === null) {
            this._postTagAssignmentsCount$ = PostTagAssignmentService.Instance.GetPostTagAssignmentsRowCount({postId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._postTagAssignmentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PostData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.post.Reload();
  //
  //  Non Async:
  //
  //     post[0].Reload().then(x => {
  //        this.post = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PostService.Instance.GetPost(this.id, includeRelations)
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
     this._postChangeHistories = null;
     this._postChangeHistoriesPromise = null;
     this._postChangeHistoriesSubject.next(null);
     this._postChangeHistoriesCount$ = null;

     this._postTagAssignments = null;
     this._postTagAssignmentsPromise = null;
     this._postTagAssignmentsSubject.next(null);
     this._postTagAssignmentsCount$ = null;

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
     * Gets the PostChangeHistories for this Post.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.post.PostChangeHistories.then(posts => { ... })
     *   or
     *   await this.post.posts
     *
    */
    public get PostChangeHistories(): Promise<PostChangeHistoryData[]> {
        if (this._postChangeHistories !== null) {
            return Promise.resolve(this._postChangeHistories);
        }

        if (this._postChangeHistoriesPromise !== null) {
            return this._postChangeHistoriesPromise;
        }

        // Start the load
        this.loadPostChangeHistories();

        return this._postChangeHistoriesPromise!;
    }



    private loadPostChangeHistories(): void {

        this._postChangeHistoriesPromise = lastValueFrom(
            PostService.Instance.GetPostChangeHistoriesForPost(this.id)
        )
        .then(PostChangeHistories => {
            this._postChangeHistories = PostChangeHistories ?? [];
            this._postChangeHistoriesSubject.next(this._postChangeHistories);
            return this._postChangeHistories;
         })
        .catch(err => {
            this._postChangeHistories = [];
            this._postChangeHistoriesSubject.next(this._postChangeHistories);
            throw err;
        })
        .finally(() => {
            this._postChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PostChangeHistory. Call after mutations to force refresh.
     */
    public ClearPostChangeHistoriesCache(): void {
        this._postChangeHistories = null;
        this._postChangeHistoriesPromise = null;
        this._postChangeHistoriesSubject.next(this._postChangeHistories);      // Emit to observable
    }

    public get HasPostChangeHistories(): Promise<boolean> {
        return this.PostChangeHistories.then(postChangeHistories => postChangeHistories.length > 0);
    }


    /**
     *
     * Gets the PostTagAssignments for this Post.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.post.PostTagAssignments.then(posts => { ... })
     *   or
     *   await this.post.posts
     *
    */
    public get PostTagAssignments(): Promise<PostTagAssignmentData[]> {
        if (this._postTagAssignments !== null) {
            return Promise.resolve(this._postTagAssignments);
        }

        if (this._postTagAssignmentsPromise !== null) {
            return this._postTagAssignmentsPromise;
        }

        // Start the load
        this.loadPostTagAssignments();

        return this._postTagAssignmentsPromise!;
    }



    private loadPostTagAssignments(): void {

        this._postTagAssignmentsPromise = lastValueFrom(
            PostService.Instance.GetPostTagAssignmentsForPost(this.id)
        )
        .then(PostTagAssignments => {
            this._postTagAssignments = PostTagAssignments ?? [];
            this._postTagAssignmentsSubject.next(this._postTagAssignments);
            return this._postTagAssignments;
         })
        .catch(err => {
            this._postTagAssignments = [];
            this._postTagAssignmentsSubject.next(this._postTagAssignments);
            throw err;
        })
        .finally(() => {
            this._postTagAssignmentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PostTagAssignment. Call after mutations to force refresh.
     */
    public ClearPostTagAssignmentsCache(): void {
        this._postTagAssignments = null;
        this._postTagAssignmentsPromise = null;
        this._postTagAssignmentsSubject.next(this._postTagAssignments);      // Emit to observable
    }

    public get HasPostTagAssignments(): Promise<boolean> {
        return this.PostTagAssignments.then(postTagAssignments => postTagAssignments.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (post.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await post.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<PostData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<PostData>> {
        const info = await lastValueFrom(
            PostService.Instance.GetPostChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this PostData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PostData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PostSubmitData {
        return PostService.Instance.ConvertToPostSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PostService extends SecureEndpointBase {

    private static _instance: PostService;
    private listCache: Map<string, Observable<Array<PostData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PostBasicListData>>>;
    private recordCache: Map<string, Observable<PostData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private postChangeHistoryService: PostChangeHistoryService,
        private postTagAssignmentService: PostTagAssignmentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PostData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PostBasicListData>>>();
        this.recordCache = new Map<string, Observable<PostData>>();

        PostService._instance = this;
    }

    public static get Instance(): PostService {
      return PostService._instance;
    }


    public ClearListCaches(config: PostQueryParameters | null = null) {

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


    public ConvertToPostSubmitData(data: PostData): PostSubmitData {

        let output = new PostSubmitData();

        output.Id = data.Id;
        output.Title = data.Title;
        output.Slug = data.Slug;
        output.Body = data.Body;
        output.Excerpt = data.Excerpt;
        output.AuthorName = data.AuthorName;
        output.PostCategoryId = data.PostCategoryId;
        output.FeaturedImageUrl = data.FeaturedImageUrl;
        output.MetaDescription = data.MetaDescription;
        output.IsPublished = data.IsPublished;
        output.PublishedDate = data.PublishedDate;
        output.IsFeatured = data.IsFeatured;
        output.VersionNumber = data.VersionNumber;
        output.Active = data.Active;
        output.Deleted = data.Deleted;

        return output;
    }

    public GetPost(id: bigint | number, includeRelations: boolean = true) : Observable<PostData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const post$ = this.requestPost(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Post", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, post$);

            return post$;
        }

        return this.recordCache.get(configHash) as Observable<PostData>;
    }

    private requestPost(id: bigint | number, includeRelations: boolean = true) : Observable<PostData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PostData>(this.baseUrl + 'api/Post/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePost(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPost(id, includeRelations));
            }));
    }

    public GetPostList(config: PostQueryParameters | any = null) : Observable<Array<PostData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const postList$ = this.requestPostList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Post list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, postList$);

            return postList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PostData>>;
    }


    private requestPostList(config: PostQueryParameters | any) : Observable <Array<PostData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PostData>>(this.baseUrl + 'api/Posts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePostList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPostList(config));
            }));
    }

    public GetPostsRowCount(config: PostQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const postsRowCount$ = this.requestPostsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Posts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, postsRowCount$);

            return postsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPostsRowCount(config: PostQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Posts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPostsRowCount(config));
            }));
    }

    public GetPostsBasicListData(config: PostQueryParameters | any = null) : Observable<Array<PostBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const postsBasicListData$ = this.requestPostsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Posts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, postsBasicListData$);

            return postsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PostBasicListData>>;
    }


    private requestPostsBasicListData(config: PostQueryParameters | any) : Observable<Array<PostBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PostBasicListData>>(this.baseUrl + 'api/Posts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPostsBasicListData(config));
            }));

    }


    public PutPost(id: bigint | number, post: PostSubmitData) : Observable<PostData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PostData>(this.baseUrl + 'api/Post/' + id.toString(), post, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePost(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPost(id, post));
            }));
    }


    public PostPost(post: PostSubmitData) : Observable<PostData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PostData>(this.baseUrl + 'api/Post', post, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePost(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPost(post));
            }));
    }

  
    public DeletePost(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Post/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePost(id));
            }));
    }

    public RollbackPost(id: bigint | number, versionNumber: bigint | number) : Observable<PostData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PostData>(this.baseUrl + 'api/Post/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePost(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackPost(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Post.
     */
    public GetPostChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<PostData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PostData>>(this.baseUrl + 'api/Post/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPostChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Post.
     */
    public GetPostAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<PostData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PostData>[]>(this.baseUrl + 'api/Post/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPostAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Post.
     */
    public GetPostVersion(id: bigint | number, version: number): Observable<PostData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PostData>(this.baseUrl + 'api/Post/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePost(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPostVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Post at a specific point in time.
     */
    public GetPostStateAtTime(id: bigint | number, time: string): Observable<PostData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PostData>(this.baseUrl + 'api/Post/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePost(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPostStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: PostQueryParameters | any): string {

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

    public userIsCommunityPostReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityPostReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.Posts
        //
        if (userIsCommunityPostReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityPostReader = user.readPermission >= 1;
            } else {
                userIsCommunityPostReader = false;
            }
        }

        return userIsCommunityPostReader;
    }


    public userIsCommunityPostWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityPostWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.Posts
        //
        if (userIsCommunityPostWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityPostWriter = user.writePermission >= 10;
          } else {
            userIsCommunityPostWriter = false;
          }      
        }

        return userIsCommunityPostWriter;
    }

    public GetPostChangeHistoriesForPost(postId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PostChangeHistoryData[]> {
        return this.postChangeHistoryService.GetPostChangeHistoryList({
            postId: postId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPostTagAssignmentsForPost(postId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PostTagAssignmentData[]> {
        return this.postTagAssignmentService.GetPostTagAssignmentList({
            postId: postId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PostData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PostData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PostTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePost(raw: any): PostData {
    if (!raw) return raw;

    //
    // Create a PostData object instance with correct prototype
    //
    const revived = Object.create(PostData.prototype) as PostData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._postChangeHistories = null;
    (revived as any)._postChangeHistoriesPromise = null;
    (revived as any)._postChangeHistoriesSubject = new BehaviorSubject<PostChangeHistoryData[] | null>(null);

    (revived as any)._postTagAssignments = null;
    (revived as any)._postTagAssignmentsPromise = null;
    (revived as any)._postTagAssignmentsSubject = new BehaviorSubject<PostTagAssignmentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadPostXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).PostChangeHistories$ = (revived as any)._postChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._postChangeHistories === null && (revived as any)._postChangeHistoriesPromise === null) {
                (revived as any).loadPostChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._postChangeHistoriesCount$ = null;


    (revived as any).PostTagAssignments$ = (revived as any)._postTagAssignmentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._postTagAssignments === null && (revived as any)._postTagAssignmentsPromise === null) {
                (revived as any).loadPostTagAssignments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._postTagAssignmentsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PostData> | null>(null);

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

  private RevivePostList(rawList: any[]): PostData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePost(raw));
  }

}
