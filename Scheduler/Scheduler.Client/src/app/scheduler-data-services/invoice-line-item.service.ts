/*

   GENERATED SERVICE FOR THE INVOICELINEITEM TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the InvoiceLineItem table.

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
import { InvoiceData } from './invoice.service';
import { EventChargeData } from './event-charge.service';
import { FinancialCategoryData } from './financial-category.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class InvoiceLineItemQueryParameters {
    invoiceId: bigint | number | null | undefined = null;
    eventChargeId: bigint | number | null | undefined = null;
    financialCategoryId: bigint | number | null | undefined = null;
    description: string | null | undefined = null;
    quantity: number | null | undefined = null;
    unitPrice: number | null | undefined = null;
    amount: number | null | undefined = null;
    taxAmount: number | null | undefined = null;
    totalAmount: number | null | undefined = null;
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
export class InvoiceLineItemSubmitData {
    id!: bigint | number;
    invoiceId!: bigint | number;
    eventChargeId: bigint | number | null = null;
    financialCategoryId: bigint | number | null = null;
    description!: string;
    quantity!: number;
    unitPrice!: number;
    amount!: number;
    taxAmount!: number;
    totalAmount!: number;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class InvoiceLineItemBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. InvoiceLineItemChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `invoiceLineItem.InvoiceLineItemChildren$` — use with `| async` in templates
//        • Promise:    `invoiceLineItem.InvoiceLineItemChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="invoiceLineItem.InvoiceLineItemChildren$ | async"`), or
//        • Access the promise getter (`invoiceLineItem.InvoiceLineItemChildren` or `await invoiceLineItem.InvoiceLineItemChildren`)
//    - Simply reading `invoiceLineItem.InvoiceLineItemChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await invoiceLineItem.Reload()` to refresh the entire object and clear all lazy caches.
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
export class InvoiceLineItemData {
    id!: bigint | number;
    invoiceId!: bigint | number;
    eventChargeId!: bigint | number;
    financialCategoryId!: bigint | number;
    description!: string;
    quantity!: number;
    unitPrice!: number;
    amount!: number;
    taxAmount!: number;
    totalAmount!: number;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    eventCharge: EventChargeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    financialCategory: FinancialCategoryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    invoice: InvoiceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //

  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any InvoiceLineItemData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.invoiceLineItem.Reload();
  //
  //  Non Async:
  //
  //     invoiceLineItem[0].Reload().then(x => {
  //        this.invoiceLineItem = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      InvoiceLineItemService.Instance.GetInvoiceLineItem(this.id, includeRelations)
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
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //


    /**
     * Updates the state of this InvoiceLineItemData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this InvoiceLineItemData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): InvoiceLineItemSubmitData {
        return InvoiceLineItemService.Instance.ConvertToInvoiceLineItemSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class InvoiceLineItemService extends SecureEndpointBase {

    private static _instance: InvoiceLineItemService;
    private listCache: Map<string, Observable<Array<InvoiceLineItemData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<InvoiceLineItemBasicListData>>>;
    private recordCache: Map<string, Observable<InvoiceLineItemData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<InvoiceLineItemData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<InvoiceLineItemBasicListData>>>();
        this.recordCache = new Map<string, Observable<InvoiceLineItemData>>();

        InvoiceLineItemService._instance = this;
    }

    public static get Instance(): InvoiceLineItemService {
      return InvoiceLineItemService._instance;
    }


    public ClearListCaches(config: InvoiceLineItemQueryParameters | null = null) {

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


    public ConvertToInvoiceLineItemSubmitData(data: InvoiceLineItemData): InvoiceLineItemSubmitData {

        let output = new InvoiceLineItemSubmitData();

        output.id = data.id;
        output.invoiceId = data.invoiceId;
        output.eventChargeId = data.eventChargeId;
        output.financialCategoryId = data.financialCategoryId;
        output.description = data.description;
        output.quantity = data.quantity;
        output.unitPrice = data.unitPrice;
        output.amount = data.amount;
        output.taxAmount = data.taxAmount;
        output.totalAmount = data.totalAmount;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetInvoiceLineItem(id: bigint | number, includeRelations: boolean = true) : Observable<InvoiceLineItemData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const invoiceLineItem$ = this.requestInvoiceLineItem(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get InvoiceLineItem", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, invoiceLineItem$);

            return invoiceLineItem$;
        }

        return this.recordCache.get(configHash) as Observable<InvoiceLineItemData>;
    }

    private requestInvoiceLineItem(id: bigint | number, includeRelations: boolean = true) : Observable<InvoiceLineItemData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<InvoiceLineItemData>(this.baseUrl + 'api/InvoiceLineItem/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveInvoiceLineItem(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestInvoiceLineItem(id, includeRelations));
            }));
    }

    public GetInvoiceLineItemList(config: InvoiceLineItemQueryParameters | any = null) : Observable<Array<InvoiceLineItemData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const invoiceLineItemList$ = this.requestInvoiceLineItemList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get InvoiceLineItem list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, invoiceLineItemList$);

            return invoiceLineItemList$;
        }

        return this.listCache.get(configHash) as Observable<Array<InvoiceLineItemData>>;
    }


    private requestInvoiceLineItemList(config: InvoiceLineItemQueryParameters | any) : Observable <Array<InvoiceLineItemData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<InvoiceLineItemData>>(this.baseUrl + 'api/InvoiceLineItems', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveInvoiceLineItemList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestInvoiceLineItemList(config));
            }));
    }

    public GetInvoiceLineItemsRowCount(config: InvoiceLineItemQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const invoiceLineItemsRowCount$ = this.requestInvoiceLineItemsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get InvoiceLineItems row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, invoiceLineItemsRowCount$);

            return invoiceLineItemsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestInvoiceLineItemsRowCount(config: InvoiceLineItemQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/InvoiceLineItems/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestInvoiceLineItemsRowCount(config));
            }));
    }

    public GetInvoiceLineItemsBasicListData(config: InvoiceLineItemQueryParameters | any = null) : Observable<Array<InvoiceLineItemBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const invoiceLineItemsBasicListData$ = this.requestInvoiceLineItemsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get InvoiceLineItems basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, invoiceLineItemsBasicListData$);

            return invoiceLineItemsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<InvoiceLineItemBasicListData>>;
    }


    private requestInvoiceLineItemsBasicListData(config: InvoiceLineItemQueryParameters | any) : Observable<Array<InvoiceLineItemBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<InvoiceLineItemBasicListData>>(this.baseUrl + 'api/InvoiceLineItems/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestInvoiceLineItemsBasicListData(config));
            }));

    }


    public PutInvoiceLineItem(id: bigint | number, invoiceLineItem: InvoiceLineItemSubmitData) : Observable<InvoiceLineItemData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<InvoiceLineItemData>(this.baseUrl + 'api/InvoiceLineItem/' + id.toString(), invoiceLineItem, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveInvoiceLineItem(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutInvoiceLineItem(id, invoiceLineItem));
            }));
    }


    public PostInvoiceLineItem(invoiceLineItem: InvoiceLineItemSubmitData) : Observable<InvoiceLineItemData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<InvoiceLineItemData>(this.baseUrl + 'api/InvoiceLineItem', invoiceLineItem, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveInvoiceLineItem(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostInvoiceLineItem(invoiceLineItem));
            }));
    }

  
    public DeleteInvoiceLineItem(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/InvoiceLineItem/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteInvoiceLineItem(id));
            }));
    }


    private getConfigHash(config: InvoiceLineItemQueryParameters | any): string {

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

    public userIsSchedulerInvoiceLineItemReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerInvoiceLineItemReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.InvoiceLineItems
        //
        if (userIsSchedulerInvoiceLineItemReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerInvoiceLineItemReader = user.readPermission >= 1;
            } else {
                userIsSchedulerInvoiceLineItemReader = false;
            }
        }

        return userIsSchedulerInvoiceLineItemReader;
    }


    public userIsSchedulerInvoiceLineItemWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerInvoiceLineItemWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.InvoiceLineItems
        //
        if (userIsSchedulerInvoiceLineItemWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerInvoiceLineItemWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerInvoiceLineItemWriter = false;
          }      
        }

        return userIsSchedulerInvoiceLineItemWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full InvoiceLineItemData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the InvoiceLineItemData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when InvoiceLineItemTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveInvoiceLineItem(raw: any): InvoiceLineItemData {
    if (!raw) return raw;

    //
    // Create a InvoiceLineItemData object instance with correct prototype
    //
    const revived = Object.create(InvoiceLineItemData.prototype) as InvoiceLineItemData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //

    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadInvoiceLineItemXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveInvoiceLineItemList(rawList: any[]): InvoiceLineItemData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveInvoiceLineItem(raw));
  }

}
