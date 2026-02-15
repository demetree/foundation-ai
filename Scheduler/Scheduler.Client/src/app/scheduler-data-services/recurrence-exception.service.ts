/*

   GENERATED SERVICE FOR THE RECURRENCEEXCEPTION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the RecurrenceException table.

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
import { RecurrenceExceptionChangeHistoryService, RecurrenceExceptionChangeHistoryData } from './recurrence-exception-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class RecurrenceExceptionQueryParameters {
    scheduledEventId: bigint | number | null | undefined = null;
    exceptionDateTime: string | null | undefined = null;        // ISO 8601 (full datetime)
    movedToDateTime: string | null | undefined = null;        // ISO 8601 (full datetime)
    reason: string | null | undefined = null;
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
export class RecurrenceExceptionSubmitData {
    id!: bigint | number;
    scheduledEventId!: bigint | number;
    exceptionDateTime!: string;      // ISO 8601 (full datetime)
    movedToDateTime: string | null = null;     // ISO 8601 (full datetime)
    reason: string | null = null;
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

export class RecurrenceExceptionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. RecurrenceExceptionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `recurrenceException.RecurrenceExceptionChildren$` — use with `| async` in templates
//        • Promise:    `recurrenceException.RecurrenceExceptionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="recurrenceException.RecurrenceExceptionChildren$ | async"`), or
//        • Access the promise getter (`recurrenceException.RecurrenceExceptionChildren` or `await recurrenceException.RecurrenceExceptionChildren`)
//    - Simply reading `recurrenceException.RecurrenceExceptionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await recurrenceException.Reload()` to refresh the entire object and clear all lazy caches.
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
export class RecurrenceExceptionData {
    id!: bigint | number;
    scheduledEventId!: bigint | number;
    exceptionDateTime!: string;      // ISO 8601 (full datetime)
    movedToDateTime!: string | null;   // ISO 8601 (full datetime)
    reason!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    scheduledEvent: ScheduledEventData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _recurrenceExceptionChangeHistories: RecurrenceExceptionChangeHistoryData[] | null = null;
    private _recurrenceExceptionChangeHistoriesPromise: Promise<RecurrenceExceptionChangeHistoryData[]> | null  = null;
    private _recurrenceExceptionChangeHistoriesSubject = new BehaviorSubject<RecurrenceExceptionChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<RecurrenceExceptionData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<RecurrenceExceptionData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<RecurrenceExceptionData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public RecurrenceExceptionChangeHistories$ = this._recurrenceExceptionChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._recurrenceExceptionChangeHistories === null && this._recurrenceExceptionChangeHistoriesPromise === null) {
            this.loadRecurrenceExceptionChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public RecurrenceExceptionChangeHistoriesCount$ = RecurrenceExceptionChangeHistoryService.Instance.GetRecurrenceExceptionChangeHistoriesRowCount({recurrenceExceptionId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any RecurrenceExceptionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.recurrenceException.Reload();
  //
  //  Non Async:
  //
  //     recurrenceException[0].Reload().then(x => {
  //        this.recurrenceException = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      RecurrenceExceptionService.Instance.GetRecurrenceException(this.id, includeRelations)
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
     this._recurrenceExceptionChangeHistories = null;
     this._recurrenceExceptionChangeHistoriesPromise = null;
     this._recurrenceExceptionChangeHistoriesSubject.next(null);

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
     * Gets the RecurrenceExceptionChangeHistories for this RecurrenceException.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.recurrenceException.RecurrenceExceptionChangeHistories.then(recurrenceExceptions => { ... })
     *   or
     *   await this.recurrenceException.recurrenceExceptions
     *
    */
    public get RecurrenceExceptionChangeHistories(): Promise<RecurrenceExceptionChangeHistoryData[]> {
        if (this._recurrenceExceptionChangeHistories !== null) {
            return Promise.resolve(this._recurrenceExceptionChangeHistories);
        }

        if (this._recurrenceExceptionChangeHistoriesPromise !== null) {
            return this._recurrenceExceptionChangeHistoriesPromise;
        }

        // Start the load
        this.loadRecurrenceExceptionChangeHistories();

        return this._recurrenceExceptionChangeHistoriesPromise!;
    }



    private loadRecurrenceExceptionChangeHistories(): void {

        this._recurrenceExceptionChangeHistoriesPromise = lastValueFrom(
            RecurrenceExceptionService.Instance.GetRecurrenceExceptionChangeHistoriesForRecurrenceException(this.id)
        )
        .then(RecurrenceExceptionChangeHistories => {
            this._recurrenceExceptionChangeHistories = RecurrenceExceptionChangeHistories ?? [];
            this._recurrenceExceptionChangeHistoriesSubject.next(this._recurrenceExceptionChangeHistories);
            return this._recurrenceExceptionChangeHistories;
         })
        .catch(err => {
            this._recurrenceExceptionChangeHistories = [];
            this._recurrenceExceptionChangeHistoriesSubject.next(this._recurrenceExceptionChangeHistories);
            throw err;
        })
        .finally(() => {
            this._recurrenceExceptionChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached RecurrenceExceptionChangeHistory. Call after mutations to force refresh.
     */
    public ClearRecurrenceExceptionChangeHistoriesCache(): void {
        this._recurrenceExceptionChangeHistories = null;
        this._recurrenceExceptionChangeHistoriesPromise = null;
        this._recurrenceExceptionChangeHistoriesSubject.next(this._recurrenceExceptionChangeHistories);      // Emit to observable
    }

    public get HasRecurrenceExceptionChangeHistories(): Promise<boolean> {
        return this.RecurrenceExceptionChangeHistories.then(recurrenceExceptionChangeHistories => recurrenceExceptionChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (recurrenceException.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await recurrenceException.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<RecurrenceExceptionData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<RecurrenceExceptionData>> {
        const info = await lastValueFrom(
            RecurrenceExceptionService.Instance.GetRecurrenceExceptionChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this RecurrenceExceptionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this RecurrenceExceptionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): RecurrenceExceptionSubmitData {
        return RecurrenceExceptionService.Instance.ConvertToRecurrenceExceptionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class RecurrenceExceptionService extends SecureEndpointBase {

    private static _instance: RecurrenceExceptionService;
    private listCache: Map<string, Observable<Array<RecurrenceExceptionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<RecurrenceExceptionBasicListData>>>;
    private recordCache: Map<string, Observable<RecurrenceExceptionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private recurrenceExceptionChangeHistoryService: RecurrenceExceptionChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<RecurrenceExceptionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<RecurrenceExceptionBasicListData>>>();
        this.recordCache = new Map<string, Observable<RecurrenceExceptionData>>();

        RecurrenceExceptionService._instance = this;
    }

    public static get Instance(): RecurrenceExceptionService {
      return RecurrenceExceptionService._instance;
    }


    public ClearListCaches(config: RecurrenceExceptionQueryParameters | null = null) {

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


    public ConvertToRecurrenceExceptionSubmitData(data: RecurrenceExceptionData): RecurrenceExceptionSubmitData {

        let output = new RecurrenceExceptionSubmitData();

        output.id = data.id;
        output.scheduledEventId = data.scheduledEventId;
        output.exceptionDateTime = data.exceptionDateTime;
        output.movedToDateTime = data.movedToDateTime;
        output.reason = data.reason;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetRecurrenceException(id: bigint | number, includeRelations: boolean = true) : Observable<RecurrenceExceptionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const recurrenceException$ = this.requestRecurrenceException(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RecurrenceException", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, recurrenceException$);

            return recurrenceException$;
        }

        return this.recordCache.get(configHash) as Observable<RecurrenceExceptionData>;
    }

    private requestRecurrenceException(id: bigint | number, includeRelations: boolean = true) : Observable<RecurrenceExceptionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RecurrenceExceptionData>(this.baseUrl + 'api/RecurrenceException/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveRecurrenceException(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestRecurrenceException(id, includeRelations));
            }));
    }

    public GetRecurrenceExceptionList(config: RecurrenceExceptionQueryParameters | any = null) : Observable<Array<RecurrenceExceptionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const recurrenceExceptionList$ = this.requestRecurrenceExceptionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RecurrenceException list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, recurrenceExceptionList$);

            return recurrenceExceptionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<RecurrenceExceptionData>>;
    }


    private requestRecurrenceExceptionList(config: RecurrenceExceptionQueryParameters | any) : Observable <Array<RecurrenceExceptionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RecurrenceExceptionData>>(this.baseUrl + 'api/RecurrenceExceptions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveRecurrenceExceptionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestRecurrenceExceptionList(config));
            }));
    }

    public GetRecurrenceExceptionsRowCount(config: RecurrenceExceptionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const recurrenceExceptionsRowCount$ = this.requestRecurrenceExceptionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RecurrenceExceptions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, recurrenceExceptionsRowCount$);

            return recurrenceExceptionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestRecurrenceExceptionsRowCount(config: RecurrenceExceptionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/RecurrenceExceptions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRecurrenceExceptionsRowCount(config));
            }));
    }

    public GetRecurrenceExceptionsBasicListData(config: RecurrenceExceptionQueryParameters | any = null) : Observable<Array<RecurrenceExceptionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const recurrenceExceptionsBasicListData$ = this.requestRecurrenceExceptionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RecurrenceExceptions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, recurrenceExceptionsBasicListData$);

            return recurrenceExceptionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<RecurrenceExceptionBasicListData>>;
    }


    private requestRecurrenceExceptionsBasicListData(config: RecurrenceExceptionQueryParameters | any) : Observable<Array<RecurrenceExceptionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RecurrenceExceptionBasicListData>>(this.baseUrl + 'api/RecurrenceExceptions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRecurrenceExceptionsBasicListData(config));
            }));

    }


    public PutRecurrenceException(id: bigint | number, recurrenceException: RecurrenceExceptionSubmitData) : Observable<RecurrenceExceptionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RecurrenceExceptionData>(this.baseUrl + 'api/RecurrenceException/' + id.toString(), recurrenceException, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRecurrenceException(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutRecurrenceException(id, recurrenceException));
            }));
    }


    public PostRecurrenceException(recurrenceException: RecurrenceExceptionSubmitData) : Observable<RecurrenceExceptionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<RecurrenceExceptionData>(this.baseUrl + 'api/RecurrenceException', recurrenceException, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRecurrenceException(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostRecurrenceException(recurrenceException));
            }));
    }

  
    public DeleteRecurrenceException(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/RecurrenceException/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteRecurrenceException(id));
            }));
    }

    public RollbackRecurrenceException(id: bigint | number, versionNumber: bigint | number) : Observable<RecurrenceExceptionData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RecurrenceExceptionData>(this.baseUrl + 'api/RecurrenceException/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRecurrenceException(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackRecurrenceException(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a RecurrenceException.
     */
    public GetRecurrenceExceptionChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<RecurrenceExceptionData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<RecurrenceExceptionData>>(this.baseUrl + 'api/RecurrenceException/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetRecurrenceExceptionChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a RecurrenceException.
     */
    public GetRecurrenceExceptionAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<RecurrenceExceptionData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<RecurrenceExceptionData>[]>(this.baseUrl + 'api/RecurrenceException/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetRecurrenceExceptionAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a RecurrenceException.
     */
    public GetRecurrenceExceptionVersion(id: bigint | number, version: number): Observable<RecurrenceExceptionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RecurrenceExceptionData>(this.baseUrl + 'api/RecurrenceException/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveRecurrenceException(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetRecurrenceExceptionVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a RecurrenceException at a specific point in time.
     */
    public GetRecurrenceExceptionStateAtTime(id: bigint | number, time: string): Observable<RecurrenceExceptionData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RecurrenceExceptionData>(this.baseUrl + 'api/RecurrenceException/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveRecurrenceException(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetRecurrenceExceptionStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: RecurrenceExceptionQueryParameters | any): string {

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

    public userIsSchedulerRecurrenceExceptionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerRecurrenceExceptionReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.RecurrenceExceptions
        //
        if (userIsSchedulerRecurrenceExceptionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerRecurrenceExceptionReader = user.readPermission >= 1;
            } else {
                userIsSchedulerRecurrenceExceptionReader = false;
            }
        }

        return userIsSchedulerRecurrenceExceptionReader;
    }


    public userIsSchedulerRecurrenceExceptionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerRecurrenceExceptionWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.RecurrenceExceptions
        //
        if (userIsSchedulerRecurrenceExceptionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerRecurrenceExceptionWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerRecurrenceExceptionWriter = false;
          }      
        }

        return userIsSchedulerRecurrenceExceptionWriter;
    }

    public GetRecurrenceExceptionChangeHistoriesForRecurrenceException(recurrenceExceptionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<RecurrenceExceptionChangeHistoryData[]> {
        return this.recurrenceExceptionChangeHistoryService.GetRecurrenceExceptionChangeHistoryList({
            recurrenceExceptionId: recurrenceExceptionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full RecurrenceExceptionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the RecurrenceExceptionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when RecurrenceExceptionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveRecurrenceException(raw: any): RecurrenceExceptionData {
    if (!raw) return raw;

    //
    // Create a RecurrenceExceptionData object instance with correct prototype
    //
    const revived = Object.create(RecurrenceExceptionData.prototype) as RecurrenceExceptionData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._recurrenceExceptionChangeHistories = null;
    (revived as any)._recurrenceExceptionChangeHistoriesPromise = null;
    (revived as any)._recurrenceExceptionChangeHistoriesSubject = new BehaviorSubject<RecurrenceExceptionChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadRecurrenceExceptionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).RecurrenceExceptionChangeHistories$ = (revived as any)._recurrenceExceptionChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._recurrenceExceptionChangeHistories === null && (revived as any)._recurrenceExceptionChangeHistoriesPromise === null) {
                (revived as any).loadRecurrenceExceptionChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).RecurrenceExceptionChangeHistoriesCount$ = RecurrenceExceptionChangeHistoryService.Instance.GetRecurrenceExceptionChangeHistoriesRowCount({recurrenceExceptionId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<RecurrenceExceptionData> | null>(null);

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

  private ReviveRecurrenceExceptionList(rawList: any[]): RecurrenceExceptionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveRecurrenceException(raw));
  }

}
