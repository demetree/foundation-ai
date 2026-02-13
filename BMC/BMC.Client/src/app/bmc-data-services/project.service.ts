/*

   GENERATED SERVICE FOR THE PROJECT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Project table.

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
import { ProjectChangeHistoryService, ProjectChangeHistoryData } from './project-change-history.service';
import { PlacedBrickService, PlacedBrickData } from './placed-brick.service';
import { BrickConnectionService, BrickConnectionData } from './brick-connection.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ProjectQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class ProjectSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
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

export class ProjectBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ProjectChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `project.ProjectChildren$` — use with `| async` in templates
//        • Promise:    `project.ProjectChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="project.ProjectChildren$ | async"`), or
//        • Access the promise getter (`project.ProjectChildren` or `await project.ProjectChildren`)
//    - Simply reading `project.ProjectChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await project.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ProjectData {
    id!: bigint | number;
    name!: string;
    description!: string;
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _projectChangeHistories: ProjectChangeHistoryData[] | null = null;
    private _projectChangeHistoriesPromise: Promise<ProjectChangeHistoryData[]> | null  = null;
    private _projectChangeHistoriesSubject = new BehaviorSubject<ProjectChangeHistoryData[] | null>(null);

                
    private _placedBricks: PlacedBrickData[] | null = null;
    private _placedBricksPromise: Promise<PlacedBrickData[]> | null  = null;
    private _placedBricksSubject = new BehaviorSubject<PlacedBrickData[] | null>(null);

                
    private _brickConnections: BrickConnectionData[] | null = null;
    private _brickConnectionsPromise: Promise<BrickConnectionData[]> | null  = null;
    private _brickConnectionsSubject = new BehaviorSubject<BrickConnectionData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ProjectData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ProjectData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ProjectData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ProjectChangeHistories$ = this._projectChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._projectChangeHistories === null && this._projectChangeHistoriesPromise === null) {
            this.loadProjectChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ProjectChangeHistoriesCount$ = ProjectChangeHistoryService.Instance.GetProjectChangeHistoriesRowCount({projectId: this.id,
      active: true,
      deleted: false
    });



    public PlacedBricks$ = this._placedBricksSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._placedBricks === null && this._placedBricksPromise === null) {
            this.loadPlacedBricks(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public PlacedBricksCount$ = PlacedBrickService.Instance.GetPlacedBricksRowCount({projectId: this.id,
      active: true,
      deleted: false
    });



    public BrickConnections$ = this._brickConnectionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickConnections === null && this._brickConnectionsPromise === null) {
            this.loadBrickConnections(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public BrickConnectionsCount$ = BrickConnectionService.Instance.GetBrickConnectionsRowCount({projectId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ProjectData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.project.Reload();
  //
  //  Non Async:
  //
  //     project[0].Reload().then(x => {
  //        this.project = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ProjectService.Instance.GetProject(this.id, includeRelations)
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
     this._projectChangeHistories = null;
     this._projectChangeHistoriesPromise = null;
     this._projectChangeHistoriesSubject.next(null);

     this._placedBricks = null;
     this._placedBricksPromise = null;
     this._placedBricksSubject.next(null);

     this._brickConnections = null;
     this._brickConnectionsPromise = null;
     this._brickConnectionsSubject.next(null);

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
     * Gets the ProjectChangeHistories for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.ProjectChangeHistories.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get ProjectChangeHistories(): Promise<ProjectChangeHistoryData[]> {
        if (this._projectChangeHistories !== null) {
            return Promise.resolve(this._projectChangeHistories);
        }

        if (this._projectChangeHistoriesPromise !== null) {
            return this._projectChangeHistoriesPromise;
        }

        // Start the load
        this.loadProjectChangeHistories();

        return this._projectChangeHistoriesPromise!;
    }



    private loadProjectChangeHistories(): void {

        this._projectChangeHistoriesPromise = lastValueFrom(
            ProjectService.Instance.GetProjectChangeHistoriesForProject(this.id)
        )
        .then(ProjectChangeHistories => {
            this._projectChangeHistories = ProjectChangeHistories ?? [];
            this._projectChangeHistoriesSubject.next(this._projectChangeHistories);
            return this._projectChangeHistories;
         })
        .catch(err => {
            this._projectChangeHistories = [];
            this._projectChangeHistoriesSubject.next(this._projectChangeHistories);
            throw err;
        })
        .finally(() => {
            this._projectChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ProjectChangeHistory. Call after mutations to force refresh.
     */
    public ClearProjectChangeHistoriesCache(): void {
        this._projectChangeHistories = null;
        this._projectChangeHistoriesPromise = null;
        this._projectChangeHistoriesSubject.next(this._projectChangeHistories);      // Emit to observable
    }

    public get HasProjectChangeHistories(): Promise<boolean> {
        return this.ProjectChangeHistories.then(projectChangeHistories => projectChangeHistories.length > 0);
    }


    /**
     *
     * Gets the PlacedBricks for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.PlacedBricks.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get PlacedBricks(): Promise<PlacedBrickData[]> {
        if (this._placedBricks !== null) {
            return Promise.resolve(this._placedBricks);
        }

        if (this._placedBricksPromise !== null) {
            return this._placedBricksPromise;
        }

        // Start the load
        this.loadPlacedBricks();

        return this._placedBricksPromise!;
    }



    private loadPlacedBricks(): void {

        this._placedBricksPromise = lastValueFrom(
            ProjectService.Instance.GetPlacedBricksForProject(this.id)
        )
        .then(PlacedBricks => {
            this._placedBricks = PlacedBricks ?? [];
            this._placedBricksSubject.next(this._placedBricks);
            return this._placedBricks;
         })
        .catch(err => {
            this._placedBricks = [];
            this._placedBricksSubject.next(this._placedBricks);
            throw err;
        })
        .finally(() => {
            this._placedBricksPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PlacedBrick. Call after mutations to force refresh.
     */
    public ClearPlacedBricksCache(): void {
        this._placedBricks = null;
        this._placedBricksPromise = null;
        this._placedBricksSubject.next(this._placedBricks);      // Emit to observable
    }

    public get HasPlacedBricks(): Promise<boolean> {
        return this.PlacedBricks.then(placedBricks => placedBricks.length > 0);
    }


    /**
     *
     * Gets the BrickConnections for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.BrickConnections.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get BrickConnections(): Promise<BrickConnectionData[]> {
        if (this._brickConnections !== null) {
            return Promise.resolve(this._brickConnections);
        }

        if (this._brickConnectionsPromise !== null) {
            return this._brickConnectionsPromise;
        }

        // Start the load
        this.loadBrickConnections();

        return this._brickConnectionsPromise!;
    }



    private loadBrickConnections(): void {

        this._brickConnectionsPromise = lastValueFrom(
            ProjectService.Instance.GetBrickConnectionsForProject(this.id)
        )
        .then(BrickConnections => {
            this._brickConnections = BrickConnections ?? [];
            this._brickConnectionsSubject.next(this._brickConnections);
            return this._brickConnections;
         })
        .catch(err => {
            this._brickConnections = [];
            this._brickConnectionsSubject.next(this._brickConnections);
            throw err;
        })
        .finally(() => {
            this._brickConnectionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BrickConnection. Call after mutations to force refresh.
     */
    public ClearBrickConnectionsCache(): void {
        this._brickConnections = null;
        this._brickConnectionsPromise = null;
        this._brickConnectionsSubject.next(this._brickConnections);      // Emit to observable
    }

    public get HasBrickConnections(): Promise<boolean> {
        return this.BrickConnections.then(brickConnections => brickConnections.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (project.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await project.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ProjectData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ProjectData>> {
        const info = await lastValueFrom(
            ProjectService.Instance.GetProjectChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ProjectData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ProjectData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ProjectSubmitData {
        return ProjectService.Instance.ConvertToProjectSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ProjectService extends SecureEndpointBase {

    private static _instance: ProjectService;
    private listCache: Map<string, Observable<Array<ProjectData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ProjectBasicListData>>>;
    private recordCache: Map<string, Observable<ProjectData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private projectChangeHistoryService: ProjectChangeHistoryService,
        private placedBrickService: PlacedBrickService,
        private brickConnectionService: BrickConnectionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ProjectData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ProjectBasicListData>>>();
        this.recordCache = new Map<string, Observable<ProjectData>>();

        ProjectService._instance = this;
    }

    public static get Instance(): ProjectService {
      return ProjectService._instance;
    }


    public ClearListCaches(config: ProjectQueryParameters | null = null) {

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


    public ConvertToProjectSubmitData(data: ProjectData): ProjectSubmitData {

        let output = new ProjectSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetProject(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const project$ = this.requestProject(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Project", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, project$);

            return project$;
        }

        return this.recordCache.get(configHash) as Observable<ProjectData>;
    }

    private requestProject(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ProjectData>(this.baseUrl + 'api/Project/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveProject(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestProject(id, includeRelations));
            }));
    }

    public GetProjectList(config: ProjectQueryParameters | any = null) : Observable<Array<ProjectData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const projectList$ = this.requestProjectList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Project list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, projectList$);

            return projectList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ProjectData>>;
    }


    private requestProjectList(config: ProjectQueryParameters | any) : Observable <Array<ProjectData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectData>>(this.baseUrl + 'api/Projects', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveProjectList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectList(config));
            }));
    }

    public GetProjectsRowCount(config: ProjectQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const projectsRowCount$ = this.requestProjectsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Projects row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, projectsRowCount$);

            return projectsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestProjectsRowCount(config: ProjectQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Projects/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectsRowCount(config));
            }));
    }

    public GetProjectsBasicListData(config: ProjectQueryParameters | any = null) : Observable<Array<ProjectBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const projectsBasicListData$ = this.requestProjectsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Projects basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, projectsBasicListData$);

            return projectsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ProjectBasicListData>>;
    }


    private requestProjectsBasicListData(config: ProjectQueryParameters | any) : Observable<Array<ProjectBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectBasicListData>>(this.baseUrl + 'api/Projects/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectsBasicListData(config));
            }));

    }


    public PutProject(id: bigint | number, project: ProjectSubmitData) : Observable<ProjectData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ProjectData>(this.baseUrl + 'api/Project/' + id.toString(), project, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProject(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutProject(id, project));
            }));
    }


    public PostProject(project: ProjectSubmitData) : Observable<ProjectData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ProjectData>(this.baseUrl + 'api/Project', project, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProject(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostProject(project));
            }));
    }

  
    public DeleteProject(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Project/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteProject(id));
            }));
    }

    public RollbackProject(id: bigint | number, versionNumber: bigint | number) : Observable<ProjectData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ProjectData>(this.baseUrl + 'api/Project/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProject(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackProject(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Project.
     */
    public GetProjectChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ProjectData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ProjectData>>(this.baseUrl + 'api/Project/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetProjectChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Project.
     */
    public GetProjectAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ProjectData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ProjectData>[]>(this.baseUrl + 'api/Project/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetProjectAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Project.
     */
    public GetProjectVersion(id: bigint | number, version: number): Observable<ProjectData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ProjectData>(this.baseUrl + 'api/Project/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveProject(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetProjectVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Project at a specific point in time.
     */
    public GetProjectStateAtTime(id: bigint | number, time: string): Observable<ProjectData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ProjectData>(this.baseUrl + 'api/Project/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveProject(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetProjectStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ProjectQueryParameters | any): string {

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

    public userIsBMCProjectReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCProjectReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.Projects
        //
        if (userIsBMCProjectReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCProjectReader = user.readPermission >= 1;
            } else {
                userIsBMCProjectReader = false;
            }
        }

        return userIsBMCProjectReader;
    }


    public userIsBMCProjectWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCProjectWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.Projects
        //
        if (userIsBMCProjectWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCProjectWriter = user.writePermission >= 1;
          } else {
            userIsBMCProjectWriter = false;
          }      
        }

        return userIsBMCProjectWriter;
    }

    public GetProjectChangeHistoriesForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ProjectChangeHistoryData[]> {
        return this.projectChangeHistoryService.GetProjectChangeHistoryList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPlacedBricksForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PlacedBrickData[]> {
        return this.placedBrickService.GetPlacedBrickList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBrickConnectionsForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickConnectionData[]> {
        return this.brickConnectionService.GetBrickConnectionList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ProjectData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ProjectData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ProjectTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveProject(raw: any): ProjectData {
    if (!raw) return raw;

    //
    // Create a ProjectData object instance with correct prototype
    //
    const revived = Object.create(ProjectData.prototype) as ProjectData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._projectChangeHistories = null;
    (revived as any)._projectChangeHistoriesPromise = null;
    (revived as any)._projectChangeHistoriesSubject = new BehaviorSubject<ProjectChangeHistoryData[] | null>(null);

    (revived as any)._placedBricks = null;
    (revived as any)._placedBricksPromise = null;
    (revived as any)._placedBricksSubject = new BehaviorSubject<PlacedBrickData[] | null>(null);

    (revived as any)._brickConnections = null;
    (revived as any)._brickConnectionsPromise = null;
    (revived as any)._brickConnectionsSubject = new BehaviorSubject<BrickConnectionData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadProjectXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ProjectChangeHistories$ = (revived as any)._projectChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._projectChangeHistories === null && (revived as any)._projectChangeHistoriesPromise === null) {
                (revived as any).loadProjectChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ProjectChangeHistoriesCount$ = ProjectChangeHistoryService.Instance.GetProjectChangeHistoriesRowCount({projectId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).PlacedBricks$ = (revived as any)._placedBricksSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._placedBricks === null && (revived as any)._placedBricksPromise === null) {
                (revived as any).loadPlacedBricks();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).PlacedBricksCount$ = PlacedBrickService.Instance.GetPlacedBricksRowCount({projectId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).BrickConnections$ = (revived as any)._brickConnectionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickConnections === null && (revived as any)._brickConnectionsPromise === null) {
                (revived as any).loadBrickConnections();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).BrickConnectionsCount$ = BrickConnectionService.Instance.GetBrickConnectionsRowCount({projectId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ProjectData> | null>(null);

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

  private ReviveProjectList(rawList: any[]): ProjectData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveProject(raw));
  }

}
