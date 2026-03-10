/*

   GENERATED SERVICE FOR THE FINANCIALTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the FinancialTransaction table.

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
import { FinancialCategoryData } from './financial-category.service';
import { FinancialOfficeData } from './financial-office.service';
import { ScheduledEventData } from './scheduled-event.service';
import { ContactData } from './contact.service';
import { ClientData } from './client.service';
import { TaxCodeData } from './tax-code.service';
import { FiscalPeriodData } from './fiscal-period.service';
import { PaymentTypeData } from './payment-type.service';
import { CurrencyData } from './currency.service';
import { FinancialTransactionChangeHistoryService, FinancialTransactionChangeHistoryData } from './financial-transaction-change-history.service';
import { PaymentTransactionService, PaymentTransactionData } from './payment-transaction.service';
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
export class FinancialTransactionQueryParameters {
    financialCategoryId: bigint | number | null | undefined = null;
    financialOfficeId: bigint | number | null | undefined = null;
    scheduledEventId: bigint | number | null | undefined = null;
    contactId: bigint | number | null | undefined = null;
    clientId: bigint | number | null | undefined = null;
    contactRole: string | null | undefined = null;
    taxCodeId: bigint | number | null | undefined = null;
    fiscalPeriodId: bigint | number | null | undefined = null;
    paymentTypeId: bigint | number | null | undefined = null;
    transactionDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    description: string | null | undefined = null;
    amount: number | null | undefined = null;
    taxAmount: number | null | undefined = null;
    totalAmount: number | null | undefined = null;
    isRevenue: boolean | null | undefined = null;
    journalEntryType: string | null | undefined = null;
    referenceNumber: string | null | undefined = null;
    notes: string | null | undefined = null;
    currencyId: bigint | number | null | undefined = null;
    exportedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    externalId: string | null | undefined = null;
    externalSystemName: string | null | undefined = null;
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
export class FinancialTransactionSubmitData {
    id!: bigint | number;
    financialCategoryId!: bigint | number;
    financialOfficeId: bigint | number | null = null;
    scheduledEventId: bigint | number | null = null;
    contactId: bigint | number | null = null;
    clientId: bigint | number | null = null;
    contactRole: string | null = null;
    taxCodeId: bigint | number | null = null;
    fiscalPeriodId: bigint | number | null = null;
    paymentTypeId: bigint | number | null = null;
    transactionDate!: string;      // ISO 8601 (full datetime)
    description!: string;
    amount!: number;
    taxAmount!: number;
    totalAmount!: number;
    isRevenue!: boolean;
    journalEntryType: string | null = null;
    referenceNumber: string | null = null;
    notes: string | null = null;
    currencyId!: bigint | number;
    exportedDate: string | null = null;     // ISO 8601 (full datetime)
    externalId: string | null = null;
    externalSystemName: string | null = null;
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

export class FinancialTransactionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. FinancialTransactionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `financialTransaction.FinancialTransactionChildren$` — use with `| async` in templates
//        • Promise:    `financialTransaction.FinancialTransactionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="financialTransaction.FinancialTransactionChildren$ | async"`), or
//        • Access the promise getter (`financialTransaction.FinancialTransactionChildren` or `await financialTransaction.FinancialTransactionChildren`)
//    - Simply reading `financialTransaction.FinancialTransactionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await financialTransaction.Reload()` to refresh the entire object and clear all lazy caches.
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
export class FinancialTransactionData {
    id!: bigint | number;
    financialCategoryId!: bigint | number;
    financialOfficeId!: bigint | number;
    scheduledEventId!: bigint | number;
    contactId!: bigint | number;
    clientId!: bigint | number;
    contactRole!: string | null;
    taxCodeId!: bigint | number;
    fiscalPeriodId!: bigint | number;
    paymentTypeId!: bigint | number;
    transactionDate!: string;      // ISO 8601 (full datetime)
    description!: string;
    amount!: number;
    taxAmount!: number;
    totalAmount!: number;
    isRevenue!: boolean;
    journalEntryType!: string | null;
    referenceNumber!: string | null;
    notes!: string | null;
    currencyId!: bigint | number;
    exportedDate!: string | null;   // ISO 8601 (full datetime)
    externalId!: string | null;
    externalSystemName!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    client: ClientData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    currency: CurrencyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    financialCategory: FinancialCategoryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    financialOffice: FinancialOfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    fiscalPeriod: FiscalPeriodData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    paymentType: PaymentTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduledEvent: ScheduledEventData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    taxCode: TaxCodeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _financialTransactionChangeHistories: FinancialTransactionChangeHistoryData[] | null = null;
    private _financialTransactionChangeHistoriesPromise: Promise<FinancialTransactionChangeHistoryData[]> | null  = null;
    private _financialTransactionChangeHistoriesSubject = new BehaviorSubject<FinancialTransactionChangeHistoryData[] | null>(null);

                
    private _paymentTransactions: PaymentTransactionData[] | null = null;
    private _paymentTransactionsPromise: Promise<PaymentTransactionData[]> | null  = null;
    private _paymentTransactionsSubject = new BehaviorSubject<PaymentTransactionData[] | null>(null);

                
    private _receipts: ReceiptData[] | null = null;
    private _receiptsPromise: Promise<ReceiptData[]> | null  = null;
    private _receiptsSubject = new BehaviorSubject<ReceiptData[] | null>(null);

                
    private _documents: DocumentData[] | null = null;
    private _documentsPromise: Promise<DocumentData[]> | null  = null;
    private _documentsSubject = new BehaviorSubject<DocumentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<FinancialTransactionData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<FinancialTransactionData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<FinancialTransactionData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public FinancialTransactionChangeHistories$ = this._financialTransactionChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._financialTransactionChangeHistories === null && this._financialTransactionChangeHistoriesPromise === null) {
            this.loadFinancialTransactionChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _financialTransactionChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get FinancialTransactionChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._financialTransactionChangeHistoriesCount$ === null) {
            this._financialTransactionChangeHistoriesCount$ = FinancialTransactionChangeHistoryService.Instance.GetFinancialTransactionChangeHistoriesRowCount({financialTransactionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._financialTransactionChangeHistoriesCount$;
    }



    public PaymentTransactions$ = this._paymentTransactionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._paymentTransactions === null && this._paymentTransactionsPromise === null) {
            this.loadPaymentTransactions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _paymentTransactionsCount$: Observable<bigint | number> | null = null;
    public get PaymentTransactionsCount$(): Observable<bigint | number> {
        if (this._paymentTransactionsCount$ === null) {
            this._paymentTransactionsCount$ = PaymentTransactionService.Instance.GetPaymentTransactionsRowCount({financialTransactionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._paymentTransactionsCount$;
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
            this._receiptsCount$ = ReceiptService.Instance.GetReceiptsRowCount({financialTransactionId: this.id,
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
            this._documentsCount$ = DocumentService.Instance.GetDocumentsRowCount({financialTransactionId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._documentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any FinancialTransactionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.financialTransaction.Reload();
  //
  //  Non Async:
  //
  //     financialTransaction[0].Reload().then(x => {
  //        this.financialTransaction = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      FinancialTransactionService.Instance.GetFinancialTransaction(this.id, includeRelations)
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
     this._financialTransactionChangeHistories = null;
     this._financialTransactionChangeHistoriesPromise = null;
     this._financialTransactionChangeHistoriesSubject.next(null);
     this._financialTransactionChangeHistoriesCount$ = null;

     this._paymentTransactions = null;
     this._paymentTransactionsPromise = null;
     this._paymentTransactionsSubject.next(null);
     this._paymentTransactionsCount$ = null;

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
     * Gets the FinancialTransactionChangeHistories for this FinancialTransaction.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialTransaction.FinancialTransactionChangeHistories.then(financialTransactions => { ... })
     *   or
     *   await this.financialTransaction.financialTransactions
     *
    */
    public get FinancialTransactionChangeHistories(): Promise<FinancialTransactionChangeHistoryData[]> {
        if (this._financialTransactionChangeHistories !== null) {
            return Promise.resolve(this._financialTransactionChangeHistories);
        }

        if (this._financialTransactionChangeHistoriesPromise !== null) {
            return this._financialTransactionChangeHistoriesPromise;
        }

        // Start the load
        this.loadFinancialTransactionChangeHistories();

        return this._financialTransactionChangeHistoriesPromise!;
    }



    private loadFinancialTransactionChangeHistories(): void {

        this._financialTransactionChangeHistoriesPromise = lastValueFrom(
            FinancialTransactionService.Instance.GetFinancialTransactionChangeHistoriesForFinancialTransaction(this.id)
        )
        .then(FinancialTransactionChangeHistories => {
            this._financialTransactionChangeHistories = FinancialTransactionChangeHistories ?? [];
            this._financialTransactionChangeHistoriesSubject.next(this._financialTransactionChangeHistories);
            return this._financialTransactionChangeHistories;
         })
        .catch(err => {
            this._financialTransactionChangeHistories = [];
            this._financialTransactionChangeHistoriesSubject.next(this._financialTransactionChangeHistories);
            throw err;
        })
        .finally(() => {
            this._financialTransactionChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached FinancialTransactionChangeHistory. Call after mutations to force refresh.
     */
    public ClearFinancialTransactionChangeHistoriesCache(): void {
        this._financialTransactionChangeHistories = null;
        this._financialTransactionChangeHistoriesPromise = null;
        this._financialTransactionChangeHistoriesSubject.next(this._financialTransactionChangeHistories);      // Emit to observable
    }

    public get HasFinancialTransactionChangeHistories(): Promise<boolean> {
        return this.FinancialTransactionChangeHistories.then(financialTransactionChangeHistories => financialTransactionChangeHistories.length > 0);
    }


    /**
     *
     * Gets the PaymentTransactions for this FinancialTransaction.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialTransaction.PaymentTransactions.then(financialTransactions => { ... })
     *   or
     *   await this.financialTransaction.financialTransactions
     *
    */
    public get PaymentTransactions(): Promise<PaymentTransactionData[]> {
        if (this._paymentTransactions !== null) {
            return Promise.resolve(this._paymentTransactions);
        }

        if (this._paymentTransactionsPromise !== null) {
            return this._paymentTransactionsPromise;
        }

        // Start the load
        this.loadPaymentTransactions();

        return this._paymentTransactionsPromise!;
    }



    private loadPaymentTransactions(): void {

        this._paymentTransactionsPromise = lastValueFrom(
            FinancialTransactionService.Instance.GetPaymentTransactionsForFinancialTransaction(this.id)
        )
        .then(PaymentTransactions => {
            this._paymentTransactions = PaymentTransactions ?? [];
            this._paymentTransactionsSubject.next(this._paymentTransactions);
            return this._paymentTransactions;
         })
        .catch(err => {
            this._paymentTransactions = [];
            this._paymentTransactionsSubject.next(this._paymentTransactions);
            throw err;
        })
        .finally(() => {
            this._paymentTransactionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PaymentTransaction. Call after mutations to force refresh.
     */
    public ClearPaymentTransactionsCache(): void {
        this._paymentTransactions = null;
        this._paymentTransactionsPromise = null;
        this._paymentTransactionsSubject.next(this._paymentTransactions);      // Emit to observable
    }

    public get HasPaymentTransactions(): Promise<boolean> {
        return this.PaymentTransactions.then(paymentTransactions => paymentTransactions.length > 0);
    }


    /**
     *
     * Gets the Receipts for this FinancialTransaction.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialTransaction.Receipts.then(financialTransactions => { ... })
     *   or
     *   await this.financialTransaction.financialTransactions
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
            FinancialTransactionService.Instance.GetReceiptsForFinancialTransaction(this.id)
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
     * Gets the Documents for this FinancialTransaction.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialTransaction.Documents.then(financialTransactions => { ... })
     *   or
     *   await this.financialTransaction.financialTransactions
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
            FinancialTransactionService.Instance.GetDocumentsForFinancialTransaction(this.id)
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
    //   Template: {{ (financialTransaction.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await financialTransaction.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<FinancialTransactionData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<FinancialTransactionData>> {
        const info = await lastValueFrom(
            FinancialTransactionService.Instance.GetFinancialTransactionChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this FinancialTransactionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this FinancialTransactionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): FinancialTransactionSubmitData {
        return FinancialTransactionService.Instance.ConvertToFinancialTransactionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class FinancialTransactionService extends SecureEndpointBase {

    private static _instance: FinancialTransactionService;
    private listCache: Map<string, Observable<Array<FinancialTransactionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<FinancialTransactionBasicListData>>>;
    private recordCache: Map<string, Observable<FinancialTransactionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private financialTransactionChangeHistoryService: FinancialTransactionChangeHistoryService,
        private paymentTransactionService: PaymentTransactionService,
        private receiptService: ReceiptService,
        private documentService: DocumentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<FinancialTransactionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<FinancialTransactionBasicListData>>>();
        this.recordCache = new Map<string, Observable<FinancialTransactionData>>();

        FinancialTransactionService._instance = this;
    }

    public static get Instance(): FinancialTransactionService {
      return FinancialTransactionService._instance;
    }


    public ClearListCaches(config: FinancialTransactionQueryParameters | null = null) {

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


    public ConvertToFinancialTransactionSubmitData(data: FinancialTransactionData): FinancialTransactionSubmitData {

        let output = new FinancialTransactionSubmitData();

        output.id = data.id;
        output.financialCategoryId = data.financialCategoryId;
        output.financialOfficeId = data.financialOfficeId;
        output.scheduledEventId = data.scheduledEventId;
        output.contactId = data.contactId;
        output.clientId = data.clientId;
        output.contactRole = data.contactRole;
        output.taxCodeId = data.taxCodeId;
        output.fiscalPeriodId = data.fiscalPeriodId;
        output.paymentTypeId = data.paymentTypeId;
        output.transactionDate = data.transactionDate;
        output.description = data.description;
        output.amount = data.amount;
        output.taxAmount = data.taxAmount;
        output.totalAmount = data.totalAmount;
        output.isRevenue = data.isRevenue;
        output.journalEntryType = data.journalEntryType;
        output.referenceNumber = data.referenceNumber;
        output.notes = data.notes;
        output.currencyId = data.currencyId;
        output.exportedDate = data.exportedDate;
        output.externalId = data.externalId;
        output.externalSystemName = data.externalSystemName;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetFinancialTransaction(id: bigint | number, includeRelations: boolean = true) : Observable<FinancialTransactionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const financialTransaction$ = this.requestFinancialTransaction(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get FinancialTransaction", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, financialTransaction$);

            return financialTransaction$;
        }

        return this.recordCache.get(configHash) as Observable<FinancialTransactionData>;
    }

    private requestFinancialTransaction(id: bigint | number, includeRelations: boolean = true) : Observable<FinancialTransactionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FinancialTransactionData>(this.baseUrl + 'api/FinancialTransaction/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveFinancialTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestFinancialTransaction(id, includeRelations));
            }));
    }

    public GetFinancialTransactionList(config: FinancialTransactionQueryParameters | any = null) : Observable<Array<FinancialTransactionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const financialTransactionList$ = this.requestFinancialTransactionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get FinancialTransaction list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, financialTransactionList$);

            return financialTransactionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<FinancialTransactionData>>;
    }


    private requestFinancialTransactionList(config: FinancialTransactionQueryParameters | any) : Observable <Array<FinancialTransactionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<FinancialTransactionData>>(this.baseUrl + 'api/FinancialTransactions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveFinancialTransactionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestFinancialTransactionList(config));
            }));
    }

    public GetFinancialTransactionsRowCount(config: FinancialTransactionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const financialTransactionsRowCount$ = this.requestFinancialTransactionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get FinancialTransactions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, financialTransactionsRowCount$);

            return financialTransactionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestFinancialTransactionsRowCount(config: FinancialTransactionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/FinancialTransactions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestFinancialTransactionsRowCount(config));
            }));
    }

    public GetFinancialTransactionsBasicListData(config: FinancialTransactionQueryParameters | any = null) : Observable<Array<FinancialTransactionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const financialTransactionsBasicListData$ = this.requestFinancialTransactionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get FinancialTransactions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, financialTransactionsBasicListData$);

            return financialTransactionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<FinancialTransactionBasicListData>>;
    }


    private requestFinancialTransactionsBasicListData(config: FinancialTransactionQueryParameters | any) : Observable<Array<FinancialTransactionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<FinancialTransactionBasicListData>>(this.baseUrl + 'api/FinancialTransactions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestFinancialTransactionsBasicListData(config));
            }));

    }


    public PutFinancialTransaction(id: bigint | number, financialTransaction: FinancialTransactionSubmitData) : Observable<FinancialTransactionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<FinancialTransactionData>(this.baseUrl + 'api/FinancialTransaction/' + id.toString(), financialTransaction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFinancialTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutFinancialTransaction(id, financialTransaction));
            }));
    }


    public PostFinancialTransaction(financialTransaction: FinancialTransactionSubmitData) : Observable<FinancialTransactionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<FinancialTransactionData>(this.baseUrl + 'api/FinancialTransaction', financialTransaction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFinancialTransaction(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostFinancialTransaction(financialTransaction));
            }));
    }

  
    public DeleteFinancialTransaction(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/FinancialTransaction/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteFinancialTransaction(id));
            }));
    }

    public RollbackFinancialTransaction(id: bigint | number, versionNumber: bigint | number) : Observable<FinancialTransactionData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<FinancialTransactionData>(this.baseUrl + 'api/FinancialTransaction/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFinancialTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackFinancialTransaction(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a FinancialTransaction.
     */
    public GetFinancialTransactionChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<FinancialTransactionData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<FinancialTransactionData>>(this.baseUrl + 'api/FinancialTransaction/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetFinancialTransactionChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a FinancialTransaction.
     */
    public GetFinancialTransactionAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<FinancialTransactionData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<FinancialTransactionData>[]>(this.baseUrl + 'api/FinancialTransaction/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetFinancialTransactionAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a FinancialTransaction.
     */
    public GetFinancialTransactionVersion(id: bigint | number, version: number): Observable<FinancialTransactionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FinancialTransactionData>(this.baseUrl + 'api/FinancialTransaction/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveFinancialTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetFinancialTransactionVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a FinancialTransaction at a specific point in time.
     */
    public GetFinancialTransactionStateAtTime(id: bigint | number, time: string): Observable<FinancialTransactionData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FinancialTransactionData>(this.baseUrl + 'api/FinancialTransaction/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveFinancialTransaction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetFinancialTransactionStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: FinancialTransactionQueryParameters | any): string {

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

    public userIsSchedulerFinancialTransactionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerFinancialTransactionReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.FinancialTransactions
        //
        if (userIsSchedulerFinancialTransactionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerFinancialTransactionReader = user.readPermission >= 1;
            } else {
                userIsSchedulerFinancialTransactionReader = false;
            }
        }

        return userIsSchedulerFinancialTransactionReader;
    }


    public userIsSchedulerFinancialTransactionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerFinancialTransactionWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.FinancialTransactions
        //
        if (userIsSchedulerFinancialTransactionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerFinancialTransactionWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerFinancialTransactionWriter = false;
          }      
        }

        return userIsSchedulerFinancialTransactionWriter;
    }

    public GetFinancialTransactionChangeHistoriesForFinancialTransaction(financialTransactionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FinancialTransactionChangeHistoryData[]> {
        return this.financialTransactionChangeHistoryService.GetFinancialTransactionChangeHistoryList({
            financialTransactionId: financialTransactionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPaymentTransactionsForFinancialTransaction(financialTransactionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PaymentTransactionData[]> {
        return this.paymentTransactionService.GetPaymentTransactionList({
            financialTransactionId: financialTransactionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetReceiptsForFinancialTransaction(financialTransactionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ReceiptData[]> {
        return this.receiptService.GetReceiptList({
            financialTransactionId: financialTransactionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetDocumentsForFinancialTransaction(financialTransactionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<DocumentData[]> {
        return this.documentService.GetDocumentList({
            financialTransactionId: financialTransactionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full FinancialTransactionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the FinancialTransactionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when FinancialTransactionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveFinancialTransaction(raw: any): FinancialTransactionData {
    if (!raw) return raw;

    //
    // Create a FinancialTransactionData object instance with correct prototype
    //
    const revived = Object.create(FinancialTransactionData.prototype) as FinancialTransactionData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._financialTransactionChangeHistories = null;
    (revived as any)._financialTransactionChangeHistoriesPromise = null;
    (revived as any)._financialTransactionChangeHistoriesSubject = new BehaviorSubject<FinancialTransactionChangeHistoryData[] | null>(null);

    (revived as any)._paymentTransactions = null;
    (revived as any)._paymentTransactionsPromise = null;
    (revived as any)._paymentTransactionsSubject = new BehaviorSubject<PaymentTransactionData[] | null>(null);

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
    // 2. But private methods (loadFinancialTransactionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).FinancialTransactionChangeHistories$ = (revived as any)._financialTransactionChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._financialTransactionChangeHistories === null && (revived as any)._financialTransactionChangeHistoriesPromise === null) {
                (revived as any).loadFinancialTransactionChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._financialTransactionChangeHistoriesCount$ = null;


    (revived as any).PaymentTransactions$ = (revived as any)._paymentTransactionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._paymentTransactions === null && (revived as any)._paymentTransactionsPromise === null) {
                (revived as any).loadPaymentTransactions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._paymentTransactionsCount$ = null;


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
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<FinancialTransactionData> | null>(null);

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

  private ReviveFinancialTransactionList(rawList: any[]): FinancialTransactionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveFinancialTransaction(raw));
  }

}
