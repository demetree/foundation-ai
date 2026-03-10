/*

   GENERATED SERVICE FOR THE INVOICESTATUS TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the InvoiceStatus table.

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
import { InvoiceService, InvoiceData } from './invoice.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class InvoiceStatusQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class InvoiceStatusSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    color: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class InvoiceStatusBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. InvoiceStatusChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `invoiceStatus.InvoiceStatusChildren$` — use with `| async` in templates
//        • Promise:    `invoiceStatus.InvoiceStatusChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="invoiceStatus.InvoiceStatusChildren$ | async"`), or
//        • Access the promise getter (`invoiceStatus.InvoiceStatusChildren` or `await invoiceStatus.InvoiceStatusChildren`)
//    - Simply reading `invoiceStatus.InvoiceStatusChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await invoiceStatus.Reload()` to refresh the entire object and clear all lazy caches.
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
export class InvoiceStatusData {
    id!: bigint | number;
    name!: string;
    description!: string;
    color!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _invoices: InvoiceData[] | null = null;
    private _invoicesPromise: Promise<InvoiceData[]> | null  = null;
    private _invoicesSubject = new BehaviorSubject<InvoiceData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Invoices$ = this._invoicesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._invoices === null && this._invoicesPromise === null) {
            this.loadInvoices(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _invoicesCount$: Observable<bigint | number> | null = null;
    public get InvoicesCount$(): Observable<bigint | number> {
        if (this._invoicesCount$ === null) {
            this._invoicesCount$ = InvoiceService.Instance.GetInvoicesRowCount({invoiceStatusId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._invoicesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any InvoiceStatusData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.invoiceStatus.Reload();
  //
  //  Non Async:
  //
  //     invoiceStatus[0].Reload().then(x => {
  //        this.invoiceStatus = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      InvoiceStatusService.Instance.GetInvoiceStatus(this.id, includeRelations)
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
     this._invoices = null;
     this._invoicesPromise = null;
     this._invoicesSubject.next(null);
     this._invoicesCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Invoices for this InvoiceStatus.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.invoiceStatus.Invoices.then(invoiceStatuses => { ... })
     *   or
     *   await this.invoiceStatus.invoiceStatuses
     *
    */
    public get Invoices(): Promise<InvoiceData[]> {
        if (this._invoices !== null) {
            return Promise.resolve(this._invoices);
        }

        if (this._invoicesPromise !== null) {
            return this._invoicesPromise;
        }

        // Start the load
        this.loadInvoices();

        return this._invoicesPromise!;
    }



    private loadInvoices(): void {

        this._invoicesPromise = lastValueFrom(
            InvoiceStatusService.Instance.GetInvoicesForInvoiceStatus(this.id)
        )
        .then(Invoices => {
            this._invoices = Invoices ?? [];
            this._invoicesSubject.next(this._invoices);
            return this._invoices;
         })
        .catch(err => {
            this._invoices = [];
            this._invoicesSubject.next(this._invoices);
            throw err;
        })
        .finally(() => {
            this._invoicesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Invoice. Call after mutations to force refresh.
     */
    public ClearInvoicesCache(): void {
        this._invoices = null;
        this._invoicesPromise = null;
        this._invoicesSubject.next(this._invoices);      // Emit to observable
    }

    public get HasInvoices(): Promise<boolean> {
        return this.Invoices.then(invoices => invoices.length > 0);
    }




    /**
     * Updates the state of this InvoiceStatusData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this InvoiceStatusData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): InvoiceStatusSubmitData {
        return InvoiceStatusService.Instance.ConvertToInvoiceStatusSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class InvoiceStatusService extends SecureEndpointBase {

    private static _instance: InvoiceStatusService;
    private listCache: Map<string, Observable<Array<InvoiceStatusData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<InvoiceStatusBasicListData>>>;
    private recordCache: Map<string, Observable<InvoiceStatusData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private invoiceService: InvoiceService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<InvoiceStatusData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<InvoiceStatusBasicListData>>>();
        this.recordCache = new Map<string, Observable<InvoiceStatusData>>();

        InvoiceStatusService._instance = this;
    }

    public static get Instance(): InvoiceStatusService {
      return InvoiceStatusService._instance;
    }


    public ClearListCaches(config: InvoiceStatusQueryParameters | null = null) {

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


    public ConvertToInvoiceStatusSubmitData(data: InvoiceStatusData): InvoiceStatusSubmitData {

        let output = new InvoiceStatusSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.color = data.color;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetInvoiceStatus(id: bigint | number, includeRelations: boolean = true) : Observable<InvoiceStatusData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const invoiceStatus$ = this.requestInvoiceStatus(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get InvoiceStatus", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, invoiceStatus$);

            return invoiceStatus$;
        }

        return this.recordCache.get(configHash) as Observable<InvoiceStatusData>;
    }

    private requestInvoiceStatus(id: bigint | number, includeRelations: boolean = true) : Observable<InvoiceStatusData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<InvoiceStatusData>(this.baseUrl + 'api/InvoiceStatus/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveInvoiceStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestInvoiceStatus(id, includeRelations));
            }));
    }

    public GetInvoiceStatusList(config: InvoiceStatusQueryParameters | any = null) : Observable<Array<InvoiceStatusData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const invoiceStatusList$ = this.requestInvoiceStatusList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get InvoiceStatus list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, invoiceStatusList$);

            return invoiceStatusList$;
        }

        return this.listCache.get(configHash) as Observable<Array<InvoiceStatusData>>;
    }


    private requestInvoiceStatusList(config: InvoiceStatusQueryParameters | any) : Observable <Array<InvoiceStatusData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<InvoiceStatusData>>(this.baseUrl + 'api/InvoiceStatuses', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveInvoiceStatusList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestInvoiceStatusList(config));
            }));
    }

    public GetInvoiceStatusesRowCount(config: InvoiceStatusQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const invoiceStatusesRowCount$ = this.requestInvoiceStatusesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get InvoiceStatuses row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, invoiceStatusesRowCount$);

            return invoiceStatusesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestInvoiceStatusesRowCount(config: InvoiceStatusQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/InvoiceStatuses/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestInvoiceStatusesRowCount(config));
            }));
    }

    public GetInvoiceStatusesBasicListData(config: InvoiceStatusQueryParameters | any = null) : Observable<Array<InvoiceStatusBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const invoiceStatusesBasicListData$ = this.requestInvoiceStatusesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get InvoiceStatuses basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, invoiceStatusesBasicListData$);

            return invoiceStatusesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<InvoiceStatusBasicListData>>;
    }


    private requestInvoiceStatusesBasicListData(config: InvoiceStatusQueryParameters | any) : Observable<Array<InvoiceStatusBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<InvoiceStatusBasicListData>>(this.baseUrl + 'api/InvoiceStatuses/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestInvoiceStatusesBasicListData(config));
            }));

    }


    public PutInvoiceStatus(id: bigint | number, invoiceStatus: InvoiceStatusSubmitData) : Observable<InvoiceStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<InvoiceStatusData>(this.baseUrl + 'api/InvoiceStatus/' + id.toString(), invoiceStatus, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveInvoiceStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutInvoiceStatus(id, invoiceStatus));
            }));
    }


    public PostInvoiceStatus(invoiceStatus: InvoiceStatusSubmitData) : Observable<InvoiceStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<InvoiceStatusData>(this.baseUrl + 'api/InvoiceStatus', invoiceStatus, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveInvoiceStatus(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostInvoiceStatus(invoiceStatus));
            }));
    }

  
    public DeleteInvoiceStatus(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/InvoiceStatus/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteInvoiceStatus(id));
            }));
    }


    private getConfigHash(config: InvoiceStatusQueryParameters | any): string {

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

    public userIsSchedulerInvoiceStatusReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerInvoiceStatusReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.InvoiceStatuses
        //
        if (userIsSchedulerInvoiceStatusReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerInvoiceStatusReader = user.readPermission >= 1;
            } else {
                userIsSchedulerInvoiceStatusReader = false;
            }
        }

        return userIsSchedulerInvoiceStatusReader;
    }


    public userIsSchedulerInvoiceStatusWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerInvoiceStatusWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.InvoiceStatuses
        //
        if (userIsSchedulerInvoiceStatusWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerInvoiceStatusWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerInvoiceStatusWriter = false;
          }      
        }

        return userIsSchedulerInvoiceStatusWriter;
    }

    public GetInvoicesForInvoiceStatus(invoiceStatusId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<InvoiceData[]> {
        return this.invoiceService.GetInvoiceList({
            invoiceStatusId: invoiceStatusId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full InvoiceStatusData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the InvoiceStatusData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when InvoiceStatusTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveInvoiceStatus(raw: any): InvoiceStatusData {
    if (!raw) return raw;

    //
    // Create a InvoiceStatusData object instance with correct prototype
    //
    const revived = Object.create(InvoiceStatusData.prototype) as InvoiceStatusData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._invoices = null;
    (revived as any)._invoicesPromise = null;
    (revived as any)._invoicesSubject = new BehaviorSubject<InvoiceData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadInvoiceStatusXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Invoices$ = (revived as any)._invoicesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._invoices === null && (revived as any)._invoicesPromise === null) {
                (revived as any).loadInvoices();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._invoicesCount$ = null;



    return revived;
  }

  private ReviveInvoiceStatusList(rawList: any[]): InvoiceStatusData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveInvoiceStatus(raw));
  }

}
