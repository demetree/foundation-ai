/*

   GENERATED SERVICE FOR THE TAG TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Tag table.

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
import { PriorityData } from './priority.service';
import { IconData } from './icon.service';
import { ContactTagService, ContactTagData } from './contact-tag.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class TagQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    isSystem: boolean | null | undefined = null;
    priorityId: bigint | number | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class TagSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    isSystem: boolean | null = null;
    priorityId: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class TagBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TagChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `tag.TagChildren$` — use with `| async` in templates
//        • Promise:    `tag.TagChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="tag.TagChildren$ | async"`), or
//        • Access the promise getter (`tag.TagChildren` or `await tag.TagChildren`)
//    - Simply reading `tag.TagChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await tag.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TagData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence!: bigint | number;
    isSystem!: boolean | null;
    priorityId!: bigint | number;
    iconId!: bigint | number;
    color!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    priority: PriorityData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _contactTags: ContactTagData[] | null = null;
    private _contactTagsPromise: Promise<ContactTagData[]> | null  = null;
    private _contactTagsSubject = new BehaviorSubject<ContactTagData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ContactTags$ = this._contactTagsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactTags === null && this._contactTagsPromise === null) {
            this.loadContactTags(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ContactTagsCount$ = TagService.Instance.GetTagsRowCount({tagId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any TagData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.tag.Reload();
  //
  //  Non Async:
  //
  //     tag[0].Reload().then(x => {
  //        this.tag = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TagService.Instance.GetTag(this.id, includeRelations)
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
     this._contactTags = null;
     this._contactTagsPromise = null;
     this._contactTagsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ContactTags for this Tag.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.tag.ContactTags.then(contactTags => { ... })
     *   or
     *   await this.tag.ContactTags
     *
    */
    public get ContactTags(): Promise<ContactTagData[]> {
        if (this._contactTags !== null) {
            return Promise.resolve(this._contactTags);
        }

        if (this._contactTagsPromise !== null) {
            return this._contactTagsPromise;
        }

        // Start the load
        this.loadContactTags();

        return this._contactTagsPromise!;
    }



    private loadContactTags(): void {

        this._contactTagsPromise = lastValueFrom(
            TagService.Instance.GetContactTagsForTag(this.id)
        )
        .then(contactTags => {
            this._contactTags = contactTags ?? [];
            this._contactTagsSubject.next(this._contactTags);
            return this._contactTags;
         })
        .catch(err => {
            this._contactTags = [];
            this._contactTagsSubject.next(this._contactTags);
            throw err;
        })
        .finally(() => {
            this._contactTagsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContactTag. Call after mutations to force refresh.
     */
    public ClearContactTagsCache(): void {
        this._contactTags = null;
        this._contactTagsPromise = null;
        this._contactTagsSubject.next(this._contactTags);      // Emit to observable
    }

    public get HasContactTags(): Promise<boolean> {
        return this.ContactTags.then(contactTags => contactTags.length > 0);
    }




    /**
     * Updates the state of this TagData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TagData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TagSubmitData {
        return TagService.Instance.ConvertToTagSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TagService extends SecureEndpointBase {

    private static _instance: TagService;
    private listCache: Map<string, Observable<Array<TagData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TagBasicListData>>>;
    private recordCache: Map<string, Observable<TagData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private contactTagService: ContactTagService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TagData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TagBasicListData>>>();
        this.recordCache = new Map<string, Observable<TagData>>();

        TagService._instance = this;
    }

    public static get Instance(): TagService {
      return TagService._instance;
    }


    public ClearListCaches(config: TagQueryParameters | null = null) {

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


    public ConvertToTagSubmitData(data: TagData): TagSubmitData {

        let output = new TagSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.isSystem = data.isSystem;
        output.priorityId = data.priorityId;
        output.iconId = data.iconId;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetTag(id: bigint | number, includeRelations: boolean = true) : Observable<TagData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const tag$ = this.requestTag(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Tag", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, tag$);

            return tag$;
        }

        return this.recordCache.get(configHash) as Observable<TagData>;
    }

    private requestTag(id: bigint | number, includeRelations: boolean = true) : Observable<TagData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TagData>(this.baseUrl + 'api/Tag/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTag(id, includeRelations));
            }));
    }

    public GetTagList(config: TagQueryParameters | any = null) : Observable<Array<TagData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const tagList$ = this.requestTagList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Tag list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, tagList$);

            return tagList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TagData>>;
    }


    private requestTagList(config: TagQueryParameters | any) : Observable <Array<TagData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TagData>>(this.baseUrl + 'api/Tags', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTagList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTagList(config));
            }));
    }

    public GetTagsRowCount(config: TagQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const tagsRowCount$ = this.requestTagsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Tags row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, tagsRowCount$);

            return tagsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTagsRowCount(config: TagQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Tags/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTagsRowCount(config));
            }));
    }

    public GetTagsBasicListData(config: TagQueryParameters | any = null) : Observable<Array<TagBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const tagsBasicListData$ = this.requestTagsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Tags basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, tagsBasicListData$);

            return tagsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TagBasicListData>>;
    }


    private requestTagsBasicListData(config: TagQueryParameters | any) : Observable<Array<TagBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TagBasicListData>>(this.baseUrl + 'api/Tags/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTagsBasicListData(config));
            }));

    }


    public PutTag(id: bigint | number, tag: TagSubmitData) : Observable<TagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TagData>(this.baseUrl + 'api/Tag/' + id.toString(), tag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTag(id, tag));
            }));
    }


    public PostTag(tag: TagSubmitData) : Observable<TagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TagData>(this.baseUrl + 'api/Tag', tag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTag(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTag(tag));
            }));
    }

  
    public DeleteTag(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Tag/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTag(id));
            }));
    }


    private getConfigHash(config: TagQueryParameters | any): string {

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

    public userIsSchedulerTagReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerTagReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Tags
        //
        if (userIsSchedulerTagReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerTagReader = user.readPermission >= 1;
            } else {
                userIsSchedulerTagReader = false;
            }
        }

        return userIsSchedulerTagReader;
    }


    public userIsSchedulerTagWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerTagWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Tags
        //
        if (userIsSchedulerTagWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerTagWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerTagWriter = false;
          }      
        }

        return userIsSchedulerTagWriter;
    }

    public GetContactTagsForTag(tagId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactTagData[]> {
        return this.contactTagService.GetContactTagList({
            tagId: tagId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full TagData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TagData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TagTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTag(raw: any): TagData {
    if (!raw) return raw;

    //
    // Create a TagData object instance with correct prototype
    //
    const revived = Object.create(TagData.prototype) as TagData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._contactTags = null;
    (revived as any)._contactTagsPromise = null;
    (revived as any)._contactTagsSubject = new BehaviorSubject<ContactTagData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadTagXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ContactTags$ = (revived as any)._contactTagsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactTags === null && (revived as any)._contactTagsPromise === null) {
                (revived as any).loadContactTags();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ContactTagsCount$ = ContactTagService.Instance.GetContactTagsRowCount({tagId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveTagList(rawList: any[]): TagData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTag(raw));
  }

}
