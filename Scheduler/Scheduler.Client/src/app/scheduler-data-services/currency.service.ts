/*

   GENERATED SERVICE FOR THE CURRENCY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Currency table.

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
import { ChargeTypeService, ChargeTypeData } from './charge-type.service';
import { OfficeService, OfficeData } from './office.service';
import { ClientService, ClientData } from './client.service';
import { RateSheetService, RateSheetData } from './rate-sheet.service';
import { EventChargeService, EventChargeData } from './event-charge.service';
import { FinancialTransactionService, FinancialTransactionData } from './financial-transaction.service';
import { BudgetService, BudgetData } from './budget.service';
import { PaymentTransactionService, PaymentTransactionData } from './payment-transaction.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class CurrencyQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    code: string | null | undefined = null;
    color: string | null | undefined = null;
    isDefault: boolean | null | undefined = null;
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
export class CurrencySubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    code!: string;
    color: string | null = null;
    isDefault!: boolean;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class CurrencyBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. CurrencyChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `currency.CurrencyChildren$` — use with `| async` in templates
//        • Promise:    `currency.CurrencyChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="currency.CurrencyChildren$ | async"`), or
//        • Access the promise getter (`currency.CurrencyChildren` or `await currency.CurrencyChildren`)
//    - Simply reading `currency.CurrencyChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await currency.Reload()` to refresh the entire object and clear all lazy caches.
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
export class CurrencyData {
    id!: bigint | number;
    name!: string;
    description!: string;
    code!: string;
    color!: string | null;
    isDefault!: boolean;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _chargeTypes: ChargeTypeData[] | null = null;
    private _chargeTypesPromise: Promise<ChargeTypeData[]> | null  = null;
    private _chargeTypesSubject = new BehaviorSubject<ChargeTypeData[] | null>(null);

                
    private _offices: OfficeData[] | null = null;
    private _officesPromise: Promise<OfficeData[]> | null  = null;
    private _officesSubject = new BehaviorSubject<OfficeData[] | null>(null);

                
    private _clients: ClientData[] | null = null;
    private _clientsPromise: Promise<ClientData[]> | null  = null;
    private _clientsSubject = new BehaviorSubject<ClientData[] | null>(null);

                
    private _rateSheets: RateSheetData[] | null = null;
    private _rateSheetsPromise: Promise<RateSheetData[]> | null  = null;
    private _rateSheetsSubject = new BehaviorSubject<RateSheetData[] | null>(null);

                
    private _eventCharges: EventChargeData[] | null = null;
    private _eventChargesPromise: Promise<EventChargeData[]> | null  = null;
    private _eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);

                
    private _financialTransactions: FinancialTransactionData[] | null = null;
    private _financialTransactionsPromise: Promise<FinancialTransactionData[]> | null  = null;
    private _financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

                
    private _budgets: BudgetData[] | null = null;
    private _budgetsPromise: Promise<BudgetData[]> | null  = null;
    private _budgetsSubject = new BehaviorSubject<BudgetData[] | null>(null);

                
    private _paymentTransactions: PaymentTransactionData[] | null = null;
    private _paymentTransactionsPromise: Promise<PaymentTransactionData[]> | null  = null;
    private _paymentTransactionsSubject = new BehaviorSubject<PaymentTransactionData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
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
            this._chargeTypesCount$ = ChargeTypeService.Instance.GetChargeTypesRowCount({currencyId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._chargeTypesCount$;
    }



    public Offices$ = this._officesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._offices === null && this._officesPromise === null) {
            this.loadOffices(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _officesCount$: Observable<bigint | number> | null = null;
    public get OfficesCount$(): Observable<bigint | number> {
        if (this._officesCount$ === null) {
            this._officesCount$ = OfficeService.Instance.GetOfficesRowCount({currencyId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._officesCount$;
    }



    public Clients$ = this._clientsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._clients === null && this._clientsPromise === null) {
            this.loadClients(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _clientsCount$: Observable<bigint | number> | null = null;
    public get ClientsCount$(): Observable<bigint | number> {
        if (this._clientsCount$ === null) {
            this._clientsCount$ = ClientService.Instance.GetClientsRowCount({currencyId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._clientsCount$;
    }



    public RateSheets$ = this._rateSheetsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._rateSheets === null && this._rateSheetsPromise === null) {
            this.loadRateSheets(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _rateSheetsCount$: Observable<bigint | number> | null = null;
    public get RateSheetsCount$(): Observable<bigint | number> {
        if (this._rateSheetsCount$ === null) {
            this._rateSheetsCount$ = RateSheetService.Instance.GetRateSheetsRowCount({currencyId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._rateSheetsCount$;
    }



    public EventCharges$ = this._eventChargesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventCharges === null && this._eventChargesPromise === null) {
            this.loadEventCharges(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _eventChargesCount$: Observable<bigint | number> | null = null;
    public get EventChargesCount$(): Observable<bigint | number> {
        if (this._eventChargesCount$ === null) {
            this._eventChargesCount$ = EventChargeService.Instance.GetEventChargesRowCount({currencyId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._eventChargesCount$;
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
            this._financialTransactionsCount$ = FinancialTransactionService.Instance.GetFinancialTransactionsRowCount({currencyId: this.id,
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
            this._budgetsCount$ = BudgetService.Instance.GetBudgetsRowCount({currencyId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._budgetsCount$;
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
            this._paymentTransactionsCount$ = PaymentTransactionService.Instance.GetPaymentTransactionsRowCount({currencyId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._paymentTransactionsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any CurrencyData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.currency.Reload();
  //
  //  Non Async:
  //
  //     currency[0].Reload().then(x => {
  //        this.currency = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      CurrencyService.Instance.GetCurrency(this.id, includeRelations)
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
     this._chargeTypes = null;
     this._chargeTypesPromise = null;
     this._chargeTypesSubject.next(null);
     this._chargeTypesCount$ = null;

     this._offices = null;
     this._officesPromise = null;
     this._officesSubject.next(null);
     this._officesCount$ = null;

     this._clients = null;
     this._clientsPromise = null;
     this._clientsSubject.next(null);
     this._clientsCount$ = null;

     this._rateSheets = null;
     this._rateSheetsPromise = null;
     this._rateSheetsSubject.next(null);
     this._rateSheetsCount$ = null;

     this._eventCharges = null;
     this._eventChargesPromise = null;
     this._eventChargesSubject.next(null);
     this._eventChargesCount$ = null;

     this._financialTransactions = null;
     this._financialTransactionsPromise = null;
     this._financialTransactionsSubject.next(null);
     this._financialTransactionsCount$ = null;

     this._budgets = null;
     this._budgetsPromise = null;
     this._budgetsSubject.next(null);
     this._budgetsCount$ = null;

     this._paymentTransactions = null;
     this._paymentTransactionsPromise = null;
     this._paymentTransactionsSubject.next(null);
     this._paymentTransactionsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ChargeTypes for this Currency.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.currency.ChargeTypes.then(currencies => { ... })
     *   or
     *   await this.currency.currencies
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
            CurrencyService.Instance.GetChargeTypesForCurrency(this.id)
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
     * Gets the Offices for this Currency.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.currency.Offices.then(currencies => { ... })
     *   or
     *   await this.currency.currencies
     *
    */
    public get Offices(): Promise<OfficeData[]> {
        if (this._offices !== null) {
            return Promise.resolve(this._offices);
        }

        if (this._officesPromise !== null) {
            return this._officesPromise;
        }

        // Start the load
        this.loadOffices();

        return this._officesPromise!;
    }



    private loadOffices(): void {

        this._officesPromise = lastValueFrom(
            CurrencyService.Instance.GetOfficesForCurrency(this.id)
        )
        .then(Offices => {
            this._offices = Offices ?? [];
            this._officesSubject.next(this._offices);
            return this._offices;
         })
        .catch(err => {
            this._offices = [];
            this._officesSubject.next(this._offices);
            throw err;
        })
        .finally(() => {
            this._officesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Office. Call after mutations to force refresh.
     */
    public ClearOfficesCache(): void {
        this._offices = null;
        this._officesPromise = null;
        this._officesSubject.next(this._offices);      // Emit to observable
    }

    public get HasOffices(): Promise<boolean> {
        return this.Offices.then(offices => offices.length > 0);
    }


    /**
     *
     * Gets the Clients for this Currency.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.currency.Clients.then(currencies => { ... })
     *   or
     *   await this.currency.currencies
     *
    */
    public get Clients(): Promise<ClientData[]> {
        if (this._clients !== null) {
            return Promise.resolve(this._clients);
        }

        if (this._clientsPromise !== null) {
            return this._clientsPromise;
        }

        // Start the load
        this.loadClients();

        return this._clientsPromise!;
    }



    private loadClients(): void {

        this._clientsPromise = lastValueFrom(
            CurrencyService.Instance.GetClientsForCurrency(this.id)
        )
        .then(Clients => {
            this._clients = Clients ?? [];
            this._clientsSubject.next(this._clients);
            return this._clients;
         })
        .catch(err => {
            this._clients = [];
            this._clientsSubject.next(this._clients);
            throw err;
        })
        .finally(() => {
            this._clientsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Client. Call after mutations to force refresh.
     */
    public ClearClientsCache(): void {
        this._clients = null;
        this._clientsPromise = null;
        this._clientsSubject.next(this._clients);      // Emit to observable
    }

    public get HasClients(): Promise<boolean> {
        return this.Clients.then(clients => clients.length > 0);
    }


    /**
     *
     * Gets the RateSheets for this Currency.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.currency.RateSheets.then(currencies => { ... })
     *   or
     *   await this.currency.currencies
     *
    */
    public get RateSheets(): Promise<RateSheetData[]> {
        if (this._rateSheets !== null) {
            return Promise.resolve(this._rateSheets);
        }

        if (this._rateSheetsPromise !== null) {
            return this._rateSheetsPromise;
        }

        // Start the load
        this.loadRateSheets();

        return this._rateSheetsPromise!;
    }



    private loadRateSheets(): void {

        this._rateSheetsPromise = lastValueFrom(
            CurrencyService.Instance.GetRateSheetsForCurrency(this.id)
        )
        .then(RateSheets => {
            this._rateSheets = RateSheets ?? [];
            this._rateSheetsSubject.next(this._rateSheets);
            return this._rateSheets;
         })
        .catch(err => {
            this._rateSheets = [];
            this._rateSheetsSubject.next(this._rateSheets);
            throw err;
        })
        .finally(() => {
            this._rateSheetsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached RateSheet. Call after mutations to force refresh.
     */
    public ClearRateSheetsCache(): void {
        this._rateSheets = null;
        this._rateSheetsPromise = null;
        this._rateSheetsSubject.next(this._rateSheets);      // Emit to observable
    }

    public get HasRateSheets(): Promise<boolean> {
        return this.RateSheets.then(rateSheets => rateSheets.length > 0);
    }


    /**
     *
     * Gets the EventCharges for this Currency.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.currency.EventCharges.then(currencies => { ... })
     *   or
     *   await this.currency.currencies
     *
    */
    public get EventCharges(): Promise<EventChargeData[]> {
        if (this._eventCharges !== null) {
            return Promise.resolve(this._eventCharges);
        }

        if (this._eventChargesPromise !== null) {
            return this._eventChargesPromise;
        }

        // Start the load
        this.loadEventCharges();

        return this._eventChargesPromise!;
    }



    private loadEventCharges(): void {

        this._eventChargesPromise = lastValueFrom(
            CurrencyService.Instance.GetEventChargesForCurrency(this.id)
        )
        .then(EventCharges => {
            this._eventCharges = EventCharges ?? [];
            this._eventChargesSubject.next(this._eventCharges);
            return this._eventCharges;
         })
        .catch(err => {
            this._eventCharges = [];
            this._eventChargesSubject.next(this._eventCharges);
            throw err;
        })
        .finally(() => {
            this._eventChargesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventCharge. Call after mutations to force refresh.
     */
    public ClearEventChargesCache(): void {
        this._eventCharges = null;
        this._eventChargesPromise = null;
        this._eventChargesSubject.next(this._eventCharges);      // Emit to observable
    }

    public get HasEventCharges(): Promise<boolean> {
        return this.EventCharges.then(eventCharges => eventCharges.length > 0);
    }


    /**
     *
     * Gets the FinancialTransactions for this Currency.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.currency.FinancialTransactions.then(currencies => { ... })
     *   or
     *   await this.currency.currencies
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
            CurrencyService.Instance.GetFinancialTransactionsForCurrency(this.id)
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
     * Gets the Budgets for this Currency.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.currency.Budgets.then(currencies => { ... })
     *   or
     *   await this.currency.currencies
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
            CurrencyService.Instance.GetBudgetsForCurrency(this.id)
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
     * Gets the PaymentTransactions for this Currency.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.currency.PaymentTransactions.then(currencies => { ... })
     *   or
     *   await this.currency.currencies
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
            CurrencyService.Instance.GetPaymentTransactionsForCurrency(this.id)
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
     * Updates the state of this CurrencyData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this CurrencyData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): CurrencySubmitData {
        return CurrencyService.Instance.ConvertToCurrencySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class CurrencyService extends SecureEndpointBase {

    private static _instance: CurrencyService;
    private listCache: Map<string, Observable<Array<CurrencyData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<CurrencyBasicListData>>>;
    private recordCache: Map<string, Observable<CurrencyData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private chargeTypeService: ChargeTypeService,
        private officeService: OfficeService,
        private clientService: ClientService,
        private rateSheetService: RateSheetService,
        private eventChargeService: EventChargeService,
        private financialTransactionService: FinancialTransactionService,
        private budgetService: BudgetService,
        private paymentTransactionService: PaymentTransactionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<CurrencyData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<CurrencyBasicListData>>>();
        this.recordCache = new Map<string, Observable<CurrencyData>>();

        CurrencyService._instance = this;
    }

    public static get Instance(): CurrencyService {
      return CurrencyService._instance;
    }


    public ClearListCaches(config: CurrencyQueryParameters | null = null) {

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


    public ConvertToCurrencySubmitData(data: CurrencyData): CurrencySubmitData {

        let output = new CurrencySubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.code = data.code;
        output.color = data.color;
        output.isDefault = data.isDefault;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetCurrency(id: bigint | number, includeRelations: boolean = true) : Observable<CurrencyData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const currency$ = this.requestCurrency(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Currency", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, currency$);

            return currency$;
        }

        return this.recordCache.get(configHash) as Observable<CurrencyData>;
    }

    private requestCurrency(id: bigint | number, includeRelations: boolean = true) : Observable<CurrencyData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CurrencyData>(this.baseUrl + 'api/Currency/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveCurrency(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestCurrency(id, includeRelations));
            }));
    }

    public GetCurrencyList(config: CurrencyQueryParameters | any = null) : Observable<Array<CurrencyData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const currencyList$ = this.requestCurrencyList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Currency list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, currencyList$);

            return currencyList$;
        }

        return this.listCache.get(configHash) as Observable<Array<CurrencyData>>;
    }


    private requestCurrencyList(config: CurrencyQueryParameters | any) : Observable <Array<CurrencyData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CurrencyData>>(this.baseUrl + 'api/Currencies', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveCurrencyList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestCurrencyList(config));
            }));
    }

    public GetCurrenciesRowCount(config: CurrencyQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const currenciesRowCount$ = this.requestCurrenciesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Currencies row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, currenciesRowCount$);

            return currenciesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestCurrenciesRowCount(config: CurrencyQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Currencies/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCurrenciesRowCount(config));
            }));
    }

    public GetCurrenciesBasicListData(config: CurrencyQueryParameters | any = null) : Observable<Array<CurrencyBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const currenciesBasicListData$ = this.requestCurrenciesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Currencies basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, currenciesBasicListData$);

            return currenciesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<CurrencyBasicListData>>;
    }


    private requestCurrenciesBasicListData(config: CurrencyQueryParameters | any) : Observable<Array<CurrencyBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CurrencyBasicListData>>(this.baseUrl + 'api/Currencies/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCurrenciesBasicListData(config));
            }));

    }


    public PutCurrency(id: bigint | number, currency: CurrencySubmitData) : Observable<CurrencyData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CurrencyData>(this.baseUrl + 'api/Currency/' + id.toString(), currency, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCurrency(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutCurrency(id, currency));
            }));
    }


    public PostCurrency(currency: CurrencySubmitData) : Observable<CurrencyData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<CurrencyData>(this.baseUrl + 'api/Currency', currency, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCurrency(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostCurrency(currency));
            }));
    }

  
    public DeleteCurrency(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Currency/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteCurrency(id));
            }));
    }


    private getConfigHash(config: CurrencyQueryParameters | any): string {

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

    public userIsSchedulerCurrencyReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerCurrencyReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Currencies
        //
        if (userIsSchedulerCurrencyReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerCurrencyReader = user.readPermission >= 1;
            } else {
                userIsSchedulerCurrencyReader = false;
            }
        }

        return userIsSchedulerCurrencyReader;
    }


    public userIsSchedulerCurrencyWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerCurrencyWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Currencies
        //
        if (userIsSchedulerCurrencyWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerCurrencyWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerCurrencyWriter = false;
          }      
        }

        return userIsSchedulerCurrencyWriter;
    }

    public GetChargeTypesForCurrency(currencyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ChargeTypeData[]> {
        return this.chargeTypeService.GetChargeTypeList({
            currencyId: currencyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetOfficesForCurrency(currencyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<OfficeData[]> {
        return this.officeService.GetOfficeList({
            currencyId: currencyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetClientsForCurrency(currencyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ClientData[]> {
        return this.clientService.GetClientList({
            currencyId: currencyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetRateSheetsForCurrency(currencyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<RateSheetData[]> {
        return this.rateSheetService.GetRateSheetList({
            currencyId: currencyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventChargesForCurrency(currencyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventChargeData[]> {
        return this.eventChargeService.GetEventChargeList({
            currencyId: currencyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetFinancialTransactionsForCurrency(currencyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FinancialTransactionData[]> {
        return this.financialTransactionService.GetFinancialTransactionList({
            currencyId: currencyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBudgetsForCurrency(currencyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BudgetData[]> {
        return this.budgetService.GetBudgetList({
            currencyId: currencyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPaymentTransactionsForCurrency(currencyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PaymentTransactionData[]> {
        return this.paymentTransactionService.GetPaymentTransactionList({
            currencyId: currencyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full CurrencyData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the CurrencyData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when CurrencyTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveCurrency(raw: any): CurrencyData {
    if (!raw) return raw;

    //
    // Create a CurrencyData object instance with correct prototype
    //
    const revived = Object.create(CurrencyData.prototype) as CurrencyData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._chargeTypes = null;
    (revived as any)._chargeTypesPromise = null;
    (revived as any)._chargeTypesSubject = new BehaviorSubject<ChargeTypeData[] | null>(null);

    (revived as any)._offices = null;
    (revived as any)._officesPromise = null;
    (revived as any)._officesSubject = new BehaviorSubject<OfficeData[] | null>(null);

    (revived as any)._clients = null;
    (revived as any)._clientsPromise = null;
    (revived as any)._clientsSubject = new BehaviorSubject<ClientData[] | null>(null);

    (revived as any)._rateSheets = null;
    (revived as any)._rateSheetsPromise = null;
    (revived as any)._rateSheetsSubject = new BehaviorSubject<RateSheetData[] | null>(null);

    (revived as any)._eventCharges = null;
    (revived as any)._eventChargesPromise = null;
    (revived as any)._eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);

    (revived as any)._financialTransactions = null;
    (revived as any)._financialTransactionsPromise = null;
    (revived as any)._financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

    (revived as any)._budgets = null;
    (revived as any)._budgetsPromise = null;
    (revived as any)._budgetsSubject = new BehaviorSubject<BudgetData[] | null>(null);

    (revived as any)._paymentTransactions = null;
    (revived as any)._paymentTransactionsPromise = null;
    (revived as any)._paymentTransactionsSubject = new BehaviorSubject<PaymentTransactionData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadCurrencyXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ChargeTypes$ = (revived as any)._chargeTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._chargeTypes === null && (revived as any)._chargeTypesPromise === null) {
                (revived as any).loadChargeTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._chargeTypesCount$ = null;


    (revived as any).Offices$ = (revived as any)._officesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._offices === null && (revived as any)._officesPromise === null) {
                (revived as any).loadOffices();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._officesCount$ = null;


    (revived as any).Clients$ = (revived as any)._clientsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._clients === null && (revived as any)._clientsPromise === null) {
                (revived as any).loadClients();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._clientsCount$ = null;


    (revived as any).RateSheets$ = (revived as any)._rateSheetsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._rateSheets === null && (revived as any)._rateSheetsPromise === null) {
                (revived as any).loadRateSheets();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._rateSheetsCount$ = null;


    (revived as any).EventCharges$ = (revived as any)._eventChargesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventCharges === null && (revived as any)._eventChargesPromise === null) {
                (revived as any).loadEventCharges();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._eventChargesCount$ = null;


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


    (revived as any).PaymentTransactions$ = (revived as any)._paymentTransactionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._paymentTransactions === null && (revived as any)._paymentTransactionsPromise === null) {
                (revived as any).loadPaymentTransactions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._paymentTransactionsCount$ = null;



    return revived;
  }

  private ReviveCurrencyList(rawList: any[]): CurrencyData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveCurrency(raw));
  }

}
