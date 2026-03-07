/*

   GENERATED SERVICE FOR THE ACCOUNTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AccountType table.

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
import { FinancialCategoryService, FinancialCategoryData } from './financial-category.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AccountTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    isRevenue: boolean | null | undefined = null;
    externalMapping: string | null | undefined = null;
    color: string | null | undefined = null;
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
export class AccountTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isRevenue!: boolean;
    externalMapping: string | null = null;
    color: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class AccountTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AccountTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `accountType.AccountTypeChildren$` — use with `| async` in templates
//        • Promise:    `accountType.AccountTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="accountType.AccountTypeChildren$ | async"`), or
//        • Access the promise getter (`accountType.AccountTypeChildren` or `await accountType.AccountTypeChildren`)
//    - Simply reading `accountType.AccountTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await accountType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AccountTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isRevenue!: boolean;
    externalMapping!: string | null;
    color!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _financialCategories: FinancialCategoryData[] | null = null;
    private _financialCategoriesPromise: Promise<FinancialCategoryData[]> | null  = null;
    private _financialCategoriesSubject = new BehaviorSubject<FinancialCategoryData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public FinancialCategories$ = this._financialCategoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._financialCategories === null && this._financialCategoriesPromise === null) {
            this.loadFinancialCategories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _financialCategoriesCount$: Observable<bigint | number> | null = null;
    public get FinancialCategoriesCount$(): Observable<bigint | number> {
        if (this._financialCategoriesCount$ === null) {
            this._financialCategoriesCount$ = FinancialCategoryService.Instance.GetFinancialCategoriesRowCount({accountTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._financialCategoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AccountTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.accountType.Reload();
  //
  //  Non Async:
  //
  //     accountType[0].Reload().then(x => {
  //        this.accountType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AccountTypeService.Instance.GetAccountType(this.id, includeRelations)
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
     this._financialCategories = null;
     this._financialCategoriesPromise = null;
     this._financialCategoriesSubject.next(null);
     this._financialCategoriesCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the FinancialCategories for this AccountType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.accountType.FinancialCategories.then(accountTypes => { ... })
     *   or
     *   await this.accountType.accountTypes
     *
    */
    public get FinancialCategories(): Promise<FinancialCategoryData[]> {
        if (this._financialCategories !== null) {
            return Promise.resolve(this._financialCategories);
        }

        if (this._financialCategoriesPromise !== null) {
            return this._financialCategoriesPromise;
        }

        // Start the load
        this.loadFinancialCategories();

        return this._financialCategoriesPromise!;
    }



    private loadFinancialCategories(): void {

        this._financialCategoriesPromise = lastValueFrom(
            AccountTypeService.Instance.GetFinancialCategoriesForAccountType(this.id)
        )
        .then(FinancialCategories => {
            this._financialCategories = FinancialCategories ?? [];
            this._financialCategoriesSubject.next(this._financialCategories);
            return this._financialCategories;
         })
        .catch(err => {
            this._financialCategories = [];
            this._financialCategoriesSubject.next(this._financialCategories);
            throw err;
        })
        .finally(() => {
            this._financialCategoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached FinancialCategory. Call after mutations to force refresh.
     */
    public ClearFinancialCategoriesCache(): void {
        this._financialCategories = null;
        this._financialCategoriesPromise = null;
        this._financialCategoriesSubject.next(this._financialCategories);      // Emit to observable
    }

    public get HasFinancialCategories(): Promise<boolean> {
        return this.FinancialCategories.then(financialCategories => financialCategories.length > 0);
    }




    /**
     * Updates the state of this AccountTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AccountTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AccountTypeSubmitData {
        return AccountTypeService.Instance.ConvertToAccountTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AccountTypeService extends SecureEndpointBase {

    private static _instance: AccountTypeService;
    private listCache: Map<string, Observable<Array<AccountTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AccountTypeBasicListData>>>;
    private recordCache: Map<string, Observable<AccountTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private financialCategoryService: FinancialCategoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AccountTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AccountTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<AccountTypeData>>();

        AccountTypeService._instance = this;
    }

    public static get Instance(): AccountTypeService {
      return AccountTypeService._instance;
    }


    public ClearListCaches(config: AccountTypeQueryParameters | null = null) {

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


    public ConvertToAccountTypeSubmitData(data: AccountTypeData): AccountTypeSubmitData {

        let output = new AccountTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.isRevenue = data.isRevenue;
        output.externalMapping = data.externalMapping;
        output.color = data.color;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetAccountType(id: bigint | number, includeRelations: boolean = true) : Observable<AccountTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const accountType$ = this.requestAccountType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AccountType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, accountType$);

            return accountType$;
        }

        return this.recordCache.get(configHash) as Observable<AccountTypeData>;
    }

    private requestAccountType(id: bigint | number, includeRelations: boolean = true) : Observable<AccountTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AccountTypeData>(this.baseUrl + 'api/AccountType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAccountType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAccountType(id, includeRelations));
            }));
    }

    public GetAccountTypeList(config: AccountTypeQueryParameters | any = null) : Observable<Array<AccountTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const accountTypeList$ = this.requestAccountTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AccountType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, accountTypeList$);

            return accountTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AccountTypeData>>;
    }


    private requestAccountTypeList(config: AccountTypeQueryParameters | any) : Observable <Array<AccountTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AccountTypeData>>(this.baseUrl + 'api/AccountTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAccountTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAccountTypeList(config));
            }));
    }

    public GetAccountTypesRowCount(config: AccountTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const accountTypesRowCount$ = this.requestAccountTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AccountTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, accountTypesRowCount$);

            return accountTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAccountTypesRowCount(config: AccountTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AccountTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAccountTypesRowCount(config));
            }));
    }

    public GetAccountTypesBasicListData(config: AccountTypeQueryParameters | any = null) : Observable<Array<AccountTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const accountTypesBasicListData$ = this.requestAccountTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AccountTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, accountTypesBasicListData$);

            return accountTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AccountTypeBasicListData>>;
    }


    private requestAccountTypesBasicListData(config: AccountTypeQueryParameters | any) : Observable<Array<AccountTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AccountTypeBasicListData>>(this.baseUrl + 'api/AccountTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAccountTypesBasicListData(config));
            }));

    }


    public PutAccountType(id: bigint | number, accountType: AccountTypeSubmitData) : Observable<AccountTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AccountTypeData>(this.baseUrl + 'api/AccountType/' + id.toString(), accountType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAccountType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAccountType(id, accountType));
            }));
    }


    public PostAccountType(accountType: AccountTypeSubmitData) : Observable<AccountTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AccountTypeData>(this.baseUrl + 'api/AccountType', accountType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAccountType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAccountType(accountType));
            }));
    }

  
    public DeleteAccountType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AccountType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAccountType(id));
            }));
    }


    private getConfigHash(config: AccountTypeQueryParameters | any): string {

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

    public userIsSchedulerAccountTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerAccountTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.AccountTypes
        //
        if (userIsSchedulerAccountTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerAccountTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerAccountTypeReader = false;
            }
        }

        return userIsSchedulerAccountTypeReader;
    }


    public userIsSchedulerAccountTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerAccountTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.AccountTypes
        //
        if (userIsSchedulerAccountTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerAccountTypeWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerAccountTypeWriter = false;
          }      
        }

        return userIsSchedulerAccountTypeWriter;
    }

    public GetFinancialCategoriesForAccountType(accountTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FinancialCategoryData[]> {
        return this.financialCategoryService.GetFinancialCategoryList({
            accountTypeId: accountTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AccountTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AccountTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AccountTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAccountType(raw: any): AccountTypeData {
    if (!raw) return raw;

    //
    // Create a AccountTypeData object instance with correct prototype
    //
    const revived = Object.create(AccountTypeData.prototype) as AccountTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._financialCategories = null;
    (revived as any)._financialCategoriesPromise = null;
    (revived as any)._financialCategoriesSubject = new BehaviorSubject<FinancialCategoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAccountTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).FinancialCategories$ = (revived as any)._financialCategoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._financialCategories === null && (revived as any)._financialCategoriesPromise === null) {
                (revived as any).loadFinancialCategories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._financialCategoriesCount$ = null;



    return revived;
  }

  private ReviveAccountTypeList(rawList: any[]): AccountTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAccountType(raw));
  }

}
