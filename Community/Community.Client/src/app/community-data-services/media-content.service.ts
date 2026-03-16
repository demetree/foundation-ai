/*

   GENERATED SERVICE FOR THE MEDIACONTENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the MediaContent table.

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
import { MediaAssetData } from './media-asset.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class MediaContentQueryParameters {
    mediaAssetId: bigint | number | null | undefined = null;
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
export class MediaContentSubmitData {
    id!: bigint | number;
    mediaAssetId!: bigint | number;
    fileData!: string;
    active!: boolean;
    deleted!: boolean;
}


export class MediaContentBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. MediaContentChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `mediaContent.MediaContentChildren$` — use with `| async` in templates
//        • Promise:    `mediaContent.MediaContentChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="mediaContent.MediaContentChildren$ | async"`), or
//        • Access the promise getter (`mediaContent.MediaContentChildren` or `await mediaContent.MediaContentChildren`)
//    - Simply reading `mediaContent.MediaContentChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await mediaContent.Reload()` to refresh the entire object and clear all lazy caches.
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
export class MediaContentData {
    id!: bigint | number;
    mediaAssetId!: bigint | number;
    fileData!: string;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    mediaAsset: MediaAssetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any MediaContentData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.mediaContent.Reload();
  //
  //  Non Async:
  //
  //     mediaContent[0].Reload().then(x => {
  //        this.mediaContent = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      MediaContentService.Instance.GetMediaContent(this.id, includeRelations)
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
     * Updates the state of this MediaContentData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this MediaContentData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): MediaContentSubmitData {
        return MediaContentService.Instance.ConvertToMediaContentSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class MediaContentService extends SecureEndpointBase {

    private static _instance: MediaContentService;
    private listCache: Map<string, Observable<Array<MediaContentData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<MediaContentBasicListData>>>;
    private recordCache: Map<string, Observable<MediaContentData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<MediaContentData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<MediaContentBasicListData>>>();
        this.recordCache = new Map<string, Observable<MediaContentData>>();

        MediaContentService._instance = this;
    }

    public static get Instance(): MediaContentService {
      return MediaContentService._instance;
    }


    public ClearListCaches(config: MediaContentQueryParameters | null = null) {

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


    public ConvertToMediaContentSubmitData(data: MediaContentData): MediaContentSubmitData {

        let output = new MediaContentSubmitData();

        output.id = data.id;
        output.mediaAssetId = data.mediaAssetId;
        output.fileData = data.fileData;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetMediaContent(id: bigint | number, includeRelations: boolean = true) : Observable<MediaContentData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const mediaContent$ = this.requestMediaContent(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MediaContent", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, mediaContent$);

            return mediaContent$;
        }

        return this.recordCache.get(configHash) as Observable<MediaContentData>;
    }

    private requestMediaContent(id: bigint | number, includeRelations: boolean = true) : Observable<MediaContentData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<MediaContentData>(this.baseUrl + 'api/MediaContent/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveMediaContent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestMediaContent(id, includeRelations));
            }));
    }

    public GetMediaContentList(config: MediaContentQueryParameters | any = null) : Observable<Array<MediaContentData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const mediaContentList$ = this.requestMediaContentList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MediaContent list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, mediaContentList$);

            return mediaContentList$;
        }

        return this.listCache.get(configHash) as Observable<Array<MediaContentData>>;
    }


    private requestMediaContentList(config: MediaContentQueryParameters | any) : Observable <Array<MediaContentData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MediaContentData>>(this.baseUrl + 'api/MediaContents', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveMediaContentList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestMediaContentList(config));
            }));
    }

    public GetMediaContentsRowCount(config: MediaContentQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const mediaContentsRowCount$ = this.requestMediaContentsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MediaContents row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, mediaContentsRowCount$);

            return mediaContentsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestMediaContentsRowCount(config: MediaContentQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/MediaContents/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMediaContentsRowCount(config));
            }));
    }

    public GetMediaContentsBasicListData(config: MediaContentQueryParameters | any = null) : Observable<Array<MediaContentBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const mediaContentsBasicListData$ = this.requestMediaContentsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MediaContents basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, mediaContentsBasicListData$);

            return mediaContentsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<MediaContentBasicListData>>;
    }


    private requestMediaContentsBasicListData(config: MediaContentQueryParameters | any) : Observable<Array<MediaContentBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MediaContentBasicListData>>(this.baseUrl + 'api/MediaContents/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMediaContentsBasicListData(config));
            }));

    }


    public PutMediaContent(id: bigint | number, mediaContent: MediaContentSubmitData) : Observable<MediaContentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<MediaContentData>(this.baseUrl + 'api/MediaContent/' + id.toString(), mediaContent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMediaContent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutMediaContent(id, mediaContent));
            }));
    }


    public PostMediaContent(mediaContent: MediaContentSubmitData) : Observable<MediaContentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<MediaContentData>(this.baseUrl + 'api/MediaContent', mediaContent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMediaContent(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostMediaContent(mediaContent));
            }));
    }

  
    public DeleteMediaContent(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/MediaContent/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteMediaContent(id));
            }));
    }


    private getConfigHash(config: MediaContentQueryParameters | any): string {

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

    public userIsCommunityMediaContentReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityMediaContentReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.MediaContents
        //
        if (userIsCommunityMediaContentReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityMediaContentReader = user.readPermission >= 1;
            } else {
                userIsCommunityMediaContentReader = false;
            }
        }

        return userIsCommunityMediaContentReader;
    }


    public userIsCommunityMediaContentWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityMediaContentWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.MediaContents
        //
        if (userIsCommunityMediaContentWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityMediaContentWriter = user.writePermission >= 10;
          } else {
            userIsCommunityMediaContentWriter = false;
          }      
        }

        return userIsCommunityMediaContentWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full MediaContentData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the MediaContentData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when MediaContentTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveMediaContent(raw: any): MediaContentData {
    if (!raw) return raw;

    //
    // Create a MediaContentData object instance with correct prototype
    //
    const revived = Object.create(MediaContentData.prototype) as MediaContentData;

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
    // 2. But private methods (loadMediaContentXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveMediaContentList(rawList: any[]): MediaContentData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveMediaContent(raw));
  }

}
