/*

   GENERATED SERVICE FOR THE FINANCIALCATEGORY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the FinancialCategory table.

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
import { AccountTypeData } from './account-type.service';
import { FinancialOfficeData } from './financial-office.service';
import { FinancialCategoryChangeHistoryService, FinancialCategoryChangeHistoryData } from './financial-category-change-history.service';
import { ChargeTypeService, ChargeTypeData } from './charge-type.service';
import { FinancialTransactionService, FinancialTransactionData } from './financial-transaction.service';
import { BudgetService, BudgetData } from './budget.service';
import { GeneralLedgerLineService, GeneralLedgerLineData } from './general-ledger-line.service';
import { InvoiceLineItemService, InvoiceLineItemData } from './invoice-line-item.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class FinancialCategoryQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    code: string | null | undefined = null;
    accountTypeId: bigint | number | null | undefined = null;
    financialOfficeId: bigint | number | null | undefined = null;
    parentFinancialCategoryId: bigint | number | null | undefined = null;
    isTaxApplicable: boolean | null | undefined = null;
    defaultAmount: number | null | undefined = null;
    externalAccountId: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class FinancialCategorySubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    code!: string;
    accountTypeId!: bigint | number;
    financialOfficeId: bigint | number | null = null;
    parentFinancialCategoryId: bigint | number | null = null;
    isTaxApplicable!: boolean;
    defaultAmount: number | null = null;
    externalAccountId: string | null = null;
    sequence: bigint | number | null = null;
    color: string | null = null;
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

export class FinancialCategoryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. FinancialCategoryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `financialCategory.FinancialCategoryChildren$` — use with `| async` in templates
//        • Promise:    `financialCategory.FinancialCategoryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="financialCategory.FinancialCategoryChildren$ | async"`), or
//        • Access the promise getter (`financialCategory.FinancialCategoryChildren` or `await financialCategory.FinancialCategoryChildren`)
//    - Simply reading `financialCategory.FinancialCategoryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await financialCategory.Reload()` to refresh the entire object and clear all lazy caches.
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
export class FinancialCategoryData {
    id!: bigint | number;
    name!: string;
    description!: string;
    code!: string;
    accountTypeId!: bigint | number;
    financialOfficeId!: bigint | number;
    parentFinancialCategoryId!: bigint | number;
    isTaxApplicable!: boolean;
    defaultAmount!: number | null;
    externalAccountId!: string | null;
    sequence!: bigint | number;
    color!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    accountType: AccountTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    financialOffice: FinancialOfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    parentFinancialCategory: FinancialCategoryData | null | undefined = null;            // Self referencing navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _financialCategoryChangeHistories: FinancialCategoryChangeHistoryData[] | null = null;
    private _financialCategoryChangeHistoriesPromise: Promise<FinancialCategoryChangeHistoryData[]> | null  = null;
    private _financialCategoryChangeHistoriesSubject = new BehaviorSubject<FinancialCategoryChangeHistoryData[] | null>(null);

                
    private _chargeTypes: ChargeTypeData[] | null = null;
    private _chargeTypesPromise: Promise<ChargeTypeData[]> | null  = null;
    private _chargeTypesSubject = new BehaviorSubject<ChargeTypeData[] | null>(null);

                
    private _financialTransactions: FinancialTransactionData[] | null = null;
    private _financialTransactionsPromise: Promise<FinancialTransactionData[]> | null  = null;
    private _financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

                
    private _budgets: BudgetData[] | null = null;
    private _budgetsPromise: Promise<BudgetData[]> | null  = null;
    private _budgetsSubject = new BehaviorSubject<BudgetData[] | null>(null);

                
    private _generalLedgerLines: GeneralLedgerLineData[] | null = null;
    private _generalLedgerLinesPromise: Promise<GeneralLedgerLineData[]> | null  = null;
    private _generalLedgerLinesSubject = new BehaviorSubject<GeneralLedgerLineData[] | null>(null);

                
    private _invoiceLineItems: InvoiceLineItemData[] | null = null;
    private _invoiceLineItemsPromise: Promise<InvoiceLineItemData[]> | null  = null;
    private _invoiceLineItemsSubject = new BehaviorSubject<InvoiceLineItemData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<FinancialCategoryData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<FinancialCategoryData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<FinancialCategoryData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public FinancialCategoryChangeHistories$ = this._financialCategoryChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._financialCategoryChangeHistories === null && this._financialCategoryChangeHistoriesPromise === null) {
            this.loadFinancialCategoryChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _financialCategoryChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get FinancialCategoryChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._financialCategoryChangeHistoriesCount$ === null) {
            this._financialCategoryChangeHistoriesCount$ = FinancialCategoryChangeHistoryService.Instance.GetFinancialCategoryChangeHistoriesRowCount({financialCategoryId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._financialCategoryChangeHistoriesCount$;
    }



    public ChargeTypes$ = this._chargeTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._chargeTypes === null && this._chargeTypesPromise === null) {
            this.loadChargeTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _chargeTypesCount$: Observable<bigint | number> | null = null;
    public get ChargeTypesCount$(): Observable<bigint | number> {
        if (this._chargeTypesCount$ === null) {
            this._chargeTypesCount$ = ChargeTypeService.Instance.GetChargeTypesRowCount({financialCategoryId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._chargeTypesCount$;
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
            this._financialTransactionsCount$ = FinancialTransactionService.Instance.GetFinancialTransactionsRowCount({financialCategoryId: this.id,
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
            this._budgetsCount$ = BudgetService.Instance.GetBudgetsRowCount({financialCategoryId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._budgetsCount$;
    }



    public GeneralLedgerLines$ = this._generalLedgerLinesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._generalLedgerLines === null && this._generalLedgerLinesPromise === null) {
            this.loadGeneralLedgerLines(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _generalLedgerLinesCount$: Observable<bigint | number> | null = null;
    public get GeneralLedgerLinesCount$(): Observable<bigint | number> {
        if (this._generalLedgerLinesCount$ === null) {
            this._generalLedgerLinesCount$ = GeneralLedgerLineService.Instance.GetGeneralLedgerLinesRowCount({financialCategoryId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._generalLedgerLinesCount$;
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
            this._invoiceLineItemsCount$ = InvoiceLineItemService.Instance.GetInvoiceLineItemsRowCount({financialCategoryId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._invoiceLineItemsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any FinancialCategoryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.financialCategory.Reload();
  //
  //  Non Async:
  //
  //     financialCategory[0].Reload().then(x => {
  //        this.financialCategory = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      FinancialCategoryService.Instance.GetFinancialCategory(this.id, includeRelations)
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
     this._financialCategoryChangeHistories = null;
     this._financialCategoryChangeHistoriesPromise = null;
     this._financialCategoryChangeHistoriesSubject.next(null);
     this._financialCategoryChangeHistoriesCount$ = null;

     this._chargeTypes = null;
     this._chargeTypesPromise = null;
     this._chargeTypesSubject.next(null);
     this._chargeTypesCount$ = null;

     this._financialTransactions = null;
     this._financialTransactionsPromise = null;
     this._financialTransactionsSubject.next(null);
     this._financialTransactionsCount$ = null;

     this._budgets = null;
     this._budgetsPromise = null;
     this._budgetsSubject.next(null);
     this._budgetsCount$ = null;

     this._generalLedgerLines = null;
     this._generalLedgerLinesPromise = null;
     this._generalLedgerLinesSubject.next(null);
     this._generalLedgerLinesCount$ = null;

     this._invoiceLineItems = null;
     this._invoiceLineItemsPromise = null;
     this._invoiceLineItemsSubject.next(null);
     this._invoiceLineItemsCount$ = null;

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
     * Gets the FinancialCategoryChangeHistories for this FinancialCategory.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialCategory.FinancialCategoryChangeHistories.then(financialCategories => { ... })
     *   or
     *   await this.financialCategory.financialCategories
     *
    */
    public get FinancialCategoryChangeHistories(): Promise<FinancialCategoryChangeHistoryData[]> {
        if (this._financialCategoryChangeHistories !== null) {
            return Promise.resolve(this._financialCategoryChangeHistories);
        }

        if (this._financialCategoryChangeHistoriesPromise !== null) {
            return this._financialCategoryChangeHistoriesPromise;
        }

        // Start the load
        this.loadFinancialCategoryChangeHistories();

        return this._financialCategoryChangeHistoriesPromise!;
    }



    private loadFinancialCategoryChangeHistories(): void {

        this._financialCategoryChangeHistoriesPromise = lastValueFrom(
            FinancialCategoryService.Instance.GetFinancialCategoryChangeHistoriesForFinancialCategory(this.id)
        )
        .then(FinancialCategoryChangeHistories => {
            this._financialCategoryChangeHistories = FinancialCategoryChangeHistories ?? [];
            this._financialCategoryChangeHistoriesSubject.next(this._financialCategoryChangeHistories);
            return this._financialCategoryChangeHistories;
         })
        .catch(err => {
            this._financialCategoryChangeHistories = [];
            this._financialCategoryChangeHistoriesSubject.next(this._financialCategoryChangeHistories);
            throw err;
        })
        .finally(() => {
            this._financialCategoryChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached FinancialCategoryChangeHistory. Call after mutations to force refresh.
     */
    public ClearFinancialCategoryChangeHistoriesCache(): void {
        this._financialCategoryChangeHistories = null;
        this._financialCategoryChangeHistoriesPromise = null;
        this._financialCategoryChangeHistoriesSubject.next(this._financialCategoryChangeHistories);      // Emit to observable
    }

    public get HasFinancialCategoryChangeHistories(): Promise<boolean> {
        return this.FinancialCategoryChangeHistories.then(financialCategoryChangeHistories => financialCategoryChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ChargeTypes for this FinancialCategory.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialCategory.ChargeTypes.then(financialCategories => { ... })
     *   or
     *   await this.financialCategory.financialCategories
     *
    */
    public get ChargeTypes(): Promise<ChargeTypeData[]> {
        if (this._chargeTypes !== null) {
            return Promise.resolve(this._chargeTypes);
        }

        if (this._chargeTypesPromise !== null) {
            return this._chargeTypesPromise;
        }

        // Start the load
        this.loadChargeTypes();

        return this._chargeTypesPromise!;
    }



    private loadChargeTypes(): void {

        this._chargeTypesPromise = lastValueFrom(
            FinancialCategoryService.Instance.GetChargeTypesForFinancialCategory(this.id)
        )
        .then(ChargeTypes => {
            this._chargeTypes = ChargeTypes ?? [];
            this._chargeTypesSubject.next(this._chargeTypes);
            return this._chargeTypes;
         })
        .catch(err => {
            this._chargeTypes = [];
            this._chargeTypesSubject.next(this._chargeTypes);
            throw err;
        })
        .finally(() => {
            this._chargeTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ChargeType. Call after mutations to force refresh.
     */
    public ClearChargeTypesCache(): void {
        this._chargeTypes = null;
        this._chargeTypesPromise = null;
        this._chargeTypesSubject.next(this._chargeTypes);      // Emit to observable
    }

    public get HasChargeTypes(): Promise<boolean> {
        return this.ChargeTypes.then(chargeTypes => chargeTypes.length > 0);
    }


    /**
     *
     * Gets the FinancialTransactions for this FinancialCategory.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialCategory.FinancialTransactions.then(financialCategories => { ... })
     *   or
     *   await this.financialCategory.financialCategories
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
            FinancialCategoryService.Instance.GetFinancialTransactionsForFinancialCategory(this.id)
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
     * Gets the Budgets for this FinancialCategory.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialCategory.Budgets.then(financialCategories => { ... })
     *   or
     *   await this.financialCategory.financialCategories
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
            FinancialCategoryService.Instance.GetBudgetsForFinancialCategory(this.id)
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
     * Gets the GeneralLedgerLines for this FinancialCategory.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialCategory.GeneralLedgerLines.then(financialCategories => { ... })
     *   or
     *   await this.financialCategory.financialCategories
     *
    */
    public get GeneralLedgerLines(): Promise<GeneralLedgerLineData[]> {
        if (this._generalLedgerLines !== null) {
            return Promise.resolve(this._generalLedgerLines);
        }

        if (this._generalLedgerLinesPromise !== null) {
            return this._generalLedgerLinesPromise;
        }

        // Start the load
        this.loadGeneralLedgerLines();

        return this._generalLedgerLinesPromise!;
    }



    private loadGeneralLedgerLines(): void {

        this._generalLedgerLinesPromise = lastValueFrom(
            FinancialCategoryService.Instance.GetGeneralLedgerLinesForFinancialCategory(this.id)
        )
        .then(GeneralLedgerLines => {
            this._generalLedgerLines = GeneralLedgerLines ?? [];
            this._generalLedgerLinesSubject.next(this._generalLedgerLines);
            return this._generalLedgerLines;
         })
        .catch(err => {
            this._generalLedgerLines = [];
            this._generalLedgerLinesSubject.next(this._generalLedgerLines);
            throw err;
        })
        .finally(() => {
            this._generalLedgerLinesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached GeneralLedgerLine. Call after mutations to force refresh.
     */
    public ClearGeneralLedgerLinesCache(): void {
        this._generalLedgerLines = null;
        this._generalLedgerLinesPromise = null;
        this._generalLedgerLinesSubject.next(this._generalLedgerLines);      // Emit to observable
    }

    public get HasGeneralLedgerLines(): Promise<boolean> {
        return this.GeneralLedgerLines.then(generalLedgerLines => generalLedgerLines.length > 0);
    }


    /**
     *
     * Gets the InvoiceLineItems for this FinancialCategory.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.financialCategory.InvoiceLineItems.then(financialCategories => { ... })
     *   or
     *   await this.financialCategory.financialCategories
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
            FinancialCategoryService.Instance.GetInvoiceLineItemsForFinancialCategory(this.id)
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




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (financialCategory.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await financialCategory.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<FinancialCategoryData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<FinancialCategoryData>> {
        const info = await lastValueFrom(
            FinancialCategoryService.Instance.GetFinancialCategoryChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this FinancialCategoryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this FinancialCategoryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): FinancialCategorySubmitData {
        return FinancialCategoryService.Instance.ConvertToFinancialCategorySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class FinancialCategoryService extends SecureEndpointBase {

    private static _instance: FinancialCategoryService;
    private listCache: Map<string, Observable<Array<FinancialCategoryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<FinancialCategoryBasicListData>>>;
    private recordCache: Map<string, Observable<FinancialCategoryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private financialCategoryChangeHistoryService: FinancialCategoryChangeHistoryService,
        private chargeTypeService: ChargeTypeService,
        private financialTransactionService: FinancialTransactionService,
        private budgetService: BudgetService,
        private generalLedgerLineService: GeneralLedgerLineService,
        private invoiceLineItemService: InvoiceLineItemService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<FinancialCategoryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<FinancialCategoryBasicListData>>>();
        this.recordCache = new Map<string, Observable<FinancialCategoryData>>();

        FinancialCategoryService._instance = this;
    }

    public static get Instance(): FinancialCategoryService {
      return FinancialCategoryService._instance;
    }


    public ClearListCaches(config: FinancialCategoryQueryParameters | null = null) {

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


    public ConvertToFinancialCategorySubmitData(data: FinancialCategoryData): FinancialCategorySubmitData {

        let output = new FinancialCategorySubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.code = data.code;
        output.accountTypeId = data.accountTypeId;
        output.financialOfficeId = data.financialOfficeId;
        output.parentFinancialCategoryId = data.parentFinancialCategoryId;
        output.isTaxApplicable = data.isTaxApplicable;
        output.defaultAmount = data.defaultAmount;
        output.externalAccountId = data.externalAccountId;
        output.sequence = data.sequence;
        output.color = data.color;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetFinancialCategory(id: bigint | number, includeRelations: boolean = true) : Observable<FinancialCategoryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const financialCategory$ = this.requestFinancialCategory(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get FinancialCategory", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, financialCategory$);

            return financialCategory$;
        }

        return this.recordCache.get(configHash) as Observable<FinancialCategoryData>;
    }

    private requestFinancialCategory(id: bigint | number, includeRelations: boolean = true) : Observable<FinancialCategoryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FinancialCategoryData>(this.baseUrl + 'api/FinancialCategory/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveFinancialCategory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestFinancialCategory(id, includeRelations));
            }));
    }

    public GetFinancialCategoryList(config: FinancialCategoryQueryParameters | any = null) : Observable<Array<FinancialCategoryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const financialCategoryList$ = this.requestFinancialCategoryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get FinancialCategory list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, financialCategoryList$);

            return financialCategoryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<FinancialCategoryData>>;
    }


    private requestFinancialCategoryList(config: FinancialCategoryQueryParameters | any) : Observable <Array<FinancialCategoryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<FinancialCategoryData>>(this.baseUrl + 'api/FinancialCategories', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveFinancialCategoryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestFinancialCategoryList(config));
            }));
    }

    public GetFinancialCategoriesRowCount(config: FinancialCategoryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const financialCategoriesRowCount$ = this.requestFinancialCategoriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get FinancialCategories row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, financialCategoriesRowCount$);

            return financialCategoriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestFinancialCategoriesRowCount(config: FinancialCategoryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/FinancialCategories/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestFinancialCategoriesRowCount(config));
            }));
    }

    public GetFinancialCategoriesBasicListData(config: FinancialCategoryQueryParameters | any = null) : Observable<Array<FinancialCategoryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const financialCategoriesBasicListData$ = this.requestFinancialCategoriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get FinancialCategories basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, financialCategoriesBasicListData$);

            return financialCategoriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<FinancialCategoryBasicListData>>;
    }


    private requestFinancialCategoriesBasicListData(config: FinancialCategoryQueryParameters | any) : Observable<Array<FinancialCategoryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<FinancialCategoryBasicListData>>(this.baseUrl + 'api/FinancialCategories/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestFinancialCategoriesBasicListData(config));
            }));

    }


    public PutFinancialCategory(id: bigint | number, financialCategory: FinancialCategorySubmitData) : Observable<FinancialCategoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<FinancialCategoryData>(this.baseUrl + 'api/FinancialCategory/' + id.toString(), financialCategory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFinancialCategory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutFinancialCategory(id, financialCategory));
            }));
    }


    public PostFinancialCategory(financialCategory: FinancialCategorySubmitData) : Observable<FinancialCategoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<FinancialCategoryData>(this.baseUrl + 'api/FinancialCategory', financialCategory, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFinancialCategory(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostFinancialCategory(financialCategory));
            }));
    }

  
    public DeleteFinancialCategory(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/FinancialCategory/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteFinancialCategory(id));
            }));
    }

    public RollbackFinancialCategory(id: bigint | number, versionNumber: bigint | number) : Observable<FinancialCategoryData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<FinancialCategoryData>(this.baseUrl + 'api/FinancialCategory/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveFinancialCategory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackFinancialCategory(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a FinancialCategory.
     */
    public GetFinancialCategoryChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<FinancialCategoryData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<FinancialCategoryData>>(this.baseUrl + 'api/FinancialCategory/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetFinancialCategoryChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a FinancialCategory.
     */
    public GetFinancialCategoryAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<FinancialCategoryData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<FinancialCategoryData>[]>(this.baseUrl + 'api/FinancialCategory/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetFinancialCategoryAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a FinancialCategory.
     */
    public GetFinancialCategoryVersion(id: bigint | number, version: number): Observable<FinancialCategoryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FinancialCategoryData>(this.baseUrl + 'api/FinancialCategory/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveFinancialCategory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetFinancialCategoryVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a FinancialCategory at a specific point in time.
     */
    public GetFinancialCategoryStateAtTime(id: bigint | number, time: string): Observable<FinancialCategoryData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<FinancialCategoryData>(this.baseUrl + 'api/FinancialCategory/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveFinancialCategory(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetFinancialCategoryStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: FinancialCategoryQueryParameters | any): string {

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

    public userIsSchedulerFinancialCategoryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerFinancialCategoryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.FinancialCategories
        //
        if (userIsSchedulerFinancialCategoryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerFinancialCategoryReader = user.readPermission >= 1;
            } else {
                userIsSchedulerFinancialCategoryReader = false;
            }
        }

        return userIsSchedulerFinancialCategoryReader;
    }


    public userIsSchedulerFinancialCategoryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerFinancialCategoryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.FinancialCategories
        //
        if (userIsSchedulerFinancialCategoryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerFinancialCategoryWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerFinancialCategoryWriter = false;
          }      
        }

        return userIsSchedulerFinancialCategoryWriter;
    }

    public GetFinancialCategoryChangeHistoriesForFinancialCategory(financialCategoryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FinancialCategoryChangeHistoryData[]> {
        return this.financialCategoryChangeHistoryService.GetFinancialCategoryChangeHistoryList({
            financialCategoryId: financialCategoryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetChargeTypesForFinancialCategory(financialCategoryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ChargeTypeData[]> {
        return this.chargeTypeService.GetChargeTypeList({
            financialCategoryId: financialCategoryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetFinancialTransactionsForFinancialCategory(financialCategoryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FinancialTransactionData[]> {
        return this.financialTransactionService.GetFinancialTransactionList({
            financialCategoryId: financialCategoryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBudgetsForFinancialCategory(financialCategoryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BudgetData[]> {
        return this.budgetService.GetBudgetList({
            financialCategoryId: financialCategoryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetGeneralLedgerLinesForFinancialCategory(financialCategoryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GeneralLedgerLineData[]> {
        return this.generalLedgerLineService.GetGeneralLedgerLineList({
            financialCategoryId: financialCategoryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetInvoiceLineItemsForFinancialCategory(financialCategoryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<InvoiceLineItemData[]> {
        return this.invoiceLineItemService.GetInvoiceLineItemList({
            financialCategoryId: financialCategoryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full FinancialCategoryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the FinancialCategoryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when FinancialCategoryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveFinancialCategory(raw: any): FinancialCategoryData {
    if (!raw) return raw;

    //
    // Create a FinancialCategoryData object instance with correct prototype
    //
    const revived = Object.create(FinancialCategoryData.prototype) as FinancialCategoryData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._financialCategoryChangeHistories = null;
    (revived as any)._financialCategoryChangeHistoriesPromise = null;
    (revived as any)._financialCategoryChangeHistoriesSubject = new BehaviorSubject<FinancialCategoryChangeHistoryData[] | null>(null);

    (revived as any)._chargeTypes = null;
    (revived as any)._chargeTypesPromise = null;
    (revived as any)._chargeTypesSubject = new BehaviorSubject<ChargeTypeData[] | null>(null);

    (revived as any)._financialTransactions = null;
    (revived as any)._financialTransactionsPromise = null;
    (revived as any)._financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

    (revived as any)._budgets = null;
    (revived as any)._budgetsPromise = null;
    (revived as any)._budgetsSubject = new BehaviorSubject<BudgetData[] | null>(null);

    (revived as any)._generalLedgerLines = null;
    (revived as any)._generalLedgerLinesPromise = null;
    (revived as any)._generalLedgerLinesSubject = new BehaviorSubject<GeneralLedgerLineData[] | null>(null);

    (revived as any)._invoiceLineItems = null;
    (revived as any)._invoiceLineItemsPromise = null;
    (revived as any)._invoiceLineItemsSubject = new BehaviorSubject<InvoiceLineItemData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadFinancialCategoryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).FinancialCategoryChangeHistories$ = (revived as any)._financialCategoryChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._financialCategoryChangeHistories === null && (revived as any)._financialCategoryChangeHistoriesPromise === null) {
                (revived as any).loadFinancialCategoryChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._financialCategoryChangeHistoriesCount$ = null;


    (revived as any).ChargeTypes$ = (revived as any)._chargeTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._chargeTypes === null && (revived as any)._chargeTypesPromise === null) {
                (revived as any).loadChargeTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._chargeTypesCount$ = null;


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


    (revived as any).GeneralLedgerLines$ = (revived as any)._generalLedgerLinesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._generalLedgerLines === null && (revived as any)._generalLedgerLinesPromise === null) {
                (revived as any).loadGeneralLedgerLines();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._generalLedgerLinesCount$ = null;


    (revived as any).InvoiceLineItems$ = (revived as any)._invoiceLineItemsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._invoiceLineItems === null && (revived as any)._invoiceLineItemsPromise === null) {
                (revived as any).loadInvoiceLineItems();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._invoiceLineItemsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<FinancialCategoryData> | null>(null);

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

  private ReviveFinancialCategoryList(rawList: any[]): FinancialCategoryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveFinancialCategory(raw));
  }

}
