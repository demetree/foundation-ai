/*

   GENERATED SERVICE FOR THE BUILDCHALLENGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BuildChallenge table.

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
import { BuildChallengeChangeHistoryService, BuildChallengeChangeHistoryData } from './build-challenge-change-history.service';
import { BuildChallengeEntryService, BuildChallengeEntryData } from './build-challenge-entry.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BuildChallengeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    rules: string | null | undefined = null;
    thumbnailImagePath: string | null | undefined = null;
    startDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    endDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    votingEndDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    isActive: boolean | null | undefined = null;
    isFeatured: boolean | null | undefined = null;
    entryCount: bigint | number | null | undefined = null;
    maxPartsLimit: bigint | number | null | undefined = null;
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
export class BuildChallengeSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    rules: string | null = null;
    thumbnailImagePath: string | null = null;
    startDate!: string;      // ISO 8601 (full datetime)
    endDate!: string;      // ISO 8601 (full datetime)
    votingEndDate: string | null = null;     // ISO 8601 (full datetime)
    isActive!: boolean;
    isFeatured!: boolean;
    entryCount!: bigint | number;
    maxPartsLimit: bigint | number | null = null;
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

export class BuildChallengeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BuildChallengeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `buildChallenge.BuildChallengeChildren$` — use with `| async` in templates
//        • Promise:    `buildChallenge.BuildChallengeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="buildChallenge.BuildChallengeChildren$ | async"`), or
//        • Access the promise getter (`buildChallenge.BuildChallengeChildren` or `await buildChallenge.BuildChallengeChildren`)
//    - Simply reading `buildChallenge.BuildChallengeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await buildChallenge.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BuildChallengeData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    rules!: string | null;
    thumbnailImagePath!: string | null;
    startDate!: string;      // ISO 8601 (full datetime)
    endDate!: string;      // ISO 8601 (full datetime)
    votingEndDate!: string | null;   // ISO 8601 (full datetime)
    isActive!: boolean;
    isFeatured!: boolean;
    entryCount!: bigint | number;
    maxPartsLimit!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _buildChallengeChangeHistories: BuildChallengeChangeHistoryData[] | null = null;
    private _buildChallengeChangeHistoriesPromise: Promise<BuildChallengeChangeHistoryData[]> | null  = null;
    private _buildChallengeChangeHistoriesSubject = new BehaviorSubject<BuildChallengeChangeHistoryData[] | null>(null);

                
    private _buildChallengeEntries: BuildChallengeEntryData[] | null = null;
    private _buildChallengeEntriesPromise: Promise<BuildChallengeEntryData[]> | null  = null;
    private _buildChallengeEntriesSubject = new BehaviorSubject<BuildChallengeEntryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<BuildChallengeData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<BuildChallengeData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<BuildChallengeData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public BuildChallengeChangeHistories$ = this._buildChallengeChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._buildChallengeChangeHistories === null && this._buildChallengeChangeHistoriesPromise === null) {
            this.loadBuildChallengeChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public BuildChallengeChangeHistoriesCount$ = BuildChallengeChangeHistoryService.Instance.GetBuildChallengeChangeHistoriesRowCount({buildChallengeId: this.id,
      active: true,
      deleted: false
    });



    public BuildChallengeEntries$ = this._buildChallengeEntriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._buildChallengeEntries === null && this._buildChallengeEntriesPromise === null) {
            this.loadBuildChallengeEntries(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public BuildChallengeEntriesCount$ = BuildChallengeEntryService.Instance.GetBuildChallengeEntriesRowCount({buildChallengeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any BuildChallengeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.buildChallenge.Reload();
  //
  //  Non Async:
  //
  //     buildChallenge[0].Reload().then(x => {
  //        this.buildChallenge = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BuildChallengeService.Instance.GetBuildChallenge(this.id, includeRelations)
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
     this._buildChallengeChangeHistories = null;
     this._buildChallengeChangeHistoriesPromise = null;
     this._buildChallengeChangeHistoriesSubject.next(null);

     this._buildChallengeEntries = null;
     this._buildChallengeEntriesPromise = null;
     this._buildChallengeEntriesSubject.next(null);

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
     * Gets the BuildChallengeChangeHistories for this BuildChallenge.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.buildChallenge.BuildChallengeChangeHistories.then(buildChallenges => { ... })
     *   or
     *   await this.buildChallenge.buildChallenges
     *
    */
    public get BuildChallengeChangeHistories(): Promise<BuildChallengeChangeHistoryData[]> {
        if (this._buildChallengeChangeHistories !== null) {
            return Promise.resolve(this._buildChallengeChangeHistories);
        }

        if (this._buildChallengeChangeHistoriesPromise !== null) {
            return this._buildChallengeChangeHistoriesPromise;
        }

        // Start the load
        this.loadBuildChallengeChangeHistories();

        return this._buildChallengeChangeHistoriesPromise!;
    }



    private loadBuildChallengeChangeHistories(): void {

        this._buildChallengeChangeHistoriesPromise = lastValueFrom(
            BuildChallengeService.Instance.GetBuildChallengeChangeHistoriesForBuildChallenge(this.id)
        )
        .then(BuildChallengeChangeHistories => {
            this._buildChallengeChangeHistories = BuildChallengeChangeHistories ?? [];
            this._buildChallengeChangeHistoriesSubject.next(this._buildChallengeChangeHistories);
            return this._buildChallengeChangeHistories;
         })
        .catch(err => {
            this._buildChallengeChangeHistories = [];
            this._buildChallengeChangeHistoriesSubject.next(this._buildChallengeChangeHistories);
            throw err;
        })
        .finally(() => {
            this._buildChallengeChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BuildChallengeChangeHistory. Call after mutations to force refresh.
     */
    public ClearBuildChallengeChangeHistoriesCache(): void {
        this._buildChallengeChangeHistories = null;
        this._buildChallengeChangeHistoriesPromise = null;
        this._buildChallengeChangeHistoriesSubject.next(this._buildChallengeChangeHistories);      // Emit to observable
    }

    public get HasBuildChallengeChangeHistories(): Promise<boolean> {
        return this.BuildChallengeChangeHistories.then(buildChallengeChangeHistories => buildChallengeChangeHistories.length > 0);
    }


    /**
     *
     * Gets the BuildChallengeEntries for this BuildChallenge.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.buildChallenge.BuildChallengeEntries.then(buildChallenges => { ... })
     *   or
     *   await this.buildChallenge.buildChallenges
     *
    */
    public get BuildChallengeEntries(): Promise<BuildChallengeEntryData[]> {
        if (this._buildChallengeEntries !== null) {
            return Promise.resolve(this._buildChallengeEntries);
        }

        if (this._buildChallengeEntriesPromise !== null) {
            return this._buildChallengeEntriesPromise;
        }

        // Start the load
        this.loadBuildChallengeEntries();

        return this._buildChallengeEntriesPromise!;
    }



    private loadBuildChallengeEntries(): void {

        this._buildChallengeEntriesPromise = lastValueFrom(
            BuildChallengeService.Instance.GetBuildChallengeEntriesForBuildChallenge(this.id)
        )
        .then(BuildChallengeEntries => {
            this._buildChallengeEntries = BuildChallengeEntries ?? [];
            this._buildChallengeEntriesSubject.next(this._buildChallengeEntries);
            return this._buildChallengeEntries;
         })
        .catch(err => {
            this._buildChallengeEntries = [];
            this._buildChallengeEntriesSubject.next(this._buildChallengeEntries);
            throw err;
        })
        .finally(() => {
            this._buildChallengeEntriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BuildChallengeEntry. Call after mutations to force refresh.
     */
    public ClearBuildChallengeEntriesCache(): void {
        this._buildChallengeEntries = null;
        this._buildChallengeEntriesPromise = null;
        this._buildChallengeEntriesSubject.next(this._buildChallengeEntries);      // Emit to observable
    }

    public get HasBuildChallengeEntries(): Promise<boolean> {
        return this.BuildChallengeEntries.then(buildChallengeEntries => buildChallengeEntries.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (buildChallenge.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await buildChallenge.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<BuildChallengeData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<BuildChallengeData>> {
        const info = await lastValueFrom(
            BuildChallengeService.Instance.GetBuildChallengeChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this BuildChallengeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BuildChallengeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BuildChallengeSubmitData {
        return BuildChallengeService.Instance.ConvertToBuildChallengeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BuildChallengeService extends SecureEndpointBase {

    private static _instance: BuildChallengeService;
    private listCache: Map<string, Observable<Array<BuildChallengeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BuildChallengeBasicListData>>>;
    private recordCache: Map<string, Observable<BuildChallengeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private buildChallengeChangeHistoryService: BuildChallengeChangeHistoryService,
        private buildChallengeEntryService: BuildChallengeEntryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BuildChallengeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BuildChallengeBasicListData>>>();
        this.recordCache = new Map<string, Observable<BuildChallengeData>>();

        BuildChallengeService._instance = this;
    }

    public static get Instance(): BuildChallengeService {
      return BuildChallengeService._instance;
    }


    public ClearListCaches(config: BuildChallengeQueryParameters | null = null) {

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


    public ConvertToBuildChallengeSubmitData(data: BuildChallengeData): BuildChallengeSubmitData {

        let output = new BuildChallengeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.rules = data.rules;
        output.thumbnailImagePath = data.thumbnailImagePath;
        output.startDate = data.startDate;
        output.endDate = data.endDate;
        output.votingEndDate = data.votingEndDate;
        output.isActive = data.isActive;
        output.isFeatured = data.isFeatured;
        output.entryCount = data.entryCount;
        output.maxPartsLimit = data.maxPartsLimit;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBuildChallenge(id: bigint | number, includeRelations: boolean = true) : Observable<BuildChallengeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const buildChallenge$ = this.requestBuildChallenge(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildChallenge", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, buildChallenge$);

            return buildChallenge$;
        }

        return this.recordCache.get(configHash) as Observable<BuildChallengeData>;
    }

    private requestBuildChallenge(id: bigint | number, includeRelations: boolean = true) : Observable<BuildChallengeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BuildChallengeData>(this.baseUrl + 'api/BuildChallenge/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBuildChallenge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildChallenge(id, includeRelations));
            }));
    }

    public GetBuildChallengeList(config: BuildChallengeQueryParameters | any = null) : Observable<Array<BuildChallengeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const buildChallengeList$ = this.requestBuildChallengeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildChallenge list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, buildChallengeList$);

            return buildChallengeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BuildChallengeData>>;
    }


    private requestBuildChallengeList(config: BuildChallengeQueryParameters | any) : Observable <Array<BuildChallengeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildChallengeData>>(this.baseUrl + 'api/BuildChallenges', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBuildChallengeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildChallengeList(config));
            }));
    }

    public GetBuildChallengesRowCount(config: BuildChallengeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const buildChallengesRowCount$ = this.requestBuildChallengesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildChallenges row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, buildChallengesRowCount$);

            return buildChallengesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBuildChallengesRowCount(config: BuildChallengeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BuildChallenges/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildChallengesRowCount(config));
            }));
    }

    public GetBuildChallengesBasicListData(config: BuildChallengeQueryParameters | any = null) : Observable<Array<BuildChallengeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const buildChallengesBasicListData$ = this.requestBuildChallengesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildChallenges basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, buildChallengesBasicListData$);

            return buildChallengesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BuildChallengeBasicListData>>;
    }


    private requestBuildChallengesBasicListData(config: BuildChallengeQueryParameters | any) : Observable<Array<BuildChallengeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildChallengeBasicListData>>(this.baseUrl + 'api/BuildChallenges/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildChallengesBasicListData(config));
            }));

    }


    public PutBuildChallenge(id: bigint | number, buildChallenge: BuildChallengeSubmitData) : Observable<BuildChallengeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BuildChallengeData>(this.baseUrl + 'api/BuildChallenge/' + id.toString(), buildChallenge, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildChallenge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBuildChallenge(id, buildChallenge));
            }));
    }


    public PostBuildChallenge(buildChallenge: BuildChallengeSubmitData) : Observable<BuildChallengeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BuildChallengeData>(this.baseUrl + 'api/BuildChallenge', buildChallenge, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildChallenge(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBuildChallenge(buildChallenge));
            }));
    }

  
    public DeleteBuildChallenge(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BuildChallenge/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBuildChallenge(id));
            }));
    }

    public RollbackBuildChallenge(id: bigint | number, versionNumber: bigint | number) : Observable<BuildChallengeData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BuildChallengeData>(this.baseUrl + 'api/BuildChallenge/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildChallenge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackBuildChallenge(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a BuildChallenge.
     */
    public GetBuildChallengeChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<BuildChallengeData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<BuildChallengeData>>(this.baseUrl + 'api/BuildChallenge/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetBuildChallengeChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a BuildChallenge.
     */
    public GetBuildChallengeAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<BuildChallengeData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<BuildChallengeData>[]>(this.baseUrl + 'api/BuildChallenge/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetBuildChallengeAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a BuildChallenge.
     */
    public GetBuildChallengeVersion(id: bigint | number, version: number): Observable<BuildChallengeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BuildChallengeData>(this.baseUrl + 'api/BuildChallenge/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveBuildChallenge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetBuildChallengeVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a BuildChallenge at a specific point in time.
     */
    public GetBuildChallengeStateAtTime(id: bigint | number, time: string): Observable<BuildChallengeData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BuildChallengeData>(this.baseUrl + 'api/BuildChallenge/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveBuildChallenge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetBuildChallengeStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: BuildChallengeQueryParameters | any): string {

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

    public userIsBMCBuildChallengeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBuildChallengeReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BuildChallenges
        //
        if (userIsBMCBuildChallengeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBuildChallengeReader = user.readPermission >= 1;
            } else {
                userIsBMCBuildChallengeReader = false;
            }
        }

        return userIsBMCBuildChallengeReader;
    }


    public userIsBMCBuildChallengeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBuildChallengeWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BuildChallenges
        //
        if (userIsBMCBuildChallengeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBuildChallengeWriter = user.writePermission >= 100;
          } else {
            userIsBMCBuildChallengeWriter = false;
          }      
        }

        return userIsBMCBuildChallengeWriter;
    }

    public GetBuildChallengeChangeHistoriesForBuildChallenge(buildChallengeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BuildChallengeChangeHistoryData[]> {
        return this.buildChallengeChangeHistoryService.GetBuildChallengeChangeHistoryList({
            buildChallengeId: buildChallengeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBuildChallengeEntriesForBuildChallenge(buildChallengeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BuildChallengeEntryData[]> {
        return this.buildChallengeEntryService.GetBuildChallengeEntryList({
            buildChallengeId: buildChallengeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full BuildChallengeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BuildChallengeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BuildChallengeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBuildChallenge(raw: any): BuildChallengeData {
    if (!raw) return raw;

    //
    // Create a BuildChallengeData object instance with correct prototype
    //
    const revived = Object.create(BuildChallengeData.prototype) as BuildChallengeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._buildChallengeChangeHistories = null;
    (revived as any)._buildChallengeChangeHistoriesPromise = null;
    (revived as any)._buildChallengeChangeHistoriesSubject = new BehaviorSubject<BuildChallengeChangeHistoryData[] | null>(null);

    (revived as any)._buildChallengeEntries = null;
    (revived as any)._buildChallengeEntriesPromise = null;
    (revived as any)._buildChallengeEntriesSubject = new BehaviorSubject<BuildChallengeEntryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadBuildChallengeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).BuildChallengeChangeHistories$ = (revived as any)._buildChallengeChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._buildChallengeChangeHistories === null && (revived as any)._buildChallengeChangeHistoriesPromise === null) {
                (revived as any).loadBuildChallengeChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).BuildChallengeChangeHistoriesCount$ = BuildChallengeChangeHistoryService.Instance.GetBuildChallengeChangeHistoriesRowCount({buildChallengeId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).BuildChallengeEntries$ = (revived as any)._buildChallengeEntriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._buildChallengeEntries === null && (revived as any)._buildChallengeEntriesPromise === null) {
                (revived as any).loadBuildChallengeEntries();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).BuildChallengeEntriesCount$ = BuildChallengeEntryService.Instance.GetBuildChallengeEntriesRowCount({buildChallengeId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<BuildChallengeData> | null>(null);

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

  private ReviveBuildChallengeList(rawList: any[]): BuildChallengeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBuildChallenge(raw));
  }

}
