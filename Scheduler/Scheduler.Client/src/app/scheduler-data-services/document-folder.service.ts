/*

   GENERATED SERVICE FOR THE DOCUMENTFOLDER TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the DocumentFolder table.

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
import { IconData } from './icon.service';
import { DocumentFolderChangeHistoryService, DocumentFolderChangeHistoryData } from './document-folder-change-history.service';
import { DocumentService, DocumentData } from './document.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class DocumentFolderQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    parentDocumentFolderId: bigint | number | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    notes: string | null | undefined = null;
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
export class DocumentFolderSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    parentDocumentFolderId: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    sequence!: bigint | number;
    notes: string | null = null;
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

export class DocumentFolderBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. DocumentFolderChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `documentFolder.DocumentFolderChildren$` — use with `| async` in templates
//        • Promise:    `documentFolder.DocumentFolderChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="documentFolder.DocumentFolderChildren$ | async"`), or
//        • Access the promise getter (`documentFolder.DocumentFolderChildren` or `await documentFolder.DocumentFolderChildren`)
//    - Simply reading `documentFolder.DocumentFolderChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await documentFolder.Reload()` to refresh the entire object and clear all lazy caches.
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
export class DocumentFolderData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    parentDocumentFolderId!: bigint | number;
    iconId!: bigint | number;
    color!: string | null;
    sequence!: bigint | number;
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    parentDocumentFolder: DocumentFolderData | null | undefined = null;            // Self referencing navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _documentFolderChangeHistories: DocumentFolderChangeHistoryData[] | null = null;
    private _documentFolderChangeHistoriesPromise: Promise<DocumentFolderChangeHistoryData[]> | null  = null;
    private _documentFolderChangeHistoriesSubject = new BehaviorSubject<DocumentFolderChangeHistoryData[] | null>(null);

                
    private _documents: DocumentData[] | null = null;
    private _documentsPromise: Promise<DocumentData[]> | null  = null;
    private _documentsSubject = new BehaviorSubject<DocumentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<DocumentFolderData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<DocumentFolderData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<DocumentFolderData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public DocumentFolderChangeHistories$ = this._documentFolderChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._documentFolderChangeHistories === null && this._documentFolderChangeHistoriesPromise === null) {
            this.loadDocumentFolderChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _documentFolderChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get DocumentFolderChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._documentFolderChangeHistoriesCount$ === null) {
            this._documentFolderChangeHistoriesCount$ = DocumentFolderChangeHistoryService.Instance.GetDocumentFolderChangeHistoriesRowCount({documentFolderId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._documentFolderChangeHistoriesCount$;
    }



    public Documents$ = this._documentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._documents === null && this._documentsPromise === null) {
            this.loadDocuments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _documentsCount$: Observable<bigint | number> | null = null;
    public get DocumentsCount$(): Observable<bigint | number> {
        if (this._documentsCount$ === null) {
            this._documentsCount$ = DocumentService.Instance.GetDocumentsRowCount({documentFolderId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._documentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any DocumentFolderData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.documentFolder.Reload();
  //
  //  Non Async:
  //
  //     documentFolder[0].Reload().then(x => {
  //        this.documentFolder = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      DocumentFolderService.Instance.GetDocumentFolder(this.id, includeRelations)
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
     this._documentFolderChangeHistories = null;
     this._documentFolderChangeHistoriesPromise = null;
     this._documentFolderChangeHistoriesSubject.next(null);
     this._documentFolderChangeHistoriesCount$ = null;

     this._documents = null;
     this._documentsPromise = null;
     this._documentsSubject.next(null);
     this._documentsCount$ = null;

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
     * Gets the DocumentFolderChangeHistories for this DocumentFolder.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.documentFolder.DocumentFolderChangeHistories.then(documentFolders => { ... })
     *   or
     *   await this.documentFolder.documentFolders
     *
    */
    public get DocumentFolderChangeHistories(): Promise<DocumentFolderChangeHistoryData[]> {
        if (this._documentFolderChangeHistories !== null) {
            return Promise.resolve(this._documentFolderChangeHistories);
        }

        if (this._documentFolderChangeHistoriesPromise !== null) {
            return this._documentFolderChangeHistoriesPromise;
        }

        // Start the load
        this.loadDocumentFolderChangeHistories();

        return this._documentFolderChangeHistoriesPromise!;
    }



    private loadDocumentFolderChangeHistories(): void {

        this._documentFolderChangeHistoriesPromise = lastValueFrom(
            DocumentFolderService.Instance.GetDocumentFolderChangeHistoriesForDocumentFolder(this.id)
        )
        .then(DocumentFolderChangeHistories => {
            this._documentFolderChangeHistories = DocumentFolderChangeHistories ?? [];
            this._documentFolderChangeHistoriesSubject.next(this._documentFolderChangeHistories);
            return this._documentFolderChangeHistories;
         })
        .catch(err => {
            this._documentFolderChangeHistories = [];
            this._documentFolderChangeHistoriesSubject.next(this._documentFolderChangeHistories);
            throw err;
        })
        .finally(() => {
            this._documentFolderChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached DocumentFolderChangeHistory. Call after mutations to force refresh.
     */
    public ClearDocumentFolderChangeHistoriesCache(): void {
        this._documentFolderChangeHistories = null;
        this._documentFolderChangeHistoriesPromise = null;
        this._documentFolderChangeHistoriesSubject.next(this._documentFolderChangeHistories);      // Emit to observable
    }

    public get HasDocumentFolderChangeHistories(): Promise<boolean> {
        return this.DocumentFolderChangeHistories.then(documentFolderChangeHistories => documentFolderChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Documents for this DocumentFolder.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.documentFolder.Documents.then(documentFolders => { ... })
     *   or
     *   await this.documentFolder.documentFolders
     *
    */
    public get Documents(): Promise<DocumentData[]> {
        if (this._documents !== null) {
            return Promise.resolve(this._documents);
        }

        if (this._documentsPromise !== null) {
            return this._documentsPromise;
        }

        // Start the load
        this.loadDocuments();

        return this._documentsPromise!;
    }



    private loadDocuments(): void {

        this._documentsPromise = lastValueFrom(
            DocumentFolderService.Instance.GetDocumentsForDocumentFolder(this.id)
        )
        .then(Documents => {
            this._documents = Documents ?? [];
            this._documentsSubject.next(this._documents);
            return this._documents;
         })
        .catch(err => {
            this._documents = [];
            this._documentsSubject.next(this._documents);
            throw err;
        })
        .finally(() => {
            this._documentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Document. Call after mutations to force refresh.
     */
    public ClearDocumentsCache(): void {
        this._documents = null;
        this._documentsPromise = null;
        this._documentsSubject.next(this._documents);      // Emit to observable
    }

    public get HasDocuments(): Promise<boolean> {
        return this.Documents.then(documents => documents.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (documentFolder.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await documentFolder.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<DocumentFolderData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<DocumentFolderData>> {
        const info = await lastValueFrom(
            DocumentFolderService.Instance.GetDocumentFolderChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this DocumentFolderData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this DocumentFolderData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): DocumentFolderSubmitData {
        return DocumentFolderService.Instance.ConvertToDocumentFolderSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class DocumentFolderService extends SecureEndpointBase {

    private static _instance: DocumentFolderService;
    private listCache: Map<string, Observable<Array<DocumentFolderData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<DocumentFolderBasicListData>>>;
    private recordCache: Map<string, Observable<DocumentFolderData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private documentFolderChangeHistoryService: DocumentFolderChangeHistoryService,
        private documentService: DocumentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<DocumentFolderData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<DocumentFolderBasicListData>>>();
        this.recordCache = new Map<string, Observable<DocumentFolderData>>();

        DocumentFolderService._instance = this;
    }

    public static get Instance(): DocumentFolderService {
      return DocumentFolderService._instance;
    }


    public ClearListCaches(config: DocumentFolderQueryParameters | null = null) {

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


    public ConvertToDocumentFolderSubmitData(data: DocumentFolderData): DocumentFolderSubmitData {

        let output = new DocumentFolderSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.parentDocumentFolderId = data.parentDocumentFolderId;
        output.iconId = data.iconId;
        output.color = data.color;
        output.sequence = data.sequence;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetDocumentFolder(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentFolderData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const documentFolder$ = this.requestDocumentFolder(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentFolder", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, documentFolder$);

            return documentFolder$;
        }

        return this.recordCache.get(configHash) as Observable<DocumentFolderData>;
    }

    private requestDocumentFolder(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentFolderData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentFolderData>(this.baseUrl + 'api/DocumentFolder/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveDocumentFolder(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentFolder(id, includeRelations));
            }));
    }

    public GetDocumentFolderList(config: DocumentFolderQueryParameters | any = null) : Observable<Array<DocumentFolderData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const documentFolderList$ = this.requestDocumentFolderList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentFolder list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, documentFolderList$);

            return documentFolderList$;
        }

        return this.listCache.get(configHash) as Observable<Array<DocumentFolderData>>;
    }


    private requestDocumentFolderList(config: DocumentFolderQueryParameters | any) : Observable <Array<DocumentFolderData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentFolderData>>(this.baseUrl + 'api/DocumentFolders', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveDocumentFolderList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentFolderList(config));
            }));
    }

    public GetDocumentFoldersRowCount(config: DocumentFolderQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const documentFoldersRowCount$ = this.requestDocumentFoldersRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentFolders row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, documentFoldersRowCount$);

            return documentFoldersRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestDocumentFoldersRowCount(config: DocumentFolderQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/DocumentFolders/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentFoldersRowCount(config));
            }));
    }

    public GetDocumentFoldersBasicListData(config: DocumentFolderQueryParameters | any = null) : Observable<Array<DocumentFolderBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const documentFoldersBasicListData$ = this.requestDocumentFoldersBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentFolders basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, documentFoldersBasicListData$);

            return documentFoldersBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<DocumentFolderBasicListData>>;
    }


    private requestDocumentFoldersBasicListData(config: DocumentFolderQueryParameters | any) : Observable<Array<DocumentFolderBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentFolderBasicListData>>(this.baseUrl + 'api/DocumentFolders/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentFoldersBasicListData(config));
            }));

    }


    public PutDocumentFolder(id: bigint | number, documentFolder: DocumentFolderSubmitData) : Observable<DocumentFolderData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentFolderData>(this.baseUrl + 'api/DocumentFolder/' + id.toString(), documentFolder, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentFolder(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutDocumentFolder(id, documentFolder));
            }));
    }


    public PostDocumentFolder(documentFolder: DocumentFolderSubmitData) : Observable<DocumentFolderData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<DocumentFolderData>(this.baseUrl + 'api/DocumentFolder', documentFolder, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentFolder(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostDocumentFolder(documentFolder));
            }));
    }

  
    public DeleteDocumentFolder(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/DocumentFolder/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteDocumentFolder(id));
            }));
    }

    public RollbackDocumentFolder(id: bigint | number, versionNumber: bigint | number) : Observable<DocumentFolderData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentFolderData>(this.baseUrl + 'api/DocumentFolder/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentFolder(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackDocumentFolder(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a DocumentFolder.
     */
    public GetDocumentFolderChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<DocumentFolderData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<DocumentFolderData>>(this.baseUrl + 'api/DocumentFolder/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentFolderChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a DocumentFolder.
     */
    public GetDocumentFolderAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<DocumentFolderData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<DocumentFolderData>[]>(this.baseUrl + 'api/DocumentFolder/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentFolderAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a DocumentFolder.
     */
    public GetDocumentFolderVersion(id: bigint | number, version: number): Observable<DocumentFolderData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentFolderData>(this.baseUrl + 'api/DocumentFolder/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveDocumentFolder(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentFolderVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a DocumentFolder at a specific point in time.
     */
    public GetDocumentFolderStateAtTime(id: bigint | number, time: string): Observable<DocumentFolderData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentFolderData>(this.baseUrl + 'api/DocumentFolder/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveDocumentFolder(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentFolderStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: DocumentFolderQueryParameters | any): string {

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

    public userIsSchedulerDocumentFolderReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerDocumentFolderReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.DocumentFolders
        //
        if (userIsSchedulerDocumentFolderReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerDocumentFolderReader = user.readPermission >= 1;
            } else {
                userIsSchedulerDocumentFolderReader = false;
            }
        }

        return userIsSchedulerDocumentFolderReader;
    }


    public userIsSchedulerDocumentFolderWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerDocumentFolderWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.DocumentFolders
        //
        if (userIsSchedulerDocumentFolderWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerDocumentFolderWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerDocumentFolderWriter = false;
          }      
        }

        return userIsSchedulerDocumentFolderWriter;
    }

    public GetDocumentFolderChangeHistoriesForDocumentFolder(documentFolderId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<DocumentFolderChangeHistoryData[]> {
        return this.documentFolderChangeHistoryService.GetDocumentFolderChangeHistoryList({
            documentFolderId: documentFolderId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetDocumentsForDocumentFolder(documentFolderId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<DocumentData[]> {
        return this.documentService.GetDocumentList({
            documentFolderId: documentFolderId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full DocumentFolderData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the DocumentFolderData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when DocumentFolderTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveDocumentFolder(raw: any): DocumentFolderData {
    if (!raw) return raw;

    //
    // Create a DocumentFolderData object instance with correct prototype
    //
    const revived = Object.create(DocumentFolderData.prototype) as DocumentFolderData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._documentFolderChangeHistories = null;
    (revived as any)._documentFolderChangeHistoriesPromise = null;
    (revived as any)._documentFolderChangeHistoriesSubject = new BehaviorSubject<DocumentFolderChangeHistoryData[] | null>(null);

    (revived as any)._documents = null;
    (revived as any)._documentsPromise = null;
    (revived as any)._documentsSubject = new BehaviorSubject<DocumentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadDocumentFolderXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).DocumentFolderChangeHistories$ = (revived as any)._documentFolderChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._documentFolderChangeHistories === null && (revived as any)._documentFolderChangeHistoriesPromise === null) {
                (revived as any).loadDocumentFolderChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._documentFolderChangeHistoriesCount$ = null;


    (revived as any).Documents$ = (revived as any)._documentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._documents === null && (revived as any)._documentsPromise === null) {
                (revived as any).loadDocuments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._documentsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<DocumentFolderData> | null>(null);

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

  private ReviveDocumentFolderList(rawList: any[]): DocumentFolderData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveDocumentFolder(raw));
  }

}
