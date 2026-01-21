/*

   GENERATED SERVICE FOR THE SYSTEMSETTING TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SystemSetting table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SystemSettingQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    value: string | null | undefined = null;
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
export class SystemSettingSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    value: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class SystemSettingBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SystemSettingChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `systemSetting.SystemSettingChildren$` — use with `| async` in templates
//        • Promise:    `systemSetting.SystemSettingChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="systemSetting.SystemSettingChildren$ | async"`), or
//        • Access the promise getter (`systemSetting.SystemSettingChildren` or `await systemSetting.SystemSettingChildren`)
//    - Simply reading `systemSetting.SystemSettingChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await systemSetting.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SystemSettingData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    value!: string | null;
    active!: boolean;
    deleted!: boolean;

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
  // Promise based reload method to allow rebuilding of any SystemSettingData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.systemSetting.Reload();
  //
  //  Non Async:
  //
  //     systemSetting[0].Reload().then(x => {
  //        this.systemSetting = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SystemSettingService.Instance.GetSystemSetting(this.id, includeRelations)
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
     * Updates the state of this SystemSettingData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SystemSettingData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SystemSettingSubmitData {
        return SystemSettingService.Instance.ConvertToSystemSettingSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SystemSettingService extends SecureEndpointBase {

    private static _instance: SystemSettingService;
    private listCache: Map<string, Observable<Array<SystemSettingData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SystemSettingBasicListData>>>;
    private recordCache: Map<string, Observable<SystemSettingData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SystemSettingData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SystemSettingBasicListData>>>();
        this.recordCache = new Map<string, Observable<SystemSettingData>>();

        SystemSettingService._instance = this;
    }

    public static get Instance(): SystemSettingService {
      return SystemSettingService._instance;
    }


    public ClearListCaches(config: SystemSettingQueryParameters | null = null) {

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


    public ConvertToSystemSettingSubmitData(data: SystemSettingData): SystemSettingSubmitData {

        let output = new SystemSettingSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.value = data.value;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSystemSetting(id: bigint | number, includeRelations: boolean = true) : Observable<SystemSettingData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const systemSetting$ = this.requestSystemSetting(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SystemSetting", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, systemSetting$);

            return systemSetting$;
        }

        return this.recordCache.get(configHash) as Observable<SystemSettingData>;
    }

    private requestSystemSetting(id: bigint | number, includeRelations: boolean = true) : Observable<SystemSettingData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SystemSettingData>(this.baseUrl + 'api/SystemSetting/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSystemSetting(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSystemSetting(id, includeRelations));
            }));
    }

    public GetSystemSettingList(config: SystemSettingQueryParameters | any = null) : Observable<Array<SystemSettingData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const systemSettingList$ = this.requestSystemSettingList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SystemSetting list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, systemSettingList$);

            return systemSettingList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SystemSettingData>>;
    }


    private requestSystemSettingList(config: SystemSettingQueryParameters | any) : Observable <Array<SystemSettingData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SystemSettingData>>(this.baseUrl + 'api/SystemSettings', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSystemSettingList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSystemSettingList(config));
            }));
    }

    public GetSystemSettingsRowCount(config: SystemSettingQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const systemSettingsRowCount$ = this.requestSystemSettingsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SystemSettings row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, systemSettingsRowCount$);

            return systemSettingsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSystemSettingsRowCount(config: SystemSettingQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SystemSettings/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSystemSettingsRowCount(config));
            }));
    }

    public GetSystemSettingsBasicListData(config: SystemSettingQueryParameters | any = null) : Observable<Array<SystemSettingBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const systemSettingsBasicListData$ = this.requestSystemSettingsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SystemSettings basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, systemSettingsBasicListData$);

            return systemSettingsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SystemSettingBasicListData>>;
    }


    private requestSystemSettingsBasicListData(config: SystemSettingQueryParameters | any) : Observable<Array<SystemSettingBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SystemSettingBasicListData>>(this.baseUrl + 'api/SystemSettings/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSystemSettingsBasicListData(config));
            }));

    }


    public PutSystemSetting(id: bigint | number, systemSetting: SystemSettingSubmitData) : Observable<SystemSettingData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SystemSettingData>(this.baseUrl + 'api/SystemSetting/' + id.toString(), systemSetting, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSystemSetting(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSystemSetting(id, systemSetting));
            }));
    }


    public PostSystemSetting(systemSetting: SystemSettingSubmitData) : Observable<SystemSettingData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SystemSettingData>(this.baseUrl + 'api/SystemSetting', systemSetting, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSystemSetting(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSystemSetting(systemSetting));
            }));
    }

  
    public DeleteSystemSetting(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SystemSetting/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSystemSetting(id));
            }));
    }


    private getConfigHash(config: SystemSettingQueryParameters | any): string {

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

    public userIsSecuritySystemSettingReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecuritySystemSettingReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.SystemSettings
        //
        if (userIsSecuritySystemSettingReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecuritySystemSettingReader = user.readPermission >= 0;
            } else {
                userIsSecuritySystemSettingReader = false;
            }
        }

        return userIsSecuritySystemSettingReader;
    }


    public userIsSecuritySystemSettingWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecuritySystemSettingWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.SystemSettings
        //
        if (userIsSecuritySystemSettingWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecuritySystemSettingWriter = user.writePermission >= 100;
          } else {
            userIsSecuritySystemSettingWriter = false;
          }      
        }

        return userIsSecuritySystemSettingWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full SystemSettingData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SystemSettingData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SystemSettingTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSystemSetting(raw: any): SystemSettingData {
    if (!raw) return raw;

    //
    // Create a SystemSettingData object instance with correct prototype
    //
    const revived = Object.create(SystemSettingData.prototype) as SystemSettingData;

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
    // 2. But private methods (loadSystemSettingXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveSystemSettingList(rawList: any[]): SystemSettingData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSystemSetting(raw));
  }

}
