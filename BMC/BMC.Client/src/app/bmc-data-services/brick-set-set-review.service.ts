/*

   GENERATED SERVICE FOR THE BRICKSETSETREVIEW TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickSetSetReview table.

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
import { LegoSetData } from './lego-set.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BrickSetSetReviewQueryParameters {
    legoSetId: bigint | number | null | undefined = null;
    reviewAuthor: string | null | undefined = null;
    reviewDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    reviewTitle: string | null | undefined = null;
    reviewBody: string | null | undefined = null;
    overallRating: bigint | number | null | undefined = null;
    buildingExperienceRating: bigint | number | null | undefined = null;
    valueForMoneyRating: bigint | number | null | undefined = null;
    partsRating: bigint | number | null | undefined = null;
    playabilityRating: bigint | number | null | undefined = null;
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
export class BrickSetSetReviewSubmitData {
    id!: bigint | number;
    legoSetId!: bigint | number;
    reviewAuthor!: string;
    reviewDate: string | null = null;     // ISO 8601 (full datetime)
    reviewTitle: string | null = null;
    reviewBody: string | null = null;
    overallRating: bigint | number | null = null;
    buildingExperienceRating: bigint | number | null = null;
    valueForMoneyRating: bigint | number | null = null;
    partsRating: bigint | number | null = null;
    playabilityRating: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BrickSetSetReviewBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickSetSetReviewChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickSetSetReview.BrickSetSetReviewChildren$` — use with `| async` in templates
//        • Promise:    `brickSetSetReview.BrickSetSetReviewChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickSetSetReview.BrickSetSetReviewChildren$ | async"`), or
//        • Access the promise getter (`brickSetSetReview.BrickSetSetReviewChildren` or `await brickSetSetReview.BrickSetSetReviewChildren`)
//    - Simply reading `brickSetSetReview.BrickSetSetReviewChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickSetSetReview.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickSetSetReviewData {
    id!: bigint | number;
    legoSetId!: bigint | number;
    reviewAuthor!: string;
    reviewDate!: string | null;   // ISO 8601 (full datetime)
    reviewTitle!: string | null;
    reviewBody!: string | null;
    overallRating!: bigint | number;
    buildingExperienceRating!: bigint | number;
    valueForMoneyRating!: bigint | number;
    partsRating!: bigint | number;
    playabilityRating!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    legoSet: LegoSetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any BrickSetSetReviewData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickSetSetReview.Reload();
  //
  //  Non Async:
  //
  //     brickSetSetReview[0].Reload().then(x => {
  //        this.brickSetSetReview = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickSetSetReviewService.Instance.GetBrickSetSetReview(this.id, includeRelations)
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
     * Updates the state of this BrickSetSetReviewData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickSetSetReviewData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickSetSetReviewSubmitData {
        return BrickSetSetReviewService.Instance.ConvertToBrickSetSetReviewSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickSetSetReviewService extends SecureEndpointBase {

    private static _instance: BrickSetSetReviewService;
    private listCache: Map<string, Observable<Array<BrickSetSetReviewData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickSetSetReviewBasicListData>>>;
    private recordCache: Map<string, Observable<BrickSetSetReviewData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickSetSetReviewData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickSetSetReviewBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickSetSetReviewData>>();

        BrickSetSetReviewService._instance = this;
    }

    public static get Instance(): BrickSetSetReviewService {
      return BrickSetSetReviewService._instance;
    }


    public ClearListCaches(config: BrickSetSetReviewQueryParameters | null = null) {

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


    public ConvertToBrickSetSetReviewSubmitData(data: BrickSetSetReviewData): BrickSetSetReviewSubmitData {

        let output = new BrickSetSetReviewSubmitData();

        output.id = data.id;
        output.legoSetId = data.legoSetId;
        output.reviewAuthor = data.reviewAuthor;
        output.reviewDate = data.reviewDate;
        output.reviewTitle = data.reviewTitle;
        output.reviewBody = data.reviewBody;
        output.overallRating = data.overallRating;
        output.buildingExperienceRating = data.buildingExperienceRating;
        output.valueForMoneyRating = data.valueForMoneyRating;
        output.partsRating = data.partsRating;
        output.playabilityRating = data.playabilityRating;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBrickSetSetReview(id: bigint | number, includeRelations: boolean = true) : Observable<BrickSetSetReviewData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickSetSetReview$ = this.requestBrickSetSetReview(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickSetSetReview", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickSetSetReview$);

            return brickSetSetReview$;
        }

        return this.recordCache.get(configHash) as Observable<BrickSetSetReviewData>;
    }

    private requestBrickSetSetReview(id: bigint | number, includeRelations: boolean = true) : Observable<BrickSetSetReviewData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickSetSetReviewData>(this.baseUrl + 'api/BrickSetSetReview/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickSetSetReview(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickSetSetReview(id, includeRelations));
            }));
    }

    public GetBrickSetSetReviewList(config: BrickSetSetReviewQueryParameters | any = null) : Observable<Array<BrickSetSetReviewData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickSetSetReviewList$ = this.requestBrickSetSetReviewList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickSetSetReview list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickSetSetReviewList$);

            return brickSetSetReviewList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickSetSetReviewData>>;
    }


    private requestBrickSetSetReviewList(config: BrickSetSetReviewQueryParameters | any) : Observable <Array<BrickSetSetReviewData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickSetSetReviewData>>(this.baseUrl + 'api/BrickSetSetReviews', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickSetSetReviewList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickSetSetReviewList(config));
            }));
    }

    public GetBrickSetSetReviewsRowCount(config: BrickSetSetReviewQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickSetSetReviewsRowCount$ = this.requestBrickSetSetReviewsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickSetSetReviews row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickSetSetReviewsRowCount$);

            return brickSetSetReviewsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickSetSetReviewsRowCount(config: BrickSetSetReviewQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickSetSetReviews/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickSetSetReviewsRowCount(config));
            }));
    }

    public GetBrickSetSetReviewsBasicListData(config: BrickSetSetReviewQueryParameters | any = null) : Observable<Array<BrickSetSetReviewBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickSetSetReviewsBasicListData$ = this.requestBrickSetSetReviewsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickSetSetReviews basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickSetSetReviewsBasicListData$);

            return brickSetSetReviewsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickSetSetReviewBasicListData>>;
    }


    private requestBrickSetSetReviewsBasicListData(config: BrickSetSetReviewQueryParameters | any) : Observable<Array<BrickSetSetReviewBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickSetSetReviewBasicListData>>(this.baseUrl + 'api/BrickSetSetReviews/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickSetSetReviewsBasicListData(config));
            }));

    }


    public PutBrickSetSetReview(id: bigint | number, brickSetSetReview: BrickSetSetReviewSubmitData) : Observable<BrickSetSetReviewData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickSetSetReviewData>(this.baseUrl + 'api/BrickSetSetReview/' + id.toString(), brickSetSetReview, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickSetSetReview(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickSetSetReview(id, brickSetSetReview));
            }));
    }


    public PostBrickSetSetReview(brickSetSetReview: BrickSetSetReviewSubmitData) : Observable<BrickSetSetReviewData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickSetSetReviewData>(this.baseUrl + 'api/BrickSetSetReview', brickSetSetReview, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickSetSetReview(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickSetSetReview(brickSetSetReview));
            }));
    }

  
    public DeleteBrickSetSetReview(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickSetSetReview/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickSetSetReview(id));
            }));
    }


    private getConfigHash(config: BrickSetSetReviewQueryParameters | any): string {

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

    public userIsBMCBrickSetSetReviewReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickSetSetReviewReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickSetSetReviews
        //
        if (userIsBMCBrickSetSetReviewReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickSetSetReviewReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickSetSetReviewReader = false;
            }
        }

        return userIsBMCBrickSetSetReviewReader;
    }


    public userIsBMCBrickSetSetReviewWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickSetSetReviewWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickSetSetReviews
        //
        if (userIsBMCBrickSetSetReviewWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickSetSetReviewWriter = user.writePermission >= 255;
          } else {
            userIsBMCBrickSetSetReviewWriter = false;
          }      
        }

        return userIsBMCBrickSetSetReviewWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BrickSetSetReviewData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickSetSetReviewData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickSetSetReviewTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickSetSetReview(raw: any): BrickSetSetReviewData {
    if (!raw) return raw;

    //
    // Create a BrickSetSetReviewData object instance with correct prototype
    //
    const revived = Object.create(BrickSetSetReviewData.prototype) as BrickSetSetReviewData;

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
    // 2. But private methods (loadBrickSetSetReviewXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBrickSetSetReviewList(rawList: any[]): BrickSetSetReviewData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickSetSetReview(raw));
  }

}
