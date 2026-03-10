/*

   GENERATED SERVICE FOR THE RECEIPT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Receipt table.

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
import { ReceiptTypeData } from './receipt-type.service';
import { InvoiceData } from './invoice.service';
import { PaymentTransactionData } from './payment-transaction.service';
import { FinancialTransactionData } from './financial-transaction.service';
import { ClientData } from './client.service';
import { ContactData } from './contact.service';
import { CurrencyData } from './currency.service';
import { ReceiptChangeHistoryService, ReceiptChangeHistoryData } from './receipt-change-history.service';
import { DocumentService, DocumentData } from './document.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ReceiptQueryParameters {
    receiptNumber: string | null | undefined = null;
    receiptTypeId: bigint | number | null | undefined = null;
    invoiceId: bigint | number | null | undefined = null;
    paymentTransactionId: bigint | number | null | undefined = null;
    financialTransactionId: bigint | number | null | undefined = null;
    clientId: bigint | number | null | undefined = null;
    contactId: bigint | number | null | undefined = null;
    currencyId: bigint | number | null | undefined = null;
    receiptDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    amount: number | null | undefined = null;
    paymentMethod: string | null | undefined = null;
    description: string | null | undefined = null;
    notes: string | null | undefined = null;
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
export class ReceiptSubmitData {
    id!: bigint | number;
    receiptNumber!: string;
    receiptTypeId!: bigint | number;
    invoiceId: bigint | number | null = null;
    paymentTransactionId: bigint | number | null = null;
    financialTransactionId: bigint | number | null = null;
    clientId: bigint | number | null = null;
    contactId: bigint | number | null = null;
    currencyId!: bigint | number;
    receiptDate!: string;      // ISO 8601 (full datetime)
    amount!: number;
    paymentMethod: string | null = null;
    description: string | null = null;
    notes: string | null = null;
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

export class ReceiptBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ReceiptChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `receipt.ReceiptChildren$` — use with `| async` in templates
//        • Promise:    `receipt.ReceiptChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="receipt.ReceiptChildren$ | async"`), or
//        • Access the promise getter (`receipt.ReceiptChildren` or `await receipt.ReceiptChildren`)
//    - Simply reading `receipt.ReceiptChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await receipt.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ReceiptData {
    id!: bigint | number;
    receiptNumber!: string;
    receiptTypeId!: bigint | number;
    invoiceId!: bigint | number;
    paymentTransactionId!: bigint | number;
    financialTransactionId!: bigint | number;
    clientId!: bigint | number;
    contactId!: bigint | number;
    currencyId!: bigint | number;
    receiptDate!: string;      // ISO 8601 (full datetime)
    amount!: number;
    paymentMethod!: string | null;
    description!: string | null;
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    client: ClientData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    currency: CurrencyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    financialTransaction: FinancialTransactionData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    invoice: InvoiceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    paymentTransaction: PaymentTransactionData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    receiptType: ReceiptTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _receiptChangeHistories: ReceiptChangeHistoryData[] | null = null;
    private _receiptChangeHistoriesPromise: Promise<ReceiptChangeHistoryData[]> | null  = null;
    private _receiptChangeHistoriesSubject = new BehaviorSubject<ReceiptChangeHistoryData[] | null>(null);

                
    private _documents: DocumentData[] | null = null;
    private _documentsPromise: Promise<DocumentData[]> | null  = null;
    private _documentsSubject = new BehaviorSubject<DocumentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ReceiptData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ReceiptData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ReceiptData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ReceiptChangeHistories$ = this._receiptChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._receiptChangeHistories === null && this._receiptChangeHistoriesPromise === null) {
            this.loadReceiptChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _receiptChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ReceiptChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._receiptChangeHistoriesCount$ === null) {
            this._receiptChangeHistoriesCount$ = ReceiptChangeHistoryService.Instance.GetReceiptChangeHistoriesRowCount({receiptId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._receiptChangeHistoriesCount$;
    }



    public Documents$ = this._documentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._documents === null && this._documentsPromise === null) {
            this.loadDocuments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _documentsCount$: Observable<bigint | number> | null = null;
    public get DocumentsCount$(): Observable<bigint | number> {
        if (this._documentsCount$ === null) {
            this._documentsCount$ = DocumentService.Instance.GetDocumentsRowCount({receiptId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._documentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ReceiptData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.receipt.Reload();
  //
  //  Non Async:
  //
  //     receipt[0].Reload().then(x => {
  //        this.receipt = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ReceiptService.Instance.GetReceipt(this.id, includeRelations)
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
     this._receiptChangeHistories = null;
     this._receiptChangeHistoriesPromise = null;
     this._receiptChangeHistoriesSubject.next(null);
     this._receiptChangeHistoriesCount$ = null;

     this._documents = null;
     this._documentsPromise = null;
     this._documentsSubject.next(null);
     this._documentsCount$ = null;

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
     * Gets the ReceiptChangeHistories for this Receipt.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.receipt.ReceiptChangeHistories.then(receipts => { ... })
     *   or
     *   await this.receipt.receipts
     *
    */
    public get ReceiptChangeHistories(): Promise<ReceiptChangeHistoryData[]> {
        if (this._receiptChangeHistories !== null) {
            return Promise.resolve(this._receiptChangeHistories);
        }

        if (this._receiptChangeHistoriesPromise !== null) {
            return this._receiptChangeHistoriesPromise;
        }

        // Start the load
        this.loadReceiptChangeHistories();

        return this._receiptChangeHistoriesPromise!;
    }



    private loadReceiptChangeHistories(): void {

        this._receiptChangeHistoriesPromise = lastValueFrom(
            ReceiptService.Instance.GetReceiptChangeHistoriesForReceipt(this.id)
        )
        .then(ReceiptChangeHistories => {
            this._receiptChangeHistories = ReceiptChangeHistories ?? [];
            this._receiptChangeHistoriesSubject.next(this._receiptChangeHistories);
            return this._receiptChangeHistories;
         })
        .catch(err => {
            this._receiptChangeHistories = [];
            this._receiptChangeHistoriesSubject.next(this._receiptChangeHistories);
            throw err;
        })
        .finally(() => {
            this._receiptChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ReceiptChangeHistory. Call after mutations to force refresh.
     */
    public ClearReceiptChangeHistoriesCache(): void {
        this._receiptChangeHistories = null;
        this._receiptChangeHistoriesPromise = null;
        this._receiptChangeHistoriesSubject.next(this._receiptChangeHistories);      // Emit to observable
    }

    public get HasReceiptChangeHistories(): Promise<boolean> {
        return this.ReceiptChangeHistories.then(receiptChangeHistories => receiptChangeHistories.length > 0);
    }


    /**
     *
     * Gets the Documents for this Receipt.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.receipt.Documents.then(receipts => { ... })
     *   or
     *   await this.receipt.receipts
     *
    */
    public get Documents(): Promise<DocumentData[]> {
        if (this._documents !== null) {
            return Promise.resolve(this._documents);
        }

        if (this._documentsPromise !== null) {
            return this._documentsPromise;
        }

        // Start the load
        this.loadDocuments();

        return this._documentsPromise!;
    }



    private loadDocuments(): void {

        this._documentsPromise = lastValueFrom(
            ReceiptService.Instance.GetDocumentsForReceipt(this.id)
        )
        .then(Documents => {
            this._documents = Documents ?? [];
            this._documentsSubject.next(this._documents);
            return this._documents;
         })
        .catch(err => {
            this._documents = [];
            this._documentsSubject.next(this._documents);
            throw err;
        })
        .finally(() => {
            this._documentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Document. Call after mutations to force refresh.
     */
    public ClearDocumentsCache(): void {
        this._documents = null;
        this._documentsPromise = null;
        this._documentsSubject.next(this._documents);      // Emit to observable
    }

    public get HasDocuments(): Promise<boolean> {
        return this.Documents.then(documents => documents.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (receipt.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await receipt.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ReceiptData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ReceiptData>> {
        const info = await lastValueFrom(
            ReceiptService.Instance.GetReceiptChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ReceiptData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ReceiptData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ReceiptSubmitData {
        return ReceiptService.Instance.ConvertToReceiptSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ReceiptService extends SecureEndpointBase {

    private static _instance: ReceiptService;
    private listCache: Map<string, Observable<Array<ReceiptData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ReceiptBasicListData>>>;
    private recordCache: Map<string, Observable<ReceiptData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private receiptChangeHistoryService: ReceiptChangeHistoryService,
        private documentService: DocumentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ReceiptData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ReceiptBasicListData>>>();
        this.recordCache = new Map<string, Observable<ReceiptData>>();

        ReceiptService._instance = this;
    }

    public static get Instance(): ReceiptService {
      return ReceiptService._instance;
    }


    public ClearListCaches(config: ReceiptQueryParameters | null = null) {

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


    public ConvertToReceiptSubmitData(data: ReceiptData): ReceiptSubmitData {

        let output = new ReceiptSubmitData();

        output.id = data.id;
        output.receiptNumber = data.receiptNumber;
        output.receiptTypeId = data.receiptTypeId;
        output.invoiceId = data.invoiceId;
        output.paymentTransactionId = data.paymentTransactionId;
        output.financialTransactionId = data.financialTransactionId;
        output.clientId = data.clientId;
        output.contactId = data.contactId;
        output.currencyId = data.currencyId;
        output.receiptDate = data.receiptDate;
        output.amount = data.amount;
        output.paymentMethod = data.paymentMethod;
        output.description = data.description;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetReceipt(id: bigint | number, includeRelations: boolean = true) : Observable<ReceiptData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const receipt$ = this.requestReceipt(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Receipt", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, receipt$);

            return receipt$;
        }

        return this.recordCache.get(configHash) as Observable<ReceiptData>;
    }

    private requestReceipt(id: bigint | number, includeRelations: boolean = true) : Observable<ReceiptData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ReceiptData>(this.baseUrl + 'api/Receipt/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveReceipt(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestReceipt(id, includeRelations));
            }));
    }

    public GetReceiptList(config: ReceiptQueryParameters | any = null) : Observable<Array<ReceiptData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const receiptList$ = this.requestReceiptList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Receipt list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, receiptList$);

            return receiptList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ReceiptData>>;
    }


    private requestReceiptList(config: ReceiptQueryParameters | any) : Observable <Array<ReceiptData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ReceiptData>>(this.baseUrl + 'api/Receipts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveReceiptList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestReceiptList(config));
            }));
    }

    public GetReceiptsRowCount(config: ReceiptQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const receiptsRowCount$ = this.requestReceiptsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Receipts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, receiptsRowCount$);

            return receiptsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestReceiptsRowCount(config: ReceiptQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Receipts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestReceiptsRowCount(config));
            }));
    }

    public GetReceiptsBasicListData(config: ReceiptQueryParameters | any = null) : Observable<Array<ReceiptBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const receiptsBasicListData$ = this.requestReceiptsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Receipts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, receiptsBasicListData$);

            return receiptsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ReceiptBasicListData>>;
    }


    private requestReceiptsBasicListData(config: ReceiptQueryParameters | any) : Observable<Array<ReceiptBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ReceiptBasicListData>>(this.baseUrl + 'api/Receipts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestReceiptsBasicListData(config));
            }));

    }


    public PutReceipt(id: bigint | number, receipt: ReceiptSubmitData) : Observable<ReceiptData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ReceiptData>(this.baseUrl + 'api/Receipt/' + id.toString(), receipt, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveReceipt(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutReceipt(id, receipt));
            }));
    }


    public PostReceipt(receipt: ReceiptSubmitData) : Observable<ReceiptData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ReceiptData>(this.baseUrl + 'api/Receipt', receipt, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveReceipt(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostReceipt(receipt));
            }));
    }

  
    public DeleteReceipt(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Receipt/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteReceipt(id));
            }));
    }

    public RollbackReceipt(id: bigint | number, versionNumber: bigint | number) : Observable<ReceiptData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ReceiptData>(this.baseUrl + 'api/Receipt/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveReceipt(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackReceipt(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Receipt.
     */
    public GetReceiptChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ReceiptData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ReceiptData>>(this.baseUrl + 'api/Receipt/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetReceiptChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Receipt.
     */
    public GetReceiptAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ReceiptData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ReceiptData>[]>(this.baseUrl + 'api/Receipt/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetReceiptAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Receipt.
     */
    public GetReceiptVersion(id: bigint | number, version: number): Observable<ReceiptData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ReceiptData>(this.baseUrl + 'api/Receipt/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveReceipt(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetReceiptVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Receipt at a specific point in time.
     */
    public GetReceiptStateAtTime(id: bigint | number, time: string): Observable<ReceiptData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ReceiptData>(this.baseUrl + 'api/Receipt/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveReceipt(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetReceiptStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ReceiptQueryParameters | any): string {

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

    public userIsSchedulerReceiptReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerReceiptReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Receipts
        //
        if (userIsSchedulerReceiptReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerReceiptReader = user.readPermission >= 1;
            } else {
                userIsSchedulerReceiptReader = false;
            }
        }

        return userIsSchedulerReceiptReader;
    }


    public userIsSchedulerReceiptWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerReceiptWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Receipts
        //
        if (userIsSchedulerReceiptWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerReceiptWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerReceiptWriter = false;
          }      
        }

        return userIsSchedulerReceiptWriter;
    }

    public GetReceiptChangeHistoriesForReceipt(receiptId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ReceiptChangeHistoryData[]> {
        return this.receiptChangeHistoryService.GetReceiptChangeHistoryList({
            receiptId: receiptId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetDocumentsForReceipt(receiptId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<DocumentData[]> {
        return this.documentService.GetDocumentList({
            receiptId: receiptId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ReceiptData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ReceiptData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ReceiptTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveReceipt(raw: any): ReceiptData {
    if (!raw) return raw;

    //
    // Create a ReceiptData object instance with correct prototype
    //
    const revived = Object.create(ReceiptData.prototype) as ReceiptData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._receiptChangeHistories = null;
    (revived as any)._receiptChangeHistoriesPromise = null;
    (revived as any)._receiptChangeHistoriesSubject = new BehaviorSubject<ReceiptChangeHistoryData[] | null>(null);

    (revived as any)._documents = null;
    (revived as any)._documentsPromise = null;
    (revived as any)._documentsSubject = new BehaviorSubject<DocumentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadReceiptXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ReceiptChangeHistories$ = (revived as any)._receiptChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._receiptChangeHistories === null && (revived as any)._receiptChangeHistoriesPromise === null) {
                (revived as any).loadReceiptChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._receiptChangeHistoriesCount$ = null;


    (revived as any).Documents$ = (revived as any)._documentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._documents === null && (revived as any)._documentsPromise === null) {
                (revived as any).loadDocuments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._documentsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ReceiptData> | null>(null);

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

  private ReviveReceiptList(rawList: any[]): ReceiptData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveReceipt(raw));
  }

}
