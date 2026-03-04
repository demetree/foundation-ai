/*

   GENERATED SERVICE FOR THE SCHEDULINGTARGETQUALIFICATIONREQUIREMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SchedulingTargetQualificationRequirement table.

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
import { SchedulingTargetData } from './scheduling-target.service';
import { QualificationData } from './qualification.service';
import { SchedulingTargetQualificationRequirementChangeHistoryService, SchedulingTargetQualificationRequirementChangeHistoryData } from './scheduling-target-qualification-requirement-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SchedulingTargetQualificationRequirementQueryParameters {
    schedulingTargetId: bigint | number | null | undefined = null;
    qualificationId: bigint | number | null | undefined = null;
    isRequired: boolean | null | undefined = null;
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
export class SchedulingTargetQualificationRequirementSubmitData {
    id!: bigint | number;
    schedulingTargetId!: bigint | number;
    qualificationId!: bigint | number;
    isRequired!: boolean;
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

export class SchedulingTargetQualificationRequirementBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SchedulingTargetQualificationRequirementChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `schedulingTargetQualificationRequirement.SchedulingTargetQualificationRequirementChildren$` — use with `| async` in templates
//        • Promise:    `schedulingTargetQualificationRequirement.SchedulingTargetQualificationRequirementChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="schedulingTargetQualificationRequirement.SchedulingTargetQualificationRequirementChildren$ | async"`), or
//        • Access the promise getter (`schedulingTargetQualificationRequirement.SchedulingTargetQualificationRequirementChildren` or `await schedulingTargetQualificationRequirement.SchedulingTargetQualificationRequirementChildren`)
//    - Simply reading `schedulingTargetQualificationRequirement.SchedulingTargetQualificationRequirementChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await schedulingTargetQualificationRequirement.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SchedulingTargetQualificationRequirementData {
    id!: bigint | number;
    schedulingTargetId!: bigint | number;
    qualificationId!: bigint | number;
    isRequired!: boolean;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    qualification: QualificationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    schedulingTarget: SchedulingTargetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _schedulingTargetQualificationRequirementChangeHistories: SchedulingTargetQualificationRequirementChangeHistoryData[] | null = null;
    private _schedulingTargetQualificationRequirementChangeHistoriesPromise: Promise<SchedulingTargetQualificationRequirementChangeHistoryData[]> | null  = null;
    private _schedulingTargetQualificationRequirementChangeHistoriesSubject = new BehaviorSubject<SchedulingTargetQualificationRequirementChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<SchedulingTargetQualificationRequirementData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<SchedulingTargetQualificationRequirementData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SchedulingTargetQualificationRequirementData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SchedulingTargetQualificationRequirementChangeHistories$ = this._schedulingTargetQualificationRequirementChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetQualificationRequirementChangeHistories === null && this._schedulingTargetQualificationRequirementChangeHistoriesPromise === null) {
            this.loadSchedulingTargetQualificationRequirementChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _schedulingTargetQualificationRequirementChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get SchedulingTargetQualificationRequirementChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._schedulingTargetQualificationRequirementChangeHistoriesCount$ === null) {
            this._schedulingTargetQualificationRequirementChangeHistoriesCount$ = SchedulingTargetQualificationRequirementChangeHistoryService.Instance.GetSchedulingTargetQualificationRequirementChangeHistoriesRowCount({schedulingTargetQualificationRequirementId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._schedulingTargetQualificationRequirementChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SchedulingTargetQualificationRequirementData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.schedulingTargetQualificationRequirement.Reload();
  //
  //  Non Async:
  //
  //     schedulingTargetQualificationRequirement[0].Reload().then(x => {
  //        this.schedulingTargetQualificationRequirement = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SchedulingTargetQualificationRequirementService.Instance.GetSchedulingTargetQualificationRequirement(this.id, includeRelations)
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
     this._schedulingTargetQualificationRequirementChangeHistories = null;
     this._schedulingTargetQualificationRequirementChangeHistoriesPromise = null;
     this._schedulingTargetQualificationRequirementChangeHistoriesSubject.next(null);
     this._schedulingTargetQualificationRequirementChangeHistoriesCount$ = null;

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
     * Gets the SchedulingTargetQualificationRequirementChangeHistories for this SchedulingTargetQualificationRequirement.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.schedulingTargetQualificationRequirement.SchedulingTargetQualificationRequirementChangeHistories.then(schedulingTargetQualificationRequirements => { ... })
     *   or
     *   await this.schedulingTargetQualificationRequirement.schedulingTargetQualificationRequirements
     *
    */
    public get SchedulingTargetQualificationRequirementChangeHistories(): Promise<SchedulingTargetQualificationRequirementChangeHistoryData[]> {
        if (this._schedulingTargetQualificationRequirementChangeHistories !== null) {
            return Promise.resolve(this._schedulingTargetQualificationRequirementChangeHistories);
        }

        if (this._schedulingTargetQualificationRequirementChangeHistoriesPromise !== null) {
            return this._schedulingTargetQualificationRequirementChangeHistoriesPromise;
        }

        // Start the load
        this.loadSchedulingTargetQualificationRequirementChangeHistories();

        return this._schedulingTargetQualificationRequirementChangeHistoriesPromise!;
    }



    private loadSchedulingTargetQualificationRequirementChangeHistories(): void {

        this._schedulingTargetQualificationRequirementChangeHistoriesPromise = lastValueFrom(
            SchedulingTargetQualificationRequirementService.Instance.GetSchedulingTargetQualificationRequirementChangeHistoriesForSchedulingTargetQualificationRequirement(this.id)
        )
        .then(SchedulingTargetQualificationRequirementChangeHistories => {
            this._schedulingTargetQualificationRequirementChangeHistories = SchedulingTargetQualificationRequirementChangeHistories ?? [];
            this._schedulingTargetQualificationRequirementChangeHistoriesSubject.next(this._schedulingTargetQualificationRequirementChangeHistories);
            return this._schedulingTargetQualificationRequirementChangeHistories;
         })
        .catch(err => {
            this._schedulingTargetQualificationRequirementChangeHistories = [];
            this._schedulingTargetQualificationRequirementChangeHistoriesSubject.next(this._schedulingTargetQualificationRequirementChangeHistories);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetQualificationRequirementChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTargetQualificationRequirementChangeHistory. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetQualificationRequirementChangeHistoriesCache(): void {
        this._schedulingTargetQualificationRequirementChangeHistories = null;
        this._schedulingTargetQualificationRequirementChangeHistoriesPromise = null;
        this._schedulingTargetQualificationRequirementChangeHistoriesSubject.next(this._schedulingTargetQualificationRequirementChangeHistories);      // Emit to observable
    }

    public get HasSchedulingTargetQualificationRequirementChangeHistories(): Promise<boolean> {
        return this.SchedulingTargetQualificationRequirementChangeHistories.then(schedulingTargetQualificationRequirementChangeHistories => schedulingTargetQualificationRequirementChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (schedulingTargetQualificationRequirement.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await schedulingTargetQualificationRequirement.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<SchedulingTargetQualificationRequirementData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<SchedulingTargetQualificationRequirementData>> {
        const info = await lastValueFrom(
            SchedulingTargetQualificationRequirementService.Instance.GetSchedulingTargetQualificationRequirementChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this SchedulingTargetQualificationRequirementData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SchedulingTargetQualificationRequirementData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SchedulingTargetQualificationRequirementSubmitData {
        return SchedulingTargetQualificationRequirementService.Instance.ConvertToSchedulingTargetQualificationRequirementSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SchedulingTargetQualificationRequirementService extends SecureEndpointBase {

    private static _instance: SchedulingTargetQualificationRequirementService;
    private listCache: Map<string, Observable<Array<SchedulingTargetQualificationRequirementData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SchedulingTargetQualificationRequirementBasicListData>>>;
    private recordCache: Map<string, Observable<SchedulingTargetQualificationRequirementData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private schedulingTargetQualificationRequirementChangeHistoryService: SchedulingTargetQualificationRequirementChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SchedulingTargetQualificationRequirementData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SchedulingTargetQualificationRequirementBasicListData>>>();
        this.recordCache = new Map<string, Observable<SchedulingTargetQualificationRequirementData>>();

        SchedulingTargetQualificationRequirementService._instance = this;
    }

    public static get Instance(): SchedulingTargetQualificationRequirementService {
      return SchedulingTargetQualificationRequirementService._instance;
    }


    public ClearListCaches(config: SchedulingTargetQualificationRequirementQueryParameters | null = null) {

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


    public ConvertToSchedulingTargetQualificationRequirementSubmitData(data: SchedulingTargetQualificationRequirementData): SchedulingTargetQualificationRequirementSubmitData {

        let output = new SchedulingTargetQualificationRequirementSubmitData();

        output.id = data.id;
        output.schedulingTargetId = data.schedulingTargetId;
        output.qualificationId = data.qualificationId;
        output.isRequired = data.isRequired;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSchedulingTargetQualificationRequirement(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetQualificationRequirementData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const schedulingTargetQualificationRequirement$ = this.requestSchedulingTargetQualificationRequirement(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetQualificationRequirement", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, schedulingTargetQualificationRequirement$);

            return schedulingTargetQualificationRequirement$;
        }

        return this.recordCache.get(configHash) as Observable<SchedulingTargetQualificationRequirementData>;
    }

    private requestSchedulingTargetQualificationRequirement(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetQualificationRequirementData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetQualificationRequirementData>(this.baseUrl + 'api/SchedulingTargetQualificationRequirement/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSchedulingTargetQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetQualificationRequirement(id, includeRelations));
            }));
    }

    public GetSchedulingTargetQualificationRequirementList(config: SchedulingTargetQualificationRequirementQueryParameters | any = null) : Observable<Array<SchedulingTargetQualificationRequirementData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const schedulingTargetQualificationRequirementList$ = this.requestSchedulingTargetQualificationRequirementList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetQualificationRequirement list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, schedulingTargetQualificationRequirementList$);

            return schedulingTargetQualificationRequirementList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SchedulingTargetQualificationRequirementData>>;
    }


    private requestSchedulingTargetQualificationRequirementList(config: SchedulingTargetQualificationRequirementQueryParameters | any) : Observable <Array<SchedulingTargetQualificationRequirementData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetQualificationRequirementData>>(this.baseUrl + 'api/SchedulingTargetQualificationRequirements', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSchedulingTargetQualificationRequirementList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetQualificationRequirementList(config));
            }));
    }

    public GetSchedulingTargetQualificationRequirementsRowCount(config: SchedulingTargetQualificationRequirementQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const schedulingTargetQualificationRequirementsRowCount$ = this.requestSchedulingTargetQualificationRequirementsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetQualificationRequirements row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, schedulingTargetQualificationRequirementsRowCount$);

            return schedulingTargetQualificationRequirementsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSchedulingTargetQualificationRequirementsRowCount(config: SchedulingTargetQualificationRequirementQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SchedulingTargetQualificationRequirements/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetQualificationRequirementsRowCount(config));
            }));
    }

    public GetSchedulingTargetQualificationRequirementsBasicListData(config: SchedulingTargetQualificationRequirementQueryParameters | any = null) : Observable<Array<SchedulingTargetQualificationRequirementBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const schedulingTargetQualificationRequirementsBasicListData$ = this.requestSchedulingTargetQualificationRequirementsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetQualificationRequirements basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, schedulingTargetQualificationRequirementsBasicListData$);

            return schedulingTargetQualificationRequirementsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SchedulingTargetQualificationRequirementBasicListData>>;
    }


    private requestSchedulingTargetQualificationRequirementsBasicListData(config: SchedulingTargetQualificationRequirementQueryParameters | any) : Observable<Array<SchedulingTargetQualificationRequirementBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetQualificationRequirementBasicListData>>(this.baseUrl + 'api/SchedulingTargetQualificationRequirements/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetQualificationRequirementsBasicListData(config));
            }));

    }


    public PutSchedulingTargetQualificationRequirement(id: bigint | number, schedulingTargetQualificationRequirement: SchedulingTargetQualificationRequirementSubmitData) : Observable<SchedulingTargetQualificationRequirementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SchedulingTargetQualificationRequirementData>(this.baseUrl + 'api/SchedulingTargetQualificationRequirement/' + id.toString(), schedulingTargetQualificationRequirement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSchedulingTargetQualificationRequirement(id, schedulingTargetQualificationRequirement));
            }));
    }


    public PostSchedulingTargetQualificationRequirement(schedulingTargetQualificationRequirement: SchedulingTargetQualificationRequirementSubmitData) : Observable<SchedulingTargetQualificationRequirementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SchedulingTargetQualificationRequirementData>(this.baseUrl + 'api/SchedulingTargetQualificationRequirement', schedulingTargetQualificationRequirement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetQualificationRequirement(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSchedulingTargetQualificationRequirement(schedulingTargetQualificationRequirement));
            }));
    }

  
    public DeleteSchedulingTargetQualificationRequirement(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SchedulingTargetQualificationRequirement/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSchedulingTargetQualificationRequirement(id));
            }));
    }

    public RollbackSchedulingTargetQualificationRequirement(id: bigint | number, versionNumber: bigint | number) : Observable<SchedulingTargetQualificationRequirementData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SchedulingTargetQualificationRequirementData>(this.baseUrl + 'api/SchedulingTargetQualificationRequirement/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackSchedulingTargetQualificationRequirement(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a SchedulingTargetQualificationRequirement.
     */
    public GetSchedulingTargetQualificationRequirementChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<SchedulingTargetQualificationRequirementData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SchedulingTargetQualificationRequirementData>>(this.baseUrl + 'api/SchedulingTargetQualificationRequirement/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSchedulingTargetQualificationRequirementChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a SchedulingTargetQualificationRequirement.
     */
    public GetSchedulingTargetQualificationRequirementAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<SchedulingTargetQualificationRequirementData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SchedulingTargetQualificationRequirementData>[]>(this.baseUrl + 'api/SchedulingTargetQualificationRequirement/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSchedulingTargetQualificationRequirementAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a SchedulingTargetQualificationRequirement.
     */
    public GetSchedulingTargetQualificationRequirementVersion(id: bigint | number, version: number): Observable<SchedulingTargetQualificationRequirementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetQualificationRequirementData>(this.baseUrl + 'api/SchedulingTargetQualificationRequirement/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSchedulingTargetQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSchedulingTargetQualificationRequirementVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a SchedulingTargetQualificationRequirement at a specific point in time.
     */
    public GetSchedulingTargetQualificationRequirementStateAtTime(id: bigint | number, time: string): Observable<SchedulingTargetQualificationRequirementData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetQualificationRequirementData>(this.baseUrl + 'api/SchedulingTargetQualificationRequirement/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSchedulingTargetQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSchedulingTargetQualificationRequirementStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: SchedulingTargetQualificationRequirementQueryParameters | any): string {

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

    public userIsSchedulerSchedulingTargetQualificationRequirementReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerSchedulingTargetQualificationRequirementReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.SchedulingTargetQualificationRequirements
        //
        if (userIsSchedulerSchedulingTargetQualificationRequirementReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerSchedulingTargetQualificationRequirementReader = user.readPermission >= 1;
            } else {
                userIsSchedulerSchedulingTargetQualificationRequirementReader = false;
            }
        }

        return userIsSchedulerSchedulingTargetQualificationRequirementReader;
    }


    public userIsSchedulerSchedulingTargetQualificationRequirementWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerSchedulingTargetQualificationRequirementWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.SchedulingTargetQualificationRequirements
        //
        if (userIsSchedulerSchedulingTargetQualificationRequirementWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerSchedulingTargetQualificationRequirementWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerSchedulingTargetQualificationRequirementWriter = false;
          }      
        }

        return userIsSchedulerSchedulingTargetQualificationRequirementWriter;
    }

    public GetSchedulingTargetQualificationRequirementChangeHistoriesForSchedulingTargetQualificationRequirement(schedulingTargetQualificationRequirementId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetQualificationRequirementChangeHistoryData[]> {
        return this.schedulingTargetQualificationRequirementChangeHistoryService.GetSchedulingTargetQualificationRequirementChangeHistoryList({
            schedulingTargetQualificationRequirementId: schedulingTargetQualificationRequirementId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SchedulingTargetQualificationRequirementData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SchedulingTargetQualificationRequirementData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SchedulingTargetQualificationRequirementTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSchedulingTargetQualificationRequirement(raw: any): SchedulingTargetQualificationRequirementData {
    if (!raw) return raw;

    //
    // Create a SchedulingTargetQualificationRequirementData object instance with correct prototype
    //
    const revived = Object.create(SchedulingTargetQualificationRequirementData.prototype) as SchedulingTargetQualificationRequirementData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._schedulingTargetQualificationRequirementChangeHistories = null;
    (revived as any)._schedulingTargetQualificationRequirementChangeHistoriesPromise = null;
    (revived as any)._schedulingTargetQualificationRequirementChangeHistoriesSubject = new BehaviorSubject<SchedulingTargetQualificationRequirementChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSchedulingTargetQualificationRequirementXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SchedulingTargetQualificationRequirementChangeHistories$ = (revived as any)._schedulingTargetQualificationRequirementChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetQualificationRequirementChangeHistories === null && (revived as any)._schedulingTargetQualificationRequirementChangeHistoriesPromise === null) {
                (revived as any).loadSchedulingTargetQualificationRequirementChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._schedulingTargetQualificationRequirementChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SchedulingTargetQualificationRequirementData> | null>(null);

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

  private ReviveSchedulingTargetQualificationRequirementList(rawList: any[]): SchedulingTargetQualificationRequirementData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSchedulingTargetQualificationRequirement(raw));
  }

}
