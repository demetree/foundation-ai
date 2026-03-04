/*

   GENERATED SERVICE FOR THE EVENTCHARGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the EventCharge table.

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
import { ScheduledEventData } from './scheduled-event.service';
import { ResourceData } from './resource.service';
import { ChargeTypeData } from './charge-type.service';
import { ChargeStatusData } from './charge-status.service';
import { CurrencyData } from './currency.service';
import { RateTypeData } from './rate-type.service';
import { EventChargeChangeHistoryService, EventChargeChangeHistoryData } from './event-charge-change-history.service';
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
export class EventChargeQueryParameters {
    scheduledEventId: bigint | number | null | undefined = null;
    resourceId: bigint | number | null | undefined = null;
    chargeTypeId: bigint | number | null | undefined = null;
    chargeStatusId: bigint | number | null | undefined = null;
    quantity: number | null | undefined = null;
    unitPrice: number | null | undefined = null;
    extendedAmount: number | null | undefined = null;
    taxAmount: number | null | undefined = null;
    currencyId: bigint | number | null | undefined = null;
    rateTypeId: bigint | number | null | undefined = null;
    notes: string | null | undefined = null;
    isAutomatic: boolean | null | undefined = null;
    isDeposit: boolean | null | undefined = null;
    depositRefundedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    exportedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    externalId: string | null | undefined = null;
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
export class EventChargeSubmitData {
    id!: bigint | number;
    scheduledEventId!: bigint | number;
    resourceId: bigint | number | null = null;
    chargeTypeId!: bigint | number;
    chargeStatusId!: bigint | number;
    quantity: number | null = null;
    unitPrice: number | null = null;
    extendedAmount!: number;
    taxAmount!: number;
    currencyId!: bigint | number;
    rateTypeId: bigint | number | null = null;
    notes: string | null = null;
    isAutomatic!: boolean;
    isDeposit!: boolean;
    depositRefundedDate: string | null = null;     // ISO 8601 (full datetime)
    exportedDate: string | null = null;     // ISO 8601 (full datetime)
    externalId: string | null = null;
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

export class EventChargeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. EventChargeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `eventCharge.EventChargeChildren$` — use with `| async` in templates
//        • Promise:    `eventCharge.EventChargeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="eventCharge.EventChargeChildren$ | async"`), or
//        • Access the promise getter (`eventCharge.EventChargeChildren` or `await eventCharge.EventChargeChildren`)
//    - Simply reading `eventCharge.EventChargeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await eventCharge.Reload()` to refresh the entire object and clear all lazy caches.
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
export class EventChargeData {
    id!: bigint | number;
    scheduledEventId!: bigint | number;
    resourceId!: bigint | number;
    chargeTypeId!: bigint | number;
    chargeStatusId!: bigint | number;
    quantity!: number | null;
    unitPrice!: number | null;
    extendedAmount!: number;
    taxAmount!: number;
    currencyId!: bigint | number;
    rateTypeId!: bigint | number;
    notes!: string | null;
    isAutomatic!: boolean;
    isDeposit!: boolean;
    depositRefundedDate!: string | null;   // ISO 8601 (full datetime)
    exportedDate!: string | null;   // ISO 8601 (full datetime)
    externalId!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    chargeStatus: ChargeStatusData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    chargeType: ChargeTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    currency: CurrencyData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    rateType: RateTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduledEvent: ScheduledEventData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _eventChargeChangeHistories: EventChargeChangeHistoryData[] | null = null;
    private _eventChargeChangeHistoriesPromise: Promise<EventChargeChangeHistoryData[]> | null  = null;
    private _eventChargeChangeHistoriesSubject = new BehaviorSubject<EventChargeChangeHistoryData[] | null>(null);

                
    private _paymentTransactions: PaymentTransactionData[] | null = null;
    private _paymentTransactionsPromise: Promise<PaymentTransactionData[]> | null  = null;
    private _paymentTransactionsSubject = new BehaviorSubject<PaymentTransactionData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<EventChargeData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<EventChargeData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<EventChargeData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public EventChargeChangeHistories$ = this._eventChargeChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventChargeChangeHistories === null && this._eventChargeChangeHistoriesPromise === null) {
            this.loadEventChargeChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _eventChargeChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get EventChargeChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._eventChargeChangeHistoriesCount$ === null) {
            this._eventChargeChangeHistoriesCount$ = EventChargeChangeHistoryService.Instance.GetEventChargeChangeHistoriesRowCount({eventChargeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._eventChargeChangeHistoriesCount$;
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
            this._paymentTransactionsCount$ = PaymentTransactionService.Instance.GetPaymentTransactionsRowCount({eventChargeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._paymentTransactionsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any EventChargeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.eventCharge.Reload();
  //
  //  Non Async:
  //
  //     eventCharge[0].Reload().then(x => {
  //        this.eventCharge = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      EventChargeService.Instance.GetEventCharge(this.id, includeRelations)
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
     this._eventChargeChangeHistories = null;
     this._eventChargeChangeHistoriesPromise = null;
     this._eventChargeChangeHistoriesSubject.next(null);
     this._eventChargeChangeHistoriesCount$ = null;

     this._paymentTransactions = null;
     this._paymentTransactionsPromise = null;
     this._paymentTransactionsSubject.next(null);
     this._paymentTransactionsCount$ = null;

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
     * Gets the EventChargeChangeHistories for this EventCharge.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.eventCharge.EventChargeChangeHistories.then(eventCharges => { ... })
     *   or
     *   await this.eventCharge.eventCharges
     *
    */
    public get EventChargeChangeHistories(): Promise<EventChargeChangeHistoryData[]> {
        if (this._eventChargeChangeHistories !== null) {
            return Promise.resolve(this._eventChargeChangeHistories);
        }

        if (this._eventChargeChangeHistoriesPromise !== null) {
            return this._eventChargeChangeHistoriesPromise;
        }

        // Start the load
        this.loadEventChargeChangeHistories();

        return this._eventChargeChangeHistoriesPromise!;
    }



    private loadEventChargeChangeHistories(): void {

        this._eventChargeChangeHistoriesPromise = lastValueFrom(
            EventChargeService.Instance.GetEventChargeChangeHistoriesForEventCharge(this.id)
        )
        .then(EventChargeChangeHistories => {
            this._eventChargeChangeHistories = EventChargeChangeHistories ?? [];
            this._eventChargeChangeHistoriesSubject.next(this._eventChargeChangeHistories);
            return this._eventChargeChangeHistories;
         })
        .catch(err => {
            this._eventChargeChangeHistories = [];
            this._eventChargeChangeHistoriesSubject.next(this._eventChargeChangeHistories);
            throw err;
        })
        .finally(() => {
            this._eventChargeChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventChargeChangeHistory. Call after mutations to force refresh.
     */
    public ClearEventChargeChangeHistoriesCache(): void {
        this._eventChargeChangeHistories = null;
        this._eventChargeChangeHistoriesPromise = null;
        this._eventChargeChangeHistoriesSubject.next(this._eventChargeChangeHistories);      // Emit to observable
    }

    public get HasEventChargeChangeHistories(): Promise<boolean> {
        return this.EventChargeChangeHistories.then(eventChargeChangeHistories => eventChargeChangeHistories.length > 0);
    }


    /**
     *
     * Gets the PaymentTransactions for this EventCharge.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.eventCharge.PaymentTransactions.then(eventCharges => { ... })
     *   or
     *   await this.eventCharge.eventCharges
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
            EventChargeService.Instance.GetPaymentTransactionsForEventCharge(this.id)
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




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (eventCharge.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await eventCharge.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<EventChargeData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<EventChargeData>> {
        const info = await lastValueFrom(
            EventChargeService.Instance.GetEventChargeChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this EventChargeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this EventChargeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): EventChargeSubmitData {
        return EventChargeService.Instance.ConvertToEventChargeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class EventChargeService extends SecureEndpointBase {

    private static _instance: EventChargeService;
    private listCache: Map<string, Observable<Array<EventChargeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<EventChargeBasicListData>>>;
    private recordCache: Map<string, Observable<EventChargeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private eventChargeChangeHistoryService: EventChargeChangeHistoryService,
        private paymentTransactionService: PaymentTransactionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<EventChargeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<EventChargeBasicListData>>>();
        this.recordCache = new Map<string, Observable<EventChargeData>>();

        EventChargeService._instance = this;
    }

    public static get Instance(): EventChargeService {
      return EventChargeService._instance;
    }


    public ClearListCaches(config: EventChargeQueryParameters | null = null) {

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


    public ConvertToEventChargeSubmitData(data: EventChargeData): EventChargeSubmitData {

        let output = new EventChargeSubmitData();

        output.id = data.id;
        output.scheduledEventId = data.scheduledEventId;
        output.resourceId = data.resourceId;
        output.chargeTypeId = data.chargeTypeId;
        output.chargeStatusId = data.chargeStatusId;
        output.quantity = data.quantity;
        output.unitPrice = data.unitPrice;
        output.extendedAmount = data.extendedAmount;
        output.taxAmount = data.taxAmount;
        output.currencyId = data.currencyId;
        output.rateTypeId = data.rateTypeId;
        output.notes = data.notes;
        output.isAutomatic = data.isAutomatic;
        output.isDeposit = data.isDeposit;
        output.depositRefundedDate = data.depositRefundedDate;
        output.exportedDate = data.exportedDate;
        output.externalId = data.externalId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetEventCharge(id: bigint | number, includeRelations: boolean = true) : Observable<EventChargeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const eventCharge$ = this.requestEventCharge(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EventCharge", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, eventCharge$);

            return eventCharge$;
        }

        return this.recordCache.get(configHash) as Observable<EventChargeData>;
    }

    private requestEventCharge(id: bigint | number, includeRelations: boolean = true) : Observable<EventChargeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventChargeData>(this.baseUrl + 'api/EventCharge/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveEventCharge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestEventCharge(id, includeRelations));
            }));
    }

    public GetEventChargeList(config: EventChargeQueryParameters | any = null) : Observable<Array<EventChargeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const eventChargeList$ = this.requestEventChargeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EventCharge list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, eventChargeList$);

            return eventChargeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<EventChargeData>>;
    }


    private requestEventChargeList(config: EventChargeQueryParameters | any) : Observable <Array<EventChargeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EventChargeData>>(this.baseUrl + 'api/EventCharges', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveEventChargeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestEventChargeList(config));
            }));
    }

    public GetEventChargesRowCount(config: EventChargeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const eventChargesRowCount$ = this.requestEventChargesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EventCharges row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, eventChargesRowCount$);

            return eventChargesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestEventChargesRowCount(config: EventChargeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/EventCharges/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEventChargesRowCount(config));
            }));
    }

    public GetEventChargesBasicListData(config: EventChargeQueryParameters | any = null) : Observable<Array<EventChargeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const eventChargesBasicListData$ = this.requestEventChargesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EventCharges basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, eventChargesBasicListData$);

            return eventChargesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<EventChargeBasicListData>>;
    }


    private requestEventChargesBasicListData(config: EventChargeQueryParameters | any) : Observable<Array<EventChargeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EventChargeBasicListData>>(this.baseUrl + 'api/EventCharges/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEventChargesBasicListData(config));
            }));

    }


    public PutEventCharge(id: bigint | number, eventCharge: EventChargeSubmitData) : Observable<EventChargeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EventChargeData>(this.baseUrl + 'api/EventCharge/' + id.toString(), eventCharge, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventCharge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutEventCharge(id, eventCharge));
            }));
    }


    public PostEventCharge(eventCharge: EventChargeSubmitData) : Observable<EventChargeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<EventChargeData>(this.baseUrl + 'api/EventCharge', eventCharge, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventCharge(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostEventCharge(eventCharge));
            }));
    }

  
    public DeleteEventCharge(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/EventCharge/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteEventCharge(id));
            }));
    }

    public RollbackEventCharge(id: bigint | number, versionNumber: bigint | number) : Observable<EventChargeData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EventChargeData>(this.baseUrl + 'api/EventCharge/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEventCharge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackEventCharge(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a EventCharge.
     */
    public GetEventChargeChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<EventChargeData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<EventChargeData>>(this.baseUrl + 'api/EventCharge/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetEventChargeChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a EventCharge.
     */
    public GetEventChargeAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<EventChargeData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<EventChargeData>[]>(this.baseUrl + 'api/EventCharge/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetEventChargeAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a EventCharge.
     */
    public GetEventChargeVersion(id: bigint | number, version: number): Observable<EventChargeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventChargeData>(this.baseUrl + 'api/EventCharge/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveEventCharge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetEventChargeVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a EventCharge at a specific point in time.
     */
    public GetEventChargeStateAtTime(id: bigint | number, time: string): Observable<EventChargeData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EventChargeData>(this.baseUrl + 'api/EventCharge/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveEventCharge(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetEventChargeStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: EventChargeQueryParameters | any): string {

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

    public userIsSchedulerEventChargeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerEventChargeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.EventCharges
        //
        if (userIsSchedulerEventChargeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerEventChargeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerEventChargeReader = false;
            }
        }

        return userIsSchedulerEventChargeReader;
    }


    public userIsSchedulerEventChargeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerEventChargeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.EventCharges
        //
        if (userIsSchedulerEventChargeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerEventChargeWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerEventChargeWriter = false;
          }      
        }

        return userIsSchedulerEventChargeWriter;
    }

    public GetEventChargeChangeHistoriesForEventCharge(eventChargeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventChargeChangeHistoryData[]> {
        return this.eventChargeChangeHistoryService.GetEventChargeChangeHistoryList({
            eventChargeId: eventChargeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPaymentTransactionsForEventCharge(eventChargeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PaymentTransactionData[]> {
        return this.paymentTransactionService.GetPaymentTransactionList({
            eventChargeId: eventChargeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full EventChargeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the EventChargeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when EventChargeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveEventCharge(raw: any): EventChargeData {
    if (!raw) return raw;

    //
    // Create a EventChargeData object instance with correct prototype
    //
    const revived = Object.create(EventChargeData.prototype) as EventChargeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._eventChargeChangeHistories = null;
    (revived as any)._eventChargeChangeHistoriesPromise = null;
    (revived as any)._eventChargeChangeHistoriesSubject = new BehaviorSubject<EventChargeChangeHistoryData[] | null>(null);

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
    // 2. But private methods (loadEventChargeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).EventChargeChangeHistories$ = (revived as any)._eventChargeChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventChargeChangeHistories === null && (revived as any)._eventChargeChangeHistoriesPromise === null) {
                (revived as any).loadEventChargeChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._eventChargeChangeHistoriesCount$ = null;


    (revived as any).PaymentTransactions$ = (revived as any)._paymentTransactionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._paymentTransactions === null && (revived as any)._paymentTransactionsPromise === null) {
                (revived as any).loadPaymentTransactions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._paymentTransactionsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<EventChargeData> | null>(null);

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

  private ReviveEventChargeList(rawList: any[]): EventChargeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveEventCharge(raw));
  }

}
