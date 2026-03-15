/*

   GENERATED SERVICE FOR THE ANNOUNCEMENTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AnnouncementChangeHistory table.

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
import { AnnouncementData } from './announcement.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AnnouncementChangeHistoryQueryParameters {
    Id: bigint | number | null | undefined = null;
    AnnouncementId: bigint | number | null | undefined = null;
    VersionNumber: bigint | number | null | undefined = null;
    TimeStamp: string | null | undefined = null;        // ISO 8601 (full datetime)
    UserId: bigint | number | null | undefined = null;
    Data: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class AnnouncementChangeHistorySubmitData {
    Id: bigint | number | null = null;
    AnnouncementId: bigint | number | null = null;
    VersionNumber: bigint | number | null = null;
    TimeStamp: string | null = null;     // ISO 8601 (full datetime)
    UserId: bigint | number | null = null;
    Data: string | null = null;
}


export class AnnouncementChangeHistoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AnnouncementChangeHistoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `announcementChangeHistory.AnnouncementChangeHistoryChildren$` — use with `| async` in templates
//        • Promise:    `announcementChangeHistory.AnnouncementChangeHistoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="announcementChangeHistory.AnnouncementChangeHistoryChildren$ | async"`), or
//        • Access the promise getter (`announcementChangeHistory.AnnouncementChangeHistoryChildren` or `await announcementChangeHistory.AnnouncementChangeHistoryChildren`)
//    - Simply reading `announcementChangeHistory.AnnouncementChangeHistoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await announcementChangeHistory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AnnouncementChangeHistoryData {
    Id!: bigint | number;
    AnnouncementId!: bigint | number;
    VersionNumber!: bigint | number;
    TimeStamp!: string | null;   // ISO 8601 (full datetime)
    UserId!: bigint | number;
    Data!: string | null;
    Announcement: AnnouncementData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any AnnouncementChangeHistoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.announcementChangeHistory.Reload();
  //
  //  Non Async:
  //
  //     announcementChangeHistory[0].Reload().then(x => {
  //        this.announcementChangeHistory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AnnouncementChangeHistoryService.Instance.GetAnnouncementChangeHistory(this.id, includeRelations)
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
     * Updates the state of this AnnouncementChangeHistoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AnnouncementChangeHistoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AnnouncementChangeHistorySubmitData {
        return AnnouncementChangeHistoryService.Instance.ConvertToAnnouncementChangeHistorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AnnouncementChangeHistoryService extends SecureEndpointBase {

    private static _instance: AnnouncementChangeHistoryService;
    private listCache: Map<string, Observable<Array<AnnouncementChangeHistoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AnnouncementChangeHistoryBasicListData>>>;
    private recordCache: Map<string, Observable<AnnouncementChangeHistoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AnnouncementChangeHistoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AnnouncementChangeHistoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<AnnouncementChangeHistoryData>>();

        AnnouncementChangeHistoryService._instance = this;
    }

    public static get Instance(): AnnouncementChangeHistoryService {
      return AnnouncementChangeHistoryService._instance;
    }


    public ClearListCaches(config: AnnouncementChangeHistoryQueryParameters | null = null) {

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


    public ConvertToAnnouncementChangeHistorySubmitData(data: AnnouncementChangeHistoryData): AnnouncementChangeHistorySubmitData {

        let output = new AnnouncementChangeHistorySubmitData();

        output.Id = data.Id;
        output.AnnouncementId = data.AnnouncementId;
        output.VersionNumber = data.VersionNumber;
        output.TimeStamp = data.TimeStamp;
        output.UserId = data.UserId;
        output.Data = data.Data;

        return output;
    }

    public GetAnnouncementChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<AnnouncementChangeHistoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const announcementChangeHistory$ = this.requestAnnouncementChangeHistory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AnnouncementChangeHistory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, announcementChangeHistory$);

            return announcementChangeHistory$;
        }

        return this.recordCache.get(configHash) as Observable<AnnouncementChangeHistoryData>;
    }

    private requestAnnouncementChangeHistory(id: bigint | number, includeRelations: boolean = true) : Observable<AnnouncementChangeHistoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AnnouncementChangeHistoryData>(this.baseUrl + 'api/AnnouncementChangeHistory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAnnouncementChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAnnouncementChangeHistory(id, includeRelations));
            }));
    }

    public GetAnnouncementChangeHistoryList(config: AnnouncementChangeHistoryQueryParameters | any = null) : Observable<Array<AnnouncementChangeHistoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const announcementChangeHistoryList$ = this.requestAnnouncementChangeHistoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AnnouncementChangeHistory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, announcementChangeHistoryList$);

            return announcementChangeHistoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AnnouncementChangeHistoryData>>;
    }


    private requestAnnouncementChangeHistoryList(config: AnnouncementChangeHistoryQueryParameters | any) : Observable <Array<AnnouncementChangeHistoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AnnouncementChangeHistoryData>>(this.baseUrl + 'api/AnnouncementChangeHistories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAnnouncementChangeHistoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAnnouncementChangeHistoryList(config));
            }));
    }

    public GetAnnouncementChangeHistoriesRowCount(config: AnnouncementChangeHistoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const announcementChangeHistoriesRowCount$ = this.requestAnnouncementChangeHistoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AnnouncementChangeHistories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, announcementChangeHistoriesRowCount$);

            return announcementChangeHistoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAnnouncementChangeHistoriesRowCount(config: AnnouncementChangeHistoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AnnouncementChangeHistories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAnnouncementChangeHistoriesRowCount(config));
            }));
    }

    public GetAnnouncementChangeHistoriesBasicListData(config: AnnouncementChangeHistoryQueryParameters | any = null) : Observable<Array<AnnouncementChangeHistoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const announcementChangeHistoriesBasicListData$ = this.requestAnnouncementChangeHistoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AnnouncementChangeHistories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, announcementChangeHistoriesBasicListData$);

            return announcementChangeHistoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AnnouncementChangeHistoryBasicListData>>;
    }


    private requestAnnouncementChangeHistoriesBasicListData(config: AnnouncementChangeHistoryQueryParameters | any) : Observable<Array<AnnouncementChangeHistoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AnnouncementChangeHistoryBasicListData>>(this.baseUrl + 'api/AnnouncementChangeHistories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAnnouncementChangeHistoriesBasicListData(config));
            }));

    }


    public PutAnnouncementChangeHistory(id: bigint | number, announcementChangeHistory: AnnouncementChangeHistorySubmitData) : Observable<AnnouncementChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AnnouncementChangeHistoryData>(this.baseUrl + 'api/AnnouncementChangeHistory/' + id.toString(), announcementChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAnnouncementChangeHistory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAnnouncementChangeHistory(id, announcementChangeHistory));
            }));
    }


    public PostAnnouncementChangeHistory(announcementChangeHistory: AnnouncementChangeHistorySubmitData) : Observable<AnnouncementChangeHistoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AnnouncementChangeHistoryData>(this.baseUrl + 'api/AnnouncementChangeHistory', announcementChangeHistory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAnnouncementChangeHistory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAnnouncementChangeHistory(announcementChangeHistory));
            }));
    }

  
    public DeleteAnnouncementChangeHistory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AnnouncementChangeHistory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAnnouncementChangeHistory(id));
            }));
    }


    private getConfigHash(config: AnnouncementChangeHistoryQueryParameters | any): string {

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

    public userIsCommunityAnnouncementChangeHistoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityAnnouncementChangeHistoryReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.AnnouncementChangeHistories
        //
        if (userIsCommunityAnnouncementChangeHistoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityAnnouncementChangeHistoryReader = user.readPermission >= 10;
            } else {
                userIsCommunityAnnouncementChangeHistoryReader = false;
            }
        }

        return userIsCommunityAnnouncementChangeHistoryReader;
    }


    public userIsCommunityAnnouncementChangeHistoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityAnnouncementChangeHistoryWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.AnnouncementChangeHistories
        //
        if (userIsCommunityAnnouncementChangeHistoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityAnnouncementChangeHistoryWriter = user.writePermission >= 255;
          } else {
            userIsCommunityAnnouncementChangeHistoryWriter = false;
          }      
        }

        return userIsCommunityAnnouncementChangeHistoryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full AnnouncementChangeHistoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AnnouncementChangeHistoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AnnouncementChangeHistoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAnnouncementChangeHistory(raw: any): AnnouncementChangeHistoryData {
    if (!raw) return raw;

    //
    // Create a AnnouncementChangeHistoryData object instance with correct prototype
    //
    const revived = Object.create(AnnouncementChangeHistoryData.prototype) as AnnouncementChangeHistoryData;

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
    // 2. But private methods (loadAnnouncementChangeHistoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveAnnouncementChangeHistoryList(rawList: any[]): AnnouncementChangeHistoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAnnouncementChangeHistory(raw));
  }

}
