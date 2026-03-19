/*

   GENERATED SERVICE FOR THE DOCUMENTFOLDERCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the DocumentFolderChangeHistory table.

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
import { DocumentFolderData } from './document-folder.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class DocumentFolderChangeHistoryQueryParameters {
    documentFolderId: bigint | number | null | undefined = null;
    versionNumber: bigint | number | null | undefined = null;
    timeStamp: string | null | undefined = null;        // ISO 8601 (full datetime)
    userId: bigint | number | null | undefined = null;
    data: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class DocumentFolderChangeHistorySubmitData {
    id!: bigint | number;
    documentFolderId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
}


export class DocumentFolderChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. DocumentFolderChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `documentFolderChangeHistory.DocumentFolderChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `documentFolderChangeHistory.DocumentFolderChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="documentFolderChangeHistory.DocumentFolderChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`documentFolderChangeHistory.DocumentFolderChangeHistoryChildren` or `await documentFolderChangeHistory.DocumentFolderChangeHistoryChildren`)
//    - Simply reading `documentFolderChangeHistory.DocumentFolderChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await documentFolderChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class DocumentFolderChangeHistoryData {
    id!: bigint | number;
    documentFolderId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
    documentFolder: DocumentFolderData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any DocumentFolderChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.documentFolderChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     documentFolderChangeHistory[0].Reload().then(x => {
  //        this.documentFolderChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      DocumentFolderChangeHistoryService.Instance.GetDocumentFolderChangeHistory(this.id, includeRelations)
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
     * Updates the state of this DocumentFolderChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this DocumentFolderChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): DocumentFolderChangeHistorySubmitData {
        return DocumentFolderChangeHistoryService.Instance.ConvertToDocumentFolderChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class DocumentFolderChangeHistoryService extends SecureEndpointBase {

    private static _instance: DocumentFolderChangeHistoryService;
    private listCache: Map<string, Observable<Array<DocumentFolderChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<DocumentFolderChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<DocumentFolderChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<DocumentFolderChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<DocumentFolderChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<DocumentFolderChangeHistoryData>>();

        DocumentFolderChangeHistoryService._instance = this;
    }

    public static get Instance(): DocumentFolderChangeHistoryService {
      return DocumentFolderChangeHistoryService._instance;
    }


    public ClearListCaches(config: DocumentFolderChangeHistoryQueryParameters | null = null) {

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


    public ConvertToDocumentFolderChangeHistorySubmitData(data: DocumentFolderChangeHistoryData): DocumentFolderChangeHistorySubmitData {

        let output = new DocumentFolderChangeHistorySubmitData();

        output.id = data.id;
        output.documentFolderId = data.documentFolderId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetDocumentFolderChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentFolderChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const documentFolderChangeHistory$ = this.requestDocumentFolderChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentFolderChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, documentFolderChangeHistory$);

            return documentFolderChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<DocumentFolderChangeHistoryData>;
    }

    private requestDocumentFolderChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentFolderChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentFolderChangeHistoryData>(this.baseUrl + 'api/DocumentFolderChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveDocumentFolderChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentFolderChangeHistory(id, includeRelations));
            }));
    }

    public GetDocumentFolderChangeHistoryList(config: DocumentFolderChangeHistoryQueryParameters | any = null) : Observable<Array<DocumentFolderChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const documentFolderChangeHistoryList$ = this.requestDocumentFolderChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentFolderChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, documentFolderChangeHistoryList$);

            return documentFolderChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<DocumentFolderChangeHistoryData>>;
    }


    private requestDocumentFolderChangeHistoryList(config: DocumentFolderChangeHistoryQueryParameters | any) : Observable <Array<DocumentFolderChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentFolderChangeHistoryData>>(this.baseUrl + 'api/DocumentFolderChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveDocumentFolderChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentFolderChangeHistoryList(config));
            }));
    }

    public GetDocumentFolderChangeHistoriesRowCount(config: DocumentFolderChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const documentFolderChangeHistoriesRowCount$ = this.requestDocumentFolderChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentFolderChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, documentFolderChangeHistoriesRowCount$);

            return documentFolderChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestDocumentFolderChangeHistoriesRowCount(config: DocumentFolderChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/DocumentFolderChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentFolderChangeHistoriesRowCount(config));
            }));
    }

    public GetDocumentFolderChangeHistoriesBasicListData(config: DocumentFolderChangeHistoryQueryParameters | any = null) : Observable<Array<DocumentFolderChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const documentFolderChangeHistoriesBasicListData$ = this.requestDocumentFolderChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentFolderChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, documentFolderChangeHistoriesBasicListData$);

            return documentFolderChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<DocumentFolderChangeHistoryBasicListData>>;
    }


    private requestDocumentFolderChangeHistoriesBasicListData(config: DocumentFolderChangeHistoryQueryParameters | any) : Observable<Array<DocumentFolderChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentFolderChangeHistoryBasicListData>>(this.baseUrl + 'api/DocumentFolderChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentFolderChangeHistoriesBasicListData(config));
            }));

    }


    public PutDocumentFolderChangeHistory(id: bigint | number, documentFolderChangeHistory: DocumentFolderChangeHistorySubmitData) : Observable<DocumentFolderChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentFolderChangeHistoryData>(this.baseUrl + 'api/DocumentFolderChangeHistory/' + id.toString(), documentFolderChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentFolderChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutDocumentFolderChangeHistory(id, documentFolderChangeHistory));
            }));
    }


    public PostDocumentFolderChangeHistory(documentFolderChangeHistory: DocumentFolderChangeHistorySubmitData) : Observable<DocumentFolderChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<DocumentFolderChangeHistoryData>(this.baseUrl + 'api/DocumentFolderChangeHistory', documentFolderChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentFolderChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostDocumentFolderChangeHistory(documentFolderChangeHistory));
            }));
    }

  
    public DeleteDocumentFolderChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/DocumentFolderChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteDocumentFolderChangeHistory(id));
            }));
    }


    private getConfigHash(config: DocumentFolderChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerDocumentFolderChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerDocumentFolderChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.DocumentFolderChangeHistories
        //
        if (userIsSchedulerDocumentFolderChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerDocumentFolderChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerDocumentFolderChangeHistoryReader = false;
            }
        }

        return userIsSchedulerDocumentFolderChangeHistoryReader;
    }


    public userIsSchedulerDocumentFolderChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerDocumentFolderChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.DocumentFolderChangeHistories
        //
        if (userIsSchedulerDocumentFolderChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerDocumentFolderChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerDocumentFolderChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerDocumentFolderChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full DocumentFolderChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the DocumentFolderChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when DocumentFolderChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveDocumentFolderChangeHistory(raw: any): DocumentFolderChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a DocumentFolderChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(DocumentFolderChangeHistoryData.prototype) as DocumentFolderChangeHistoryData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //

    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadDocumentFolderChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveDocumentFolderChangeHistoryList(rawList: any[]): DocumentFolderChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveDocumentFolderChangeHistory(raw));
  }

}
