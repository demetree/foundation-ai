/*

   GENERATED SERVICE FOR THE PROJECTTAGASSIGNMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ProjectTagAssignment table.

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
import { ProjectTagData } from './project-tag.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ProjectTagAssignmentQueryParameters {
    projectId: bigint | number | null | undefined = null;
    projectTagId: bigint | number | null | undefined = null;
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
export class ProjectTagAssignmentSubmitData {
    id!: bigint | number;
    projectId!: bigint | number;
    projectTagId!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class ProjectTagAssignmentBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ProjectTagAssignmentChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `projectTagAssignment.ProjectTagAssignmentChildren$` — use with `| async` in templates
//        • Promise:    `projectTagAssignment.ProjectTagAssignmentChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="projectTagAssignment.ProjectTagAssignmentChildren$ | async"`), or
//        • Access the promise getter (`projectTagAssignment.ProjectTagAssignmentChildren` or `await projectTagAssignment.ProjectTagAssignmentChildren`)
//    - Simply reading `projectTagAssignment.ProjectTagAssignmentChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await projectTagAssignment.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ProjectTagAssignmentData {
    id!: bigint | number;
    projectId!: bigint | number;
    projectTagId!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    project: ProjectData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    projectTag: ProjectTagData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ProjectTagAssignmentData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.projectTagAssignment.Reload();
  //
  //  Non Async:
  //
  //     projectTagAssignment[0].Reload().then(x => {
  //        this.projectTagAssignment = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ProjectTagAssignmentService.Instance.GetProjectTagAssignment(this.id, includeRelations)
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
     * Updates the state of this ProjectTagAssignmentData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ProjectTagAssignmentData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ProjectTagAssignmentSubmitData {
        return ProjectTagAssignmentService.Instance.ConvertToProjectTagAssignmentSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ProjectTagAssignmentService extends SecureEndpointBase {

    private static _instance: ProjectTagAssignmentService;
    private listCache: Map<string, Observable<Array<ProjectTagAssignmentData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ProjectTagAssignmentBasicListData>>>;
    private recordCache: Map<string, Observable<ProjectTagAssignmentData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ProjectTagAssignmentData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ProjectTagAssignmentBasicListData>>>();
        this.recordCache = new Map<string, Observable<ProjectTagAssignmentData>>();

        ProjectTagAssignmentService._instance = this;
    }

    public static get Instance(): ProjectTagAssignmentService {
      return ProjectTagAssignmentService._instance;
    }


    public ClearListCaches(config: ProjectTagAssignmentQueryParameters | null = null) {

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


    public ConvertToProjectTagAssignmentSubmitData(data: ProjectTagAssignmentData): ProjectTagAssignmentSubmitData {

        let output = new ProjectTagAssignmentSubmitData();

        output.id = data.id;
        output.projectId = data.projectId;
        output.projectTagId = data.projectTagId;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetProjectTagAssignment(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectTagAssignmentData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const projectTagAssignment$ = this.requestProjectTagAssignment(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ProjectTagAssignment", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, projectTagAssignment$);

            return projectTagAssignment$;
        }

        return this.recordCache.get(configHash) as Observable<ProjectTagAssignmentData>;
    }

    private requestProjectTagAssignment(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectTagAssignmentData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ProjectTagAssignmentData>(this.baseUrl + 'api/ProjectTagAssignment/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveProjectTagAssignment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectTagAssignment(id, includeRelations));
            }));
    }

    public GetProjectTagAssignmentList(config: ProjectTagAssignmentQueryParameters | any = null) : Observable<Array<ProjectTagAssignmentData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const projectTagAssignmentList$ = this.requestProjectTagAssignmentList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ProjectTagAssignment list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, projectTagAssignmentList$);

            return projectTagAssignmentList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ProjectTagAssignmentData>>;
    }


    private requestProjectTagAssignmentList(config: ProjectTagAssignmentQueryParameters | any) : Observable <Array<ProjectTagAssignmentData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectTagAssignmentData>>(this.baseUrl + 'api/ProjectTagAssignments', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveProjectTagAssignmentList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectTagAssignmentList(config));
            }));
    }

    public GetProjectTagAssignmentsRowCount(config: ProjectTagAssignmentQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const projectTagAssignmentsRowCount$ = this.requestProjectTagAssignmentsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ProjectTagAssignments row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, projectTagAssignmentsRowCount$);

            return projectTagAssignmentsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestProjectTagAssignmentsRowCount(config: ProjectTagAssignmentQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ProjectTagAssignments/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectTagAssignmentsRowCount(config));
            }));
    }

    public GetProjectTagAssignmentsBasicListData(config: ProjectTagAssignmentQueryParameters | any = null) : Observable<Array<ProjectTagAssignmentBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const projectTagAssignmentsBasicListData$ = this.requestProjectTagAssignmentsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ProjectTagAssignments basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, projectTagAssignmentsBasicListData$);

            return projectTagAssignmentsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ProjectTagAssignmentBasicListData>>;
    }


    private requestProjectTagAssignmentsBasicListData(config: ProjectTagAssignmentQueryParameters | any) : Observable<Array<ProjectTagAssignmentBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectTagAssignmentBasicListData>>(this.baseUrl + 'api/ProjectTagAssignments/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectTagAssignmentsBasicListData(config));
            }));

    }


    public PutProjectTagAssignment(id: bigint | number, projectTagAssignment: ProjectTagAssignmentSubmitData) : Observable<ProjectTagAssignmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ProjectTagAssignmentData>(this.baseUrl + 'api/ProjectTagAssignment/' + id.toString(), projectTagAssignment, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProjectTagAssignment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutProjectTagAssignment(id, projectTagAssignment));
            }));
    }


    public PostProjectTagAssignment(projectTagAssignment: ProjectTagAssignmentSubmitData) : Observable<ProjectTagAssignmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ProjectTagAssignmentData>(this.baseUrl + 'api/ProjectTagAssignment', projectTagAssignment, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProjectTagAssignment(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostProjectTagAssignment(projectTagAssignment));
            }));
    }

  
    public DeleteProjectTagAssignment(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ProjectTagAssignment/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteProjectTagAssignment(id));
            }));
    }


    private getConfigHash(config: ProjectTagAssignmentQueryParameters | any): string {

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

    public userIsBMCProjectTagAssignmentReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCProjectTagAssignmentReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ProjectTagAssignments
        //
        if (userIsBMCProjectTagAssignmentReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCProjectTagAssignmentReader = user.readPermission >= 1;
            } else {
                userIsBMCProjectTagAssignmentReader = false;
            }
        }

        return userIsBMCProjectTagAssignmentReader;
    }


    public userIsBMCProjectTagAssignmentWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCProjectTagAssignmentWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ProjectTagAssignments
        //
        if (userIsBMCProjectTagAssignmentWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCProjectTagAssignmentWriter = user.writePermission >= 1;
          } else {
            userIsBMCProjectTagAssignmentWriter = false;
          }      
        }

        return userIsBMCProjectTagAssignmentWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ProjectTagAssignmentData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ProjectTagAssignmentData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ProjectTagAssignmentTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveProjectTagAssignment(raw: any): ProjectTagAssignmentData {
    if (!raw) return raw;

    //
    // Create a ProjectTagAssignmentData object instance with correct prototype
    //
    const revived = Object.create(ProjectTagAssignmentData.prototype) as ProjectTagAssignmentData;

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
    // 2. But private methods (loadProjectTagAssignmentXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveProjectTagAssignmentList(rawList: any[]): ProjectTagAssignmentData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveProjectTagAssignment(raw));
  }

}
