/*

   GENERATED SERVICE FOR THE ESCALATIONPOLICY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the EscalationPolicy table.

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
import { EscalationPolicyChangeHistoryService, EscalationPolicyChangeHistoryData } from './escalation-policy-change-history.service';
import { ServiceService, ServiceData } from './service.service';
import { EscalationRuleService, EscalationRuleData } from './escalation-rule.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class EscalationPolicyQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class EscalationPolicySubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
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

export class EscalationPolicyBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. EscalationPolicyChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `escalationPolicy.EscalationPolicyChildren$` — use with `| async` in templates
//        • Promise:    `escalationPolicy.EscalationPolicyChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="escalationPolicy.EscalationPolicyChildren$ | async"`), or
//        • Access the promise getter (`escalationPolicy.EscalationPolicyChildren` or `await escalationPolicy.EscalationPolicyChildren`)
//    - Simply reading `escalationPolicy.EscalationPolicyChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await escalationPolicy.Reload()` to refresh the entire object and clear all lazy caches.
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
export class EscalationPolicyData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _escalationPolicyChangeHistories: EscalationPolicyChangeHistoryData[] | null = null;
    private _escalationPolicyChangeHistoriesPromise: Promise<EscalationPolicyChangeHistoryData[]> | null  = null;
    private _escalationPolicyChangeHistoriesSubject = new BehaviorSubject<EscalationPolicyChangeHistoryData[] | null>(null);

                
    private _services: ServiceData[] | null = null;
    private _servicesPromise: Promise<ServiceData[]> | null  = null;
    private _servicesSubject = new BehaviorSubject<ServiceData[] | null>(null);

                
    private _escalationRules: EscalationRuleData[] | null = null;
    private _escalationRulesPromise: Promise<EscalationRuleData[]> | null  = null;
    private _escalationRulesSubject = new BehaviorSubject<EscalationRuleData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<EscalationPolicyData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<EscalationPolicyData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<EscalationPolicyData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public EscalationPolicyChangeHistories$ = this._escalationPolicyChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._escalationPolicyChangeHistories === null && this._escalationPolicyChangeHistoriesPromise === null) {
            this.loadEscalationPolicyChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public EscalationPolicyChangeHistoriesCount$ = EscalationPolicyChangeHistoryService.Instance.GetEscalationPolicyChangeHistoriesRowCount({escalationPolicyId: this.id,
      active: true,
      deleted: false
    });



    public Services$ = this._servicesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._services === null && this._servicesPromise === null) {
            this.loadServices(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ServicesCount$ = ServiceService.Instance.GetServicesRowCount({escalationPolicyId: this.id,
      active: true,
      deleted: false
    });



    public EscalationRules$ = this._escalationRulesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._escalationRules === null && this._escalationRulesPromise === null) {
            this.loadEscalationRules(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public EscalationRulesCount$ = EscalationRuleService.Instance.GetEscalationRulesRowCount({escalationPolicyId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any EscalationPolicyData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.escalationPolicy.Reload();
  //
  //  Non Async:
  //
  //     escalationPolicy[0].Reload().then(x => {
  //        this.escalationPolicy = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      EscalationPolicyService.Instance.GetEscalationPolicy(this.id, includeRelations)
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
     this._escalationPolicyChangeHistories = null;
     this._escalationPolicyChangeHistoriesPromise = null;
     this._escalationPolicyChangeHistoriesSubject.next(null);

     this._services = null;
     this._servicesPromise = null;
     this._servicesSubject.next(null);

     this._escalationRules = null;
     this._escalationRulesPromise = null;
     this._escalationRulesSubject.next(null);

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
     * Gets the EscalationPolicyChangeHistories for this EscalationPolicy.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.escalationPolicy.EscalationPolicyChangeHistories.then(escalationPolicies => { ... })
     *   or
     *   await this.escalationPolicy.escalationPolicies
     *
    */
    public get EscalationPolicyChangeHistories(): Promise<EscalationPolicyChangeHistoryData[]> {
        if (this._escalationPolicyChangeHistories !== null) {
            return Promise.resolve(this._escalationPolicyChangeHistories);
        }

        if (this._escalationPolicyChangeHistoriesPromise !== null) {
            return this._escalationPolicyChangeHistoriesPromise;
        }

        // Start the load
        this.loadEscalationPolicyChangeHistories();

        return this._escalationPolicyChangeHistoriesPromise!;
    }



    private loadEscalationPolicyChangeHistories(): void {

        this._escalationPolicyChangeHistoriesPromise = lastValueFrom(
            EscalationPolicyService.Instance.GetEscalationPolicyChangeHistoriesForEscalationPolicy(this.id)
        )
        .then(EscalationPolicyChangeHistories => {
            this._escalationPolicyChangeHistories = EscalationPolicyChangeHistories ?? [];
            this._escalationPolicyChangeHistoriesSubject.next(this._escalationPolicyChangeHistories);
            return this._escalationPolicyChangeHistories;
         })
        .catch(err => {
            this._escalationPolicyChangeHistories = [];
            this._escalationPolicyChangeHistoriesSubject.next(this._escalationPolicyChangeHistories);
            throw err;
        })
        .finally(() => {
            this._escalationPolicyChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EscalationPolicyChangeHistory. Call after mutations to force refresh.
     */
    public ClearEscalationPolicyChangeHistoriesCache(): void {
        this._escalationPolicyChangeHistories = null;
        this._escalationPolicyChangeHistoriesPromise = null;
        this._escalationPolicyChangeHistoriesSubject.next(this._escalationPolicyChangeHistories);      // Emit to observable
    }

    public get HasEscalationPolicyChangeHistories(): Promise<boolean> {
        return this.EscalationPolicyChangeHistories.then(escalationPolicyChangeHistories => escalationPolicyChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Services for this EscalationPolicy.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.escalationPolicy.Services.then(escalationPolicies => { ... })
     *   or
     *   await this.escalationPolicy.escalationPolicies
     *
    */
    public get Services(): Promise<ServiceData[]> {
        if (this._services !== null) {
            return Promise.resolve(this._services);
        }

        if (this._servicesPromise !== null) {
            return this._servicesPromise;
        }

        // Start the load
        this.loadServices();

        return this._servicesPromise!;
    }



    private loadServices(): void {

        this._servicesPromise = lastValueFrom(
            EscalationPolicyService.Instance.GetServicesForEscalationPolicy(this.id)
        )
        .then(Services => {
            this._services = Services ?? [];
            this._servicesSubject.next(this._services);
            return this._services;
         })
        .catch(err => {
            this._services = [];
            this._servicesSubject.next(this._services);
            throw err;
        })
        .finally(() => {
            this._servicesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Service. Call after mutations to force refresh.
     */
    public ClearServicesCache(): void {
        this._services = null;
        this._servicesPromise = null;
        this._servicesSubject.next(this._services);      // Emit to observable
    }

    public get HasServices(): Promise<boolean> {
        return this.Services.then(services => services.length > 0);
    }


    /**
     *
     * Gets the EscalationRules for this EscalationPolicy.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.escalationPolicy.EscalationRules.then(escalationPolicies => { ... })
     *   or
     *   await this.escalationPolicy.escalationPolicies
     *
    */
    public get EscalationRules(): Promise<EscalationRuleData[]> {
        if (this._escalationRules !== null) {
            return Promise.resolve(this._escalationRules);
        }

        if (this._escalationRulesPromise !== null) {
            return this._escalationRulesPromise;
        }

        // Start the load
        this.loadEscalationRules();

        return this._escalationRulesPromise!;
    }



    private loadEscalationRules(): void {

        this._escalationRulesPromise = lastValueFrom(
            EscalationPolicyService.Instance.GetEscalationRulesForEscalationPolicy(this.id)
        )
        .then(EscalationRules => {
            this._escalationRules = EscalationRules ?? [];
            this._escalationRulesSubject.next(this._escalationRules);
            return this._escalationRules;
         })
        .catch(err => {
            this._escalationRules = [];
            this._escalationRulesSubject.next(this._escalationRules);
            throw err;
        })
        .finally(() => {
            this._escalationRulesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EscalationRule. Call after mutations to force refresh.
     */
    public ClearEscalationRulesCache(): void {
        this._escalationRules = null;
        this._escalationRulesPromise = null;
        this._escalationRulesSubject.next(this._escalationRules);      // Emit to observable
    }

    public get HasEscalationRules(): Promise<boolean> {
        return this.EscalationRules.then(escalationRules => escalationRules.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (escalationPolicy.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await escalationPolicy.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<EscalationPolicyData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<EscalationPolicyData>> {
        const info = await lastValueFrom(
            EscalationPolicyService.Instance.GetEscalationPolicyChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this EscalationPolicyData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this EscalationPolicyData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): EscalationPolicySubmitData {
        return EscalationPolicyService.Instance.ConvertToEscalationPolicySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class EscalationPolicyService extends SecureEndpointBase {

    private static _instance: EscalationPolicyService;
    private listCache: Map<string, Observable<Array<EscalationPolicyData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<EscalationPolicyBasicListData>>>;
    private recordCache: Map<string, Observable<EscalationPolicyData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private escalationPolicyChangeHistoryService: EscalationPolicyChangeHistoryService,
        private serviceService: ServiceService,
        private escalationRuleService: EscalationRuleService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<EscalationPolicyData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<EscalationPolicyBasicListData>>>();
        this.recordCache = new Map<string, Observable<EscalationPolicyData>>();

        EscalationPolicyService._instance = this;
    }

    public static get Instance(): EscalationPolicyService {
      return EscalationPolicyService._instance;
    }


    public ClearListCaches(config: EscalationPolicyQueryParameters | null = null) {

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


    public ConvertToEscalationPolicySubmitData(data: EscalationPolicyData): EscalationPolicySubmitData {

        let output = new EscalationPolicySubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetEscalationPolicy(id: bigint | number, includeRelations: boolean = true) : Observable<EscalationPolicyData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const escalationPolicy$ = this.requestEscalationPolicy(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EscalationPolicy", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, escalationPolicy$);

            return escalationPolicy$;
        }

        return this.recordCache.get(configHash) as Observable<EscalationPolicyData>;
    }

    private requestEscalationPolicy(id: bigint | number, includeRelations: boolean = true) : Observable<EscalationPolicyData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EscalationPolicyData>(this.baseUrl + 'api/EscalationPolicy/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveEscalationPolicy(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestEscalationPolicy(id, includeRelations));
            }));
    }

    public GetEscalationPolicyList(config: EscalationPolicyQueryParameters | any = null) : Observable<Array<EscalationPolicyData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const escalationPolicyList$ = this.requestEscalationPolicyList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EscalationPolicy list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, escalationPolicyList$);

            return escalationPolicyList$;
        }

        return this.listCache.get(configHash) as Observable<Array<EscalationPolicyData>>;
    }


    private requestEscalationPolicyList(config: EscalationPolicyQueryParameters | any) : Observable <Array<EscalationPolicyData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EscalationPolicyData>>(this.baseUrl + 'api/EscalationPolicies', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveEscalationPolicyList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestEscalationPolicyList(config));
            }));
    }

    public GetEscalationPoliciesRowCount(config: EscalationPolicyQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const escalationPoliciesRowCount$ = this.requestEscalationPoliciesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EscalationPolicies row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, escalationPoliciesRowCount$);

            return escalationPoliciesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestEscalationPoliciesRowCount(config: EscalationPolicyQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/EscalationPolicies/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEscalationPoliciesRowCount(config));
            }));
    }

    public GetEscalationPoliciesBasicListData(config: EscalationPolicyQueryParameters | any = null) : Observable<Array<EscalationPolicyBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const escalationPoliciesBasicListData$ = this.requestEscalationPoliciesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EscalationPolicies basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, escalationPoliciesBasicListData$);

            return escalationPoliciesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<EscalationPolicyBasicListData>>;
    }


    private requestEscalationPoliciesBasicListData(config: EscalationPolicyQueryParameters | any) : Observable<Array<EscalationPolicyBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EscalationPolicyBasicListData>>(this.baseUrl + 'api/EscalationPolicies/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEscalationPoliciesBasicListData(config));
            }));

    }


    public PutEscalationPolicy(id: bigint | number, escalationPolicy: EscalationPolicySubmitData) : Observable<EscalationPolicyData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EscalationPolicyData>(this.baseUrl + 'api/EscalationPolicy/' + id.toString(), escalationPolicy, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEscalationPolicy(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutEscalationPolicy(id, escalationPolicy));
            }));
    }


    public PostEscalationPolicy(escalationPolicy: EscalationPolicySubmitData) : Observable<EscalationPolicyData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<EscalationPolicyData>(this.baseUrl + 'api/EscalationPolicy', escalationPolicy, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEscalationPolicy(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostEscalationPolicy(escalationPolicy));
            }));
    }

  
    public DeleteEscalationPolicy(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/EscalationPolicy/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteEscalationPolicy(id));
            }));
    }

    public RollbackEscalationPolicy(id: bigint | number, versionNumber: bigint | number) : Observable<EscalationPolicyData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EscalationPolicyData>(this.baseUrl + 'api/EscalationPolicy/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEscalationPolicy(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackEscalationPolicy(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a EscalationPolicy.
     */
    public GetEscalationPolicyChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<EscalationPolicyData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<EscalationPolicyData>>(this.baseUrl + 'api/EscalationPolicy/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetEscalationPolicyChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a EscalationPolicy.
     */
    public GetEscalationPolicyAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<EscalationPolicyData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<EscalationPolicyData>[]>(this.baseUrl + 'api/EscalationPolicy/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetEscalationPolicyAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a EscalationPolicy.
     */
    public GetEscalationPolicyVersion(id: bigint | number, version: number): Observable<EscalationPolicyData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EscalationPolicyData>(this.baseUrl + 'api/EscalationPolicy/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveEscalationPolicy(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetEscalationPolicyVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a EscalationPolicy at a specific point in time.
     */
    public GetEscalationPolicyStateAtTime(id: bigint | number, time: string): Observable<EscalationPolicyData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EscalationPolicyData>(this.baseUrl + 'api/EscalationPolicy/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveEscalationPolicy(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetEscalationPolicyStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: EscalationPolicyQueryParameters | any): string {

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

    public userIsAlertingEscalationPolicyReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingEscalationPolicyReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.EscalationPolicies
        //
        if (userIsAlertingEscalationPolicyReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingEscalationPolicyReader = user.readPermission >= 1;
            } else {
                userIsAlertingEscalationPolicyReader = false;
            }
        }

        return userIsAlertingEscalationPolicyReader;
    }


    public userIsAlertingEscalationPolicyWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingEscalationPolicyWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.EscalationPolicies
        //
        if (userIsAlertingEscalationPolicyWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingEscalationPolicyWriter = user.writePermission >= 150;
          } else {
            userIsAlertingEscalationPolicyWriter = false;
          }      
        }

        return userIsAlertingEscalationPolicyWriter;
    }

    public GetEscalationPolicyChangeHistoriesForEscalationPolicy(escalationPolicyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EscalationPolicyChangeHistoryData[]> {
        return this.escalationPolicyChangeHistoryService.GetEscalationPolicyChangeHistoryList({
            escalationPolicyId: escalationPolicyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetServicesForEscalationPolicy(escalationPolicyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ServiceData[]> {
        return this.serviceService.GetServiceList({
            escalationPolicyId: escalationPolicyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEscalationRulesForEscalationPolicy(escalationPolicyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EscalationRuleData[]> {
        return this.escalationRuleService.GetEscalationRuleList({
            escalationPolicyId: escalationPolicyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full EscalationPolicyData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the EscalationPolicyData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when EscalationPolicyTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveEscalationPolicy(raw: any): EscalationPolicyData {
    if (!raw) return raw;

    //
    // Create a EscalationPolicyData object instance with correct prototype
    //
    const revived = Object.create(EscalationPolicyData.prototype) as EscalationPolicyData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._escalationPolicyChangeHistories = null;
    (revived as any)._escalationPolicyChangeHistoriesPromise = null;
    (revived as any)._escalationPolicyChangeHistoriesSubject = new BehaviorSubject<EscalationPolicyChangeHistoryData[] | null>(null);

    (revived as any)._services = null;
    (revived as any)._servicesPromise = null;
    (revived as any)._servicesSubject = new BehaviorSubject<ServiceData[] | null>(null);

    (revived as any)._escalationRules = null;
    (revived as any)._escalationRulesPromise = null;
    (revived as any)._escalationRulesSubject = new BehaviorSubject<EscalationRuleData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadEscalationPolicyXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).EscalationPolicyChangeHistories$ = (revived as any)._escalationPolicyChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._escalationPolicyChangeHistories === null && (revived as any)._escalationPolicyChangeHistoriesPromise === null) {
                (revived as any).loadEscalationPolicyChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).EscalationPolicyChangeHistoriesCount$ = EscalationPolicyChangeHistoryService.Instance.GetEscalationPolicyChangeHistoriesRowCount({escalationPolicyId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Services$ = (revived as any)._servicesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._services === null && (revived as any)._servicesPromise === null) {
                (revived as any).loadServices();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ServicesCount$ = ServiceService.Instance.GetServicesRowCount({escalationPolicyId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).EscalationRules$ = (revived as any)._escalationRulesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._escalationRules === null && (revived as any)._escalationRulesPromise === null) {
                (revived as any).loadEscalationRules();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).EscalationRulesCount$ = EscalationRuleService.Instance.GetEscalationRulesRowCount({escalationPolicyId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<EscalationPolicyData> | null>(null);

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

  private ReviveEscalationPolicyList(rawList: any[]): EscalationPolicyData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveEscalationPolicy(raw));
  }

}
