/*

   GENERATED SERVICE FOR THE MOCVERSION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the MocVersion table.

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
import { PublishedMocData } from './published-moc.service';
import { MocVersionChangeHistoryService, MocVersionChangeHistoryData } from './moc-version-change-history.service';
import { MocForkService, MocForkData } from './moc-fork.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class MocVersionQueryParameters {
    publishedMocId: bigint | number | null | undefined = null;
    commitMessage: string | null | undefined = null;
    mpdSnapshot: string | null | undefined = null;
    partCount: bigint | number | null | undefined = null;
    addedPartCount: bigint | number | null | undefined = null;
    removedPartCount: bigint | number | null | undefined = null;
    modifiedPartCount: bigint | number | null | undefined = null;
    snapshotDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    authorTenantGuid: string | null | undefined = null;
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
export class MocVersionSubmitData {
    id!: bigint | number;
    publishedMocId!: bigint | number;
    commitMessage!: string;
    mpdSnapshot!: string;
    partCount: bigint | number | null = null;
    addedPartCount: bigint | number | null = null;
    removedPartCount: bigint | number | null = null;
    modifiedPartCount: bigint | number | null = null;
    snapshotDate!: string;      // ISO 8601 (full datetime)
    authorTenantGuid!: string;
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

export class MocVersionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. MocVersionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `mocVersion.MocVersionChildren$` — use with `| async` in templates
//        • Promise:    `mocVersion.MocVersionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="mocVersion.MocVersionChildren$ | async"`), or
//        • Access the promise getter (`mocVersion.MocVersionChildren` or `await mocVersion.MocVersionChildren`)
//    - Simply reading `mocVersion.MocVersionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await mocVersion.Reload()` to refresh the entire object and clear all lazy caches.
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
export class MocVersionData {
    id!: bigint | number;
    publishedMocId!: bigint | number;
    commitMessage!: string;
    mpdSnapshot!: string;
    partCount!: bigint | number;
    addedPartCount!: bigint | number;
    removedPartCount!: bigint | number;
    modifiedPartCount!: bigint | number;
    snapshotDate!: string;      // ISO 8601 (full datetime)
    authorTenantGuid!: string;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    publishedMoc: PublishedMocData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _mocVersionChangeHistories: MocVersionChangeHistoryData[] | null = null;
    private _mocVersionChangeHistoriesPromise: Promise<MocVersionChangeHistoryData[]> | null  = null;
    private _mocVersionChangeHistoriesSubject = new BehaviorSubject<MocVersionChangeHistoryData[] | null>(null);

                
    private _mocForks: MocForkData[] | null = null;
    private _mocForksPromise: Promise<MocForkData[]> | null  = null;
    private _mocForksSubject = new BehaviorSubject<MocForkData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<MocVersionData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<MocVersionData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<MocVersionData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public MocVersionChangeHistories$ = this._mocVersionChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._mocVersionChangeHistories === null && this._mocVersionChangeHistoriesPromise === null) {
            this.loadMocVersionChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _mocVersionChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get MocVersionChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._mocVersionChangeHistoriesCount$ === null) {
            this._mocVersionChangeHistoriesCount$ = MocVersionChangeHistoryService.Instance.GetMocVersionChangeHistoriesRowCount({mocVersionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._mocVersionChangeHistoriesCount$;
    }



    public MocForks$ = this._mocForksSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._mocForks === null && this._mocForksPromise === null) {
            this.loadMocForks(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _mocForksCount$: Observable<bigint | number> | null = null;
    public get MocForksCount$(): Observable<bigint | number> {
        if (this._mocForksCount$ === null) {
            this._mocForksCount$ = MocForkService.Instance.GetMocForksRowCount({mocVersionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._mocForksCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any MocVersionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.mocVersion.Reload();
  //
  //  Non Async:
  //
  //     mocVersion[0].Reload().then(x => {
  //        this.mocVersion = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      MocVersionService.Instance.GetMocVersion(this.id, includeRelations)
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
     this._mocVersionChangeHistories = null;
     this._mocVersionChangeHistoriesPromise = null;
     this._mocVersionChangeHistoriesSubject.next(null);
     this._mocVersionChangeHistoriesCount$ = null;

     this._mocForks = null;
     this._mocForksPromise = null;
     this._mocForksSubject.next(null);
     this._mocForksCount$ = null;

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
     * Gets the MocVersionChangeHistories for this MocVersion.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.mocVersion.MocVersionChangeHistories.then(mocVersions => { ... })
     *   or
     *   await this.mocVersion.mocVersions
     *
    */
    public get MocVersionChangeHistories(): Promise<MocVersionChangeHistoryData[]> {
        if (this._mocVersionChangeHistories !== null) {
            return Promise.resolve(this._mocVersionChangeHistories);
        }

        if (this._mocVersionChangeHistoriesPromise !== null) {
            return this._mocVersionChangeHistoriesPromise;
        }

        // Start the load
        this.loadMocVersionChangeHistories();

        return this._mocVersionChangeHistoriesPromise!;
    }



    private loadMocVersionChangeHistories(): void {

        this._mocVersionChangeHistoriesPromise = lastValueFrom(
            MocVersionService.Instance.GetMocVersionChangeHistoriesForMocVersion(this.id)
        )
        .then(MocVersionChangeHistories => {
            this._mocVersionChangeHistories = MocVersionChangeHistories ?? [];
            this._mocVersionChangeHistoriesSubject.next(this._mocVersionChangeHistories);
            return this._mocVersionChangeHistories;
         })
        .catch(err => {
            this._mocVersionChangeHistories = [];
            this._mocVersionChangeHistoriesSubject.next(this._mocVersionChangeHistories);
            throw err;
        })
        .finally(() => {
            this._mocVersionChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached MocVersionChangeHistory. Call after mutations to force refresh.
     */
    public ClearMocVersionChangeHistoriesCache(): void {
        this._mocVersionChangeHistories = null;
        this._mocVersionChangeHistoriesPromise = null;
        this._mocVersionChangeHistoriesSubject.next(this._mocVersionChangeHistories);      // Emit to observable
    }

    public get HasMocVersionChangeHistories(): Promise<boolean> {
        return this.MocVersionChangeHistories.then(mocVersionChangeHistories => mocVersionChangeHistories.length > 0);
    }


    /**
     *
     * Gets the MocForks for this MocVersion.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.mocVersion.MocForks.then(mocVersions => { ... })
     *   or
     *   await this.mocVersion.mocVersions
     *
    */
    public get MocForks(): Promise<MocForkData[]> {
        if (this._mocForks !== null) {
            return Promise.resolve(this._mocForks);
        }

        if (this._mocForksPromise !== null) {
            return this._mocForksPromise;
        }

        // Start the load
        this.loadMocForks();

        return this._mocForksPromise!;
    }



    private loadMocForks(): void {

        this._mocForksPromise = lastValueFrom(
            MocVersionService.Instance.GetMocForksForMocVersion(this.id)
        )
        .then(MocForks => {
            this._mocForks = MocForks ?? [];
            this._mocForksSubject.next(this._mocForks);
            return this._mocForks;
         })
        .catch(err => {
            this._mocForks = [];
            this._mocForksSubject.next(this._mocForks);
            throw err;
        })
        .finally(() => {
            this._mocForksPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached MocFork. Call after mutations to force refresh.
     */
    public ClearMocForksCache(): void {
        this._mocForks = null;
        this._mocForksPromise = null;
        this._mocForksSubject.next(this._mocForks);      // Emit to observable
    }

    public get HasMocForks(): Promise<boolean> {
        return this.MocForks.then(mocForks => mocForks.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (mocVersion.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await mocVersion.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<MocVersionData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<MocVersionData>> {
        const info = await lastValueFrom(
            MocVersionService.Instance.GetMocVersionChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this MocVersionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this MocVersionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): MocVersionSubmitData {
        return MocVersionService.Instance.ConvertToMocVersionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class MocVersionService extends SecureEndpointBase {

    private static _instance: MocVersionService;
    private listCache: Map<string, Observable<Array<MocVersionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<MocVersionBasicListData>>>;
    private recordCache: Map<string, Observable<MocVersionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private mocVersionChangeHistoryService: MocVersionChangeHistoryService,
        private mocForkService: MocForkService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<MocVersionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<MocVersionBasicListData>>>();
        this.recordCache = new Map<string, Observable<MocVersionData>>();

        MocVersionService._instance = this;
    }

    public static get Instance(): MocVersionService {
      return MocVersionService._instance;
    }


    public ClearListCaches(config: MocVersionQueryParameters | null = null) {

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


    public ConvertToMocVersionSubmitData(data: MocVersionData): MocVersionSubmitData {

        let output = new MocVersionSubmitData();

        output.id = data.id;
        output.publishedMocId = data.publishedMocId;
        output.commitMessage = data.commitMessage;
        output.mpdSnapshot = data.mpdSnapshot;
        output.partCount = data.partCount;
        output.addedPartCount = data.addedPartCount;
        output.removedPartCount = data.removedPartCount;
        output.modifiedPartCount = data.modifiedPartCount;
        output.snapshotDate = data.snapshotDate;
        output.authorTenantGuid = data.authorTenantGuid;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetMocVersion(id: bigint | number, includeRelations: boolean = true) : Observable<MocVersionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const mocVersion$ = this.requestMocVersion(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MocVersion", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, mocVersion$);

            return mocVersion$;
        }

        return this.recordCache.get(configHash) as Observable<MocVersionData>;
    }

    private requestMocVersion(id: bigint | number, includeRelations: boolean = true) : Observable<MocVersionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<MocVersionData>(this.baseUrl + 'api/MocVersion/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveMocVersion(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestMocVersion(id, includeRelations));
            }));
    }

    public GetMocVersionList(config: MocVersionQueryParameters | any = null) : Observable<Array<MocVersionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const mocVersionList$ = this.requestMocVersionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MocVersion list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, mocVersionList$);

            return mocVersionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<MocVersionData>>;
    }


    private requestMocVersionList(config: MocVersionQueryParameters | any) : Observable <Array<MocVersionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MocVersionData>>(this.baseUrl + 'api/MocVersions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveMocVersionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestMocVersionList(config));
            }));
    }

    public GetMocVersionsRowCount(config: MocVersionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const mocVersionsRowCount$ = this.requestMocVersionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MocVersions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, mocVersionsRowCount$);

            return mocVersionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestMocVersionsRowCount(config: MocVersionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/MocVersions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMocVersionsRowCount(config));
            }));
    }

    public GetMocVersionsBasicListData(config: MocVersionQueryParameters | any = null) : Observable<Array<MocVersionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const mocVersionsBasicListData$ = this.requestMocVersionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MocVersions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, mocVersionsBasicListData$);

            return mocVersionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<MocVersionBasicListData>>;
    }


    private requestMocVersionsBasicListData(config: MocVersionQueryParameters | any) : Observable<Array<MocVersionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MocVersionBasicListData>>(this.baseUrl + 'api/MocVersions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMocVersionsBasicListData(config));
            }));

    }


    public PutMocVersion(id: bigint | number, mocVersion: MocVersionSubmitData) : Observable<MocVersionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<MocVersionData>(this.baseUrl + 'api/MocVersion/' + id.toString(), mocVersion, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMocVersion(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutMocVersion(id, mocVersion));
            }));
    }


    public PostMocVersion(mocVersion: MocVersionSubmitData) : Observable<MocVersionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<MocVersionData>(this.baseUrl + 'api/MocVersion', mocVersion, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMocVersion(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostMocVersion(mocVersion));
            }));
    }

  
    public DeleteMocVersion(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/MocVersion/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteMocVersion(id));
            }));
    }

    public RollbackMocVersion(id: bigint | number, versionNumber: bigint | number) : Observable<MocVersionData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<MocVersionData>(this.baseUrl + 'api/MocVersion/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMocVersion(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackMocVersion(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a MocVersion.
     */
    public GetMocVersionChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<MocVersionData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<MocVersionData>>(this.baseUrl + 'api/MocVersion/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetMocVersionChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a MocVersion.
     */
    public GetMocVersionAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<MocVersionData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<MocVersionData>[]>(this.baseUrl + 'api/MocVersion/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetMocVersionAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a MocVersion.
     */
    public GetMocVersionVersion(id: bigint | number, version: number): Observable<MocVersionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<MocVersionData>(this.baseUrl + 'api/MocVersion/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveMocVersion(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetMocVersionVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a MocVersion at a specific point in time.
     */
    public GetMocVersionStateAtTime(id: bigint | number, time: string): Observable<MocVersionData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<MocVersionData>(this.baseUrl + 'api/MocVersion/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveMocVersion(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetMocVersionStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: MocVersionQueryParameters | any): string {

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

    public userIsBMCMocVersionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCMocVersionReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.MocVersions
        //
        if (userIsBMCMocVersionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCMocVersionReader = user.readPermission >= 1;
            } else {
                userIsBMCMocVersionReader = false;
            }
        }

        return userIsBMCMocVersionReader;
    }


    public userIsBMCMocVersionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCMocVersionWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.MocVersions
        //
        if (userIsBMCMocVersionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCMocVersionWriter = user.writePermission >= 1;
          } else {
            userIsBMCMocVersionWriter = false;
          }      
        }

        return userIsBMCMocVersionWriter;
    }

    public GetMocVersionChangeHistoriesForMocVersion(mocVersionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MocVersionChangeHistoryData[]> {
        return this.mocVersionChangeHistoryService.GetMocVersionChangeHistoryList({
            mocVersionId: mocVersionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetMocForksForMocVersion(mocVersionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MocForkData[]> {
        return this.mocForkService.GetMocForkList({
            mocVersionId: mocVersionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full MocVersionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the MocVersionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when MocVersionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveMocVersion(raw: any): MocVersionData {
    if (!raw) return raw;

    //
    // Create a MocVersionData object instance with correct prototype
    //
    const revived = Object.create(MocVersionData.prototype) as MocVersionData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._mocVersionChangeHistories = null;
    (revived as any)._mocVersionChangeHistoriesPromise = null;
    (revived as any)._mocVersionChangeHistoriesSubject = new BehaviorSubject<MocVersionChangeHistoryData[] | null>(null);

    (revived as any)._mocForks = null;
    (revived as any)._mocForksPromise = null;
    (revived as any)._mocForksSubject = new BehaviorSubject<MocForkData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadMocVersionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).MocVersionChangeHistories$ = (revived as any)._mocVersionChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._mocVersionChangeHistories === null && (revived as any)._mocVersionChangeHistoriesPromise === null) {
                (revived as any).loadMocVersionChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._mocVersionChangeHistoriesCount$ = null;


    (revived as any).MocForks$ = (revived as any)._mocForksSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._mocForks === null && (revived as any)._mocForksPromise === null) {
                (revived as any).loadMocForks();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._mocForksCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<MocVersionData> | null>(null);

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

  private ReviveMocVersionList(rawList: any[]): MocVersionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveMocVersion(raw));
  }

}
