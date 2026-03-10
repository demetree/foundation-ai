/*

   GENERATED SERVICE FOR THE INVOICE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Invoice table.

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
import { ClientData } from './client.service';
import { ContactData } from './contact.service';
import { ScheduledEventData } from './scheduled-event.service';
import { FinancialOfficeData } from './financial-office.service';
import { InvoiceStatusData } from './invoice-status.service';
import { CurrencyData } from './currency.service';
import { TaxCodeData } from './tax-code.service';
import { InvoiceChangeHistoryService, InvoiceChangeHistoryData } from './invoice-change-history.service';
import { InvoiceLineItemService, InvoiceLineItemData } from './invoice-line-item.service';
import { ReceiptService, ReceiptData } from './receipt.service';
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
export class InvoiceQueryParameters {
    invoiceNumber: string | null | undefined = null;
    clientId: bigint | number | null | undefined = null;
    contactId: bigint | number | null | undefined = null;
    scheduledEventId: bigint | number | null | undefined = null;
    financialOfficeId: bigint | number | null | undefined = null;
    invoiceStatusId: bigint | number | null | undefined = null;
    currencyId: bigint | number | null | undefined = null;
    taxCodeId: bigint | number | null | undefined = null;
    invoiceDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    dueDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    subtotal: number | null | undefined = null;
    taxAmount: number | null | undefined = null;
    totalAmount: number | null | undefined = null;
    amountPaid: number | null | undefined = null;
    sentDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    paidDate: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class InvoiceSubmitData {
    id!: bigint | number;
    invoiceNumber!: string;
    clientId!: bigint | number;
    contactId: bigint | number | null = null;
    scheduledEventId: bigint | number | null = null;
    financialOfficeId: bigint | number | null = null;
    invoiceStatusId!: bigint | number;
    currencyId!: bigint | number;
    taxCodeId: bigint | number | null = null;
    invoiceDate!: string;      // ISO 8601 (full datetime)
    dueDate!: string;      // ISO 8601 (full datetime)
    subtotal!: number;
    taxAmount!: number;
    totalAmount!: number;
    amountPaid!: number;
    sentDate: string | null = null;     // ISO 8601 (full datetime)
    paidDate: string | null = null;     // ISO 8601 (full datetime)
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

export class InvoiceBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. InvoiceChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `invoice.InvoiceChildren$` — use with `| async` in templates
//        • Promise:    `invoice.InvoiceChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="invoice.InvoiceChildren$ | async"`), or
//        • Access the promise getter (`invoice.InvoiceChildren` or `await invoice.InvoiceChildren`)
//    - Simply reading `invoice.InvoiceChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await invoice.Reload()` to refresh the entire object and clear all lazy caches.
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
export class InvoiceData {
    id!: bigint | number;
    invoiceNumber!: string;
    clientId!: bigint | number;
    contactId!: bigint | number;
    scheduledEventId!: bigint | number;
    financialOfficeId!: bigint | number;
    invoiceStatusId!: bigint | number;
    currencyId!: bigint | number;
    taxCodeId!: bigint | number;
    invoiceDate!: string;      // ISO 8601 (full datetime)
    dueDate!: string;      // ISO 8601 (full datetime)
    subtotal!: number;
    taxAmount!: number;
    totalAmount!: number;
    amountPaid!: number;
    sentDate!: string | null;   // ISO 8601 (full datetime)
    paidDate!: string | null;   // ISO 8601 (full datetime)
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    client: ClientData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    currency: CurrencyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    financialOffice: FinancialOfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    invoiceStatus: InvoiceStatusData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduledEvent: ScheduledEventData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    taxCode: TaxCodeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _invoiceChangeHistories: InvoiceChangeHistoryData[] | null = null;
    private _invoiceChangeHistoriesPromise: Promise<InvoiceChangeHistoryData[]> | null  = null;
    private _invoiceChangeHistoriesSubject = new BehaviorSubject<InvoiceChangeHistoryData[] | null>(null);

                
    private _invoiceLineItems: InvoiceLineItemData[] | null = null;
    private _invoiceLineItemsPromise: Promise<InvoiceLineItemData[]> | null  = null;
    private _invoiceLineItemsSubject = new BehaviorSubject<InvoiceLineItemData[] | null>(null);

                
    private _receipts: ReceiptData[] | null = null;
    private _receiptsPromise: Promise<ReceiptData[]> | null  = null;
    private _receiptsSubject = new BehaviorSubject<ReceiptData[] | null>(null);

                
    private _documents: DocumentData[] | null = null;
    private _documentsPromise: Promise<DocumentData[]> | null  = null;
    private _documentsSubject = new BehaviorSubject<DocumentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<InvoiceData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<InvoiceData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<InvoiceData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public InvoiceChangeHistories$ = this._invoiceChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._invoiceChangeHistories === null && this._invoiceChangeHistoriesPromise === null) {
            this.loadInvoiceChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _invoiceChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get InvoiceChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._invoiceChangeHistoriesCount$ === null) {
            this._invoiceChangeHistoriesCount$ = InvoiceChangeHistoryService.Instance.GetInvoiceChangeHistoriesRowCount({invoiceId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._invoiceChangeHistoriesCount$;
    }



    public InvoiceLineItems$ = this._invoiceLineItemsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._invoiceLineItems === null && this._invoiceLineItemsPromise === null) {
            this.loadInvoiceLineItems(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _invoiceLineItemsCount$: Observable<bigint | number> | null = null;
    public get InvoiceLineItemsCount$(): Observable<bigint | number> {
        if (this._invoiceLineItemsCount$ === null) {
            this._invoiceLineItemsCount$ = InvoiceLineItemService.Instance.GetInvoiceLineItemsRowCount({invoiceId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._invoiceLineItemsCount$;
    }



    public Receipts$ = this._receiptsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._receipts === null && this._receiptsPromise === null) {
            this.loadReceipts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _receiptsCount$: Observable<bigint | number> | null = null;
    public get ReceiptsCount$(): Observable<bigint | number> {
        if (this._receiptsCount$ === null) {
            this._receiptsCount$ = ReceiptService.Instance.GetReceiptsRowCount({invoiceId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._receiptsCount$;
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
            this._documentsCount$ = DocumentService.Instance.GetDocumentsRowCount({invoiceId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._documentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any InvoiceData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.invoice.Reload();
  //
  //  Non Async:
  //
  //     invoice[0].Reload().then(x => {
  //        this.invoice = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      InvoiceService.Instance.GetInvoice(this.id, includeRelations)
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
     this._invoiceChangeHistories = null;
     this._invoiceChangeHistoriesPromise = null;
     this._invoiceChangeHistoriesSubject.next(null);
     this._invoiceChangeHistoriesCount$ = null;

     this._invoiceLineItems = null;
     this._invoiceLineItemsPromise = null;
     this._invoiceLineItemsSubject.next(null);
     this._invoiceLineItemsCount$ = null;

     this._receipts = null;
     this._receiptsPromise = null;
     this._receiptsSubject.next(null);
     this._receiptsCount$ = null;

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
     * Gets the InvoiceChangeHistories for this Invoice.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.invoice.InvoiceChangeHistories.then(invoices => { ... })
     *   or
     *   await this.invoice.invoices
     *
    */
    public get InvoiceChangeHistories(): Promise<InvoiceChangeHistoryData[]> {
        if (this._invoiceChangeHistories !== null) {
            return Promise.resolve(this._invoiceChangeHistories);
        }

        if (this._invoiceChangeHistoriesPromise !== null) {
            return this._invoiceChangeHistoriesPromise;
        }

        // Start the load
        this.loadInvoiceChangeHistories();

        return this._invoiceChangeHistoriesPromise!;
    }



    private loadInvoiceChangeHistories(): void {

        this._invoiceChangeHistoriesPromise = lastValueFrom(
            InvoiceService.Instance.GetInvoiceChangeHistoriesForInvoice(this.id)
        )
        .then(InvoiceChangeHistories => {
            this._invoiceChangeHistories = InvoiceChangeHistories ?? [];
            this._invoiceChangeHistoriesSubject.next(this._invoiceChangeHistories);
            return this._invoiceChangeHistories;
         })
        .catch(err => {
            this._invoiceChangeHistories = [];
            this._invoiceChangeHistoriesSubject.next(this._invoiceChangeHistories);
            throw err;
        })
        .finally(() => {
            this._invoiceChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached InvoiceChangeHistory. Call after mutations to force refresh.
     */
    public ClearInvoiceChangeHistoriesCache(): void {
        this._invoiceChangeHistories = null;
        this._invoiceChangeHistoriesPromise = null;
        this._invoiceChangeHistoriesSubject.next(this._invoiceChangeHistories);      // Emit to observable
    }

    public get HasInvoiceChangeHistories(): Promise<boolean> {
        return this.InvoiceChangeHistories.then(invoiceChangeHistories => invoiceChangeHistories.length > 0);
    }


    /**
     *
     * Gets the InvoiceLineItems for this Invoice.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.invoice.InvoiceLineItems.then(invoices => { ... })
     *   or
     *   await this.invoice.invoices
     *
    */
    public get InvoiceLineItems(): Promise<InvoiceLineItemData[]> {
        if (this._invoiceLineItems !== null) {
            return Promise.resolve(this._invoiceLineItems);
        }

        if (this._invoiceLineItemsPromise !== null) {
            return this._invoiceLineItemsPromise;
        }

        // Start the load
        this.loadInvoiceLineItems();

        return this._invoiceLineItemsPromise!;
    }



    private loadInvoiceLineItems(): void {

        this._invoiceLineItemsPromise = lastValueFrom(
            InvoiceService.Instance.GetInvoiceLineItemsForInvoice(this.id)
        )
        .then(InvoiceLineItems => {
            this._invoiceLineItems = InvoiceLineItems ?? [];
            this._invoiceLineItemsSubject.next(this._invoiceLineItems);
            return this._invoiceLineItems;
         })
        .catch(err => {
            this._invoiceLineItems = [];
            this._invoiceLineItemsSubject.next(this._invoiceLineItems);
            throw err;
        })
        .finally(() => {
            this._invoiceLineItemsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached InvoiceLineItem. Call after mutations to force refresh.
     */
    public ClearInvoiceLineItemsCache(): void {
        this._invoiceLineItems = null;
        this._invoiceLineItemsPromise = null;
        this._invoiceLineItemsSubject.next(this._invoiceLineItems);      // Emit to observable
    }

    public get HasInvoiceLineItems(): Promise<boolean> {
        return this.InvoiceLineItems.then(invoiceLineItems => invoiceLineItems.length > 0);
    }


    /**
     *
     * Gets the Receipts for this Invoice.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.invoice.Receipts.then(invoices => { ... })
     *   or
     *   await this.invoice.invoices
     *
    */
    public get Receipts(): Promise<ReceiptData[]> {
        if (this._receipts !== null) {
            return Promise.resolve(this._receipts);
        }

        if (this._receiptsPromise !== null) {
            return this._receiptsPromise;
        }

        // Start the load
        this.loadReceipts();

        return this._receiptsPromise!;
    }



    private loadReceipts(): void {

        this._receiptsPromise = lastValueFrom(
            InvoiceService.Instance.GetReceiptsForInvoice(this.id)
        )
        .then(Receipts => {
            this._receipts = Receipts ?? [];
            this._receiptsSubject.next(this._receipts);
            return this._receipts;
         })
        .catch(err => {
            this._receipts = [];
            this._receiptsSubject.next(this._receipts);
            throw err;
        })
        .finally(() => {
            this._receiptsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Receipt. Call after mutations to force refresh.
     */
    public ClearReceiptsCache(): void {
        this._receipts = null;
        this._receiptsPromise = null;
        this._receiptsSubject.next(this._receipts);      // Emit to observable
    }

    public get HasReceipts(): Promise<boolean> {
        return this.Receipts.then(receipts => receipts.length > 0);
    }


    /**
     *
     * Gets the Documents for this Invoice.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.invoice.Documents.then(invoices => { ... })
     *   or
     *   await this.invoice.invoices
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
            InvoiceService.Instance.GetDocumentsForInvoice(this.id)
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
    //   Template: {{ (invoice.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await invoice.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<InvoiceData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<InvoiceData>> {
        const info = await lastValueFrom(
            InvoiceService.Instance.GetInvoiceChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this InvoiceData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this InvoiceData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): InvoiceSubmitData {
        return InvoiceService.Instance.ConvertToInvoiceSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class InvoiceService extends SecureEndpointBase {

    private static _instance: InvoiceService;
    private listCache: Map<string, Observable<Array<InvoiceData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<InvoiceBasicListData>>>;
    private recordCache: Map<string, Observable<InvoiceData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private invoiceChangeHistoryService: InvoiceChangeHistoryService,
        private invoiceLineItemService: InvoiceLineItemService,
        private receiptService: ReceiptService,
        private documentService: DocumentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<InvoiceData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<InvoiceBasicListData>>>();
        this.recordCache = new Map<string, Observable<InvoiceData>>();

        InvoiceService._instance = this;
    }

    public static get Instance(): InvoiceService {
      return InvoiceService._instance;
    }


    public ClearListCaches(config: InvoiceQueryParameters | null = null) {

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


    public ConvertToInvoiceSubmitData(data: InvoiceData): InvoiceSubmitData {

        let output = new InvoiceSubmitData();

        output.id = data.id;
        output.invoiceNumber = data.invoiceNumber;
        output.clientId = data.clientId;
        output.contactId = data.contactId;
        output.scheduledEventId = data.scheduledEventId;
        output.financialOfficeId = data.financialOfficeId;
        output.invoiceStatusId = data.invoiceStatusId;
        output.currencyId = data.currencyId;
        output.taxCodeId = data.taxCodeId;
        output.invoiceDate = data.invoiceDate;
        output.dueDate = data.dueDate;
        output.subtotal = data.subtotal;
        output.taxAmount = data.taxAmount;
        output.totalAmount = data.totalAmount;
        output.amountPaid = data.amountPaid;
        output.sentDate = data.sentDate;
        output.paidDate = data.paidDate;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetInvoice(id: bigint | number, includeRelations: boolean = true) : Observable<InvoiceData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const invoice$ = this.requestInvoice(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Invoice", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, invoice$);

            return invoice$;
        }

        return this.recordCache.get(configHash) as Observable<InvoiceData>;
    }

    private requestInvoice(id: bigint | number, includeRelations: boolean = true) : Observable<InvoiceData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<InvoiceData>(this.baseUrl + 'api/Invoice/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveInvoice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestInvoice(id, includeRelations));
            }));
    }

    public GetInvoiceList(config: InvoiceQueryParameters | any = null) : Observable<Array<InvoiceData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const invoiceList$ = this.requestInvoiceList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Invoice list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, invoiceList$);

            return invoiceList$;
        }

        return this.listCache.get(configHash) as Observable<Array<InvoiceData>>;
    }


    private requestInvoiceList(config: InvoiceQueryParameters | any) : Observable <Array<InvoiceData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<InvoiceData>>(this.baseUrl + 'api/Invoices', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveInvoiceList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestInvoiceList(config));
            }));
    }

    public GetInvoicesRowCount(config: InvoiceQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const invoicesRowCount$ = this.requestInvoicesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Invoices row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, invoicesRowCount$);

            return invoicesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestInvoicesRowCount(config: InvoiceQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Invoices/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestInvoicesRowCount(config));
            }));
    }

    public GetInvoicesBasicListData(config: InvoiceQueryParameters | any = null) : Observable<Array<InvoiceBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const invoicesBasicListData$ = this.requestInvoicesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Invoices basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, invoicesBasicListData$);

            return invoicesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<InvoiceBasicListData>>;
    }


    private requestInvoicesBasicListData(config: InvoiceQueryParameters | any) : Observable<Array<InvoiceBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<InvoiceBasicListData>>(this.baseUrl + 'api/Invoices/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestInvoicesBasicListData(config));
            }));

    }


    public PutInvoice(id: bigint | number, invoice: InvoiceSubmitData) : Observable<InvoiceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<InvoiceData>(this.baseUrl + 'api/Invoice/' + id.toString(), invoice, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveInvoice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutInvoice(id, invoice));
            }));
    }


    public PostInvoice(invoice: InvoiceSubmitData) : Observable<InvoiceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<InvoiceData>(this.baseUrl + 'api/Invoice', invoice, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveInvoice(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostInvoice(invoice));
            }));
    }

  
    public DeleteInvoice(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Invoice/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteInvoice(id));
            }));
    }

    public RollbackInvoice(id: bigint | number, versionNumber: bigint | number) : Observable<InvoiceData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<InvoiceData>(this.baseUrl + 'api/Invoice/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveInvoice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackInvoice(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Invoice.
     */
    public GetInvoiceChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<InvoiceData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<InvoiceData>>(this.baseUrl + 'api/Invoice/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetInvoiceChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Invoice.
     */
    public GetInvoiceAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<InvoiceData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<InvoiceData>[]>(this.baseUrl + 'api/Invoice/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetInvoiceAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Invoice.
     */
    public GetInvoiceVersion(id: bigint | number, version: number): Observable<InvoiceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<InvoiceData>(this.baseUrl + 'api/Invoice/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveInvoice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetInvoiceVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Invoice at a specific point in time.
     */
    public GetInvoiceStateAtTime(id: bigint | number, time: string): Observable<InvoiceData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<InvoiceData>(this.baseUrl + 'api/Invoice/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveInvoice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetInvoiceStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: InvoiceQueryParameters | any): string {

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

    public userIsSchedulerInvoiceReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerInvoiceReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Invoices
        //
        if (userIsSchedulerInvoiceReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerInvoiceReader = user.readPermission >= 1;
            } else {
                userIsSchedulerInvoiceReader = false;
            }
        }

        return userIsSchedulerInvoiceReader;
    }


    public userIsSchedulerInvoiceWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerInvoiceWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Invoices
        //
        if (userIsSchedulerInvoiceWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerInvoiceWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerInvoiceWriter = false;
          }      
        }

        return userIsSchedulerInvoiceWriter;
    }

    public GetInvoiceChangeHistoriesForInvoice(invoiceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<InvoiceChangeHistoryData[]> {
        return this.invoiceChangeHistoryService.GetInvoiceChangeHistoryList({
            invoiceId: invoiceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetInvoiceLineItemsForInvoice(invoiceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<InvoiceLineItemData[]> {
        return this.invoiceLineItemService.GetInvoiceLineItemList({
            invoiceId: invoiceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetReceiptsForInvoice(invoiceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ReceiptData[]> {
        return this.receiptService.GetReceiptList({
            invoiceId: invoiceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetDocumentsForInvoice(invoiceId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<DocumentData[]> {
        return this.documentService.GetDocumentList({
            invoiceId: invoiceId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full InvoiceData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the InvoiceData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when InvoiceTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveInvoice(raw: any): InvoiceData {
    if (!raw) return raw;

    //
    // Create a InvoiceData object instance with correct prototype
    //
    const revived = Object.create(InvoiceData.prototype) as InvoiceData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._invoiceChangeHistories = null;
    (revived as any)._invoiceChangeHistoriesPromise = null;
    (revived as any)._invoiceChangeHistoriesSubject = new BehaviorSubject<InvoiceChangeHistoryData[] | null>(null);

    (revived as any)._invoiceLineItems = null;
    (revived as any)._invoiceLineItemsPromise = null;
    (revived as any)._invoiceLineItemsSubject = new BehaviorSubject<InvoiceLineItemData[] | null>(null);

    (revived as any)._receipts = null;
    (revived as any)._receiptsPromise = null;
    (revived as any)._receiptsSubject = new BehaviorSubject<ReceiptData[] | null>(null);

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
    // 2. But private methods (loadInvoiceXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).InvoiceChangeHistories$ = (revived as any)._invoiceChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._invoiceChangeHistories === null && (revived as any)._invoiceChangeHistoriesPromise === null) {
                (revived as any).loadInvoiceChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._invoiceChangeHistoriesCount$ = null;


    (revived as any).InvoiceLineItems$ = (revived as any)._invoiceLineItemsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._invoiceLineItems === null && (revived as any)._invoiceLineItemsPromise === null) {
                (revived as any).loadInvoiceLineItems();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._invoiceLineItemsCount$ = null;


    (revived as any).Receipts$ = (revived as any)._receiptsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._receipts === null && (revived as any)._receiptsPromise === null) {
                (revived as any).loadReceipts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._receiptsCount$ = null;


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
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<InvoiceData> | null>(null);

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

  private ReviveInvoiceList(rawList: any[]): InvoiceData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveInvoice(raw));
  }

}
