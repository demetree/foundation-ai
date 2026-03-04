/*

   GENERATED SERVICE FOR THE STATEPROVINCE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the StateProvince table.

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
import { CountryData } from './country.service';
import { OfficeService, OfficeData } from './office.service';
import { ClientService, ClientData } from './client.service';
import { TenantProfileService, TenantProfileData } from './tenant-profile.service';
import { SchedulingTargetAddressService, SchedulingTargetAddressData } from './scheduling-target-address.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class StateProvinceQueryParameters {
    countryId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    abbreviation: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class StateProvinceSubmitData {
    id!: bigint | number;
    countryId!: bigint | number;
    name!: string;
    description!: string;
    abbreviation!: string;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class StateProvinceBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. StateProvinceChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `stateProvince.StateProvinceChildren$` — use with `| async` in templates
//        • Promise:    `stateProvince.StateProvinceChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="stateProvince.StateProvinceChildren$ | async"`), or
//        • Access the promise getter (`stateProvince.StateProvinceChildren` or `await stateProvince.StateProvinceChildren`)
//    - Simply reading `stateProvince.StateProvinceChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await stateProvince.Reload()` to refresh the entire object and clear all lazy caches.
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
export class StateProvinceData {
    id!: bigint | number;
    countryId!: bigint | number;
    name!: string;
    description!: string;
    abbreviation!: string;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    country: CountryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _offices: OfficeData[] | null = null;
    private _officesPromise: Promise<OfficeData[]> | null  = null;
    private _officesSubject = new BehaviorSubject<OfficeData[] | null>(null);

                
    private _clients: ClientData[] | null = null;
    private _clientsPromise: Promise<ClientData[]> | null  = null;
    private _clientsSubject = new BehaviorSubject<ClientData[] | null>(null);

                
    private _tenantProfiles: TenantProfileData[] | null = null;
    private _tenantProfilesPromise: Promise<TenantProfileData[]> | null  = null;
    private _tenantProfilesSubject = new BehaviorSubject<TenantProfileData[] | null>(null);

                
    private _schedulingTargetAddresses: SchedulingTargetAddressData[] | null = null;
    private _schedulingTargetAddressesPromise: Promise<SchedulingTargetAddressData[]> | null  = null;
    private _schedulingTargetAddressesSubject = new BehaviorSubject<SchedulingTargetAddressData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Offices$ = this._officesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._offices === null && this._officesPromise === null) {
            this.loadOffices(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _officesCount$: Observable<bigint | number> | null = null;
    public get OfficesCount$(): Observable<bigint | number> {
        if (this._officesCount$ === null) {
            this._officesCount$ = OfficeService.Instance.GetOfficesRowCount({stateProvinceId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._officesCount$;
    }



    public Clients$ = this._clientsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._clients === null && this._clientsPromise === null) {
            this.loadClients(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _clientsCount$: Observable<bigint | number> | null = null;
    public get ClientsCount$(): Observable<bigint | number> {
        if (this._clientsCount$ === null) {
            this._clientsCount$ = ClientService.Instance.GetClientsRowCount({stateProvinceId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._clientsCount$;
    }



    public TenantProfiles$ = this._tenantProfilesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._tenantProfiles === null && this._tenantProfilesPromise === null) {
            this.loadTenantProfiles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _tenantProfilesCount$: Observable<bigint | number> | null = null;
    public get TenantProfilesCount$(): Observable<bigint | number> {
        if (this._tenantProfilesCount$ === null) {
            this._tenantProfilesCount$ = TenantProfileService.Instance.GetTenantProfilesRowCount({stateProvinceId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._tenantProfilesCount$;
    }



    public SchedulingTargetAddresses$ = this._schedulingTargetAddressesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetAddresses === null && this._schedulingTargetAddressesPromise === null) {
            this.loadSchedulingTargetAddresses(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _schedulingTargetAddressesCount$: Observable<bigint | number> | null = null;
    public get SchedulingTargetAddressesCount$(): Observable<bigint | number> {
        if (this._schedulingTargetAddressesCount$ === null) {
            this._schedulingTargetAddressesCount$ = SchedulingTargetAddressService.Instance.GetSchedulingTargetAddressesRowCount({stateProvinceId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._schedulingTargetAddressesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any StateProvinceData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.stateProvince.Reload();
  //
  //  Non Async:
  //
  //     stateProvince[0].Reload().then(x => {
  //        this.stateProvince = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      StateProvinceService.Instance.GetStateProvince(this.id, includeRelations)
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
     this._offices = null;
     this._officesPromise = null;
     this._officesSubject.next(null);
     this._officesCount$ = null;

     this._clients = null;
     this._clientsPromise = null;
     this._clientsSubject.next(null);
     this._clientsCount$ = null;

     this._tenantProfiles = null;
     this._tenantProfilesPromise = null;
     this._tenantProfilesSubject.next(null);
     this._tenantProfilesCount$ = null;

     this._schedulingTargetAddresses = null;
     this._schedulingTargetAddressesPromise = null;
     this._schedulingTargetAddressesSubject.next(null);
     this._schedulingTargetAddressesCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Offices for this StateProvince.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.stateProvince.Offices.then(stateProvinces => { ... })
     *   or
     *   await this.stateProvince.stateProvinces
     *
    */
    public get Offices(): Promise<OfficeData[]> {
        if (this._offices !== null) {
            return Promise.resolve(this._offices);
        }

        if (this._officesPromise !== null) {
            return this._officesPromise;
        }

        // Start the load
        this.loadOffices();

        return this._officesPromise!;
    }



    private loadOffices(): void {

        this._officesPromise = lastValueFrom(
            StateProvinceService.Instance.GetOfficesForStateProvince(this.id)
        )
        .then(Offices => {
            this._offices = Offices ?? [];
            this._officesSubject.next(this._offices);
            return this._offices;
         })
        .catch(err => {
            this._offices = [];
            this._officesSubject.next(this._offices);
            throw err;
        })
        .finally(() => {
            this._officesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Office. Call after mutations to force refresh.
     */
    public ClearOfficesCache(): void {
        this._offices = null;
        this._officesPromise = null;
        this._officesSubject.next(this._offices);      // Emit to observable
    }

    public get HasOffices(): Promise<boolean> {
        return this.Offices.then(offices => offices.length > 0);
    }


    /**
     *
     * Gets the Clients for this StateProvince.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.stateProvince.Clients.then(stateProvinces => { ... })
     *   or
     *   await this.stateProvince.stateProvinces
     *
    */
    public get Clients(): Promise<ClientData[]> {
        if (this._clients !== null) {
            return Promise.resolve(this._clients);
        }

        if (this._clientsPromise !== null) {
            return this._clientsPromise;
        }

        // Start the load
        this.loadClients();

        return this._clientsPromise!;
    }



    private loadClients(): void {

        this._clientsPromise = lastValueFrom(
            StateProvinceService.Instance.GetClientsForStateProvince(this.id)
        )
        .then(Clients => {
            this._clients = Clients ?? [];
            this._clientsSubject.next(this._clients);
            return this._clients;
         })
        .catch(err => {
            this._clients = [];
            this._clientsSubject.next(this._clients);
            throw err;
        })
        .finally(() => {
            this._clientsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Client. Call after mutations to force refresh.
     */
    public ClearClientsCache(): void {
        this._clients = null;
        this._clientsPromise = null;
        this._clientsSubject.next(this._clients);      // Emit to observable
    }

    public get HasClients(): Promise<boolean> {
        return this.Clients.then(clients => clients.length > 0);
    }


    /**
     *
     * Gets the TenantProfiles for this StateProvince.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.stateProvince.TenantProfiles.then(stateProvinces => { ... })
     *   or
     *   await this.stateProvince.stateProvinces
     *
    */
    public get TenantProfiles(): Promise<TenantProfileData[]> {
        if (this._tenantProfiles !== null) {
            return Promise.resolve(this._tenantProfiles);
        }

        if (this._tenantProfilesPromise !== null) {
            return this._tenantProfilesPromise;
        }

        // Start the load
        this.loadTenantProfiles();

        return this._tenantProfilesPromise!;
    }



    private loadTenantProfiles(): void {

        this._tenantProfilesPromise = lastValueFrom(
            StateProvinceService.Instance.GetTenantProfilesForStateProvince(this.id)
        )
        .then(TenantProfiles => {
            this._tenantProfiles = TenantProfiles ?? [];
            this._tenantProfilesSubject.next(this._tenantProfiles);
            return this._tenantProfiles;
         })
        .catch(err => {
            this._tenantProfiles = [];
            this._tenantProfilesSubject.next(this._tenantProfiles);
            throw err;
        })
        .finally(() => {
            this._tenantProfilesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached TenantProfile. Call after mutations to force refresh.
     */
    public ClearTenantProfilesCache(): void {
        this._tenantProfiles = null;
        this._tenantProfilesPromise = null;
        this._tenantProfilesSubject.next(this._tenantProfiles);      // Emit to observable
    }

    public get HasTenantProfiles(): Promise<boolean> {
        return this.TenantProfiles.then(tenantProfiles => tenantProfiles.length > 0);
    }


    /**
     *
     * Gets the SchedulingTargetAddresses for this StateProvince.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.stateProvince.SchedulingTargetAddresses.then(stateProvinces => { ... })
     *   or
     *   await this.stateProvince.stateProvinces
     *
    */
    public get SchedulingTargetAddresses(): Promise<SchedulingTargetAddressData[]> {
        if (this._schedulingTargetAddresses !== null) {
            return Promise.resolve(this._schedulingTargetAddresses);
        }

        if (this._schedulingTargetAddressesPromise !== null) {
            return this._schedulingTargetAddressesPromise;
        }

        // Start the load
        this.loadSchedulingTargetAddresses();

        return this._schedulingTargetAddressesPromise!;
    }



    private loadSchedulingTargetAddresses(): void {

        this._schedulingTargetAddressesPromise = lastValueFrom(
            StateProvinceService.Instance.GetSchedulingTargetAddressesForStateProvince(this.id)
        )
        .then(SchedulingTargetAddresses => {
            this._schedulingTargetAddresses = SchedulingTargetAddresses ?? [];
            this._schedulingTargetAddressesSubject.next(this._schedulingTargetAddresses);
            return this._schedulingTargetAddresses;
         })
        .catch(err => {
            this._schedulingTargetAddresses = [];
            this._schedulingTargetAddressesSubject.next(this._schedulingTargetAddresses);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetAddressesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTargetAddress. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetAddressesCache(): void {
        this._schedulingTargetAddresses = null;
        this._schedulingTargetAddressesPromise = null;
        this._schedulingTargetAddressesSubject.next(this._schedulingTargetAddresses);      // Emit to observable
    }

    public get HasSchedulingTargetAddresses(): Promise<boolean> {
        return this.SchedulingTargetAddresses.then(schedulingTargetAddresses => schedulingTargetAddresses.length > 0);
    }




    /**
     * Updates the state of this StateProvinceData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this StateProvinceData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): StateProvinceSubmitData {
        return StateProvinceService.Instance.ConvertToStateProvinceSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class StateProvinceService extends SecureEndpointBase {

    private static _instance: StateProvinceService;
    private listCache: Map<string, Observable<Array<StateProvinceData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<StateProvinceBasicListData>>>;
    private recordCache: Map<string, Observable<StateProvinceData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private officeService: OfficeService,
        private clientService: ClientService,
        private tenantProfileService: TenantProfileService,
        private schedulingTargetAddressService: SchedulingTargetAddressService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<StateProvinceData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<StateProvinceBasicListData>>>();
        this.recordCache = new Map<string, Observable<StateProvinceData>>();

        StateProvinceService._instance = this;
    }

    public static get Instance(): StateProvinceService {
      return StateProvinceService._instance;
    }


    public ClearListCaches(config: StateProvinceQueryParameters | null = null) {

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


    public ConvertToStateProvinceSubmitData(data: StateProvinceData): StateProvinceSubmitData {

        let output = new StateProvinceSubmitData();

        output.id = data.id;
        output.countryId = data.countryId;
        output.name = data.name;
        output.description = data.description;
        output.abbreviation = data.abbreviation;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetStateProvince(id: bigint | number, includeRelations: boolean = true) : Observable<StateProvinceData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const stateProvince$ = this.requestStateProvince(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get StateProvince", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, stateProvince$);

            return stateProvince$;
        }

        return this.recordCache.get(configHash) as Observable<StateProvinceData>;
    }

    private requestStateProvince(id: bigint | number, includeRelations: boolean = true) : Observable<StateProvinceData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<StateProvinceData>(this.baseUrl + 'api/StateProvince/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveStateProvince(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestStateProvince(id, includeRelations));
            }));
    }

    public GetStateProvinceList(config: StateProvinceQueryParameters | any = null) : Observable<Array<StateProvinceData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const stateProvinceList$ = this.requestStateProvinceList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get StateProvince list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, stateProvinceList$);

            return stateProvinceList$;
        }

        return this.listCache.get(configHash) as Observable<Array<StateProvinceData>>;
    }


    private requestStateProvinceList(config: StateProvinceQueryParameters | any) : Observable <Array<StateProvinceData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<StateProvinceData>>(this.baseUrl + 'api/StateProvinces', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveStateProvinceList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestStateProvinceList(config));
            }));
    }

    public GetStateProvincesRowCount(config: StateProvinceQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const stateProvincesRowCount$ = this.requestStateProvincesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get StateProvinces row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, stateProvincesRowCount$);

            return stateProvincesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestStateProvincesRowCount(config: StateProvinceQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/StateProvinces/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestStateProvincesRowCount(config));
            }));
    }

    public GetStateProvincesBasicListData(config: StateProvinceQueryParameters | any = null) : Observable<Array<StateProvinceBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const stateProvincesBasicListData$ = this.requestStateProvincesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get StateProvinces basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, stateProvincesBasicListData$);

            return stateProvincesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<StateProvinceBasicListData>>;
    }


    private requestStateProvincesBasicListData(config: StateProvinceQueryParameters | any) : Observable<Array<StateProvinceBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<StateProvinceBasicListData>>(this.baseUrl + 'api/StateProvinces/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestStateProvincesBasicListData(config));
            }));

    }


    public PutStateProvince(id: bigint | number, stateProvince: StateProvinceSubmitData) : Observable<StateProvinceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<StateProvinceData>(this.baseUrl + 'api/StateProvince/' + id.toString(), stateProvince, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveStateProvince(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutStateProvince(id, stateProvince));
            }));
    }


    public PostStateProvince(stateProvince: StateProvinceSubmitData) : Observable<StateProvinceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<StateProvinceData>(this.baseUrl + 'api/StateProvince', stateProvince, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveStateProvince(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostStateProvince(stateProvince));
            }));
    }

  
    public DeleteStateProvince(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/StateProvince/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteStateProvince(id));
            }));
    }


    private getConfigHash(config: StateProvinceQueryParameters | any): string {

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

    public userIsSchedulerStateProvinceReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerStateProvinceReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.StateProvinces
        //
        if (userIsSchedulerStateProvinceReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerStateProvinceReader = user.readPermission >= 0;
            } else {
                userIsSchedulerStateProvinceReader = false;
            }
        }

        return userIsSchedulerStateProvinceReader;
    }


    public userIsSchedulerStateProvinceWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerStateProvinceWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.StateProvinces
        //
        if (userIsSchedulerStateProvinceWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerStateProvinceWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerStateProvinceWriter = false;
          }      
        }

        return userIsSchedulerStateProvinceWriter;
    }

    public GetOfficesForStateProvince(stateProvinceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<OfficeData[]> {
        return this.officeService.GetOfficeList({
            stateProvinceId: stateProvinceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetClientsForStateProvince(stateProvinceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ClientData[]> {
        return this.clientService.GetClientList({
            stateProvinceId: stateProvinceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetTenantProfilesForStateProvince(stateProvinceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TenantProfileData[]> {
        return this.tenantProfileService.GetTenantProfileList({
            stateProvinceId: stateProvinceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetAddressesForStateProvince(stateProvinceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetAddressData[]> {
        return this.schedulingTargetAddressService.GetSchedulingTargetAddressList({
            stateProvinceId: stateProvinceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full StateProvinceData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the StateProvinceData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when StateProvinceTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveStateProvince(raw: any): StateProvinceData {
    if (!raw) return raw;

    //
    // Create a StateProvinceData object instance with correct prototype
    //
    const revived = Object.create(StateProvinceData.prototype) as StateProvinceData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._offices = null;
    (revived as any)._officesPromise = null;
    (revived as any)._officesSubject = new BehaviorSubject<OfficeData[] | null>(null);

    (revived as any)._clients = null;
    (revived as any)._clientsPromise = null;
    (revived as any)._clientsSubject = new BehaviorSubject<ClientData[] | null>(null);

    (revived as any)._tenantProfiles = null;
    (revived as any)._tenantProfilesPromise = null;
    (revived as any)._tenantProfilesSubject = new BehaviorSubject<TenantProfileData[] | null>(null);

    (revived as any)._schedulingTargetAddresses = null;
    (revived as any)._schedulingTargetAddressesPromise = null;
    (revived as any)._schedulingTargetAddressesSubject = new BehaviorSubject<SchedulingTargetAddressData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadStateProvinceXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Offices$ = (revived as any)._officesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._offices === null && (revived as any)._officesPromise === null) {
                (revived as any).loadOffices();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._officesCount$ = null;


    (revived as any).Clients$ = (revived as any)._clientsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._clients === null && (revived as any)._clientsPromise === null) {
                (revived as any).loadClients();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._clientsCount$ = null;


    (revived as any).TenantProfiles$ = (revived as any)._tenantProfilesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._tenantProfiles === null && (revived as any)._tenantProfilesPromise === null) {
                (revived as any).loadTenantProfiles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._tenantProfilesCount$ = null;


    (revived as any).SchedulingTargetAddresses$ = (revived as any)._schedulingTargetAddressesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetAddresses === null && (revived as any)._schedulingTargetAddressesPromise === null) {
                (revived as any).loadSchedulingTargetAddresses();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._schedulingTargetAddressesCount$ = null;



    return revived;
  }

  private ReviveStateProvinceList(rawList: any[]): StateProvinceData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveStateProvince(raw));
  }

}
