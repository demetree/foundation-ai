/*

   GENERATED SERVICE FOR THE CLIENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Client table.

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
import { ClientTypeData } from './client-type.service';
import { CurrencyData } from './currency.service';
import { TimeZoneData } from './time-zone.service';
import { CalendarData } from './calendar.service';
import { StateProvinceData } from './state-province.service';
import { CountryData } from './country.service';
import { ClientChangeHistoryService, ClientChangeHistoryData } from './client-change-history.service';
import { ClientContactService, ClientContactData } from './client-contact.service';
import { SchedulingTargetService, SchedulingTargetData } from './scheduling-target.service';
import { SchedulingTargetAddressService, SchedulingTargetAddressData } from './scheduling-target-address.service';
import { ScheduledEventService, ScheduledEventData } from './scheduled-event.service';
import { FinancialTransactionService, FinancialTransactionData } from './financial-transaction.service';
import { InvoiceService, InvoiceData } from './invoice.service';
import { ReceiptService, ReceiptData } from './receipt.service';
import { ConstituentService, ConstituentData } from './constituent.service';
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
export class ClientQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    clientTypeId: bigint | number | null | undefined = null;
    currencyId: bigint | number | null | undefined = null;
    timeZoneId: bigint | number | null | undefined = null;
    calendarId: bigint | number | null | undefined = null;
    addressLine1: string | null | undefined = null;
    addressLine2: string | null | undefined = null;
    city: string | null | undefined = null;
    postalCode: string | null | undefined = null;
    stateProvinceId: bigint | number | null | undefined = null;
    countryId: bigint | number | null | undefined = null;
    phone: string | null | undefined = null;
    email: string | null | undefined = null;
    latitude: number | null | undefined = null;
    longitude: number | null | undefined = null;
    notes: string | null | undefined = null;
    externalId: string | null | undefined = null;
    color: string | null | undefined = null;
    attributes: string | null | undefined = null;
    avatarFileName: string | null | undefined = null;
    avatarSize: bigint | number | null | undefined = null;
    avatarMimeType: string | null | undefined = null;
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
export class ClientSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    clientTypeId!: bigint | number;
    currencyId!: bigint | number;
    timeZoneId!: bigint | number;
    calendarId: bigint | number | null = null;
    addressLine1!: string;
    addressLine2: string | null = null;
    city!: string;
    postalCode: string | null = null;
    stateProvinceId!: bigint | number;
    countryId!: bigint | number;
    phone: string | null = null;
    email: string | null = null;
    latitude: number | null = null;
    longitude: number | null = null;
    notes: string | null = null;
    externalId: string | null = null;
    color: string | null = null;
    attributes: string | null = null;
    avatarFileName: string | null = null;
    avatarSize: bigint | number | null = null;
    avatarData: string | null = null;
    avatarMimeType: string | null = null;
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

export class ClientBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ClientChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `client.ClientChildren$` — use with `| async` in templates
//        • Promise:    `client.ClientChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="client.ClientChildren$ | async"`), or
//        • Access the promise getter (`client.ClientChildren` or `await client.ClientChildren`)
//    - Simply reading `client.ClientChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await client.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ClientData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    clientTypeId!: bigint | number;
    currencyId!: bigint | number;
    timeZoneId!: bigint | number;
    calendarId!: bigint | number;
    addressLine1!: string;
    addressLine2!: string | null;
    city!: string;
    postalCode!: string | null;
    stateProvinceId!: bigint | number;
    countryId!: bigint | number;
    phone!: string | null;
    email!: string | null;
    latitude!: number | null;
    longitude!: number | null;
    notes!: string | null;
    externalId!: string | null;
    color!: string | null;
    attributes!: string | null;
    avatarFileName!: string | null;
    avatarSize!: bigint | number;
    avatarData!: string | null;
    avatarMimeType!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    calendar: CalendarData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    clientType: ClientTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    country: CountryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    currency: CurrencyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    stateProvince: StateProvinceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    timeZone: TimeZoneData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _clientChangeHistories: ClientChangeHistoryData[] | null = null;
    private _clientChangeHistoriesPromise: Promise<ClientChangeHistoryData[]> | null  = null;
    private _clientChangeHistoriesSubject = new BehaviorSubject<ClientChangeHistoryData[] | null>(null);

                
    private _clientContacts: ClientContactData[] | null = null;
    private _clientContactsPromise: Promise<ClientContactData[]> | null  = null;
    private _clientContactsSubject = new BehaviorSubject<ClientContactData[] | null>(null);

                
    private _schedulingTargets: SchedulingTargetData[] | null = null;
    private _schedulingTargetsPromise: Promise<SchedulingTargetData[]> | null  = null;
    private _schedulingTargetsSubject = new BehaviorSubject<SchedulingTargetData[] | null>(null);

                
    private _schedulingTargetAddresses: SchedulingTargetAddressData[] | null = null;
    private _schedulingTargetAddressesPromise: Promise<SchedulingTargetAddressData[]> | null  = null;
    private _schedulingTargetAddressesSubject = new BehaviorSubject<SchedulingTargetAddressData[] | null>(null);

                
    private _scheduledEvents: ScheduledEventData[] | null = null;
    private _scheduledEventsPromise: Promise<ScheduledEventData[]> | null  = null;
    private _scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

                
    private _financialTransactions: FinancialTransactionData[] | null = null;
    private _financialTransactionsPromise: Promise<FinancialTransactionData[]> | null  = null;
    private _financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

                
    private _invoices: InvoiceData[] | null = null;
    private _invoicesPromise: Promise<InvoiceData[]> | null  = null;
    private _invoicesSubject = new BehaviorSubject<InvoiceData[] | null>(null);

                
    private _receipts: ReceiptData[] | null = null;
    private _receiptsPromise: Promise<ReceiptData[]> | null  = null;
    private _receiptsSubject = new BehaviorSubject<ReceiptData[] | null>(null);

                
    private _constituents: ConstituentData[] | null = null;
    private _constituentsPromise: Promise<ConstituentData[]> | null  = null;
    private _constituentsSubject = new BehaviorSubject<ConstituentData[] | null>(null);

                
    private _documents: DocumentData[] | null = null;
    private _documentsPromise: Promise<DocumentData[]> | null  = null;
    private _documentsSubject = new BehaviorSubject<DocumentData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ClientData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ClientData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ClientData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ClientChangeHistories$ = this._clientChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._clientChangeHistories === null && this._clientChangeHistoriesPromise === null) {
            this.loadClientChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _clientChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ClientChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._clientChangeHistoriesCount$ === null) {
            this._clientChangeHistoriesCount$ = ClientChangeHistoryService.Instance.GetClientChangeHistoriesRowCount({clientId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._clientChangeHistoriesCount$;
    }



    public ClientContacts$ = this._clientContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._clientContacts === null && this._clientContactsPromise === null) {
            this.loadClientContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _clientContactsCount$: Observable<bigint | number> | null = null;
    public get ClientContactsCount$(): Observable<bigint | number> {
        if (this._clientContactsCount$ === null) {
            this._clientContactsCount$ = ClientContactService.Instance.GetClientContactsRowCount({clientId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._clientContactsCount$;
    }



    public SchedulingTargets$ = this._schedulingTargetsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargets === null && this._schedulingTargetsPromise === null) {
            this.loadSchedulingTargets(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _schedulingTargetsCount$: Observable<bigint | number> | null = null;
    public get SchedulingTargetsCount$(): Observable<bigint | number> {
        if (this._schedulingTargetsCount$ === null) {
            this._schedulingTargetsCount$ = SchedulingTargetService.Instance.GetSchedulingTargetsRowCount({clientId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._schedulingTargetsCount$;
    }



    public SchedulingTargetAddresses$ = this._schedulingTargetAddressesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetAddresses === null && this._schedulingTargetAddressesPromise === null) {
            this.loadSchedulingTargetAddresses(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _schedulingTargetAddressesCount$: Observable<bigint | number> | null = null;
    public get SchedulingTargetAddressesCount$(): Observable<bigint | number> {
        if (this._schedulingTargetAddressesCount$ === null) {
            this._schedulingTargetAddressesCount$ = SchedulingTargetAddressService.Instance.GetSchedulingTargetAddressesRowCount({clientId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._schedulingTargetAddressesCount$;
    }



    public ScheduledEvents$ = this._scheduledEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEvents === null && this._scheduledEventsPromise === null) {
            this.loadScheduledEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _scheduledEventsCount$: Observable<bigint | number> | null = null;
    public get ScheduledEventsCount$(): Observable<bigint | number> {
        if (this._scheduledEventsCount$ === null) {
            this._scheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({clientId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._scheduledEventsCount$;
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
            this._financialTransactionsCount$ = FinancialTransactionService.Instance.GetFinancialTransactionsRowCount({clientId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._financialTransactionsCount$;
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
            this._invoicesCount$ = InvoiceService.Instance.GetInvoicesRowCount({clientId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._invoicesCount$;
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
            this._receiptsCount$ = ReceiptService.Instance.GetReceiptsRowCount({clientId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._receiptsCount$;
    }



    public Constituents$ = this._constituentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._constituents === null && this._constituentsPromise === null) {
            this.loadConstituents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _constituentsCount$: Observable<bigint | number> | null = null;
    public get ConstituentsCount$(): Observable<bigint | number> {
        if (this._constituentsCount$ === null) {
            this._constituentsCount$ = ConstituentService.Instance.GetConstituentsRowCount({clientId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._constituentsCount$;
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
            this._documentsCount$ = DocumentService.Instance.GetDocumentsRowCount({clientId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._documentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ClientData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.client.Reload();
  //
  //  Non Async:
  //
  //     client[0].Reload().then(x => {
  //        this.client = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ClientService.Instance.GetClient(this.id, includeRelations)
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
     this._clientChangeHistories = null;
     this._clientChangeHistoriesPromise = null;
     this._clientChangeHistoriesSubject.next(null);
     this._clientChangeHistoriesCount$ = null;

     this._clientContacts = null;
     this._clientContactsPromise = null;
     this._clientContactsSubject.next(null);
     this._clientContactsCount$ = null;

     this._schedulingTargets = null;
     this._schedulingTargetsPromise = null;
     this._schedulingTargetsSubject.next(null);
     this._schedulingTargetsCount$ = null;

     this._schedulingTargetAddresses = null;
     this._schedulingTargetAddressesPromise = null;
     this._schedulingTargetAddressesSubject.next(null);
     this._schedulingTargetAddressesCount$ = null;

     this._scheduledEvents = null;
     this._scheduledEventsPromise = null;
     this._scheduledEventsSubject.next(null);
     this._scheduledEventsCount$ = null;

     this._financialTransactions = null;
     this._financialTransactionsPromise = null;
     this._financialTransactionsSubject.next(null);
     this._financialTransactionsCount$ = null;

     this._invoices = null;
     this._invoicesPromise = null;
     this._invoicesSubject.next(null);
     this._invoicesCount$ = null;

     this._receipts = null;
     this._receiptsPromise = null;
     this._receiptsSubject.next(null);
     this._receiptsCount$ = null;

     this._constituents = null;
     this._constituentsPromise = null;
     this._constituentsSubject.next(null);
     this._constituentsCount$ = null;

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
     * Gets the ClientChangeHistories for this Client.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.client.ClientChangeHistories.then(clients => { ... })
     *   or
     *   await this.client.clients
     *
    */
    public get ClientChangeHistories(): Promise<ClientChangeHistoryData[]> {
        if (this._clientChangeHistories !== null) {
            return Promise.resolve(this._clientChangeHistories);
        }

        if (this._clientChangeHistoriesPromise !== null) {
            return this._clientChangeHistoriesPromise;
        }

        // Start the load
        this.loadClientChangeHistories();

        return this._clientChangeHistoriesPromise!;
    }



    private loadClientChangeHistories(): void {

        this._clientChangeHistoriesPromise = lastValueFrom(
            ClientService.Instance.GetClientChangeHistoriesForClient(this.id)
        )
        .then(ClientChangeHistories => {
            this._clientChangeHistories = ClientChangeHistories ?? [];
            this._clientChangeHistoriesSubject.next(this._clientChangeHistories);
            return this._clientChangeHistories;
         })
        .catch(err => {
            this._clientChangeHistories = [];
            this._clientChangeHistoriesSubject.next(this._clientChangeHistories);
            throw err;
        })
        .finally(() => {
            this._clientChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ClientChangeHistory. Call after mutations to force refresh.
     */
    public ClearClientChangeHistoriesCache(): void {
        this._clientChangeHistories = null;
        this._clientChangeHistoriesPromise = null;
        this._clientChangeHistoriesSubject.next(this._clientChangeHistories);      // Emit to observable
    }

    public get HasClientChangeHistories(): Promise<boolean> {
        return this.ClientChangeHistories.then(clientChangeHistories => clientChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ClientContacts for this Client.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.client.ClientContacts.then(clients => { ... })
     *   or
     *   await this.client.clients
     *
    */
    public get ClientContacts(): Promise<ClientContactData[]> {
        if (this._clientContacts !== null) {
            return Promise.resolve(this._clientContacts);
        }

        if (this._clientContactsPromise !== null) {
            return this._clientContactsPromise;
        }

        // Start the load
        this.loadClientContacts();

        return this._clientContactsPromise!;
    }



    private loadClientContacts(): void {

        this._clientContactsPromise = lastValueFrom(
            ClientService.Instance.GetClientContactsForClient(this.id)
        )
        .then(ClientContacts => {
            this._clientContacts = ClientContacts ?? [];
            this._clientContactsSubject.next(this._clientContacts);
            return this._clientContacts;
         })
        .catch(err => {
            this._clientContacts = [];
            this._clientContactsSubject.next(this._clientContacts);
            throw err;
        })
        .finally(() => {
            this._clientContactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ClientContact. Call after mutations to force refresh.
     */
    public ClearClientContactsCache(): void {
        this._clientContacts = null;
        this._clientContactsPromise = null;
        this._clientContactsSubject.next(this._clientContacts);      // Emit to observable
    }

    public get HasClientContacts(): Promise<boolean> {
        return this.ClientContacts.then(clientContacts => clientContacts.length > 0);
    }


    /**
     *
     * Gets the SchedulingTargets for this Client.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.client.SchedulingTargets.then(clients => { ... })
     *   or
     *   await this.client.clients
     *
    */
    public get SchedulingTargets(): Promise<SchedulingTargetData[]> {
        if (this._schedulingTargets !== null) {
            return Promise.resolve(this._schedulingTargets);
        }

        if (this._schedulingTargetsPromise !== null) {
            return this._schedulingTargetsPromise;
        }

        // Start the load
        this.loadSchedulingTargets();

        return this._schedulingTargetsPromise!;
    }



    private loadSchedulingTargets(): void {

        this._schedulingTargetsPromise = lastValueFrom(
            ClientService.Instance.GetSchedulingTargetsForClient(this.id)
        )
        .then(SchedulingTargets => {
            this._schedulingTargets = SchedulingTargets ?? [];
            this._schedulingTargetsSubject.next(this._schedulingTargets);
            return this._schedulingTargets;
         })
        .catch(err => {
            this._schedulingTargets = [];
            this._schedulingTargetsSubject.next(this._schedulingTargets);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTarget. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetsCache(): void {
        this._schedulingTargets = null;
        this._schedulingTargetsPromise = null;
        this._schedulingTargetsSubject.next(this._schedulingTargets);      // Emit to observable
    }

    public get HasSchedulingTargets(): Promise<boolean> {
        return this.SchedulingTargets.then(schedulingTargets => schedulingTargets.length > 0);
    }


    /**
     *
     * Gets the SchedulingTargetAddresses for this Client.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.client.SchedulingTargetAddresses.then(clients => { ... })
     *   or
     *   await this.client.clients
     *
    */
    public get SchedulingTargetAddresses(): Promise<SchedulingTargetAddressData[]> {
        if (this._schedulingTargetAddresses !== null) {
            return Promise.resolve(this._schedulingTargetAddresses);
        }

        if (this._schedulingTargetAddressesPromise !== null) {
            return this._schedulingTargetAddressesPromise;
        }

        // Start the load
        this.loadSchedulingTargetAddresses();

        return this._schedulingTargetAddressesPromise!;
    }



    private loadSchedulingTargetAddresses(): void {

        this._schedulingTargetAddressesPromise = lastValueFrom(
            ClientService.Instance.GetSchedulingTargetAddressesForClient(this.id)
        )
        .then(SchedulingTargetAddresses => {
            this._schedulingTargetAddresses = SchedulingTargetAddresses ?? [];
            this._schedulingTargetAddressesSubject.next(this._schedulingTargetAddresses);
            return this._schedulingTargetAddresses;
         })
        .catch(err => {
            this._schedulingTargetAddresses = [];
            this._schedulingTargetAddressesSubject.next(this._schedulingTargetAddresses);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetAddressesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTargetAddress. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetAddressesCache(): void {
        this._schedulingTargetAddresses = null;
        this._schedulingTargetAddressesPromise = null;
        this._schedulingTargetAddressesSubject.next(this._schedulingTargetAddresses);      // Emit to observable
    }

    public get HasSchedulingTargetAddresses(): Promise<boolean> {
        return this.SchedulingTargetAddresses.then(schedulingTargetAddresses => schedulingTargetAddresses.length > 0);
    }


    /**
     *
     * Gets the ScheduledEvents for this Client.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.client.ScheduledEvents.then(clients => { ... })
     *   or
     *   await this.client.clients
     *
    */
    public get ScheduledEvents(): Promise<ScheduledEventData[]> {
        if (this._scheduledEvents !== null) {
            return Promise.resolve(this._scheduledEvents);
        }

        if (this._scheduledEventsPromise !== null) {
            return this._scheduledEventsPromise;
        }

        // Start the load
        this.loadScheduledEvents();

        return this._scheduledEventsPromise!;
    }



    private loadScheduledEvents(): void {

        this._scheduledEventsPromise = lastValueFrom(
            ClientService.Instance.GetScheduledEventsForClient(this.id)
        )
        .then(ScheduledEvents => {
            this._scheduledEvents = ScheduledEvents ?? [];
            this._scheduledEventsSubject.next(this._scheduledEvents);
            return this._scheduledEvents;
         })
        .catch(err => {
            this._scheduledEvents = [];
            this._scheduledEventsSubject.next(this._scheduledEvents);
            throw err;
        })
        .finally(() => {
            this._scheduledEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEvent. Call after mutations to force refresh.
     */
    public ClearScheduledEventsCache(): void {
        this._scheduledEvents = null;
        this._scheduledEventsPromise = null;
        this._scheduledEventsSubject.next(this._scheduledEvents);      // Emit to observable
    }

    public get HasScheduledEvents(): Promise<boolean> {
        return this.ScheduledEvents.then(scheduledEvents => scheduledEvents.length > 0);
    }


    /**
     *
     * Gets the FinancialTransactions for this Client.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.client.FinancialTransactions.then(clients => { ... })
     *   or
     *   await this.client.clients
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
            ClientService.Instance.GetFinancialTransactionsForClient(this.id)
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
     * Gets the Invoices for this Client.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.client.Invoices.then(clients => { ... })
     *   or
     *   await this.client.clients
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
            ClientService.Instance.GetInvoicesForClient(this.id)
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
     * Gets the Receipts for this Client.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.client.Receipts.then(clients => { ... })
     *   or
     *   await this.client.clients
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
            ClientService.Instance.GetReceiptsForClient(this.id)
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
     * Gets the Constituents for this Client.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.client.Constituents.then(clients => { ... })
     *   or
     *   await this.client.clients
     *
    */
    public get Constituents(): Promise<ConstituentData[]> {
        if (this._constituents !== null) {
            return Promise.resolve(this._constituents);
        }

        if (this._constituentsPromise !== null) {
            return this._constituentsPromise;
        }

        // Start the load
        this.loadConstituents();

        return this._constituentsPromise!;
    }



    private loadConstituents(): void {

        this._constituentsPromise = lastValueFrom(
            ClientService.Instance.GetConstituentsForClient(this.id)
        )
        .then(Constituents => {
            this._constituents = Constituents ?? [];
            this._constituentsSubject.next(this._constituents);
            return this._constituents;
         })
        .catch(err => {
            this._constituents = [];
            this._constituentsSubject.next(this._constituents);
            throw err;
        })
        .finally(() => {
            this._constituentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Constituent. Call after mutations to force refresh.
     */
    public ClearConstituentsCache(): void {
        this._constituents = null;
        this._constituentsPromise = null;
        this._constituentsSubject.next(this._constituents);      // Emit to observable
    }

    public get HasConstituents(): Promise<boolean> {
        return this.Constituents.then(constituents => constituents.length > 0);
    }


    /**
     *
     * Gets the Documents for this Client.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.client.Documents.then(clients => { ... })
     *   or
     *   await this.client.clients
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
            ClientService.Instance.GetDocumentsForClient(this.id)
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
    //   Template: {{ (client.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await client.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ClientData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ClientData>> {
        const info = await lastValueFrom(
            ClientService.Instance.GetClientChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ClientData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ClientData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ClientSubmitData {
        return ClientService.Instance.ConvertToClientSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ClientService extends SecureEndpointBase {

    private static _instance: ClientService;
    private listCache: Map<string, Observable<Array<ClientData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ClientBasicListData>>>;
    private recordCache: Map<string, Observable<ClientData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private clientChangeHistoryService: ClientChangeHistoryService,
        private clientContactService: ClientContactService,
        private schedulingTargetService: SchedulingTargetService,
        private schedulingTargetAddressService: SchedulingTargetAddressService,
        private scheduledEventService: ScheduledEventService,
        private financialTransactionService: FinancialTransactionService,
        private invoiceService: InvoiceService,
        private receiptService: ReceiptService,
        private constituentService: ConstituentService,
        private documentService: DocumentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ClientData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ClientBasicListData>>>();
        this.recordCache = new Map<string, Observable<ClientData>>();

        ClientService._instance = this;
    }

    public static get Instance(): ClientService {
      return ClientService._instance;
    }


    public ClearListCaches(config: ClientQueryParameters | null = null) {

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


    public ConvertToClientSubmitData(data: ClientData): ClientSubmitData {

        let output = new ClientSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.clientTypeId = data.clientTypeId;
        output.currencyId = data.currencyId;
        output.timeZoneId = data.timeZoneId;
        output.calendarId = data.calendarId;
        output.addressLine1 = data.addressLine1;
        output.addressLine2 = data.addressLine2;
        output.city = data.city;
        output.postalCode = data.postalCode;
        output.stateProvinceId = data.stateProvinceId;
        output.countryId = data.countryId;
        output.phone = data.phone;
        output.email = data.email;
        output.latitude = data.latitude;
        output.longitude = data.longitude;
        output.notes = data.notes;
        output.externalId = data.externalId;
        output.color = data.color;
        output.attributes = data.attributes;
        output.avatarFileName = data.avatarFileName;
        output.avatarSize = data.avatarSize;
        output.avatarData = data.avatarData;
        output.avatarMimeType = data.avatarMimeType;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetClient(id: bigint | number, includeRelations: boolean = true) : Observable<ClientData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const client$ = this.requestClient(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Client", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, client$);

            return client$;
        }

        return this.recordCache.get(configHash) as Observable<ClientData>;
    }

    private requestClient(id: bigint | number, includeRelations: boolean = true) : Observable<ClientData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ClientData>(this.baseUrl + 'api/Client/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveClient(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestClient(id, includeRelations));
            }));
    }

    public GetClientList(config: ClientQueryParameters | any = null) : Observable<Array<ClientData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const clientList$ = this.requestClientList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Client list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, clientList$);

            return clientList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ClientData>>;
    }


    private requestClientList(config: ClientQueryParameters | any) : Observable <Array<ClientData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ClientData>>(this.baseUrl + 'api/Clients', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveClientList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestClientList(config));
            }));
    }

    public GetClientsRowCount(config: ClientQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const clientsRowCount$ = this.requestClientsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Clients row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, clientsRowCount$);

            return clientsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestClientsRowCount(config: ClientQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Clients/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestClientsRowCount(config));
            }));
    }

    public GetClientsBasicListData(config: ClientQueryParameters | any = null) : Observable<Array<ClientBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const clientsBasicListData$ = this.requestClientsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Clients basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, clientsBasicListData$);

            return clientsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ClientBasicListData>>;
    }


    private requestClientsBasicListData(config: ClientQueryParameters | any) : Observable<Array<ClientBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ClientBasicListData>>(this.baseUrl + 'api/Clients/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestClientsBasicListData(config));
            }));

    }


    public PutClient(id: bigint | number, client: ClientSubmitData) : Observable<ClientData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ClientData>(this.baseUrl + 'api/Client/' + id.toString(), client, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveClient(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutClient(id, client));
            }));
    }


    public PostClient(client: ClientSubmitData) : Observable<ClientData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ClientData>(this.baseUrl + 'api/Client', client, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveClient(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostClient(client));
            }));
    }

  
    public DeleteClient(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Client/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteClient(id));
            }));
    }

    public RollbackClient(id: bigint | number, versionNumber: bigint | number) : Observable<ClientData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ClientData>(this.baseUrl + 'api/Client/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveClient(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackClient(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Client.
     */
    public GetClientChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ClientData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ClientData>>(this.baseUrl + 'api/Client/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetClientChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Client.
     */
    public GetClientAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ClientData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ClientData>[]>(this.baseUrl + 'api/Client/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetClientAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Client.
     */
    public GetClientVersion(id: bigint | number, version: number): Observable<ClientData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ClientData>(this.baseUrl + 'api/Client/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveClient(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetClientVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Client at a specific point in time.
     */
    public GetClientStateAtTime(id: bigint | number, time: string): Observable<ClientData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ClientData>(this.baseUrl + 'api/Client/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveClient(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetClientStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ClientQueryParameters | any): string {

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

    public userIsSchedulerClientReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerClientReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Clients
        //
        if (userIsSchedulerClientReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerClientReader = user.readPermission >= 1;
            } else {
                userIsSchedulerClientReader = false;
            }
        }

        return userIsSchedulerClientReader;
    }


    public userIsSchedulerClientWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerClientWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Clients
        //
        if (userIsSchedulerClientWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerClientWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerClientWriter = false;
          }      
        }

        return userIsSchedulerClientWriter;
    }

    public GetClientChangeHistoriesForClient(clientId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ClientChangeHistoryData[]> {
        return this.clientChangeHistoryService.GetClientChangeHistoryList({
            clientId: clientId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetClientContactsForClient(clientId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ClientContactData[]> {
        return this.clientContactService.GetClientContactList({
            clientId: clientId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetsForClient(clientId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetData[]> {
        return this.schedulingTargetService.GetSchedulingTargetList({
            clientId: clientId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetAddressesForClient(clientId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetAddressData[]> {
        return this.schedulingTargetAddressService.GetSchedulingTargetAddressList({
            clientId: clientId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventsForClient(clientId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventData[]> {
        return this.scheduledEventService.GetScheduledEventList({
            clientId: clientId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetFinancialTransactionsForClient(clientId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FinancialTransactionData[]> {
        return this.financialTransactionService.GetFinancialTransactionList({
            clientId: clientId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetInvoicesForClient(clientId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<InvoiceData[]> {
        return this.invoiceService.GetInvoiceList({
            clientId: clientId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetReceiptsForClient(clientId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ReceiptData[]> {
        return this.receiptService.GetReceiptList({
            clientId: clientId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConstituentsForClient(clientId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConstituentData[]> {
        return this.constituentService.GetConstituentList({
            clientId: clientId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetDocumentsForClient(clientId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<DocumentData[]> {
        return this.documentService.GetDocumentList({
            clientId: clientId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ClientData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ClientData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ClientTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveClient(raw: any): ClientData {
    if (!raw) return raw;

    //
    // Create a ClientData object instance with correct prototype
    //
    const revived = Object.create(ClientData.prototype) as ClientData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._clientChangeHistories = null;
    (revived as any)._clientChangeHistoriesPromise = null;
    (revived as any)._clientChangeHistoriesSubject = new BehaviorSubject<ClientChangeHistoryData[] | null>(null);

    (revived as any)._clientContacts = null;
    (revived as any)._clientContactsPromise = null;
    (revived as any)._clientContactsSubject = new BehaviorSubject<ClientContactData[] | null>(null);

    (revived as any)._schedulingTargets = null;
    (revived as any)._schedulingTargetsPromise = null;
    (revived as any)._schedulingTargetsSubject = new BehaviorSubject<SchedulingTargetData[] | null>(null);

    (revived as any)._schedulingTargetAddresses = null;
    (revived as any)._schedulingTargetAddressesPromise = null;
    (revived as any)._schedulingTargetAddressesSubject = new BehaviorSubject<SchedulingTargetAddressData[] | null>(null);

    (revived as any)._scheduledEvents = null;
    (revived as any)._scheduledEventsPromise = null;
    (revived as any)._scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

    (revived as any)._financialTransactions = null;
    (revived as any)._financialTransactionsPromise = null;
    (revived as any)._financialTransactionsSubject = new BehaviorSubject<FinancialTransactionData[] | null>(null);

    (revived as any)._invoices = null;
    (revived as any)._invoicesPromise = null;
    (revived as any)._invoicesSubject = new BehaviorSubject<InvoiceData[] | null>(null);

    (revived as any)._receipts = null;
    (revived as any)._receiptsPromise = null;
    (revived as any)._receiptsSubject = new BehaviorSubject<ReceiptData[] | null>(null);

    (revived as any)._constituents = null;
    (revived as any)._constituentsPromise = null;
    (revived as any)._constituentsSubject = new BehaviorSubject<ConstituentData[] | null>(null);

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
    // 2. But private methods (loadClientXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ClientChangeHistories$ = (revived as any)._clientChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._clientChangeHistories === null && (revived as any)._clientChangeHistoriesPromise === null) {
                (revived as any).loadClientChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._clientChangeHistoriesCount$ = null;


    (revived as any).ClientContacts$ = (revived as any)._clientContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._clientContacts === null && (revived as any)._clientContactsPromise === null) {
                (revived as any).loadClientContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._clientContactsCount$ = null;


    (revived as any).SchedulingTargets$ = (revived as any)._schedulingTargetsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargets === null && (revived as any)._schedulingTargetsPromise === null) {
                (revived as any).loadSchedulingTargets();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._schedulingTargetsCount$ = null;


    (revived as any).SchedulingTargetAddresses$ = (revived as any)._schedulingTargetAddressesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetAddresses === null && (revived as any)._schedulingTargetAddressesPromise === null) {
                (revived as any).loadSchedulingTargetAddresses();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._schedulingTargetAddressesCount$ = null;


    (revived as any).ScheduledEvents$ = (revived as any)._scheduledEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEvents === null && (revived as any)._scheduledEventsPromise === null) {
                (revived as any).loadScheduledEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._scheduledEventsCount$ = null;


    (revived as any).FinancialTransactions$ = (revived as any)._financialTransactionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._financialTransactions === null && (revived as any)._financialTransactionsPromise === null) {
                (revived as any).loadFinancialTransactions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._financialTransactionsCount$ = null;


    (revived as any).Invoices$ = (revived as any)._invoicesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._invoices === null && (revived as any)._invoicesPromise === null) {
                (revived as any).loadInvoices();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._invoicesCount$ = null;


    (revived as any).Receipts$ = (revived as any)._receiptsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._receipts === null && (revived as any)._receiptsPromise === null) {
                (revived as any).loadReceipts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._receiptsCount$ = null;


    (revived as any).Constituents$ = (revived as any)._constituentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._constituents === null && (revived as any)._constituentsPromise === null) {
                (revived as any).loadConstituents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._constituentsCount$ = null;


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
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ClientData> | null>(null);

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

  private ReviveClientList(rawList: any[]): ClientData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveClient(raw));
  }

}
