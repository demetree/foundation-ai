/*

   GENERATED SERVICE FOR THE NOTIFICATION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Notification table.

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
import { NotificationTypeData } from './notification-type.service';
import { NotificationChangeHistoryService, NotificationChangeHistoryData } from './notification-change-history.service';
import { NotificationAttachmentService, NotificationAttachmentData } from './notification-attachment.service';
import { NotificationDistributionService, NotificationDistributionData } from './notification-distribution.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class NotificationQueryParameters {
    notificationTypeId: bigint | number | null | undefined = null;
    createdByUserId: bigint | number | null | undefined = null;
    message: string | null | undefined = null;
    priority: bigint | number | null | undefined = null;
    entity: string | null | undefined = null;
    entityId: bigint | number | null | undefined = null;
    externalURL: string | null | undefined = null;
    dateTimeCreated: string | null | undefined = null;        // ISO 8601 (full datetime)
    dateTimeDistributed: string | null | undefined = null;        // ISO 8601 (full datetime)
    distributionCompleted: boolean | null | undefined = null;
    userId: bigint | number | null | undefined = null;
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
export class NotificationSubmitData {
    id!: bigint | number;
    notificationTypeId: bigint | number | null = null;
    createdByUserId: bigint | number | null = null;
    message!: string;
    priority!: bigint | number;
    entity: string | null = null;
    entityId: bigint | number | null = null;
    externalURL: string | null = null;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    dateTimeDistributed: string | null = null;     // ISO 8601 (full datetime)
    distributionCompleted!: boolean;
    userId: bigint | number | null = null;
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

export class NotificationBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. NotificationChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `notification.NotificationChildren$` — use with `| async` in templates
//        • Promise:    `notification.NotificationChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="notification.NotificationChildren$ | async"`), or
//        • Access the promise getter (`notification.NotificationChildren` or `await notification.NotificationChildren`)
//    - Simply reading `notification.NotificationChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await notification.Reload()` to refresh the entire object and clear all lazy caches.
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
export class NotificationData {
    id!: bigint | number;
    notificationTypeId!: bigint | number;
    createdByUserId!: bigint | number;
    message!: string;
    priority!: bigint | number;
    entity!: string | null;
    entityId!: bigint | number;
    externalURL!: string | null;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    dateTimeDistributed!: string | null;   // ISO 8601 (full datetime)
    distributionCompleted!: boolean;
    userId!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    notificationType: NotificationTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _notificationChangeHistories: NotificationChangeHistoryData[] | null = null;
    private _notificationChangeHistoriesPromise: Promise<NotificationChangeHistoryData[]> | null  = null;
    private _notificationChangeHistoriesSubject = new BehaviorSubject<NotificationChangeHistoryData[] | null>(null);

                
    private _notificationAttachments: NotificationAttachmentData[] | null = null;
    private _notificationAttachmentsPromise: Promise<NotificationAttachmentData[]> | null  = null;
    private _notificationAttachmentsSubject = new BehaviorSubject<NotificationAttachmentData[] | null>(null);

                
    private _notificationDistributions: NotificationDistributionData[] | null = null;
    private _notificationDistributionsPromise: Promise<NotificationDistributionData[]> | null  = null;
    private _notificationDistributionsSubject = new BehaviorSubject<NotificationDistributionData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<NotificationData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<NotificationData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<NotificationData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public NotificationChangeHistories$ = this._notificationChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._notificationChangeHistories === null && this._notificationChangeHistoriesPromise === null) {
            this.loadNotificationChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _notificationChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get NotificationChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._notificationChangeHistoriesCount$ === null) {
            this._notificationChangeHistoriesCount$ = NotificationChangeHistoryService.Instance.GetNotificationChangeHistoriesRowCount({notificationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._notificationChangeHistoriesCount$;
    }



    public NotificationAttachments$ = this._notificationAttachmentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._notificationAttachments === null && this._notificationAttachmentsPromise === null) {
            this.loadNotificationAttachments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _notificationAttachmentsCount$: Observable<bigint | number> | null = null;
    public get NotificationAttachmentsCount$(): Observable<bigint | number> {
        if (this._notificationAttachmentsCount$ === null) {
            this._notificationAttachmentsCount$ = NotificationAttachmentService.Instance.GetNotificationAttachmentsRowCount({notificationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._notificationAttachmentsCount$;
    }



    public NotificationDistributions$ = this._notificationDistributionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._notificationDistributions === null && this._notificationDistributionsPromise === null) {
            this.loadNotificationDistributions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _notificationDistributionsCount$: Observable<bigint | number> | null = null;
    public get NotificationDistributionsCount$(): Observable<bigint | number> {
        if (this._notificationDistributionsCount$ === null) {
            this._notificationDistributionsCount$ = NotificationDistributionService.Instance.GetNotificationDistributionsRowCount({notificationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._notificationDistributionsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any NotificationData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.notification.Reload();
  //
  //  Non Async:
  //
  //     notification[0].Reload().then(x => {
  //        this.notification = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      NotificationService.Instance.GetNotification(this.id, includeRelations)
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
     this._notificationChangeHistories = null;
     this._notificationChangeHistoriesPromise = null;
     this._notificationChangeHistoriesSubject.next(null);
     this._notificationChangeHistoriesCount$ = null;

     this._notificationAttachments = null;
     this._notificationAttachmentsPromise = null;
     this._notificationAttachmentsSubject.next(null);
     this._notificationAttachmentsCount$ = null;

     this._notificationDistributions = null;
     this._notificationDistributionsPromise = null;
     this._notificationDistributionsSubject.next(null);
     this._notificationDistributionsCount$ = null;

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
     * Gets the NotificationChangeHistories for this Notification.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.notification.NotificationChangeHistories.then(notifications => { ... })
     *   or
     *   await this.notification.notifications
     *
    */
    public get NotificationChangeHistories(): Promise<NotificationChangeHistoryData[]> {
        if (this._notificationChangeHistories !== null) {
            return Promise.resolve(this._notificationChangeHistories);
        }

        if (this._notificationChangeHistoriesPromise !== null) {
            return this._notificationChangeHistoriesPromise;
        }

        // Start the load
        this.loadNotificationChangeHistories();

        return this._notificationChangeHistoriesPromise!;
    }



    private loadNotificationChangeHistories(): void {

        this._notificationChangeHistoriesPromise = lastValueFrom(
            NotificationService.Instance.GetNotificationChangeHistoriesForNotification(this.id)
        )
        .then(NotificationChangeHistories => {
            this._notificationChangeHistories = NotificationChangeHistories ?? [];
            this._notificationChangeHistoriesSubject.next(this._notificationChangeHistories);
            return this._notificationChangeHistories;
         })
        .catch(err => {
            this._notificationChangeHistories = [];
            this._notificationChangeHistoriesSubject.next(this._notificationChangeHistories);
            throw err;
        })
        .finally(() => {
            this._notificationChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached NotificationChangeHistory. Call after mutations to force refresh.
     */
    public ClearNotificationChangeHistoriesCache(): void {
        this._notificationChangeHistories = null;
        this._notificationChangeHistoriesPromise = null;
        this._notificationChangeHistoriesSubject.next(this._notificationChangeHistories);      // Emit to observable
    }

    public get HasNotificationChangeHistories(): Promise<boolean> {
        return this.NotificationChangeHistories.then(notificationChangeHistories => notificationChangeHistories.length > 0);
    }


    /**
     *
     * Gets the NotificationAttachments for this Notification.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.notification.NotificationAttachments.then(notifications => { ... })
     *   or
     *   await this.notification.notifications
     *
    */
    public get NotificationAttachments(): Promise<NotificationAttachmentData[]> {
        if (this._notificationAttachments !== null) {
            return Promise.resolve(this._notificationAttachments);
        }

        if (this._notificationAttachmentsPromise !== null) {
            return this._notificationAttachmentsPromise;
        }

        // Start the load
        this.loadNotificationAttachments();

        return this._notificationAttachmentsPromise!;
    }



    private loadNotificationAttachments(): void {

        this._notificationAttachmentsPromise = lastValueFrom(
            NotificationService.Instance.GetNotificationAttachmentsForNotification(this.id)
        )
        .then(NotificationAttachments => {
            this._notificationAttachments = NotificationAttachments ?? [];
            this._notificationAttachmentsSubject.next(this._notificationAttachments);
            return this._notificationAttachments;
         })
        .catch(err => {
            this._notificationAttachments = [];
            this._notificationAttachmentsSubject.next(this._notificationAttachments);
            throw err;
        })
        .finally(() => {
            this._notificationAttachmentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached NotificationAttachment. Call after mutations to force refresh.
     */
    public ClearNotificationAttachmentsCache(): void {
        this._notificationAttachments = null;
        this._notificationAttachmentsPromise = null;
        this._notificationAttachmentsSubject.next(this._notificationAttachments);      // Emit to observable
    }

    public get HasNotificationAttachments(): Promise<boolean> {
        return this.NotificationAttachments.then(notificationAttachments => notificationAttachments.length > 0);
    }


    /**
     *
     * Gets the NotificationDistributions for this Notification.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.notification.NotificationDistributions.then(notifications => { ... })
     *   or
     *   await this.notification.notifications
     *
    */
    public get NotificationDistributions(): Promise<NotificationDistributionData[]> {
        if (this._notificationDistributions !== null) {
            return Promise.resolve(this._notificationDistributions);
        }

        if (this._notificationDistributionsPromise !== null) {
            return this._notificationDistributionsPromise;
        }

        // Start the load
        this.loadNotificationDistributions();

        return this._notificationDistributionsPromise!;
    }



    private loadNotificationDistributions(): void {

        this._notificationDistributionsPromise = lastValueFrom(
            NotificationService.Instance.GetNotificationDistributionsForNotification(this.id)
        )
        .then(NotificationDistributions => {
            this._notificationDistributions = NotificationDistributions ?? [];
            this._notificationDistributionsSubject.next(this._notificationDistributions);
            return this._notificationDistributions;
         })
        .catch(err => {
            this._notificationDistributions = [];
            this._notificationDistributionsSubject.next(this._notificationDistributions);
            throw err;
        })
        .finally(() => {
            this._notificationDistributionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached NotificationDistribution. Call after mutations to force refresh.
     */
    public ClearNotificationDistributionsCache(): void {
        this._notificationDistributions = null;
        this._notificationDistributionsPromise = null;
        this._notificationDistributionsSubject.next(this._notificationDistributions);      // Emit to observable
    }

    public get HasNotificationDistributions(): Promise<boolean> {
        return this.NotificationDistributions.then(notificationDistributions => notificationDistributions.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (notification.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await notification.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<NotificationData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<NotificationData>> {
        const info = await lastValueFrom(
            NotificationService.Instance.GetNotificationChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this NotificationData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this NotificationData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): NotificationSubmitData {
        return NotificationService.Instance.ConvertToNotificationSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class NotificationService extends SecureEndpointBase {

    private static _instance: NotificationService;
    private listCache: Map<string, Observable<Array<NotificationData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<NotificationBasicListData>>>;
    private recordCache: Map<string, Observable<NotificationData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private notificationChangeHistoryService: NotificationChangeHistoryService,
        private notificationAttachmentService: NotificationAttachmentService,
        private notificationDistributionService: NotificationDistributionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<NotificationData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<NotificationBasicListData>>>();
        this.recordCache = new Map<string, Observable<NotificationData>>();

        NotificationService._instance = this;
    }

    public static get Instance(): NotificationService {
      return NotificationService._instance;
    }


    public ClearListCaches(config: NotificationQueryParameters | null = null) {

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


    public ConvertToNotificationSubmitData(data: NotificationData): NotificationSubmitData {

        let output = new NotificationSubmitData();

        output.id = data.id;
        output.notificationTypeId = data.notificationTypeId;
        output.createdByUserId = data.createdByUserId;
        output.message = data.message;
        output.priority = data.priority;
        output.entity = data.entity;
        output.entityId = data.entityId;
        output.externalURL = data.externalURL;
        output.dateTimeCreated = data.dateTimeCreated;
        output.dateTimeDistributed = data.dateTimeDistributed;
        output.distributionCompleted = data.distributionCompleted;
        output.userId = data.userId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetNotification(id: bigint | number, includeRelations: boolean = true) : Observable<NotificationData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const notification$ = this.requestNotification(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Notification", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, notification$);

            return notification$;
        }

        return this.recordCache.get(configHash) as Observable<NotificationData>;
    }

    private requestNotification(id: bigint | number, includeRelations: boolean = true) : Observable<NotificationData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<NotificationData>(this.baseUrl + 'api/Notification/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveNotification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestNotification(id, includeRelations));
            }));
    }

    public GetNotificationList(config: NotificationQueryParameters | any = null) : Observable<Array<NotificationData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const notificationList$ = this.requestNotificationList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Notification list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, notificationList$);

            return notificationList$;
        }

        return this.listCache.get(configHash) as Observable<Array<NotificationData>>;
    }


    private requestNotificationList(config: NotificationQueryParameters | any) : Observable <Array<NotificationData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<NotificationData>>(this.baseUrl + 'api/Notifications', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveNotificationList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationList(config));
            }));
    }

    public GetNotificationsRowCount(config: NotificationQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const notificationsRowCount$ = this.requestNotificationsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Notifications row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, notificationsRowCount$);

            return notificationsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestNotificationsRowCount(config: NotificationQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Notifications/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationsRowCount(config));
            }));
    }

    public GetNotificationsBasicListData(config: NotificationQueryParameters | any = null) : Observable<Array<NotificationBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const notificationsBasicListData$ = this.requestNotificationsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Notifications basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, notificationsBasicListData$);

            return notificationsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<NotificationBasicListData>>;
    }


    private requestNotificationsBasicListData(config: NotificationQueryParameters | any) : Observable<Array<NotificationBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<NotificationBasicListData>>(this.baseUrl + 'api/Notifications/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestNotificationsBasicListData(config));
            }));

    }


    public PutNotification(id: bigint | number, notification: NotificationSubmitData) : Observable<NotificationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<NotificationData>(this.baseUrl + 'api/Notification/' + id.toString(), notification, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutNotification(id, notification));
            }));
    }


    public PostNotification(notification: NotificationSubmitData) : Observable<NotificationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<NotificationData>(this.baseUrl + 'api/Notification', notification, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotification(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostNotification(notification));
            }));
    }

  
    public DeleteNotification(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Notification/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteNotification(id));
            }));
    }

    public RollbackNotification(id: bigint | number, versionNumber: bigint | number) : Observable<NotificationData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<NotificationData>(this.baseUrl + 'api/Notification/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveNotification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackNotification(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Notification.
     */
    public GetNotificationChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<NotificationData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<NotificationData>>(this.baseUrl + 'api/Notification/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetNotificationChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Notification.
     */
    public GetNotificationAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<NotificationData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<NotificationData>[]>(this.baseUrl + 'api/Notification/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetNotificationAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Notification.
     */
    public GetNotificationVersion(id: bigint | number, version: number): Observable<NotificationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<NotificationData>(this.baseUrl + 'api/Notification/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveNotification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetNotificationVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Notification at a specific point in time.
     */
    public GetNotificationStateAtTime(id: bigint | number, time: string): Observable<NotificationData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<NotificationData>(this.baseUrl + 'api/Notification/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveNotification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetNotificationStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: NotificationQueryParameters | any): string {

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

    public userIsSchedulerNotificationReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerNotificationReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Notifications
        //
        if (userIsSchedulerNotificationReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerNotificationReader = user.readPermission >= 50;
            } else {
                userIsSchedulerNotificationReader = false;
            }
        }

        return userIsSchedulerNotificationReader;
    }


    public userIsSchedulerNotificationWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerNotificationWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Notifications
        //
        if (userIsSchedulerNotificationWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerNotificationWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerNotificationWriter = false;
          }      
        }

        return userIsSchedulerNotificationWriter;
    }

    public GetNotificationChangeHistoriesForNotification(notificationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<NotificationChangeHistoryData[]> {
        return this.notificationChangeHistoryService.GetNotificationChangeHistoryList({
            notificationId: notificationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetNotificationAttachmentsForNotification(notificationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<NotificationAttachmentData[]> {
        return this.notificationAttachmentService.GetNotificationAttachmentList({
            notificationId: notificationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetNotificationDistributionsForNotification(notificationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<NotificationDistributionData[]> {
        return this.notificationDistributionService.GetNotificationDistributionList({
            notificationId: notificationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full NotificationData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the NotificationData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when NotificationTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveNotification(raw: any): NotificationData {
    if (!raw) return raw;

    //
    // Create a NotificationData object instance with correct prototype
    //
    const revived = Object.create(NotificationData.prototype) as NotificationData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._notificationChangeHistories = null;
    (revived as any)._notificationChangeHistoriesPromise = null;
    (revived as any)._notificationChangeHistoriesSubject = new BehaviorSubject<NotificationChangeHistoryData[] | null>(null);

    (revived as any)._notificationAttachments = null;
    (revived as any)._notificationAttachmentsPromise = null;
    (revived as any)._notificationAttachmentsSubject = new BehaviorSubject<NotificationAttachmentData[] | null>(null);

    (revived as any)._notificationDistributions = null;
    (revived as any)._notificationDistributionsPromise = null;
    (revived as any)._notificationDistributionsSubject = new BehaviorSubject<NotificationDistributionData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadNotificationXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).NotificationChangeHistories$ = (revived as any)._notificationChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._notificationChangeHistories === null && (revived as any)._notificationChangeHistoriesPromise === null) {
                (revived as any).loadNotificationChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._notificationChangeHistoriesCount$ = null;


    (revived as any).NotificationAttachments$ = (revived as any)._notificationAttachmentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._notificationAttachments === null && (revived as any)._notificationAttachmentsPromise === null) {
                (revived as any).loadNotificationAttachments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._notificationAttachmentsCount$ = null;


    (revived as any).NotificationDistributions$ = (revived as any)._notificationDistributionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._notificationDistributions === null && (revived as any)._notificationDistributionsPromise === null) {
                (revived as any).loadNotificationDistributions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._notificationDistributionsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<NotificationData> | null>(null);

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

  private ReviveNotificationList(rawList: any[]): NotificationData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveNotification(raw));
  }

}
