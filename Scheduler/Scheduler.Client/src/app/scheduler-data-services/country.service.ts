/*

   GENERATED SERVICE FOR THE COUNTRY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Country table.

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
import { StateProvinceService, StateProvinceData } from './state-province.service';
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
export class CountryQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    abbreviation: string | null | undefined = null;
    postalCodeFormat: string | null | undefined = null;
    postalCodeRegEx: string | null | undefined = null;
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
export class CountrySubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    abbreviation!: string;
    postalCodeFormat: string | null = null;
    postalCodeRegEx: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class CountryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. CountryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `country.CountryChildren$` — use with `| async` in templates
//        • Promise:    `country.CountryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="country.CountryChildren$ | async"`), or
//        • Access the promise getter (`country.CountryChildren` or `await country.CountryChildren`)
//    - Simply reading `country.CountryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await country.Reload()` to refresh the entire object and clear all lazy caches.
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
export class CountryData {
    id!: bigint | number;
    name!: string;
    description!: string;
    abbreviation!: string;
    postalCodeFormat!: string | null;
    postalCodeRegEx!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _stateProvinces: StateProvinceData[] | null = null;
    private _stateProvincesPromise: Promise<StateProvinceData[]> | null  = null;
    private _stateProvincesSubject = new BehaviorSubject<StateProvinceData[] | null>(null);

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
    public StateProvinces$ = this._stateProvincesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._stateProvinces === null && this._stateProvincesPromise === null) {
            this.loadStateProvinces(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public StateProvincesCount$ = CountryService.Instance.GetCountriesRowCount({countryId: this.id,
      active: true,
      deleted: false
    });



    public Offices$ = this._officesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._offices === null && this._officesPromise === null) {
            this.loadOffices(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public OfficesCount$ = CountryService.Instance.GetCountriesRowCount({countryId: this.id,
      active: true,
      deleted: false
    });



    public Clients$ = this._clientsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._clients === null && this._clientsPromise === null) {
            this.loadClients(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ClientsCount$ = CountryService.Instance.GetCountriesRowCount({countryId: this.id,
      active: true,
      deleted: false
    });



    public TenantProfiles$ = this._tenantProfilesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._tenantProfiles === null && this._tenantProfilesPromise === null) {
            this.loadTenantProfiles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public TenantProfilesCount$ = CountryService.Instance.GetCountriesRowCount({countryId: this.id,
      active: true,
      deleted: false
    });



    public SchedulingTargetAddresses$ = this._schedulingTargetAddressesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetAddresses === null && this._schedulingTargetAddressesPromise === null) {
            this.loadSchedulingTargetAddresses(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SchedulingTargetAddressesCount$ = CountryService.Instance.GetCountriesRowCount({countryId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any CountryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.country.Reload();
  //
  //  Non Async:
  //
  //     country[0].Reload().then(x => {
  //        this.country = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      CountryService.Instance.GetCountry(this.id, includeRelations)
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
     this._stateProvinces = null;
     this._stateProvincesPromise = null;
     this._stateProvincesSubject.next(null);

     this._offices = null;
     this._officesPromise = null;
     this._officesSubject.next(null);

     this._clients = null;
     this._clientsPromise = null;
     this._clientsSubject.next(null);

     this._tenantProfiles = null;
     this._tenantProfilesPromise = null;
     this._tenantProfilesSubject.next(null);

     this._schedulingTargetAddresses = null;
     this._schedulingTargetAddressesPromise = null;
     this._schedulingTargetAddressesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the StateProvinces for this Country.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.country.StateProvinces.then(stateProvinces => { ... })
     *   or
     *   await this.country.StateProvinces
     *
    */
    public get StateProvinces(): Promise<StateProvinceData[]> {
        if (this._stateProvinces !== null) {
            return Promise.resolve(this._stateProvinces);
        }

        if (this._stateProvincesPromise !== null) {
            return this._stateProvincesPromise;
        }

        // Start the load
        this.loadStateProvinces();

        return this._stateProvincesPromise!;
    }



    private loadStateProvinces(): void {

        this._stateProvincesPromise = lastValueFrom(
            CountryService.Instance.GetStateProvincesForCountry(this.id)
        )
        .then(stateProvinces => {
            this._stateProvinces = stateProvinces ?? [];
            this._stateProvincesSubject.next(this._stateProvinces);
            return this._stateProvinces;
         })
        .catch(err => {
            this._stateProvinces = [];
            this._stateProvincesSubject.next(this._stateProvinces);
            throw err;
        })
        .finally(() => {
            this._stateProvincesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached StateProvince. Call after mutations to force refresh.
     */
    public ClearStateProvincesCache(): void {
        this._stateProvinces = null;
        this._stateProvincesPromise = null;
        this._stateProvincesSubject.next(this._stateProvinces);      // Emit to observable
    }

    public get HasStateProvinces(): Promise<boolean> {
        return this.StateProvinces.then(stateProvinces => stateProvinces.length > 0);
    }


    /**
     *
     * Gets the Offices for this Country.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.country.Offices.then(offices => { ... })
     *   or
     *   await this.country.Offices
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
            CountryService.Instance.GetOfficesForCountry(this.id)
        )
        .then(offices => {
            this._offices = offices ?? [];
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
     * Gets the Clients for this Country.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.country.Clients.then(clients => { ... })
     *   or
     *   await this.country.Clients
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
            CountryService.Instance.GetClientsForCountry(this.id)
        )
        .then(clients => {
            this._clients = clients ?? [];
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
     * Gets the TenantProfiles for this Country.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.country.TenantProfiles.then(tenantProfiles => { ... })
     *   or
     *   await this.country.TenantProfiles
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
            CountryService.Instance.GetTenantProfilesForCountry(this.id)
        )
        .then(tenantProfiles => {
            this._tenantProfiles = tenantProfiles ?? [];
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
     * Gets the SchedulingTargetAddresses for this Country.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.country.SchedulingTargetAddresses.then(schedulingTargetAddresses => { ... })
     *   or
     *   await this.country.SchedulingTargetAddresses
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
            CountryService.Instance.GetSchedulingTargetAddressesForCountry(this.id)
        )
        .then(schedulingTargetAddresses => {
            this._schedulingTargetAddresses = schedulingTargetAddresses ?? [];
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
     * Updates the state of this CountryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this CountryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): CountrySubmitData {
        return CountryService.Instance.ConvertToCountrySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class CountryService extends SecureEndpointBase {

    private static _instance: CountryService;
    private listCache: Map<string, Observable<Array<CountryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<CountryBasicListData>>>;
    private recordCache: Map<string, Observable<CountryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private stateProvinceService: StateProvinceService,
        private officeService: OfficeService,
        private clientService: ClientService,
        private tenantProfileService: TenantProfileService,
        private schedulingTargetAddressService: SchedulingTargetAddressService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<CountryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<CountryBasicListData>>>();
        this.recordCache = new Map<string, Observable<CountryData>>();

        CountryService._instance = this;
    }

    public static get Instance(): CountryService {
      return CountryService._instance;
    }


    public ClearListCaches(config: CountryQueryParameters | null = null) {

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


    public ConvertToCountrySubmitData(data: CountryData): CountrySubmitData {

        let output = new CountrySubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.abbreviation = data.abbreviation;
        output.postalCodeFormat = data.postalCodeFormat;
        output.postalCodeRegEx = data.postalCodeRegEx;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetCountry(id: bigint | number, includeRelations: boolean = true) : Observable<CountryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const country$ = this.requestCountry(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Country", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, country$);

            return country$;
        }

        return this.recordCache.get(configHash) as Observable<CountryData>;
    }

    private requestCountry(id: bigint | number, includeRelations: boolean = true) : Observable<CountryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CountryData>(this.baseUrl + 'api/Country/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveCountry(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestCountry(id, includeRelations));
            }));
    }

    public GetCountryList(config: CountryQueryParameters | any = null) : Observable<Array<CountryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const countryList$ = this.requestCountryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Country list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, countryList$);

            return countryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<CountryData>>;
    }


    private requestCountryList(config: CountryQueryParameters | any) : Observable <Array<CountryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CountryData>>(this.baseUrl + 'api/Countries', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveCountryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestCountryList(config));
            }));
    }

    public GetCountriesRowCount(config: CountryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const countriesRowCount$ = this.requestCountriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Countries row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, countriesRowCount$);

            return countriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestCountriesRowCount(config: CountryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Countries/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCountriesRowCount(config));
            }));
    }

    public GetCountriesBasicListData(config: CountryQueryParameters | any = null) : Observable<Array<CountryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const countriesBasicListData$ = this.requestCountriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Countries basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, countriesBasicListData$);

            return countriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<CountryBasicListData>>;
    }


    private requestCountriesBasicListData(config: CountryQueryParameters | any) : Observable<Array<CountryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CountryBasicListData>>(this.baseUrl + 'api/Countries/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCountriesBasicListData(config));
            }));

    }


    public PutCountry(id: bigint | number, country: CountrySubmitData) : Observable<CountryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CountryData>(this.baseUrl + 'api/Country/' + id.toString(), country, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCountry(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutCountry(id, country));
            }));
    }


    public PostCountry(country: CountrySubmitData) : Observable<CountryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<CountryData>(this.baseUrl + 'api/Country', country, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCountry(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostCountry(country));
            }));
    }

  
    public DeleteCountry(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Country/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteCountry(id));
            }));
    }


    private getConfigHash(config: CountryQueryParameters | any): string {

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

    public userIsSchedulerCountryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerCountryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Countries
        //
        if (userIsSchedulerCountryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerCountryReader = user.readPermission >= 0;
            } else {
                userIsSchedulerCountryReader = false;
            }
        }

        return userIsSchedulerCountryReader;
    }


    public userIsSchedulerCountryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerCountryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Countries
        //
        if (userIsSchedulerCountryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerCountryWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerCountryWriter = false;
          }      
        }

        return userIsSchedulerCountryWriter;
    }

    public GetStateProvincesForCountry(countryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<StateProvinceData[]> {
        return this.stateProvinceService.GetStateProvinceList({
            countryId: countryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetOfficesForCountry(countryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<OfficeData[]> {
        return this.officeService.GetOfficeList({
            countryId: countryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetClientsForCountry(countryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ClientData[]> {
        return this.clientService.GetClientList({
            countryId: countryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetTenantProfilesForCountry(countryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TenantProfileData[]> {
        return this.tenantProfileService.GetTenantProfileList({
            countryId: countryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetAddressesForCountry(countryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetAddressData[]> {
        return this.schedulingTargetAddressService.GetSchedulingTargetAddressList({
            countryId: countryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full CountryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the CountryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when CountryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveCountry(raw: any): CountryData {
    if (!raw) return raw;

    //
    // Create a CountryData object instance with correct prototype
    //
    const revived = Object.create(CountryData.prototype) as CountryData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._stateProvinces = null;
    (revived as any)._stateProvincesPromise = null;
    (revived as any)._stateProvincesSubject = new BehaviorSubject<StateProvinceData[] | null>(null);

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
    // 2. But private methods (loadCountryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).StateProvinces$ = (revived as any)._stateProvincesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._stateProvinces === null && (revived as any)._stateProvincesPromise === null) {
                (revived as any).loadStateProvinces();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).StateProvincesCount$ = StateProvinceService.Instance.GetStateProvincesRowCount({countryId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Offices$ = (revived as any)._officesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._offices === null && (revived as any)._officesPromise === null) {
                (revived as any).loadOffices();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).OfficesCount$ = OfficeService.Instance.GetOfficesRowCount({countryId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Clients$ = (revived as any)._clientsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._clients === null && (revived as any)._clientsPromise === null) {
                (revived as any).loadClients();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ClientsCount$ = ClientService.Instance.GetClientsRowCount({countryId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).TenantProfiles$ = (revived as any)._tenantProfilesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._tenantProfiles === null && (revived as any)._tenantProfilesPromise === null) {
                (revived as any).loadTenantProfiles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).TenantProfilesCount$ = TenantProfileService.Instance.GetTenantProfilesRowCount({countryId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SchedulingTargetAddresses$ = (revived as any)._schedulingTargetAddressesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetAddresses === null && (revived as any)._schedulingTargetAddressesPromise === null) {
                (revived as any).loadSchedulingTargetAddresses();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SchedulingTargetAddressesCount$ = SchedulingTargetAddressService.Instance.GetSchedulingTargetAddressesRowCount({countryId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveCountryList(rawList: any[]): CountryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveCountry(raw));
  }

}
