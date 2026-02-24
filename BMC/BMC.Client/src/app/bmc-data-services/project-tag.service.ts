/*

   GENERATED SERVICE FOR THE PROJECTTAG TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ProjectTag table.

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
import { ProjectTagAssignmentService, ProjectTagAssignmentData } from './project-tag-assignment.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ProjectTagQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class ProjectTagSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    active!: boolean;
    deleted!: boolean;
}


export class ProjectTagBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ProjectTagChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `projectTag.ProjectTagChildren$` — use with `| async` in templates
//        • Promise:    `projectTag.ProjectTagChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="projectTag.ProjectTagChildren$ | async"`), or
//        • Access the promise getter (`projectTag.ProjectTagChildren` or `await projectTag.ProjectTagChildren`)
//    - Simply reading `projectTag.ProjectTagChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await projectTag.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ProjectTagData {
    id!: bigint | number;
    name!: string;
    description!: string;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _projectTagAssignments: ProjectTagAssignmentData[] | null = null;
    private _projectTagAssignmentsPromise: Promise<ProjectTagAssignmentData[]> | null  = null;
    private _projectTagAssignmentsSubject = new BehaviorSubject<ProjectTagAssignmentData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ProjectTagAssignments$ = this._projectTagAssignmentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._projectTagAssignments === null && this._projectTagAssignmentsPromise === null) {
            this.loadProjectTagAssignments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _projectTagAssignmentsCount$: Observable<bigint | number> | null = null;
    public get ProjectTagAssignmentsCount$(): Observable<bigint | number> {
        if (this._projectTagAssignmentsCount$ === null) {
            this._projectTagAssignmentsCount$ = ProjectTagAssignmentService.Instance.GetProjectTagAssignmentsRowCount({projectTagId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._projectTagAssignmentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ProjectTagData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.projectTag.Reload();
  //
  //  Non Async:
  //
  //     projectTag[0].Reload().then(x => {
  //        this.projectTag = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ProjectTagService.Instance.GetProjectTag(this.id, includeRelations)
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
     this._projectTagAssignments = null;
     this._projectTagAssignmentsPromise = null;
     this._projectTagAssignmentsSubject.next(null);
     this._projectTagAssignmentsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ProjectTagAssignments for this ProjectTag.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.projectTag.ProjectTagAssignments.then(projectTags => { ... })
     *   or
     *   await this.projectTag.projectTags
     *
    */
    public get ProjectTagAssignments(): Promise<ProjectTagAssignmentData[]> {
        if (this._projectTagAssignments !== null) {
            return Promise.resolve(this._projectTagAssignments);
        }

        if (this._projectTagAssignmentsPromise !== null) {
            return this._projectTagAssignmentsPromise;
        }

        // Start the load
        this.loadProjectTagAssignments();

        return this._projectTagAssignmentsPromise!;
    }



    private loadProjectTagAssignments(): void {

        this._projectTagAssignmentsPromise = lastValueFrom(
            ProjectTagService.Instance.GetProjectTagAssignmentsForProjectTag(this.id)
        )
        .then(ProjectTagAssignments => {
            this._projectTagAssignments = ProjectTagAssignments ?? [];
            this._projectTagAssignmentsSubject.next(this._projectTagAssignments);
            return this._projectTagAssignments;
         })
        .catch(err => {
            this._projectTagAssignments = [];
            this._projectTagAssignmentsSubject.next(this._projectTagAssignments);
            throw err;
        })
        .finally(() => {
            this._projectTagAssignmentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ProjectTagAssignment. Call after mutations to force refresh.
     */
    public ClearProjectTagAssignmentsCache(): void {
        this._projectTagAssignments = null;
        this._projectTagAssignmentsPromise = null;
        this._projectTagAssignmentsSubject.next(this._projectTagAssignments);      // Emit to observable
    }

    public get HasProjectTagAssignments(): Promise<boolean> {
        return this.ProjectTagAssignments.then(projectTagAssignments => projectTagAssignments.length > 0);
    }




    /**
     * Updates the state of this ProjectTagData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ProjectTagData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ProjectTagSubmitData {
        return ProjectTagService.Instance.ConvertToProjectTagSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ProjectTagService extends SecureEndpointBase {

    private static _instance: ProjectTagService;
    private listCache: Map<string, Observable<Array<ProjectTagData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ProjectTagBasicListData>>>;
    private recordCache: Map<string, Observable<ProjectTagData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private projectTagAssignmentService: ProjectTagAssignmentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ProjectTagData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ProjectTagBasicListData>>>();
        this.recordCache = new Map<string, Observable<ProjectTagData>>();

        ProjectTagService._instance = this;
    }

    public static get Instance(): ProjectTagService {
      return ProjectTagService._instance;
    }


    public ClearListCaches(config: ProjectTagQueryParameters | null = null) {

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


    public ConvertToProjectTagSubmitData(data: ProjectTagData): ProjectTagSubmitData {

        let output = new ProjectTagSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetProjectTag(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectTagData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const projectTag$ = this.requestProjectTag(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ProjectTag", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, projectTag$);

            return projectTag$;
        }

        return this.recordCache.get(configHash) as Observable<ProjectTagData>;
    }

    private requestProjectTag(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectTagData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ProjectTagData>(this.baseUrl + 'api/ProjectTag/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveProjectTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectTag(id, includeRelations));
            }));
    }

    public GetProjectTagList(config: ProjectTagQueryParameters | any = null) : Observable<Array<ProjectTagData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const projectTagList$ = this.requestProjectTagList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ProjectTag list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, projectTagList$);

            return projectTagList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ProjectTagData>>;
    }


    private requestProjectTagList(config: ProjectTagQueryParameters | any) : Observable <Array<ProjectTagData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectTagData>>(this.baseUrl + 'api/ProjectTags', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveProjectTagList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectTagList(config));
            }));
    }

    public GetProjectTagsRowCount(config: ProjectTagQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const projectTagsRowCount$ = this.requestProjectTagsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ProjectTags row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, projectTagsRowCount$);

            return projectTagsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestProjectTagsRowCount(config: ProjectTagQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ProjectTags/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectTagsRowCount(config));
            }));
    }

    public GetProjectTagsBasicListData(config: ProjectTagQueryParameters | any = null) : Observable<Array<ProjectTagBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const projectTagsBasicListData$ = this.requestProjectTagsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ProjectTags basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, projectTagsBasicListData$);

            return projectTagsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ProjectTagBasicListData>>;
    }


    private requestProjectTagsBasicListData(config: ProjectTagQueryParameters | any) : Observable<Array<ProjectTagBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectTagBasicListData>>(this.baseUrl + 'api/ProjectTags/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectTagsBasicListData(config));
            }));

    }


    public PutProjectTag(id: bigint | number, projectTag: ProjectTagSubmitData) : Observable<ProjectTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ProjectTagData>(this.baseUrl + 'api/ProjectTag/' + id.toString(), projectTag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProjectTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutProjectTag(id, projectTag));
            }));
    }


    public PostProjectTag(projectTag: ProjectTagSubmitData) : Observable<ProjectTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ProjectTagData>(this.baseUrl + 'api/ProjectTag', projectTag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProjectTag(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostProjectTag(projectTag));
            }));
    }

  
    public DeleteProjectTag(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ProjectTag/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteProjectTag(id));
            }));
    }


    private getConfigHash(config: ProjectTagQueryParameters | any): string {

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

    public userIsBMCProjectTagReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCProjectTagReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ProjectTags
        //
        if (userIsBMCProjectTagReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCProjectTagReader = user.readPermission >= 1;
            } else {
                userIsBMCProjectTagReader = false;
            }
        }

        return userIsBMCProjectTagReader;
    }


    public userIsBMCProjectTagWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCProjectTagWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ProjectTags
        //
        if (userIsBMCProjectTagWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCProjectTagWriter = user.writePermission >= 1;
          } else {
            userIsBMCProjectTagWriter = false;
          }      
        }

        return userIsBMCProjectTagWriter;
    }

    public GetProjectTagAssignmentsForProjectTag(projectTagId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ProjectTagAssignmentData[]> {
        return this.projectTagAssignmentService.GetProjectTagAssignmentList({
            projectTagId: projectTagId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ProjectTagData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ProjectTagData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ProjectTagTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveProjectTag(raw: any): ProjectTagData {
    if (!raw) return raw;

    //
    // Create a ProjectTagData object instance with correct prototype
    //
    const revived = Object.create(ProjectTagData.prototype) as ProjectTagData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._projectTagAssignments = null;
    (revived as any)._projectTagAssignmentsPromise = null;
    (revived as any)._projectTagAssignmentsSubject = new BehaviorSubject<ProjectTagAssignmentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadProjectTagXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ProjectTagAssignments$ = (revived as any)._projectTagAssignmentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._projectTagAssignments === null && (revived as any)._projectTagAssignmentsPromise === null) {
                (revived as any).loadProjectTagAssignments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._projectTagAssignmentsCount$ = null;



    return revived;
  }

  private ReviveProjectTagList(rawList: any[]): ProjectTagData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveProjectTag(raw));
  }

}
