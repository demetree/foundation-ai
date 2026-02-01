/*

   GENERATED SERVICE FOR THE ESCALATIONRULE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the EscalationRule table.

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
import { EscalationPolicyData } from './escalation-policy.service';
import { EscalationRuleChangeHistoryService, EscalationRuleChangeHistoryData } from './escalation-rule-change-history.service';
import { IncidentService, IncidentData } from './incident.service';
import { IncidentNotificationService, IncidentNotificationData } from './incident-notification.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class EscalationRuleQueryParameters {
    escalationPolicyId: bigint | number | null | undefined = null;
    ruleOrder: bigint | number | null | undefined = null;
    delayMinutes: bigint | number | null | undefined = null;
    repeatCount: bigint | number | null | undefined = null;
    repeatDelayMinutes: bigint | number | null | undefined = null;
    targetType: string | null | undefined = null;
    targetObjectGuid: string | null | undefined = null;
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
export class EscalationRuleSubmitData {
    id!: bigint | number;
    escalationPolicyId!: bigint | number;
    ruleOrder!: bigint | number;
    delayMinutes!: bigint | number;
    repeatCount!: bigint | number;
    repeatDelayMinutes: bigint | number | null = null;
    targetType!: string;
    targetObjectGuid: string | null = null;
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

export class EscalationRuleBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. EscalationRuleChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `escalationRule.EscalationRuleChildren$` — use with `| async` in templates
//        • Promise:    `escalationRule.EscalationRuleChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="escalationRule.EscalationRuleChildren$ | async"`), or
//        • Access the promise getter (`escalationRule.EscalationRuleChildren` or `await escalationRule.EscalationRuleChildren`)
//    - Simply reading `escalationRule.EscalationRuleChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await escalationRule.Reload()` to refresh the entire object and clear all lazy caches.
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
export class EscalationRuleData {
    id!: bigint | number;
    escalationPolicyId!: bigint | number;
    ruleOrder!: bigint | number;
    delayMinutes!: bigint | number;
    repeatCount!: bigint | number;
    repeatDelayMinutes!: bigint | number;
    targetType!: string;
    targetObjectGuid!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    escalationPolicy: EscalationPolicyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _escalationRuleChangeHistories: EscalationRuleChangeHistoryData[] | null = null;
    private _escalationRuleChangeHistoriesPromise: Promise<EscalationRuleChangeHistoryData[]> | null  = null;
    private _escalationRuleChangeHistoriesSubject = new BehaviorSubject<EscalationRuleChangeHistoryData[] | null>(null);

                
    private _incidents: IncidentData[] | null = null;
    private _incidentsPromise: Promise<IncidentData[]> | null  = null;
    private _incidentsSubject = new BehaviorSubject<IncidentData[] | null>(null);

                
    private _incidentNotifications: IncidentNotificationData[] | null = null;
    private _incidentNotificationsPromise: Promise<IncidentNotificationData[]> | null  = null;
    private _incidentNotificationsSubject = new BehaviorSubject<IncidentNotificationData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<EscalationRuleData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<EscalationRuleData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<EscalationRuleData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public EscalationRuleChangeHistories$ = this._escalationRuleChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._escalationRuleChangeHistories === null && this._escalationRuleChangeHistoriesPromise === null) {
            this.loadEscalationRuleChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public EscalationRuleChangeHistoriesCount$ = EscalationRuleChangeHistoryService.Instance.GetEscalationRuleChangeHistoriesRowCount({escalationRuleId: this.id,
      active: true,
      deleted: false
    });



    public Incidents$ = this._incidentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._incidents === null && this._incidentsPromise === null) {
            this.loadIncidents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public IncidentsCount$ = IncidentService.Instance.GetIncidentsRowCount({escalationRuleId: this.id,
      active: true,
      deleted: false
    });



    public IncidentNotifications$ = this._incidentNotificationsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._incidentNotifications === null && this._incidentNotificationsPromise === null) {
            this.loadIncidentNotifications(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public IncidentNotificationsCount$ = IncidentNotificationService.Instance.GetIncidentNotificationsRowCount({escalationRuleId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any EscalationRuleData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.escalationRule.Reload();
  //
  //  Non Async:
  //
  //     escalationRule[0].Reload().then(x => {
  //        this.escalationRule = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      EscalationRuleService.Instance.GetEscalationRule(this.id, includeRelations)
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
     this._escalationRuleChangeHistories = null;
     this._escalationRuleChangeHistoriesPromise = null;
     this._escalationRuleChangeHistoriesSubject.next(null);

     this._incidents = null;
     this._incidentsPromise = null;
     this._incidentsSubject.next(null);

     this._incidentNotifications = null;
     this._incidentNotificationsPromise = null;
     this._incidentNotificationsSubject.next(null);

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
     * Gets the EscalationRuleChangeHistories for this EscalationRule.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.escalationRule.EscalationRuleChangeHistories.then(escalationRules => { ... })
     *   or
     *   await this.escalationRule.escalationRules
     *
    */
    public get EscalationRuleChangeHistories(): Promise<EscalationRuleChangeHistoryData[]> {
        if (this._escalationRuleChangeHistories !== null) {
            return Promise.resolve(this._escalationRuleChangeHistories);
        }

        if (this._escalationRuleChangeHistoriesPromise !== null) {
            return this._escalationRuleChangeHistoriesPromise;
        }

        // Start the load
        this.loadEscalationRuleChangeHistories();

        return this._escalationRuleChangeHistoriesPromise!;
    }



    private loadEscalationRuleChangeHistories(): void {

        this._escalationRuleChangeHistoriesPromise = lastValueFrom(
            EscalationRuleService.Instance.GetEscalationRuleChangeHistoriesForEscalationRule(this.id)
        )
        .then(EscalationRuleChangeHistories => {
            this._escalationRuleChangeHistories = EscalationRuleChangeHistories ?? [];
            this._escalationRuleChangeHistoriesSubject.next(this._escalationRuleChangeHistories);
            return this._escalationRuleChangeHistories;
         })
        .catch(err => {
            this._escalationRuleChangeHistories = [];
            this._escalationRuleChangeHistoriesSubject.next(this._escalationRuleChangeHistories);
            throw err;
        })
        .finally(() => {
            this._escalationRuleChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EscalationRuleChangeHistory. Call after mutations to force refresh.
     */
    public ClearEscalationRuleChangeHistoriesCache(): void {
        this._escalationRuleChangeHistories = null;
        this._escalationRuleChangeHistoriesPromise = null;
        this._escalationRuleChangeHistoriesSubject.next(this._escalationRuleChangeHistories);      // Emit to observable
    }

    public get HasEscalationRuleChangeHistories(): Promise<boolean> {
        return this.EscalationRuleChangeHistories.then(escalationRuleChangeHistories => escalationRuleChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Incidents for this EscalationRule.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.escalationRule.Incidents.then(escalationRules => { ... })
     *   or
     *   await this.escalationRule.escalationRules
     *
    */
    public get Incidents(): Promise<IncidentData[]> {
        if (this._incidents !== null) {
            return Promise.resolve(this._incidents);
        }

        if (this._incidentsPromise !== null) {
            return this._incidentsPromise;
        }

        // Start the load
        this.loadIncidents();

        return this._incidentsPromise!;
    }



    private loadIncidents(): void {

        this._incidentsPromise = lastValueFrom(
            EscalationRuleService.Instance.GetIncidentsForEscalationRule(this.id)
        )
        .then(Incidents => {
            this._incidents = Incidents ?? [];
            this._incidentsSubject.next(this._incidents);
            return this._incidents;
         })
        .catch(err => {
            this._incidents = [];
            this._incidentsSubject.next(this._incidents);
            throw err;
        })
        .finally(() => {
            this._incidentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Incident. Call after mutations to force refresh.
     */
    public ClearIncidentsCache(): void {
        this._incidents = null;
        this._incidentsPromise = null;
        this._incidentsSubject.next(this._incidents);      // Emit to observable
    }

    public get HasIncidents(): Promise<boolean> {
        return this.Incidents.then(incidents => incidents.length > 0);
    }


    /**
     *
     * Gets the IncidentNotifications for this EscalationRule.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.escalationRule.IncidentNotifications.then(escalationRules => { ... })
     *   or
     *   await this.escalationRule.escalationRules
     *
    */
    public get IncidentNotifications(): Promise<IncidentNotificationData[]> {
        if (this._incidentNotifications !== null) {
            return Promise.resolve(this._incidentNotifications);
        }

        if (this._incidentNotificationsPromise !== null) {
            return this._incidentNotificationsPromise;
        }

        // Start the load
        this.loadIncidentNotifications();

        return this._incidentNotificationsPromise!;
    }



    private loadIncidentNotifications(): void {

        this._incidentNotificationsPromise = lastValueFrom(
            EscalationRuleService.Instance.GetIncidentNotificationsForEscalationRule(this.id)
        )
        .then(IncidentNotifications => {
            this._incidentNotifications = IncidentNotifications ?? [];
            this._incidentNotificationsSubject.next(this._incidentNotifications);
            return this._incidentNotifications;
         })
        .catch(err => {
            this._incidentNotifications = [];
            this._incidentNotificationsSubject.next(this._incidentNotifications);
            throw err;
        })
        .finally(() => {
            this._incidentNotificationsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached IncidentNotification. Call after mutations to force refresh.
     */
    public ClearIncidentNotificationsCache(): void {
        this._incidentNotifications = null;
        this._incidentNotificationsPromise = null;
        this._incidentNotificationsSubject.next(this._incidentNotifications);      // Emit to observable
    }

    public get HasIncidentNotifications(): Promise<boolean> {
        return this.IncidentNotifications.then(incidentNotifications => incidentNotifications.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (escalationRule.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await escalationRule.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<EscalationRuleData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<EscalationRuleData>> {
        const info = await lastValueFrom(
            EscalationRuleService.Instance.GetEscalationRuleChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this EscalationRuleData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this EscalationRuleData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): EscalationRuleSubmitData {
        return EscalationRuleService.Instance.ConvertToEscalationRuleSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class EscalationRuleService extends SecureEndpointBase {

    private static _instance: EscalationRuleService;
    private listCache: Map<string, Observable<Array<EscalationRuleData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<EscalationRuleBasicListData>>>;
    private recordCache: Map<string, Observable<EscalationRuleData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private escalationRuleChangeHistoryService: EscalationRuleChangeHistoryService,
        private incidentService: IncidentService,
        private incidentNotificationService: IncidentNotificationService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<EscalationRuleData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<EscalationRuleBasicListData>>>();
        this.recordCache = new Map<string, Observable<EscalationRuleData>>();

        EscalationRuleService._instance = this;
    }

    public static get Instance(): EscalationRuleService {
      return EscalationRuleService._instance;
    }


    public ClearListCaches(config: EscalationRuleQueryParameters | null = null) {

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


    public ConvertToEscalationRuleSubmitData(data: EscalationRuleData): EscalationRuleSubmitData {

        let output = new EscalationRuleSubmitData();

        output.id = data.id;
        output.escalationPolicyId = data.escalationPolicyId;
        output.ruleOrder = data.ruleOrder;
        output.delayMinutes = data.delayMinutes;
        output.repeatCount = data.repeatCount;
        output.repeatDelayMinutes = data.repeatDelayMinutes;
        output.targetType = data.targetType;
        output.targetObjectGuid = data.targetObjectGuid;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetEscalationRule(id: bigint | number, includeRelations: boolean = true) : Observable<EscalationRuleData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const escalationRule$ = this.requestEscalationRule(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EscalationRule", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, escalationRule$);

            return escalationRule$;
        }

        return this.recordCache.get(configHash) as Observable<EscalationRuleData>;
    }

    private requestEscalationRule(id: bigint | number, includeRelations: boolean = true) : Observable<EscalationRuleData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EscalationRuleData>(this.baseUrl + 'api/EscalationRule/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveEscalationRule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestEscalationRule(id, includeRelations));
            }));
    }

    public GetEscalationRuleList(config: EscalationRuleQueryParameters | any = null) : Observable<Array<EscalationRuleData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const escalationRuleList$ = this.requestEscalationRuleList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EscalationRule list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, escalationRuleList$);

            return escalationRuleList$;
        }

        return this.listCache.get(configHash) as Observable<Array<EscalationRuleData>>;
    }


    private requestEscalationRuleList(config: EscalationRuleQueryParameters | any) : Observable <Array<EscalationRuleData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EscalationRuleData>>(this.baseUrl + 'api/EscalationRules', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveEscalationRuleList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestEscalationRuleList(config));
            }));
    }

    public GetEscalationRulesRowCount(config: EscalationRuleQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const escalationRulesRowCount$ = this.requestEscalationRulesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EscalationRules row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, escalationRulesRowCount$);

            return escalationRulesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestEscalationRulesRowCount(config: EscalationRuleQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/EscalationRules/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEscalationRulesRowCount(config));
            }));
    }

    public GetEscalationRulesBasicListData(config: EscalationRuleQueryParameters | any = null) : Observable<Array<EscalationRuleBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const escalationRulesBasicListData$ = this.requestEscalationRulesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EscalationRules basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, escalationRulesBasicListData$);

            return escalationRulesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<EscalationRuleBasicListData>>;
    }


    private requestEscalationRulesBasicListData(config: EscalationRuleQueryParameters | any) : Observable<Array<EscalationRuleBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EscalationRuleBasicListData>>(this.baseUrl + 'api/EscalationRules/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEscalationRulesBasicListData(config));
            }));

    }


    public PutEscalationRule(id: bigint | number, escalationRule: EscalationRuleSubmitData) : Observable<EscalationRuleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EscalationRuleData>(this.baseUrl + 'api/EscalationRule/' + id.toString(), escalationRule, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEscalationRule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutEscalationRule(id, escalationRule));
            }));
    }


    public PostEscalationRule(escalationRule: EscalationRuleSubmitData) : Observable<EscalationRuleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<EscalationRuleData>(this.baseUrl + 'api/EscalationRule', escalationRule, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEscalationRule(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostEscalationRule(escalationRule));
            }));
    }

  
    public DeleteEscalationRule(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/EscalationRule/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteEscalationRule(id));
            }));
    }

    public RollbackEscalationRule(id: bigint | number, versionNumber: bigint | number) : Observable<EscalationRuleData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EscalationRuleData>(this.baseUrl + 'api/EscalationRule/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEscalationRule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackEscalationRule(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a EscalationRule.
     */
    public GetEscalationRuleChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<EscalationRuleData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<EscalationRuleData>>(this.baseUrl + 'api/EscalationRule/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetEscalationRuleChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a EscalationRule.
     */
    public GetEscalationRuleAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<EscalationRuleData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<EscalationRuleData>[]>(this.baseUrl + 'api/EscalationRule/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetEscalationRuleAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a EscalationRule.
     */
    public GetEscalationRuleVersion(id: bigint | number, version: number): Observable<EscalationRuleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EscalationRuleData>(this.baseUrl + 'api/EscalationRule/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveEscalationRule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetEscalationRuleVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a EscalationRule at a specific point in time.
     */
    public GetEscalationRuleStateAtTime(id: bigint | number, time: string): Observable<EscalationRuleData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EscalationRuleData>(this.baseUrl + 'api/EscalationRule/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveEscalationRule(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetEscalationRuleStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: EscalationRuleQueryParameters | any): string {

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

    public userIsAlertingEscalationRuleReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingEscalationRuleReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.EscalationRules
        //
        if (userIsAlertingEscalationRuleReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingEscalationRuleReader = user.readPermission >= 0;
            } else {
                userIsAlertingEscalationRuleReader = false;
            }
        }

        return userIsAlertingEscalationRuleReader;
    }


    public userIsAlertingEscalationRuleWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingEscalationRuleWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.EscalationRules
        //
        if (userIsAlertingEscalationRuleWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingEscalationRuleWriter = user.writePermission >= 0;
          } else {
            userIsAlertingEscalationRuleWriter = false;
          }      
        }

        return userIsAlertingEscalationRuleWriter;
    }

    public GetEscalationRuleChangeHistoriesForEscalationRule(escalationRuleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EscalationRuleChangeHistoryData[]> {
        return this.escalationRuleChangeHistoryService.GetEscalationRuleChangeHistoryList({
            escalationRuleId: escalationRuleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetIncidentsForEscalationRule(escalationRuleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IncidentData[]> {
        return this.incidentService.GetIncidentList({
            escalationRuleId: escalationRuleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetIncidentNotificationsForEscalationRule(escalationRuleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IncidentNotificationData[]> {
        return this.incidentNotificationService.GetIncidentNotificationList({
            escalationRuleId: escalationRuleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full EscalationRuleData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the EscalationRuleData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when EscalationRuleTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveEscalationRule(raw: any): EscalationRuleData {
    if (!raw) return raw;

    //
    // Create a EscalationRuleData object instance with correct prototype
    //
    const revived = Object.create(EscalationRuleData.prototype) as EscalationRuleData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._escalationRuleChangeHistories = null;
    (revived as any)._escalationRuleChangeHistoriesPromise = null;
    (revived as any)._escalationRuleChangeHistoriesSubject = new BehaviorSubject<EscalationRuleChangeHistoryData[] | null>(null);

    (revived as any)._incidents = null;
    (revived as any)._incidentsPromise = null;
    (revived as any)._incidentsSubject = new BehaviorSubject<IncidentData[] | null>(null);

    (revived as any)._incidentNotifications = null;
    (revived as any)._incidentNotificationsPromise = null;
    (revived as any)._incidentNotificationsSubject = new BehaviorSubject<IncidentNotificationData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadEscalationRuleXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).EscalationRuleChangeHistories$ = (revived as any)._escalationRuleChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._escalationRuleChangeHistories === null && (revived as any)._escalationRuleChangeHistoriesPromise === null) {
                (revived as any).loadEscalationRuleChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).EscalationRuleChangeHistoriesCount$ = EscalationRuleChangeHistoryService.Instance.GetEscalationRuleChangeHistoriesRowCount({escalationRuleId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Incidents$ = (revived as any)._incidentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._incidents === null && (revived as any)._incidentsPromise === null) {
                (revived as any).loadIncidents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).IncidentsCount$ = IncidentService.Instance.GetIncidentsRowCount({escalationRuleId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).IncidentNotifications$ = (revived as any)._incidentNotificationsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._incidentNotifications === null && (revived as any)._incidentNotificationsPromise === null) {
                (revived as any).loadIncidentNotifications();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).IncidentNotificationsCount$ = IncidentNotificationService.Instance.GetIncidentNotificationsRowCount({escalationRuleId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<EscalationRuleData> | null>(null);

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

  private ReviveEscalationRuleList(rawList: any[]): EscalationRuleData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveEscalationRule(raw));
  }

}
