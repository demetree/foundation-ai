/*

   GENERATED SERVICE FOR THE DOCUMENTSHARELINK TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the DocumentShareLink table.

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
import { DocumentShareLinkChangeHistoryService, DocumentShareLinkChangeHistoryData } from './document-share-link-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class DocumentShareLinkQueryParameters {
    documentId: bigint | number | null | undefined = null;
    token: string | null | undefined = null;
    passwordHash: string | null | undefined = null;
    expiresAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    maxDownloads: bigint | number | null | undefined = null;
    downloadCount: bigint | number | null | undefined = null;
    createdBy: string | null | undefined = null;
    createdDate: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class DocumentShareLinkSubmitData {
    id!: bigint | number;
    documentId!: bigint | number;
    token!: string;
    passwordHash: string | null = null;
    expiresAt: string | null = null;     // ISO 8601 (full datetime)
    maxDownloads: bigint | number | null = null;
    downloadCount!: bigint | number;
    createdBy!: string;
    createdDate!: string;      // ISO 8601 (full datetime)
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

export class DocumentShareLinkBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. DocumentShareLinkChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `documentShareLink.DocumentShareLinkChildren$` — use with `| async` in templates
//        • Promise:    `documentShareLink.DocumentShareLinkChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="documentShareLink.DocumentShareLinkChildren$ | async"`), or
//        • Access the promise getter (`documentShareLink.DocumentShareLinkChildren` or `await documentShareLink.DocumentShareLinkChildren`)
//    - Simply reading `documentShareLink.DocumentShareLinkChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await documentShareLink.Reload()` to refresh the entire object and clear all lazy caches.
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
export class DocumentShareLinkData {
    id!: bigint | number;
    documentId!: bigint | number;
    token!: string;
    passwordHash!: string | null;
    expiresAt!: string | null;   // ISO 8601 (full datetime)
    maxDownloads!: bigint | number;
    downloadCount!: bigint | number;
    createdBy!: string;
    createdDate!: string;      // ISO 8601 (full datetime)
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    document: DocumentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _documentShareLinkChangeHistories: DocumentShareLinkChangeHistoryData[] | null = null;
    private _documentShareLinkChangeHistoriesPromise: Promise<DocumentShareLinkChangeHistoryData[]> | null  = null;
    private _documentShareLinkChangeHistoriesSubject = new BehaviorSubject<DocumentShareLinkChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<DocumentShareLinkData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<DocumentShareLinkData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<DocumentShareLinkData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public DocumentShareLinkChangeHistories$ = this._documentShareLinkChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._documentShareLinkChangeHistories === null && this._documentShareLinkChangeHistoriesPromise === null) {
            this.loadDocumentShareLinkChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _documentShareLinkChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get DocumentShareLinkChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._documentShareLinkChangeHistoriesCount$ === null) {
            this._documentShareLinkChangeHistoriesCount$ = DocumentShareLinkChangeHistoryService.Instance.GetDocumentShareLinkChangeHistoriesRowCount({documentShareLinkId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._documentShareLinkChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any DocumentShareLinkData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.documentShareLink.Reload();
  //
  //  Non Async:
  //
  //     documentShareLink[0].Reload().then(x => {
  //        this.documentShareLink = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      DocumentShareLinkService.Instance.GetDocumentShareLink(this.id, includeRelations)
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
     this._documentShareLinkChangeHistories = null;
     this._documentShareLinkChangeHistoriesPromise = null;
     this._documentShareLinkChangeHistoriesSubject.next(null);
     this._documentShareLinkChangeHistoriesCount$ = null;

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
     * Gets the DocumentShareLinkChangeHistories for this DocumentShareLink.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.documentShareLink.DocumentShareLinkChangeHistories.then(documentShareLinks => { ... })
     *   or
     *   await this.documentShareLink.documentShareLinks
     *
    */
    public get DocumentShareLinkChangeHistories(): Promise<DocumentShareLinkChangeHistoryData[]> {
        if (this._documentShareLinkChangeHistories !== null) {
            return Promise.resolve(this._documentShareLinkChangeHistories);
        }

        if (this._documentShareLinkChangeHistoriesPromise !== null) {
            return this._documentShareLinkChangeHistoriesPromise;
        }

        // Start the load
        this.loadDocumentShareLinkChangeHistories();

        return this._documentShareLinkChangeHistoriesPromise!;
    }



    private loadDocumentShareLinkChangeHistories(): void {

        this._documentShareLinkChangeHistoriesPromise = lastValueFrom(
            DocumentShareLinkService.Instance.GetDocumentShareLinkChangeHistoriesForDocumentShareLink(this.id)
        )
        .then(DocumentShareLinkChangeHistories => {
            this._documentShareLinkChangeHistories = DocumentShareLinkChangeHistories ?? [];
            this._documentShareLinkChangeHistoriesSubject.next(this._documentShareLinkChangeHistories);
            return this._documentShareLinkChangeHistories;
         })
        .catch(err => {
            this._documentShareLinkChangeHistories = [];
            this._documentShareLinkChangeHistoriesSubject.next(this._documentShareLinkChangeHistories);
            throw err;
        })
        .finally(() => {
            this._documentShareLinkChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached DocumentShareLinkChangeHistory. Call after mutations to force refresh.
     */
    public ClearDocumentShareLinkChangeHistoriesCache(): void {
        this._documentShareLinkChangeHistories = null;
        this._documentShareLinkChangeHistoriesPromise = null;
        this._documentShareLinkChangeHistoriesSubject.next(this._documentShareLinkChangeHistories);      // Emit to observable
    }

    public get HasDocumentShareLinkChangeHistories(): Promise<boolean> {
        return this.DocumentShareLinkChangeHistories.then(documentShareLinkChangeHistories => documentShareLinkChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (documentShareLink.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await documentShareLink.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<DocumentShareLinkData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<DocumentShareLinkData>> {
        const info = await lastValueFrom(
            DocumentShareLinkService.Instance.GetDocumentShareLinkChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this DocumentShareLinkData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this DocumentShareLinkData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): DocumentShareLinkSubmitData {
        return DocumentShareLinkService.Instance.ConvertToDocumentShareLinkSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class DocumentShareLinkService extends SecureEndpointBase {

    private static _instance: DocumentShareLinkService;
    private listCache: Map<string, Observable<Array<DocumentShareLinkData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<DocumentShareLinkBasicListData>>>;
    private recordCache: Map<string, Observable<DocumentShareLinkData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private documentShareLinkChangeHistoryService: DocumentShareLinkChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<DocumentShareLinkData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<DocumentShareLinkBasicListData>>>();
        this.recordCache = new Map<string, Observable<DocumentShareLinkData>>();

        DocumentShareLinkService._instance = this;
    }

    public static get Instance(): DocumentShareLinkService {
      return DocumentShareLinkService._instance;
    }


    public ClearListCaches(config: DocumentShareLinkQueryParameters | null = null) {

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


    public ConvertToDocumentShareLinkSubmitData(data: DocumentShareLinkData): DocumentShareLinkSubmitData {

        let output = new DocumentShareLinkSubmitData();

        output.id = data.id;
        output.documentId = data.documentId;
        output.token = data.token;
        output.passwordHash = data.passwordHash;
        output.expiresAt = data.expiresAt;
        output.maxDownloads = data.maxDownloads;
        output.downloadCount = data.downloadCount;
        output.createdBy = data.createdBy;
        output.createdDate = data.createdDate;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetDocumentShareLink(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentShareLinkData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const documentShareLink$ = this.requestDocumentShareLink(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentShareLink", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, documentShareLink$);

            return documentShareLink$;
        }

        return this.recordCache.get(configHash) as Observable<DocumentShareLinkData>;
    }

    private requestDocumentShareLink(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentShareLinkData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentShareLinkData>(this.baseUrl + 'api/DocumentShareLink/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveDocumentShareLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentShareLink(id, includeRelations));
            }));
    }

    public GetDocumentShareLinkList(config: DocumentShareLinkQueryParameters | any = null) : Observable<Array<DocumentShareLinkData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const documentShareLinkList$ = this.requestDocumentShareLinkList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentShareLink list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, documentShareLinkList$);

            return documentShareLinkList$;
        }

        return this.listCache.get(configHash) as Observable<Array<DocumentShareLinkData>>;
    }


    private requestDocumentShareLinkList(config: DocumentShareLinkQueryParameters | any) : Observable <Array<DocumentShareLinkData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentShareLinkData>>(this.baseUrl + 'api/DocumentShareLinks', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveDocumentShareLinkList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentShareLinkList(config));
            }));
    }

    public GetDocumentShareLinksRowCount(config: DocumentShareLinkQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const documentShareLinksRowCount$ = this.requestDocumentShareLinksRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentShareLinks row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, documentShareLinksRowCount$);

            return documentShareLinksRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestDocumentShareLinksRowCount(config: DocumentShareLinkQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/DocumentShareLinks/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentShareLinksRowCount(config));
            }));
    }

    public GetDocumentShareLinksBasicListData(config: DocumentShareLinkQueryParameters | any = null) : Observable<Array<DocumentShareLinkBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const documentShareLinksBasicListData$ = this.requestDocumentShareLinksBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentShareLinks basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, documentShareLinksBasicListData$);

            return documentShareLinksBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<DocumentShareLinkBasicListData>>;
    }


    private requestDocumentShareLinksBasicListData(config: DocumentShareLinkQueryParameters | any) : Observable<Array<DocumentShareLinkBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentShareLinkBasicListData>>(this.baseUrl + 'api/DocumentShareLinks/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentShareLinksBasicListData(config));
            }));

    }


    public PutDocumentShareLink(id: bigint | number, documentShareLink: DocumentShareLinkSubmitData) : Observable<DocumentShareLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentShareLinkData>(this.baseUrl + 'api/DocumentShareLink/' + id.toString(), documentShareLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentShareLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutDocumentShareLink(id, documentShareLink));
            }));
    }


    public PostDocumentShareLink(documentShareLink: DocumentShareLinkSubmitData) : Observable<DocumentShareLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<DocumentShareLinkData>(this.baseUrl + 'api/DocumentShareLink', documentShareLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentShareLink(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostDocumentShareLink(documentShareLink));
            }));
    }

  
    public DeleteDocumentShareLink(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/DocumentShareLink/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteDocumentShareLink(id));
            }));
    }

    public RollbackDocumentShareLink(id: bigint | number, versionNumber: bigint | number) : Observable<DocumentShareLinkData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentShareLinkData>(this.baseUrl + 'api/DocumentShareLink/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentShareLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackDocumentShareLink(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a DocumentShareLink.
     */
    public GetDocumentShareLinkChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<DocumentShareLinkData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<DocumentShareLinkData>>(this.baseUrl + 'api/DocumentShareLink/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentShareLinkChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a DocumentShareLink.
     */
    public GetDocumentShareLinkAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<DocumentShareLinkData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<DocumentShareLinkData>[]>(this.baseUrl + 'api/DocumentShareLink/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentShareLinkAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a DocumentShareLink.
     */
    public GetDocumentShareLinkVersion(id: bigint | number, version: number): Observable<DocumentShareLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentShareLinkData>(this.baseUrl + 'api/DocumentShareLink/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveDocumentShareLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentShareLinkVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a DocumentShareLink at a specific point in time.
     */
    public GetDocumentShareLinkStateAtTime(id: bigint | number, time: string): Observable<DocumentShareLinkData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentShareLinkData>(this.baseUrl + 'api/DocumentShareLink/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveDocumentShareLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentShareLinkStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: DocumentShareLinkQueryParameters | any): string {

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

    public userIsSchedulerDocumentShareLinkReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerDocumentShareLinkReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.DocumentShareLinks
        //
        if (userIsSchedulerDocumentShareLinkReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerDocumentShareLinkReader = user.readPermission >= 1;
            } else {
                userIsSchedulerDocumentShareLinkReader = false;
            }
        }

        return userIsSchedulerDocumentShareLinkReader;
    }


    public userIsSchedulerDocumentShareLinkWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerDocumentShareLinkWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.DocumentShareLinks
        //
        if (userIsSchedulerDocumentShareLinkWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerDocumentShareLinkWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerDocumentShareLinkWriter = false;
          }      
        }

        return userIsSchedulerDocumentShareLinkWriter;
    }

    public GetDocumentShareLinkChangeHistoriesForDocumentShareLink(documentShareLinkId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<DocumentShareLinkChangeHistoryData[]> {
        return this.documentShareLinkChangeHistoryService.GetDocumentShareLinkChangeHistoryList({
            documentShareLinkId: documentShareLinkId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full DocumentShareLinkData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the DocumentShareLinkData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when DocumentShareLinkTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveDocumentShareLink(raw: any): DocumentShareLinkData {
    if (!raw) return raw;

    //
    // Create a DocumentShareLinkData object instance with correct prototype
    //
    const revived = Object.create(DocumentShareLinkData.prototype) as DocumentShareLinkData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._documentShareLinkChangeHistories = null;
    (revived as any)._documentShareLinkChangeHistoriesPromise = null;
    (revived as any)._documentShareLinkChangeHistoriesSubject = new BehaviorSubject<DocumentShareLinkChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadDocumentShareLinkXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).DocumentShareLinkChangeHistories$ = (revived as any)._documentShareLinkChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._documentShareLinkChangeHistories === null && (revived as any)._documentShareLinkChangeHistoriesPromise === null) {
                (revived as any).loadDocumentShareLinkChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._documentShareLinkChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<DocumentShareLinkData> | null>(null);

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

  private ReviveDocumentShareLinkList(rawList: any[]): DocumentShareLinkData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveDocumentShareLink(raw));
  }

}
