/*

   GENERATED SERVICE FOR THE SCHEDULEDEVENTTEMPLATEQUALIFICATIONREQUIREMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ScheduledEventTemplateQualificationRequirement table.

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
import { ScheduledEventTemplateData } from './scheduled-event-template.service';
import { QualificationData } from './qualification.service';
import { ScheduledEventTemplateQualificationRequirementChangeHistoryService, ScheduledEventTemplateQualificationRequirementChangeHistoryData } from './scheduled-event-template-qualification-requirement-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ScheduledEventTemplateQualificationRequirementQueryParameters {
    scheduledEventTemplateId: bigint | number | null | undefined = null;
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
export class ScheduledEventTemplateQualificationRequirementSubmitData {
    id!: bigint | number;
    scheduledEventTemplateId!: bigint | number;
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

export class ScheduledEventTemplateQualificationRequirementBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ScheduledEventTemplateQualificationRequirementChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `scheduledEventTemplateQualificationRequirement.ScheduledEventTemplateQualificationRequirementChildren$` — use with `| async` in templates
//        • Promise:    `scheduledEventTemplateQualificationRequirement.ScheduledEventTemplateQualificationRequirementChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="scheduledEventTemplateQualificationRequirement.ScheduledEventTemplateQualificationRequirementChildren$ | async"`), or
//        • Access the promise getter (`scheduledEventTemplateQualificationRequirement.ScheduledEventTemplateQualificationRequirementChildren` or `await scheduledEventTemplateQualificationRequirement.ScheduledEventTemplateQualificationRequirementChildren`)
//    - Simply reading `scheduledEventTemplateQualificationRequirement.ScheduledEventTemplateQualificationRequirementChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await scheduledEventTemplateQualificationRequirement.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ScheduledEventTemplateQualificationRequirementData {
    id!: bigint | number;
    scheduledEventTemplateId!: bigint | number;
    qualificationId!: bigint | number;
    isRequired!: boolean;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    qualification: QualificationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduledEventTemplate: ScheduledEventTemplateData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _scheduledEventTemplateQualificationRequirementChangeHistories: ScheduledEventTemplateQualificationRequirementChangeHistoryData[] | null = null;
    private _scheduledEventTemplateQualificationRequirementChangeHistoriesPromise: Promise<ScheduledEventTemplateQualificationRequirementChangeHistoryData[]> | null  = null;
    private _scheduledEventTemplateQualificationRequirementChangeHistoriesSubject = new BehaviorSubject<ScheduledEventTemplateQualificationRequirementChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ScheduledEventTemplateQualificationRequirementData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ScheduledEventTemplateQualificationRequirementData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduledEventTemplateQualificationRequirementData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ScheduledEventTemplateQualificationRequirementChangeHistories$ = this._scheduledEventTemplateQualificationRequirementChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventTemplateQualificationRequirementChangeHistories === null && this._scheduledEventTemplateQualificationRequirementChangeHistoriesPromise === null) {
            this.loadScheduledEventTemplateQualificationRequirementChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _scheduledEventTemplateQualificationRequirementChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ScheduledEventTemplateQualificationRequirementChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._scheduledEventTemplateQualificationRequirementChangeHistoriesCount$ === null) {
            this._scheduledEventTemplateQualificationRequirementChangeHistoriesCount$ = ScheduledEventTemplateQualificationRequirementChangeHistoryService.Instance.GetScheduledEventTemplateQualificationRequirementChangeHistoriesRowCount({scheduledEventTemplateQualificationRequirementId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._scheduledEventTemplateQualificationRequirementChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ScheduledEventTemplateQualificationRequirementData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.scheduledEventTemplateQualificationRequirement.Reload();
  //
  //  Non Async:
  //
  //     scheduledEventTemplateQualificationRequirement[0].Reload().then(x => {
  //        this.scheduledEventTemplateQualificationRequirement = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ScheduledEventTemplateQualificationRequirementService.Instance.GetScheduledEventTemplateQualificationRequirement(this.id, includeRelations)
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
     this._scheduledEventTemplateQualificationRequirementChangeHistories = null;
     this._scheduledEventTemplateQualificationRequirementChangeHistoriesPromise = null;
     this._scheduledEventTemplateQualificationRequirementChangeHistoriesSubject.next(null);
     this._scheduledEventTemplateQualificationRequirementChangeHistoriesCount$ = null;

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
     * Gets the ScheduledEventTemplateQualificationRequirementChangeHistories for this ScheduledEventTemplateQualificationRequirement.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.scheduledEventTemplateQualificationRequirement.ScheduledEventTemplateQualificationRequirementChangeHistories.then(scheduledEventTemplateQualificationRequirements => { ... })
     *   or
     *   await this.scheduledEventTemplateQualificationRequirement.scheduledEventTemplateQualificationRequirements
     *
    */
    public get ScheduledEventTemplateQualificationRequirementChangeHistories(): Promise<ScheduledEventTemplateQualificationRequirementChangeHistoryData[]> {
        if (this._scheduledEventTemplateQualificationRequirementChangeHistories !== null) {
            return Promise.resolve(this._scheduledEventTemplateQualificationRequirementChangeHistories);
        }

        if (this._scheduledEventTemplateQualificationRequirementChangeHistoriesPromise !== null) {
            return this._scheduledEventTemplateQualificationRequirementChangeHistoriesPromise;
        }

        // Start the load
        this.loadScheduledEventTemplateQualificationRequirementChangeHistories();

        return this._scheduledEventTemplateQualificationRequirementChangeHistoriesPromise!;
    }



    private loadScheduledEventTemplateQualificationRequirementChangeHistories(): void {

        this._scheduledEventTemplateQualificationRequirementChangeHistoriesPromise = lastValueFrom(
            ScheduledEventTemplateQualificationRequirementService.Instance.GetScheduledEventTemplateQualificationRequirementChangeHistoriesForScheduledEventTemplateQualificationRequirement(this.id)
        )
        .then(ScheduledEventTemplateQualificationRequirementChangeHistories => {
            this._scheduledEventTemplateQualificationRequirementChangeHistories = ScheduledEventTemplateQualificationRequirementChangeHistories ?? [];
            this._scheduledEventTemplateQualificationRequirementChangeHistoriesSubject.next(this._scheduledEventTemplateQualificationRequirementChangeHistories);
            return this._scheduledEventTemplateQualificationRequirementChangeHistories;
         })
        .catch(err => {
            this._scheduledEventTemplateQualificationRequirementChangeHistories = [];
            this._scheduledEventTemplateQualificationRequirementChangeHistoriesSubject.next(this._scheduledEventTemplateQualificationRequirementChangeHistories);
            throw err;
        })
        .finally(() => {
            this._scheduledEventTemplateQualificationRequirementChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventTemplateQualificationRequirementChangeHistory. Call after mutations to force refresh.
     */
    public ClearScheduledEventTemplateQualificationRequirementChangeHistoriesCache(): void {
        this._scheduledEventTemplateQualificationRequirementChangeHistories = null;
        this._scheduledEventTemplateQualificationRequirementChangeHistoriesPromise = null;
        this._scheduledEventTemplateQualificationRequirementChangeHistoriesSubject.next(this._scheduledEventTemplateQualificationRequirementChangeHistories);      // Emit to observable
    }

    public get HasScheduledEventTemplateQualificationRequirementChangeHistories(): Promise<boolean> {
        return this.ScheduledEventTemplateQualificationRequirementChangeHistories.then(scheduledEventTemplateQualificationRequirementChangeHistories => scheduledEventTemplateQualificationRequirementChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (scheduledEventTemplateQualificationRequirement.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await scheduledEventTemplateQualificationRequirement.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ScheduledEventTemplateQualificationRequirementData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ScheduledEventTemplateQualificationRequirementData>> {
        const info = await lastValueFrom(
            ScheduledEventTemplateQualificationRequirementService.Instance.GetScheduledEventTemplateQualificationRequirementChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ScheduledEventTemplateQualificationRequirementData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ScheduledEventTemplateQualificationRequirementData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ScheduledEventTemplateQualificationRequirementSubmitData {
        return ScheduledEventTemplateQualificationRequirementService.Instance.ConvertToScheduledEventTemplateQualificationRequirementSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ScheduledEventTemplateQualificationRequirementService extends SecureEndpointBase {

    private static _instance: ScheduledEventTemplateQualificationRequirementService;
    private listCache: Map<string, Observable<Array<ScheduledEventTemplateQualificationRequirementData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ScheduledEventTemplateQualificationRequirementBasicListData>>>;
    private recordCache: Map<string, Observable<ScheduledEventTemplateQualificationRequirementData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private scheduledEventTemplateQualificationRequirementChangeHistoryService: ScheduledEventTemplateQualificationRequirementChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ScheduledEventTemplateQualificationRequirementData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ScheduledEventTemplateQualificationRequirementBasicListData>>>();
        this.recordCache = new Map<string, Observable<ScheduledEventTemplateQualificationRequirementData>>();

        ScheduledEventTemplateQualificationRequirementService._instance = this;
    }

    public static get Instance(): ScheduledEventTemplateQualificationRequirementService {
      return ScheduledEventTemplateQualificationRequirementService._instance;
    }


    public ClearListCaches(config: ScheduledEventTemplateQualificationRequirementQueryParameters | null = null) {

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


    public ConvertToScheduledEventTemplateQualificationRequirementSubmitData(data: ScheduledEventTemplateQualificationRequirementData): ScheduledEventTemplateQualificationRequirementSubmitData {

        let output = new ScheduledEventTemplateQualificationRequirementSubmitData();

        output.id = data.id;
        output.scheduledEventTemplateId = data.scheduledEventTemplateId;
        output.qualificationId = data.qualificationId;
        output.isRequired = data.isRequired;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetScheduledEventTemplateQualificationRequirement(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventTemplateQualificationRequirementData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const scheduledEventTemplateQualificationRequirement$ = this.requestScheduledEventTemplateQualificationRequirement(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateQualificationRequirement", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, scheduledEventTemplateQualificationRequirement$);

            return scheduledEventTemplateQualificationRequirement$;
        }

        return this.recordCache.get(configHash) as Observable<ScheduledEventTemplateQualificationRequirementData>;
    }

    private requestScheduledEventTemplateQualificationRequirement(id: bigint | number, includeRelations: boolean = true) : Observable<ScheduledEventTemplateQualificationRequirementData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventTemplateQualificationRequirementData>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirement/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveScheduledEventTemplateQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateQualificationRequirement(id, includeRelations));
            }));
    }

    public GetScheduledEventTemplateQualificationRequirementList(config: ScheduledEventTemplateQualificationRequirementQueryParameters | any = null) : Observable<Array<ScheduledEventTemplateQualificationRequirementData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const scheduledEventTemplateQualificationRequirementList$ = this.requestScheduledEventTemplateQualificationRequirementList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateQualificationRequirement list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, scheduledEventTemplateQualificationRequirementList$);

            return scheduledEventTemplateQualificationRequirementList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ScheduledEventTemplateQualificationRequirementData>>;
    }


    private requestScheduledEventTemplateQualificationRequirementList(config: ScheduledEventTemplateQualificationRequirementQueryParameters | any) : Observable <Array<ScheduledEventTemplateQualificationRequirementData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventTemplateQualificationRequirementData>>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirements', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveScheduledEventTemplateQualificationRequirementList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateQualificationRequirementList(config));
            }));
    }

    public GetScheduledEventTemplateQualificationRequirementsRowCount(config: ScheduledEventTemplateQualificationRequirementQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const scheduledEventTemplateQualificationRequirementsRowCount$ = this.requestScheduledEventTemplateQualificationRequirementsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateQualificationRequirements row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, scheduledEventTemplateQualificationRequirementsRowCount$);

            return scheduledEventTemplateQualificationRequirementsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestScheduledEventTemplateQualificationRequirementsRowCount(config: ScheduledEventTemplateQualificationRequirementQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirements/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateQualificationRequirementsRowCount(config));
            }));
    }

    public GetScheduledEventTemplateQualificationRequirementsBasicListData(config: ScheduledEventTemplateQualificationRequirementQueryParameters | any = null) : Observable<Array<ScheduledEventTemplateQualificationRequirementBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const scheduledEventTemplateQualificationRequirementsBasicListData$ = this.requestScheduledEventTemplateQualificationRequirementsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ScheduledEventTemplateQualificationRequirements basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, scheduledEventTemplateQualificationRequirementsBasicListData$);

            return scheduledEventTemplateQualificationRequirementsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ScheduledEventTemplateQualificationRequirementBasicListData>>;
    }


    private requestScheduledEventTemplateQualificationRequirementsBasicListData(config: ScheduledEventTemplateQualificationRequirementQueryParameters | any) : Observable<Array<ScheduledEventTemplateQualificationRequirementBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ScheduledEventTemplateQualificationRequirementBasicListData>>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirements/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestScheduledEventTemplateQualificationRequirementsBasicListData(config));
            }));

    }


    public PutScheduledEventTemplateQualificationRequirement(id: bigint | number, scheduledEventTemplateQualificationRequirement: ScheduledEventTemplateQualificationRequirementSubmitData) : Observable<ScheduledEventTemplateQualificationRequirementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventTemplateQualificationRequirementData>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirement/' + id.toString(), scheduledEventTemplateQualificationRequirement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplateQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutScheduledEventTemplateQualificationRequirement(id, scheduledEventTemplateQualificationRequirement));
            }));
    }


    public PostScheduledEventTemplateQualificationRequirement(scheduledEventTemplateQualificationRequirement: ScheduledEventTemplateQualificationRequirementSubmitData) : Observable<ScheduledEventTemplateQualificationRequirementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ScheduledEventTemplateQualificationRequirementData>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirement', scheduledEventTemplateQualificationRequirement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplateQualificationRequirement(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostScheduledEventTemplateQualificationRequirement(scheduledEventTemplateQualificationRequirement));
            }));
    }

  
    public DeleteScheduledEventTemplateQualificationRequirement(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirement/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteScheduledEventTemplateQualificationRequirement(id));
            }));
    }

    public RollbackScheduledEventTemplateQualificationRequirement(id: bigint | number, versionNumber: bigint | number) : Observable<ScheduledEventTemplateQualificationRequirementData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ScheduledEventTemplateQualificationRequirementData>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirement/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveScheduledEventTemplateQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackScheduledEventTemplateQualificationRequirement(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ScheduledEventTemplateQualificationRequirement.
     */
    public GetScheduledEventTemplateQualificationRequirementChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ScheduledEventTemplateQualificationRequirementData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduledEventTemplateQualificationRequirementData>>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirement/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventTemplateQualificationRequirementChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ScheduledEventTemplateQualificationRequirement.
     */
    public GetScheduledEventTemplateQualificationRequirementAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ScheduledEventTemplateQualificationRequirementData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ScheduledEventTemplateQualificationRequirementData>[]>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirement/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventTemplateQualificationRequirementAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ScheduledEventTemplateQualificationRequirement.
     */
    public GetScheduledEventTemplateQualificationRequirementVersion(id: bigint | number, version: number): Observable<ScheduledEventTemplateQualificationRequirementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventTemplateQualificationRequirementData>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirement/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduledEventTemplateQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventTemplateQualificationRequirementVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ScheduledEventTemplateQualificationRequirement at a specific point in time.
     */
    public GetScheduledEventTemplateQualificationRequirementStateAtTime(id: bigint | number, time: string): Observable<ScheduledEventTemplateQualificationRequirementData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ScheduledEventTemplateQualificationRequirementData>(this.baseUrl + 'api/ScheduledEventTemplateQualificationRequirement/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveScheduledEventTemplateQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetScheduledEventTemplateQualificationRequirementStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ScheduledEventTemplateQualificationRequirementQueryParameters | any): string {

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

    public userIsSchedulerScheduledEventTemplateQualificationRequirementReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerScheduledEventTemplateQualificationRequirementReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ScheduledEventTemplateQualificationRequirements
        //
        if (userIsSchedulerScheduledEventTemplateQualificationRequirementReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerScheduledEventTemplateQualificationRequirementReader = user.readPermission >= 1;
            } else {
                userIsSchedulerScheduledEventTemplateQualificationRequirementReader = false;
            }
        }

        return userIsSchedulerScheduledEventTemplateQualificationRequirementReader;
    }


    public userIsSchedulerScheduledEventTemplateQualificationRequirementWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerScheduledEventTemplateQualificationRequirementWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ScheduledEventTemplateQualificationRequirements
        //
        if (userIsSchedulerScheduledEventTemplateQualificationRequirementWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerScheduledEventTemplateQualificationRequirementWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerScheduledEventTemplateQualificationRequirementWriter = false;
          }      
        }

        return userIsSchedulerScheduledEventTemplateQualificationRequirementWriter;
    }

    public GetScheduledEventTemplateQualificationRequirementChangeHistoriesForScheduledEventTemplateQualificationRequirement(scheduledEventTemplateQualificationRequirementId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventTemplateQualificationRequirementChangeHistoryData[]> {
        return this.scheduledEventTemplateQualificationRequirementChangeHistoryService.GetScheduledEventTemplateQualificationRequirementChangeHistoryList({
            scheduledEventTemplateQualificationRequirementId: scheduledEventTemplateQualificationRequirementId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ScheduledEventTemplateQualificationRequirementData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ScheduledEventTemplateQualificationRequirementData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ScheduledEventTemplateQualificationRequirementTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveScheduledEventTemplateQualificationRequirement(raw: any): ScheduledEventTemplateQualificationRequirementData {
    if (!raw) return raw;

    //
    // Create a ScheduledEventTemplateQualificationRequirementData object instance with correct prototype
    //
    const revived = Object.create(ScheduledEventTemplateQualificationRequirementData.prototype) as ScheduledEventTemplateQualificationRequirementData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._scheduledEventTemplateQualificationRequirementChangeHistories = null;
    (revived as any)._scheduledEventTemplateQualificationRequirementChangeHistoriesPromise = null;
    (revived as any)._scheduledEventTemplateQualificationRequirementChangeHistoriesSubject = new BehaviorSubject<ScheduledEventTemplateQualificationRequirementChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadScheduledEventTemplateQualificationRequirementXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ScheduledEventTemplateQualificationRequirementChangeHistories$ = (revived as any)._scheduledEventTemplateQualificationRequirementChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventTemplateQualificationRequirementChangeHistories === null && (revived as any)._scheduledEventTemplateQualificationRequirementChangeHistoriesPromise === null) {
                (revived as any).loadScheduledEventTemplateQualificationRequirementChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._scheduledEventTemplateQualificationRequirementChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ScheduledEventTemplateQualificationRequirementData> | null>(null);

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

  private ReviveScheduledEventTemplateQualificationRequirementList(rawList: any[]): ScheduledEventTemplateQualificationRequirementData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveScheduledEventTemplateQualificationRequirement(raw));
  }

}
