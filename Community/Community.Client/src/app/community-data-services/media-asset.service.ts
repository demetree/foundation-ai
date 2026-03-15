/*

   GENERATED SERVICE FOR THE MEDIAASSET TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the MediaAsset table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class MediaAssetQueryParameters {
    Id: bigint | number | null | undefined = null;
    FileName: string | null | undefined = null;
    FilePath: string | null | undefined = null;
    MimeType: string | null | undefined = null;
    AltText: string | null | undefined = null;
    Caption: string | null | undefined = null;
    FileSizeBytes: bigint | number | null | undefined = null;
    ImageWidth: bigint | number | null | undefined = null;
    ImageHeight: bigint | number | null | undefined = null;
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
export class MediaAssetSubmitData {
    Id: bigint | number | null = null;
    FileName: string | null = null;
    FilePath: string | null = null;
    MimeType: string | null = null;
    AltText: string | null = null;
    Caption: string | null = null;
    FileSizeBytes: bigint | number | null = null;
    ImageWidth: bigint | number | null = null;
    ImageHeight: bigint | number | null = null;
    Active: boolean | null = null;
    Deleted: boolean | null = null;
}


export class MediaAssetBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. MediaAssetChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `mediaAsset.MediaAssetChildren$` — use with `| async` in templates
//        • Promise:    `mediaAsset.MediaAssetChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="mediaAsset.MediaAssetChildren$ | async"`), or
//        • Access the promise getter (`mediaAsset.MediaAssetChildren` or `await mediaAsset.MediaAssetChildren`)
//    - Simply reading `mediaAsset.MediaAssetChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await mediaAsset.Reload()` to refresh the entire object and clear all lazy caches.
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
export class MediaAssetData {
    Id!: bigint | number;
    FileName!: string | null;
    FilePath!: string | null;
    MimeType!: string | null;
    AltText!: string | null;
    Caption!: string | null;
    FileSizeBytes!: bigint | number;
    ImageWidth!: bigint | number;
    ImageHeight!: bigint | number;
    ObjectGuid!: string | null;
    Active!: boolean | null;
    Deleted!: boolean | null;

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
  // Promise based reload method to allow rebuilding of any MediaAssetData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.mediaAsset.Reload();
  //
  //  Non Async:
  //
  //     mediaAsset[0].Reload().then(x => {
  //        this.mediaAsset = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      MediaAssetService.Instance.GetMediaAsset(this.id, includeRelations)
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
     * Updates the state of this MediaAssetData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this MediaAssetData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): MediaAssetSubmitData {
        return MediaAssetService.Instance.ConvertToMediaAssetSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class MediaAssetService extends SecureEndpointBase {

    private static _instance: MediaAssetService;
    private listCache: Map<string, Observable<Array<MediaAssetData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<MediaAssetBasicListData>>>;
    private recordCache: Map<string, Observable<MediaAssetData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<MediaAssetData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<MediaAssetBasicListData>>>();
        this.recordCache = new Map<string, Observable<MediaAssetData>>();

        MediaAssetService._instance = this;
    }

    public static get Instance(): MediaAssetService {
      return MediaAssetService._instance;
    }


    public ClearListCaches(config: MediaAssetQueryParameters | null = null) {

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


    public ConvertToMediaAssetSubmitData(data: MediaAssetData): MediaAssetSubmitData {

        let output = new MediaAssetSubmitData();

        output.Id = data.Id;
        output.FileName = data.FileName;
        output.FilePath = data.FilePath;
        output.MimeType = data.MimeType;
        output.AltText = data.AltText;
        output.Caption = data.Caption;
        output.FileSizeBytes = data.FileSizeBytes;
        output.ImageWidth = data.ImageWidth;
        output.ImageHeight = data.ImageHeight;
        output.Active = data.Active;
        output.Deleted = data.Deleted;

        return output;
    }

    public GetMediaAsset(id: bigint | number, includeRelations: boolean = true) : Observable<MediaAssetData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const mediaAsset$ = this.requestMediaAsset(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MediaAsset", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, mediaAsset$);

            return mediaAsset$;
        }

        return this.recordCache.get(configHash) as Observable<MediaAssetData>;
    }

    private requestMediaAsset(id: bigint | number, includeRelations: boolean = true) : Observable<MediaAssetData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<MediaAssetData>(this.baseUrl + 'api/MediaAsset/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveMediaAsset(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestMediaAsset(id, includeRelations));
            }));
    }

    public GetMediaAssetList(config: MediaAssetQueryParameters | any = null) : Observable<Array<MediaAssetData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const mediaAssetList$ = this.requestMediaAssetList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MediaAsset list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, mediaAssetList$);

            return mediaAssetList$;
        }

        return this.listCache.get(configHash) as Observable<Array<MediaAssetData>>;
    }


    private requestMediaAssetList(config: MediaAssetQueryParameters | any) : Observable <Array<MediaAssetData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MediaAssetData>>(this.baseUrl + 'api/MediaAssets', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveMediaAssetList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestMediaAssetList(config));
            }));
    }

    public GetMediaAssetsRowCount(config: MediaAssetQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const mediaAssetsRowCount$ = this.requestMediaAssetsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MediaAssets row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, mediaAssetsRowCount$);

            return mediaAssetsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestMediaAssetsRowCount(config: MediaAssetQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/MediaAssets/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMediaAssetsRowCount(config));
            }));
    }

    public GetMediaAssetsBasicListData(config: MediaAssetQueryParameters | any = null) : Observable<Array<MediaAssetBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const mediaAssetsBasicListData$ = this.requestMediaAssetsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MediaAssets basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, mediaAssetsBasicListData$);

            return mediaAssetsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<MediaAssetBasicListData>>;
    }


    private requestMediaAssetsBasicListData(config: MediaAssetQueryParameters | any) : Observable<Array<MediaAssetBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MediaAssetBasicListData>>(this.baseUrl + 'api/MediaAssets/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMediaAssetsBasicListData(config));
            }));

    }


    public PutMediaAsset(id: bigint | number, mediaAsset: MediaAssetSubmitData) : Observable<MediaAssetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<MediaAssetData>(this.baseUrl + 'api/MediaAsset/' + id.toString(), mediaAsset, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMediaAsset(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutMediaAsset(id, mediaAsset));
            }));
    }


    public PostMediaAsset(mediaAsset: MediaAssetSubmitData) : Observable<MediaAssetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<MediaAssetData>(this.baseUrl + 'api/MediaAsset', mediaAsset, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMediaAsset(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostMediaAsset(mediaAsset));
            }));
    }

  
    public DeleteMediaAsset(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/MediaAsset/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteMediaAsset(id));
            }));
    }


    private getConfigHash(config: MediaAssetQueryParameters | any): string {

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

    public userIsCommunityMediaAssetReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityMediaAssetReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.MediaAssets
        //
        if (userIsCommunityMediaAssetReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityMediaAssetReader = user.readPermission >= 1;
            } else {
                userIsCommunityMediaAssetReader = false;
            }
        }

        return userIsCommunityMediaAssetReader;
    }


    public userIsCommunityMediaAssetWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityMediaAssetWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.MediaAssets
        //
        if (userIsCommunityMediaAssetWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityMediaAssetWriter = user.writePermission >= 10;
          } else {
            userIsCommunityMediaAssetWriter = false;
          }      
        }

        return userIsCommunityMediaAssetWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full MediaAssetData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the MediaAssetData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when MediaAssetTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveMediaAsset(raw: any): MediaAssetData {
    if (!raw) return raw;

    //
    // Create a MediaAssetData object instance with correct prototype
    //
    const revived = Object.create(MediaAssetData.prototype) as MediaAssetData;

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
    // 2. But private methods (loadMediaAssetXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveMediaAssetList(rawList: any[]): MediaAssetData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveMediaAsset(raw));
  }

}
