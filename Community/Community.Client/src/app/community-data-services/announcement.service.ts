/*

   GENERATED SERVICE FOR THE ANNOUNCEMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Announcement table.

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
import { AnnouncementChangeHistoryService, AnnouncementChangeHistoryData } from './announcement-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AnnouncementQueryParameters {
    Id: bigint | number | null | undefined = null;
    Title: string | null | undefined = null;
    Body: string | null | undefined = null;
    Severity: string | null | undefined = null;
    StartDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    EndDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    IsPinned: boolean | null | undefined = null;
    VersionNumber: bigint | number | null | undefined = null;
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
export class AnnouncementSubmitData {
    Id: bigint | number | null = null;
    Title: string | null = null;
    Body: string | null = null;
    Severity: string | null = null;
    StartDate: string | null = null;     // ISO 8601 (full datetime)
    EndDate: string | null = null;     // ISO 8601 (full datetime)
    IsPinned: boolean | null = null;
    VersionNumber: bigint | number | null = null;
    Active: boolean | null = null;
    Deleted: boolean | null = null;
}



//
// Version history information returned from version history API endpoints.
// Matches server-side VersionInformation<T> structure.
//
export interface VersionInformationUser {
    id: bigint | number;
    userName: string;
    firstName: string | null;
    middleName: string | null;
    lastName: string | null;
}

export interface VersionInformation<T> {
    timeStamp: string;           // ISO 8601
    user: VersionInformationUser;
    versionNumber: number;
    data: T | null;
}

export class AnnouncementBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AnnouncementChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `announcement.AnnouncementChildren$` — use with `| async` in templates
//        • Promise:    `announcement.AnnouncementChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="announcement.AnnouncementChildren$ | async"`), or
//        • Access the promise getter (`announcement.AnnouncementChildren` or `await announcement.AnnouncementChildren`)
//    - Simply reading `announcement.AnnouncementChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await announcement.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AnnouncementData {
    Id!: bigint | number;
    Title!: string | null;
    Body!: string | null;
    Severity!: string | null;
    StartDate!: string | null;   // ISO 8601 (full datetime)
    EndDate!: string | null;   // ISO 8601 (full datetime)
    IsPinned!: boolean | null;
    VersionNumber!: bigint | number;
    ObjectGuid!: string | null;
    Active!: boolean | null;
    Deleted!: boolean | null;

    //
    // Private lazy-loading caches for related collections
    //
    private _announcementChangeHistories: AnnouncementChangeHistoryData[] | null = null;
    private _announcementChangeHistoriesPromise: Promise<AnnouncementChangeHistoryData[]> | null  = null;
    private _announcementChangeHistoriesSubject = new BehaviorSubject<AnnouncementChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<AnnouncementData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<AnnouncementData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<AnnouncementData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public AnnouncementChangeHistories$ = this._announcementChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._announcementChangeHistories === null && this._announcementChangeHistoriesPromise === null) {
            this.loadAnnouncementChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _announcementChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get AnnouncementChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._announcementChangeHistoriesCount$ === null) {
            this._announcementChangeHistoriesCount$ = AnnouncementChangeHistoryService.Instance.GetAnnouncementChangeHistoriesRowCount({announcementId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._announcementChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AnnouncementData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.announcement.Reload();
  //
  //  Non Async:
  //
  //     announcement[0].Reload().then(x => {
  //        this.announcement = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AnnouncementService.Instance.GetAnnouncement(this.id, includeRelations)
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
     this._announcementChangeHistories = null;
     this._announcementChangeHistoriesPromise = null;
     this._announcementChangeHistoriesSubject.next(null);
     this._announcementChangeHistoriesCount$ = null;

     this._currentVersionInfo = null;
     this._currentVersionInfoPromise = null;
     this._currentVersionInfoSubject.next(null);
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the AnnouncementChangeHistories for this Announcement.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.announcement.AnnouncementChangeHistories.then(announcements => { ... })
     *   or
     *   await this.announcement.announcements
     *
    */
    public get AnnouncementChangeHistories(): Promise<AnnouncementChangeHistoryData[]> {
        if (this._announcementChangeHistories !== null) {
            return Promise.resolve(this._announcementChangeHistories);
        }

        if (this._announcementChangeHistoriesPromise !== null) {
            return this._announcementChangeHistoriesPromise;
        }

        // Start the load
        this.loadAnnouncementChangeHistories();

        return this._announcementChangeHistoriesPromise!;
    }



    private loadAnnouncementChangeHistories(): void {

        this._announcementChangeHistoriesPromise = lastValueFrom(
            AnnouncementService.Instance.GetAnnouncementChangeHistoriesForAnnouncement(this.id)
        )
        .then(AnnouncementChangeHistories => {
            this._announcementChangeHistories = AnnouncementChangeHistories ?? [];
            this._announcementChangeHistoriesSubject.next(this._announcementChangeHistories);
            return this._announcementChangeHistories;
         })
        .catch(err => {
            this._announcementChangeHistories = [];
            this._announcementChangeHistoriesSubject.next(this._announcementChangeHistories);
            throw err;
        })
        .finally(() => {
            this._announcementChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached AnnouncementChangeHistory. Call after mutations to force refresh.
     */
    public ClearAnnouncementChangeHistoriesCache(): void {
        this._announcementChangeHistories = null;
        this._announcementChangeHistoriesPromise = null;
        this._announcementChangeHistoriesSubject.next(this._announcementChangeHistories);      // Emit to observable
    }

    public get HasAnnouncementChangeHistories(): Promise<boolean> {
        return this.AnnouncementChangeHistories.then(announcementChangeHistories => announcementChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (announcement.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await announcement.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<AnnouncementData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<AnnouncementData>> {
        const info = await lastValueFrom(
            AnnouncementService.Instance.GetAnnouncementChangeMetadata(this.id, this.versionNumber as number)
        );
        this._currentVersionInfo = info;
        this._currentVersionInfoSubject.next(info);
        return info;
    }


    public ClearCurrentVersionInfoCache(): void {
        this._currentVersionInfo = null;
        this._currentVersionInfoPromise = null;
        this._currentVersionInfoSubject.next(null);
    }



    /**
     * Updates the state of this AnnouncementData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AnnouncementData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AnnouncementSubmitData {
        return AnnouncementService.Instance.ConvertToAnnouncementSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AnnouncementService extends SecureEndpointBase {

    private static _instance: AnnouncementService;
    private listCache: Map<string, Observable<Array<AnnouncementData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AnnouncementBasicListData>>>;
    private recordCache: Map<string, Observable<AnnouncementData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private announcementChangeHistoryService: AnnouncementChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AnnouncementData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AnnouncementBasicListData>>>();
        this.recordCache = new Map<string, Observable<AnnouncementData>>();

        AnnouncementService._instance = this;
    }

    public static get Instance(): AnnouncementService {
      return AnnouncementService._instance;
    }


    public ClearListCaches(config: AnnouncementQueryParameters | null = null) {

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


    public ConvertToAnnouncementSubmitData(data: AnnouncementData): AnnouncementSubmitData {

        let output = new AnnouncementSubmitData();

        output.Id = data.Id;
        output.Title = data.Title;
        output.Body = data.Body;
        output.Severity = data.Severity;
        output.StartDate = data.StartDate;
        output.EndDate = data.EndDate;
        output.IsPinned = data.IsPinned;
        output.VersionNumber = data.VersionNumber;
        output.Active = data.Active;
        output.Deleted = data.Deleted;

        return output;
    }

    public GetAnnouncement(id: bigint | number, includeRelations: boolean = true) : Observable<AnnouncementData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const announcement$ = this.requestAnnouncement(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Announcement", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, announcement$);

            return announcement$;
        }

        return this.recordCache.get(configHash) as Observable<AnnouncementData>;
    }

    private requestAnnouncement(id: bigint | number, includeRelations: boolean = true) : Observable<AnnouncementData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AnnouncementData>(this.baseUrl + 'api/Announcement/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAnnouncement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAnnouncement(id, includeRelations));
            }));
    }

    public GetAnnouncementList(config: AnnouncementQueryParameters | any = null) : Observable<Array<AnnouncementData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const announcementList$ = this.requestAnnouncementList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Announcement list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, announcementList$);

            return announcementList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AnnouncementData>>;
    }


    private requestAnnouncementList(config: AnnouncementQueryParameters | any) : Observable <Array<AnnouncementData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AnnouncementData>>(this.baseUrl + 'api/Announcements', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAnnouncementList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAnnouncementList(config));
            }));
    }

    public GetAnnouncementsRowCount(config: AnnouncementQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const announcementsRowCount$ = this.requestAnnouncementsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Announcements row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, announcementsRowCount$);

            return announcementsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAnnouncementsRowCount(config: AnnouncementQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Announcements/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAnnouncementsRowCount(config));
            }));
    }

    public GetAnnouncementsBasicListData(config: AnnouncementQueryParameters | any = null) : Observable<Array<AnnouncementBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const announcementsBasicListData$ = this.requestAnnouncementsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Announcements basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, announcementsBasicListData$);

            return announcementsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AnnouncementBasicListData>>;
    }


    private requestAnnouncementsBasicListData(config: AnnouncementQueryParameters | any) : Observable<Array<AnnouncementBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AnnouncementBasicListData>>(this.baseUrl + 'api/Announcements/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAnnouncementsBasicListData(config));
            }));

    }


    public PutAnnouncement(id: bigint | number, announcement: AnnouncementSubmitData) : Observable<AnnouncementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AnnouncementData>(this.baseUrl + 'api/Announcement/' + id.toString(), announcement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAnnouncement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAnnouncement(id, announcement));
            }));
    }


    public PostAnnouncement(announcement: AnnouncementSubmitData) : Observable<AnnouncementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AnnouncementData>(this.baseUrl + 'api/Announcement', announcement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAnnouncement(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAnnouncement(announcement));
            }));
    }

  
    public DeleteAnnouncement(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Announcement/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAnnouncement(id));
            }));
    }

    public RollbackAnnouncement(id: bigint | number, versionNumber: bigint | number) : Observable<AnnouncementData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AnnouncementData>(this.baseUrl + 'api/Announcement/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAnnouncement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackAnnouncement(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Announcement.
     */
    public GetAnnouncementChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<AnnouncementData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<AnnouncementData>>(this.baseUrl + 'api/Announcement/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetAnnouncementChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Announcement.
     */
    public GetAnnouncementAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<AnnouncementData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<AnnouncementData>[]>(this.baseUrl + 'api/Announcement/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetAnnouncementAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Announcement.
     */
    public GetAnnouncementVersion(id: bigint | number, version: number): Observable<AnnouncementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AnnouncementData>(this.baseUrl + 'api/Announcement/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveAnnouncement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetAnnouncementVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Announcement at a specific point in time.
     */
    public GetAnnouncementStateAtTime(id: bigint | number, time: string): Observable<AnnouncementData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AnnouncementData>(this.baseUrl + 'api/Announcement/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveAnnouncement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetAnnouncementStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: AnnouncementQueryParameters | any): string {

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

    public userIsCommunityAnnouncementReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityAnnouncementReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.Announcements
        //
        if (userIsCommunityAnnouncementReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityAnnouncementReader = user.readPermission >= 1;
            } else {
                userIsCommunityAnnouncementReader = false;
            }
        }

        return userIsCommunityAnnouncementReader;
    }


    public userIsCommunityAnnouncementWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityAnnouncementWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.Announcements
        //
        if (userIsCommunityAnnouncementWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityAnnouncementWriter = user.writePermission >= 10;
          } else {
            userIsCommunityAnnouncementWriter = false;
          }      
        }

        return userIsCommunityAnnouncementWriter;
    }

    public GetAnnouncementChangeHistoriesForAnnouncement(announcementId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AnnouncementChangeHistoryData[]> {
        return this.announcementChangeHistoryService.GetAnnouncementChangeHistoryList({
            announcementId: announcementId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AnnouncementData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AnnouncementData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AnnouncementTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAnnouncement(raw: any): AnnouncementData {
    if (!raw) return raw;

    //
    // Create a AnnouncementData object instance with correct prototype
    //
    const revived = Object.create(AnnouncementData.prototype) as AnnouncementData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._announcementChangeHistories = null;
    (revived as any)._announcementChangeHistoriesPromise = null;
    (revived as any)._announcementChangeHistoriesSubject = new BehaviorSubject<AnnouncementChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAnnouncementXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).AnnouncementChangeHistories$ = (revived as any)._announcementChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._announcementChangeHistories === null && (revived as any)._announcementChangeHistoriesPromise === null) {
                (revived as any).loadAnnouncementChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._announcementChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<AnnouncementData> | null>(null);

    (revived as any).CurrentVersionInfo$ = (revived as any)._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if ((revived as any)._currentVersionInfo === null && (revived as any)._currentVersionInfoPromise === null) {
                (revived as any).loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    return revived;
  }

  private ReviveAnnouncementList(rawList: any[]): AnnouncementData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAnnouncement(raw));
  }

}
