/*

   GENERATED SERVICE FOR THE PROJECTRENDER TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ProjectRender table.

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
import { RenderPresetData } from './render-preset.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ProjectRenderQueryParameters {
    projectId: bigint | number | null | undefined = null;
    renderPresetId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    outputFilePath: string | null | undefined = null;
    resolutionWidth: bigint | number | null | undefined = null;
    resolutionHeight: bigint | number | null | undefined = null;
    renderedDate: string | null | undefined = null;        // ISO 8601
    renderDurationSeconds: number | null | undefined = null;
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
export class ProjectRenderSubmitData {
    id!: bigint | number;
    projectId!: bigint | number;
    renderPresetId: bigint | number | null = null;
    name!: string;
    outputFilePath: string | null = null;
    resolutionWidth: bigint | number | null = null;
    resolutionHeight: bigint | number | null = null;
    renderedDate: string | null = null;     // ISO 8601
    renderDurationSeconds: number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ProjectRenderBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ProjectRenderChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `projectRender.ProjectRenderChildren$` — use with `| async` in templates
//        • Promise:    `projectRender.ProjectRenderChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="projectRender.ProjectRenderChildren$ | async"`), or
//        • Access the promise getter (`projectRender.ProjectRenderChildren` or `await projectRender.ProjectRenderChildren`)
//    - Simply reading `projectRender.ProjectRenderChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await projectRender.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ProjectRenderData {
    id!: bigint | number;
    projectId!: bigint | number;
    renderPresetId!: bigint | number;
    name!: string;
    outputFilePath!: string | null;
    resolutionWidth!: bigint | number;
    resolutionHeight!: bigint | number;
    renderedDate!: string | null;   // ISO 8601
    renderDurationSeconds!: number | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    project: ProjectData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    renderPreset: RenderPresetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ProjectRenderData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.projectRender.Reload();
  //
  //  Non Async:
  //
  //     projectRender[0].Reload().then(x => {
  //        this.projectRender = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ProjectRenderService.Instance.GetProjectRender(this.id, includeRelations)
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
     * Updates the state of this ProjectRenderData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ProjectRenderData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ProjectRenderSubmitData {
        return ProjectRenderService.Instance.ConvertToProjectRenderSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ProjectRenderService extends SecureEndpointBase {

    private static _instance: ProjectRenderService;
    private listCache: Map<string, Observable<Array<ProjectRenderData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ProjectRenderBasicListData>>>;
    private recordCache: Map<string, Observable<ProjectRenderData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ProjectRenderData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ProjectRenderBasicListData>>>();
        this.recordCache = new Map<string, Observable<ProjectRenderData>>();

        ProjectRenderService._instance = this;
    }

    public static get Instance(): ProjectRenderService {
      return ProjectRenderService._instance;
    }


    public ClearListCaches(config: ProjectRenderQueryParameters | null = null) {

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


    public ConvertToProjectRenderSubmitData(data: ProjectRenderData): ProjectRenderSubmitData {

        let output = new ProjectRenderSubmitData();

        output.id = data.id;
        output.projectId = data.projectId;
        output.renderPresetId = data.renderPresetId;
        output.name = data.name;
        output.outputFilePath = data.outputFilePath;
        output.resolutionWidth = data.resolutionWidth;
        output.resolutionHeight = data.resolutionHeight;
        output.renderedDate = data.renderedDate;
        output.renderDurationSeconds = data.renderDurationSeconds;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetProjectRender(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectRenderData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const projectRender$ = this.requestProjectRender(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ProjectRender", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, projectRender$);

            return projectRender$;
        }

        return this.recordCache.get(configHash) as Observable<ProjectRenderData>;
    }

    private requestProjectRender(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectRenderData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ProjectRenderData>(this.baseUrl + 'api/ProjectRender/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveProjectRender(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectRender(id, includeRelations));
            }));
    }

    public GetProjectRenderList(config: ProjectRenderQueryParameters | any = null) : Observable<Array<ProjectRenderData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const projectRenderList$ = this.requestProjectRenderList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ProjectRender list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, projectRenderList$);

            return projectRenderList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ProjectRenderData>>;
    }


    private requestProjectRenderList(config: ProjectRenderQueryParameters | any) : Observable <Array<ProjectRenderData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectRenderData>>(this.baseUrl + 'api/ProjectRenders', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveProjectRenderList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectRenderList(config));
            }));
    }

    public GetProjectRendersRowCount(config: ProjectRenderQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const projectRendersRowCount$ = this.requestProjectRendersRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ProjectRenders row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, projectRendersRowCount$);

            return projectRendersRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestProjectRendersRowCount(config: ProjectRenderQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ProjectRenders/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectRendersRowCount(config));
            }));
    }

    public GetProjectRendersBasicListData(config: ProjectRenderQueryParameters | any = null) : Observable<Array<ProjectRenderBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const projectRendersBasicListData$ = this.requestProjectRendersBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ProjectRenders basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, projectRendersBasicListData$);

            return projectRendersBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ProjectRenderBasicListData>>;
    }


    private requestProjectRendersBasicListData(config: ProjectRenderQueryParameters | any) : Observable<Array<ProjectRenderBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectRenderBasicListData>>(this.baseUrl + 'api/ProjectRenders/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectRendersBasicListData(config));
            }));

    }


    public PutProjectRender(id: bigint | number, projectRender: ProjectRenderSubmitData) : Observable<ProjectRenderData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ProjectRenderData>(this.baseUrl + 'api/ProjectRender/' + id.toString(), projectRender, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProjectRender(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutProjectRender(id, projectRender));
            }));
    }


    public PostProjectRender(projectRender: ProjectRenderSubmitData) : Observable<ProjectRenderData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ProjectRenderData>(this.baseUrl + 'api/ProjectRender', projectRender, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProjectRender(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostProjectRender(projectRender));
            }));
    }

  
    public DeleteProjectRender(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ProjectRender/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteProjectRender(id));
            }));
    }


    private getConfigHash(config: ProjectRenderQueryParameters | any): string {

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

    public userIsBMCProjectRenderReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCProjectRenderReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ProjectRenders
        //
        if (userIsBMCProjectRenderReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCProjectRenderReader = user.readPermission >= 1;
            } else {
                userIsBMCProjectRenderReader = false;
            }
        }

        return userIsBMCProjectRenderReader;
    }


    public userIsBMCProjectRenderWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCProjectRenderWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ProjectRenders
        //
        if (userIsBMCProjectRenderWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCProjectRenderWriter = user.writePermission >= 1;
          } else {
            userIsBMCProjectRenderWriter = false;
          }      
        }

        return userIsBMCProjectRenderWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ProjectRenderData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ProjectRenderData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ProjectRenderTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveProjectRender(raw: any): ProjectRenderData {
    if (!raw) return raw;

    //
    // Create a ProjectRenderData object instance with correct prototype
    //
    const revived = Object.create(ProjectRenderData.prototype) as ProjectRenderData;

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
    // 2. But private methods (loadProjectRenderXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveProjectRenderList(rawList: any[]): ProjectRenderData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveProjectRender(raw));
  }

}
