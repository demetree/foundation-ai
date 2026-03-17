/*

   GENERATED SERVICE FOR THE GENERALLEDGERLINE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the GeneralLedgerLine table.

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
import { GeneralLedgerEntryData } from './general-ledger-entry.service';
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
export class GeneralLedgerLineQueryParameters {
    generalLedgerEntryId: bigint | number | null | undefined = null;
    financialCategoryId: bigint | number | null | undefined = null;
    debitAmount: number | null | undefined = null;
    creditAmount: number | null | undefined = null;
    description: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class GeneralLedgerLineSubmitData {
    id!: bigint | number;
    generalLedgerEntryId!: bigint | number;
    financialCategoryId!: bigint | number;
    debitAmount!: number;
    creditAmount!: number;
    description: string | null = null;
}


export class GeneralLedgerLineBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. GeneralLedgerLineChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `generalLedgerLine.GeneralLedgerLineChildren$` — use with `| async` in templates
//        • Promise:    `generalLedgerLine.GeneralLedgerLineChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="generalLedgerLine.GeneralLedgerLineChildren$ | async"`), or
//        • Access the promise getter (`generalLedgerLine.GeneralLedgerLineChildren` or `await generalLedgerLine.GeneralLedgerLineChildren`)
//    - Simply reading `generalLedgerLine.GeneralLedgerLineChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await generalLedgerLine.Reload()` to refresh the entire object and clear all lazy caches.
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
export class GeneralLedgerLineData {
    id!: bigint | number;
    generalLedgerEntryId!: bigint | number;
    financialCategoryId!: bigint | number;
    debitAmount!: number;
    creditAmount!: number;
    description!: string | null;
    financialCategory: FinancialCategoryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    generalLedgerEntry: GeneralLedgerEntryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any GeneralLedgerLineData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.generalLedgerLine.Reload();
  //
  //  Non Async:
  //
  //     generalLedgerLine[0].Reload().then(x => {
  //        this.generalLedgerLine = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      GeneralLedgerLineService.Instance.GetGeneralLedgerLine(this.id, includeRelations)
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
     * Updates the state of this GeneralLedgerLineData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this GeneralLedgerLineData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): GeneralLedgerLineSubmitData {
        return GeneralLedgerLineService.Instance.ConvertToGeneralLedgerLineSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class GeneralLedgerLineService extends SecureEndpointBase {

    private static _instance: GeneralLedgerLineService;
    private listCache: Map<string, Observable<Array<GeneralLedgerLineData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<GeneralLedgerLineBasicListData>>>;
    private recordCache: Map<string, Observable<GeneralLedgerLineData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<GeneralLedgerLineData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<GeneralLedgerLineBasicListData>>>();
        this.recordCache = new Map<string, Observable<GeneralLedgerLineData>>();

        GeneralLedgerLineService._instance = this;
    }

    public static get Instance(): GeneralLedgerLineService {
      return GeneralLedgerLineService._instance;
    }


    public ClearListCaches(config: GeneralLedgerLineQueryParameters | null = null) {

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


    public ConvertToGeneralLedgerLineSubmitData(data: GeneralLedgerLineData): GeneralLedgerLineSubmitData {

        let output = new GeneralLedgerLineSubmitData();

        output.id = data.id;
        output.generalLedgerEntryId = data.generalLedgerEntryId;
        output.financialCategoryId = data.financialCategoryId;
        output.debitAmount = data.debitAmount;
        output.creditAmount = data.creditAmount;
        output.description = data.description;

        return output;
    }

    public GetGeneralLedgerLine(id: bigint | number, includeRelations: boolean = true) : Observable<GeneralLedgerLineData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const generalLedgerLine$ = this.requestGeneralLedgerLine(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get GeneralLedgerLine", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, generalLedgerLine$);

            return generalLedgerLine$;
        }

        return this.recordCache.get(configHash) as Observable<GeneralLedgerLineData>;
    }

    private requestGeneralLedgerLine(id: bigint | number, includeRelations: boolean = true) : Observable<GeneralLedgerLineData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<GeneralLedgerLineData>(this.baseUrl + 'api/GeneralLedgerLine/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveGeneralLedgerLine(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestGeneralLedgerLine(id, includeRelations));
            }));
    }

    public GetGeneralLedgerLineList(config: GeneralLedgerLineQueryParameters | any = null) : Observable<Array<GeneralLedgerLineData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const generalLedgerLineList$ = this.requestGeneralLedgerLineList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get GeneralLedgerLine list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, generalLedgerLineList$);

            return generalLedgerLineList$;
        }

        return this.listCache.get(configHash) as Observable<Array<GeneralLedgerLineData>>;
    }


    private requestGeneralLedgerLineList(config: GeneralLedgerLineQueryParameters | any) : Observable <Array<GeneralLedgerLineData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<GeneralLedgerLineData>>(this.baseUrl + 'api/GeneralLedgerLines', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveGeneralLedgerLineList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestGeneralLedgerLineList(config));
            }));
    }

    public GetGeneralLedgerLinesRowCount(config: GeneralLedgerLineQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const generalLedgerLinesRowCount$ = this.requestGeneralLedgerLinesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get GeneralLedgerLines row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, generalLedgerLinesRowCount$);

            return generalLedgerLinesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestGeneralLedgerLinesRowCount(config: GeneralLedgerLineQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/GeneralLedgerLines/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestGeneralLedgerLinesRowCount(config));
            }));
    }

    public GetGeneralLedgerLinesBasicListData(config: GeneralLedgerLineQueryParameters | any = null) : Observable<Array<GeneralLedgerLineBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const generalLedgerLinesBasicListData$ = this.requestGeneralLedgerLinesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get GeneralLedgerLines basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, generalLedgerLinesBasicListData$);

            return generalLedgerLinesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<GeneralLedgerLineBasicListData>>;
    }


    private requestGeneralLedgerLinesBasicListData(config: GeneralLedgerLineQueryParameters | any) : Observable<Array<GeneralLedgerLineBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<GeneralLedgerLineBasicListData>>(this.baseUrl + 'api/GeneralLedgerLines/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestGeneralLedgerLinesBasicListData(config));
            }));

    }


    public PutGeneralLedgerLine(id: bigint | number, generalLedgerLine: GeneralLedgerLineSubmitData) : Observable<GeneralLedgerLineData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<GeneralLedgerLineData>(this.baseUrl + 'api/GeneralLedgerLine/' + id.toString(), generalLedgerLine, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveGeneralLedgerLine(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutGeneralLedgerLine(id, generalLedgerLine));
            }));
    }


    public PostGeneralLedgerLine(generalLedgerLine: GeneralLedgerLineSubmitData) : Observable<GeneralLedgerLineData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<GeneralLedgerLineData>(this.baseUrl + 'api/GeneralLedgerLine', generalLedgerLine, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveGeneralLedgerLine(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostGeneralLedgerLine(generalLedgerLine));
            }));
    }

  
    public DeleteGeneralLedgerLine(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/GeneralLedgerLine/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteGeneralLedgerLine(id));
            }));
    }


    private getConfigHash(config: GeneralLedgerLineQueryParameters | any): string {

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

    public userIsSchedulerGeneralLedgerLineReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerGeneralLedgerLineReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.GeneralLedgerLines
        //
        if (userIsSchedulerGeneralLedgerLineReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerGeneralLedgerLineReader = user.readPermission >= 1;
            } else {
                userIsSchedulerGeneralLedgerLineReader = false;
            }
        }

        return userIsSchedulerGeneralLedgerLineReader;
    }


    public userIsSchedulerGeneralLedgerLineWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerGeneralLedgerLineWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.GeneralLedgerLines
        //
        if (userIsSchedulerGeneralLedgerLineWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerGeneralLedgerLineWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerGeneralLedgerLineWriter = false;
          }      
        }

        return userIsSchedulerGeneralLedgerLineWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full GeneralLedgerLineData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the GeneralLedgerLineData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when GeneralLedgerLineTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveGeneralLedgerLine(raw: any): GeneralLedgerLineData {
    if (!raw) return raw;

    //
    // Create a GeneralLedgerLineData object instance with correct prototype
    //
    const revived = Object.create(GeneralLedgerLineData.prototype) as GeneralLedgerLineData;

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
    // 2. But private methods (loadGeneralLedgerLineXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveGeneralLedgerLineList(rawList: any[]): GeneralLedgerLineData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveGeneralLedgerLine(raw));
  }

}
