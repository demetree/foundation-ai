/*

   GENERATED SERVICE FOR THE SOFTCREDIT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SoftCredit table.

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
import { GiftData } from './gift.service';
import { ConstituentData } from './constituent.service';
import { SoftCreditChangeHistoryService, SoftCreditChangeHistoryData } from './soft-credit-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SoftCreditQueryParameters {
    giftId: bigint | number | null | undefined = null;
    constituentId: bigint | number | null | undefined = null;
    amount: number | null | undefined = null;
    notes: string | null | undefined = null;
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
export class SoftCreditSubmitData {
    id!: bigint | number;
    giftId!: bigint | number;
    constituentId!: bigint | number;
    amount!: number;
    notes: string | null = null;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}



//
// Version history information returned from version history API endpoints.
// Matches server-side VersionInformation<T> structure.
//
export interface VersionInformation<T> {
    timeStamp: string;           // ISO 8601
    userId: bigint | number;
    userName: string;
    versionNumber: number;
    data: T | null;
}

export class SoftCreditBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SoftCreditChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `softCredit.SoftCreditChildren$` — use with `| async` in templates
//        • Promise:    `softCredit.SoftCreditChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="softCredit.SoftCreditChildren$ | async"`), or
//        • Access the promise getter (`softCredit.SoftCreditChildren` or `await softCredit.SoftCreditChildren`)
//    - Simply reading `softCredit.SoftCreditChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await softCredit.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SoftCreditData {
    id!: bigint | number;
    giftId!: bigint | number;
    constituentId!: bigint | number;
    amount!: number;
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    constituent: ConstituentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    gift: GiftData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _softCreditChangeHistories: SoftCreditChangeHistoryData[] | null = null;
    private _softCreditChangeHistoriesPromise: Promise<SoftCreditChangeHistoryData[]> | null  = null;
    private _softCreditChangeHistoriesSubject = new BehaviorSubject<SoftCreditChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<SoftCreditData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<SoftCreditData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SoftCreditData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SoftCreditChangeHistories$ = this._softCreditChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._softCreditChangeHistories === null && this._softCreditChangeHistoriesPromise === null) {
            this.loadSoftCreditChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SoftCreditChangeHistoriesCount$ = SoftCreditChangeHistoryService.Instance.GetSoftCreditChangeHistoriesRowCount({softCreditId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SoftCreditData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.softCredit.Reload();
  //
  //  Non Async:
  //
  //     softCredit[0].Reload().then(x => {
  //        this.softCredit = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SoftCreditService.Instance.GetSoftCredit(this.id, includeRelations)
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
     this._softCreditChangeHistories = null;
     this._softCreditChangeHistoriesPromise = null;
     this._softCreditChangeHistoriesSubject.next(null);

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
     * Gets the SoftCreditChangeHistories for this SoftCredit.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.softCredit.SoftCreditChangeHistories.then(softCredits => { ... })
     *   or
     *   await this.softCredit.softCredits
     *
    */
    public get SoftCreditChangeHistories(): Promise<SoftCreditChangeHistoryData[]> {
        if (this._softCreditChangeHistories !== null) {
            return Promise.resolve(this._softCreditChangeHistories);
        }

        if (this._softCreditChangeHistoriesPromise !== null) {
            return this._softCreditChangeHistoriesPromise;
        }

        // Start the load
        this.loadSoftCreditChangeHistories();

        return this._softCreditChangeHistoriesPromise!;
    }



    private loadSoftCreditChangeHistories(): void {

        this._softCreditChangeHistoriesPromise = lastValueFrom(
            SoftCreditService.Instance.GetSoftCreditChangeHistoriesForSoftCredit(this.id)
        )
        .then(SoftCreditChangeHistories => {
            this._softCreditChangeHistories = SoftCreditChangeHistories ?? [];
            this._softCreditChangeHistoriesSubject.next(this._softCreditChangeHistories);
            return this._softCreditChangeHistories;
         })
        .catch(err => {
            this._softCreditChangeHistories = [];
            this._softCreditChangeHistoriesSubject.next(this._softCreditChangeHistories);
            throw err;
        })
        .finally(() => {
            this._softCreditChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SoftCreditChangeHistory. Call after mutations to force refresh.
     */
    public ClearSoftCreditChangeHistoriesCache(): void {
        this._softCreditChangeHistories = null;
        this._softCreditChangeHistoriesPromise = null;
        this._softCreditChangeHistoriesSubject.next(this._softCreditChangeHistories);      // Emit to observable
    }

    public get HasSoftCreditChangeHistories(): Promise<boolean> {
        return this.SoftCreditChangeHistories.then(softCreditChangeHistories => softCreditChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (softCredit.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await softCredit.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<SoftCreditData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<SoftCreditData>> {
        const info = await lastValueFrom(
            SoftCreditService.Instance.GetSoftCreditChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this SoftCreditData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SoftCreditData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SoftCreditSubmitData {
        return SoftCreditService.Instance.ConvertToSoftCreditSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SoftCreditService extends SecureEndpointBase {

    private static _instance: SoftCreditService;
    private listCache: Map<string, Observable<Array<SoftCreditData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SoftCreditBasicListData>>>;
    private recordCache: Map<string, Observable<SoftCreditData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private softCreditChangeHistoryService: SoftCreditChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SoftCreditData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SoftCreditBasicListData>>>();
        this.recordCache = new Map<string, Observable<SoftCreditData>>();

        SoftCreditService._instance = this;
    }

    public static get Instance(): SoftCreditService {
      return SoftCreditService._instance;
    }


    public ClearListCaches(config: SoftCreditQueryParameters | null = null) {

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


    public ConvertToSoftCreditSubmitData(data: SoftCreditData): SoftCreditSubmitData {

        let output = new SoftCreditSubmitData();

        output.id = data.id;
        output.giftId = data.giftId;
        output.constituentId = data.constituentId;
        output.amount = data.amount;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSoftCredit(id: bigint | number, includeRelations: boolean = true) : Observable<SoftCreditData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const softCredit$ = this.requestSoftCredit(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SoftCredit", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, softCredit$);

            return softCredit$;
        }

        return this.recordCache.get(configHash) as Observable<SoftCreditData>;
    }

    private requestSoftCredit(id: bigint | number, includeRelations: boolean = true) : Observable<SoftCreditData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SoftCreditData>(this.baseUrl + 'api/SoftCredit/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSoftCredit(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSoftCredit(id, includeRelations));
            }));
    }

    public GetSoftCreditList(config: SoftCreditQueryParameters | any = null) : Observable<Array<SoftCreditData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const softCreditList$ = this.requestSoftCreditList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SoftCredit list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, softCreditList$);

            return softCreditList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SoftCreditData>>;
    }


    private requestSoftCreditList(config: SoftCreditQueryParameters | any) : Observable <Array<SoftCreditData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SoftCreditData>>(this.baseUrl + 'api/SoftCredits', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSoftCreditList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSoftCreditList(config));
            }));
    }

    public GetSoftCreditsRowCount(config: SoftCreditQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const softCreditsRowCount$ = this.requestSoftCreditsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SoftCredits row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, softCreditsRowCount$);

            return softCreditsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSoftCreditsRowCount(config: SoftCreditQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SoftCredits/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSoftCreditsRowCount(config));
            }));
    }

    public GetSoftCreditsBasicListData(config: SoftCreditQueryParameters | any = null) : Observable<Array<SoftCreditBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const softCreditsBasicListData$ = this.requestSoftCreditsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SoftCredits basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, softCreditsBasicListData$);

            return softCreditsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SoftCreditBasicListData>>;
    }


    private requestSoftCreditsBasicListData(config: SoftCreditQueryParameters | any) : Observable<Array<SoftCreditBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SoftCreditBasicListData>>(this.baseUrl + 'api/SoftCredits/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSoftCreditsBasicListData(config));
            }));

    }


    public PutSoftCredit(id: bigint | number, softCredit: SoftCreditSubmitData) : Observable<SoftCreditData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SoftCreditData>(this.baseUrl + 'api/SoftCredit/' + id.toString(), softCredit, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSoftCredit(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSoftCredit(id, softCredit));
            }));
    }


    public PostSoftCredit(softCredit: SoftCreditSubmitData) : Observable<SoftCreditData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SoftCreditData>(this.baseUrl + 'api/SoftCredit', softCredit, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSoftCredit(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSoftCredit(softCredit));
            }));
    }

  
    public DeleteSoftCredit(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SoftCredit/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSoftCredit(id));
            }));
    }

    public RollbackSoftCredit(id: bigint | number, versionNumber: bigint | number) : Observable<SoftCreditData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SoftCreditData>(this.baseUrl + 'api/SoftCredit/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSoftCredit(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackSoftCredit(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a SoftCredit.
     */
    public GetSoftCreditChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<SoftCreditData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SoftCreditData>>(this.baseUrl + 'api/SoftCredit/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSoftCreditChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a SoftCredit.
     */
    public GetSoftCreditAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<SoftCreditData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SoftCreditData>[]>(this.baseUrl + 'api/SoftCredit/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSoftCreditAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a SoftCredit.
     */
    public GetSoftCreditVersion(id: bigint | number, version: number): Observable<SoftCreditData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SoftCreditData>(this.baseUrl + 'api/SoftCredit/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSoftCredit(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSoftCreditVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a SoftCredit at a specific point in time.
     */
    public GetSoftCreditStateAtTime(id: bigint | number, time: string): Observable<SoftCreditData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SoftCreditData>(this.baseUrl + 'api/SoftCredit/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSoftCredit(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSoftCreditStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: SoftCreditQueryParameters | any): string {

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

    public userIsSchedulerSoftCreditReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerSoftCreditReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.SoftCredits
        //
        if (userIsSchedulerSoftCreditReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerSoftCreditReader = user.readPermission >= 0;
            } else {
                userIsSchedulerSoftCreditReader = false;
            }
        }

        return userIsSchedulerSoftCreditReader;
    }


    public userIsSchedulerSoftCreditWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerSoftCreditWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.SoftCredits
        //
        if (userIsSchedulerSoftCreditWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerSoftCreditWriter = user.writePermission >= 0;
          } else {
            userIsSchedulerSoftCreditWriter = false;
          }      
        }

        return userIsSchedulerSoftCreditWriter;
    }

    public GetSoftCreditChangeHistoriesForSoftCredit(softCreditId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SoftCreditChangeHistoryData[]> {
        return this.softCreditChangeHistoryService.GetSoftCreditChangeHistoryList({
            softCreditId: softCreditId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SoftCreditData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SoftCreditData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SoftCreditTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSoftCredit(raw: any): SoftCreditData {
    if (!raw) return raw;

    //
    // Create a SoftCreditData object instance with correct prototype
    //
    const revived = Object.create(SoftCreditData.prototype) as SoftCreditData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._softCreditChangeHistories = null;
    (revived as any)._softCreditChangeHistoriesPromise = null;
    (revived as any)._softCreditChangeHistoriesSubject = new BehaviorSubject<SoftCreditChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSoftCreditXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SoftCreditChangeHistories$ = (revived as any)._softCreditChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._softCreditChangeHistories === null && (revived as any)._softCreditChangeHistoriesPromise === null) {
                (revived as any).loadSoftCreditChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SoftCreditChangeHistoriesCount$ = SoftCreditChangeHistoryService.Instance.GetSoftCreditChangeHistoriesRowCount({softCreditId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveSoftCreditList(rawList: any[]): SoftCreditData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSoftCredit(raw));
  }

}
