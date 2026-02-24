/*

   GENERATED SERVICE FOR THE EXPORTFORMAT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ExportFormat table.

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
import { ProjectExportService, ProjectExportData } from './project-export.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ExportFormatQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    fileExtension: string | null | undefined = null;
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
export class ExportFormatSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    fileExtension: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ExportFormatBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ExportFormatChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `exportFormat.ExportFormatChildren$` — use with `| async` in templates
//        • Promise:    `exportFormat.ExportFormatChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="exportFormat.ExportFormatChildren$ | async"`), or
//        • Access the promise getter (`exportFormat.ExportFormatChildren` or `await exportFormat.ExportFormatChildren`)
//    - Simply reading `exportFormat.ExportFormatChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await exportFormat.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ExportFormatData {
    id!: bigint | number;
    name!: string;
    description!: string;
    fileExtension!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _projectExports: ProjectExportData[] | null = null;
    private _projectExportsPromise: Promise<ProjectExportData[]> | null  = null;
    private _projectExportsSubject = new BehaviorSubject<ProjectExportData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ProjectExports$ = this._projectExportsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._projectExports === null && this._projectExportsPromise === null) {
            this.loadProjectExports(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _projectExportsCount$: Observable<bigint | number> | null = null;
    public get ProjectExportsCount$(): Observable<bigint | number> {
        if (this._projectExportsCount$ === null) {
            this._projectExportsCount$ = ProjectExportService.Instance.GetProjectExportsRowCount({exportFormatId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._projectExportsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ExportFormatData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.exportFormat.Reload();
  //
  //  Non Async:
  //
  //     exportFormat[0].Reload().then(x => {
  //        this.exportFormat = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ExportFormatService.Instance.GetExportFormat(this.id, includeRelations)
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
     this._projectExports = null;
     this._projectExportsPromise = null;
     this._projectExportsSubject.next(null);
     this._projectExportsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ProjectExports for this ExportFormat.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.exportFormat.ProjectExports.then(exportFormats => { ... })
     *   or
     *   await this.exportFormat.exportFormats
     *
    */
    public get ProjectExports(): Promise<ProjectExportData[]> {
        if (this._projectExports !== null) {
            return Promise.resolve(this._projectExports);
        }

        if (this._projectExportsPromise !== null) {
            return this._projectExportsPromise;
        }

        // Start the load
        this.loadProjectExports();

        return this._projectExportsPromise!;
    }



    private loadProjectExports(): void {

        this._projectExportsPromise = lastValueFrom(
            ExportFormatService.Instance.GetProjectExportsForExportFormat(this.id)
        )
        .then(ProjectExports => {
            this._projectExports = ProjectExports ?? [];
            this._projectExportsSubject.next(this._projectExports);
            return this._projectExports;
         })
        .catch(err => {
            this._projectExports = [];
            this._projectExportsSubject.next(this._projectExports);
            throw err;
        })
        .finally(() => {
            this._projectExportsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ProjectExport. Call after mutations to force refresh.
     */
    public ClearProjectExportsCache(): void {
        this._projectExports = null;
        this._projectExportsPromise = null;
        this._projectExportsSubject.next(this._projectExports);      // Emit to observable
    }

    public get HasProjectExports(): Promise<boolean> {
        return this.ProjectExports.then(projectExports => projectExports.length > 0);
    }




    /**
     * Updates the state of this ExportFormatData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ExportFormatData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ExportFormatSubmitData {
        return ExportFormatService.Instance.ConvertToExportFormatSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ExportFormatService extends SecureEndpointBase {

    private static _instance: ExportFormatService;
    private listCache: Map<string, Observable<Array<ExportFormatData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ExportFormatBasicListData>>>;
    private recordCache: Map<string, Observable<ExportFormatData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private projectExportService: ProjectExportService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ExportFormatData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ExportFormatBasicListData>>>();
        this.recordCache = new Map<string, Observable<ExportFormatData>>();

        ExportFormatService._instance = this;
    }

    public static get Instance(): ExportFormatService {
      return ExportFormatService._instance;
    }


    public ClearListCaches(config: ExportFormatQueryParameters | null = null) {

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


    public ConvertToExportFormatSubmitData(data: ExportFormatData): ExportFormatSubmitData {

        let output = new ExportFormatSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.fileExtension = data.fileExtension;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetExportFormat(id: bigint | number, includeRelations: boolean = true) : Observable<ExportFormatData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const exportFormat$ = this.requestExportFormat(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ExportFormat", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, exportFormat$);

            return exportFormat$;
        }

        return this.recordCache.get(configHash) as Observable<ExportFormatData>;
    }

    private requestExportFormat(id: bigint | number, includeRelations: boolean = true) : Observable<ExportFormatData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ExportFormatData>(this.baseUrl + 'api/ExportFormat/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveExportFormat(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestExportFormat(id, includeRelations));
            }));
    }

    public GetExportFormatList(config: ExportFormatQueryParameters | any = null) : Observable<Array<ExportFormatData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const exportFormatList$ = this.requestExportFormatList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ExportFormat list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, exportFormatList$);

            return exportFormatList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ExportFormatData>>;
    }


    private requestExportFormatList(config: ExportFormatQueryParameters | any) : Observable <Array<ExportFormatData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ExportFormatData>>(this.baseUrl + 'api/ExportFormats', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveExportFormatList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestExportFormatList(config));
            }));
    }

    public GetExportFormatsRowCount(config: ExportFormatQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const exportFormatsRowCount$ = this.requestExportFormatsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ExportFormats row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, exportFormatsRowCount$);

            return exportFormatsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestExportFormatsRowCount(config: ExportFormatQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ExportFormats/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestExportFormatsRowCount(config));
            }));
    }

    public GetExportFormatsBasicListData(config: ExportFormatQueryParameters | any = null) : Observable<Array<ExportFormatBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const exportFormatsBasicListData$ = this.requestExportFormatsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ExportFormats basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, exportFormatsBasicListData$);

            return exportFormatsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ExportFormatBasicListData>>;
    }


    private requestExportFormatsBasicListData(config: ExportFormatQueryParameters | any) : Observable<Array<ExportFormatBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ExportFormatBasicListData>>(this.baseUrl + 'api/ExportFormats/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestExportFormatsBasicListData(config));
            }));

    }


    public PutExportFormat(id: bigint | number, exportFormat: ExportFormatSubmitData) : Observable<ExportFormatData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ExportFormatData>(this.baseUrl + 'api/ExportFormat/' + id.toString(), exportFormat, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveExportFormat(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutExportFormat(id, exportFormat));
            }));
    }


    public PostExportFormat(exportFormat: ExportFormatSubmitData) : Observable<ExportFormatData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ExportFormatData>(this.baseUrl + 'api/ExportFormat', exportFormat, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveExportFormat(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostExportFormat(exportFormat));
            }));
    }

  
    public DeleteExportFormat(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ExportFormat/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteExportFormat(id));
            }));
    }


    private getConfigHash(config: ExportFormatQueryParameters | any): string {

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

    public userIsBMCExportFormatReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCExportFormatReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.ExportFormats
        //
        if (userIsBMCExportFormatReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCExportFormatReader = user.readPermission >= 1;
            } else {
                userIsBMCExportFormatReader = false;
            }
        }

        return userIsBMCExportFormatReader;
    }


    public userIsBMCExportFormatWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCExportFormatWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.ExportFormats
        //
        if (userIsBMCExportFormatWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCExportFormatWriter = user.writePermission >= 255;
          } else {
            userIsBMCExportFormatWriter = false;
          }      
        }

        return userIsBMCExportFormatWriter;
    }

    public GetProjectExportsForExportFormat(exportFormatId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ProjectExportData[]> {
        return this.projectExportService.GetProjectExportList({
            exportFormatId: exportFormatId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ExportFormatData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ExportFormatData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ExportFormatTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveExportFormat(raw: any): ExportFormatData {
    if (!raw) return raw;

    //
    // Create a ExportFormatData object instance with correct prototype
    //
    const revived = Object.create(ExportFormatData.prototype) as ExportFormatData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._projectExports = null;
    (revived as any)._projectExportsPromise = null;
    (revived as any)._projectExportsSubject = new BehaviorSubject<ProjectExportData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadExportFormatXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ProjectExports$ = (revived as any)._projectExportsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._projectExports === null && (revived as any)._projectExportsPromise === null) {
                (revived as any).loadProjectExports();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._projectExportsCount$ = null;



    return revived;
  }

  private ReviveExportFormatList(rawList: any[]): ExportFormatData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveExportFormat(raw));
  }

}
