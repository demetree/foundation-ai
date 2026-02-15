/*

   GENERATED SERVICE FOR THE RATESHEET TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the RateSheet table.

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
import { OfficeData } from './office.service';
import { AssignmentRoleData } from './assignment-role.service';
import { ResourceData } from './resource.service';
import { SchedulingTargetData } from './scheduling-target.service';
import { RateTypeData } from './rate-type.service';
import { CurrencyData } from './currency.service';
import { RateSheetChangeHistoryService, RateSheetChangeHistoryData } from './rate-sheet-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class RateSheetQueryParameters {
    officeId: bigint | number | null | undefined = null;
    assignmentRoleId: bigint | number | null | undefined = null;
    resourceId: bigint | number | null | undefined = null;
    schedulingTargetId: bigint | number | null | undefined = null;
    rateTypeId: bigint | number | null | undefined = null;
    effectiveDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    currencyId: bigint | number | null | undefined = null;
    costRate: number | null | undefined = null;
    billingRate: number | null | undefined = null;
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
export class RateSheetSubmitData {
    id!: bigint | number;
    officeId: bigint | number | null = null;
    assignmentRoleId: bigint | number | null = null;
    resourceId: bigint | number | null = null;
    schedulingTargetId: bigint | number | null = null;
    rateTypeId!: bigint | number;
    effectiveDate!: string;      // ISO 8601 (full datetime)
    currencyId!: bigint | number;
    costRate!: number;
    billingRate!: number;
    notes: string | null = null;
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

export class RateSheetBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. RateSheetChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `rateSheet.RateSheetChildren$` — use with `| async` in templates
//        • Promise:    `rateSheet.RateSheetChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="rateSheet.RateSheetChildren$ | async"`), or
//        • Access the promise getter (`rateSheet.RateSheetChildren` or `await rateSheet.RateSheetChildren`)
//    - Simply reading `rateSheet.RateSheetChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await rateSheet.Reload()` to refresh the entire object and clear all lazy caches.
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
export class RateSheetData {
    id!: bigint | number;
    officeId!: bigint | number;
    assignmentRoleId!: bigint | number;
    resourceId!: bigint | number;
    schedulingTargetId!: bigint | number;
    rateTypeId!: bigint | number;
    effectiveDate!: string;      // ISO 8601 (full datetime)
    currencyId!: bigint | number;
    costRate!: number;
    billingRate!: number;
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    assignmentRole: AssignmentRoleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    currency: CurrencyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    office: OfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    rateType: RateTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    schedulingTarget: SchedulingTargetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _rateSheetChangeHistories: RateSheetChangeHistoryData[] | null = null;
    private _rateSheetChangeHistoriesPromise: Promise<RateSheetChangeHistoryData[]> | null  = null;
    private _rateSheetChangeHistoriesSubject = new BehaviorSubject<RateSheetChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<RateSheetData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<RateSheetData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<RateSheetData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public RateSheetChangeHistories$ = this._rateSheetChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._rateSheetChangeHistories === null && this._rateSheetChangeHistoriesPromise === null) {
            this.loadRateSheetChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public RateSheetChangeHistoriesCount$ = RateSheetChangeHistoryService.Instance.GetRateSheetChangeHistoriesRowCount({rateSheetId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any RateSheetData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.rateSheet.Reload();
  //
  //  Non Async:
  //
  //     rateSheet[0].Reload().then(x => {
  //        this.rateSheet = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      RateSheetService.Instance.GetRateSheet(this.id, includeRelations)
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
     this._rateSheetChangeHistories = null;
     this._rateSheetChangeHistoriesPromise = null;
     this._rateSheetChangeHistoriesSubject.next(null);

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
     * Gets the RateSheetChangeHistories for this RateSheet.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.rateSheet.RateSheetChangeHistories.then(rateSheets => { ... })
     *   or
     *   await this.rateSheet.rateSheets
     *
    */
    public get RateSheetChangeHistories(): Promise<RateSheetChangeHistoryData[]> {
        if (this._rateSheetChangeHistories !== null) {
            return Promise.resolve(this._rateSheetChangeHistories);
        }

        if (this._rateSheetChangeHistoriesPromise !== null) {
            return this._rateSheetChangeHistoriesPromise;
        }

        // Start the load
        this.loadRateSheetChangeHistories();

        return this._rateSheetChangeHistoriesPromise!;
    }



    private loadRateSheetChangeHistories(): void {

        this._rateSheetChangeHistoriesPromise = lastValueFrom(
            RateSheetService.Instance.GetRateSheetChangeHistoriesForRateSheet(this.id)
        )
        .then(RateSheetChangeHistories => {
            this._rateSheetChangeHistories = RateSheetChangeHistories ?? [];
            this._rateSheetChangeHistoriesSubject.next(this._rateSheetChangeHistories);
            return this._rateSheetChangeHistories;
         })
        .catch(err => {
            this._rateSheetChangeHistories = [];
            this._rateSheetChangeHistoriesSubject.next(this._rateSheetChangeHistories);
            throw err;
        })
        .finally(() => {
            this._rateSheetChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached RateSheetChangeHistory. Call after mutations to force refresh.
     */
    public ClearRateSheetChangeHistoriesCache(): void {
        this._rateSheetChangeHistories = null;
        this._rateSheetChangeHistoriesPromise = null;
        this._rateSheetChangeHistoriesSubject.next(this._rateSheetChangeHistories);      // Emit to observable
    }

    public get HasRateSheetChangeHistories(): Promise<boolean> {
        return this.RateSheetChangeHistories.then(rateSheetChangeHistories => rateSheetChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (rateSheet.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await rateSheet.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<RateSheetData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<RateSheetData>> {
        const info = await lastValueFrom(
            RateSheetService.Instance.GetRateSheetChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this RateSheetData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this RateSheetData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): RateSheetSubmitData {
        return RateSheetService.Instance.ConvertToRateSheetSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class RateSheetService extends SecureEndpointBase {

    private static _instance: RateSheetService;
    private listCache: Map<string, Observable<Array<RateSheetData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<RateSheetBasicListData>>>;
    private recordCache: Map<string, Observable<RateSheetData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private rateSheetChangeHistoryService: RateSheetChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<RateSheetData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<RateSheetBasicListData>>>();
        this.recordCache = new Map<string, Observable<RateSheetData>>();

        RateSheetService._instance = this;
    }

    public static get Instance(): RateSheetService {
      return RateSheetService._instance;
    }


    public ClearListCaches(config: RateSheetQueryParameters | null = null) {

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


    public ConvertToRateSheetSubmitData(data: RateSheetData): RateSheetSubmitData {

        let output = new RateSheetSubmitData();

        output.id = data.id;
        output.officeId = data.officeId;
        output.assignmentRoleId = data.assignmentRoleId;
        output.resourceId = data.resourceId;
        output.schedulingTargetId = data.schedulingTargetId;
        output.rateTypeId = data.rateTypeId;
        output.effectiveDate = data.effectiveDate;
        output.currencyId = data.currencyId;
        output.costRate = data.costRate;
        output.billingRate = data.billingRate;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetRateSheet(id: bigint | number, includeRelations: boolean = true) : Observable<RateSheetData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const rateSheet$ = this.requestRateSheet(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RateSheet", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, rateSheet$);

            return rateSheet$;
        }

        return this.recordCache.get(configHash) as Observable<RateSheetData>;
    }

    private requestRateSheet(id: bigint | number, includeRelations: boolean = true) : Observable<RateSheetData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RateSheetData>(this.baseUrl + 'api/RateSheet/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveRateSheet(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestRateSheet(id, includeRelations));
            }));
    }

    public GetRateSheetList(config: RateSheetQueryParameters | any = null) : Observable<Array<RateSheetData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const rateSheetList$ = this.requestRateSheetList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RateSheet list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, rateSheetList$);

            return rateSheetList$;
        }

        return this.listCache.get(configHash) as Observable<Array<RateSheetData>>;
    }


    private requestRateSheetList(config: RateSheetQueryParameters | any) : Observable <Array<RateSheetData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RateSheetData>>(this.baseUrl + 'api/RateSheets', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveRateSheetList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestRateSheetList(config));
            }));
    }

    public GetRateSheetsRowCount(config: RateSheetQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const rateSheetsRowCount$ = this.requestRateSheetsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RateSheets row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, rateSheetsRowCount$);

            return rateSheetsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestRateSheetsRowCount(config: RateSheetQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/RateSheets/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRateSheetsRowCount(config));
            }));
    }

    public GetRateSheetsBasicListData(config: RateSheetQueryParameters | any = null) : Observable<Array<RateSheetBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const rateSheetsBasicListData$ = this.requestRateSheetsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RateSheets basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, rateSheetsBasicListData$);

            return rateSheetsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<RateSheetBasicListData>>;
    }


    private requestRateSheetsBasicListData(config: RateSheetQueryParameters | any) : Observable<Array<RateSheetBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RateSheetBasicListData>>(this.baseUrl + 'api/RateSheets/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRateSheetsBasicListData(config));
            }));

    }


    public PutRateSheet(id: bigint | number, rateSheet: RateSheetSubmitData) : Observable<RateSheetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RateSheetData>(this.baseUrl + 'api/RateSheet/' + id.toString(), rateSheet, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRateSheet(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutRateSheet(id, rateSheet));
            }));
    }


    public PostRateSheet(rateSheet: RateSheetSubmitData) : Observable<RateSheetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<RateSheetData>(this.baseUrl + 'api/RateSheet', rateSheet, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRateSheet(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostRateSheet(rateSheet));
            }));
    }

  
    public DeleteRateSheet(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/RateSheet/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteRateSheet(id));
            }));
    }

    public RollbackRateSheet(id: bigint | number, versionNumber: bigint | number) : Observable<RateSheetData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RateSheetData>(this.baseUrl + 'api/RateSheet/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRateSheet(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackRateSheet(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a RateSheet.
     */
    public GetRateSheetChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<RateSheetData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<RateSheetData>>(this.baseUrl + 'api/RateSheet/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetRateSheetChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a RateSheet.
     */
    public GetRateSheetAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<RateSheetData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<RateSheetData>[]>(this.baseUrl + 'api/RateSheet/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetRateSheetAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a RateSheet.
     */
    public GetRateSheetVersion(id: bigint | number, version: number): Observable<RateSheetData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RateSheetData>(this.baseUrl + 'api/RateSheet/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveRateSheet(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetRateSheetVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a RateSheet at a specific point in time.
     */
    public GetRateSheetStateAtTime(id: bigint | number, time: string): Observable<RateSheetData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RateSheetData>(this.baseUrl + 'api/RateSheet/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveRateSheet(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetRateSheetStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: RateSheetQueryParameters | any): string {

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

    public userIsSchedulerRateSheetReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerRateSheetReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.RateSheets
        //
        if (userIsSchedulerRateSheetReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerRateSheetReader = user.readPermission >= 1;
            } else {
                userIsSchedulerRateSheetReader = false;
            }
        }

        return userIsSchedulerRateSheetReader;
    }


    public userIsSchedulerRateSheetWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerRateSheetWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.RateSheets
        //
        if (userIsSchedulerRateSheetWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerRateSheetWriter = user.writePermission >= 30;
          } else {
            userIsSchedulerRateSheetWriter = false;
          }      
        }

        return userIsSchedulerRateSheetWriter;
    }

    public GetRateSheetChangeHistoriesForRateSheet(rateSheetId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<RateSheetChangeHistoryData[]> {
        return this.rateSheetChangeHistoryService.GetRateSheetChangeHistoryList({
            rateSheetId: rateSheetId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full RateSheetData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the RateSheetData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when RateSheetTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveRateSheet(raw: any): RateSheetData {
    if (!raw) return raw;

    //
    // Create a RateSheetData object instance with correct prototype
    //
    const revived = Object.create(RateSheetData.prototype) as RateSheetData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._rateSheetChangeHistories = null;
    (revived as any)._rateSheetChangeHistoriesPromise = null;
    (revived as any)._rateSheetChangeHistoriesSubject = new BehaviorSubject<RateSheetChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadRateSheetXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).RateSheetChangeHistories$ = (revived as any)._rateSheetChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._rateSheetChangeHistories === null && (revived as any)._rateSheetChangeHistoriesPromise === null) {
                (revived as any).loadRateSheetChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).RateSheetChangeHistoriesCount$ = RateSheetChangeHistoryService.Instance.GetRateSheetChangeHistoriesRowCount({rateSheetId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<RateSheetData> | null>(null);

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

  private ReviveRateSheetList(rawList: any[]): RateSheetData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveRateSheet(raw));
  }

}
