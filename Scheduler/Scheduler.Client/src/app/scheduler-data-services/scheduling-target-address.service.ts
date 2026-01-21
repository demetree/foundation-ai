/*

   GENERATED SERVICE FOR THE SCHEDULINGTARGETADDRESS TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SchedulingTargetAddress table.

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
import { SchedulingTargetData } from './scheduling-target.service';
import { ClientData } from './client.service';
import { StateProvinceData } from './state-province.service';
import { CountryData } from './country.service';
import { SchedulingTargetAddressChangeHistoryService, SchedulingTargetAddressChangeHistoryData } from './scheduling-target-address-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SchedulingTargetAddressQueryParameters {
    schedulingTargetId: bigint | number | null | undefined = null;
    clientId: bigint | number | null | undefined = null;
    addressLine1: string | null | undefined = null;
    addressLine2: string | null | undefined = null;
    city: string | null | undefined = null;
    postalCode: string | null | undefined = null;
    stateProvinceId: bigint | number | null | undefined = null;
    countryId: bigint | number | null | undefined = null;
    latitude: number | null | undefined = null;
    longitude: number | null | undefined = null;
    label: string | null | undefined = null;
    isPrimary: boolean | null | undefined = null;
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
export class SchedulingTargetAddressSubmitData {
    id!: bigint | number;
    schedulingTargetId!: bigint | number;
    clientId: bigint | number | null = null;
    addressLine1!: string;
    addressLine2: string | null = null;
    city!: string;
    postalCode: string | null = null;
    stateProvinceId!: bigint | number;
    countryId!: bigint | number;
    latitude: number | null = null;
    longitude: number | null = null;
    label: string | null = null;
    isPrimary!: boolean;
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

export class SchedulingTargetAddressBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SchedulingTargetAddressChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `schedulingTargetAddress.SchedulingTargetAddressChildren$` — use with `| async` in templates
//        • Promise:    `schedulingTargetAddress.SchedulingTargetAddressChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="schedulingTargetAddress.SchedulingTargetAddressChildren$ | async"`), or
//        • Access the promise getter (`schedulingTargetAddress.SchedulingTargetAddressChildren` or `await schedulingTargetAddress.SchedulingTargetAddressChildren`)
//    - Simply reading `schedulingTargetAddress.SchedulingTargetAddressChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await schedulingTargetAddress.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SchedulingTargetAddressData {
    id!: bigint | number;
    schedulingTargetId!: bigint | number;
    clientId!: bigint | number;
    addressLine1!: string;
    addressLine2!: string | null;
    city!: string;
    postalCode!: string | null;
    stateProvinceId!: bigint | number;
    countryId!: bigint | number;
    latitude!: number | null;
    longitude!: number | null;
    label!: string | null;
    isPrimary!: boolean;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    client: ClientData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    country: CountryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    schedulingTarget: SchedulingTargetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    stateProvince: StateProvinceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _schedulingTargetAddressChangeHistories: SchedulingTargetAddressChangeHistoryData[] | null = null;
    private _schedulingTargetAddressChangeHistoriesPromise: Promise<SchedulingTargetAddressChangeHistoryData[]> | null  = null;
    private _schedulingTargetAddressChangeHistoriesSubject = new BehaviorSubject<SchedulingTargetAddressChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<SchedulingTargetAddressData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<SchedulingTargetAddressData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SchedulingTargetAddressData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SchedulingTargetAddressChangeHistories$ = this._schedulingTargetAddressChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetAddressChangeHistories === null && this._schedulingTargetAddressChangeHistoriesPromise === null) {
            this.loadSchedulingTargetAddressChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SchedulingTargetAddressChangeHistoriesCount$ = SchedulingTargetAddressChangeHistoryService.Instance.GetSchedulingTargetAddressChangeHistoriesRowCount({schedulingTargetAddressId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SchedulingTargetAddressData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.schedulingTargetAddress.Reload();
  //
  //  Non Async:
  //
  //     schedulingTargetAddress[0].Reload().then(x => {
  //        this.schedulingTargetAddress = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SchedulingTargetAddressService.Instance.GetSchedulingTargetAddress(this.id, includeRelations)
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
     this._schedulingTargetAddressChangeHistories = null;
     this._schedulingTargetAddressChangeHistoriesPromise = null;
     this._schedulingTargetAddressChangeHistoriesSubject.next(null);

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
     * Gets the SchedulingTargetAddressChangeHistories for this SchedulingTargetAddress.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.schedulingTargetAddress.SchedulingTargetAddressChangeHistories.then(schedulingTargetAddresses => { ... })
     *   or
     *   await this.schedulingTargetAddress.schedulingTargetAddresses
     *
    */
    public get SchedulingTargetAddressChangeHistories(): Promise<SchedulingTargetAddressChangeHistoryData[]> {
        if (this._schedulingTargetAddressChangeHistories !== null) {
            return Promise.resolve(this._schedulingTargetAddressChangeHistories);
        }

        if (this._schedulingTargetAddressChangeHistoriesPromise !== null) {
            return this._schedulingTargetAddressChangeHistoriesPromise;
        }

        // Start the load
        this.loadSchedulingTargetAddressChangeHistories();

        return this._schedulingTargetAddressChangeHistoriesPromise!;
    }



    private loadSchedulingTargetAddressChangeHistories(): void {

        this._schedulingTargetAddressChangeHistoriesPromise = lastValueFrom(
            SchedulingTargetAddressService.Instance.GetSchedulingTargetAddressChangeHistoriesForSchedulingTargetAddress(this.id)
        )
        .then(SchedulingTargetAddressChangeHistories => {
            this._schedulingTargetAddressChangeHistories = SchedulingTargetAddressChangeHistories ?? [];
            this._schedulingTargetAddressChangeHistoriesSubject.next(this._schedulingTargetAddressChangeHistories);
            return this._schedulingTargetAddressChangeHistories;
         })
        .catch(err => {
            this._schedulingTargetAddressChangeHistories = [];
            this._schedulingTargetAddressChangeHistoriesSubject.next(this._schedulingTargetAddressChangeHistories);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetAddressChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTargetAddressChangeHistory. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetAddressChangeHistoriesCache(): void {
        this._schedulingTargetAddressChangeHistories = null;
        this._schedulingTargetAddressChangeHistoriesPromise = null;
        this._schedulingTargetAddressChangeHistoriesSubject.next(this._schedulingTargetAddressChangeHistories);      // Emit to observable
    }

    public get HasSchedulingTargetAddressChangeHistories(): Promise<boolean> {
        return this.SchedulingTargetAddressChangeHistories.then(schedulingTargetAddressChangeHistories => schedulingTargetAddressChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (schedulingTargetAddress.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await schedulingTargetAddress.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<SchedulingTargetAddressData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<SchedulingTargetAddressData>> {
        const info = await lastValueFrom(
            SchedulingTargetAddressService.Instance.GetSchedulingTargetAddressChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this SchedulingTargetAddressData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SchedulingTargetAddressData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SchedulingTargetAddressSubmitData {
        return SchedulingTargetAddressService.Instance.ConvertToSchedulingTargetAddressSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SchedulingTargetAddressService extends SecureEndpointBase {

    private static _instance: SchedulingTargetAddressService;
    private listCache: Map<string, Observable<Array<SchedulingTargetAddressData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SchedulingTargetAddressBasicListData>>>;
    private recordCache: Map<string, Observable<SchedulingTargetAddressData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private schedulingTargetAddressChangeHistoryService: SchedulingTargetAddressChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SchedulingTargetAddressData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SchedulingTargetAddressBasicListData>>>();
        this.recordCache = new Map<string, Observable<SchedulingTargetAddressData>>();

        SchedulingTargetAddressService._instance = this;
    }

    public static get Instance(): SchedulingTargetAddressService {
      return SchedulingTargetAddressService._instance;
    }


    public ClearListCaches(config: SchedulingTargetAddressQueryParameters | null = null) {

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


    public ConvertToSchedulingTargetAddressSubmitData(data: SchedulingTargetAddressData): SchedulingTargetAddressSubmitData {

        let output = new SchedulingTargetAddressSubmitData();

        output.id = data.id;
        output.schedulingTargetId = data.schedulingTargetId;
        output.clientId = data.clientId;
        output.addressLine1 = data.addressLine1;
        output.addressLine2 = data.addressLine2;
        output.city = data.city;
        output.postalCode = data.postalCode;
        output.stateProvinceId = data.stateProvinceId;
        output.countryId = data.countryId;
        output.latitude = data.latitude;
        output.longitude = data.longitude;
        output.label = data.label;
        output.isPrimary = data.isPrimary;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSchedulingTargetAddress(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetAddressData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const schedulingTargetAddress$ = this.requestSchedulingTargetAddress(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetAddress", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, schedulingTargetAddress$);

            return schedulingTargetAddress$;
        }

        return this.recordCache.get(configHash) as Observable<SchedulingTargetAddressData>;
    }

    private requestSchedulingTargetAddress(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetAddressData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetAddressData>(this.baseUrl + 'api/SchedulingTargetAddress/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSchedulingTargetAddress(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetAddress(id, includeRelations));
            }));
    }

    public GetSchedulingTargetAddressList(config: SchedulingTargetAddressQueryParameters | any = null) : Observable<Array<SchedulingTargetAddressData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const schedulingTargetAddressList$ = this.requestSchedulingTargetAddressList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetAddress list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, schedulingTargetAddressList$);

            return schedulingTargetAddressList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SchedulingTargetAddressData>>;
    }


    private requestSchedulingTargetAddressList(config: SchedulingTargetAddressQueryParameters | any) : Observable <Array<SchedulingTargetAddressData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetAddressData>>(this.baseUrl + 'api/SchedulingTargetAddresses', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSchedulingTargetAddressList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetAddressList(config));
            }));
    }

    public GetSchedulingTargetAddressesRowCount(config: SchedulingTargetAddressQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const schedulingTargetAddressesRowCount$ = this.requestSchedulingTargetAddressesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetAddresses row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, schedulingTargetAddressesRowCount$);

            return schedulingTargetAddressesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSchedulingTargetAddressesRowCount(config: SchedulingTargetAddressQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SchedulingTargetAddresses/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetAddressesRowCount(config));
            }));
    }

    public GetSchedulingTargetAddressesBasicListData(config: SchedulingTargetAddressQueryParameters | any = null) : Observable<Array<SchedulingTargetAddressBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const schedulingTargetAddressesBasicListData$ = this.requestSchedulingTargetAddressesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetAddresses basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, schedulingTargetAddressesBasicListData$);

            return schedulingTargetAddressesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SchedulingTargetAddressBasicListData>>;
    }


    private requestSchedulingTargetAddressesBasicListData(config: SchedulingTargetAddressQueryParameters | any) : Observable<Array<SchedulingTargetAddressBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetAddressBasicListData>>(this.baseUrl + 'api/SchedulingTargetAddresses/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetAddressesBasicListData(config));
            }));

    }


    public PutSchedulingTargetAddress(id: bigint | number, schedulingTargetAddress: SchedulingTargetAddressSubmitData) : Observable<SchedulingTargetAddressData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SchedulingTargetAddressData>(this.baseUrl + 'api/SchedulingTargetAddress/' + id.toString(), schedulingTargetAddress, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetAddress(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSchedulingTargetAddress(id, schedulingTargetAddress));
            }));
    }


    public PostSchedulingTargetAddress(schedulingTargetAddress: SchedulingTargetAddressSubmitData) : Observable<SchedulingTargetAddressData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SchedulingTargetAddressData>(this.baseUrl + 'api/SchedulingTargetAddress', schedulingTargetAddress, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetAddress(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSchedulingTargetAddress(schedulingTargetAddress));
            }));
    }

  
    public DeleteSchedulingTargetAddress(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SchedulingTargetAddress/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSchedulingTargetAddress(id));
            }));
    }

    public RollbackSchedulingTargetAddress(id: bigint | number, versionNumber: bigint | number) : Observable<SchedulingTargetAddressData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SchedulingTargetAddressData>(this.baseUrl + 'api/SchedulingTargetAddress/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetAddress(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackSchedulingTargetAddress(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a SchedulingTargetAddress.
     */
    public GetSchedulingTargetAddressChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<SchedulingTargetAddressData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SchedulingTargetAddressData>>(this.baseUrl + 'api/SchedulingTargetAddress/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSchedulingTargetAddressChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a SchedulingTargetAddress.
     */
    public GetSchedulingTargetAddressAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<SchedulingTargetAddressData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SchedulingTargetAddressData>[]>(this.baseUrl + 'api/SchedulingTargetAddress/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSchedulingTargetAddressAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a SchedulingTargetAddress.
     */
    public GetSchedulingTargetAddressVersion(id: bigint | number, version: number): Observable<SchedulingTargetAddressData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetAddressData>(this.baseUrl + 'api/SchedulingTargetAddress/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSchedulingTargetAddress(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSchedulingTargetAddressVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a SchedulingTargetAddress at a specific point in time.
     */
    public GetSchedulingTargetAddressStateAtTime(id: bigint | number, time: string): Observable<SchedulingTargetAddressData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetAddressData>(this.baseUrl + 'api/SchedulingTargetAddress/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSchedulingTargetAddress(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSchedulingTargetAddressStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: SchedulingTargetAddressQueryParameters | any): string {

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

    public userIsSchedulerSchedulingTargetAddressReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerSchedulingTargetAddressReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.SchedulingTargetAddresses
        //
        if (userIsSchedulerSchedulingTargetAddressReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerSchedulingTargetAddressReader = user.readPermission >= 1;
            } else {
                userIsSchedulerSchedulingTargetAddressReader = false;
            }
        }

        return userIsSchedulerSchedulingTargetAddressReader;
    }


    public userIsSchedulerSchedulingTargetAddressWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerSchedulingTargetAddressWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.SchedulingTargetAddresses
        //
        if (userIsSchedulerSchedulingTargetAddressWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerSchedulingTargetAddressWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerSchedulingTargetAddressWriter = false;
          }      
        }

        return userIsSchedulerSchedulingTargetAddressWriter;
    }

    public GetSchedulingTargetAddressChangeHistoriesForSchedulingTargetAddress(schedulingTargetAddressId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetAddressChangeHistoryData[]> {
        return this.schedulingTargetAddressChangeHistoryService.GetSchedulingTargetAddressChangeHistoryList({
            schedulingTargetAddressId: schedulingTargetAddressId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SchedulingTargetAddressData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SchedulingTargetAddressData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SchedulingTargetAddressTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSchedulingTargetAddress(raw: any): SchedulingTargetAddressData {
    if (!raw) return raw;

    //
    // Create a SchedulingTargetAddressData object instance with correct prototype
    //
    const revived = Object.create(SchedulingTargetAddressData.prototype) as SchedulingTargetAddressData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._schedulingTargetAddressChangeHistories = null;
    (revived as any)._schedulingTargetAddressChangeHistoriesPromise = null;
    (revived as any)._schedulingTargetAddressChangeHistoriesSubject = new BehaviorSubject<SchedulingTargetAddressChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSchedulingTargetAddressXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SchedulingTargetAddressChangeHistories$ = (revived as any)._schedulingTargetAddressChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetAddressChangeHistories === null && (revived as any)._schedulingTargetAddressChangeHistoriesPromise === null) {
                (revived as any).loadSchedulingTargetAddressChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SchedulingTargetAddressChangeHistoriesCount$ = SchedulingTargetAddressChangeHistoryService.Instance.GetSchedulingTargetAddressChangeHistoriesRowCount({schedulingTargetAddressId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SchedulingTargetAddressData> | null>(null);

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

  private ReviveSchedulingTargetAddressList(rawList: any[]): SchedulingTargetAddressData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSchedulingTargetAddress(raw));
  }

}
