/*

   GENERATED SERVICE FOR THE SALESFORCETENANTLINK TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SalesforceTenantLink table.

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
import { SalesforceTenantLinkChangeHistoryService, SalesforceTenantLinkChangeHistoryData } from './salesforce-tenant-link-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SalesforceTenantLinkQueryParameters {
    syncEnabled: boolean | null | undefined = null;
    syncDirectionFlags: string | null | undefined = null;
    pullIntervalMinutes: bigint | number | null | undefined = null;
    lastPullDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    loginUrl: string | null | undefined = null;
    sfClientId: string | null | undefined = null;
    sfClientSecret: string | null | undefined = null;
    sfUsername: string | null | undefined = null;
    sfPassword: string | null | undefined = null;
    sfSecurityToken: string | null | undefined = null;
    instanceUrl: string | null | undefined = null;
    apiVersion: string | null | undefined = null;
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
export class SalesforceTenantLinkSubmitData {
    id!: bigint | number;
    syncEnabled!: boolean;
    syncDirectionFlags: string | null = null;
    pullIntervalMinutes: bigint | number | null = null;
    lastPullDate: string | null = null;     // ISO 8601 (full datetime)
    loginUrl: string | null = null;
    sfClientId: string | null = null;
    sfClientSecret: string | null = null;
    sfUsername: string | null = null;
    sfPassword: string | null = null;
    sfSecurityToken: string | null = null;
    instanceUrl: string | null = null;
    apiVersion: string | null = null;
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

export class SalesforceTenantLinkBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SalesforceTenantLinkChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `salesforceTenantLink.SalesforceTenantLinkChildren$` — use with `| async` in templates
//        • Promise:    `salesforceTenantLink.SalesforceTenantLinkChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="salesforceTenantLink.SalesforceTenantLinkChildren$ | async"`), or
//        • Access the promise getter (`salesforceTenantLink.SalesforceTenantLinkChildren` or `await salesforceTenantLink.SalesforceTenantLinkChildren`)
//    - Simply reading `salesforceTenantLink.SalesforceTenantLinkChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await salesforceTenantLink.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SalesforceTenantLinkData {
    id!: bigint | number;
    syncEnabled!: boolean;
    syncDirectionFlags!: string | null;
    pullIntervalMinutes!: bigint | number;
    lastPullDate!: string | null;   // ISO 8601 (full datetime)
    loginUrl!: string | null;
    sfClientId!: string | null;
    sfClientSecret!: string | null;
    sfUsername!: string | null;
    sfPassword!: string | null;
    sfSecurityToken!: string | null;
    instanceUrl!: string | null;
    apiVersion!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _salesforceTenantLinkChangeHistories: SalesforceTenantLinkChangeHistoryData[] | null = null;
    private _salesforceTenantLinkChangeHistoriesPromise: Promise<SalesforceTenantLinkChangeHistoryData[]> | null  = null;
    private _salesforceTenantLinkChangeHistoriesSubject = new BehaviorSubject<SalesforceTenantLinkChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<SalesforceTenantLinkData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<SalesforceTenantLinkData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SalesforceTenantLinkData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SalesforceTenantLinkChangeHistories$ = this._salesforceTenantLinkChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._salesforceTenantLinkChangeHistories === null && this._salesforceTenantLinkChangeHistoriesPromise === null) {
            this.loadSalesforceTenantLinkChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _salesforceTenantLinkChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get SalesforceTenantLinkChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._salesforceTenantLinkChangeHistoriesCount$ === null) {
            this._salesforceTenantLinkChangeHistoriesCount$ = SalesforceTenantLinkChangeHistoryService.Instance.GetSalesforceTenantLinkChangeHistoriesRowCount({salesforceTenantLinkId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._salesforceTenantLinkChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SalesforceTenantLinkData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.salesforceTenantLink.Reload();
  //
  //  Non Async:
  //
  //     salesforceTenantLink[0].Reload().then(x => {
  //        this.salesforceTenantLink = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SalesforceTenantLinkService.Instance.GetSalesforceTenantLink(this.id, includeRelations)
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
     this._salesforceTenantLinkChangeHistories = null;
     this._salesforceTenantLinkChangeHistoriesPromise = null;
     this._salesforceTenantLinkChangeHistoriesSubject.next(null);
     this._salesforceTenantLinkChangeHistoriesCount$ = null;

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
     * Gets the SalesforceTenantLinkChangeHistories for this SalesforceTenantLink.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.salesforceTenantLink.SalesforceTenantLinkChangeHistories.then(salesforceTenantLinks => { ... })
     *   or
     *   await this.salesforceTenantLink.salesforceTenantLinks
     *
    */
    public get SalesforceTenantLinkChangeHistories(): Promise<SalesforceTenantLinkChangeHistoryData[]> {
        if (this._salesforceTenantLinkChangeHistories !== null) {
            return Promise.resolve(this._salesforceTenantLinkChangeHistories);
        }

        if (this._salesforceTenantLinkChangeHistoriesPromise !== null) {
            return this._salesforceTenantLinkChangeHistoriesPromise;
        }

        // Start the load
        this.loadSalesforceTenantLinkChangeHistories();

        return this._salesforceTenantLinkChangeHistoriesPromise!;
    }



    private loadSalesforceTenantLinkChangeHistories(): void {

        this._salesforceTenantLinkChangeHistoriesPromise = lastValueFrom(
            SalesforceTenantLinkService.Instance.GetSalesforceTenantLinkChangeHistoriesForSalesforceTenantLink(this.id)
        )
        .then(SalesforceTenantLinkChangeHistories => {
            this._salesforceTenantLinkChangeHistories = SalesforceTenantLinkChangeHistories ?? [];
            this._salesforceTenantLinkChangeHistoriesSubject.next(this._salesforceTenantLinkChangeHistories);
            return this._salesforceTenantLinkChangeHistories;
         })
        .catch(err => {
            this._salesforceTenantLinkChangeHistories = [];
            this._salesforceTenantLinkChangeHistoriesSubject.next(this._salesforceTenantLinkChangeHistories);
            throw err;
        })
        .finally(() => {
            this._salesforceTenantLinkChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SalesforceTenantLinkChangeHistory. Call after mutations to force refresh.
     */
    public ClearSalesforceTenantLinkChangeHistoriesCache(): void {
        this._salesforceTenantLinkChangeHistories = null;
        this._salesforceTenantLinkChangeHistoriesPromise = null;
        this._salesforceTenantLinkChangeHistoriesSubject.next(this._salesforceTenantLinkChangeHistories);      // Emit to observable
    }

    public get HasSalesforceTenantLinkChangeHistories(): Promise<boolean> {
        return this.SalesforceTenantLinkChangeHistories.then(salesforceTenantLinkChangeHistories => salesforceTenantLinkChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (salesforceTenantLink.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await salesforceTenantLink.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<SalesforceTenantLinkData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<SalesforceTenantLinkData>> {
        const info = await lastValueFrom(
            SalesforceTenantLinkService.Instance.GetSalesforceTenantLinkChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this SalesforceTenantLinkData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SalesforceTenantLinkData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SalesforceTenantLinkSubmitData {
        return SalesforceTenantLinkService.Instance.ConvertToSalesforceTenantLinkSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SalesforceTenantLinkService extends SecureEndpointBase {

    private static _instance: SalesforceTenantLinkService;
    private listCache: Map<string, Observable<Array<SalesforceTenantLinkData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SalesforceTenantLinkBasicListData>>>;
    private recordCache: Map<string, Observable<SalesforceTenantLinkData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private salesforceTenantLinkChangeHistoryService: SalesforceTenantLinkChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SalesforceTenantLinkData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SalesforceTenantLinkBasicListData>>>();
        this.recordCache = new Map<string, Observable<SalesforceTenantLinkData>>();

        SalesforceTenantLinkService._instance = this;
    }

    public static get Instance(): SalesforceTenantLinkService {
      return SalesforceTenantLinkService._instance;
    }


    public ClearListCaches(config: SalesforceTenantLinkQueryParameters | null = null) {

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


    public ConvertToSalesforceTenantLinkSubmitData(data: SalesforceTenantLinkData): SalesforceTenantLinkSubmitData {

        let output = new SalesforceTenantLinkSubmitData();

        output.id = data.id;
        output.syncEnabled = data.syncEnabled;
        output.syncDirectionFlags = data.syncDirectionFlags;
        output.pullIntervalMinutes = data.pullIntervalMinutes;
        output.lastPullDate = data.lastPullDate;
        output.loginUrl = data.loginUrl;
        output.sfClientId = data.sfClientId;
        output.sfClientSecret = data.sfClientSecret;
        output.sfUsername = data.sfUsername;
        output.sfPassword = data.sfPassword;
        output.sfSecurityToken = data.sfSecurityToken;
        output.instanceUrl = data.instanceUrl;
        output.apiVersion = data.apiVersion;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSalesforceTenantLink(id: bigint | number, includeRelations: boolean = true) : Observable<SalesforceTenantLinkData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const salesforceTenantLink$ = this.requestSalesforceTenantLink(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SalesforceTenantLink", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, salesforceTenantLink$);

            return salesforceTenantLink$;
        }

        return this.recordCache.get(configHash) as Observable<SalesforceTenantLinkData>;
    }

    private requestSalesforceTenantLink(id: bigint | number, includeRelations: boolean = true) : Observable<SalesforceTenantLinkData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SalesforceTenantLinkData>(this.baseUrl + 'api/SalesforceTenantLink/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSalesforceTenantLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSalesforceTenantLink(id, includeRelations));
            }));
    }

    public GetSalesforceTenantLinkList(config: SalesforceTenantLinkQueryParameters | any = null) : Observable<Array<SalesforceTenantLinkData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const salesforceTenantLinkList$ = this.requestSalesforceTenantLinkList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SalesforceTenantLink list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, salesforceTenantLinkList$);

            return salesforceTenantLinkList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SalesforceTenantLinkData>>;
    }


    private requestSalesforceTenantLinkList(config: SalesforceTenantLinkQueryParameters | any) : Observable <Array<SalesforceTenantLinkData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SalesforceTenantLinkData>>(this.baseUrl + 'api/SalesforceTenantLinks', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSalesforceTenantLinkList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSalesforceTenantLinkList(config));
            }));
    }

    public GetSalesforceTenantLinksRowCount(config: SalesforceTenantLinkQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const salesforceTenantLinksRowCount$ = this.requestSalesforceTenantLinksRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SalesforceTenantLinks row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, salesforceTenantLinksRowCount$);

            return salesforceTenantLinksRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSalesforceTenantLinksRowCount(config: SalesforceTenantLinkQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SalesforceTenantLinks/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSalesforceTenantLinksRowCount(config));
            }));
    }

    public GetSalesforceTenantLinksBasicListData(config: SalesforceTenantLinkQueryParameters | any = null) : Observable<Array<SalesforceTenantLinkBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const salesforceTenantLinksBasicListData$ = this.requestSalesforceTenantLinksBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SalesforceTenantLinks basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, salesforceTenantLinksBasicListData$);

            return salesforceTenantLinksBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SalesforceTenantLinkBasicListData>>;
    }


    private requestSalesforceTenantLinksBasicListData(config: SalesforceTenantLinkQueryParameters | any) : Observable<Array<SalesforceTenantLinkBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SalesforceTenantLinkBasicListData>>(this.baseUrl + 'api/SalesforceTenantLinks/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSalesforceTenantLinksBasicListData(config));
            }));

    }


    public PutSalesforceTenantLink(id: bigint | number, salesforceTenantLink: SalesforceTenantLinkSubmitData) : Observable<SalesforceTenantLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SalesforceTenantLinkData>(this.baseUrl + 'api/SalesforceTenantLink/' + id.toString(), salesforceTenantLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSalesforceTenantLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSalesforceTenantLink(id, salesforceTenantLink));
            }));
    }


    public PostSalesforceTenantLink(salesforceTenantLink: SalesforceTenantLinkSubmitData) : Observable<SalesforceTenantLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SalesforceTenantLinkData>(this.baseUrl + 'api/SalesforceTenantLink', salesforceTenantLink, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSalesforceTenantLink(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSalesforceTenantLink(salesforceTenantLink));
            }));
    }

  
    public DeleteSalesforceTenantLink(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SalesforceTenantLink/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSalesforceTenantLink(id));
            }));
    }

    public RollbackSalesforceTenantLink(id: bigint | number, versionNumber: bigint | number) : Observable<SalesforceTenantLinkData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SalesforceTenantLinkData>(this.baseUrl + 'api/SalesforceTenantLink/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSalesforceTenantLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackSalesforceTenantLink(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a SalesforceTenantLink.
     */
    public GetSalesforceTenantLinkChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<SalesforceTenantLinkData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SalesforceTenantLinkData>>(this.baseUrl + 'api/SalesforceTenantLink/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSalesforceTenantLinkChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a SalesforceTenantLink.
     */
    public GetSalesforceTenantLinkAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<SalesforceTenantLinkData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SalesforceTenantLinkData>[]>(this.baseUrl + 'api/SalesforceTenantLink/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSalesforceTenantLinkAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a SalesforceTenantLink.
     */
    public GetSalesforceTenantLinkVersion(id: bigint | number, version: number): Observable<SalesforceTenantLinkData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SalesforceTenantLinkData>(this.baseUrl + 'api/SalesforceTenantLink/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSalesforceTenantLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSalesforceTenantLinkVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a SalesforceTenantLink at a specific point in time.
     */
    public GetSalesforceTenantLinkStateAtTime(id: bigint | number, time: string): Observable<SalesforceTenantLinkData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SalesforceTenantLinkData>(this.baseUrl + 'api/SalesforceTenantLink/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSalesforceTenantLink(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSalesforceTenantLinkStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: SalesforceTenantLinkQueryParameters | any): string {

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

    public userIsSchedulerSalesforceTenantLinkReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerSalesforceTenantLinkReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.SalesforceTenantLinks
        //
        if (userIsSchedulerSalesforceTenantLinkReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerSalesforceTenantLinkReader = user.readPermission >= 1;
            } else {
                userIsSchedulerSalesforceTenantLinkReader = false;
            }
        }

        return userIsSchedulerSalesforceTenantLinkReader;
    }


    public userIsSchedulerSalesforceTenantLinkWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerSalesforceTenantLinkWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.SalesforceTenantLinks
        //
        if (userIsSchedulerSalesforceTenantLinkWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerSalesforceTenantLinkWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerSalesforceTenantLinkWriter = false;
          }      
        }

        return userIsSchedulerSalesforceTenantLinkWriter;
    }

    public GetSalesforceTenantLinkChangeHistoriesForSalesforceTenantLink(salesforceTenantLinkId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SalesforceTenantLinkChangeHistoryData[]> {
        return this.salesforceTenantLinkChangeHistoryService.GetSalesforceTenantLinkChangeHistoryList({
            salesforceTenantLinkId: salesforceTenantLinkId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SalesforceTenantLinkData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SalesforceTenantLinkData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SalesforceTenantLinkTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSalesforceTenantLink(raw: any): SalesforceTenantLinkData {
    if (!raw) return raw;

    //
    // Create a SalesforceTenantLinkData object instance with correct prototype
    //
    const revived = Object.create(SalesforceTenantLinkData.prototype) as SalesforceTenantLinkData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._salesforceTenantLinkChangeHistories = null;
    (revived as any)._salesforceTenantLinkChangeHistoriesPromise = null;
    (revived as any)._salesforceTenantLinkChangeHistoriesSubject = new BehaviorSubject<SalesforceTenantLinkChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSalesforceTenantLinkXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SalesforceTenantLinkChangeHistories$ = (revived as any)._salesforceTenantLinkChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._salesforceTenantLinkChangeHistories === null && (revived as any)._salesforceTenantLinkChangeHistoriesPromise === null) {
                (revived as any).loadSalesforceTenantLinkChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._salesforceTenantLinkChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SalesforceTenantLinkData> | null>(null);

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

  private ReviveSalesforceTenantLinkList(rawList: any[]): SalesforceTenantLinkData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSalesforceTenantLink(raw));
  }

}
