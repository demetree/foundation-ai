/*

   GENERATED SERVICE FOR THE NOTIFICATIONSUBSCRIPTION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the NotificationSubscription table.

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
import { ResourceData } from './resource.service';
import { ContactData } from './contact.service';
import { NotificationTypeData } from './notification-type.service';
import { NotificationSubscriptionChangeHistoryService, NotificationSubscriptionChangeHistoryData } from './notification-subscription-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class NotificationSubscriptionQueryParameters {
    resourceId: bigint | number | null | undefined = null;
    contactId: bigint | number | null | undefined = null;
    notificationTypeId: bigint | number | null | undefined = null;
    triggerEvents: bigint | number | null | undefined = null;
    recipientAddress: string | null | undefined = null;
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
export class NotificationSubscriptionSubmitData {
    id!: bigint | number;
    resourceId: bigint | number | null = null;
    contactId: bigint | number | null = null;
    notificationTypeId!: bigint | number;
    triggerEvents!: bigint | number;
    recipientAddress!: string;
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

export class NotificationSubscriptionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. NotificationSubscriptionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `notificationSubscription.NotificationSubscriptionChildren$` — use with `| async` in templates
//        • Promise:    `notificationSubscription.NotificationSubscriptionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="notificationSubscription.NotificationSubscriptionChildren$ | async"`), or
//        • Access the promise getter (`notificationSubscription.NotificationSubscriptionChildren` or `await notificationSubscription.NotificationSubscriptionChildren`)
//    - Simply reading `notificationSubscription.NotificationSubscriptionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await notificationSubscription.Reload()` to refresh the entire object and clear all lazy caches.
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
export class NotificationSubscriptionData {
    id!: bigint | number;
    resourceId!: bigint | number;
    contactId!: bigint | number;
    notificationTypeId!: bigint | number;
    triggerEvents!: bigint | number;
    recipientAddress!: string;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    notificationType: NotificationTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _notificationSubscriptionChangeHistories: NotificationSubscriptionChangeHistoryData[] | null = null;
    private _notificationSubscriptionChangeHistoriesPromise: Promise<NotificationSubscriptionChangeHistoryData[]> | null  = null;
    private _notificationSubscriptionChangeHistoriesSubject = new BehaviorSubject<NotificationSubscriptionChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<NotificationSubscriptionData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<NotificationSubscriptionData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<NotificationSubscriptionData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public NotificationSubscriptionChangeHistories$ = this._notificationSubscriptionChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._notificationSubscriptionChangeHistories === null && this._notificationSubscriptionChangeHistoriesPromise === null) {
            this.loadNotificationSubscriptionChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _notificationSubscriptionChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get NotificationSubscriptionChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._notificationSubscriptionChangeHistoriesCount$ === null) {
            this._notificationSubscriptionChangeHistoriesCount$ = NotificationSubscriptionChangeHistoryService.Instance.GetNotificationSubscriptionChangeHistoriesRowCount({notificationSubscriptionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._notificationSubscriptionChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any NotificationSubscriptionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.notificationSubscription.Reload();
  //
  //  Non Async:
  //
  //     notificationSubscription[0].Reload().then(x => {
  //        this.notificationSubscription = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      NotificationSubscriptionService.Instance.GetNotificationSubscription(this.id, includeRelations)
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
     this._notificationSubscriptionChangeHistories = null;
     this._notificationSubscriptionChangeHistoriesPromise = null;
     this._notificationSubscriptionChangeHistoriesSubject.next(null);
     this._notificationSubscriptionChangeHistoriesCount$ = null;

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
     * Gets the NotificationSubscriptionChangeHistories for this NotificationSubscription.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.notificationSubscription.NotificationSubscriptionChangeHistories.then(notificationSubscriptions => { ... })
     *   or
     *   await this.notificationSubscription.notificationSubscriptions
     *
    */
    public get NotificationSubscriptionChangeHistories(): Promise<NotificationSubscriptionChangeHistoryData[]> {
        if (this._notificationSubscriptionChangeHistories !== null) {
            return Promise.resolve(this._notificationSubscriptionChangeHistories);
        }

        if (this._notificationSubscriptionChangeHistoriesPromise !== null) {
            return this._notificationSubscriptionChangeHistoriesPromise;
        }

        // Start the load
        this.loadNotificationSubscriptionChangeHistories();

        return this._notificationSubscriptionChangeHistoriesPromise!;
    }



    private loadNotificationSubscriptionChangeHistories(): void {

        this._notificationSubscriptionChangeHistoriesPromise = lastValueFrom(
            NotificationSubscriptionService.Instance.GetNotificationSubscriptionChangeHistoriesForNotificationSubscription(this.id)
        )
        .then(NotificationSubscriptionChangeHistories => {
            this._notificationSubscriptionChangeHistories = NotificationSubscriptionChangeHistories ?? [];
            this._notificationSubscriptionChangeHistoriesSubject.next(this._notificationSubscriptionChangeHistories);
            return this._notificationSubscriptionChangeHistories;
         })
        .catch(err => {
            this._notificationSubscriptionChangeHistories = [];
            this._notificationSubscriptionChangeHistoriesSubject.next(this._notificationSubscriptionChangeHistories);
            throw err;
        })
        .finally(() => {
            this._notificationSubscriptionChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached NotificationSubscriptionChangeHistory. Call after mutations to force refresh.
     */
    public ClearNotificationSubscriptionChangeHistoriesCache(): void {
        this._notificationSubscriptionChangeHistories = null;
        this._notificationSubscriptionChangeHistoriesPromise = null;
        this._notificationSubscriptionChangeHistoriesSubject.next(this._notificationSubscriptionChangeHistories);      // Emit to observable
    }

    public get HasNotificationSubscriptionChangeHistories(): Promise<boolean> {
        return this.NotificationSubscriptionChangeHistories.then(notificationSubscriptionChangeHistories => notificationSubscriptionChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (notificationSubscription.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await notificationSubscription.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<NotificationSubscriptionData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<NotificationSubscriptionData>> {
        const info = await lastValueFrom(
            NotificationSubscriptionService.Instance.GetNotificationSubscriptionChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this NotificationSubscriptionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this NotificationSubscriptionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): NotificationSubscriptionSubmitData {
        return NotificationSubscriptionService.Instance.ConvertToNotificationSubscriptionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class NotificationSubscriptionService extends SecureEndpointBase {

    private static _instance: NotificationSubscriptionService;
    private listCache: Map<string, Observable<Array<NotificationSubscriptionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<NotificationSubscriptionBasicListData>>>;
    private recordCache: Map<string, Observable<NotificationSubscriptionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private notificationSubscriptionChangeHistoryService: NotificationSubscriptionChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<NotificationSubscriptionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<NotificationSubscriptionBasicListData>>>();
        this.recordCache = new Map<string, Observable<NotificationSubscriptionData>>();

        NotificationSubscriptionService._instance = this;
    }

    public static get Instance(): NotificationSubscriptionService {
      return NotificationSubscriptionService._instance;
    }


    public ClearListCaches(config: NotificationSubscriptionQueryParameters | null = null) {

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


    public ConvertToNotificationSubscriptionSubmitData(data: NotificationSubscriptionData): NotificationSubscriptionSubmitData {

        let output = new NotificationSubscriptionSubmitData();

        output.id = data.id;
        output.resourceId = data.resourceId;
        output.contactId = data.contactId;
        output.notificationTypeId = data.notificationTypeId;
        output.triggerEvents = data.triggerEvents;
        output.recipientAddress = data.recipientAddress;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetNotificationSubscription(id: bigint | number, includeRelations: boolean = true) : Observable<NotificationSubscriptionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const notificationSubscription$ = this.requestNotificationSubscription(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get NotificationSubscription", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, notificationSubscription$);

            return notificationSubscription$;
        }

        return this.recordCache.get(configHash) as Observable<NotificationSubscriptionData>;
    }

    private requestNotificationSubscription(id: bigint | number, includeRelations: boolean = true) : Observable<NotificationSubscriptionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<NotificationSubscriptionData>(this.baseUrl + 'api/NotificationSubscription/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveNotificationSubscription(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationSubscription(id, includeRelations));
            }));
    }

    public GetNotificationSubscriptionList(config: NotificationSubscriptionQueryParameters | any = null) : Observable<Array<NotificationSubscriptionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const notificationSubscriptionList$ = this.requestNotificationSubscriptionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get NotificationSubscription list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, notificationSubscriptionList$);

            return notificationSubscriptionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<NotificationSubscriptionData>>;
    }


    private requestNotificationSubscriptionList(config: NotificationSubscriptionQueryParameters | any) : Observable <Array<NotificationSubscriptionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<NotificationSubscriptionData>>(this.baseUrl + 'api/NotificationSubscriptions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveNotificationSubscriptionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationSubscriptionList(config));
            }));
    }

    public GetNotificationSubscriptionsRowCount(config: NotificationSubscriptionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const notificationSubscriptionsRowCount$ = this.requestNotificationSubscriptionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get NotificationSubscriptions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, notificationSubscriptionsRowCount$);

            return notificationSubscriptionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestNotificationSubscriptionsRowCount(config: NotificationSubscriptionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/NotificationSubscriptions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationSubscriptionsRowCount(config));
            }));
    }

    public GetNotificationSubscriptionsBasicListData(config: NotificationSubscriptionQueryParameters | any = null) : Observable<Array<NotificationSubscriptionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const notificationSubscriptionsBasicListData$ = this.requestNotificationSubscriptionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get NotificationSubscriptions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, notificationSubscriptionsBasicListData$);

            return notificationSubscriptionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<NotificationSubscriptionBasicListData>>;
    }


    private requestNotificationSubscriptionsBasicListData(config: NotificationSubscriptionQueryParameters | any) : Observable<Array<NotificationSubscriptionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<NotificationSubscriptionBasicListData>>(this.baseUrl + 'api/NotificationSubscriptions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationSubscriptionsBasicListData(config));
            }));

    }


    public PutNotificationSubscription(id: bigint | number, notificationSubscription: NotificationSubscriptionSubmitData) : Observable<NotificationSubscriptionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<NotificationSubscriptionData>(this.baseUrl + 'api/NotificationSubscription/' + id.toString(), notificationSubscription, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotificationSubscription(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutNotificationSubscription(id, notificationSubscription));
            }));
    }


    public PostNotificationSubscription(notificationSubscription: NotificationSubscriptionSubmitData) : Observable<NotificationSubscriptionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<NotificationSubscriptionData>(this.baseUrl + 'api/NotificationSubscription', notificationSubscription, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotificationSubscription(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostNotificationSubscription(notificationSubscription));
            }));
    }

  
    public DeleteNotificationSubscription(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/NotificationSubscription/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteNotificationSubscription(id));
            }));
    }

    public RollbackNotificationSubscription(id: bigint | number, versionNumber: bigint | number) : Observable<NotificationSubscriptionData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<NotificationSubscriptionData>(this.baseUrl + 'api/NotificationSubscription/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotificationSubscription(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackNotificationSubscription(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a NotificationSubscription.
     */
    public GetNotificationSubscriptionChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<NotificationSubscriptionData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<NotificationSubscriptionData>>(this.baseUrl + 'api/NotificationSubscription/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetNotificationSubscriptionChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a NotificationSubscription.
     */
    public GetNotificationSubscriptionAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<NotificationSubscriptionData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<NotificationSubscriptionData>[]>(this.baseUrl + 'api/NotificationSubscription/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetNotificationSubscriptionAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a NotificationSubscription.
     */
    public GetNotificationSubscriptionVersion(id: bigint | number, version: number): Observable<NotificationSubscriptionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<NotificationSubscriptionData>(this.baseUrl + 'api/NotificationSubscription/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveNotificationSubscription(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetNotificationSubscriptionVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a NotificationSubscription at a specific point in time.
     */
    public GetNotificationSubscriptionStateAtTime(id: bigint | number, time: string): Observable<NotificationSubscriptionData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<NotificationSubscriptionData>(this.baseUrl + 'api/NotificationSubscription/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveNotificationSubscription(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetNotificationSubscriptionStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: NotificationSubscriptionQueryParameters | any): string {

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

    public userIsSchedulerNotificationSubscriptionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerNotificationSubscriptionReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.NotificationSubscriptions
        //
        if (userIsSchedulerNotificationSubscriptionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerNotificationSubscriptionReader = user.readPermission >= 1;
            } else {
                userIsSchedulerNotificationSubscriptionReader = false;
            }
        }

        return userIsSchedulerNotificationSubscriptionReader;
    }


    public userIsSchedulerNotificationSubscriptionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerNotificationSubscriptionWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.NotificationSubscriptions
        //
        if (userIsSchedulerNotificationSubscriptionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerNotificationSubscriptionWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerNotificationSubscriptionWriter = false;
          }      
        }

        return userIsSchedulerNotificationSubscriptionWriter;
    }

    public GetNotificationSubscriptionChangeHistoriesForNotificationSubscription(notificationSubscriptionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<NotificationSubscriptionChangeHistoryData[]> {
        return this.notificationSubscriptionChangeHistoryService.GetNotificationSubscriptionChangeHistoryList({
            notificationSubscriptionId: notificationSubscriptionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full NotificationSubscriptionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the NotificationSubscriptionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when NotificationSubscriptionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveNotificationSubscription(raw: any): NotificationSubscriptionData {
    if (!raw) return raw;

    //
    // Create a NotificationSubscriptionData object instance with correct prototype
    //
    const revived = Object.create(NotificationSubscriptionData.prototype) as NotificationSubscriptionData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._notificationSubscriptionChangeHistories = null;
    (revived as any)._notificationSubscriptionChangeHistoriesPromise = null;
    (revived as any)._notificationSubscriptionChangeHistoriesSubject = new BehaviorSubject<NotificationSubscriptionChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadNotificationSubscriptionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).NotificationSubscriptionChangeHistories$ = (revived as any)._notificationSubscriptionChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._notificationSubscriptionChangeHistories === null && (revived as any)._notificationSubscriptionChangeHistoriesPromise === null) {
                (revived as any).loadNotificationSubscriptionChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._notificationSubscriptionChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<NotificationSubscriptionData> | null>(null);

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

  private ReviveNotificationSubscriptionList(rawList: any[]): NotificationSubscriptionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveNotificationSubscription(raw));
  }

}
