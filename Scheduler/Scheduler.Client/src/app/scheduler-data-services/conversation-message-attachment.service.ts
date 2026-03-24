/*

   GENERATED SERVICE FOR THE CONVERSATIONMESSAGEATTACHMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ConversationMessageAttachment table.

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
import { ConversationMessageAttachmentChangeHistoryService, ConversationMessageAttachmentChangeHistoryData } from './conversation-message-attachment-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ConversationMessageAttachmentQueryParameters {
    conversationMessageId: bigint | number | null | undefined = null;
    userId: bigint | number | null | undefined = null;
    dateTimeCreated: string | null | undefined = null;        // ISO 8601 (full datetime)
    contentFileName: string | null | undefined = null;
    contentSize: bigint | number | null | undefined = null;
    contentMimeType: string | null | undefined = null;
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
export class ConversationMessageAttachmentSubmitData {
    id!: bigint | number;
    conversationMessageId!: bigint | number;
    userId!: bigint | number;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    contentFileName!: string;
    contentSize!: bigint | number;
    contentData!: string;
    contentMimeType!: string;
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

export class ConversationMessageAttachmentBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ConversationMessageAttachmentChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `conversationMessageAttachment.ConversationMessageAttachmentChildren$` — use with `| async` in templates
//        • Promise:    `conversationMessageAttachment.ConversationMessageAttachmentChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="conversationMessageAttachment.ConversationMessageAttachmentChildren$ | async"`), or
//        • Access the promise getter (`conversationMessageAttachment.ConversationMessageAttachmentChildren` or `await conversationMessageAttachment.ConversationMessageAttachmentChildren`)
//    - Simply reading `conversationMessageAttachment.ConversationMessageAttachmentChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await conversationMessageAttachment.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ConversationMessageAttachmentData {
    id!: bigint | number;
    conversationMessageId!: bigint | number;
    userId!: bigint | number;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    contentFileName!: string;
    contentSize!: bigint | number;
    contentData!: string;
    contentMimeType!: string;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    conversationMessage: ConversationMessageData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _conversationMessageAttachmentChangeHistories: ConversationMessageAttachmentChangeHistoryData[] | null = null;
    private _conversationMessageAttachmentChangeHistoriesPromise: Promise<ConversationMessageAttachmentChangeHistoryData[]> | null  = null;
    private _conversationMessageAttachmentChangeHistoriesSubject = new BehaviorSubject<ConversationMessageAttachmentChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ConversationMessageAttachmentData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ConversationMessageAttachmentData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ConversationMessageAttachmentData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ConversationMessageAttachmentChangeHistories$ = this._conversationMessageAttachmentChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationMessageAttachmentChangeHistories === null && this._conversationMessageAttachmentChangeHistoriesPromise === null) {
            this.loadConversationMessageAttachmentChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationMessageAttachmentChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ConversationMessageAttachmentChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._conversationMessageAttachmentChangeHistoriesCount$ === null) {
            this._conversationMessageAttachmentChangeHistoriesCount$ = ConversationMessageAttachmentChangeHistoryService.Instance.GetConversationMessageAttachmentChangeHistoriesRowCount({conversationMessageAttachmentId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationMessageAttachmentChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ConversationMessageAttachmentData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.conversationMessageAttachment.Reload();
  //
  //  Non Async:
  //
  //     conversationMessageAttachment[0].Reload().then(x => {
  //        this.conversationMessageAttachment = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ConversationMessageAttachmentService.Instance.GetConversationMessageAttachment(this.id, includeRelations)
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
     this._conversationMessageAttachmentChangeHistories = null;
     this._conversationMessageAttachmentChangeHistoriesPromise = null;
     this._conversationMessageAttachmentChangeHistoriesSubject.next(null);
     this._conversationMessageAttachmentChangeHistoriesCount$ = null;

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
     * Gets the ConversationMessageAttachmentChangeHistories for this ConversationMessageAttachment.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationMessageAttachment.ConversationMessageAttachmentChangeHistories.then(conversationMessageAttachments => { ... })
     *   or
     *   await this.conversationMessageAttachment.conversationMessageAttachments
     *
    */
    public get ConversationMessageAttachmentChangeHistories(): Promise<ConversationMessageAttachmentChangeHistoryData[]> {
        if (this._conversationMessageAttachmentChangeHistories !== null) {
            return Promise.resolve(this._conversationMessageAttachmentChangeHistories);
        }

        if (this._conversationMessageAttachmentChangeHistoriesPromise !== null) {
            return this._conversationMessageAttachmentChangeHistoriesPromise;
        }

        // Start the load
        this.loadConversationMessageAttachmentChangeHistories();

        return this._conversationMessageAttachmentChangeHistoriesPromise!;
    }



    private loadConversationMessageAttachmentChangeHistories(): void {

        this._conversationMessageAttachmentChangeHistoriesPromise = lastValueFrom(
            ConversationMessageAttachmentService.Instance.GetConversationMessageAttachmentChangeHistoriesForConversationMessageAttachment(this.id)
        )
        .then(ConversationMessageAttachmentChangeHistories => {
            this._conversationMessageAttachmentChangeHistories = ConversationMessageAttachmentChangeHistories ?? [];
            this._conversationMessageAttachmentChangeHistoriesSubject.next(this._conversationMessageAttachmentChangeHistories);
            return this._conversationMessageAttachmentChangeHistories;
         })
        .catch(err => {
            this._conversationMessageAttachmentChangeHistories = [];
            this._conversationMessageAttachmentChangeHistoriesSubject.next(this._conversationMessageAttachmentChangeHistories);
            throw err;
        })
        .finally(() => {
            this._conversationMessageAttachmentChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationMessageAttachmentChangeHistory. Call after mutations to force refresh.
     */
    public ClearConversationMessageAttachmentChangeHistoriesCache(): void {
        this._conversationMessageAttachmentChangeHistories = null;
        this._conversationMessageAttachmentChangeHistoriesPromise = null;
        this._conversationMessageAttachmentChangeHistoriesSubject.next(this._conversationMessageAttachmentChangeHistories);      // Emit to observable
    }

    public get HasConversationMessageAttachmentChangeHistories(): Promise<boolean> {
        return this.ConversationMessageAttachmentChangeHistories.then(conversationMessageAttachmentChangeHistories => conversationMessageAttachmentChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (conversationMessageAttachment.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await conversationMessageAttachment.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ConversationMessageAttachmentData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ConversationMessageAttachmentData>> {
        const info = await lastValueFrom(
            ConversationMessageAttachmentService.Instance.GetConversationMessageAttachmentChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ConversationMessageAttachmentData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ConversationMessageAttachmentData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ConversationMessageAttachmentSubmitData {
        return ConversationMessageAttachmentService.Instance.ConvertToConversationMessageAttachmentSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ConversationMessageAttachmentService extends SecureEndpointBase {

    private static _instance: ConversationMessageAttachmentService;
    private listCache: Map<string, Observable<Array<ConversationMessageAttachmentData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ConversationMessageAttachmentBasicListData>>>;
    private recordCache: Map<string, Observable<ConversationMessageAttachmentData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private conversationMessageAttachmentChangeHistoryService: ConversationMessageAttachmentChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ConversationMessageAttachmentData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ConversationMessageAttachmentBasicListData>>>();
        this.recordCache = new Map<string, Observable<ConversationMessageAttachmentData>>();

        ConversationMessageAttachmentService._instance = this;
    }

    public static get Instance(): ConversationMessageAttachmentService {
      return ConversationMessageAttachmentService._instance;
    }


    public ClearListCaches(config: ConversationMessageAttachmentQueryParameters | null = null) {

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


    public ConvertToConversationMessageAttachmentSubmitData(data: ConversationMessageAttachmentData): ConversationMessageAttachmentSubmitData {

        let output = new ConversationMessageAttachmentSubmitData();

        output.id = data.id;
        output.conversationMessageId = data.conversationMessageId;
        output.userId = data.userId;
        output.dateTimeCreated = data.dateTimeCreated;
        output.contentFileName = data.contentFileName;
        output.contentSize = data.contentSize;
        output.contentData = data.contentData;
        output.contentMimeType = data.contentMimeType;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetConversationMessageAttachment(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationMessageAttachmentData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const conversationMessageAttachment$ = this.requestConversationMessageAttachment(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessageAttachment", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, conversationMessageAttachment$);

            return conversationMessageAttachment$;
        }

        return this.recordCache.get(configHash) as Observable<ConversationMessageAttachmentData>;
    }

    private requestConversationMessageAttachment(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationMessageAttachmentData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationMessageAttachmentData>(this.baseUrl + 'api/ConversationMessageAttachment/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveConversationMessageAttachment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageAttachment(id, includeRelations));
            }));
    }

    public GetConversationMessageAttachmentList(config: ConversationMessageAttachmentQueryParameters | any = null) : Observable<Array<ConversationMessageAttachmentData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const conversationMessageAttachmentList$ = this.requestConversationMessageAttachmentList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessageAttachment list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, conversationMessageAttachmentList$);

            return conversationMessageAttachmentList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ConversationMessageAttachmentData>>;
    }


    private requestConversationMessageAttachmentList(config: ConversationMessageAttachmentQueryParameters | any) : Observable <Array<ConversationMessageAttachmentData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationMessageAttachmentData>>(this.baseUrl + 'api/ConversationMessageAttachments', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveConversationMessageAttachmentList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageAttachmentList(config));
            }));
    }

    public GetConversationMessageAttachmentsRowCount(config: ConversationMessageAttachmentQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const conversationMessageAttachmentsRowCount$ = this.requestConversationMessageAttachmentsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessageAttachments row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, conversationMessageAttachmentsRowCount$);

            return conversationMessageAttachmentsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestConversationMessageAttachmentsRowCount(config: ConversationMessageAttachmentQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ConversationMessageAttachments/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageAttachmentsRowCount(config));
            }));
    }

    public GetConversationMessageAttachmentsBasicListData(config: ConversationMessageAttachmentQueryParameters | any = null) : Observable<Array<ConversationMessageAttachmentBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const conversationMessageAttachmentsBasicListData$ = this.requestConversationMessageAttachmentsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessageAttachments basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, conversationMessageAttachmentsBasicListData$);

            return conversationMessageAttachmentsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ConversationMessageAttachmentBasicListData>>;
    }


    private requestConversationMessageAttachmentsBasicListData(config: ConversationMessageAttachmentQueryParameters | any) : Observable<Array<ConversationMessageAttachmentBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationMessageAttachmentBasicListData>>(this.baseUrl + 'api/ConversationMessageAttachments/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageAttachmentsBasicListData(config));
            }));

    }


    public PutConversationMessageAttachment(id: bigint | number, conversationMessageAttachment: ConversationMessageAttachmentSubmitData) : Observable<ConversationMessageAttachmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConversationMessageAttachmentData>(this.baseUrl + 'api/ConversationMessageAttachment/' + id.toString(), conversationMessageAttachment, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationMessageAttachment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutConversationMessageAttachment(id, conversationMessageAttachment));
            }));
    }


    public PostConversationMessageAttachment(conversationMessageAttachment: ConversationMessageAttachmentSubmitData) : Observable<ConversationMessageAttachmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ConversationMessageAttachmentData>(this.baseUrl + 'api/ConversationMessageAttachment', conversationMessageAttachment, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationMessageAttachment(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostConversationMessageAttachment(conversationMessageAttachment));
            }));
    }

  
    public DeleteConversationMessageAttachment(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ConversationMessageAttachment/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteConversationMessageAttachment(id));
            }));
    }

    public RollbackConversationMessageAttachment(id: bigint | number, versionNumber: bigint | number) : Observable<ConversationMessageAttachmentData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConversationMessageAttachmentData>(this.baseUrl + 'api/ConversationMessageAttachment/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationMessageAttachment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackConversationMessageAttachment(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ConversationMessageAttachment.
     */
    public GetConversationMessageAttachmentChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ConversationMessageAttachmentData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ConversationMessageAttachmentData>>(this.baseUrl + 'api/ConversationMessageAttachment/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetConversationMessageAttachmentChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ConversationMessageAttachment.
     */
    public GetConversationMessageAttachmentAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ConversationMessageAttachmentData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ConversationMessageAttachmentData>[]>(this.baseUrl + 'api/ConversationMessageAttachment/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetConversationMessageAttachmentAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ConversationMessageAttachment.
     */
    public GetConversationMessageAttachmentVersion(id: bigint | number, version: number): Observable<ConversationMessageAttachmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationMessageAttachmentData>(this.baseUrl + 'api/ConversationMessageAttachment/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveConversationMessageAttachment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetConversationMessageAttachmentVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ConversationMessageAttachment at a specific point in time.
     */
    public GetConversationMessageAttachmentStateAtTime(id: bigint | number, time: string): Observable<ConversationMessageAttachmentData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationMessageAttachmentData>(this.baseUrl + 'api/ConversationMessageAttachment/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveConversationMessageAttachment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetConversationMessageAttachmentStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ConversationMessageAttachmentQueryParameters | any): string {

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

    public userIsSchedulerConversationMessageAttachmentReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerConversationMessageAttachmentReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ConversationMessageAttachments
        //
        if (userIsSchedulerConversationMessageAttachmentReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerConversationMessageAttachmentReader = user.readPermission >= 50;
            } else {
                userIsSchedulerConversationMessageAttachmentReader = false;
            }
        }

        return userIsSchedulerConversationMessageAttachmentReader;
    }


    public userIsSchedulerConversationMessageAttachmentWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerConversationMessageAttachmentWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ConversationMessageAttachments
        //
        if (userIsSchedulerConversationMessageAttachmentWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerConversationMessageAttachmentWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerConversationMessageAttachmentWriter = false;
          }      
        }

        return userIsSchedulerConversationMessageAttachmentWriter;
    }

    public GetConversationMessageAttachmentChangeHistoriesForConversationMessageAttachment(conversationMessageAttachmentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationMessageAttachmentChangeHistoryData[]> {
        return this.conversationMessageAttachmentChangeHistoryService.GetConversationMessageAttachmentChangeHistoryList({
            conversationMessageAttachmentId: conversationMessageAttachmentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ConversationMessageAttachmentData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ConversationMessageAttachmentData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ConversationMessageAttachmentTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveConversationMessageAttachment(raw: any): ConversationMessageAttachmentData {
    if (!raw) return raw;

    //
    // Create a ConversationMessageAttachmentData object instance with correct prototype
    //
    const revived = Object.create(ConversationMessageAttachmentData.prototype) as ConversationMessageAttachmentData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._conversationMessageAttachmentChangeHistories = null;
    (revived as any)._conversationMessageAttachmentChangeHistoriesPromise = null;
    (revived as any)._conversationMessageAttachmentChangeHistoriesSubject = new BehaviorSubject<ConversationMessageAttachmentChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadConversationMessageAttachmentXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ConversationMessageAttachmentChangeHistories$ = (revived as any)._conversationMessageAttachmentChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationMessageAttachmentChangeHistories === null && (revived as any)._conversationMessageAttachmentChangeHistoriesPromise === null) {
                (revived as any).loadConversationMessageAttachmentChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationMessageAttachmentChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ConversationMessageAttachmentData> | null>(null);

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

  private ReviveConversationMessageAttachmentList(rawList: any[]): ConversationMessageAttachmentData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveConversationMessageAttachment(raw));
  }

}
