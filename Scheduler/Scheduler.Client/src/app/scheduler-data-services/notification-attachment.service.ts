/*

   GENERATED SERVICE FOR THE NOTIFICATIONATTACHMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the NotificationAttachment table.

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
import { NotificationData } from './notification.service';
import { NotificationAttachmentChangeHistoryService, NotificationAttachmentChangeHistoryData } from './notification-attachment-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class NotificationAttachmentQueryParameters {
    notificationId: bigint | number | null | undefined = null;
    userId: bigint | number | null | undefined = null;
    dateTimeCreated: string | null | undefined = null;        // ISO 8601 (full datetime)
    contentFileName: string | null | undefined = null;
    contentSize: bigint | number | null | undefined = null;
    contentMimeType: string | null | undefined = null;
    versionNumber: bigint | number | null | undefined = null;
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
export class NotificationAttachmentSubmitData {
    id!: bigint | number;
    notificationId!: bigint | number;
    userId!: bigint | number;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    contentFileName!: string;
    contentSize!: bigint | number;
    contentData!: string;
    contentMimeType!: string;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
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

export class NotificationAttachmentBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. NotificationAttachmentChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `notificationAttachment.NotificationAttachmentChildren$` — use with `| async` in templates
//        • Promise:    `notificationAttachment.NotificationAttachmentChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="notificationAttachment.NotificationAttachmentChildren$ | async"`), or
//        • Access the promise getter (`notificationAttachment.NotificationAttachmentChildren` or `await notificationAttachment.NotificationAttachmentChildren`)
//    - Simply reading `notificationAttachment.NotificationAttachmentChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await notificationAttachment.Reload()` to refresh the entire object and clear all lazy caches.
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
export class NotificationAttachmentData {
    id!: bigint | number;
    notificationId!: bigint | number;
    userId!: bigint | number;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    contentFileName!: string;
    contentSize!: bigint | number;
    contentData!: string;
    contentMimeType!: string;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    notification: NotificationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _notificationAttachmentChangeHistories: NotificationAttachmentChangeHistoryData[] | null = null;
    private _notificationAttachmentChangeHistoriesPromise: Promise<NotificationAttachmentChangeHistoryData[]> | null  = null;
    private _notificationAttachmentChangeHistoriesSubject = new BehaviorSubject<NotificationAttachmentChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<NotificationAttachmentData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<NotificationAttachmentData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<NotificationAttachmentData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public NotificationAttachmentChangeHistories$ = this._notificationAttachmentChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._notificationAttachmentChangeHistories === null && this._notificationAttachmentChangeHistoriesPromise === null) {
            this.loadNotificationAttachmentChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _notificationAttachmentChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get NotificationAttachmentChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._notificationAttachmentChangeHistoriesCount$ === null) {
            this._notificationAttachmentChangeHistoriesCount$ = NotificationAttachmentChangeHistoryService.Instance.GetNotificationAttachmentChangeHistoriesRowCount({notificationAttachmentId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._notificationAttachmentChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any NotificationAttachmentData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.notificationAttachment.Reload();
  //
  //  Non Async:
  //
  //     notificationAttachment[0].Reload().then(x => {
  //        this.notificationAttachment = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      NotificationAttachmentService.Instance.GetNotificationAttachment(this.id, includeRelations)
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
     this._notificationAttachmentChangeHistories = null;
     this._notificationAttachmentChangeHistoriesPromise = null;
     this._notificationAttachmentChangeHistoriesSubject.next(null);
     this._notificationAttachmentChangeHistoriesCount$ = null;

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
     * Gets the NotificationAttachmentChangeHistories for this NotificationAttachment.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.notificationAttachment.NotificationAttachmentChangeHistories.then(notificationAttachments => { ... })
     *   or
     *   await this.notificationAttachment.notificationAttachments
     *
    */
    public get NotificationAttachmentChangeHistories(): Promise<NotificationAttachmentChangeHistoryData[]> {
        if (this._notificationAttachmentChangeHistories !== null) {
            return Promise.resolve(this._notificationAttachmentChangeHistories);
        }

        if (this._notificationAttachmentChangeHistoriesPromise !== null) {
            return this._notificationAttachmentChangeHistoriesPromise;
        }

        // Start the load
        this.loadNotificationAttachmentChangeHistories();

        return this._notificationAttachmentChangeHistoriesPromise!;
    }



    private loadNotificationAttachmentChangeHistories(): void {

        this._notificationAttachmentChangeHistoriesPromise = lastValueFrom(
            NotificationAttachmentService.Instance.GetNotificationAttachmentChangeHistoriesForNotificationAttachment(this.id)
        )
        .then(NotificationAttachmentChangeHistories => {
            this._notificationAttachmentChangeHistories = NotificationAttachmentChangeHistories ?? [];
            this._notificationAttachmentChangeHistoriesSubject.next(this._notificationAttachmentChangeHistories);
            return this._notificationAttachmentChangeHistories;
         })
        .catch(err => {
            this._notificationAttachmentChangeHistories = [];
            this._notificationAttachmentChangeHistoriesSubject.next(this._notificationAttachmentChangeHistories);
            throw err;
        })
        .finally(() => {
            this._notificationAttachmentChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached NotificationAttachmentChangeHistory. Call after mutations to force refresh.
     */
    public ClearNotificationAttachmentChangeHistoriesCache(): void {
        this._notificationAttachmentChangeHistories = null;
        this._notificationAttachmentChangeHistoriesPromise = null;
        this._notificationAttachmentChangeHistoriesSubject.next(this._notificationAttachmentChangeHistories);      // Emit to observable
    }

    public get HasNotificationAttachmentChangeHistories(): Promise<boolean> {
        return this.NotificationAttachmentChangeHistories.then(notificationAttachmentChangeHistories => notificationAttachmentChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (notificationAttachment.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await notificationAttachment.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<NotificationAttachmentData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<NotificationAttachmentData>> {
        const info = await lastValueFrom(
            NotificationAttachmentService.Instance.GetNotificationAttachmentChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this NotificationAttachmentData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this NotificationAttachmentData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): NotificationAttachmentSubmitData {
        return NotificationAttachmentService.Instance.ConvertToNotificationAttachmentSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class NotificationAttachmentService extends SecureEndpointBase {

    private static _instance: NotificationAttachmentService;
    private listCache: Map<string, Observable<Array<NotificationAttachmentData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<NotificationAttachmentBasicListData>>>;
    private recordCache: Map<string, Observable<NotificationAttachmentData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private notificationAttachmentChangeHistoryService: NotificationAttachmentChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<NotificationAttachmentData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<NotificationAttachmentBasicListData>>>();
        this.recordCache = new Map<string, Observable<NotificationAttachmentData>>();

        NotificationAttachmentService._instance = this;
    }

    public static get Instance(): NotificationAttachmentService {
      return NotificationAttachmentService._instance;
    }


    public ClearListCaches(config: NotificationAttachmentQueryParameters | null = null) {

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


    public ConvertToNotificationAttachmentSubmitData(data: NotificationAttachmentData): NotificationAttachmentSubmitData {

        let output = new NotificationAttachmentSubmitData();

        output.id = data.id;
        output.notificationId = data.notificationId;
        output.userId = data.userId;
        output.dateTimeCreated = data.dateTimeCreated;
        output.contentFileName = data.contentFileName;
        output.contentSize = data.contentSize;
        output.contentData = data.contentData;
        output.contentMimeType = data.contentMimeType;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetNotificationAttachment(id: bigint | number, includeRelations: boolean = true) : Observable<NotificationAttachmentData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const notificationAttachment$ = this.requestNotificationAttachment(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get NotificationAttachment", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, notificationAttachment$);

            return notificationAttachment$;
        }

        return this.recordCache.get(configHash) as Observable<NotificationAttachmentData>;
    }

    private requestNotificationAttachment(id: bigint | number, includeRelations: boolean = true) : Observable<NotificationAttachmentData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<NotificationAttachmentData>(this.baseUrl + 'api/NotificationAttachment/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveNotificationAttachment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationAttachment(id, includeRelations));
            }));
    }

    public GetNotificationAttachmentList(config: NotificationAttachmentQueryParameters | any = null) : Observable<Array<NotificationAttachmentData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const notificationAttachmentList$ = this.requestNotificationAttachmentList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get NotificationAttachment list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, notificationAttachmentList$);

            return notificationAttachmentList$;
        }

        return this.listCache.get(configHash) as Observable<Array<NotificationAttachmentData>>;
    }


    private requestNotificationAttachmentList(config: NotificationAttachmentQueryParameters | any) : Observable <Array<NotificationAttachmentData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<NotificationAttachmentData>>(this.baseUrl + 'api/NotificationAttachments', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveNotificationAttachmentList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationAttachmentList(config));
            }));
    }

    public GetNotificationAttachmentsRowCount(config: NotificationAttachmentQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const notificationAttachmentsRowCount$ = this.requestNotificationAttachmentsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get NotificationAttachments row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, notificationAttachmentsRowCount$);

            return notificationAttachmentsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestNotificationAttachmentsRowCount(config: NotificationAttachmentQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/NotificationAttachments/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationAttachmentsRowCount(config));
            }));
    }

    public GetNotificationAttachmentsBasicListData(config: NotificationAttachmentQueryParameters | any = null) : Observable<Array<NotificationAttachmentBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const notificationAttachmentsBasicListData$ = this.requestNotificationAttachmentsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get NotificationAttachments basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, notificationAttachmentsBasicListData$);

            return notificationAttachmentsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<NotificationAttachmentBasicListData>>;
    }


    private requestNotificationAttachmentsBasicListData(config: NotificationAttachmentQueryParameters | any) : Observable<Array<NotificationAttachmentBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<NotificationAttachmentBasicListData>>(this.baseUrl + 'api/NotificationAttachments/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationAttachmentsBasicListData(config));
            }));

    }


    public PutNotificationAttachment(id: bigint | number, notificationAttachment: NotificationAttachmentSubmitData) : Observable<NotificationAttachmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<NotificationAttachmentData>(this.baseUrl + 'api/NotificationAttachment/' + id.toString(), notificationAttachment, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotificationAttachment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutNotificationAttachment(id, notificationAttachment));
            }));
    }


    public PostNotificationAttachment(notificationAttachment: NotificationAttachmentSubmitData) : Observable<NotificationAttachmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<NotificationAttachmentData>(this.baseUrl + 'api/NotificationAttachment', notificationAttachment, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotificationAttachment(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostNotificationAttachment(notificationAttachment));
            }));
    }

  
    public DeleteNotificationAttachment(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/NotificationAttachment/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteNotificationAttachment(id));
            }));
    }

    public RollbackNotificationAttachment(id: bigint | number, versionNumber: bigint | number) : Observable<NotificationAttachmentData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<NotificationAttachmentData>(this.baseUrl + 'api/NotificationAttachment/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotificationAttachment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackNotificationAttachment(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a NotificationAttachment.
     */
    public GetNotificationAttachmentChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<NotificationAttachmentData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<NotificationAttachmentData>>(this.baseUrl + 'api/NotificationAttachment/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetNotificationAttachmentChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a NotificationAttachment.
     */
    public GetNotificationAttachmentAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<NotificationAttachmentData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<NotificationAttachmentData>[]>(this.baseUrl + 'api/NotificationAttachment/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetNotificationAttachmentAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a NotificationAttachment.
     */
    public GetNotificationAttachmentVersion(id: bigint | number, version: number): Observable<NotificationAttachmentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<NotificationAttachmentData>(this.baseUrl + 'api/NotificationAttachment/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveNotificationAttachment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetNotificationAttachmentVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a NotificationAttachment at a specific point in time.
     */
    public GetNotificationAttachmentStateAtTime(id: bigint | number, time: string): Observable<NotificationAttachmentData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<NotificationAttachmentData>(this.baseUrl + 'api/NotificationAttachment/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveNotificationAttachment(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetNotificationAttachmentStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: NotificationAttachmentQueryParameters | any): string {

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

    public userIsSchedulerNotificationAttachmentReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerNotificationAttachmentReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.NotificationAttachments
        //
        if (userIsSchedulerNotificationAttachmentReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerNotificationAttachmentReader = user.readPermission >= 50;
            } else {
                userIsSchedulerNotificationAttachmentReader = false;
            }
        }

        return userIsSchedulerNotificationAttachmentReader;
    }


    public userIsSchedulerNotificationAttachmentWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerNotificationAttachmentWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.NotificationAttachments
        //
        if (userIsSchedulerNotificationAttachmentWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerNotificationAttachmentWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerNotificationAttachmentWriter = false;
          }      
        }

        return userIsSchedulerNotificationAttachmentWriter;
    }

    public GetNotificationAttachmentChangeHistoriesForNotificationAttachment(notificationAttachmentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<NotificationAttachmentChangeHistoryData[]> {
        return this.notificationAttachmentChangeHistoryService.GetNotificationAttachmentChangeHistoryList({
            notificationAttachmentId: notificationAttachmentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full NotificationAttachmentData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the NotificationAttachmentData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when NotificationAttachmentTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveNotificationAttachment(raw: any): NotificationAttachmentData {
    if (!raw) return raw;

    //
    // Create a NotificationAttachmentData object instance with correct prototype
    //
    const revived = Object.create(NotificationAttachmentData.prototype) as NotificationAttachmentData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._notificationAttachmentChangeHistories = null;
    (revived as any)._notificationAttachmentChangeHistoriesPromise = null;
    (revived as any)._notificationAttachmentChangeHistoriesSubject = new BehaviorSubject<NotificationAttachmentChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadNotificationAttachmentXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).NotificationAttachmentChangeHistories$ = (revived as any)._notificationAttachmentChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._notificationAttachmentChangeHistories === null && (revived as any)._notificationAttachmentChangeHistoriesPromise === null) {
                (revived as any).loadNotificationAttachmentChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._notificationAttachmentChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<NotificationAttachmentData> | null>(null);

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

  private ReviveNotificationAttachmentList(rawList: any[]): NotificationAttachmentData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveNotificationAttachment(raw));
  }

}
