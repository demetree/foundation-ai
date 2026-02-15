/*

   GENERATED SERVICE FOR THE BUILDCHALLENGEENTRY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BuildChallengeEntry table.

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
import { BuildChallengeData } from './build-challenge.service';
import { PublishedMocData } from './published-moc.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BuildChallengeEntryQueryParameters {
    buildChallengeId: bigint | number | null | undefined = null;
    publishedMocId: bigint | number | null | undefined = null;
    submittedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    entryNotes: string | null | undefined = null;
    voteCount: bigint | number | null | undefined = null;
    isWinner: boolean | null | undefined = null;
    isDisqualified: boolean | null | undefined = null;
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
export class BuildChallengeEntrySubmitData {
    id!: bigint | number;
    buildChallengeId!: bigint | number;
    publishedMocId!: bigint | number;
    submittedDate!: string;      // ISO 8601 (full datetime)
    entryNotes: string | null = null;
    voteCount!: bigint | number;
    isWinner!: boolean;
    isDisqualified!: boolean;
    active!: boolean;
    deleted!: boolean;
}


export class BuildChallengeEntryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BuildChallengeEntryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `buildChallengeEntry.BuildChallengeEntryChildren$` — use with `| async` in templates
//        • Promise:    `buildChallengeEntry.BuildChallengeEntryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="buildChallengeEntry.BuildChallengeEntryChildren$ | async"`), or
//        • Access the promise getter (`buildChallengeEntry.BuildChallengeEntryChildren` or `await buildChallengeEntry.BuildChallengeEntryChildren`)
//    - Simply reading `buildChallengeEntry.BuildChallengeEntryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await buildChallengeEntry.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BuildChallengeEntryData {
    id!: bigint | number;
    buildChallengeId!: bigint | number;
    publishedMocId!: bigint | number;
    submittedDate!: string;      // ISO 8601 (full datetime)
    entryNotes!: string | null;
    voteCount!: bigint | number;
    isWinner!: boolean;
    isDisqualified!: boolean;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    buildChallenge: BuildChallengeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    publishedMoc: PublishedMocData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any BuildChallengeEntryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.buildChallengeEntry.Reload();
  //
  //  Non Async:
  //
  //     buildChallengeEntry[0].Reload().then(x => {
  //        this.buildChallengeEntry = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BuildChallengeEntryService.Instance.GetBuildChallengeEntry(this.id, includeRelations)
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
     * Updates the state of this BuildChallengeEntryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BuildChallengeEntryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BuildChallengeEntrySubmitData {
        return BuildChallengeEntryService.Instance.ConvertToBuildChallengeEntrySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BuildChallengeEntryService extends SecureEndpointBase {

    private static _instance: BuildChallengeEntryService;
    private listCache: Map<string, Observable<Array<BuildChallengeEntryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BuildChallengeEntryBasicListData>>>;
    private recordCache: Map<string, Observable<BuildChallengeEntryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BuildChallengeEntryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BuildChallengeEntryBasicListData>>>();
        this.recordCache = new Map<string, Observable<BuildChallengeEntryData>>();

        BuildChallengeEntryService._instance = this;
    }

    public static get Instance(): BuildChallengeEntryService {
      return BuildChallengeEntryService._instance;
    }


    public ClearListCaches(config: BuildChallengeEntryQueryParameters | null = null) {

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


    public ConvertToBuildChallengeEntrySubmitData(data: BuildChallengeEntryData): BuildChallengeEntrySubmitData {

        let output = new BuildChallengeEntrySubmitData();

        output.id = data.id;
        output.buildChallengeId = data.buildChallengeId;
        output.publishedMocId = data.publishedMocId;
        output.submittedDate = data.submittedDate;
        output.entryNotes = data.entryNotes;
        output.voteCount = data.voteCount;
        output.isWinner = data.isWinner;
        output.isDisqualified = data.isDisqualified;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBuildChallengeEntry(id: bigint | number, includeRelations: boolean = true) : Observable<BuildChallengeEntryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const buildChallengeEntry$ = this.requestBuildChallengeEntry(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildChallengeEntry", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, buildChallengeEntry$);

            return buildChallengeEntry$;
        }

        return this.recordCache.get(configHash) as Observable<BuildChallengeEntryData>;
    }

    private requestBuildChallengeEntry(id: bigint | number, includeRelations: boolean = true) : Observable<BuildChallengeEntryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BuildChallengeEntryData>(this.baseUrl + 'api/BuildChallengeEntry/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBuildChallengeEntry(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildChallengeEntry(id, includeRelations));
            }));
    }

    public GetBuildChallengeEntryList(config: BuildChallengeEntryQueryParameters | any = null) : Observable<Array<BuildChallengeEntryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const buildChallengeEntryList$ = this.requestBuildChallengeEntryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildChallengeEntry list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, buildChallengeEntryList$);

            return buildChallengeEntryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BuildChallengeEntryData>>;
    }


    private requestBuildChallengeEntryList(config: BuildChallengeEntryQueryParameters | any) : Observable <Array<BuildChallengeEntryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildChallengeEntryData>>(this.baseUrl + 'api/BuildChallengeEntries', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBuildChallengeEntryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBuildChallengeEntryList(config));
            }));
    }

    public GetBuildChallengeEntriesRowCount(config: BuildChallengeEntryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const buildChallengeEntriesRowCount$ = this.requestBuildChallengeEntriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BuildChallengeEntries row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, buildChallengeEntriesRowCount$);

            return buildChallengeEntriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBuildChallengeEntriesRowCount(config: BuildChallengeEntryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BuildChallengeEntries/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildChallengeEntriesRowCount(config));
            }));
    }

    public GetBuildChallengeEntriesBasicListData(config: BuildChallengeEntryQueryParameters | any = null) : Observable<Array<BuildChallengeEntryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const buildChallengeEntriesBasicListData$ = this.requestBuildChallengeEntriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BuildChallengeEntries basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, buildChallengeEntriesBasicListData$);

            return buildChallengeEntriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BuildChallengeEntryBasicListData>>;
    }


    private requestBuildChallengeEntriesBasicListData(config: BuildChallengeEntryQueryParameters | any) : Observable<Array<BuildChallengeEntryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BuildChallengeEntryBasicListData>>(this.baseUrl + 'api/BuildChallengeEntries/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBuildChallengeEntriesBasicListData(config));
            }));

    }


    public PutBuildChallengeEntry(id: bigint | number, buildChallengeEntry: BuildChallengeEntrySubmitData) : Observable<BuildChallengeEntryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BuildChallengeEntryData>(this.baseUrl + 'api/BuildChallengeEntry/' + id.toString(), buildChallengeEntry, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildChallengeEntry(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBuildChallengeEntry(id, buildChallengeEntry));
            }));
    }


    public PostBuildChallengeEntry(buildChallengeEntry: BuildChallengeEntrySubmitData) : Observable<BuildChallengeEntryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BuildChallengeEntryData>(this.baseUrl + 'api/BuildChallengeEntry', buildChallengeEntry, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBuildChallengeEntry(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBuildChallengeEntry(buildChallengeEntry));
            }));
    }

  
    public DeleteBuildChallengeEntry(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BuildChallengeEntry/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBuildChallengeEntry(id));
            }));
    }


    private getConfigHash(config: BuildChallengeEntryQueryParameters | any): string {

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

    public userIsBMCBuildChallengeEntryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBuildChallengeEntryReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BuildChallengeEntries
        //
        if (userIsBMCBuildChallengeEntryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBuildChallengeEntryReader = user.readPermission >= 1;
            } else {
                userIsBMCBuildChallengeEntryReader = false;
            }
        }

        return userIsBMCBuildChallengeEntryReader;
    }


    public userIsBMCBuildChallengeEntryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBuildChallengeEntryWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BuildChallengeEntries
        //
        if (userIsBMCBuildChallengeEntryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBuildChallengeEntryWriter = user.writePermission >= 1;
          } else {
            userIsBMCBuildChallengeEntryWriter = false;
          }      
        }

        return userIsBMCBuildChallengeEntryWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BuildChallengeEntryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BuildChallengeEntryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BuildChallengeEntryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBuildChallengeEntry(raw: any): BuildChallengeEntryData {
    if (!raw) return raw;

    //
    // Create a BuildChallengeEntryData object instance with correct prototype
    //
    const revived = Object.create(BuildChallengeEntryData.prototype) as BuildChallengeEntryData;

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
    // 2. But private methods (loadBuildChallengeEntryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBuildChallengeEntryList(rawList: any[]): BuildChallengeEntryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBuildChallengeEntry(raw));
  }

}
