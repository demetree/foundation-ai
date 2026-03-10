/*

   GENERATED SERVICE FOR THE TENANTPROFILE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the TenantProfile table.

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
import { StateProvinceData } from './state-province.service';
import { CountryData } from './country.service';
import { TimeZoneData } from './time-zone.service';
import { TenantProfileChangeHistoryService, TenantProfileChangeHistoryData } from './tenant-profile-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class TenantProfileQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    companyLogoFileName: string | null | undefined = null;
    companyLogoSize: bigint | number | null | undefined = null;
    companyLogoMimeType: string | null | undefined = null;
    addressLine1: string | null | undefined = null;
    addressLine2: string | null | undefined = null;
    addressLine3: string | null | undefined = null;
    city: string | null | undefined = null;
    postalCode: string | null | undefined = null;
    stateProvinceId: bigint | number | null | undefined = null;
    countryId: bigint | number | null | undefined = null;
    timeZoneId: bigint | number | null | undefined = null;
    phoneNumber: string | null | undefined = null;
    email: string | null | undefined = null;
    website: string | null | undefined = null;
    latitude: number | null | undefined = null;
    longitude: number | null | undefined = null;
    primaryColor: string | null | undefined = null;
    secondaryColor: string | null | undefined = null;
    displaysMetric: boolean | null | undefined = null;
    displaysUSTerms: boolean | null | undefined = null;
    invoiceNumberMask: string | null | undefined = null;
    receiptNumberMask: string | null | undefined = null;
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
export class TenantProfileSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    companyLogoFileName: string | null = null;
    companyLogoSize: bigint | number | null = null;
    companyLogoData: string | null = null;
    companyLogoMimeType: string | null = null;
    addressLine1: string | null = null;
    addressLine2: string | null = null;
    addressLine3: string | null = null;
    city: string | null = null;
    postalCode: string | null = null;
    stateProvinceId: bigint | number | null = null;
    countryId: bigint | number | null = null;
    timeZoneId: bigint | number | null = null;
    phoneNumber: string | null = null;
    email: string | null = null;
    website: string | null = null;
    latitude: number | null = null;
    longitude: number | null = null;
    primaryColor: string | null = null;
    secondaryColor: string | null = null;
    displaysMetric!: boolean;
    displaysUSTerms!: boolean;
    invoiceNumberMask: string | null = null;
    receiptNumberMask: string | null = null;
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

export class TenantProfileBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. TenantProfileChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `tenantProfile.TenantProfileChildren$` — use with `| async` in templates
//        • Promise:    `tenantProfile.TenantProfileChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="tenantProfile.TenantProfileChildren$ | async"`), or
//        • Access the promise getter (`tenantProfile.TenantProfileChildren` or `await tenantProfile.TenantProfileChildren`)
//    - Simply reading `tenantProfile.TenantProfileChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await tenantProfile.Reload()` to refresh the entire object and clear all lazy caches.
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
export class TenantProfileData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    companyLogoFileName!: string | null;
    companyLogoSize!: bigint | number;
    companyLogoData!: string | null;
    companyLogoMimeType!: string | null;
    addressLine1!: string | null;
    addressLine2!: string | null;
    addressLine3!: string | null;
    city!: string | null;
    postalCode!: string | null;
    stateProvinceId!: bigint | number;
    countryId!: bigint | number;
    timeZoneId!: bigint | number;
    phoneNumber!: string | null;
    email!: string | null;
    website!: string | null;
    latitude!: number | null;
    longitude!: number | null;
    primaryColor!: string | null;
    secondaryColor!: string | null;
    displaysMetric!: boolean;
    displaysUSTerms!: boolean;
    invoiceNumberMask!: string | null;
    receiptNumberMask!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    country: CountryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    stateProvince: StateProvinceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    timeZone: TimeZoneData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _tenantProfileChangeHistories: TenantProfileChangeHistoryData[] | null = null;
    private _tenantProfileChangeHistoriesPromise: Promise<TenantProfileChangeHistoryData[]> | null  = null;
    private _tenantProfileChangeHistoriesSubject = new BehaviorSubject<TenantProfileChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<TenantProfileData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<TenantProfileData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<TenantProfileData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public TenantProfileChangeHistories$ = this._tenantProfileChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._tenantProfileChangeHistories === null && this._tenantProfileChangeHistoriesPromise === null) {
            this.loadTenantProfileChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _tenantProfileChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get TenantProfileChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._tenantProfileChangeHistoriesCount$ === null) {
            this._tenantProfileChangeHistoriesCount$ = TenantProfileChangeHistoryService.Instance.GetTenantProfileChangeHistoriesRowCount({tenantProfileId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._tenantProfileChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any TenantProfileData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.tenantProfile.Reload();
  //
  //  Non Async:
  //
  //     tenantProfile[0].Reload().then(x => {
  //        this.tenantProfile = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      TenantProfileService.Instance.GetTenantProfile(this.id, includeRelations)
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
     this._tenantProfileChangeHistories = null;
     this._tenantProfileChangeHistoriesPromise = null;
     this._tenantProfileChangeHistoriesSubject.next(null);
     this._tenantProfileChangeHistoriesCount$ = null;

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
     * Gets the TenantProfileChangeHistories for this TenantProfile.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.tenantProfile.TenantProfileChangeHistories.then(tenantProfiles => { ... })
     *   or
     *   await this.tenantProfile.tenantProfiles
     *
    */
    public get TenantProfileChangeHistories(): Promise<TenantProfileChangeHistoryData[]> {
        if (this._tenantProfileChangeHistories !== null) {
            return Promise.resolve(this._tenantProfileChangeHistories);
        }

        if (this._tenantProfileChangeHistoriesPromise !== null) {
            return this._tenantProfileChangeHistoriesPromise;
        }

        // Start the load
        this.loadTenantProfileChangeHistories();

        return this._tenantProfileChangeHistoriesPromise!;
    }



    private loadTenantProfileChangeHistories(): void {

        this._tenantProfileChangeHistoriesPromise = lastValueFrom(
            TenantProfileService.Instance.GetTenantProfileChangeHistoriesForTenantProfile(this.id)
        )
        .then(TenantProfileChangeHistories => {
            this._tenantProfileChangeHistories = TenantProfileChangeHistories ?? [];
            this._tenantProfileChangeHistoriesSubject.next(this._tenantProfileChangeHistories);
            return this._tenantProfileChangeHistories;
         })
        .catch(err => {
            this._tenantProfileChangeHistories = [];
            this._tenantProfileChangeHistoriesSubject.next(this._tenantProfileChangeHistories);
            throw err;
        })
        .finally(() => {
            this._tenantProfileChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached TenantProfileChangeHistory. Call after mutations to force refresh.
     */
    public ClearTenantProfileChangeHistoriesCache(): void {
        this._tenantProfileChangeHistories = null;
        this._tenantProfileChangeHistoriesPromise = null;
        this._tenantProfileChangeHistoriesSubject.next(this._tenantProfileChangeHistories);      // Emit to observable
    }

    public get HasTenantProfileChangeHistories(): Promise<boolean> {
        return this.TenantProfileChangeHistories.then(tenantProfileChangeHistories => tenantProfileChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (tenantProfile.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await tenantProfile.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<TenantProfileData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<TenantProfileData>> {
        const info = await lastValueFrom(
            TenantProfileService.Instance.GetTenantProfileChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this TenantProfileData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this TenantProfileData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): TenantProfileSubmitData {
        return TenantProfileService.Instance.ConvertToTenantProfileSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class TenantProfileService extends SecureEndpointBase {

    private static _instance: TenantProfileService;
    private listCache: Map<string, Observable<Array<TenantProfileData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<TenantProfileBasicListData>>>;
    private recordCache: Map<string, Observable<TenantProfileData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private tenantProfileChangeHistoryService: TenantProfileChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<TenantProfileData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<TenantProfileBasicListData>>>();
        this.recordCache = new Map<string, Observable<TenantProfileData>>();

        TenantProfileService._instance = this;
    }

    public static get Instance(): TenantProfileService {
      return TenantProfileService._instance;
    }


    public ClearListCaches(config: TenantProfileQueryParameters | null = null) {

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


    public ConvertToTenantProfileSubmitData(data: TenantProfileData): TenantProfileSubmitData {

        let output = new TenantProfileSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.companyLogoFileName = data.companyLogoFileName;
        output.companyLogoSize = data.companyLogoSize;
        output.companyLogoData = data.companyLogoData;
        output.companyLogoMimeType = data.companyLogoMimeType;
        output.addressLine1 = data.addressLine1;
        output.addressLine2 = data.addressLine2;
        output.addressLine3 = data.addressLine3;
        output.city = data.city;
        output.postalCode = data.postalCode;
        output.stateProvinceId = data.stateProvinceId;
        output.countryId = data.countryId;
        output.timeZoneId = data.timeZoneId;
        output.phoneNumber = data.phoneNumber;
        output.email = data.email;
        output.website = data.website;
        output.latitude = data.latitude;
        output.longitude = data.longitude;
        output.primaryColor = data.primaryColor;
        output.secondaryColor = data.secondaryColor;
        output.displaysMetric = data.displaysMetric;
        output.displaysUSTerms = data.displaysUSTerms;
        output.invoiceNumberMask = data.invoiceNumberMask;
        output.receiptNumberMask = data.receiptNumberMask;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetTenantProfile(id: bigint | number, includeRelations: boolean = true) : Observable<TenantProfileData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const tenantProfile$ = this.requestTenantProfile(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TenantProfile", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, tenantProfile$);

            return tenantProfile$;
        }

        return this.recordCache.get(configHash) as Observable<TenantProfileData>;
    }

    private requestTenantProfile(id: bigint | number, includeRelations: boolean = true) : Observable<TenantProfileData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TenantProfileData>(this.baseUrl + 'api/TenantProfile/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveTenantProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestTenantProfile(id, includeRelations));
            }));
    }

    public GetTenantProfileList(config: TenantProfileQueryParameters | any = null) : Observable<Array<TenantProfileData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const tenantProfileList$ = this.requestTenantProfileList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TenantProfile list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, tenantProfileList$);

            return tenantProfileList$;
        }

        return this.listCache.get(configHash) as Observable<Array<TenantProfileData>>;
    }


    private requestTenantProfileList(config: TenantProfileQueryParameters | any) : Observable <Array<TenantProfileData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TenantProfileData>>(this.baseUrl + 'api/TenantProfiles', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveTenantProfileList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestTenantProfileList(config));
            }));
    }

    public GetTenantProfilesRowCount(config: TenantProfileQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const tenantProfilesRowCount$ = this.requestTenantProfilesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get TenantProfiles row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, tenantProfilesRowCount$);

            return tenantProfilesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestTenantProfilesRowCount(config: TenantProfileQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/TenantProfiles/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTenantProfilesRowCount(config));
            }));
    }

    public GetTenantProfilesBasicListData(config: TenantProfileQueryParameters | any = null) : Observable<Array<TenantProfileBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const tenantProfilesBasicListData$ = this.requestTenantProfilesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get TenantProfiles basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, tenantProfilesBasicListData$);

            return tenantProfilesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<TenantProfileBasicListData>>;
    }


    private requestTenantProfilesBasicListData(config: TenantProfileQueryParameters | any) : Observable<Array<TenantProfileBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<TenantProfileBasicListData>>(this.baseUrl + 'api/TenantProfiles/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestTenantProfilesBasicListData(config));
            }));

    }


    public PutTenantProfile(id: bigint | number, tenantProfile: TenantProfileSubmitData) : Observable<TenantProfileData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TenantProfileData>(this.baseUrl + 'api/TenantProfile/' + id.toString(), tenantProfile, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTenantProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutTenantProfile(id, tenantProfile));
            }));
    }


    public PostTenantProfile(tenantProfile: TenantProfileSubmitData) : Observable<TenantProfileData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<TenantProfileData>(this.baseUrl + 'api/TenantProfile', tenantProfile, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTenantProfile(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostTenantProfile(tenantProfile));
            }));
    }

  
    public DeleteTenantProfile(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/TenantProfile/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteTenantProfile(id));
            }));
    }

    public RollbackTenantProfile(id: bigint | number, versionNumber: bigint | number) : Observable<TenantProfileData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<TenantProfileData>(this.baseUrl + 'api/TenantProfile/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveTenantProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackTenantProfile(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a TenantProfile.
     */
    public GetTenantProfileChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<TenantProfileData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<TenantProfileData>>(this.baseUrl + 'api/TenantProfile/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetTenantProfileChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a TenantProfile.
     */
    public GetTenantProfileAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<TenantProfileData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<TenantProfileData>[]>(this.baseUrl + 'api/TenantProfile/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetTenantProfileAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a TenantProfile.
     */
    public GetTenantProfileVersion(id: bigint | number, version: number): Observable<TenantProfileData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TenantProfileData>(this.baseUrl + 'api/TenantProfile/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveTenantProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetTenantProfileVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a TenantProfile at a specific point in time.
     */
    public GetTenantProfileStateAtTime(id: bigint | number, time: string): Observable<TenantProfileData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<TenantProfileData>(this.baseUrl + 'api/TenantProfile/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveTenantProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetTenantProfileStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: TenantProfileQueryParameters | any): string {

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

    public userIsSchedulerTenantProfileReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerTenantProfileReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.TenantProfiles
        //
        if (userIsSchedulerTenantProfileReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerTenantProfileReader = user.readPermission >= 1;
            } else {
                userIsSchedulerTenantProfileReader = false;
            }
        }

        return userIsSchedulerTenantProfileReader;
    }


    public userIsSchedulerTenantProfileWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerTenantProfileWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.TenantProfiles
        //
        if (userIsSchedulerTenantProfileWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerTenantProfileWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerTenantProfileWriter = false;
          }      
        }

        return userIsSchedulerTenantProfileWriter;
    }

    public GetTenantProfileChangeHistoriesForTenantProfile(tenantProfileId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TenantProfileChangeHistoryData[]> {
        return this.tenantProfileChangeHistoryService.GetTenantProfileChangeHistoryList({
            tenantProfileId: tenantProfileId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full TenantProfileData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the TenantProfileData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when TenantProfileTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveTenantProfile(raw: any): TenantProfileData {
    if (!raw) return raw;

    //
    // Create a TenantProfileData object instance with correct prototype
    //
    const revived = Object.create(TenantProfileData.prototype) as TenantProfileData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._tenantProfileChangeHistories = null;
    (revived as any)._tenantProfileChangeHistoriesPromise = null;
    (revived as any)._tenantProfileChangeHistoriesSubject = new BehaviorSubject<TenantProfileChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadTenantProfileXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).TenantProfileChangeHistories$ = (revived as any)._tenantProfileChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._tenantProfileChangeHistories === null && (revived as any)._tenantProfileChangeHistoriesPromise === null) {
                (revived as any).loadTenantProfileChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._tenantProfileChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<TenantProfileData> | null>(null);

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

  private ReviveTenantProfileList(rawList: any[]): TenantProfileData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveTenantProfile(raw));
  }

}
