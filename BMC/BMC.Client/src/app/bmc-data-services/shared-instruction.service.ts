/*

   GENERATED SERVICE FOR THE SHAREDINSTRUCTION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SharedInstruction table.

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
import { BuildManualData } from './build-manual.service';
import { PublishedMocData } from './published-moc.service';
import { SharedInstructionChangeHistoryService, SharedInstructionChangeHistoryData } from './shared-instruction-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SharedInstructionQueryParameters {
    buildManualId: bigint | number | null | undefined = null;
    publishedMocId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    formatType: string | null | undefined = null;
    filePath: string | null | undefined = null;
    isPublished: boolean | null | undefined = null;
    publishedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    downloadCount: bigint | number | null | undefined = null;
    pageCount: bigint | number | null | undefined = null;
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
export class SharedInstructionSubmitData {
    id!: bigint | number;
    buildManualId: bigint | number | null = null;
    publishedMocId: bigint | number | null = null;
    name!: string;
    description: string | null = null;
    formatType!: string;
    filePath: string | null = null;
    isPublished!: boolean;
    publishedDate: string | null = null;     // ISO 8601 (full datetime)
    downloadCount!: bigint | number;
    pageCount: bigint | number | null = null;
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

export class SharedInstructionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SharedInstructionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `sharedInstruction.SharedInstructionChildren$` — use with `| async` in templates
//        • Promise:    `sharedInstruction.SharedInstructionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="sharedInstruction.SharedInstructionChildren$ | async"`), or
//        • Access the promise getter (`sharedInstruction.SharedInstructionChildren` or `await sharedInstruction.SharedInstructionChildren`)
//    - Simply reading `sharedInstruction.SharedInstructionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await sharedInstruction.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SharedInstructionData {
    id!: bigint | number;
    buildManualId!: bigint | number;
    publishedMocId!: bigint | number;
    name!: string;
    description!: string | null;
    formatType!: string;
    filePath!: string | null;
    isPublished!: boolean;
    publishedDate!: string | null;   // ISO 8601 (full datetime)
    downloadCount!: bigint | number;
    pageCount!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    buildManual: BuildManualData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    publishedMoc: PublishedMocData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _sharedInstructionChangeHistories: SharedInstructionChangeHistoryData[] | null = null;
    private _sharedInstructionChangeHistoriesPromise: Promise<SharedInstructionChangeHistoryData[]> | null  = null;
    private _sharedInstructionChangeHistoriesSubject = new BehaviorSubject<SharedInstructionChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<SharedInstructionData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<SharedInstructionData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SharedInstructionData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SharedInstructionChangeHistories$ = this._sharedInstructionChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._sharedInstructionChangeHistories === null && this._sharedInstructionChangeHistoriesPromise === null) {
            this.loadSharedInstructionChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _sharedInstructionChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get SharedInstructionChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._sharedInstructionChangeHistoriesCount$ === null) {
            this._sharedInstructionChangeHistoriesCount$ = SharedInstructionChangeHistoryService.Instance.GetSharedInstructionChangeHistoriesRowCount({sharedInstructionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._sharedInstructionChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SharedInstructionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.sharedInstruction.Reload();
  //
  //  Non Async:
  //
  //     sharedInstruction[0].Reload().then(x => {
  //        this.sharedInstruction = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SharedInstructionService.Instance.GetSharedInstruction(this.id, includeRelations)
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
     this._sharedInstructionChangeHistories = null;
     this._sharedInstructionChangeHistoriesPromise = null;
     this._sharedInstructionChangeHistoriesSubject.next(null);
     this._sharedInstructionChangeHistoriesCount$ = null;

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
     * Gets the SharedInstructionChangeHistories for this SharedInstruction.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.sharedInstruction.SharedInstructionChangeHistories.then(sharedInstructions => { ... })
     *   or
     *   await this.sharedInstruction.sharedInstructions
     *
    */
    public get SharedInstructionChangeHistories(): Promise<SharedInstructionChangeHistoryData[]> {
        if (this._sharedInstructionChangeHistories !== null) {
            return Promise.resolve(this._sharedInstructionChangeHistories);
        }

        if (this._sharedInstructionChangeHistoriesPromise !== null) {
            return this._sharedInstructionChangeHistoriesPromise;
        }

        // Start the load
        this.loadSharedInstructionChangeHistories();

        return this._sharedInstructionChangeHistoriesPromise!;
    }



    private loadSharedInstructionChangeHistories(): void {

        this._sharedInstructionChangeHistoriesPromise = lastValueFrom(
            SharedInstructionService.Instance.GetSharedInstructionChangeHistoriesForSharedInstruction(this.id)
        )
        .then(SharedInstructionChangeHistories => {
            this._sharedInstructionChangeHistories = SharedInstructionChangeHistories ?? [];
            this._sharedInstructionChangeHistoriesSubject.next(this._sharedInstructionChangeHistories);
            return this._sharedInstructionChangeHistories;
         })
        .catch(err => {
            this._sharedInstructionChangeHistories = [];
            this._sharedInstructionChangeHistoriesSubject.next(this._sharedInstructionChangeHistories);
            throw err;
        })
        .finally(() => {
            this._sharedInstructionChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SharedInstructionChangeHistory. Call after mutations to force refresh.
     */
    public ClearSharedInstructionChangeHistoriesCache(): void {
        this._sharedInstructionChangeHistories = null;
        this._sharedInstructionChangeHistoriesPromise = null;
        this._sharedInstructionChangeHistoriesSubject.next(this._sharedInstructionChangeHistories);      // Emit to observable
    }

    public get HasSharedInstructionChangeHistories(): Promise<boolean> {
        return this.SharedInstructionChangeHistories.then(sharedInstructionChangeHistories => sharedInstructionChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (sharedInstruction.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await sharedInstruction.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<SharedInstructionData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<SharedInstructionData>> {
        const info = await lastValueFrom(
            SharedInstructionService.Instance.GetSharedInstructionChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this SharedInstructionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SharedInstructionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SharedInstructionSubmitData {
        return SharedInstructionService.Instance.ConvertToSharedInstructionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SharedInstructionService extends SecureEndpointBase {

    private static _instance: SharedInstructionService;
    private listCache: Map<string, Observable<Array<SharedInstructionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SharedInstructionBasicListData>>>;
    private recordCache: Map<string, Observable<SharedInstructionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private sharedInstructionChangeHistoryService: SharedInstructionChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SharedInstructionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SharedInstructionBasicListData>>>();
        this.recordCache = new Map<string, Observable<SharedInstructionData>>();

        SharedInstructionService._instance = this;
    }

    public static get Instance(): SharedInstructionService {
      return SharedInstructionService._instance;
    }


    public ClearListCaches(config: SharedInstructionQueryParameters | null = null) {

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


    public ConvertToSharedInstructionSubmitData(data: SharedInstructionData): SharedInstructionSubmitData {

        let output = new SharedInstructionSubmitData();

        output.id = data.id;
        output.buildManualId = data.buildManualId;
        output.publishedMocId = data.publishedMocId;
        output.name = data.name;
        output.description = data.description;
        output.formatType = data.formatType;
        output.filePath = data.filePath;
        output.isPublished = data.isPublished;
        output.publishedDate = data.publishedDate;
        output.downloadCount = data.downloadCount;
        output.pageCount = data.pageCount;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSharedInstruction(id: bigint | number, includeRelations: boolean = true) : Observable<SharedInstructionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const sharedInstruction$ = this.requestSharedInstruction(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SharedInstruction", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, sharedInstruction$);

            return sharedInstruction$;
        }

        return this.recordCache.get(configHash) as Observable<SharedInstructionData>;
    }

    private requestSharedInstruction(id: bigint | number, includeRelations: boolean = true) : Observable<SharedInstructionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SharedInstructionData>(this.baseUrl + 'api/SharedInstruction/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSharedInstruction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSharedInstruction(id, includeRelations));
            }));
    }

    public GetSharedInstructionList(config: SharedInstructionQueryParameters | any = null) : Observable<Array<SharedInstructionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const sharedInstructionList$ = this.requestSharedInstructionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SharedInstruction list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, sharedInstructionList$);

            return sharedInstructionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SharedInstructionData>>;
    }


    private requestSharedInstructionList(config: SharedInstructionQueryParameters | any) : Observable <Array<SharedInstructionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SharedInstructionData>>(this.baseUrl + 'api/SharedInstructions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSharedInstructionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSharedInstructionList(config));
            }));
    }

    public GetSharedInstructionsRowCount(config: SharedInstructionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const sharedInstructionsRowCount$ = this.requestSharedInstructionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SharedInstructions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, sharedInstructionsRowCount$);

            return sharedInstructionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSharedInstructionsRowCount(config: SharedInstructionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SharedInstructions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSharedInstructionsRowCount(config));
            }));
    }

    public GetSharedInstructionsBasicListData(config: SharedInstructionQueryParameters | any = null) : Observable<Array<SharedInstructionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const sharedInstructionsBasicListData$ = this.requestSharedInstructionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SharedInstructions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, sharedInstructionsBasicListData$);

            return sharedInstructionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SharedInstructionBasicListData>>;
    }


    private requestSharedInstructionsBasicListData(config: SharedInstructionQueryParameters | any) : Observable<Array<SharedInstructionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SharedInstructionBasicListData>>(this.baseUrl + 'api/SharedInstructions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSharedInstructionsBasicListData(config));
            }));

    }


    public PutSharedInstruction(id: bigint | number, sharedInstruction: SharedInstructionSubmitData) : Observable<SharedInstructionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SharedInstructionData>(this.baseUrl + 'api/SharedInstruction/' + id.toString(), sharedInstruction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSharedInstruction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSharedInstruction(id, sharedInstruction));
            }));
    }


    public PostSharedInstruction(sharedInstruction: SharedInstructionSubmitData) : Observable<SharedInstructionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SharedInstructionData>(this.baseUrl + 'api/SharedInstruction', sharedInstruction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSharedInstruction(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSharedInstruction(sharedInstruction));
            }));
    }

  
    public DeleteSharedInstruction(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SharedInstruction/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSharedInstruction(id));
            }));
    }

    public RollbackSharedInstruction(id: bigint | number, versionNumber: bigint | number) : Observable<SharedInstructionData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SharedInstructionData>(this.baseUrl + 'api/SharedInstruction/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSharedInstruction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackSharedInstruction(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a SharedInstruction.
     */
    public GetSharedInstructionChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<SharedInstructionData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SharedInstructionData>>(this.baseUrl + 'api/SharedInstruction/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSharedInstructionChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a SharedInstruction.
     */
    public GetSharedInstructionAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<SharedInstructionData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SharedInstructionData>[]>(this.baseUrl + 'api/SharedInstruction/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSharedInstructionAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a SharedInstruction.
     */
    public GetSharedInstructionVersion(id: bigint | number, version: number): Observable<SharedInstructionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SharedInstructionData>(this.baseUrl + 'api/SharedInstruction/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSharedInstruction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSharedInstructionVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a SharedInstruction at a specific point in time.
     */
    public GetSharedInstructionStateAtTime(id: bigint | number, time: string): Observable<SharedInstructionData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SharedInstructionData>(this.baseUrl + 'api/SharedInstruction/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSharedInstruction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSharedInstructionStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: SharedInstructionQueryParameters | any): string {

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

    public userIsBMCSharedInstructionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCSharedInstructionReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.SharedInstructions
        //
        if (userIsBMCSharedInstructionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCSharedInstructionReader = user.readPermission >= 1;
            } else {
                userIsBMCSharedInstructionReader = false;
            }
        }

        return userIsBMCSharedInstructionReader;
    }


    public userIsBMCSharedInstructionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCSharedInstructionWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.SharedInstructions
        //
        if (userIsBMCSharedInstructionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCSharedInstructionWriter = user.writePermission >= 1;
          } else {
            userIsBMCSharedInstructionWriter = false;
          }      
        }

        return userIsBMCSharedInstructionWriter;
    }

    public GetSharedInstructionChangeHistoriesForSharedInstruction(sharedInstructionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SharedInstructionChangeHistoryData[]> {
        return this.sharedInstructionChangeHistoryService.GetSharedInstructionChangeHistoryList({
            sharedInstructionId: sharedInstructionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SharedInstructionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SharedInstructionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SharedInstructionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSharedInstruction(raw: any): SharedInstructionData {
    if (!raw) return raw;

    //
    // Create a SharedInstructionData object instance with correct prototype
    //
    const revived = Object.create(SharedInstructionData.prototype) as SharedInstructionData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._sharedInstructionChangeHistories = null;
    (revived as any)._sharedInstructionChangeHistoriesPromise = null;
    (revived as any)._sharedInstructionChangeHistoriesSubject = new BehaviorSubject<SharedInstructionChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSharedInstructionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SharedInstructionChangeHistories$ = (revived as any)._sharedInstructionChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._sharedInstructionChangeHistories === null && (revived as any)._sharedInstructionChangeHistoriesPromise === null) {
                (revived as any).loadSharedInstructionChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._sharedInstructionChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SharedInstructionData> | null>(null);

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

  private ReviveSharedInstructionList(rawList: any[]): SharedInstructionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSharedInstruction(raw));
  }

}
