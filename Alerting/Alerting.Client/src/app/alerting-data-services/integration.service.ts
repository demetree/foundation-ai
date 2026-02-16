/*

   GENERATED SERVICE FOR THE INTEGRATION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Integration table.

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
import { ServiceData } from './service.service';
import { IntegrationChangeHistoryService, IntegrationChangeHistoryData } from './integration-change-history.service';
import { IntegrationCallbackIncidentEventTypeService, IntegrationCallbackIncidentEventTypeData } from './integration-callback-incident-event-type.service';
import { WebhookDeliveryAttemptService, WebhookDeliveryAttemptData } from './webhook-delivery-attempt.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class IntegrationQueryParameters {
    serviceId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    apiKeyHash: string | null | undefined = null;
    callbackWebhookUrl: string | null | undefined = null;
    maxRetryAttempts: bigint | number | null | undefined = null;
    retryBackoffSeconds: bigint | number | null | undefined = null;
    lastCallbackSuccessAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    consecutiveCallbackFailures: bigint | number | null | undefined = null;
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
export class IntegrationSubmitData {
    id!: bigint | number;
    serviceId!: bigint | number;
    name!: string;
    description: string | null = null;
    apiKeyHash!: string;
    callbackWebhookUrl: string | null = null;
    maxRetryAttempts: bigint | number | null = null;
    retryBackoffSeconds: bigint | number | null = null;
    lastCallbackSuccessAt: string | null = null;     // ISO 8601 (full datetime)
    consecutiveCallbackFailures: bigint | number | null = null;
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

export class IntegrationBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. IntegrationChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `integration.IntegrationChildren$` — use with `| async` in templates
//        • Promise:    `integration.IntegrationChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="integration.IntegrationChildren$ | async"`), or
//        • Access the promise getter (`integration.IntegrationChildren` or `await integration.IntegrationChildren`)
//    - Simply reading `integration.IntegrationChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await integration.Reload()` to refresh the entire object and clear all lazy caches.
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
export class IntegrationData {
    id!: bigint | number;
    serviceId!: bigint | number;
    name!: string;
    description!: string | null;
    apiKeyHash!: string;
    callbackWebhookUrl!: string | null;
    maxRetryAttempts!: bigint | number;
    retryBackoffSeconds!: bigint | number;
    lastCallbackSuccessAt!: string | null;   // ISO 8601 (full datetime)
    consecutiveCallbackFailures!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    service: ServiceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _integrationChangeHistories: IntegrationChangeHistoryData[] | null = null;
    private _integrationChangeHistoriesPromise: Promise<IntegrationChangeHistoryData[]> | null  = null;
    private _integrationChangeHistoriesSubject = new BehaviorSubject<IntegrationChangeHistoryData[] | null>(null);

                
    private _integrationCallbackIncidentEventTypes: IntegrationCallbackIncidentEventTypeData[] | null = null;
    private _integrationCallbackIncidentEventTypesPromise: Promise<IntegrationCallbackIncidentEventTypeData[]> | null  = null;
    private _integrationCallbackIncidentEventTypesSubject = new BehaviorSubject<IntegrationCallbackIncidentEventTypeData[] | null>(null);

                
    private _webhookDeliveryAttempts: WebhookDeliveryAttemptData[] | null = null;
    private _webhookDeliveryAttemptsPromise: Promise<WebhookDeliveryAttemptData[]> | null  = null;
    private _webhookDeliveryAttemptsSubject = new BehaviorSubject<WebhookDeliveryAttemptData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<IntegrationData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<IntegrationData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<IntegrationData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public IntegrationChangeHistories$ = this._integrationChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._integrationChangeHistories === null && this._integrationChangeHistoriesPromise === null) {
            this.loadIntegrationChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public IntegrationChangeHistoriesCount$ = IntegrationChangeHistoryService.Instance.GetIntegrationChangeHistoriesRowCount({integrationId: this.id,
      active: true,
      deleted: false
    });



    public IntegrationCallbackIncidentEventTypes$ = this._integrationCallbackIncidentEventTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._integrationCallbackIncidentEventTypes === null && this._integrationCallbackIncidentEventTypesPromise === null) {
            this.loadIntegrationCallbackIncidentEventTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public IntegrationCallbackIncidentEventTypesCount$ = IntegrationCallbackIncidentEventTypeService.Instance.GetIntegrationCallbackIncidentEventTypesRowCount({integrationId: this.id,
      active: true,
      deleted: false
    });



    public WebhookDeliveryAttempts$ = this._webhookDeliveryAttemptsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._webhookDeliveryAttempts === null && this._webhookDeliveryAttemptsPromise === null) {
            this.loadWebhookDeliveryAttempts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public WebhookDeliveryAttemptsCount$ = WebhookDeliveryAttemptService.Instance.GetWebhookDeliveryAttemptsRowCount({integrationId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any IntegrationData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.integration.Reload();
  //
  //  Non Async:
  //
  //     integration[0].Reload().then(x => {
  //        this.integration = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      IntegrationService.Instance.GetIntegration(this.id, includeRelations)
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
     this._integrationChangeHistories = null;
     this._integrationChangeHistoriesPromise = null;
     this._integrationChangeHistoriesSubject.next(null);

     this._integrationCallbackIncidentEventTypes = null;
     this._integrationCallbackIncidentEventTypesPromise = null;
     this._integrationCallbackIncidentEventTypesSubject.next(null);

     this._webhookDeliveryAttempts = null;
     this._webhookDeliveryAttemptsPromise = null;
     this._webhookDeliveryAttemptsSubject.next(null);

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
     * Gets the IntegrationChangeHistories for this Integration.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.integration.IntegrationChangeHistories.then(integrations => { ... })
     *   or
     *   await this.integration.integrations
     *
    */
    public get IntegrationChangeHistories(): Promise<IntegrationChangeHistoryData[]> {
        if (this._integrationChangeHistories !== null) {
            return Promise.resolve(this._integrationChangeHistories);
        }

        if (this._integrationChangeHistoriesPromise !== null) {
            return this._integrationChangeHistoriesPromise;
        }

        // Start the load
        this.loadIntegrationChangeHistories();

        return this._integrationChangeHistoriesPromise!;
    }



    private loadIntegrationChangeHistories(): void {

        this._integrationChangeHistoriesPromise = lastValueFrom(
            IntegrationService.Instance.GetIntegrationChangeHistoriesForIntegration(this.id)
        )
        .then(IntegrationChangeHistories => {
            this._integrationChangeHistories = IntegrationChangeHistories ?? [];
            this._integrationChangeHistoriesSubject.next(this._integrationChangeHistories);
            return this._integrationChangeHistories;
         })
        .catch(err => {
            this._integrationChangeHistories = [];
            this._integrationChangeHistoriesSubject.next(this._integrationChangeHistories);
            throw err;
        })
        .finally(() => {
            this._integrationChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached IntegrationChangeHistory. Call after mutations to force refresh.
     */
    public ClearIntegrationChangeHistoriesCache(): void {
        this._integrationChangeHistories = null;
        this._integrationChangeHistoriesPromise = null;
        this._integrationChangeHistoriesSubject.next(this._integrationChangeHistories);      // Emit to observable
    }

    public get HasIntegrationChangeHistories(): Promise<boolean> {
        return this.IntegrationChangeHistories.then(integrationChangeHistories => integrationChangeHistories.length > 0);
    }


    /**
     *
     * Gets the IntegrationCallbackIncidentEventTypes for this Integration.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.integration.IntegrationCallbackIncidentEventTypes.then(integrations => { ... })
     *   or
     *   await this.integration.integrations
     *
    */
    public get IntegrationCallbackIncidentEventTypes(): Promise<IntegrationCallbackIncidentEventTypeData[]> {
        if (this._integrationCallbackIncidentEventTypes !== null) {
            return Promise.resolve(this._integrationCallbackIncidentEventTypes);
        }

        if (this._integrationCallbackIncidentEventTypesPromise !== null) {
            return this._integrationCallbackIncidentEventTypesPromise;
        }

        // Start the load
        this.loadIntegrationCallbackIncidentEventTypes();

        return this._integrationCallbackIncidentEventTypesPromise!;
    }



    private loadIntegrationCallbackIncidentEventTypes(): void {

        this._integrationCallbackIncidentEventTypesPromise = lastValueFrom(
            IntegrationService.Instance.GetIntegrationCallbackIncidentEventTypesForIntegration(this.id)
        )
        .then(IntegrationCallbackIncidentEventTypes => {
            this._integrationCallbackIncidentEventTypes = IntegrationCallbackIncidentEventTypes ?? [];
            this._integrationCallbackIncidentEventTypesSubject.next(this._integrationCallbackIncidentEventTypes);
            return this._integrationCallbackIncidentEventTypes;
         })
        .catch(err => {
            this._integrationCallbackIncidentEventTypes = [];
            this._integrationCallbackIncidentEventTypesSubject.next(this._integrationCallbackIncidentEventTypes);
            throw err;
        })
        .finally(() => {
            this._integrationCallbackIncidentEventTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached IntegrationCallbackIncidentEventType. Call after mutations to force refresh.
     */
    public ClearIntegrationCallbackIncidentEventTypesCache(): void {
        this._integrationCallbackIncidentEventTypes = null;
        this._integrationCallbackIncidentEventTypesPromise = null;
        this._integrationCallbackIncidentEventTypesSubject.next(this._integrationCallbackIncidentEventTypes);      // Emit to observable
    }

    public get HasIntegrationCallbackIncidentEventTypes(): Promise<boolean> {
        return this.IntegrationCallbackIncidentEventTypes.then(integrationCallbackIncidentEventTypes => integrationCallbackIncidentEventTypes.length > 0);
    }


    /**
     *
     * Gets the WebhookDeliveryAttempts for this Integration.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.integration.WebhookDeliveryAttempts.then(integrations => { ... })
     *   or
     *   await this.integration.integrations
     *
    */
    public get WebhookDeliveryAttempts(): Promise<WebhookDeliveryAttemptData[]> {
        if (this._webhookDeliveryAttempts !== null) {
            return Promise.resolve(this._webhookDeliveryAttempts);
        }

        if (this._webhookDeliveryAttemptsPromise !== null) {
            return this._webhookDeliveryAttemptsPromise;
        }

        // Start the load
        this.loadWebhookDeliveryAttempts();

        return this._webhookDeliveryAttemptsPromise!;
    }



    private loadWebhookDeliveryAttempts(): void {

        this._webhookDeliveryAttemptsPromise = lastValueFrom(
            IntegrationService.Instance.GetWebhookDeliveryAttemptsForIntegration(this.id)
        )
        .then(WebhookDeliveryAttempts => {
            this._webhookDeliveryAttempts = WebhookDeliveryAttempts ?? [];
            this._webhookDeliveryAttemptsSubject.next(this._webhookDeliveryAttempts);
            return this._webhookDeliveryAttempts;
         })
        .catch(err => {
            this._webhookDeliveryAttempts = [];
            this._webhookDeliveryAttemptsSubject.next(this._webhookDeliveryAttempts);
            throw err;
        })
        .finally(() => {
            this._webhookDeliveryAttemptsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached WebhookDeliveryAttempt. Call after mutations to force refresh.
     */
    public ClearWebhookDeliveryAttemptsCache(): void {
        this._webhookDeliveryAttempts = null;
        this._webhookDeliveryAttemptsPromise = null;
        this._webhookDeliveryAttemptsSubject.next(this._webhookDeliveryAttempts);      // Emit to observable
    }

    public get HasWebhookDeliveryAttempts(): Promise<boolean> {
        return this.WebhookDeliveryAttempts.then(webhookDeliveryAttempts => webhookDeliveryAttempts.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (integration.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await integration.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<IntegrationData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<IntegrationData>> {
        const info = await lastValueFrom(
            IntegrationService.Instance.GetIntegrationChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this IntegrationData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this IntegrationData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): IntegrationSubmitData {
        return IntegrationService.Instance.ConvertToIntegrationSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class IntegrationService extends SecureEndpointBase {

    private static _instance: IntegrationService;
    private listCache: Map<string, Observable<Array<IntegrationData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<IntegrationBasicListData>>>;
    private recordCache: Map<string, Observable<IntegrationData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private integrationChangeHistoryService: IntegrationChangeHistoryService,
        private integrationCallbackIncidentEventTypeService: IntegrationCallbackIncidentEventTypeService,
        private webhookDeliveryAttemptService: WebhookDeliveryAttemptService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<IntegrationData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<IntegrationBasicListData>>>();
        this.recordCache = new Map<string, Observable<IntegrationData>>();

        IntegrationService._instance = this;
    }

    public static get Instance(): IntegrationService {
      return IntegrationService._instance;
    }


    public ClearListCaches(config: IntegrationQueryParameters | null = null) {

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


    public ConvertToIntegrationSubmitData(data: IntegrationData): IntegrationSubmitData {

        let output = new IntegrationSubmitData();

        output.id = data.id;
        output.serviceId = data.serviceId;
        output.name = data.name;
        output.description = data.description;
        output.apiKeyHash = data.apiKeyHash;
        output.callbackWebhookUrl = data.callbackWebhookUrl;
        output.maxRetryAttempts = data.maxRetryAttempts;
        output.retryBackoffSeconds = data.retryBackoffSeconds;
        output.lastCallbackSuccessAt = data.lastCallbackSuccessAt;
        output.consecutiveCallbackFailures = data.consecutiveCallbackFailures;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetIntegration(id: bigint | number, includeRelations: boolean = true) : Observable<IntegrationData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const integration$ = this.requestIntegration(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Integration", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, integration$);

            return integration$;
        }

        return this.recordCache.get(configHash) as Observable<IntegrationData>;
    }

    private requestIntegration(id: bigint | number, includeRelations: boolean = true) : Observable<IntegrationData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IntegrationData>(this.baseUrl + 'api/Integration/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveIntegration(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestIntegration(id, includeRelations));
            }));
    }

    public GetIntegrationList(config: IntegrationQueryParameters | any = null) : Observable<Array<IntegrationData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const integrationList$ = this.requestIntegrationList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Integration list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, integrationList$);

            return integrationList$;
        }

        return this.listCache.get(configHash) as Observable<Array<IntegrationData>>;
    }


    private requestIntegrationList(config: IntegrationQueryParameters | any) : Observable <Array<IntegrationData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IntegrationData>>(this.baseUrl + 'api/Integrations', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveIntegrationList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestIntegrationList(config));
            }));
    }

    public GetIntegrationsRowCount(config: IntegrationQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const integrationsRowCount$ = this.requestIntegrationsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Integrations row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, integrationsRowCount$);

            return integrationsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestIntegrationsRowCount(config: IntegrationQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Integrations/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIntegrationsRowCount(config));
            }));
    }

    public GetIntegrationsBasicListData(config: IntegrationQueryParameters | any = null) : Observable<Array<IntegrationBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const integrationsBasicListData$ = this.requestIntegrationsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Integrations basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, integrationsBasicListData$);

            return integrationsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<IntegrationBasicListData>>;
    }


    private requestIntegrationsBasicListData(config: IntegrationQueryParameters | any) : Observable<Array<IntegrationBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IntegrationBasicListData>>(this.baseUrl + 'api/Integrations/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIntegrationsBasicListData(config));
            }));

    }


    public PutIntegration(id: bigint | number, integration: IntegrationSubmitData) : Observable<IntegrationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IntegrationData>(this.baseUrl + 'api/Integration/' + id.toString(), integration, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIntegration(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutIntegration(id, integration));
            }));
    }


    public PostIntegration(integration: IntegrationSubmitData) : Observable<IntegrationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<IntegrationData>(this.baseUrl + 'api/Integration', integration, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIntegration(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostIntegration(integration));
            }));
    }

  
    public DeleteIntegration(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Integration/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteIntegration(id));
            }));
    }

    public RollbackIntegration(id: bigint | number, versionNumber: bigint | number) : Observable<IntegrationData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IntegrationData>(this.baseUrl + 'api/Integration/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIntegration(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackIntegration(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Integration.
     */
    public GetIntegrationChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<IntegrationData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<IntegrationData>>(this.baseUrl + 'api/Integration/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetIntegrationChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Integration.
     */
    public GetIntegrationAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<IntegrationData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<IntegrationData>[]>(this.baseUrl + 'api/Integration/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetIntegrationAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Integration.
     */
    public GetIntegrationVersion(id: bigint | number, version: number): Observable<IntegrationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IntegrationData>(this.baseUrl + 'api/Integration/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveIntegration(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetIntegrationVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Integration at a specific point in time.
     */
    public GetIntegrationStateAtTime(id: bigint | number, time: string): Observable<IntegrationData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IntegrationData>(this.baseUrl + 'api/Integration/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveIntegration(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetIntegrationStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: IntegrationQueryParameters | any): string {

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

    public userIsAlertingIntegrationReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingIntegrationReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.Integrations
        //
        if (userIsAlertingIntegrationReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingIntegrationReader = user.readPermission >= 1;
            } else {
                userIsAlertingIntegrationReader = false;
            }
        }

        return userIsAlertingIntegrationReader;
    }


    public userIsAlertingIntegrationWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingIntegrationWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.Integrations
        //
        if (userIsAlertingIntegrationWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingIntegrationWriter = user.writePermission >= 150;
          } else {
            userIsAlertingIntegrationWriter = false;
          }      
        }

        return userIsAlertingIntegrationWriter;
    }

    public GetIntegrationChangeHistoriesForIntegration(integrationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IntegrationChangeHistoryData[]> {
        return this.integrationChangeHistoryService.GetIntegrationChangeHistoryList({
            integrationId: integrationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetIntegrationCallbackIncidentEventTypesForIntegration(integrationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IntegrationCallbackIncidentEventTypeData[]> {
        return this.integrationCallbackIncidentEventTypeService.GetIntegrationCallbackIncidentEventTypeList({
            integrationId: integrationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetWebhookDeliveryAttemptsForIntegration(integrationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<WebhookDeliveryAttemptData[]> {
        return this.webhookDeliveryAttemptService.GetWebhookDeliveryAttemptList({
            integrationId: integrationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full IntegrationData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the IntegrationData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when IntegrationTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveIntegration(raw: any): IntegrationData {
    if (!raw) return raw;

    //
    // Create a IntegrationData object instance with correct prototype
    //
    const revived = Object.create(IntegrationData.prototype) as IntegrationData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._integrationChangeHistories = null;
    (revived as any)._integrationChangeHistoriesPromise = null;
    (revived as any)._integrationChangeHistoriesSubject = new BehaviorSubject<IntegrationChangeHistoryData[] | null>(null);

    (revived as any)._integrationCallbackIncidentEventTypes = null;
    (revived as any)._integrationCallbackIncidentEventTypesPromise = null;
    (revived as any)._integrationCallbackIncidentEventTypesSubject = new BehaviorSubject<IntegrationCallbackIncidentEventTypeData[] | null>(null);

    (revived as any)._webhookDeliveryAttempts = null;
    (revived as any)._webhookDeliveryAttemptsPromise = null;
    (revived as any)._webhookDeliveryAttemptsSubject = new BehaviorSubject<WebhookDeliveryAttemptData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadIntegrationXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).IntegrationChangeHistories$ = (revived as any)._integrationChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._integrationChangeHistories === null && (revived as any)._integrationChangeHistoriesPromise === null) {
                (revived as any).loadIntegrationChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).IntegrationChangeHistoriesCount$ = IntegrationChangeHistoryService.Instance.GetIntegrationChangeHistoriesRowCount({integrationId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).IntegrationCallbackIncidentEventTypes$ = (revived as any)._integrationCallbackIncidentEventTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._integrationCallbackIncidentEventTypes === null && (revived as any)._integrationCallbackIncidentEventTypesPromise === null) {
                (revived as any).loadIntegrationCallbackIncidentEventTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).IntegrationCallbackIncidentEventTypesCount$ = IntegrationCallbackIncidentEventTypeService.Instance.GetIntegrationCallbackIncidentEventTypesRowCount({integrationId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).WebhookDeliveryAttempts$ = (revived as any)._webhookDeliveryAttemptsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._webhookDeliveryAttempts === null && (revived as any)._webhookDeliveryAttemptsPromise === null) {
                (revived as any).loadWebhookDeliveryAttempts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).WebhookDeliveryAttemptsCount$ = WebhookDeliveryAttemptService.Instance.GetWebhookDeliveryAttemptsRowCount({integrationId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<IntegrationData> | null>(null);

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

  private ReviveIntegrationList(rawList: any[]): IntegrationData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveIntegration(raw));
  }

}
