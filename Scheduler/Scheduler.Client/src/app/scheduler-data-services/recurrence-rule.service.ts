/*

   GENERATED SERVICE FOR THE RECURRENCERULE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the RecurrenceRule table.

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
import { RecurrenceFrequencyData } from './recurrence-frequency.service';
import { RecurrenceRuleChangeHistoryService, RecurrenceRuleChangeHistoryData } from './recurrence-rule-change-history.service';
import { ScheduledEventService, ScheduledEventData } from './scheduled-event.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class RecurrenceRuleQueryParameters {
    recurrenceFrequencyId: bigint | number | null | undefined = null;
    interval: bigint | number | null | undefined = null;
    untilDateTime: string | null | undefined = null;        // ISO 8601 (full datetime)
    count: bigint | number | null | undefined = null;
    dayOfWeekMask: bigint | number | null | undefined = null;
    dayOfMonth: bigint | number | null | undefined = null;
    dayOfWeekInMonth: bigint | number | null | undefined = null;
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
export class RecurrenceRuleSubmitData {
    id!: bigint | number;
    recurrenceFrequencyId!: bigint | number;
    interval!: bigint | number;
    untilDateTime: string | null = null;     // ISO 8601 (full datetime)
    count: bigint | number | null = null;
    dayOfWeekMask: bigint | number | null = null;
    dayOfMonth: bigint | number | null = null;
    dayOfWeekInMonth: bigint | number | null = null;
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

export class RecurrenceRuleBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. RecurrenceRuleChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `recurrenceRule.RecurrenceRuleChildren$` — use with `| async` in templates
//        • Promise:    `recurrenceRule.RecurrenceRuleChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="recurrenceRule.RecurrenceRuleChildren$ | async"`), or
//        • Access the promise getter (`recurrenceRule.RecurrenceRuleChildren` or `await recurrenceRule.RecurrenceRuleChildren`)
//    - Simply reading `recurrenceRule.RecurrenceRuleChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await recurrenceRule.Reload()` to refresh the entire object and clear all lazy caches.
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
export class RecurrenceRuleData {
    id!: bigint | number;
    recurrenceFrequencyId!: bigint | number;
    interval!: bigint | number;
    untilDateTime!: string | null;   // ISO 8601 (full datetime)
    count!: bigint | number;
    dayOfWeekMask!: bigint | number;
    dayOfMonth!: bigint | number;
    dayOfWeekInMonth!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    recurrenceFrequency: RecurrenceFrequencyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _recurrenceRuleChangeHistories: RecurrenceRuleChangeHistoryData[] | null = null;
    private _recurrenceRuleChangeHistoriesPromise: Promise<RecurrenceRuleChangeHistoryData[]> | null  = null;
    private _recurrenceRuleChangeHistoriesSubject = new BehaviorSubject<RecurrenceRuleChangeHistoryData[] | null>(null);

                
    private _scheduledEvents: ScheduledEventData[] | null = null;
    private _scheduledEventsPromise: Promise<ScheduledEventData[]> | null  = null;
    private _scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<RecurrenceRuleData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<RecurrenceRuleData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<RecurrenceRuleData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public RecurrenceRuleChangeHistories$ = this._recurrenceRuleChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._recurrenceRuleChangeHistories === null && this._recurrenceRuleChangeHistoriesPromise === null) {
            this.loadRecurrenceRuleChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public RecurrenceRuleChangeHistoriesCount$ = RecurrenceRuleChangeHistoryService.Instance.GetRecurrenceRuleChangeHistoriesRowCount({recurrenceRuleId: this.id,
      active: true,
      deleted: false
    });



    public ScheduledEvents$ = this._scheduledEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEvents === null && this._scheduledEventsPromise === null) {
            this.loadScheduledEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({recurrenceRuleId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any RecurrenceRuleData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.recurrenceRule.Reload();
  //
  //  Non Async:
  //
  //     recurrenceRule[0].Reload().then(x => {
  //        this.recurrenceRule = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      RecurrenceRuleService.Instance.GetRecurrenceRule(this.id, includeRelations)
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
     this._recurrenceRuleChangeHistories = null;
     this._recurrenceRuleChangeHistoriesPromise = null;
     this._recurrenceRuleChangeHistoriesSubject.next(null);

     this._scheduledEvents = null;
     this._scheduledEventsPromise = null;
     this._scheduledEventsSubject.next(null);

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
     * Gets the RecurrenceRuleChangeHistories for this RecurrenceRule.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.recurrenceRule.RecurrenceRuleChangeHistories.then(recurrenceRules => { ... })
     *   or
     *   await this.recurrenceRule.recurrenceRules
     *
    */
    public get RecurrenceRuleChangeHistories(): Promise<RecurrenceRuleChangeHistoryData[]> {
        if (this._recurrenceRuleChangeHistories !== null) {
            return Promise.resolve(this._recurrenceRuleChangeHistories);
        }

        if (this._recurrenceRuleChangeHistoriesPromise !== null) {
            return this._recurrenceRuleChangeHistoriesPromise;
        }

        // Start the load
        this.loadRecurrenceRuleChangeHistories();

        return this._recurrenceRuleChangeHistoriesPromise!;
    }



    private loadRecurrenceRuleChangeHistories(): void {

        this._recurrenceRuleChangeHistoriesPromise = lastValueFrom(
            RecurrenceRuleService.Instance.GetRecurrenceRuleChangeHistoriesForRecurrenceRule(this.id)
        )
        .then(RecurrenceRuleChangeHistories => {
            this._recurrenceRuleChangeHistories = RecurrenceRuleChangeHistories ?? [];
            this._recurrenceRuleChangeHistoriesSubject.next(this._recurrenceRuleChangeHistories);
            return this._recurrenceRuleChangeHistories;
         })
        .catch(err => {
            this._recurrenceRuleChangeHistories = [];
            this._recurrenceRuleChangeHistoriesSubject.next(this._recurrenceRuleChangeHistories);
            throw err;
        })
        .finally(() => {
            this._recurrenceRuleChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached RecurrenceRuleChangeHistory. Call after mutations to force refresh.
     */
    public ClearRecurrenceRuleChangeHistoriesCache(): void {
        this._recurrenceRuleChangeHistories = null;
        this._recurrenceRuleChangeHistoriesPromise = null;
        this._recurrenceRuleChangeHistoriesSubject.next(this._recurrenceRuleChangeHistories);      // Emit to observable
    }

    public get HasRecurrenceRuleChangeHistories(): Promise<boolean> {
        return this.RecurrenceRuleChangeHistories.then(recurrenceRuleChangeHistories => recurrenceRuleChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ScheduledEvents for this RecurrenceRule.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.recurrenceRule.ScheduledEvents.then(recurrenceRules => { ... })
     *   or
     *   await this.recurrenceRule.recurrenceRules
     *
    */
    public get ScheduledEvents(): Promise<ScheduledEventData[]> {
        if (this._scheduledEvents !== null) {
            return Promise.resolve(this._scheduledEvents);
        }

        if (this._scheduledEventsPromise !== null) {
            return this._scheduledEventsPromise;
        }

        // Start the load
        this.loadScheduledEvents();

        return this._scheduledEventsPromise!;
    }



    private loadScheduledEvents(): void {

        this._scheduledEventsPromise = lastValueFrom(
            RecurrenceRuleService.Instance.GetScheduledEventsForRecurrenceRule(this.id)
        )
        .then(ScheduledEvents => {
            this._scheduledEvents = ScheduledEvents ?? [];
            this._scheduledEventsSubject.next(this._scheduledEvents);
            return this._scheduledEvents;
         })
        .catch(err => {
            this._scheduledEvents = [];
            this._scheduledEventsSubject.next(this._scheduledEvents);
            throw err;
        })
        .finally(() => {
            this._scheduledEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEvent. Call after mutations to force refresh.
     */
    public ClearScheduledEventsCache(): void {
        this._scheduledEvents = null;
        this._scheduledEventsPromise = null;
        this._scheduledEventsSubject.next(this._scheduledEvents);      // Emit to observable
    }

    public get HasScheduledEvents(): Promise<boolean> {
        return this.ScheduledEvents.then(scheduledEvents => scheduledEvents.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (recurrenceRule.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await recurrenceRule.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<RecurrenceRuleData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<RecurrenceRuleData>> {
        const info = await lastValueFrom(
            RecurrenceRuleService.Instance.GetRecurrenceRuleChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this RecurrenceRuleData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this RecurrenceRuleData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): RecurrenceRuleSubmitData {
        return RecurrenceRuleService.Instance.ConvertToRecurrenceRuleSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class RecurrenceRuleService extends SecureEndpointBase {

    private static _instance: RecurrenceRuleService;
    private listCache: Map<string, Observable<Array<RecurrenceRuleData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<RecurrenceRuleBasicListData>>>;
    private recordCache: Map<string, Observable<RecurrenceRuleData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private recurrenceRuleChangeHistoryService: RecurrenceRuleChangeHistoryService,
        private scheduledEventService: ScheduledEventService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<RecurrenceRuleData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<RecurrenceRuleBasicListData>>>();
        this.recordCache = new Map<string, Observable<RecurrenceRuleData>>();

        RecurrenceRuleService._instance = this;
    }

    public static get Instance(): RecurrenceRuleService {
      return RecurrenceRuleService._instance;
    }


    public ClearListCaches(config: RecurrenceRuleQueryParameters | null = null) {

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


    public ConvertToRecurrenceRuleSubmitData(data: RecurrenceRuleData): RecurrenceRuleSubmitData {

        let output = new RecurrenceRuleSubmitData();

        output.id = data.id;
        output.recurrenceFrequencyId = data.recurrenceFrequencyId;
        output.interval = data.interval;
        output.untilDateTime = data.untilDateTime;
        output.count = data.count;
        output.dayOfWeekMask = data.dayOfWeekMask;
        output.dayOfMonth = data.dayOfMonth;
        output.dayOfWeekInMonth = data.dayOfWeekInMonth;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetRecurrenceRule(id: bigint | number, includeRelations: boolean = true) : Observable<RecurrenceRuleData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const recurrenceRule$ = this.requestRecurrenceRule(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RecurrenceRule", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, recurrenceRule$);

            return recurrenceRule$;
        }

        return this.recordCache.get(configHash) as Observable<RecurrenceRuleData>;
    }

    private requestRecurrenceRule(id: bigint | number, includeRelations: boolean = true) : Observable<RecurrenceRuleData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RecurrenceRuleData>(this.baseUrl + 'api/RecurrenceRule/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveRecurrenceRule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestRecurrenceRule(id, includeRelations));
            }));
    }

    public GetRecurrenceRuleList(config: RecurrenceRuleQueryParameters | any = null) : Observable<Array<RecurrenceRuleData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const recurrenceRuleList$ = this.requestRecurrenceRuleList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RecurrenceRule list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, recurrenceRuleList$);

            return recurrenceRuleList$;
        }

        return this.listCache.get(configHash) as Observable<Array<RecurrenceRuleData>>;
    }


    private requestRecurrenceRuleList(config: RecurrenceRuleQueryParameters | any) : Observable <Array<RecurrenceRuleData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RecurrenceRuleData>>(this.baseUrl + 'api/RecurrenceRules', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveRecurrenceRuleList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestRecurrenceRuleList(config));
            }));
    }

    public GetRecurrenceRulesRowCount(config: RecurrenceRuleQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const recurrenceRulesRowCount$ = this.requestRecurrenceRulesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RecurrenceRules row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, recurrenceRulesRowCount$);

            return recurrenceRulesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestRecurrenceRulesRowCount(config: RecurrenceRuleQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/RecurrenceRules/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRecurrenceRulesRowCount(config));
            }));
    }

    public GetRecurrenceRulesBasicListData(config: RecurrenceRuleQueryParameters | any = null) : Observable<Array<RecurrenceRuleBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const recurrenceRulesBasicListData$ = this.requestRecurrenceRulesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RecurrenceRules basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, recurrenceRulesBasicListData$);

            return recurrenceRulesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<RecurrenceRuleBasicListData>>;
    }


    private requestRecurrenceRulesBasicListData(config: RecurrenceRuleQueryParameters | any) : Observable<Array<RecurrenceRuleBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RecurrenceRuleBasicListData>>(this.baseUrl + 'api/RecurrenceRules/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRecurrenceRulesBasicListData(config));
            }));

    }


    public PutRecurrenceRule(id: bigint | number, recurrenceRule: RecurrenceRuleSubmitData) : Observable<RecurrenceRuleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RecurrenceRuleData>(this.baseUrl + 'api/RecurrenceRule/' + id.toString(), recurrenceRule, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRecurrenceRule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutRecurrenceRule(id, recurrenceRule));
            }));
    }


    public PostRecurrenceRule(recurrenceRule: RecurrenceRuleSubmitData) : Observable<RecurrenceRuleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<RecurrenceRuleData>(this.baseUrl + 'api/RecurrenceRule', recurrenceRule, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRecurrenceRule(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostRecurrenceRule(recurrenceRule));
            }));
    }

  
    public DeleteRecurrenceRule(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/RecurrenceRule/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteRecurrenceRule(id));
            }));
    }

    public RollbackRecurrenceRule(id: bigint | number, versionNumber: bigint | number) : Observable<RecurrenceRuleData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RecurrenceRuleData>(this.baseUrl + 'api/RecurrenceRule/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRecurrenceRule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackRecurrenceRule(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a RecurrenceRule.
     */
    public GetRecurrenceRuleChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<RecurrenceRuleData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<RecurrenceRuleData>>(this.baseUrl + 'api/RecurrenceRule/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetRecurrenceRuleChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a RecurrenceRule.
     */
    public GetRecurrenceRuleAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<RecurrenceRuleData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<RecurrenceRuleData>[]>(this.baseUrl + 'api/RecurrenceRule/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetRecurrenceRuleAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a RecurrenceRule.
     */
    public GetRecurrenceRuleVersion(id: bigint | number, version: number): Observable<RecurrenceRuleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RecurrenceRuleData>(this.baseUrl + 'api/RecurrenceRule/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveRecurrenceRule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetRecurrenceRuleVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a RecurrenceRule at a specific point in time.
     */
    public GetRecurrenceRuleStateAtTime(id: bigint | number, time: string): Observable<RecurrenceRuleData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RecurrenceRuleData>(this.baseUrl + 'api/RecurrenceRule/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveRecurrenceRule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetRecurrenceRuleStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: RecurrenceRuleQueryParameters | any): string {

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

    public userIsSchedulerRecurrenceRuleReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerRecurrenceRuleReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.RecurrenceRules
        //
        if (userIsSchedulerRecurrenceRuleReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerRecurrenceRuleReader = user.readPermission >= 1;
            } else {
                userIsSchedulerRecurrenceRuleReader = false;
            }
        }

        return userIsSchedulerRecurrenceRuleReader;
    }


    public userIsSchedulerRecurrenceRuleWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerRecurrenceRuleWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.RecurrenceRules
        //
        if (userIsSchedulerRecurrenceRuleWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerRecurrenceRuleWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerRecurrenceRuleWriter = false;
          }      
        }

        return userIsSchedulerRecurrenceRuleWriter;
    }

    public GetRecurrenceRuleChangeHistoriesForRecurrenceRule(recurrenceRuleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<RecurrenceRuleChangeHistoryData[]> {
        return this.recurrenceRuleChangeHistoryService.GetRecurrenceRuleChangeHistoryList({
            recurrenceRuleId: recurrenceRuleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventsForRecurrenceRule(recurrenceRuleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventData[]> {
        return this.scheduledEventService.GetScheduledEventList({
            recurrenceRuleId: recurrenceRuleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full RecurrenceRuleData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the RecurrenceRuleData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when RecurrenceRuleTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveRecurrenceRule(raw: any): RecurrenceRuleData {
    if (!raw) return raw;

    //
    // Create a RecurrenceRuleData object instance with correct prototype
    //
    const revived = Object.create(RecurrenceRuleData.prototype) as RecurrenceRuleData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._recurrenceRuleChangeHistories = null;
    (revived as any)._recurrenceRuleChangeHistoriesPromise = null;
    (revived as any)._recurrenceRuleChangeHistoriesSubject = new BehaviorSubject<RecurrenceRuleChangeHistoryData[] | null>(null);

    (revived as any)._scheduledEvents = null;
    (revived as any)._scheduledEventsPromise = null;
    (revived as any)._scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadRecurrenceRuleXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).RecurrenceRuleChangeHistories$ = (revived as any)._recurrenceRuleChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._recurrenceRuleChangeHistories === null && (revived as any)._recurrenceRuleChangeHistoriesPromise === null) {
                (revived as any).loadRecurrenceRuleChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).RecurrenceRuleChangeHistoriesCount$ = RecurrenceRuleChangeHistoryService.Instance.GetRecurrenceRuleChangeHistoriesRowCount({recurrenceRuleId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduledEvents$ = (revived as any)._scheduledEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEvents === null && (revived as any)._scheduledEventsPromise === null) {
                (revived as any).loadScheduledEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({recurrenceRuleId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<RecurrenceRuleData> | null>(null);

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

  private ReviveRecurrenceRuleList(rawList: any[]): RecurrenceRuleData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveRecurrenceRule(raw));
  }

}
