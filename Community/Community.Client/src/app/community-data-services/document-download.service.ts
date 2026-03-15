/*

   GENERATED SERVICE FOR THE DOCUMENTDOWNLOAD TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the DocumentDownload table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class DocumentDownloadQueryParameters {
    Id: bigint | number | null | undefined = null;
    Title: string | null | undefined = null;
    Description: string | null | undefined = null;
    FilePath: string | null | undefined = null;
    FileName: string | null | undefined = null;
    MimeType: string | null | undefined = null;
    FileSizeBytes: bigint | number | null | undefined = null;
    CategoryName: string | null | undefined = null;
    DocumentDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    IsPublished: boolean | null | undefined = null;
    Sequence: bigint | number | null | undefined = null;
    ObjectGuid: string | null | undefined = null;
    Active: boolean | null | undefined = null;
    Deleted: boolean | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class DocumentDownloadSubmitData {
    Id: bigint | number | null = null;
    Title: string | null = null;
    Description: string | null = null;
    FilePath: string | null = null;
    FileName: string | null = null;
    MimeType: string | null = null;
    FileSizeBytes: bigint | number | null = null;
    CategoryName: string | null = null;
    DocumentDate: string | null = null;     // ISO 8601 (full datetime)
    IsPublished: boolean | null = null;
    Sequence: bigint | number | null = null;
    Active: boolean | null = null;
    Deleted: boolean | null = null;
}


export class DocumentDownloadBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. DocumentDownloadChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `documentDownload.DocumentDownloadChildren$` — use with `| async` in templates
//        • Promise:    `documentDownload.DocumentDownloadChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="documentDownload.DocumentDownloadChildren$ | async"`), or
//        • Access the promise getter (`documentDownload.DocumentDownloadChildren` or `await documentDownload.DocumentDownloadChildren`)
//    - Simply reading `documentDownload.DocumentDownloadChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await documentDownload.Reload()` to refresh the entire object and clear all lazy caches.
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
export class DocumentDownloadData {
    Id!: bigint | number;
    Title!: string | null;
    Description!: string | null;
    FilePath!: string | null;
    FileName!: string | null;
    MimeType!: string | null;
    FileSizeBytes!: bigint | number;
    CategoryName!: string | null;
    DocumentDate!: string | null;   // ISO 8601 (full datetime)
    IsPublished!: boolean | null;
    Sequence!: bigint | number;
    ObjectGuid!: string | null;
    Active!: boolean | null;
    Deleted!: boolean | null;

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
  // Promise based reload method to allow rebuilding of any DocumentDownloadData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.documentDownload.Reload();
  //
  //  Non Async:
  //
  //     documentDownload[0].Reload().then(x => {
  //        this.documentDownload = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      DocumentDownloadService.Instance.GetDocumentDownload(this.id, includeRelations)
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
     * Updates the state of this DocumentDownloadData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this DocumentDownloadData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): DocumentDownloadSubmitData {
        return DocumentDownloadService.Instance.ConvertToDocumentDownloadSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class DocumentDownloadService extends SecureEndpointBase {

    private static _instance: DocumentDownloadService;
    private listCache: Map<string, Observable<Array<DocumentDownloadData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<DocumentDownloadBasicListData>>>;
    private recordCache: Map<string, Observable<DocumentDownloadData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<DocumentDownloadData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<DocumentDownloadBasicListData>>>();
        this.recordCache = new Map<string, Observable<DocumentDownloadData>>();

        DocumentDownloadService._instance = this;
    }

    public static get Instance(): DocumentDownloadService {
      return DocumentDownloadService._instance;
    }


    public ClearListCaches(config: DocumentDownloadQueryParameters | null = null) {

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


    public ConvertToDocumentDownloadSubmitData(data: DocumentDownloadData): DocumentDownloadSubmitData {

        let output = new DocumentDownloadSubmitData();

        output.Id = data.Id;
        output.Title = data.Title;
        output.Description = data.Description;
        output.FilePath = data.FilePath;
        output.FileName = data.FileName;
        output.MimeType = data.MimeType;
        output.FileSizeBytes = data.FileSizeBytes;
        output.CategoryName = data.CategoryName;
        output.DocumentDate = data.DocumentDate;
        output.IsPublished = data.IsPublished;
        output.Sequence = data.Sequence;
        output.Active = data.Active;
        output.Deleted = data.Deleted;

        return output;
    }

    public GetDocumentDownload(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentDownloadData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const documentDownload$ = this.requestDocumentDownload(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentDownload", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, documentDownload$);

            return documentDownload$;
        }

        return this.recordCache.get(configHash) as Observable<DocumentDownloadData>;
    }

    private requestDocumentDownload(id: bigint | number, includeRelations: boolean = true) : Observable<DocumentDownloadData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<DocumentDownloadData>(this.baseUrl + 'api/DocumentDownload/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveDocumentDownload(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentDownload(id, includeRelations));
            }));
    }

    public GetDocumentDownloadList(config: DocumentDownloadQueryParameters | any = null) : Observable<Array<DocumentDownloadData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const documentDownloadList$ = this.requestDocumentDownloadList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentDownload list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, documentDownloadList$);

            return documentDownloadList$;
        }

        return this.listCache.get(configHash) as Observable<Array<DocumentDownloadData>>;
    }


    private requestDocumentDownloadList(config: DocumentDownloadQueryParameters | any) : Observable <Array<DocumentDownloadData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentDownloadData>>(this.baseUrl + 'api/DocumentDownloads', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveDocumentDownloadList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentDownloadList(config));
            }));
    }

    public GetDocumentDownloadsRowCount(config: DocumentDownloadQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const documentDownloadsRowCount$ = this.requestDocumentDownloadsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get DocumentDownloads row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, documentDownloadsRowCount$);

            return documentDownloadsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestDocumentDownloadsRowCount(config: DocumentDownloadQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/DocumentDownloads/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentDownloadsRowCount(config));
            }));
    }

    public GetDocumentDownloadsBasicListData(config: DocumentDownloadQueryParameters | any = null) : Observable<Array<DocumentDownloadBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const documentDownloadsBasicListData$ = this.requestDocumentDownloadsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get DocumentDownloads basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, documentDownloadsBasicListData$);

            return documentDownloadsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<DocumentDownloadBasicListData>>;
    }


    private requestDocumentDownloadsBasicListData(config: DocumentDownloadQueryParameters | any) : Observable<Array<DocumentDownloadBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<DocumentDownloadBasicListData>>(this.baseUrl + 'api/DocumentDownloads/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestDocumentDownloadsBasicListData(config));
            }));

    }


    public PutDocumentDownload(id: bigint | number, documentDownload: DocumentDownloadSubmitData) : Observable<DocumentDownloadData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<DocumentDownloadData>(this.baseUrl + 'api/DocumentDownload/' + id.toString(), documentDownload, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentDownload(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutDocumentDownload(id, documentDownload));
            }));
    }


    public PostDocumentDownload(documentDownload: DocumentDownloadSubmitData) : Observable<DocumentDownloadData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<DocumentDownloadData>(this.baseUrl + 'api/DocumentDownload', documentDownload, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveDocumentDownload(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostDocumentDownload(documentDownload));
            }));
    }

  
    public DeleteDocumentDownload(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/DocumentDownload/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteDocumentDownload(id));
            }));
    }


    private getConfigHash(config: DocumentDownloadQueryParameters | any): string {

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

    public userIsCommunityDocumentDownloadReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityDocumentDownloadReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.DocumentDownloads
        //
        if (userIsCommunityDocumentDownloadReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityDocumentDownloadReader = user.readPermission >= 1;
            } else {
                userIsCommunityDocumentDownloadReader = false;
            }
        }

        return userIsCommunityDocumentDownloadReader;
    }


    public userIsCommunityDocumentDownloadWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityDocumentDownloadWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.DocumentDownloads
        //
        if (userIsCommunityDocumentDownloadWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityDocumentDownloadWriter = user.writePermission >= 10;
          } else {
            userIsCommunityDocumentDownloadWriter = false;
          }      
        }

        return userIsCommunityDocumentDownloadWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full DocumentDownloadData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the DocumentDownloadData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when DocumentDownloadTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveDocumentDownload(raw: any): DocumentDownloadData {
    if (!raw) return raw;

    //
    // Create a DocumentDownloadData object instance with correct prototype
    //
    const revived = Object.create(DocumentDownloadData.prototype) as DocumentDownloadData;

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
    // 2. But private methods (loadDocumentDownloadXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveDocumentDownloadList(rawList: any[]): DocumentDownloadData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveDocumentDownload(raw));
  }

}
