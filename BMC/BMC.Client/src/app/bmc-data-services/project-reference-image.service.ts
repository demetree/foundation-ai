/*

   GENERATED SERVICE FOR THE PROJECTREFERENCEIMAGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ProjectReferenceImage table.

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
export class ProjectReferenceImageQueryParameters {
    projectId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    imageFilePath: string | null | undefined = null;
    opacity: number | null | undefined = null;
    positionX: number | null | undefined = null;
    positionY: number | null | undefined = null;
    positionZ: number | null | undefined = null;
    scaleX: number | null | undefined = null;
    scaleY: number | null | undefined = null;
    isVisible: boolean | null | undefined = null;
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
export class ProjectReferenceImageSubmitData {
    id!: bigint | number;
    projectId!: bigint | number;
    name!: string;
    imageFilePath: string | null = null;
    opacity: number | null = null;
    positionX: number | null = null;
    positionY: number | null = null;
    positionZ: number | null = null;
    scaleX: number | null = null;
    scaleY: number | null = null;
    isVisible!: boolean;
    active!: boolean;
    deleted!: boolean;
}


export class ProjectReferenceImageBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ProjectReferenceImageChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `projectReferenceImage.ProjectReferenceImageChildren$` — use with `| async` in templates
//        • Promise:    `projectReferenceImage.ProjectReferenceImageChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="projectReferenceImage.ProjectReferenceImageChildren$ | async"`), or
//        • Access the promise getter (`projectReferenceImage.ProjectReferenceImageChildren` or `await projectReferenceImage.ProjectReferenceImageChildren`)
//    - Simply reading `projectReferenceImage.ProjectReferenceImageChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await projectReferenceImage.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ProjectReferenceImageData {
    id!: bigint | number;
    projectId!: bigint | number;
    name!: string;
    imageFilePath!: string | null;
    opacity!: number | null;
    positionX!: number | null;
    positionY!: number | null;
    positionZ!: number | null;
    scaleX!: number | null;
    scaleY!: number | null;
    isVisible!: boolean;
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
  // Promise based reload method to allow rebuilding of any ProjectReferenceImageData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.projectReferenceImage.Reload();
  //
  //  Non Async:
  //
  //     projectReferenceImage[0].Reload().then(x => {
  //        this.projectReferenceImage = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ProjectReferenceImageService.Instance.GetProjectReferenceImage(this.id, includeRelations)
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
     * Updates the state of this ProjectReferenceImageData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ProjectReferenceImageData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ProjectReferenceImageSubmitData {
        return ProjectReferenceImageService.Instance.ConvertToProjectReferenceImageSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ProjectReferenceImageService extends SecureEndpointBase {

    private static _instance: ProjectReferenceImageService;
    private listCache: Map<string, Observable<Array<ProjectReferenceImageData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ProjectReferenceImageBasicListData>>>;
    private recordCache: Map<string, Observable<ProjectReferenceImageData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ProjectReferenceImageData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ProjectReferenceImageBasicListData>>>();
        this.recordCache = new Map<string, Observable<ProjectReferenceImageData>>();

        ProjectReferenceImageService._instance = this;
    }

    public static get Instance(): ProjectReferenceImageService {
      return ProjectReferenceImageService._instance;
    }


    public ClearListCaches(config: ProjectReferenceImageQueryParameters | null = null) {

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


    public ConvertToProjectReferenceImageSubmitData(data: ProjectReferenceImageData): ProjectReferenceImageSubmitData {

        let output = new ProjectReferenceImageSubmitData();

        output.id = data.id;
        output.projectId = data.projectId;
        output.name = data.name;
        output.imageFilePath = data.imageFilePath;
        output.opacity = data.opacity;
        output.positionX = data.positionX;
        output.positionY = data.positionY;
        output.positionZ = data.positionZ;
        output.scaleX = data.scaleX;
        output.scaleY = data.scaleY;
        output.isVisible = data.isVisible;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetProjectReferenceImage(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectReferenceImageData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const projectReferenceImage$ = this.requestProjectReferenceImage(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ProjectReferenceImage", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, projectReferenceImage$);

            return projectReferenceImage$;
        }

        return this.recordCache.get(configHash) as Observable<ProjectReferenceImageData>;
    }

    private requestProjectReferenceImage(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectReferenceImageData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ProjectReferenceImageData>(this.baseUrl + 'api/ProjectReferenceImage/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveProjectReferenceImage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectReferenceImage(id, includeRelations));
            }));
    }

    public GetProjectReferenceImageList(config: ProjectReferenceImageQueryParameters | any = null) : Observable<Array<ProjectReferenceImageData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const projectReferenceImageList$ = this.requestProjectReferenceImageList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ProjectReferenceImage list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, projectReferenceImageList$);

            return projectReferenceImageList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ProjectReferenceImageData>>;
    }


    private requestProjectReferenceImageList(config: ProjectReferenceImageQueryParameters | any) : Observable <Array<ProjectReferenceImageData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectReferenceImageData>>(this.baseUrl + 'api/ProjectReferenceImages', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveProjectReferenceImageList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectReferenceImageList(config));
            }));
    }

    public GetProjectReferenceImagesRowCount(config: ProjectReferenceImageQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const projectReferenceImagesRowCount$ = this.requestProjectReferenceImagesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ProjectReferenceImages row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, projectReferenceImagesRowCount$);

            return projectReferenceImagesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestProjectReferenceImagesRowCount(config: ProjectReferenceImageQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ProjectReferenceImages/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectReferenceImagesRowCount(config));
            }));
    }

    public GetProjectReferenceImagesBasicListData(config: ProjectReferenceImageQueryParameters | any = null) : Observable<Array<ProjectReferenceImageBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const projectReferenceImagesBasicListData$ = this.requestProjectReferenceImagesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ProjectReferenceImages basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, projectReferenceImagesBasicListData$);

            return projectReferenceImagesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ProjectReferenceImageBasicListData>>;
    }


    private requestProjectReferenceImagesBasicListData(config: ProjectReferenceImageQueryParameters | any) : Observable<Array<ProjectReferenceImageBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectReferenceImageBasicListData>>(this.baseUrl + 'api/ProjectReferenceImages/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectReferenceImagesBasicListData(config));
            }));

    }


    public PutProjectReferenceImage(id: bigint | number, projectReferenceImage: ProjectReferenceImageSubmitData) : Observable<ProjectReferenceImageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ProjectReferenceImageData>(this.baseUrl + 'api/ProjectReferenceImage/' + id.toString(), projectReferenceImage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProjectReferenceImage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutProjectReferenceImage(id, projectReferenceImage));
            }));
    }


    public PostProjectReferenceImage(projectReferenceImage: ProjectReferenceImageSubmitData) : Observable<ProjectReferenceImageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ProjectReferenceImageData>(this.baseUrl + 'api/ProjectReferenceImage', projectReferenceImage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProjectReferenceImage(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostProjectReferenceImage(projectReferenceImage));
            }));
    }

  
    public DeleteProjectReferenceImage(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ProjectReferenceImage/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteProjectReferenceImage(id));
            }));
    }


    private getConfigHash(config: ProjectReferenceImageQueryParameters | any): string {

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

    public userIsBMCProjectReferenceImageReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCProjectReferenceImageReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ProjectReferenceImages
        //
        if (userIsBMCProjectReferenceImageReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCProjectReferenceImageReader = user.readPermission >= 1;
            } else {
                userIsBMCProjectReferenceImageReader = false;
            }
        }

        return userIsBMCProjectReferenceImageReader;
    }


    public userIsBMCProjectReferenceImageWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCProjectReferenceImageWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ProjectReferenceImages
        //
        if (userIsBMCProjectReferenceImageWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCProjectReferenceImageWriter = user.writePermission >= 1;
          } else {
            userIsBMCProjectReferenceImageWriter = false;
          }      
        }

        return userIsBMCProjectReferenceImageWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ProjectReferenceImageData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ProjectReferenceImageData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ProjectReferenceImageTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveProjectReferenceImage(raw: any): ProjectReferenceImageData {
    if (!raw) return raw;

    //
    // Create a ProjectReferenceImageData object instance with correct prototype
    //
    const revived = Object.create(ProjectReferenceImageData.prototype) as ProjectReferenceImageData;

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
    // 2. But private methods (loadProjectReferenceImageXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveProjectReferenceImageList(rawList: any[]): ProjectReferenceImageData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveProjectReferenceImage(raw));
  }

}
