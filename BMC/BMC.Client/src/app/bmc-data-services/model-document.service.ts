/*

   GENERATED SERVICE FOR THE MODELDOCUMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ModelDocument table.

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
import { ProjectData } from './project.service';
import { ModelDocumentChangeHistoryService, ModelDocumentChangeHistoryData } from './model-document-change-history.service';
import { ModelSubFileService, ModelSubFileData } from './model-sub-file.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ModelDocumentQueryParameters {
    projectId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    sourceFormat: string | null | undefined = null;
    sourceFileName: string | null | undefined = null;
    sourceFileFileName: string | null | undefined = null;
    sourceFileSize: bigint | number | null | undefined = null;
    sourceFileMimeType: string | null | undefined = null;
    author: string | null | undefined = null;
    totalPartCount: bigint | number | null | undefined = null;
    totalStepCount: bigint | number | null | undefined = null;
    studioVersion: string | null | undefined = null;
    instructionSettingsXml: string | null | undefined = null;
    errorPartList: string | null | undefined = null;
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
export class ModelDocumentSubmitData {
    id!: bigint | number;
    projectId: bigint | number | null = null;
    name!: string;
    description: string | null = null;
    sourceFormat!: string;
    sourceFileName: string | null = null;
    sourceFileFileName: string | null = null;
    sourceFileSize: bigint | number | null = null;
    sourceFileData: string | null = null;
    sourceFileMimeType: string | null = null;
    author: string | null = null;
    totalPartCount: bigint | number | null = null;
    totalStepCount: bigint | number | null = null;
    studioVersion: string | null = null;
    instructionSettingsXml: string | null = null;
    errorPartList: string | null = null;
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

export class ModelDocumentBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ModelDocumentChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `modelDocument.ModelDocumentChildren$` — use with `| async` in templates
//        • Promise:    `modelDocument.ModelDocumentChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="modelDocument.ModelDocumentChildren$ | async"`), or
//        • Access the promise getter (`modelDocument.ModelDocumentChildren` or `await modelDocument.ModelDocumentChildren`)
//    - Simply reading `modelDocument.ModelDocumentChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await modelDocument.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ModelDocumentData {
    id!: bigint | number;
    projectId!: bigint | number;
    name!: string;
    description!: string | null;
    sourceFormat!: string;
    sourceFileName!: string | null;
    sourceFileFileName!: string | null;
    sourceFileSize!: bigint | number;
    sourceFileData!: string | null;
    sourceFileMimeType!: string | null;
    author!: string | null;
    totalPartCount!: bigint | number;
    totalStepCount!: bigint | number;
    studioVersion!: string | null;
    instructionSettingsXml!: string | null;
    errorPartList!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    project: ProjectData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _modelDocumentChangeHistories: ModelDocumentChangeHistoryData[] | null = null;
    private _modelDocumentChangeHistoriesPromise: Promise<ModelDocumentChangeHistoryData[]> | null  = null;
    private _modelDocumentChangeHistoriesSubject = new BehaviorSubject<ModelDocumentChangeHistoryData[] | null>(null);

                
    private _modelSubFiles: ModelSubFileData[] | null = null;
    private _modelSubFilesPromise: Promise<ModelSubFileData[]> | null  = null;
    private _modelSubFilesSubject = new BehaviorSubject<ModelSubFileData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ModelDocumentData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ModelDocumentData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ModelDocumentData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ModelDocumentChangeHistories$ = this._modelDocumentChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._modelDocumentChangeHistories === null && this._modelDocumentChangeHistoriesPromise === null) {
            this.loadModelDocumentChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _modelDocumentChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ModelDocumentChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._modelDocumentChangeHistoriesCount$ === null) {
            this._modelDocumentChangeHistoriesCount$ = ModelDocumentChangeHistoryService.Instance.GetModelDocumentChangeHistoriesRowCount({modelDocumentId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._modelDocumentChangeHistoriesCount$;
    }



    public ModelSubFiles$ = this._modelSubFilesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._modelSubFiles === null && this._modelSubFilesPromise === null) {
            this.loadModelSubFiles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _modelSubFilesCount$: Observable<bigint | number> | null = null;
    public get ModelSubFilesCount$(): Observable<bigint | number> {
        if (this._modelSubFilesCount$ === null) {
            this._modelSubFilesCount$ = ModelSubFileService.Instance.GetModelSubFilesRowCount({modelDocumentId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._modelSubFilesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ModelDocumentData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.modelDocument.Reload();
  //
  //  Non Async:
  //
  //     modelDocument[0].Reload().then(x => {
  //        this.modelDocument = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ModelDocumentService.Instance.GetModelDocument(this.id, includeRelations)
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
     this._modelDocumentChangeHistories = null;
     this._modelDocumentChangeHistoriesPromise = null;
     this._modelDocumentChangeHistoriesSubject.next(null);
     this._modelDocumentChangeHistoriesCount$ = null;

     this._modelSubFiles = null;
     this._modelSubFilesPromise = null;
     this._modelSubFilesSubject.next(null);
     this._modelSubFilesCount$ = null;

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
     * Gets the ModelDocumentChangeHistories for this ModelDocument.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.modelDocument.ModelDocumentChangeHistories.then(modelDocuments => { ... })
     *   or
     *   await this.modelDocument.modelDocuments
     *
    */
    public get ModelDocumentChangeHistories(): Promise<ModelDocumentChangeHistoryData[]> {
        if (this._modelDocumentChangeHistories !== null) {
            return Promise.resolve(this._modelDocumentChangeHistories);
        }

        if (this._modelDocumentChangeHistoriesPromise !== null) {
            return this._modelDocumentChangeHistoriesPromise;
        }

        // Start the load
        this.loadModelDocumentChangeHistories();

        return this._modelDocumentChangeHistoriesPromise!;
    }



    private loadModelDocumentChangeHistories(): void {

        this._modelDocumentChangeHistoriesPromise = lastValueFrom(
            ModelDocumentService.Instance.GetModelDocumentChangeHistoriesForModelDocument(this.id)
        )
        .then(ModelDocumentChangeHistories => {
            this._modelDocumentChangeHistories = ModelDocumentChangeHistories ?? [];
            this._modelDocumentChangeHistoriesSubject.next(this._modelDocumentChangeHistories);
            return this._modelDocumentChangeHistories;
         })
        .catch(err => {
            this._modelDocumentChangeHistories = [];
            this._modelDocumentChangeHistoriesSubject.next(this._modelDocumentChangeHistories);
            throw err;
        })
        .finally(() => {
            this._modelDocumentChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ModelDocumentChangeHistory. Call after mutations to force refresh.
     */
    public ClearModelDocumentChangeHistoriesCache(): void {
        this._modelDocumentChangeHistories = null;
        this._modelDocumentChangeHistoriesPromise = null;
        this._modelDocumentChangeHistoriesSubject.next(this._modelDocumentChangeHistories);      // Emit to observable
    }

    public get HasModelDocumentChangeHistories(): Promise<boolean> {
        return this.ModelDocumentChangeHistories.then(modelDocumentChangeHistories => modelDocumentChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ModelSubFiles for this ModelDocument.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.modelDocument.ModelSubFiles.then(modelDocuments => { ... })
     *   or
     *   await this.modelDocument.modelDocuments
     *
    */
    public get ModelSubFiles(): Promise<ModelSubFileData[]> {
        if (this._modelSubFiles !== null) {
            return Promise.resolve(this._modelSubFiles);
        }

        if (this._modelSubFilesPromise !== null) {
            return this._modelSubFilesPromise;
        }

        // Start the load
        this.loadModelSubFiles();

        return this._modelSubFilesPromise!;
    }



    private loadModelSubFiles(): void {

        this._modelSubFilesPromise = lastValueFrom(
            ModelDocumentService.Instance.GetModelSubFilesForModelDocument(this.id)
        )
        .then(ModelSubFiles => {
            this._modelSubFiles = ModelSubFiles ?? [];
            this._modelSubFilesSubject.next(this._modelSubFiles);
            return this._modelSubFiles;
         })
        .catch(err => {
            this._modelSubFiles = [];
            this._modelSubFilesSubject.next(this._modelSubFiles);
            throw err;
        })
        .finally(() => {
            this._modelSubFilesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ModelSubFile. Call after mutations to force refresh.
     */
    public ClearModelSubFilesCache(): void {
        this._modelSubFiles = null;
        this._modelSubFilesPromise = null;
        this._modelSubFilesSubject.next(this._modelSubFiles);      // Emit to observable
    }

    public get HasModelSubFiles(): Promise<boolean> {
        return this.ModelSubFiles.then(modelSubFiles => modelSubFiles.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (modelDocument.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await modelDocument.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ModelDocumentData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ModelDocumentData>> {
        const info = await lastValueFrom(
            ModelDocumentService.Instance.GetModelDocumentChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ModelDocumentData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ModelDocumentData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ModelDocumentSubmitData {
        return ModelDocumentService.Instance.ConvertToModelDocumentSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ModelDocumentService extends SecureEndpointBase {

    private static _instance: ModelDocumentService;
    private listCache: Map<string, Observable<Array<ModelDocumentData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ModelDocumentBasicListData>>>;
    private recordCache: Map<string, Observable<ModelDocumentData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private modelDocumentChangeHistoryService: ModelDocumentChangeHistoryService,
        private modelSubFileService: ModelSubFileService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ModelDocumentData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ModelDocumentBasicListData>>>();
        this.recordCache = new Map<string, Observable<ModelDocumentData>>();

        ModelDocumentService._instance = this;
    }

    public static get Instance(): ModelDocumentService {
      return ModelDocumentService._instance;
    }


    public ClearListCaches(config: ModelDocumentQueryParameters | null = null) {

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


    public ConvertToModelDocumentSubmitData(data: ModelDocumentData): ModelDocumentSubmitData {

        let output = new ModelDocumentSubmitData();

        output.id = data.id;
        output.projectId = data.projectId;
        output.name = data.name;
        output.description = data.description;
        output.sourceFormat = data.sourceFormat;
        output.sourceFileName = data.sourceFileName;
        output.sourceFileFileName = data.sourceFileFileName;
        output.sourceFileSize = data.sourceFileSize;
        output.sourceFileData = data.sourceFileData;
        output.sourceFileMimeType = data.sourceFileMimeType;
        output.author = data.author;
        output.totalPartCount = data.totalPartCount;
        output.totalStepCount = data.totalStepCount;
        output.studioVersion = data.studioVersion;
        output.instructionSettingsXml = data.instructionSettingsXml;
        output.errorPartList = data.errorPartList;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetModelDocument(id: bigint | number, includeRelations: boolean = true) : Observable<ModelDocumentData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const modelDocument$ = this.requestModelDocument(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ModelDocument", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, modelDocument$);

            return modelDocument$;
        }

        return this.recordCache.get(configHash) as Observable<ModelDocumentData>;
    }

    private requestModelDocument(id: bigint | number, includeRelations: boolean = true) : Observable<ModelDocumentData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ModelDocumentData>(this.baseUrl + 'api/ModelDocument/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveModelDocument(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestModelDocument(id, includeRelations));
            }));
    }

    public GetModelDocumentList(config: ModelDocumentQueryParameters | any = null) : Observable<Array<ModelDocumentData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const modelDocumentList$ = this.requestModelDocumentList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ModelDocument list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, modelDocumentList$);

            return modelDocumentList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ModelDocumentData>>;
    }


    private requestModelDocumentList(config: ModelDocumentQueryParameters | any) : Observable <Array<ModelDocumentData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ModelDocumentData>>(this.baseUrl + 'api/ModelDocuments', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveModelDocumentList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestModelDocumentList(config));
            }));
    }

    public GetModelDocumentsRowCount(config: ModelDocumentQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const modelDocumentsRowCount$ = this.requestModelDocumentsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ModelDocuments row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, modelDocumentsRowCount$);

            return modelDocumentsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestModelDocumentsRowCount(config: ModelDocumentQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ModelDocuments/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestModelDocumentsRowCount(config));
            }));
    }

    public GetModelDocumentsBasicListData(config: ModelDocumentQueryParameters | any = null) : Observable<Array<ModelDocumentBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const modelDocumentsBasicListData$ = this.requestModelDocumentsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ModelDocuments basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, modelDocumentsBasicListData$);

            return modelDocumentsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ModelDocumentBasicListData>>;
    }


    private requestModelDocumentsBasicListData(config: ModelDocumentQueryParameters | any) : Observable<Array<ModelDocumentBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ModelDocumentBasicListData>>(this.baseUrl + 'api/ModelDocuments/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestModelDocumentsBasicListData(config));
            }));

    }


    public PutModelDocument(id: bigint | number, modelDocument: ModelDocumentSubmitData) : Observable<ModelDocumentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ModelDocumentData>(this.baseUrl + 'api/ModelDocument/' + id.toString(), modelDocument, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveModelDocument(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutModelDocument(id, modelDocument));
            }));
    }


    public PostModelDocument(modelDocument: ModelDocumentSubmitData) : Observable<ModelDocumentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ModelDocumentData>(this.baseUrl + 'api/ModelDocument', modelDocument, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveModelDocument(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostModelDocument(modelDocument));
            }));
    }

  
    public DeleteModelDocument(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ModelDocument/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteModelDocument(id));
            }));
    }

    public RollbackModelDocument(id: bigint | number, versionNumber: bigint | number) : Observable<ModelDocumentData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ModelDocumentData>(this.baseUrl + 'api/ModelDocument/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveModelDocument(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackModelDocument(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ModelDocument.
     */
    public GetModelDocumentChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ModelDocumentData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ModelDocumentData>>(this.baseUrl + 'api/ModelDocument/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetModelDocumentChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ModelDocument.
     */
    public GetModelDocumentAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ModelDocumentData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ModelDocumentData>[]>(this.baseUrl + 'api/ModelDocument/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetModelDocumentAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ModelDocument.
     */
    public GetModelDocumentVersion(id: bigint | number, version: number): Observable<ModelDocumentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ModelDocumentData>(this.baseUrl + 'api/ModelDocument/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveModelDocument(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetModelDocumentVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ModelDocument at a specific point in time.
     */
    public GetModelDocumentStateAtTime(id: bigint | number, time: string): Observable<ModelDocumentData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ModelDocumentData>(this.baseUrl + 'api/ModelDocument/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveModelDocument(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetModelDocumentStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ModelDocumentQueryParameters | any): string {

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

    public userIsBMCModelDocumentReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCModelDocumentReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ModelDocuments
        //
        if (userIsBMCModelDocumentReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCModelDocumentReader = user.readPermission >= 1;
            } else {
                userIsBMCModelDocumentReader = false;
            }
        }

        return userIsBMCModelDocumentReader;
    }


    public userIsBMCModelDocumentWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCModelDocumentWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ModelDocuments
        //
        if (userIsBMCModelDocumentWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCModelDocumentWriter = user.writePermission >= 1;
          } else {
            userIsBMCModelDocumentWriter = false;
          }      
        }

        return userIsBMCModelDocumentWriter;
    }

    public GetModelDocumentChangeHistoriesForModelDocument(modelDocumentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ModelDocumentChangeHistoryData[]> {
        return this.modelDocumentChangeHistoryService.GetModelDocumentChangeHistoryList({
            modelDocumentId: modelDocumentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetModelSubFilesForModelDocument(modelDocumentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ModelSubFileData[]> {
        return this.modelSubFileService.GetModelSubFileList({
            modelDocumentId: modelDocumentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ModelDocumentData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ModelDocumentData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ModelDocumentTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveModelDocument(raw: any): ModelDocumentData {
    if (!raw) return raw;

    //
    // Create a ModelDocumentData object instance with correct prototype
    //
    const revived = Object.create(ModelDocumentData.prototype) as ModelDocumentData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._modelDocumentChangeHistories = null;
    (revived as any)._modelDocumentChangeHistoriesPromise = null;
    (revived as any)._modelDocumentChangeHistoriesSubject = new BehaviorSubject<ModelDocumentChangeHistoryData[] | null>(null);

    (revived as any)._modelSubFiles = null;
    (revived as any)._modelSubFilesPromise = null;
    (revived as any)._modelSubFilesSubject = new BehaviorSubject<ModelSubFileData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadModelDocumentXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ModelDocumentChangeHistories$ = (revived as any)._modelDocumentChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._modelDocumentChangeHistories === null && (revived as any)._modelDocumentChangeHistoriesPromise === null) {
                (revived as any).loadModelDocumentChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._modelDocumentChangeHistoriesCount$ = null;


    (revived as any).ModelSubFiles$ = (revived as any)._modelSubFilesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._modelSubFiles === null && (revived as any)._modelSubFilesPromise === null) {
                (revived as any).loadModelSubFiles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._modelSubFilesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ModelDocumentData> | null>(null);

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

  private ReviveModelDocumentList(rawList: any[]): ModelDocumentData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveModelDocument(raw));
  }

}
