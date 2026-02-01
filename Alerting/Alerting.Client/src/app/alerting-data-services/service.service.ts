/*

   GENERATED SERVICE FOR THE SERVICE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Service table.

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
import { ServiceChangeHistoryService, ServiceChangeHistoryData } from './service-change-history.service';
import { IntegrationService, IntegrationData } from './integration.service';
import { IncidentService, IncidentData } from './incident.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ServiceQueryParameters {
    escalationPolicyId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    ownerTeamObjectGuid: string | null | undefined = null;
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
export class ServiceSubmitData {
    id!: bigint | number;
    escalationPolicyId: bigint | number | null = null;
    name!: string;
    description: string | null = null;
    ownerTeamObjectGuid: string | null = null;
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

export class ServiceBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ServiceChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `service.ServiceChildren$` — use with `| async` in templates
//        • Promise:    `service.ServiceChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="service.ServiceChildren$ | async"`), or
//        • Access the promise getter (`service.ServiceChildren` or `await service.ServiceChildren`)
//    - Simply reading `service.ServiceChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await service.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ServiceData {
    id!: bigint | number;
    escalationPolicyId!: bigint | number;
    name!: string;
    description!: string | null;
    ownerTeamObjectGuid!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    escalationPolicy: EscalationPolicyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _serviceChangeHistories: ServiceChangeHistoryData[] | null = null;
    private _serviceChangeHistoriesPromise: Promise<ServiceChangeHistoryData[]> | null  = null;
    private _serviceChangeHistoriesSubject = new BehaviorSubject<ServiceChangeHistoryData[] | null>(null);

                
    private _integrations: IntegrationData[] | null = null;
    private _integrationsPromise: Promise<IntegrationData[]> | null  = null;
    private _integrationsSubject = new BehaviorSubject<IntegrationData[] | null>(null);

                
    private _incidents: IncidentData[] | null = null;
    private _incidentsPromise: Promise<IncidentData[]> | null  = null;
    private _incidentsSubject = new BehaviorSubject<IncidentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ServiceData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ServiceData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ServiceData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ServiceChangeHistories$ = this._serviceChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._serviceChangeHistories === null && this._serviceChangeHistoriesPromise === null) {
            this.loadServiceChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ServiceChangeHistoriesCount$ = ServiceChangeHistoryService.Instance.GetServiceChangeHistoriesRowCount({serviceId: this.id,
      active: true,
      deleted: false
    });



    public Integrations$ = this._integrationsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._integrations === null && this._integrationsPromise === null) {
            this.loadIntegrations(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public IntegrationsCount$ = IntegrationService.Instance.GetIntegrationsRowCount({serviceId: this.id,
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

  
    public IncidentsCount$ = IncidentService.Instance.GetIncidentsRowCount({serviceId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ServiceData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.service.Reload();
  //
  //  Non Async:
  //
  //     service[0].Reload().then(x => {
  //        this.service = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ServiceService.Instance.GetService(this.id, includeRelations)
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
     this._serviceChangeHistories = null;
     this._serviceChangeHistoriesPromise = null;
     this._serviceChangeHistoriesSubject.next(null);

     this._integrations = null;
     this._integrationsPromise = null;
     this._integrationsSubject.next(null);

     this._incidents = null;
     this._incidentsPromise = null;
     this._incidentsSubject.next(null);

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
     * Gets the ServiceChangeHistories for this Service.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.service.ServiceChangeHistories.then(services => { ... })
     *   or
     *   await this.service.services
     *
    */
    public get ServiceChangeHistories(): Promise<ServiceChangeHistoryData[]> {
        if (this._serviceChangeHistories !== null) {
            return Promise.resolve(this._serviceChangeHistories);
        }

        if (this._serviceChangeHistoriesPromise !== null) {
            return this._serviceChangeHistoriesPromise;
        }

        // Start the load
        this.loadServiceChangeHistories();

        return this._serviceChangeHistoriesPromise!;
    }



    private loadServiceChangeHistories(): void {

        this._serviceChangeHistoriesPromise = lastValueFrom(
            ServiceService.Instance.GetServiceChangeHistoriesForService(this.id)
        )
        .then(ServiceChangeHistories => {
            this._serviceChangeHistories = ServiceChangeHistories ?? [];
            this._serviceChangeHistoriesSubject.next(this._serviceChangeHistories);
            return this._serviceChangeHistories;
         })
        .catch(err => {
            this._serviceChangeHistories = [];
            this._serviceChangeHistoriesSubject.next(this._serviceChangeHistories);
            throw err;
        })
        .finally(() => {
            this._serviceChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ServiceChangeHistory. Call after mutations to force refresh.
     */
    public ClearServiceChangeHistoriesCache(): void {
        this._serviceChangeHistories = null;
        this._serviceChangeHistoriesPromise = null;
        this._serviceChangeHistoriesSubject.next(this._serviceChangeHistories);      // Emit to observable
    }

    public get HasServiceChangeHistories(): Promise<boolean> {
        return this.ServiceChangeHistories.then(serviceChangeHistories => serviceChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Integrations for this Service.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.service.Integrations.then(services => { ... })
     *   or
     *   await this.service.services
     *
    */
    public get Integrations(): Promise<IntegrationData[]> {
        if (this._integrations !== null) {
            return Promise.resolve(this._integrations);
        }

        if (this._integrationsPromise !== null) {
            return this._integrationsPromise;
        }

        // Start the load
        this.loadIntegrations();

        return this._integrationsPromise!;
    }



    private loadIntegrations(): void {

        this._integrationsPromise = lastValueFrom(
            ServiceService.Instance.GetIntegrationsForService(this.id)
        )
        .then(Integrations => {
            this._integrations = Integrations ?? [];
            this._integrationsSubject.next(this._integrations);
            return this._integrations;
         })
        .catch(err => {
            this._integrations = [];
            this._integrationsSubject.next(this._integrations);
            throw err;
        })
        .finally(() => {
            this._integrationsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Integration. Call after mutations to force refresh.
     */
    public ClearIntegrationsCache(): void {
        this._integrations = null;
        this._integrationsPromise = null;
        this._integrationsSubject.next(this._integrations);      // Emit to observable
    }

    public get HasIntegrations(): Promise<boolean> {
        return this.Integrations.then(integrations => integrations.length > 0);
    }


    /**
     *
     * Gets the Incidents for this Service.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.service.Incidents.then(services => { ... })
     *   or
     *   await this.service.services
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
            ServiceService.Instance.GetIncidentsForService(this.id)
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




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (service.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await service.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ServiceData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ServiceData>> {
        const info = await lastValueFrom(
            ServiceService.Instance.GetServiceChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ServiceData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ServiceData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ServiceSubmitData {
        return ServiceService.Instance.ConvertToServiceSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ServiceService extends SecureEndpointBase {

    private static _instance: ServiceService;
    private listCache: Map<string, Observable<Array<ServiceData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ServiceBasicListData>>>;
    private recordCache: Map<string, Observable<ServiceData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private serviceChangeHistoryService: ServiceChangeHistoryService,
        private integrationService: IntegrationService,
        private incidentService: IncidentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ServiceData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ServiceBasicListData>>>();
        this.recordCache = new Map<string, Observable<ServiceData>>();

        ServiceService._instance = this;
    }

    public static get Instance(): ServiceService {
      return ServiceService._instance;
    }


    public ClearListCaches(config: ServiceQueryParameters | null = null) {

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


    public ConvertToServiceSubmitData(data: ServiceData): ServiceSubmitData {

        let output = new ServiceSubmitData();

        output.id = data.id;
        output.escalationPolicyId = data.escalationPolicyId;
        output.name = data.name;
        output.description = data.description;
        output.ownerTeamObjectGuid = data.ownerTeamObjectGuid;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetService(id: bigint | number, includeRelations: boolean = true) : Observable<ServiceData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const service$ = this.requestService(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Service", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, service$);

            return service$;
        }

        return this.recordCache.get(configHash) as Observable<ServiceData>;
    }

    private requestService(id: bigint | number, includeRelations: boolean = true) : Observable<ServiceData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ServiceData>(this.baseUrl + 'api/Service/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveService(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestService(id, includeRelations));
            }));
    }

    public GetServiceList(config: ServiceQueryParameters | any = null) : Observable<Array<ServiceData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const serviceList$ = this.requestServiceList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Service list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, serviceList$);

            return serviceList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ServiceData>>;
    }


    private requestServiceList(config: ServiceQueryParameters | any) : Observable <Array<ServiceData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ServiceData>>(this.baseUrl + 'api/Services', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveServiceList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestServiceList(config));
            }));
    }

    public GetServicesRowCount(config: ServiceQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const servicesRowCount$ = this.requestServicesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Services row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, servicesRowCount$);

            return servicesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestServicesRowCount(config: ServiceQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Services/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestServicesRowCount(config));
            }));
    }

    public GetServicesBasicListData(config: ServiceQueryParameters | any = null) : Observable<Array<ServiceBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const servicesBasicListData$ = this.requestServicesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Services basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, servicesBasicListData$);

            return servicesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ServiceBasicListData>>;
    }


    private requestServicesBasicListData(config: ServiceQueryParameters | any) : Observable<Array<ServiceBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ServiceBasicListData>>(this.baseUrl + 'api/Services/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestServicesBasicListData(config));
            }));

    }


    public PutService(id: bigint | number, service: ServiceSubmitData) : Observable<ServiceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ServiceData>(this.baseUrl + 'api/Service/' + id.toString(), service, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveService(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutService(id, service));
            }));
    }


    public PostService(service: ServiceSubmitData) : Observable<ServiceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ServiceData>(this.baseUrl + 'api/Service', service, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveService(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostService(service));
            }));
    }

  
    public DeleteService(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Service/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteService(id));
            }));
    }

    public RollbackService(id: bigint | number, versionNumber: bigint | number) : Observable<ServiceData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ServiceData>(this.baseUrl + 'api/Service/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveService(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackService(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Service.
     */
    public GetServiceChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ServiceData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ServiceData>>(this.baseUrl + 'api/Service/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetServiceChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Service.
     */
    public GetServiceAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ServiceData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ServiceData>[]>(this.baseUrl + 'api/Service/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetServiceAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Service.
     */
    public GetServiceVersion(id: bigint | number, version: number): Observable<ServiceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ServiceData>(this.baseUrl + 'api/Service/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveService(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetServiceVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Service at a specific point in time.
     */
    public GetServiceStateAtTime(id: bigint | number, time: string): Observable<ServiceData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ServiceData>(this.baseUrl + 'api/Service/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveService(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetServiceStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ServiceQueryParameters | any): string {

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

    public userIsAlertingServiceReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingServiceReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.Services
        //
        if (userIsAlertingServiceReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingServiceReader = user.readPermission >= 0;
            } else {
                userIsAlertingServiceReader = false;
            }
        }

        return userIsAlertingServiceReader;
    }


    public userIsAlertingServiceWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingServiceWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.Services
        //
        if (userIsAlertingServiceWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingServiceWriter = user.writePermission >= 0;
          } else {
            userIsAlertingServiceWriter = false;
          }      
        }

        return userIsAlertingServiceWriter;
    }

    public GetServiceChangeHistoriesForService(serviceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ServiceChangeHistoryData[]> {
        return this.serviceChangeHistoryService.GetServiceChangeHistoryList({
            serviceId: serviceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetIntegrationsForService(serviceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IntegrationData[]> {
        return this.integrationService.GetIntegrationList({
            serviceId: serviceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetIncidentsForService(serviceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IncidentData[]> {
        return this.incidentService.GetIncidentList({
            serviceId: serviceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ServiceData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ServiceData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ServiceTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveService(raw: any): ServiceData {
    if (!raw) return raw;

    //
    // Create a ServiceData object instance with correct prototype
    //
    const revived = Object.create(ServiceData.prototype) as ServiceData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._serviceChangeHistories = null;
    (revived as any)._serviceChangeHistoriesPromise = null;
    (revived as any)._serviceChangeHistoriesSubject = new BehaviorSubject<ServiceChangeHistoryData[] | null>(null);

    (revived as any)._integrations = null;
    (revived as any)._integrationsPromise = null;
    (revived as any)._integrationsSubject = new BehaviorSubject<IntegrationData[] | null>(null);

    (revived as any)._incidents = null;
    (revived as any)._incidentsPromise = null;
    (revived as any)._incidentsSubject = new BehaviorSubject<IncidentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadServiceXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ServiceChangeHistories$ = (revived as any)._serviceChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._serviceChangeHistories === null && (revived as any)._serviceChangeHistoriesPromise === null) {
                (revived as any).loadServiceChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ServiceChangeHistoriesCount$ = ServiceChangeHistoryService.Instance.GetServiceChangeHistoriesRowCount({serviceId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Integrations$ = (revived as any)._integrationsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._integrations === null && (revived as any)._integrationsPromise === null) {
                (revived as any).loadIntegrations();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).IntegrationsCount$ = IntegrationService.Instance.GetIntegrationsRowCount({serviceId: (revived as any).id,
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

    (revived as any).IncidentsCount$ = IncidentService.Instance.GetIncidentsRowCount({serviceId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ServiceData> | null>(null);

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

  private ReviveServiceList(rawList: any[]): ServiceData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveService(raw));
  }

}
