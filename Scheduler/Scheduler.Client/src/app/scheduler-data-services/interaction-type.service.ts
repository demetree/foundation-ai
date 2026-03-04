/*

   GENERATED SERVICE FOR THE INTERACTIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the InteractionType table.

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
import { IconData } from './icon.service';
import { ContactInteractionService, ContactInteractionData } from './contact-interaction.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class InteractionTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class InteractionTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class InteractionTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. InteractionTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `interactionType.InteractionTypeChildren$` — use with `| async` in templates
//        • Promise:    `interactionType.InteractionTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="interactionType.InteractionTypeChildren$ | async"`), or
//        • Access the promise getter (`interactionType.InteractionTypeChildren` or `await interactionType.InteractionTypeChildren`)
//    - Simply reading `interactionType.InteractionTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await interactionType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class InteractionTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence!: bigint | number;
    iconId!: bigint | number;
    color!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _contactInteractions: ContactInteractionData[] | null = null;
    private _contactInteractionsPromise: Promise<ContactInteractionData[]> | null  = null;
    private _contactInteractionsSubject = new BehaviorSubject<ContactInteractionData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ContactInteractions$ = this._contactInteractionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactInteractions === null && this._contactInteractionsPromise === null) {
            this.loadContactInteractions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _contactInteractionsCount$: Observable<bigint | number> | null = null;
    public get ContactInteractionsCount$(): Observable<bigint | number> {
        if (this._contactInteractionsCount$ === null) {
            this._contactInteractionsCount$ = ContactInteractionService.Instance.GetContactInteractionsRowCount({interactionTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._contactInteractionsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any InteractionTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.interactionType.Reload();
  //
  //  Non Async:
  //
  //     interactionType[0].Reload().then(x => {
  //        this.interactionType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      InteractionTypeService.Instance.GetInteractionType(this.id, includeRelations)
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
     this._contactInteractions = null;
     this._contactInteractionsPromise = null;
     this._contactInteractionsSubject.next(null);
     this._contactInteractionsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ContactInteractions for this InteractionType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.interactionType.ContactInteractions.then(interactionTypes => { ... })
     *   or
     *   await this.interactionType.interactionTypes
     *
    */
    public get ContactInteractions(): Promise<ContactInteractionData[]> {
        if (this._contactInteractions !== null) {
            return Promise.resolve(this._contactInteractions);
        }

        if (this._contactInteractionsPromise !== null) {
            return this._contactInteractionsPromise;
        }

        // Start the load
        this.loadContactInteractions();

        return this._contactInteractionsPromise!;
    }



    private loadContactInteractions(): void {

        this._contactInteractionsPromise = lastValueFrom(
            InteractionTypeService.Instance.GetContactInteractionsForInteractionType(this.id)
        )
        .then(ContactInteractions => {
            this._contactInteractions = ContactInteractions ?? [];
            this._contactInteractionsSubject.next(this._contactInteractions);
            return this._contactInteractions;
         })
        .catch(err => {
            this._contactInteractions = [];
            this._contactInteractionsSubject.next(this._contactInteractions);
            throw err;
        })
        .finally(() => {
            this._contactInteractionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContactInteraction. Call after mutations to force refresh.
     */
    public ClearContactInteractionsCache(): void {
        this._contactInteractions = null;
        this._contactInteractionsPromise = null;
        this._contactInteractionsSubject.next(this._contactInteractions);      // Emit to observable
    }

    public get HasContactInteractions(): Promise<boolean> {
        return this.ContactInteractions.then(contactInteractions => contactInteractions.length > 0);
    }




    /**
     * Updates the state of this InteractionTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this InteractionTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): InteractionTypeSubmitData {
        return InteractionTypeService.Instance.ConvertToInteractionTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class InteractionTypeService extends SecureEndpointBase {

    private static _instance: InteractionTypeService;
    private listCache: Map<string, Observable<Array<InteractionTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<InteractionTypeBasicListData>>>;
    private recordCache: Map<string, Observable<InteractionTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private contactInteractionService: ContactInteractionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<InteractionTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<InteractionTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<InteractionTypeData>>();

        InteractionTypeService._instance = this;
    }

    public static get Instance(): InteractionTypeService {
      return InteractionTypeService._instance;
    }


    public ClearListCaches(config: InteractionTypeQueryParameters | null = null) {

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


    public ConvertToInteractionTypeSubmitData(data: InteractionTypeData): InteractionTypeSubmitData {

        let output = new InteractionTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.iconId = data.iconId;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetInteractionType(id: bigint | number, includeRelations: boolean = true) : Observable<InteractionTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const interactionType$ = this.requestInteractionType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get InteractionType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, interactionType$);

            return interactionType$;
        }

        return this.recordCache.get(configHash) as Observable<InteractionTypeData>;
    }

    private requestInteractionType(id: bigint | number, includeRelations: boolean = true) : Observable<InteractionTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<InteractionTypeData>(this.baseUrl + 'api/InteractionType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveInteractionType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestInteractionType(id, includeRelations));
            }));
    }

    public GetInteractionTypeList(config: InteractionTypeQueryParameters | any = null) : Observable<Array<InteractionTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const interactionTypeList$ = this.requestInteractionTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get InteractionType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, interactionTypeList$);

            return interactionTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<InteractionTypeData>>;
    }


    private requestInteractionTypeList(config: InteractionTypeQueryParameters | any) : Observable <Array<InteractionTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<InteractionTypeData>>(this.baseUrl + 'api/InteractionTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveInteractionTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestInteractionTypeList(config));
            }));
    }

    public GetInteractionTypesRowCount(config: InteractionTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const interactionTypesRowCount$ = this.requestInteractionTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get InteractionTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, interactionTypesRowCount$);

            return interactionTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestInteractionTypesRowCount(config: InteractionTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/InteractionTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestInteractionTypesRowCount(config));
            }));
    }

    public GetInteractionTypesBasicListData(config: InteractionTypeQueryParameters | any = null) : Observable<Array<InteractionTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const interactionTypesBasicListData$ = this.requestInteractionTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get InteractionTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, interactionTypesBasicListData$);

            return interactionTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<InteractionTypeBasicListData>>;
    }


    private requestInteractionTypesBasicListData(config: InteractionTypeQueryParameters | any) : Observable<Array<InteractionTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<InteractionTypeBasicListData>>(this.baseUrl + 'api/InteractionTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestInteractionTypesBasicListData(config));
            }));

    }


    public PutInteractionType(id: bigint | number, interactionType: InteractionTypeSubmitData) : Observable<InteractionTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<InteractionTypeData>(this.baseUrl + 'api/InteractionType/' + id.toString(), interactionType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveInteractionType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutInteractionType(id, interactionType));
            }));
    }


    public PostInteractionType(interactionType: InteractionTypeSubmitData) : Observable<InteractionTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<InteractionTypeData>(this.baseUrl + 'api/InteractionType', interactionType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveInteractionType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostInteractionType(interactionType));
            }));
    }

  
    public DeleteInteractionType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/InteractionType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteInteractionType(id));
            }));
    }


    private getConfigHash(config: InteractionTypeQueryParameters | any): string {

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

    public userIsSchedulerInteractionTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerInteractionTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.InteractionTypes
        //
        if (userIsSchedulerInteractionTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerInteractionTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerInteractionTypeReader = false;
            }
        }

        return userIsSchedulerInteractionTypeReader;
    }


    public userIsSchedulerInteractionTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerInteractionTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.InteractionTypes
        //
        if (userIsSchedulerInteractionTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerInteractionTypeWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerInteractionTypeWriter = false;
          }      
        }

        return userIsSchedulerInteractionTypeWriter;
    }

    public GetContactInteractionsForInteractionType(interactionTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactInteractionData[]> {
        return this.contactInteractionService.GetContactInteractionList({
            interactionTypeId: interactionTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full InteractionTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the InteractionTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when InteractionTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveInteractionType(raw: any): InteractionTypeData {
    if (!raw) return raw;

    //
    // Create a InteractionTypeData object instance with correct prototype
    //
    const revived = Object.create(InteractionTypeData.prototype) as InteractionTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._contactInteractions = null;
    (revived as any)._contactInteractionsPromise = null;
    (revived as any)._contactInteractionsSubject = new BehaviorSubject<ContactInteractionData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadInteractionTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ContactInteractions$ = (revived as any)._contactInteractionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactInteractions === null && (revived as any)._contactInteractionsPromise === null) {
                (revived as any).loadContactInteractions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._contactInteractionsCount$ = null;



    return revived;
  }

  private ReviveInteractionTypeList(rawList: any[]): InteractionTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveInteractionType(raw));
  }

}
