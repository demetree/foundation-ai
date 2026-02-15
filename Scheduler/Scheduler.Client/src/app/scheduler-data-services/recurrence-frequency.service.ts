/*

   GENERATED SERVICE FOR THE RECURRENCEFREQUENCY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the RecurrenceFrequency table.

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
import { RecurrenceRuleService, RecurrenceRuleData } from './recurrence-rule.service';
import { PledgeService, PledgeData } from './pledge.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class RecurrenceFrequencyQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class RecurrenceFrequencySubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class RecurrenceFrequencyBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. RecurrenceFrequencyChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `recurrenceFrequency.RecurrenceFrequencyChildren$` — use with `| async` in templates
//        • Promise:    `recurrenceFrequency.RecurrenceFrequencyChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="recurrenceFrequency.RecurrenceFrequencyChildren$ | async"`), or
//        • Access the promise getter (`recurrenceFrequency.RecurrenceFrequencyChildren` or `await recurrenceFrequency.RecurrenceFrequencyChildren`)
//    - Simply reading `recurrenceFrequency.RecurrenceFrequencyChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await recurrenceFrequency.Reload()` to refresh the entire object and clear all lazy caches.
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
export class RecurrenceFrequencyData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _recurrenceRules: RecurrenceRuleData[] | null = null;
    private _recurrenceRulesPromise: Promise<RecurrenceRuleData[]> | null  = null;
    private _recurrenceRulesSubject = new BehaviorSubject<RecurrenceRuleData[] | null>(null);

                
    private _pledges: PledgeData[] | null = null;
    private _pledgesPromise: Promise<PledgeData[]> | null  = null;
    private _pledgesSubject = new BehaviorSubject<PledgeData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public RecurrenceRules$ = this._recurrenceRulesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._recurrenceRules === null && this._recurrenceRulesPromise === null) {
            this.loadRecurrenceRules(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public RecurrenceRulesCount$ = RecurrenceRuleService.Instance.GetRecurrenceRulesRowCount({recurrenceFrequencyId: this.id,
      active: true,
      deleted: false
    });



    public Pledges$ = this._pledgesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._pledges === null && this._pledgesPromise === null) {
            this.loadPledges(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public PledgesCount$ = PledgeService.Instance.GetPledgesRowCount({recurrenceFrequencyId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any RecurrenceFrequencyData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.recurrenceFrequency.Reload();
  //
  //  Non Async:
  //
  //     recurrenceFrequency[0].Reload().then(x => {
  //        this.recurrenceFrequency = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      RecurrenceFrequencyService.Instance.GetRecurrenceFrequency(this.id, includeRelations)
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
     this._recurrenceRules = null;
     this._recurrenceRulesPromise = null;
     this._recurrenceRulesSubject.next(null);

     this._pledges = null;
     this._pledgesPromise = null;
     this._pledgesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the RecurrenceRules for this RecurrenceFrequency.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.recurrenceFrequency.RecurrenceRules.then(recurrenceFrequencies => { ... })
     *   or
     *   await this.recurrenceFrequency.recurrenceFrequencies
     *
    */
    public get RecurrenceRules(): Promise<RecurrenceRuleData[]> {
        if (this._recurrenceRules !== null) {
            return Promise.resolve(this._recurrenceRules);
        }

        if (this._recurrenceRulesPromise !== null) {
            return this._recurrenceRulesPromise;
        }

        // Start the load
        this.loadRecurrenceRules();

        return this._recurrenceRulesPromise!;
    }



    private loadRecurrenceRules(): void {

        this._recurrenceRulesPromise = lastValueFrom(
            RecurrenceFrequencyService.Instance.GetRecurrenceRulesForRecurrenceFrequency(this.id)
        )
        .then(RecurrenceRules => {
            this._recurrenceRules = RecurrenceRules ?? [];
            this._recurrenceRulesSubject.next(this._recurrenceRules);
            return this._recurrenceRules;
         })
        .catch(err => {
            this._recurrenceRules = [];
            this._recurrenceRulesSubject.next(this._recurrenceRules);
            throw err;
        })
        .finally(() => {
            this._recurrenceRulesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached RecurrenceRule. Call after mutations to force refresh.
     */
    public ClearRecurrenceRulesCache(): void {
        this._recurrenceRules = null;
        this._recurrenceRulesPromise = null;
        this._recurrenceRulesSubject.next(this._recurrenceRules);      // Emit to observable
    }

    public get HasRecurrenceRules(): Promise<boolean> {
        return this.RecurrenceRules.then(recurrenceRules => recurrenceRules.length > 0);
    }


    /**
     *
     * Gets the Pledges for this RecurrenceFrequency.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.recurrenceFrequency.Pledges.then(recurrenceFrequencies => { ... })
     *   or
     *   await this.recurrenceFrequency.recurrenceFrequencies
     *
    */
    public get Pledges(): Promise<PledgeData[]> {
        if (this._pledges !== null) {
            return Promise.resolve(this._pledges);
        }

        if (this._pledgesPromise !== null) {
            return this._pledgesPromise;
        }

        // Start the load
        this.loadPledges();

        return this._pledgesPromise!;
    }



    private loadPledges(): void {

        this._pledgesPromise = lastValueFrom(
            RecurrenceFrequencyService.Instance.GetPledgesForRecurrenceFrequency(this.id)
        )
        .then(Pledges => {
            this._pledges = Pledges ?? [];
            this._pledgesSubject.next(this._pledges);
            return this._pledges;
         })
        .catch(err => {
            this._pledges = [];
            this._pledgesSubject.next(this._pledges);
            throw err;
        })
        .finally(() => {
            this._pledgesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Pledge. Call after mutations to force refresh.
     */
    public ClearPledgesCache(): void {
        this._pledges = null;
        this._pledgesPromise = null;
        this._pledgesSubject.next(this._pledges);      // Emit to observable
    }

    public get HasPledges(): Promise<boolean> {
        return this.Pledges.then(pledges => pledges.length > 0);
    }




    /**
     * Updates the state of this RecurrenceFrequencyData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this RecurrenceFrequencyData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): RecurrenceFrequencySubmitData {
        return RecurrenceFrequencyService.Instance.ConvertToRecurrenceFrequencySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class RecurrenceFrequencyService extends SecureEndpointBase {

    private static _instance: RecurrenceFrequencyService;
    private listCache: Map<string, Observable<Array<RecurrenceFrequencyData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<RecurrenceFrequencyBasicListData>>>;
    private recordCache: Map<string, Observable<RecurrenceFrequencyData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private recurrenceRuleService: RecurrenceRuleService,
        private pledgeService: PledgeService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<RecurrenceFrequencyData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<RecurrenceFrequencyBasicListData>>>();
        this.recordCache = new Map<string, Observable<RecurrenceFrequencyData>>();

        RecurrenceFrequencyService._instance = this;
    }

    public static get Instance(): RecurrenceFrequencyService {
      return RecurrenceFrequencyService._instance;
    }


    public ClearListCaches(config: RecurrenceFrequencyQueryParameters | null = null) {

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


    public ConvertToRecurrenceFrequencySubmitData(data: RecurrenceFrequencyData): RecurrenceFrequencySubmitData {

        let output = new RecurrenceFrequencySubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetRecurrenceFrequency(id: bigint | number, includeRelations: boolean = true) : Observable<RecurrenceFrequencyData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const recurrenceFrequency$ = this.requestRecurrenceFrequency(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RecurrenceFrequency", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, recurrenceFrequency$);

            return recurrenceFrequency$;
        }

        return this.recordCache.get(configHash) as Observable<RecurrenceFrequencyData>;
    }

    private requestRecurrenceFrequency(id: bigint | number, includeRelations: boolean = true) : Observable<RecurrenceFrequencyData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RecurrenceFrequencyData>(this.baseUrl + 'api/RecurrenceFrequency/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveRecurrenceFrequency(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestRecurrenceFrequency(id, includeRelations));
            }));
    }

    public GetRecurrenceFrequencyList(config: RecurrenceFrequencyQueryParameters | any = null) : Observable<Array<RecurrenceFrequencyData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const recurrenceFrequencyList$ = this.requestRecurrenceFrequencyList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RecurrenceFrequency list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, recurrenceFrequencyList$);

            return recurrenceFrequencyList$;
        }

        return this.listCache.get(configHash) as Observable<Array<RecurrenceFrequencyData>>;
    }


    private requestRecurrenceFrequencyList(config: RecurrenceFrequencyQueryParameters | any) : Observable <Array<RecurrenceFrequencyData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RecurrenceFrequencyData>>(this.baseUrl + 'api/RecurrenceFrequencies', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveRecurrenceFrequencyList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestRecurrenceFrequencyList(config));
            }));
    }

    public GetRecurrenceFrequenciesRowCount(config: RecurrenceFrequencyQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const recurrenceFrequenciesRowCount$ = this.requestRecurrenceFrequenciesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RecurrenceFrequencies row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, recurrenceFrequenciesRowCount$);

            return recurrenceFrequenciesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestRecurrenceFrequenciesRowCount(config: RecurrenceFrequencyQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/RecurrenceFrequencies/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRecurrenceFrequenciesRowCount(config));
            }));
    }

    public GetRecurrenceFrequenciesBasicListData(config: RecurrenceFrequencyQueryParameters | any = null) : Observable<Array<RecurrenceFrequencyBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const recurrenceFrequenciesBasicListData$ = this.requestRecurrenceFrequenciesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RecurrenceFrequencies basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, recurrenceFrequenciesBasicListData$);

            return recurrenceFrequenciesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<RecurrenceFrequencyBasicListData>>;
    }


    private requestRecurrenceFrequenciesBasicListData(config: RecurrenceFrequencyQueryParameters | any) : Observable<Array<RecurrenceFrequencyBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RecurrenceFrequencyBasicListData>>(this.baseUrl + 'api/RecurrenceFrequencies/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRecurrenceFrequenciesBasicListData(config));
            }));

    }


    public PutRecurrenceFrequency(id: bigint | number, recurrenceFrequency: RecurrenceFrequencySubmitData) : Observable<RecurrenceFrequencyData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RecurrenceFrequencyData>(this.baseUrl + 'api/RecurrenceFrequency/' + id.toString(), recurrenceFrequency, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRecurrenceFrequency(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutRecurrenceFrequency(id, recurrenceFrequency));
            }));
    }


    public PostRecurrenceFrequency(recurrenceFrequency: RecurrenceFrequencySubmitData) : Observable<RecurrenceFrequencyData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<RecurrenceFrequencyData>(this.baseUrl + 'api/RecurrenceFrequency', recurrenceFrequency, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRecurrenceFrequency(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostRecurrenceFrequency(recurrenceFrequency));
            }));
    }

  
    public DeleteRecurrenceFrequency(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/RecurrenceFrequency/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteRecurrenceFrequency(id));
            }));
    }


    private getConfigHash(config: RecurrenceFrequencyQueryParameters | any): string {

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

    public userIsSchedulerRecurrenceFrequencyReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerRecurrenceFrequencyReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.RecurrenceFrequencies
        //
        if (userIsSchedulerRecurrenceFrequencyReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerRecurrenceFrequencyReader = user.readPermission >= 1;
            } else {
                userIsSchedulerRecurrenceFrequencyReader = false;
            }
        }

        return userIsSchedulerRecurrenceFrequencyReader;
    }


    public userIsSchedulerRecurrenceFrequencyWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerRecurrenceFrequencyWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.RecurrenceFrequencies
        //
        if (userIsSchedulerRecurrenceFrequencyWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerRecurrenceFrequencyWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerRecurrenceFrequencyWriter = false;
          }      
        }

        return userIsSchedulerRecurrenceFrequencyWriter;
    }

    public GetRecurrenceRulesForRecurrenceFrequency(recurrenceFrequencyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<RecurrenceRuleData[]> {
        return this.recurrenceRuleService.GetRecurrenceRuleList({
            recurrenceFrequencyId: recurrenceFrequencyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPledgesForRecurrenceFrequency(recurrenceFrequencyId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PledgeData[]> {
        return this.pledgeService.GetPledgeList({
            recurrenceFrequencyId: recurrenceFrequencyId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full RecurrenceFrequencyData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the RecurrenceFrequencyData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when RecurrenceFrequencyTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveRecurrenceFrequency(raw: any): RecurrenceFrequencyData {
    if (!raw) return raw;

    //
    // Create a RecurrenceFrequencyData object instance with correct prototype
    //
    const revived = Object.create(RecurrenceFrequencyData.prototype) as RecurrenceFrequencyData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._recurrenceRules = null;
    (revived as any)._recurrenceRulesPromise = null;
    (revived as any)._recurrenceRulesSubject = new BehaviorSubject<RecurrenceRuleData[] | null>(null);

    (revived as any)._pledges = null;
    (revived as any)._pledgesPromise = null;
    (revived as any)._pledgesSubject = new BehaviorSubject<PledgeData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadRecurrenceFrequencyXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).RecurrenceRules$ = (revived as any)._recurrenceRulesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._recurrenceRules === null && (revived as any)._recurrenceRulesPromise === null) {
                (revived as any).loadRecurrenceRules();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).RecurrenceRulesCount$ = RecurrenceRuleService.Instance.GetRecurrenceRulesRowCount({recurrenceFrequencyId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Pledges$ = (revived as any)._pledgesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._pledges === null && (revived as any)._pledgesPromise === null) {
                (revived as any).loadPledges();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).PledgesCount$ = PledgeService.Instance.GetPledgesRowCount({recurrenceFrequencyId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveRecurrenceFrequencyList(rawList: any[]): RecurrenceFrequencyData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveRecurrenceFrequency(raw));
  }

}
