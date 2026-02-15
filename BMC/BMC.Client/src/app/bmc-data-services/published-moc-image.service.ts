/*

   GENERATED SERVICE FOR THE PUBLISHEDMOCIMAGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PublishedMocImage table.

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
import { PublishedMocData } from './published-moc.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PublishedMocImageQueryParameters {
    publishedMocId: bigint | number | null | undefined = null;
    imagePath: string | null | undefined = null;
    caption: string | null | undefined = null;
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
export class PublishedMocImageSubmitData {
    id!: bigint | number;
    publishedMocId!: bigint | number;
    imagePath!: string;
    caption: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class PublishedMocImageBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PublishedMocImageChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `publishedMocImage.PublishedMocImageChildren$` — use with `| async` in templates
//        • Promise:    `publishedMocImage.PublishedMocImageChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="publishedMocImage.PublishedMocImageChildren$ | async"`), or
//        • Access the promise getter (`publishedMocImage.PublishedMocImageChildren` or `await publishedMocImage.PublishedMocImageChildren`)
//    - Simply reading `publishedMocImage.PublishedMocImageChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await publishedMocImage.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PublishedMocImageData {
    id!: bigint | number;
    publishedMocId!: bigint | number;
    imagePath!: string;
    caption!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    publishedMoc: PublishedMocData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any PublishedMocImageData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.publishedMocImage.Reload();
  //
  //  Non Async:
  //
  //     publishedMocImage[0].Reload().then(x => {
  //        this.publishedMocImage = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PublishedMocImageService.Instance.GetPublishedMocImage(this.id, includeRelations)
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
     * Updates the state of this PublishedMocImageData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PublishedMocImageData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PublishedMocImageSubmitData {
        return PublishedMocImageService.Instance.ConvertToPublishedMocImageSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PublishedMocImageService extends SecureEndpointBase {

    private static _instance: PublishedMocImageService;
    private listCache: Map<string, Observable<Array<PublishedMocImageData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PublishedMocImageBasicListData>>>;
    private recordCache: Map<string, Observable<PublishedMocImageData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PublishedMocImageData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PublishedMocImageBasicListData>>>();
        this.recordCache = new Map<string, Observable<PublishedMocImageData>>();

        PublishedMocImageService._instance = this;
    }

    public static get Instance(): PublishedMocImageService {
      return PublishedMocImageService._instance;
    }


    public ClearListCaches(config: PublishedMocImageQueryParameters | null = null) {

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


    public ConvertToPublishedMocImageSubmitData(data: PublishedMocImageData): PublishedMocImageSubmitData {

        let output = new PublishedMocImageSubmitData();

        output.id = data.id;
        output.publishedMocId = data.publishedMocId;
        output.imagePath = data.imagePath;
        output.caption = data.caption;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPublishedMocImage(id: bigint | number, includeRelations: boolean = true) : Observable<PublishedMocImageData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const publishedMocImage$ = this.requestPublishedMocImage(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PublishedMocImage", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, publishedMocImage$);

            return publishedMocImage$;
        }

        return this.recordCache.get(configHash) as Observable<PublishedMocImageData>;
    }

    private requestPublishedMocImage(id: bigint | number, includeRelations: boolean = true) : Observable<PublishedMocImageData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PublishedMocImageData>(this.baseUrl + 'api/PublishedMocImage/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePublishedMocImage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPublishedMocImage(id, includeRelations));
            }));
    }

    public GetPublishedMocImageList(config: PublishedMocImageQueryParameters | any = null) : Observable<Array<PublishedMocImageData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const publishedMocImageList$ = this.requestPublishedMocImageList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PublishedMocImage list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, publishedMocImageList$);

            return publishedMocImageList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PublishedMocImageData>>;
    }


    private requestPublishedMocImageList(config: PublishedMocImageQueryParameters | any) : Observable <Array<PublishedMocImageData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PublishedMocImageData>>(this.baseUrl + 'api/PublishedMocImages', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePublishedMocImageList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPublishedMocImageList(config));
            }));
    }

    public GetPublishedMocImagesRowCount(config: PublishedMocImageQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const publishedMocImagesRowCount$ = this.requestPublishedMocImagesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PublishedMocImages row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, publishedMocImagesRowCount$);

            return publishedMocImagesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPublishedMocImagesRowCount(config: PublishedMocImageQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PublishedMocImages/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPublishedMocImagesRowCount(config));
            }));
    }

    public GetPublishedMocImagesBasicListData(config: PublishedMocImageQueryParameters | any = null) : Observable<Array<PublishedMocImageBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const publishedMocImagesBasicListData$ = this.requestPublishedMocImagesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PublishedMocImages basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, publishedMocImagesBasicListData$);

            return publishedMocImagesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PublishedMocImageBasicListData>>;
    }


    private requestPublishedMocImagesBasicListData(config: PublishedMocImageQueryParameters | any) : Observable<Array<PublishedMocImageBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PublishedMocImageBasicListData>>(this.baseUrl + 'api/PublishedMocImages/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPublishedMocImagesBasicListData(config));
            }));

    }


    public PutPublishedMocImage(id: bigint | number, publishedMocImage: PublishedMocImageSubmitData) : Observable<PublishedMocImageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PublishedMocImageData>(this.baseUrl + 'api/PublishedMocImage/' + id.toString(), publishedMocImage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePublishedMocImage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPublishedMocImage(id, publishedMocImage));
            }));
    }


    public PostPublishedMocImage(publishedMocImage: PublishedMocImageSubmitData) : Observable<PublishedMocImageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PublishedMocImageData>(this.baseUrl + 'api/PublishedMocImage', publishedMocImage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePublishedMocImage(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPublishedMocImage(publishedMocImage));
            }));
    }

  
    public DeletePublishedMocImage(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PublishedMocImage/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePublishedMocImage(id));
            }));
    }


    private getConfigHash(config: PublishedMocImageQueryParameters | any): string {

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

    public userIsBMCPublishedMocImageReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCPublishedMocImageReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.PublishedMocImages
        //
        if (userIsBMCPublishedMocImageReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCPublishedMocImageReader = user.readPermission >= 1;
            } else {
                userIsBMCPublishedMocImageReader = false;
            }
        }

        return userIsBMCPublishedMocImageReader;
    }


    public userIsBMCPublishedMocImageWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCPublishedMocImageWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.PublishedMocImages
        //
        if (userIsBMCPublishedMocImageWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCPublishedMocImageWriter = user.writePermission >= 1;
          } else {
            userIsBMCPublishedMocImageWriter = false;
          }      
        }

        return userIsBMCPublishedMocImageWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full PublishedMocImageData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PublishedMocImageData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PublishedMocImageTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePublishedMocImage(raw: any): PublishedMocImageData {
    if (!raw) return raw;

    //
    // Create a PublishedMocImageData object instance with correct prototype
    //
    const revived = Object.create(PublishedMocImageData.prototype) as PublishedMocImageData;

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
    // 2. But private methods (loadPublishedMocImageXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private RevivePublishedMocImageList(rawList: any[]): PublishedMocImageData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePublishedMocImage(raw));
  }

}
