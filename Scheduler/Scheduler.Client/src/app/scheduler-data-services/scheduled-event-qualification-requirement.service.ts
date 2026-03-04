/*

   GENERATED SERVICE FOR THE SCHEDULEDEVENTQUALIFICATIONREQUIREMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ScheduledEventQualificationRequirement table.

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
import { ScheduledEventData } from './scheduled-event.service';
import { QualificationData } from './qualification.service';
import { ScheduledEventQualificationRequirementChangeHistoryService, ScheduledEventQualificationRequirementChangeHistoryData } from './scheduled-event-qualification-requirement-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ScheduledEventQualificationRequirementQueryParameters {
    scheduledEventId: bigint | number | null | undefined = null;
    qualificationId: bigint | number | null | undefined = null;
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
export class ScheduledEventQualificationRequirementSubmitData {
    id!: bigint | number;
    scheduledEventId!: bigint | number;
    qualificationId!: bigint | number;
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

export class ScheduledEventQualificationRequirementBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ScheduledEventQualificationRequirementChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `scheduledEventQualificationRequirement.ScheduledEventQualificationRequirementChildren$` — use with `| async` in templates
//        • Promise:    `scheduledEventQualificationRequirement.ScheduledEventQualificationRequirementChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="scheduledEventQualificationRequirement.ScheduledEventQualificationRequirementChildren$ | async"`), or
//        • Access the promise getter (`scheduledEventQualificationRequirement.ScheduledEventQualificationRequirementChildren` or `await scheduledEventQualificationRequirement.ScheduledEventQualificationRequirementChildren`)
//    - Simply reading `scheduledEventQualificationRequirement.ScheduledEventQualificationRequirementChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await scheduledEventQualificationRequirement.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ScheduledEventQualificationRequirementData {
    id!: bigint | number;
    scheduledEventId!: bigint | number;
    qualificationId!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    qualification: QualificationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduledEvent: ScheduledEventData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _scheduledEventQualificationRequirementChangeHistories: ScheduledEventQualificationRequirementChangeHistoryData[] | null = null;
    private _scheduledEventQualificationRequirementChangeHistoriesPromise: Promise<ScheduledEventQualificationRequirementChangeHistoryData[]> | null  = null;
    private _scheduledEventQualificationRequirementChangeHistoriesSubject = new BehaviorSubject<ScheduledEventQualificationRequirementChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ScheduledEventQualificationRequirementData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ScheduledEventQualificationRequirementData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduledEventQualificationRequirementData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ScheduledEventQualificationRequirementChangeHistories$ = this._scheduledEventQualificationRequirementChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventQualificationRequirementChangeHistories === null && this._scheduledEventQualificationRequirementChangeHistoriesPromise === null) {
            this.loadScheduledEventQualificationRequirementChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _scheduledEventQualificationRequirementChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ScheduledEventQualificationRequirementChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._scheduledEventQualificationRequirementChangeHistoriesCount$ === null) {
            this._scheduledEventQualificationRequirementChangeHistoriesCount$ = ScheduledEventQualificationRequirementChangeHistoryService.Instance.GetScheduledEventQualificationRequirementChangeHistoriesRowCount({scheduledEventQualificationRequirementId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._scheduledEventQualificationRequirementChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ScheduledEventQualificationRequirementData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.scheduledEventQualificationRequirement.Reload();
  //
  //  Non Async:
  //
  //     scheduledEventQualificationRequirement[0].Reload().then(x => {
  //        this.scheduledEventQualificationRequirement = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ScheduledEventQualificationRequirementService.Instance.GetScheduledEventQualificationRequirement(this.id, includeRelations)
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
     this._scheduledEventQualificationRequirementChangeHistories = null;
     this._scheduledEventQualificationRequirementChangeHistoriesPromise = null;
     this._scheduledEventQualificationRequirementChangeHistoriesSubject.next(null);
     this._scheduledEventQualificationRequirementChangeHistoriesCount$ = null;

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
     * Gets the ScheduledEventQualificationRequirementChangeHistories for this ScheduledEventQualificationRequirement.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEventQualificationRequirement.ScheduledEventQualificationRequirementChangeHistories.then(scheduledEventQualificationRequirements => { ... })
     *   or
     *   await this.scheduledEventQualificationRequirement.scheduledEventQualificationRequirements
     *
    */
    public get ScheduledEventQualificationRequirementChangeHistories(): Promise<ScheduledEventQualificationRequirementChangeHistoryData[]> {
        if (this._scheduledEventQualificationRequirementChangeHistories !== null) {
            return Promise.resolve(this._scheduledEventQualificationRequirementChangeHistories);
        }

        if (this._scheduledEventQualificationRequirementChangeHistoriesPromise !== null) {
            return this._scheduledEventQualificationRequirementChangeHistoriesPromise;
        }

        // Start the load
        this.loadScheduledEventQualificationRequirementChangeHistories();

        return this._scheduledEventQualificationRequirementChangeHistoriesPromise!;
    }



    private loadScheduledEventQualificationRequirementChangeHistories(): void {

        this._scheduledEventQualificationRequirementChangeHistoriesPromise = lastValueFrom(
            ScheduledEventQualificationRequirementService.Instance.GetScheduledEventQualificationRequirementChangeHistoriesForScheduledEventQualificationRequirement(this.id)
        )
        .then(ScheduledEventQualificationRequirementChangeHistories => {
            this._scheduledEventQualificationRequirementChangeHistories = ScheduledEventQualificationRequirementChangeHistories ?? [];
            this._scheduledEventQualificationRequirementChangeHistoriesSubject.next(this._scheduledEventQualificationRequirementChangeHistories);
            return this._scheduledEventQualificationRequirementChangeHistories;
         })
        .catch(err => {
            this._scheduledEventQualificationRequirementChangeHistories = [];
            this._scheduledEventQualificationRequirementChangeHistoriesSubject.next(this._scheduledEventQualificationRequirementChangeHistories);
            throw err;
        })
        .finally(() => {
            this._scheduledEventQualificationRequirementChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventQualificationRequirementChangeHistory. Call after mutations to force refresh.
     */
    public ClearScheduledEventQualificationRequirementChangeHistoriesCache(): void {
        this._scheduledEventQualificationRequirementChangeHistories = null;
        this._scheduledEventQualificationRequirementChangeHistoriesPromise = null;
        this._scheduledEventQualificationRequirementChangeHistoriesSubject.next(this._scheduledEventQualificationRequirementChangeHistories);      // Emit to observable
    }

    public get HasScheduledEventQualificationRequirementChangeHistories(): Promise<boolean> {
        return this.ScheduledEventQualificationRequirementChangeHistories.then(scheduledEventQualificationRequirementChangeHistories => scheduledEventQualificationRequirementChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (scheduledEventQualificationRequirement.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await scheduledEventQualificationRequirement.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ScheduledEventQualificationRequirementData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ScheduledEventQualificationRequirementData>> {
        const info = await lastValueFrom(
            ScheduledEventQualificationRequirementService.Instance.GetScheduledEventQualificationRequirementChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ScheduledEventQualificationRequirementData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ScheduledEventQualificationRequirementData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ScheduledEventQualificationRequirementSubmitData {
        return ScheduledEventQualificationRequirementService.Instance.ConvertToScheduledEventQualificationRequirementSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ScheduledEventQualificationRequirementService extends SecureEndpointBase {

    private static _instance: ScheduledEventQualificationRequirementService;
    private listCache: Map<string, Observable<Array<ScheduledEventQualificationRequirementData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ScheduledEventQualificationRequirementBasicListData>>>;
    private recordCache: Map<string, Observable<ScheduledEventQualificationRequirementData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private scheduledEventQualificationRequirementChangeHistoryService: ScheduledEventQualificationRequirementChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ScheduledEventQualificationRequirementData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ScheduledEventQualificationRequirementBasicListData>>>();
        this.recordCache = new Map<string, Observable<ScheduledEventQualificationRequirementData>>();

        ScheduledEventQualificationRequirementService._instance = this;
    }

    public static get Instance(): ScheduledEventQualificationRequirementService {
      return ScheduledEventQualificationRequirementService._instance;
    }


    public ClearListCaches(config: ScheduledEventQualificationRequirementQueryParameters | null = null) {

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


    public ConvertToScheduledEventQualificationRequirementSubmitData(data: ScheduledEventQualificationRequirementData): ScheduledEventQualificationRequirementSubmitData {

        let output = new ScheduledEventQualificationRequirementSubmitData();

        output.id = data.id;
        output.scheduledEventId = data.scheduledEventId;
        output.qualificationId = data.qualificationId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetScheduledEventQualificationRequirement(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventQualificationRequirementData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const scheduledEventQualificationRequirement$ = this.requestScheduledEventQualificationRequirement(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventQualificationRequirement", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, scheduledEventQualificationRequirement$);

            return scheduledEventQualificationRequirement$;
        }

        return this.recordCache.get(configHash) as Observable<ScheduledEventQualificationRequirementData>;
    }

    private requestScheduledEventQualificationRequirement(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventQualificationRequirementData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventQualificationRequirementData>(this.baseUrl + 'api/ScheduledEventQualificationRequirement/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveScheduledEventQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventQualificationRequirement(id, includeRelations));
            }));
    }

    public GetScheduledEventQualificationRequirementList(config: ScheduledEventQualificationRequirementQueryParameters | any = null) : Observable<Array<ScheduledEventQualificationRequirementData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const scheduledEventQualificationRequirementList$ = this.requestScheduledEventQualificationRequirementList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventQualificationRequirement list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, scheduledEventQualificationRequirementList$);

            return scheduledEventQualificationRequirementList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ScheduledEventQualificationRequirementData>>;
    }


    private requestScheduledEventQualificationRequirementList(config: ScheduledEventQualificationRequirementQueryParameters | any) : Observable <Array<ScheduledEventQualificationRequirementData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventQualificationRequirementData>>(this.baseUrl + 'api/ScheduledEventQualificationRequirements', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveScheduledEventQualificationRequirementList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventQualificationRequirementList(config));
            }));
    }

    public GetScheduledEventQualificationRequirementsRowCount(config: ScheduledEventQualificationRequirementQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const scheduledEventQualificationRequirementsRowCount$ = this.requestScheduledEventQualificationRequirementsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventQualificationRequirements row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, scheduledEventQualificationRequirementsRowCount$);

            return scheduledEventQualificationRequirementsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestScheduledEventQualificationRequirementsRowCount(config: ScheduledEventQualificationRequirementQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ScheduledEventQualificationRequirements/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventQualificationRequirementsRowCount(config));
            }));
    }

    public GetScheduledEventQualificationRequirementsBasicListData(config: ScheduledEventQualificationRequirementQueryParameters | any = null) : Observable<Array<ScheduledEventQualificationRequirementBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const scheduledEventQualificationRequirementsBasicListData$ = this.requestScheduledEventQualificationRequirementsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventQualificationRequirements basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, scheduledEventQualificationRequirementsBasicListData$);

            return scheduledEventQualificationRequirementsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ScheduledEventQualificationRequirementBasicListData>>;
    }


    private requestScheduledEventQualificationRequirementsBasicListData(config: ScheduledEventQualificationRequirementQueryParameters | any) : Observable<Array<ScheduledEventQualificationRequirementBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventQualificationRequirementBasicListData>>(this.baseUrl + 'api/ScheduledEventQualificationRequirements/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventQualificationRequirementsBasicListData(config));
            }));

    }


    public PutScheduledEventQualificationRequirement(id: bigint | number, scheduledEventQualificationRequirement: ScheduledEventQualificationRequirementSubmitData) : Observable<ScheduledEventQualificationRequirementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventQualificationRequirementData>(this.baseUrl + 'api/ScheduledEventQualificationRequirement/' + id.toString(), scheduledEventQualificationRequirement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutScheduledEventQualificationRequirement(id, scheduledEventQualificationRequirement));
            }));
    }


    public PostScheduledEventQualificationRequirement(scheduledEventQualificationRequirement: ScheduledEventQualificationRequirementSubmitData) : Observable<ScheduledEventQualificationRequirementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ScheduledEventQualificationRequirementData>(this.baseUrl + 'api/ScheduledEventQualificationRequirement', scheduledEventQualificationRequirement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventQualificationRequirement(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostScheduledEventQualificationRequirement(scheduledEventQualificationRequirement));
            }));
    }

  
    public DeleteScheduledEventQualificationRequirement(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ScheduledEventQualificationRequirement/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteScheduledEventQualificationRequirement(id));
            }));
    }

    public RollbackScheduledEventQualificationRequirement(id: bigint | number, versionNumber: bigint | number) : Observable<ScheduledEventQualificationRequirementData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventQualificationRequirementData>(this.baseUrl + 'api/ScheduledEventQualificationRequirement/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackScheduledEventQualificationRequirement(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ScheduledEventQualificationRequirement.
     */
    public GetScheduledEventQualificationRequirementChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ScheduledEventQualificationRequirementData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduledEventQualificationRequirementData>>(this.baseUrl + 'api/ScheduledEventQualificationRequirement/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventQualificationRequirementChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ScheduledEventQualificationRequirement.
     */
    public GetScheduledEventQualificationRequirementAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ScheduledEventQualificationRequirementData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduledEventQualificationRequirementData>[]>(this.baseUrl + 'api/ScheduledEventQualificationRequirement/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventQualificationRequirementAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ScheduledEventQualificationRequirement.
     */
    public GetScheduledEventQualificationRequirementVersion(id: bigint | number, version: number): Observable<ScheduledEventQualificationRequirementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventQualificationRequirementData>(this.baseUrl + 'api/ScheduledEventQualificationRequirement/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduledEventQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventQualificationRequirementVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ScheduledEventQualificationRequirement at a specific point in time.
     */
    public GetScheduledEventQualificationRequirementStateAtTime(id: bigint | number, time: string): Observable<ScheduledEventQualificationRequirementData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventQualificationRequirementData>(this.baseUrl + 'api/ScheduledEventQualificationRequirement/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduledEventQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventQualificationRequirementStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ScheduledEventQualificationRequirementQueryParameters | any): string {

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

    public userIsSchedulerScheduledEventQualificationRequirementReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerScheduledEventQualificationRequirementReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ScheduledEventQualificationRequirements
        //
        if (userIsSchedulerScheduledEventQualificationRequirementReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerScheduledEventQualificationRequirementReader = user.readPermission >= 1;
            } else {
                userIsSchedulerScheduledEventQualificationRequirementReader = false;
            }
        }

        return userIsSchedulerScheduledEventQualificationRequirementReader;
    }


    public userIsSchedulerScheduledEventQualificationRequirementWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerScheduledEventQualificationRequirementWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ScheduledEventQualificationRequirements
        //
        if (userIsSchedulerScheduledEventQualificationRequirementWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerScheduledEventQualificationRequirementWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerScheduledEventQualificationRequirementWriter = false;
          }      
        }

        return userIsSchedulerScheduledEventQualificationRequirementWriter;
    }

    public GetScheduledEventQualificationRequirementChangeHistoriesForScheduledEventQualificationRequirement(scheduledEventQualificationRequirementId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventQualificationRequirementChangeHistoryData[]> {
        return this.scheduledEventQualificationRequirementChangeHistoryService.GetScheduledEventQualificationRequirementChangeHistoryList({
            scheduledEventQualificationRequirementId: scheduledEventQualificationRequirementId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ScheduledEventQualificationRequirementData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ScheduledEventQualificationRequirementData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ScheduledEventQualificationRequirementTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveScheduledEventQualificationRequirement(raw: any): ScheduledEventQualificationRequirementData {
    if (!raw) return raw;

    //
    // Create a ScheduledEventQualificationRequirementData object instance with correct prototype
    //
    const revived = Object.create(ScheduledEventQualificationRequirementData.prototype) as ScheduledEventQualificationRequirementData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._scheduledEventQualificationRequirementChangeHistories = null;
    (revived as any)._scheduledEventQualificationRequirementChangeHistoriesPromise = null;
    (revived as any)._scheduledEventQualificationRequirementChangeHistoriesSubject = new BehaviorSubject<ScheduledEventQualificationRequirementChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadScheduledEventQualificationRequirementXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ScheduledEventQualificationRequirementChangeHistories$ = (revived as any)._scheduledEventQualificationRequirementChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventQualificationRequirementChangeHistories === null && (revived as any)._scheduledEventQualificationRequirementChangeHistoriesPromise === null) {
                (revived as any).loadScheduledEventQualificationRequirementChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._scheduledEventQualificationRequirementChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduledEventQualificationRequirementData> | null>(null);

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

  private ReviveScheduledEventQualificationRequirementList(rawList: any[]): ScheduledEventQualificationRequirementData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveScheduledEventQualificationRequirement(raw));
  }

}
