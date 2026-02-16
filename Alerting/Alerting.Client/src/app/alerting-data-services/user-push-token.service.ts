/*

   GENERATED SERVICE FOR THE USERPUSHTOKEN TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserPushToken table.

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
import { UserPushTokenChangeHistoryService, UserPushTokenChangeHistoryData } from './user-push-token-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserPushTokenQueryParameters {
    userObjectGuid: string | null | undefined = null;
    fcmToken: string | null | undefined = null;
    deviceFingerprint: string | null | undefined = null;
    platform: string | null | undefined = null;
    userAgent: string | null | undefined = null;
    registeredAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    lastUpdatedAt: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class UserPushTokenSubmitData {
    id!: bigint | number;
    userObjectGuid!: string;
    fcmToken!: string;
    deviceFingerprint!: string;
    platform!: string;
    userAgent: string | null = null;
    registeredAt!: string;      // ISO 8601 (full datetime)
    lastUpdatedAt!: string;      // ISO 8601 (full datetime)
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

export class UserPushTokenBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserPushTokenChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userPushToken.UserPushTokenChildren$` — use with `| async` in templates
//        • Promise:    `userPushToken.UserPushTokenChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userPushToken.UserPushTokenChildren$ | async"`), or
//        • Access the promise getter (`userPushToken.UserPushTokenChildren` or `await userPushToken.UserPushTokenChildren`)
//    - Simply reading `userPushToken.UserPushTokenChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userPushToken.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserPushTokenData {
    id!: bigint | number;
    userObjectGuid!: string;
    fcmToken!: string;
    deviceFingerprint!: string;
    platform!: string;
    userAgent!: string | null;
    registeredAt!: string;      // ISO 8601 (full datetime)
    lastUpdatedAt!: string;      // ISO 8601 (full datetime)
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _userPushTokenChangeHistories: UserPushTokenChangeHistoryData[] | null = null;
    private _userPushTokenChangeHistoriesPromise: Promise<UserPushTokenChangeHistoryData[]> | null  = null;
    private _userPushTokenChangeHistoriesSubject = new BehaviorSubject<UserPushTokenChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<UserPushTokenData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<UserPushTokenData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserPushTokenData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public UserPushTokenChangeHistories$ = this._userPushTokenChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userPushTokenChangeHistories === null && this._userPushTokenChangeHistoriesPromise === null) {
            this.loadUserPushTokenChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public UserPushTokenChangeHistoriesCount$ = UserPushTokenChangeHistoryService.Instance.GetUserPushTokenChangeHistoriesRowCount({userPushTokenId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any UserPushTokenData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userPushToken.Reload();
  //
  //  Non Async:
  //
  //     userPushToken[0].Reload().then(x => {
  //        this.userPushToken = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserPushTokenService.Instance.GetUserPushToken(this.id, includeRelations)
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
     this._userPushTokenChangeHistories = null;
     this._userPushTokenChangeHistoriesPromise = null;
     this._userPushTokenChangeHistoriesSubject.next(null);

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
     * Gets the UserPushTokenChangeHistories for this UserPushToken.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.userPushToken.UserPushTokenChangeHistories.then(userPushTokens => { ... })
     *   or
     *   await this.userPushToken.userPushTokens
     *
    */
    public get UserPushTokenChangeHistories(): Promise<UserPushTokenChangeHistoryData[]> {
        if (this._userPushTokenChangeHistories !== null) {
            return Promise.resolve(this._userPushTokenChangeHistories);
        }

        if (this._userPushTokenChangeHistoriesPromise !== null) {
            return this._userPushTokenChangeHistoriesPromise;
        }

        // Start the load
        this.loadUserPushTokenChangeHistories();

        return this._userPushTokenChangeHistoriesPromise!;
    }



    private loadUserPushTokenChangeHistories(): void {

        this._userPushTokenChangeHistoriesPromise = lastValueFrom(
            UserPushTokenService.Instance.GetUserPushTokenChangeHistoriesForUserPushToken(this.id)
        )
        .then(UserPushTokenChangeHistories => {
            this._userPushTokenChangeHistories = UserPushTokenChangeHistories ?? [];
            this._userPushTokenChangeHistoriesSubject.next(this._userPushTokenChangeHistories);
            return this._userPushTokenChangeHistories;
         })
        .catch(err => {
            this._userPushTokenChangeHistories = [];
            this._userPushTokenChangeHistoriesSubject.next(this._userPushTokenChangeHistories);
            throw err;
        })
        .finally(() => {
            this._userPushTokenChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserPushTokenChangeHistory. Call after mutations to force refresh.
     */
    public ClearUserPushTokenChangeHistoriesCache(): void {
        this._userPushTokenChangeHistories = null;
        this._userPushTokenChangeHistoriesPromise = null;
        this._userPushTokenChangeHistoriesSubject.next(this._userPushTokenChangeHistories);      // Emit to observable
    }

    public get HasUserPushTokenChangeHistories(): Promise<boolean> {
        return this.UserPushTokenChangeHistories.then(userPushTokenChangeHistories => userPushTokenChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (userPushToken.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await userPushToken.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<UserPushTokenData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<UserPushTokenData>> {
        const info = await lastValueFrom(
            UserPushTokenService.Instance.GetUserPushTokenChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this UserPushTokenData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserPushTokenData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserPushTokenSubmitData {
        return UserPushTokenService.Instance.ConvertToUserPushTokenSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserPushTokenService extends SecureEndpointBase {

    private static _instance: UserPushTokenService;
    private listCache: Map<string, Observable<Array<UserPushTokenData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserPushTokenBasicListData>>>;
    private recordCache: Map<string, Observable<UserPushTokenData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private userPushTokenChangeHistoryService: UserPushTokenChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserPushTokenData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserPushTokenBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserPushTokenData>>();

        UserPushTokenService._instance = this;
    }

    public static get Instance(): UserPushTokenService {
      return UserPushTokenService._instance;
    }


    public ClearListCaches(config: UserPushTokenQueryParameters | null = null) {

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


    public ConvertToUserPushTokenSubmitData(data: UserPushTokenData): UserPushTokenSubmitData {

        let output = new UserPushTokenSubmitData();

        output.id = data.id;
        output.userObjectGuid = data.userObjectGuid;
        output.fcmToken = data.fcmToken;
        output.deviceFingerprint = data.deviceFingerprint;
        output.platform = data.platform;
        output.userAgent = data.userAgent;
        output.registeredAt = data.registeredAt;
        output.lastUpdatedAt = data.lastUpdatedAt;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserPushToken(id: bigint | number, includeRelations: boolean = true) : Observable<UserPushTokenData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userPushToken$ = this.requestUserPushToken(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserPushToken", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userPushToken$);

            return userPushToken$;
        }

        return this.recordCache.get(configHash) as Observable<UserPushTokenData>;
    }

    private requestUserPushToken(id: bigint | number, includeRelations: boolean = true) : Observable<UserPushTokenData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserPushTokenData>(this.baseUrl + 'api/UserPushToken/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserPushToken(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserPushToken(id, includeRelations));
            }));
    }

    public GetUserPushTokenList(config: UserPushTokenQueryParameters | any = null) : Observable<Array<UserPushTokenData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userPushTokenList$ = this.requestUserPushTokenList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserPushToken list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userPushTokenList$);

            return userPushTokenList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserPushTokenData>>;
    }


    private requestUserPushTokenList(config: UserPushTokenQueryParameters | any) : Observable <Array<UserPushTokenData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserPushTokenData>>(this.baseUrl + 'api/UserPushTokens', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserPushTokenList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserPushTokenList(config));
            }));
    }

    public GetUserPushTokensRowCount(config: UserPushTokenQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userPushTokensRowCount$ = this.requestUserPushTokensRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserPushTokens row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userPushTokensRowCount$);

            return userPushTokensRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserPushTokensRowCount(config: UserPushTokenQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserPushTokens/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserPushTokensRowCount(config));
            }));
    }

    public GetUserPushTokensBasicListData(config: UserPushTokenQueryParameters | any = null) : Observable<Array<UserPushTokenBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userPushTokensBasicListData$ = this.requestUserPushTokensBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserPushTokens basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userPushTokensBasicListData$);

            return userPushTokensBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserPushTokenBasicListData>>;
    }


    private requestUserPushTokensBasicListData(config: UserPushTokenQueryParameters | any) : Observable<Array<UserPushTokenBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserPushTokenBasicListData>>(this.baseUrl + 'api/UserPushTokens/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserPushTokensBasicListData(config));
            }));

    }


    public PutUserPushToken(id: bigint | number, userPushToken: UserPushTokenSubmitData) : Observable<UserPushTokenData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserPushTokenData>(this.baseUrl + 'api/UserPushToken/' + id.toString(), userPushToken, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserPushToken(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserPushToken(id, userPushToken));
            }));
    }


    public PostUserPushToken(userPushToken: UserPushTokenSubmitData) : Observable<UserPushTokenData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserPushTokenData>(this.baseUrl + 'api/UserPushToken', userPushToken, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserPushToken(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserPushToken(userPushToken));
            }));
    }

  
    public DeleteUserPushToken(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserPushToken/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserPushToken(id));
            }));
    }

    public RollbackUserPushToken(id: bigint | number, versionNumber: bigint | number) : Observable<UserPushTokenData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserPushTokenData>(this.baseUrl + 'api/UserPushToken/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserPushToken(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackUserPushToken(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a UserPushToken.
     */
    public GetUserPushTokenChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<UserPushTokenData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserPushTokenData>>(this.baseUrl + 'api/UserPushToken/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserPushTokenChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a UserPushToken.
     */
    public GetUserPushTokenAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<UserPushTokenData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<UserPushTokenData>[]>(this.baseUrl + 'api/UserPushToken/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetUserPushTokenAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a UserPushToken.
     */
    public GetUserPushTokenVersion(id: bigint | number, version: number): Observable<UserPushTokenData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserPushTokenData>(this.baseUrl + 'api/UserPushToken/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserPushToken(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserPushTokenVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a UserPushToken at a specific point in time.
     */
    public GetUserPushTokenStateAtTime(id: bigint | number, time: string): Observable<UserPushTokenData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserPushTokenData>(this.baseUrl + 'api/UserPushToken/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveUserPushToken(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetUserPushTokenStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: UserPushTokenQueryParameters | any): string {

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

    public userIsAlertingUserPushTokenReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingUserPushTokenReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.UserPushTokens
        //
        if (userIsAlertingUserPushTokenReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingUserPushTokenReader = user.readPermission >= 1;
            } else {
                userIsAlertingUserPushTokenReader = false;
            }
        }

        return userIsAlertingUserPushTokenReader;
    }


    public userIsAlertingUserPushTokenWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingUserPushTokenWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.UserPushTokens
        //
        if (userIsAlertingUserPushTokenWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingUserPushTokenWriter = user.writePermission >= 50;
          } else {
            userIsAlertingUserPushTokenWriter = false;
          }      
        }

        return userIsAlertingUserPushTokenWriter;
    }

    public GetUserPushTokenChangeHistoriesForUserPushToken(userPushTokenId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserPushTokenChangeHistoryData[]> {
        return this.userPushTokenChangeHistoryService.GetUserPushTokenChangeHistoryList({
            userPushTokenId: userPushTokenId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full UserPushTokenData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserPushTokenData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserPushTokenTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserPushToken(raw: any): UserPushTokenData {
    if (!raw) return raw;

    //
    // Create a UserPushTokenData object instance with correct prototype
    //
    const revived = Object.create(UserPushTokenData.prototype) as UserPushTokenData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._userPushTokenChangeHistories = null;
    (revived as any)._userPushTokenChangeHistoriesPromise = null;
    (revived as any)._userPushTokenChangeHistoriesSubject = new BehaviorSubject<UserPushTokenChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadUserPushTokenXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).UserPushTokenChangeHistories$ = (revived as any)._userPushTokenChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userPushTokenChangeHistories === null && (revived as any)._userPushTokenChangeHistoriesPromise === null) {
                (revived as any).loadUserPushTokenChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).UserPushTokenChangeHistoriesCount$ = UserPushTokenChangeHistoryService.Instance.GetUserPushTokenChangeHistoriesRowCount({userPushTokenId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<UserPushTokenData> | null>(null);

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

  private ReviveUserPushTokenList(rawList: any[]): UserPushTokenData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserPushToken(raw));
  }

}
