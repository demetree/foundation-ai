/*

   GENERATED SERVICE FOR THE FINANCIALOFFICE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the FinancialOffice table.

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
import { FinancialOfficeChangeHistoryService, FinancialOfficeChangeHistoryData } from './financial-office-change-history.service';
import { FinancialCategoryService, FinancialCategoryData } from './financial-category.service';
import { FinancialTransactionService, FinancialTransactionData } from './financial-transaction.service';
import { BudgetService, BudgetData } from './budget.service';
import { GeneralLedgerEntryService, GeneralLedgerEntryData } from './general-ledger-entry.service';
import { InvoiceService, InvoiceData } from './invoice.service';
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
export class FinancialOfficeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    code: string | null | undefined = null;
    contactName: string | null | undefined = null;
    contactEmail: string | null | undefined = null;
    exportFormat: string | null | undefined = null;
    color: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class FinancialOfficeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    code!: string;
    contactName: string | null = null;
    contactEmail: string | null = null;
    exportFormat: string | null = null;
    color: string | null = null;
    sequence: bigint | number | null = null;
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

export class FinancialOfficeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. FinancialOfficeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `financialOffice.FinancialOfficeChildren$` — use with `| async` in templates
//        • Promise:    `financialOffice.FinancialOfficeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="financialOffice.FinancialOfficeChildren$ | async"`), or
//        • Access the promise getter (`financialOffice.FinancialOfficeChildren` or `await financialOffice.FinancialOfficeChildren`)
//    - Simply reading `financialOffice.FinancialOfficeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await financialOffice.Reload()` to refresh the entire object and clear all lazy caches.
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
export class FinancialOfficeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    code!: string;
    contactName!: string | null;
    contactEmail!: string | null;
    exportFormat!: string | null;
    color!: string | null;
    sequence!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _financialOfficeChangeHistories: FinancialOfficeChangeHistoryData[] | null = null;
    private _financialOfficeChangeHistoriesPromise: Promise<FinancialOfficeChangeHistoryData[]> | null  = null;
    private _financialOfficeChangeHistoriesSubject = new BehaviorSubject<FinancialOfficeChangeHistoryData[] | null>(null);

                
    private _financialCategories: FinancialCategoryData[] | null = null;
    private _financialCategoriesPromise: Promise<FinancialCategoryData[]> | null  = null;
    private _financialCategoriesSubject = new BehaviorSubject<FinancialCategoryData[] | null>(null);

                
    private _financialTransactions: FinancialTransactionData[] | null = null;
    private _financialTransactionsPromise: Promise<FinancialTransactionData[]> | null  = null;
    private _financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

                
    private _budgets: BudgetData[] | null = null;
    private _budgetsPromise: Promise<BudgetData[]> | null  = null;
    private _budgetsSubject = new BehaviorSubject<BudgetData[] | null>(null);

                
    private _generalLedgerEntries: GeneralLedgerEntryData[] | null = null;
    private _generalLedgerEntriesPromise: Promise<GeneralLedgerEntryData[]> | null  = null;
    private _generalLedgerEntriesSubject = new BehaviorSubject<GeneralLedgerEntryData[] | null>(null);

                
    private _invoices: InvoiceData[] | null = null;
    private _invoicesPromise: Promise<InvoiceData[]> | null  = null;
    private _invoicesSubject = new BehaviorSubject<InvoiceData[] | null>(null);

                
    private _documents: DocumentData[] | null = null;
    private _documentsPromise: Promise<DocumentData[]> | null  = null;
    private _documentsSubject = new BehaviorSubject<DocumentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<FinancialOfficeData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<FinancialOfficeData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<FinancialOfficeData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public FinancialOfficeChangeHistories$ = this._financialOfficeChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._financialOfficeChangeHistories === null && this._financialOfficeChangeHistoriesPromise === null) {
            this.loadFinancialOfficeChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _financialOfficeChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get FinancialOfficeChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._financialOfficeChangeHistoriesCount$ === null) {
            this._financialOfficeChangeHistoriesCount$ = FinancialOfficeChangeHistoryService.Instance.GetFinancialOfficeChangeHistoriesRowCount({financialOfficeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._financialOfficeChangeHistoriesCount$;
    }



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
            this._financialCategoriesCount$ = FinancialCategoryService.Instance.GetFinancialCategoriesRowCount({financialOfficeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._financialCategoriesCount$;
    }



    public FinancialTransactions$ = this._financialTransactionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._financialTransactions === null && this._financialTransactionsPromise === null) {
            this.loadFinancialTransactions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _financialTransactionsCount$: Observable<bigint | number> | null = null;
    public get FinancialTransactionsCount$(): Observable<bigint | number> {
        if (this._financialTransactionsCount$ === null) {
            this._financialTransactionsCount$ = FinancialTransactionService.Instance.GetFinancialTransactionsRowCount({financialOfficeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._financialTransactionsCount$;
    }



    public Budgets$ = this._budgetsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._budgets === null && this._budgetsPromise === null) {
            this.loadBudgets(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _budgetsCount$: Observable<bigint | number> | null = null;
    public get BudgetsCount$(): Observable<bigint | number> {
        if (this._budgetsCount$ === null) {
            this._budgetsCount$ = BudgetService.Instance.GetBudgetsRowCount({financialOfficeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._budgetsCount$;
    }



    public GeneralLedgerEntries$ = this._generalLedgerEntriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._generalLedgerEntries === null && this._generalLedgerEntriesPromise === null) {
            this.loadGeneralLedgerEntries(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _generalLedgerEntriesCount$: Observable<bigint | number> | null = null;
    public get GeneralLedgerEntriesCount$(): Observable<bigint | number> {
        if (this._generalLedgerEntriesCount$ === null) {
            this._generalLedgerEntriesCount$ = GeneralLedgerEntryService.Instance.GetGeneralLedgerEntriesRowCount({financialOfficeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._generalLedgerEntriesCount$;
    }



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
            this._invoicesCount$ = InvoiceService.Instance.GetInvoicesRowCount({financialOfficeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._invoicesCount$;
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
            this._documentsCount$ = DocumentService.Instance.GetDocumentsRowCount({financialOfficeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._documentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any FinancialOfficeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.financialOffice.Reload();
  //
  //  Non Async:
  //
  //     financialOffice[0].Reload().then(x => {
  //        this.financialOffice = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      FinancialOfficeService.Instance.GetFinancialOffice(this.id, includeRelations)
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
     this._financialOfficeChangeHistories = null;
     this._financialOfficeChangeHistoriesPromise = null;
     this._financialOfficeChangeHistoriesSubject.next(null);
     this._financialOfficeChangeHistoriesCount$ = null;

     this._financialCategories = null;
     this._financialCategoriesPromise = null;
     this._financialCategoriesSubject.next(null);
     this._financialCategoriesCount$ = null;

     this._financialTransactions = null;
     this._financialTransactionsPromise = null;
     this._financialTransactionsSubject.next(null);
     this._financialTransactionsCount$ = null;

     this._budgets = null;
     this._budgetsPromise = null;
     this._budgetsSubject.next(null);
     this._budgetsCount$ = null;

     this._generalLedgerEntries = null;
     this._generalLedgerEntriesPromise = null;
     this._generalLedgerEntriesSubject.next(null);
     this._generalLedgerEntriesCount$ = null;

     this._invoices = null;
     this._invoicesPromise = null;
     this._invoicesSubject.next(null);
     this._invoicesCount$ = null;

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
     * Gets the FinancialOfficeChangeHistories for this FinancialOffice.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialOffice.FinancialOfficeChangeHistories.then(financialOffices => { ... })
     *   or
     *   await this.financialOffice.financialOffices
     *
    */
    public get FinancialOfficeChangeHistories(): Promise<FinancialOfficeChangeHistoryData[]> {
        if (this._financialOfficeChangeHistories !== null) {
            return Promise.resolve(this._financialOfficeChangeHistories);
        }

        if (this._financialOfficeChangeHistoriesPromise !== null) {
            return this._financialOfficeChangeHistoriesPromise;
        }

        // Start the load
        this.loadFinancialOfficeChangeHistories();

        return this._financialOfficeChangeHistoriesPromise!;
    }



    private loadFinancialOfficeChangeHistories(): void {

        this._financialOfficeChangeHistoriesPromise = lastValueFrom(
            FinancialOfficeService.Instance.GetFinancialOfficeChangeHistoriesForFinancialOffice(this.id)
        )
        .then(FinancialOfficeChangeHistories => {
            this._financialOfficeChangeHistories = FinancialOfficeChangeHistories ?? [];
            this._financialOfficeChangeHistoriesSubject.next(this._financialOfficeChangeHistories);
            return this._financialOfficeChangeHistories;
         })
        .catch(err => {
            this._financialOfficeChangeHistories = [];
            this._financialOfficeChangeHistoriesSubject.next(this._financialOfficeChangeHistories);
            throw err;
        })
        .finally(() => {
            this._financialOfficeChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached FinancialOfficeChangeHistory. Call after mutations to force refresh.
     */
    public ClearFinancialOfficeChangeHistoriesCache(): void {
        this._financialOfficeChangeHistories = null;
        this._financialOfficeChangeHistoriesPromise = null;
        this._financialOfficeChangeHistoriesSubject.next(this._financialOfficeChangeHistories);      // Emit to observable
    }

    public get HasFinancialOfficeChangeHistories(): Promise<boolean> {
        return this.FinancialOfficeChangeHistories.then(financialOfficeChangeHistories => financialOfficeChangeHistories.length > 0);
    }


    /**
     *
     * Gets the FinancialCategories for this FinancialOffice.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialOffice.FinancialCategories.then(financialOffices => { ... })
     *   or
     *   await this.financialOffice.financialOffices
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
            FinancialOfficeService.Instance.GetFinancialCategoriesForFinancialOffice(this.id)
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
     *
     * Gets the FinancialTransactions for this FinancialOffice.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialOffice.FinancialTransactions.then(financialOffices => { ... })
     *   or
     *   await this.financialOffice.financialOffices
     *
    */
    public get FinancialTransactions(): Promise<FinancialTransactionData[]> {
        if (this._financialTransactions !== null) {
            return Promise.resolve(this._financialTransactions);
        }

        if (this._financialTransactionsPromise !== null) {
            return this._financialTransactionsPromise;
        }

        // Start the load
        this.loadFinancialTransactions();

        return this._financialTransactionsPromise!;
    }



    private loadFinancialTransactions(): void {

        this._financialTransactionsPromise = lastValueFrom(
            FinancialOfficeService.Instance.GetFinancialTransactionsForFinancialOffice(this.id)
        )
        .then(FinancialTransactions => {
            this._financialTransactions = FinancialTransactions ?? [];
            this._financialTransactionsSubject.next(this._financialTransactions);
            return this._financialTransactions;
         })
        .catch(err => {
            this._financialTransactions = [];
            this._financialTransactionsSubject.next(this._financialTransactions);
            throw err;
        })
        .finally(() => {
            this._financialTransactionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached FinancialTransaction. Call after mutations to force refresh.
     */
    public ClearFinancialTransactionsCache(): void {
        this._financialTransactions = null;
        this._financialTransactionsPromise = null;
        this._financialTransactionsSubject.next(this._financialTransactions);      // Emit to observable
    }

    public get HasFinancialTransactions(): Promise<boolean> {
        return this.FinancialTransactions.then(financialTransactions => financialTransactions.length > 0);
    }


    /**
     *
     * Gets the Budgets for this FinancialOffice.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialOffice.Budgets.then(financialOffices => { ... })
     *   or
     *   await this.financialOffice.financialOffices
     *
    */
    public get Budgets(): Promise<BudgetData[]> {
        if (this._budgets !== null) {
            return Promise.resolve(this._budgets);
        }

        if (this._budgetsPromise !== null) {
            return this._budgetsPromise;
        }

        // Start the load
        this.loadBudgets();

        return this._budgetsPromise!;
    }



    private loadBudgets(): void {

        this._budgetsPromise = lastValueFrom(
            FinancialOfficeService.Instance.GetBudgetsForFinancialOffice(this.id)
        )
        .then(Budgets => {
            this._budgets = Budgets ?? [];
            this._budgetsSubject.next(this._budgets);
            return this._budgets;
         })
        .catch(err => {
            this._budgets = [];
            this._budgetsSubject.next(this._budgets);
            throw err;
        })
        .finally(() => {
            this._budgetsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Budget. Call after mutations to force refresh.
     */
    public ClearBudgetsCache(): void {
        this._budgets = null;
        this._budgetsPromise = null;
        this._budgetsSubject.next(this._budgets);      // Emit to observable
    }

    public get HasBudgets(): Promise<boolean> {
        return this.Budgets.then(budgets => budgets.length > 0);
    }


    /**
     *
     * Gets the GeneralLedgerEntries for this FinancialOffice.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialOffice.GeneralLedgerEntries.then(financialOffices => { ... })
     *   or
     *   await this.financialOffice.financialOffices
     *
    */
    public get GeneralLedgerEntries(): Promise<GeneralLedgerEntryData[]> {
        if (this._generalLedgerEntries !== null) {
            return Promise.resolve(this._generalLedgerEntries);
        }

        if (this._generalLedgerEntriesPromise !== null) {
            return this._generalLedgerEntriesPromise;
        }

        // Start the load
        this.loadGeneralLedgerEntries();

        return this._generalLedgerEntriesPromise!;
    }



    private loadGeneralLedgerEntries(): void {

        this._generalLedgerEntriesPromise = lastValueFrom(
            FinancialOfficeService.Instance.GetGeneralLedgerEntriesForFinancialOffice(this.id)
        )
        .then(GeneralLedgerEntries => {
            this._generalLedgerEntries = GeneralLedgerEntries ?? [];
            this._generalLedgerEntriesSubject.next(this._generalLedgerEntries);
            return this._generalLedgerEntries;
         })
        .catch(err => {
            this._generalLedgerEntries = [];
            this._generalLedgerEntriesSubject.next(this._generalLedgerEntries);
            throw err;
        })
        .finally(() => {
            this._generalLedgerEntriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached GeneralLedgerEntry. Call after mutations to force refresh.
     */
    public ClearGeneralLedgerEntriesCache(): void {
        this._generalLedgerEntries = null;
        this._generalLedgerEntriesPromise = null;
        this._generalLedgerEntriesSubject.next(this._generalLedgerEntries);      // Emit to observable
    }

    public get HasGeneralLedgerEntries(): Promise<boolean> {
        return this.GeneralLedgerEntries.then(generalLedgerEntries => generalLedgerEntries.length > 0);
    }


    /**
     *
     * Gets the Invoices for this FinancialOffice.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialOffice.Invoices.then(financialOffices => { ... })
     *   or
     *   await this.financialOffice.financialOffices
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
            FinancialOfficeService.Instance.GetInvoicesForFinancialOffice(this.id)
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
     *
     * Gets the Documents for this FinancialOffice.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialOffice.Documents.then(financialOffices => { ... })
     *   or
     *   await this.financialOffice.financialOffices
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
            FinancialOfficeService.Instance.GetDocumentsForFinancialOffice(this.id)
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
    //   Template: {{ (financialOffice.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await financialOffice.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<FinancialOfficeData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<FinancialOfficeData>> {
        const info = await lastValueFrom(
            FinancialOfficeService.Instance.GetFinancialOfficeChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this FinancialOfficeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this FinancialOfficeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): FinancialOfficeSubmitData {
        return FinancialOfficeService.Instance.ConvertToFinancialOfficeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class FinancialOfficeService extends SecureEndpointBase {

    private static _instance: FinancialOfficeService;
    private listCache: Map<string, Observable<Array<FinancialOfficeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<FinancialOfficeBasicListData>>>;
    private recordCache: Map<string, Observable<FinancialOfficeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private financialOfficeChangeHistoryService: FinancialOfficeChangeHistoryService,
        private financialCategoryService: FinancialCategoryService,
        private financialTransactionService: FinancialTransactionService,
        private budgetService: BudgetService,
        private generalLedgerEntryService: GeneralLedgerEntryService,
        private invoiceService: InvoiceService,
        private documentService: DocumentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<FinancialOfficeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<FinancialOfficeBasicListData>>>();
        this.recordCache = new Map<string, Observable<FinancialOfficeData>>();

        FinancialOfficeService._instance = this;
    }

    public static get Instance(): FinancialOfficeService {
      return FinancialOfficeService._instance;
    }


    public ClearListCaches(config: FinancialOfficeQueryParameters | null = null) {

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


    public ConvertToFinancialOfficeSubmitData(data: FinancialOfficeData): FinancialOfficeSubmitData {

        let output = new FinancialOfficeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.code = data.code;
        output.contactName = data.contactName;
        output.contactEmail = data.contactEmail;
        output.exportFormat = data.exportFormat;
        output.color = data.color;
        output.sequence = data.sequence;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetFinancialOffice(id: bigint | number, includeRelations: boolean = true) : Observable<FinancialOfficeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const financialOffice$ = this.requestFinancialOffice(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get FinancialOffice", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, financialOffice$);

            return financialOffice$;
        }

        return this.recordCache.get(configHash) as Observable<FinancialOfficeData>;
    }

    private requestFinancialOffice(id: bigint | number, includeRelations: boolean = true) : Observable<FinancialOfficeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FinancialOfficeData>(this.baseUrl + 'api/FinancialOffice/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveFinancialOffice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestFinancialOffice(id, includeRelations));
            }));
    }

    public GetFinancialOfficeList(config: FinancialOfficeQueryParameters | any = null) : Observable<Array<FinancialOfficeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const financialOfficeList$ = this.requestFinancialOfficeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get FinancialOffice list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, financialOfficeList$);

            return financialOfficeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<FinancialOfficeData>>;
    }


    private requestFinancialOfficeList(config: FinancialOfficeQueryParameters | any) : Observable <Array<FinancialOfficeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<FinancialOfficeData>>(this.baseUrl + 'api/FinancialOffices', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveFinancialOfficeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestFinancialOfficeList(config));
            }));
    }

    public GetFinancialOfficesRowCount(config: FinancialOfficeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const financialOfficesRowCount$ = this.requestFinancialOfficesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get FinancialOffices row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, financialOfficesRowCount$);

            return financialOfficesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestFinancialOfficesRowCount(config: FinancialOfficeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/FinancialOffices/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestFinancialOfficesRowCount(config));
            }));
    }

    public GetFinancialOfficesBasicListData(config: FinancialOfficeQueryParameters | any = null) : Observable<Array<FinancialOfficeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const financialOfficesBasicListData$ = this.requestFinancialOfficesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get FinancialOffices basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, financialOfficesBasicListData$);

            return financialOfficesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<FinancialOfficeBasicListData>>;
    }


    private requestFinancialOfficesBasicListData(config: FinancialOfficeQueryParameters | any) : Observable<Array<FinancialOfficeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<FinancialOfficeBasicListData>>(this.baseUrl + 'api/FinancialOffices/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestFinancialOfficesBasicListData(config));
            }));

    }


    public PutFinancialOffice(id: bigint | number, financialOffice: FinancialOfficeSubmitData) : Observable<FinancialOfficeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<FinancialOfficeData>(this.baseUrl + 'api/FinancialOffice/' + id.toString(), financialOffice, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFinancialOffice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutFinancialOffice(id, financialOffice));
            }));
    }


    public PostFinancialOffice(financialOffice: FinancialOfficeSubmitData) : Observable<FinancialOfficeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<FinancialOfficeData>(this.baseUrl + 'api/FinancialOffice', financialOffice, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFinancialOffice(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostFinancialOffice(financialOffice));
            }));
    }

  
    public DeleteFinancialOffice(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/FinancialOffice/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteFinancialOffice(id));
            }));
    }

    public RollbackFinancialOffice(id: bigint | number, versionNumber: bigint | number) : Observable<FinancialOfficeData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<FinancialOfficeData>(this.baseUrl + 'api/FinancialOffice/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFinancialOffice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackFinancialOffice(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a FinancialOffice.
     */
    public GetFinancialOfficeChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<FinancialOfficeData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<FinancialOfficeData>>(this.baseUrl + 'api/FinancialOffice/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetFinancialOfficeChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a FinancialOffice.
     */
    public GetFinancialOfficeAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<FinancialOfficeData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<FinancialOfficeData>[]>(this.baseUrl + 'api/FinancialOffice/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetFinancialOfficeAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a FinancialOffice.
     */
    public GetFinancialOfficeVersion(id: bigint | number, version: number): Observable<FinancialOfficeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FinancialOfficeData>(this.baseUrl + 'api/FinancialOffice/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveFinancialOffice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetFinancialOfficeVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a FinancialOffice at a specific point in time.
     */
    public GetFinancialOfficeStateAtTime(id: bigint | number, time: string): Observable<FinancialOfficeData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FinancialOfficeData>(this.baseUrl + 'api/FinancialOffice/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveFinancialOffice(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetFinancialOfficeStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: FinancialOfficeQueryParameters | any): string {

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

    public userIsSchedulerFinancialOfficeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerFinancialOfficeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.FinancialOffices
        //
        if (userIsSchedulerFinancialOfficeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerFinancialOfficeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerFinancialOfficeReader = false;
            }
        }

        return userIsSchedulerFinancialOfficeReader;
    }


    public userIsSchedulerFinancialOfficeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerFinancialOfficeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.FinancialOffices
        //
        if (userIsSchedulerFinancialOfficeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerFinancialOfficeWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerFinancialOfficeWriter = false;
          }      
        }

        return userIsSchedulerFinancialOfficeWriter;
    }

    public GetFinancialOfficeChangeHistoriesForFinancialOffice(financialOfficeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FinancialOfficeChangeHistoryData[]> {
        return this.financialOfficeChangeHistoryService.GetFinancialOfficeChangeHistoryList({
            financialOfficeId: financialOfficeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetFinancialCategoriesForFinancialOffice(financialOfficeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FinancialCategoryData[]> {
        return this.financialCategoryService.GetFinancialCategoryList({
            financialOfficeId: financialOfficeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetFinancialTransactionsForFinancialOffice(financialOfficeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FinancialTransactionData[]> {
        return this.financialTransactionService.GetFinancialTransactionList({
            financialOfficeId: financialOfficeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBudgetsForFinancialOffice(financialOfficeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BudgetData[]> {
        return this.budgetService.GetBudgetList({
            financialOfficeId: financialOfficeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetGeneralLedgerEntriesForFinancialOffice(financialOfficeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GeneralLedgerEntryData[]> {
        return this.generalLedgerEntryService.GetGeneralLedgerEntryList({
            financialOfficeId: financialOfficeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetInvoicesForFinancialOffice(financialOfficeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<InvoiceData[]> {
        return this.invoiceService.GetInvoiceList({
            financialOfficeId: financialOfficeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetDocumentsForFinancialOffice(financialOfficeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<DocumentData[]> {
        return this.documentService.GetDocumentList({
            financialOfficeId: financialOfficeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full FinancialOfficeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the FinancialOfficeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when FinancialOfficeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveFinancialOffice(raw: any): FinancialOfficeData {
    if (!raw) return raw;

    //
    // Create a FinancialOfficeData object instance with correct prototype
    //
    const revived = Object.create(FinancialOfficeData.prototype) as FinancialOfficeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._financialOfficeChangeHistories = null;
    (revived as any)._financialOfficeChangeHistoriesPromise = null;
    (revived as any)._financialOfficeChangeHistoriesSubject = new BehaviorSubject<FinancialOfficeChangeHistoryData[] | null>(null);

    (revived as any)._financialCategories = null;
    (revived as any)._financialCategoriesPromise = null;
    (revived as any)._financialCategoriesSubject = new BehaviorSubject<FinancialCategoryData[] | null>(null);

    (revived as any)._financialTransactions = null;
    (revived as any)._financialTransactionsPromise = null;
    (revived as any)._financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

    (revived as any)._budgets = null;
    (revived as any)._budgetsPromise = null;
    (revived as any)._budgetsSubject = new BehaviorSubject<BudgetData[] | null>(null);

    (revived as any)._generalLedgerEntries = null;
    (revived as any)._generalLedgerEntriesPromise = null;
    (revived as any)._generalLedgerEntriesSubject = new BehaviorSubject<GeneralLedgerEntryData[] | null>(null);

    (revived as any)._invoices = null;
    (revived as any)._invoicesPromise = null;
    (revived as any)._invoicesSubject = new BehaviorSubject<InvoiceData[] | null>(null);

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
    // 2. But private methods (loadFinancialOfficeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).FinancialOfficeChangeHistories$ = (revived as any)._financialOfficeChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._financialOfficeChangeHistories === null && (revived as any)._financialOfficeChangeHistoriesPromise === null) {
                (revived as any).loadFinancialOfficeChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._financialOfficeChangeHistoriesCount$ = null;


    (revived as any).FinancialCategories$ = (revived as any)._financialCategoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._financialCategories === null && (revived as any)._financialCategoriesPromise === null) {
                (revived as any).loadFinancialCategories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._financialCategoriesCount$ = null;


    (revived as any).FinancialTransactions$ = (revived as any)._financialTransactionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._financialTransactions === null && (revived as any)._financialTransactionsPromise === null) {
                (revived as any).loadFinancialTransactions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._financialTransactionsCount$ = null;


    (revived as any).Budgets$ = (revived as any)._budgetsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._budgets === null && (revived as any)._budgetsPromise === null) {
                (revived as any).loadBudgets();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._budgetsCount$ = null;


    (revived as any).GeneralLedgerEntries$ = (revived as any)._generalLedgerEntriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._generalLedgerEntries === null && (revived as any)._generalLedgerEntriesPromise === null) {
                (revived as any).loadGeneralLedgerEntries();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._generalLedgerEntriesCount$ = null;


    (revived as any).Invoices$ = (revived as any)._invoicesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._invoices === null && (revived as any)._invoicesPromise === null) {
                (revived as any).loadInvoices();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._invoicesCount$ = null;


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
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<FinancialOfficeData> | null>(null);

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

  private ReviveFinancialOfficeList(rawList: any[]): FinancialOfficeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveFinancialOffice(raw));
  }

}
