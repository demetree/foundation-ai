/*

   GENERATED SERVICE FOR THE DOCUMENTDOCUMENTTAG TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the DocumentDocumentTag table.

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
import { DocumentData } from './document.service';
import { DocumentTagData } from './document-tag.service';
import { DocumentDocumentTagChangeHistoryService, DocumentDocumentTagChangeHistoryData } from './document-document-tag-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class DocumentDocumentTagQueryParameters {
    versionNumber: bigint | number | null | undefined = null;
    objectGuid: string | null | undefined = null;
    active: boolean | null | undefined = null;
    deleted: boolean | null | undefined = null;
    documentId: bigint | number | null | undefined = null;
    documentTagId: bigint | number | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class DocumentDocumentTagSubmitData {
    id!: bigint | number;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
    documentId!: bigint | number;
    documentTagId!: bigint | number;
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

export class DocumentDocumentTagBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. DocumentDocumentTagChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `documentDocumentTag.DocumentDocumentTagChildren$` — use with `| async` in templates
//        • Promise:    `documentDocumentTag.DocumentDocumentTagChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="documentDocumentTag.DocumentDocumentTagChildren$ | async"`), or
//        • Access the promise getter (`documentDocumentTag.DocumentDocumentTagChildren` or `await documentDocumentTag.DocumentDocumentTagChildren`)
//    - Simply reading `documentDocumentTag.DocumentDocumentTagChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await documentDocumentTag.Reload()` to refresh the entire object and clear all lazy caches.
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
export class DocumentDocumentTagData {
    id!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    documentId!: bigint | number;
    documentTagId!: bigint | number;
    document: DocumentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    documentTag: DocumentTagData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _documentDocumentTagChangeHistories: DocumentDocumentTagChangeHistoryData[] | null = null;
    private _documentDocumentTagChangeHistoriesPromise: Promise<DocumentDocumentTagChangeHistoryData[]> | null  = null;
    private _documentDocumentTagChangeHistoriesSubject = new BehaviorSubject<DocumentDocumentTagChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<DocumentDocumentTagData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<DocumentDocumentTagData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<DocumentDocumentTagData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public DocumentDocumentTagChangeHistories$ = this._documentDocumentTagChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._documentDocumentTagChangeHistories === null && this._documentDocumentTagChangeHistoriesPromise === null) {
            this.loadDocumentDocumentTagChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _documentDocumentTagChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get DocumentDocumentTagChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._documentDocumentTagChangeHistoriesCount$ === null) {
            this._documentDocumentTagChangeHistoriesCount$ = DocumentDocumentTagChangeHistoryService.Instance.GetDocumentDocumentTagChangeHistoriesRowCount({documentDocumentTagId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._documentDocumentTagChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any DocumentDocumentTagData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.documentDocumentTag.Reload();
  //
  //  Non Async:
  //
  //     documentDocumentTag[0].Reload().then(x => {
  //        this.documentDocumentTag = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      DocumentDocumentTagService.Instance.GetDocumentDocumentTag(this.id, includeRelations)
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
     this._documentDocumentTagChangeHistories = null;
     this._documentDocumentTagChangeHistoriesPromise = null;
     this._documentDocumentTagChangeHistoriesSubject.next(null);
     this._documentDocumentTagChangeHistoriesCount$ = null;

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
     * Gets the DocumentDocumentTagChangeHistories for this DocumentDocumentTag.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.documentDocumentTag.DocumentDocumentTagChangeHistories.then(documentDocumentTags => { ... })
     *   or
     *   await this.documentDocumentTag.documentDocumentTags
     *
    */
    public get DocumentDocumentTagChangeHistories(): Promise<DocumentDocumentTagChangeHistoryData[]> {
        if (this._documentDocumentTagChangeHistories !== null) {
            return Promise.resolve(this._documentDocumentTagChangeHistories);
        }

        if (this._documentDocumentTagChangeHistoriesPromise !== null) {
            return this._documentDocumentTagChangeHistoriesPromise;
        }

        // Start the load
        this.loadDocumentDocumentTagChangeHistories();

        return this._documentDocumentTagChangeHistoriesPromise!;
    }



    private loadDocumentDocumentTagChangeHistories(): void {

        this._documentDocumentTagChangeHistoriesPromise = lastValueFrom(
            DocumentDocumentTagService.Instance.GetDocumentDocumentTagChangeHistoriesForDocumentDocumentTag(this.id)
        )
        .then(DocumentDocumentTagChangeHistories => {
            this._documentDocumentTagChangeHistories = DocumentDocumentTagChangeHistories ?? [];
            this._documentDocumentTagChangeHistoriesSubject.next(this._documentDocumentTagChangeHistories);
            return this._documentDocumentTagChangeHistories;
         })
        .catch(err => {
            this._documentDocumentTagChangeHistories = [];
            this._documentDocumentTagChangeHistoriesSubject.next(this._documentDocumentTagChangeHistories);
            throw err;
        })
        .finally(() => {
            this._documentDocumentTagChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached DocumentDocumentTagChangeHistory. Call after mutations to force refresh.
     */
    public ClearDocumentDocumentTagChangeHistoriesCache(): void {
        this._documentDocumentTagChangeHistories = null;
        this._documentDocumentTagChangeHistoriesPromise = null;
        this._documentDocumentTagChangeHistoriesSubject.next(this._documentDocumentTagChangeHistories);      // Emit to observable
    }

    public get HasDocumentDocumentTagChangeHistories(): Promise<boolean> {
        return this.DocumentDocumentTagChangeHistories.then(documentDocumentTagChangeHistories => documentDocumentTagChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (documentDocumentTag.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await documentDocumentTag.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<DocumentDocumentTagData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<DocumentDocumentTagData>> {
        const info = await lastValueFrom(
            DocumentDocumentTagService.Instance.GetDocumentDocumentTagChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this DocumentDocumentTagData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this DocumentDocumentTagData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): DocumentDocumentTagSubmitData {
        return DocumentDocumentTagService.Instance.ConvertToDocumentDocumentTagSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class DocumentDocumentTagService extends SecureEndpointBase {

    private static _instance: DocumentDocumentTagService;
    private listCache: Map<string, Observable<Array<DocumentDocumentTagData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<DocumentDocumentTagBasicListData>>>;
    private recordCache: Map<string, Observable<DocumentDocumentTagData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private documentDocumentTagChangeHistoryService: DocumentDocumentTagChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<DocumentDocumentTagData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<DocumentDocumentTagBasicListData>>>();
        this.recordCache = new Map<string, Observable<DocumentDocumentTagData>>();

        DocumentDocumentTagService._instance = this;
    }

    public static get Instance(): DocumentDocumentTagService {
      return DocumentDocumentTagService._instance;
    }


    public ClearListCaches(config: DocumentDocumentTagQueryParameters | null = null) {

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


    public ConvertToDocumentDocumentTagSubmitData(data: DocumentDocumentTagData): DocumentDocumentTagSubmitData {

        let output = new DocumentDocumentTagSubmitData();

        output.id = data.id;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;
        output.documentId = data.documentId;
        output.documentTagId = data.documentTagId;

        return output;
    }

    public GetDocumentDocumentTag(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentDocumentTagData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const documentDocumentTag$ = this.requestDocumentDocumentTag(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentDocumentTag", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, documentDocumentTag$);

            return documentDocumentTag$;
        }

        return this.recordCache.get(configHash) as Observable<DocumentDocumentTagData>;
    }

    private requestDocumentDocumentTag(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentDocumentTagData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentDocumentTagData>(this.baseUrl + 'api/DocumentDocumentTag/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveDocumentDocumentTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentDocumentTag(id, includeRelations));
            }));
    }

    public GetDocumentDocumentTagList(config: DocumentDocumentTagQueryParameters | any = null) : Observable<Array<DocumentDocumentTagData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const documentDocumentTagList$ = this.requestDocumentDocumentTagList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentDocumentTag list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, documentDocumentTagList$);

            return documentDocumentTagList$;
        }

        return this.listCache.get(configHash) as Observable<Array<DocumentDocumentTagData>>;
    }


    private requestDocumentDocumentTagList(config: DocumentDocumentTagQueryParameters | any) : Observable <Array<DocumentDocumentTagData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentDocumentTagData>>(this.baseUrl + 'api/DocumentDocumentTags', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveDocumentDocumentTagList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentDocumentTagList(config));
            }));
    }

    public GetDocumentDocumentTagsRowCount(config: DocumentDocumentTagQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const documentDocumentTagsRowCount$ = this.requestDocumentDocumentTagsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentDocumentTags row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, documentDocumentTagsRowCount$);

            return documentDocumentTagsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestDocumentDocumentTagsRowCount(config: DocumentDocumentTagQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/DocumentDocumentTags/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentDocumentTagsRowCount(config));
            }));
    }

    public GetDocumentDocumentTagsBasicListData(config: DocumentDocumentTagQueryParameters | any = null) : Observable<Array<DocumentDocumentTagBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const documentDocumentTagsBasicListData$ = this.requestDocumentDocumentTagsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentDocumentTags basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, documentDocumentTagsBasicListData$);

            return documentDocumentTagsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<DocumentDocumentTagBasicListData>>;
    }


    private requestDocumentDocumentTagsBasicListData(config: DocumentDocumentTagQueryParameters | any) : Observable<Array<DocumentDocumentTagBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentDocumentTagBasicListData>>(this.baseUrl + 'api/DocumentDocumentTags/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentDocumentTagsBasicListData(config));
            }));

    }


    public PutDocumentDocumentTag(id: bigint | number, documentDocumentTag: DocumentDocumentTagSubmitData) : Observable<DocumentDocumentTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentDocumentTagData>(this.baseUrl + 'api/DocumentDocumentTag/' + id.toString(), documentDocumentTag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentDocumentTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutDocumentDocumentTag(id, documentDocumentTag));
            }));
    }


    public PostDocumentDocumentTag(documentDocumentTag: DocumentDocumentTagSubmitData) : Observable<DocumentDocumentTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<DocumentDocumentTagData>(this.baseUrl + 'api/DocumentDocumentTag', documentDocumentTag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentDocumentTag(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostDocumentDocumentTag(documentDocumentTag));
            }));
    }

  
    public DeleteDocumentDocumentTag(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/DocumentDocumentTag/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteDocumentDocumentTag(id));
            }));
    }

    public RollbackDocumentDocumentTag(id: bigint | number, versionNumber: bigint | number) : Observable<DocumentDocumentTagData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentDocumentTagData>(this.baseUrl + 'api/DocumentDocumentTag/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentDocumentTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackDocumentDocumentTag(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a DocumentDocumentTag.
     */
    public GetDocumentDocumentTagChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<DocumentDocumentTagData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<DocumentDocumentTagData>>(this.baseUrl + 'api/DocumentDocumentTag/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentDocumentTagChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a DocumentDocumentTag.
     */
    public GetDocumentDocumentTagAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<DocumentDocumentTagData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<DocumentDocumentTagData>[]>(this.baseUrl + 'api/DocumentDocumentTag/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentDocumentTagAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a DocumentDocumentTag.
     */
    public GetDocumentDocumentTagVersion(id: bigint | number, version: number): Observable<DocumentDocumentTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentDocumentTagData>(this.baseUrl + 'api/DocumentDocumentTag/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveDocumentDocumentTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentDocumentTagVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a DocumentDocumentTag at a specific point in time.
     */
    public GetDocumentDocumentTagStateAtTime(id: bigint | number, time: string): Observable<DocumentDocumentTagData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentDocumentTagData>(this.baseUrl + 'api/DocumentDocumentTag/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveDocumentDocumentTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentDocumentTagStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: DocumentDocumentTagQueryParameters | any): string {

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

    public userIsSchedulerDocumentDocumentTagReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerDocumentDocumentTagReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.DocumentDocumentTags
        //
        if (userIsSchedulerDocumentDocumentTagReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerDocumentDocumentTagReader = user.readPermission >= 1;
            } else {
                userIsSchedulerDocumentDocumentTagReader = false;
            }
        }

        return userIsSchedulerDocumentDocumentTagReader;
    }


    public userIsSchedulerDocumentDocumentTagWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerDocumentDocumentTagWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.DocumentDocumentTags
        //
        if (userIsSchedulerDocumentDocumentTagWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerDocumentDocumentTagWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerDocumentDocumentTagWriter = false;
          }      
        }

        return userIsSchedulerDocumentDocumentTagWriter;
    }

    public GetDocumentDocumentTagChangeHistoriesForDocumentDocumentTag(documentDocumentTagId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<DocumentDocumentTagChangeHistoryData[]> {
        return this.documentDocumentTagChangeHistoryService.GetDocumentDocumentTagChangeHistoryList({
            documentDocumentTagId: documentDocumentTagId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full DocumentDocumentTagData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the DocumentDocumentTagData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when DocumentDocumentTagTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveDocumentDocumentTag(raw: any): DocumentDocumentTagData {
    if (!raw) return raw;

    //
    // Create a DocumentDocumentTagData object instance with correct prototype
    //
    const revived = Object.create(DocumentDocumentTagData.prototype) as DocumentDocumentTagData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._documentDocumentTagChangeHistories = null;
    (revived as any)._documentDocumentTagChangeHistoriesPromise = null;
    (revived as any)._documentDocumentTagChangeHistoriesSubject = new BehaviorSubject<DocumentDocumentTagChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadDocumentDocumentTagXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).DocumentDocumentTagChangeHistories$ = (revived as any)._documentDocumentTagChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._documentDocumentTagChangeHistories === null && (revived as any)._documentDocumentTagChangeHistoriesPromise === null) {
                (revived as any).loadDocumentDocumentTagChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._documentDocumentTagChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<DocumentDocumentTagData> | null>(null);

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

  private ReviveDocumentDocumentTagList(rawList: any[]): DocumentDocumentTagData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveDocumentDocumentTag(raw));
  }

}
