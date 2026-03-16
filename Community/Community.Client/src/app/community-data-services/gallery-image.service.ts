/*

   GENERATED SERVICE FOR THE GALLERYIMAGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the GalleryImage table.

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
import { GalleryAlbumData } from './gallery-album.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class GalleryImageQueryParameters {
    galleryAlbumId: bigint | number | null | undefined = null;
    imageUrl: string | null | undefined = null;
    caption: string | null | undefined = null;
    altText: string | null | undefined = null;
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
export class GalleryImageSubmitData {
    id!: bigint | number;
    galleryAlbumId!: bigint | number;
    imageUrl!: string;
    caption: string | null = null;
    altText: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class GalleryImageBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. GalleryImageChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `galleryImage.GalleryImageChildren$` — use with `| async` in templates
//        • Promise:    `galleryImage.GalleryImageChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="galleryImage.GalleryImageChildren$ | async"`), or
//        • Access the promise getter (`galleryImage.GalleryImageChildren` or `await galleryImage.GalleryImageChildren`)
//    - Simply reading `galleryImage.GalleryImageChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await galleryImage.Reload()` to refresh the entire object and clear all lazy caches.
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
export class GalleryImageData {
    id!: bigint | number;
    galleryAlbumId!: bigint | number;
    imageUrl!: string;
    caption!: string | null;
    altText!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    galleryAlbum: GalleryAlbumData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any GalleryImageData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.galleryImage.Reload();
  //
  //  Non Async:
  //
  //     galleryImage[0].Reload().then(x => {
  //        this.galleryImage = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      GalleryImageService.Instance.GetGalleryImage(this.id, includeRelations)
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
     * Updates the state of this GalleryImageData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this GalleryImageData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): GalleryImageSubmitData {
        return GalleryImageService.Instance.ConvertToGalleryImageSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class GalleryImageService extends SecureEndpointBase {

    private static _instance: GalleryImageService;
    private listCache: Map<string, Observable<Array<GalleryImageData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<GalleryImageBasicListData>>>;
    private recordCache: Map<string, Observable<GalleryImageData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<GalleryImageData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<GalleryImageBasicListData>>>();
        this.recordCache = new Map<string, Observable<GalleryImageData>>();

        GalleryImageService._instance = this;
    }

    public static get Instance(): GalleryImageService {
      return GalleryImageService._instance;
    }


    public ClearListCaches(config: GalleryImageQueryParameters | null = null) {

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


    public ConvertToGalleryImageSubmitData(data: GalleryImageData): GalleryImageSubmitData {

        let output = new GalleryImageSubmitData();

        output.id = data.id;
        output.galleryAlbumId = data.galleryAlbumId;
        output.imageUrl = data.imageUrl;
        output.caption = data.caption;
        output.altText = data.altText;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetGalleryImage(id: bigint | number, includeRelations: boolean = true) : Observable<GalleryImageData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const galleryImage$ = this.requestGalleryImage(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get GalleryImage", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, galleryImage$);

            return galleryImage$;
        }

        return this.recordCache.get(configHash) as Observable<GalleryImageData>;
    }

    private requestGalleryImage(id: bigint | number, includeRelations: boolean = true) : Observable<GalleryImageData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<GalleryImageData>(this.baseUrl + 'api/GalleryImage/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveGalleryImage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestGalleryImage(id, includeRelations));
            }));
    }

    public GetGalleryImageList(config: GalleryImageQueryParameters | any = null) : Observable<Array<GalleryImageData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const galleryImageList$ = this.requestGalleryImageList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get GalleryImage list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, galleryImageList$);

            return galleryImageList$;
        }

        return this.listCache.get(configHash) as Observable<Array<GalleryImageData>>;
    }


    private requestGalleryImageList(config: GalleryImageQueryParameters | any) : Observable <Array<GalleryImageData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<GalleryImageData>>(this.baseUrl + 'api/GalleryImages', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveGalleryImageList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestGalleryImageList(config));
            }));
    }

    public GetGalleryImagesRowCount(config: GalleryImageQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const galleryImagesRowCount$ = this.requestGalleryImagesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get GalleryImages row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, galleryImagesRowCount$);

            return galleryImagesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestGalleryImagesRowCount(config: GalleryImageQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/GalleryImages/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestGalleryImagesRowCount(config));
            }));
    }

    public GetGalleryImagesBasicListData(config: GalleryImageQueryParameters | any = null) : Observable<Array<GalleryImageBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const galleryImagesBasicListData$ = this.requestGalleryImagesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get GalleryImages basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, galleryImagesBasicListData$);

            return galleryImagesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<GalleryImageBasicListData>>;
    }


    private requestGalleryImagesBasicListData(config: GalleryImageQueryParameters | any) : Observable<Array<GalleryImageBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<GalleryImageBasicListData>>(this.baseUrl + 'api/GalleryImages/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestGalleryImagesBasicListData(config));
            }));

    }


    public PutGalleryImage(id: bigint | number, galleryImage: GalleryImageSubmitData) : Observable<GalleryImageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<GalleryImageData>(this.baseUrl + 'api/GalleryImage/' + id.toString(), galleryImage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveGalleryImage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutGalleryImage(id, galleryImage));
            }));
    }


    public PostGalleryImage(galleryImage: GalleryImageSubmitData) : Observable<GalleryImageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<GalleryImageData>(this.baseUrl + 'api/GalleryImage', galleryImage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveGalleryImage(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostGalleryImage(galleryImage));
            }));
    }

  
    public DeleteGalleryImage(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/GalleryImage/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteGalleryImage(id));
            }));
    }


    private getConfigHash(config: GalleryImageQueryParameters | any): string {

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

    public userIsCommunityGalleryImageReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityGalleryImageReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.GalleryImages
        //
        if (userIsCommunityGalleryImageReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityGalleryImageReader = user.readPermission >= 1;
            } else {
                userIsCommunityGalleryImageReader = false;
            }
        }

        return userIsCommunityGalleryImageReader;
    }


    public userIsCommunityGalleryImageWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityGalleryImageWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.GalleryImages
        //
        if (userIsCommunityGalleryImageWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityGalleryImageWriter = user.writePermission >= 10;
          } else {
            userIsCommunityGalleryImageWriter = false;
          }      
        }

        return userIsCommunityGalleryImageWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full GalleryImageData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the GalleryImageData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when GalleryImageTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveGalleryImage(raw: any): GalleryImageData {
    if (!raw) return raw;

    //
    // Create a GalleryImageData object instance with correct prototype
    //
    const revived = Object.create(GalleryImageData.prototype) as GalleryImageData;

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
    // 2. But private methods (loadGalleryImageXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveGalleryImageList(rawList: any[]): GalleryImageData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveGalleryImage(raw));
  }

}
