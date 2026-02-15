/*

   GENERATED SERVICE FOR THE PROJECTCAMERAPRESET TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ProjectCameraPreset table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ProjectCameraPresetQueryParameters {
    projectId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    positionX: number | null | undefined = null;
    positionY: number | null | undefined = null;
    positionZ: number | null | undefined = null;
    targetX: number | null | undefined = null;
    targetY: number | null | undefined = null;
    targetZ: number | null | undefined = null;
    zoom: number | null | undefined = null;
    isPerspective: boolean | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class ProjectCameraPresetSubmitData {
    id!: bigint | number;
    projectId!: bigint | number;
    name!: string;
    positionX: number | null = null;
    positionY: number | null = null;
    positionZ: number | null = null;
    targetX: number | null = null;
    targetY: number | null = null;
    targetZ: number | null = null;
    zoom: number | null = null;
    isPerspective!: boolean;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ProjectCameraPresetBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ProjectCameraPresetChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `projectCameraPreset.ProjectCameraPresetChildren$` — use with `| async` in templates
//        • Promise:    `projectCameraPreset.ProjectCameraPresetChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="projectCameraPreset.ProjectCameraPresetChildren$ | async"`), or
//        • Access the promise getter (`projectCameraPreset.ProjectCameraPresetChildren` or `await projectCameraPreset.ProjectCameraPresetChildren`)
//    - Simply reading `projectCameraPreset.ProjectCameraPresetChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await projectCameraPreset.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ProjectCameraPresetData {
    id!: bigint | number;
    projectId!: bigint | number;
    name!: string;
    positionX!: number | null;
    positionY!: number | null;
    positionZ!: number | null;
    targetX!: number | null;
    targetY!: number | null;
    targetZ!: number | null;
    zoom!: number | null;
    isPerspective!: boolean;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    project: ProjectData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ProjectCameraPresetData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.projectCameraPreset.Reload();
  //
  //  Non Async:
  //
  //     projectCameraPreset[0].Reload().then(x => {
  //        this.projectCameraPreset = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ProjectCameraPresetService.Instance.GetProjectCameraPreset(this.id, includeRelations)
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
     * Updates the state of this ProjectCameraPresetData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ProjectCameraPresetData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ProjectCameraPresetSubmitData {
        return ProjectCameraPresetService.Instance.ConvertToProjectCameraPresetSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ProjectCameraPresetService extends SecureEndpointBase {

    private static _instance: ProjectCameraPresetService;
    private listCache: Map<string, Observable<Array<ProjectCameraPresetData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ProjectCameraPresetBasicListData>>>;
    private recordCache: Map<string, Observable<ProjectCameraPresetData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ProjectCameraPresetData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ProjectCameraPresetBasicListData>>>();
        this.recordCache = new Map<string, Observable<ProjectCameraPresetData>>();

        ProjectCameraPresetService._instance = this;
    }

    public static get Instance(): ProjectCameraPresetService {
      return ProjectCameraPresetService._instance;
    }


    public ClearListCaches(config: ProjectCameraPresetQueryParameters | null = null) {

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


    public ConvertToProjectCameraPresetSubmitData(data: ProjectCameraPresetData): ProjectCameraPresetSubmitData {

        let output = new ProjectCameraPresetSubmitData();

        output.id = data.id;
        output.projectId = data.projectId;
        output.name = data.name;
        output.positionX = data.positionX;
        output.positionY = data.positionY;
        output.positionZ = data.positionZ;
        output.targetX = data.targetX;
        output.targetY = data.targetY;
        output.targetZ = data.targetZ;
        output.zoom = data.zoom;
        output.isPerspective = data.isPerspective;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetProjectCameraPreset(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectCameraPresetData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const projectCameraPreset$ = this.requestProjectCameraPreset(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ProjectCameraPreset", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, projectCameraPreset$);

            return projectCameraPreset$;
        }

        return this.recordCache.get(configHash) as Observable<ProjectCameraPresetData>;
    }

    private requestProjectCameraPreset(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectCameraPresetData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ProjectCameraPresetData>(this.baseUrl + 'api/ProjectCameraPreset/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveProjectCameraPreset(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectCameraPreset(id, includeRelations));
            }));
    }

    public GetProjectCameraPresetList(config: ProjectCameraPresetQueryParameters | any = null) : Observable<Array<ProjectCameraPresetData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const projectCameraPresetList$ = this.requestProjectCameraPresetList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ProjectCameraPreset list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, projectCameraPresetList$);

            return projectCameraPresetList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ProjectCameraPresetData>>;
    }


    private requestProjectCameraPresetList(config: ProjectCameraPresetQueryParameters | any) : Observable <Array<ProjectCameraPresetData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectCameraPresetData>>(this.baseUrl + 'api/ProjectCameraPresets', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveProjectCameraPresetList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectCameraPresetList(config));
            }));
    }

    public GetProjectCameraPresetsRowCount(config: ProjectCameraPresetQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const projectCameraPresetsRowCount$ = this.requestProjectCameraPresetsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ProjectCameraPresets row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, projectCameraPresetsRowCount$);

            return projectCameraPresetsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestProjectCameraPresetsRowCount(config: ProjectCameraPresetQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ProjectCameraPresets/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectCameraPresetsRowCount(config));
            }));
    }

    public GetProjectCameraPresetsBasicListData(config: ProjectCameraPresetQueryParameters | any = null) : Observable<Array<ProjectCameraPresetBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const projectCameraPresetsBasicListData$ = this.requestProjectCameraPresetsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ProjectCameraPresets basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, projectCameraPresetsBasicListData$);

            return projectCameraPresetsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ProjectCameraPresetBasicListData>>;
    }


    private requestProjectCameraPresetsBasicListData(config: ProjectCameraPresetQueryParameters | any) : Observable<Array<ProjectCameraPresetBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectCameraPresetBasicListData>>(this.baseUrl + 'api/ProjectCameraPresets/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectCameraPresetsBasicListData(config));
            }));

    }


    public PutProjectCameraPreset(id: bigint | number, projectCameraPreset: ProjectCameraPresetSubmitData) : Observable<ProjectCameraPresetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ProjectCameraPresetData>(this.baseUrl + 'api/ProjectCameraPreset/' + id.toString(), projectCameraPreset, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProjectCameraPreset(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutProjectCameraPreset(id, projectCameraPreset));
            }));
    }


    public PostProjectCameraPreset(projectCameraPreset: ProjectCameraPresetSubmitData) : Observable<ProjectCameraPresetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ProjectCameraPresetData>(this.baseUrl + 'api/ProjectCameraPreset', projectCameraPreset, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProjectCameraPreset(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostProjectCameraPreset(projectCameraPreset));
            }));
    }

  
    public DeleteProjectCameraPreset(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ProjectCameraPreset/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteProjectCameraPreset(id));
            }));
    }


    private getConfigHash(config: ProjectCameraPresetQueryParameters | any): string {

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

    public userIsBMCProjectCameraPresetReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCProjectCameraPresetReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ProjectCameraPresets
        //
        if (userIsBMCProjectCameraPresetReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCProjectCameraPresetReader = user.readPermission >= 1;
            } else {
                userIsBMCProjectCameraPresetReader = false;
            }
        }

        return userIsBMCProjectCameraPresetReader;
    }


    public userIsBMCProjectCameraPresetWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCProjectCameraPresetWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ProjectCameraPresets
        //
        if (userIsBMCProjectCameraPresetWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCProjectCameraPresetWriter = user.writePermission >= 1;
          } else {
            userIsBMCProjectCameraPresetWriter = false;
          }      
        }

        return userIsBMCProjectCameraPresetWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ProjectCameraPresetData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ProjectCameraPresetData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ProjectCameraPresetTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveProjectCameraPreset(raw: any): ProjectCameraPresetData {
    if (!raw) return raw;

    //
    // Create a ProjectCameraPresetData object instance with correct prototype
    //
    const revived = Object.create(ProjectCameraPresetData.prototype) as ProjectCameraPresetData;

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
    // 2. But private methods (loadProjectCameraPresetXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveProjectCameraPresetList(rawList: any[]): ProjectCameraPresetData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveProjectCameraPreset(raw));
  }

}
