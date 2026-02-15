/*

   GENERATED SERVICE FOR THE CONTENTREPORTREASON TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ContentReportReason table.

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
import { ContentReportService, ContentReportData } from './content-report.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ContentReportReasonQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class ContentReportReasonSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ContentReportReasonBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ContentReportReasonChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `contentReportReason.ContentReportReasonChildren$` — use with `| async` in templates
//        • Promise:    `contentReportReason.ContentReportReasonChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="contentReportReason.ContentReportReasonChildren$ | async"`), or
//        • Access the promise getter (`contentReportReason.ContentReportReasonChildren` or `await contentReportReason.ContentReportReasonChildren`)
//    - Simply reading `contentReportReason.ContentReportReasonChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await contentReportReason.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ContentReportReasonData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _contentReports: ContentReportData[] | null = null;
    private _contentReportsPromise: Promise<ContentReportData[]> | null  = null;
    private _contentReportsSubject = new BehaviorSubject<ContentReportData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ContentReports$ = this._contentReportsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contentReports === null && this._contentReportsPromise === null) {
            this.loadContentReports(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ContentReportsCount$ = ContentReportService.Instance.GetContentReportsRowCount({contentReportReasonId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ContentReportReasonData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.contentReportReason.Reload();
  //
  //  Non Async:
  //
  //     contentReportReason[0].Reload().then(x => {
  //        this.contentReportReason = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ContentReportReasonService.Instance.GetContentReportReason(this.id, includeRelations)
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
     this._contentReports = null;
     this._contentReportsPromise = null;
     this._contentReportsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ContentReports for this ContentReportReason.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contentReportReason.ContentReports.then(contentReportReasons => { ... })
     *   or
     *   await this.contentReportReason.contentReportReasons
     *
    */
    public get ContentReports(): Promise<ContentReportData[]> {
        if (this._contentReports !== null) {
            return Promise.resolve(this._contentReports);
        }

        if (this._contentReportsPromise !== null) {
            return this._contentReportsPromise;
        }

        // Start the load
        this.loadContentReports();

        return this._contentReportsPromise!;
    }



    private loadContentReports(): void {

        this._contentReportsPromise = lastValueFrom(
            ContentReportReasonService.Instance.GetContentReportsForContentReportReason(this.id)
        )
        .then(ContentReports => {
            this._contentReports = ContentReports ?? [];
            this._contentReportsSubject.next(this._contentReports);
            return this._contentReports;
         })
        .catch(err => {
            this._contentReports = [];
            this._contentReportsSubject.next(this._contentReports);
            throw err;
        })
        .finally(() => {
            this._contentReportsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContentReport. Call after mutations to force refresh.
     */
    public ClearContentReportsCache(): void {
        this._contentReports = null;
        this._contentReportsPromise = null;
        this._contentReportsSubject.next(this._contentReports);      // Emit to observable
    }

    public get HasContentReports(): Promise<boolean> {
        return this.ContentReports.then(contentReports => contentReports.length > 0);
    }




    /**
     * Updates the state of this ContentReportReasonData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ContentReportReasonData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ContentReportReasonSubmitData {
        return ContentReportReasonService.Instance.ConvertToContentReportReasonSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ContentReportReasonService extends SecureEndpointBase {

    private static _instance: ContentReportReasonService;
    private listCache: Map<string, Observable<Array<ContentReportReasonData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ContentReportReasonBasicListData>>>;
    private recordCache: Map<string, Observable<ContentReportReasonData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private contentReportService: ContentReportService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ContentReportReasonData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ContentReportReasonBasicListData>>>();
        this.recordCache = new Map<string, Observable<ContentReportReasonData>>();

        ContentReportReasonService._instance = this;
    }

    public static get Instance(): ContentReportReasonService {
      return ContentReportReasonService._instance;
    }


    public ClearListCaches(config: ContentReportReasonQueryParameters | null = null) {

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


    public ConvertToContentReportReasonSubmitData(data: ContentReportReasonData): ContentReportReasonSubmitData {

        let output = new ContentReportReasonSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetContentReportReason(id: bigint | number, includeRelations: boolean = true) : Observable<ContentReportReasonData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const contentReportReason$ = this.requestContentReportReason(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContentReportReason", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, contentReportReason$);

            return contentReportReason$;
        }

        return this.recordCache.get(configHash) as Observable<ContentReportReasonData>;
    }

    private requestContentReportReason(id: bigint | number, includeRelations: boolean = true) : Observable<ContentReportReasonData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContentReportReasonData>(this.baseUrl + 'api/ContentReportReason/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveContentReportReason(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestContentReportReason(id, includeRelations));
            }));
    }

    public GetContentReportReasonList(config: ContentReportReasonQueryParameters | any = null) : Observable<Array<ContentReportReasonData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const contentReportReasonList$ = this.requestContentReportReasonList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContentReportReason list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, contentReportReasonList$);

            return contentReportReasonList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ContentReportReasonData>>;
    }


    private requestContentReportReasonList(config: ContentReportReasonQueryParameters | any) : Observable <Array<ContentReportReasonData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContentReportReasonData>>(this.baseUrl + 'api/ContentReportReasons', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveContentReportReasonList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestContentReportReasonList(config));
            }));
    }

    public GetContentReportReasonsRowCount(config: ContentReportReasonQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const contentReportReasonsRowCount$ = this.requestContentReportReasonsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContentReportReasons row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, contentReportReasonsRowCount$);

            return contentReportReasonsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestContentReportReasonsRowCount(config: ContentReportReasonQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ContentReportReasons/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContentReportReasonsRowCount(config));
            }));
    }

    public GetContentReportReasonsBasicListData(config: ContentReportReasonQueryParameters | any = null) : Observable<Array<ContentReportReasonBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const contentReportReasonsBasicListData$ = this.requestContentReportReasonsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContentReportReasons basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, contentReportReasonsBasicListData$);

            return contentReportReasonsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ContentReportReasonBasicListData>>;
    }


    private requestContentReportReasonsBasicListData(config: ContentReportReasonQueryParameters | any) : Observable<Array<ContentReportReasonBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContentReportReasonBasicListData>>(this.baseUrl + 'api/ContentReportReasons/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContentReportReasonsBasicListData(config));
            }));

    }


    public PutContentReportReason(id: bigint | number, contentReportReason: ContentReportReasonSubmitData) : Observable<ContentReportReasonData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ContentReportReasonData>(this.baseUrl + 'api/ContentReportReason/' + id.toString(), contentReportReason, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContentReportReason(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutContentReportReason(id, contentReportReason));
            }));
    }


    public PostContentReportReason(contentReportReason: ContentReportReasonSubmitData) : Observable<ContentReportReasonData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ContentReportReasonData>(this.baseUrl + 'api/ContentReportReason', contentReportReason, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContentReportReason(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostContentReportReason(contentReportReason));
            }));
    }

  
    public DeleteContentReportReason(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ContentReportReason/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteContentReportReason(id));
            }));
    }


    private getConfigHash(config: ContentReportReasonQueryParameters | any): string {

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

    public userIsBMCContentReportReasonReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCContentReportReasonReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ContentReportReasons
        //
        if (userIsBMCContentReportReasonReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCContentReportReasonReader = user.readPermission >= 1;
            } else {
                userIsBMCContentReportReasonReader = false;
            }
        }

        return userIsBMCContentReportReasonReader;
    }


    public userIsBMCContentReportReasonWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCContentReportReasonWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ContentReportReasons
        //
        if (userIsBMCContentReportReasonWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCContentReportReasonWriter = user.writePermission >= 255;
          } else {
            userIsBMCContentReportReasonWriter = false;
          }      
        }

        return userIsBMCContentReportReasonWriter;
    }

    public GetContentReportsForContentReportReason(contentReportReasonId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContentReportData[]> {
        return this.contentReportService.GetContentReportList({
            contentReportReasonId: contentReportReasonId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ContentReportReasonData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ContentReportReasonData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ContentReportReasonTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveContentReportReason(raw: any): ContentReportReasonData {
    if (!raw) return raw;

    //
    // Create a ContentReportReasonData object instance with correct prototype
    //
    const revived = Object.create(ContentReportReasonData.prototype) as ContentReportReasonData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._contentReports = null;
    (revived as any)._contentReportsPromise = null;
    (revived as any)._contentReportsSubject = new BehaviorSubject<ContentReportData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadContentReportReasonXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ContentReports$ = (revived as any)._contentReportsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contentReports === null && (revived as any)._contentReportsPromise === null) {
                (revived as any).loadContentReports();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ContentReportsCount$ = ContentReportService.Instance.GetContentReportsRowCount({contentReportReasonId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveContentReportReasonList(rawList: any[]): ContentReportReasonData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveContentReportReason(raw));
  }

}
