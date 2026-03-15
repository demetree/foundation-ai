/*

   GENERATED SERVICE FOR THE POSTTAG TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PostTag table.

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
export class PostTagQueryParameters {
    Id: bigint | number | null | undefined = null;
    Name: string | null | undefined = null;
    Slug: string | null | undefined = null;
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
export class PostTagSubmitData {
    Id: bigint | number | null = null;
    Name: string | null = null;
    Slug: string | null = null;
    Active: boolean | null = null;
    Deleted: boolean | null = null;
}


export class PostTagBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PostTagChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `postTag.PostTagChildren$` — use with `| async` in templates
//        • Promise:    `postTag.PostTagChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="postTag.PostTagChildren$ | async"`), or
//        • Access the promise getter (`postTag.PostTagChildren` or `await postTag.PostTagChildren`)
//    - Simply reading `postTag.PostTagChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await postTag.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PostTagData {
    Id!: bigint | number;
    Name!: string | null;
    Slug!: string | null;
    ObjectGuid!: string | null;
    Active!: boolean | null;
    Deleted!: boolean | null;

    //
    // Private lazy-loading caches for related collections
    //
    private _postTagAssignments: PostTagAssignmentData[] | null = null;
    private _postTagAssignmentsPromise: Promise<PostTagAssignmentData[]> | null  = null;
    private _postTagAssignmentsSubject = new BehaviorSubject<PostTagAssignmentData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
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
            this._postTagAssignmentsCount$ = PostTagAssignmentService.Instance.GetPostTagAssignmentsRowCount({postTagId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._postTagAssignmentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PostTagData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.postTag.Reload();
  //
  //  Non Async:
  //
  //     postTag[0].Reload().then(x => {
  //        this.postTag = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PostTagService.Instance.GetPostTag(this.id, includeRelations)
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
     this._postTagAssignments = null;
     this._postTagAssignmentsPromise = null;
     this._postTagAssignmentsSubject.next(null);
     this._postTagAssignmentsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the PostTagAssignments for this PostTag.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.postTag.PostTagAssignments.then(postTags => { ... })
     *   or
     *   await this.postTag.postTags
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
            PostTagService.Instance.GetPostTagAssignmentsForPostTag(this.id)
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




    /**
     * Updates the state of this PostTagData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PostTagData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PostTagSubmitData {
        return PostTagService.Instance.ConvertToPostTagSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PostTagService extends SecureEndpointBase {

    private static _instance: PostTagService;
    private listCache: Map<string, Observable<Array<PostTagData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PostTagBasicListData>>>;
    private recordCache: Map<string, Observable<PostTagData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private postTagAssignmentService: PostTagAssignmentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PostTagData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PostTagBasicListData>>>();
        this.recordCache = new Map<string, Observable<PostTagData>>();

        PostTagService._instance = this;
    }

    public static get Instance(): PostTagService {
      return PostTagService._instance;
    }


    public ClearListCaches(config: PostTagQueryParameters | null = null) {

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


    public ConvertToPostTagSubmitData(data: PostTagData): PostTagSubmitData {

        let output = new PostTagSubmitData();

        output.Id = data.Id;
        output.Name = data.Name;
        output.Slug = data.Slug;
        output.Active = data.Active;
        output.Deleted = data.Deleted;

        return output;
    }

    public GetPostTag(id: bigint | number, includeRelations: boolean = true) : Observable<PostTagData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const postTag$ = this.requestPostTag(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PostTag", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, postTag$);

            return postTag$;
        }

        return this.recordCache.get(configHash) as Observable<PostTagData>;
    }

    private requestPostTag(id: bigint | number, includeRelations: boolean = true) : Observable<PostTagData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PostTagData>(this.baseUrl + 'api/PostTag/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePostTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPostTag(id, includeRelations));
            }));
    }

    public GetPostTagList(config: PostTagQueryParameters | any = null) : Observable<Array<PostTagData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const postTagList$ = this.requestPostTagList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PostTag list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, postTagList$);

            return postTagList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PostTagData>>;
    }


    private requestPostTagList(config: PostTagQueryParameters | any) : Observable <Array<PostTagData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PostTagData>>(this.baseUrl + 'api/PostTags', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePostTagList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPostTagList(config));
            }));
    }

    public GetPostTagsRowCount(config: PostTagQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const postTagsRowCount$ = this.requestPostTagsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PostTags row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, postTagsRowCount$);

            return postTagsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPostTagsRowCount(config: PostTagQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PostTags/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPostTagsRowCount(config));
            }));
    }

    public GetPostTagsBasicListData(config: PostTagQueryParameters | any = null) : Observable<Array<PostTagBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const postTagsBasicListData$ = this.requestPostTagsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PostTags basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, postTagsBasicListData$);

            return postTagsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PostTagBasicListData>>;
    }


    private requestPostTagsBasicListData(config: PostTagQueryParameters | any) : Observable<Array<PostTagBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PostTagBasicListData>>(this.baseUrl + 'api/PostTags/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPostTagsBasicListData(config));
            }));

    }


    public PutPostTag(id: bigint | number, postTag: PostTagSubmitData) : Observable<PostTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PostTagData>(this.baseUrl + 'api/PostTag/' + id.toString(), postTag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePostTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPostTag(id, postTag));
            }));
    }


    public PostPostTag(postTag: PostTagSubmitData) : Observable<PostTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PostTagData>(this.baseUrl + 'api/PostTag', postTag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePostTag(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPostTag(postTag));
            }));
    }

  
    public DeletePostTag(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PostTag/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePostTag(id));
            }));
    }


    private getConfigHash(config: PostTagQueryParameters | any): string {

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

    public userIsCommunityPostTagReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityPostTagReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.PostTags
        //
        if (userIsCommunityPostTagReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityPostTagReader = user.readPermission >= 1;
            } else {
                userIsCommunityPostTagReader = false;
            }
        }

        return userIsCommunityPostTagReader;
    }


    public userIsCommunityPostTagWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityPostTagWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.PostTags
        //
        if (userIsCommunityPostTagWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityPostTagWriter = user.writePermission >= 10;
          } else {
            userIsCommunityPostTagWriter = false;
          }      
        }

        return userIsCommunityPostTagWriter;
    }

    public GetPostTagAssignmentsForPostTag(postTagId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PostTagAssignmentData[]> {
        return this.postTagAssignmentService.GetPostTagAssignmentList({
            postTagId: postTagId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PostTagData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PostTagData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PostTagTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePostTag(raw: any): PostTagData {
    if (!raw) return raw;

    //
    // Create a PostTagData object instance with correct prototype
    //
    const revived = Object.create(PostTagData.prototype) as PostTagData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
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
    // 2. But private methods (loadPostTagXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).PostTagAssignments$ = (revived as any)._postTagAssignmentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._postTagAssignments === null && (revived as any)._postTagAssignmentsPromise === null) {
                (revived as any).loadPostTagAssignments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._postTagAssignmentsCount$ = null;



    return revived;
  }

  private RevivePostTagList(rawList: any[]): PostTagData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePostTag(raw));
  }

}
