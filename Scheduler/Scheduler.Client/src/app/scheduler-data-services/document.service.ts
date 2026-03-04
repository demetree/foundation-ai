/*

   GENERATED SERVICE FOR THE DOCUMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Document table.

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
import { DocumentTypeData } from './document-type.service';
import { ScheduledEventData } from './scheduled-event.service';
import { FinancialTransactionData } from './financial-transaction.service';
import { ContactData } from './contact.service';
import { ResourceData } from './resource.service';
import { DocumentChangeHistoryService, DocumentChangeHistoryData } from './document-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class DocumentQueryParameters {
    documentTypeId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    fileName: string | null | undefined = null;
    mimeType: string | null | undefined = null;
    fileSizeBytes: bigint | number | null | undefined = null;
    fileDataFileName: string | null | undefined = null;
    fileDataSize: bigint | number | null | undefined = null;
    fileDataMimeType: string | null | undefined = null;
    scheduledEventId: bigint | number | null | undefined = null;
    financialTransactionId: bigint | number | null | undefined = null;
    contactId: bigint | number | null | undefined = null;
    resourceId: bigint | number | null | undefined = null;
    status: string | null | undefined = null;
    statusDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    statusChangedBy: string | null | undefined = null;
    uploadedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    uploadedBy: string | null | undefined = null;
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
export class DocumentSubmitData {
    id!: bigint | number;
    documentTypeId!: bigint | number;
    name!: string;
    description: string | null = null;
    fileName!: string;
    mimeType!: string;
    fileSizeBytes!: bigint | number;
    fileDataFileName: string | null = null;
    fileDataSize: bigint | number | null = null;
    fileDataData: string | null = null;
    fileDataMimeType: string | null = null;
    scheduledEventId: bigint | number | null = null;
    financialTransactionId: bigint | number | null = null;
    contactId: bigint | number | null = null;
    resourceId: bigint | number | null = null;
    status: string | null = null;
    statusDate: string | null = null;     // ISO 8601 (full datetime)
    statusChangedBy: string | null = null;
    uploadedDate!: string;      // ISO 8601 (full datetime)
    uploadedBy: string | null = null;
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

export class DocumentBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. DocumentChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `document.DocumentChildren$` — use with `| async` in templates
//        • Promise:    `document.DocumentChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="document.DocumentChildren$ | async"`), or
//        • Access the promise getter (`document.DocumentChildren` or `await document.DocumentChildren`)
//    - Simply reading `document.DocumentChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await document.Reload()` to refresh the entire object and clear all lazy caches.
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
export class DocumentData {
    id!: bigint | number;
    documentTypeId!: bigint | number;
    name!: string;
    description!: string | null;
    fileName!: string;
    mimeType!: string;
    fileSizeBytes!: bigint | number;
    fileDataFileName!: string | null;
    fileDataSize!: bigint | number;
    fileDataData!: string | null;
    fileDataMimeType!: string | null;
    scheduledEventId!: bigint | number;
    financialTransactionId!: bigint | number;
    contactId!: bigint | number;
    resourceId!: bigint | number;
    status!: string | null;
    statusDate!: string | null;   // ISO 8601 (full datetime)
    statusChangedBy!: string | null;
    uploadedDate!: string;      // ISO 8601 (full datetime)
    uploadedBy!: string | null;
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    documentType: DocumentTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    financialTransaction: FinancialTransactionData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduledEvent: ScheduledEventData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _documentChangeHistories: DocumentChangeHistoryData[] | null = null;
    private _documentChangeHistoriesPromise: Promise<DocumentChangeHistoryData[]> | null  = null;
    private _documentChangeHistoriesSubject = new BehaviorSubject<DocumentChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<DocumentData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<DocumentData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<DocumentData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public DocumentChangeHistories$ = this._documentChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._documentChangeHistories === null && this._documentChangeHistoriesPromise === null) {
            this.loadDocumentChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _documentChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get DocumentChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._documentChangeHistoriesCount$ === null) {
            this._documentChangeHistoriesCount$ = DocumentChangeHistoryService.Instance.GetDocumentChangeHistoriesRowCount({documentId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._documentChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any DocumentData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.document.Reload();
  //
  //  Non Async:
  //
  //     document[0].Reload().then(x => {
  //        this.document = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      DocumentService.Instance.GetDocument(this.id, includeRelations)
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
     this._documentChangeHistories = null;
     this._documentChangeHistoriesPromise = null;
     this._documentChangeHistoriesSubject.next(null);
     this._documentChangeHistoriesCount$ = null;

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
     * Gets the DocumentChangeHistories for this Document.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.document.DocumentChangeHistories.then(documents => { ... })
     *   or
     *   await this.document.documents
     *
    */
    public get DocumentChangeHistories(): Promise<DocumentChangeHistoryData[]> {
        if (this._documentChangeHistories !== null) {
            return Promise.resolve(this._documentChangeHistories);
        }

        if (this._documentChangeHistoriesPromise !== null) {
            return this._documentChangeHistoriesPromise;
        }

        // Start the load
        this.loadDocumentChangeHistories();

        return this._documentChangeHistoriesPromise!;
    }



    private loadDocumentChangeHistories(): void {

        this._documentChangeHistoriesPromise = lastValueFrom(
            DocumentService.Instance.GetDocumentChangeHistoriesForDocument(this.id)
        )
        .then(DocumentChangeHistories => {
            this._documentChangeHistories = DocumentChangeHistories ?? [];
            this._documentChangeHistoriesSubject.next(this._documentChangeHistories);
            return this._documentChangeHistories;
         })
        .catch(err => {
            this._documentChangeHistories = [];
            this._documentChangeHistoriesSubject.next(this._documentChangeHistories);
            throw err;
        })
        .finally(() => {
            this._documentChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached DocumentChangeHistory. Call after mutations to force refresh.
     */
    public ClearDocumentChangeHistoriesCache(): void {
        this._documentChangeHistories = null;
        this._documentChangeHistoriesPromise = null;
        this._documentChangeHistoriesSubject.next(this._documentChangeHistories);      // Emit to observable
    }

    public get HasDocumentChangeHistories(): Promise<boolean> {
        return this.DocumentChangeHistories.then(documentChangeHistories => documentChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (document.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await document.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<DocumentData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<DocumentData>> {
        const info = await lastValueFrom(
            DocumentService.Instance.GetDocumentChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this DocumentData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this DocumentData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): DocumentSubmitData {
        return DocumentService.Instance.ConvertToDocumentSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class DocumentService extends SecureEndpointBase {

    private static _instance: DocumentService;
    private listCache: Map<string, Observable<Array<DocumentData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<DocumentBasicListData>>>;
    private recordCache: Map<string, Observable<DocumentData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private documentChangeHistoryService: DocumentChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<DocumentData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<DocumentBasicListData>>>();
        this.recordCache = new Map<string, Observable<DocumentData>>();

        DocumentService._instance = this;
    }

    public static get Instance(): DocumentService {
      return DocumentService._instance;
    }


    public ClearListCaches(config: DocumentQueryParameters | null = null) {

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


    public ConvertToDocumentSubmitData(data: DocumentData): DocumentSubmitData {

        let output = new DocumentSubmitData();

        output.id = data.id;
        output.documentTypeId = data.documentTypeId;
        output.name = data.name;
        output.description = data.description;
        output.fileName = data.fileName;
        output.mimeType = data.mimeType;
        output.fileSizeBytes = data.fileSizeBytes;
        output.fileDataFileName = data.fileDataFileName;
        output.fileDataSize = data.fileDataSize;
        output.fileDataData = data.fileDataData;
        output.fileDataMimeType = data.fileDataMimeType;
        output.scheduledEventId = data.scheduledEventId;
        output.financialTransactionId = data.financialTransactionId;
        output.contactId = data.contactId;
        output.resourceId = data.resourceId;
        output.status = data.status;
        output.statusDate = data.statusDate;
        output.statusChangedBy = data.statusChangedBy;
        output.uploadedDate = data.uploadedDate;
        output.uploadedBy = data.uploadedBy;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetDocument(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const document$ = this.requestDocument(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Document", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, document$);

            return document$;
        }

        return this.recordCache.get(configHash) as Observable<DocumentData>;
    }

    private requestDocument(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentData>(this.baseUrl + 'api/Document/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveDocument(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocument(id, includeRelations));
            }));
    }

    public GetDocumentList(config: DocumentQueryParameters | any = null) : Observable<Array<DocumentData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const documentList$ = this.requestDocumentList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Document list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, documentList$);

            return documentList$;
        }

        return this.listCache.get(configHash) as Observable<Array<DocumentData>>;
    }


    private requestDocumentList(config: DocumentQueryParameters | any) : Observable <Array<DocumentData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentData>>(this.baseUrl + 'api/Documents', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveDocumentList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentList(config));
            }));
    }

    public GetDocumentsRowCount(config: DocumentQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const documentsRowCount$ = this.requestDocumentsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Documents row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, documentsRowCount$);

            return documentsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestDocumentsRowCount(config: DocumentQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Documents/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentsRowCount(config));
            }));
    }

    public GetDocumentsBasicListData(config: DocumentQueryParameters | any = null) : Observable<Array<DocumentBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const documentsBasicListData$ = this.requestDocumentsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Documents basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, documentsBasicListData$);

            return documentsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<DocumentBasicListData>>;
    }


    private requestDocumentsBasicListData(config: DocumentQueryParameters | any) : Observable<Array<DocumentBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentBasicListData>>(this.baseUrl + 'api/Documents/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentsBasicListData(config));
            }));

    }


    public PutDocument(id: bigint | number, document: DocumentSubmitData) : Observable<DocumentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentData>(this.baseUrl + 'api/Document/' + id.toString(), document, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocument(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutDocument(id, document));
            }));
    }


    public PostDocument(document: DocumentSubmitData) : Observable<DocumentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<DocumentData>(this.baseUrl + 'api/Document', document, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocument(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostDocument(document));
            }));
    }

  
    public DeleteDocument(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Document/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteDocument(id));
            }));
    }

    public RollbackDocument(id: bigint | number, versionNumber: bigint | number) : Observable<DocumentData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentData>(this.baseUrl + 'api/Document/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocument(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackDocument(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Document.
     */
    public GetDocumentChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<DocumentData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<DocumentData>>(this.baseUrl + 'api/Document/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Document.
     */
    public GetDocumentAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<DocumentData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<DocumentData>[]>(this.baseUrl + 'api/Document/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Document.
     */
    public GetDocumentVersion(id: bigint | number, version: number): Observable<DocumentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentData>(this.baseUrl + 'api/Document/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveDocument(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Document at a specific point in time.
     */
    public GetDocumentStateAtTime(id: bigint | number, time: string): Observable<DocumentData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentData>(this.baseUrl + 'api/Document/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveDocument(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetDocumentStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: DocumentQueryParameters | any): string {

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

    public userIsSchedulerDocumentReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerDocumentReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Documents
        //
        if (userIsSchedulerDocumentReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerDocumentReader = user.readPermission >= 1;
            } else {
                userIsSchedulerDocumentReader = false;
            }
        }

        return userIsSchedulerDocumentReader;
    }


    public userIsSchedulerDocumentWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerDocumentWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Documents
        //
        if (userIsSchedulerDocumentWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerDocumentWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerDocumentWriter = false;
          }      
        }

        return userIsSchedulerDocumentWriter;
    }

    public GetDocumentChangeHistoriesForDocument(documentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<DocumentChangeHistoryData[]> {
        return this.documentChangeHistoryService.GetDocumentChangeHistoryList({
            documentId: documentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full DocumentData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the DocumentData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when DocumentTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveDocument(raw: any): DocumentData {
    if (!raw) return raw;

    //
    // Create a DocumentData object instance with correct prototype
    //
    const revived = Object.create(DocumentData.prototype) as DocumentData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._documentChangeHistories = null;
    (revived as any)._documentChangeHistoriesPromise = null;
    (revived as any)._documentChangeHistoriesSubject = new BehaviorSubject<DocumentChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadDocumentXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).DocumentChangeHistories$ = (revived as any)._documentChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._documentChangeHistories === null && (revived as any)._documentChangeHistoriesPromise === null) {
                (revived as any).loadDocumentChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._documentChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<DocumentData> | null>(null);

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

  private ReviveDocumentList(rawList: any[]): DocumentData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveDocument(raw));
  }

}
