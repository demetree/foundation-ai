/*

   GENERATED SERVICE FOR THE DOCUMENTSHARELINKCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the DocumentShareLinkChangeHistory table.

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
import { DocumentShareLinkData } from './document-share-link.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class DocumentShareLinkChangeHistoryQueryParameters {
    documentShareLinkId: bigint | number | null | undefined = null;
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
export class DocumentShareLinkChangeHistorySubmitData {
    id!: bigint | number;
    documentShareLinkId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
}


export class DocumentShareLinkChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. DocumentShareLinkChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `documentShareLinkChangeHistory.DocumentShareLinkChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `documentShareLinkChangeHistory.DocumentShareLinkChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="documentShareLinkChangeHistory.DocumentShareLinkChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`documentShareLinkChangeHistory.DocumentShareLinkChangeHistoryChildren` or `await documentShareLinkChangeHistory.DocumentShareLinkChangeHistoryChildren`)
//    - Simply reading `documentShareLinkChangeHistory.DocumentShareLinkChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await documentShareLinkChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class DocumentShareLinkChangeHistoryData {
    id!: bigint | number;
    documentShareLinkId!: bigint | number;
    versionNumber!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    userId!: bigint | number;
    data!: string;
    documentShareLink: DocumentShareLinkData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any DocumentShareLinkChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.documentShareLinkChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     documentShareLinkChangeHistory[0].Reload().then(x => {
  //        this.documentShareLinkChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      DocumentShareLinkChangeHistoryService.Instance.GetDocumentShareLinkChangeHistory(this.id, includeRelations)
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
     * Updates the state of this DocumentShareLinkChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this DocumentShareLinkChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): DocumentShareLinkChangeHistorySubmitData {
        return DocumentShareLinkChangeHistoryService.Instance.ConvertToDocumentShareLinkChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class DocumentShareLinkChangeHistoryService extends SecureEndpointBase {

    private static _instance: DocumentShareLinkChangeHistoryService;
    private listCache: Map<string, Observable<Array<DocumentShareLinkChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<DocumentShareLinkChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<DocumentShareLinkChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<DocumentShareLinkChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<DocumentShareLinkChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<DocumentShareLinkChangeHistoryData>>();

        DocumentShareLinkChangeHistoryService._instance = this;
    }

    public static get Instance(): DocumentShareLinkChangeHistoryService {
      return DocumentShareLinkChangeHistoryService._instance;
    }


    public ClearListCaches(config: DocumentShareLinkChangeHistoryQueryParameters | null = null) {

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


    public ConvertToDocumentShareLinkChangeHistorySubmitData(data: DocumentShareLinkChangeHistoryData): DocumentShareLinkChangeHistorySubmitData {

        let output = new DocumentShareLinkChangeHistorySubmitData();

        output.id = data.id;
        output.documentShareLinkId = data.documentShareLinkId;
        output.versionNumber = data.versionNumber;
        output.timeStamp = data.timeStamp;
        output.userId = data.userId;
        output.data = data.data;

        return output;
    }

    public GetDocumentShareLinkChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentShareLinkChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const documentShareLinkChangeHistory$ = this.requestDocumentShareLinkChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentShareLinkChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, documentShareLinkChangeHistory$);

            return documentShareLinkChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<DocumentShareLinkChangeHistoryData>;
    }

    private requestDocumentShareLinkChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentShareLinkChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentShareLinkChangeHistoryData>(this.baseUrl + 'api/DocumentShareLinkChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveDocumentShareLinkChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentShareLinkChangeHistory(id, includeRelations));
            }));
    }

    public GetDocumentShareLinkChangeHistoryList(config: DocumentShareLinkChangeHistoryQueryParameters | any = null) : Observable<Array<DocumentShareLinkChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const documentShareLinkChangeHistoryList$ = this.requestDocumentShareLinkChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentShareLinkChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, documentShareLinkChangeHistoryList$);

            return documentShareLinkChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<DocumentShareLinkChangeHistoryData>>;
    }


    private requestDocumentShareLinkChangeHistoryList(config: DocumentShareLinkChangeHistoryQueryParameters | any) : Observable <Array<DocumentShareLinkChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentShareLinkChangeHistoryData>>(this.baseUrl + 'api/DocumentShareLinkChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveDocumentShareLinkChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentShareLinkChangeHistoryList(config));
            }));
    }

    public GetDocumentShareLinkChangeHistoriesRowCount(config: DocumentShareLinkChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const documentShareLinkChangeHistoriesRowCount$ = this.requestDocumentShareLinkChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentShareLinkChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, documentShareLinkChangeHistoriesRowCount$);

            return documentShareLinkChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestDocumentShareLinkChangeHistoriesRowCount(config: DocumentShareLinkChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/DocumentShareLinkChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentShareLinkChangeHistoriesRowCount(config));
            }));
    }

    public GetDocumentShareLinkChangeHistoriesBasicListData(config: DocumentShareLinkChangeHistoryQueryParameters | any = null) : Observable<Array<DocumentShareLinkChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const documentShareLinkChangeHistoriesBasicListData$ = this.requestDocumentShareLinkChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentShareLinkChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, documentShareLinkChangeHistoriesBasicListData$);

            return documentShareLinkChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<DocumentShareLinkChangeHistoryBasicListData>>;
    }


    private requestDocumentShareLinkChangeHistoriesBasicListData(config: DocumentShareLinkChangeHistoryQueryParameters | any) : Observable<Array<DocumentShareLinkChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentShareLinkChangeHistoryBasicListData>>(this.baseUrl + 'api/DocumentShareLinkChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentShareLinkChangeHistoriesBasicListData(config));
            }));

    }


    public PutDocumentShareLinkChangeHistory(id: bigint | number, documentShareLinkChangeHistory: DocumentShareLinkChangeHistorySubmitData) : Observable<DocumentShareLinkChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentShareLinkChangeHistoryData>(this.baseUrl + 'api/DocumentShareLinkChangeHistory/' + id.toString(), documentShareLinkChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentShareLinkChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutDocumentShareLinkChangeHistory(id, documentShareLinkChangeHistory));
            }));
    }


    public PostDocumentShareLinkChangeHistory(documentShareLinkChangeHistory: DocumentShareLinkChangeHistorySubmitData) : Observable<DocumentShareLinkChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<DocumentShareLinkChangeHistoryData>(this.baseUrl + 'api/DocumentShareLinkChangeHistory', documentShareLinkChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentShareLinkChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostDocumentShareLinkChangeHistory(documentShareLinkChangeHistory));
            }));
    }

  
    public DeleteDocumentShareLinkChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/DocumentShareLinkChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteDocumentShareLinkChangeHistory(id));
            }));
    }


    private getConfigHash(config: DocumentShareLinkChangeHistoryQueryParameters | any): string {

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

    public userIsSchedulerDocumentShareLinkChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerDocumentShareLinkChangeHistoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.DocumentShareLinkChangeHistories
        //
        if (userIsSchedulerDocumentShareLinkChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerDocumentShareLinkChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsSchedulerDocumentShareLinkChangeHistoryReader = false;
            }
        }

        return userIsSchedulerDocumentShareLinkChangeHistoryReader;
    }


    public userIsSchedulerDocumentShareLinkChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerDocumentShareLinkChangeHistoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.DocumentShareLinkChangeHistories
        //
        if (userIsSchedulerDocumentShareLinkChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerDocumentShareLinkChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerDocumentShareLinkChangeHistoryWriter = false;
          }      
        }

        return userIsSchedulerDocumentShareLinkChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full DocumentShareLinkChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the DocumentShareLinkChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when DocumentShareLinkChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveDocumentShareLinkChangeHistory(raw: any): DocumentShareLinkChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a DocumentShareLinkChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(DocumentShareLinkChangeHistoryData.prototype) as DocumentShareLinkChangeHistoryData;

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
    // 2. But private methods (loadDocumentShareLinkChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveDocumentShareLinkChangeHistoryList(rawList: any[]): DocumentShareLinkChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveDocumentShareLinkChangeHistory(raw));
  }

}
