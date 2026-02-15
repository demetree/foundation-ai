/*

   GENERATED SERVICE FOR THE PROJECTEXPORT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ProjectExport table.

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
import { ExportFormatData } from './export-format.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ProjectExportQueryParameters {
    projectId: bigint | number | null | undefined = null;
    exportFormatId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    outputFilePath: string | null | undefined = null;
    exportedDate: string | null | undefined = null;        // ISO 8601
    includeInstructions: boolean | null | undefined = null;
    includePartsList: boolean | null | undefined = null;
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
export class ProjectExportSubmitData {
    id!: bigint | number;
    projectId!: bigint | number;
    exportFormatId!: bigint | number;
    name!: string;
    outputFilePath: string | null = null;
    exportedDate: string | null = null;     // ISO 8601
    includeInstructions!: boolean;
    includePartsList!: boolean;
    active!: boolean;
    deleted!: boolean;
}


export class ProjectExportBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ProjectExportChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `projectExport.ProjectExportChildren$` — use with `| async` in templates
//        • Promise:    `projectExport.ProjectExportChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="projectExport.ProjectExportChildren$ | async"`), or
//        • Access the promise getter (`projectExport.ProjectExportChildren` or `await projectExport.ProjectExportChildren`)
//    - Simply reading `projectExport.ProjectExportChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await projectExport.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ProjectExportData {
    id!: bigint | number;
    projectId!: bigint | number;
    exportFormatId!: bigint | number;
    name!: string;
    outputFilePath!: string | null;
    exportedDate!: string | null;   // ISO 8601
    includeInstructions!: boolean;
    includePartsList!: boolean;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    exportFormat: ExportFormatData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
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
  // Promise based reload method to allow rebuilding of any ProjectExportData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.projectExport.Reload();
  //
  //  Non Async:
  //
  //     projectExport[0].Reload().then(x => {
  //        this.projectExport = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ProjectExportService.Instance.GetProjectExport(this.id, includeRelations)
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
     * Updates the state of this ProjectExportData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ProjectExportData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ProjectExportSubmitData {
        return ProjectExportService.Instance.ConvertToProjectExportSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ProjectExportService extends SecureEndpointBase {

    private static _instance: ProjectExportService;
    private listCache: Map<string, Observable<Array<ProjectExportData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ProjectExportBasicListData>>>;
    private recordCache: Map<string, Observable<ProjectExportData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ProjectExportData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ProjectExportBasicListData>>>();
        this.recordCache = new Map<string, Observable<ProjectExportData>>();

        ProjectExportService._instance = this;
    }

    public static get Instance(): ProjectExportService {
      return ProjectExportService._instance;
    }


    public ClearListCaches(config: ProjectExportQueryParameters | null = null) {

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


    public ConvertToProjectExportSubmitData(data: ProjectExportData): ProjectExportSubmitData {

        let output = new ProjectExportSubmitData();

        output.id = data.id;
        output.projectId = data.projectId;
        output.exportFormatId = data.exportFormatId;
        output.name = data.name;
        output.outputFilePath = data.outputFilePath;
        output.exportedDate = data.exportedDate;
        output.includeInstructions = data.includeInstructions;
        output.includePartsList = data.includePartsList;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetProjectExport(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectExportData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const projectExport$ = this.requestProjectExport(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ProjectExport", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, projectExport$);

            return projectExport$;
        }

        return this.recordCache.get(configHash) as Observable<ProjectExportData>;
    }

    private requestProjectExport(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectExportData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ProjectExportData>(this.baseUrl + 'api/ProjectExport/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveProjectExport(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectExport(id, includeRelations));
            }));
    }

    public GetProjectExportList(config: ProjectExportQueryParameters | any = null) : Observable<Array<ProjectExportData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const projectExportList$ = this.requestProjectExportList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ProjectExport list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, projectExportList$);

            return projectExportList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ProjectExportData>>;
    }


    private requestProjectExportList(config: ProjectExportQueryParameters | any) : Observable <Array<ProjectExportData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectExportData>>(this.baseUrl + 'api/ProjectExports', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveProjectExportList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectExportList(config));
            }));
    }

    public GetProjectExportsRowCount(config: ProjectExportQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const projectExportsRowCount$ = this.requestProjectExportsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ProjectExports row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, projectExportsRowCount$);

            return projectExportsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestProjectExportsRowCount(config: ProjectExportQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ProjectExports/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectExportsRowCount(config));
            }));
    }

    public GetProjectExportsBasicListData(config: ProjectExportQueryParameters | any = null) : Observable<Array<ProjectExportBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const projectExportsBasicListData$ = this.requestProjectExportsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ProjectExports basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, projectExportsBasicListData$);

            return projectExportsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ProjectExportBasicListData>>;
    }


    private requestProjectExportsBasicListData(config: ProjectExportQueryParameters | any) : Observable<Array<ProjectExportBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectExportBasicListData>>(this.baseUrl + 'api/ProjectExports/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectExportsBasicListData(config));
            }));

    }


    public PutProjectExport(id: bigint | number, projectExport: ProjectExportSubmitData) : Observable<ProjectExportData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ProjectExportData>(this.baseUrl + 'api/ProjectExport/' + id.toString(), projectExport, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProjectExport(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutProjectExport(id, projectExport));
            }));
    }


    public PostProjectExport(projectExport: ProjectExportSubmitData) : Observable<ProjectExportData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ProjectExportData>(this.baseUrl + 'api/ProjectExport', projectExport, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProjectExport(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostProjectExport(projectExport));
            }));
    }

  
    public DeleteProjectExport(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ProjectExport/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteProjectExport(id));
            }));
    }


    private getConfigHash(config: ProjectExportQueryParameters | any): string {

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

    public userIsBMCProjectExportReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCProjectExportReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ProjectExports
        //
        if (userIsBMCProjectExportReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCProjectExportReader = user.readPermission >= 1;
            } else {
                userIsBMCProjectExportReader = false;
            }
        }

        return userIsBMCProjectExportReader;
    }


    public userIsBMCProjectExportWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCProjectExportWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ProjectExports
        //
        if (userIsBMCProjectExportWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCProjectExportWriter = user.writePermission >= 1;
          } else {
            userIsBMCProjectExportWriter = false;
          }      
        }

        return userIsBMCProjectExportWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ProjectExportData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ProjectExportData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ProjectExportTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveProjectExport(raw: any): ProjectExportData {
    if (!raw) return raw;

    //
    // Create a ProjectExportData object instance with correct prototype
    //
    const revived = Object.create(ProjectExportData.prototype) as ProjectExportData;

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
    // 2. But private methods (loadProjectExportXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveProjectExportList(rawList: any[]): ProjectExportData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveProjectExport(raw));
  }

}
