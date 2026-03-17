/*

   GENERATED SERVICE FOR THE GENERALLEDGERENTRY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the GeneralLedgerEntry table.

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
import { FinancialTransactionData } from './financial-transaction.service';
import { FiscalPeriodData } from './fiscal-period.service';
import { FinancialOfficeData } from './financial-office.service';
import { GeneralLedgerLineService, GeneralLedgerLineData } from './general-ledger-line.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class GeneralLedgerEntryQueryParameters {
    journalEntryNumber: bigint | number | null | undefined = null;
    transactionDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    description: string | null | undefined = null;
    referenceNumber: string | null | undefined = null;
    financialTransactionId: bigint | number | null | undefined = null;
    fiscalPeriodId: bigint | number | null | undefined = null;
    financialOfficeId: bigint | number | null | undefined = null;
    postedBy: bigint | number | null | undefined = null;
    postedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    reversalOfId: bigint | number | null | undefined = null;
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
export class GeneralLedgerEntrySubmitData {
    id!: bigint | number;
    journalEntryNumber!: bigint | number;
    transactionDate!: string;      // ISO 8601 (full datetime)
    description: string | null = null;
    referenceNumber: string | null = null;
    financialTransactionId: bigint | number | null = null;
    fiscalPeriodId: bigint | number | null = null;
    financialOfficeId: bigint | number | null = null;
    postedBy!: bigint | number;
    postedDate!: string;      // ISO 8601 (full datetime)
    reversalOfId: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class GeneralLedgerEntryBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. GeneralLedgerEntryChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `generalLedgerEntry.GeneralLedgerEntryChildren$` — use with `| async` in templates
//        • Promise:    `generalLedgerEntry.GeneralLedgerEntryChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="generalLedgerEntry.GeneralLedgerEntryChildren$ | async"`), or
//        • Access the promise getter (`generalLedgerEntry.GeneralLedgerEntryChildren` or `await generalLedgerEntry.GeneralLedgerEntryChildren`)
//    - Simply reading `generalLedgerEntry.GeneralLedgerEntryChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await generalLedgerEntry.Reload()` to refresh the entire object and clear all lazy caches.
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
export class GeneralLedgerEntryData {
    id!: bigint | number;
    journalEntryNumber!: bigint | number;
    transactionDate!: string;      // ISO 8601 (full datetime)
    description!: string | null;
    referenceNumber!: string | null;
    financialTransactionId!: bigint | number;
    fiscalPeriodId!: bigint | number;
    financialOfficeId!: bigint | number;
    postedBy!: bigint | number;
    postedDate!: string;      // ISO 8601 (full datetime)
    reversalOfId!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    financialOffice: FinancialOfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    financialTransaction: FinancialTransactionData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    fiscalPeriod: FiscalPeriodData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _generalLedgerLines: GeneralLedgerLineData[] | null = null;
    private _generalLedgerLinesPromise: Promise<GeneralLedgerLineData[]> | null  = null;
    private _generalLedgerLinesSubject = new BehaviorSubject<GeneralLedgerLineData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
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
            this._generalLedgerLinesCount$ = GeneralLedgerLineService.Instance.GetGeneralLedgerLinesRowCount({generalLedgerEntryId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._generalLedgerLinesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any GeneralLedgerEntryData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.generalLedgerEntry.Reload();
  //
  //  Non Async:
  //
  //     generalLedgerEntry[0].Reload().then(x => {
  //        this.generalLedgerEntry = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      GeneralLedgerEntryService.Instance.GetGeneralLedgerEntry(this.id, includeRelations)
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
     this._generalLedgerLines = null;
     this._generalLedgerLinesPromise = null;
     this._generalLedgerLinesSubject.next(null);
     this._generalLedgerLinesCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the GeneralLedgerLines for this GeneralLedgerEntry.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.generalLedgerEntry.GeneralLedgerLines.then(generalLedgerEntries => { ... })
     *   or
     *   await this.generalLedgerEntry.generalLedgerEntries
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
            GeneralLedgerEntryService.Instance.GetGeneralLedgerLinesForGeneralLedgerEntry(this.id)
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
     * Updates the state of this GeneralLedgerEntryData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this GeneralLedgerEntryData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): GeneralLedgerEntrySubmitData {
        return GeneralLedgerEntryService.Instance.ConvertToGeneralLedgerEntrySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class GeneralLedgerEntryService extends SecureEndpointBase {

    private static _instance: GeneralLedgerEntryService;
    private listCache: Map<string, Observable<Array<GeneralLedgerEntryData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<GeneralLedgerEntryBasicListData>>>;
    private recordCache: Map<string, Observable<GeneralLedgerEntryData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private generalLedgerLineService: GeneralLedgerLineService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<GeneralLedgerEntryData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<GeneralLedgerEntryBasicListData>>>();
        this.recordCache = new Map<string, Observable<GeneralLedgerEntryData>>();

        GeneralLedgerEntryService._instance = this;
    }

    public static get Instance(): GeneralLedgerEntryService {
      return GeneralLedgerEntryService._instance;
    }


    public ClearListCaches(config: GeneralLedgerEntryQueryParameters | null = null) {

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


    public ConvertToGeneralLedgerEntrySubmitData(data: GeneralLedgerEntryData): GeneralLedgerEntrySubmitData {

        let output = new GeneralLedgerEntrySubmitData();

        output.id = data.id;
        output.journalEntryNumber = data.journalEntryNumber;
        output.transactionDate = data.transactionDate;
        output.description = data.description;
        output.referenceNumber = data.referenceNumber;
        output.financialTransactionId = data.financialTransactionId;
        output.fiscalPeriodId = data.fiscalPeriodId;
        output.financialOfficeId = data.financialOfficeId;
        output.postedBy = data.postedBy;
        output.postedDate = data.postedDate;
        output.reversalOfId = data.reversalOfId;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetGeneralLedgerEntry(id: bigint | number, includeRelations: boolean = true) : Observable<GeneralLedgerEntryData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const generalLedgerEntry$ = this.requestGeneralLedgerEntry(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get GeneralLedgerEntry", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, generalLedgerEntry$);

            return generalLedgerEntry$;
        }

        return this.recordCache.get(configHash) as Observable<GeneralLedgerEntryData>;
    }

    private requestGeneralLedgerEntry(id: bigint | number, includeRelations: boolean = true) : Observable<GeneralLedgerEntryData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<GeneralLedgerEntryData>(this.baseUrl + 'api/GeneralLedgerEntry/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveGeneralLedgerEntry(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestGeneralLedgerEntry(id, includeRelations));
            }));
    }

    public GetGeneralLedgerEntryList(config: GeneralLedgerEntryQueryParameters | any = null) : Observable<Array<GeneralLedgerEntryData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const generalLedgerEntryList$ = this.requestGeneralLedgerEntryList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get GeneralLedgerEntry list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, generalLedgerEntryList$);

            return generalLedgerEntryList$;
        }

        return this.listCache.get(configHash) as Observable<Array<GeneralLedgerEntryData>>;
    }


    private requestGeneralLedgerEntryList(config: GeneralLedgerEntryQueryParameters | any) : Observable <Array<GeneralLedgerEntryData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<GeneralLedgerEntryData>>(this.baseUrl + 'api/GeneralLedgerEntries', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveGeneralLedgerEntryList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestGeneralLedgerEntryList(config));
            }));
    }

    public GetGeneralLedgerEntriesRowCount(config: GeneralLedgerEntryQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const generalLedgerEntriesRowCount$ = this.requestGeneralLedgerEntriesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get GeneralLedgerEntries row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, generalLedgerEntriesRowCount$);

            return generalLedgerEntriesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestGeneralLedgerEntriesRowCount(config: GeneralLedgerEntryQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/GeneralLedgerEntries/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestGeneralLedgerEntriesRowCount(config));
            }));
    }

    public GetGeneralLedgerEntriesBasicListData(config: GeneralLedgerEntryQueryParameters | any = null) : Observable<Array<GeneralLedgerEntryBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const generalLedgerEntriesBasicListData$ = this.requestGeneralLedgerEntriesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get GeneralLedgerEntries basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, generalLedgerEntriesBasicListData$);

            return generalLedgerEntriesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<GeneralLedgerEntryBasicListData>>;
    }


    private requestGeneralLedgerEntriesBasicListData(config: GeneralLedgerEntryQueryParameters | any) : Observable<Array<GeneralLedgerEntryBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<GeneralLedgerEntryBasicListData>>(this.baseUrl + 'api/GeneralLedgerEntries/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestGeneralLedgerEntriesBasicListData(config));
            }));

    }


    public PutGeneralLedgerEntry(id: bigint | number, generalLedgerEntry: GeneralLedgerEntrySubmitData) : Observable<GeneralLedgerEntryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<GeneralLedgerEntryData>(this.baseUrl + 'api/GeneralLedgerEntry/' + id.toString(), generalLedgerEntry, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveGeneralLedgerEntry(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutGeneralLedgerEntry(id, generalLedgerEntry));
            }));
    }


    public PostGeneralLedgerEntry(generalLedgerEntry: GeneralLedgerEntrySubmitData) : Observable<GeneralLedgerEntryData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<GeneralLedgerEntryData>(this.baseUrl + 'api/GeneralLedgerEntry', generalLedgerEntry, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveGeneralLedgerEntry(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostGeneralLedgerEntry(generalLedgerEntry));
            }));
    }

  
    public DeleteGeneralLedgerEntry(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/GeneralLedgerEntry/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteGeneralLedgerEntry(id));
            }));
    }


    private getConfigHash(config: GeneralLedgerEntryQueryParameters | any): string {

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

    public userIsSchedulerGeneralLedgerEntryReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerGeneralLedgerEntryReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.GeneralLedgerEntries
        //
        if (userIsSchedulerGeneralLedgerEntryReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerGeneralLedgerEntryReader = user.readPermission >= 1;
            } else {
                userIsSchedulerGeneralLedgerEntryReader = false;
            }
        }

        return userIsSchedulerGeneralLedgerEntryReader;
    }


    public userIsSchedulerGeneralLedgerEntryWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerGeneralLedgerEntryWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.GeneralLedgerEntries
        //
        if (userIsSchedulerGeneralLedgerEntryWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerGeneralLedgerEntryWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerGeneralLedgerEntryWriter = false;
          }      
        }

        return userIsSchedulerGeneralLedgerEntryWriter;
    }

    public GetGeneralLedgerLinesForGeneralLedgerEntry(generalLedgerEntryId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GeneralLedgerLineData[]> {
        return this.generalLedgerLineService.GetGeneralLedgerLineList({
            generalLedgerEntryId: generalLedgerEntryId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full GeneralLedgerEntryData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the GeneralLedgerEntryData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when GeneralLedgerEntryTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveGeneralLedgerEntry(raw: any): GeneralLedgerEntryData {
    if (!raw) return raw;

    //
    // Create a GeneralLedgerEntryData object instance with correct prototype
    //
    const revived = Object.create(GeneralLedgerEntryData.prototype) as GeneralLedgerEntryData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._generalLedgerLines = null;
    (revived as any)._generalLedgerLinesPromise = null;
    (revived as any)._generalLedgerLinesSubject = new BehaviorSubject<GeneralLedgerLineData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadGeneralLedgerEntryXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).GeneralLedgerLines$ = (revived as any)._generalLedgerLinesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._generalLedgerLines === null && (revived as any)._generalLedgerLinesPromise === null) {
                (revived as any).loadGeneralLedgerLines();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._generalLedgerLinesCount$ = null;



    return revived;
  }

  private ReviveGeneralLedgerEntryList(rawList: any[]): GeneralLedgerEntryData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveGeneralLedgerEntry(raw));
  }

}
