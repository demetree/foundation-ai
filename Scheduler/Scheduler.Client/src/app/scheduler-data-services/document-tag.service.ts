/*

   GENERATED SERVICE FOR THE DOCUMENTTAG TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the DocumentTag table.

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
import { DocumentTagChangeHistoryService, DocumentTagChangeHistoryData } from './document-tag-change-history.service';
import { DocumentDocumentTagService, DocumentDocumentTagData } from './document-document-tag.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class DocumentTagQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    color: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class DocumentTagSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    color: string | null = null;
    sequence: bigint | number | null = null;
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

export class DocumentTagBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. DocumentTagChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `documentTag.DocumentTagChildren$` — use with `| async` in templates
//        • Promise:    `documentTag.DocumentTagChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="documentTag.DocumentTagChildren$ | async"`), or
//        • Access the promise getter (`documentTag.DocumentTagChildren` or `await documentTag.DocumentTagChildren`)
//    - Simply reading `documentTag.DocumentTagChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await documentTag.Reload()` to refresh the entire object and clear all lazy caches.
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
export class DocumentTagData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    color!: string | null;
    sequence!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _documentTagChangeHistories: DocumentTagChangeHistoryData[] | null = null;
    private _documentTagChangeHistoriesPromise: Promise<DocumentTagChangeHistoryData[]> | null  = null;
    private _documentTagChangeHistoriesSubject = new BehaviorSubject<DocumentTagChangeHistoryData[] | null>(null);

                
    private _documentDocumentTags: DocumentDocumentTagData[] | null = null;
    private _documentDocumentTagsPromise: Promise<DocumentDocumentTagData[]> | null  = null;
    private _documentDocumentTagsSubject = new BehaviorSubject<DocumentDocumentTagData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<DocumentTagData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<DocumentTagData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<DocumentTagData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public DocumentTagChangeHistories$ = this._documentTagChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._documentTagChangeHistories === null && this._documentTagChangeHistoriesPromise === null) {
            this.loadDocumentTagChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _documentTagChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get DocumentTagChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._documentTagChangeHistoriesCount$ === null) {
            this._documentTagChangeHistoriesCount$ = DocumentTagChangeHistoryService.Instance.GetDocumentTagChangeHistoriesRowCount({documentTagId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._documentTagChangeHistoriesCount$;
    }



    public DocumentDocumentTags$ = this._documentDocumentTagsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._documentDocumentTags === null && this._documentDocumentTagsPromise === null) {
            this.loadDocumentDocumentTags(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _documentDocumentTagsCount$: Observable<bigint | number> | null = null;
    public get DocumentDocumentTagsCount$(): Observable<bigint | number> {
        if (this._documentDocumentTagsCount$ === null) {
            this._documentDocumentTagsCount$ = DocumentDocumentTagService.Instance.GetDocumentDocumentTagsRowCount({documentTagId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._documentDocumentTagsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any DocumentTagData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.documentTag.Reload();
  //
  //  Non Async:
  //
  //     documentTag[0].Reload().then(x => {
  //        this.documentTag = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      DocumentTagService.Instance.GetDocumentTag(this.id, includeRelations)
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
     this._documentTagChangeHistories = null;
     this._documentTagChangeHistoriesPromise = null;
     this._documentTagChangeHistoriesSubject.next(null);
     this._documentTagChangeHistoriesCount$ = null;

     this._documentDocumentTags = null;
     this._documentDocumentTagsPromise = null;
     this._documentDocumentTagsSubject.next(null);
     this._documentDocumentTagsCount$ = null;

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
     * Gets the DocumentTagChangeHistories for this DocumentTag.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.documentTag.DocumentTagChangeHistories.then(documentTags => { ... })
     *   or
     *   await this.documentTag.documentTags
     *
    */
    public get DocumentTagChangeHistories(): Promise<DocumentTagChangeHistoryData[]> {
        if (this._documentTagChangeHistories !== null) {
            return Promise.resolve(this._documentTagChangeHistories);
        }

        if (this._documentTagChangeHistoriesPromise !== null) {
            return this._documentTagChangeHistoriesPromise;
        }

        // Start the load
        this.loadDocumentTagChangeHistories();

        return this._documentTagChangeHistoriesPromise!;
    }



    private loadDocumentTagChangeHistories(): void {

        this._documentTagChangeHistoriesPromise = lastValueFrom(
            DocumentTagService.Instance.GetDocumentTagChangeHistoriesForDocumentTag(this.id)
        )
        .then(DocumentTagChangeHistories => {
            this._documentTagChangeHistories = DocumentTagChangeHistories ?? [];
            this._documentTagChangeHistoriesSubject.next(this._documentTagChangeHistories);
            return this._documentTagChangeHistories;
         })
        .catch(err => {
            this._documentTagChangeHistories = [];
            this._documentTagChangeHistoriesSubject.next(this._documentTagChangeHistories);
            throw err;
        })
        .finally(() => {
            this._documentTagChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached DocumentTagChangeHistory. Call after mutations to force refresh.
     */
    public ClearDocumentTagChangeHistoriesCache(): void {
        this._documentTagChangeHistories = null;
        this._documentTagChangeHistoriesPromise = null;
        this._documentTagChangeHistoriesSubject.next(this._documentTagChangeHistories);      // Emit to observable
    }

    public get HasDocumentTagChangeHistories(): Promise<boolean> {
        return this.DocumentTagChangeHistories.then(documentTagChangeHistories => documentTagChangeHistories.length > 0);
    }


    /**
     *
     * Gets the DocumentDocumentTags for this DocumentTag.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.documentTag.DocumentDocumentTags.then(documentTags => { ... })
     *   or
     *   await this.documentTag.documentTags
     *
    */
    public get DocumentDocumentTags(): Promise<DocumentDocumentTagData[]> {
        if (this._documentDocumentTags !== null) {
            return Promise.resolve(this._documentDocumentTags);
        }

        if (this._documentDocumentTagsPromise !== null) {
            return this._documentDocumentTagsPromise;
        }

        // Start the load
        this.loadDocumentDocumentTags();

        return this._documentDocumentTagsPromise!;
    }



    private loadDocumentDocumentTags(): void {

        this._documentDocumentTagsPromise = lastValueFrom(
            DocumentTagService.Instance.GetDocumentDocumentTagsForDocumentTag(this.id)
        )
        .then(DocumentDocumentTags => {
            this._documentDocumentTags = DocumentDocumentTags ?? [];
            this._documentDocumentTagsSubject.next(this._documentDocumentTags);
            return this._documentDocumentTags;
         })
        .catch(err => {
            this._documentDocumentTags = [];
            this._documentDocumentTagsSubject.next(this._documentDocumentTags);
            throw err;
        })
        .finally(() => {
            this._documentDocumentTagsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached DocumentDocumentTag. Call after mutations to force refresh.
     */
    public ClearDocumentDocumentTagsCache(): void {
        this._documentDocumentTags = null;
        this._documentDocumentTagsPromise = null;
        this._documentDocumentTagsSubject.next(this._documentDocumentTags);      // Emit to observable
    }

    public get HasDocumentDocumentTags(): Promise<boolean> {
        return this.DocumentDocumentTags.then(documentDocumentTags => documentDocumentTags.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (documentTag.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await documentTag.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<DocumentTagData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<DocumentTagData>> {
        const info = await lastValueFrom(
            DocumentTagService.Instance.GetDocumentTagChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this DocumentTagData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this DocumentTagData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): DocumentTagSubmitData {
        return DocumentTagService.Instance.ConvertToDocumentTagSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class DocumentTagService extends SecureEndpointBase {

    private static _instance: DocumentTagService;
    private listCache: Map<string, Observable<Array<DocumentTagData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<DocumentTagBasicListData>>>;
    private recordCache: Map<string, Observable<DocumentTagData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private documentTagChangeHistoryService: DocumentTagChangeHistoryService,
        private documentDocumentTagService: DocumentDocumentTagService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<DocumentTagData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<DocumentTagBasicListData>>>();
        this.recordCache = new Map<string, Observable<DocumentTagData>>();

        DocumentTagService._instance = this;
    }

    public static get Instance(): DocumentTagService {
      return DocumentTagService._instance;
    }


    public ClearListCaches(config: DocumentTagQueryParameters | null = null) {

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


    public ConvertToDocumentTagSubmitData(data: DocumentTagData): DocumentTagSubmitData {

        let output = new DocumentTagSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.color = data.color;
        output.sequence = data.sequence;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetDocumentTag(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentTagData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const documentTag$ = this.requestDocumentTag(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentTag", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, documentTag$);

            return documentTag$;
        }

        return this.recordCache.get(configHash) as Observable<DocumentTagData>;
    }

    private requestDocumentTag(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentTagData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentTagData>(this.baseUrl + 'api/DocumentTag/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveDocumentTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentTag(id, includeRelations));
            }));
    }

    public GetDocumentTagList(config: DocumentTagQueryParameters | any = null) : Observable<Array<DocumentTagData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const documentTagList$ = this.requestDocumentTagList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentTag list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, documentTagList$);

            return documentTagList$;
        }

        return this.listCache.get(configHash) as Observable<Array<DocumentTagData>>;
    }


    private requestDocumentTagList(config: DocumentTagQueryParameters | any) : Observable <Array<DocumentTagData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentTagData>>(this.baseUrl + 'api/DocumentTags', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveDocumentTagList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentTagList(config));
            }));
    }

    public GetDocumentTagsRowCount(config: DocumentTagQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const documentTagsRowCount$ = this.requestDocumentTagsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentTags row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, documentTagsRowCount$);

            return documentTagsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestDocumentTagsRowCount(config: DocumentTagQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/DocumentTags/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentTagsRowCount(config));
            }));
    }

    public GetDocumentTagsBasicListData(config: DocumentTagQueryParameters | any = null) : Observable<Array<DocumentTagBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const documentTagsBasicListData$ = this.requestDocumentTagsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentTags basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, documentTagsBasicListData$);

            return documentTagsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<DocumentTagBasicListData>>;
    }


    private requestDocumentTagsBasicListData(config: DocumentTagQueryParameters | any) : Observable<Array<DocumentTagBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentTagBasicListData>>(this.baseUrl + 'api/DocumentTags/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentTagsBasicListData(config));
            }));

    }


    public PutDocumentTag(id: bigint | number, documentTag: DocumentTagSubmitData) : Observable<DocumentTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentTagData>(this.baseUrl + 'api/DocumentTag/' + id.toString(), documentTag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutDocumentTag(id, documentTag));
            }));
    }


    public PostDocumentTag(documentTag: DocumentTagSubmitData) : Observable<DocumentTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<DocumentTagData>(this.baseUrl + 'api/DocumentTag', documentTag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentTag(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostDocumentTag(documentTag));
            }));
    }

  
    public DeleteDocumentTag(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/DocumentTag/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteDocumentTag(id));
            }));
    }

    public RollbackDocumentTag(id: bigint | number, versionNumber: bigint | number) : Observable<DocumentTagData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentTagData>(this.baseUrl + 'api/DocumentTag/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackDocumentTag(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a DocumentTag.
     */
    public GetDocumentTagChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<DocumentTagData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<DocumentTagData>>(this.baseUrl + 'api/DocumentTag/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentTagChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a DocumentTag.
     */
    public GetDocumentTagAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<DocumentTagData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<DocumentTagData>[]>(this.baseUrl + 'api/DocumentTag/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentTagAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a DocumentTag.
     */
    public GetDocumentTagVersion(id: bigint | number, version: number): Observable<DocumentTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentTagData>(this.baseUrl + 'api/DocumentTag/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveDocumentTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentTagVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a DocumentTag at a specific point in time.
     */
    public GetDocumentTagStateAtTime(id: bigint | number, time: string): Observable<DocumentTagData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentTagData>(this.baseUrl + 'api/DocumentTag/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveDocumentTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentTagStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: DocumentTagQueryParameters | any): string {

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

    public userIsSchedulerDocumentTagReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerDocumentTagReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.DocumentTags
        //
        if (userIsSchedulerDocumentTagReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerDocumentTagReader = user.readPermission >= 1;
            } else {
                userIsSchedulerDocumentTagReader = false;
            }
        }

        return userIsSchedulerDocumentTagReader;
    }


    public userIsSchedulerDocumentTagWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerDocumentTagWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.DocumentTags
        //
        if (userIsSchedulerDocumentTagWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerDocumentTagWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerDocumentTagWriter = false;
          }      
        }

        return userIsSchedulerDocumentTagWriter;
    }

    public GetDocumentTagChangeHistoriesForDocumentTag(documentTagId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<DocumentTagChangeHistoryData[]> {
        return this.documentTagChangeHistoryService.GetDocumentTagChangeHistoryList({
            documentTagId: documentTagId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetDocumentDocumentTagsForDocumentTag(documentTagId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<DocumentDocumentTagData[]> {
        return this.documentDocumentTagService.GetDocumentDocumentTagList({
            documentTagId: documentTagId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full DocumentTagData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the DocumentTagData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when DocumentTagTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveDocumentTag(raw: any): DocumentTagData {
    if (!raw) return raw;

    //
    // Create a DocumentTagData object instance with correct prototype
    //
    const revived = Object.create(DocumentTagData.prototype) as DocumentTagData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._documentTagChangeHistories = null;
    (revived as any)._documentTagChangeHistoriesPromise = null;
    (revived as any)._documentTagChangeHistoriesSubject = new BehaviorSubject<DocumentTagChangeHistoryData[] | null>(null);

    (revived as any)._documentDocumentTags = null;
    (revived as any)._documentDocumentTagsPromise = null;
    (revived as any)._documentDocumentTagsSubject = new BehaviorSubject<DocumentDocumentTagData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadDocumentTagXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).DocumentTagChangeHistories$ = (revived as any)._documentTagChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._documentTagChangeHistories === null && (revived as any)._documentTagChangeHistoriesPromise === null) {
                (revived as any).loadDocumentTagChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._documentTagChangeHistoriesCount$ = null;


    (revived as any).DocumentDocumentTags$ = (revived as any)._documentDocumentTagsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._documentDocumentTags === null && (revived as any)._documentDocumentTagsPromise === null) {
                (revived as any).loadDocumentDocumentTags();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._documentDocumentTagsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<DocumentTagData> | null>(null);

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

  private ReviveDocumentTagList(rawList: any[]): DocumentTagData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveDocumentTag(raw));
  }

}
