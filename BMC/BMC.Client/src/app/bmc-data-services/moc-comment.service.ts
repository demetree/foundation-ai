/*

   GENERATED SERVICE FOR THE MOCCOMMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the MocComment table.

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
export class MocCommentQueryParameters {
    publishedMocId: bigint | number | null | undefined = null;
    commenterTenantGuid: string | null | undefined = null;
    commentText: string | null | undefined = null;
    postedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    mocCommentId: bigint | number | null | undefined = null;
    isEdited: boolean | null | undefined = null;
    isHidden: boolean | null | undefined = null;
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
export class MocCommentSubmitData {
    id!: bigint | number;
    publishedMocId!: bigint | number;
    commenterTenantGuid!: string;
    commentText!: string;
    postedDate!: string;      // ISO 8601 (full datetime)
    mocCommentId: bigint | number | null = null;
    isEdited!: boolean;
    isHidden!: boolean;
    active!: boolean;
    deleted!: boolean;
}


export class MocCommentBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. MocCommentChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `mocComment.MocCommentChildren$` — use with `| async` in templates
//        • Promise:    `mocComment.MocCommentChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="mocComment.MocCommentChildren$ | async"`), or
//        • Access the promise getter (`mocComment.MocCommentChildren` or `await mocComment.MocCommentChildren`)
//    - Simply reading `mocComment.MocCommentChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await mocComment.Reload()` to refresh the entire object and clear all lazy caches.
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
export class MocCommentData {
    id!: bigint | number;
    publishedMocId!: bigint | number;
    commenterTenantGuid!: string;
    commentText!: string;
    postedDate!: string;      // ISO 8601 (full datetime)
    mocCommentId!: bigint | number;
    isEdited!: boolean;
    isHidden!: boolean;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    mocComment: MocCommentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
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
  // Promise based reload method to allow rebuilding of any MocCommentData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.mocComment.Reload();
  //
  //  Non Async:
  //
  //     mocComment[0].Reload().then(x => {
  //        this.mocComment = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      MocCommentService.Instance.GetMocComment(this.id, includeRelations)
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
     * Updates the state of this MocCommentData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this MocCommentData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): MocCommentSubmitData {
        return MocCommentService.Instance.ConvertToMocCommentSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class MocCommentService extends SecureEndpointBase {

    private static _instance: MocCommentService;
    private listCache: Map<string, Observable<Array<MocCommentData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<MocCommentBasicListData>>>;
    private recordCache: Map<string, Observable<MocCommentData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<MocCommentData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<MocCommentBasicListData>>>();
        this.recordCache = new Map<string, Observable<MocCommentData>>();

        MocCommentService._instance = this;
    }

    public static get Instance(): MocCommentService {
      return MocCommentService._instance;
    }


    public ClearListCaches(config: MocCommentQueryParameters | null = null) {

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


    public ConvertToMocCommentSubmitData(data: MocCommentData): MocCommentSubmitData {

        let output = new MocCommentSubmitData();

        output.id = data.id;
        output.publishedMocId = data.publishedMocId;
        output.commenterTenantGuid = data.commenterTenantGuid;
        output.commentText = data.commentText;
        output.postedDate = data.postedDate;
        output.mocCommentId = data.mocCommentId;
        output.isEdited = data.isEdited;
        output.isHidden = data.isHidden;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetMocComment(id: bigint | number, includeRelations: boolean = true) : Observable<MocCommentData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const mocComment$ = this.requestMocComment(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MocComment", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, mocComment$);

            return mocComment$;
        }

        return this.recordCache.get(configHash) as Observable<MocCommentData>;
    }

    private requestMocComment(id: bigint | number, includeRelations: boolean = true) : Observable<MocCommentData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<MocCommentData>(this.baseUrl + 'api/MocComment/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveMocComment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestMocComment(id, includeRelations));
            }));
    }

    public GetMocCommentList(config: MocCommentQueryParameters | any = null) : Observable<Array<MocCommentData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const mocCommentList$ = this.requestMocCommentList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MocComment list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, mocCommentList$);

            return mocCommentList$;
        }

        return this.listCache.get(configHash) as Observable<Array<MocCommentData>>;
    }


    private requestMocCommentList(config: MocCommentQueryParameters | any) : Observable <Array<MocCommentData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MocCommentData>>(this.baseUrl + 'api/MocComments', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveMocCommentList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestMocCommentList(config));
            }));
    }

    public GetMocCommentsRowCount(config: MocCommentQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const mocCommentsRowCount$ = this.requestMocCommentsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MocComments row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, mocCommentsRowCount$);

            return mocCommentsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestMocCommentsRowCount(config: MocCommentQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/MocComments/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMocCommentsRowCount(config));
            }));
    }

    public GetMocCommentsBasicListData(config: MocCommentQueryParameters | any = null) : Observable<Array<MocCommentBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const mocCommentsBasicListData$ = this.requestMocCommentsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MocComments basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, mocCommentsBasicListData$);

            return mocCommentsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<MocCommentBasicListData>>;
    }


    private requestMocCommentsBasicListData(config: MocCommentQueryParameters | any) : Observable<Array<MocCommentBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MocCommentBasicListData>>(this.baseUrl + 'api/MocComments/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMocCommentsBasicListData(config));
            }));

    }


    public PutMocComment(id: bigint | number, mocComment: MocCommentSubmitData) : Observable<MocCommentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<MocCommentData>(this.baseUrl + 'api/MocComment/' + id.toString(), mocComment, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMocComment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutMocComment(id, mocComment));
            }));
    }


    public PostMocComment(mocComment: MocCommentSubmitData) : Observable<MocCommentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<MocCommentData>(this.baseUrl + 'api/MocComment', mocComment, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMocComment(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostMocComment(mocComment));
            }));
    }

  
    public DeleteMocComment(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/MocComment/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteMocComment(id));
            }));
    }


    private getConfigHash(config: MocCommentQueryParameters | any): string {

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

    public userIsBMCMocCommentReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCMocCommentReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.MocComments
        //
        if (userIsBMCMocCommentReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCMocCommentReader = user.readPermission >= 1;
            } else {
                userIsBMCMocCommentReader = false;
            }
        }

        return userIsBMCMocCommentReader;
    }


    public userIsBMCMocCommentWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCMocCommentWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.MocComments
        //
        if (userIsBMCMocCommentWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCMocCommentWriter = user.writePermission >= 1;
          } else {
            userIsBMCMocCommentWriter = false;
          }      
        }

        return userIsBMCMocCommentWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full MocCommentData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the MocCommentData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when MocCommentTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveMocComment(raw: any): MocCommentData {
    if (!raw) return raw;

    //
    // Create a MocCommentData object instance with correct prototype
    //
    const revived = Object.create(MocCommentData.prototype) as MocCommentData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._mocComments = null;
    (revived as any)._mocCommentsPromise = null;
    (revived as any)._mocCommentsSubject = new BehaviorSubject<MocCommentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadMocCommentXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).MocComments$ = (revived as any)._mocCommentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._mocComments === null && (revived as any)._mocCommentsPromise === null) {
                (revived as any).loadMocComments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).MocCommentsCount$ = MocCommentService.Instance.GetMocCommentsRowCount({mocCommentId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveMocCommentList(rawList: any[]): MocCommentData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveMocComment(raw));
  }

}
