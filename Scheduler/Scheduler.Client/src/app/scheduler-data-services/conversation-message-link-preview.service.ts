/*

   GENERATED SERVICE FOR THE CONVERSATIONMESSAGELINKPREVIEW TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ConversationMessageLinkPreview table.

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
import { ConversationMessageData } from './conversation-message.service';
import { ConversationMessageLinkPreviewChangeHistoryService, ConversationMessageLinkPreviewChangeHistoryData } from './conversation-message-link-preview-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ConversationMessageLinkPreviewQueryParameters {
    conversationMessageId: bigint | number | null | undefined = null;
    url: string | null | undefined = null;
    title: string | null | undefined = null;
    description: string | null | undefined = null;
    imageUrl: string | null | undefined = null;
    siteName: string | null | undefined = null;
    fetchedDateTime: string | null | undefined = null;        // ISO 8601 (full datetime)
    versionNumber: bigint | number | null | undefined = null;
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
export class ConversationMessageLinkPreviewSubmitData {
    id!: bigint | number;
    conversationMessageId!: bigint | number;
    url!: string;
    title: string | null = null;
    description: string | null = null;
    imageUrl: string | null = null;
    siteName: string | null = null;
    fetchedDateTime!: string;      // ISO 8601 (full datetime)
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}



//
// Version history information returned from version history API endpoints.
// Matches server-side VersionInformation<T> structure.
//
export interface VersionInformationUser {
    id: bigint | number;
    userName: string;
    firstName: string | null;
    middleName: string | null;
    lastName: string | null;
}

export interface VersionInformation<T> {
    timeStamp: string;           // ISO 8601
    user: VersionInformationUser;
    versionNumber: number;
    data: T | null;
}

export class ConversationMessageLinkPreviewBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ConversationMessageLinkPreviewChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `conversationMessageLinkPreview.ConversationMessageLinkPreviewChildren$` — use with `| async` in templates
//        • Promise:    `conversationMessageLinkPreview.ConversationMessageLinkPreviewChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="conversationMessageLinkPreview.ConversationMessageLinkPreviewChildren$ | async"`), or
//        • Access the promise getter (`conversationMessageLinkPreview.ConversationMessageLinkPreviewChildren` or `await conversationMessageLinkPreview.ConversationMessageLinkPreviewChildren`)
//    - Simply reading `conversationMessageLinkPreview.ConversationMessageLinkPreviewChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await conversationMessageLinkPreview.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ConversationMessageLinkPreviewData {
    id!: bigint | number;
    conversationMessageId!: bigint | number;
    url!: string;
    title!: string | null;
    description!: string | null;
    imageUrl!: string | null;
    siteName!: string | null;
    fetchedDateTime!: string;      // ISO 8601 (full datetime)
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    conversationMessage: ConversationMessageData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _conversationMessageLinkPreviewChangeHistories: ConversationMessageLinkPreviewChangeHistoryData[] | null = null;
    private _conversationMessageLinkPreviewChangeHistoriesPromise: Promise<ConversationMessageLinkPreviewChangeHistoryData[]> | null  = null;
    private _conversationMessageLinkPreviewChangeHistoriesSubject = new BehaviorSubject<ConversationMessageLinkPreviewChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ConversationMessageLinkPreviewData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ConversationMessageLinkPreviewData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ConversationMessageLinkPreviewData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ConversationMessageLinkPreviewChangeHistories$ = this._conversationMessageLinkPreviewChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationMessageLinkPreviewChangeHistories === null && this._conversationMessageLinkPreviewChangeHistoriesPromise === null) {
            this.loadConversationMessageLinkPreviewChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationMessageLinkPreviewChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ConversationMessageLinkPreviewChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._conversationMessageLinkPreviewChangeHistoriesCount$ === null) {
            this._conversationMessageLinkPreviewChangeHistoriesCount$ = ConversationMessageLinkPreviewChangeHistoryService.Instance.GetConversationMessageLinkPreviewChangeHistoriesRowCount({conversationMessageLinkPreviewId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationMessageLinkPreviewChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ConversationMessageLinkPreviewData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.conversationMessageLinkPreview.Reload();
  //
  //  Non Async:
  //
  //     conversationMessageLinkPreview[0].Reload().then(x => {
  //        this.conversationMessageLinkPreview = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ConversationMessageLinkPreviewService.Instance.GetConversationMessageLinkPreview(this.id, includeRelations)
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
     this._conversationMessageLinkPreviewChangeHistories = null;
     this._conversationMessageLinkPreviewChangeHistoriesPromise = null;
     this._conversationMessageLinkPreviewChangeHistoriesSubject.next(null);
     this._conversationMessageLinkPreviewChangeHistoriesCount$ = null;

     this._currentVersionInfo = null;
     this._currentVersionInfoPromise = null;
     this._currentVersionInfoSubject.next(null);
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ConversationMessageLinkPreviewChangeHistories for this ConversationMessageLinkPreview.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationMessageLinkPreview.ConversationMessageLinkPreviewChangeHistories.then(conversationMessageLinkPreviews => { ... })
     *   or
     *   await this.conversationMessageLinkPreview.conversationMessageLinkPreviews
     *
    */
    public get ConversationMessageLinkPreviewChangeHistories(): Promise<ConversationMessageLinkPreviewChangeHistoryData[]> {
        if (this._conversationMessageLinkPreviewChangeHistories !== null) {
            return Promise.resolve(this._conversationMessageLinkPreviewChangeHistories);
        }

        if (this._conversationMessageLinkPreviewChangeHistoriesPromise !== null) {
            return this._conversationMessageLinkPreviewChangeHistoriesPromise;
        }

        // Start the load
        this.loadConversationMessageLinkPreviewChangeHistories();

        return this._conversationMessageLinkPreviewChangeHistoriesPromise!;
    }



    private loadConversationMessageLinkPreviewChangeHistories(): void {

        this._conversationMessageLinkPreviewChangeHistoriesPromise = lastValueFrom(
            ConversationMessageLinkPreviewService.Instance.GetConversationMessageLinkPreviewChangeHistoriesForConversationMessageLinkPreview(this.id)
        )
        .then(ConversationMessageLinkPreviewChangeHistories => {
            this._conversationMessageLinkPreviewChangeHistories = ConversationMessageLinkPreviewChangeHistories ?? [];
            this._conversationMessageLinkPreviewChangeHistoriesSubject.next(this._conversationMessageLinkPreviewChangeHistories);
            return this._conversationMessageLinkPreviewChangeHistories;
         })
        .catch(err => {
            this._conversationMessageLinkPreviewChangeHistories = [];
            this._conversationMessageLinkPreviewChangeHistoriesSubject.next(this._conversationMessageLinkPreviewChangeHistories);
            throw err;
        })
        .finally(() => {
            this._conversationMessageLinkPreviewChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationMessageLinkPreviewChangeHistory. Call after mutations to force refresh.
     */
    public ClearConversationMessageLinkPreviewChangeHistoriesCache(): void {
        this._conversationMessageLinkPreviewChangeHistories = null;
        this._conversationMessageLinkPreviewChangeHistoriesPromise = null;
        this._conversationMessageLinkPreviewChangeHistoriesSubject.next(this._conversationMessageLinkPreviewChangeHistories);      // Emit to observable
    }

    public get HasConversationMessageLinkPreviewChangeHistories(): Promise<boolean> {
        return this.ConversationMessageLinkPreviewChangeHistories.then(conversationMessageLinkPreviewChangeHistories => conversationMessageLinkPreviewChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (conversationMessageLinkPreview.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await conversationMessageLinkPreview.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ConversationMessageLinkPreviewData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ConversationMessageLinkPreviewData>> {
        const info = await lastValueFrom(
            ConversationMessageLinkPreviewService.Instance.GetConversationMessageLinkPreviewChangeMetadata(this.id, this.versionNumber as number)
        );
        this._currentVersionInfo = info;
        this._currentVersionInfoSubject.next(info);
        return info;
    }


    public ClearCurrentVersionInfoCache(): void {
        this._currentVersionInfo = null;
        this._currentVersionInfoPromise = null;
        this._currentVersionInfoSubject.next(null);
    }



    /**
     * Updates the state of this ConversationMessageLinkPreviewData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ConversationMessageLinkPreviewData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ConversationMessageLinkPreviewSubmitData {
        return ConversationMessageLinkPreviewService.Instance.ConvertToConversationMessageLinkPreviewSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ConversationMessageLinkPreviewService extends SecureEndpointBase {

    private static _instance: ConversationMessageLinkPreviewService;
    private listCache: Map<string, Observable<Array<ConversationMessageLinkPreviewData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ConversationMessageLinkPreviewBasicListData>>>;
    private recordCache: Map<string, Observable<ConversationMessageLinkPreviewData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private conversationMessageLinkPreviewChangeHistoryService: ConversationMessageLinkPreviewChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ConversationMessageLinkPreviewData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ConversationMessageLinkPreviewBasicListData>>>();
        this.recordCache = new Map<string, Observable<ConversationMessageLinkPreviewData>>();

        ConversationMessageLinkPreviewService._instance = this;
    }

    public static get Instance(): ConversationMessageLinkPreviewService {
      return ConversationMessageLinkPreviewService._instance;
    }


    public ClearListCaches(config: ConversationMessageLinkPreviewQueryParameters | null = null) {

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


    public ConvertToConversationMessageLinkPreviewSubmitData(data: ConversationMessageLinkPreviewData): ConversationMessageLinkPreviewSubmitData {

        let output = new ConversationMessageLinkPreviewSubmitData();

        output.id = data.id;
        output.conversationMessageId = data.conversationMessageId;
        output.url = data.url;
        output.title = data.title;
        output.description = data.description;
        output.imageUrl = data.imageUrl;
        output.siteName = data.siteName;
        output.fetchedDateTime = data.fetchedDateTime;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetConversationMessageLinkPreview(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationMessageLinkPreviewData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const conversationMessageLinkPreview$ = this.requestConversationMessageLinkPreview(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessageLinkPreview", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, conversationMessageLinkPreview$);

            return conversationMessageLinkPreview$;
        }

        return this.recordCache.get(configHash) as Observable<ConversationMessageLinkPreviewData>;
    }

    private requestConversationMessageLinkPreview(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationMessageLinkPreviewData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationMessageLinkPreviewData>(this.baseUrl + 'api/ConversationMessageLinkPreview/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveConversationMessageLinkPreview(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageLinkPreview(id, includeRelations));
            }));
    }

    public GetConversationMessageLinkPreviewList(config: ConversationMessageLinkPreviewQueryParameters | any = null) : Observable<Array<ConversationMessageLinkPreviewData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const conversationMessageLinkPreviewList$ = this.requestConversationMessageLinkPreviewList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessageLinkPreview list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, conversationMessageLinkPreviewList$);

            return conversationMessageLinkPreviewList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ConversationMessageLinkPreviewData>>;
    }


    private requestConversationMessageLinkPreviewList(config: ConversationMessageLinkPreviewQueryParameters | any) : Observable <Array<ConversationMessageLinkPreviewData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationMessageLinkPreviewData>>(this.baseUrl + 'api/ConversationMessageLinkPreviews', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveConversationMessageLinkPreviewList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageLinkPreviewList(config));
            }));
    }

    public GetConversationMessageLinkPreviewsRowCount(config: ConversationMessageLinkPreviewQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const conversationMessageLinkPreviewsRowCount$ = this.requestConversationMessageLinkPreviewsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessageLinkPreviews row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, conversationMessageLinkPreviewsRowCount$);

            return conversationMessageLinkPreviewsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestConversationMessageLinkPreviewsRowCount(config: ConversationMessageLinkPreviewQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ConversationMessageLinkPreviews/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageLinkPreviewsRowCount(config));
            }));
    }

    public GetConversationMessageLinkPreviewsBasicListData(config: ConversationMessageLinkPreviewQueryParameters | any = null) : Observable<Array<ConversationMessageLinkPreviewBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const conversationMessageLinkPreviewsBasicListData$ = this.requestConversationMessageLinkPreviewsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessageLinkPreviews basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, conversationMessageLinkPreviewsBasicListData$);

            return conversationMessageLinkPreviewsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ConversationMessageLinkPreviewBasicListData>>;
    }


    private requestConversationMessageLinkPreviewsBasicListData(config: ConversationMessageLinkPreviewQueryParameters | any) : Observable<Array<ConversationMessageLinkPreviewBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationMessageLinkPreviewBasicListData>>(this.baseUrl + 'api/ConversationMessageLinkPreviews/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageLinkPreviewsBasicListData(config));
            }));

    }


    public PutConversationMessageLinkPreview(id: bigint | number, conversationMessageLinkPreview: ConversationMessageLinkPreviewSubmitData) : Observable<ConversationMessageLinkPreviewData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConversationMessageLinkPreviewData>(this.baseUrl + 'api/ConversationMessageLinkPreview/' + id.toString(), conversationMessageLinkPreview, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationMessageLinkPreview(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutConversationMessageLinkPreview(id, conversationMessageLinkPreview));
            }));
    }


    public PostConversationMessageLinkPreview(conversationMessageLinkPreview: ConversationMessageLinkPreviewSubmitData) : Observable<ConversationMessageLinkPreviewData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ConversationMessageLinkPreviewData>(this.baseUrl + 'api/ConversationMessageLinkPreview', conversationMessageLinkPreview, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationMessageLinkPreview(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostConversationMessageLinkPreview(conversationMessageLinkPreview));
            }));
    }

  
    public DeleteConversationMessageLinkPreview(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ConversationMessageLinkPreview/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteConversationMessageLinkPreview(id));
            }));
    }

    public RollbackConversationMessageLinkPreview(id: bigint | number, versionNumber: bigint | number) : Observable<ConversationMessageLinkPreviewData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConversationMessageLinkPreviewData>(this.baseUrl + 'api/ConversationMessageLinkPreview/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationMessageLinkPreview(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackConversationMessageLinkPreview(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ConversationMessageLinkPreview.
     */
    public GetConversationMessageLinkPreviewChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ConversationMessageLinkPreviewData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ConversationMessageLinkPreviewData>>(this.baseUrl + 'api/ConversationMessageLinkPreview/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetConversationMessageLinkPreviewChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ConversationMessageLinkPreview.
     */
    public GetConversationMessageLinkPreviewAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ConversationMessageLinkPreviewData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ConversationMessageLinkPreviewData>[]>(this.baseUrl + 'api/ConversationMessageLinkPreview/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetConversationMessageLinkPreviewAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ConversationMessageLinkPreview.
     */
    public GetConversationMessageLinkPreviewVersion(id: bigint | number, version: number): Observable<ConversationMessageLinkPreviewData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationMessageLinkPreviewData>(this.baseUrl + 'api/ConversationMessageLinkPreview/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveConversationMessageLinkPreview(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetConversationMessageLinkPreviewVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ConversationMessageLinkPreview at a specific point in time.
     */
    public GetConversationMessageLinkPreviewStateAtTime(id: bigint | number, time: string): Observable<ConversationMessageLinkPreviewData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationMessageLinkPreviewData>(this.baseUrl + 'api/ConversationMessageLinkPreview/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveConversationMessageLinkPreview(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetConversationMessageLinkPreviewStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ConversationMessageLinkPreviewQueryParameters | any): string {

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

    public userIsSchedulerConversationMessageLinkPreviewReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerConversationMessageLinkPreviewReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ConversationMessageLinkPreviews
        //
        if (userIsSchedulerConversationMessageLinkPreviewReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerConversationMessageLinkPreviewReader = user.readPermission >= 50;
            } else {
                userIsSchedulerConversationMessageLinkPreviewReader = false;
            }
        }

        return userIsSchedulerConversationMessageLinkPreviewReader;
    }


    public userIsSchedulerConversationMessageLinkPreviewWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerConversationMessageLinkPreviewWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ConversationMessageLinkPreviews
        //
        if (userIsSchedulerConversationMessageLinkPreviewWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerConversationMessageLinkPreviewWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerConversationMessageLinkPreviewWriter = false;
          }      
        }

        return userIsSchedulerConversationMessageLinkPreviewWriter;
    }

    public GetConversationMessageLinkPreviewChangeHistoriesForConversationMessageLinkPreview(conversationMessageLinkPreviewId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationMessageLinkPreviewChangeHistoryData[]> {
        return this.conversationMessageLinkPreviewChangeHistoryService.GetConversationMessageLinkPreviewChangeHistoryList({
            conversationMessageLinkPreviewId: conversationMessageLinkPreviewId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ConversationMessageLinkPreviewData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ConversationMessageLinkPreviewData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ConversationMessageLinkPreviewTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveConversationMessageLinkPreview(raw: any): ConversationMessageLinkPreviewData {
    if (!raw) return raw;

    //
    // Create a ConversationMessageLinkPreviewData object instance with correct prototype
    //
    const revived = Object.create(ConversationMessageLinkPreviewData.prototype) as ConversationMessageLinkPreviewData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._conversationMessageLinkPreviewChangeHistories = null;
    (revived as any)._conversationMessageLinkPreviewChangeHistoriesPromise = null;
    (revived as any)._conversationMessageLinkPreviewChangeHistoriesSubject = new BehaviorSubject<ConversationMessageLinkPreviewChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadConversationMessageLinkPreviewXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ConversationMessageLinkPreviewChangeHistories$ = (revived as any)._conversationMessageLinkPreviewChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationMessageLinkPreviewChangeHistories === null && (revived as any)._conversationMessageLinkPreviewChangeHistoriesPromise === null) {
                (revived as any).loadConversationMessageLinkPreviewChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationMessageLinkPreviewChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ConversationMessageLinkPreviewData> | null>(null);

    (revived as any).CurrentVersionInfo$ = (revived as any)._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if ((revived as any)._currentVersionInfo === null && (revived as any)._currentVersionInfoPromise === null) {
                (revived as any).loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    return revived;
  }

  private ReviveConversationMessageLinkPreviewList(rawList: any[]): ConversationMessageLinkPreviewData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveConversationMessageLinkPreview(raw));
  }

}
