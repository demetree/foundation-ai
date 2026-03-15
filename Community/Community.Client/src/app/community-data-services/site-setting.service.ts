/*

   GENERATED SERVICE FOR THE SITESETTING TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SiteSetting table.

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
export class SiteSettingQueryParameters {
    Id: bigint | number | null | undefined = null;
    SettingKey: string | null | undefined = null;
    SettingValue: string | null | undefined = null;
    Description: string | null | undefined = null;
    SettingGroup: string | null | undefined = null;
    ObjectGuid: string | null | undefined = null;
    Active: boolean | null | undefined = null;
    Deleted: boolean | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class SiteSettingSubmitData {
    Id: bigint | number | null = null;
    SettingKey: string | null = null;
    SettingValue: string | null = null;
    Description: string | null = null;
    SettingGroup: string | null = null;
    Active: boolean | null = null;
    Deleted: boolean | null = null;
}


export class SiteSettingBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SiteSettingChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `siteSetting.SiteSettingChildren$` — use with `| async` in templates
//        • Promise:    `siteSetting.SiteSettingChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="siteSetting.SiteSettingChildren$ | async"`), or
//        • Access the promise getter (`siteSetting.SiteSettingChildren` or `await siteSetting.SiteSettingChildren`)
//    - Simply reading `siteSetting.SiteSettingChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await siteSetting.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SiteSettingData {
    Id!: bigint | number;
    SettingKey!: string | null;
    SettingValue!: string | null;
    Description!: string | null;
    SettingGroup!: string | null;
    ObjectGuid!: string | null;
    Active!: boolean | null;
    Deleted!: boolean | null;

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
  // Promise based reload method to allow rebuilding of any SiteSettingData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.siteSetting.Reload();
  //
  //  Non Async:
  //
  //     siteSetting[0].Reload().then(x => {
  //        this.siteSetting = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SiteSettingService.Instance.GetSiteSetting(this.id, includeRelations)
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
     * Updates the state of this SiteSettingData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SiteSettingData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SiteSettingSubmitData {
        return SiteSettingService.Instance.ConvertToSiteSettingSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SiteSettingService extends SecureEndpointBase {

    private static _instance: SiteSettingService;
    private listCache: Map<string, Observable<Array<SiteSettingData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SiteSettingBasicListData>>>;
    private recordCache: Map<string, Observable<SiteSettingData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SiteSettingData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SiteSettingBasicListData>>>();
        this.recordCache = new Map<string, Observable<SiteSettingData>>();

        SiteSettingService._instance = this;
    }

    public static get Instance(): SiteSettingService {
      return SiteSettingService._instance;
    }


    public ClearListCaches(config: SiteSettingQueryParameters | null = null) {

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


    public ConvertToSiteSettingSubmitData(data: SiteSettingData): SiteSettingSubmitData {

        let output = new SiteSettingSubmitData();

        output.Id = data.Id;
        output.SettingKey = data.SettingKey;
        output.SettingValue = data.SettingValue;
        output.Description = data.Description;
        output.SettingGroup = data.SettingGroup;
        output.Active = data.Active;
        output.Deleted = data.Deleted;

        return output;
    }

    public GetSiteSetting(id: bigint | number, includeRelations: boolean = true) : Observable<SiteSettingData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const siteSetting$ = this.requestSiteSetting(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SiteSetting", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, siteSetting$);

            return siteSetting$;
        }

        return this.recordCache.get(configHash) as Observable<SiteSettingData>;
    }

    private requestSiteSetting(id: bigint | number, includeRelations: boolean = true) : Observable<SiteSettingData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SiteSettingData>(this.baseUrl + 'api/SiteSetting/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSiteSetting(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSiteSetting(id, includeRelations));
            }));
    }

    public GetSiteSettingList(config: SiteSettingQueryParameters | any = null) : Observable<Array<SiteSettingData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const siteSettingList$ = this.requestSiteSettingList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SiteSetting list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, siteSettingList$);

            return siteSettingList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SiteSettingData>>;
    }


    private requestSiteSettingList(config: SiteSettingQueryParameters | any) : Observable <Array<SiteSettingData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SiteSettingData>>(this.baseUrl + 'api/SiteSettings', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSiteSettingList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSiteSettingList(config));
            }));
    }

    public GetSiteSettingsRowCount(config: SiteSettingQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const siteSettingsRowCount$ = this.requestSiteSettingsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SiteSettings row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, siteSettingsRowCount$);

            return siteSettingsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSiteSettingsRowCount(config: SiteSettingQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SiteSettings/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSiteSettingsRowCount(config));
            }));
    }

    public GetSiteSettingsBasicListData(config: SiteSettingQueryParameters | any = null) : Observable<Array<SiteSettingBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const siteSettingsBasicListData$ = this.requestSiteSettingsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SiteSettings basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, siteSettingsBasicListData$);

            return siteSettingsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SiteSettingBasicListData>>;
    }


    private requestSiteSettingsBasicListData(config: SiteSettingQueryParameters | any) : Observable<Array<SiteSettingBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SiteSettingBasicListData>>(this.baseUrl + 'api/SiteSettings/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSiteSettingsBasicListData(config));
            }));

    }


    public PutSiteSetting(id: bigint | number, siteSetting: SiteSettingSubmitData) : Observable<SiteSettingData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SiteSettingData>(this.baseUrl + 'api/SiteSetting/' + id.toString(), siteSetting, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSiteSetting(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSiteSetting(id, siteSetting));
            }));
    }


    public PostSiteSetting(siteSetting: SiteSettingSubmitData) : Observable<SiteSettingData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SiteSettingData>(this.baseUrl + 'api/SiteSetting', siteSetting, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSiteSetting(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSiteSetting(siteSetting));
            }));
    }

  
    public DeleteSiteSetting(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SiteSetting/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSiteSetting(id));
            }));
    }


    private getConfigHash(config: SiteSettingQueryParameters | any): string {

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

    public userIsCommunitySiteSettingReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunitySiteSettingReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.SiteSettings
        //
        if (userIsCommunitySiteSettingReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunitySiteSettingReader = user.readPermission >= 1;
            } else {
                userIsCommunitySiteSettingReader = false;
            }
        }

        return userIsCommunitySiteSettingReader;
    }


    public userIsCommunitySiteSettingWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunitySiteSettingWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.SiteSettings
        //
        if (userIsCommunitySiteSettingWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunitySiteSettingWriter = user.writePermission >= 100;
          } else {
            userIsCommunitySiteSettingWriter = false;
          }      
        }

        return userIsCommunitySiteSettingWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full SiteSettingData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SiteSettingData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SiteSettingTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSiteSetting(raw: any): SiteSettingData {
    if (!raw) return raw;

    //
    // Create a SiteSettingData object instance with correct prototype
    //
    const revived = Object.create(SiteSettingData.prototype) as SiteSettingData;

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
    // 2. But private methods (loadSiteSettingXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveSiteSettingList(rawList: any[]): SiteSettingData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSiteSetting(raw));
  }

}
