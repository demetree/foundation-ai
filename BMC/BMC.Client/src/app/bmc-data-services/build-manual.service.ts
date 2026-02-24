/*

   GENERATED SERVICE FOR THE BUILDMANUAL TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BuildManual table.

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
import { BuildManualChangeHistoryService, BuildManualChangeHistoryData } from './build-manual-change-history.service';
import { BuildManualPageService, BuildManualPageData } from './build-manual-page.service';
import { SharedInstructionService, SharedInstructionData } from './shared-instruction.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BuildManualQueryParameters {
    projectId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    pageWidthMm: number | null | undefined = null;
    pageHeightMm: number | null | undefined = null;
    isPublished: boolean | null | undefined = null;
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
export class BuildManualSubmitData {
    id!: bigint | number;
    projectId!: bigint | number;
    name!: string;
    description!: string;
    pageWidthMm: number | null = null;
    pageHeightMm: number | null = null;
    isPublished!: boolean;
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

export class BuildManualBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BuildManualChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `buildManual.BuildManualChildren$` — use with `| async` in templates
//        • Promise:    `buildManual.BuildManualChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="buildManual.BuildManualChildren$ | async"`), or
//        • Access the promise getter (`buildManual.BuildManualChildren` or `await buildManual.BuildManualChildren`)
//    - Simply reading `buildManual.BuildManualChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await buildManual.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BuildManualData {
    id!: bigint | number;
    projectId!: bigint | number;
    name!: string;
    description!: string;
    pageWidthMm!: number | null;
    pageHeightMm!: number | null;
    isPublished!: boolean;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    project: ProjectData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _buildManualChangeHistories: BuildManualChangeHistoryData[] | null = null;
    private _buildManualChangeHistoriesPromise: Promise<BuildManualChangeHistoryData[]> | null  = null;
    private _buildManualChangeHistoriesSubject = new BehaviorSubject<BuildManualChangeHistoryData[] | null>(null);

                
    private _buildManualPages: BuildManualPageData[] | null = null;
    private _buildManualPagesPromise: Promise<BuildManualPageData[]> | null  = null;
    private _buildManualPagesSubject = new BehaviorSubject<BuildManualPageData[] | null>(null);

                
    private _sharedInstructions: SharedInstructionData[] | null = null;
    private _sharedInstructionsPromise: Promise<SharedInstructionData[]> | null  = null;
    private _sharedInstructionsSubject = new BehaviorSubject<SharedInstructionData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<BuildManualData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<BuildManualData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<BuildManualData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public BuildManualChangeHistories$ = this._buildManualChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._buildManualChangeHistories === null && this._buildManualChangeHistoriesPromise === null) {
            this.loadBuildManualChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _buildManualChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get BuildManualChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._buildManualChangeHistoriesCount$ === null) {
            this._buildManualChangeHistoriesCount$ = BuildManualChangeHistoryService.Instance.GetBuildManualChangeHistoriesRowCount({buildManualId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._buildManualChangeHistoriesCount$;
    }



    public BuildManualPages$ = this._buildManualPagesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._buildManualPages === null && this._buildManualPagesPromise === null) {
            this.loadBuildManualPages(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _buildManualPagesCount$: Observable<bigint | number> | null = null;
    public get BuildManualPagesCount$(): Observable<bigint | number> {
        if (this._buildManualPagesCount$ === null) {
            this._buildManualPagesCount$ = BuildManualPageService.Instance.GetBuildManualPagesRowCount({buildManualId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._buildManualPagesCount$;
    }



    public SharedInstructions$ = this._sharedInstructionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._sharedInstructions === null && this._sharedInstructionsPromise === null) {
            this.loadSharedInstructions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _sharedInstructionsCount$: Observable<bigint | number> | null = null;
    public get SharedInstructionsCount$(): Observable<bigint | number> {
        if (this._sharedInstructionsCount$ === null) {
            this._sharedInstructionsCount$ = SharedInstructionService.Instance.GetSharedInstructionsRowCount({buildManualId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._sharedInstructionsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any BuildManualData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.buildManual.Reload();
  //
  //  Non Async:
  //
  //     buildManual[0].Reload().then(x => {
  //        this.buildManual = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BuildManualService.Instance.GetBuildManual(this.id, includeRelations)
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
     this._buildManualChangeHistories = null;
     this._buildManualChangeHistoriesPromise = null;
     this._buildManualChangeHistoriesSubject.next(null);
     this._buildManualChangeHistoriesCount$ = null;

     this._buildManualPages = null;
     this._buildManualPagesPromise = null;
     this._buildManualPagesSubject.next(null);
     this._buildManualPagesCount$ = null;

     this._sharedInstructions = null;
     this._sharedInstructionsPromise = null;
     this._sharedInstructionsSubject.next(null);
     this._sharedInstructionsCount$ = null;

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
     * Gets the BuildManualChangeHistories for this BuildManual.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.buildManual.BuildManualChangeHistories.then(buildManuals => { ... })
     *   or
     *   await this.buildManual.buildManuals
     *
    */
    public get BuildManualChangeHistories(): Promise<BuildManualChangeHistoryData[]> {
        if (this._buildManualChangeHistories !== null) {
            return Promise.resolve(this._buildManualChangeHistories);
        }

        if (this._buildManualChangeHistoriesPromise !== null) {
            return this._buildManualChangeHistoriesPromise;
        }

        // Start the load
        this.loadBuildManualChangeHistories();

        return this._buildManualChangeHistoriesPromise!;
    }



    private loadBuildManualChangeHistories(): void {

        this._buildManualChangeHistoriesPromise = lastValueFrom(
            BuildManualService.Instance.GetBuildManualChangeHistoriesForBuildManual(this.id)
        )
        .then(BuildManualChangeHistories => {
            this._buildManualChangeHistories = BuildManualChangeHistories ?? [];
            this._buildManualChangeHistoriesSubject.next(this._buildManualChangeHistories);
            return this._buildManualChangeHistories;
         })
        .catch(err => {
            this._buildManualChangeHistories = [];
            this._buildManualChangeHistoriesSubject.next(this._buildManualChangeHistories);
            throw err;
        })
        .finally(() => {
            this._buildManualChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BuildManualChangeHistory. Call after mutations to force refresh.
     */
    public ClearBuildManualChangeHistoriesCache(): void {
        this._buildManualChangeHistories = null;
        this._buildManualChangeHistoriesPromise = null;
        this._buildManualChangeHistoriesSubject.next(this._buildManualChangeHistories);      // Emit to observable
    }

    public get HasBuildManualChangeHistories(): Promise<boolean> {
        return this.BuildManualChangeHistories.then(buildManualChangeHistories => buildManualChangeHistories.length > 0);
    }


    /**
     *
     * Gets the BuildManualPages for this BuildManual.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.buildManual.BuildManualPages.then(buildManuals => { ... })
     *   or
     *   await this.buildManual.buildManuals
     *
    */
    public get BuildManualPages(): Promise<BuildManualPageData[]> {
        if (this._buildManualPages !== null) {
            return Promise.resolve(this._buildManualPages);
        }

        if (this._buildManualPagesPromise !== null) {
            return this._buildManualPagesPromise;
        }

        // Start the load
        this.loadBuildManualPages();

        return this._buildManualPagesPromise!;
    }



    private loadBuildManualPages(): void {

        this._buildManualPagesPromise = lastValueFrom(
            BuildManualService.Instance.GetBuildManualPagesForBuildManual(this.id)
        )
        .then(BuildManualPages => {
            this._buildManualPages = BuildManualPages ?? [];
            this._buildManualPagesSubject.next(this._buildManualPages);
            return this._buildManualPages;
         })
        .catch(err => {
            this._buildManualPages = [];
            this._buildManualPagesSubject.next(this._buildManualPages);
            throw err;
        })
        .finally(() => {
            this._buildManualPagesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BuildManualPage. Call after mutations to force refresh.
     */
    public ClearBuildManualPagesCache(): void {
        this._buildManualPages = null;
        this._buildManualPagesPromise = null;
        this._buildManualPagesSubject.next(this._buildManualPages);      // Emit to observable
    }

    public get HasBuildManualPages(): Promise<boolean> {
        return this.BuildManualPages.then(buildManualPages => buildManualPages.length > 0);
    }


    /**
     *
     * Gets the SharedInstructions for this BuildManual.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.buildManual.SharedInstructions.then(buildManuals => { ... })
     *   or
     *   await this.buildManual.buildManuals
     *
    */
    public get SharedInstructions(): Promise<SharedInstructionData[]> {
        if (this._sharedInstructions !== null) {
            return Promise.resolve(this._sharedInstructions);
        }

        if (this._sharedInstructionsPromise !== null) {
            return this._sharedInstructionsPromise;
        }

        // Start the load
        this.loadSharedInstructions();

        return this._sharedInstructionsPromise!;
    }



    private loadSharedInstructions(): void {

        this._sharedInstructionsPromise = lastValueFrom(
            BuildManualService.Instance.GetSharedInstructionsForBuildManual(this.id)
        )
        .then(SharedInstructions => {
            this._sharedInstructions = SharedInstructions ?? [];
            this._sharedInstructionsSubject.next(this._sharedInstructions);
            return this._sharedInstructions;
         })
        .catch(err => {
            this._sharedInstructions = [];
            this._sharedInstructionsSubject.next(this._sharedInstructions);
            throw err;
        })
        .finally(() => {
            this._sharedInstructionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SharedInstruction. Call after mutations to force refresh.
     */
    public ClearSharedInstructionsCache(): void {
        this._sharedInstructions = null;
        this._sharedInstructionsPromise = null;
        this._sharedInstructionsSubject.next(this._sharedInstructions);      // Emit to observable
    }

    public get HasSharedInstructions(): Promise<boolean> {
        return this.SharedInstructions.then(sharedInstructions => sharedInstructions.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (buildManual.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await buildManual.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<BuildManualData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<BuildManualData>> {
        const info = await lastValueFrom(
            BuildManualService.Instance.GetBuildManualChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this BuildManualData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BuildManualData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BuildManualSubmitData {
        return BuildManualService.Instance.ConvertToBuildManualSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BuildManualService extends SecureEndpointBase {

    private static _instance: BuildManualService;
    private listCache: Map<string, Observable<Array<BuildManualData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BuildManualBasicListData>>>;
    private recordCache: Map<string, Observable<BuildManualData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private buildManualChangeHistoryService: BuildManualChangeHistoryService,
        private buildManualPageService: BuildManualPageService,
        private sharedInstructionService: SharedInstructionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BuildManualData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BuildManualBasicListData>>>();
        this.recordCache = new Map<string, Observable<BuildManualData>>();

        BuildManualService._instance = this;
    }

    public static get Instance(): BuildManualService {
      return BuildManualService._instance;
    }


    public ClearListCaches(config: BuildManualQueryParameters | null = null) {

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


    public ConvertToBuildManualSubmitData(data: BuildManualData): BuildManualSubmitData {

        let output = new BuildManualSubmitData();

        output.id = data.id;
        output.projectId = data.projectId;
        output.name = data.name;
        output.description = data.description;
        output.pageWidthMm = data.pageWidthMm;
        output.pageHeightMm = data.pageHeightMm;
        output.isPublished = data.isPublished;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBuildManual(id: bigint | number, includeRelations: boolean = true) : Observable<BuildManualData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const buildManual$ = this.requestBuildManual(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildManual", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, buildManual$);

            return buildManual$;
        }

        return this.recordCache.get(configHash) as Observable<BuildManualData>;
    }

    private requestBuildManual(id: bigint | number, includeRelations: boolean = true) : Observable<BuildManualData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BuildManualData>(this.baseUrl + 'api/BuildManual/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBuildManual(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManual(id, includeRelations));
            }));
    }

    public GetBuildManualList(config: BuildManualQueryParameters | any = null) : Observable<Array<BuildManualData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const buildManualList$ = this.requestBuildManualList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildManual list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, buildManualList$);

            return buildManualList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BuildManualData>>;
    }


    private requestBuildManualList(config: BuildManualQueryParameters | any) : Observable <Array<BuildManualData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildManualData>>(this.baseUrl + 'api/BuildManuals', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBuildManualList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualList(config));
            }));
    }

    public GetBuildManualsRowCount(config: BuildManualQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const buildManualsRowCount$ = this.requestBuildManualsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildManuals row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, buildManualsRowCount$);

            return buildManualsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBuildManualsRowCount(config: BuildManualQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BuildManuals/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualsRowCount(config));
            }));
    }

    public GetBuildManualsBasicListData(config: BuildManualQueryParameters | any = null) : Observable<Array<BuildManualBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const buildManualsBasicListData$ = this.requestBuildManualsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildManuals basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, buildManualsBasicListData$);

            return buildManualsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BuildManualBasicListData>>;
    }


    private requestBuildManualsBasicListData(config: BuildManualQueryParameters | any) : Observable<Array<BuildManualBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildManualBasicListData>>(this.baseUrl + 'api/BuildManuals/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildManualsBasicListData(config));
            }));

    }


    public PutBuildManual(id: bigint | number, buildManual: BuildManualSubmitData) : Observable<BuildManualData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BuildManualData>(this.baseUrl + 'api/BuildManual/' + id.toString(), buildManual, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildManual(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBuildManual(id, buildManual));
            }));
    }


    public PostBuildManual(buildManual: BuildManualSubmitData) : Observable<BuildManualData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BuildManualData>(this.baseUrl + 'api/BuildManual', buildManual, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildManual(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBuildManual(buildManual));
            }));
    }

  
    public DeleteBuildManual(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BuildManual/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBuildManual(id));
            }));
    }

    public RollbackBuildManual(id: bigint | number, versionNumber: bigint | number) : Observable<BuildManualData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BuildManualData>(this.baseUrl + 'api/BuildManual/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildManual(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackBuildManual(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a BuildManual.
     */
    public GetBuildManualChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<BuildManualData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<BuildManualData>>(this.baseUrl + 'api/BuildManual/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetBuildManualChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a BuildManual.
     */
    public GetBuildManualAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<BuildManualData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<BuildManualData>[]>(this.baseUrl + 'api/BuildManual/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetBuildManualAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a BuildManual.
     */
    public GetBuildManualVersion(id: bigint | number, version: number): Observable<BuildManualData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BuildManualData>(this.baseUrl + 'api/BuildManual/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveBuildManual(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetBuildManualVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a BuildManual at a specific point in time.
     */
    public GetBuildManualStateAtTime(id: bigint | number, time: string): Observable<BuildManualData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BuildManualData>(this.baseUrl + 'api/BuildManual/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveBuildManual(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetBuildManualStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: BuildManualQueryParameters | any): string {

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

    public userIsBMCBuildManualReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBuildManualReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BuildManuals
        //
        if (userIsBMCBuildManualReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBuildManualReader = user.readPermission >= 1;
            } else {
                userIsBMCBuildManualReader = false;
            }
        }

        return userIsBMCBuildManualReader;
    }


    public userIsBMCBuildManualWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBuildManualWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BuildManuals
        //
        if (userIsBMCBuildManualWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBuildManualWriter = user.writePermission >= 20;
          } else {
            userIsBMCBuildManualWriter = false;
          }      
        }

        return userIsBMCBuildManualWriter;
    }

    public GetBuildManualChangeHistoriesForBuildManual(buildManualId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BuildManualChangeHistoryData[]> {
        return this.buildManualChangeHistoryService.GetBuildManualChangeHistoryList({
            buildManualId: buildManualId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBuildManualPagesForBuildManual(buildManualId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BuildManualPageData[]> {
        return this.buildManualPageService.GetBuildManualPageList({
            buildManualId: buildManualId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSharedInstructionsForBuildManual(buildManualId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SharedInstructionData[]> {
        return this.sharedInstructionService.GetSharedInstructionList({
            buildManualId: buildManualId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full BuildManualData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BuildManualData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BuildManualTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBuildManual(raw: any): BuildManualData {
    if (!raw) return raw;

    //
    // Create a BuildManualData object instance with correct prototype
    //
    const revived = Object.create(BuildManualData.prototype) as BuildManualData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._buildManualChangeHistories = null;
    (revived as any)._buildManualChangeHistoriesPromise = null;
    (revived as any)._buildManualChangeHistoriesSubject = new BehaviorSubject<BuildManualChangeHistoryData[] | null>(null);

    (revived as any)._buildManualPages = null;
    (revived as any)._buildManualPagesPromise = null;
    (revived as any)._buildManualPagesSubject = new BehaviorSubject<BuildManualPageData[] | null>(null);

    (revived as any)._sharedInstructions = null;
    (revived as any)._sharedInstructionsPromise = null;
    (revived as any)._sharedInstructionsSubject = new BehaviorSubject<SharedInstructionData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadBuildManualXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).BuildManualChangeHistories$ = (revived as any)._buildManualChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._buildManualChangeHistories === null && (revived as any)._buildManualChangeHistoriesPromise === null) {
                (revived as any).loadBuildManualChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._buildManualChangeHistoriesCount$ = null;


    (revived as any).BuildManualPages$ = (revived as any)._buildManualPagesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._buildManualPages === null && (revived as any)._buildManualPagesPromise === null) {
                (revived as any).loadBuildManualPages();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._buildManualPagesCount$ = null;


    (revived as any).SharedInstructions$ = (revived as any)._sharedInstructionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._sharedInstructions === null && (revived as any)._sharedInstructionsPromise === null) {
                (revived as any).loadSharedInstructions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._sharedInstructionsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<BuildManualData> | null>(null);

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

  private ReviveBuildManualList(rawList: any[]): BuildManualData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBuildManual(raw));
  }

}
