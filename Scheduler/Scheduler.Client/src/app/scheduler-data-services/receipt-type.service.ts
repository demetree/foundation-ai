/*

   GENERATED SERVICE FOR THE RECEIPTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ReceiptType table.

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
import { GiftService, GiftData } from './gift.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ReceiptTypeQueryParameters {
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
export class ReceiptTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ReceiptTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ReceiptTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `receiptType.ReceiptTypeChildren$` — use with `| async` in templates
//        • Promise:    `receiptType.ReceiptTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="receiptType.ReceiptTypeChildren$ | async"`), or
//        • Access the promise getter (`receiptType.ReceiptTypeChildren` or `await receiptType.ReceiptTypeChildren`)
//    - Simply reading `receiptType.ReceiptTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await receiptType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ReceiptTypeData {
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
    private _gifts: GiftData[] | null = null;
    private _giftsPromise: Promise<GiftData[]> | null  = null;
    private _giftsSubject = new BehaviorSubject<GiftData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Gifts$ = this._giftsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._gifts === null && this._giftsPromise === null) {
            this.loadGifts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _giftsCount$: Observable<bigint | number> | null = null;
    public get GiftsCount$(): Observable<bigint | number> {
        if (this._giftsCount$ === null) {
            this._giftsCount$ = GiftService.Instance.GetGiftsRowCount({receiptTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._giftsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ReceiptTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.receiptType.Reload();
  //
  //  Non Async:
  //
  //     receiptType[0].Reload().then(x => {
  //        this.receiptType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ReceiptTypeService.Instance.GetReceiptType(this.id, includeRelations)
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
     this._gifts = null;
     this._giftsPromise = null;
     this._giftsSubject.next(null);
     this._giftsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Gifts for this ReceiptType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.receiptType.Gifts.then(receiptTypes => { ... })
     *   or
     *   await this.receiptType.receiptTypes
     *
    */
    public get Gifts(): Promise<GiftData[]> {
        if (this._gifts !== null) {
            return Promise.resolve(this._gifts);
        }

        if (this._giftsPromise !== null) {
            return this._giftsPromise;
        }

        // Start the load
        this.loadGifts();

        return this._giftsPromise!;
    }



    private loadGifts(): void {

        this._giftsPromise = lastValueFrom(
            ReceiptTypeService.Instance.GetGiftsForReceiptType(this.id)
        )
        .then(Gifts => {
            this._gifts = Gifts ?? [];
            this._giftsSubject.next(this._gifts);
            return this._gifts;
         })
        .catch(err => {
            this._gifts = [];
            this._giftsSubject.next(this._gifts);
            throw err;
        })
        .finally(() => {
            this._giftsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Gift. Call after mutations to force refresh.
     */
    public ClearGiftsCache(): void {
        this._gifts = null;
        this._giftsPromise = null;
        this._giftsSubject.next(this._gifts);      // Emit to observable
    }

    public get HasGifts(): Promise<boolean> {
        return this.Gifts.then(gifts => gifts.length > 0);
    }




    /**
     * Updates the state of this ReceiptTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ReceiptTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ReceiptTypeSubmitData {
        return ReceiptTypeService.Instance.ConvertToReceiptTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ReceiptTypeService extends SecureEndpointBase {

    private static _instance: ReceiptTypeService;
    private listCache: Map<string, Observable<Array<ReceiptTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ReceiptTypeBasicListData>>>;
    private recordCache: Map<string, Observable<ReceiptTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private giftService: GiftService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ReceiptTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ReceiptTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<ReceiptTypeData>>();

        ReceiptTypeService._instance = this;
    }

    public static get Instance(): ReceiptTypeService {
      return ReceiptTypeService._instance;
    }


    public ClearListCaches(config: ReceiptTypeQueryParameters | null = null) {

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


    public ConvertToReceiptTypeSubmitData(data: ReceiptTypeData): ReceiptTypeSubmitData {

        let output = new ReceiptTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetReceiptType(id: bigint | number, includeRelations: boolean = true) : Observable<ReceiptTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const receiptType$ = this.requestReceiptType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ReceiptType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, receiptType$);

            return receiptType$;
        }

        return this.recordCache.get(configHash) as Observable<ReceiptTypeData>;
    }

    private requestReceiptType(id: bigint | number, includeRelations: boolean = true) : Observable<ReceiptTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ReceiptTypeData>(this.baseUrl + 'api/ReceiptType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveReceiptType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestReceiptType(id, includeRelations));
            }));
    }

    public GetReceiptTypeList(config: ReceiptTypeQueryParameters | any = null) : Observable<Array<ReceiptTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const receiptTypeList$ = this.requestReceiptTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ReceiptType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, receiptTypeList$);

            return receiptTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ReceiptTypeData>>;
    }


    private requestReceiptTypeList(config: ReceiptTypeQueryParameters | any) : Observable <Array<ReceiptTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ReceiptTypeData>>(this.baseUrl + 'api/ReceiptTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveReceiptTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestReceiptTypeList(config));
            }));
    }

    public GetReceiptTypesRowCount(config: ReceiptTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const receiptTypesRowCount$ = this.requestReceiptTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ReceiptTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, receiptTypesRowCount$);

            return receiptTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestReceiptTypesRowCount(config: ReceiptTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ReceiptTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestReceiptTypesRowCount(config));
            }));
    }

    public GetReceiptTypesBasicListData(config: ReceiptTypeQueryParameters | any = null) : Observable<Array<ReceiptTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const receiptTypesBasicListData$ = this.requestReceiptTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ReceiptTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, receiptTypesBasicListData$);

            return receiptTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ReceiptTypeBasicListData>>;
    }


    private requestReceiptTypesBasicListData(config: ReceiptTypeQueryParameters | any) : Observable<Array<ReceiptTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ReceiptTypeBasicListData>>(this.baseUrl + 'api/ReceiptTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestReceiptTypesBasicListData(config));
            }));

    }


    public PutReceiptType(id: bigint | number, receiptType: ReceiptTypeSubmitData) : Observable<ReceiptTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ReceiptTypeData>(this.baseUrl + 'api/ReceiptType/' + id.toString(), receiptType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveReceiptType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutReceiptType(id, receiptType));
            }));
    }


    public PostReceiptType(receiptType: ReceiptTypeSubmitData) : Observable<ReceiptTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ReceiptTypeData>(this.baseUrl + 'api/ReceiptType', receiptType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveReceiptType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostReceiptType(receiptType));
            }));
    }

  
    public DeleteReceiptType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ReceiptType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteReceiptType(id));
            }));
    }


    private getConfigHash(config: ReceiptTypeQueryParameters | any): string {

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

    public userIsSchedulerReceiptTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerReceiptTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ReceiptTypes
        //
        if (userIsSchedulerReceiptTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerReceiptTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerReceiptTypeReader = false;
            }
        }

        return userIsSchedulerReceiptTypeReader;
    }


    public userIsSchedulerReceiptTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerReceiptTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ReceiptTypes
        //
        if (userIsSchedulerReceiptTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerReceiptTypeWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerReceiptTypeWriter = false;
          }      
        }

        return userIsSchedulerReceiptTypeWriter;
    }

    public GetGiftsForReceiptType(receiptTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<GiftData[]> {
        return this.giftService.GetGiftList({
            receiptTypeId: receiptTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ReceiptTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ReceiptTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ReceiptTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveReceiptType(raw: any): ReceiptTypeData {
    if (!raw) return raw;

    //
    // Create a ReceiptTypeData object instance with correct prototype
    //
    const revived = Object.create(ReceiptTypeData.prototype) as ReceiptTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._gifts = null;
    (revived as any)._giftsPromise = null;
    (revived as any)._giftsSubject = new BehaviorSubject<GiftData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadReceiptTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Gifts$ = (revived as any)._giftsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._gifts === null && (revived as any)._giftsPromise === null) {
                (revived as any).loadGifts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._giftsCount$ = null;



    return revived;
  }

  private ReviveReceiptTypeList(rawList: any[]): ReceiptTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveReceiptType(raw));
  }

}
