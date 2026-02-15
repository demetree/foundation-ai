/*

   GENERATED SERVICE FOR THE CONTENTREPORT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ContentReport table.

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
import { ContentReportReasonData } from './content-report-reason.service';
import { ModerationActionService, ModerationActionData } from './moderation-action.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ContentReportQueryParameters {
    contentReportReasonId: bigint | number | null | undefined = null;
    reporterTenantGuid: string | null | undefined = null;
    reportedEntityType: string | null | undefined = null;
    reportedEntityId: bigint | number | null | undefined = null;
    description: string | null | undefined = null;
    status: string | null | undefined = null;
    reportedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    reviewedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    reviewerTenantGuid: string | null | undefined = null;
    reviewNotes: string | null | undefined = null;
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
export class ContentReportSubmitData {
    id!: bigint | number;
    contentReportReasonId!: bigint | number;
    reporterTenantGuid!: string;
    reportedEntityType!: string;
    reportedEntityId!: bigint | number;
    description: string | null = null;
    status!: string;
    reportedDate!: string;      // ISO 8601 (full datetime)
    reviewedDate: string | null = null;     // ISO 8601 (full datetime)
    reviewerTenantGuid: string | null = null;
    reviewNotes: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ContentReportBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ContentReportChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `contentReport.ContentReportChildren$` — use with `| async` in templates
//        • Promise:    `contentReport.ContentReportChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="contentReport.ContentReportChildren$ | async"`), or
//        • Access the promise getter (`contentReport.ContentReportChildren` or `await contentReport.ContentReportChildren`)
//    - Simply reading `contentReport.ContentReportChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await contentReport.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ContentReportData {
    id!: bigint | number;
    contentReportReasonId!: bigint | number;
    reporterTenantGuid!: string;
    reportedEntityType!: string;
    reportedEntityId!: bigint | number;
    description!: string | null;
    status!: string;
    reportedDate!: string;      // ISO 8601 (full datetime)
    reviewedDate!: string | null;   // ISO 8601 (full datetime)
    reviewerTenantGuid!: string | null;
    reviewNotes!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    contentReportReason: ContentReportReasonData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _moderationActions: ModerationActionData[] | null = null;
    private _moderationActionsPromise: Promise<ModerationActionData[]> | null  = null;
    private _moderationActionsSubject = new BehaviorSubject<ModerationActionData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ModerationActions$ = this._moderationActionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._moderationActions === null && this._moderationActionsPromise === null) {
            this.loadModerationActions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ModerationActionsCount$ = ModerationActionService.Instance.GetModerationActionsRowCount({contentReportId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ContentReportData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.contentReport.Reload();
  //
  //  Non Async:
  //
  //     contentReport[0].Reload().then(x => {
  //        this.contentReport = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ContentReportService.Instance.GetContentReport(this.id, includeRelations)
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
     this._moderationActions = null;
     this._moderationActionsPromise = null;
     this._moderationActionsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ModerationActions for this ContentReport.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contentReport.ModerationActions.then(contentReports => { ... })
     *   or
     *   await this.contentReport.contentReports
     *
    */
    public get ModerationActions(): Promise<ModerationActionData[]> {
        if (this._moderationActions !== null) {
            return Promise.resolve(this._moderationActions);
        }

        if (this._moderationActionsPromise !== null) {
            return this._moderationActionsPromise;
        }

        // Start the load
        this.loadModerationActions();

        return this._moderationActionsPromise!;
    }



    private loadModerationActions(): void {

        this._moderationActionsPromise = lastValueFrom(
            ContentReportService.Instance.GetModerationActionsForContentReport(this.id)
        )
        .then(ModerationActions => {
            this._moderationActions = ModerationActions ?? [];
            this._moderationActionsSubject.next(this._moderationActions);
            return this._moderationActions;
         })
        .catch(err => {
            this._moderationActions = [];
            this._moderationActionsSubject.next(this._moderationActions);
            throw err;
        })
        .finally(() => {
            this._moderationActionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ModerationAction. Call after mutations to force refresh.
     */
    public ClearModerationActionsCache(): void {
        this._moderationActions = null;
        this._moderationActionsPromise = null;
        this._moderationActionsSubject.next(this._moderationActions);      // Emit to observable
    }

    public get HasModerationActions(): Promise<boolean> {
        return this.ModerationActions.then(moderationActions => moderationActions.length > 0);
    }




    /**
     * Updates the state of this ContentReportData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ContentReportData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ContentReportSubmitData {
        return ContentReportService.Instance.ConvertToContentReportSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ContentReportService extends SecureEndpointBase {

    private static _instance: ContentReportService;
    private listCache: Map<string, Observable<Array<ContentReportData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ContentReportBasicListData>>>;
    private recordCache: Map<string, Observable<ContentReportData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private moderationActionService: ModerationActionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ContentReportData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ContentReportBasicListData>>>();
        this.recordCache = new Map<string, Observable<ContentReportData>>();

        ContentReportService._instance = this;
    }

    public static get Instance(): ContentReportService {
      return ContentReportService._instance;
    }


    public ClearListCaches(config: ContentReportQueryParameters | null = null) {

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


    public ConvertToContentReportSubmitData(data: ContentReportData): ContentReportSubmitData {

        let output = new ContentReportSubmitData();

        output.id = data.id;
        output.contentReportReasonId = data.contentReportReasonId;
        output.reporterTenantGuid = data.reporterTenantGuid;
        output.reportedEntityType = data.reportedEntityType;
        output.reportedEntityId = data.reportedEntityId;
        output.description = data.description;
        output.status = data.status;
        output.reportedDate = data.reportedDate;
        output.reviewedDate = data.reviewedDate;
        output.reviewerTenantGuid = data.reviewerTenantGuid;
        output.reviewNotes = data.reviewNotes;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetContentReport(id: bigint | number, includeRelations: boolean = true) : Observable<ContentReportData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const contentReport$ = this.requestContentReport(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContentReport", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, contentReport$);

            return contentReport$;
        }

        return this.recordCache.get(configHash) as Observable<ContentReportData>;
    }

    private requestContentReport(id: bigint | number, includeRelations: boolean = true) : Observable<ContentReportData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContentReportData>(this.baseUrl + 'api/ContentReport/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveContentReport(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestContentReport(id, includeRelations));
            }));
    }

    public GetContentReportList(config: ContentReportQueryParameters | any = null) : Observable<Array<ContentReportData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const contentReportList$ = this.requestContentReportList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContentReport list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, contentReportList$);

            return contentReportList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ContentReportData>>;
    }


    private requestContentReportList(config: ContentReportQueryParameters | any) : Observable <Array<ContentReportData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContentReportData>>(this.baseUrl + 'api/ContentReports', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveContentReportList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestContentReportList(config));
            }));
    }

    public GetContentReportsRowCount(config: ContentReportQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const contentReportsRowCount$ = this.requestContentReportsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContentReports row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, contentReportsRowCount$);

            return contentReportsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestContentReportsRowCount(config: ContentReportQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ContentReports/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContentReportsRowCount(config));
            }));
    }

    public GetContentReportsBasicListData(config: ContentReportQueryParameters | any = null) : Observable<Array<ContentReportBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const contentReportsBasicListData$ = this.requestContentReportsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContentReports basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, contentReportsBasicListData$);

            return contentReportsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ContentReportBasicListData>>;
    }


    private requestContentReportsBasicListData(config: ContentReportQueryParameters | any) : Observable<Array<ContentReportBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContentReportBasicListData>>(this.baseUrl + 'api/ContentReports/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContentReportsBasicListData(config));
            }));

    }


    public PutContentReport(id: bigint | number, contentReport: ContentReportSubmitData) : Observable<ContentReportData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ContentReportData>(this.baseUrl + 'api/ContentReport/' + id.toString(), contentReport, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContentReport(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutContentReport(id, contentReport));
            }));
    }


    public PostContentReport(contentReport: ContentReportSubmitData) : Observable<ContentReportData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ContentReportData>(this.baseUrl + 'api/ContentReport', contentReport, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContentReport(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostContentReport(contentReport));
            }));
    }

  
    public DeleteContentReport(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ContentReport/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteContentReport(id));
            }));
    }


    private getConfigHash(config: ContentReportQueryParameters | any): string {

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

    public userIsBMCContentReportReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCContentReportReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ContentReports
        //
        if (userIsBMCContentReportReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCContentReportReader = user.readPermission >= 1;
            } else {
                userIsBMCContentReportReader = false;
            }
        }

        return userIsBMCContentReportReader;
    }


    public userIsBMCContentReportWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCContentReportWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ContentReports
        //
        if (userIsBMCContentReportWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCContentReportWriter = user.writePermission >= 1;
          } else {
            userIsBMCContentReportWriter = false;
          }      
        }

        return userIsBMCContentReportWriter;
    }

    public GetModerationActionsForContentReport(contentReportId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ModerationActionData[]> {
        return this.moderationActionService.GetModerationActionList({
            contentReportId: contentReportId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ContentReportData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ContentReportData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ContentReportTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveContentReport(raw: any): ContentReportData {
    if (!raw) return raw;

    //
    // Create a ContentReportData object instance with correct prototype
    //
    const revived = Object.create(ContentReportData.prototype) as ContentReportData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._moderationActions = null;
    (revived as any)._moderationActionsPromise = null;
    (revived as any)._moderationActionsSubject = new BehaviorSubject<ModerationActionData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadContentReportXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ModerationActions$ = (revived as any)._moderationActionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._moderationActions === null && (revived as any)._moderationActionsPromise === null) {
                (revived as any).loadModerationActions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ModerationActionsCount$ = ModerationActionService.Instance.GetModerationActionsRowCount({contentReportId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveContentReportList(rawList: any[]): ContentReportData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveContentReport(raw));
  }

}
