/*

   GENERATED SERVICE FOR THE SUBMODEL TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Submodel table.

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
import { SubmodelChangeHistoryService, SubmodelChangeHistoryData } from './submodel-change-history.service';
import { SubmodelPlacedBrickService, SubmodelPlacedBrickData } from './submodel-placed-brick.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SubmodelQueryParameters {
    projectId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    submodelId: bigint | number | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class SubmodelSubmitData {
    id!: bigint | number;
    projectId!: bigint | number;
    name!: string;
    description!: string;
    submodelId: bigint | number | null = null;
    sequence: bigint | number | null = null;
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

export class SubmodelBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SubmodelChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `submodel.SubmodelChildren$` — use with `| async` in templates
//        • Promise:    `submodel.SubmodelChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="submodel.SubmodelChildren$ | async"`), or
//        • Access the promise getter (`submodel.SubmodelChildren` or `await submodel.SubmodelChildren`)
//    - Simply reading `submodel.SubmodelChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await submodel.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SubmodelData {
    id!: bigint | number;
    projectId!: bigint | number;
    name!: string;
    description!: string;
    submodelId!: bigint | number;
    sequence!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    project: ProjectData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    submodel: SubmodelData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _submodelChangeHistories: SubmodelChangeHistoryData[] | null = null;
    private _submodelChangeHistoriesPromise: Promise<SubmodelChangeHistoryData[]> | null  = null;
    private _submodelChangeHistoriesSubject = new BehaviorSubject<SubmodelChangeHistoryData[] | null>(null);

                
    private _submodelPlacedBricks: SubmodelPlacedBrickData[] | null = null;
    private _submodelPlacedBricksPromise: Promise<SubmodelPlacedBrickData[]> | null  = null;
    private _submodelPlacedBricksSubject = new BehaviorSubject<SubmodelPlacedBrickData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<SubmodelData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<SubmodelData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SubmodelData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SubmodelChangeHistories$ = this._submodelChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._submodelChangeHistories === null && this._submodelChangeHistoriesPromise === null) {
            this.loadSubmodelChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SubmodelChangeHistoriesCount$ = SubmodelChangeHistoryService.Instance.GetSubmodelChangeHistoriesRowCount({submodelId: this.id,
      active: true,
      deleted: false
    });



    public SubmodelPlacedBricks$ = this._submodelPlacedBricksSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._submodelPlacedBricks === null && this._submodelPlacedBricksPromise === null) {
            this.loadSubmodelPlacedBricks(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SubmodelPlacedBricksCount$ = SubmodelPlacedBrickService.Instance.GetSubmodelPlacedBricksRowCount({submodelId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SubmodelData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.submodel.Reload();
  //
  //  Non Async:
  //
  //     submodel[0].Reload().then(x => {
  //        this.submodel = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SubmodelService.Instance.GetSubmodel(this.id, includeRelations)
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
     this._submodelChangeHistories = null;
     this._submodelChangeHistoriesPromise = null;
     this._submodelChangeHistoriesSubject.next(null);

     this._submodelPlacedBricks = null;
     this._submodelPlacedBricksPromise = null;
     this._submodelPlacedBricksSubject.next(null);

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
     * Gets the SubmodelChangeHistories for this Submodel.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.submodel.SubmodelChangeHistories.then(submodels => { ... })
     *   or
     *   await this.submodel.submodels
     *
    */
    public get SubmodelChangeHistories(): Promise<SubmodelChangeHistoryData[]> {
        if (this._submodelChangeHistories !== null) {
            return Promise.resolve(this._submodelChangeHistories);
        }

        if (this._submodelChangeHistoriesPromise !== null) {
            return this._submodelChangeHistoriesPromise;
        }

        // Start the load
        this.loadSubmodelChangeHistories();

        return this._submodelChangeHistoriesPromise!;
    }



    private loadSubmodelChangeHistories(): void {

        this._submodelChangeHistoriesPromise = lastValueFrom(
            SubmodelService.Instance.GetSubmodelChangeHistoriesForSubmodel(this.id)
        )
        .then(SubmodelChangeHistories => {
            this._submodelChangeHistories = SubmodelChangeHistories ?? [];
            this._submodelChangeHistoriesSubject.next(this._submodelChangeHistories);
            return this._submodelChangeHistories;
         })
        .catch(err => {
            this._submodelChangeHistories = [];
            this._submodelChangeHistoriesSubject.next(this._submodelChangeHistories);
            throw err;
        })
        .finally(() => {
            this._submodelChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SubmodelChangeHistory. Call after mutations to force refresh.
     */
    public ClearSubmodelChangeHistoriesCache(): void {
        this._submodelChangeHistories = null;
        this._submodelChangeHistoriesPromise = null;
        this._submodelChangeHistoriesSubject.next(this._submodelChangeHistories);      // Emit to observable
    }

    public get HasSubmodelChangeHistories(): Promise<boolean> {
        return this.SubmodelChangeHistories.then(submodelChangeHistories => submodelChangeHistories.length > 0);
    }


    /**
     *
     * Gets the SubmodelPlacedBricks for this Submodel.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.submodel.SubmodelPlacedBricks.then(submodels => { ... })
     *   or
     *   await this.submodel.submodels
     *
    */
    public get SubmodelPlacedBricks(): Promise<SubmodelPlacedBrickData[]> {
        if (this._submodelPlacedBricks !== null) {
            return Promise.resolve(this._submodelPlacedBricks);
        }

        if (this._submodelPlacedBricksPromise !== null) {
            return this._submodelPlacedBricksPromise;
        }

        // Start the load
        this.loadSubmodelPlacedBricks();

        return this._submodelPlacedBricksPromise!;
    }



    private loadSubmodelPlacedBricks(): void {

        this._submodelPlacedBricksPromise = lastValueFrom(
            SubmodelService.Instance.GetSubmodelPlacedBricksForSubmodel(this.id)
        )
        .then(SubmodelPlacedBricks => {
            this._submodelPlacedBricks = SubmodelPlacedBricks ?? [];
            this._submodelPlacedBricksSubject.next(this._submodelPlacedBricks);
            return this._submodelPlacedBricks;
         })
        .catch(err => {
            this._submodelPlacedBricks = [];
            this._submodelPlacedBricksSubject.next(this._submodelPlacedBricks);
            throw err;
        })
        .finally(() => {
            this._submodelPlacedBricksPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SubmodelPlacedBrick. Call after mutations to force refresh.
     */
    public ClearSubmodelPlacedBricksCache(): void {
        this._submodelPlacedBricks = null;
        this._submodelPlacedBricksPromise = null;
        this._submodelPlacedBricksSubject.next(this._submodelPlacedBricks);      // Emit to observable
    }

    public get HasSubmodelPlacedBricks(): Promise<boolean> {
        return this.SubmodelPlacedBricks.then(submodelPlacedBricks => submodelPlacedBricks.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (submodel.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await submodel.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<SubmodelData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<SubmodelData>> {
        const info = await lastValueFrom(
            SubmodelService.Instance.GetSubmodelChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this SubmodelData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SubmodelData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SubmodelSubmitData {
        return SubmodelService.Instance.ConvertToSubmodelSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SubmodelService extends SecureEndpointBase {

    private static _instance: SubmodelService;
    private listCache: Map<string, Observable<Array<SubmodelData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SubmodelBasicListData>>>;
    private recordCache: Map<string, Observable<SubmodelData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private submodelChangeHistoryService: SubmodelChangeHistoryService,
        private submodelPlacedBrickService: SubmodelPlacedBrickService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SubmodelData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SubmodelBasicListData>>>();
        this.recordCache = new Map<string, Observable<SubmodelData>>();

        SubmodelService._instance = this;
    }

    public static get Instance(): SubmodelService {
      return SubmodelService._instance;
    }


    public ClearListCaches(config: SubmodelQueryParameters | null = null) {

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


    public ConvertToSubmodelSubmitData(data: SubmodelData): SubmodelSubmitData {

        let output = new SubmodelSubmitData();

        output.id = data.id;
        output.projectId = data.projectId;
        output.name = data.name;
        output.description = data.description;
        output.submodelId = data.submodelId;
        output.sequence = data.sequence;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSubmodel(id: bigint | number, includeRelations: boolean = true) : Observable<SubmodelData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const submodel$ = this.requestSubmodel(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Submodel", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, submodel$);

            return submodel$;
        }

        return this.recordCache.get(configHash) as Observable<SubmodelData>;
    }

    private requestSubmodel(id: bigint | number, includeRelations: boolean = true) : Observable<SubmodelData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SubmodelData>(this.baseUrl + 'api/Submodel/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSubmodel(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSubmodel(id, includeRelations));
            }));
    }

    public GetSubmodelList(config: SubmodelQueryParameters | any = null) : Observable<Array<SubmodelData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const submodelList$ = this.requestSubmodelList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Submodel list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, submodelList$);

            return submodelList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SubmodelData>>;
    }


    private requestSubmodelList(config: SubmodelQueryParameters | any) : Observable <Array<SubmodelData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SubmodelData>>(this.baseUrl + 'api/Submodels', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSubmodelList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSubmodelList(config));
            }));
    }

    public GetSubmodelsRowCount(config: SubmodelQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const submodelsRowCount$ = this.requestSubmodelsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Submodels row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, submodelsRowCount$);

            return submodelsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSubmodelsRowCount(config: SubmodelQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Submodels/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSubmodelsRowCount(config));
            }));
    }

    public GetSubmodelsBasicListData(config: SubmodelQueryParameters | any = null) : Observable<Array<SubmodelBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const submodelsBasicListData$ = this.requestSubmodelsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Submodels basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, submodelsBasicListData$);

            return submodelsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SubmodelBasicListData>>;
    }


    private requestSubmodelsBasicListData(config: SubmodelQueryParameters | any) : Observable<Array<SubmodelBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SubmodelBasicListData>>(this.baseUrl + 'api/Submodels/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSubmodelsBasicListData(config));
            }));

    }


    public PutSubmodel(id: bigint | number, submodel: SubmodelSubmitData) : Observable<SubmodelData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SubmodelData>(this.baseUrl + 'api/Submodel/' + id.toString(), submodel, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSubmodel(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSubmodel(id, submodel));
            }));
    }


    public PostSubmodel(submodel: SubmodelSubmitData) : Observable<SubmodelData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SubmodelData>(this.baseUrl + 'api/Submodel', submodel, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSubmodel(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSubmodel(submodel));
            }));
    }

  
    public DeleteSubmodel(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Submodel/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSubmodel(id));
            }));
    }

    public RollbackSubmodel(id: bigint | number, versionNumber: bigint | number) : Observable<SubmodelData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SubmodelData>(this.baseUrl + 'api/Submodel/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSubmodel(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackSubmodel(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Submodel.
     */
    public GetSubmodelChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<SubmodelData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SubmodelData>>(this.baseUrl + 'api/Submodel/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSubmodelChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Submodel.
     */
    public GetSubmodelAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<SubmodelData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SubmodelData>[]>(this.baseUrl + 'api/Submodel/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSubmodelAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Submodel.
     */
    public GetSubmodelVersion(id: bigint | number, version: number): Observable<SubmodelData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SubmodelData>(this.baseUrl + 'api/Submodel/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSubmodel(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSubmodelVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Submodel at a specific point in time.
     */
    public GetSubmodelStateAtTime(id: bigint | number, time: string): Observable<SubmodelData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SubmodelData>(this.baseUrl + 'api/Submodel/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSubmodel(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSubmodelStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: SubmodelQueryParameters | any): string {

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

    public userIsBMCSubmodelReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCSubmodelReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.Submodels
        //
        if (userIsBMCSubmodelReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCSubmodelReader = user.readPermission >= 1;
            } else {
                userIsBMCSubmodelReader = false;
            }
        }

        return userIsBMCSubmodelReader;
    }


    public userIsBMCSubmodelWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCSubmodelWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.Submodels
        //
        if (userIsBMCSubmodelWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCSubmodelWriter = user.writePermission >= 1;
          } else {
            userIsBMCSubmodelWriter = false;
          }      
        }

        return userIsBMCSubmodelWriter;
    }

    public GetSubmodelChangeHistoriesForSubmodel(submodelId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SubmodelChangeHistoryData[]> {
        return this.submodelChangeHistoryService.GetSubmodelChangeHistoryList({
            submodelId: submodelId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSubmodelPlacedBricksForSubmodel(submodelId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SubmodelPlacedBrickData[]> {
        return this.submodelPlacedBrickService.GetSubmodelPlacedBrickList({
            submodelId: submodelId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SubmodelData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SubmodelData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SubmodelTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSubmodel(raw: any): SubmodelData {
    if (!raw) return raw;

    //
    // Create a SubmodelData object instance with correct prototype
    //
    const revived = Object.create(SubmodelData.prototype) as SubmodelData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._submodels = null;
    (revived as any)._submodelsPromise = null;
    (revived as any)._submodelsSubject = new BehaviorSubject<SubmodelData[] | null>(null);

    (revived as any)._submodelChangeHistories = null;
    (revived as any)._submodelChangeHistoriesPromise = null;
    (revived as any)._submodelChangeHistoriesSubject = new BehaviorSubject<SubmodelChangeHistoryData[] | null>(null);

    (revived as any)._submodelPlacedBricks = null;
    (revived as any)._submodelPlacedBricksPromise = null;
    (revived as any)._submodelPlacedBricksSubject = new BehaviorSubject<SubmodelPlacedBrickData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSubmodelXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Submodels$ = (revived as any)._submodelsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._submodels === null && (revived as any)._submodelsPromise === null) {
                (revived as any).loadSubmodels();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SubmodelsCount$ = SubmodelService.Instance.GetSubmodelsRowCount({submodelId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SubmodelChangeHistories$ = (revived as any)._submodelChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._submodelChangeHistories === null && (revived as any)._submodelChangeHistoriesPromise === null) {
                (revived as any).loadSubmodelChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SubmodelChangeHistoriesCount$ = SubmodelChangeHistoryService.Instance.GetSubmodelChangeHistoriesRowCount({submodelId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SubmodelPlacedBricks$ = (revived as any)._submodelPlacedBricksSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._submodelPlacedBricks === null && (revived as any)._submodelPlacedBricksPromise === null) {
                (revived as any).loadSubmodelPlacedBricks();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SubmodelPlacedBricksCount$ = SubmodelPlacedBrickService.Instance.GetSubmodelPlacedBricksRowCount({submodelId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SubmodelData> | null>(null);

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

  private ReviveSubmodelList(rawList: any[]): SubmodelData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSubmodel(raw));
  }

}
